using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using NUnit.Framework;
using Zenject;
using Moq;

using Assets.Simulation;
using Assets.Simulation.HexMap;
using Assets.Simulation.Civilizations;
using Assets.Simulation.Visibility;
using Assets.Simulation.Barbarians;
using Assets.Simulation.Units;
using Assets.Simulation.AI;

namespace Assets.Tests.Simulation.Barbarians {

    public class BarbarianSpawningToolsTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<ICivilizationFactory>     MockCivFactory;
        private Mock<IBarbarianConfig>         MockBarbarianConfig;
        private Mock<IEncampmentFactory>       MockEncampmentFactory;
        private Mock<IVisibilityCanon>         MockVisibilityCanon;        
        private Mock<IEncampmentLocationCanon> MockEncampmentLocationCanon;
        private Mock<IHexGrid>                 MockGrid;
        private Mock<IRandomizer>              MockRandomizer;

        private List<ICivilization> AllCivs = new List<ICivilization>();

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            AllCivs.Clear();

            MockCivFactory              = new Mock<ICivilizationFactory>();
            MockBarbarianConfig         = new Mock<IBarbarianConfig>();
            MockEncampmentFactory       = new Mock<IEncampmentFactory>();
            MockVisibilityCanon         = new Mock<IVisibilityCanon>();
            MockEncampmentLocationCanon = new Mock<IEncampmentLocationCanon>();
            MockGrid                    = new Mock<IHexGrid>();
            MockRandomizer              = new Mock<IRandomizer>();

            MockCivFactory.Setup(factory => factory.AllCivilizations).Returns(AllCivs.AsReadOnly());

            Container.Bind<ICivilizationFactory>    ().FromInstance(MockCivFactory             .Object);
            Container.Bind<IBarbarianConfig>        ().FromInstance(MockBarbarianConfig        .Object);
            Container.Bind<IEncampmentFactory>      ().FromInstance(MockEncampmentFactory      .Object);
            Container.Bind<IVisibilityCanon>        ().FromInstance(MockVisibilityCanon        .Object);
            Container.Bind<IEncampmentLocationCanon>().FromInstance(MockEncampmentLocationCanon.Object);
            Container.Bind<IHexGrid>                ().FromInstance(MockGrid                   .Object);
            Container.Bind<IRandomizer>             ().FromInstance(MockRandomizer             .Object);

            Container.Bind<BarbarianSpawningTools>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void GetEncampmentSpawnChance_AndEncampmentCountLessThanMinEncampments_ReturnsOne() {
            MockBarbarianConfig.Setup(config => config.MinEncampmentsPerPlayer).Returns(1);

            var spawningTools = Container.Resolve<BarbarianSpawningTools>();

            Assert.AreEqual(1f, spawningTools.GetEncampmentSpawnChance(7, 8));
        }

        [Test]
        public void GetEncampmentSpawnChance_AndEncampmentCountGreaterThanMaxEncampments_ReturnsZero() {
            MockBarbarianConfig.Setup(config => config.MaxEncampmentsPerPlayer).Returns(1);

            var spawningTools = Container.Resolve<BarbarianSpawningTools>();

            Assert.AreEqual(0f, spawningTools.GetEncampmentSpawnChance(9, 8));
        }

        [Test(Description = "The equation should be (maxEncampments - encampmentCount) / (maxEncampments - minEncampments)")]
        public void GetEncampmentSpawnChance_AndEncampmentCountBetweenMinAndMax_ReturnsProperValue() {
            MockBarbarianConfig.Setup(config => config.MinEncampmentsPerPlayer).Returns(2);
            MockBarbarianConfig.Setup(config => config.MaxEncampmentsPerPlayer).Returns(4);

            var spawningTools = Container.Resolve<BarbarianSpawningTools>();

            Assert.AreEqual(
                1f / 4f, spawningTools.GetEncampmentSpawnChance(35, 10)
            );
        }

        [Test]
        public void IsCellValidForEncampment_TrueIfValidForEncampmentsAndNotVisible() {
            var cell = BuildCell(true);

            var spawningTools = Container.Resolve<BarbarianSpawningTools>();

            Assert.IsTrue(spawningTools.IsCellValidForEncampment(cell));
        }

        [Test]
        public void IsCellValidForEncampment_FalseIfNotValidForEncampment() {
            var cell = BuildCell(false);

            var spawningTools = Container.Resolve<BarbarianSpawningTools>();

            Assert.IsFalse(spawningTools.IsCellValidForEncampment(cell));
        }

        [Test]
        public void IsCellValidForEncampment_FalseIfCellIsVisibleToSomeNonBarbarianCiv() {
            var cell = BuildCell(true);

            BuildCiv(BuildCivTemplate(false), cell);

            var spawningTools = Container.Resolve<BarbarianSpawningTools>();

            Assert.IsFalse(spawningTools.IsCellValidForEncampment(cell));
        }

        [Test]
        public void IsCellValidForEncampment_TrueIfCellIsVisibleToOnlyBarbarianCivs() {
            var cell = BuildCell(true);

            BuildCiv(BuildCivTemplate(true), cell);
            BuildCiv(BuildCivTemplate(true), cell);
            BuildCiv(BuildCivTemplate(false));

            var spawningTools = Container.Resolve<BarbarianSpawningTools>();

            Assert.IsTrue(spawningTools.IsCellValidForEncampment(cell));
        }

        [Test]
        public void BuildEncampmentWeightFunction_ReturnedFunction_ReturnsBaseEncampmentSpawnWeightByDefault() {
            var cell = BuildCell(false);

            var maps = new InfluenceMaps() {
                AllyPresence = new float[] { 0f }, EnemyPresence = new float[] { 0f }
            };

            MockBarbarianConfig.Setup(config => config.BaseEncampmentSpawnWeight) .Returns(25f);
            MockBarbarianConfig.Setup(config => config.AllyEncampmentSpawnWeight) .Returns(3f);            
            MockBarbarianConfig.Setup(config => config.EnemyEncampmentSpawnWeight).Returns(7f);

            var spawningTools = Container.Resolve<BarbarianSpawningTools>();

            var weightFunction = spawningTools.BuildEncampmentWeightFunction(maps);

            Assert.AreEqual(25, weightFunction(cell));
        }

        [Test]
        public void BuildEncampmentWeightFunction_ReturnedFunction_ReturnedWeightDecreasedByAlliedWeight() {
            var cell = BuildCell(false);

            var maps = new InfluenceMaps() {
                AllyPresence = new float[] { 3.5f }, EnemyPresence = new float[] { 0f }
            };

            MockBarbarianConfig.Setup(config => config.BaseEncampmentSpawnWeight) .Returns(25f);
            MockBarbarianConfig.Setup(config => config.AllyEncampmentSpawnWeight) .Returns(3f);            
            MockBarbarianConfig.Setup(config => config.EnemyEncampmentSpawnWeight).Returns(7f);

            var spawningTools = Container.Resolve<BarbarianSpawningTools>();

            var weightFunction = spawningTools.BuildEncampmentWeightFunction(maps);

            Assert.AreEqual(Mathf.RoundToInt(25f / (1 + 3f * 3.5f)), weightFunction(cell));
        }

        [Test]
        public void BuildEncampmentWeightFunction_ReturnedFunction_ReturnedWeightDecreasedByEnemyWeight() {
            var cell = BuildCell(false);

            var maps = new InfluenceMaps() {
                AllyPresence = new float[] { 0f }, EnemyPresence = new float[] { 3f }
            };

            MockBarbarianConfig.Setup(config => config.BaseEncampmentSpawnWeight) .Returns(25f);
            MockBarbarianConfig.Setup(config => config.AllyEncampmentSpawnWeight) .Returns(3f);            
            MockBarbarianConfig.Setup(config => config.EnemyEncampmentSpawnWeight).Returns(7f);

            var spawningTools = Container.Resolve<BarbarianSpawningTools>();

            var weightFunction = spawningTools.BuildEncampmentWeightFunction(maps);

            Assert.AreEqual(Mathf.RoundToInt(25f / 22f), weightFunction(cell));
        }

        [Test]
        public void BuildEncampmentWeightFunction_ReturnedFunction_ReturnedWeightDoesNotDropBelowOne() {
            var cell = BuildCell(false);

            var maps = new InfluenceMaps() {
                AllyPresence = new float[] { 1000f }, EnemyPresence = new float[] { 1000f }
            };

            MockBarbarianConfig.Setup(config => config.BaseEncampmentSpawnWeight) .Returns(25f);
            MockBarbarianConfig.Setup(config => config.AllyEncampmentSpawnWeight) .Returns(3f);            
            MockBarbarianConfig.Setup(config => config.EnemyEncampmentSpawnWeight).Returns(7f);

            var spawningTools = Container.Resolve<BarbarianSpawningTools>();

            var weightFunction = spawningTools.BuildEncampmentWeightFunction(maps);

            Assert.AreEqual(1, weightFunction(cell));
        }

        [Test]
        public void TryGetValidSpawn_AndSpawningValid_IsValidSpawnOfReturnedInfoIsTrue() {
            var encampmentLocation = BuildCell();

            var cellOne   = BuildCell();
            var cellTwo   = BuildCell();
            var cellThree = BuildCell();

            MockGrid.Setup(grid => grid.GetCellsInRadius(encampmentLocation, 1, true))
                    .Returns(new List<IHexCell>() { cellOne, cellTwo, cellThree });

            var unitTemplateTwo   = BuildUnitTemplate();
            var unitTemplateThree = BuildUnitTemplate();

            var encampment = BuildEncampment(encampmentLocation);

            Func<IHexCell, IEnumerable<IUnitTemplate>> selector = delegate(IHexCell cell) {
                if(cell == cellOne)   return new List<IUnitTemplate>();
                if(cell == cellTwo)   return new List<IUnitTemplate>() { unitTemplateTwo };
                if(cell == cellThree) return new List<IUnitTemplate>() { unitTemplateThree };
                throw new NotImplementedException();
            };

            var spawningTools = Container.Resolve<BarbarianSpawningTools>();

            var spawnInfo = spawningTools.TryGetValidSpawn(encampment, selector);

            Assert.IsTrue(spawnInfo.IsValidSpawn);
        }

        [Test]
        public void TryGetValidSpawn_AndSpawningValid_DataTakesFirstNearbyCell_AndARandomUnitValidForIt() {
            var encampmentLocation = BuildCell();

            var cellOne   = BuildCell();
            var cellTwo   = BuildCell();
            var cellThree = BuildCell();

            MockGrid.Setup(grid => grid.GetCellsInRadius(encampmentLocation, 1, true))
                    .Returns(new List<IHexCell>() { cellOne, cellTwo, cellThree });

            var unitTemplateTwo   = BuildUnitTemplate();
            var unitTemplateThree = BuildUnitTemplate();

            var encampment = BuildEncampment(encampmentLocation);

            Func<IHexCell, IEnumerable<IUnitTemplate>> selector = delegate(IHexCell cell) {
                if(cell == cellOne)   return new List<IUnitTemplate>();
                if(cell == cellTwo)   return new List<IUnitTemplate>() { unitTemplateTwo };
                if(cell == cellThree) return new List<IUnitTemplate>() { unitTemplateThree };
                throw new NotImplementedException();
            };

            MockRandomizer.Setup(randomizer => randomizer.TakeRandom(It.IsAny<IEnumerable<IUnitTemplate>>()))
                          .Returns<IEnumerable<IUnitTemplate>>(set => set.FirstOrDefault());

            var spawningTools = Container.Resolve<BarbarianSpawningTools>();

            var spawnInfo = spawningTools.TryGetValidSpawn(encampment, selector);

            Assert.AreEqual(cellTwo,         spawnInfo.LocationOfUnit,  "spawnInfo has an unexpected LocationOfUnit");
            Assert.AreEqual(unitTemplateTwo, spawnInfo.TemplateToBuild, "spawnInfo has an unexpected TemplateToBuild");
        }

        [Test]
        public void TryGetValidSpawn_AndSpawningInvalid_IsValidSpawnOfReturnedInfoIsFalse() {
            var encampmentLocation = BuildCell();

            var cellOne   = BuildCell();
            var cellTwo   = BuildCell();
            var cellThree = BuildCell();

            MockGrid.Setup(grid => grid.GetCellsInRadius(encampmentLocation, 1, true))
                    .Returns(new List<IHexCell>() { cellOne, cellTwo, cellThree });

            var encampment = BuildEncampment(encampmentLocation);

            Func<IHexCell, IEnumerable<IUnitTemplate>> selector = cell => new List<IUnitTemplate>();

            MockRandomizer.Setup(randomizer => randomizer.TakeRandom(It.IsAny<IEnumerable<IUnitTemplate>>()))
                          .Returns<IEnumerable<IUnitTemplate>>(set => set.FirstOrDefault());

            var spawningTools = Container.Resolve<BarbarianSpawningTools>();

            var spawnInfo = spawningTools.TryGetValidSpawn(encampment, selector);

            Assert.IsFalse(spawnInfo.IsValidSpawn);
        }

        [Test]
        public void TryGetValidSpawn_AndSpawningInvalid_ReturnedDataHasNullLocationAndUnit() {
            var encampmentLocation = BuildCell();

            var cellOne   = BuildCell();
            var cellTwo   = BuildCell();
            var cellThree = BuildCell();

            MockGrid.Setup(grid => grid.GetCellsInRadius(encampmentLocation, 1, true))
                    .Returns(new List<IHexCell>() { cellOne, cellTwo, cellThree });

            var encampment = BuildEncampment(encampmentLocation);

            Func<IHexCell, IEnumerable<IUnitTemplate>> selector = cell => new List<IUnitTemplate>();

            MockRandomizer.Setup(randomizer => randomizer.TakeRandom(It.IsAny<IEnumerable<IUnitTemplate>>()))
                          .Returns<IEnumerable<IUnitTemplate>>(set => set.FirstOrDefault());

            var spawningTools = Container.Resolve<BarbarianSpawningTools>();

            var spawnInfo = spawningTools.TryGetValidSpawn(encampment, selector);

            Assert.IsNull(spawnInfo.LocationOfUnit,  "LocationOfUnit not null as expected");
            Assert.IsNull(spawnInfo.TemplateToBuild, "TemplateToBuild not null as expected");
        }

        #endregion

        #region utilities

        private IHexCell BuildCell(bool isValidForEncampment = false) {
            var newCell = new Mock<IHexCell>().Object;

            MockEncampmentFactory.Setup(factory => factory.CanCreateEncampment(newCell)).Returns(isValidForEncampment);

            return newCell;
        }

        private ICivilizationTemplate BuildCivTemplate(bool isBarbaric) {
            var mockTemplate = new Mock<ICivilizationTemplate>();

            mockTemplate.Setup(template => template.IsBarbaric).Returns(isBarbaric);

            return mockTemplate.Object;
        }

        private ICivilization BuildCiv(ICivilizationTemplate template, params IHexCell[] visibleCells) {
            var mockCiv = new Mock<ICivilization>();

            mockCiv.Setup(civ => civ.Template).Returns(template);

            var newCiv = mockCiv.Object;

            foreach(var cell in visibleCells) {
                MockVisibilityCanon.Setup(canon => canon.IsCellVisibleToCiv(cell, newCiv)).Returns(true);
            }

            AllCivs.Add(newCiv);
            
            return newCiv;
        }

        private IEncampment BuildEncampment(IHexCell location) {
            var newEncampment = new Mock<IEncampment>().Object;

            MockEncampmentLocationCanon.Setup(canon => canon.GetOwnerOfPossession(newEncampment)).Returns(location);

            return newEncampment;
        }

        private IUnitTemplate BuildUnitTemplate() {
            return new Mock<IUnitTemplate>().Object;
        }

        #endregion

        #endregion

    }

}
