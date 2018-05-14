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

        private ICombatPromotionParser   CombatParser;
        private IMovementPromotionParser MovementParser;

        #endregion

        #region constructors

        [Inject]
        public PromotionParser(ICombatPromotionParser combatParser, IMovementPromotionParser movementParser) {
            CombatParser   = combatParser;
            MovementParser = movementParser;
        }

        #endregion

        #region instance methods

        #region from IPromotionParser

        public CombatInfo GetCombatInfo(IUnit attacker, IUnit defender, IHexCell location, CombatType combatType) {
            var retval = new CombatInfo();

            retval.CombatType = combatType;

            foreach(var promotion in attacker.Promotions) {
                CombatParser.ParsePromotionForAttacker(promotion, attacker, defender, location, retval);
            }

            foreach(var promotion in defender.Promotions) {
                CombatParser.ParsePromotionForDefender(promotion, attacker, defender, location, retval);
            }

            return retval;
        }

        public MovementInfo GetMovementInfo(IUnit unit) {
            var retval = new MovementInfo();

            foreach(var promotion in unit.Promotions) {
                MovementParser.ParsePromotionForUnitMovement(promotion, unit, retval);
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
