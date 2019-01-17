﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using Zenject;
using Moq;
using UniRx;

using Assets.Simulation.HexMap;
using Assets.Simulation.Units;
using Assets.Simulation.Units.Combat;

namespace Assets.Tests.Simulation.Units.Combat {

    public class CombatEstimatorTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IUnitPositionCanon> MockUnitPositionCanon;
        private Mock<ICombatInfoLogic>   MockCombatInfoLogic;
        private Mock<ICombatCalculator>  MockCombatCalculator;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockUnitPositionCanon = new Mock<IUnitPositionCanon>();
            MockCombatInfoLogic   = new Mock<ICombatInfoLogic>();
            MockCombatCalculator  = new Mock<ICombatCalculator>();

            Container.Bind<IUnitPositionCanon>().FromInstance(MockUnitPositionCanon.Object);
            Container.Bind<ICombatInfoLogic>  ().FromInstance(MockCombatInfoLogic  .Object);
            Container.Bind<ICombatCalculator> ().FromInstance(MockCombatCalculator .Object);

            Container.Bind<CombatEstimator>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void EstimateMeleeAttackResults_ReturnedResultsHasCorrectAttackerAndDefender() {
            var defenderLocation = BuildCell();

            var attacker = BuildUnit(BuildCell());
            var defender = BuildUnit(defenderLocation);

            var combatInfo = new CombatInfo();

            var combatCalculations = new Tuple<int, int>(1, 22);

            MockCombatInfoLogic.Setup(logic => logic.GetAttackInfo(attacker, defender, defenderLocation, CombatType.Melee))
                               .Returns(combatInfo);

            MockCombatCalculator.Setup(calculator => calculator.CalculateCombat(attacker, defender, combatInfo))
                                .Returns(combatCalculations);

            var estimator = Container.Resolve<CombatEstimator>();

            var combatResults = estimator.EstimateMeleeAttackResults(attacker, defender);

            Assert.AreEqual(attacker, combatResults.Attacker, "CombatResults has an unexpected Attacker");
            Assert.AreEqual(defender, combatResults.Defender, "CombatResults has an unexpected Defender");
        }

        [Test]
        public void EstimateMeleeAttackResults_ReturnedResultsHasCorrectDamageDone() {
            var defenderLocation = BuildCell();

            var attacker = BuildUnit(BuildCell());
            var defender = BuildUnit(defenderLocation);

            var combatInfo = new CombatInfo();

            var combatCalculations = new Tuple<int, int>(1, 22);

            MockCombatInfoLogic.Setup(logic => logic.GetAttackInfo(attacker, defender, defenderLocation, CombatType.Melee))
                               .Returns(combatInfo);

            MockCombatCalculator.Setup(calculator => calculator.CalculateCombat(attacker, defender, combatInfo))
                                .Returns(combatCalculations);

            var estimator = Container.Resolve<CombatEstimator>();

            var combatResults = estimator.EstimateMeleeAttackResults(attacker, defender);

            Assert.AreEqual(1,  combatResults.DamageToAttacker, "CombatResults has an unexpected DamageToAttacker");
            Assert.AreEqual(22, combatResults.DamageToDefender, "CombatResults has an unexpected DamageToDefender");
        }

        [Test]
        public void EstimateMeleeAttackResults_ReturnedResultsHasCorrectCombatInfo() {
            var defenderLocation = BuildCell();

            var attacker = BuildUnit(BuildCell());
            var defender = BuildUnit(defenderLocation);

            var combatInfo = new CombatInfo();

            var combatCalculations = new Tuple<int, int>(1, 22);

            MockCombatInfoLogic.Setup(logic => logic.GetAttackInfo(attacker, defender, defenderLocation, CombatType.Melee))
                               .Returns(combatInfo);

            MockCombatCalculator.Setup(calculator => calculator.CalculateCombat(attacker, defender, combatInfo))
                                .Returns(combatCalculations);

            var estimator = Container.Resolve<CombatEstimator>();

            var combatResults = estimator.EstimateMeleeAttackResults(attacker, defender);

            Assert.AreEqual(combatInfo, combatResults.InfoOfAttack);
        }

        [Test]
        public void EstimateRangedAttackResults_ReturnedResultsHasCorrectAttackerAndDefender() {
            var defenderLocation = BuildCell();

            var attacker = BuildUnit(BuildCell());
            var defender = BuildUnit(defenderLocation);

            var combatInfo = new CombatInfo();

            var combatCalculations = new Tuple<int, int>(1, 22);

            MockCombatInfoLogic.Setup(logic => logic.GetAttackInfo(attacker, defender, defenderLocation, CombatType.Ranged))
                               .Returns(combatInfo);

            MockCombatCalculator.Setup(calculator => calculator.CalculateCombat(attacker, defender, combatInfo))
                                .Returns(combatCalculations);

            var estimator = Container.Resolve<CombatEstimator>();

            var combatResults = estimator.EstimateRangedAttackResults(attacker, defender);

            Assert.AreEqual(attacker, combatResults.Attacker, "CombatResults has an unexpected Attacker");
            Assert.AreEqual(defender, combatResults.Defender, "CombatResults has an unexpected Defender");
        }

        [Test]
        public void EstimateRangedAttackResults_ReturnedResultsHasCorrectDamageDone() {
            var defenderLocation = BuildCell();

            var attacker = BuildUnit(BuildCell());
            var defender = BuildUnit(defenderLocation);

            var combatInfo = new CombatInfo();

            var combatCalculations = new Tuple<int, int>(1, 22);

            MockCombatInfoLogic.Setup(logic => logic.GetAttackInfo(attacker, defender, defenderLocation, CombatType.Ranged))
                               .Returns(combatInfo);

            MockCombatCalculator.Setup(calculator => calculator.CalculateCombat(attacker, defender, combatInfo))
                                .Returns(combatCalculations);

            var estimator = Container.Resolve<CombatEstimator>();

            var combatResults = estimator.EstimateRangedAttackResults(attacker, defender);

            Assert.AreEqual(1,  combatResults.DamageToAttacker, "CombatResults has an unexpected DamageToAttacker");
            Assert.AreEqual(22, combatResults.DamageToDefender, "CombatResults has an unexpected DamageToDefender");
        }

        [Test]
        public void EstimateRangedAttackResults_ReturnedResultsHasCorrectCombatInfo() {
            var defenderLocation = BuildCell();

            var attacker = BuildUnit(BuildCell());
            var defender = BuildUnit(defenderLocation);

            var combatInfo = new CombatInfo();

            var combatCalculations = new Tuple<int, int>(1, 22);

            MockCombatInfoLogic.Setup(logic => logic.GetAttackInfo(attacker, defender, defenderLocation, CombatType.Ranged))
                               .Returns(combatInfo);

            MockCombatCalculator.Setup(calculator => calculator.CalculateCombat(attacker, defender, combatInfo))
                                .Returns(combatCalculations);

            var estimator = Container.Resolve<CombatEstimator>();

            var combatResults = estimator.EstimateRangedAttackResults(attacker, defender);

            Assert.AreEqual(combatInfo, combatResults.InfoOfAttack);
        }

        [Test]
        public void EstimateMeleeAttackResults_ThrowsOnNullAttacker() {
            var unit = BuildUnit(BuildCell());

            var estimator = Container.Resolve<CombatEstimator>();

            Assert.Throws<ArgumentNullException>(() => estimator.EstimateMeleeAttackResults(null, unit));
        }

        [Test]
        public void EstimateMeleeAttackResults_ThrowsOnNullDefender() {
            var unit = BuildUnit(BuildCell());

            var estimator = Container.Resolve<CombatEstimator>();

            Assert.Throws<ArgumentNullException>(() => estimator.EstimateMeleeAttackResults(unit, null));
        }

        [Test]
        public void EstimateRangedAttackResults_ThrowsOnNullAttacker() {
            var unit = BuildUnit(BuildCell());

            var estimator = Container.Resolve<CombatEstimator>();

            Assert.Throws<ArgumentNullException>(() => estimator.EstimateRangedAttackResults(null, unit));
        }

        [Test]
        public void EstimateRangedAttackResults_ThrowsOnNullDefender() {
            var unit = BuildUnit(BuildCell());

            var estimator = Container.Resolve<CombatEstimator>();

            Assert.Throws<ArgumentNullException>(() => estimator.EstimateRangedAttackResults(unit, null));
        }

        #endregion

        #region utilities

        private IHexCell BuildCell() {
            return new Mock<IHexCell>().Object;
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
