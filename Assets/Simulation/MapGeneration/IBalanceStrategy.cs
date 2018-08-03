using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapGeneration {

    public interface IBalanceStrategy {

        #region properties

        int SelectionWeight { get; }

        #endregion

        #region methods

        bool TryIncreaseYield(MapRegion region, YieldType type, out YieldSummary yieldAdded);

        bool TryIncreaseScore(MapRegion region, out float scoreAdded);
        bool TryDecreaseScore(MapRegion region, out float scoreRemoved);

        #endregion

    }

}
