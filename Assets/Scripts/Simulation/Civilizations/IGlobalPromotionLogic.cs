using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.Units.Promotions;

namespace Assets.Simulation.Civilizations {

    public interface IGlobalPromotionLogic {

        #region methods

        IEnumerable<IPromotion> GetGlobalPromotionsOfCiv(ICivilization civ);

        #endregion

    }

}
