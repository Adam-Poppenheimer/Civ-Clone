using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using Zenject;
using Moq;

using Assets.Simulation;
using Assets.Simulation.Units;
using Assets.Simulation.Units.Combat;
using Assets.Simulation.HexMap;
using Assets.Simulation.Civilizations;

namespace Assets.Tests.Simulation.Units.Combat {

    public class CombatAuraLogicTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IUnitConfig>                                   MockUnitConfig;
        private Mock<IUnitPositionCanon>                            MockUnitPositionCanon;
        private Mock<IPossessionRelationship<ICivilization, IUnit>> MockUnitPossessionCanon;
        private Mock<IHexGrid>                                      MockGrid;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockUnitConfig          = new Mock<IUnitConfig>();
            MockUnitPositionCanon   = new Mock<IUnitPositionCanon>();
            MockUnitPossessionCanon = new Mock<IPossessionRelationship<ICivilization, IUnit>>();
            MockGrid                = new Mock<IHexGrid>();

            Container.Bind<IUnitConfig>                                  ().FromInstance(MockUnitConfig         .Object);
            Container.Bind<IUnitPositionCanon>                           ().FromInstance(MockUnitPositionCanon  .Object);
            Container.Bind<IPossessionRelationship<ICivilization, IUnit>>().FromInstance(MockUnitPossessionCanon.Object);
            Container.Bind<IHexGrid>                                     ().FromInstance(MockGrid               .Object);

            Container.Bind<CombatAuraLogic>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void ApplyAurasToCombat_AddsAttackerAurasOfNearbyFriendlyUnitsToAttackerModifier() {
            var domesticCiv = BuildCiv();
            var foreignCiv  = BuildCiv();

            var attacker = BuildUnit(domesticCiv, new UnitCombatSummary());
            var defender = BuildUnit(foreignCiv,  new UnitCombatSummary());

            var domesticWithinRange = BuildUnit(
                domesticCiv,  new UnitCombatSummary() {
                    auraModifiersWhenAttacking = new List<ICombatModifier>() { BuildModifier(10f, true), BuildModifier(10f, true) },
                }
            );

            var foreignWithinRange  = BuildUnit(
                foreignCiv, new UnitCombatSummary() {
                    auraModifiersWhenAttacking= new List<ICombatModifier>() { BuildModifier(12f, true) }
                }
            );

            var cellWithinRange = BuildCell(domesticWithinRange, foreignWithinRange);

            var attackerLocation = BuildCell(attacker);
            var defenderLocation = BuildCell(defender);

            MockUnitConfig.Setup(config => config.AuraRange).Returns(7);

            MockGrid.Setup(grid => grid.GetCellsInRadius(attackerLocation, 7)).Returns(new List<IHexCell>() { cellWithinRange });
            MockGrid.Setup(grid => grid.GetCellsInRadius(defenderLocation, 7)).Returns(new List<IHexCell>());

            var combatInfo = new CombatInfo();

            var auraLogic = Container.Resolve<CombatAuraLogic>();

            auraLogic.ApplyAurasToCombat(attacker, defender, combatInfo);

            Assert.AreEqual(20f, combatInfo.AttackerCombatModifier);
        }

        [Test]
        public void ApplyAurasToCombat_IgnoresAttackerAurasThatDontApplyToCombat() {
            var domesticCiv = BuildCiv();
            var foreignCiv  = BuildCiv();

            var attacker = BuildUnit(domesticCiv, new UnitCombatSummary());
            var defender = BuildUnit(foreignCiv,  new UnitCombatSummary());

            var domesticWithinRange = BuildUnit(
                domesticCiv,  new UnitCombatSummary() {
                    auraModifiersWhenAttacking = new List<ICombatModifier>() { BuildModifier(10f, true), BuildModifier(10f, false) },
                }
            );

            var foreignWithinRange  = BuildUnit(
                foreignCiv, new UnitCombatSummary() {
                    auraModifiersWhenAttacking= new List<ICombatModifier>() { BuildModifier(12f, true) }
                }
            );

            var cellWithinRange = BuildCell(domesticWithinRange, foreignWithinRange);

            var attackerLocation = BuildCell(attacker);
            var defenderLocation = BuildCell(defender);

            MockUnitConfig.Setup(config => config.AuraRange).Returns(7);

            MockGrid.Setup(grid => grid.GetCellsInRadius(attackerLocation, 7)).Returns(new List<IHexCell>() { cellWithinRange });
            MockGrid.Setup(grid => grid.GetCellsInRadius(defenderLocation, 7)).Returns(new List<IHexCell>());

            var combatInfo = new CombatInfo();

            var auraLogic = Container.Resolve<CombatAuraLogic>();

            auraLogic.ApplyAurasToCombat(attacker, defender, combatInfo);

            Assert.AreEqual(10f, combatInfo.AttackerCombatModifier);
        }

        [Test]
        public void ApplyAurasToCombat_AddsDefenderAurasOfNearbyFriendlyUnitsToDefenderModifier() {
            var domesticCiv = BuildCiv();
            var foreignCiv  = BuildCiv();

            var attacker = BuildUnit(foreignCiv,  new UnitCombatSummary());
            var defender = BuildUnit(domesticCiv, new UnitCombatSummary());

            var domesticWithinRange = BuildUnit(
                domesticCiv,  new UnitCombatSummary() {
                    auraModifiersWhenDefending = new List<ICombatModifier>() { BuildModifier(10f, true), BuildModifier(10f, true) },
                }
            );

            var foreignWithinRange  = BuildUnit(
                foreignCiv, new UnitCombatSummary() {
                    auraModifiersWhenDefending = new List<ICombatModifier>() { BuildModifier(12f, true) }
                }
            );

            var cellWithinRange = BuildCell(domesticWithinRange, foreignWithinRange);

            var attackerLocation = BuildCell(attacker);
            var defenderLocation = BuildCell(defender);

            MockUnitConfig.Setup(config => config.AuraRange).Returns(7);

            MockGrid.Setup(grid => grid.GetCellsInRadius(attackerLocation, 7)).Returns(new List<IHexCell>());
            MockGrid.Setup(grid => grid.GetCellsInRadius(defenderLocation, 7)).Returns(new List<IHexCell>() { cellWithinRange } );

            var combatInfo = new CombatInfo();

            var auraLogic = Container.Resolve<CombatAuraLogic>();

            auraLogic.ApplyAurasToCombat(attacker, defender, combatInfo);

            Assert.AreEqual(20f, combatInfo.DefenderCombatModifier);
        }

        [Test]
        public void ApplyAurasToCombat_IgnoresDefenderAurasThatDontApplyToCombat() {
            var domesticCiv = BuildCiv();
            var foreignCiv  = BuildCiv();

            var attacker = BuildUnit(foreignCiv,  new UnitCombatSummary());
            var defender = BuildUnit(domesticCiv, new UnitCombatSummary());

            var domesticWithinRange = BuildUnit(
                domesticCiv,  new UnitCombatSummary() {
                    auraModifiersWhenDefending = new List<ICombatModifier>() { BuildModifier(10f, true), BuildModifier(10f, false) },
                }
            );

            var foreignWithinRange  = BuildUnit(
                foreignCiv, new UnitCombatSummary() {
                    auraModifiersWhenDefending = new List<ICombatModifier>() { BuildModifier(12f, true) }
                }
            );

            var cellWithinRange = BuildCell(domesticWithinRange, foreignWithinRange);

            var attackerLocation = BuildCell(attacker);
            var defenderLocation = BuildCell(defender);

            MockUnitConfig.Setup(config => config.AuraRange).Returns(7);

            MockGrid.Setup(grid => grid.GetCellsInRadius(attackerLocation, 7)).Returns(new List<IHexCell>());
            MockGrid.Setup(grid => grid.GetCellsInRadius(defenderLocation, 7)).Returns(new List<IHexCell>() { cellWithinRange } );

            var combatInfo = new CombatInfo();

            var auraLogic = Container.Resolve<CombatAuraLogic>();

            auraLogic.ApplyAurasToCombat(attacker, defender, combatInfo);

            Assert.AreEqual(10f, combatInfo.DefenderCombatModifier);
        }

        #endregion

        #region utilities

        private ICivilization BuildCiv() {
            return new Mock<ICivilization>().Object;
        }

        private ICombatModifier BuildModifier(float modifierBonus, bool doesApply) {
            var mockModifier = new Mock<ICombatModifier>();

            mockModifier.Setup(modifier => modifier.Modifier).Returns(modifierBonus);

            mockModifier.Setup(
                modifier => modifier.DoesModifierApply(
                    It.IsAny<IUnit>(), It.IsAny<IUnit>(), It.IsAny<IHexCell>(), It.IsAny<CombatType>()
                )
            ).Returns(
                doesApply
            );

            return mockModifier.Object;
        }

        private IUnit BuildUnit(ICivilization civ, UnitCombatSummary combatSummary) {
            var mockUnit = new Mock<IUnit>();

            mockUnit.Setup(unit => unit.CombatSummary).Returns(combatSummary);

            var newUnit = mockUnit.Object;

            MockUnitPossessionCanon.Setup(canon => canon.GetOwnerOfPossession(newUnit)).Returns(civ);

            return newUnit;
        }

        private IHexCell BuildCell(params IUnit[] unitsAt) {
            var newCell = new Mock<IHexCell>().Object;

            MockUnitPositionCanon.Setup(canon => canon.GetPossessionsOfOwner(newCell)).Returns(unitsAt);

            foreach(var unit in unitsAt) {
                MockUnitPositionCanon.Setup(canon => canon.GetOwnerOfPossession(unit)).Returns(newCell);
            }

            return newCell;
        }

        #endregion

        #endregion

    }

}
