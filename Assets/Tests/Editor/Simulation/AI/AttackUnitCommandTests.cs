using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using Zenject;
using Moq;

using Assets.Simulation.AI;
using Assets.Simulation.HexMap;
using Assets.Simulation.Units;
using Assets.Simulation.Units.Combat;

namespace Assets.Tests.Simulation.AI {

    public class AttackUnitCommandTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<ICombatExecuter>       MockCombatExecuter;
        private Mock<IUnitAttackOrderLogic> MockAttackOrderLogic;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockCombatExecuter   = new Mock<ICombatExecuter>();
            MockAttackOrderLogic = new Mock<IUnitAttackOrderLogic>();

            Container.Bind<ICombatExecuter>      ().FromInstance(MockCombatExecuter  .Object);
            Container.Bind<IUnitAttackOrderLogic>().FromInstance(MockAttackOrderLogic.Object);

            Container.Bind<AttackUnitCommand>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void StartExecution_AndNoAttackTargetsOnCell_StatusSetToFailed() {
            var attacker = BuildUnit(true, true);

            var locationToAttack = BuildCell(null);

            var attackCommand = Container.Resolve<AttackUnitCommand>();

            attackCommand.Attacker         = attacker;
            attackCommand.LocationToAttack = locationToAttack;
            attackCommand.CombatType       = CombatType.Melee;

            attackCommand.StartExecution();

            Assert.AreEqual(CommandStatus.Failed, attackCommand.Status);
        }

        [Test]
        public void StartExecution_CombatTypeIsMelee_AndMeleeAttackValid_PerformsMeleeAttack() {
            var attacker = BuildUnit(true, true);
            var defender = BuildUnit();

            var locationToAttack = BuildCell(defender);

            MockCombatExecuter.Setup(executer => executer.CanPerformMeleeAttack(attacker, defender)).Returns(true);

            var attackCommand = Container.Resolve<AttackUnitCommand>();

            attackCommand.Attacker         = attacker;
            attackCommand.LocationToAttack = locationToAttack;
            attackCommand.CombatType       = CombatType.Melee;

            attackCommand.StartExecution();

            MockCombatExecuter.Verify(
                executer => executer.PerformMeleeAttack(attacker, defender, It.IsAny<Action>(), It.IsAny<Action>()),
                Times.Once, "PerformMeleeAttack not called as expected"
            );
        }

        [Test]
        public void StartExecution_CombatTypeIsMelee_AndMeleeAttackNotValid_MeleeAttackNotPerformed_AndStatusSetToFailed() {
            var attacker = BuildUnit(true, true);
            var defender = BuildUnit();

            var locationToAttack = BuildCell(defender);

            MockCombatExecuter.Setup(executer => executer.CanPerformMeleeAttack(attacker, defender)).Returns(false);

            var attackCommand = Container.Resolve<AttackUnitCommand>();

            attackCommand.Attacker         = attacker;
            attackCommand.LocationToAttack = locationToAttack;
            attackCommand.CombatType       = CombatType.Melee;

            attackCommand.StartExecution();

            MockCombatExecuter.Verify(
                executer => executer.PerformMeleeAttack(attacker, defender, It.IsAny<Action>(), It.IsAny<Action>()),
                Times.Never, "PerformMeleeAttack unexpectedly called"
            );

            Assert.AreEqual(CommandStatus.Failed, attackCommand.Status, "AttackCommand.Status has an unexpected value");
        }

        [Test]
        public void StartExecution_MeleeAttackOccurs_AndAttackSucceeds_StatusSetToSucceeded() {
            var attacker = BuildUnit(true, true);
            var defender = BuildUnit();

            var locationToAttack = BuildCell(defender);

            MockCombatExecuter.Setup(executer => executer.CanPerformMeleeAttack(attacker, defender)).Returns(true);

            MockCombatExecuter.Setup(
                executer => executer.PerformMeleeAttack(attacker, defender, It.IsAny<Action>(), It.IsAny<Action>())
            ).Callback<IUnit, IUnit, Action, Action>(
                (attack, defend, succeed, fail) => succeed()
            );

            var attackCommand = Container.Resolve<AttackUnitCommand>();

            attackCommand.Attacker         = attacker;
            attackCommand.LocationToAttack = locationToAttack;
            attackCommand.CombatType       = CombatType.Melee;

            attackCommand.StartExecution();

            Assert.AreEqual(CommandStatus.Succeeded, attackCommand.Status);
        }

        [Test]
        public void StartExecution_MeleeAttackOccurs_AndAttackFails_StatuSetToFailed() {
            var attacker = BuildUnit(true, true);
            var defender = BuildUnit();

            var locationToAttack = BuildCell(defender);

            MockCombatExecuter.Setup(executer => executer.CanPerformMeleeAttack(attacker, defender)).Returns(true);

            MockCombatExecuter.Setup(
                executer => executer.PerformMeleeAttack(attacker, defender, It.IsAny<Action>(), It.IsAny<Action>())
            ).Callback<IUnit, IUnit, Action, Action>(
                (attack, defend, succeed, fail) => fail()
            );

            var attackCommand = Container.Resolve<AttackUnitCommand>();

            attackCommand.Attacker         = attacker;
            attackCommand.LocationToAttack = locationToAttack;
            attackCommand.CombatType       = CombatType.Melee;

            attackCommand.StartExecution();

            Assert.AreEqual(CommandStatus.Failed, attackCommand.Status);
        }

        [Test]
        public void StartExecution_CombatTypeIsRanged_AndRangedAttackValid_PerformsRangedAttack() {
            var attacker = BuildUnit(true, true);
            var defender = BuildUnit();

            var locationToAttack = BuildCell(defender);

            MockCombatExecuter.Setup(executer => executer.CanPerformRangedAttack(attacker, defender)).Returns(true);

            var attackCommand = Container.Resolve<AttackUnitCommand>();

            attackCommand.Attacker         = attacker;
            attackCommand.LocationToAttack = locationToAttack;
            attackCommand.CombatType       = CombatType.Ranged;

            attackCommand.StartExecution();

            MockCombatExecuter.Verify(executer => executer.PerformRangedAttack(attacker, defender), Times.Once);
        }

        [Test]
        public void StartExecution_CombatTypeIsRanged_AndRangedAttackValid_StatusSetToSucceeded() {
            var attacker = BuildUnit(true, true);
            var defender = BuildUnit();

            var locationToAttack = BuildCell(defender);

            MockCombatExecuter.Setup(executer => executer.CanPerformRangedAttack(attacker, defender)).Returns(true);

            var attackCommand = Container.Resolve<AttackUnitCommand>();

            attackCommand.Attacker         = attacker;
            attackCommand.LocationToAttack = locationToAttack;
            attackCommand.CombatType       = CombatType.Ranged;

            attackCommand.StartExecution();

            Assert.AreEqual(CommandStatus.Succeeded, attackCommand.Status);
        }

        [Test]
        public void StartExecutionCombatTypeIsRanged_AndRangedAttackNotValid_RangedAttackNotPerformed_AndStatusSetToFailed() {
            var attacker = BuildUnit(true, true);
            var defender = BuildUnit();

            var locationToAttack = BuildCell(defender);

            MockCombatExecuter.Setup(executer => executer.CanPerformRangedAttack(attacker, defender)).Returns(false);

            var attackCommand = Container.Resolve<AttackUnitCommand>();

            attackCommand.Attacker         = attacker;
            attackCommand.LocationToAttack = locationToAttack;
            attackCommand.CombatType       = CombatType.Ranged;

            attackCommand.StartExecution();

            MockCombatExecuter.Verify(
                executer => executer.PerformRangedAttack(attacker, defender),
                Times.Never, "PerformRangedAttack was unexpectedly called"
            );

            Assert.AreEqual(CommandStatus.Failed, attackCommand.Status, "AttackCommand.Status has an unexpected value");
        }

        [Test]
        public void StartExecution_AndAttackerIsNull_ThrowsInvalidOperationException() {
            var attackCommand = Container.Resolve<AttackUnitCommand>();

            attackCommand.Attacker         = null;
            attackCommand.LocationToAttack = BuildCell(BuildUnit());
            attackCommand.CombatType       = CombatType.Ranged;

            Assert.Throws<InvalidOperationException>(() => attackCommand.StartExecution());
        }

        [Test]
        public void StartExecution_AndLocationToAttackIsNull_ThrowsInvalidOperationException() {
            var attackCommand = Container.Resolve<AttackUnitCommand>();

            attackCommand.Attacker         = BuildUnit();
            attackCommand.LocationToAttack = null;
            attackCommand.CombatType       = CombatType.Ranged;

            Assert.Throws<InvalidOperationException>(() => attackCommand.StartExecution());
        }

        #endregion

        #region utilities

        private IUnit BuildUnit(bool canPerformMeleeAttack = false, bool canPerformRangedAttack = false) {
            var newUnit = new Mock<IUnit>().Object;

            MockCombatExecuter.Setup(
                executer => executer.CanPerformMeleeAttack(newUnit, It.IsAny<IUnit>())
            ).Returns(canPerformMeleeAttack);

            MockCombatExecuter.Setup(
                executer => executer.CanPerformRangedAttack(newUnit, It.IsAny<IUnit>())
            ).Returns(canPerformRangedAttack);

            return newUnit;
        }

        private IHexCell BuildCell(IUnit nextAttackTarget) {
            var newCell = new Mock<IHexCell>().Object;

            MockAttackOrderLogic.Setup(logic => logic.GetNextAttackTargetOnCell(newCell)).Returns(nextAttackTarget);

            return newCell;
        }

        #endregion

        #endregion

    }

}
