using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using Zenject;
using Moq;

using Assets.Simulation.Units;
using Assets.Simulation.Units.Combat;
using Assets.Simulation.HexMap;
using System.Collections.ObjectModel;

namespace Assets.Tests.Simulation.Units.Combat {

    [TestFixture]
    public class CombatModifierLogicTests : ZenjectUnitTestFixture {

        #region internal types

        private class MockUnitConfig : IUnitConfig {

            #region instance fields and properties

            public ReadOnlyCollection<float> FeatureMeleeDefensiveness {
                get { return _featureMeleeDefensiveness.AsReadOnly(); }
            }
            private List<float> _featureMeleeDefensiveness = new List<float>() {
                1f, 2f
            };

            public ReadOnlyCollection<float> FeatureRangedDefensiveness {
                get { return _featureRangedDefensiveness.AsReadOnly(); }
            }
            private List<float> _featureRangedDefensiveness = new List<float>() {
                10f, 20f
            };

            public int MaxHealth { get; set; }

            public float RiverCrossingAttackModifier {
                get { return -0.5f; }
            }

            public ReadOnlyCollection<float> TerrainMeleeDefensiveness {
                get { return _terrainMeleeDefensiveness.AsReadOnly(); }
            }
            private List<float> _terrainMeleeDefensiveness = new List<float>() {
                1.5f, 2.5f, 3.5f, 4.5f, 6.5f
            };

            public ReadOnlyCollection<float> TerrainRangedDefensiveness {
                get { return _terrainRangedDefensiveness.AsReadOnly(); }
            }
            private List<float> _terrainRangedDefensiveness = new List<float>() {
                10.5f, 20.5f, 30.5f, 40.5f, 50.5f
            };

            #endregion
            
        }

        public class ModifierTestArgs {

            public TerrainType Terrain;
            public TerrainFeature Feature;

            public bool HasRiver;

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
            }
        }

        #endregion

        #region instance fields and properties

        private Mock<IRiverCanon> MockRiverCanon;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockRiverCanon = new Mock<IRiverCanon>();

            Container.Bind<IRiverCanon>().FromInstance(MockRiverCanon.Object);

            Container.Bind<IUnitConfig>().To<MockUnitConfig>().AsSingle();

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

            var attacker = BuildUnit();
            var defender = BuildUnit();

            var modifierLogic = Container.Resolve<CombatModifierLogic>();

            return new ModifierTestResults() {
                AttackerMelee  = modifierLogic.GetMeleeOffensiveModifierAtLocation (attacker, defender, location),
                AttackerRanged = modifierLogic.GetRangedOffensiveModifierAtLocation(attacker, defender, location),

                DefenderMelee  = modifierLogic.GetMeleeDefensiveModifierAtLocation (attacker, defender, location),
                DefenderRanged = modifierLogic.GetRangedDefensiveModifierAtLocation(attacker, defender, location)
            };
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

        private IUnit BuildUnit() {
            return new Mock<IUnit>().Object;
        }

        #endregion

        #endregion

    }

}
