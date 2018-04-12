using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.HexMap;
using Assets.Simulation.Units.Combat;

namespace Assets.Simulation.Units.Promotions {

    public class PromotionParser : IPromotionParser {

        #region instance fields and properties

        

        #endregion

        #region instance methods

        #region from IPromotionParser

        public CombatInfo GetCombatInfo(IUnit attacker, IUnit defender, IHexCell location, CombatType combatType) {
            var retval = new CombatInfo();

            retval.CombatType = combatType;

            IEnumerable<IPromotion> attackerPromotions, defenderPromotions;
            if(combatType == CombatType.Melee) {
                attackerPromotions = GetMeleeAttackPromotions (attacker, defender, location);
                defenderPromotions = GetMeleeDefensePromotions(attacker, defender, location);
            }else {
                attackerPromotions = GetRangedAttackPromotions (attacker, defender, location);
                defenderPromotions = GetRangedDefensePromotions(attacker, defender, location);
            }

            foreach(var promotion in attackerPromotions) {
                retval.AttackerCanMoveAfterAttacking    |= promotion.HasArg(PromotionArgType.CanMoveAfterAttacking);
                retval.AttackerCanAttackAfterAttacking  |= promotion.HasArg(PromotionArgType.CanAttackAfterAttacking);
                retval.AttackerIgnoresAmphibiousPenalty |= promotion.HasArg(PromotionArgType.IgnoresAmphibiousPenalty);

                if(promotion.HasArg(PromotionArgType.CombatStrength)) {
                    retval.AttackerCombatModifier += promotion.GetFloat();
                }
            }

            foreach(var promotion in defenderPromotions) {
                retval.DefenderIgnoresDefensiveTerrainBonuses |= promotion.HasArg(PromotionArgType.NoDefensiveTerrainBonuses);

                if(promotion.HasArg(PromotionArgType.CombatStrength)) {
                    retval.DefenderCombatModifier += promotion.GetFloat();
                }
            }

            return retval;
        }

        public MovementInfo GetMovementInfo(IUnit unit) {
            var retval = new MovementInfo();

            foreach(var promotion in unit.Promotions) {
                retval.HasRoughTerrainPenalty |= promotion.HasArg(PromotionArgType.HasRoughTerrainPenalty);
                retval.IgnoresTerrainCosts    |= promotion.HasArg(PromotionArgType.IgnoresTerrainCosts);
            }

            return retval;
        }

        public PromotionVisionChanges GetVisionInfo(IUnit unit) {
            throw new NotImplementedException();
        }

        public PromotionHealingChanges GetHealingInfo(IUnit unit) {
            throw new NotImplementedException();
        }

        #endregion

        private IEnumerable<IPromotion> GetMeleeAttackPromotions(IUnit attacker, IUnit defender, IHexCell location) {
            return attacker.Promotions.Where(
                promotion => !IsDefensive(promotion) && AppliesToCell(promotion, location) && 
                AppliesToUnitType(promotion, defender.Type) && !IsRanged(promotion)
            );
        }

        private IEnumerable<IPromotion> GetMeleeDefensePromotions(IUnit attacker, IUnit defender, IHexCell location) {
            return defender.Promotions.Where(
                promotion => !IsOffensive(promotion) && AppliesToCell(promotion, location) &&
                AppliesToUnitType(promotion, attacker.Type) && !IsRanged(promotion)
            );
        }

        private IEnumerable<IPromotion> GetRangedAttackPromotions(IUnit attacker, IUnit defender, IHexCell location) {
            return attacker.Promotions.Where(
                promotion => !IsDefensive(promotion) && AppliesToCell(promotion, location) && 
                AppliesToUnitType(promotion, defender.Type) && !IsMelee(promotion)
            );
        }

        private IEnumerable<IPromotion> GetRangedDefensePromotions(IUnit attacker, IUnit defender, IHexCell location) {
            return defender.Promotions.Where(
                promotion => !IsOffensive(promotion) && AppliesToCell(promotion, location) &&
                AppliesToUnitType(promotion, attacker.Type) && !IsMelee(promotion)
            );
        }

        private bool IsDefensive(IPromotion promotion) {
            return promotion.HasArg(PromotionArgType.WhenDefending);
        }

        private bool IsOffensive(IPromotion promotion) {
            return promotion.HasArg(PromotionArgType.WhenAttacking);
        }

        private bool AppliesToCell(IPromotion promotion, IHexCell cell) {
            if(promotion.HasArg(PromotionArgType.OnFlatTerrain) && cell.IsRoughTerrain) {
                return false;
            }else if(promotion.HasArg(PromotionArgType.OnRoughTerrain) && !cell.IsRoughTerrain) {
                return false;
            }else if(
                promotion.HasArg(PromotionArgType.InForestAndJungle) &&
                cell.Feature != TerrainFeature.Forest &&
                cell.Feature != TerrainFeature.Jungle
            ){
                return false;
            }else {
                return true;
            }
        }

        private bool AppliesToUnitType(IPromotion promotion, UnitType type) {
            if(promotion.HasArg(PromotionArgType.AgainstUnitType)) {
                return promotion.GetUnitType() == type;
            }else {
                return true;
            }
        }

        private bool IsMelee(IPromotion promotion) {
            return promotion.HasArg(PromotionArgType.Melee);
        }

        private bool IsRanged(IPromotion promotion) {
            return promotion.HasArg(PromotionArgType.Ranged);
        }

        #endregion
        
    }

}
