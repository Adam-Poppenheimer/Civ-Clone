﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.MapResources;
using Assets.Simulation.Units;
using Assets.Simulation.Units.Abilities;
using Assets.Simulation.Units.Promotions;

namespace Assets.Simulation.Cities {

    public class CityCombatantTemplate : IUnitTemplate {

        #region internal types

        private class EmptyPromotionTreeData : IPromotionTreeTemplate {

            #region static fields and properties

            public static EmptyPromotionTreeData Instance {
                get {
                    if(_instance == null) {
                        _instance = new EmptyPromotionTreeData();
                    }
                    return _instance;
                }
            }

            private static EmptyPromotionTreeData _instance;

            private static List<IPromotionPrerequisiteData> EmptyPromotionPrerequisites = new List<IPromotionPrerequisiteData>();

            #endregion

            #region instance fields and properties

            #region from IPromotionTreeData

            public string name {
                get { return ""; }
            }

            public IEnumerable<IPromotionPrerequisiteData> PrerequisiteData {
                get { return EmptyPromotionPrerequisites; }
            }

            #endregion

            #endregion

            #region constructors

            private EmptyPromotionTreeData() { }

            #endregion

        }

        #endregion

        #region instance fields and properties

        #region from IUnitTemplate

        public string name {
            get { return UnderlyingCity.Name; }
        }

        public string Description {
            get { return ""; }
        }

        public Sprite Image {
            get { return CityConfig.CombatantImage; }
        }

        public Sprite Icon {
            get { return CityConfig.CombatantIcon; }
        }

        public GameObject Prefab {
            get { return CityConfig.CombatantPrefab; }
        }

        public int ProductionCost {
            get { return 0; }
        }

        public int MaxMovement {
            get { return 1; }
        }

        public UnitType Type {
            get { return UnitType.City; }
        }

        public bool IsAquatic {
            get { return false; }
        }

        public bool CanMoveAfterAttacking {
            get { return false; }
        }

        public IEnumerable<IAbilityDefinition> Abilities {
            get { return _emptyAbilities; }
        }
        private static List<IAbilityDefinition> _emptyAbilities = new List<IAbilityDefinition>();

        public int AttackRange {
            get { return CityConfig.CityAttackRange; }
        }

        public int CombatStrength {
            get { return CombatLogic.GetCombatStrengthOfCity(UnderlyingCity); }
        }

        public int RangedAttackStrength {
            get { return CombatLogic.GetRangedAttackStrengthOfCity(UnderlyingCity); }
        }

        public int MaxHitpoints {
            get { return CombatLogic.GetMaxHitpointsOfCity(UnderlyingCity); }
        }

        public IEnumerable<IResourceDefinition> RequiredResources {
            get { return _emptyResources; }
        }
        private static List<IResourceDefinition> _emptyResources = new List<IResourceDefinition>();

        public bool BenefitsFromDefensiveTerrain {
            get { return false; }
        }

        public bool IgnoresTerrainCosts {
            get { return false; }
        }

        public bool HasRoughTerrainPenalty {
            get { return false; }
        }

        public IEnumerable<UnitTypeCombatBonusData> CombatBonusesByType {
            get { return _emptyBonuses; }
        }
        private static List<UnitTypeCombatBonusData> _emptyBonuses = new List<UnitTypeCombatBonusData>();

        public bool MustSetUpToBombard {
            get { return false; }
        }

        public int VisionRange {
            get { return 0; }
        }

        public bool IgnoresLineOfSight {
            get { return true; }
        }

        public IEnumerable<IPromotion> StartingPromotions {
            get { return _startingPromotions; }
        }
        private List<IPromotion> _startingPromotions = new List<IPromotion>();

        public IPromotionTreeTemplate PromotionTreeData {
            get { return EmptyPromotionTreeData.Instance; }
        }


        public IUnitMovementSummary MovementSummary {
            get { return _movementSummary; }
        }
        private IUnitMovementSummary _movementSummary = new UnitMovementSummary();

        #endregion



        private ICity UnderlyingCity;

        private ICityConfig CityConfig;

        private ICityCombatLogic CombatLogic;

        private UnitSignals Signals;

        #endregion

        #region constructors

        public CityCombatantTemplate(
            ICity underlyingCity, ICityConfig cityConfig, ICityCombatLogic combatLogic
        ){
            UnderlyingCity = underlyingCity;
            CityConfig     = cityConfig;
            CombatLogic    = combatLogic;
        }

        #endregion

    }

}
