using System.Collections.Generic;

namespace Assets.Simulation.Units.Promotions {

    public interface IPromotionTreeTemplate {

        #region properties

        string name { get; }

        IEnumerable<IPromotionPrerequisiteData> PrerequisiteData { get; }

        #endregion

    }

}