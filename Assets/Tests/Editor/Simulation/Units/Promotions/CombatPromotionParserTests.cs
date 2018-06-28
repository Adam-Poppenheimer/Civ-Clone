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
            public IEnumerable<CellTerrain> ValidTerrains;

            public bool RestrictedByShapes;
            public IEnumerable<CellShape> ValidShapes;

            public bool RestrictedByVegetation;
            public IEnumerable<CellVegetation> ValidVegetation;

            public bool RestrictedByOpponentTypes;
            public IEnumerable<UnitType> ValidOpponentTypes;

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

            public bool IgnoresLineOfSight;

            public bool RestrictedByOpponentWoundedState;
            public bool ValidOpponentWoundedState;

        }

        public class UnitTestData {

            public UnitType Type;

            public bool IsWounded;

        }

        public class HexCellTestData {
            
            public CellTerrain Terrain;

            public CellShape Shape;

            public CellVegetation Vegetation;

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
                        IgnoresLineOfSight = true, AppliesWhileAttacking = true
                    }
                }).SetName("Promotion applies while attacking | Sets attacker.IgnoresLineOfSight").Returns(new CombatInfo() {
                    Attacker = new UnitCombatInfo() { IgnoresLineOfSight = true }
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
                        IgnoresLineOfSight = true, AppliesWhileAttacking = false
                    }
                }).SetName("Promotion does not apply while attacking | attacker.IgnoresLineOfSight unchanged").Returns(new CombatInfo() {
                    Attacker = new UnitCombatInfo() { IgnoresLineOfSight = false }
                });



                yield return new TestCaseData(new ParsePromotionTestData() {
                    Promotion = new PromotionTestData() {
                        CombatModifier = 0.5f, AppliesWhileAttacking = true,
                        RestrictedByTerrains = true,
                        ValidTerrains = new List<CellTerrain>() { CellTerrain.Plains }
                    },
                    Location = new HexCellTestData() { Terrain = CellTerrain.Grassland }
                }).SetName("Promotion restricted by terrains and location not a valid terrain | promotion ignored").Returns(new CombatInfo() {
                    Attacker = new UnitCombatInfo() { CombatModifier = 0f }
                });

                yield return new TestCaseData(new ParsePromotionTestData() {
                    Promotion = new PromotionTestData() {
                        CombatModifier = 0.5f, AppliesWhileAttacking = true,
                        RestrictedByTerrains = false,
                        ValidTerrains = new List<CellTerrain>() { CellTerrain.Plains }
                    },
                    Location = new HexCellTestData() { Terrain = CellTerrain.Grassland }
                }).SetName("Promotion not restricted by terrains and location not a valid terrain | promotion applied").Returns(new CombatInfo() {
                    Attacker = new UnitCombatInfo() { CombatModifier = 0.5f }
                });

                yield return new TestCaseData(new ParsePromotionTestData() {
                    Promotion = new PromotionTestData() {
                        CombatModifier = 0.5f, AppliesWhileAttacking = true,
                        RestrictedByTerrains = true,
                        ValidTerrains = new List<CellTerrain>() { CellTerrain.Plains }
                    },
                    Location = new HexCellTestData() { Terrain = CellTerrain.Plains }
                }).SetName("Promotion restricted by terrains and location a valid terrain | promotion applied").Returns(new CombatInfo() {
                    Attacker = new UnitCombatInfo() { CombatModifier = 0.5f }
                });

                yield return new TestCaseData(new ParsePromotionTestData() {
                    Promotion = new PromotionTestData() {
                        CombatModifier = 0.5f, AppliesWhileAttacking = true,
                        RestrictedByShapes = true,
                        ValidShapes = new List<CellShape>() { CellShape.Hills, CellShape.Mountains }
                    },
                    Location = new HexCellTestData() { Shape = CellShape.Flatlands }
                }).SetName("Promotion restricted by shapes and location not a valid shape | promotion ignored").Returns(new CombatInfo() {
                    Attacker = new UnitCombatInfo() { CombatModifier = 0f }
                });

                yield return new TestCaseData(new ParsePromotionTestData() {
                    Promotion = new PromotionTestData() {
                        CombatModifier = 0.5f, AppliesWhileAttacking = true,
                        RestrictedByShapes = false,
                        ValidShapes = new List<CellShape>() { CellShape.Hills, CellShape.Mountains }
                    },
                    Location = new HexCellTestData() { Shape = CellShape.Flatlands }
                }).SetName("Promotion not restricted by shapes and location not a valid shape | promotion applied").Returns(new CombatInfo() {
                    Attacker = new UnitCombatInfo() { CombatModifier = 0.5f }
                });

                yield return new TestCaseData(new ParsePromotionTestData() {
                    Promotion = new PromotionTestData() {
                        CombatModifier = 0.5f, AppliesWhileAttacking = true,
                        RestrictedByShapes = true,
                        ValidShapes = new List<CellShape>() { CellShape.Hills, CellShape.Mountains }
                    },
                    Location = new HexCellTestData() { Shape = CellShape.Mountains }
                }).SetName("Promotion restricted by shapes and location a valid shape | promotion applied").Returns(new CombatInfo() {
                    Attacker = new UnitCombatInfo() { CombatModifier = 0.5f }
                });

                yield return new TestCaseData(new ParsePromotionTestData() {
                    Promotion = new PromotionTestData() {
                        CombatModifier = 0.5f, AppliesWhileAttacking = true,
                        RestrictedByVegetation = true,
                        ValidVegetation = new List<CellVegetation>() { CellVegetation.Forest, CellVegetation.Marsh }
                    },
                    Location = new HexCellTestData() { Vegetation = CellVegetation.None }
                }).SetName("Promotion restricted by features and location not a valid feature | promotion ignored").Returns(new CombatInfo() {
                    Attacker = new UnitCombatInfo() { CombatModifier = 0f }
                });

                yield return new TestCaseData(new ParsePromotionTestData() {
                    Promotion = new PromotionTestData() {
                        CombatModifier = 0.5f, AppliesWhileAttacking = true,
                        RestrictedByVegetation = false,
                        ValidVegetation = new List<CellVegetation>() { CellVegetation.Forest, CellVegetation.Marsh }
                    },
                    Location = new HexCellTestData() { Vegetation = CellVegetation.None }
                }).SetName("Promotion not restricted by features and location not a valid feature | promotion applied").Returns(new CombatInfo() {
                    Attacker = new UnitCombatInfo() { CombatModifier = 0.5f }
                });

                yield return new TestCaseData(new ParsePromotionTestData() {
                    Promotion = new PromotionTestData() {
                        CombatModifier = 0.5f, AppliesWhileAttacking = true,
                        RestrictedByVegetation = true,
                        ValidVegetation = new List<CellVegetation>() { CellVegetation.Forest, CellVegetation.Marsh }
                    },
                    Location = new HexCellTestData() { Vegetation = CellVegetation.Marsh }
                }).SetName("Promotion restricted by features and location a valid feature | promotion applied").Returns(new CombatInfo() {
                    Attacker = new UnitCombatInfo() { CombatModifier = 0.5f }
                });

                yield return new TestCaseData(new ParsePromotionTestData() {
                    Promotion = new PromotionTestData() {
                        CombatModifier = 0.5f, AppliesWhileAttacking = true,
                        RestrictedByOpponentTypes = true,
                        ValidOpponentTypes = new List<UnitType>() { UnitType.Melee, UnitType.NavalMelee }
                    },
                    Defender = new UnitTestData() { Type = UnitType.Archery }
                }).SetName("Promotion restricted by opponent types and defender wrong type | promotion ignored").Returns(new CombatInfo() {
                    Attacker = new UnitCombatInfo() { CombatModifier = 0f }
                });

                yield return new TestCaseData(new ParsePromotionTestData() {
                    Promotion = new PromotionTestData() {
                        CombatModifier = 0.5f, AppliesWhileAttacking = true,
                        RestrictedByOpponentTypes = false,
                        ValidOpponentTypes = new List<UnitType>() { UnitType.Melee, UnitType.NavalMelee }
                    },
                    Defender = new UnitTestData() { Type = UnitType.Archery }
                }).SetName("Promotion not restricted by opponent types and defender wrong type | promotion applied").Returns(new CombatInfo() {
                    Attacker = new UnitCombatInfo() { CombatModifier = 0.5f }
                });

                yield return new TestCaseData(new ParsePromotionTestData() {
                    Promotion = new PromotionTestData() {
                        CombatModifier = 0.5f, AppliesWhileAttacking = true,
                        RestrictedByOpponentTypes = true,
                        ValidOpponentTypes = new List<UnitType>() { UnitType.Melee, UnitType.NavalMelee }
                    },
                    Defender = new UnitTestData() { Type = UnitType.Melee }
                }).SetName("Promotion restricted by opponent types and defender valid type | promotion applied").Returns(new CombatInfo() {
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

                yield return new TestCaseData(new ParsePromotionTestData() {
                    Promotion = new PromotionTestData() {
                        CombatModifier = 0.5f, AppliesWhileAttacking = true,
                        RestrictedByOpponentWoundedState = true,
                        ValidOpponentWoundedState = true
                    },
                    Defender = new UnitTestData() { IsWounded = false }
                }).SetName("Promotion restricted by opponent wounded state and wounded state is incorrect | promotion ignored").Returns(new CombatInfo() {
                    Attacker = new UnitCombatInfo() { CombatModifier = 0f }
                });

                yield return new TestCaseData(new ParsePromotionTestData() {
                    Promotion = new PromotionTestData() {
                        CombatModifier = 0.5f, AppliesWhileAttacking = true,
                        RestrictedByOpponentWoundedState = false,
                        ValidOpponentWoundedState = true
                    },
                    Defender = new UnitTestData() { IsWounded = false }
                }).SetName("Promotion not restricted by opponent wounded state and wounded state is incorrect | promotion applied").Returns(new CombatInfo() {
                    Attacker = new UnitCombatInfo() { CombatModifier = 0.5f }
                });

                yield return new TestCaseData(new ParsePromotionTestData() {
                    Promotion = new PromotionTestData() {
                        CombatModifier = 0.5f, AppliesWhileAttacking = true,
                        RestrictedByOpponentWoundedState = true,
                        ValidOpponentWoundedState = true
                    },
                    Defender = new UnitTestData() { IsWounded = true }
                }).SetName("Promotion restricted by opponent wounded state and wounded state is correct | promotion applied").Returns(new CombatInfo() {
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
                        ValidTerrains = new List<CellTerrain>() { CellTerrain.Plains }
                    },
                    Location = new HexCellTestData() { Terrain = CellTerrain.Grassland }
                }).SetName("Promotion restricted by terrains and location not a valid terrain | promotion ignored").Returns(new CombatInfo() {
                    Defender = new UnitCombatInfo() { CombatModifier = 0f }
                });

                yield return new TestCaseData(new ParsePromotionTestData() {
                    Promotion = new PromotionTestData() {
                        CombatModifier = 0.5f, AppliesWhileDefending = true,
                        RestrictedByTerrains = false,
                        ValidTerrains = new List<CellTerrain>() { CellTerrain.Plains }
                    },
                    Location = new HexCellTestData() { Terrain = CellTerrain.Grassland }
                }).SetName("Promotion not restricted by terrains and location not a valid terrain | promotion applied").Returns(new CombatInfo() {
                    Defender = new UnitCombatInfo() { CombatModifier = 0.5f }
                });

                yield return new TestCaseData(new ParsePromotionTestData() {
                    Promotion = new PromotionTestData() {
                        CombatModifier = 0.5f, AppliesWhileDefending = true,
                        RestrictedByTerrains = true,
                        ValidTerrains = new List<CellTerrain>() { CellTerrain.Plains }
                    },
                    Location = new HexCellTestData() { Terrain = CellTerrain.Plains }
                }).SetName("Promotion restricted by terrains and location a valid terrain | promotion applied").Returns(new CombatInfo() {
                    Defender = new UnitCombatInfo() { CombatModifier = 0.5f }
                });

                yield return new TestCaseData(new ParsePromotionTestData() {
                    Promotion = new PromotionTestData() {
                        CombatModifier = 0.5f, AppliesWhileDefending = true,
                        RestrictedByShapes = true,
                        ValidShapes = new List<CellShape>() { CellShape.Hills, CellShape.Mountains }
                    },
                    Location = new HexCellTestData() { Shape = CellShape.Flatlands }
                }).SetName("Promotion restricted by shapes and location not a valid shape | promotion ignored").Returns(new CombatInfo() {
                    Defender = new UnitCombatInfo() { CombatModifier = 0f }
                });

                yield return new TestCaseData(new ParsePromotionTestData() {
                    Promotion = new PromotionTestData() {
                        CombatModifier = 0.5f, AppliesWhileDefending = true,
                        RestrictedByShapes = false,
                        ValidShapes = new List<CellShape>() { CellShape.Hills, CellShape.Mountains }
                    },
                    Location = new HexCellTestData() { Shape = CellShape.Flatlands }
                }).SetName("Promotion not restricted by shapes and location not a valid shapes | promotion applied").Returns(new CombatInfo() {
                    Defender = new UnitCombatInfo() { CombatModifier = 0.5f }
                });

                yield return new TestCaseData(new ParsePromotionTestData() {
                    Promotion = new PromotionTestData() {
                        CombatModifier = 0.5f, AppliesWhileDefending = true,
                        RestrictedByShapes = true,
                        ValidShapes = new List<CellShape>() { CellShape.Hills, CellShape.Mountains }
                    },
                    Location = new HexCellTestData() { Shape = CellShape.Mountains }
                }).SetName("Promotion restricted by shapes and location a valid shape | promotion applied").Returns(new CombatInfo() {
                    Defender = new UnitCombatInfo() { CombatModifier = 0.5f }
                });

                yield return new TestCaseData(new ParsePromotionTestData() {
                    Promotion = new PromotionTestData() {
                        CombatModifier = 0.5f, AppliesWhileDefending = true,
                        RestrictedByVegetation = true,
                        ValidVegetation = new List<CellVegetation>() { CellVegetation.Forest, CellVegetation.Marsh }
                    },
                    Location = new HexCellTestData() { Vegetation = CellVegetation.None }
                }).SetName("Promotion restricted by features and location not a valid feature | promotion ignored").Returns(new CombatInfo() {
                    Defender = new UnitCombatInfo() { CombatModifier = 0f }
                });

                yield return new TestCaseData(new ParsePromotionTestData() {
                    Promotion = new PromotionTestData() {
                        CombatModifier = 0.5f, AppliesWhileDefending = true,
                        RestrictedByVegetation = false,
                        ValidVegetation = new List<CellVegetation>() { CellVegetation.Forest, CellVegetation.Marsh }
                    },
                    Location = new HexCellTestData() { Vegetation = CellVegetation.None }
                }).SetName("Promotion not restricted by features and location not a valid feature | promotion applied").Returns(new CombatInfo() {
                    Defender = new UnitCombatInfo() { CombatModifier = 0.5f }
                });

                yield return new TestCaseData(new ParsePromotionTestData() {
                    Promotion = new PromotionTestData() {
                        CombatModifier = 0.5f, AppliesWhileDefending = true,
                        RestrictedByVegetation = true,
                        ValidVegetation = new List<CellVegetation>() { CellVegetation.Forest, CellVegetation.Marsh }
                    },
                    Location = new HexCellTestData() { Vegetation = CellVegetation.Marsh }
                }).SetName("Promotion restricted by features and location a valid feature | promotion applied").Returns(new CombatInfo() {
                    Defender = new UnitCombatInfo() { CombatModifier = 0.5f }
                });

                yield return new TestCaseData(new ParsePromotionTestData() {
                    Promotion = new PromotionTestData() {
                        CombatModifier = 0.5f, AppliesWhileDefending = true,
                        RestrictedByOpponentTypes = true,
                        ValidOpponentTypes = new List<UnitType>() { UnitType.Melee, UnitType.NavalMelee }
                    },
                    Attacker = new UnitTestData() { Type = UnitType.Archery }
                }).SetName("Promotion restricted by opponent types and attacker wrong type | promotion ignored").Returns(new CombatInfo() {
                    Defender = new UnitCombatInfo() { CombatModifier = 0f }
                });

                yield return new TestCaseData(new ParsePromotionTestData() {
                    Promotion = new PromotionTestData() {
                        CombatModifier = 0.5f, AppliesWhileDefending = true,
                        RestrictedByOpponentTypes = false,
                        ValidOpponentTypes = new List<UnitType>() { UnitType.Melee, UnitType.NavalMelee }
                    },
                    Attacker = new UnitTestData() { Type = UnitType.Archery }
                }).SetName("Promotion not restricted by opponent types and attacker wrong type | promotion applied").Returns(new CombatInfo() {
                    Defender = new UnitCombatInfo() { CombatModifier = 0.5f }
                });

                yield return new TestCaseData(new ParsePromotionTestData() {
                    Promotion = new PromotionTestData() {
                        CombatModifier = 0.5f, AppliesWhileDefending = true,
                        RestrictedByOpponentTypes = true,
                        ValidOpponentTypes = new List<UnitType>() { UnitType.Melee, UnitType.NavalMelee }
                    },
                    Attacker = new UnitTestData() { Type = UnitType.Melee }
                }).SetName("Promotion restricted by opponent types and attacker valid type | promotion applied").Returns(new CombatInfo() {
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

                yield return new TestCaseData(new ParsePromotionTestData() {
                    Promotion = new PromotionTestData() {
                        CombatModifier = 0.5f, AppliesWhileDefending = true,
                        RestrictedByOpponentWoundedState = true,
                        ValidOpponentWoundedState = true
                    },
                    Attacker = new UnitTestData() { IsWounded = false }
                }).SetName("Promotion restricted by opponent wounded state and wounded state is incorrect | promotion ignored").Returns(new CombatInfo() {
                    Defender = new UnitCombatInfo() { CombatModifier = 0f }
                });

                yield return new TestCaseData(new ParsePromotionTestData() {
                    Promotion = new PromotionTestData() {
                        CombatModifier = 0.5f, AppliesWhileDefending = true,
                        RestrictedByOpponentWoundedState = false,
                        ValidOpponentWoundedState = true
                    },
                    Attacker = new UnitTestData() { IsWounded = false }
                }).SetName("Promotion not restricted by opponent wounded state and wounded state is incorrect | promotion applied").Returns(new CombatInfo() {
                    Defender = new UnitCombatInfo() { CombatModifier = 0.5f }
                });

                yield return new TestCaseData(new ParsePromotionTestData() {
                    Promotion = new PromotionTestData() {
                        CombatModifier = 0.5f, AppliesWhileDefending = true,
                        RestrictedByOpponentWoundedState = true,
                        ValidOpponentWoundedState = true
                    },
                    Attacker = new UnitTestData() { IsWounded = true }
                }).SetName("Promotion restricted by opponent wounded state and wounded state is correct | promotion applied").Returns(new CombatInfo() {
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
            mockCell.Setup(cell => cell.Vegetation)    .Returns(cellData.Vegetation);
            mockCell.Setup(cell => cell.IsRoughTerrain).Returns(cellData.IsRoughTerrain);

            return mockCell.Object;
        }

        private IUnit BuildUnit(UnitTestData unitData) {
            var mockUnit = new Mock<IUnit>();

            mockUnit.Setup(unit => unit.Type)     .Returns(unitData.Type);
            mockUnit.Setup(unit => unit.IsWounded).Returns(unitData.IsWounded);

            return mockUnit.Object;
        }

        private IPromotion BuildPromotion(PromotionTestData promotionData) {
            var mockPromotion = new Mock<IPromotion>();

            mockPromotion.Setup(promotion => promotion.RestrictedByTerrains).Returns(promotionData.RestrictedByTerrains);
            mockPromotion.Setup(promotion => promotion.ValidTerrains)       .Returns(promotionData.ValidTerrains);

            mockPromotion.Setup(promotion => promotion.RestrictedByShapes).Returns(promotionData.RestrictedByShapes);
            mockPromotion.Setup(promotion => promotion.ValidShapes)       .Returns(promotionData.ValidShapes);

            mockPromotion.Setup(promotion => promotion.RestrictedByVegetations).Returns(promotionData.RestrictedByVegetation);
            mockPromotion.Setup(promotion => promotion.ValidVegetations)       .Returns(promotionData.ValidVegetation);

            mockPromotion.Setup(promotion => promotion.RestrictedByOpponentTypes).Returns(promotionData.RestrictedByOpponentTypes);
            mockPromotion.Setup(promotion => promotion.ValidOpponentTypes)       .Returns(promotionData.ValidOpponentTypes);

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

            mockPromotion.Setup(promotion => promotion.IgnoresLineOfSight).Returns(promotionData.IgnoresLineOfSight);

            mockPromotion.Setup(promotion => promotion.RestrictedByOpponentWoundedState).Returns(promotionData.RestrictedByOpponentWoundedState);
            mockPromotion.Setup(promotion => promotion.ValidOpponentWoundedState)       .Returns(promotionData.ValidOpponentWoundedState);

            return mockPromotion.Object;
        }

        #endregion

        #endregion

    }

}
