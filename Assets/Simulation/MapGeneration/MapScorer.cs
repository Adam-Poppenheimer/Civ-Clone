using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using Assets.Simulation.HexMap;
using Assets.Simulation.MapResources;

namespace Assets.Simulation.MapGeneration {

    public class MapScorer : IMapScorer {

        #region instance fields and properties

        private IMapGenerationConfig Config;

        #endregion

        #region constructors

        [Inject]
        public MapScorer(IMapGenerationConfig config) {
            Config = config;
        }

        #endregion

        #region instance methods

        #region from IYieldScorer

        public float GetScoreOfYield(YieldSummary yield) {
            return (yield * Config.YieldScoringWeights).Total;
        }

        public float GetScoreOfResourceNode(IResourceNode node) {
            return node.Resource.Type != ResourceType.Bonus ? node.Resource.Score * node.Copies : 0;
        }

        #endregion

        #endregion
        
    }

}
