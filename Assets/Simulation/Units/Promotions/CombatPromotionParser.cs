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
            if( CombatMeetsGenericConditions(promotion, attacker, defender, location, combatInfo.CombatType) &&
                promotion.AppliesWhileAttacking
            ) {
                PerformInfoModification(promotion, combatInfo.Attacker);
            }
        }

        public void ParsePromotionForDefender(
            IPromotion promotion, IUnit attacker, IUnit defender,
            IHexCell location, CombatInfo combatInfo
        ){
            if( CombatMeetsGenericConditions(promotion, attacker, defender, location, combatInfo.CombatType) &&
                promotion.AppliesWhileDefending
            ) {
                PerformInfoModification(promotion, combatInfo.Defender);
            }
        }

        #endregion

        private bool CombatMeetsGenericConditions(
            IPromotion promotion, IUnit attacker, IUnit defender,
            IHexCell location, CombatType combatType
        ){
            if(promotion.RestrictedByTerrains && !promotion.ValidTerrains.Contains(location.Terrain)) {
                return false;

            }else if(promotion.RestrictedByShapes && !promotion.ValidShapes.Contains(location.Shape)) {
                return false;

            }else if(promotion.RestrictedByFeatures && !promotion.ValidFeatures.Contains(location.Feature)) {
                return false;

            }else if(promotion.RestrictedByAttackerTypes && !promotion.ValidAttackerTypes.Contains(attacker.Type)) {
                return false;

            }else if(promotion.RestrictedByDefenderTypes && !promotion.ValidDefenderTypes.Contains(defender.Type)) {
                return false;

            }else if(promotion.RequiresFlatTerrain && location.IsRoughTerrain) {
                return false;

            }else if(promotion.RequiresRoughTerrain && !location.IsRoughTerrain) {
                return false;

            }else if(promotion.RestrictedByCombatType && combatType != promotion.ValidCombatType) {
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
        }

        #endregion
        
    }

}
