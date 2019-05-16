using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using NUnit.Framework;
using Zenject;
using Moq;

using Assets.Simulation.HexMap;
using Assets.Simulation.AI;

namespace Assets.Tests.Simulation.AI {

    public class InfluenceMapApplierTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IHexGrid> MockGrid;

        private List<IHexCell> AllCells = new List<IHexCell>();

        private int nextIndex = 0;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            AllCells.Clear();
            nextIndex = 0;

            MockGrid = new Mock<IHexGrid>();

            MockGrid.Setup(grid => grid.Cells).Returns(AllCells.AsReadOnly());

            Container.Bind<IHexGrid>().FromInstance(MockGrid.Object);

            Container.Bind<InfluenceMapApplier>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void ApplyInfluenceToMap_AppliesFullStrengthToCenter_WithApplierFunction() {
            var center = BuildCell();

            MockGrid.Setup(grid => grid.GetCellsInRing(center, It.IsAny<int>())).Returns(new List<IHexCell>());

            var map = new float[2] { 3f, 3f };

            InfluenceRolloff rolloff = (strength, distance) => 0;
            InfluenceApplier applier = (previous, calculated) => previous + calculated + 2f;

            var influenceApplier = Container.Resolve<InfluenceMapApplier>();

            influenceApplier.ApplyInfluenceToMap(20f, map, center, 2, rolloff, applier);
            
            CollectionAssert.AreEqual(new float[2] { 25f, 3f }, map);
        }

        [Test]
        public void ApplyInfluenceToMap_AppliesStrengthToNearbyCells_FromRolloffAndApplierFunctions() {
            var center = BuildCell();

            MockGrid.Setup(grid => grid.GetCellsInRing(center, 1)).Returns(new List<IHexCell>() { BuildCell() });
            MockGrid.Setup(grid => grid.GetCellsInRing(center, 2)).Returns(new List<IHexCell>() { BuildCell(), BuildCell() });
            MockGrid.Setup(grid => grid.GetCellsInRing(center, 3)).Returns(new List<IHexCell>() { BuildCell() });

            var map = new float[] { 1f, 2f, 3f, 4f, 5f };

            InfluenceRolloff rolloff = (strength, distance) => strength * distance;
            InfluenceApplier applier = (previous, calculated) => previous + calculated + 2;

            var influenceApplier = Container.Resolve<InfluenceMapApplier>();

            influenceApplier.ApplyInfluenceToMap(10f, map, center, 3, rolloff, applier);

            CollectionAssert.AreEqual(
                new float[] { 13f, 14f, 25f, 26f, 37f }, map
            );
        }

        [Test]
        public void ApplyInfluenceMap_StopsAtMaxDistance() {
            var center = BuildCell();

            MockGrid.Setup(grid => grid.GetCellsInRing(center, 1)).Returns(new List<IHexCell>() { BuildCell() });
            MockGrid.Setup(grid => grid.GetCellsInRing(center, 2)).Returns(new List<IHexCell>() { BuildCell(), BuildCell() });
            MockGrid.Setup(grid => grid.GetCellsInRing(center, 3)).Returns(new List<IHexCell>() { BuildCell() });

            var map = new float[] { 1f, 2f, 3f, 4f, 5f };

            InfluenceRolloff rolloff = (strength, distance) => strength * distance;
            InfluenceApplier applier = (previous, calculated) => previous + calculated + 2;

            var influenceApplier = Container.Resolve<InfluenceMapApplier>();

            influenceApplier.ApplyInfluenceToMap(10f, map, center, 2, rolloff, applier);

            CollectionAssert.AreEqual(
                new float[] { 13f, 14f, 25f, 26f, 5f }, map
            );
        }

        [Test]
        public void PowerOfTwoRolloff_DividesStrengthByPowerOfTwo() {
            var influenceApplier = Container.Resolve<InfluenceMapApplier>();

            Assert.AreEqual(10f / Mathf.Pow(2, 4), influenceApplier.PowerOfTwoRolloff(10f, 4));
        }

        [Test]
        public void LinearRolloff_DividesStrengthByValue() {
            var influenceApplier = Container.Resolve<InfluenceMapApplier>();

            Assert.AreEqual(20f / 6, influenceApplier.LinearRolloff(20f, 6));
        }

        [Test]
        public void ApplyHighest_TakesHighestValue() {
            var influenceApplier = Container.Resolve<InfluenceMapApplier>();

            Assert.AreEqual(17f, influenceApplier.ApplyHighest(9f, 17f));
        }

        [Test]
        public void ApplySum_ReturnsSumOfValues() {
            var influenceApplier = Container.Resolve<InfluenceMapApplier>();

            Assert.AreEqual(26f, influenceApplier.ApplySum(9f, 17f));
        }

        #endregion

        #region utilities

        private IHexCell BuildCell() {
            var mockCell = new Mock<IHexCell>();

            mockCell.Setup(cell => cell.Index).Returns(nextIndex++);

            var newCell = mockCell.Object;

            AllCells.Add(newCell);

            return newCell;
        }

        #endregion

        #endregion

    }

}
