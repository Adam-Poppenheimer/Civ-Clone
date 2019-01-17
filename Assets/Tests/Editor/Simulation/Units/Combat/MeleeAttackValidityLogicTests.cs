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

    public class MeleeAttackValidityLogicTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IUnitPositionCanon>         MockUnitPositionCanon;
        private Mock<IHexGrid>                   MockGrid;
        private Mock<IHexPathfinder>             MockHexPathfinder;
        private Mock<IUnitLineOfSightLogic>      MockLineOfSightLogic;
        private Mock<ICommonAttackValidityLogic> MockCommonAttackValidityLogic;

        private List<IHexCell> AllCells = new List<IHexCell>();

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            AllCells.Clear();

            MockUnitPositionCanon         = new Mock<IUnitPositionCanon>();
            MockGrid                      = new Mock<IHexGrid>();
            MockHexPathfinder             = new Mock<IHexPathfinder>();
            MockLineOfSightLogic          = new Mock<IUnitLineOfSightLogic>();
            MockCommonAttackValidityLogic = new Mock<ICommonAttackValidityLogic>();

            MockGrid.Setup(grid => grid.Cells).Returns(AllCells.AsReadOnly());

            Container.Bind<IUnitPositionCanon>        ().FromInstance(MockUnitPositionCanon        .Object);
            Container.Bind<IHexGrid>                  ().FromInstance(MockGrid                     .Object);
            Container.Bind<IHexPathfinder>            ().FromInstance(MockHexPathfinder            .Object);
            Container.Bind<IUnitLineOfSightLogic>     ().FromInstance(MockLineOfSightLogic         .Object);
            Container.Bind<ICommonAttackValidityLogic>().FromInstance(MockCommonAttackValidityLogic.Object);

            Container.Bind<MeleeAttackValidityLogic>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void IsMeleeAttackValid_FalseIfDoesntMeetCommonConditions() {
            var attackerLocation = BuildCell();
            var defenderLocation = BuildCell();

            var attacker = BuildUnit(attackerLocation, 3.5f, 1, BuildCell(), defenderLocation);
            var defender = BuildUnit(defenderLocation, 0f, 0);

            MockCommonAttackValidityLogic.Setup(
                logic => logic.DoesAttackMeetCommonConditions(attacker, defender)
            ).Returns(false);

            List<IHexCell> pathTo = new List<IHexCell>();

            MockHexPathfinder.Setup(
                pathfinder => pathfinder.GetShortestPathBetween(
                    attackerLocation, defenderLocation, 3.5f, It.IsAny<Func<IHexCell, IHexCell, float>>(),
                    AllCells
                )
            ).Returns(pathTo);

            var validityLogic = Container.Resolve<MeleeAttackValidityLogic>();

            Assert.IsFalse(validityLogic.IsMeleeAttackValid(attacker, defender));
        }

        [Test]
        public void IsMeleeAttackValid_FalseIfNoPathFromAttackerToDefender_WithinCurrentMovementOfAttacker() {
            var attackerLocation = BuildCell();
            var defenderLocation = BuildCell();

            var attacker = BuildUnit(attackerLocation, 3.5f, 1, BuildCell(), defenderLocation);
            var defender = BuildUnit(defenderLocation, 0f, 0);

            MockCommonAttackValidityLogic.Setup(
                logic => logic.DoesAttackMeetCommonConditions(attacker, defender)
            ).Returns(true);

            List<IHexCell> pathTo = null;

            MockHexPathfinder.Setup(
                pathfinder => pathfinder.GetShortestPathBetween(
                    attackerLocation, defenderLocation, 3.5f, It.IsAny<Func<IHexCell, IHexCell, float>>(),
                    AllCells
                )
            ).Returns(pathTo);

            var validityLogic = Container.Resolve<MeleeAttackValidityLogic>();

            Assert.IsFalse(validityLogic.IsMeleeAttackValid(attacker, defender));
        }

        [Test]
        public void IsMeleeAttackValid_FalseIfDefenderNotInLineOfSightOfAttacker() {
            var attackerLocation = BuildCell();
            var defenderLocation = BuildCell();

            var attacker = BuildUnit(attackerLocation, 3.5f, 1, BuildCell());
            var defender = BuildUnit(defenderLocation, 0f, 0);

            MockCommonAttackValidityLogic.Setup(
                logic => logic.DoesAttackMeetCommonConditions(attacker, defender)
            ).Returns(true);

            List<IHexCell> pathTo = new List<IHexCell>();

            MockHexPathfinder.Setup(
                pathfinder => pathfinder.GetShortestPathBetween(
                    attackerLocation, defenderLocation, 3.5f, It.IsAny<Func<IHexCell, IHexCell, float>>(),
                    AllCells
                )
            ).Returns(pathTo);

            var validityLogic = Container.Resolve<MeleeAttackValidityLogic>();

            Assert.IsFalse(validityLogic.IsMeleeAttackValid(attacker, defender));
        }

        [Test]
        public void IsMeleeAttackValid_FalseIfAttackerCombatStrengthNotPositive() {
            var attackerLocation = BuildCell();
            var defenderLocation = BuildCell();

            var attacker = BuildUnit(attackerLocation, 3.5f, 0, BuildCell(), defenderLocation);
            var defender = BuildUnit(defenderLocation, 0f, 0);

            MockCommonAttackValidityLogic.Setup(
                logic => logic.DoesAttackMeetCommonConditions(attacker, defender)
            ).Returns(true);

            List<IHexCell> pathTo = new List<IHexCell>();

            MockHexPathfinder.Setup(
                pathfinder => pathfinder.GetShortestPathBetween(
                    attackerLocation, defenderLocation, 3.5f, It.IsAny<Func<IHexCell, IHexCell, float>>(),
                    AllCells
                )
            ).Returns(pathTo);

            var validityLogic = Container.Resolve<MeleeAttackValidityLogic>();

            Assert.IsFalse(validityLogic.IsMeleeAttackValid(attacker, defender));
        }

        [Test]
        public void IsMeleeAttackValid_TrueIfNoInvalidatingConditionsMet() {
            var attackerLocation = BuildCell();
            var defenderLocation = BuildCell();

            var attacker = BuildUnit(attackerLocation, 3.5f, 1, BuildCell(), defenderLocation);
            var defender = BuildUnit(defenderLocation, 0f, 0);

            MockCommonAttackValidityLogic.Setup(
                logic => logic.DoesAttackMeetCommonConditions(attacker, defender)
            ).Returns(true);

            List<IHexCell> pathTo = new List<IHexCell>();

            MockHexPathfinder.Setup(
                pathfinder => pathfinder.GetShortestPathBetween(
                    attackerLocation, defenderLocation, 3.5f, It.IsAny<Func<IHexCell, IHexCell, float>>(),
                    AllCells
                )
            ).Returns(pathTo);

            var validityLogic = Container.Resolve<MeleeAttackValidityLogic>();

            Assert.IsTrue(validityLogic.IsMeleeAttackValid(attacker, defender));
        }

        #endregion

        #region utilities

        private IHexCell BuildCell() {
            var newCell = new Mock<IHexCell>().Object;

            AllCells.Add(newCell);

            return newCell;
        }

        private IUnit BuildUnit(
            IHexCell location, float currentMovement, int combatStrength, params IHexCell[] visibleCells
        ) {
            var mockUnit = new Mock<IUnit>();
            
            mockUnit.Setup(unit => unit.CurrentMovement).Returns(currentMovement);
            mockUnit.Setup(unit => unit.CombatStrength) .Returns(combatStrength);

            var newUnit = mockUnit.Object;

            MockUnitPositionCanon.Setup(canon => canon.GetOwnerOfPossession(newUnit)).Returns(location);

            MockLineOfSightLogic.Setup(logic => logic.GetCellsVisibleToUnit(newUnit)).Returns(visibleCells);

            return newUnit;
        }

        #endregion

        #endregion

    }

}
