using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.Units.Combat;

namespace Assets.Simulation.Units.Promotions {

    public class CombatPromotionParser : ICombatPromotionParser {

        #region instance methods

        #region from ICombatPromotionParser

        public void AddPromotionToCombatSummary(IPromotion promotion, UnitCombatSummary combatSummary) {
            combatSummary.CanMoveAfterAttacking   |= promotion.CanMoveAfterAttacking;
            combatSummary.CanAttackAfterAttacking |= promotion.CanAttackAfterAttacking;

            combatSummary.IgnoresAmphibiousPenalty     |= promotion.IgnoresAmphibiousPenalty;
            combatSummary.IgnoresDefensiveTerrainBonus |= promotion.IgnoresDefensiveTerrainBonuses;
            combatSummary.IgnoresLineOfSight           |= promotion.IgnoresLineOfSight;

            combatSummary.modifiersWhenAttacking.AddRange(promotion.ModifiersWhenAttacking);
            combatSummary.modifiersWhenDefending.AddRange(promotion.ModifiersWhenDefending);

            combatSummary.auraModifiersWhenAttacking.AddRange(promotion.AuraModifiersWhenAttacking);
            combatSummary.auraModifiersWhenDefending.AddRange(promotion.AuraModifiersWhenDefending);
        }

        #endregion

        #endregion
        
    }

}
