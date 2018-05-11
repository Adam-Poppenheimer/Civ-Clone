using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation;
using Assets.Simulation.Units;
using Assets.Simulation.Units.Combat;
using Assets.Simulation.Units.Promotions;
using Assets.Simulation.HexMap;
using Assets.Simulation.Civilizations;
using Assets.Simulation.Improvements;

namespace Assets.Tests.Simulation.Units.Combat {

    [TestFixture]
    public class CombatInfoLogicTests : ZenjectUnitTestFixture {

        #region internal types

        public class CombatInfoLogicTestData {

            public UnitTestData Attacker = new UnitTestData();

            public UnitTestData Defender = new UnitTestData();

            public HexCellTestData Location = new HexCellTestData();

            public CombatInfo CombatInfoFromPromotions = new CombatInfo();

            public CivConfigTestData CivConfig = new CivConfigTestData();

            public UnitConfigTestData UnitConfig = new UnitConfigTestData();

        }

        public class UnitTestData {

            public CivilizationTestData Owner = new CivilizationTestData();

        }

        public class CivilizationTestData {

            public int NetHappiness;

        }

        public class HexCellTestData {

            public TerrainType    Terrain;
            public TerrainShape   Shape;
            public TerrainFeature Feature;

            public bool HasRiver;

            public ImprovementTestData Improvement;

        }

        public class ImprovementTestData {

            public float DefensiveBonus;

        }

        public class CivConfigTestData {

            public float ModifierLossPerUnhappiness;

        }

        public class UnitConfigTestData {

            public float TerrainDefensiveness = 0;
            public float FeatureDefensiveness = 0;
            public float ShapeDefensiveness   = 0;

            public float RiverCrossingAttackingModifier;

        }

        #endregion

        #region static fields and properties

        public static IEnumerable GetMeleeAttackInfoTestCases {
            get {
                yield return new TestCaseData(new CombatInfoLogicTestData() {

                }).SetName("Empty promotion info | returned info's CombatType set to Melee").Returns(new CombatInfo() {
                    CombatType = CombatType.Melee
                });

                yield return new TestCaseData(new CombatInfoLogicTestData() {
                    Location = new HexCellTestData() {
                        Terrain = TerrainType.Grassland, Shape = TerrainShape.Hills,
                         Feature = TerrainFeature.Jungle
                    },
                    UnitConfig = new UnitConfigTestData() {
                        TerrainDefensiveness = 1f,
                        ShapeDefensiveness   = 2f,
                        FeatureDefensiveness = 4f
                    }
                }).SetName("Empty promotion info | returned info contains defensiveness from terrain").Returns(new CombatInfo() {
                    CombatType = CombatType.Melee,
                    Defender = new UnitCombatInfo() { CombatModifier = 7f }
                });

                yield return new TestCaseData(new CombatInfoLogicTestData() {
                    Location = new HexCellTestData() {
                        Terrain = TerrainType.Grassland, Shape = TerrainShape.Hills,
                         Feature = TerrainFeature.Jungle,
                         Improvement = new ImprovementTestData() { DefensiveBonus = 2f }
                    },
                    UnitConfig = new UnitConfigTestData() {
                        TerrainDefensiveness = 1f,
                        ShapeDefensiveness   = 2f,
                        FeatureDefensiveness = 4f
                    },
                    CombatInfoFromPromotions = new CombatInfo() {
                        Defender = new UnitCombatInfo() { IgnoresDefensiveTerrainBonuses = true }
                    }
                }).SetName("Promotion info suppresses terrain defensiveness | terrain defensiveness not included").Returns(new CombatInfo() {
                    CombatType = CombatType.Melee,
                    Defender = new UnitCombatInfo() { CombatModifier = 0f, IgnoresDefensiveTerrainBonuses = true }
                });

                yield return new TestCaseData(new CombatInfoLogicTestData() {
                    Location = new HexCellTestData() {
                        Improvement = new ImprovementTestData() {
                            DefensiveBonus = 5f
                        }
                    }
                }).SetName("Empty promotion info | returned info contains defensiveness from improvements at location").Returns(new CombatInfo() {
                    CombatType = CombatType.Melee,
                    Defender = new UnitCombatInfo() { CombatModifier = 5f }
                });

                yield return new TestCaseData(new CombatInfoLogicTestData() {
                    Location = new HexCellTestData() {
                        Improvement = new ImprovementTestData() {
                            DefensiveBonus = 5f
                        }
                    },
                    CombatInfoFromPromotions = new CombatInfo() {
                        Attacker = new UnitCombatInfo() {
                            CombatModifier = 3f, CanMoveAfterAttacking = true,
                            CanAttackAfterAttacking = true, IgnoresAmphibiousPenalty = false
                        },
                        Defender = new UnitCombatInfo() { CombatModifier = 5f }
                    }
                }).SetName("Promotion info is non-empty | fields set from promotions carry through to output").Returns(new CombatInfo() {
                    CombatType = CombatType.Melee,
                    Attacker = new UnitCombatInfo() {
                        CombatModifier = 3f, CanMoveAfterAttacking = true,
                        CanAttackAfterAttacking = true, IgnoresAmphibiousPenalty = false
                    },
                    Defender = new UnitCombatInfo() { CombatModifier = 10f }
                });

                yield return new TestCaseData(new CombatInfoLogicTestData() {
                    Attacker = new UnitTestData() {
                        Owner = new CivilizationTestData() { NetHappiness = -10 }
                    },
                    Defender = new UnitTestData() {
                        Owner = new CivilizationTestData() { NetHappiness = -15 }
                    },
                    CivConfig = new CivConfigTestData() { ModifierLossPerUnhappiness = -2f }
                }).SetName("Unhappiness reduces attacker and defender modifier by configured amount").Returns(new CombatInfo() {
                    CombatType = CombatType.Melee,
                    Attacker = new UnitCombatInfo() { CombatModifier = -20f },
                    Defender = new UnitCombatInfo() { CombatModifier = -30f }
                });

                yield return new TestCaseData(new CombatInfoLogicTestData() {
                    Location = new HexCellTestData() {
                        HasRiver = true
                    },
                    UnitConfig = new UnitConfigTestData() { RiverCrossingAttackingModifier = -2f }
                }).SetName("River crossing inflicts penalty on attacker").Returns(new CombatInfo() {
                    CombatType = CombatType.Melee,
                    Attacker = new UnitCombatInfo() { CombatModifier = -2f }
                });

                yield return new TestCaseData(new CombatInfoLogicTestData() {
                    Location = new HexCellTestData() {
                        HasRiver = true
                    },
                    UnitConfig = new UnitConfigTestData() { RiverCrossingAttackingModifier = -2f },
                    CombatInfoFromPromotions = new CombatInfo() {
                        Attacker = new UnitCombatInfo() { IgnoresAmphibiousPenalty = true }
                    }
                }).SetName("River crossing penalty ignored if promotions say so").Returns(new CombatInfo() {
                    CombatType = CombatType.Melee,
                    Attacker = new UnitCombatInfo() { IgnoresAmphibiousPenalty = true }
                });
            }
        }

        public static IEnumerable GetRangedAttackInfoTestCases {
            get {
                yield return new TestCaseData(new CombatInfoLogicTestData() {

                }).SetName("Empty promotion info | returned info's CombatType set to Ranged").Returns(new CombatInfo() {
                    CombatType = CombatType.Ranged
                });

                yield return new TestCaseData(new CombatInfoLogicTestData() {
                    Location = new HexCellTestData() {
                        Terrain = TerrainType.Grassland, Shape = TerrainShape.Hills,
                        Feature = TerrainFeature.Jungle
                    },
                    UnitConfig = new UnitConfigTestData() {
                        TerrainDefensiveness = 1f,
                        ShapeDefensiveness   = 2f,
                        FeatureDefensiveness = 4f
                    }
                }).SetName("Empty promotion info | returned info contains defensiveness from terrain").Returns(new CombatInfo() {
                    CombatType = CombatType.Ranged,
                    Defender = new UnitCombatInfo() { CombatModifier = 7f }
                });

                yield return new TestCaseData(new CombatInfoLogicTestData() {
                    Location = new HexCellTestData() {
                        Terrain = TerrainType.Grassland, Shape = TerrainShape.Hills,
                        Feature = TerrainFeature.Jungle,
                        Improvement = new ImprovementTestData() { DefensiveBonus = 2f }
                    },
                    UnitConfig = new UnitConfigTestData() {
                        TerrainDefensiveness = 1f,
                        ShapeDefensiveness   = 2f,
                        FeatureDefensiveness = 4f
                    },
                    CombatInfoFromPromotions = new CombatInfo() {
                        Defender = new UnitCombatInfo() { IgnoresDefensiveTerrainBonuses = true }
                    }
                }).SetName("Promotion info suppresses terrain defensiveness | terrain defensiveness not included").Returns(new CombatInfo() {
                    CombatType = CombatType.Ranged,
                    Defender = new UnitCombatInfo() { CombatModifier = 0f, IgnoresDefensiveTerrainBonuses = true }
                });

                yield return new TestCaseData(new CombatInfoLogicTestData() {
                    Location = new HexCellTestData() {
                        Improvement = new ImprovementTestData() {
                            DefensiveBonus = 5f
                        }
                    }
                }).SetName("Empty promotion info | returned info contains defensiveness from improvements at location").Returns(new CombatInfo() {
                    CombatType = CombatType.Ranged,
                    Defender = new UnitCombatInfo() { CombatModifier = 5f }
                });

                yield return new TestCaseData(new CombatInfoLogicTestData() {
                    Location = new HexCellTestData() {
                        Improvement = new ImprovementTestData() {
                            DefensiveBonus = 5f
                        }
                    },
                    CombatInfoFromPromotions = new CombatInfo() {
                        Attacker = new UnitCombatInfo() {
                            CanMoveAfterAttacking = true, CanAttackAfterAttacking = true,
                            IgnoresAmphibiousPenalty = false, CombatModifier = 3f
                        },
                        Defender = new UnitCombatInfo() { CombatModifier = 5f }
                    }
                }).SetName("Promotion info is non-empty | fields set from promotions carry through to output").Returns(new CombatInfo() {
                    CombatType = CombatType.Ranged,
                    Attacker = new UnitCombatInfo() {
                        CanMoveAfterAttacking = true, CanAttackAfterAttacking = true,
                        IgnoresAmphibiousPenalty = false, CombatModifier = 3f
                    },
                    Defender = new UnitCombatInfo() { CombatModifier = 10f }
                });

                yield return new TestCaseData(new CombatInfoLogicTestData() {
                    Attacker = new UnitTestData() {
                        Owner = new CivilizationTestData() { NetHappiness = -10 }
                    },
                    Defender = new UnitTestData() {
                        Owner = new CivilizationTestData() { NetHappiness = -15 }
                    },
                    CivConfig = new CivConfigTestData() { ModifierLossPerUnhappiness = -2f }
                }).SetName("Unhappiness reduces attacker and defender modifier by configured amount").Returns(new CombatInfo() {
                    CombatType = CombatType.Ranged,
                    Attacker = new UnitCombatInfo() { CombatModifier = -20f },
                    Defender = new UnitCombatInfo() { CombatModifier = -30f }
                });

                yield return new TestCaseData(new CombatInfoLogicTestData() {
                    Location = new HexCellTestData() {
                        HasRiver = true
                    },
                    UnitConfig = new UnitConfigTestData() { RiverCrossingAttackingModifier = -2f }
                }).SetName("River crossing has no effect on attacker").Returns(new CombatInfo() {
                    CombatType = CombatType.Ranged
                });
            }
        }

        #endregion

        #region instance fields and properties

        private Mock<IUnitConfig>                                   MockUnitConfig;
        private Mock<IRiverCanon>                                   MockRiverCanon;
        private Mock<IImprovementLocationCanon>                     MockImprovementLocationCanon;
        private Mock<IPossessionRelationship<ICivilization, IUnit>> MockUnitPossessionCanon;
        private Mock<ICivilizationHappinessLogic>                   MockCivilizationHappinessLogic;
        private Mock<ICivilizationConfig>                           MockCivConfig;
        private Mock<IPromotionParser>                              MockPromotionParser;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockUnitConfig                 = new Mock<IUnitConfig>();
            MockRiverCanon                 = new Mock<IRiverCanon>();
            MockImprovementLocationCanon   = new Mock<IImprovementLocationCanon>();
            MockUnitPossessionCanon        = new Mock<IPossessionRelationship<ICivilization, IUnit>>();
            MockCivilizationHappinessLogic = new Mock<ICivilizationHappinessLogic>();
            MockCivConfig                  = new Mock<ICivilizationConfig>();
            MockPromotionParser            = new Mock<IPromotionParser>();

            Container.Bind<IUnitConfig>                                  ().FromInstance(MockUnitConfig                .Object);
            Container.Bind<IRiverCanon>                                  ().FromInstance(MockRiverCanon                .Object);
            Container.Bind<IImprovementLocationCanon>                    ().FromInstance(MockImprovementLocationCanon  .Object);
            Container.Bind<IPossessionRelationship<ICivilization, IUnit>>().FromInstance(MockUnitPossessionCanon       .Object);
            Container.Bind<ICivilizationHappinessLogic>                  ().FromInstance(MockCivilizationHappinessLogic.Object);
            Container.Bind<ICivilizationConfig>                          ().FromInstance(MockCivConfig                 .Object);
            Container.Bind<IPromotionParser>                             ().FromInstance(MockPromotionParser           .Object);

            Container.Bind<CombatInfoLogic>().AsSingle();
        }

        private void SetUpConfigs(CombatInfoLogicTestData testData) {
            MockUnitConfig.Setup(config => config.GetTerrainDefensiveness(It.IsAny<TerrainType>()))
                          .Returns(testData.UnitConfig.TerrainDefensiveness);

            MockUnitConfig.Setup(config => config.GetShapeDefensiveness(It.IsAny<TerrainShape>()))
                          .Returns(testData.UnitConfig.ShapeDefensiveness);

            MockUnitConfig.Setup(config => config.GetFeatureDefensiveness(It.IsAny<TerrainFeature>()))
                          .Returns(testData.UnitConfig.FeatureDefensiveness);

            MockUnitConfig.Setup(config => config.RiverCrossingAttackModifier).Returns(testData.UnitConfig.RiverCrossingAttackingModifier);

            MockCivConfig.Setup(config => config.ModifierLossPerUnhappiness).Returns(testData.CivConfig.ModifierLossPerUnhappiness);
        }

        #endregion

        #region tests

        [TestCaseSource("GetMeleeAttackInfoTestCases")]
        [Test(Description = "")]
        public CombatInfo GetMeleeAttackInfoTests(CombatInfoLogicTestData testData) {
            SetUpConfigs(testData);

            var attacker = BuildUnit(testData.Attacker);
            var defender = BuildUnit(testData.Defender);

            var location = BuildHexCell(testData.Location);

            MockPromotionParser
                .Setup(parser => parser.GetCombatInfo(attacker, defender, location, CombatType.Melee))
                .Returns(testData.CombatInfoFromPromotions);

            var combatInfoLogic = Container.Resolve<CombatInfoLogic>();

            return combatInfoLogic.GetMeleeAttackInfo(attacker, defender, location);
        }

        [TestCaseSource("GetRangedAttackInfoTestCases")]
        [Test(Description = "")]
        public CombatInfo GetRangedAttackInfoTests(CombatInfoLogicTestData testData) {
            SetUpConfigs(testData);

            var attacker = BuildUnit(testData.Attacker);
            var defender = BuildUnit(testData.Defender);

            var location = BuildHexCell(testData.Location);

            MockPromotionParser
                .Setup(parser => parser.GetCombatInfo(attacker, defender, location, CombatType.Ranged))
                .Returns(testData.CombatInfoFromPromotions);

            var combatInfoLogic = Container.Resolve<CombatInfoLogic>();

            return combatInfoLogic.GetRangedAttackInfo(attacker, defender, location);
        }

        #endregion

        #region utilities

        private IUnit BuildUnit(UnitTestData data) {
            var newUnit = new Mock<IUnit>().Object;

            var owner = BuildCivilization(data.Owner);

            MockUnitPossessionCanon.Setup(canon => canon.GetOwnerOfPossession(newUnit)).Returns(owner);

            return newUnit;
        }

        private ICivilization BuildCivilization(CivilizationTestData data) {
            var newCiv = new Mock<ICivilization>().Object;

            MockCivilizationHappinessLogic.Setup(logic => logic.GetNetHappinessOfCiv(newCiv)).Returns(data.NetHappiness);

            return newCiv;
        }

        private IHexCell BuildHexCell(HexCellTestData data) {
            var mockCell = new Mock<IHexCell>();

            mockCell.Setup(cell => cell.Terrain).Returns(data.Terrain);
            mockCell.Setup(cell => cell.Shape)  .Returns(data.Shape);
            mockCell.Setup(cell => cell.Feature).Returns(data.Feature);

            var newCell = mockCell.Object;

            MockRiverCanon.Setup(canon => canon.HasRiver(newCell)).Returns(data.HasRiver);

            if(data.Improvement != null) {
                BuildImprovement(data.Improvement, newCell);
            }

            return newCell;
        }

        private IImprovement BuildImprovement(ImprovementTestData data, IHexCell location) {
            var mockImprovement = new Mock<IImprovement>();

            var mockTemplate = new Mock<IImprovementTemplate>();

            mockTemplate.Setup(template => template.DefensiveBonus).Returns(data.DefensiveBonus);

            mockImprovement.Setup(improvement => improvement.Template).Returns(mockTemplate.Object);

            var newImprovement = mockImprovement.Object;

            MockImprovementLocationCanon
                .Setup(canon => canon.GetPossessionsOfOwner(location))
                .Returns(new List<IImprovement>() { newImprovement });

            return newImprovement;
        }

        #endregion

        #endregion

    }

}
