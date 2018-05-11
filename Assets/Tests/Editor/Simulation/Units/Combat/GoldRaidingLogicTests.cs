using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

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

        private UnitSignals UnitSignals;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockUnitPossessionCanon = new Mock<IPossessionRelationship<ICivilization, IUnit>>();
            UnitSignals             = new UnitSignals();

            Container.Bind<IPossessionRelationship<ICivilization, IUnit>>().FromInstance(MockUnitPossessionCanon.Object);
            Container.Bind<UnitSignals>().FromInstance(UnitSignals);

            Container.Bind<GoldRaidingLogic>().AsSingle().NonLazy();
        }

        #endregion

        #region tests

        [Test]
        public void MeleeCombatWithUnitFired_DoesNothingIfDefenderNotCity() {
            var attackerOwner = BuildCivilization(100);
            var defenderOwner = BuildCivilization(200);

            var attacker = BuildUnit(attackerOwner, UnitType.Melee);
            var defender = BuildUnit(defenderOwner, UnitType.Melee);

            Container.Resolve<GoldRaidingLogic>();

            var results = new UnitCombatResults(attacker, defender, 20, 20, new CombatInfo() {
                Attacker = new UnitCombatInfo() { GoldRaidingPercentage = 0.5f }
            });

            UnitSignals.MeleeCombatWithUnitSignal.OnNext(results);

            Assert.AreEqual(100, attackerOwner.GoldStockpile, "AttackerOwner.GoldStockpile has an unexpected value");
            Assert.AreEqual(200, defenderOwner.GoldStockpile, "DefenderOwner.GoldStockpile has an unexpected value");
        }

        [Test]
        public void MeleeCombatWithUnitFired_TransfersGoldBasedOnDamageAndGoldRaiding() {
            var attackerOwner = BuildCivilization(100);
            var defenderOwner = BuildCivilization(200);

            var attacker = BuildUnit(attackerOwner, UnitType.Melee);
            var defender = BuildUnit(defenderOwner, UnitType.City);

            Container.Resolve<GoldRaidingLogic>();

            var results = new UnitCombatResults(attacker, defender, 50, 50, new CombatInfo() {
                Attacker = new UnitCombatInfo() { GoldRaidingPercentage = 0.5f }
            });

            UnitSignals.MeleeCombatWithUnitSignal.OnNext(results);

            Assert.AreEqual(125, attackerOwner.GoldStockpile, "AttackerOwner.GoldStockpile has an unexpected value");
            Assert.AreEqual(175, defenderOwner.GoldStockpile, "DefenderOwner.GoldStockpile has an unexpected value");
        }

        [Test]
        public void MeleeCombatWithUnitFired_GoldTransferLimitedByDefenderStockpile() {
            var attackerOwner = BuildCivilization(100);
            var defenderOwner = BuildCivilization(2);

            var attacker = BuildUnit(attackerOwner, UnitType.Melee);
            var defender = BuildUnit(defenderOwner, UnitType.City);

            Container.Resolve<GoldRaidingLogic>();

            var results = new UnitCombatResults(attacker, defender, 50, 50, new CombatInfo() {
                Attacker = new UnitCombatInfo() { GoldRaidingPercentage = 1f }
            });

            UnitSignals.MeleeCombatWithUnitSignal.OnNext(results);

            Assert.AreEqual(102, attackerOwner.GoldStockpile, "AttackerOwner.GoldStockpile has an unexpected value");
            Assert.AreEqual(0,   defenderOwner.GoldStockpile, "DefenderOwner.GoldStockpile has an unexpected value");
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

        private IUnit BuildUnit(ICivilization owner, UnitType type) {
            var mockUnit = new Mock<IUnit>();

            mockUnit.Setup(unit => unit.Type).Returns(type);

            var newUnit = mockUnit.Object;

            MockUnitPossessionCanon.Setup(canon => canon.GetOwnerOfPossession(newUnit)).Returns(owner);

            return newUnit;
        }

        #endregion

        #endregion

    }

}
