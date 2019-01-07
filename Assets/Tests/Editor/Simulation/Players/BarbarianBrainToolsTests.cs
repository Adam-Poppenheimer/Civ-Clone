using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using NUnit.Framework;
using Zenject;
using Moq;

using Assets.Simulation.HexMap;
using Assets.Simulation.Units;
using Assets.Simulation.Players;
using Assets.Simulation.Players.Barbarians;

namespace Assets.Tests.Simulation.Players {

    public class BarbarianBrainToolsTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IHexGrid>           MockGrid;
        private Mock<IUnitPositionCanon> MockUnitPositionCanon;
        private Mock<IPlayerConfig>      MockPlayerConfig;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockGrid              = new Mock<IHexGrid>();
            MockUnitPositionCanon = new Mock<IUnitPositionCanon>();
            MockPlayerConfig      = new Mock<IPlayerConfig>();

            Container.Bind<IHexGrid>          ().FromInstance(MockGrid             .Object);
            Container.Bind<IUnitPositionCanon>().FromInstance(MockUnitPositionCanon.Object);
            Container.Bind<IPlayerConfig>     ().FromInstance(MockPlayerConfig     .Object);

            Container.Bind<BarbarianBrainTools>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void GetWanderWeightFunction_ReturnedDelegate_ReturnsZeroOfCellIsLocationOfUnit() {
            var cell = BuildCell(0);
            var unit = BuildUnit(cell);

            var maps = new BarbarianInfluenceMaps() { AllyPresence = new float[1] { 1 } };

            MockUnitPositionCanon.Setup(canon => canon.CanPlaceUnitAtLocation(unit, cell, false)).Returns(true);

            var brainTools = Container.Resolve<BarbarianBrainTools>();

            var returnedDelegate = brainTools.GetWanderWeightFunction(unit, maps);

            Assert.AreEqual(0, returnedDelegate(cell));
        }

        [Test]
        public void GetWanderWeightFunction_ReturnedDelegate_ReturnsZeroIfCellIsImpassableToUnit() {
            var cell = BuildCell(0);
            var unit = BuildUnit(BuildCell(1));

            var maps = new BarbarianInfluenceMaps() { AllyPresence = new float[1] { 1 } };

            MockUnitPositionCanon.Setup(canon => canon.CanPlaceUnitAtLocation(unit, cell, false)).Returns(false);

            var brainTools = Container.Resolve<BarbarianBrainTools>();

            var returnedDelegate = brainTools.GetWanderWeightFunction(unit, maps);

            Assert.AreEqual(0, returnedDelegate(cell));
        }

        [Test]
        public void GetWanderWeightFunction_ReturnedDelegate_ValueIncreasedFromDistanceToUnitLocation() {
            var cellToTest   = BuildCell(0);
            var unitLocation = BuildCell(1);

            var unit = BuildUnit(unitLocation);

            var maps = new BarbarianInfluenceMaps() { AllyPresence = new float[1] { 0 } };

            MockUnitPositionCanon.Setup(canon => canon.CanPlaceUnitAtLocation(unit, cellToTest, false)).Returns(true);

            MockGrid.Setup(grid => grid.GetDistance(cellToTest, unitLocation)).Returns(5);

            MockPlayerConfig.Setup(config => config.WanderSelectionWeight_Distance).Returns(5.5f);

            var brainTools = Container.Resolve<BarbarianBrainTools>();

            var returnedDelegate = brainTools.GetWanderWeightFunction(unit, maps);

            Assert.AreEqual(Mathf.RoundToInt(5 * 5.5f), returnedDelegate(cellToTest));
        }

        [Test]
        public void GetWanderWeightFunction_ReturnedDelegate_ValueDecreasedFromAllyPresenceOfCell() {
            var cellToTest   = BuildCell(0);
            var unitLocation = BuildCell(1);

            var unit = BuildUnit(unitLocation);

            var maps = new BarbarianInfluenceMaps() { AllyPresence = new float[1] { 4.5f } };

            MockUnitPositionCanon.Setup(canon => canon.CanPlaceUnitAtLocation(unit, cellToTest, false)).Returns(true);

            MockGrid.Setup(grid => grid.GetDistance(cellToTest, unitLocation)).Returns(5);

            MockPlayerConfig.Setup(config => config.WanderSelectionWeight_Distance).Returns(5.5f);
            MockPlayerConfig.Setup(config => config.WanderSelectionWeight_Allies  ).Returns(5f);

            var brainTools = Container.Resolve<BarbarianBrainTools>();

            var returnedDelegate = brainTools.GetWanderWeightFunction(unit, maps);

            Assert.AreEqual(Mathf.RoundToInt(5 * 5.5f - 4.5f * 5f), returnedDelegate(cellToTest));
        }

        [Test]
        public void GetWanderWeightFunction_ReturnedDelegate_DoesNotGoBelowZero() {
            var cellToTest   = BuildCell(0);
            var unitLocation = BuildCell(1);

            var unit = BuildUnit(unitLocation);

            var maps = new BarbarianInfluenceMaps() { AllyPresence = new float[1] { 4.5f } };

            MockUnitPositionCanon.Setup(canon => canon.CanPlaceUnitAtLocation(unit, cellToTest, false)).Returns(true);

            MockGrid.Setup(grid => grid.GetDistance(cellToTest, unitLocation)).Returns(0);

            MockPlayerConfig.Setup(config => config.WanderSelectionWeight_Distance).Returns(0f);
            MockPlayerConfig.Setup(config => config.WanderSelectionWeight_Allies  ).Returns(5f);

            var brainTools = Container.Resolve<BarbarianBrainTools>();

            var returnedDelegate = brainTools.GetWanderWeightFunction(unit, maps);

            Assert.AreEqual(0f, returnedDelegate(cellToTest));
        }

        #endregion

        #region utilities

        private IHexCell BuildCell(int index) {
            var mockCell = new Mock<IHexCell>();

            mockCell.Setup(cell => cell.Index).Returns(index);

            return mockCell.Object;
        }

        private IUnit BuildUnit(IHexCell location) {
            var newUnit = new Mock<IUnit>().Object;

            MockUnitPositionCanon.Setup(canon => canon.GetOwnerOfPossession(newUnit)).Returns(location);

            return newUnit;
        }

        #endregion

        #endregion

    }

}
