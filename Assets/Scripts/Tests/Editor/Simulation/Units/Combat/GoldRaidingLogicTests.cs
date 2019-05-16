using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;
using NUnit.Framework;
using Moq;
using UniRx;

using Assets.Simulation;
using Assets.Simulation.Civilizations;
using Assets.Simulation.Units;
using Assets.Simulation.Units.Combat;

namespace Assets.Tests.Simulation.Units.Combat {

    [TestFixture]
    public class GoldRaidingLogicTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IPossessionRelationship<ICivilization, IUnit>> MockUnitPossessionCanon;
        private Mock<ICivModifiers>                                 MockCivModifiers;
        private UnitSignals                                         UnitSignals;

        private Mock<ICivModifier<float>> MockGoldBountyPerProductionModifier;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockUnitPossessionCanon = new Mock<IPossessionRelationship<ICivilization, IUnit>>();
            MockCivModifiers        = new Mock<ICivModifiers>();
            UnitSignals             = new UnitSignals();

            MockGoldBountyPerProductionModifier = new Mock<ICivModifier<float>>();

            MockCivModifiers.Setup(modifiers => modifiers.GoldBountyPerProduction).Returns(MockGoldBountyPerProductionModifier.Object);

            Container.Bind<IPossessionRelationship<ICivilization, IUnit>>().FromInstance(MockUnitPossessionCanon.Object);
            Container.Bind<ICivModifiers>                                ().FromInstance(MockCivModifiers       .Object);
            Container.Bind<UnitSignals>                                  ().FromInstance(UnitSignals);

            Container.Bind<GoldRaidingLogic>().AsSingle().NonLazy();
        }

        #endregion

        #region tests

        [Test]
        public void MeleeCombatWithUnitFired_DoesNothingIfDefenderNotCity() {
            var attackerOwner = BuildCivilization(100);
            var defenderOwner = BuildCivilization(200);

            var attacker = BuildUnit(attackerOwner, UnitType.Melee, 0.5f);
            var defender = BuildUnit(defenderOwner, UnitType.Melee);

            Container.Resolve<GoldRaidingLogic>();

            var results = new UnitCombatResults(attacker, defender, 20, 20, new CombatInfo());

            UnitSignals.MeleeCombatWithUnit.OnNext(results);

            Assert.AreEqual(100, attackerOwner.GoldStockpile, "AttackerOwner.GoldStockpile has an unexpected value");
            Assert.AreEqual(200, defenderOwner.GoldStockpile, "DefenderOwner.GoldStockpile has an unexpected value");
        }

        [Test]
        public void MeleeCombatWithUnitFired_TransfersGoldBasedOnDamageAndGoldRaiding() {
            var attackerOwner = BuildCivilization(100);
            var defenderOwner = BuildCivilization(200);

            var attacker = BuildUnit(attackerOwner, UnitType.Melee, 0.5f);
            var defender = BuildUnit(defenderOwner, UnitType.City);

            Container.Resolve<GoldRaidingLogic>();

            var results = new UnitCombatResults(attacker, defender, 50, 50, new CombatInfo());

            UnitSignals.MeleeCombatWithUnit.OnNext(results);

            Assert.AreEqual(125, attackerOwner.GoldStockpile, "AttackerOwner.GoldStockpile has an unexpected value");
            Assert.AreEqual(175, defenderOwner.GoldStockpile, "DefenderOwner.GoldStockpile has an unexpected value");
        }

        [Test]
        public void MeleeCombatWithUnitFired_GoldTransferLimitedByDefenderStockpile() {
            var attackerOwner = BuildCivilization(100);
            var defenderOwner = BuildCivilization(2);

            var attacker = BuildUnit(attackerOwner, UnitType.Melee, 1f);
            var defender = BuildUnit(defenderOwner, UnitType.City);

            Container.Resolve<GoldRaidingLogic>();

            var results = new UnitCombatResults(attacker, defender, 50, 50, new CombatInfo());

            UnitSignals.MeleeCombatWithUnit.OnNext(results);

            Assert.AreEqual(102, attackerOwner.GoldStockpile, "AttackerOwner.GoldStockpile has an unexpected value");
            Assert.AreEqual(0,   defenderOwner.GoldStockpile, "DefenderOwner.GoldStockpile has an unexpected value");
        }

        [Test]
        public void MeleeCombatWithUnitFired_AttackerAliveAndDefenderDead_AttackerOwnerGainsGoldBounty() {
            var attackerOwner = BuildCivilization(0);
            var defenderOwner = BuildCivilization(0);

            var attacker = BuildUnit(attackerOwner, BuildUnitTemplate(100), 50);
            var defender = BuildUnit(defenderOwner, BuildUnitTemplate(200), 0);

            MockGoldBountyPerProductionModifier.Setup(modifier => modifier.GetValueForCiv(attackerOwner)).Returns(1.5f);
            MockGoldBountyPerProductionModifier.Setup(modifier => modifier.GetValueForCiv(defenderOwner)).Returns(2.5f);

            Container.Resolve<GoldRaidingLogic>();

            var results = new UnitCombatResults(attacker, defender, 0, 0, new CombatInfo());

            UnitSignals.MeleeCombatWithUnit.OnNext(results);

            Assert.AreEqual(Mathf.RoundToInt(200 * 1.5f), attackerOwner.GoldStockpile, "AttackerOwner has an unexpected gold stockpile");
            Assert.AreEqual(0,                            defenderOwner.GoldStockpile, "DefenderOwner has an unexpected gold stockpile");
        }

        [Test]
        public void MeleeCombatWithUnitFired_AttackerDeadAndDefenderAlive_DefenderOwnerGainsGoldBounty() {
            var attackerOwner = BuildCivilization(0);
            var defenderOwner = BuildCivilization(0);

            var attacker = BuildUnit(attackerOwner, BuildUnitTemplate(100), 0);
            var defender = BuildUnit(defenderOwner, BuildUnitTemplate(200), 50);

            MockGoldBountyPerProductionModifier.Setup(modifier => modifier.GetValueForCiv(attackerOwner)).Returns(1.5f);
            MockGoldBountyPerProductionModifier.Setup(modifier => modifier.GetValueForCiv(defenderOwner)).Returns(2.5f);

            Container.Resolve<GoldRaidingLogic>();

            var results = new UnitCombatResults(attacker, defender, 0, 0, new CombatInfo());

            UnitSignals.MeleeCombatWithUnit.OnNext(results);

            Assert.AreEqual(0,                            attackerOwner.GoldStockpile, "AttackerOwner has an unexpected gold stockpile");
            Assert.AreEqual(Mathf.RoundToInt(100 * 2.5f), defenderOwner.GoldStockpile, "DefenderOwner has an unexpected gold stockpile");
        }

        [Test]
        public void MeleeCombatWithUnitFired_AttackerAliveAndDefenderAlive_NoGoldBountyGained() {
            var attackerOwner = BuildCivilization(0);
            var defenderOwner = BuildCivilization(0);

            var attacker = BuildUnit(attackerOwner, BuildUnitTemplate(100), 50);
            var defender = BuildUnit(defenderOwner, BuildUnitTemplate(200), 50);

            MockGoldBountyPerProductionModifier.Setup(modifier => modifier.GetValueForCiv(attackerOwner)).Returns(1.5f);
            MockGoldBountyPerProductionModifier.Setup(modifier => modifier.GetValueForCiv(defenderOwner)).Returns(2.5f);

            Container.Resolve<GoldRaidingLogic>();

            var results = new UnitCombatResults(attacker, defender, 0, 0, new CombatInfo());

            UnitSignals.MeleeCombatWithUnit.OnNext(results);

            Assert.AreEqual(0, attackerOwner.GoldStockpile, "AttackerOwner has an unexpected gold stockpile");
            Assert.AreEqual(0, defenderOwner.GoldStockpile, "DefenderOwner has an unexpected gold stockpile");
        }

        [Test]
        public void MeleeCombatWithUnitFired_AttackerDeadAndDefenderDead_NoGoldBountyGained() {
            var attackerOwner = BuildCivilization(0);
            var defenderOwner = BuildCivilization(0);

            var attacker = BuildUnit(attackerOwner, BuildUnitTemplate(100), 0);
            var defender = BuildUnit(defenderOwner, BuildUnitTemplate(200), 0);

            MockGoldBountyPerProductionModifier.Setup(modifier => modifier.GetValueForCiv(attackerOwner)).Returns(1.5f);
            MockGoldBountyPerProductionModifier.Setup(modifier => modifier.GetValueForCiv(defenderOwner)).Returns(2.5f);

            Container.Resolve<GoldRaidingLogic>();

            var results = new UnitCombatResults(attacker, defender, 0, 0, new CombatInfo());

            UnitSignals.MeleeCombatWithUnit.OnNext(results);

            Assert.AreEqual(0, attackerOwner.GoldStockpile, "AttackerOwner has an unexpected gold stockpile");
            Assert.AreEqual(0, defenderOwner.GoldStockpile, "DefenderOwner has an unexpected gold stockpile");
        }

        [Test]
        public void RangedCombatWithUnitFired_AttackerAliveAndDefenderDead_AttackerOwnerGainsGoldBounty() {
            var attackerOwner = BuildCivilization(0);
            var defenderOwner = BuildCivilization(0);

            var attacker = BuildUnit(attackerOwner, BuildUnitTemplate(100), 50);
            var defender = BuildUnit(defenderOwner, BuildUnitTemplate(200), 0);

            MockGoldBountyPerProductionModifier.Setup(modifier => modifier.GetValueForCiv(attackerOwner)).Returns(1.5f);
            MockGoldBountyPerProductionModifier.Setup(modifier => modifier.GetValueForCiv(defenderOwner)).Returns(2.5f);

            Container.Resolve<GoldRaidingLogic>();

            var results = new UnitCombatResults(attacker, defender, 0, 0, new CombatInfo());

            UnitSignals.RangedCombatWithUnit.OnNext(results);

            Assert.AreEqual(Mathf.RoundToInt(200 * 1.5f), attackerOwner.GoldStockpile, "AttackerOwner has an unexpected gold stockpile");
            Assert.AreEqual(0,                            defenderOwner.GoldStockpile, "DefenderOwner has an unexpected gold stockpile");
        }

        [Test]
        public void RangedCombatWithUnitFired_AttackerDeadAndDefenderAlive_DefenderOwnerGainsGoldBounty() {
            var attackerOwner = BuildCivilization(0);
            var defenderOwner = BuildCivilization(0);

            var attacker = BuildUnit(attackerOwner, BuildUnitTemplate(100), 0);
            var defender = BuildUnit(defenderOwner, BuildUnitTemplate(200), 50);

            MockGoldBountyPerProductionModifier.Setup(modifier => modifier.GetValueForCiv(attackerOwner)).Returns(1.5f);
            MockGoldBountyPerProductionModifier.Setup(modifier => modifier.GetValueForCiv(defenderOwner)).Returns(2.5f);

            Container.Resolve<GoldRaidingLogic>();

            var results = new UnitCombatResults(attacker, defender, 0, 0, new CombatInfo());

            UnitSignals.RangedCombatWithUnit.OnNext(results);

            Assert.AreEqual(0,                            attackerOwner.GoldStockpile, "AttackerOwner has an unexpected gold stockpile");
            Assert.AreEqual(Mathf.RoundToInt(100 * 2.5f), defenderOwner.GoldStockpile, "DefenderOwner has an unexpected gold stockpile");
        }

        [Test]
        public void RangedCombatWithUnitFired_AttackerAliveAndDefenderAlive_NoGoldBountyGained() {
            var attackerOwner = BuildCivilization(0);
            var defenderOwner = BuildCivilization(0);

            var attacker = BuildUnit(attackerOwner, BuildUnitTemplate(100), 50);
            var defender = BuildUnit(defenderOwner, BuildUnitTemplate(200), 50);

            MockGoldBountyPerProductionModifier.Setup(modifier => modifier.GetValueForCiv(attackerOwner)).Returns(1.5f);
            MockGoldBountyPerProductionModifier.Setup(modifier => modifier.GetValueForCiv(defenderOwner)).Returns(2.5f);

            Container.Resolve<GoldRaidingLogic>();

            var results = new UnitCombatResults(attacker, defender, 0, 0, new CombatInfo());

            UnitSignals.RangedCombatWithUnit.OnNext(results);

            Assert.AreEqual(0, attackerOwner.GoldStockpile, "AttackerOwner has an unexpected gold stockpile");
            Assert.AreEqual(0, defenderOwner.GoldStockpile, "DefenderOwner has an unexpected gold stockpile");
        }

        [Test]
        public void RangedCombatWithUnitFired_AttackerDeadAndDefenderDead_NoGoldBountyGained() {
            var attackerOwner = BuildCivilization(0);
            var defenderOwner = BuildCivilization(0);

            var attacker = BuildUnit(attackerOwner, BuildUnitTemplate(100), 0);
            var defender = BuildUnit(defenderOwner, BuildUnitTemplate(200), 0);

            MockGoldBountyPerProductionModifier.Setup(modifier => modifier.GetValueForCiv(attackerOwner)).Returns(1.5f);
            MockGoldBountyPerProductionModifier.Setup(modifier => modifier.GetValueForCiv(defenderOwner)).Returns(2.5f);

            Container.Resolve<GoldRaidingLogic>();

            var results = new UnitCombatResults(attacker, defender, 0, 0, new CombatInfo());

            UnitSignals.RangedCombatWithUnit.OnNext(results);

            Assert.AreEqual(0, attackerOwner.GoldStockpile, "AttackerOwner has an unexpected gold stockpile");
            Assert.AreEqual(0, defenderOwner.GoldStockpile, "DefenderOwner has an unexpected gold stockpile");
        }

        #endregion

        #region utilities

        private ICivilization BuildCivilization(int goldStockpile) {
            var mockCiv = new Mock<ICivilization>();

            mockCiv.SetupAllProperties();

            var newCiv = mockCiv.Object;

            newCiv.GoldStockpile = goldStockpile;

            return newCiv;
        }

        private IUnitTemplate BuildUnitTemplate(int productionCost) {
            var mockTemplate = new Mock<IUnitTemplate>();

            mockTemplate.Setup(template => template.ProductionCost).Returns(productionCost);

            return mockTemplate.Object;
        }

        private IUnit BuildUnit(ICivilization owner, UnitType type, float goldRaidingPercentage = 0f) {
            var mockUnit = new Mock<IUnit>();

            var combatSummary = new UnitCombatSummary() {
                GoldRaidingPercentage = goldRaidingPercentage
            }; 

            mockUnit.Setup(unit => unit.Type).Returns(type);
            mockUnit.Setup(unit => unit.CombatSummary).Returns(combatSummary);           

            var newUnit = mockUnit.Object;

            MockUnitPossessionCanon.Setup(canon => canon.GetOwnerOfPossession(newUnit)).Returns(owner);

            return newUnit;
        }

        private IUnit BuildUnit(ICivilization owner, IUnitTemplate template, int currentHitpoints) {
            var mockUnit = new Mock<IUnit>();

            mockUnit.Setup(unit => unit.Template)        .Returns(template);
            mockUnit.Setup(unit => unit.CurrentHitpoints).Returns(currentHitpoints);

            var newUnit = mockUnit.Object;

            MockUnitPossessionCanon.Setup(canon => canon.GetOwnerOfPossession(newUnit)).Returns(owner);

            return newUnit;
        }

        #endregion

        #endregion

    }

}
