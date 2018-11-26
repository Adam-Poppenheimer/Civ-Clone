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

        public class GetAttackInfoTestData {

            public UnitTestData Attacker = new UnitTestData();

            public UnitTestData Defender = new UnitTestData();

            public HexCellTestData Location = new HexCellTestData();

            public CivConfigTestData CivConfig = new CivConfigTestData();

            public UnitConfigTestData UnitConfig = new UnitConfigTestData();

            public CombatType CombatType = CombatType.Melee;

        }

        public class UnitTestData {

            public CivilizationTestData Owner = new CivilizationTestData();

            public UnitCombatSummary CombatSummary = new UnitCombatSummary();

            public float FortificationModifier = 0f;

        }

        public class CivilizationTestData {

            public int NetHappiness;

        }

        public class HexCellTestData {

            public CellTerrain    Terrain;
            public CellShape      Shape;
            public CellVegetation Vegetation;

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

            public float TerrainDefensiveness    = 0;
            public float VegetationDefensiveness = 0;
            public float ShapeDefensiveness      = 0;

            public float RiverCrossingAttackingModifier;

        }

        public class MockCombatModifier : ICombatModifier {

            public float Modifier { get; set; }

            private bool ModifierApplies;

            public bool DoesModifierApply(IUnit self, IUnit opponent, IHexCell location, CombatType combatType) {
                return ModifierApplies;
            }

            public MockCombatModifier(float modifier, bool modifierApplies) {
                Modifier = modifier;
                ModifierApplies = modifierApplies;
            }
        }

        #endregion

        #region static fields and properties

        public static IEnumerable GetAttackInfoTestCases {
            get {
                yield return new TestCaseData(new GetAttackInfoTestData() {
                    CombatType = CombatType.Ranged
                }).SetName("No modifiers | returned info's CombatType is argued combat type").Returns(new CombatInfo() {
                    CombatType = CombatType.Ranged
                });

                yield return new TestCaseData(new GetAttackInfoTestData() {
                    Location = new HexCellTestData() {
                        Terrain = CellTerrain.Grassland, Shape = CellShape.Hills,
                         Vegetation = CellVegetation.Jungle
                    },
                    UnitConfig = new UnitConfigTestData() {
                        TerrainDefensiveness = 1f,
                        ShapeDefensiveness   = 2f,
                        VegetationDefensiveness = 4f
                    }
                }).SetName("No modifiers | returned info contains defensiveness from terrain").Returns(new CombatInfo() {
                    CombatType = CombatType.Melee,
                    DefenderCombatModifier = 7f
                });

                yield return new TestCaseData(new GetAttackInfoTestData() {
                    Location = new HexCellTestData() {
                        Terrain = CellTerrain.Grassland, Shape = CellShape.Hills,
                         Vegetation = CellVegetation.Jungle,
                         Improvement = new ImprovementTestData() { DefensiveBonus = 2f }
                    },
                    UnitConfig = new UnitConfigTestData() {
                        TerrainDefensiveness = 1f,
                        ShapeDefensiveness   = 2f,
                        VegetationDefensiveness = 4f
                    },
                    Defender = new UnitTestData() {
                        CombatSummary = new UnitCombatSummary() { IgnoresDefensiveTerrainBonus = true }
                    }
                }).SetName("Defender suppresses terrain defensiveness | terrain defensiveness not included").Returns(new CombatInfo() {
                    CombatType = CombatType.Melee,
                    DefenderCombatModifier = 0f
                });

                yield return new TestCaseData(new GetAttackInfoTestData() {
                    Location = new HexCellTestData() {
                        Improvement = new ImprovementTestData() {
                            DefensiveBonus = 5f
                        }
                    }
                }).SetName("Defender doesn't suppress terrain defensiveness | improvements contribute").Returns(new CombatInfo() {
                    CombatType = CombatType.Melee,
                    DefenderCombatModifier = 5f
                });

                yield return new TestCaseData(new GetAttackInfoTestData() {
                    Location = new HexCellTestData() {
                        Terrain = CellTerrain.Grassland, Shape = CellShape.Flatlands,
                        Vegetation = CellVegetation.None
                    },
                    Attacker = new UnitTestData() {
                        CombatSummary = new UnitCombatSummary() {
                            modifiersWhenAttacking = new List<ICombatModifier>() {
                                new MockCombatModifier(1f, true),
                                new MockCombatModifier(2f, true),
                                new MockCombatModifier(7f, false)
                            }
                        }
                    }
                }).SetName("Attacker's ModifiersWhenAttacking applied when DoesModifierApply returns true").Returns(new CombatInfo() {
                    CombatType = CombatType.Melee,
                    AttackerCombatModifier = 3f
                });

                yield return new TestCaseData(new GetAttackInfoTestData() {
                    Location = new HexCellTestData() {
                        Terrain = CellTerrain.Grassland, Shape = CellShape.Flatlands,
                        Vegetation = CellVegetation.None
                    },
                    Defender = new UnitTestData() {
                        CombatSummary = new UnitCombatSummary() {
                            modifiersWhenDefending = new List<ICombatModifier>() {
                                new MockCombatModifier(1f, true),
                                new MockCombatModifier(2f, true),
                                new MockCombatModifier(7f, false)
                            }
                        }
                    }
                }).SetName("Defender's ModifiersWhenDefending applied when DoesModifierApply returns true").Returns(new CombatInfo() {
                    CombatType = CombatType.Melee,
                    DefenderCombatModifier = 3f
                });

                yield return new TestCaseData(new GetAttackInfoTestData() {
                    Location = new HexCellTestData() {
                        Terrain = CellTerrain.Grassland, Shape = CellShape.Flatlands,
                        Vegetation = CellVegetation.None
                    },
                    Attacker = new UnitTestData() {
                        CombatSummary = new UnitCombatSummary() {
                            modifiersWhenDefending = new List<ICombatModifier>() {
                                new MockCombatModifier(1f, true),
                                new MockCombatModifier(2f, true),
                                new MockCombatModifier(7f, false)
                            }
                        }
                    }
                }).SetName("Attacker's ModifiersWhenDefending never applied").Returns(new CombatInfo() {
                    CombatType = CombatType.Melee,
                    AttackerCombatModifier = 0f
                });

                yield return new TestCaseData(new GetAttackInfoTestData() {
                    Location = new HexCellTestData() {
                        Terrain = CellTerrain.Grassland, Shape = CellShape.Flatlands,
                        Vegetation = CellVegetation.None
                    },
                    Defender = new UnitTestData() {
                        CombatSummary = new UnitCombatSummary() {
                            modifiersWhenAttacking = new List<ICombatModifier>() {
                                new MockCombatModifier(1f, true),
                                new MockCombatModifier(2f, true),
                                new MockCombatModifier(7f, false)
                            }
                        }
                    }
                }).SetName("Defender's ModifiersWhenAttacking never applied").Returns(new CombatInfo() {
                    CombatType = CombatType.Melee,
                    DefenderCombatModifier = 0f
                });

                yield return new TestCaseData(new GetAttackInfoTestData() {
                    Attacker = new UnitTestData() {
                        Owner = new CivilizationTestData() { NetHappiness = -10 }
                    },
                    Defender = new UnitTestData() {
                        Owner = new CivilizationTestData() { NetHappiness = -15 }
                    },
                    CivConfig = new CivConfigTestData() { ModifierLossPerUnhappiness = -2f }
                }).SetName("Unhappiness reduces attacker and defender modifier by configured amount").Returns(new CombatInfo() {
                    CombatType = CombatType.Melee,
                    AttackerCombatModifier = -20f,
                    DefenderCombatModifier = -30f 
                });

                yield return new TestCaseData(new GetAttackInfoTestData() {
                    Location = new HexCellTestData() {
                        HasRiver = true
                    },
                    UnitConfig = new UnitConfigTestData() { RiverCrossingAttackingModifier = -2f }
                }).SetName("River crossing inflicts penalty when attack is melee").Returns(new CombatInfo() {
                    CombatType = CombatType.Melee,
                    AttackerCombatModifier = -2f
                });

                yield return new TestCaseData(new GetAttackInfoTestData() {
                    CombatType = CombatType.Ranged,
                    Location = new HexCellTestData() {
                        HasRiver = true
                    },
                    UnitConfig = new UnitConfigTestData() { RiverCrossingAttackingModifier = -2f }
                }).SetName("River crossing inflicts ignored when attacked is ranged").Returns(new CombatInfo() {
                    CombatType = CombatType.Ranged,
                    AttackerCombatModifier = 0f
                });

                yield return new TestCaseData(new GetAttackInfoTestData() {
                    CombatType = CombatType.Melee,
                    Location = new HexCellTestData() {
                        HasRiver = true
                    },
                    UnitConfig = new UnitConfigTestData() { RiverCrossingAttackingModifier = -2f },
                    Attacker = new UnitTestData() {
                        CombatSummary = new UnitCombatSummary() { IgnoresAmphibiousPenalty = true }
                    }
                }).SetName("River crossing penalty ignored if promotions say so").Returns(new CombatInfo() {
                    CombatType = CombatType.Melee,
                    AttackerCombatModifier = 0f
                });

                yield return new TestCaseData(new GetAttackInfoTestData() {
                    CombatType = CombatType.Melee,
                    Defender = new UnitTestData() {
                        FortificationModifier = 0.5f,
                        CombatSummary = new UnitCombatSummary() { IgnoresDefensiveTerrainBonus = false }
                    }
                }).SetName("Defender adds fortification bonus when defender doesn't ignore defensive terrain bonus").Returns(new CombatInfo() {
                    CombatType = CombatType.Melee, DefenderCombatModifier = 0.5f
                });

                yield return new TestCaseData(new GetAttackInfoTestData() {
                    CombatType = CombatType.Melee,
                    Defender = new UnitTestData() {
                        FortificationModifier = 0.5f,
                        CombatSummary = new UnitCombatSummary() { IgnoresDefensiveTerrainBonus = true }
                    }
                }).SetName("Defender doesn't add fortification bonus when defender ignores defensive terrain bonus").Returns(new CombatInfo() {
                    CombatType = CombatType.Melee, DefenderCombatModifier = 0f
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
        private Mock<IUnitFortificationLogic>                       MockFortificationLogic;
        private Mock<ICombatAuraLogic>                              MockCombatAuraLogic;
        private Mock<ICityCombatModifierLogic>                      MockCityCombatModifierLogic;

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
            MockFortificationLogic         = new Mock<IUnitFortificationLogic>();
            MockCombatAuraLogic            = new Mock<ICombatAuraLogic>();
            MockCityCombatModifierLogic    = new Mock<ICityCombatModifierLogic>();

            Container.Bind<IUnitConfig>                                  ().FromInstance(MockUnitConfig                .Object);
            Container.Bind<IRiverCanon>                                  ().FromInstance(MockRiverCanon                .Object);
            Container.Bind<IImprovementLocationCanon>                    ().FromInstance(MockImprovementLocationCanon  .Object);
            Container.Bind<IPossessionRelationship<ICivilization, IUnit>>().FromInstance(MockUnitPossessionCanon       .Object);
            Container.Bind<ICivilizationHappinessLogic>                  ().FromInstance(MockCivilizationHappinessLogic.Object);
            Container.Bind<ICivilizationConfig>                          ().FromInstance(MockCivConfig                 .Object);
            Container.Bind<IUnitFortificationLogic>                      ().FromInstance(MockFortificationLogic        .Object);
            Container.Bind<ICombatAuraLogic>                             ().FromInstance(MockCombatAuraLogic           .Object);
            Container.Bind<ICityCombatModifierLogic>                     ().FromInstance(MockCityCombatModifierLogic   .Object);

            Container.Bind<CombatInfoLogic>().AsSingle();
        }

        private void SetUpConfigs(GetAttackInfoTestData testData) {
            MockUnitConfig.Setup(config => config.GetTerrainDefensiveness(It.IsAny<CellTerrain>()))
                          .Returns(testData.UnitConfig.TerrainDefensiveness);

            MockUnitConfig.Setup(config => config.GetShapeDefensiveness(It.IsAny<CellShape>()))
                          .Returns(testData.UnitConfig.ShapeDefensiveness);

            MockUnitConfig.Setup(config => config.GetVegetationDefensiveness(It.IsAny<CellVegetation>()))
                          .Returns(testData.UnitConfig.VegetationDefensiveness);

            MockUnitConfig.Setup(config => config.RiverCrossingAttackModifier).Returns(testData.UnitConfig.RiverCrossingAttackingModifier);

            MockCivConfig.Setup(config => config.ModifierLossPerUnhappiness).Returns(testData.CivConfig.ModifierLossPerUnhappiness);
        }

        #endregion

        #region tests

        [Test]
        [TestCaseSource("GetAttackInfoTestCases")]       
        public CombatInfo GetAttackInfoTests(GetAttackInfoTestData testData) {
            SetUpConfigs(testData);

            var attacker = BuildUnit(testData.Attacker);
            var defender = BuildUnit(testData.Defender);

            var location = BuildHexCell(testData.Location);

            var combatInfoLogic = Container.Resolve<CombatInfoLogic>();

            return combatInfoLogic.GetAttackInfo(attacker, defender, location, testData.CombatType);
        }

        [Test]
        public void GetAttackInfo_CallsIntoCombatAuraLogic() {
            var attacker = BuildUnit(new UnitTestData());
            var defender = BuildUnit(new UnitTestData());

            var location = BuildHexCell(new HexCellTestData());

            var combatInfoLogic = Container.Resolve<CombatInfoLogic>();

            var combatInfo = combatInfoLogic.GetAttackInfo(attacker, defender, location, CombatType.Melee);

            MockCombatAuraLogic.Verify(logic => logic.ApplyAurasToCombat(attacker, defender, combatInfo), Times.Once);
        }

        [Test]
        public void GetAttackInfo_CallsIntoCityCombatModifierLogic() {
            var attacker = BuildUnit(new UnitTestData());
            var defender = BuildUnit(new UnitTestData());

            var location = BuildHexCell(new HexCellTestData());

            var combatInfoLogic = Container.Resolve<CombatInfoLogic>();

            var combatInfo = combatInfoLogic.GetAttackInfo(attacker, defender, location, CombatType.Melee);

            MockCityCombatModifierLogic.Verify(
                logic => logic.ApplyCityModifiersToCombat(attacker, defender, combatInfo), Times.Once
            );
        }

        #endregion

        #region utilities

        private IUnit BuildUnit(UnitTestData data) {
            var mockUnit = new Mock<IUnit>();

            mockUnit.Setup(unit => unit.CombatSummary).Returns(data.CombatSummary);

            var newUnit = mockUnit.Object;

            var owner = BuildCivilization(data.Owner);

            MockUnitPossessionCanon.Setup(canon => canon.GetOwnerOfPossession(newUnit)).Returns(owner);

            MockFortificationLogic.Setup(logic => logic.GetFortificationModifierForUnit(newUnit)).Returns(data.FortificationModifier);

            return newUnit;
        }

        private ICivilization BuildCivilization(CivilizationTestData data) {
            var newCiv = new Mock<ICivilization>().Object;

            MockCivilizationHappinessLogic.Setup(logic => logic.GetNetHappinessOfCiv(newCiv)).Returns(data.NetHappiness);

            return newCiv;
        }

        private IHexCell BuildHexCell(HexCellTestData data) {
            var mockCell = new Mock<IHexCell>();

            mockCell.Setup(cell => cell.Terrain)   .Returns(data.Terrain);
            mockCell.Setup(cell => cell.Shape)     .Returns(data.Shape);
            mockCell.Setup(cell => cell.Vegetation).Returns(data.Vegetation);

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
