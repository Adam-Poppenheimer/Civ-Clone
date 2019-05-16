using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Improvements;

namespace Assets.Simulation.MapResources {

    public class ResourceNodeYieldLogic : IResourceNodeYieldLogic {

        #region constructors

        [Inject]
        public ResourceNodeYieldLogic() { }

        #endregion

        #region instance methods

        #region from IResourceNodeYieldLogic

        public YieldSummary GetYieldFromNode(
            IResourceNode node, IEnumerable<IResourceDefinition> visibleResources,
            IImprovement improvementAtLocation
        ) {
            var retval = YieldSummary.Empty;

            if(visibleResources.Contains(node.Resource)) {
                retval += node.Resource.BonusYieldBase;

                if( improvementAtLocation != null &&
                    node.Resource.Extractor == improvementAtLocation.Template &&
                    improvementAtLocation.IsConstructed &&
                    !improvementAtLocation.IsPillaged
                ) {
                    retval += node.Resource.BonusYieldWhenImproved;
                }
            }

            return retval;
        }

        #endregion

        #endregion

    }

}
