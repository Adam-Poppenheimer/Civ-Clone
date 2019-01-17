using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation.Units;
using Assets.Simulation.Units.Combat;
using Assets.Simulation.HexMap;

namespace Assets.Tests.Simulation.Units.Combat {

    [TestFixture]
    public class MovementPostCombatResponderTests : ZenjectUnitTestFixture {

        #region internal types

        public class HandleAttackerMovementAfterCombatTestData {

            public UnitTestData Attacker;
            public UnitTestData Defender;

            public int TraversalCostBetweenLocations;

            public CombatInfo CombatInfo;

        }

        public class UnitTestData {

            public int CurrentMovement;

            public UnitCombatSummary CombatSummary = new UnitCombatSummary();

        }

        #endregion

        #region static fields and properties

        public static IEnumerable HandleAttackerMovementAfterCombatTestCases {
            get {
                yield return new TestCaseData(new HandleAttackerMovementAfterCombatTestData() {
                    Attacker = new UnitTestData() {
                        CurrentMovement = 3,
                        CombatSummary = new UnitCombatSummary() { CanMoveAfterAttacking = false }
                    },
                    Defender = new UnitTestData(),
                    TraversalCostBetweenLocations = 1
                }).SetName("AttackerCanMoveAfterAttacking false | clears movement").Returns(0f);

                yield return new TestCaseData(new HandleAttackerMovementAfterCombatTestData() {
                    Attacker = new UnitTestData() {
                        CurrentMovement = 3,
                        CombatSummary = new UnitCombatSummary() { CanMoveAfterAttacking = true }
                    },
                    Defender = new UnitTestData(),
                    TraversalCostBetweenLocations = 2,
                    CombatInfo = new CombatInfo() { CombatType = CombatType.Melee }
                }).SetName("AttackerCanMoveAfterAttacking true and melee combat | reduces movement by travel cost").Returns(1f);

                yield return new TestCaseData(new HandleAttackerMovementAfterCombatTestData() {
                    Attacker = new UnitTestData() {
                        CurrentMovement = 3,
                        CombatSummary = new UnitCombatSummary() { CanMoveAfterAttacking = true }
                    },
                    Defender = new UnitTestData(),
                    TraversalCostBetweenLocations = 2,
                    CombatInfo = new CombatInfo() { CombatType = CombatType.Ranged }
                }).SetName("AttackerCanMoveAfterAttacking true and ranged combat | reduces movement by one").Returns(2f);

                yield return new TestCaseData(new HandleAttackerMovementAfterCombatTestData() {
                    Attacker = new UnitTestData() {
                        CurrentMovement = 3,
                        CombatSummary = new UnitCombatSummary() { CanMoveAfterAttacking = true }
                    },
                    Defender = new UnitTestData(),
                    TraversalCostBetweenLocations = 10,
                    CombatInfo = new CombatInfo() { CombatType = CombatType.Melee }
                }).SetName("AttackerCanMoveAfterAttacking true and melee combat | movement floors at 0").Returns(0f);
            }
        }

        #endregion

        #region instance fields and properties

        private Mock<IUnitPositionCanon> MockUnitPositionCanon;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockUnitPositionCanon = new Mock<IUnitPositionCanon>();

            Container.Bind<IUnitPositionCanon>().FromInstance(MockUnitPositionCanon.Object);

            Container.Bind<MovementPostCombatResponder>().AsSingle();
        }

        #endregion

        #region tests

        [Test(Description = "")]
        [TestCaseSource("HandleAttackerMovementAfterCombatTestCases")]
        public float HandleAttackerMovementAfterCombatTests(HandleAttackerMovementAfterCombatTestData testData) {
            var attackerLocation = BuildCell();
            var defenderLocation = BuildCell();

            var attacker = BuildUnit(testData.Attacker, attackerLocation);
            var defender = BuildUnit(testData.Defender, defenderLocation);

            MockUnitPositionCanon
                .Setup(logic => logic.GetTraversalCostForUnit(attacker, attackerLocation, defenderLocation, true))
                .Returns(testData.TraversalCostBetweenLocations);

            var movementLogic = Container.Resolve<MovementPostCombatResponder>();

            movementLogic.RespondToCombat(attacker, defender, testData.CombatInfo);

            return attacker.CurrentMovement;
        }

        #endregion

        #region utilities

        private IHexCell BuildCell() {
            return new Mock<IHexCell>().Object;
        }

        private IUnit BuildUnit(UnitTestData unitData, IHexCell location) {
            var mockUnit = new Mock<IUnit>();

            mockUnit.SetupAllProperties();

            mockUnit.Setup(unit => unit.CombatSummary).Returns(unitData.CombatSummary);

            var newUnit = mockUnit.Object;

            newUnit.CurrentMovement = unitData.CurrentMovement;

            MockUnitPositionCanon.Setup(canon => canon.GetOwnerOfPossession(newUnit)).Returns(location);

            return newUnit;
        }

        #endregion

        #endregion

    }

}
