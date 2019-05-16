using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine.Profiling;

using Zenject;

using Assets.Simulation.MapResources;
using Assets.Simulation.Technology;

namespace Assets.Simulation.Improvements {

    public class ImprovementYieldLogic : IImprovementYieldLogic {

        #region instance fields and properties

        

        #endregion

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
            var improvementModification = discoveredTechs.SelectMany(tech => tech.ImprovementYieldModifications);

            return GetYieldOfImprovementTemplate(
                template, nodeAtLocation, visibleResources, improvementModification, hasFreshWater
            );
        }

        public YieldSummary GetYieldOfImprovementTemplate(
            IImprovementTemplate template, IResourceNode nodeAtLocation,
            IEnumerable<IResourceDefinition> visibleResources,
            IEnumerable<IImprovementModificationData> improvementModifications,
            bool hasFreshWater
        ) {
            YieldSummary retval = YieldSummary.Empty;

            if( nodeAtLocation == null || nodeAtLocation.Resource.Extractor != template ||
                !visibleResources.Contains(nodeAtLocation.Resource)
            ) {
                retval += template.BonusYieldNormal;
            }

            foreach(var mod in improvementModifications) {
                if(mod.Template == template && (!mod.RequiresFreshWater || hasFreshWater)) {
                    retval += mod.BonusYield;
                }
            }

            return retval;
        }

        #endregion

        #endregion

    }

}
