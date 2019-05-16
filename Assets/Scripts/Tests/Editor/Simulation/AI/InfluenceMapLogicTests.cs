using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using Zenject;
using Moq;

using Assets.Simulation.AI;
using Assets.Simulation.HexMap;
using Assets.Simulation.Civilizations;

namespace Assets.Tests.Simulation.AI {

    public class InfluenceMapLogicTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IHexGrid> MockGrid;

        private List<Mock<IInfluenceSource>> AllMockInfluenceSources = new List<Mock<IInfluenceSource>>();

        private List<IHexCell> AllCells = new List<IHexCell>();

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            AllMockInfluenceSources.Clear();

            AllCells.Clear();

            MockGrid = new Mock<IHexGrid>();

            MockGrid.Setup(grid => grid.Cells).Returns(AllCells.AsReadOnly());

            Container.Bind<IHexGrid>().FromInstance(MockGrid.Object);

            Container.Bind<List<IInfluenceSource>>().FromInstance(
                AllMockInfluenceSources.Select(mockSource => mockSource.Object).ToList()
            );

            Container.Bind<InfluenceMapLogic>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void ClearMaps_AllMapArraysSetToNull() {
            var maps = new InfluenceMaps() {
                AllyPresence     = new float[0],
                EnemyPresence    = new float[0],
                PillagingValue = new float[0],
            };

            var mapGenerator = Container.Resolve<InfluenceMapLogic>();

            mapGenerator.ClearMaps(maps);
            
            Assert.IsNull(maps.AllyPresence,     "AllyPresence was not set to null");
            Assert.IsNull(maps.EnemyPresence,    "EnemyPresence was not set to null");
            Assert.IsNull(maps.PillagingValue, "ImprovementValue was not set to null");
        }

        [Test]
        public void GenerateMaps_AllNullMapsConstructedProperly() {
            var maps = new InfluenceMaps() {
                AllyPresence = null,
                EnemyPresence = new float[3],
                PillagingValue = null
            };

            BuildCell();
            BuildCell();

            var mapGenerator = Container.Resolve<InfluenceMapLogic>();

            mapGenerator.AssignMaps(maps, BuildCiv());

            CollectionAssert.AreEquivalent(
                new float[] { 0f, 0f }, maps.AllyPresence, "AllyPresence not constructed as expected"
            );

            CollectionAssert.AreEquivalent(
                new float[] { 0f, 0f, 0f }, maps.EnemyPresence, "EnemyPresence unexpectedly modified"
            );

            CollectionAssert.AreEquivalent(
                new float[] { 0f, 0f }, maps.PillagingValue, "ImprovementValue not constructed as expected"
            );
        }

        [Test]
        public void GenerateMaps_AllMapsInitializedToZero() {
            var maps = new InfluenceMaps() {
                AllyPresence     = new float[] { 1, 1 },
                EnemyPresence    = new float[] { 2, 2, 2 },
                PillagingValue = new float[] { 3, 3, 3, 3 },
            };

            BuildCell();
            BuildCell();

            var mapGenerator = Container.Resolve<InfluenceMapLogic>();

            mapGenerator.AssignMaps(maps, BuildCiv());

            CollectionAssert.AreEquivalent(
                new float[] { 0f, 0f }, maps.AllyPresence, "AllyPresence not initialized as expected"
            );

            CollectionAssert.AreEquivalent(
                new float[] { 0f, 0f, 2f }, maps.EnemyPresence, "EnemyPresence not initialized as expected"
            );

            CollectionAssert.AreEquivalent(
                new float[] { 0f, 0f, 3f, 3f }, maps.PillagingValue, "ImprovementValue not initialized as expected"
            );
        }

        [Test]
        public void GenerateMaps_MapsPassedToAllInfluenceSources() {
            var maps = new InfluenceMaps();

            BuildCell();
            BuildCell();

            var civ = BuildCiv();

            var mapGenerator = Container.Resolve<InfluenceMapLogic>();

            mapGenerator.AssignMaps(maps, civ);

            foreach(var mockSource in AllMockInfluenceSources) {
                mockSource.Verify(
                    source => source.ApplyToMaps(maps, civ), Times.Once,
                    "An influence source was not applied to the maps as expected"
                );
            }
        }

        #endregion

        #region utilities

        private IHexCell BuildCell() {
            var mockCell = new Mock<IHexCell>();

            mockCell.Setup(cell => cell.Index).Returns(AllCells.Count);

            var newCell = mockCell.Object;

            AllCells.Add(newCell);

            return newCell;
        }

        private ICivilization BuildCiv() {
            return new Mock<ICivilization>().Object;
        }

        #endregion

        #endregion

    }

}
