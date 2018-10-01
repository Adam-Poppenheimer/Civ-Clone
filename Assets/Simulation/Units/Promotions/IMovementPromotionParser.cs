using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.Units.Promotions {

    public interface IMovementPromotionParser {

        #region methods

        void AddPromotionToMovementSummary(
            IPromotion promotion, UnitMovementSummary summary
        );

        #endregion

    }

}
