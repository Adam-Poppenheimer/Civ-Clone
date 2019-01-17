using System;
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

    public class CombatExecuterTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IMeleeAttackValidityLogic>   MockMeleeAttackValidityLogic;
        private Mock<IRangedAttackValidityLogic>  MockRangedAttackValidityLogic;
        private Mock<IUnitPositionCanon>          MockUnitPositionCanon;
        private Mock<IHexGrid>                    MockGrid;
        private Mock<ICombatInfoLogic>            MockCombatInfoLogic;
        private Mock<IHexPathfinder>              MockHexPathfinder;
        private Mock<ICommonCombatExecutionLogic> MockCommonCombatExecutionLogic;

        private UnitSignals UnitSignals;

        private List<IHexCell> AllCells = new List<IHexCell>();

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            AllCells.Clear();

            MockMeleeAttackValidityLogic  = new Mock<IMeleeAttackValidityLogic>();
            MockRangedAttackValidityLogic = new Mock<IRangedAttackValidityLogic>();
            MockUnitPositionCanon         = new Mock<IUnitPositionCanon>();
            MockGrid                      = new Mock<IHexGrid>();
            MockCombatInfoLogic           = new Mock<ICombatInfoLogic>();
            MockHexPathfinder             = new Mock<IHexPathfinder>();
            MockCommonCombatExecutionLogic = new Mock<ICommonCombatExecutionLogic>();

            MockGrid.Setup(grid => grid.Cells).Returns(AllCells.AsReadOnly());

            Container.Bind<IMeleeAttackValidityLogic>  ().FromInstance(MockMeleeAttackValidityLogic  .Object);
            Container.Bind<IRangedAttackValidityLogic> ().FromInstance(MockRangedAttackValidityLogic .Object);
            Container.Bind<IUnitPositionCanon>         ().FromInstance(MockUnitPositionCanon         .Object);
            Container.Bind<IHexGrid>                   ().FromInstance(MockGrid                      .Object);
            Container.Bind<ICombatInfoLogic>           ().FromInstance(MockCombatInfoLogic           .Object);
            Container.Bind<IHexPathfinder>             ().FromInstance(MockHexPathfinder             .Object);
            Container.Bind<ICommonCombatExecutionLogic>().FromInstance(MockCommonCombatExecutionLogic.Object);

            UnitSignals = new UnitSignals();

            Container.Bind<UnitSignals>().FromInstance(UnitSignals);

            Container.Bind<CombatExecuter>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void CanPerformMeleeAttack_EchoesMeleeAttackValidityLogic() {
            var attacker = BuildUnit(currentMovement: 1f, currentHitpoints: 100, location: BuildCell());
            var defender = BuildUnit(currentMovement: 1f, currentHitpoints: 100, location: BuildCell());

            var executer = Container.Resolve<CombatExecuter>();

            Assert.IsFalse(executer.CanPerformMeleeAttack(attacker, defender), "CanPerformMeleeAttack did not return false as expected");

            MockMeleeAttackValidityLogic.Setup(logic => logic.IsMeleeAttackValid(attacker, defender)).Returns(true);

            Assert.IsTrue(executer.CanPerformMeleeAttack(attacker, defender), "CanPerformMeleeAttack did not return true as expected");
        }

        [Test]
        public void CanPerformRangedAttack_EchoesRangedAttackValidityLogic() {
            var attacker = BuildUnit(currentMovement: 1f, currentHitpoints: 100, location: BuildCell());
            var defender = BuildUnit(currentMovement: 1f, currentHitpoints: 100, location: BuildCell());

            var executer = Container.Resolve<CombatExecuter>();

            Assert.IsFalse(executer.CanPerformRangedAttack(attacker, defender), "CanPerformRangedAttack did not return false as expected");

            MockRangedAttackValidityLogic.Setup(logic => logic.IsRangedAttackValid(attacker, defender)).Returns(true);

            Assert.IsTrue(executer.CanPerformRangedAttack(attacker, defender), "CanPerformRangedAttack did not return true as expected");
        }

        [Test]
        public void PerformMeleeAttack_OrdersAttackerToWalkUpToDefender() {
            var attackerLocation = BuildCell();
            var defenderLocation = BuildCell();

            Mock<IUnit> attackerMock;
            var attacker = BuildUnit(3f, 100, attackerLocation, out attackerMock);
            var defender = BuildUnit(currentMovement: 0f, currentHitpoints: 100, location: defenderLocation);

            var path = new List<IHexCell>() {
                BuildCell(), BuildCell(), BuildCell()
            };

            var shorterPath = path.GetRange(0, path.Count - 1);

            MockHexPathfinder.Setup(pathfinder => pathfinder.GetShortestPathBetween(
                attackerLocation, defenderLocation, 3f, It.IsAny<Func<IHexCell, IHexCell, float>>(),
                AllCells
            )).Returns(path);

            MockMeleeAttackValidityLogic.Setup(logic => logic.IsMeleeAttackValid(attacker, defender)).Returns(true);

            var executer = Container.Resolve<CombatExecuter>();

            executer.PerformMeleeAttack(attacker, defender);

            attackerMock.VerifySet(
                unit => unit.CurrentPath = It.Is<List<IHexCell>>(list => list.SequenceEqual(shorterPath)),
                Times.Once, "Attacker not given the expected path"
            );

            attackerMock.Verify(
                unit => unit.PerformMovement(false, It.IsAny<Action>()),
                Times.Once, "Attacker.PerformMovement not called as expected"
            );
        }

        [Test]
        public void PerformMeleeAttack_AfterMovement_PerformsCommonCombatTasks() {
            var attackerLocation = BuildCell();
            var defenderLocation = BuildCell();

            Mock<IUnit> attackerMock;
            var attacker = BuildUnit(3f, 100, attackerLocation, out attackerMock);
            var defender = BuildUnit(currentMovement: 0f, currentHitpoints: 100, location: defenderLocation);

            var path = new List<IHexCell>() { BuildCell() };

            MockHexPathfinder.Setup(pathfinder => pathfinder.GetShortestPathBetween(
                attackerLocation, defenderLocation, 3f, It.IsAny<Func<IHexCell, IHexCell, float>>(),
                AllCells
            )).Returns(path);

            var combatInfo = new CombatInfo();

            MockCombatInfoLogic.Setup(logic => logic.GetAttackInfo(attacker, defender, defenderLocation, CombatType.Melee))
                               .Returns(combatInfo);

            MockMeleeAttackValidityLogic.Setup(logic => logic.IsMeleeAttackValid(attacker, defender)).Returns(true);

            attackerMock.Setup(unit => unit.PerformMovement(false, It.IsAny<Action>()))
                        .Callback<bool, Action>((ignoreMovementCosts, postMovementCallback) => postMovementCallback());

            var executer = Container.Resolve<CombatExecuter>();

            executer.PerformMeleeAttack(attacker, defender);

            MockCommonCombatExecutionLogic.Verify(
                logic => logic.PerformCommonCombatTasks(attacker, defender, combatInfo), Times.Once
            );
        }

        [Test]
        public void PerformMeleeAttack_AfterMovement_FiresMeleeCombatWithUnitSignal() {
            var attackerLocation = BuildCell();
            var defenderLocation = BuildCell();

            Mock<IUnit> attackerMock;
            var attacker = BuildUnit(3f, 100, attackerLocation, out attackerMock);
            var defender = BuildUnit(currentMovement: 0f, currentHitpoints: 100, location: defenderLocation);

            var path = new List<IHexCell>() { BuildCell() };

            MockHexPathfinder.Setup(pathfinder => pathfinder.GetShortestPathBetween(
                attackerLocation, defenderLocation, 3f, It.IsAny<Func<IHexCell, IHexCell, float>>(),
                AllCells
            )).Returns(path);

            var combatInfo = new CombatInfo();

            MockCombatInfoLogic.Setup(logic => logic.GetAttackInfo(attacker, defender, defenderLocation, CombatType.Melee))
                               .Returns(combatInfo);

            MockMeleeAttackValidityLogic.Setup(logic => logic.IsMeleeAttackValid(attacker, defender)).Returns(true);

            attackerMock.Setup(unit => unit.PerformMovement(false, It.IsAny<Action>()))
                        .Callback<bool, Action>((ignoreMovementCosts, postMovementCallback) => postMovementCallback());

            MockCommonCombatExecutionLogic.Setup(
                logic => logic.PerformCommonCombatTasks(attacker, defender, combatInfo)
            ).Callback<IUnit, IUnit, CombatInfo>(
                (attkr, dfndr, info) => { attkr.CurrentHitpoints -= 30; dfndr.CurrentHitpoints -= 40; }
            );

            UnitSignals.MeleeCombatWithUnitSignal.Subscribe(delegate(UnitCombatResults results) {
                Assert.AreEqual(attacker,   results.Attacker,         "Results had an unexpected Attacker");
                Assert.AreEqual(defender,   results.Defender,         "Results had an unexpected Defender");
                Assert.AreEqual(30,         results.DamageToAttacker, "Results had an unexpected DamageToAttacker");
                Assert.AreEqual(40,         results.DamageToDefender, "Results had an unexpected DamageToDefender");
                Assert.AreEqual(combatInfo, results.InfoOfAttack,     "Results had an unexpected Attacker");
                Assert.Pass();
            });

            var executer = Container.Resolve<CombatExecuter>();

            executer.PerformMeleeAttack(attacker, defender);

            Assert.Fail("MeleeCombatWithUnitSignal never fired");
        }

        [Test]
        public void PerformRangedAttack_PerformsCommonCombatTasks() {
            var attackerLocation = BuildCell();
            var defenderLocation = BuildCell();

            Mock<IUnit> attackerMock;
            var attacker = BuildUnit(3f, 100, attackerLocation, out attackerMock);
            var defender = BuildUnit(currentMovement: 0f, currentHitpoints: 100, location: defenderLocation);

            var combatInfo = new CombatInfo();

            MockCombatInfoLogic.Setup(logic => logic.GetAttackInfo(attacker, defender, defenderLocation, CombatType.Ranged))
                               .Returns(combatInfo);

            MockRangedAttackValidityLogic.Setup(logic => logic.IsRangedAttackValid(attacker, defender)).Returns(true);

            attackerMock.Setup(unit => unit.PerformMovement(false, It.IsAny<Action>()))
                        .Callback<bool, Action>((ignoreMovementCosts, postMovementCallback) => postMovementCallback());

            MockCommonCombatExecutionLogic.Setup(
                logic => logic.PerformCommonCombatTasks(attacker, defender, combatInfo)
            ).Callback<IUnit, IUnit, CombatInfo>(
                (attkr, dfndr, info) => { attkr.CurrentHitpoints -= 30; dfndr.CurrentHitpoints -= 40; }
            );

            UnitSignals.RangedCombatWithUnitSignal.Subscribe(delegate(UnitCombatResults results) {
                Assert.AreEqual(attacker,   results.Attacker,         "Results had an unexpected Attacker");
                Assert.AreEqual(defender,   results.Defender,         "Results had an unexpected Defender");
                Assert.AreEqual(30,         results.DamageToAttacker, "Results had an unexpected DamageToAttacker");
                Assert.AreEqual(40,         results.DamageToDefender, "Results had an unexpected DamageToDefender");
                Assert.AreEqual(combatInfo, results.InfoOfAttack,     "Results had an unexpected Attacker");
                Assert.Pass();
            });

            var executer = Container.Resolve<CombatExecuter>();

            executer.PerformRangedAttack(attacker, defender);

            Assert.Fail("RangedCombatWithUnitSignal never fired");
        }

        [Test]
        public void PerformRangedAttack_FiresRangedCombatWithUnitSignal() {
            var attackerLocation = BuildCell();
            var defenderLocation = BuildCell();

            Mock<IUnit> attackerMock;
            var attacker = BuildUnit(3f, 100, attackerLocation, out attackerMock);
            var defender = BuildUnit(currentMovement: 0f, currentHitpoints: 100, location: defenderLocation);

            var combatInfo = new CombatInfo();

            MockCombatInfoLogic.Setup(logic => logic.GetAttackInfo(attacker, defender, defenderLocation, CombatType.Ranged))
                               .Returns(combatInfo);

            MockRangedAttackValidityLogic.Setup(logic => logic.IsRangedAttackValid(attacker, defender)).Returns(true);

            attackerMock.Setup(unit => unit.PerformMovement(false, It.IsAny<Action>()))
                        .Callback<bool, Action>((ignoreMovementCosts, postMovementCallback) => postMovementCallback());

            var executer = Container.Resolve<CombatExecuter>();

            executer.PerformRangedAttack(attacker, defender);

            MockCommonCombatExecutionLogic.Verify(
                logic => logic.PerformCommonCombatTasks(attacker, defender, combatInfo), Times.Once
            );
        }

        #endregion

        #region utilities

        private IHexCell BuildCell() {
            var newCell = new Mock<IHexCell>().Object;

            AllCells.Add(newCell);

            return newCell;
        }

        private IUnit BuildUnit(float currentMovement, int currentHitpoints, IHexCell location) {
            Mock<IUnit> mock;

            return BuildUnit(currentMovement, currentHitpoints, location, out mock);
        }

        private IUnit BuildUnit(float currentMovement, int currentHitpoints, IHexCell location, out Mock<IUnit> mock) {
            mock = new Mock<IUnit>();

            mock.SetupAllProperties();

            var newUnit = mock.Object;

            newUnit.CurrentMovement  = currentMovement;
            newUnit.CurrentHitpoints = currentHitpoints;

            MockUnitPositionCanon.Setup(canon => canon.GetOwnerOfPossession(newUnit)).Returns(location);

            return newUnit;
        }

        #endregion

        #endregion

    }

}
