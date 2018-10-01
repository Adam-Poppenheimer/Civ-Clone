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
        private IHealingPromotionParser  HealingParser;

        #endregion

        #region constructors

        [Inject]
        public PromotionParser(
            ICombatPromotionParser combatParser, IMovementPromotionParser movementParser,
            IHealingPromotionParser healingParser
        ){
            CombatParser   = combatParser;
            MovementParser = movementParser;
            HealingParser  = healingParser;
        }

        #endregion

        #region instance methods

        #region from IPromotionParser

        public void SetCombatSummary(UnitCombatSummary summary, IEnumerable<IPromotion> promotions) {
            summary.Reset();

            foreach(var promotion in promotions) {
                CombatParser.AddPromotionToCombatSummary(promotion, summary);
            }
        }

        public void SetMovementSummary(UnitMovementSummary summary, IEnumerable<IPromotion> promotions) {
            summary.Reset();

            foreach(var promotion in promotions) {
                MovementParser.AddPromotionToMovementSummary(promotion, summary);
            }
        }

        public VisionInfo GetVisionInfo(IUnit unit) {
            throw new NotImplementedException();
        }

        public HealingInfo GetHealingInfo(IUnit unit) {
            var retval = new HealingInfo();

            foreach(var promotion in unit.Promotions) {
                HealingParser.ParsePromotionForHealingInfo(promotion, unit, retval);
            }

            return retval;
        }

        #endregion

        #endregion
        
    }

}
