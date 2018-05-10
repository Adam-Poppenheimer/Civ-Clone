using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.Units.Promotions {

    public interface IPromotionTree {

        #region methods

        bool ContainsPromotion(IPromotion promotion);

        IEnumerable<IPromotion> GetChosenPromotions();
        IEnumerable<IPromotion> GetAvailablePromotions();

        bool CanChoosePromotion(IPromotion promotion);
        void ChoosePromotion   (IPromotion promotion);

        #endregion

    }

}
