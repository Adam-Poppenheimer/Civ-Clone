using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Assets.Simulation.HexMap;
using Assets.Simulation.Units.Combat;

namespace Assets.Simulation.Units.Promotions {

    public class Promotion : ScriptableObject, IPromotion {

        #region instance fields and properties

        #region from IPromotion

        public string Name {
            get { return name; }
        }

        public string Description {
            get { return _description; }
        }
        [SerializeField, TextArea(minLines: 5, maxLines: 10)] private string _description;

        public Sprite Icon {
            get { return _icon; }
        }
        [SerializeField] private Sprite _icon;

        #endregion

        [SerializeField] private bool RestrictedByTerrains;
        [SerializeField] private List<TerrainType> ValidTerrains;

        [SerializeField] private bool RestrictedByShapes;
        [SerializeField] private List<TerrainShape> ValidShapes;

        [SerializeField] private bool RestrictedByFeatures;
        [SerializeField] private List<TerrainFeature> ValidFeatures;

        [SerializeField] private bool RestrictedByAttackerTypes;
        [SerializeField] private List<UnitType> ValidAttackerTypes;

        [SerializeField] private bool RestrictedByDefenderTypes;
        [SerializeField] private List<UnitType> ValidDefenderTypes;

        [SerializeField] private bool RequiresFlatTerrain;
        [SerializeField] private bool RequiresRoughTerrain;

        [SerializeField] private bool RestrictedByCombatType;
        [SerializeField] private CombatType ValidCombatType;

        [SerializeField] private bool AppliesWhileAttacking;
        [SerializeField] private bool AppliesWhileDefending;

        [SerializeField] private float CombatModifier;

        [SerializeField] private bool CanMoveAfterAttacking;
        [SerializeField] private bool CanAttackAfterAttacking;
        [SerializeField] private bool IgnoresAmphibiousPenalty;

        [SerializeField] private bool IgnoresDefensiveTerrainBonuses;

        [SerializeField] private float GoldRaidingPercentage;

        [SerializeField] private bool IgnoresLOSWhenAttacking;


        [SerializeField] private bool IgnoresTerrainCosts;

        #endregion

        #region instance methods

        #region from IPromotion

        public void ModifyCombatInfoForAttacker(
            IUnit attacker, IUnit defender, IHexCell location,
            CombatType combatType, CombatInfo combatInfo
        ){
            if( CombatMeetsGenericConditions (attacker, defender, location, combatType) &&
                AppliesWhileAttacking
            ){
                PerformInfoModification(combatInfo.Attacker);
            }
        }

        public void ModifyCombatInfoForDefender(
            IUnit attacker, IUnit defender, IHexCell location,
            CombatType combatType, CombatInfo combatInfo
        ){
            if( CombatMeetsGenericConditions (attacker, defender, location, combatType) &&
                AppliesWhileDefending
            ){
                PerformInfoModification(combatInfo.Defender);
            }
        }

        public void ModifyMovementInfo(IUnit unit, MovementInfo movementInfo) {
            
        }

        #endregion

        private bool CombatMeetsGenericConditions(IUnit attacker, IUnit defender, IHexCell location, CombatType combatType) {
            if(RestrictedByTerrains && !ValidTerrains.Contains(location.Terrain)) {
                return false;

            }else if(RestrictedByShapes && !ValidShapes.Contains(location.Shape)) {
                return false;

            }else if(RestrictedByFeatures && !ValidFeatures.Contains(location.Feature)) {
                return false;

            }else if(RestrictedByAttackerTypes && !ValidAttackerTypes.Contains(attacker.Type)) {
                return false;

            }else if(RestrictedByDefenderTypes && !ValidDefenderTypes.Contains(defender.Type)) {
                return false;

            }else if(RequiresFlatTerrain && location.IsRoughTerrain) {
                return false;

            }else if(RequiresRoughTerrain && !location.IsRoughTerrain) {
                return false;

            }else if(RestrictedByCombatType && combatType != ValidCombatType) {
                return false;

            }else {
                return true;
            }
        }

        private void PerformInfoModification(UnitCombatInfo unitInfo) {
            unitInfo.CombatModifier += CombatModifier;

            unitInfo.CanMoveAfterAttacking    |= CanMoveAfterAttacking;
            unitInfo.CanAttackAfterAttacking  |= CanAttackAfterAttacking;
            unitInfo.IgnoresAmphibiousPenalty |= IgnoresAmphibiousPenalty;

            unitInfo.IgnoresDefensiveTerrainBonuses |= IgnoresDefensiveTerrainBonuses;

            unitInfo.GoldRaidingPercentage += GoldRaidingPercentage;
        }

        #endregion

    }

}
