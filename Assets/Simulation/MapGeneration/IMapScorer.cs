using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.MapResources;
using Assets.Simulation.Technology;

namespace Assets.Simulation.MapGeneration {

    public interface IMapScorer {

        #region methods

        float GetScoreOfYield(YieldSummary yield);

        float GetScoreOfResourceNode(
            IResourceNode node, IEnumerable<ITechDefinition> availableTechs
        );

        #endregion

    }

}
