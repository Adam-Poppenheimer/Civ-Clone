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

    public class CommonCombatExecutionLogicTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<ICombatCalculator> MockCombatCalculator;

        private List<Mock<IPostCombatResponder>> MockPostCombatResponders = 
            new List<Mock<IPostCombatResponder>>();

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockPostCombatResponders.Clear();

            MockCombatCalculator = new Mock<ICombatCalculator>();

            Container.Bind<ICombatCalculator>().FromInstance(MockCombatCalculator.Object);

            Container.Bind<List<IPostCombatResponder>>().FromMethod(
                context => MockPostCombatResponders.Select(mock => mock.Object).ToList()
            );

            Container.Bind<CommonCombatExecutionLogic>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void PerformCommonCombatTasks_ReducesParticipiantHealthFromCalculatedCombatResults() {
            var attacker = BuildUnit(100, false, new UnitCombatSummary());
            var defender = BuildUnit(100, false, new UnitCombatSummary());

            var combatInfo = new CombatInfo();

            MockCombatCalculator.Setup(calculator => calculator.CalculateCombat(attacker, defender, combatInfo))
                                .Returns(new UniRx.Tuple<int, int>(40, 55));

            var executionLogic = Container.Resolve<CommonCombatExecutionLogic>();

            executionLogic.PerformCommonCombatTasks(attacker, defender, combatInfo);

            Assert.AreEqual(60, attacker.CurrentHitpoints, "Attacker.CurrentHitpoints has an unexpected value");
            Assert.AreEqual(45, defender.CurrentHitpoints, "Defender.CurrentHitpoints has an unexpected value");
        }

        [Test]
        public void PerformCommonCombatTasks_CallsIntoAllPostCombatResponders() {
            var attacker = BuildUnit(100, false, new UnitCombatSummary());
            var defender = BuildUnit(100, false, new UnitCombatSummary());

            var combatInfo = new CombatInfo();

            BuildResponder();
            BuildResponder();
            BuildResponder();

            var executionLogic = Container.Resolve<CommonCombatExecutionLogic>();

            executionLogic.PerformCommonCombatTasks(attacker, defender, combatInfo);

            foreach(var mockResponder in MockPostCombatResponders) {
                mockResponder.Verify(
                    responder => responder.RespondToCombat(attacker, defender, combatInfo),
                    Times.Once, "A PostCombatResponder wasn't called into as expected"
                );
            }
        }

        [Test]
        public void PerformCommonCombatTasks_CanAttackSetToTrue_IfCombatSummaryPermitsAttackingAfterAttacking() {
            var attacker = BuildUnit(100, false, new UnitCombatSummary() { CanAttackAfterAttacking = true });
            var defender = BuildUnit(100, false, new UnitCombatSummary());

            var combatInfo = new CombatInfo();

            var executionLogic = Container.Resolve<CommonCombatExecutionLogic>();

            executionLogic.PerformCommonCombatTasks(attacker, defender, combatInfo);

            Assert.IsTrue(attacker.CanAttack);
        }

        [Test]
        public void PerformCommonCombatTasks_CanAttackSetToFalse_IfCombatSummaryDoesNotPermitAttackingAfterAttacking() {
            var attacker = BuildUnit(100, true, new UnitCombatSummary() { CanAttackAfterAttacking = false });
            var defender = BuildUnit(100, false, new UnitCombatSummary());

            var combatInfo = new CombatInfo();

            var executionLogic = Container.Resolve<CommonCombatExecutionLogic>();

            executionLogic.PerformCommonCombatTasks(attacker, defender, combatInfo);

            Assert.IsFalse(attacker.CanAttack);
        }

        #endregion

        #region utilities

        private IUnit BuildUnit(int currentHitpoints, bool canAttack, UnitCombatSummary combatSummary) {
            var mockUnit = new Mock<IUnit>();

            mockUnit.SetupAllProperties();
            mockUnit.Setup(unit => unit.CombatSummary).Returns(combatSummary);

            var newUnit = mockUnit.Object;

            newUnit.CurrentHitpoints = currentHitpoints;
            newUnit.CanAttack        = canAttack;

            return newUnit;
        }

        private void BuildResponder() {
            var mockResponder = new Mock<IPostCombatResponder>();

            MockPostCombatResponders.Add(mockResponder);
        }

        #endregion

        #endregion

    }

}
