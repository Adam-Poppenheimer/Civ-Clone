using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.HexMap;
using Assets.Simulation.Units.Combat;

namespace Assets.Simulation.Units.Promotions {

    public interface ICombatPromotionParser {

        #region methods

        void AddPromotionToCombatSummary(IPromotion promotion, UnitCombatSummary combatSummary);

        #endregion

    }

}
