using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using NUnit.Framework;
using Zenject;
using Moq;

using Assets.Simulation.AI;
using Assets.Simulation.Barbarians;
using Assets.Simulation.HexMap;
using Assets.Simulation.Units;

namespace Assets.Tests.Simulation.Barbarians {

    public class BarbarianUtilityLogicTests : ZenjectUnitTestFixture {

        #region instance fields and properties
        private Mock<IUnitPositionCanon> MockUnitPositionCanon;
        private Mock<IHexGrid>           MockGrid;
        private Mock<IBarbarianConfig>   MockBarbarianConfig;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockUnitPositionCanon = new Mock<IUnitPositionCanon>();
            MockGrid              = new Mock<IHexGrid>();
            MockBarbarianConfig   = new Mock<IBarbarianConfig>();

            Container.Bind<IUnitPositionCanon>().FromInstance(MockUnitPositionCanon.Object);
            Container.Bind<IHexGrid>          ().FromInstance(MockGrid             .Object);
            Container.Bind<IBarbarianConfig>  ().FromInstance(MockBarbarianConfig  .Object);

            Container.Bind<BarbarianUtilityLogic>().AsSingle();
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

            var utilityLogic = Container.Resolve<BarbarianUtilityLogic>();

            var returnedDelegate = utilityLogic.GetPillageUtilityFunction(unit, maps);

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

            var utilityLogic = Container.Resolve<BarbarianUtilityLogic>();

            var returnedDelegate = utilityLogic.GetPillageUtilityFunction(unit, maps);

            Assert.That(Mathf.Approximately(5f   * 0.01f / 3, returnedDelegate(cellOne  )), "Delegate returned an unexpected value for CellOne");
            Assert.That(Mathf.Approximately(7.2f * 0.01f / 4, returnedDelegate(cellTwo  )), "Delegate returned an unexpected value for CellTwo");
            Assert.That(Mathf.Approximately(3.4f * 0.01f / 5, returnedDelegate(cellThree)), "Delegate returned an unexpected value for CellThree");
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

            var utilityLogic = Container.Resolve<BarbarianUtilityLogic>();

            var returnedDelegate = utilityLogic.GetPillageUtilityFunction(unit, maps);

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

            var utilityLogic = Container.Resolve<BarbarianUtilityLogic>();

            var returnedDelegate = utilityLogic.GetPillageUtilityFunction(unit, maps);

            Assert.AreEqual(1f, returnedDelegate(cellOne));
        }

        [Test]
        public void GetAttackUtilityFunction_ReturnedDelegate_FollowsLogisticCurve_WithCorrectSigmoidAndGrowthRate() {
            var cell = BuildCell(0);

            var unit = BuildUnit(BuildCell(-1));

            var maps = new InfluenceMaps() {
                AllyPresence  = new float[] { 0f },
                EnemyPresence = new float[] { 0f }
            };

            MockBarbarianConfig.Setup(config => config.AttackUtilityLogisticsSlope).Returns(5.2f);

            var utilityLogic = Container.Resolve<BarbarianUtilityLogic>();
            
            var returnedDelegate = utilityLogic.GetAttackUtilityFunction(unit, maps);

            Assert.AreEqual(
                AIMath.NormalizedLogisticCurve(0f, 5.2f, 0f), returnedDelegate(cell)
            );
        }

        [Test]
        public void GetAttackUtilityFunction_ReturnedDelegate_LogisticXIncreasedByAllyPresence() {
            var cell = BuildCell(1);

            var unit = BuildUnit(BuildCell(-1));

            var maps = new InfluenceMaps() {
                AllyPresence  = new float[] { 0f, 15f, 0f },
                EnemyPresence = new float[] { 0f, 0f,  0f }
            };

            MockBarbarianConfig.Setup(config => config.AttackUtilityLogisticsSlope).Returns(5.2f);

            var utilityLogic = Container.Resolve<BarbarianUtilityLogic>();
            
            var returnedDelegate = utilityLogic.GetAttackUtilityFunction(unit, maps);

            Assert.AreEqual(
                AIMath.NormalizedLogisticCurve(0f, 5.2f, 15f), returnedDelegate(cell)
            );
        }

        [Test]
        public void GetAttackUtilityFunction_ReturnedDelegate_LogisticXDecreasedByEnemyPresence() {
            var cell = BuildCell(1);

            var unit = BuildUnit(BuildCell(-1));

            var maps = new InfluenceMaps() {
                AllyPresence  = new float[] { 0f, 15f,   0f },
                EnemyPresence = new float[] { 0f, 10.8f, 0f }
            };

            MockBarbarianConfig.Setup(config => config.AttackUtilityLogisticsSlope).Returns(5.2f);

            var utilityLogic = Container.Resolve<BarbarianUtilityLogic>();
            
            var returnedDelegate = utilityLogic.GetAttackUtilityFunction(unit, maps);

            Assert.AreEqual(
                AIMath.NormalizedLogisticCurve(0f, 5.2f, 15f - 10.8f), returnedDelegate(cell)
            );
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
