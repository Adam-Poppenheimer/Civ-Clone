using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using Zenject;
using Moq;

using Assets.Simulation.Core;
using Assets.Simulation.HexMap;
using Assets.Simulation.Improvements;
using Assets.Simulation.Units;

namespace Assets.Tests.Simulation.Core {

    public class UnitRoundExecuterTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IUnitFactory>                     MockUnitFactory;
        private Mock<IUnitHealingLogic>                MockUnitHealingLogic;
        private Mock<IImprovementDamageExecuter>       MockImprovementDamageExecuter;
        private Mock<IImprovementConstructionExecuter> MockIImprovementConstructionExecuter;

        private List<IUnit> AllUnits = new List<IUnit>();

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            AllUnits.Clear();

            MockUnitFactory                      = new Mock<IUnitFactory>();
            MockUnitHealingLogic                 = new Mock<IUnitHealingLogic>();
            MockImprovementDamageExecuter        = new Mock<IImprovementDamageExecuter>();
            MockIImprovementConstructionExecuter = new Mock<IImprovementConstructionExecuter>();

            MockUnitFactory.Setup(factory => factory.AllUnits).Returns(AllUnits);

            Container.Bind<IUnitFactory>                    ().FromInstance(MockUnitFactory                     .Object);
            Container.Bind<IUnitHealingLogic>               ().FromInstance(MockUnitHealingLogic                .Object);
            Container.Bind<IImprovementDamageExecuter>      ().FromInstance(MockImprovementDamageExecuter       .Object);
            Container.Bind<IImprovementConstructionExecuter>().FromInstance(MockIImprovementConstructionExecuter.Object);

            Container.Bind<UnitRoundExecuter>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void PerformStartOfRoundActions_ForEachUnit_PerformsHealingOnUnit() {
            var units = new List<IUnit>() {
                BuildUnit(0f, 0f), BuildUnit(0f, 0f), BuildUnit(0f, 0f)
            };

            var roundExecuter = Container.Resolve<UnitRoundExecuter>();

            roundExecuter.PerformStartOfRoundActions();

            foreach(var unit in units) {
                MockUnitHealingLogic.Verify(
                    logic => logic.PerformHealingOnUnit(unit), Times.Once, "Failed to perform healing on unit"
                );
            }
        }

        [Test]
        public void PerformStartOfRoundActions_ForEachUnit_SetsCurrentMovementToMaxMovement() {
            var units = new List<IUnit>() {
                BuildUnit(1f, 3f), BuildUnit(-1f, 4f), BuildUnit(3f, 2f)
            };

            var roundExecuter = Container.Resolve<UnitRoundExecuter>();

            roundExecuter.PerformStartOfRoundActions();

            foreach(var unit in units) {
                Assert.AreEqual(unit.MaxMovement, unit.CurrentMovement, "A unit has an unexpected CurrentMovement");
            }
        }

        [Test]
        public void PerformEndOfRoundActions_ForEachUnit_PerformsMovementOnUnit() {
            var unitMocks = new List<Mock<IUnit>>() {
                BuildUnitMock(0f, 0f), BuildUnitMock(0f, 0f), BuildUnitMock(0f, 0f)
            };

            var roundExecuter = Container.Resolve<UnitRoundExecuter>();

            roundExecuter.PerformEndOfRoundActions();

            foreach(var unitMock in unitMocks) {
                unitMock.Verify(mock => mock.PerformMovement(), Times.Once, "A unit was not ordered to move as expected");
            }
        }

        [Test]
        public void PerformEndOfRoundActions_ForEachUnit_CanAttackSetToTrue() {
            var unitMocks = new List<Mock<IUnit>>() {
                BuildUnitMock(0f, 0f), BuildUnitMock(0f, 0f), BuildUnitMock(0f, 0f)
            };

            var roundExecuter = Container.Resolve<UnitRoundExecuter>();

            roundExecuter.PerformEndOfRoundActions();

            foreach(var unitMock in unitMocks) {
                unitMock.VerifySet(mock => mock.CanAttack = true, Times.Once, "A unit didn't have its CanAttack property reset as expected");
            }
        }

        [Test]
        public void PerformEndOfRoundActions_ForEachUnit_PerformsDamageOnUnitFromImprovements() {
            var units = new List<IUnit>() {
                BuildUnit(0f, 0f), BuildUnit(0f, 0f), BuildUnit(0f, 0f)
            };

            var roundExecuter = Container.Resolve<UnitRoundExecuter>();

            roundExecuter.PerformEndOfRoundActions();

            foreach(var unit in units) {
                MockImprovementDamageExecuter.Verify(
                    executer => executer.PerformDamageOnUnitFromImprovements(unit),
                    Times.Once, "PerformDamageOnUnitFromImprovements not called on a unit as expected"
                );
            }
        }

        [Test]
        public void PerformEndofRoundActions_ForEachUnit_PerformConstructionOfImprovementsOnUnit() {
            var units = new List<IUnit>() {
                BuildUnit(0f, 0f), BuildUnit(0f, 0f), BuildUnit(0f, 0f)
            };

            var roundExecuter = Container.Resolve<UnitRoundExecuter>();

            roundExecuter.PerformEndOfRoundActions();

            foreach(var unit in units) {
                MockIImprovementConstructionExecuter.Verify(
                    executer => executer.PerformImprovementConstruction(unit),
                    Times.Once, "PerformImprovementConstruction not called on a unit as expected"
                );
            }
        }

        #endregion

        #region utilities

        private IUnit BuildUnit(
            float currentMovement, float maxMovement
        ) {
            Mock<IUnit> mock;

            return BuildUnit(currentMovement, maxMovement, out mock);
        }

        private Mock<IUnit> BuildUnitMock(
            float currentMovement, float maxMovement
        ) {
            Mock<IUnit> mock;

            BuildUnit(currentMovement, maxMovement, out mock);

            return mock;
        }

        private IUnit BuildUnit(
            float currentMovement, float maxMovement, out Mock<IUnit> mock
        ) {
            mock = new Mock<IUnit>();

            mock.SetupAllProperties();

            mock.Setup(unit => unit.MaxMovement).Returns(maxMovement);

            var newUnit = mock.Object;

            newUnit.CurrentMovement = currentMovement;

            AllUnits.Add(newUnit);

            return newUnit;
        }

        #endregion

        #endregion

    }

}
