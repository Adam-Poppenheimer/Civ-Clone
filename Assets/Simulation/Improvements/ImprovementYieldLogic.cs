using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.MapResources;
using Assets.Simulation.Technology;

namespace Assets.Simulation.Improvements {

    public class ImprovementYieldLogic : IImprovementYieldLogic {

        #region constructors

        [Inject]
        public ImprovementYieldLogic() { }

        #endregion

        #region instance methods

        #region from IImprovementYieldLogic

        public YieldSummary GetYieldOfImprovement(
            IImprovement improvement, IResourceNode nodeAtLocation,
            IEnumerable<IResourceDefinition> visibleResources,
            IEnumerable<ITechDefinition> discoveredTechs, bool hasFreshWater
        ) {
            if(improvement.IsConstructed && !improvement.IsPillaged) {
                return GetYieldOfImprovementTemplate(
                    improvement.Template, nodeAtLocation, visibleResources,
                    discoveredTechs, hasFreshWater
                );
            }else {
                return YieldSummary.Empty;
            }
        }

        public YieldSummary GetYieldOfImprovementTemplate(
            IImprovementTemplate template, IResourceNode nodeAtLocation,
            IEnumerable<IResourceDefinition> visibleResources,
            IEnumerable<ITechDefinition> discoveredTechs, bool hasFreshWater
        ) {
            YieldSummary retval = YieldSummary.Empty;

            if( nodeAtLocation == null || nodeAtLocation.Resource.Extractor != template ||
                !visibleResources.Contains(nodeAtLocation.Resource)
            ) {
                retval += template.BonusYieldNormal;
            }

            var applicableTechMods = discoveredTechs.SelectMany(tech => tech.ImprovementYieldModifications)
                                                    .Where(mod => mod.Template == template);

            foreach(var mod in applicableTechMods) {
                if(!mod.RequiresFreshWater || hasFreshWater) {
                    retval += mod.BonusYield;
                }
            }

            return retval;
        }

        #endregion

        #endregion

    }

}
