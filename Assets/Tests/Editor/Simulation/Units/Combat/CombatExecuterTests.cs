using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using NUnit.Framework;
using Zenject;
using Moq;

using Assets.Simulation;
using Assets.Simulation.HexMap;
using Assets.Simulation.Units;
using Assets.Simulation.Units.Combat;
using Assets.Simulation.Civilizations;

namespace Assets.Tests.Simulation.Units.Combat {

    [TestFixture]
    public class CombatExecuterTests : ZenjectUnitTestFixture {

        #region internal types

        public class CombatTestData {

            public AttackerTestData Attacker;
            public DefenderTestData Defender;

            public bool UnitsHaveSameOwner;

        }

        public class AttackerTestData {

            public int CombatStrength;

            public int RangedAttackStrength;

            public int CurrentMovement;

            public int Range;

            public int LocationElevation;

            public int DistanceFromDefender;

            public bool CanMoveToDefender = true;

            public bool CanSeeDefender = true;

            public float CombatModifier;

            public bool HasAttacked = false;

            public bool CanMoveAfterAttacking;

        }

        public class DefenderTestData {

            public int CombatStrength;

            public float CombatModifier;

            public TerrainType LocationTerrain;

            public TerrainFeature LocationFeature;

            public TerrainShape LocationShape;

            public int LocationElevation;

        }

        public struct CombatTestOutput {

            public bool  CanPerformMeleeAttack;
            public int   MeleeAttackerHealthLeft;
            public float MeleeAttackerMovementLeft;
            public int   MeleeDefenderHealthLeft;

            public bool  CanPerformRangedAttack;
            public int   RangedAttackerHealthLeft;
            public float RangedAttackerMovementLeft;
            public int   RangedDefenderHealthLeft;

            public override string ToString() {
                return string.Format(
                    "\nMelee -- CanPerform: {0}, AttackerHealth: {1}, AttackerMovement: {2} DefenderHealth: {3}\n" + 
                      "Ranged -- CanPerform: {4}, AttackerHealth: {5}, AttackerMovement: {6}, DefenderHealth: {7}",
                    CanPerformMeleeAttack, MeleeAttackerHealthLeft, MeleeAttackerMovementLeft, MeleeDefenderHealthLeft,
                    CanPerformRangedAttack, RangedAttackerHealthLeft, RangedAttackerMovementLeft, RangedDefenderHealthLeft
                );
            }

        }

        #endregion

        #region static fields and properties

        private static IEnumerable TestCases {
            get {
                yield return new TestCaseData(new CombatTestData() {
                    Attacker = new AttackerTestData() {
                        CombatStrength = 100,
                        RangedAttackStrength = 100,
                        CurrentMovement = 2,
                        Range = 1,
                        LocationElevation = 0,
                        DistanceFromDefender = 1,
                        CanMoveToDefender = true,
                        CanSeeDefender = true,
                        CombatModifier = 0f,
                    },
                    Defender = new DefenderTestData() {
                        CombatStrength = 100,
                        LocationElevation = 0,
                        LocationTerrain = TerrainType.Grassland,
                        LocationFeature = TerrainFeature.None,
                        CombatModifier = 0f
                    }
                }).SetName("Basic terrain/no inhibitors/no modifiers/even strength")
                .Returns(new CombatTestOutput() {
                    CanPerformMeleeAttack = true,
                    MeleeAttackerHealthLeft = 70,
                    MeleeAttackerMovementLeft = 0,
                    MeleeDefenderHealthLeft = 70,
                    
                    CanPerformRangedAttack = true,
                    RangedAttackerHealthLeft = 100,
                    RangedAttackerMovementLeft = 0,
                    RangedDefenderHealthLeft = 70
                });

                yield return new TestCaseData(new CombatTestData() {
                    Attacker = new AttackerTestData() {
                        CombatStrength = 150,
                        RangedAttackStrength = 150,
                        CurrentMovement = 2,
                        Range = 1,
                        LocationElevation = 0,
                        DistanceFromDefender = 1,
                        CanMoveToDefender = true,
                        CanSeeDefender = true,
                        CombatModifier = 0f,
                    },
                    Defender = new DefenderTestData() { 
                        CombatStrength = 100,
                        LocationElevation = 0,
                        LocationTerrain = TerrainType.Grassland,
                        LocationFeature = TerrainFeature.None,
                        CombatModifier = 0f         
                    }
                }).SetName("Basic terrain/no inhibitors/no modifiers/1.5 attacker strength advantage")
                .Returns(new CombatTestOutput() {
                    CanPerformMeleeAttack = true,
                    MeleeAttackerHealthLeft = 80,
                    MeleeAttackerMovementLeft = 0,
                    MeleeDefenderHealthLeft = 55,
                    
                    CanPerformRangedAttack = true,
                    RangedAttackerHealthLeft = 100,
                    RangedAttackerMovementLeft = 0,
                    RangedDefenderHealthLeft = 55
                });

                yield return new TestCaseData(new CombatTestData() {
                    Attacker = new AttackerTestData() {
                        CombatStrength = 175,
                        RangedAttackStrength = 175,
                        CurrentMovement = 2,
                        Range = 1,
                        LocationElevation = 0,
                        DistanceFromDefender = 1,
                        CanMoveToDefender = true,
                        CanSeeDefender = true,
                        CombatModifier = 0f,
                    },
                    Defender = new DefenderTestData() {
                        CombatStrength = 100,
                        LocationElevation = 0,
                        LocationTerrain = TerrainType.Grassland,
                        LocationFeature = TerrainFeature.None,
                        CombatModifier = 0f
                    }
                }).SetName("Basic terrain/no inhibitors/no modifiers/1.75 attacker strength advantage")
                .Returns(new CombatTestOutput() {
                    CanPerformMeleeAttack = true,
                    MeleeAttackerHealthLeft = 83,
                    MeleeAttackerMovementLeft = 0,
                    MeleeDefenderHealthLeft = 48,
                    
                    CanPerformRangedAttack = true,
                    RangedAttackerHealthLeft = 100,
                    RangedAttackerMovementLeft = 0,
                    RangedDefenderHealthLeft = 48
                });

                yield return new TestCaseData(new CombatTestData() {
                    Attacker = new AttackerTestData() {
                        CombatStrength = 200,
                        RangedAttackStrength = 200,
                        CurrentMovement = 2,
                        Range = 1,
                        LocationElevation = 0,
                        DistanceFromDefender = 1,
                        CanMoveToDefender = true,
                        CanSeeDefender = true,
                        CombatModifier = 0f,
                    },
                    Defender = new DefenderTestData() {
                        CombatStrength = 100,
                        LocationElevation = 0,
                        LocationTerrain = TerrainType.Grassland,
                        LocationFeature = TerrainFeature.None,
                        CombatModifier = 0f
                    }
                }).SetName("Basic terrain/no inhibitors/no modifiers/2.0 attacker strength advantage")
                .Returns(new CombatTestOutput() {
                    CanPerformMeleeAttack = true,
                    MeleeAttackerHealthLeft = 85,
                    MeleeAttackerMovementLeft = 0,
                    MeleeDefenderHealthLeft = 40,
                    
                    CanPerformRangedAttack = true,
                    RangedAttackerHealthLeft = 100,
                    RangedAttackerMovementLeft = 0,
                    RangedDefenderHealthLeft = 40
                });

                yield return new TestCaseData(new CombatTestData() {
                    Attacker = new AttackerTestData() {
                        CombatStrength = 250,
                        RangedAttackStrength = 250,
                        CurrentMovement = 2,
                        Range = 1,
                        LocationElevation = 0,
                        DistanceFromDefender = 1,
                        CanMoveToDefender = true,
                        CanSeeDefender = true,
                        CombatModifier = 0f,
                    },
                    Defender = new DefenderTestData() {
                        CombatStrength = 100,
                        LocationElevation = 0,
                        LocationTerrain = TerrainType.Grassland,
                        LocationFeature = TerrainFeature.None,
                        CombatModifier = 0f
                    }
                }).SetName("Basic terrain/no inhibitors/no modifiers/2.5 attacker strength advantage")
                .Returns(new CombatTestOutput() {
                    CanPerformMeleeAttack = true,
                    MeleeAttackerHealthLeft = 88,
                    MeleeAttackerMovementLeft = 0,
                    MeleeDefenderHealthLeft = 25,
                    
                    CanPerformRangedAttack = true,
                    RangedAttackerHealthLeft = 100,
                    RangedAttackerMovementLeft = 0,
                    RangedDefenderHealthLeft = 25
                });

                yield return new TestCaseData(new CombatTestData() {
                    Attacker = new AttackerTestData() {
                        CombatStrength = 300,
                        RangedAttackStrength = 300,
                        CurrentMovement = 2,
                        Range = 1,
                        LocationElevation = 0,
                        DistanceFromDefender = 1,
                        CanMoveToDefender = true,
                        CanSeeDefender = true,
                        CombatModifier = 0f,
                    },
                    Defender = new DefenderTestData() {
                        CombatStrength = 100,
                        LocationElevation = 0,
                        LocationTerrain = TerrainType.Grassland,
                        LocationFeature = TerrainFeature.None,
                        CombatModifier = 0f
                    }
                }).SetName("Basic terrain/no inhibitors/no modifiers/3 attacker strength advantage")
                .Returns(new CombatTestOutput() {
                    CanPerformMeleeAttack = true,
                    MeleeAttackerHealthLeft = 90,
                    MeleeAttackerMovementLeft = 0,
                    MeleeDefenderHealthLeft = 10,
                    
                    CanPerformRangedAttack = true,
                    RangedAttackerHealthLeft = 100,
                    RangedAttackerMovementLeft = 0,
                    RangedDefenderHealthLeft = 10
                });

                yield return new TestCaseData(new CombatTestData() {
                    Attacker = new AttackerTestData() {
                        CombatStrength = 400,
                        RangedAttackStrength = 400,
                        CurrentMovement = 2,
                        Range = 1,
                        LocationElevation = 0,
                        DistanceFromDefender = 1,
                        CanMoveToDefender = true,
                        CanSeeDefender = true,
                        CombatModifier = 0f,
                    },
                    Defender = new DefenderTestData() {
                        CombatStrength = 100,
                        LocationElevation = 0,
                        LocationTerrain = TerrainType.Grassland,
                        LocationFeature = TerrainFeature.None,
                        CombatModifier = 0f
                    }
                }).SetName("Basic terrain/no inhibitors/no modifiers/4 attacker strength advantage")
                .Returns(new CombatTestOutput() {
                    CanPerformMeleeAttack = true,
                    MeleeAttackerHealthLeft = 92,
                    MeleeAttackerMovementLeft = 0,
                    MeleeDefenderHealthLeft = 0,
                    
                    CanPerformRangedAttack = true,
                    RangedAttackerHealthLeft = 100,
                    RangedAttackerMovementLeft = 0,
                    RangedDefenderHealthLeft = 0
                });

                yield return new TestCaseData(new CombatTestData() {
                    Attacker = new AttackerTestData() {
                        CombatStrength = 100,
                        RangedAttackStrength = 100,
                        CurrentMovement = 2,
                        Range = 1,
                        LocationElevation = 0,
                        DistanceFromDefender = 1,
                        CanMoveToDefender = true,
                        CanSeeDefender = true,
                        CombatModifier = 0.5f,
                    },
                    Defender = new DefenderTestData() {
                        CombatStrength = 100,
                        LocationElevation = 0,
                        LocationTerrain = TerrainType.Grassland,
                        LocationFeature = TerrainFeature.None,
                        CombatModifier = 0f
                    }
                }).SetName("Basic terrain/no inhibitors/no modifiers/even strength/50% attacker modifier")
                .Returns(new CombatTestOutput() {
                    CanPerformMeleeAttack = true,
                    MeleeAttackerHealthLeft = 80,
                    MeleeAttackerMovementLeft = 0,
                    MeleeDefenderHealthLeft = 55,
                    
                    CanPerformRangedAttack = true,
                    RangedAttackerHealthLeft = 100,
                    RangedAttackerMovementLeft = 0,
                    RangedDefenderHealthLeft = 55
                });

                yield return new TestCaseData(new CombatTestData() {
                    Attacker = new AttackerTestData() {
                        CombatStrength = 100,
                        RangedAttackStrength = 100,
                        CurrentMovement = 2,
                        Range = 1,
                        LocationElevation = 0,
                        DistanceFromDefender = 1,
                        CanMoveToDefender = true,
                        CanSeeDefender = true,
                        CombatModifier = 0f,
                    },
                    Defender = new DefenderTestData() {
                        CombatStrength = 100,
                        LocationElevation = 0,
                        LocationTerrain = TerrainType.Grassland,
                        LocationFeature = TerrainFeature.None,
                        CombatModifier = 0.5f
                    }
                }).SetName("Basic terrain/no inhibitors/no modifiers/even strength/50% defender modifier")
                .Returns(new CombatTestOutput() {
                    CanPerformMeleeAttack = true,
                    MeleeAttackerHealthLeft = 55,
                    MeleeAttackerMovementLeft = 0,
                    MeleeDefenderHealthLeft = 80,
                    
                    CanPerformRangedAttack = true,
                    RangedAttackerHealthLeft = 100,
                    RangedAttackerMovementLeft = 0,
                    RangedDefenderHealthLeft = 80
                });

                yield return new TestCaseData(new CombatTestData() {
                    Attacker = new AttackerTestData() {
                        CombatStrength = 100,
                        RangedAttackStrength = 100,
                        CurrentMovement = 0,
                        Range = 1,
                        LocationElevation = 0,
                        DistanceFromDefender = 1,
                        CanMoveToDefender = true,
                        CanSeeDefender = true,
                        CombatModifier = 0f,
                    },
                    Defender = new DefenderTestData() {
                        CombatStrength = 100,
                        LocationElevation = 0,
                        LocationTerrain = TerrainType.Grassland,
                        LocationFeature = TerrainFeature.None,
                        CombatModifier = 0f
                    }
                }).SetName("Basic terrain/no attacker movement/no modifiers/even strength")
                .Returns(new CombatTestOutput() {
                    CanPerformMeleeAttack = false,
                    MeleeAttackerHealthLeft = 100,
                    MeleeAttackerMovementLeft = 0,
                    MeleeDefenderHealthLeft = 100,
                    
                    CanPerformRangedAttack = false,
                    RangedAttackerHealthLeft = 100,
                    RangedAttackerMovementLeft = 0,
                    RangedDefenderHealthLeft = 100
                });

                yield return new TestCaseData(new CombatTestData() {
                    Attacker = new AttackerTestData() {
                        CombatStrength = 100,
                        RangedAttackStrength = 100,
                        CurrentMovement = 2,
                        Range = 1,
                        LocationElevation = 0,
                        DistanceFromDefender = 1,
                        CanMoveToDefender = true,
                        CanSeeDefender = true,
                        CombatModifier = 0f,
                        HasAttacked = true,
                    },
                    Defender = new DefenderTestData() {
                        CombatStrength = 100,
                        LocationElevation = 0,
                        LocationTerrain = TerrainType.Grassland,
                        LocationFeature = TerrainFeature.None,
                        CombatModifier = 0f
                    }
                }).SetName("Basic terrain/attacker has already attacked/no modifiers/even strength")
                .Returns(new CombatTestOutput() {
                    CanPerformMeleeAttack = false,
                    MeleeAttackerHealthLeft = 100,
                    MeleeAttackerMovementLeft = 2,
                    MeleeDefenderHealthLeft = 100,
                    
                    CanPerformRangedAttack = false,
                    RangedAttackerHealthLeft = 100,
                    RangedAttackerMovementLeft = 2,
                    RangedDefenderHealthLeft = 100
                });

                yield return new TestCaseData(new CombatTestData() {
                    Attacker = new AttackerTestData() {
                        CombatStrength = 100,
                        RangedAttackStrength = 100,
                        CurrentMovement = 2,
                        Range = 0,
                        LocationElevation = 0,
                        DistanceFromDefender = 1,
                        CanMoveToDefender = true,
                        CanSeeDefender = true,
                        CombatModifier = 0f,
                    },
                    Defender = new DefenderTestData() {
                        CombatStrength = 100,
                        LocationElevation = 0,
                        LocationTerrain = TerrainType.Grassland,
                        LocationFeature = TerrainFeature.None,
                        CombatModifier = 0f
                    }
                }).SetName("Basic terrain/no attacker range/no modifiers/even strength")
                .Returns(new CombatTestOutput() {
                    CanPerformMeleeAttack = true,
                    MeleeAttackerHealthLeft = 70,
                    MeleeAttackerMovementLeft = 0,
                    MeleeDefenderHealthLeft = 70,
                    
                    CanPerformRangedAttack = false,
                    RangedAttackerHealthLeft = 100,
                    RangedAttackerMovementLeft = 2,
                    RangedDefenderHealthLeft = 100
                });

                yield return new TestCaseData(new CombatTestData() {
                    Attacker = new AttackerTestData() {
                        CombatStrength = 100,
                        RangedAttackStrength = 100,
                        CurrentMovement = 2,
                        Range = 1,
                        LocationElevation = 0,
                        DistanceFromDefender = 1,
                        CanMoveToDefender = true,
                        CanSeeDefender = true,
                        CombatModifier = 0f,
                    },
                    Defender = new DefenderTestData() {
                        CombatStrength = 100,
                        LocationElevation = 1,
                        LocationTerrain = TerrainType.Grassland,
                        LocationFeature = TerrainFeature.None,
                        CombatModifier = 0f
                    }
                }).SetName("Basic terrain/attacking up slope/no modifiers/even strength")
                .Returns(new CombatTestOutput() {
                    CanPerformMeleeAttack = true,
                    MeleeAttackerHealthLeft = 70,
                    MeleeAttackerMovementLeft = 0,
                    MeleeDefenderHealthLeft = 70,
                    
                    CanPerformRangedAttack = true,
                    RangedAttackerHealthLeft = 100,
                    RangedAttackerMovementLeft = 0,
                    RangedDefenderHealthLeft = 70
                });

                yield return new TestCaseData(new CombatTestData() {
                    Attacker = new AttackerTestData() {
                        CombatStrength = 100,
                        RangedAttackStrength = 100,
                        CurrentMovement = 2,
                        Range = 1,
                        LocationElevation = 0,
                        DistanceFromDefender = 1,
                        CanMoveToDefender = true,
                        CanSeeDefender = true,
                        CombatModifier = 0f,
                    },
                    Defender = new DefenderTestData() {
                        CombatStrength = 100,
                        LocationElevation = 2,
                        LocationTerrain = TerrainType.Grassland,
                        LocationFeature = TerrainFeature.None,
                        CombatModifier = 0f
                    }
                }).SetName("Basic terrain/attacking up cliff/no modifiers/even strength")
                .Returns(new CombatTestOutput() {
                    CanPerformMeleeAttack = false,
                    MeleeAttackerHealthLeft = 100,
                    MeleeAttackerMovementLeft = 2,
                    MeleeDefenderHealthLeft = 100,
                    
                    CanPerformRangedAttack = true,
                    RangedAttackerHealthLeft = 100,
                    RangedAttackerMovementLeft = 0,
                    RangedDefenderHealthLeft = 70
                });

                yield return new TestCaseData(new CombatTestData() {
                    Attacker = new AttackerTestData() {
                        CombatStrength = 100,
                        RangedAttackStrength = 100,
                        CurrentMovement = 2,
                        Range = 1,
                        LocationElevation = 2,
                        DistanceFromDefender = 1,
                        CanMoveToDefender = true,
                        CanSeeDefender = true,
                        CombatModifier = 0f,
                    },
                    Defender = new DefenderTestData() {
                        CombatStrength = 100,
                        LocationElevation = 0,
                        LocationTerrain = TerrainType.Grassland,
                        LocationFeature = TerrainFeature.None,
                        CombatModifier = 0f
                    }
                }).SetName("Basic terrain/attacking down cliff/no modifiers/even strength")
                .Returns(new CombatTestOutput() {
                    CanPerformMeleeAttack = false,
                    MeleeAttackerHealthLeft = 100,
                    MeleeAttackerMovementLeft = 2,
                    MeleeDefenderHealthLeft = 100,
                    
                    CanPerformRangedAttack = true,
                    RangedAttackerHealthLeft = 100,
                    RangedAttackerMovementLeft = 0,
                    RangedDefenderHealthLeft = 70
                });

                yield return new TestCaseData(new CombatTestData() {
                    Attacker = new AttackerTestData() {
                        CombatStrength = 100,
                        RangedAttackStrength = 100,
                        CurrentMovement = 2,
                        Range = 1,
                        LocationElevation = 0,
                        DistanceFromDefender = 1,
                        CanMoveToDefender = false,
                        CanSeeDefender = true,
                        CombatModifier = 0f,
                    },
                    Defender = new DefenderTestData() {
                        CombatStrength = 100,
                        LocationElevation = 0,
                        LocationTerrain = TerrainType.Grassland,
                        LocationFeature = TerrainFeature.None,
                        CombatModifier = 0f
                    }
                }).SetName("Basic terrain/attacker cannot move to defender/no modifiers/even strength")
                .Returns(new CombatTestOutput() {
                    CanPerformMeleeAttack = false,
                    MeleeAttackerHealthLeft = 100,
                    MeleeAttackerMovementLeft = 2,
                    MeleeDefenderHealthLeft = 100,
                    
                    CanPerformRangedAttack = true,
                    RangedAttackerHealthLeft = 100,
                    RangedAttackerMovementLeft = 0,
                    RangedDefenderHealthLeft = 70
                });

                yield return new TestCaseData(new CombatTestData() {
                    Attacker = new AttackerTestData() {
                        CombatStrength = 100,
                        RangedAttackStrength = 100,
                        CurrentMovement = 2,
                        Range = 1,
                        LocationElevation = 0,
                        DistanceFromDefender = 1,
                        CanMoveToDefender = true,
                        CanSeeDefender = false,
                        CombatModifier = 0f,
                    },
                    Defender = new DefenderTestData() {
                        CombatStrength = 100,
                        LocationElevation = 0,
                        LocationTerrain = TerrainType.Grassland,
                        LocationFeature = TerrainFeature.None,
                        CombatModifier = 0f
                    }
                }).SetName("Basic terrain/attacker cannot see defender/no modifiers/even strength")
                .Returns(new CombatTestOutput() {
                    CanPerformMeleeAttack = false,
                    MeleeAttackerHealthLeft = 100,
                    MeleeAttackerMovementLeft = 2,
                    MeleeDefenderHealthLeft = 100,
                    
                    CanPerformRangedAttack = false,
                    RangedAttackerHealthLeft = 100,
                    RangedAttackerMovementLeft = 2,
                    RangedDefenderHealthLeft = 100
                });

                yield return new TestCaseData(new CombatTestData() {
                    Attacker = new AttackerTestData() {
                        CombatStrength = 100,
                        RangedAttackStrength = 100,
                        CurrentMovement = 2,
                        Range = 1,
                        LocationElevation = 0,
                        DistanceFromDefender = 1,
                        CanMoveToDefender = true,
                        CanSeeDefender = true,
                        CombatModifier = 0f,
                    },
                    Defender = new DefenderTestData() {
                        CombatStrength = 0,
                        LocationElevation = 0,
                        LocationTerrain = TerrainType.Grassland,
                        LocationFeature = TerrainFeature.None,
                        CombatModifier = 0f
                    }
                }).SetName("Basic terrain/no inhibitors/no modifiers/zero-strength defender")
                .Returns(new CombatTestOutput() {
                    CanPerformMeleeAttack = true,
                    MeleeAttackerHealthLeft = 100,
                    MeleeAttackerMovementLeft = 0,
                    MeleeDefenderHealthLeft = 0,
                    
                    CanPerformRangedAttack = true,
                    RangedAttackerHealthLeft = 100,
                    RangedAttackerMovementLeft = 0,
                    RangedDefenderHealthLeft = 0
                });

                yield return new TestCaseData(new CombatTestData() {
                    Attacker = new AttackerTestData() {
                        CombatStrength = 0,
                        RangedAttackStrength = 0,
                        CurrentMovement = 2,
                        Range = 1,
                        LocationElevation = 0,
                        DistanceFromDefender = 1,
                        CanMoveToDefender = true,
                        CanSeeDefender = true,
                        CombatModifier = 0f,
                    },
                    Defender = new DefenderTestData() {
                        CombatStrength = 100,
                        LocationElevation = 0,
                        LocationTerrain = TerrainType.Grassland,
                        LocationFeature = TerrainFeature.None,
                        CombatModifier = 0f
                    }
                }).SetName("Basic terrain/no inhibitors/no modifiers/zero-strength attacker")
                .Returns(new CombatTestOutput() {
                    CanPerformMeleeAttack = false,
                    MeleeAttackerHealthLeft = 100,
                    MeleeAttackerMovementLeft = 2,
                    MeleeDefenderHealthLeft = 100,
                    
                    CanPerformRangedAttack = false,
                    RangedAttackerHealthLeft = 100,
                    RangedAttackerMovementLeft = 2,
                    RangedDefenderHealthLeft = 100,
                });

                yield return new TestCaseData(new CombatTestData() {
                    Attacker = new AttackerTestData() {
                        CombatStrength = 100,
                        RangedAttackStrength = 100,
                        CurrentMovement = 2,
                        Range = 1,
                        LocationElevation = 0,
                        DistanceFromDefender = 1,
                        CanMoveToDefender = true,
                        CanSeeDefender = true,
                        CombatModifier = 0f,
                    },
                    Defender = new DefenderTestData() {
                        CombatStrength = 100,
                        LocationElevation = 0,
                        LocationTerrain = TerrainType.Grassland,
                        LocationFeature = TerrainFeature.None,
                        CombatModifier = 0f,
                    },
                    UnitsHaveSameOwner = true

                }).SetName("Basic terrain/no inhibitors/no modifiers/even strength/same owner")
                .Returns(new CombatTestOutput() {
                    CanPerformMeleeAttack = false,
                    MeleeAttackerHealthLeft = 100,
                    MeleeAttackerMovementLeft = 2,
                    MeleeDefenderHealthLeft = 100,
                    
                    CanPerformRangedAttack = false,
                    RangedAttackerHealthLeft = 100,
                    RangedAttackerMovementLeft = 2,
                    RangedDefenderHealthLeft = 100,
                });

                yield return new TestCaseData(new CombatTestData() {
                    Attacker = new AttackerTestData() {
                        CombatStrength = 100,
                        RangedAttackStrength = 100,
                        CurrentMovement = 2,
                        Range = 1,
                        LocationElevation = 0,
                        DistanceFromDefender = 1,
                        CanMoveToDefender = true,
                        CanSeeDefender = true,
                        CombatModifier = 0f,
                        CanMoveAfterAttacking = true,
                    },
                    Defender = new DefenderTestData() {
                        CombatStrength = 100,
                        LocationElevation = 0,
                        LocationTerrain = TerrainType.Grassland,
                        LocationFeature = TerrainFeature.None,
                        CombatModifier = 0f
                    }
                }).SetName("Basic terrain/no inhibitors/no modifiers/attacker can move after attacking")
                .Returns(new CombatTestOutput() {
                    CanPerformMeleeAttack = true,
                    MeleeAttackerHealthLeft = 70,
                    MeleeAttackerMovementLeft = 1,
                    MeleeDefenderHealthLeft = 70,
                    
                    CanPerformRangedAttack = true,
                    RangedAttackerHealthLeft = 100,
                    RangedAttackerMovementLeft = 1,
                    RangedDefenderHealthLeft = 70
                });
            }
        }

        #endregion

        #region instance fields and properties

        private Mock<IUnitPositionCanon> MockUnitPositionCanon;

        private Mock<IHexGrid> MockGrid;

        private Mock<ILineOfSightLogic> MockLineOfSightLogic;

        private Mock<ICombatModifierLogic> MockCombatModifierLogic;

        private Mock<IPossessionRelationship<ICivilization, IUnit>> MockUnitPossessionCanon;

        private IUnitConfig UnitConfig;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockUnitPositionCanon   = new Mock<IUnitPositionCanon>();
            MockGrid                = new Mock<IHexGrid>();
            MockLineOfSightLogic    = new Mock<ILineOfSightLogic>();
            MockCombatModifierLogic = new Mock<ICombatModifierLogic>();
            MockUnitPossessionCanon = new Mock<IPossessionRelationship<ICivilization, IUnit>>();

            Container.Bind<IUnitPositionCanon>                           ().FromInstance(MockUnitPositionCanon  .Object);
            Container.Bind<IHexGrid>                                     ().FromInstance(MockGrid               .Object);
            Container.Bind<ILineOfSightLogic>                            ().FromInstance(MockLineOfSightLogic   .Object);
            Container.Bind<ICombatModifierLogic>                         ().FromInstance(MockCombatModifierLogic.Object);
            Container.Bind<IPossessionRelationship<ICivilization, IUnit>>().FromInstance(MockUnitPossessionCanon.Object);

            Container.Bind<UnitSignals>().AsSingle();

            Container.Bind<IUnitConfig>().To<UnitConfig>().FromNewScriptableObjectResource("Tests/Combat Executer Unit Config").AsSingle();

            UnitConfig = Container.Resolve<IUnitConfig>();

            Container.Bind<CombatExecuter>().AsSingle();
        }

        #endregion

        #region tests

        [Test(Description = "")]
        [TestCaseSource("TestCases")]
        public CombatTestOutput PerformCombatTests(CombatTestData input) {
            IHexCell attackerLocation = BuildCell(input.Attacker.LocationElevation);
            IHexCell defenderLocation = BuildCell(
                input.Defender.LocationTerrain, input.Defender.LocationFeature,
                input.Defender.LocationShape,   input.Defender.LocationElevation
            );

            IUnit meleeAttacker = BuildUnit(attackerLocation, input.Attacker);
            IUnit meleeDefender = BuildUnit(defenderLocation, input.Defender);

            IUnit rangedAttacker = BuildUnit(attackerLocation, input.Attacker);
            IUnit rangedDefender = BuildUnit(defenderLocation, input.Defender);

            SetCombatConditions(
                input.Attacker.DistanceFromDefender, input.Attacker.CanMoveToDefender,
                input.Attacker.CanSeeDefender
            );

            if(input.UnitsHaveSameOwner) {
                var owner = BuildCivilization();

                MockUnitPossessionCanon.Setup(canon => canon.GetOwnerOfPossession(meleeAttacker )).Returns(owner);
                MockUnitPossessionCanon.Setup(canon => canon.GetOwnerOfPossession(rangedAttacker)).Returns(owner);
                MockUnitPossessionCanon.Setup(canon => canon.GetOwnerOfPossession(meleeDefender )).Returns(owner);
                MockUnitPossessionCanon.Setup(canon => canon.GetOwnerOfPossession(rangedDefender)).Returns(owner);
            }else {
                var attackerOwner = BuildCivilization();
                var defenderOwner = BuildCivilization();

                MockUnitPossessionCanon.Setup(canon => canon.GetOwnerOfPossession(meleeAttacker )).Returns(attackerOwner);
                MockUnitPossessionCanon.Setup(canon => canon.GetOwnerOfPossession(rangedAttacker)).Returns(attackerOwner);
                MockUnitPossessionCanon.Setup(canon => canon.GetOwnerOfPossession(meleeDefender )).Returns(defenderOwner);
                MockUnitPossessionCanon.Setup(canon => canon.GetOwnerOfPossession(rangedDefender)).Returns(defenderOwner);
            }

            var combatExecuter = Container.Resolve<CombatExecuter>();

            var results = new CombatTestOutput();

            results.CanPerformMeleeAttack = combatExecuter.CanPerformMeleeAttack(meleeAttacker, meleeDefender);

            if(results.CanPerformMeleeAttack) {
                combatExecuter.PerformMeleeAttack(meleeAttacker, meleeDefender);
            }

            results.MeleeAttackerHealthLeft   = meleeAttacker.Hitpoints;
            results.MeleeAttackerMovementLeft = meleeAttacker.CurrentMovement;
            results.MeleeDefenderHealthLeft   = meleeDefender.Hitpoints;
            
            results.CanPerformRangedAttack = combatExecuter.CanPerformRangedAttack(rangedAttacker, rangedDefender);

            if(results.CanPerformRangedAttack) {
                combatExecuter.PerformRangedAttack(rangedAttacker, rangedDefender);
            }

            results.RangedAttackerHealthLeft   = rangedAttacker.Hitpoints;
            results.RangedAttackerMovementLeft = rangedAttacker.CurrentMovement;
            results.RangedDefenderHealthLeft   = rangedDefender.Hitpoints;

            return results;
        }


        #endregion

        #region utilities

        private void SetCombatConditions(int distanceBetween, bool attackerCanMoveTo, bool defenderCanBeSeen) {
            MockGrid.Setup(grid => grid.GetDistance(It.IsAny<IHexCell>(), It.IsAny<IHexCell>()))
                .Returns(distanceBetween);

            MockUnitPositionCanon.Setup(canon => canon.CanPlaceUnitAtLocation(It.IsAny<IUnit>(), It.IsAny<IHexCell>(), true))
                .Returns(attackerCanMoveTo);

            MockLineOfSightLogic.Setup(logic => logic.CanUnitSeeCell(It.IsAny<IUnit>(), It.IsAny<IHexCell>()))
                .Returns(defenderCanBeSeen);
        }

        private IUnit BuildUnit(IHexCell location, AttackerTestData attackerData) {
            return BuildUnit(
                location:              location,
                currentMovement:       attackerData.CurrentMovement,
                combatStrength:        attackerData.CombatStrength,
                rangedAttackStrength:  attackerData.RangedAttackStrength,
                attackModifier:        attackerData.CombatModifier,
                defenseModifier:       0f,
                attackRange:           attackerData.Range,
                hasAttacked:           attackerData.HasAttacked,
                canMoveAfterAttacking: attackerData.CanMoveAfterAttacking
            );
        }

        private IUnit BuildUnit(IHexCell location, DefenderTestData defenderData) {
            return BuildUnit(
                location:              location,
                currentMovement:       0,
                combatStrength:        defenderData.CombatStrength,
                rangedAttackStrength:  0,
                attackModifier:        0,
                defenseModifier:       defenderData.CombatModifier,
                attackRange:           0,
                hasAttacked:           false,
                canMoveAfterAttacking: false
            );
        }

        private IUnit BuildUnit(
            IHexCell location, int currentMovement,
            int combatStrength, int rangedAttackStrength,
            float attackModifier, float defenseModifier,
            int attackRange, bool hasAttacked, bool canMoveAfterAttacking
        ){
            var mockUnit = new Mock<IUnit>();

            var mockTemplate = new Mock<IUnitTemplate>();

            mockTemplate.Setup(template => template.CanMoveAfterAttacking).Returns(canMoveAfterAttacking);

            mockUnit.SetupAllProperties();

            mockUnit.Setup(unit => unit.Template).Returns(mockTemplate.Object);

            mockUnit.Setup(unit => unit.CombatStrength)      .Returns(combatStrength);
            mockUnit.Setup(unit => unit.RangedAttackStrength).Returns(rangedAttackStrength);
            mockUnit.Setup(unit => unit.AttackRange)         .Returns(attackRange);
            mockUnit.Setup(unit => unit.HasAttacked)         .Returns(hasAttacked);

            mockUnit.Object.CurrentMovement = currentMovement;

            var newUnit = mockUnit.Object;

            newUnit.Hitpoints = UnitConfig.MaxHealth;

            MockUnitPositionCanon.Setup(canon => canon.GetOwnerOfPossession(newUnit)).Returns(location);

            MockCombatModifierLogic
                .Setup(logic => logic.GetMeleeOffensiveModifierAtLocation(
                    newUnit, It.IsAny<IUnit>(), It.IsAny<IHexCell>()
                ))
                .Returns(attackModifier);

            MockCombatModifierLogic
                .Setup(logic => logic.GetRangedOffensiveModifierAtLocation(
                    newUnit, It.IsAny<IUnit>(), It.IsAny<IHexCell>()
                ))
                .Returns(attackModifier);

            MockCombatModifierLogic
                .Setup(logic => logic.GetMeleeDefensiveModifierAtLocation(
                    It.IsAny<IUnit>(), newUnit, It.IsAny<IHexCell>()
                ))
                .Returns(defenseModifier);

            MockCombatModifierLogic
                .Setup(logic => logic.GetRangedDefensiveModifierAtLocation(
                    It.IsAny<IUnit>(), newUnit, It.IsAny<IHexCell>()
                ))
                .Returns(defenseModifier);

            return newUnit;
        }

        private IHexCell BuildCell(int elevation) {
            return BuildCell(TerrainType.Grassland, TerrainFeature.None, TerrainShape.Flatlands, elevation);
        }

        private IHexCell BuildCell(TerrainType terrain, TerrainFeature feature, TerrainShape shape, int elevation) {
            var mockCell = new Mock<IHexCell>();

            mockCell.SetupAllProperties();

            mockCell.Setup(cell => cell.EdgeElevation).Returns(elevation);

            var newCell = mockCell.Object;

            newCell.Terrain = terrain;
            newCell.Feature = feature;
            newCell.Shape   = TerrainShape.Flatlands;

            return newCell;
        }

        private ICivilization BuildCivilization() {
            return new Mock<ICivilization>().Object;
        }

        #endregion

        #endregion

    }

}
