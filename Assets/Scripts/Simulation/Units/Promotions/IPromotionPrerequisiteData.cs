using System.Collections.Generic;

namespace Assets.Simulation.Units.Promotions {

    public interface IPromotionPrerequisiteData {

        #region properties

        IPromotion Promotion { get; }

        IEnumerable<IPromotion> Prerequisites { get; }

        #endregion

    }

}