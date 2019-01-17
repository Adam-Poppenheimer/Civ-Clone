using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation;
using Assets.Simulation.Civilizations;
using Assets.Simulation.HexMap;
using Assets.Simulation.Units;
using Assets.Simulation.Units.Combat;

namespace Assets.Tests.Simulation.Units.Combat {

    [TestFixture]
    public class DestructionPostCombatResponderTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IUnitPositionCanon>                            MockUnitPositionCanon;        
        private Mock<IUnitConfig>                                   MockUnitConfig;
        private Mock<IPossessionRelationship<ICivilization, IUnit>> MockUnitPossessionCanon;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockUnitPositionCanon   = new Mock<IUnitPositionCanon>();
            MockUnitConfig          = new Mock<IUnitConfig>();
            MockUnitPossessionCanon = new Mock<IPossessionRelationship<ICivilization, IUnit>>();

            Container.Bind<IUnitPositionCanon>                           ().FromInstance(MockUnitPositionCanon  .Object);
            Container.Bind<IUnitConfig>                                  ().FromInstance(MockUnitConfig         .Object);
            Container.Bind<IPossessionRelationship<ICivilization, IUnit>>().FromInstance(MockUnitPossessionCanon.Object);

            Container.Bind<DestructionPostCombatResponder>().AsSingle();
        }

        #endregion

        #region tests

        [Test(Description = "")]
        public void RespondToCombat_DestroysAttackerIfHealthNotPositive() {
            Mock<IUnit> attackerMock, defenderMock;

            var attacker = BuildUnit(0,  UnitType.Melee, BuildCell(), null, out attackerMock);
            var defender = BuildUnit(50, UnitType.Melee, BuildCell(), null, out defenderMock);

            var combatInfo = new CombatInfo() { CombatType = CombatType.Melee };

            var destructionLogic = Container.Resolve<DestructionPostCombatResponder>();

            destructionLogic.RespondToCombat(attacker, defender, combatInfo);

            attackerMock.Verify(unit => unit.Destroy(), Times.Once, "Attacker was not destroyed");
        }

        [Test(Description = "")]
        public void RespondToCombat_DoesNotDestroyAttackerIfOfTypeCity() {
            Mock<IUnit> attackerMock, defenderMock;

            var attacker = BuildUnit(0,  UnitType.City,  BuildCell(), null, out attackerMock);
            var defender = BuildUnit(50, UnitType.Melee, BuildCell(), null, out defenderMock);

            var combatInfo = new CombatInfo() { CombatType = CombatType.Melee };

            var destructionLogic = Container.Resolve<DestructionPostCombatResponder>();

            destructionLogic.RespondToCombat(attacker, defender, combatInfo);

            attackerMock.Verify(unit => unit.Destroy(), Times.Never, "Attacker was unexpectedly destroyed");
        }

        [Test(Description = "")]
        public void RespondToCombat_DoesNotDestroyAttackerIfHealthPositive() {
            Mock<IUnit> attackerMock, defenderMock;

            var attacker = BuildUnit(1,  UnitType.Melee, BuildCell(), null, out attackerMock);
            var defender = BuildUnit(50, UnitType.Melee, BuildCell(), null, out defenderMock);

            var combatInfo = new CombatInfo() { CombatType = CombatType.Melee };

            var destructionLogic = Container.Resolve<DestructionPostCombatResponder>();

            destructionLogic.RespondToCombat(attacker, defender, combatInfo);

            attackerMock.Verify(unit => unit.Destroy(), Times.Never, "Attacker was unexpectedly destroyed");
        }

        [Test(Description = "")]
        public void RespondToCombat_DestroysDefenderIfHealthNotPositive() {
            Mock<IUnit> attackerMock, defenderMock;

            var attacker = BuildUnit(50, UnitType.Melee, BuildCell(), null, out attackerMock);
            var defender = BuildUnit(0,  UnitType.Melee, BuildCell(), null, out defenderMock);

            var combatInfo = new CombatInfo() { CombatType = CombatType.Melee };

            var destructionLogic = Container.Resolve<DestructionPostCombatResponder>();

            destructionLogic.RespondToCombat(attacker, defender, combatInfo);

            defenderMock.Verify(unit => unit.Destroy(), Times.Once, "Defender was not destroyed as expected");
        }

        [Test(Description = "")]
        public void RespondToCombat_DoesNotDestroyDefenderIfOfTypeCity() {
            Mock<IUnit> attackerMock, defenderMock;

            var attacker = BuildUnit(50, UnitType.Melee, BuildCell(), null, out attackerMock);
            var defender = BuildUnit(0,  UnitType.City,  BuildCell(), null, out defenderMock);

            var combatInfo = new CombatInfo() { CombatType = CombatType.Melee };

            var destructionLogic = Container.Resolve<DestructionPostCombatResponder>();

            destructionLogic.RespondToCombat(attacker, defender, combatInfo);

            defenderMock.Verify(unit => unit.Destroy(), Times.Never, "Defender was unexpectedly destroyed");
        }

        [Test(Description = "")]
        public void RespondToCombat_SetsCityHealthToOneInsteadOfDestroying() {
            Mock<IUnit> attackerMock, defenderMock;

            var attacker = BuildUnit(50, UnitType.Melee, BuildCell(), null, out attackerMock);
            var defender = BuildUnit(0,  UnitType.City,  BuildCell(), null, out defenderMock);

            var combatInfo = new CombatInfo() { CombatType = CombatType.Melee };

            var destructionLogic = Container.Resolve<DestructionPostCombatResponder>();

            destructionLogic.RespondToCombat(attacker, defender, combatInfo);

            Assert.AreEqual(1, defender.CurrentHitpoints);
        }

        [Test(Description = "")]
        public void RespondToCombat_DoesNotDestroyDefenderIfHealthPositive() {
            Mock<IUnit> attackerMock, defenderMock;

            var attacker = BuildUnit(50, UnitType.Melee, BuildCell(), null, out attackerMock);
            var defender = BuildUnit(1,  UnitType.City,  BuildCell(), null, out defenderMock);

            var combatInfo = new CombatInfo() { CombatType = CombatType.Melee };

            var destructionLogic = Container.Resolve<DestructionPostCombatResponder>();

            destructionLogic.RespondToCombat(attacker, defender, combatInfo);

            defenderMock.Verify(unit => unit.Destroy(), Times.Never, "Defender was unexpectedly destroyed");
        }

        [Test(Description = "When the defender dies and the attacker lives in melee combat, " +
            "HandleUnitDestructionFromCombat should set the attacker's current path to the " +
            "defender's old location and call attacker.PerformMovement(false)")]
        public void RespondToCombat_MovesAttackerOntoLocationWhenValid() {
            Mock<IUnit> attackerMock, defenderMock;

            var defenderLocation = BuildCell();

            var attacker = BuildUnit(50, UnitType.Melee, BuildCell(),      null, out attackerMock);
            var defender = BuildUnit(0,  UnitType.Melee, defenderLocation, null, out defenderMock);

            var combatInfo = new CombatInfo() { CombatType = CombatType.Melee };

            var destructionLogic = Container.Resolve<DestructionPostCombatResponder>();

            destructionLogic.RespondToCombat(attacker, defender, combatInfo);

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
        public void RespondToCombat_AttackerDoesNotMoveIfCombatNotMelee() {
            Mock<IUnit> attackerMock, defenderMock;

            var defenderLocation = BuildCell();

            var attacker = BuildUnit(50, UnitType.Melee, BuildCell(),      null, out attackerMock);
            var defender = BuildUnit(0,  UnitType.Melee, defenderLocation, null, out defenderMock);

            var combatInfo = new CombatInfo() { CombatType = CombatType.Ranged };

            var destructionLogic = Container.Resolve<DestructionPostCombatResponder>();

            destructionLogic.RespondToCombat(attacker, defender, combatInfo);

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
        public void RespondToCombat_AttackerDoesNotMoveIfAttackerDead() {
            Mock<IUnit> attackerMock, defenderMock;

            var defenderLocation = BuildCell();

            var attacker = BuildUnit(0, UnitType.Melee, BuildCell(),      null, out attackerMock);
            var defender = BuildUnit(0, UnitType.Melee, defenderLocation, null, out defenderMock);

            var combatInfo = new CombatInfo() { CombatType = CombatType.Melee };

            var destructionLogic = Container.Resolve<DestructionPostCombatResponder>();

            destructionLogic.RespondToCombat(attacker, defender, combatInfo);

            Assert.Null(
                attacker.CurrentPath,
                "attacker.CurrentPath has an unexpected value"
            );

            attackerMock.Verify(
                unit => unit.PerformMovement(It.IsAny<bool>()), Times.Never,
                "attacker.PerformMovement was unexpectedly called"
            );
        }

        [Test]
        public void RespondToCombat_DefenderIsCapturedIfTemplateIsCapturable_AndAttackerOwnerCanPossessIt() {
            var templates = new List<IUnitTemplate>() {
                BuildTemplate(), BuildTemplate(), BuildTemplate()
            };

            MockUnitConfig.Setup(config => config.CapturableTemplates).Returns(templates);

            Mock<IUnit> attackerMock, defenderMock;

            var attacker = BuildUnit(50, UnitType.Melee, BuildCell(), null,         out attackerMock);
            var defender = BuildUnit(0,  UnitType.Melee, BuildCell(), templates[1], out defenderMock);

            var attackerOwner = BuildCiv();

            MockUnitPossessionCanon.Setup(canon => canon.GetOwnerOfPossession(attacker)).Returns(attackerOwner);
            MockUnitPossessionCanon.Setup(canon => canon.CanChangeOwnerOfPossession(defender, attackerOwner)).Returns(true);

            var combatInfo = new CombatInfo() { CombatType = CombatType.Melee };

            var destructionLogic = Container.Resolve<DestructionPostCombatResponder>();

            destructionLogic.RespondToCombat(attacker, defender, combatInfo);

            MockUnitPossessionCanon.Verify(
                canon => canon.ChangeOwnerOfPossession(defender, attackerOwner), Times.Once
            );
        }

        [Test]
        public void RespondToCombat_DefenderIsDestroyedIfTemplateIsCapturable_ButAttackerCannotPossessIt() {
            var templates = new List<IUnitTemplate>() {
                BuildTemplate(), BuildTemplate(), BuildTemplate()
            };

            MockUnitConfig.Setup(config => config.CapturableTemplates).Returns(templates);

            Mock<IUnit> attackerMock, defenderMock;

            var attacker = BuildUnit(50, UnitType.Melee, BuildCell(), null,         out attackerMock);
            var defender = BuildUnit(0,  UnitType.Melee, BuildCell(), templates[1], out defenderMock);

            var attackerOwner = BuildCiv();

            MockUnitPossessionCanon.Setup(canon => canon.GetOwnerOfPossession(attacker)).Returns(attackerOwner);
            MockUnitPossessionCanon.Setup(canon => canon.CanChangeOwnerOfPossession(defender, attackerOwner)).Returns(false);

            var combatInfo = new CombatInfo() { CombatType = CombatType.Melee };

            var destructionLogic = Container.Resolve<DestructionPostCombatResponder>();

            destructionLogic.RespondToCombat(attacker, defender, combatInfo);

            MockUnitPossessionCanon.Verify(
                canon => canon.ChangeOwnerOfPossession(defender, attackerOwner),
                Times.Never, "ChangeOwnerOfPossession was unexpectedly called"
            );

            defenderMock.Verify(unit => unit.Destroy(), Times.Once, "Defender.Destroy() was not called as expected");
        }

        [Test]
        public void RespondToCombat_DefenderIsDestroyedIfTemplateIsCapturable_ButCombatTypeIsNotMelee() {
            var templates = new List<IUnitTemplate>() {
                BuildTemplate(), BuildTemplate(), BuildTemplate()
            };

            MockUnitConfig.Setup(config => config.CapturableTemplates).Returns(templates);

            Mock<IUnit> attackerMock, defenderMock;

            var attacker = BuildUnit(50, UnitType.Melee, BuildCell(), null,         out attackerMock);
            var defender = BuildUnit(0,  UnitType.Melee, BuildCell(), templates[1], out defenderMock);

            var attackerOwner = BuildCiv();

            MockUnitPossessionCanon.Setup(canon => canon.GetOwnerOfPossession(attacker)).Returns(attackerOwner);
            MockUnitPossessionCanon.Setup(canon => canon.CanChangeOwnerOfPossession(defender, attackerOwner)).Returns(true);

            var combatInfo = new CombatInfo() { CombatType = CombatType.Ranged };

            var destructionLogic = Container.Resolve<DestructionPostCombatResponder>();

            destructionLogic.RespondToCombat(attacker, defender, combatInfo);

            MockUnitPossessionCanon.Verify(
                canon => canon.ChangeOwnerOfPossession(defender, attackerOwner),
                Times.Never, "ChangeOwnerOfPossession was unexpectedly called"
            );

            defenderMock.Verify(unit => unit.Destroy(), Times.Once, "Defender.Destroy() was not called as expected");
        }

        #endregion

        #region utilities

        private IUnitTemplate BuildTemplate() {
            return new Mock<IUnitTemplate>().Object;
        }

        private IUnit BuildUnit(
            int hitpoints, UnitType type, IHexCell location,
            IUnitTemplate template, out Mock<IUnit> mock
        ) {
            mock = new Mock<IUnit>();

            mock.SetupAllProperties();

            mock.Setup(unit => unit.Type)    .Returns(type);
            mock.Setup(unit => unit.Template).Returns(template);

            var newUnit = mock.Object;

            newUnit.CurrentHitpoints = hitpoints;

            MockUnitPositionCanon.Setup(canon => canon.GetOwnerOfPossession(newUnit)).Returns(location);

            return newUnit;
        }

        private IHexCell BuildCell() {
            return new Mock<IHexCell>().Object;
        }

        private ICivilization BuildCiv() {
            return new Mock<ICivilization>().Object;
        }

        #endregion

        #endregion

    }

}
