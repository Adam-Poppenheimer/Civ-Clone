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
    public class PostCombatMovementLogicTests : ZenjectUnitTestFixture {

        #region internal types

        public class HandleAttackerMovementAfterCombatTestData {

            public UnitTestData Attacker;
            public UnitTestData Defender;

            public int TraversalCostBetweenLocations;

            public CombatInfo CombatInfo;

        }

        public class UnitTestData {

            public int CurrentMovement;

        }

        #endregion

        #region static fields and properties

        public static IEnumerable HandleAttackerMovementAfterCombatTestCases {
            get {
                yield return new TestCaseData(new HandleAttackerMovementAfterCombatTestData() {
                    Attacker = new UnitTestData() { CurrentMovement = 3 },
                    Defender = new UnitTestData(),
                    TraversalCostBetweenLocations = 1,
                    CombatInfo = new CombatInfo() {
                        Attacker = new UnitCombatInfo() { CanMoveAfterAttacking = false }
                    }
                }).SetName("AttackerCanMoveAfterAttacking false | clears movement").Returns(0f);

                yield return new TestCaseData(new HandleAttackerMovementAfterCombatTestData() {
                    Attacker = new UnitTestData() { CurrentMovement = 3 },
                    Defender = new UnitTestData(),
                    TraversalCostBetweenLocations = 2,
                    CombatInfo = new CombatInfo() {
                        CombatType = CombatType.Melee,
                        Attacker = new UnitCombatInfo() { CanMoveAfterAttacking = true }
                    }
                }).SetName("AttackerCanMoveAfterAttacking true and melee combat | reduces movement by travel cost").Returns(1f);

                yield return new TestCaseData(new HandleAttackerMovementAfterCombatTestData() {
                    Attacker = new UnitTestData() { CurrentMovement = 3 },
                    Defender = new UnitTestData(),
                    TraversalCostBetweenLocations = 2,
                    CombatInfo = new CombatInfo() {
                        CombatType = CombatType.Ranged,
                        Attacker = new UnitCombatInfo() { CanMoveAfterAttacking = true }
                    }
                }).SetName("AttackerCanMoveAfterAttacking true and ranged combat | reduces movement by one").Returns(2f);

                yield return new TestCaseData(new HandleAttackerMovementAfterCombatTestData() {
                    Attacker = new UnitTestData() { CurrentMovement = 3 },
                    Defender = new UnitTestData(),
                    TraversalCostBetweenLocations = 10,
                    CombatInfo = new CombatInfo() {
                        CombatType = CombatType.Melee,
                        Attacker = new UnitCombatInfo() { CanMoveAfterAttacking = true }
                    }
                }).SetName("AttackerCanMoveAfterAttacking true and melee combat | movement floors at 0").Returns(0f);
            }
        }

        #endregion

        #region instance fields and properties

        private Mock<IUnitPositionCanon>    MockUnitPositionCanon;
        private Mock<IUnitTerrainCostLogic> MockTerrainCostLogic;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockUnitPositionCanon = new Mock<IUnitPositionCanon>();
            MockTerrainCostLogic  = new Mock<IUnitTerrainCostLogic>();

            Container.Bind<IUnitPositionCanon>   ().FromInstance(MockUnitPositionCanon.Object);
            Container.Bind<IUnitTerrainCostLogic>().FromInstance(MockTerrainCostLogic .Object);

            Container.Bind<PostCombatMovementLogic>().AsSingle();
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

            MockTerrainCostLogic
                .Setup(logic => logic.GetTraversalCostForUnit(attacker, attackerLocation, defenderLocation))
                .Returns(testData.TraversalCostBetweenLocations);

            var movementLogic = Container.Resolve<PostCombatMovementLogic>();

            movementLogic.HandleAttackerMovementAfterCombat(attacker, defender, testData.CombatInfo);

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

            var newUnit = mockUnit.Object;

            newUnit.CurrentMovement = unitData.CurrentMovement;

            MockUnitPositionCanon.Setup(canon => canon.GetOwnerOfPossession(newUnit)).Returns(location);

            return newUnit;
        }

        #endregion

        #endregion

    }

}
