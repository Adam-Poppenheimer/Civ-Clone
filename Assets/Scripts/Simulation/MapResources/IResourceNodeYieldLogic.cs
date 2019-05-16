using System.Collections.Generic;
using Assets.Simulation.Improvements;

namespace Assets.Simulation.MapResources {

    public interface IResourceNodeYieldLogic {

        #region methods

        YieldSummary GetYieldFromNode(
            IResourceNode node, IEnumerable<IResourceDefinition> visibleResources,
            IImprovement improvementAtLocation
        );

        #endregion

    }

}