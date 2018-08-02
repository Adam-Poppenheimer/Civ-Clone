using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.MapGeneration {

    public interface IYieldScorer {

        #region methods

        float GetScoreOfYield(YieldSummary yield);

        #endregion

    }

}
