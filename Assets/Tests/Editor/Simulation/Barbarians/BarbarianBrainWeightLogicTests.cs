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
using Assets.Simulation.Units.Combat;
using Assets.Simulation.Barbarians;
using Assets.Simulation.AI;

namespace Assets.Tests.Simulation.Barbarians {

    public class BarbarianBrainWeightLogicTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IHexGrid>               MockGrid;
        private Mock<IUnitPositionCanon>     MockUnitPositionCanon;
        private Mock<IBarbarianConfig>       MockBarbarianConfig;
        private Mock<ICombatExecuter>        MockCombatExecuter;
        private Mock<IUnitStrengthEstimator> MockUnitStrengthEstimator;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockGrid                  = new Mock<IHexGrid>();
            MockUnitPositionCanon     = new Mock<IUnitPositionCanon>();
            MockBarbarianConfig       = new Mock<IBarbarianConfig>();
            MockCombatExecuter        = new Mock<ICombatExecuter>();
            MockUnitStrengthEstimator = new Mock<IUnitStrengthEstimator>();

            Container.Bind<IHexGrid>              ().FromInstance(MockGrid                 .Object);
            Container.Bind<IUnitPositionCanon>    ().FromInstance(MockUnitPositionCanon    .Object);
            Container.Bind<IBarbarianConfig>      ().FromInstance(MockBarbarianConfig      .Object);
            Container.Bind<ICombatExecuter>       ().FromInstance(MockCombatExecuter       .Object);
            Container.Bind<IUnitStrengthEstimator>().FromInstance(MockUnitStrengthEstimator.Object);

            Container.Bind<BarbarianBrainWeightLogic>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void GetWanderWeightFunction_ReturnedDelegate_ReturnsZeroIfCellIsLocationOfUnit() {
            var cell = BuildCell(0);
            var unit = BuildUnit(cell);

            var maps = new InfluenceMaps() { AllyPresence = new float[1] { 1 } };

            MockUnitPositionCanon.Setup(canon => canon.CanPlaceUnitAtLocation(unit, cell, false)).Returns(true);

            var brainTools = Container.Resolve<BarbarianBrainWeightLogic>();

            var returnedDelegate = brainTools.GetWanderWeightFunction(unit, maps);

            Assert.AreEqual(0, returnedDelegate(cell));
        }

        [Test]
        public void GetWanderWeightFunction_ReturnedDelegate_ReturnsZeroIfCellIsImpassableToUnit() {
            var cell = BuildCell(0);
            var unit = BuildUnit(BuildCell(1));

            var maps = new InfluenceMaps() { AllyPresence = new float[1] { 1 } };

            MockUnitPositionCanon.Setup(canon => canon.CanPlaceUnitAtLocation(unit, cell, false)).Returns(false);

            var brainTools = Container.Resolve<BarbarianBrainWeightLogic>();

            var returnedDelegate = brainTools.GetWanderWeightFunction(unit, maps);

            Assert.AreEqual(0, returnedDelegate(cell));
        }

        [Test]
        public void GetWanderWeightFunction_ReturnedDelegate_ValueIncreasedFromDistanceToUnitLocation() {
            var cellToTest   = BuildCell(0);
            var unitLocation = BuildCell(1);

            var unit = BuildUnit(unitLocation);

            var maps = new InfluenceMaps() {
                AllyPresence  = new float[1] { 0 },
                EnemyPresence = new float[1]
            };

            MockUnitPositionCanon.Setup(canon => canon.CanPlaceUnitAtLocation(unit, cellToTest, false)).Returns(true);

            MockGrid.Setup(grid => grid.GetDistance(cellToTest, unitLocation)).Returns(5);

            MockBarbarianConfig.Setup(config => config.WanderSelectionWeight_Distance).Returns(5.5f);

            var brainTools = Container.Resolve<BarbarianBrainWeightLogic>();

            var returnedDelegate = brainTools.GetWanderWeightFunction(unit, maps);

            Assert.AreEqual(Mathf.RoundToInt(5 * 5.5f), returnedDelegate(cellToTest));
        }

        [Test]
        public void GetWanderWeightFunction_ReturnedDelegate_ValueDecreasedFromAllyPresenceOfCell() {
            var cellToTest   = BuildCell(0);
            var unitLocation = BuildCell(1);

            var unit = BuildUnit(unitLocation);

            var maps = new InfluenceMaps() {
                AllyPresence  = new float[1] { 4.5f },
                EnemyPresence = new float[1]
            };

            MockUnitPositionCanon.Setup(canon => canon.CanPlaceUnitAtLocation(unit, cellToTest, false)).Returns(true);

            MockGrid.Setup(grid => grid.GetDistance(cellToTest, unitLocation)).Returns(5);

            MockBarbarianConfig.Setup(config => config.WanderSelectionWeight_Distance).Returns(5.5f);
            MockBarbarianConfig.Setup(config => config.WanderSelectionWeight_Allies  ).Returns(5f);

            var brainTools = Container.Resolve<BarbarianBrainWeightLogic>();

            var returnedDelegate = brainTools.GetWanderWeightFunction(unit, maps);

            Assert.AreEqual(Mathf.RoundToInt(5 * 5.5f - 4.5f * 5f), returnedDelegate(cellToTest));
        }

        [Test]
        public void GetWanderWeightFunction_ReturnedDelegate_ValueDecreasedFromEnemyPresenceOfCell() {
            var cellToTest   = BuildCell(0);
            var unitLocation = BuildCell(1);

            var unit = BuildUnit(unitLocation);

            var maps = new InfluenceMaps() {
                AllyPresence  = new float[1] { 4.5f },
                EnemyPresence = new float[1] { 1.3f }
            };

            MockUnitPositionCanon.Setup(canon => canon.CanPlaceUnitAtLocation(unit, cellToTest, false)).Returns(true);

            MockGrid.Setup(grid => grid.GetDistance(cellToTest, unitLocation)).Returns(5);

            MockBarbarianConfig.Setup(config => config.WanderSelectionWeight_Distance).Returns(55f);
            MockBarbarianConfig.Setup(config => config.WanderSelectionWeight_Allies  ).Returns(5f);
            MockBarbarianConfig.Setup(config => config.WanderSelectionWeight_Enemies ).Returns(4f);

            var brainTools = Container.Resolve<BarbarianBrainWeightLogic>();

            var returnedDelegate = brainTools.GetWanderWeightFunction(unit, maps);

            Assert.AreEqual(Mathf.RoundToInt(5 * 55f - 4.5f * 5f - 1.3f * 4f), returnedDelegate(cellToTest));
        }

        [Test]
        public void GetWanderWeightFunction_ReturnedDelegate_DoesNotGoBelowZero() {
            var cellToTest   = BuildCell(0);
            var unitLocation = BuildCell(1);

            var unit = BuildUnit(unitLocation);

            var maps = new InfluenceMaps() {
                AllyPresence  = new float[1] { 4.5f },
                EnemyPresence = new float[1]
            };

            MockUnitPositionCanon.Setup(canon => canon.CanPlaceUnitAtLocation(unit, cellToTest, false)).Returns(true);

            MockGrid.Setup(grid => grid.GetDistance(cellToTest, unitLocation)).Returns(0);

            MockBarbarianConfig.Setup(config => config.WanderSelectionWeight_Distance).Returns(0f);
            MockBarbarianConfig.Setup(config => config.WanderSelectionWeight_Allies  ).Returns(5f);

            var brainTools = Container.Resolve<BarbarianBrainWeightLogic>();

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

            var brainTools = Container.Resolve<BarbarianBrainWeightLogic>();

            var returnedDelegate = brainTools.GetPillageWeightFunction(unit, maps);

            Assert.AreEqual(Mathf.RoundToInt(4.5f), returnedDelegate(cellOne),   "Delegate returned an unexpected value for CellOne");
            Assert.AreEqual(Mathf.RoundToInt(8.2f), returnedDelegate(cellTwo),   "Delegate returned an unexpected value for CellTwo");
            Assert.AreEqual(Mathf.RoundToInt(-10f), returnedDelegate(cellThree), "Delegate returned an unexpected value for CellThree");
        }

        [Test]
        public void GetFleeWeightFunction_ReturnedDelegate_IncreasedByEstimatedStrengthOnCell() {
            var unitLocation = BuildCell(-1);

            var unit = BuildUnit(unitLocation);

            var cell = BuildCell(1);

            MockUnitStrengthEstimator.Setup(estimator => estimator.EstimateUnitDefensiveStrength(unit, cell))
                                     .Returns(11f);

            var maps = new InfluenceMaps() {
                AllyPresence  = new float[] { 0f, 0f, 0f },
                EnemyPresence = new float[] { 0f, 0f, 0f }
            };

            var brainTools = Container.Resolve<BarbarianBrainWeightLogic>();

            var returnedDelegate = brainTools.GetFleeWeightFunction(unit, maps);

            Assert.AreEqual(11f, returnedDelegate(cell));
        }

        [Test]
        public void GetFleeWeightFunction_ReturnedDelegate_IncreasedByAllyPresenceOfCell() {
            var unitLocation = BuildCell(-1);

            var unit = BuildUnit(unitLocation);

            var cell = BuildCell(1);

            MockUnitStrengthEstimator.Setup(estimator => estimator.EstimateUnitDefensiveStrength(unit, cell))
                                     .Returns(11f);

            var maps = new InfluenceMaps() {
                AllyPresence  = new float[] { 1f, 2f, 3f },
                EnemyPresence = new float[] { 0f, 0f, 0f }
            };

            var brainTools = Container.Resolve<BarbarianBrainWeightLogic>();

            var returnedDelegate = brainTools.GetFleeWeightFunction(unit, maps);

            Assert.AreEqual(13f, returnedDelegate(cell));
        }

        [Test]
        public void GetFleeWeightFunction_ReturnedDelegate_DecreasedByEnemyPresenceOfCell() {
            var unitLocation = BuildCell(-1);

            var unit = BuildUnit(unitLocation);

            var cell = BuildCell(1);

            MockUnitStrengthEstimator.Setup(estimator => estimator.EstimateUnitDefensiveStrength(unit, cell))
                                     .Returns(11f);

            var maps = new InfluenceMaps() {
                AllyPresence  = new float[] { 0f, 0f, 0f },
                EnemyPresence = new float[] { 10f, 20f, 30f }
            };

            var brainTools = Container.Resolve<BarbarianBrainWeightLogic>();

            var returnedDelegate = brainTools.GetFleeWeightFunction(unit, maps);

            Assert.AreEqual(-9f, returnedDelegate(cell));
        }

        [Test]
        public void GetFleeWeightFunction_ReturnedDelegate_IncreasedByDistanceFromUnitLocationToCell() {
            var unitLocation = BuildCell(-1);

            var unit = BuildUnit(unitLocation);

            var cell = BuildCell(1);

            MockGrid.Setup(grid => grid.GetDistance(unitLocation, cell)).Returns(5);

            MockUnitStrengthEstimator.Setup(estimator => estimator.EstimateUnitDefensiveStrength(unit, cell))
                                     .Returns(11f);

            var maps = new InfluenceMaps() {
                AllyPresence  = new float[] { 0f, 0f, 0f },
                EnemyPresence = new float[] { 0f, 0f, 0f }
            };

            var brainTools = Container.Resolve<BarbarianBrainWeightLogic>();

            var returnedDelegate = brainTools.GetFleeWeightFunction(unit, maps);

            Assert.AreEqual(16f, returnedDelegate(cell));
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
