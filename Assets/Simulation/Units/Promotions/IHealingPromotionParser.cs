using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.Units.Promotions {

    public interface IHealingPromotionParser {

        #region methods

        void ParsePromotionForHealingInfo(IPromotion promotion, IUnit unit, HealingInfo info);

        #endregion

    }

}
