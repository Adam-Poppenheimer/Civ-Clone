using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation.HexMap;
using Assets.Simulation.Units;
using Assets.Simulation.Units.Combat;

namespace Assets.Tests.Simulation.Units.Combat {

    [TestFixture]
    public class CombatDestructionLogicTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IUnitPositionCanon> MockUnitPositionCanon;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockUnitPositionCanon = new Mock<IUnitPositionCanon>();

            Container.Bind<IUnitPositionCanon>().FromInstance(MockUnitPositionCanon.Object);

            Container.Bind<CombatDestructionLogic>().AsSingle();
        }

        #endregion

        #region tests

        [Test(Description = "")]
        public void HandleUnitDestructionFromCombat_DestroysAttackerIfHealthNotPositive() {
            Mock<IUnit> attackerMock, defenderMock;

            var attacker = BuildUnit(0,  UnitType.Melee, BuildCell(), out attackerMock);
            var defender = BuildUnit(50, UnitType.Melee, BuildCell(), out defenderMock);

            var combatInfo = new CombatInfo() { CombatType = CombatType.Melee };

            var destructionLogic = Container.Resolve<CombatDestructionLogic>();

            destructionLogic.HandleUnitDestructionFromCombat(attacker, defender, combatInfo);

            attackerMock.Verify(unit => unit.Destroy(), Times.Once, "Attacker was not destroyed");
        }

        [Test(Description = "")]
        public void HandleUnitDestructionFromCombat_DoesNotDestroyAttackerIfOfTypeCity() {
            Mock<IUnit> attackerMock, defenderMock;

            var attacker = BuildUnit(0,  UnitType.City,  BuildCell(), out attackerMock);
            var defender = BuildUnit(50, UnitType.Melee, BuildCell(), out defenderMock);

            var combatInfo = new CombatInfo() { CombatType = CombatType.Melee };

            var destructionLogic = Container.Resolve<CombatDestructionLogic>();

            destructionLogic.HandleUnitDestructionFromCombat(attacker, defender, combatInfo);

            attackerMock.Verify(unit => unit.Destroy(), Times.Never, "Attacker was unexpectedly destroyed");
        }

        [Test(Description = "")]
        public void HandleUnitDestructionFromCombat_DoesNotDestroyAttackerIfHealthPositive() {
            Mock<IUnit> attackerMock, defenderMock;

            var attacker = BuildUnit(1,  UnitType.Melee, BuildCell(), out attackerMock);
            var defender = BuildUnit(50, UnitType.Melee, BuildCell(), out defenderMock);

            var combatInfo = new CombatInfo() { CombatType = CombatType.Melee };

            var destructionLogic = Container.Resolve<CombatDestructionLogic>();

            destructionLogic.HandleUnitDestructionFromCombat(attacker, defender, combatInfo);

            attackerMock.Verify(unit => unit.Destroy(), Times.Never, "Attacker was unexpectedly destroyed");
        }

        [Test(Description = "")]
        public void HandleUnitDestructionFromCombat_DestroysDefenderIfHealthNotPositive() {
            Mock<IUnit> attackerMock, defenderMock;

            var attacker = BuildUnit(50, UnitType.Melee, BuildCell(), out attackerMock);
            var defender = BuildUnit(0,  UnitType.Melee, BuildCell(), out defenderMock);

            var combatInfo = new CombatInfo() { CombatType = CombatType.Melee };

            var destructionLogic = Container.Resolve<CombatDestructionLogic>();

            destructionLogic.HandleUnitDestructionFromCombat(attacker, defender, combatInfo);

            defenderMock.Verify(unit => unit.Destroy(), Times.Once, "Defender was not destroyed as expected");
        }

        [Test(Description = "")]
        public void HandleUnitDestructionFromCombat_DoesNotDestroyDefenderIfOfTypeCity() {
            Mock<IUnit> attackerMock, defenderMock;

            var attacker = BuildUnit(50, UnitType.Melee, BuildCell(), out attackerMock);
            var defender = BuildUnit(0,  UnitType.City,  BuildCell(), out defenderMock);

            var combatInfo = new CombatInfo() { CombatType = CombatType.Melee };

            var destructionLogic = Container.Resolve<CombatDestructionLogic>();

            destructionLogic.HandleUnitDestructionFromCombat(attacker, defender, combatInfo);

            defenderMock.Verify(unit => unit.Destroy(), Times.Never, "Defender was unexpectedly destroyed");
        }

        [Test(Description = "")]
        public void HandleUnitDestructionFromCombat_SetsCityHealthToOneInsteadOfDestroying() {
            Mock<IUnit> attackerMock, defenderMock;

            var attacker = BuildUnit(50, UnitType.Melee, BuildCell(), out attackerMock);
            var defender = BuildUnit(0,  UnitType.City,  BuildCell(), out defenderMock);

            var combatInfo = new CombatInfo() { CombatType = CombatType.Melee };

            var destructionLogic = Container.Resolve<CombatDestructionLogic>();

            destructionLogic.HandleUnitDestructionFromCombat(attacker, defender, combatInfo);

            Assert.AreEqual(1, defender.Hitpoints);
        }

        [Test(Description = "")]
        public void HandleUnitDestructionFromCombat_DoesNotDestroyDefenderIfHealthPositive() {
            Mock<IUnit> attackerMock, defenderMock;

            var attacker = BuildUnit(50, UnitType.Melee, BuildCell(), out attackerMock);
            var defender = BuildUnit(1,  UnitType.City,  BuildCell(), out defenderMock);

            var combatInfo = new CombatInfo() { CombatType = CombatType.Melee };

            var destructionLogic = Container.Resolve<CombatDestructionLogic>();

            destructionLogic.HandleUnitDestructionFromCombat(attacker, defender, combatInfo);

            defenderMock.Verify(unit => unit.Destroy(), Times.Never, "Defender was unexpectedly destroyed");
        }

        [Test(Description = "When the defender dies and the attacker lives in melee combat, " +
            "HandleUnitDestructionFromCombat should set the attacker's current path to the " +
            "defender's old location and call attacker.PerformMovement(false)")]
        public void HandleUnitDestructionFromCombat_MovesAttackerOntoLocationWhenValid() {
            Mock<IUnit> attackerMock, defenderMock;

            var defenderLocation = BuildCell();

            var attacker = BuildUnit(50, UnitType.Melee, BuildCell(),      out attackerMock);
            var defender = BuildUnit(0,  UnitType.Melee, defenderLocation, out defenderMock);

            var combatInfo = new CombatInfo() { CombatType = CombatType.Melee };

            var destructionLogic = Container.Resolve<CombatDestructionLogic>();

            destructionLogic.HandleUnitDestructionFromCombat(attacker, defender, combatInfo);

            CollectionAssert.AreEqual(
                new List<IHexCell>() { defenderLocation }, attacker.CurrentPath,
                "attacker.CurrentPath has an unexpected value"
            );

            attackerMock.Verify(
                unit => unit.PerformMovement(false), Times.Once,
                "attacker.PerformMovement wasn't called as expected"
            );
        }

        [Test(Description = "")]
        public void HandleUnitDestructionFromCombat_AttackerDoesNotMoveIfCombatNotMelee() {
            Mock<IUnit> attackerMock, defenderMock;

            var defenderLocation = BuildCell();

            var attacker = BuildUnit(50, UnitType.Melee, BuildCell(),      out attackerMock);
            var defender = BuildUnit(0,  UnitType.Melee, defenderLocation, out defenderMock);

            var combatInfo = new CombatInfo() { CombatType = CombatType.Ranged };

            var destructionLogic = Container.Resolve<CombatDestructionLogic>();

            destructionLogic.HandleUnitDestructionFromCombat(attacker, defender, combatInfo);

            Assert.Null(
                attacker.CurrentPath,
                "attacker.CurrentPath has an unexpected value"
            );

            attackerMock.Verify(
                unit => unit.PerformMovement(It.IsAny<bool>()), Times.Never,
                "attacker.PerformMovement was unexpectedly called"
            );
        }

        [Test(Description = "")]
        public void HandleUnitDestructionFromCombat_AttackerDoesNotMoveIfAttackerDead() {
            Mock<IUnit> attackerMock, defenderMock;

            var defenderLocation = BuildCell();

            var attacker = BuildUnit(0, UnitType.Melee, BuildCell(),      out attackerMock);
            var defender = BuildUnit(0, UnitType.Melee, defenderLocation, out defenderMock);

            var combatInfo = new CombatInfo() { CombatType = CombatType.Melee };

            var destructionLogic = Container.Resolve<CombatDestructionLogic>();

            destructionLogic.HandleUnitDestructionFromCombat(attacker, defender, combatInfo);

            Assert.Null(
                attacker.CurrentPath,
                "attacker.CurrentPath has an unexpected value"
            );

            attackerMock.Verify(
                unit => unit.PerformMovement(It.IsAny<bool>()), Times.Never,
                "attacker.PerformMovement was unexpectedly called"
            );
        }

        #endregion

        #region utilities

        private IUnit BuildUnit(int hitpoints, UnitType type, IHexCell location, out Mock<IUnit> mock) {
            mock = new Mock<IUnit>();

            mock.SetupAllProperties();

            mock.Setup(unit => unit.Type).Returns(type);

            var newUnit = mock.Object;

            newUnit.Hitpoints = hitpoints;

            MockUnitPositionCanon.Setup(canon => canon.GetOwnerOfPossession(newUnit)).Returns(location);

            return newUnit;
        }

        private IHexCell BuildCell() {
            return new Mock<IHexCell>().Object;
        }

        #endregion

        #endregion

    }

}
