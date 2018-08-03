using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

namespace Assets.Simulation.MapGeneration {

    public class YieldScorer : IYieldScorer {

        #region instance fields and properties

        private IMapGenerationConfig Config;

        #endregion

        #region constructors

        [Inject]
        public YieldScorer(IMapGenerationConfig config) {
            Config = config;
        }

        #endregion

        #region instance methods

        #region from IYieldScorer

        public float GetScoreOfYield(YieldSummary yield) {
            return (yield * Config.YieldScoringWeights).Total;
        }

        #endregion

        #endregion
        
    }

}
