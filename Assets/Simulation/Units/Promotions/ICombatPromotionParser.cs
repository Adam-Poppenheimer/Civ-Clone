using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.HexMap;
using Assets.Simulation.Units.Combat;

namespace Assets.Simulation.Units.Promotions {

    public interface ICombatPromotionParser {

        #region methods

        void ParsePromotionForAttacker(
            IPromotion promotion, IUnit attacker, IUnit defender,
            IHexCell location, CombatInfo combatInfo
        );

        void ParsePromotionForDefender(
            IPromotion promotion, IUnit attacker, IUnit defender,
            IHexCell location, CombatInfo combatInfo
        );

        #endregion

    }

}
