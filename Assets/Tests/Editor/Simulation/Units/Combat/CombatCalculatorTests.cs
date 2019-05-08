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

namespace Assets.Tests.Simulation.Units.Combat {

    public class CombatCalculatorTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IUnitConfig> MockUnitConfig;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockUnitConfig = new Mock<IUnitConfig>();

            Container.Bind<IUnitConfig>().FromInstance(MockUnitConfig.Object);

            Container.Bind<CombatCalculator>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        [TestCaseSource(typeof(CombatCalculatorTestData), "Cases")]
        public UniRx.Tuple<int, int> CalculateCombatTests(CombatCalculatorTestData.CalculateCombatTestData testData) {
            var attacker = BuildUnit(testData.Attacker);
            var defender = BuildUnit(testData.Defender);

            MockUnitConfig.Setup(config => config.CombatBaseDamage).Returns(testData.CombatBaseDamage);

            var calculator = Container.Resolve<CombatCalculator>();

            return calculator.CalculateCombat(attacker, defender, testData.CombatInfo);
        }

        #endregion

        #region utilities

        private IUnit BuildUnit(CombatCalculatorTestData.UnitTestData unitData) {
            var mockUnit = new Mock<IUnit>();

            mockUnit.Setup(unit => unit.CombatStrength)      .Returns(unitData.CombatStrength);
            mockUnit.Setup(unit => unit.RangedAttackStrength).Returns(unitData.RangedAttackStrength);
            mockUnit.Setup(unit => unit.CurrentHitpoints)    .Returns(unitData.CurrentHitpoints);

            return mockUnit.Object;
        }

        #endregion

        #endregion

    }

}
