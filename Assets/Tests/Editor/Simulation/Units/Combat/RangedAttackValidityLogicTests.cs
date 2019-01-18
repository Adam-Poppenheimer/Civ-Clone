using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using Zenject;
using Moq;

using Assets.Simulation.HexMap;
using Assets.Simulation.Units;
using Assets.Simulation.Units.Combat;

namespace Assets.Tests.Simulation.Units.Combat {

    public class RangedAttackValidityLogicTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<ICommonAttackValidityLogic> MockCommonAttackValidityLogic;
        private Mock<IUnitPositionCanon>         MockUnitPositionCanon;
        private Mock<IHexGrid>                   MockGrid;
        private Mock<IUnitVisibilityLogic>      MockUnitLineOfSightLogic;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockCommonAttackValidityLogic = new Mock<ICommonAttackValidityLogic>();
            MockUnitPositionCanon         = new Mock<IUnitPositionCanon>();
            MockGrid                      = new Mock<IHexGrid>();
            MockUnitLineOfSightLogic      = new Mock<IUnitVisibilityLogic>();

            Container.Bind<ICommonAttackValidityLogic>().FromInstance(MockCommonAttackValidityLogic.Object);
            Container.Bind<IUnitPositionCanon>        ().FromInstance(MockUnitPositionCanon        .Object);
            Container.Bind<IHexGrid>                  ().FromInstance(MockGrid                     .Object);
            Container.Bind<IUnitVisibilityLogic>     ().FromInstance(MockUnitLineOfSightLogic     .Object);

            Container.Bind<RangedAttackValidityLogic>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void IsRangedAttackValid_FalseIfDoesntMeetCommonConditions() {
            var attackerLocation = BuildCell();
            var defenderLocation = BuildCell();

            var attacker = BuildUnit(
                location: attackerLocation, attackRange: 3, rangedAttackStrength: 1,
                currentMovement: 0.1f, isReadyForRangedAttack: true, combatSummary: new UnitCombatSummary(),
                visibleCells: new IHexCell[] { BuildCell(), defenderLocation }
            );

            var defender = BuildUnit(defenderLocation, 0, 0, 0f, false, new UnitCombatSummary());

            MockCommonAttackValidityLogic.Setup(logic => logic.DoesAttackMeetCommonConditions(attacker, defender))
                                         .Returns(false);

            MockGrid.Setup(grid => grid.GetDistance(attackerLocation, defenderLocation)).Returns(3);

            var validityLogic = Container.Resolve<RangedAttackValidityLogic>();

            Assert.IsFalse(validityLogic.IsRangedAttackValid(attacker, defender));
        }

        [Test]
        public void IsRangedAttackValid_FalseIfDistanceBetweenUnitsGreaterThanAttackerRange() {
            var attackerLocation = BuildCell();
            var defenderLocation = BuildCell();

            var attacker = BuildUnit(
                location: attackerLocation, attackRange: 2, rangedAttackStrength: 1,
                currentMovement: 0.1f, isReadyForRangedAttack: true, combatSummary: new UnitCombatSummary(),
                visibleCells: new IHexCell[] { BuildCell(), defenderLocation }
            );

            var defender = BuildUnit(defenderLocation, 0, 0, 0f, false, new UnitCombatSummary());

            MockCommonAttackValidityLogic.Setup(logic => logic.DoesAttackMeetCommonConditions(attacker, defender))
                                         .Returns(true);

            MockGrid.Setup(grid => grid.GetDistance(attackerLocation, defenderLocation)).Returns(3);

            var validityLogic = Container.Resolve<RangedAttackValidityLogic>();

            Assert.IsFalse(validityLogic.IsRangedAttackValid(attacker, defender));
        }

        [Test]
        public void IsRangedAttackValid_FalseIfCurrentMovementOfAttackerNotPositive() {
            var attackerLocation = BuildCell();
            var defenderLocation = BuildCell();

            var attacker = BuildUnit(
                location: attackerLocation, attackRange: 3, rangedAttackStrength: 1,
                currentMovement: 0f, isReadyForRangedAttack: true, combatSummary: new UnitCombatSummary(),
                visibleCells: new IHexCell[] { BuildCell(), defenderLocation }
            );

            var defender = BuildUnit(defenderLocation, 0, 0, 0f, false, new UnitCombatSummary());

            MockCommonAttackValidityLogic.Setup(logic => logic.DoesAttackMeetCommonConditions(attacker, defender))
                                         .Returns(true);

            MockGrid.Setup(grid => grid.GetDistance(attackerLocation, defenderLocation)).Returns(3);

            var validityLogic = Container.Resolve<RangedAttackValidityLogic>();

            Assert.IsFalse(validityLogic.IsRangedAttackValid(attacker, defender));
        }

        [Test]
        public void IsRangedAttackValid_FalseIfAttackerNotReadyForRangedAttack() {
            var attackerLocation = BuildCell();
            var defenderLocation = BuildCell();

            var attacker = BuildUnit(
                location: attackerLocation, attackRange: 3, rangedAttackStrength: 1,
                currentMovement: 0.1f, isReadyForRangedAttack: false, combatSummary: new UnitCombatSummary(),
                visibleCells: new IHexCell[] { BuildCell(), defenderLocation }
            );

            var defender = BuildUnit(defenderLocation, 0, 0, 0f, false, new UnitCombatSummary());

            MockCommonAttackValidityLogic.Setup(logic => logic.DoesAttackMeetCommonConditions(attacker, defender))
                                         .Returns(true);

            MockGrid.Setup(grid => grid.GetDistance(attackerLocation, defenderLocation)).Returns(3);

            var validityLogic = Container.Resolve<RangedAttackValidityLogic>();

            Assert.IsFalse(validityLogic.IsRangedAttackValid(attacker, defender));
        }

        [Test]
        public void IsRangedAttackValid_FalseIfAttackerCantSeeDefenderLocation() {
            var attackerLocation = BuildCell();
            var defenderLocation = BuildCell();

            var attacker = BuildUnit(
                location: attackerLocation, attackRange: 3, rangedAttackStrength: 1,
                currentMovement: 0.1f, isReadyForRangedAttack: true, combatSummary: new UnitCombatSummary(),
                visibleCells: new IHexCell[] { BuildCell() }
            );

            var defender = BuildUnit(defenderLocation, 0, 0, 0f, false, new UnitCombatSummary());

            MockCommonAttackValidityLogic.Setup(logic => logic.DoesAttackMeetCommonConditions(attacker, defender))
                                         .Returns(true);

            MockGrid.Setup(grid => grid.GetDistance(attackerLocation, defenderLocation)).Returns(3);

            var validityLogic = Container.Resolve<RangedAttackValidityLogic>();

            Assert.IsFalse(validityLogic.IsRangedAttackValid(attacker, defender));
        }

        [Test]
        public void IsRangedAttackValid_FalseIfAttackerRangedAttackStrengthNotPositive() {
            var attackerLocation = BuildCell();
            var defenderLocation = BuildCell();

            var attacker = BuildUnit(
                location: attackerLocation, attackRange: 3, rangedAttackStrength: 0,
                currentMovement: 0.1f, isReadyForRangedAttack: true, combatSummary: new UnitCombatSummary(),
                visibleCells: new IHexCell[] { BuildCell(), defenderLocation }
            );

            var defender = BuildUnit(defenderLocation, 0, 0, 0f, false, new UnitCombatSummary());

            MockCommonAttackValidityLogic.Setup(logic => logic.DoesAttackMeetCommonConditions(attacker, defender))
                                         .Returns(true);

            MockGrid.Setup(grid => grid.GetDistance(attackerLocation, defenderLocation)).Returns(3);

            var validityLogic = Container.Resolve<RangedAttackValidityLogic>();

            Assert.IsFalse(validityLogic.IsRangedAttackValid(attacker, defender));
        }

        [Test]
        public void IsRangedAttackValid_TrueIfNoInvalidatingConditionsMet() {
            var attackerLocation = BuildCell();
            var defenderLocation = BuildCell();

            var attacker = BuildUnit(
                location: attackerLocation, attackRange: 3, rangedAttackStrength: 1,
                currentMovement: 0.1f, isReadyForRangedAttack: true, combatSummary: new UnitCombatSummary(),
                visibleCells: new IHexCell[] { BuildCell(), defenderLocation }
            );

            var defender = BuildUnit(defenderLocation, 0, 0, 0f, false, new UnitCombatSummary());

            MockCommonAttackValidityLogic.Setup(logic => logic.DoesAttackMeetCommonConditions(attacker, defender))
                                         .Returns(true);

            MockGrid.Setup(grid => grid.GetDistance(attackerLocation, defenderLocation)).Returns(3);

            var validityLogic = Container.Resolve<RangedAttackValidityLogic>();

            Assert.IsTrue(validityLogic.IsRangedAttackValid(attacker, defender));
        }

        [Test]
        public void IsRangedAttackValid_AndAttackerIgnoresLineOfSight_IgnoresVisibilityConditions() {
            var attackerLocation = BuildCell();
            var defenderLocation = BuildCell();

            var attacker = BuildUnit(
                location: attackerLocation, attackRange: 3, rangedAttackStrength: 1,
                currentMovement: 0.1f, isReadyForRangedAttack: true, combatSummary: new UnitCombatSummary() { IgnoresLineOfSight = true },
                visibleCells: new IHexCell[] { BuildCell() }
            );

            var defender = BuildUnit(defenderLocation, 0, 0, 0f, false, new UnitCombatSummary());

            MockCommonAttackValidityLogic.Setup(logic => logic.DoesAttackMeetCommonConditions(attacker, defender))
                                         .Returns(true);

            MockGrid.Setup(grid => grid.GetDistance(attackerLocation, defenderLocation)).Returns(3);

            var validityLogic = Container.Resolve<RangedAttackValidityLogic>();

            Assert.IsTrue(validityLogic.IsRangedAttackValid(attacker, defender));
        }

        #endregion

        #region utilities

        private IHexCell BuildCell() {
            return new Mock<IHexCell>().Object;
        }

        private IUnit BuildUnit(
            IHexCell location, int attackRange, int rangedAttackStrength, float currentMovement,
            bool isReadyForRangedAttack, UnitCombatSummary combatSummary, params IHexCell[] visibleCells
        ) {
            var mockUnit = new Mock<IUnit>();

            mockUnit.Setup(unit => unit.AttackRange)           .Returns(attackRange);
            mockUnit.Setup(unit => unit.RangedAttackStrength)  .Returns(rangedAttackStrength);
            mockUnit.Setup(unit => unit.CurrentMovement)       .Returns(currentMovement);
            mockUnit.Setup(unit => unit.IsReadyForRangedAttack).Returns(isReadyForRangedAttack);
            mockUnit.Setup(unit => unit.CombatSummary)         .Returns(combatSummary);

            var newUnit = mockUnit.Object;

            MockUnitPositionCanon   .Setup(canon => canon.GetOwnerOfPossession (newUnit)).Returns(location);
            MockUnitLineOfSightLogic.Setup(logic => logic.GetCellsVisibleToUnit(newUnit)).Returns(visibleCells);

            return newUnit;
        }

        #endregion

        #endregion

    }

}
