using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.Units.Promotions {

    public interface IMovementPromotionParser {

        #region methods

        void ParsePromotionForUnitMovement(IPromotion promotion, IUnit unit, MovementInfo info);

        #endregion

    }

}
