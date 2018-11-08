using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.Units.Promotions;

namespace Assets.Simulation.Civilizations {

    public interface IGlobalPromotionCanon {

        #region methods

        IEnumerable<IPromotion> GetGlobalPromotionsOfCiv(ICivilization civ);

        void AddGlobalPromotionToCiv     (IPromotion promotion, ICivilization civ);
        void RemoveGlobalPromotionFromCiv(IPromotion promotion, ICivilization civ);

        #endregion

    }

}
