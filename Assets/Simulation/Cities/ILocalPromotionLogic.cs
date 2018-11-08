using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.Units.Promotions;

namespace Assets.Simulation.Cities {

    public interface ILocalPromotionLogic {

        #region methods

        IEnumerable<IPromotion> GetLocalPromotionsForCity(ICity city);

        #endregion

    }

}
