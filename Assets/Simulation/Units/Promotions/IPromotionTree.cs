using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.Units.Promotions {

    public interface IPromotionTree {

        #region properties

        IPromotionTreeTemplate Template { get; }

        #endregion

        #region events

        event EventHandler<EventArgs> PromotionsChanged;

        #endregion

        #region methods

        bool ContainsPromotion(IPromotion promotion);

        IEnumerable<IPromotion> GetChosenPromotions();
        IEnumerable<IPromotion> GetAvailablePromotions();
        IEnumerable<IPromotion> GetAppendedPromotions();
        IEnumerable<IPromotion> GetAllPromotions();

        bool CanChoosePromotion(IPromotion promotion);
        void ChoosePromotion   (IPromotion promotion);

        void AppendPromotion        (IPromotion promotion);
        void RemoveAppendedPromotion(IPromotion promotion);

        #endregion

    }

}
