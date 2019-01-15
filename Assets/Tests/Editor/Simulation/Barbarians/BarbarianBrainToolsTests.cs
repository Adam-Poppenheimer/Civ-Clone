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
using Assets.Simulation.Barbarians;
using Assets.Simulation.AI;

namespace Assets.Tests.Simulation.Barbarians {

    public class BarbarianBrainToolsTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IHexGrid>           MockGrid;
        private Mock<IUnitPositionCanon> MockUnitPositionCanon;
        private Mock<IBarbarianConfig>   MockBarbarianConfig;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockGrid              = new Mock<IHexGrid>();
            MockUnitPositionCanon = new Mock<IUnitPositionCanon>();
            MockBarbarianConfig      = new Mock<IBarbarianConfig>();

            Container.Bind<IHexGrid>          ().FromInstance(MockGrid             .Object);
            Container.Bind<IUnitPositionCanon>().FromInstance(MockUnitPositionCanon.Object);
            Container.Bind<IBarbarianConfig>  ().FromInstance(MockBarbarianConfig  .Object);

            Container.Bind<BarbarianBrainTools>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void GetPillageUtilityFunction_ReturnedDelegate_ReturnsPillagingValueOfCell_ModifiedByUtilityCoefficient() {
            var unitLocation = BuildCell(-1);
            var unit = BuildUnit(unitLocation);

            var cellOne   = BuildCell(0);
            var cellTwo   = BuildCell(1);
            var cellThree = BuildCell(2);

            MockBarbarianConfig.Setup(config => config.PillageUtilityCoefficient).Returns(0.01f);

            var maps = new InfluenceMaps() {
                PillagingValue = new float[] { 5f, 7.2f, 3.4f }
            };

            var brainTools = Container.Resolve<BarbarianBrainTools>();

            var returnedDelegate = brainTools.GetPillageUtilityFunction(unit, maps);

            Assert.AreEqual(5f   * 0.01f, returnedDelegate(cellOne),   "Delegate returned an unexpected value for CellOne");
            Assert.AreEqual(7.2f * 0.01f, returnedDelegate(cellTwo),   "Delegate returned an unexpected value for CellTwo");
            Assert.AreEqual(3.4f * 0.01f, returnedDelegate(cellThree), "Delegate returned an unexpected value for CellThree");
        }

        [Test]
        public void GetPillageUtilityFunction_ReturnedDelegate_UtilityDividedByDistancePlusOne() {
            var unitLocation = BuildCell(-1);
            var unit = BuildUnit(unitLocation);

            var cellOne   = BuildCell(0);
            var cellTwo   = BuildCell(1);
            var cellThree = BuildCell(2);

            MockGrid.Setup(grid => grid.GetDistance(unitLocation, cellOne  )).Returns(2);
            MockGrid.Setup(grid => grid.GetDistance(unitLocation, cellTwo  )).Returns(3);
            MockGrid.Setup(grid => grid.GetDistance(unitLocation, cellThree)).Returns(4);

            MockBarbarianConfig.Setup(config => config.PillageUtilityCoefficient).Returns(0.01f);

            var maps = new InfluenceMaps() {
                PillagingValue = new float[] { 5f, 7.2f, 3.4f }
            };

            var brainTools = Container.Resolve<BarbarianBrainTools>();

            var returnedDelegate = brainTools.GetPillageUtilityFunction(unit, maps);

            Assert.AreEqual(5f   * 0.01f / 3, returnedDelegate(cellOne),   "Delegate returned an unexpected value for CellOne");
            Assert.AreEqual(7.2f * 0.01f / 4, returnedDelegate(cellTwo),   "Delegate returned an unexpected value for CellTwo");
            Assert.AreEqual(3.4f * 0.01f / 5, returnedDelegate(cellThree), "Delegate returned an unexpected value for CellThree");
        }

        [Test]
        public void GetPillageUtilityFunction_ReturnedDelegate_DoesNotGoBelowZero() {
            var unitLocation = BuildCell(-1);
            var unit = BuildUnit(unitLocation);

            var cellOne = BuildCell(0);

            MockBarbarianConfig.Setup(config => config.PillageUtilityCoefficient).Returns(1f);

            var maps = new InfluenceMaps() {
                PillagingValue = new float[] { -5f }
            };

            var brainTools = Container.Resolve<BarbarianBrainTools>();

            var returnedDelegate = brainTools.GetPillageUtilityFunction(unit, maps);

            Assert.AreEqual(0f, returnedDelegate(cellOne));
        }

        [Test]
        public void GetPillageUtilityFunction_ReturnedDelegate_DoesNotGoAboveOne() {
            var unitLocation = BuildCell(-1);
            var unit = BuildUnit(unitLocation);

            var cellOne = BuildCell(0);

            MockBarbarianConfig.Setup(config => config.PillageUtilityCoefficient).Returns(1f);

            var maps = new InfluenceMaps() {
                PillagingValue = new float[] { 5f }
            };

            var brainTools = Container.Resolve<BarbarianBrainTools>();

            var returnedDelegate = brainTools.GetPillageUtilityFunction(unit, maps);

            Assert.AreEqual(1f, returnedDelegate(cellOne));
        }

        [Test]
        public void GetWanderWeightFunction_ReturnedDelegate_ReturnsZeroIfCellIsLocationOfUnit() {
            var cell = BuildCell(0);
            var unit = BuildUnit(cell);

            var maps = new InfluenceMaps() { AllyPresence = new float[1] { 1 } };

            MockUnitPositionCanon.Setup(canon => canon.CanPlaceUnitAtLocation(unit, cell, false)).Returns(true);

            var brainTools = Container.Resolve<BarbarianBrainTools>();

            var returnedDelegate = brainTools.GetWanderWeightFunction(unit, maps);

            Assert.AreEqual(0, returnedDelegate(cell));
        }

        [Test]
        public void GetWanderWeightFunction_ReturnedDelegate_ReturnsZeroIfCellIsImpassableToUnit() {
            var cell = BuildCell(0);
            var unit = BuildUnit(BuildCell(1));

            var maps = new InfluenceMaps() { AllyPresence = new float[1] { 1 } };

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

            var maps = new InfluenceMaps() { AllyPresence = new float[1] { 0 } };

            MockUnitPositionCanon.Setup(canon => canon.CanPlaceUnitAtLocation(unit, cellToTest, false)).Returns(true);

            MockGrid.Setup(grid => grid.GetDistance(cellToTest, unitLocation)).Returns(5);

            MockBarbarianConfig.Setup(config => config.WanderSelectionWeight_Distance).Returns(5.5f);

            var brainTools = Container.Resolve<BarbarianBrainTools>();

            var returnedDelegate = brainTools.GetWanderWeightFunction(unit, maps);

            Assert.AreEqual(Mathf.RoundToInt(5 * 5.5f), returnedDelegate(cellToTest));
        }

        [Test]
        public void GetWanderWeightFunction_ReturnedDelegate_ValueDecreasedFromAllyPresenceOfCell() {
            var cellToTest   = BuildCell(0);
            var unitLocation = BuildCell(1);

            var unit = BuildUnit(unitLocation);

            var maps = new InfluenceMaps() { AllyPresence = new float[1] { 4.5f } };

            MockUnitPositionCanon.Setup(canon => canon.CanPlaceUnitAtLocation(unit, cellToTest, false)).Returns(true);

            MockGrid.Setup(grid => grid.GetDistance(cellToTest, unitLocation)).Returns(5);

            MockBarbarianConfig.Setup(config => config.WanderSelectionWeight_Distance).Returns(5.5f);
            MockBarbarianConfig.Setup(config => config.WanderSelectionWeight_Allies  ).Returns(5f);

            var brainTools = Container.Resolve<BarbarianBrainTools>();

            var returnedDelegate = brainTools.GetWanderWeightFunction(unit, maps);

            Assert.AreEqual(Mathf.RoundToInt(5 * 5.5f - 4.5f * 5f), returnedDelegate(cellToTest));
        }

        [Test]
        public void GetWanderWeightFunction_ReturnedDelegate_DoesNotGoBelowZero() {
            var cellToTest   = BuildCell(0);
            var unitLocation = BuildCell(1);

            var unit = BuildUnit(unitLocation);

            var maps = new InfluenceMaps() { AllyPresence = new float[1] { 4.5f } };

            MockUnitPositionCanon.Setup(canon => canon.CanPlaceUnitAtLocation(unit, cellToTest, false)).Returns(true);

            MockGrid.Setup(grid => grid.GetDistance(cellToTest, unitLocation)).Returns(0);

            MockBarbarianConfig.Setup(config => config.WanderSelectionWeight_Distance).Returns(0f);
            MockBarbarianConfig.Setup(config => config.WanderSelectionWeight_Allies  ).Returns(5f);

            var brainTools = Container.Resolve<BarbarianBrainTools>();

            var returnedDelegate = brainTools.GetWanderWeightFunction(unit, maps);

            Assert.AreEqual(0f, returnedDelegate(cellToTest));
        }

        [Test]
        public void GetPillageWeightFunction_ReturnedDelegate_ReturnsPillagingValueOfCell() {
            var cellOne   = BuildCell(0);
            var cellTwo   = BuildCell(1);
            var cellThree = BuildCell(2);

            var unit = BuildUnit(BuildCell(3));

            var maps = new InfluenceMaps() { PillagingValue = new float[] { 4.5f, 8.2f, -10f } };

            var brainTools = Container.Resolve<BarbarianBrainTools>();

            var returnedDelegate = brainTools.GetPillageWeightFunction(unit, maps);

            Assert.AreEqual(Mathf.RoundToInt(4.5f), returnedDelegate(cellOne),   "Delegate returned an unexpected value for CellOne");
            Assert.AreEqual(Mathf.RoundToInt(8.2f), returnedDelegate(cellTwo),   "Delegate returned an unexpected value for CellTwo");
            Assert.AreEqual(Mathf.RoundToInt(-10f), returnedDelegate(cellThree), "Delegate returned an unexpected value for CellThree");
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
