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

    public class UnitComparativeStrengthEstimatorTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<ICombatInfoLogic> MockCombatInfoLogic;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockCombatInfoLogic = new Mock<ICombatInfoLogic>();

            Container.Bind<ICombatInfoLogic>().FromInstance(MockCombatInfoLogic.Object);

            Container.Bind<UnitComparativeStrengthEstimator>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void EstimateComparativeStrength_AndUnitRanged_ModifiesStrengthWithRangedAttackInfo() {
            var unit     = BuildUnit(combatStrength: 10, rangedAttackStrength: 2);
            var defender = BuildUnit();
            var location = BuildCell();

            MockCombatInfoLogic.Setup(logic => logic.GetAttackInfo(unit, defender, location, CombatType.Ranged))
                               .Returns(new CombatInfo() { AttackerCombatModifier = 2.5f });

            var estimator = Container.Resolve<UnitComparativeStrengthEstimator>();

            Assert.AreEqual(2 * 3.5f, estimator.EstimateComparativeStrength(unit, defender, location));
        }

        [Test]
        public void EstimateComparativeStrength_AndUnitMelee_ModifiesStrengthWithMeleeAttackInfo() {
            var unit     = BuildUnit(combatStrength: 10, rangedAttackStrength: 0);
            var defender = BuildUnit();
            var location = BuildCell();

            MockCombatInfoLogic.Setup(logic => logic.GetAttackInfo(unit, defender, location, CombatType.Melee))
                               .Returns(new CombatInfo() { AttackerCombatModifier = 2.5f });

            var estimator = Container.Resolve<UnitComparativeStrengthEstimator>();

            Assert.AreEqual(10 * 3.5f, estimator.EstimateComparativeStrength(unit, defender, location));
        }

        [Test]
        public void EstimateComparativeDefensiveStrength_AndAttackerRanged_ModifiesStrengthWithRangedAttackInfo() {
            var unit     = BuildUnit(combatStrength: 10, rangedAttackStrength: 2);
            var attacker = BuildUnit(combatStrength: 2,  rangedAttackStrength: 1);
            var location = BuildCell();

            MockCombatInfoLogic.Setup(logic => logic.GetAttackInfo(attacker, unit, location, CombatType.Ranged))
                               .Returns(new CombatInfo() { DefenderCombatModifier = 2.5f });

            var estimator = Container.Resolve<UnitComparativeStrengthEstimator>();

            Assert.AreEqual(10 * 3.5f, estimator.EstimateComparativeDefensiveStrength(unit, attacker, location));
        }

        [Test]
        public void EstimateComparativeDefensiveStrength_AndAttackerMelee_ModifiesStrengthWithMeleeAttackInfo() {
            var unit     = BuildUnit(combatStrength: 10, rangedAttackStrength: 2);
            var attacker = BuildUnit(combatStrength: 2,  rangedAttackStrength: 0);
            var location = BuildCell();

            MockCombatInfoLogic.Setup(logic => logic.GetAttackInfo(attacker, unit, location, CombatType.Melee))
                               .Returns(new CombatInfo() { DefenderCombatModifier = 2.5f });

            var estimator = Container.Resolve<UnitComparativeStrengthEstimator>();

            Assert.AreEqual(10 * 3.5f, estimator.EstimateComparativeDefensiveStrength(unit, attacker, location));
        }

        #endregion

        #region utilities

        private IUnit BuildUnit(int combatStrength = 0, int rangedAttackStrength = 0) {
            var mockUnit = new Mock<IUnit>();

            mockUnit.Setup(unit => unit.CombatStrength)      .Returns(combatStrength);
            mockUnit.Setup(unit => unit.RangedAttackStrength).Returns(rangedAttackStrength);

            return mockUnit.Object;
        }

        private IHexCell BuildCell() {
            return new Mock<IHexCell>().Object;
        }

        #endregion

        #endregion

    }

}
