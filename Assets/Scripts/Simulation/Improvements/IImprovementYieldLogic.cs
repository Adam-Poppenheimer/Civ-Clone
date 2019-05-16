using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.MapResources;
using Assets.Simulation.Technology;

namespace Assets.Simulation.Improvements {

    public interface IImprovementYieldLogic {

        #region methods

        YieldSummary GetYieldOfImprovement(
            IImprovement improvement, IResourceNode nodeAtLocation,
            IEnumerable<IResourceDefinition> visibleResources,
            IEnumerable<ITechDefinition> discoveredTechs, bool hasFreshWater
        );

        YieldSummary GetYieldOfImprovementTemplate(
            IImprovementTemplate template, IResourceNode nodeAtLocation,
            IEnumerable<IResourceDefinition> visibleResources,
            IEnumerable<ITechDefinition> discoveredTechs, bool hasFreshWater
        );

        YieldSummary GetYieldOfImprovementTemplate(
            IImprovementTemplate template, IResourceNode nodeAtLocation,
            IEnumerable<IResourceDefinition> visibleResources,
            IEnumerable<IImprovementModificationData> improvementModifications,
            bool hasFreshWater
        );

        #endregion

    }

}
