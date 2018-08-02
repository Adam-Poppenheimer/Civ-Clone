using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

namespace Assets.Simulation.MapGeneration {

    public class YieldScorer : IYieldScorer {

        #region instance fields and properties



        #endregion

        #region constructors

        [Inject]
        public YieldScorer() {

        }

        #endregion

        #region instance methods

        #region from IYieldScorer

        public float GetScoreOfYield(YieldSummary yield) {
            return yield.Total;
        }

        #endregion

        #endregion
        
    }

}
