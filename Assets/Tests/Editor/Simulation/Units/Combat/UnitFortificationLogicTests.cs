using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using Zenject;
using Moq;
using UniRx;

using Assets.Simulation.Units;
using Assets.Simulation.Units.Combat;
using Assets.Simulation.Core;

namespace Assets.Tests.Simulation.Units.Combat {

    public class UnitFortificationLogicTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IUnitConfig> MockUnitConfig;
        private CoreSignals       CoreSignals;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockUnitConfig = new Mock<IUnitConfig>();
            CoreSignals    = new CoreSignals();

            Container.Bind<IUnitConfig>().FromInstance(MockUnitConfig.Object);
            Container.Bind<CoreSignals>().FromInstance(CoreSignals);

            Container.Bind<UnitFortificationLogic>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void SetFortificationStatusForUnit_PositiveResultsReflectedInGetMethod() {
            var unit = BuildUnit(false);

            var fortLogic = Container.Resolve<UnitFortificationLogic>();

            fortLogic.SetFortificationStatusForUnit(unit, true);

            Assert.IsTrue(fortLogic.GetFortificationStatusForUnit(unit));
        }

        [Test]
        public void SetFortificationStatusForUnit_NegativeResultsReflectedInGetMethod() {
            var unit = BuildUnit(false);

            var fortLogic = Container.Resolve<UnitFortificationLogic>();

            fortLogic.SetFortificationStatusForUnit(unit, true);
            fortLogic.SetFortificationStatusForUnit(unit, false);

            Assert.IsFalse(fortLogic.GetFortificationStatusForUnit(unit));
        }

        [Test]
        public void SetFortificationStatusForUnit_FortificationModifierInitializedFromConfig() {
            var unit = BuildUnit(true);

            SetFortificationConfig(fortificiationBonusPerTurn: 0.25f, maxFortificationBonus: 1f);

            var fortLogic = Container.Resolve<UnitFortificationLogic>();

            fortLogic.SetFortificationStatusForUnit(unit, true);

            Assert.AreEqual(0.25f, fortLogic.GetFortificationModifierForUnit(unit));
        }

        [Test]
        public void SetFortificationStatusForUnit_UnitThatDontBenefitFromFortificationsHaveModifierOfZero() {
            var unit = BuildUnit(false);

            SetFortificationConfig(fortificiationBonusPerTurn: 0.25f, maxFortificationBonus: 1f);

            var fortLogic = Container.Resolve<UnitFortificationLogic>();

            fortLogic.SetFortificationStatusForUnit(unit, true);

            Assert.AreEqual(0f, fortLogic.GetFortificationModifierForUnit(unit));
        }

        [Test]
        public void GetFortificationModifierForUnit_ZeroIfUnitNotFortified() {
            var unit = BuildUnit(true);

            SetFortificationConfig(fortificiationBonusPerTurn: 0.25f, maxFortificationBonus: 1f);

            var fortLogic = Container.Resolve<UnitFortificationLogic>();

            Assert.AreEqual(0f, fortLogic.GetFortificationModifierForUnit(unit));
        }

        [Test]
        public void RoundBeganSignalFired_FortModifierForFortifiedUnitsIncreases_ByConfiguredAmount() {
            var unit = BuildUnit(true);

            SetFortificationConfig(fortificiationBonusPerTurn: 0.25f, maxFortificationBonus: 1f);

            var fortLogic = Container.Resolve<UnitFortificationLogic>();

            fortLogic.SetFortificationStatusForUnit(unit, true);

            CoreSignals.RoundBegan.OnNext(-5);

            Assert.AreEqual(0.25f + 0.25f, fortLogic.GetFortificationModifierForUnit(unit));
        }

        [Test]
        public void RoundBeganSignalFired_ModifierIncreaseLimitedByConfiguredMax() {
            var unit = BuildUnit(true);

            SetFortificationConfig(fortificiationBonusPerTurn: 0.75f, maxFortificationBonus: 1f);

            var fortLogic = Container.Resolve<UnitFortificationLogic>();

            fortLogic.SetFortificationStatusForUnit(unit, true);

            CoreSignals.RoundBegan.OnNext(-5);

            Assert.AreEqual(1f, fortLogic.GetFortificationModifierForUnit(unit));
        }

        [Test]
        public void RoundBeganSignalFired_UnitsThatDontBenefitFromFortificationsDontGainModifier() {
            var unit = BuildUnit(false);

            SetFortificationConfig(fortificiationBonusPerTurn: 0.25f, maxFortificationBonus: 1f);

            var fortLogic = Container.Resolve<UnitFortificationLogic>();

            fortLogic.SetFortificationStatusForUnit(unit, true);

            CoreSignals.RoundBegan.OnNext(-5);

            Assert.AreEqual(0f, fortLogic.GetFortificationModifierForUnit(unit));
        }

        #endregion

        #region utilities

        private IUnit BuildUnit(bool benefitsFromFortifications) {
            var combatSummary = new UnitCombatSummary() {
                BenefitsFromFortifications = benefitsFromFortifications
            };

            var mockUnit = new Mock<IUnit>();

            mockUnit.Setup(unit => unit.CombatSummary).Returns(combatSummary);

            return mockUnit.Object;
        }

        private void SetFortificationConfig(
            float fortificiationBonusPerTurn, float maxFortificationBonus
        ) {
            MockUnitConfig.Setup(config => config.FortificationBonusPerTurn)
                          .Returns(fortificiationBonusPerTurn);

            MockUnitConfig.Setup(config => config.MaxFortificationBonus)
                          .Returns(maxFortificationBonus);
        }

        #endregion

        #endregion

    }

}
