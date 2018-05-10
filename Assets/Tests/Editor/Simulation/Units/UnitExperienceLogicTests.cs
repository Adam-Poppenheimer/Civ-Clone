using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation.Units;
using Assets.Simulation.Units.Promotions;
using Assets.Simulation.Units.Combat;

namespace Assets.Tests.Simulation.Units {

    [TestFixture]
    public class UnitExperienceLogicTests : ZenjectUnitTestFixture {

        #region internal types

        public class GetExperienceForNextLevelOnUnitTestData {

            public UnitTestData Unit;

            public UnitConfigTestData UnitConfig;

        }

        public class CombatExperienceGainTestData {

            public UnitTestData Attacker;
            public UnitTestData Defender;

            public UnitConfigTestData UnitConfig;

            public int DamageToDefender;

        }

        public class UnitTestData {

            public int Level;

            public UnitType Type;

            public int Hitpoints;

        }

        public class UnitConfigTestData {

            public int NextLevelExperienceCoefficient;
            public int MaxLevel;

            public int MeleeAttackerExperience;
            public int MeleeDefenderExperience;

            public int RangedAttackerExperience;
            public int RangedDefenderExperience;

        }

        #endregion

        #region static fields and properties

        public static IEnumerable GetExperienceForNextLevelOnUnitTestCases {
            get {
                yield return new TestCaseData(new GetExperienceForNextLevelOnUnitTestData() {
                    Unit = new UnitTestData() { Level = 1 },
                    UnitConfig = new UnitConfigTestData() {
                        MaxLevel = 10, NextLevelExperienceCoefficient = 10
                    }
                }).SetName("MaxLevel = 10, NextLevelExperienceCoefficient = 10, Unit.Level = 1").Returns(10);

                yield return new TestCaseData(new GetExperienceForNextLevelOnUnitTestData() {
                    Unit = new UnitTestData() { Level = 2 },
                    UnitConfig = new UnitConfigTestData() {
                        MaxLevel = 10, NextLevelExperienceCoefficient = 10
                    }
                }).SetName("MaxLevel = 10, NextLevelExperienceCoefficient = 10, Unit.Level = 2").Returns(20);

                yield return new TestCaseData(new GetExperienceForNextLevelOnUnitTestData() {
                    Unit = new UnitTestData() { Level = 3 },
                    UnitConfig = new UnitConfigTestData() {
                        MaxLevel = 10, NextLevelExperienceCoefficient = 10
                    }
                }).SetName("MaxLevel = 10, NextLevelExperienceCoefficient = 10, Unit.Level = 3").Returns(30);

                yield return new TestCaseData(new GetExperienceForNextLevelOnUnitTestData() {
                    Unit = new UnitTestData() { Level = 4 },
                    UnitConfig = new UnitConfigTestData() {
                        MaxLevel = 10, NextLevelExperienceCoefficient = 10
                    }
                }).SetName("MaxLevel = 10, NextLevelExperienceCoefficient = 10, Unit.Level = 4").Returns(40);

                yield return new TestCaseData(new GetExperienceForNextLevelOnUnitTestData() {
                    Unit = new UnitTestData() { Level = 5 },
                    UnitConfig = new UnitConfigTestData() {
                        MaxLevel = 10, NextLevelExperienceCoefficient = 10
                    }
                }).SetName("MaxLevel = 10, NextLevelExperienceCoefficient = 10, Unit.Level = 5").Returns(50);

                yield return new TestCaseData(new GetExperienceForNextLevelOnUnitTestData() {
                    Unit = new UnitTestData() { Level = 6 },
                    UnitConfig = new UnitConfigTestData() {
                        MaxLevel = 10, NextLevelExperienceCoefficient = 10
                    }
                }).SetName("MaxLevel = 10, NextLevelExperienceCoefficient = 10, Unit.Level = 6").Returns(60);

                yield return new TestCaseData(new GetExperienceForNextLevelOnUnitTestData() {
                    Unit = new UnitTestData() { Level = 7 },
                    UnitConfig = new UnitConfigTestData() {
                        MaxLevel = 10, NextLevelExperienceCoefficient = 10
                    }
                }).SetName("MaxLevel = 10, NextLevelExperienceCoefficient = 10, Unit.Level = 7").Returns(70);

                yield return new TestCaseData(new GetExperienceForNextLevelOnUnitTestData() {
                    Unit = new UnitTestData() { Level = 8 },
                    UnitConfig = new UnitConfigTestData() {
                        MaxLevel = 10, NextLevelExperienceCoefficient = 10
                    }
                }).SetName("MaxLevel = 10, NextLevelExperienceCoefficient = 10, Unit.Level = 8").Returns(80);

                yield return new TestCaseData(new GetExperienceForNextLevelOnUnitTestData() {
                    Unit = new UnitTestData() { Level = 9 },
                    UnitConfig = new UnitConfigTestData() {
                        MaxLevel = 10, NextLevelExperienceCoefficient = 10
                    }
                }).SetName("MaxLevel = 10, NextLevelExperienceCoefficient = 10, Unit.Level = 9").Returns(90);

                yield return new TestCaseData(new GetExperienceForNextLevelOnUnitTestData() {
                    Unit = new UnitTestData() { Level = 10 },
                    UnitConfig = new UnitConfigTestData() {
                        MaxLevel = 10, NextLevelExperienceCoefficient = 10
                    }
                }).SetName("MaxLevel = 10, NextLevelExperienceCoefficient = 10, Unit.Level = 10").Returns(int.MaxValue);
            }
        }

        public static IEnumerable AttackerExperienceGainFromMeleeCombatTestCases {
            get {
                yield return new TestCaseData(new CombatExperienceGainTestData() {
                    Attacker = new UnitTestData() { Type = UnitType.Melee, Hitpoints = 50 },
                    Defender = new UnitTestData() { Type = UnitType.Melee, Hitpoints = 50 },
                    DamageToDefender = 50,
                    UnitConfig = new UnitConfigTestData() { MeleeAttackerExperience = 5 }
                }).SetName("Two melee units, defender and attacker survive").Returns(5);

                yield return new TestCaseData(new CombatExperienceGainTestData() {
                    Attacker = new UnitTestData() { Type = UnitType.Melee, Hitpoints = 50 },
                    Defender = new UnitTestData() { Type = UnitType.Melee, Hitpoints = 0 },
                    DamageToDefender = 50,
                    UnitConfig = new UnitConfigTestData() { MeleeAttackerExperience = 5 }
                }).SetName("Two melee units, defender does not survive").Returns(5);

                yield return new TestCaseData(new CombatExperienceGainTestData() {
                    Attacker = new UnitTestData() { Type = UnitType.Melee, Hitpoints = 0 },
                    Defender = new UnitTestData() { Type = UnitType.Melee, Hitpoints = 50 },
                    DamageToDefender = 50,
                    UnitConfig = new UnitConfigTestData() { MeleeAttackerExperience = 5 }
                }).SetName("Two melee units, attacker does not survive").Returns(0);

                yield return new TestCaseData(new CombatExperienceGainTestData() {
                    Attacker = new UnitTestData() { Type = UnitType.Melee, Hitpoints = 50 },
                    Defender = new UnitTestData() { Type = UnitType.City,  Hitpoints = 50 },
                    DamageToDefender = 50,
                    UnitConfig = new UnitConfigTestData() { MeleeAttackerExperience = 5 }
                }).SetName("Melee attacking city").Returns(5);

                yield return new TestCaseData(new CombatExperienceGainTestData() {
                    Attacker = new UnitTestData() { Type = UnitType.City,  Hitpoints = 50 },
                    Defender = new UnitTestData() { Type = UnitType.Melee, Hitpoints = 50 },
                    DamageToDefender = 50,
                    UnitConfig = new UnitConfigTestData() { MeleeAttackerExperience = 5 }
                }).SetName("Attacker is city").Returns(0);

                yield return new TestCaseData(new CombatExperienceGainTestData() {
                    Attacker = new UnitTestData() { Type = UnitType.Civilian, Hitpoints = 50 },
                    Defender = new UnitTestData() { Type = UnitType.Melee, Hitpoints = 50 },
                    DamageToDefender = 50,
                    UnitConfig = new UnitConfigTestData() { MeleeAttackerExperience = 5 }
                }).SetName("Attacker is civilian").Returns(5);

                yield return new TestCaseData(new CombatExperienceGainTestData() {
                    Attacker = new UnitTestData() { Type = UnitType.Melee, Hitpoints = 50 },
                    Defender = new UnitTestData() { Type = UnitType.Civilian, Hitpoints = 50 },
                    DamageToDefender = 50,
                    UnitConfig = new UnitConfigTestData() { MeleeAttackerExperience = 5 }
                }).SetName("Defender is civilian").Returns(0);
            }
        }

        public static IEnumerable DefenderExperienceGainFromMeleeCombatTestCases {
            get {
                yield return new TestCaseData(new CombatExperienceGainTestData() {
                    Attacker = new UnitTestData() { Type = UnitType.Melee, Hitpoints = 50 },
                    Defender = new UnitTestData() { Type = UnitType.Melee, Hitpoints = 50 },
                    DamageToDefender = 50,
                    UnitConfig = new UnitConfigTestData() { MeleeDefenderExperience = 5 }
                }).SetName("Two melee units, defender and attacker survive").Returns(5);

                yield return new TestCaseData(new CombatExperienceGainTestData() {
                    Attacker = new UnitTestData() { Type = UnitType.Melee, Hitpoints = 50 },
                    Defender = new UnitTestData() { Type = UnitType.Melee, Hitpoints = 0 },
                    DamageToDefender = 50,
                    UnitConfig = new UnitConfigTestData() { MeleeDefenderExperience = 5 }
                }).SetName("Two melee units, defender does not survive").Returns(0);

                yield return new TestCaseData(new CombatExperienceGainTestData() {
                    Attacker = new UnitTestData() { Type = UnitType.Melee, Hitpoints = 0 },
                    Defender = new UnitTestData() { Type = UnitType.Melee, Hitpoints = 50 },
                    DamageToDefender = 50,
                    UnitConfig = new UnitConfigTestData() { MeleeDefenderExperience = 5 }
                }).SetName("Two melee units, attacker does not survive").Returns(5);

                yield return new TestCaseData(new CombatExperienceGainTestData() {
                    Attacker = new UnitTestData() { Type = UnitType.Melee, Hitpoints = 50 },
                    Defender = new UnitTestData() { Type = UnitType.City,  Hitpoints = 50 },
                    DamageToDefender = 50,
                    UnitConfig = new UnitConfigTestData() { MeleeDefenderExperience = 5 }
                }).SetName("Defender is city").Returns(0);

                yield return new TestCaseData(new CombatExperienceGainTestData() {
                    Attacker = new UnitTestData() { Type = UnitType.City,  Hitpoints = 50 },
                    Defender = new UnitTestData() { Type = UnitType.Melee, Hitpoints = 50 },
                    DamageToDefender = 50,
                    UnitConfig = new UnitConfigTestData() { MeleeDefenderExperience = 5 }
                }).SetName("Attacker is city").Returns(5);

                yield return new TestCaseData(new CombatExperienceGainTestData() {
                    Attacker = new UnitTestData() { Type = UnitType.Civilian, Hitpoints = 50 },
                    Defender = new UnitTestData() { Type = UnitType.Melee, Hitpoints = 50 },
                    DamageToDefender = 50,
                    UnitConfig = new UnitConfigTestData() { MeleeDefenderExperience = 5 }
                }).SetName("Attacker is civilian").Returns(5);

                yield return new TestCaseData(new CombatExperienceGainTestData() {
                    Attacker = new UnitTestData() { Type = UnitType.Melee, Hitpoints = 50 },
                    Defender = new UnitTestData() { Type = UnitType.Civilian, Hitpoints = 50 },
                    DamageToDefender = 50,
                    UnitConfig = new UnitConfigTestData() { MeleeDefenderExperience = 5 }
                }).SetName("Defender is civilian").Returns(0);
            }
        }

        public static IEnumerable AttackerExperienceGainFromRangedCombatTestCases {
            get {
                yield return new TestCaseData(new CombatExperienceGainTestData() {
                    Attacker = new UnitTestData() { Type = UnitType.Melee, Hitpoints = 50 },
                    Defender = new UnitTestData() { Type = UnitType.Melee, Hitpoints = 50 },
                    DamageToDefender = 50,
                    UnitConfig = new UnitConfigTestData() { RangedAttackerExperience = 5 }
                }).SetName("Two melee units, defender and attacker survive").Returns(5);

                yield return new TestCaseData(new CombatExperienceGainTestData() {
                    Attacker = new UnitTestData() { Type = UnitType.Melee, Hitpoints = 50 },
                    Defender = new UnitTestData() { Type = UnitType.Melee, Hitpoints = 0 },
                    DamageToDefender = 50,
                    UnitConfig = new UnitConfigTestData() { RangedAttackerExperience = 5 }
                }).SetName("Two melee units, defender does not survive").Returns(5);

                yield return new TestCaseData(new CombatExperienceGainTestData() {
                    Attacker = new UnitTestData() { Type = UnitType.Melee, Hitpoints = 0 },
                    Defender = new UnitTestData() { Type = UnitType.Melee, Hitpoints = 50 },
                    DamageToDefender = 50,
                    UnitConfig = new UnitConfigTestData() { RangedAttackerExperience = 5 }
                }).SetName("Two melee units, attacker does not survive").Returns(0);

                yield return new TestCaseData(new CombatExperienceGainTestData() {
                    Attacker = new UnitTestData() { Type = UnitType.Melee, Hitpoints = 50 },
                    Defender = new UnitTestData() { Type = UnitType.City,  Hitpoints = 50 },
                    DamageToDefender = 50,
                    UnitConfig = new UnitConfigTestData() { RangedAttackerExperience = 5 }
                }).SetName("Melee attacking city, and city has taken damage").Returns(5);

                yield return new TestCaseData(new CombatExperienceGainTestData() {
                    Attacker = new UnitTestData() { Type = UnitType.Melee, Hitpoints = 50 },
                    Defender = new UnitTestData() { Type = UnitType.City,  Hitpoints = 50 },
                    DamageToDefender = 0,
                    UnitConfig = new UnitConfigTestData() { RangedAttackerExperience = 5 }
                }).SetName("Melee attacking city, and city has not taken damage").Returns(0);

                yield return new TestCaseData(new CombatExperienceGainTestData() {
                    Attacker = new UnitTestData() { Type = UnitType.City,  Hitpoints = 50 },
                    Defender = new UnitTestData() { Type = UnitType.Melee, Hitpoints = 50 },
                    DamageToDefender = 50,
                    UnitConfig = new UnitConfigTestData() { RangedAttackerExperience = 5 }
                }).SetName("Attacker is city").Returns(0);

                yield return new TestCaseData(new CombatExperienceGainTestData() {
                    Attacker = new UnitTestData() { Type = UnitType.Civilian, Hitpoints = 50 },
                    Defender = new UnitTestData() { Type = UnitType.Melee, Hitpoints = 50 },
                    DamageToDefender = 50,
                    UnitConfig = new UnitConfigTestData() { RangedAttackerExperience = 5 }
                }).SetName("Attacker is civilian").Returns(5);

                yield return new TestCaseData(new CombatExperienceGainTestData() {
                    Attacker = new UnitTestData() { Type = UnitType.Melee, Hitpoints = 50 },
                    Defender = new UnitTestData() { Type = UnitType.Civilian, Hitpoints = 50 },
                    DamageToDefender = 50,
                    UnitConfig = new UnitConfigTestData() { RangedAttackerExperience = 5 }
                }).SetName("Defender is civilian").Returns(0);
            }
        }

        public static IEnumerable DefenderExperienceGainFromRangedCombatTestCases {
            get {
                yield return new TestCaseData(new CombatExperienceGainTestData() {
                    Attacker = new UnitTestData() { Type = UnitType.Melee, Hitpoints = 50 },
                    Defender = new UnitTestData() { Type = UnitType.Melee, Hitpoints = 50 },
                    DamageToDefender = 50,
                    UnitConfig = new UnitConfigTestData() { RangedDefenderExperience = 5 }
                }).SetName("Two melee units, defender and attacker survive").Returns(5);

                yield return new TestCaseData(new CombatExperienceGainTestData() {
                    Attacker = new UnitTestData() { Type = UnitType.Melee, Hitpoints = 50 },
                    Defender = new UnitTestData() { Type = UnitType.Melee, Hitpoints = 0 },
                    DamageToDefender = 50,
                    UnitConfig = new UnitConfigTestData() { RangedDefenderExperience = 5 }
                }).SetName("Two melee units, defender does not survive").Returns(0);

                yield return new TestCaseData(new CombatExperienceGainTestData() {
                    Attacker = new UnitTestData() { Type = UnitType.Melee, Hitpoints = 0 },
                    Defender = new UnitTestData() { Type = UnitType.Melee, Hitpoints = 50 },
                    DamageToDefender = 50,
                    UnitConfig = new UnitConfigTestData() { RangedDefenderExperience = 5 }
                }).SetName("Two melee units, attacker does not survive").Returns(5);

                yield return new TestCaseData(new CombatExperienceGainTestData() {
                    Attacker = new UnitTestData() { Type = UnitType.Melee, Hitpoints = 50 },
                    Defender = new UnitTestData() { Type = UnitType.City,  Hitpoints = 50 },
                    DamageToDefender = 50,
                    UnitConfig = new UnitConfigTestData() { RangedDefenderExperience = 5 }
                }).SetName("Defender is city").Returns(0);

                yield return new TestCaseData(new CombatExperienceGainTestData() {
                    Attacker = new UnitTestData() { Type = UnitType.City,  Hitpoints = 50 },
                    Defender = new UnitTestData() { Type = UnitType.Melee, Hitpoints = 50 },
                    DamageToDefender = 50,
                    UnitConfig = new UnitConfigTestData() { RangedDefenderExperience = 5 }
                }).SetName("Attacker is city").Returns(5);

                yield return new TestCaseData(new CombatExperienceGainTestData() {
                    Attacker = new UnitTestData() { Type = UnitType.Civilian, Hitpoints = 50 },
                    Defender = new UnitTestData() { Type = UnitType.Melee, Hitpoints = 50 },
                    DamageToDefender = 50,
                    UnitConfig = new UnitConfigTestData() { RangedDefenderExperience = 5 }
                }).SetName("Attacker is civilian").Returns(5);

                yield return new TestCaseData(new CombatExperienceGainTestData() {
                    Attacker = new UnitTestData() { Type = UnitType.Melee, Hitpoints = 50 },
                    Defender = new UnitTestData() { Type = UnitType.Civilian, Hitpoints = 50 },
                    DamageToDefender = 50,
                    UnitConfig = new UnitConfigTestData() { RangedDefenderExperience = 5 }
                }).SetName("Defender is civilian").Returns(0);
            }
        }

        #endregion

        #region instance fields and properties

        private Mock<IUnitConfig> MockUnitConfig;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockUnitConfig = new Mock<IUnitConfig>();

            Container.Bind<IUnitConfig>().FromInstance(MockUnitConfig.Object);
            Container.Bind<UnitSignals>().AsSingle();

            Container.Bind<UnitExperienceLogic>().AsSingle().NonLazy();
        }

        private void SetupConfig(UnitConfigTestData data) {
            MockUnitConfig.Setup(config => config.NextLevelExperienceCoefficient)
                          .Returns(data.NextLevelExperienceCoefficient);

            MockUnitConfig.Setup(config => config.MaxLevel).Returns(data.MaxLevel);

            MockUnitConfig.Setup(config => config.MeleeAttackerExperience).Returns(data.MeleeAttackerExperience);
            MockUnitConfig.Setup(config => config.MeleeDefenderExperience).Returns(data.MeleeDefenderExperience);

            MockUnitConfig.Setup(config => config.RangedAttackerExperience).Returns(data.RangedAttackerExperience);
            MockUnitConfig.Setup(config => config.RangedDefenderExperience).Returns(data.RangedDefenderExperience);
        }

        #endregion

        #region tests

        [Test]
        [TestCaseSource("GetExperienceForNextLevelOnUnitTestCases")]
        public int GetExperienceForNextLevelOnUnitTests(GetExperienceForNextLevelOnUnitTestData testData) {
            SetupConfig(testData.UnitConfig);

            var unit = BuildUnit(testData.Unit);

            var experienceLogic = Container.Resolve<UnitExperienceLogic>();

            return experienceLogic.GetExperienceForNextLevelOnUnit(unit);
        }

        [Test]
        [TestCaseSource("AttackerExperienceGainFromMeleeCombatTestCases")]
        public int AttackerExperienceGainFromMeleeCombatTests(CombatExperienceGainTestData testData) {
            SetupConfig(testData.UnitConfig);

            var attacker = BuildUnit(testData.Attacker);
            var defender = BuildUnit(testData.Defender);

            var results = new UnitCombatResults(attacker, defender, 0, testData.DamageToDefender);

            Container.Resolve<UnitExperienceLogic>();

            var unitSignals = Container.Resolve<UnitSignals>();

            unitSignals.MeleeCombatWithUnitSignal.OnNext(results);

            return attacker.Experience;
        }

        [Test]
        [TestCaseSource("DefenderExperienceGainFromMeleeCombatTestCases")]
        public int DefenderExperienceGainFromMeleeCombatTests(CombatExperienceGainTestData testData) {
            SetupConfig(testData.UnitConfig);

            var attacker = BuildUnit(testData.Attacker);
            var defender = BuildUnit(testData.Defender);

            var results = new UnitCombatResults(attacker, defender, 0, testData.DamageToDefender);

            Container.Resolve<UnitExperienceLogic>();

            var unitSignals = Container.Resolve<UnitSignals>();

            unitSignals.MeleeCombatWithUnitSignal.OnNext(results);

            return defender.Experience;
        }

        [Test]
        [TestCaseSource("AttackerExperienceGainFromRangedCombatTestCases")]
        public int AttackerExperienceGainFromRangedCombatTests(CombatExperienceGainTestData testData) {
            SetupConfig(testData.UnitConfig);

            var attacker = BuildUnit(testData.Attacker);
            var defender = BuildUnit(testData.Defender);

            var results = new UnitCombatResults(attacker, defender, 0, testData.DamageToDefender);

            Container.Resolve<UnitExperienceLogic>();

            var unitSignals = Container.Resolve<UnitSignals>();

            unitSignals.RangedCombatWithUnitSignal.OnNext(results);

            return attacker.Experience;
        }

        [Test]
        [TestCaseSource("DefenderExperienceGainFromRangedCombatTestCases")]
        public int DefenderExperienceGainFromRangedCombatTests(CombatExperienceGainTestData testData) {
            SetupConfig(testData.UnitConfig);

            var attacker = BuildUnit(testData.Attacker);
            var defender = BuildUnit(testData.Defender);

            var results = new UnitCombatResults(attacker, defender, 0, testData.DamageToDefender);

            Container.Resolve<UnitExperienceLogic>();

            var unitSignals = Container.Resolve<UnitSignals>();

            unitSignals.RangedCombatWithUnitSignal.OnNext(results);

            return defender.Experience;
        }

        #endregion

        #region utilities

        private IUnit BuildUnit(UnitTestData unitData) {
            var mockUnit = new Mock<IUnit>();

            mockUnit.SetupAllProperties();

            mockUnit.Setup(unit => unit.Type).Returns(unitData.Type);

            var newUnit = mockUnit.Object;

            newUnit.Level     = unitData.Level;
            newUnit.Hitpoints = unitData.Hitpoints;

            return newUnit;
        }

        #endregion

        #endregion

    }

}
