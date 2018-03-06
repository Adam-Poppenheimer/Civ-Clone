using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using Zenject;
using Moq;

using Assets.Simulation;
using Assets.Simulation.Civilizations;
using Assets.Simulation.Units;
using Assets.Simulation.Units.Combat;
using Assets.Simulation.HexMap;
using Assets.Simulation.Improvements;

namespace Assets.Tests.Simulation.Units.Combat {

    [TestFixture]
    public class CombatModifierLogicTests : ZenjectUnitTestFixture {

        #region internal types

        public class ModifierTestArgs {

            public TerrainType Terrain;
            public TerrainFeature Feature;

            public bool HasRiver;

            public int AttackerOwnerNetHappiness;
            public int DefenderOwnerNetHappiness;

            public float ModifierLossPerUnhappiness;

        }

        public struct ModifierTestResults {

            public float AttackerMelee;
            public float AttackerRanged;

            public float DefenderMelee;
            public float DefenderRanged;

            public override string ToString() {
                return string.Format("AM: {0} | AR: {1} | DM: {2} | DR: {3}",
                    AttackerMelee, AttackerRanged, DefenderMelee, DefenderRanged);
            }

        }

        #endregion

        #region static fields and properties

        private static IEnumerable TestCases {
            get {
                yield return new TestCaseData(
                    new ModifierTestArgs() {
                        Terrain = TerrainType.Grassland,
                        Feature = TerrainFeature.None,
                        HasRiver = false,
                    }
                )
                .SetName("Grassland/None/No River")
                .Returns(new ModifierTestResults() {
                    AttackerMelee = 0f,
                    AttackerRanged = 0f,
                    DefenderMelee = 2.5f,
                    DefenderRanged = 20.5f,
                });



                yield return new TestCaseData(
                    new ModifierTestArgs() {
                        Terrain = TerrainType.Grassland,
                        Feature = TerrainFeature.None,
                        HasRiver = true,
                    }
                )
                .SetName("Grassland/None/River")
                .Returns(new ModifierTestResults() {
                    AttackerMelee = -0.5f,
                    AttackerRanged = 0f,
                    DefenderMelee = 2.5f,
                    DefenderRanged = 20.5f,
                });



                yield return new TestCaseData(
                    new ModifierTestArgs() {
                        Terrain = TerrainType.Grassland,
                        Feature = TerrainFeature.Forest,
                        HasRiver = false,
                    }
                )
                .SetName("Grassland/Forest/No River")
                .Returns(new ModifierTestResults() {
                    AttackerMelee = 0f,
                    AttackerRanged = 0f,
                    DefenderMelee = 3.5f,
                    DefenderRanged = 30.5f,
                });



                yield return new TestCaseData(
                    new ModifierTestArgs() {
                        Terrain = TerrainType.Grassland,
                        Feature = TerrainFeature.Forest,
                        HasRiver = true,
                    }
                )
                .SetName("Grassland/Forest/River")
                .Returns(new ModifierTestResults() {
                    AttackerMelee = -0.5f,
                    AttackerRanged = 0f,
                    DefenderMelee = 3.5f,
                    DefenderRanged = 30.5f,
                });



                yield return new TestCaseData(
                    new ModifierTestArgs() {
                        Terrain = TerrainType.Plains,
                        Feature = TerrainFeature.None,
                        HasRiver = false,
                    }
                )
                .SetName("Plains/None/No River")
                .Returns(new ModifierTestResults() {
                    AttackerMelee = 0f,
                    AttackerRanged = 0f,
                    DefenderMelee = 3.5f,
                    DefenderRanged = 30.5f,
                });



                yield return new TestCaseData(
                    new ModifierTestArgs() {
                        Terrain = TerrainType.Plains,
                        Feature = TerrainFeature.Forest,
                        HasRiver = true,
                    }
                )
                .SetName("Plains/Forest/River")
                .Returns(new ModifierTestResults() {
                    AttackerMelee = -0.5f,
                    AttackerRanged = 0f,
                    DefenderMelee = 4.5f,
                    DefenderRanged = 40.5f,
                });



                yield return new TestCaseData(
                    new ModifierTestArgs() {
                        Terrain = TerrainType.Plains,
                        Feature = TerrainFeature.Forest,
                        HasRiver = false,
                    }
                )
                .SetName("Plains/Forest/No River")
                .Returns(new ModifierTestResults() {
                    AttackerMelee = 0f,
                    AttackerRanged = 0f,
                    DefenderMelee = 4.5f,
                    DefenderRanged = 40.5f,
                });



                yield return new TestCaseData(
                    new ModifierTestArgs() {
                        Terrain = TerrainType.Grassland,
                        Feature = TerrainFeature.None,
                        HasRiver = false,
                        AttackerOwnerNetHappiness = -10,
                        DefenderOwnerNetHappiness = -8,
                        ModifierLossPerUnhappiness = -0.02f
                    }

                ).SetName("Owning civilization is unhappy")
                .Returns(new ModifierTestResults() {
                    AttackerMelee = -0.02f * 10,
                    AttackerRanged = -0.02f * 10,
                    DefenderMelee = 2.5f - (0.02f * 8),
                    DefenderRanged = 20.5f - (0.02f * 8),
                });
            }
        }

        #endregion

        #region instance fields and properties

        private Mock<IRiverCanon>                                   MockRiverCanon;
        private Mock<IImprovementLocationCanon>                     MockImprovementLocationCanon;
        private Mock<IPossessionRelationship<ICivilization, IUnit>> MockUnitPossessionCanon;
        private Mock<ICivilizationHappinessLogic>                   MockCivilizationHappinessLogic;
        private Mock<ICivilizationConfig>                           MockCivConfig;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockRiverCanon                 = new Mock<IRiverCanon>();
            MockImprovementLocationCanon   = new Mock<IImprovementLocationCanon>();
            MockUnitPossessionCanon        = new Mock<IPossessionRelationship<ICivilization, IUnit>>();
            MockCivilizationHappinessLogic = new Mock<ICivilizationHappinessLogic>();
            MockCivConfig                  = new Mock<ICivilizationConfig>();


            Container.Bind<IRiverCanon>                                  ().FromInstance(MockRiverCanon                .Object);
            Container.Bind<IImprovementLocationCanon>                    ().FromInstance(MockImprovementLocationCanon  .Object);
            Container.Bind<IPossessionRelationship<ICivilization, IUnit>>().FromInstance(MockUnitPossessionCanon       .Object);
            Container.Bind<ICivilizationHappinessLogic>                  ().FromInstance(MockCivilizationHappinessLogic.Object);
            Container.Bind<ICivilizationConfig>                          ().FromInstance(MockCivConfig                 .Object);

            Container.Bind<IUnitConfig>().To<UnitConfig>().FromNewScriptableObjectResource("Tests/Combat Modifier Logic UI Config").AsSingle();

            Container.Bind<CombatModifierLogic>().AsSingle();
        }

        #endregion

        #region tests

        [Test(Description = "Defensive combat modifiers should be the sum of the defensiveness of the location's " +
            "terrain and feature. Offensive combat modifiers should be RiverCrossingAttackModifier when there's a " +
            "river at the location and zero otherwise")]
        [TestCaseSource("TestCases")]
        public ModifierTestResults CombatModifierTests(ModifierTestArgs testData) {
            var location = BuildCell(testData.Terrain, testData.Feature, testData.HasRiver);

            var attacker = BuildUnit(BuildCivilization(testData.AttackerOwnerNetHappiness));
            var defender = BuildUnit(BuildCivilization(testData.DefenderOwnerNetHappiness));

            MockCivConfig.Setup(config => config.ModifierLossPerUnhappiness).Returns(testData.ModifierLossPerUnhappiness);

            var modifierLogic = Container.Resolve<CombatModifierLogic>();

            return new ModifierTestResults() {
                AttackerMelee  = modifierLogic.GetMeleeOffensiveModifierAtLocation (attacker, defender, location),
                AttackerRanged = modifierLogic.GetRangedOffensiveModifierAtLocation(attacker, defender, location),

                DefenderMelee  = modifierLogic.GetMeleeDefensiveModifierAtLocation (attacker, defender, location),
                DefenderRanged = modifierLogic.GetRangedDefensiveModifierAtLocation(attacker, defender, location)
            };
        }

        [Test(Description = "Defensive ranged and melee combat modifiers should be affected by " +
            "the DefensiveBonus of any improvements at the argued location")]
        public void DefensiveCombatModifiers_ModifiedByImprovements() {
            var location = BuildCell(TerrainType.Grassland, TerrainFeature.None, false);

            var attacker = BuildUnit(BuildCivilization(0));
            var defender = BuildUnit(BuildCivilization(0));

            BuildImprovement(location, 0.5f);

            var modifierLogic = Container.Resolve<CombatModifierLogic>();

            Assert.AreEqual(3f, modifierLogic.GetMeleeDefensiveModifierAtLocation(attacker, defender, location),
                "Melee defense did not take improvements into account");

            Assert.AreEqual(21f, modifierLogic.GetRangedDefensiveModifierAtLocation(attacker, defender, location),
                "Ranged defense did not take improvements into account");
        }

        #endregion

        #region utilities

        private IHexCell BuildCell(TerrainType terrain, TerrainFeature feature, bool hasRiver) {
            var mockCell = new Mock<IHexCell>();

            mockCell.SetupAllProperties();

            var newCell = mockCell.Object;

            newCell.Terrain = terrain;
            newCell.Feature = feature;

            MockRiverCanon.Setup(canon => canon.HasRiver(newCell)).Returns(hasRiver);

            return newCell;
        }

        private IUnit BuildUnit(ICivilization owner) {
            var newUnit = new Mock<IUnit>().Object;

            MockUnitPossessionCanon.Setup(canon => canon.GetOwnerOfPossession(newUnit)).Returns(owner);

            return newUnit;
        }

        private IImprovement BuildImprovement(IHexCell location, float defensiveBonus) {
            var mockImprovement = new Mock<IImprovement>();

            var mockTemplate = new Mock<IImprovementTemplate>();
            mockTemplate.Setup(template => template.DefensiveBonus).Returns(defensiveBonus);

            mockImprovement.Setup(improvement => improvement.Template).Returns(mockTemplate.Object);

            var newImprovement = mockImprovement.Object;

            MockImprovementLocationCanon.Setup(canon => canon.GetPossessionsOfOwner(location))
                .Returns(new List<IImprovement>() { newImprovement });

            return newImprovement;
        }

        private ICivilization BuildCivilization(int netHappiness) {
            var newCiv = new Mock<ICivilization>().Object;

            MockCivilizationHappinessLogic.Setup(logic => logic.GetNetHappinessOfCiv(newCiv)).Returns(netHappiness);

            return newCiv;
        }

        #endregion

        #endregion

    }

}
