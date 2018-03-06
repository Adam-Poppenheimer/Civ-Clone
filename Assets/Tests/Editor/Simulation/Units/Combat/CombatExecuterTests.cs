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

        public class CombatTestInput {

            public int AttackerCombatStrength;

            public int AttackerRangedAttackStrength;

            public int AttackerCurrentMovement;

            public int AttackerRange;

            public int AttackerLocationElevation;

            public int AttackerDistanceFromDefender;

            public bool AttackerCanMoveToDefender = true;

            public bool AttackerCanSeeDefender = true;

            public float AttackerCombatModifier;

            



            public int DefenderCombatStrength;

            public float DefenderCombatModifier;

            public TerrainType DefenderLocationTerrain;

            public TerrainFeature DefenderLocationFeature;

            public TerrainShape DefenderLocationShape;

            public int DefenderLocationElevation;

            public bool UnitsHaveSameOwner;

        }

        public struct CombatTestOutput {

            public bool CanPerformMeleeAttack;
            public int  MeleeAttackerHealthLeft;
            public int  MeleeDefenderHealthLeft;

            public bool CanPerformRangedAttack;
            public int  RangedAttackerHealthLeft;
            public int  RangedDefenderHealthLeft;

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
                yield return new TestCaseData(new CombatTestInput() {
                    AttackerCombatStrength = 100,
                    AttackerRangedAttackStrength = 100,
                    AttackerCurrentMovement = 1,
                    AttackerRange = 1,
                    AttackerLocationElevation = 0,
                    AttackerDistanceFromDefender = 1,
                    AttackerCanMoveToDefender = true,
                    AttackerCanSeeDefender = true,
                    AttackerCombatModifier = 0f,

                    DefenderCombatStrength = 100,
                    DefenderLocationElevation = 0,
                    DefenderLocationTerrain = TerrainType.Grassland,
                    DefenderLocationFeature = TerrainFeature.None,
                    DefenderCombatModifier = 0f

                }).SetName("Basic terrain/no inhibitors/no modifiers/even strength")
                .Returns(new CombatTestOutput() {
                    CanPerformMeleeAttack = true,
                    MeleeAttackerHealthLeft = 70,
                    MeleeDefenderHealthLeft = 70,
                    
                    CanPerformRangedAttack = true,
                    RangedAttackerHealthLeft = 100,
                    RangedDefenderHealthLeft = 70
                });

                yield return new TestCaseData(new CombatTestInput() {
                    AttackerCombatStrength = 150,
                    AttackerRangedAttackStrength = 150,
                    AttackerCurrentMovement = 1,
                    AttackerRange = 1,
                    AttackerLocationElevation = 0,
                    AttackerDistanceFromDefender = 1,
                    AttackerCanMoveToDefender = true,
                    AttackerCanSeeDefender = true,
                    AttackerCombatModifier = 0f,

                    DefenderCombatStrength = 100,
                    DefenderLocationElevation = 0,
                    DefenderLocationTerrain = TerrainType.Grassland,
                    DefenderLocationFeature = TerrainFeature.None,
                    DefenderCombatModifier = 0f

                }).SetName("Basic terrain/no inhibitors/no modifiers/1.5 attacker strength advantage")
                .Returns(new CombatTestOutput() {
                    CanPerformMeleeAttack = true,
                    MeleeAttackerHealthLeft = 80,
                    MeleeDefenderHealthLeft = 55,
                    
                    CanPerformRangedAttack = true,
                    RangedAttackerHealthLeft = 100,
                    RangedDefenderHealthLeft = 55
                });

                yield return new TestCaseData(new CombatTestInput() {
                    AttackerCombatStrength = 175,
                    AttackerRangedAttackStrength = 175,
                    AttackerCurrentMovement = 1,
                    AttackerRange = 1,
                    AttackerLocationElevation = 0,
                    AttackerDistanceFromDefender = 1,
                    AttackerCanMoveToDefender = true,
                    AttackerCanSeeDefender = true,
                    AttackerCombatModifier = 0f,

                    DefenderCombatStrength = 100,
                    DefenderLocationElevation = 0,
                    DefenderLocationTerrain = TerrainType.Grassland,
                    DefenderLocationFeature = TerrainFeature.None,
                    DefenderCombatModifier = 0f

                }).SetName("Basic terrain/no inhibitors/no modifiers/1.75 attacker strength advantage")
                .Returns(new CombatTestOutput() {
                    CanPerformMeleeAttack = true,
                    MeleeAttackerHealthLeft = 83,
                    MeleeDefenderHealthLeft = 48,
                    
                    CanPerformRangedAttack = true,
                    RangedAttackerHealthLeft = 100,
                    RangedDefenderHealthLeft = 48
                });

                yield return new TestCaseData(new CombatTestInput() {
                    AttackerCombatStrength = 200,
                    AttackerRangedAttackStrength = 200,
                    AttackerCurrentMovement = 1,
                    AttackerRange = 1,
                    AttackerLocationElevation = 0,
                    AttackerDistanceFromDefender = 1,
                    AttackerCanMoveToDefender = true,
                    AttackerCanSeeDefender = true,
                    AttackerCombatModifier = 0f,

                    DefenderCombatStrength = 100,
                    DefenderLocationElevation = 0,
                    DefenderLocationTerrain = TerrainType.Grassland,
                    DefenderLocationFeature = TerrainFeature.None,
                    DefenderCombatModifier = 0f

                }).SetName("Basic terrain/no inhibitors/no modifiers/2.0 attacker strength advantage")
                .Returns(new CombatTestOutput() {
                    CanPerformMeleeAttack = true,
                    MeleeAttackerHealthLeft = 85,
                    MeleeDefenderHealthLeft = 40,
                    
                    CanPerformRangedAttack = true,
                    RangedAttackerHealthLeft = 100,
                    RangedDefenderHealthLeft = 40
                });

                yield return new TestCaseData(new CombatTestInput() {
                    AttackerCombatStrength = 250,
                    AttackerRangedAttackStrength = 250,
                    AttackerCurrentMovement = 1,
                    AttackerRange = 1,
                    AttackerLocationElevation = 0,
                    AttackerDistanceFromDefender = 1,
                    AttackerCanMoveToDefender = true,
                    AttackerCanSeeDefender = true,
                    AttackerCombatModifier = 0f,

                    DefenderCombatStrength = 100,
                    DefenderLocationElevation = 0,
                    DefenderLocationTerrain = TerrainType.Grassland,
                    DefenderLocationFeature = TerrainFeature.None,
                    DefenderCombatModifier = 0f

                }).SetName("Basic terrain/no inhibitors/no modifiers/2.5 attacker strength advantage")
                .Returns(new CombatTestOutput() {
                    CanPerformMeleeAttack = true,
                    MeleeAttackerHealthLeft = 88,
                    MeleeDefenderHealthLeft = 25,
                    
                    CanPerformRangedAttack = true,
                    RangedAttackerHealthLeft = 100,
                    RangedDefenderHealthLeft = 25
                });

                yield return new TestCaseData(new CombatTestInput() {
                    AttackerCombatStrength = 300,
                    AttackerRangedAttackStrength = 300,
                    AttackerCurrentMovement = 1,
                    AttackerRange = 1,
                    AttackerLocationElevation = 0,
                    AttackerDistanceFromDefender = 1,
                    AttackerCanMoveToDefender = true,
                    AttackerCanSeeDefender = true,
                    AttackerCombatModifier = 0f,

                    DefenderCombatStrength = 100,
                    DefenderLocationElevation = 0,
                    DefenderLocationTerrain = TerrainType.Grassland,
                    DefenderLocationFeature = TerrainFeature.None,
                    DefenderCombatModifier = 0f

                }).SetName("Basic terrain/no inhibitors/no modifiers/3 attacker strength advantage")
                .Returns(new CombatTestOutput() {
                    CanPerformMeleeAttack = true,
                    MeleeAttackerHealthLeft = 90,
                    MeleeDefenderHealthLeft = 10,
                    
                    CanPerformRangedAttack = true,
                    RangedAttackerHealthLeft = 100,
                    RangedDefenderHealthLeft = 10
                });

                yield return new TestCaseData(new CombatTestInput() {
                    AttackerCombatStrength = 400,
                    AttackerRangedAttackStrength = 400,
                    AttackerCurrentMovement = 1,
                    AttackerRange = 1,
                    AttackerLocationElevation = 0,
                    AttackerDistanceFromDefender = 1,
                    AttackerCanMoveToDefender = true,
                    AttackerCanSeeDefender = true,
                    AttackerCombatModifier = 0f,

                    DefenderCombatStrength = 100,
                    DefenderLocationElevation = 0,
                    DefenderLocationTerrain = TerrainType.Grassland,
                    DefenderLocationFeature = TerrainFeature.None,
                    DefenderCombatModifier = 0f

                }).SetName("Basic terrain/no inhibitors/no modifiers/4 attacker strength advantage")
                .Returns(new CombatTestOutput() {
                    CanPerformMeleeAttack = true,
                    MeleeAttackerHealthLeft = 92,
                    MeleeDefenderHealthLeft = 0,
                    
                    CanPerformRangedAttack = true,
                    RangedAttackerHealthLeft = 100,
                    RangedDefenderHealthLeft = 0
                });

                yield return new TestCaseData(new CombatTestInput() {
                    AttackerCombatStrength = 100,
                    AttackerRangedAttackStrength = 100,
                    AttackerCurrentMovement = 1,
                    AttackerRange = 1,
                    AttackerLocationElevation = 0,
                    AttackerDistanceFromDefender = 1,
                    AttackerCanMoveToDefender = true,
                    AttackerCanSeeDefender = true,
                    AttackerCombatModifier = 0.5f,

                    DefenderCombatStrength = 100,
                    DefenderLocationElevation = 0,
                    DefenderLocationTerrain = TerrainType.Grassland,
                    DefenderLocationFeature = TerrainFeature.None,
                    DefenderCombatModifier = 0f

                }).SetName("Basic terrain/no inhibitors/no modifiers/even strength/50% attacker modifier")
                .Returns(new CombatTestOutput() {
                    CanPerformMeleeAttack = true,
                    MeleeAttackerHealthLeft = 80,
                    MeleeDefenderHealthLeft = 55,
                    
                    CanPerformRangedAttack = true,
                    RangedAttackerHealthLeft = 100,
                    RangedDefenderHealthLeft = 55
                });

                yield return new TestCaseData(new CombatTestInput() {
                    AttackerCombatStrength = 100,
                    AttackerRangedAttackStrength = 100,
                    AttackerCurrentMovement = 1,
                    AttackerRange = 1,
                    AttackerLocationElevation = 0,
                    AttackerDistanceFromDefender = 1,
                    AttackerCanMoveToDefender = true,
                    AttackerCanSeeDefender = true,
                    AttackerCombatModifier = 0f,

                    DefenderCombatStrength = 100,
                    DefenderLocationElevation = 0,
                    DefenderLocationTerrain = TerrainType.Grassland,
                    DefenderLocationFeature = TerrainFeature.None,
                    DefenderCombatModifier = 0.5f

                }).SetName("Basic terrain/no inhibitors/no modifiers/even strength/50% defender modifier")
                .Returns(new CombatTestOutput() {
                    CanPerformMeleeAttack = true,
                    MeleeAttackerHealthLeft = 55,
                    MeleeDefenderHealthLeft = 80,
                    
                    CanPerformRangedAttack = true,
                    RangedAttackerHealthLeft = 100,
                    RangedDefenderHealthLeft = 80
                });

                yield return new TestCaseData(new CombatTestInput() {
                    AttackerCombatStrength = 100,
                    AttackerRangedAttackStrength = 100,
                    AttackerCurrentMovement = 0,
                    AttackerRange = 1,
                    AttackerLocationElevation = 0,
                    AttackerDistanceFromDefender = 1,
                    AttackerCanMoveToDefender = true,
                    AttackerCanSeeDefender = true,
                    AttackerCombatModifier = 0f,

                    DefenderCombatStrength = 100,
                    DefenderLocationElevation = 0,
                    DefenderLocationTerrain = TerrainType.Grassland,
                    DefenderLocationFeature = TerrainFeature.None,
                    DefenderCombatModifier = 0f

                }).SetName("Basic terrain/no attacker movement/no modifiers/even strength")
                .Returns(new CombatTestOutput() {
                    CanPerformMeleeAttack = false,
                    MeleeAttackerHealthLeft = 100,
                    MeleeDefenderHealthLeft = 100,
                    
                    CanPerformRangedAttack = false,
                    RangedAttackerHealthLeft = 100,
                    RangedDefenderHealthLeft = 100
                });

                yield return new TestCaseData(new CombatTestInput() {
                    AttackerCombatStrength = 100,
                    AttackerRangedAttackStrength = 100,
                    AttackerCurrentMovement = 1,
                    AttackerRange = 0,
                    AttackerLocationElevation = 0,
                    AttackerDistanceFromDefender = 1,
                    AttackerCanMoveToDefender = true,
                    AttackerCanSeeDefender = true,
                    AttackerCombatModifier = 0f,

                    DefenderCombatStrength = 100,
                    DefenderLocationElevation = 0,
                    DefenderLocationTerrain = TerrainType.Grassland,
                    DefenderLocationFeature = TerrainFeature.None,
                    DefenderCombatModifier = 0f

                }).SetName("Basic terrain/no attacker range/no modifiers/even strength")
                .Returns(new CombatTestOutput() {
                    CanPerformMeleeAttack = true,
                    MeleeAttackerHealthLeft = 70,
                    MeleeDefenderHealthLeft = 70,
                    
                    CanPerformRangedAttack = false,
                    RangedAttackerHealthLeft = 100,
                    RangedDefenderHealthLeft = 100
                });

                yield return new TestCaseData(new CombatTestInput() {
                    AttackerCombatStrength = 100,
                    AttackerRangedAttackStrength = 100,
                    AttackerCurrentMovement = 1,
                    AttackerRange = 1,
                    AttackerLocationElevation = 0,
                    AttackerDistanceFromDefender = 1,
                    AttackerCanMoveToDefender = true,
                    AttackerCanSeeDefender = true,
                    AttackerCombatModifier = 0f,

                    DefenderCombatStrength = 100,
                    DefenderLocationElevation = 1,
                    DefenderLocationTerrain = TerrainType.Grassland,
                    DefenderLocationFeature = TerrainFeature.None,
                    DefenderCombatModifier = 0f

                }).SetName("Basic terrain/attacking up slope/no modifiers/even strength")
                .Returns(new CombatTestOutput() {
                    CanPerformMeleeAttack = true,
                    MeleeAttackerHealthLeft = 70,
                    MeleeDefenderHealthLeft = 70,
                    
                    CanPerformRangedAttack = true,
                    RangedAttackerHealthLeft = 100,
                    RangedDefenderHealthLeft = 70
                });

                yield return new TestCaseData(new CombatTestInput() {
                    AttackerCombatStrength = 100,
                    AttackerRangedAttackStrength = 100,
                    AttackerCurrentMovement = 1,
                    AttackerRange = 1,
                    AttackerLocationElevation = 0,
                    AttackerDistanceFromDefender = 1,
                    AttackerCanMoveToDefender = true,
                    AttackerCanSeeDefender = true,
                    AttackerCombatModifier = 0f,

                    DefenderCombatStrength = 100,
                    DefenderLocationElevation = 2,
                    DefenderLocationTerrain = TerrainType.Grassland,
                    DefenderLocationFeature = TerrainFeature.None,
                    DefenderCombatModifier = 0f

                }).SetName("Basic terrain/attacking up cliff/no modifiers/even strength")
                .Returns(new CombatTestOutput() {
                    CanPerformMeleeAttack = false,
                    MeleeAttackerHealthLeft = 100,
                    MeleeDefenderHealthLeft = 100,
                    
                    CanPerformRangedAttack = true,
                    RangedAttackerHealthLeft = 100,
                    RangedDefenderHealthLeft = 70
                });

                yield return new TestCaseData(new CombatTestInput() {
                    AttackerCombatStrength = 100,
                    AttackerRangedAttackStrength = 100,
                    AttackerCurrentMovement = 1,
                    AttackerRange = 1,
                    AttackerLocationElevation = 2,
                    AttackerDistanceFromDefender = 1,
                    AttackerCanMoveToDefender = true,
                    AttackerCanSeeDefender = true,
                    AttackerCombatModifier = 0f,

                    DefenderCombatStrength = 100,
                    DefenderLocationElevation = 0,
                    DefenderLocationTerrain = TerrainType.Grassland,
                    DefenderLocationFeature = TerrainFeature.None,
                    DefenderCombatModifier = 0f

                }).SetName("Basic terrain/attacking down cliff/no modifiers/even strength")
                .Returns(new CombatTestOutput() {
                    CanPerformMeleeAttack = false,
                    MeleeAttackerHealthLeft = 100,
                    MeleeDefenderHealthLeft = 100,
                    
                    CanPerformRangedAttack = true,
                    RangedAttackerHealthLeft = 100,
                    RangedDefenderHealthLeft = 70
                });

                yield return new TestCaseData(new CombatTestInput() {
                    AttackerCombatStrength = 100,
                    AttackerRangedAttackStrength = 100,
                    AttackerCurrentMovement = 1,
                    AttackerRange = 1,
                    AttackerLocationElevation = 0,
                    AttackerDistanceFromDefender = 1,
                    AttackerCanMoveToDefender = false,
                    AttackerCanSeeDefender = true,
                    AttackerCombatModifier = 0f,

                    DefenderCombatStrength = 100,
                    DefenderLocationElevation = 0,
                    DefenderLocationTerrain = TerrainType.Grassland,
                    DefenderLocationFeature = TerrainFeature.None,
                    DefenderCombatModifier = 0f

                }).SetName("Basic terrain/attacker cannot move to defender/no modifiers/even strength")
                .Returns(new CombatTestOutput() {
                    CanPerformMeleeAttack = false,
                    MeleeAttackerHealthLeft = 100,
                    MeleeDefenderHealthLeft = 100,
                    
                    CanPerformRangedAttack = true,
                    RangedAttackerHealthLeft = 100,
                    RangedDefenderHealthLeft = 70
                });

                yield return new TestCaseData(new CombatTestInput() {
                    AttackerCombatStrength = 100,
                    AttackerRangedAttackStrength = 100,
                    AttackerCurrentMovement = 1,
                    AttackerRange = 1,
                    AttackerLocationElevation = 0,
                    AttackerDistanceFromDefender = 1,
                    AttackerCanMoveToDefender = true,
                    AttackerCanSeeDefender = false,
                    AttackerCombatModifier = 0f,

                    DefenderCombatStrength = 100,
                    DefenderLocationElevation = 0,
                    DefenderLocationTerrain = TerrainType.Grassland,
                    DefenderLocationFeature = TerrainFeature.None,
                    DefenderCombatModifier = 0f

                }).SetName("Basic terrain/attacker cannot see defender/no modifiers/even strength")
                .Returns(new CombatTestOutput() {
                    CanPerformMeleeAttack = false,
                    MeleeAttackerHealthLeft = 100,
                    MeleeDefenderHealthLeft = 100,
                    
                    CanPerformRangedAttack = false,
                    RangedAttackerHealthLeft = 100,
                    RangedDefenderHealthLeft = 100
                });

                yield return new TestCaseData(new CombatTestInput() {
                    AttackerCombatStrength = 100,
                    AttackerRangedAttackStrength = 100,
                    AttackerCurrentMovement = 1,
                    AttackerRange = 1,
                    AttackerLocationElevation = 0,
                    AttackerDistanceFromDefender = 1,
                    AttackerCanMoveToDefender = true,
                    AttackerCanSeeDefender = true,
                    AttackerCombatModifier = 0f,

                    DefenderCombatStrength = 0,
                    DefenderLocationElevation = 0,
                    DefenderLocationTerrain = TerrainType.Grassland,
                    DefenderLocationFeature = TerrainFeature.None,
                    DefenderCombatModifier = 0f

                }).SetName("Basic terrain/no inhibitors/no modifiers/zero-strength defender")
                .Returns(new CombatTestOutput() {
                    CanPerformMeleeAttack = true,
                    MeleeAttackerHealthLeft = 100,
                    MeleeDefenderHealthLeft = 0,
                    
                    CanPerformRangedAttack = true,
                    RangedAttackerHealthLeft = 100,
                    RangedDefenderHealthLeft = 0
                });

                yield return new TestCaseData(new CombatTestInput() {
                    AttackerCombatStrength = 0,
                    AttackerRangedAttackStrength = 0,
                    AttackerCurrentMovement = 1,
                    AttackerRange = 1,
                    AttackerLocationElevation = 0,
                    AttackerDistanceFromDefender = 1,
                    AttackerCanMoveToDefender = true,
                    AttackerCanSeeDefender = true,
                    AttackerCombatModifier = 0f,

                    DefenderCombatStrength = 100,
                    DefenderLocationElevation = 0,
                    DefenderLocationTerrain = TerrainType.Grassland,
                    DefenderLocationFeature = TerrainFeature.None,
                    DefenderCombatModifier = 0f

                }).SetName("Basic terrain/no inhibitors/no modifiers/zero-strength attacker")
                .Returns(new CombatTestOutput() {
                    CanPerformMeleeAttack = false,
                    MeleeAttackerHealthLeft = 100,
                    MeleeDefenderHealthLeft = 100,
                    
                    CanPerformRangedAttack = false,
                    RangedAttackerHealthLeft = 100,
                    RangedDefenderHealthLeft = 100
                });

                yield return new TestCaseData(new CombatTestInput() {
                    AttackerCombatStrength = 100,
                    AttackerRangedAttackStrength = 100,
                    AttackerCurrentMovement = 1,
                    AttackerRange = 1,
                    AttackerLocationElevation = 0,
                    AttackerDistanceFromDefender = 1,
                    AttackerCanMoveToDefender = true,
                    AttackerCanSeeDefender = true,
                    AttackerCombatModifier = 0f,

                    DefenderCombatStrength = 100,
                    DefenderLocationElevation = 0,
                    DefenderLocationTerrain = TerrainType.Grassland,
                    DefenderLocationFeature = TerrainFeature.None,
                    DefenderCombatModifier = 0f,

                    UnitsHaveSameOwner = true

                }).SetName("Basic terrain/no inhibitors/no modifiers/even strength/same owner")
                .Returns(new CombatTestOutput() {
                    CanPerformMeleeAttack = false,
                    MeleeAttackerHealthLeft = 100,
                    MeleeDefenderHealthLeft = 100,
                    
                    CanPerformRangedAttack = false,
                    RangedAttackerHealthLeft = 100,
                    RangedDefenderHealthLeft = 100
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
        public CombatTestOutput PerformCombatTests(CombatTestInput input) {
            IHexCell attackerLocation = BuildCell(input.AttackerLocationElevation);
            IHexCell defenderLocation = BuildCell(
                input.DefenderLocationTerrain, input.DefenderLocationFeature,
                input.DefenderLocationShape,   input.DefenderLocationElevation
            );

            IUnit meleeAttacker = BuildUnit(
                attackerLocation, input.AttackerCurrentMovement, input.AttackerCombatStrength,
                input.AttackerRangedAttackStrength, input.AttackerCombatModifier, 0f, input.AttackerRange
            );
            IUnit meleeDefender = BuildUnit(
                defenderLocation, input.DefenderCombatStrength, 0f, input.DefenderCombatModifier
            );

            IUnit rangedAttacker = BuildUnit(
                attackerLocation, input.AttackerCurrentMovement, input.AttackerCombatStrength,
                input.AttackerRangedAttackStrength, input.AttackerCombatModifier, 0f, input.AttackerRange
            );
            IUnit rangedDefender = BuildUnit(
                defenderLocation, input.DefenderCombatStrength, 0f, input.DefenderCombatModifier
            );

            SetCombatConditions(
                input.AttackerDistanceFromDefender, input.AttackerCanMoveToDefender,
                input.AttackerCanSeeDefender
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

            results.MeleeAttackerHealthLeft = meleeAttacker.Health;
            results.MeleeDefenderHealthLeft = meleeDefender.Health;
            
            results.CanPerformRangedAttack = combatExecuter.CanPerformRangedAttack(rangedAttacker, rangedDefender);

            if(results.CanPerformRangedAttack) {
                combatExecuter.PerformRangedAttack(rangedAttacker, rangedDefender);
            }

            results.RangedAttackerHealthLeft = rangedAttacker.Health;
            results.RangedDefenderHealthLeft = rangedDefender.Health;

            return results;
        }


        #endregion

        #region utilities

        private void SetCombatConditions(int distanceBetween, bool attackerCanMoveTo, bool defenderCanBeSeen) {
            MockGrid.Setup(grid => grid.GetDistance(It.IsAny<IHexCell>(), It.IsAny<IHexCell>()))
                .Returns(distanceBetween);

            MockUnitPositionCanon.Setup(canon => canon.CanPlaceUnitOfTypeAtLocation(It.IsAny<UnitType>(), It.IsAny<IHexCell>(), true))
                .Returns(attackerCanMoveTo);

            MockLineOfSightLogic.Setup(logic => logic.CanUnitSeeCell(It.IsAny<IUnit>(), It.IsAny<IHexCell>()))
                .Returns(defenderCanBeSeen);
        }

        private IUnit BuildUnit(
            IHexCell location, int combatStrength,
            float attackModifier, float defenseModifier
        ){
            return BuildUnit(location, 0, combatStrength, 0, attackModifier, defenseModifier, 0);
        }

        private IUnit BuildUnit(
            IHexCell location, int currentMovement,
            int combatStrength, int rangedAttackStrength,
            float attackModifier, float defenseModifier,
            int attackRange
        ){
            var mockUnit = new Mock<IUnit>();

            mockUnit.SetupAllProperties();

            mockUnit.Setup(unit => unit.CombatStrength)      .Returns(combatStrength);
            mockUnit.Setup(unit => unit.RangedAttackStrength).Returns(rangedAttackStrength);
            mockUnit.Setup(unit => unit.AttackRange)         .Returns(attackRange);
            mockUnit.Setup(unit => unit.CurrentMovement)     .Returns(currentMovement);            

            var newUnit = mockUnit.Object;

            newUnit.Health = UnitConfig.MaxHealth;

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
