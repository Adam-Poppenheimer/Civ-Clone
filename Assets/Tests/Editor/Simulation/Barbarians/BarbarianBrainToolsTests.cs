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

    public class BarbarianBrainToolsTests : ZenjectUnitTestFixture {

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
        public void GetCaptureCivilianFilter_ReturnedDelegate_ReturnsTrueIfCellHasCiviliansTheCaptorCanAttack() {
            var unit = BuildUnit(UnitType.Melee);

            var cell = BuildCell(
                BuildUnit(UnitType.Civilian), BuildUnit(UnitType.Civilian), BuildUnit(UnitType.Civilian)
            );

            MockCombatExecuter.Setup(executer => executer.CanPerformMeleeAttack(unit, It.IsAny<IUnit>()))
                              .Returns(true);

            var brainTools = Container.Resolve<BarbarianBrainTools>();

            var returnedDelegate = brainTools.GetCaptureCivilianFilter(unit);

            Assert.IsTrue(returnedDelegate(cell));
        }

        [Test]
        public void GetCaptureCivilianFilter_ReturnedDelegate_ReturnsFalseIfCivilianCannotBeAttacked() {
            var unit = BuildUnit(UnitType.Melee);

            var unitOne   = BuildUnit(UnitType.Civilian);
            var unitTwo   = BuildUnit(UnitType.Civilian);
            var unitThree = BuildUnit(UnitType.Civilian);

            var cell = BuildCell(unitOne, unitTwo, unitThree);

            MockCombatExecuter.Setup(executer => executer.CanPerformMeleeAttack(unit, unitOne  )).Returns(true);
            MockCombatExecuter.Setup(executer => executer.CanPerformMeleeAttack(unit, unitTwo  )).Returns(false);
            MockCombatExecuter.Setup(executer => executer.CanPerformMeleeAttack(unit, unitThree)).Returns(true);

            var brainTools = Container.Resolve<BarbarianBrainTools>();

            var returnedDelegate = brainTools.GetCaptureCivilianFilter(unit);

            Assert.IsFalse(returnedDelegate(cell));
        }

        [Test]
        public void GetCaptureCivilianFilter_ReturnedDelegate_ReturnsFalseIfNonCivilianUnitAtLocation() {
            var unit = BuildUnit(UnitType.Melee);

            var cell = BuildCell(
                BuildUnit(UnitType.Civilian), BuildUnit(UnitType.Civilian), BuildUnit(UnitType.Melee)
            );

            MockCombatExecuter.Setup(executer => executer.CanPerformMeleeAttack(unit, It.IsAny<IUnit>()))
                              .Returns(true);

            var brainTools = Container.Resolve<BarbarianBrainTools>();

            var returnedDelegate = brainTools.GetCaptureCivilianFilter(unit);

            Assert.IsFalse(returnedDelegate(cell));
        }

        [Test]
        public void GetCaptureCivilianFilter_ReturnedDelegate_ReturnsFalseIfNoUnitsAtLocation() {
            var unit = BuildUnit(UnitType.Melee);

            var cell = BuildCell();

            MockCombatExecuter.Setup(executer => executer.CanPerformMeleeAttack(unit, It.IsAny<IUnit>()))
                              .Returns(true);

            var brainTools = Container.Resolve<BarbarianBrainTools>();

            var returnedDelegate = brainTools.GetCaptureCivilianFilter(unit);

            Assert.IsFalse(returnedDelegate(cell));
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

            var maps = new InfluenceMaps() {
                AllyPresence  = new float[1] { 0 },
                EnemyPresence = new float[1]
            };

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

            var maps = new InfluenceMaps() {
                AllyPresence  = new float[1] { 4.5f },
                EnemyPresence = new float[1]
            };

            MockUnitPositionCanon.Setup(canon => canon.CanPlaceUnitAtLocation(unit, cellToTest, false)).Returns(true);

            MockGrid.Setup(grid => grid.GetDistance(cellToTest, unitLocation)).Returns(5);

            MockBarbarianConfig.Setup(config => config.WanderSelectionWeight_Distance).Returns(5.5f);
            MockBarbarianConfig.Setup(config => config.WanderSelectionWeight_Allies  ).Returns(5f);

            var brainTools = Container.Resolve<BarbarianBrainTools>();

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

            var brainTools = Container.Resolve<BarbarianBrainTools>();

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

            var brainTools = Container.Resolve<BarbarianBrainTools>();

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

            var brainTools = Container.Resolve<BarbarianBrainTools>();

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

            var brainTools = Container.Resolve<BarbarianBrainTools>();

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

            var brainTools = Container.Resolve<BarbarianBrainTools>();

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

        private IHexCell BuildCell(params IUnit[] units) {
            var newCell = new Mock<IHexCell>().Object;

            MockUnitPositionCanon.Setup(canon => canon.GetPossessionsOfOwner(newCell)).Returns(units);

            return newCell;
        }

        private IUnit BuildUnit(IHexCell location) {
            var newUnit = new Mock<IUnit>().Object;

            MockUnitPositionCanon.Setup(canon => canon.GetOwnerOfPossession(newUnit)).Returns(location);

            return newUnit;
        }

        private IUnit BuildUnit(UnitType type) {
            var mockUnit = new Mock<IUnit>();

            mockUnit.Setup(unit => unit.Type).Returns(type);

            return mockUnit.Object;
        }

        #endregion

        #endregion

    }

}
