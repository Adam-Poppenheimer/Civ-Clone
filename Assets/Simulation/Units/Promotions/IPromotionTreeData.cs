using System.Collections.Generic;

namespace Assets.Simulation.Units.Promotions {

    public interface IPromotionTreeData {

        #region properties

        IEnumerable<IPromotionPrerequisiteData> PrerequisiteData { get; }

        #endregion

    }

}