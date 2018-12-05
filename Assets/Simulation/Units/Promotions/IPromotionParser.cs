using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.HexMap;
using Assets.Simulation.Units.Combat;

namespace Assets.Simulation.Units.Promotions {

    public interface IPromotionParser {

        #region methods

        void SetCombatSummary(UnitCombatSummary summary, IUnit unit);
        void SetCombatSummary(UnitCombatSummary summary, IEnumerable<IPromotion> promotions);

        void SetMovementSummary(UnitMovementSummary summary, IUnit unit);
        void SetMovementSummary(UnitMovementSummary summary, IEnumerable<IPromotion> unit);

        VisionInfo GetVisionInfo(IUnit unit);

        HealingInfo GetHealingInfo(IUnit unit);

        #endregion

    }

}
