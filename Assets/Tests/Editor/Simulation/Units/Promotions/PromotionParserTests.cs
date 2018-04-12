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
using Assets.Simulation.Units.Promotions;
using Assets.Simulation.HexMap;

namespace Assets.Tests.Simulation.Units.Promotions {

    public class PromotionParserTests : ZenjectUnitTestFixture {

        #region internal types

        public class GetCombatInfoTestData {

            public UnitTestData Attacker = new UnitTestData();

            public UnitTestData Defender = new UnitTestData();

            public HexCellTestData Location = new HexCellTestData();

            public CombatType CombatType = CombatType.Melee;

        }

        public class GetMovementInfoTestData {

            public UnitTestData Unit;

        }

        public class UnitTestData {

            public UnitType Type;

            public List<PromotionTestData> Promotions = new List<PromotionTestData>();

        }

        public class HexCellTestData {

            public bool IsRoughTerrain;

            public TerrainFeature Feature;

        }

        public class PromotionTestData {

            public List<PromotionArgType> Args = new List<PromotionArgType>();

            public float Float;

            public UnitType UnitType;

        }

        #endregion

        #region static fields and properties

        public static IEnumerable GetCombatInfoTestCases {
            get {
                yield return new TestCaseData(new GetCombatInfoTestData() {
                    CombatType = CombatType.Ranged
                }).SetName("Info contains passed combat type").Returns(new CombatInfo() {
                    CombatType = CombatType.Ranged
                });

                yield return new TestCaseData(new GetCombatInfoTestData() {
                    Attacker = new UnitTestData() {
                        Type = UnitType.Melee,
                        Promotions = new List<PromotionTestData>() {
                            new PromotionTestData() {
                                Args = new List<PromotionArgType>() { PromotionArgType.CombatStrength },
                                Float = 1f
                            },
                            new PromotionTestData() {
                                Args = new List<PromotionArgType>() { PromotionArgType.CombatStrength },
                                Float = 2f
                            }
                        }
                    },
                    Defender = new UnitTestData() {
                        Type = UnitType.Melee,
                        Promotions = new List<PromotionTestData>() {
                            new PromotionTestData() {
                                Args = new List<PromotionArgType>() { PromotionArgType.CombatStrength },
                                Float = 3f
                            },
                            new PromotionTestData() {
                                Args = new List<PromotionArgType>() { PromotionArgType.CombatStrength },
                                Float = 4f
                            }
                        }
                    }
                }).SetName("Promotions with CombatStrength arg add float to combat modifiers").Returns(new CombatInfo() {
                    CombatType = CombatType.Melee, AttackerCombatModifier = 3f, DefenderCombatModifier = 7f
                });

                yield return new TestCaseData(new GetCombatInfoTestData() {
                    Attacker = new UnitTestData() {
                        Promotions = new List<PromotionTestData>() {
                            new PromotionTestData() {
                                Args = new List<PromotionArgType>() { PromotionArgType.CanMoveAfterAttacking }
                            }
                        }
                    }
                }).SetName("CanMoveAfterAttacking promotion reflected in returned info").Returns(new CombatInfo() {
                    CombatType = CombatType.Melee, AttackerCanMoveAfterAttacking = true
                });

                yield return new TestCaseData(new GetCombatInfoTestData() {
                    Attacker = new UnitTestData() {
                        Promotions = new List<PromotionTestData>() {
                            new PromotionTestData() {
                                Args = new List<PromotionArgType>() { PromotionArgType.CanAttackAfterAttacking }
                            }
                        }
                    }
                }).SetName("CanAttackAfterAttacking promotion reflected in returned info").Returns(new CombatInfo() {
                    CombatType = CombatType.Melee, AttackerCanAttackAfterAttacking = true
                });

                yield return new TestCaseData(new GetCombatInfoTestData() {
                    Attacker = new UnitTestData() {
                        Promotions = new List<PromotionTestData>() {
                            new PromotionTestData() {
                                Args = new List<PromotionArgType>() { PromotionArgType.IgnoresAmphibiousPenalty }
                            }
                        }
                    }
                }).SetName("IgnoresAmphibiousPenalty promotion reflected in returned info").Returns(new CombatInfo() {
                    CombatType = CombatType.Melee, AttackerIgnoresAmphibiousPenalty = true
                });

                yield return new TestCaseData(new GetCombatInfoTestData() {
                    Defender = new UnitTestData() {
                        Promotions = new List<PromotionTestData>() {
                            new PromotionTestData() {
                                Args = new List<PromotionArgType>() { PromotionArgType.NoDefensiveTerrainBonuses }
                            }
                        }
                    }
                }).SetName("NoDefensiveTerrainBonuses promotion reflected in returned info").Returns(new CombatInfo() {
                    CombatType = CombatType.Melee, DefenderIgnoresDefensiveTerrainBonuses = true
                });

                yield return new TestCaseData(new GetCombatInfoTestData() {
                    Defender = new UnitTestData() {
                        Promotions = new List<PromotionTestData>() {
                            new PromotionTestData() {
                                Args = new List<PromotionArgType>() { PromotionArgType.CanMoveAfterAttacking }
                            },
                            new PromotionTestData() {
                                Args = new List<PromotionArgType>() { PromotionArgType.CanAttackAfterAttacking }
                            },
                            new PromotionTestData() {
                                Args = new List<PromotionArgType>() { PromotionArgType.IgnoresAmphibiousPenalty }
                            }
                        }
                    }
                }).SetName("Attacker-related promotions ignored if belonging to defender").Returns(new CombatInfo() {
                    CombatType = CombatType.Melee
                });

                yield return new TestCaseData(new GetCombatInfoTestData() {
                    Attacker = new UnitTestData() {
                        Promotions = new List<PromotionTestData>() {
                            new PromotionTestData() {
                                Args = new List<PromotionArgType>() { PromotionArgType.NoDefensiveTerrainBonuses }
                            }
                        }
                    }
                }).SetName("Defender-related promotions ignored if belonging to attacker").Returns(new CombatInfo() {
                    CombatType = CombatType.Melee
                });

                yield return new TestCaseData(new GetCombatInfoTestData() {
                    Attacker = new UnitTestData() {
                        Type = UnitType.Melee,
                        Promotions = new List<PromotionTestData>() {
                            new PromotionTestData() {
                                Args = new List<PromotionArgType>() { PromotionArgType.CombatStrength, PromotionArgType.Melee },
                                Float = 2f
                            },
                            new PromotionTestData() {
                                Args = new List<PromotionArgType>() { PromotionArgType.CombatStrength, PromotionArgType.Ranged },
                                Float = 1f
                            }
                        }
                    },
                    Defender = new UnitTestData() {
                        Type = UnitType.Melee,
                        Promotions = new List<PromotionTestData>() {
                            new PromotionTestData() {
                                Args = new List<PromotionArgType>() { PromotionArgType.CombatStrength, PromotionArgType.Ranged },
                                Float = 1f
                            },
                            new PromotionTestData() {
                                Args = new List<PromotionArgType>() { PromotionArgType.CombatStrength },
                                Float = 3f
                            },
                        }
                    }
                }).SetName("Promotions with Ranged arg don't contribute in melee combat").Returns(new CombatInfo() {
                    CombatType = CombatType.Melee, AttackerCombatModifier = 2f, DefenderCombatModifier = 3f
                });

                yield return new TestCaseData(new GetCombatInfoTestData() {
                    Attacker = new UnitTestData() {
                        Type = UnitType.Melee,
                        Promotions = new List<PromotionTestData>() {
                            new PromotionTestData() {
                                Args = new List<PromotionArgType>() { PromotionArgType.CombatStrength, PromotionArgType.Ranged },
                                Float = 2f
                            },
                            new PromotionTestData() {
                                Args = new List<PromotionArgType>() { PromotionArgType.CombatStrength, PromotionArgType.Melee },
                                Float = 1f
                            }
                        }
                    },
                    Defender = new UnitTestData() {
                        Type = UnitType.Melee,
                        Promotions = new List<PromotionTestData>() {
                            new PromotionTestData() {
                                Args = new List<PromotionArgType>() { PromotionArgType.CombatStrength, PromotionArgType.Melee },
                                Float = 1f
                            },
                            new PromotionTestData() {
                                Args = new List<PromotionArgType>() { PromotionArgType.CombatStrength },
                                Float = 3f
                            },
                        }
                    },
                    CombatType = CombatType.Ranged
                }).SetName("Promotions with Melee arg don't contribute in ranged combat").Returns(new CombatInfo() {
                    CombatType = CombatType.Ranged, AttackerCombatModifier = 2f, DefenderCombatModifier = 3f
                });

                yield return new TestCaseData(new GetCombatInfoTestData() {
                    Attacker = new UnitTestData() {
                        Type = UnitType.Melee,
                        Promotions = new List<PromotionTestData>() {
                            new PromotionTestData() {
                                Args = new List<PromotionArgType>() { PromotionArgType.CombatStrength, PromotionArgType.WhenDefending },
                                Float = 3f
                            }
                        }
                    }
                }).SetName("Promotions with WhenDefending arg don't apply to attacker").Returns(new CombatInfo() {
                    CombatType = CombatType.Melee, AttackerCombatModifier = 0f
                });

                yield return new TestCaseData(new GetCombatInfoTestData() {
                    Defender = new UnitTestData() {
                        Type = UnitType.Melee,
                        Promotions = new List<PromotionTestData>() {
                            new PromotionTestData() {
                                Args = new List<PromotionArgType>() { PromotionArgType.CombatStrength, PromotionArgType.WhenAttacking },
                                Float = 3f
                            }
                        }
                    }
                }).SetName("Promotions with WhenAttacking arg don't apply to defender").Returns(new CombatInfo() {
                    CombatType = CombatType.Melee, DefenderCombatModifier = 0f
                });

                yield return new TestCaseData(new GetCombatInfoTestData() {
                    Attacker = new UnitTestData() {
                        Type = UnitType.Melee,
                        Promotions = new List<PromotionTestData>() {
                            new PromotionTestData() {
                                Args = new List<PromotionArgType>() { PromotionArgType.CombatStrength, PromotionArgType.OnFlatTerrain },
                                Float = 3f
                            }
                        }
                    },
                    Defender = new UnitTestData() {
                        Type = UnitType.Melee,
                        Promotions = new List<PromotionTestData>() {
                            new PromotionTestData() {
                                Args = new List<PromotionArgType>() { PromotionArgType.CombatStrength, PromotionArgType.OnFlatTerrain },
                                Float = 3f
                            }
                        }
                    },
                    Location = new HexCellTestData() {
                        IsRoughTerrain = true
                    }
                }).SetName("Promotions with OnFlatTerrain arg don't apply to rough terrain").Returns(new CombatInfo() {
                    CombatType = CombatType.Melee, AttackerCombatModifier = 0f, DefenderCombatModifier = 0f
                });

                yield return new TestCaseData(new GetCombatInfoTestData() {
                    Attacker = new UnitTestData() {
                        Type = UnitType.Melee,
                        Promotions = new List<PromotionTestData>() {
                            new PromotionTestData() {
                                Args = new List<PromotionArgType>() { PromotionArgType.CombatStrength, PromotionArgType.OnRoughTerrain },
                                Float = 3f
                            }
                        }
                    },
                    Defender = new UnitTestData() {
                        Type = UnitType.Melee,
                        Promotions = new List<PromotionTestData>() {
                            new PromotionTestData() {
                                Args = new List<PromotionArgType>() { PromotionArgType.CombatStrength, PromotionArgType.OnRoughTerrain },
                                Float = 3f
                            }
                        }
                    },
                    Location = new HexCellTestData() {
                        IsRoughTerrain = false
                    }
                }).SetName("Promotions with OnRoughTerrain arg don't apply to flat terrain").Returns(new CombatInfo() {
                    CombatType = CombatType.Melee, AttackerCombatModifier = 0f, DefenderCombatModifier = 0f
                });

                yield return new TestCaseData(new GetCombatInfoTestData() {
                    Attacker = new UnitTestData() {
                        Type = UnitType.Melee,
                        Promotions = new List<PromotionTestData>() {
                            new PromotionTestData() {
                                Args = new List<PromotionArgType>() { PromotionArgType.CombatStrength, PromotionArgType.InForestAndJungle },
                                Float = 3f
                            }
                        }
                    },
                    Defender = new UnitTestData() {
                        Type = UnitType.Melee,
                        Promotions = new List<PromotionTestData>() {
                            new PromotionTestData() {
                                Args = new List<PromotionArgType>() { PromotionArgType.CombatStrength, PromotionArgType.InForestAndJungle },
                                Float = 3f
                            }
                        }
                    },
                    Location = new HexCellTestData() {
                        Feature = TerrainFeature.None
                    }
                }).SetName("Promotions with InForestAndJungle arg don't apply when no feature").Returns(new CombatInfo() {
                    CombatType = CombatType.Melee, AttackerCombatModifier = 0f, DefenderCombatModifier = 0f
                });

                yield return new TestCaseData(new GetCombatInfoTestData() {
                    Attacker = new UnitTestData() {
                        Type = UnitType.Melee,
                        Promotions = new List<PromotionTestData>() {
                            new PromotionTestData() {
                                Args = new List<PromotionArgType>() { PromotionArgType.CombatStrength, PromotionArgType.InForestAndJungle },
                                Float = 3f
                            }
                        }
                    },
                    Defender = new UnitTestData() {
                        Type = UnitType.Melee,
                        Promotions = new List<PromotionTestData>() {
                            new PromotionTestData() {
                                Args = new List<PromotionArgType>() { PromotionArgType.CombatStrength, PromotionArgType.InForestAndJungle },
                                Float = 3f
                            }
                        }
                    },
                    Location = new HexCellTestData() {
                        Feature = TerrainFeature.None
                    }
                }).SetName("Promotions with InForestAndJungle arg don't apply when in marsh").Returns(new CombatInfo() {
                    CombatType = CombatType.Melee, AttackerCombatModifier = 0f, DefenderCombatModifier = 0f
                });

                yield return new TestCaseData(new GetCombatInfoTestData() {
                    Attacker = new UnitTestData() {
                        Type = UnitType.Melee,
                        Promotions = new List<PromotionTestData>() {
                            new PromotionTestData() {
                                Args = new List<PromotionArgType>() { PromotionArgType.CombatStrength, PromotionArgType.AgainstUnitType },
                                Float = 3f, UnitType = UnitType.Melee
                            }
                        }
                    },
                    Defender = new UnitTestData() {
                        Type = UnitType.Archery,
                        Promotions = new List<PromotionTestData>() {
                            new PromotionTestData() {
                                Args = new List<PromotionArgType>() { PromotionArgType.CombatStrength, PromotionArgType.AgainstUnitType },
                                Float = 3f, UnitType = UnitType.Mounted
                            }
                        }
                    }
                }).SetName("Promotions with AgainstUnitType arg don't when other unit is wrong type").Returns(new CombatInfo() {
                    CombatType = CombatType.Melee, AttackerCombatModifier = 0f, DefenderCombatModifier = 0f
                });
            }
        }

        public static IEnumerable GetMovementInfoTestCases {
            get {
                yield return new TestCaseData(new GetMovementInfoTestData() {
                    Unit = new UnitTestData() {
                        Promotions = new List<PromotionTestData>() {
                            new PromotionTestData() {
                                Args = new List<PromotionArgType>() { PromotionArgType.HasRoughTerrainPenalty }
                            }
                        }
                    }
                }).SetName("Promotions with HasRoughTerrainPenalty reflected in info").Returns(new MovementInfo() {
                    HasRoughTerrainPenalty = true
                });

                yield return new TestCaseData(new GetMovementInfoTestData() {
                    Unit = new UnitTestData() {
                        Promotions = new List<PromotionTestData>() {
                            new PromotionTestData() {
                                Args = new List<PromotionArgType>() { PromotionArgType.IgnoresTerrainCosts }
                            }
                        }
                    }
                }).SetName("Promotions with IgnoresTerrainCosts reflected in info").Returns(new MovementInfo() {
                    IgnoresTerrainCosts = true
                });
            }
        }

        #endregion

        #region instance fields and properties

        

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            Container.Bind<PromotionParser>().AsSingle();
        }

        #endregion

        #region tests

        [TestCaseSource("GetCombatInfoTestCases")]
        [Test(Description = "")]
        public CombatInfo GetCombatInfoTests(GetCombatInfoTestData testData) {
            var attacker = BuildUnit(testData.Attacker);
            var defender = BuildUnit(testData.Defender);

            var location = BuildHexCell(testData.Location);

            var parser = Container.Resolve<PromotionParser>();

            return parser.GetCombatInfo(attacker, defender, location, testData.CombatType);
        }

        [TestCaseSource("GetMovementInfoTestCases")]
        [Test(Description = "")]
        public MovementInfo GetMovementInfoTests(GetMovementInfoTestData testData) {
            var unit = BuildUnit(testData.Unit);

            var parser = Container.Resolve<PromotionParser>();

            return parser.GetMovementInfo(unit);
        }

        #endregion

        #region utilities

        private IUnit BuildUnit(UnitTestData unitData) {
            var mockUnit = new Mock<IUnit>();

            mockUnit.Setup(unit => unit.Type).Returns(unitData.Type);

            mockUnit.Setup(unit => unit.Promotions).Returns(unitData.Promotions.Select(promotionData => BuildPromotion(promotionData)));

            return mockUnit.Object;
        }

        private IHexCell BuildHexCell(HexCellTestData cellData) {
            var mockCell = new Mock<IHexCell>();

            mockCell.Setup(cell => cell.IsRoughTerrain).Returns(cellData.IsRoughTerrain);
            mockCell.Setup(cell => cell.Feature)       .Returns(cellData.Feature);

            return mockCell.Object;
        }

        private IPromotion BuildPromotion(PromotionTestData promotionData) {
            var mockPromotion = new Mock<IPromotion>();

            foreach(var arg in promotionData.Args) {
                mockPromotion.Setup(promotion => promotion.HasArg(arg)).Returns(true);
            }

            mockPromotion.Setup(promotion => promotion.GetFloat()).Returns(promotionData.Float);
            mockPromotion.Setup(promotion => promotion.GetUnitType()).Returns(promotionData.UnitType);

            return mockPromotion.Object;
        }

        #endregion

        #endregion

    }

}
