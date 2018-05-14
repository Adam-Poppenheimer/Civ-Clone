using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation.HexMap;
using Assets.Simulation.Units;
using Assets.Simulation.Units.Combat;
using Assets.Simulation.Units.Promotions;

namespace Assets.Tests.Simulation.Units.Promotions {

    [TestFixture]
    public class CombatPromotionParserTests : ZenjectUnitTestFixture {

        #region internal types

        public class ParsePromotionTestData {

            public PromotionTestData Promotion = new PromotionTestData();

            public UnitTestData Attacker = new UnitTestData();

            public UnitTestData Defender = new UnitTestData();

            public HexCellTestData Location = new HexCellTestData();

            public CombatInfo CombatInfo = new CombatInfo();

        }

        public class PromotionTestData {

            public bool RestrictedByTerrains;
            public IEnumerable<TerrainType> ValidTerrains;

            public bool RestrictedByShapes;
            public IEnumerable<TerrainShape> ValidShapes;

            public bool RestrictedByFeatures;
            public IEnumerable<TerrainFeature> ValidFeatures;

            public bool RestrictedByAttackerTypes;
            public IEnumerable<UnitType> ValidAttackerTypes;

            public bool RestrictedByDefenderTypes;
            public IEnumerable<UnitType> ValidDefenderTypes;

            public bool RequiresFlatTerrain;
            public bool RequiresRoughTerrain;

            public bool RestrictedByCombatType;
            public CombatType ValidCombatType;

            public bool AppliesWhileAttacking;
            public bool AppliesWhileDefending;

            public float CombatModifier;

            public bool CanMoveAfterAttacking;
            public bool CanAttackAfterAttacking;
            public bool IgnoresAmphibiousPenalty;

            public bool IgnoresDefensiveTerrainBonuses;

            public float GoldRaidingPercentage;

            public bool IgnoresLOSWhenAttacking;

        }

        public class UnitTestData {

            public UnitType Type;

        }

        public class HexCellTestData {
            
            public TerrainType Terrain;

            public TerrainShape Shape;

            public TerrainFeature Feature;

            public bool IsRoughTerrain;

        }

        #endregion

        #region static fields and properties

        public static IEnumerable ParsePromotionForAttackerTestCases {
            get {
                yield return new TestCaseData(new ParsePromotionTestData() {
                    Promotion = new PromotionTestData() {
                        CombatModifier = 0.5f, AppliesWhileAttacking = true
                    }
                }).SetName("Promotion applies while attacking | adds combat modifiers to attacker.CombatModifier").Returns(new CombatInfo() {
                    Attacker = new UnitCombatInfo() { CombatModifier = 0.5f }
                });

                yield return new TestCaseData(new ParsePromotionTestData() {
                    Promotion = new PromotionTestData() {
                        CanMoveAfterAttacking = true, AppliesWhileAttacking = true
                    }
                }).SetName("Promotion applies while attacking | Sets attacker.CanMoveAfterAttacking").Returns(new CombatInfo() {
                    Attacker = new UnitCombatInfo() { CanMoveAfterAttacking = true }
                });

                yield return new TestCaseData(new ParsePromotionTestData() {
                    Promotion = new PromotionTestData() {
                        CanAttackAfterAttacking = true, AppliesWhileAttacking = true
                    }
                }).SetName("Promotion applies while attacking | Sets attacker.CanAttackAfterAttacking").Returns(new CombatInfo() {
                    Attacker = new UnitCombatInfo() { CanAttackAfterAttacking = true }
                });

                yield return new TestCaseData(new ParsePromotionTestData() {
                    Promotion = new PromotionTestData() {
                        IgnoresAmphibiousPenalty = true, AppliesWhileAttacking = true
                    }
                }).SetName("Promotion applies while attacking | Sets attacker.IgnoresAmphibiousPenalty").Returns(new CombatInfo() {
                    Attacker = new UnitCombatInfo() { IgnoresAmphibiousPenalty = true }
                });

                yield return new TestCaseData(new ParsePromotionTestData() {
                    Promotion = new PromotionTestData() {
                        IgnoresDefensiveTerrainBonuses = true, AppliesWhileAttacking = true
                    }
                }).SetName("Promotion applies while attacking | Sets attacker.IgnoresDefensiveTerrainBonuses").Returns(new CombatInfo() {
                    Attacker = new UnitCombatInfo() { IgnoresDefensiveTerrainBonuses = true }
                });

                yield return new TestCaseData(new ParsePromotionTestData() {
                    Promotion = new PromotionTestData() {
                        GoldRaidingPercentage = 0.5f, AppliesWhileAttacking = true
                    }
                }).SetName("Promotion applies while attacking | adds to attacker.GoldRaidingPercentage").Returns(new CombatInfo() {
                    Attacker = new UnitCombatInfo() { GoldRaidingPercentage = 0.5f }
                });



                yield return new TestCaseData(new ParsePromotionTestData() {
                    Promotion = new PromotionTestData() {
                        CombatModifier = 0.5f, AppliesWhileAttacking = false
                    }
                }).SetName("Promotion does not apply while attacking | attacker.CombatModifier unchanged").Returns(new CombatInfo() {
                    Attacker = new UnitCombatInfo() { CombatModifier = 0f }
                });

                yield return new TestCaseData(new ParsePromotionTestData() {
                    Promotion = new PromotionTestData() {
                        CanMoveAfterAttacking = true, AppliesWhileAttacking = false
                    }
                }).SetName("Promotion does not apply while attacking | attacker.CanMoveAfterAttacking unchanged").Returns(new CombatInfo() {
                    Attacker = new UnitCombatInfo() { CanMoveAfterAttacking = false }
                });

                yield return new TestCaseData(new ParsePromotionTestData() {
                    Promotion = new PromotionTestData() {
                        CanAttackAfterAttacking = true, AppliesWhileAttacking = false
                    }
                }).SetName("Promotion does not apply while attacking | attacker.CanAttackAfterAttacking unchanged").Returns(new CombatInfo() {
                    Attacker = new UnitCombatInfo() { CanAttackAfterAttacking = false }
                });

                yield return new TestCaseData(new ParsePromotionTestData() {
                    Promotion = new PromotionTestData() {
                        IgnoresAmphibiousPenalty = true, AppliesWhileAttacking = false
                    }
                }).SetName("Promotion does not apply while attacking | attacker.IgnoresAmphibiousPenalty unchanged").Returns(new CombatInfo() {
                    Attacker = new UnitCombatInfo() { IgnoresAmphibiousPenalty = false }
                });

                yield return new TestCaseData(new ParsePromotionTestData() {
                    Promotion = new PromotionTestData() {
                        IgnoresDefensiveTerrainBonuses = true, AppliesWhileAttacking = false
                    }
                }).SetName("Promotion does not apply while attacking | attacker.IgnoresDefensiveTerrainBonuses unchanged").Returns(new CombatInfo() {
                    Attacker = new UnitCombatInfo() { IgnoresDefensiveTerrainBonuses = false }
                });

                yield return new TestCaseData(new ParsePromotionTestData() {
                    Promotion = new PromotionTestData() {
                        GoldRaidingPercentage = 0.5f, AppliesWhileAttacking = false
                    }
                }).SetName("Promotion does not apply while attacking | attacker.GoldRaidingPercentage unchanged").Returns(new CombatInfo() {
                    Attacker = new UnitCombatInfo() { GoldRaidingPercentage = 0f }
                });



                yield return new TestCaseData(new ParsePromotionTestData() {
                    Promotion = new PromotionTestData() {
                        CombatModifier = 0.5f, AppliesWhileAttacking = true,
                        RestrictedByTerrains = true,
                        ValidTerrains = new List<TerrainType>() { TerrainType.Plains }
                    },
                    Location = new HexCellTestData() { Terrain = TerrainType.Grassland }
                }).SetName("Promotion restricted by terrains and location not a valid terrain | promotion ignored").Returns(new CombatInfo() {
                    Attacker = new UnitCombatInfo() { CombatModifier = 0f }
                });

                yield return new TestCaseData(new ParsePromotionTestData() {
                    Promotion = new PromotionTestData() {
                        CombatModifier = 0.5f, AppliesWhileAttacking = true,
                        RestrictedByTerrains = false,
                        ValidTerrains = new List<TerrainType>() { TerrainType.Plains }
                    },
                    Location = new HexCellTestData() { Terrain = TerrainType.Grassland }
                }).SetName("Promotion not restricted by terrains and location not a valid terrain | promotion applied").Returns(new CombatInfo() {
                    Attacker = new UnitCombatInfo() { CombatModifier = 0.5f }
                });

                yield return new TestCaseData(new ParsePromotionTestData() {
                    Promotion = new PromotionTestData() {
                        CombatModifier = 0.5f, AppliesWhileAttacking = true,
                        RestrictedByTerrains = true,
                        ValidTerrains = new List<TerrainType>() { TerrainType.Plains }
                    },
                    Location = new HexCellTestData() { Terrain = TerrainType.Plains }
                }).SetName("Promotion restricted by terrains and location a valid terrain | promotion applied").Returns(new CombatInfo() {
                    Attacker = new UnitCombatInfo() { CombatModifier = 0.5f }
                });

                yield return new TestCaseData(new ParsePromotionTestData() {
                    Promotion = new PromotionTestData() {
                        CombatModifier = 0.5f, AppliesWhileAttacking = true,
                        RestrictedByShapes = true,
                        ValidShapes = new List<TerrainShape>() { TerrainShape.Hills, TerrainShape.Mountains }
                    },
                    Location = new HexCellTestData() { Shape = TerrainShape.Flatlands }
                }).SetName("Promotion restricted by shapes and location not a valid shape | promotion ignored").Returns(new CombatInfo() {
                    Attacker = new UnitCombatInfo() { CombatModifier = 0f }
                });

                yield return new TestCaseData(new ParsePromotionTestData() {
                    Promotion = new PromotionTestData() {
                        CombatModifier = 0.5f, AppliesWhileAttacking = true,
                        RestrictedByShapes = false,
                        ValidShapes = new List<TerrainShape>() { TerrainShape.Hills, TerrainShape.Mountains }
                    },
                    Location = new HexCellTestData() { Shape = TerrainShape.Flatlands }
                }).SetName("Promotion not restricted by shapes and location not a valid shape | promotion applied").Returns(new CombatInfo() {
                    Attacker = new UnitCombatInfo() { CombatModifier = 0.5f }
                });

                yield return new TestCaseData(new ParsePromotionTestData() {
                    Promotion = new PromotionTestData() {
                        CombatModifier = 0.5f, AppliesWhileAttacking = true,
                        RestrictedByShapes = true,
                        ValidShapes = new List<TerrainShape>() { TerrainShape.Hills, TerrainShape.Mountains }
                    },
                    Location = new HexCellTestData() { Shape = TerrainShape.Mountains }
                }).SetName("Promotion restricted by shapes and location a valid shape | promotion applied").Returns(new CombatInfo() {
                    Attacker = new UnitCombatInfo() { CombatModifier = 0.5f }
                });

                yield return new TestCaseData(new ParsePromotionTestData() {
                    Promotion = new PromotionTestData() {
                        CombatModifier = 0.5f, AppliesWhileAttacking = true,
                        RestrictedByFeatures = true,
                        ValidFeatures = new List<TerrainFeature>() { TerrainFeature.Forest, TerrainFeature.Marsh }
                    },
                    Location = new HexCellTestData() { Feature = TerrainFeature.None }
                }).SetName("Promotion restricted by features and location not a valid feature | promotion ignored").Returns(new CombatInfo() {
                    Attacker = new UnitCombatInfo() { CombatModifier = 0f }
                });

                yield return new TestCaseData(new ParsePromotionTestData() {
                    Promotion = new PromotionTestData() {
                        CombatModifier = 0.5f, AppliesWhileAttacking = true,
                        RestrictedByFeatures = false,
                        ValidFeatures = new List<TerrainFeature>() { TerrainFeature.Forest, TerrainFeature.Marsh }
                    },
                    Location = new HexCellTestData() { Feature = TerrainFeature.None }
                }).SetName("Promotion not restricted by features and location not a valid feature | promotion applied").Returns(new CombatInfo() {
                    Attacker = new UnitCombatInfo() { CombatModifier = 0.5f }
                });

                yield return new TestCaseData(new ParsePromotionTestData() {
                    Promotion = new PromotionTestData() {
                        CombatModifier = 0.5f, AppliesWhileAttacking = true,
                        RestrictedByFeatures = true,
                        ValidFeatures = new List<TerrainFeature>() { TerrainFeature.Forest, TerrainFeature.Marsh }
                    },
                    Location = new HexCellTestData() { Feature = TerrainFeature.Marsh }
                }).SetName("Promotion restricted by features and location a valid feature | promotion applied").Returns(new CombatInfo() {
                    Attacker = new UnitCombatInfo() { CombatModifier = 0.5f }
                });

                yield return new TestCaseData(new ParsePromotionTestData() {
                    Promotion = new PromotionTestData() {
                        CombatModifier = 0.5f, AppliesWhileAttacking = true,
                        RestrictedByAttackerTypes = true,
                        ValidAttackerTypes = new List<UnitType>() { UnitType.Melee, UnitType.NavalMelee }
                    },
                    Attacker = new UnitTestData() { Type = UnitType.Archery }
                }).SetName("Promotion restricted by attacker types and attacker wrong type | promotion ignored").Returns(new CombatInfo() {
                    Attacker = new UnitCombatInfo() { CombatModifier = 0f }
                });

                yield return new TestCaseData(new ParsePromotionTestData() {
                    Promotion = new PromotionTestData() {
                        CombatModifier = 0.5f, AppliesWhileAttacking = true,
                        RestrictedByAttackerTypes = false,
                        ValidAttackerTypes = new List<UnitType>() { UnitType.Melee, UnitType.NavalMelee }
                    },
                    Attacker = new UnitTestData() { Type = UnitType.Archery }
                }).SetName("Promotion not restricted by attacker types and attacker wrong type | promotion applied").Returns(new CombatInfo() {
                    Attacker = new UnitCombatInfo() { CombatModifier = 0.5f }
                });

                yield return new TestCaseData(new ParsePromotionTestData() {
                    Promotion = new PromotionTestData() {
                        CombatModifier = 0.5f, AppliesWhileAttacking = true,
                        RestrictedByAttackerTypes = true,
                        ValidAttackerTypes = new List<UnitType>() { UnitType.Melee, UnitType.NavalMelee }
                    },
                    Attacker = new UnitTestData() { Type = UnitType.Melee }
                }).SetName("Promotion restricted by attacker types and attacker valid type | promotion applied").Returns(new CombatInfo() {
                    Attacker = new UnitCombatInfo() { CombatModifier = 0.5f }
                });

                yield return new TestCaseData(new ParsePromotionTestData() {
                    Promotion = new PromotionTestData() {
                        CombatModifier = 0.5f, AppliesWhileAttacking = true,
                        RestrictedByDefenderTypes = true,
                        ValidDefenderTypes = new List<UnitType>() { UnitType.Melee, UnitType.NavalMelee }
                    },
                    Defender = new UnitTestData() { Type = UnitType.Archery }
                }).SetName("Promotion restricted by defender types and defender wrong type | promotion ignored").Returns(new CombatInfo() {
                    Attacker = new UnitCombatInfo() { CombatModifier = 0f }
                });

                yield return new TestCaseData(new ParsePromotionTestData() {
                    Promotion = new PromotionTestData() {
                        CombatModifier = 0.5f, AppliesWhileAttacking = true,
                        RestrictedByDefenderTypes = false,
                        ValidDefenderTypes = new List<UnitType>() { UnitType.Melee, UnitType.NavalMelee }
                    },
                    Defender = new UnitTestData() { Type = UnitType.Archery }
                }).SetName("Promotion not restricted by defender types and defender wrong type | promotion applied").Returns(new CombatInfo() {
                    Attacker = new UnitCombatInfo() { CombatModifier = 0.5f }
                });

                yield return new TestCaseData(new ParsePromotionTestData() {
                    Promotion = new PromotionTestData() {
                        CombatModifier = 0.5f, AppliesWhileAttacking = true,
                        RestrictedByDefenderTypes = true,
                        ValidDefenderTypes = new List<UnitType>() { UnitType.Melee, UnitType.NavalMelee }
                    },
                    Defender = new UnitTestData() { Type = UnitType.Melee }
                }).SetName("Promotion restricted by defender types and defender valid type | promotion applied").Returns(new CombatInfo() {
                    Attacker = new UnitCombatInfo() { CombatModifier = 0.5f }
                });

                yield return new TestCaseData(new ParsePromotionTestData() {
                    Promotion = new PromotionTestData() {
                        CombatModifier = 0.5f, AppliesWhileAttacking = true,
                        RequiresFlatTerrain = true
                    },
                    Location = new HexCellTestData() { IsRoughTerrain = false }
                }).SetName("Promotion requires flat terrain and location flat | promotion applied").Returns(new CombatInfo() {
                    Attacker = new UnitCombatInfo() { CombatModifier = 0.5f }
                });

                yield return new TestCaseData(new ParsePromotionTestData() {
                    Promotion = new PromotionTestData() {
                        CombatModifier = 0.5f, AppliesWhileAttacking = true,
                        RequiresFlatTerrain = true
                    },
                    Location = new HexCellTestData() { IsRoughTerrain = true }
                }).SetName("Promotion requires flat terrain and location rough | promotion ignored").Returns(new CombatInfo() {
                    Attacker = new UnitCombatInfo() { CombatModifier = 0f }
                });

                yield return new TestCaseData(new ParsePromotionTestData() {
                    Promotion = new PromotionTestData() {
                        CombatModifier = 0.5f, AppliesWhileAttacking = true,
                        RequiresRoughTerrain = true
                    },
                    Location = new HexCellTestData() { IsRoughTerrain = true }
                }).SetName("Promotion requires rough terrain and location rough | promotion applied").Returns(new CombatInfo() {
                    Attacker = new UnitCombatInfo() { CombatModifier = 0.5f }
                });

                yield return new TestCaseData(new ParsePromotionTestData() {
                    Promotion = new PromotionTestData() {
                        CombatModifier = 0.5f, AppliesWhileAttacking = true,
                        RequiresRoughTerrain = true
                    },
                    Location = new HexCellTestData() { IsRoughTerrain = false }
                }).SetName("Promotion requires rough terrain and location flat | promotion ignored").Returns(new CombatInfo() {
                    Attacker = new UnitCombatInfo() { CombatModifier = 0f }
                });

                yield return new TestCaseData(new ParsePromotionTestData() {
                    Promotion = new PromotionTestData() {
                        CombatModifier = 0.5f, AppliesWhileAttacking = true,
                        RestrictedByCombatType = true,
                        ValidCombatType = CombatType.Ranged
                    },
                    CombatInfo = new CombatInfo() { CombatType = CombatType.Melee }
                }).SetName("Promotion restricted by combat types and combat wrong type | promotion ignored").Returns(new CombatInfo() {
                    Attacker = new UnitCombatInfo() { CombatModifier = 0f }
                });

                yield return new TestCaseData(new ParsePromotionTestData() {
                    Promotion = new PromotionTestData() {
                        CombatModifier = 0.5f, AppliesWhileAttacking = true,
                        RestrictedByCombatType = false,
                        ValidCombatType = CombatType.Ranged
                    },
                    CombatInfo = new CombatInfo() { CombatType = CombatType.Melee }
                }).SetName("Promotion not restricted by combat types and combat wrong type | promotion applied").Returns(new CombatInfo() {
                    Attacker = new UnitCombatInfo() { CombatModifier = 0.5f }
                });

                yield return new TestCaseData(new ParsePromotionTestData() {
                    Promotion = new PromotionTestData() {
                        CombatModifier = 0.5f, AppliesWhileAttacking = true,
                        RestrictedByCombatType = true,
                        ValidCombatType = CombatType.Ranged
                    },
                    CombatInfo = new CombatInfo() { CombatType = CombatType.Ranged }
                }).SetName("Promotion restricted by combat types and combat valid type | promotion applied").Returns(new CombatInfo() {
                    CombatType = CombatType.Ranged,
                    Attacker = new UnitCombatInfo() { CombatModifier = 0.5f }
                });
            }
        }

        public static IEnumerable ParsePromotionForDefenderTestCases {
            get {
                yield return new TestCaseData(new ParsePromotionTestData() {
                    Promotion = new PromotionTestData() {
                        CombatModifier = 0.5f, AppliesWhileDefending = true
                    }
                }).SetName("Promotion applies while defending | adds combat modifiers to defender.CombatModifier").Returns(new CombatInfo() {
                    Defender = new UnitCombatInfo() { CombatModifier = 0.5f }
                });

                yield return new TestCaseData(new ParsePromotionTestData() {
                    Promotion = new PromotionTestData() {
                        CanMoveAfterAttacking = true, AppliesWhileDefending = true
                    }
                }).SetName("Promotion applies while defending | Sets defender.CanMoveAfterAttacking").Returns(new CombatInfo() {
                    Defender = new UnitCombatInfo() { CanMoveAfterAttacking = true }
                });

                yield return new TestCaseData(new ParsePromotionTestData() {
                    Promotion = new PromotionTestData() {
                        CanAttackAfterAttacking = true, AppliesWhileDefending = true
                    }
                }).SetName("Promotion applies while defending | Sets defender.CanAttackAfterAttacking").Returns(new CombatInfo() {
                    Defender = new UnitCombatInfo() { CanAttackAfterAttacking = true }
                });

                yield return new TestCaseData(new ParsePromotionTestData() {
                    Promotion = new PromotionTestData() {
                        IgnoresAmphibiousPenalty = true, AppliesWhileDefending = true
                    }
                }).SetName("Promotion applies while defending | Sets defender.IgnoresAmphibiousPenalty").Returns(new CombatInfo() {
                    Defender = new UnitCombatInfo() { IgnoresAmphibiousPenalty = true }
                });

                yield return new TestCaseData(new ParsePromotionTestData() {
                    Promotion = new PromotionTestData() {
                        IgnoresDefensiveTerrainBonuses = true, AppliesWhileDefending = true
                    }
                }).SetName("Promotion applies while defending | Sets defender.IgnoresDefensiveTerrainBonuses").Returns(new CombatInfo() {
                    Defender = new UnitCombatInfo() { IgnoresDefensiveTerrainBonuses = true }
                });

                yield return new TestCaseData(new ParsePromotionTestData() {
                    Promotion = new PromotionTestData() {
                        GoldRaidingPercentage = 0.5f, AppliesWhileDefending = true
                    }
                }).SetName("Promotion applies while defending | adds to defender.GoldRaidingPercentage").Returns(new CombatInfo() {
                    Defender = new UnitCombatInfo() { GoldRaidingPercentage = 0.5f }
                });



                yield return new TestCaseData(new ParsePromotionTestData() {
                    Promotion = new PromotionTestData() {
                        CombatModifier = 0.5f, AppliesWhileDefending = false
                    }
                }).SetName("Promotion does not apply while defending | defender.CombatModifier unchanged").Returns(new CombatInfo() {
                    Defender = new UnitCombatInfo() { CombatModifier = 0f }
                });

                yield return new TestCaseData(new ParsePromotionTestData() {
                    Promotion = new PromotionTestData() {
                        CanMoveAfterAttacking = true, AppliesWhileDefending = false
                    }
                }).SetName("Promotion does not apply while defending | defender.CanMoveAfterAttacking unchanged").Returns(new CombatInfo() {
                    Defender = new UnitCombatInfo() { CanMoveAfterAttacking = false }
                });

                yield return new TestCaseData(new ParsePromotionTestData() {
                    Promotion = new PromotionTestData() {
                        CanAttackAfterAttacking = true, AppliesWhileDefending = false
                    }
                }).SetName("Promotion does not apply while defending | defender.CanAttackAfterAttacking unchanged").Returns(new CombatInfo() {
                    Defender = new UnitCombatInfo() { CanAttackAfterAttacking = false }
                });

                yield return new TestCaseData(new ParsePromotionTestData() {
                    Promotion = new PromotionTestData() {
                        IgnoresAmphibiousPenalty = true, AppliesWhileDefending = false
                    }
                }).SetName("Promotion does not apply while defending | defender.IgnoresAmphibiousPenalty unchanged").Returns(new CombatInfo() {
                    Defender = new UnitCombatInfo() { IgnoresAmphibiousPenalty = false }
                });

                yield return new TestCaseData(new ParsePromotionTestData() {
                    Promotion = new PromotionTestData() {
                        IgnoresDefensiveTerrainBonuses = true, AppliesWhileDefending = false
                    }
                }).SetName("Promotion does not apply while defending | defender.IgnoresDefensiveTerrainBonuses unchanged").Returns(new CombatInfo() {
                    Defender = new UnitCombatInfo() { IgnoresDefensiveTerrainBonuses = false }
                });

                yield return new TestCaseData(new ParsePromotionTestData() {
                    Promotion = new PromotionTestData() {
                        GoldRaidingPercentage = 0.5f, AppliesWhileDefending = false
                    }
                }).SetName("Promotion does not apply while defending | defender.GoldRaidingPercentage unchanged").Returns(new CombatInfo() {
                    Defender = new UnitCombatInfo() { GoldRaidingPercentage = 0f }
                });

                yield return new TestCaseData(new ParsePromotionTestData() {
                    Promotion = new PromotionTestData() {
                        CombatModifier = 0.5f, AppliesWhileDefending = true,
                        RestrictedByTerrains = true,
                        ValidTerrains = new List<TerrainType>() { TerrainType.Plains }
                    },
                    Location = new HexCellTestData() { Terrain = TerrainType.Grassland }
                }).SetName("Promotion restricted by terrains and location not a valid terrain | promotion ignored").Returns(new CombatInfo() {
                    Defender = new UnitCombatInfo() { CombatModifier = 0f }
                });

                yield return new TestCaseData(new ParsePromotionTestData() {
                    Promotion = new PromotionTestData() {
                        CombatModifier = 0.5f, AppliesWhileDefending = true,
                        RestrictedByTerrains = false,
                        ValidTerrains = new List<TerrainType>() { TerrainType.Plains }
                    },
                    Location = new HexCellTestData() { Terrain = TerrainType.Grassland }
                }).SetName("Promotion not restricted by terrains and location not a valid terrain | promotion applied").Returns(new CombatInfo() {
                    Defender = new UnitCombatInfo() { CombatModifier = 0.5f }
                });

                yield return new TestCaseData(new ParsePromotionTestData() {
                    Promotion = new PromotionTestData() {
                        CombatModifier = 0.5f, AppliesWhileDefending = true,
                        RestrictedByTerrains = true,
                        ValidTerrains = new List<TerrainType>() { TerrainType.Plains }
                    },
                    Location = new HexCellTestData() { Terrain = TerrainType.Plains }
                }).SetName("Promotion restricted by terrains and location a valid terrain | promotion applied").Returns(new CombatInfo() {
                    Defender = new UnitCombatInfo() { CombatModifier = 0.5f }
                });

                yield return new TestCaseData(new ParsePromotionTestData() {
                    Promotion = new PromotionTestData() {
                        CombatModifier = 0.5f, AppliesWhileDefending = true,
                        RestrictedByShapes = true,
                        ValidShapes = new List<TerrainShape>() { TerrainShape.Hills, TerrainShape.Mountains }
                    },
                    Location = new HexCellTestData() { Shape = TerrainShape.Flatlands }
                }).SetName("Promotion restricted by shapes and location not a valid shape | promotion ignored").Returns(new CombatInfo() {
                    Defender = new UnitCombatInfo() { CombatModifier = 0f }
                });

                yield return new TestCaseData(new ParsePromotionTestData() {
                    Promotion = new PromotionTestData() {
                        CombatModifier = 0.5f, AppliesWhileDefending = true,
                        RestrictedByShapes = false,
                        ValidShapes = new List<TerrainShape>() { TerrainShape.Hills, TerrainShape.Mountains }
                    },
                    Location = new HexCellTestData() { Shape = TerrainShape.Flatlands }
                }).SetName("Promotion not restricted by shapes and location not a valid shapes | promotion applied").Returns(new CombatInfo() {
                    Defender = new UnitCombatInfo() { CombatModifier = 0.5f }
                });

                yield return new TestCaseData(new ParsePromotionTestData() {
                    Promotion = new PromotionTestData() {
                        CombatModifier = 0.5f, AppliesWhileDefending = true,
                        RestrictedByShapes = true,
                        ValidShapes = new List<TerrainShape>() { TerrainShape.Hills, TerrainShape.Mountains }
                    },
                    Location = new HexCellTestData() { Shape = TerrainShape.Mountains }
                }).SetName("Promotion restricted by shapes and location a valid shape | promotion applied").Returns(new CombatInfo() {
                    Defender = new UnitCombatInfo() { CombatModifier = 0.5f }
                });

                yield return new TestCaseData(new ParsePromotionTestData() {
                    Promotion = new PromotionTestData() {
                        CombatModifier = 0.5f, AppliesWhileDefending = true,
                        RestrictedByFeatures = true,
                        ValidFeatures = new List<TerrainFeature>() { TerrainFeature.Forest, TerrainFeature.Marsh }
                    },
                    Location = new HexCellTestData() { Feature = TerrainFeature.None }
                }).SetName("Promotion restricted by features and location not a valid feature | promotion ignored").Returns(new CombatInfo() {
                    Defender = new UnitCombatInfo() { CombatModifier = 0f }
                });

                yield return new TestCaseData(new ParsePromotionTestData() {
                    Promotion = new PromotionTestData() {
                        CombatModifier = 0.5f, AppliesWhileDefending = true,
                        RestrictedByFeatures = false,
                        ValidFeatures = new List<TerrainFeature>() { TerrainFeature.Forest, TerrainFeature.Marsh }
                    },
                    Location = new HexCellTestData() { Feature = TerrainFeature.None }
                }).SetName("Promotion not restricted by features and location not a valid feature | promotion applied").Returns(new CombatInfo() {
                    Defender = new UnitCombatInfo() { CombatModifier = 0.5f }
                });

                yield return new TestCaseData(new ParsePromotionTestData() {
                    Promotion = new PromotionTestData() {
                        CombatModifier = 0.5f, AppliesWhileDefending = true,
                        RestrictedByFeatures = true,
                        ValidFeatures = new List<TerrainFeature>() { TerrainFeature.Forest, TerrainFeature.Marsh }
                    },
                    Location = new HexCellTestData() { Feature = TerrainFeature.Marsh }
                }).SetName("Promotion restricted by features and location a valid feature | promotion applied").Returns(new CombatInfo() {
                    Defender = new UnitCombatInfo() { CombatModifier = 0.5f }
                });

                yield return new TestCaseData(new ParsePromotionTestData() {
                    Promotion = new PromotionTestData() {
                        CombatModifier = 0.5f, AppliesWhileDefending = true,
                        RestrictedByAttackerTypes = true,
                        ValidAttackerTypes = new List<UnitType>() { UnitType.Melee, UnitType.NavalMelee }
                    },
                    Attacker = new UnitTestData() { Type = UnitType.Archery }
                }).SetName("Promotion restricted by attacker types and attacker wrong type | promotion ignored").Returns(new CombatInfo() {
                    Defender = new UnitCombatInfo() { CombatModifier = 0f }
                });

                yield return new TestCaseData(new ParsePromotionTestData() {
                    Promotion = new PromotionTestData() {
                        CombatModifier = 0.5f, AppliesWhileDefending = true,
                        RestrictedByAttackerTypes = false,
                        ValidAttackerTypes = new List<UnitType>() { UnitType.Melee, UnitType.NavalMelee }
                    },
                    Attacker = new UnitTestData() { Type = UnitType.Archery }
                }).SetName("Promotion not restricted by attacker types and attacker wrong type | promotion applied").Returns(new CombatInfo() {
                    Defender = new UnitCombatInfo() { CombatModifier = 0.5f }
                });

                yield return new TestCaseData(new ParsePromotionTestData() {
                    Promotion = new PromotionTestData() {
                        CombatModifier = 0.5f, AppliesWhileDefending = true,
                        RestrictedByAttackerTypes = true,
                        ValidAttackerTypes = new List<UnitType>() { UnitType.Melee, UnitType.NavalMelee }
                    },
                    Attacker = new UnitTestData() { Type = UnitType.Melee }
                }).SetName("Promotion restricted by attacker types and attacker valid type | promotion applied").Returns(new CombatInfo() {
                    Defender = new UnitCombatInfo() { CombatModifier = 0.5f }
                });

                yield return new TestCaseData(new ParsePromotionTestData() {
                    Promotion = new PromotionTestData() {
                        CombatModifier = 0.5f, AppliesWhileDefending = true,
                        RestrictedByDefenderTypes = true,
                        ValidDefenderTypes = new List<UnitType>() { UnitType.Melee, UnitType.NavalMelee }
                    },
                    Defender = new UnitTestData() { Type = UnitType.Archery }
                }).SetName("Promotion restricted by defender types and defender wrong type | promotion ignored").Returns(new CombatInfo() {
                    Defender = new UnitCombatInfo() { CombatModifier = 0f }
                });

                yield return new TestCaseData(new ParsePromotionTestData() {
                    Promotion = new PromotionTestData() {
                        CombatModifier = 0.5f, AppliesWhileDefending = true,
                        RestrictedByDefenderTypes = false,
                        ValidDefenderTypes = new List<UnitType>() { UnitType.Melee, UnitType.NavalMelee }
                    },
                    Defender = new UnitTestData() { Type = UnitType.Archery }
                }).SetName("Promotion not restricted by defender types and defender wrong type | promotion applied").Returns(new CombatInfo() {
                    Defender = new UnitCombatInfo() { CombatModifier = 0.5f }
                });

                yield return new TestCaseData(new ParsePromotionTestData() {
                    Promotion = new PromotionTestData() {
                        CombatModifier = 0.5f, AppliesWhileDefending = true,
                        RestrictedByDefenderTypes = true,
                        ValidDefenderTypes = new List<UnitType>() { UnitType.Melee, UnitType.NavalMelee }
                    },
                    Defender = new UnitTestData() { Type = UnitType.Melee }
                }).SetName("Promotion restricted by defender types and defender valid type | promotion applied").Returns(new CombatInfo() {
                    Defender = new UnitCombatInfo() { CombatModifier = 0.5f }
                });

                yield return new TestCaseData(new ParsePromotionTestData() {
                    Promotion = new PromotionTestData() {
                        CombatModifier = 0.5f, AppliesWhileDefending = true,
                        RequiresFlatTerrain = true
                    },
                    Location = new HexCellTestData() { IsRoughTerrain = false }
                }).SetName("Promotion requires flat terrain and location flat | promotion applied").Returns(new CombatInfo() {
                    Defender = new UnitCombatInfo() { CombatModifier = 0.5f }
                });

                yield return new TestCaseData(new ParsePromotionTestData() {
                    Promotion = new PromotionTestData() {
                        CombatModifier = 0.5f, AppliesWhileDefending = true,
                        RequiresFlatTerrain = true
                    },
                    Location = new HexCellTestData() { IsRoughTerrain = true }
                }).SetName("Promotion requires flat terrain and location rough | promotion ignored").Returns(new CombatInfo() {
                    Defender = new UnitCombatInfo() { CombatModifier = 0f }
                });

                yield return new TestCaseData(new ParsePromotionTestData() {
                    Promotion = new PromotionTestData() {
                        CombatModifier = 0.5f, AppliesWhileDefending = true,
                        RequiresRoughTerrain = true
                    },
                    Location = new HexCellTestData() { IsRoughTerrain = true }
                }).SetName("Promotion requires rough terrain and location rough | promotion applied").Returns(new CombatInfo() {
                    Defender = new UnitCombatInfo() { CombatModifier = 0.5f }
                });

                yield return new TestCaseData(new ParsePromotionTestData() {
                    Promotion = new PromotionTestData() {
                        CombatModifier = 0.5f, AppliesWhileDefending = true,
                        RequiresRoughTerrain = true
                    },
                    Location = new HexCellTestData() { IsRoughTerrain = false }
                }).SetName("Promotion requires rough terrain and location flat | promotion ignored").Returns(new CombatInfo() {
                    Defender = new UnitCombatInfo() { CombatModifier = 0f }
                });

                yield return new TestCaseData(new ParsePromotionTestData() {
                    Promotion = new PromotionTestData() {
                        CombatModifier = 0.5f, AppliesWhileDefending = true,
                        RestrictedByCombatType = true,
                        ValidCombatType = CombatType.Ranged
                    },
                    CombatInfo = new CombatInfo() { CombatType = CombatType.Melee }
                }).SetName("Promotion restricted by combat types and combat wrong type | promotion ignored").Returns(new CombatInfo() {
                    Defender = new UnitCombatInfo() { CombatModifier = 0f }
                });

                yield return new TestCaseData(new ParsePromotionTestData() {
                    Promotion = new PromotionTestData() {
                        CombatModifier = 0.5f, AppliesWhileDefending = true,
                        RestrictedByCombatType = false,
                        ValidCombatType = CombatType.Ranged
                    },
                    CombatInfo = new CombatInfo() { CombatType = CombatType.Melee }
                }).SetName("Promotion not restricted by combat types and combat wrong type | promotion applied").Returns(new CombatInfo() {
                    Defender = new UnitCombatInfo() { CombatModifier = 0.5f }
                });

                yield return new TestCaseData(new ParsePromotionTestData() {
                    Promotion = new PromotionTestData() {
                        CombatModifier = 0.5f, AppliesWhileDefending = true,
                        RestrictedByCombatType = true,
                        ValidCombatType = CombatType.Ranged
                    },
                    CombatInfo = new CombatInfo() { CombatType = CombatType.Ranged }
                }).SetName("Promotion restricted by combat types and combat valid type | promotion applied").Returns(new CombatInfo() {
                    CombatType = CombatType.Ranged,
                    Defender = new UnitCombatInfo() { CombatModifier = 0.5f }
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
            Container.Bind<CombatPromotionParser>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        [TestCaseSource("ParsePromotionForAttackerTestCases")]
        public CombatInfo ParsePromotionForAttackerTests(ParsePromotionTestData testData) {
            var promotion = BuildPromotion(testData.Promotion);

            var attacker = BuildUnit(testData.Attacker);
            var defender = BuildUnit(testData.Defender);

            var location = BuildHexCell(testData.Location);

            var parser = Container.Resolve<CombatPromotionParser>();

            parser.ParsePromotionForAttacker(promotion, attacker, defender, location, testData.CombatInfo);

            return testData.CombatInfo;
        }

        [Test]
        [TestCaseSource("ParsePromotionForDefenderTestCases")]
        public CombatInfo ParsePromotionForDefenderTests(ParsePromotionTestData testData) {
            var promotion = BuildPromotion(testData.Promotion);

            var attacker = BuildUnit(testData.Attacker);
            var defender = BuildUnit(testData.Defender);

            var location = BuildHexCell(testData.Location);

            var parser = Container.Resolve<CombatPromotionParser>();

            parser.ParsePromotionForDefender(promotion, attacker, defender, location, testData.CombatInfo);

            return testData.CombatInfo;
        }

        #endregion

        #region utilities

        private IHexCell BuildHexCell(HexCellTestData cellData) {
            var mockCell = new Mock<IHexCell>();

            mockCell.Setup(cell => cell.Terrain)       .Returns(cellData.Terrain);
            mockCell.Setup(cell => cell.Shape)         .Returns(cellData.Shape);
            mockCell.Setup(cell => cell.Feature)       .Returns(cellData.Feature);
            mockCell.Setup(cell => cell.IsRoughTerrain).Returns(cellData.IsRoughTerrain);

            return mockCell.Object;
        }

        private IUnit BuildUnit(UnitTestData unitData) {
            var mockUnit = new Mock<IUnit>();

            mockUnit.Setup(unit => unit.Type).Returns(unitData.Type);

            return mockUnit.Object;
        }

        private IPromotion BuildPromotion(PromotionTestData promotionData) {
            var mockPromotion = new Mock<IPromotion>();

            mockPromotion.Setup(promotion => promotion.RestrictedByTerrains).Returns(promotionData.RestrictedByTerrains);
            mockPromotion.Setup(promotion => promotion.ValidTerrains)       .Returns(promotionData.ValidTerrains);

            mockPromotion.Setup(promotion => promotion.RestrictedByShapes).Returns(promotionData.RestrictedByShapes);
            mockPromotion.Setup(promotion => promotion.ValidShapes)       .Returns(promotionData.ValidShapes);

            mockPromotion.Setup(promotion => promotion.RestrictedByFeatures).Returns(promotionData.RestrictedByFeatures);
            mockPromotion.Setup(promotion => promotion.ValidFeatures)       .Returns(promotionData.ValidFeatures);

            mockPromotion.Setup(promotion => promotion.RestrictedByAttackerTypes).Returns(promotionData.RestrictedByAttackerTypes);
            mockPromotion.Setup(promotion => promotion.ValidAttackerTypes)       .Returns(promotionData.ValidAttackerTypes);

            mockPromotion.Setup(promotion => promotion.RestrictedByDefenderTypes).Returns(promotionData.RestrictedByDefenderTypes);
            mockPromotion.Setup(promotion => promotion.ValidDefenderTypes)       .Returns(promotionData.ValidDefenderTypes);

            mockPromotion.Setup(promotion => promotion.RequiresFlatTerrain) .Returns(promotionData.RequiresFlatTerrain);
            mockPromotion.Setup(promotion => promotion.RequiresRoughTerrain).Returns(promotionData.RequiresRoughTerrain);

            mockPromotion.Setup(promotion => promotion.RestrictedByCombatType).Returns(promotionData.RestrictedByCombatType);
            mockPromotion.Setup(promotion => promotion.ValidCombatType)       .Returns(promotionData.ValidCombatType);

            mockPromotion.Setup(promotion => promotion.AppliesWhileAttacking).Returns(promotionData.AppliesWhileAttacking);
            mockPromotion.Setup(promotion => promotion.AppliesWhileDefending).Returns(promotionData.AppliesWhileDefending);

            mockPromotion.Setup(promotion => promotion.CombatModifier).Returns(promotionData.CombatModifier);

            mockPromotion.Setup(promotion => promotion.CanMoveAfterAttacking)   .Returns(promotionData.CanMoveAfterAttacking);
            mockPromotion.Setup(promotion => promotion.CanAttackAfterAttacking) .Returns(promotionData.CanAttackAfterAttacking);
            mockPromotion.Setup(promotion => promotion.IgnoresAmphibiousPenalty).Returns(promotionData.IgnoresAmphibiousPenalty);

            mockPromotion.Setup(promotion => promotion.IgnoresDefensiveTerrainBonuses).Returns(promotionData.IgnoresDefensiveTerrainBonuses);

            mockPromotion.Setup(promotion => promotion.GoldRaidingPercentage).Returns(promotionData.GoldRaidingPercentage);

            mockPromotion.Setup(promotion => promotion.IgnoresLOSWhenAttacking).Returns(promotionData.IgnoresLOSWhenAttacking);

            return mockPromotion.Object;
        }

        #endregion

        #endregion

    }

}
