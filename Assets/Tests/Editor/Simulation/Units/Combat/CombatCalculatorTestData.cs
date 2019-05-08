using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using UniRx;

using Assets.Simulation.Units.Combat;

namespace Assets.Tests.Simulation.Units.Combat {

    public class CombatCalculatorTestData {

        #region internal types

        public class CalculateCombatTestData {

            public UnitTestData Attacker;
            public UnitTestData Defender;

            public CombatInfo CombatInfo;

            public int CombatBaseDamage = 30;

        }

        public class UnitTestData {

            public int CombatStrength;
            public int RangedAttackStrength;
            public int CurrentHitpoints;

        }

        #endregion

        #region instance fields and properties

        public static IEnumerable Cases {
            get {
                yield return new TestCaseData(new CalculateCombatTestData() {
                    Attacker = new UnitTestData() {
                        CombatStrength = 100, CurrentHitpoints = 100
                    },
                    Defender = new UnitTestData() {
                        CombatStrength = 100, CurrentHitpoints = 100,
                    },
                    CombatInfo = new CombatInfo() {
                        AttackerCombatModifier = 0f, DefenderCombatModifier = 0f,
                        CombatType = CombatType.Melee
                    },
                    CombatBaseDamage = 30
                }).SetName("(100 * 0f) vs (100 * 0f) with base 30 => (30 and 30) ").Returns(new UniRx.Tuple<int, int>(30, 30));

                yield return new TestCaseData(new CalculateCombatTestData() {
                    Attacker = new UnitTestData() {
                        CombatStrength = 150, CurrentHitpoints = 100
                    },
                    Defender = new UnitTestData() {
                        CombatStrength = 100, CurrentHitpoints = 100,
                    },
                    CombatInfo = new CombatInfo() {
                        AttackerCombatModifier = 0f, DefenderCombatModifier = 0f,
                        CombatType = CombatType.Melee
                    },
                    CombatBaseDamage = 30
                }).SetName("(150 * 0f) vs (100 * 0f) with base 30 => (20 and 45) ").Returns(new UniRx.Tuple<int, int>(20, 45));

                yield return new TestCaseData(new CalculateCombatTestData() {
                    Attacker = new UnitTestData() {
                        CombatStrength = 175, CurrentHitpoints = 100
                    },
                    Defender = new UnitTestData() {
                        CombatStrength = 100, CurrentHitpoints = 100,
                    },
                    CombatInfo = new CombatInfo() {
                        AttackerCombatModifier = 0f, DefenderCombatModifier = 0f,
                        CombatType = CombatType.Melee
                    },
                    CombatBaseDamage = 30
                }).SetName("(175 * 0f) vs (100 * 0f) with base 30 => (20 and 45) ").Returns(new UniRx.Tuple<int, int>(17, 52));

                yield return new TestCaseData(new CalculateCombatTestData() {
                    Attacker = new UnitTestData() {
                        CombatStrength = 200, CurrentHitpoints = 100
                    },
                    Defender = new UnitTestData() {
                        CombatStrength = 100, CurrentHitpoints = 100,
                    },
                    CombatInfo = new CombatInfo() {
                        AttackerCombatModifier = 0f, DefenderCombatModifier = 0f,
                        CombatType = CombatType.Melee
                    },
                    CombatBaseDamage = 30
                }).SetName("(200 * 0f) vs (100 * 0f) with base 30 => (15 and 60) ").Returns(new UniRx.Tuple<int, int>(15, 60));

                yield return new TestCaseData(new CalculateCombatTestData() {
                    Attacker = new UnitTestData() {
                        CombatStrength = 250, CurrentHitpoints = 100
                    },
                    Defender = new UnitTestData() {
                        CombatStrength = 100, CurrentHitpoints = 100,
                    },
                    CombatInfo = new CombatInfo() {
                        AttackerCombatModifier = 0f, DefenderCombatModifier = 0f,
                        CombatType = CombatType.Melee
                    },
                    CombatBaseDamage = 30
                }).SetName("(250 * 0f) vs (100 * 0f) with base 30 => (12 and 75) ").Returns(new UniRx.Tuple<int, int>(12, 75));

                yield return new TestCaseData(new CalculateCombatTestData() {
                    Attacker = new UnitTestData() {
                        CombatStrength = 300, CurrentHitpoints = 100
                    },
                    Defender = new UnitTestData() {
                        CombatStrength = 100, CurrentHitpoints = 100,
                    },
                    CombatInfo = new CombatInfo() {
                        AttackerCombatModifier = 0f, DefenderCombatModifier = 0f,
                        CombatType = CombatType.Melee
                    },
                    CombatBaseDamage = 30
                }).SetName("(300 * 0f) vs (100 * 0f) with base 30 => (10 and 90) ").Returns(new UniRx.Tuple<int, int>(10, 90));

                yield return new TestCaseData(new CalculateCombatTestData() {
                    Attacker = new UnitTestData() {
                        CombatStrength = 400, CurrentHitpoints = 100
                    },
                    Defender = new UnitTestData() {
                        CombatStrength = 100, CurrentHitpoints = 100,
                    },
                    CombatInfo = new CombatInfo() {
                        AttackerCombatModifier = 0f, DefenderCombatModifier = 0f,
                        CombatType = CombatType.Melee
                    },
                    CombatBaseDamage = 30
                }).SetName("(400 * 0f) vs (100 * 0f) with base 30 => (8 and 100) ").Returns(new UniRx.Tuple<int, int>(8, 100));

                yield return new TestCaseData(new CalculateCombatTestData() {
                    Attacker = new UnitTestData() {
                        CombatStrength = 100, CurrentHitpoints = 100
                    },
                    Defender = new UnitTestData() {
                        CombatStrength = 100, CurrentHitpoints = 100,
                    },
                    CombatInfo = new CombatInfo() {
                        AttackerCombatModifier = 0.5f, DefenderCombatModifier = 0f,
                        CombatType = CombatType.Melee
                    },
                    CombatBaseDamage = 30
                }).SetName("(100 * 0.5f) vs (100 * 0f) with base 30 => (8 and 100) ").Returns(new UniRx.Tuple<int, int>(20, 45));

                yield return new TestCaseData(new CalculateCombatTestData() {
                    Attacker = new UnitTestData() {
                        CombatStrength = 100, CurrentHitpoints = 100
                    },
                    Defender = new UnitTestData() {
                        CombatStrength = 100, CurrentHitpoints = 100,
                    },
                    CombatInfo = new CombatInfo() {
                        AttackerCombatModifier = 0f, DefenderCombatModifier = 0.5f,
                        CombatType = CombatType.Melee
                    },
                    CombatBaseDamage = 30
                }).SetName("(100 * 0f) vs (100 * 0.5f) with base 30 => (8 and 100) ").Returns(new UniRx.Tuple<int, int>(45, 20));

                yield return new TestCaseData(new CalculateCombatTestData() {
                    Attacker = new UnitTestData() {
                        CombatStrength = 100, RangedAttackStrength = 100, CurrentHitpoints = 100
                    },
                    Defender = new UnitTestData() {
                        CombatStrength = 100, CurrentHitpoints = 100,
                    },
                    CombatInfo = new CombatInfo() {
                        AttackerCombatModifier = 0f, DefenderCombatModifier = 0f,
                        CombatType = CombatType.Ranged
                    },
                    CombatBaseDamage = 30
                }).SetName("Ranged combat deals no damage to attacker").Returns(new UniRx.Tuple<int, int>(0, 30));

                yield return new TestCaseData(new CalculateCombatTestData() {
                    Attacker = new UnitTestData() {
                        CombatStrength = 400, RangedAttackStrength = 100, CurrentHitpoints = 100
                    },
                    Defender = new UnitTestData() {
                        CombatStrength = 100, CurrentHitpoints = 100,
                    },
                    CombatInfo = new CombatInfo() {
                        AttackerCombatModifier = 0f, DefenderCombatModifier = 0f,
                        CombatType = CombatType.Ranged
                    },
                    CombatBaseDamage = 30
                }).SetName("Ranged combat deals pulls ranged attack strength from attacker").Returns(new UniRx.Tuple<int, int>(0, 30));
            }
        }

        #endregion

    }

}
