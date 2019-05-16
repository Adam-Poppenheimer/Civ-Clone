using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.Units.Promotions;

namespace Assets.Simulation.MapManagement {

    public interface IPromotionTreeComposer {

        #region methods

        SerializablePromotionTreeData ComposePromotionTree(IPromotionTree promotionTree);

        IPromotionTree DecomposePromotionTree(SerializablePromotionTreeData treeData);

        #endregion

    }

}
