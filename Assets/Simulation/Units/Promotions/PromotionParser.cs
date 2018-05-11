using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.HexMap;
using Assets.Simulation.Units.Combat;

namespace Assets.Simulation.Units.Promotions {

    public class PromotionParser : IPromotionParser {

        #region instance methods

        #region from IPromotionParser

        public CombatInfo GetCombatInfo(IUnit attacker, IUnit defender, IHexCell location, CombatType combatType) {
            var retval = new CombatInfo();

            retval.CombatType = combatType;

            foreach(var promotion in attacker.Promotions) {
                promotion.ModifyCombatInfoForAttacker(attacker, defender, location, combatType, retval);
            }

            foreach(var promotion in defender.Promotions) {
                promotion.ModifyCombatInfoForDefender(attacker, defender, location, combatType, retval);
            }

            return retval;
        }

        public MovementInfo GetMovementInfo(IUnit unit) {
            var retval = new MovementInfo();

            foreach(var promotion in unit.Promotions) {
                promotion.ModifyMovementInfo(unit, retval);
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

        #endregion
        
    }

}
