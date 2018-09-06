using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.Technology;
using Assets.Simulation.MapResources;

namespace Assets.Simulation.MapGeneration {

    public class MapScorer : IMapScorer {

        #region instance fields and properties

        private IMapGenerationConfig Config;
        private ITechCanon           TechCanon;

        #endregion

        #region constructors

        [Inject]
        public MapScorer(IMapGenerationConfig config, ITechCanon techCanon) {
            Config    = config;
            TechCanon = techCanon;
        }

        #endregion

        #region instance methods

        #region from IYieldScorer

        public float GetScoreOfYield(YieldSummary yield) {
            return (yield * Config.YieldScoringWeights).Total;
        }

        public float GetScoreOfResourceNode(
            IResourceNode node, IEnumerable<ITechDefinition> availableTechs
        ) {
            if(node == null) {
                return 0f;
            }

            if(TechCanon.GetVisibleResourcesFromTechs(availableTechs).Contains(node.Resource)) {
                return node.Resource.Type != ResourceType.Bonus ? node.Resource.Score * node.Copies : 0;
            }else {
                return 0f;
            }
        }

        #endregion

        #endregion
        
    }

}
