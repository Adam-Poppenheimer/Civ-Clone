using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.HexMap;
using Assets.Simulation.Units.Combat;

namespace Assets.Simulation.Units.Promotions {

    public class CombatPromotionParser : ICombatPromotionParser {

        #region instance methods

        #region from ICombatPromotionParser

        public void ParsePromotionForAttacker(
            IPromotion promotion, IUnit attacker, IUnit defender,
            IHexCell location, CombatInfo combatInfo
        ){
            if( CombatMeetsLocationRestrictions  (promotion, location)           &&
                CombatMeetsCombatInfoRestrictions(promotion, combatInfo)         &&
                CombatMeetsOpponentConditions    (promotion, attacker, defender) &&
                promotion.AppliesWhileAttacking
            ) {
                PerformInfoModification(promotion, combatInfo.Attacker);
            }
        }

        public void ParsePromotionForDefender(
            IPromotion promotion, IUnit attacker, IUnit defender,
            IHexCell location, CombatInfo combatInfo
        ){
            if( CombatMeetsLocationRestrictions  (promotion, location)           &&
                CombatMeetsCombatInfoRestrictions(promotion, combatInfo)         &&
                CombatMeetsOpponentConditions    (promotion, defender, attacker) &&
                promotion.AppliesWhileDefending
            ) {
                PerformInfoModification(promotion, combatInfo.Defender);
            }
        }

        #endregion

        private bool CombatMeetsLocationRestrictions(
            IPromotion promotion, IHexCell location
        ){
            if(promotion.RestrictedByTerrains && !promotion.ValidTerrains.Contains(location.Terrain)) {
                return false;

            }else if(promotion.RestrictedByShapes && !promotion.ValidShapes.Contains(location.Shape)) {
                return false;

            }else if(promotion.RestrictedByFeatures && !promotion.ValidFeatures.Contains(location.Feature)) {
                return false;

            }else if(promotion.RequiresFlatTerrain && location.IsRoughTerrain) {
                return false;

            }else if(promotion.RequiresRoughTerrain && !location.IsRoughTerrain) {
                return false;

            }else {
                return true;
            }
        }

        private bool CombatMeetsCombatInfoRestrictions(IPromotion promotion, CombatInfo info) {
            return !promotion.RestrictedByCombatType || promotion.ValidCombatType == info.CombatType;
        }

        private bool CombatMeetsOpponentConditions(IPromotion promotion, IUnit thisUnit, IUnit opponent) {
            if(promotion.RestrictedByOpponentTypes && !promotion.ValidOpponentTypes.Contains(opponent.Type)) {
                return false;

            }else if(promotion.RestrictedByOpponentWoundedState && promotion.ValidOpponentWoundedState != opponent.IsWounded) {
                return false;

            }else {
                return true;
            }
        }

        private void PerformInfoModification(IPromotion promotion, UnitCombatInfo unitInfo) {
            unitInfo.CombatModifier += promotion.CombatModifier;

            unitInfo.CanMoveAfterAttacking    |= promotion.CanMoveAfterAttacking;
            unitInfo.CanAttackAfterAttacking  |= promotion.CanAttackAfterAttacking;
            unitInfo.IgnoresAmphibiousPenalty |= promotion.IgnoresAmphibiousPenalty;

            unitInfo.IgnoresDefensiveTerrainBonuses |= promotion.IgnoresDefensiveTerrainBonuses;

            unitInfo.GoldRaidingPercentage += promotion.GoldRaidingPercentage;

            unitInfo.IgnoresLineOfSight |= promotion.IgnoresLineOfSight;
        }

        #endregion
        
    }

}
