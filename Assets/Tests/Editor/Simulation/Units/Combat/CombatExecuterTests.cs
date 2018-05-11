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
using Assets.Simulation.Diplomacy;
using Assets.Simulation.Cities;

namespace Assets.Tests.Simulation.Units.Combat {

    [TestFixture]
    public class CombatExecuterTests : ZenjectUnitTestFixture {

        #region internal types

        public class CombatTestData {

            public AttackerTestData Attacker;
            public DefenderTestData Defender;

            public bool UnitsHaveSameOwner;

            public bool OwnersAreAtWar = true;

            public CombatInfo MeleeCombatInfo  = new CombatInfo() { CombatType = CombatType.Melee  };
            public CombatInfo RangedCombatInfo = new CombatInfo() { CombatType = CombatType.Ranged };

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

            public bool CanAttack = true;

            public bool CanMakeRangedAttack = true;

        }

        public class DefenderTestData {

            public int CombatStrength;

            public int LocationElevation;

        }

        public struct CombatTestOutput {

            public bool  CanPerformMeleeAttack;
            public int   MeleeAttackerHealthLeft;
            public int   MeleeDefenderHealthLeft;

            public bool  CanPerformRangedAttack;
            public int   RangedAttackerHealthLeft;
            public int   RangedDefenderHealthLeft;

            public override string ToString() {
                return string.Format(
                    "\nMelee -- CanPerform: {0}, AttackerHealth: {1}, DefenderHealth: {2}\n" + 
                      "Ranged -- CanPerform: {3}, AttackerHealth: {4}, DefenderHealth: {5}",
                    CanPerformMeleeAttack, MeleeAttackerHealthLeft, MeleeDefenderHealthLeft,
                    CanPerformRangedAttack, RangedAttackerHealthLeft, RangedDefenderHealthLeft
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
                    },
                    Defender = new DefenderTestData() {
                        CombatStrength = 100,
                        LocationElevation = 0
                    }
                }).SetName("Basic terrain/no inhibitors/no modifiers/even strength")
                .Returns(new CombatTestOutput() {
                    CanPerformMeleeAttack = true,
                    MeleeAttackerHealthLeft = 70,
                    MeleeDefenderHealthLeft = 70,
                    
                    CanPerformRangedAttack = true,
                    RangedAttackerHealthLeft = 100,
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
                        CanSeeDefender = true
                    },
                    Defender = new DefenderTestData() { 
                        CombatStrength = 100,
                        LocationElevation = 0      
                    }
                }).SetName("Basic terrain/no inhibitors/no modifiers/1.5 attacker strength advantage")
                .Returns(new CombatTestOutput() {
                    CanPerformMeleeAttack = true,
                    MeleeAttackerHealthLeft = 80,
                    MeleeDefenderHealthLeft = 55,
                    
                    CanPerformRangedAttack = true,
                    RangedAttackerHealthLeft = 100,
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
                        CanSeeDefender = true
                    },
                    Defender = new DefenderTestData() {
                        CombatStrength = 100,
                        LocationElevation = 0
                    }
                }).SetName("Basic terrain/no inhibitors/no modifiers/1.75 attacker strength advantage")
                .Returns(new CombatTestOutput() {
                    CanPerformMeleeAttack = true,
                    MeleeAttackerHealthLeft = 83,
                    MeleeDefenderHealthLeft = 48,
                    
                    CanPerformRangedAttack = true,
                    RangedAttackerHealthLeft = 100,
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
                        CanSeeDefender = true
                    },
                    Defender = new DefenderTestData() {
                        CombatStrength = 100,
                        LocationElevation = 0
                    }
                }).SetName("Basic terrain/no inhibitors/no modifiers/2.0 attacker strength advantage")
                .Returns(new CombatTestOutput() {
                    CanPerformMeleeAttack = true,
                    MeleeAttackerHealthLeft = 85,
                    MeleeDefenderHealthLeft = 40,
                    
                    CanPerformRangedAttack = true,
                    RangedAttackerHealthLeft = 100,
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
                        CanSeeDefender = true
                    },
                    Defender = new DefenderTestData() {
                        CombatStrength = 100,
                        LocationElevation = 0
                    }
                }).SetName("Basic terrain/no inhibitors/no modifiers/2.5 attacker strength advantage")
                .Returns(new CombatTestOutput() {
                    CanPerformMeleeAttack = true,
                    MeleeAttackerHealthLeft = 88,
                    MeleeDefenderHealthLeft = 25,
                    
                    CanPerformRangedAttack = true,
                    RangedAttackerHealthLeft = 100,
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
                        CanSeeDefender = true
                    },
                    Defender = new DefenderTestData() {
                        CombatStrength = 100,
                        LocationElevation = 0
                    }
                }).SetName("Basic terrain/no inhibitors/no modifiers/3 attacker strength advantage")
                .Returns(new CombatTestOutput() {
                    CanPerformMeleeAttack = true,
                    MeleeAttackerHealthLeft = 90,
                    MeleeDefenderHealthLeft = 10,
                    
                    CanPerformRangedAttack = true,
                    RangedAttackerHealthLeft = 100,
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
                        CanSeeDefender = true
                    },
                    Defender = new DefenderTestData() {
                        CombatStrength = 100,
                        LocationElevation = 0
                    }
                }).SetName("Basic terrain/no inhibitors/no modifiers/4 attacker strength advantage")
                .Returns(new CombatTestOutput() {
                    CanPerformMeleeAttack = true,
                    MeleeAttackerHealthLeft = 92,
                    MeleeDefenderHealthLeft = 0,
                    
                    CanPerformRangedAttack = true,
                    RangedAttackerHealthLeft = 100,
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
                        CanSeeDefender = true
                    },
                    Defender = new DefenderTestData() {
                        CombatStrength = 100,
                        LocationElevation = 0
                    },
                    MeleeCombatInfo = new CombatInfo() {
                        CombatType = CombatType.Melee, Attacker = new UnitCombatInfo() { CombatModifier = 0.5f }
                    },
                    RangedCombatInfo = new CombatInfo() {
                        CombatType = CombatType.Ranged, Attacker = new UnitCombatInfo() { CombatModifier = 0.5f }
                    }
                }).SetName("Basic terrain/no inhibitors/even strength/50% attacker modifier")
                .Returns(new CombatTestOutput() {
                    CanPerformMeleeAttack = true,
                    MeleeAttackerHealthLeft = 80,
                    MeleeDefenderHealthLeft = 55,
                    
                    CanPerformRangedAttack = true,
                    RangedAttackerHealthLeft = 100,
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
                        CanSeeDefender = true
                    },
                    Defender = new DefenderTestData() {
                        CombatStrength = 100,
                        LocationElevation = 0
                    },
                    MeleeCombatInfo = new CombatInfo() {
                        CombatType = CombatType.Melee, Defender = new UnitCombatInfo() { CombatModifier = 0.5f }
                    },
                    RangedCombatInfo = new CombatInfo() {
                        CombatType = CombatType.Ranged, Defender = new UnitCombatInfo() { CombatModifier = 0.5f }
                    }
                }).SetName("Basic terrain/no inhibitors/even strength/50% defender modifier")
                .Returns(new CombatTestOutput() {
                    CanPerformMeleeAttack = true,
                    MeleeAttackerHealthLeft = 55,
                    MeleeDefenderHealthLeft = 80,
                    
                    CanPerformRangedAttack = true,
                    RangedAttackerHealthLeft = 100,
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
                        CanSeeDefender = true
                    },
                    Defender = new DefenderTestData() {
                        CombatStrength = 100,
                        LocationElevation = 0
                    }
                }).SetName("No attacker movement")
                .Returns(new CombatTestOutput() {
                    CanPerformMeleeAttack = false,
                    MeleeAttackerHealthLeft = 100,
                    MeleeDefenderHealthLeft = 100,
                    
                    CanPerformRangedAttack = false,
                    RangedAttackerHealthLeft = 100,
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
                        CanAttack = false,
                    },
                    Defender = new DefenderTestData() {
                        CombatStrength = 100,
                        LocationElevation = 0
                    }
                }).SetName("Attacker CanAttack field is false")
                .Returns(new CombatTestOutput() {
                    CanPerformMeleeAttack = false,
                    MeleeAttackerHealthLeft = 100,
                    MeleeDefenderHealthLeft = 100,
                    
                    CanPerformRangedAttack = false,
                    RangedAttackerHealthLeft = 100,
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
                        CanSeeDefender = true
                    },
                    Defender = new DefenderTestData() {
                        CombatStrength = 100,
                        LocationElevation = 0
                    }
                }).SetName("Basic terrain/no attacker range/no modifiers/even strength")
                .Returns(new CombatTestOutput() {
                    CanPerformMeleeAttack = true,
                    MeleeAttackerHealthLeft = 70,
                    MeleeDefenderHealthLeft = 70,
                    
                    CanPerformRangedAttack = false,
                    RangedAttackerHealthLeft = 100,
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
                        CanSeeDefender = true
                    },
                    Defender = new DefenderTestData() {
                        CombatStrength = 100,
                        LocationElevation = 1
                    }
                }).SetName("Basic terrain/attacking up slope/no modifiers/even strength")
                .Returns(new CombatTestOutput() {
                    CanPerformMeleeAttack = true,
                    MeleeAttackerHealthLeft = 70,
                    MeleeDefenderHealthLeft = 70,
                    
                    CanPerformRangedAttack = true,
                    RangedAttackerHealthLeft = 100,
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
                        CanSeeDefender = true
                    },
                    Defender = new DefenderTestData() {
                        CombatStrength = 100,
                        LocationElevation = 2
                    }
                }).SetName("Basic terrain/attacking up cliff/no modifiers/even strength")
                .Returns(new CombatTestOutput() {
                    CanPerformMeleeAttack = false,
                    MeleeAttackerHealthLeft = 100,
                    MeleeDefenderHealthLeft = 100,
                    
                    CanPerformRangedAttack = true,
                    RangedAttackerHealthLeft = 100,
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
                        CanSeeDefender = true
                    },
                    Defender = new DefenderTestData() {
                        CombatStrength = 100,
                        LocationElevation = 0
                    }
                }).SetName("Basic terrain/attacking down cliff/no modifiers/even strength")
                .Returns(new CombatTestOutput() {
                    CanPerformMeleeAttack = false,
                    MeleeAttackerHealthLeft = 100,
                    MeleeDefenderHealthLeft = 100,
                    
                    CanPerformRangedAttack = true,
                    RangedAttackerHealthLeft = 100,
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
                        CanSeeDefender = true
                    },
                    Defender = new DefenderTestData() {
                        CombatStrength = 100,
                        LocationElevation = 0
                    }
                }).SetName("Basic terrain/attacker cannot move to defender/no modifiers/even strength")
                .Returns(new CombatTestOutput() {
                    CanPerformMeleeAttack = false,
                    MeleeAttackerHealthLeft = 100,
                    MeleeDefenderHealthLeft = 100,
                    
                    CanPerformRangedAttack = true,
                    RangedAttackerHealthLeft = 100,
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
                        CanSeeDefender = false
                    },
                    Defender = new DefenderTestData() {
                        CombatStrength = 100,
                        LocationElevation = 0
                    }
                }).SetName("Basic terrain/attacker cannot see defender/no modifiers/even strength")
                .Returns(new CombatTestOutput() {
                    CanPerformMeleeAttack = false,
                    MeleeAttackerHealthLeft = 100,
                    MeleeDefenderHealthLeft = 100,
                    
                    CanPerformRangedAttack = false,
                    RangedAttackerHealthLeft = 100,
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
                        CanSeeDefender = true
                    },
                    Defender = new DefenderTestData() {
                        CombatStrength = 0,
                        LocationElevation = 0
                    }
                }).SetName("Basic terrain/no inhibitors/no modifiers/zero-strength defender")
                .Returns(new CombatTestOutput() {
                    CanPerformMeleeAttack = true,
                    MeleeAttackerHealthLeft = 100,
                    MeleeDefenderHealthLeft = 0,
                    
                    CanPerformRangedAttack = true,
                    RangedAttackerHealthLeft = 100,
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
                        CanSeeDefender = true
                    },
                    Defender = new DefenderTestData() {
                        CombatStrength = 100,
                        LocationElevation = 0
                    }
                }).SetName("Basic terrain/no inhibitors/no modifiers/zero-strength attacker")
                .Returns(new CombatTestOutput() {
                    CanPerformMeleeAttack = false,
                    MeleeAttackerHealthLeft = 100,
                    MeleeDefenderHealthLeft = 100,
                    
                    CanPerformRangedAttack = false,
                    RangedAttackerHealthLeft = 100,
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
                        CanSeeDefender = true
                    },
                    Defender = new DefenderTestData() {
                        CombatStrength = 100,
                        LocationElevation = 0
                    },
                    UnitsHaveSameOwner = true

                }).SetName("Basic terrain/no inhibitors/no modifiers/even strength/same owner")
                .Returns(new CombatTestOutput() {
                    CanPerformMeleeAttack = false,
                    MeleeAttackerHealthLeft = 100,
                    MeleeDefenderHealthLeft = 100,
                    
                    CanPerformRangedAttack = false,
                    RangedAttackerHealthLeft = 100,
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
                        CanSeeDefender = true
                    },
                    Defender = new DefenderTestData() {
                        CombatStrength = 100,
                        LocationElevation = 0
                    },
                    OwnersAreAtWar = false
                }).SetName("Otherwise valid attack/owners are not at war")
                .Returns(new CombatTestOutput() {
                    CanPerformMeleeAttack = false,
                    MeleeAttackerHealthLeft = 100,
                    MeleeDefenderHealthLeft = 100,
                    
                    CanPerformRangedAttack = false,
                    RangedAttackerHealthLeft = 100,
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
                        CanMakeRangedAttack = false
                    },
                    Defender = new DefenderTestData() {
                        CombatStrength = 100,
                        LocationElevation = 0
                    }
                }).SetName("Otherwise valid attack/attacker cannot make ranged attack")
                .Returns(new CombatTestOutput() {
                    CanPerformMeleeAttack = true,
                    MeleeAttackerHealthLeft = 70,
                    MeleeDefenderHealthLeft = 70,
                    
                    CanPerformRangedAttack = false,
                    RangedAttackerHealthLeft = 100,
                    RangedDefenderHealthLeft = 100
                });
            }
        }

        #endregion

        #region instance fields and properties

        private Mock<IUnitPositionCanon>                            MockUnitPositionCanon;
        private Mock<IHexGrid>                                      MockGrid;
        private Mock<IUnitLineOfSightLogic>                         MockUnitLineOfSightLogic;
        private Mock<ICombatInfoLogic>                              MockCombatInfoLogic;
        private Mock<IPossessionRelationship<ICivilization, IUnit>> MockUnitPossessionCanon;
        private Mock<IWarCanon>                                     MockWarCanon;
        private Mock<ICityConquestLogic>                            MockCityConquestLogic;
        private Mock<IPostCombatMovementLogic>                      MockPostCombatMovementLogic;
        private Mock<ICombatDestructionLogic>                       MockCombatDestructionLogic;


        private IUnitConfig UnitConfig;
        

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockUnitPositionCanon       = new Mock<IUnitPositionCanon>();
            MockGrid                    = new Mock<IHexGrid>();
            MockUnitLineOfSightLogic    = new Mock<IUnitLineOfSightLogic>();
            MockCombatInfoLogic         = new Mock<ICombatInfoLogic>();
            MockUnitPossessionCanon     = new Mock<IPossessionRelationship<ICivilization, IUnit>>();
            MockWarCanon                = new Mock<IWarCanon>();
            MockCityConquestLogic       = new Mock<ICityConquestLogic>();
            MockPostCombatMovementLogic = new Mock<IPostCombatMovementLogic>();
            MockCombatDestructionLogic  = new Mock<ICombatDestructionLogic>();

            Container.Bind<IUnitPositionCanon>                           ().FromInstance(MockUnitPositionCanon     .Object);
            Container.Bind<IHexGrid>                                     ().FromInstance(MockGrid                   .Object);
            Container.Bind<IUnitLineOfSightLogic>                        ().FromInstance(MockUnitLineOfSightLogic   .Object);
            Container.Bind<ICombatInfoLogic>                             ().FromInstance(MockCombatInfoLogic        .Object);
            Container.Bind<IPossessionRelationship<ICivilization, IUnit>>().FromInstance(MockUnitPossessionCanon    .Object);
            Container.Bind<IWarCanon>                                    ().FromInstance(MockWarCanon               .Object);
            Container.Bind<ICityConquestLogic>                           ().FromInstance(MockCityConquestLogic      .Object);
            Container.Bind<IPostCombatMovementLogic>                     ().FromInstance(MockPostCombatMovementLogic.Object);
            Container.Bind<ICombatDestructionLogic>                      ().FromInstance(MockCombatDestructionLogic .Object);

            Container.Bind<UnitSignals>().AsSingle();

            Container.Bind<IUnitConfig>().To<UnitConfig>().FromNewScriptableObjectResource("Tests/Combat Executer Unit Config").AsSingle();

            UnitConfig = Container.Resolve<IUnitConfig>();

            Container.Bind<CombatExecuter>().AsSingle();
        }

        #endregion

        #region tests

        [Test(Description = "")]
        [TestCaseSource("TestCases")]
        public CombatTestOutput PerformCombatTests(CombatTestData testData) {
            IHexCell attackerLocation = BuildCell(testData.Attacker.LocationElevation);
            IHexCell defenderLocation = BuildCell(testData.Defender.LocationElevation);

            IUnit meleeAttacker = BuildUnit(attackerLocation, testData.Attacker);
            IUnit meleeDefender = BuildUnit(defenderLocation, testData.Defender);

            IUnit rangedAttacker = BuildUnit(attackerLocation, testData.Attacker);
            IUnit rangedDefender = BuildUnit(defenderLocation, testData.Defender);

            SetCombatConditions(
                testData.Attacker.DistanceFromDefender, testData.Attacker.CanMoveToDefender,
                testData.Attacker.CanSeeDefender, defenderLocation
            );

            if(testData.UnitsHaveSameOwner) {
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

                MockWarCanon.Setup(canon => canon.AreAtWar(attackerOwner, defenderOwner)).Returns(testData.OwnersAreAtWar);
                MockWarCanon.Setup(canon => canon.AreAtWar(defenderOwner, attackerOwner)).Returns(testData.OwnersAreAtWar);
            }

            MockCombatInfoLogic.Setup(
                logic => logic.GetMeleeAttackInfo(It.IsAny<IUnit>(), It.IsAny<IUnit>(), It.IsAny<IHexCell>())
            ).Returns(testData.MeleeCombatInfo);

            MockCombatInfoLogic.Setup(
                logic => logic.GetRangedAttackInfo(It.IsAny<IUnit>(), It.IsAny<IUnit>(), It.IsAny<IHexCell>())
            ).Returns(testData.RangedCombatInfo);

            var combatExecuter = Container.Resolve<CombatExecuter>();

            var results = new CombatTestOutput();

            results.CanPerformMeleeAttack = combatExecuter.CanPerformMeleeAttack(meleeAttacker, meleeDefender);

            if(results.CanPerformMeleeAttack) {
                combatExecuter.PerformMeleeAttack(meleeAttacker, meleeDefender);
            }

            results.MeleeAttackerHealthLeft   = meleeAttacker.Hitpoints;
            results.MeleeDefenderHealthLeft   = meleeDefender.Hitpoints;
            
            results.CanPerformRangedAttack = combatExecuter.CanPerformRangedAttack(rangedAttacker, rangedDefender);

            if(results.CanPerformRangedAttack) {
                combatExecuter.PerformRangedAttack(rangedAttacker, rangedDefender);
            }

            results.RangedAttackerHealthLeft   = rangedAttacker.Hitpoints;
            results.RangedDefenderHealthLeft   = rangedDefender.Hitpoints;

            if(results.CanPerformMeleeAttack) {
                Assert.AreEqual(
                    meleeAttacker.CanAttack, testData.MeleeCombatInfo.Attacker.CanAttackAfterAttacking,
                    "MeleeAttacker.CanAttack has an unexpected value"
                );
            }
            
            if(results.CanPerformRangedAttack) {
                Assert.AreEqual(
                    rangedAttacker.CanAttack, testData.RangedCombatInfo.Attacker.CanAttackAfterAttacking,
                    "RangedAttacker.CanAttack has an unexpected value"
                );
            }

            return results;
        }

        [Test(Description = "When PerformMeleeCombat is called on a valid melee combat, " +
            "it should call into ICityConquestLogic.HandleCityCaptureFromCombat, " +
            "ICombatDestructionLogic.HandleUnitDestructionFromCombat, and " +
            "IPostCombatMovementLogic.HandleAttackerMovementAfterCombat. These methods should " +
            "be provided with the attacker and defender passed into PerformMeleeCombat as well " +
            "as the CombatInfo provided by ICombatInfoLogic")]
        public void PerformMeleeCombat_CallsIntoCombatPerformanceLogicsCorrectly() {
            var attackerLocation = BuildCell(0);
            var defenderLocation = BuildCell(0);

            var attacker = BuildUnit(attackerLocation, new AttackerTestData() {
                CombatStrength = 100, CurrentMovement = 2
            });

            var defender = BuildUnit(defenderLocation, new DefenderTestData() {
                CombatStrength = 100,
            });

            SetCombatConditions(0, true, true, defenderLocation);

            var attackerOwner = BuildCivilization();
            var defenderOwner = BuildCivilization();

            MockWarCanon.Setup(canon => canon.AreAtWar(attackerOwner, defenderOwner)).Returns(true);
            MockWarCanon.Setup(canon => canon.AreAtWar(defenderOwner, attackerOwner)).Returns(true);

            MockUnitPossessionCanon.Setup(canon => canon.GetOwnerOfPossession(attacker)).Returns(attackerOwner);
            MockUnitPossessionCanon.Setup(canon => canon.GetOwnerOfPossession(defender)).Returns(defenderOwner);

            var combatInfo = new CombatInfo() {
                CombatType = CombatType.Melee,
                Attacker = new UnitCombatInfo() { CombatModifier = 1f, CanMoveAfterAttacking = true },
                Defender = new UnitCombatInfo() { CombatModifier = 1f }
            };

            MockCombatInfoLogic
                .Setup(logic => logic.GetMeleeAttackInfo(attacker, defender, defenderLocation))
                .Returns(combatInfo);

            var combatExecuter = Container.Resolve<CombatExecuter>();

            var executionSequence = new MockSequence();

            MockCityConquestLogic.InSequence(executionSequence).Setup(
                logic => logic.HandleCityCaptureFromCombat(attacker, defender, combatInfo)
            );

            MockCombatDestructionLogic.InSequence(executionSequence).Setup(
                logic => logic.HandleUnitDestructionFromCombat(attacker, defender, combatInfo)
            );

            MockPostCombatMovementLogic.InSequence(executionSequence).Setup(
                logic => logic.HandleAttackerMovementAfterCombat(attacker, defender, combatInfo)
            );

            combatExecuter.PerformMeleeAttack(attacker, defender);

            MockCityConquestLogic      .VerifyAll();
            MockCombatDestructionLogic .VerifyAll();
            MockPostCombatMovementLogic.VerifyAll();
        }

        [Test(Description = "When PerformRangedCombat is called on a valid ranged combat, " +
            "it should call into ICityConquestLogic.HandleCityCaptureFromCombat, " +
            "ICombatDestructionLogic.HandleUnitDestructionFromCombat, and " +
            "IPostCombatMovementLogic.HandleAttackerMovementAfterCombat. These methods should " +
            "be provided with the attacker and defender passed into PerformRangedCombat as well " +
            "as the CombatInfo provided by ICombatInfoLogic")]
        public void PerformRangedCombat_CallsIntoCombatPerformanceLogicsCorrectly() {
            var attackerLocation = BuildCell(0);
            var defenderLocation = BuildCell(0);

            var attacker = BuildUnit(attackerLocation, new AttackerTestData() {
                RangedAttackStrength = 100, CurrentMovement = 2
            });

            var defender = BuildUnit(defenderLocation, new DefenderTestData() {
                CombatStrength = 100,
            });

            SetCombatConditions(0, true, true, defenderLocation);

            var attackerOwner = BuildCivilization();
            var defenderOwner = BuildCivilization();

            MockWarCanon.Setup(canon => canon.AreAtWar(attackerOwner, defenderOwner)).Returns(true);
            MockWarCanon.Setup(canon => canon.AreAtWar(defenderOwner, attackerOwner)).Returns(true);

            MockUnitPossessionCanon.Setup(canon => canon.GetOwnerOfPossession(attacker)).Returns(attackerOwner);
            MockUnitPossessionCanon.Setup(canon => canon.GetOwnerOfPossession(defender)).Returns(defenderOwner);

            var combatInfo = new CombatInfo() {
                Attacker = new UnitCombatInfo() { CombatModifier = 1f, CanMoveAfterAttacking = true },
                Defender = new UnitCombatInfo() { CombatModifier = 1f }
            };

            MockCombatInfoLogic
                .Setup(logic => logic.GetRangedAttackInfo(attacker, defender, defenderLocation))
                .Returns(combatInfo);

            var combatExecuter = Container.Resolve<CombatExecuter>();

            var executionSequence = new MockSequence();

            MockCityConquestLogic.InSequence(executionSequence).Setup(
                logic => logic.HandleCityCaptureFromCombat(attacker, defender, combatInfo)
            );

            MockCombatDestructionLogic.InSequence(executionSequence).Setup(
                logic => logic.HandleUnitDestructionFromCombat(attacker, defender, combatInfo)
            );

            MockPostCombatMovementLogic.InSequence(executionSequence).Setup(
                logic => logic.HandleAttackerMovementAfterCombat(attacker, defender, combatInfo)
            );

            combatExecuter.PerformRangedAttack(attacker, defender);

            MockCityConquestLogic      .VerifyAll();
            MockCombatDestructionLogic .VerifyAll();
            MockPostCombatMovementLogic.VerifyAll();
        }

        #endregion

        #region utilities

        private void SetCombatConditions(
            int distanceBetween, bool attackerCanMoveTo, bool defenderCanBeSeen, IHexCell defenderLocation
        ){
            MockGrid.Setup(grid => grid.GetDistance(It.IsAny<IHexCell>(), It.IsAny<IHexCell>()))
                .Returns(distanceBetween);

            MockUnitPositionCanon.Setup(canon => canon.CanPlaceUnitAtLocation(It.IsAny<IUnit>(), It.IsAny<IHexCell>(), true))
                .Returns(attackerCanMoveTo);

            MockUnitLineOfSightLogic
                .Setup(
                    logic => logic.GetCellsVisibleToUnit(It.IsAny<IUnit>())
                ).Returns(
                    defenderCanBeSeen ? new List<IHexCell>() { defenderLocation } : new List<IHexCell>()
                );
        }

        private IUnit BuildUnit(IHexCell location, AttackerTestData attackerData) {
            return BuildUnit(
                location:              location,
                currentMovement:       attackerData.CurrentMovement,
                combatStrength:        attackerData.CombatStrength,
                rangedAttackStrength:  attackerData.RangedAttackStrength,
                attackRange:           attackerData.Range,
                canAttack:             attackerData.CanAttack,
                isReadyForRangedAttack:   attackerData.CanMakeRangedAttack
            );
        }

        private IUnit BuildUnit(IHexCell location, DefenderTestData defenderData) {
            return BuildUnit(
                location:              location,
                currentMovement:       0,
                combatStrength:        defenderData.CombatStrength,
                rangedAttackStrength:  0,
                attackRange:           0,
                canAttack:             true,
                isReadyForRangedAttack:   true
            );
        }

        private IUnit BuildUnit(
            IHexCell location, int currentMovement,
            int combatStrength, int rangedAttackStrength,
            int attackRange, bool canAttack,
            bool isReadyForRangedAttack
        ){
            var mockUnit = new Mock<IUnit>();

            var mockTemplate = new Mock<IUnitTemplate>();

            mockUnit.SetupAllProperties();

            mockUnit.Setup(unit => unit.Template).Returns(mockTemplate.Object);

            mockUnit.Setup(unit => unit.CombatStrength)        .Returns(combatStrength);
            mockUnit.Setup(unit => unit.RangedAttackStrength)  .Returns(rangedAttackStrength);
            mockUnit.Setup(unit => unit.AttackRange)           .Returns(attackRange);
            mockUnit.Setup(unit => unit.IsReadyForRangedAttack).Returns(isReadyForRangedAttack);

            mockUnit.Object.CurrentMovement = currentMovement;

            var newUnit = mockUnit.Object;

            newUnit.Hitpoints = UnitConfig.MaxHealth;
            newUnit.CanAttack = canAttack;

            MockUnitPositionCanon.Setup(canon => canon.GetOwnerOfPossession(newUnit)).Returns(location);

            return newUnit;
        }

        private IHexCell BuildCell(int elevation) {
            var mockCell = new Mock<IHexCell>();

            mockCell.SetupAllProperties();

            mockCell.Setup(cell => cell.EdgeElevation).Returns(elevation);

            return mockCell.Object;
        }

        private ICivilization BuildCivilization() {
            return new Mock<ICivilization>().Object;
        }

        #endregion

        #endregion

    }

}
