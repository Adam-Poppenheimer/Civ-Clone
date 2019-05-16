using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapGeneration {

    public interface IBalanceStrategy {

        #region properties

        string Name { get; }

        #endregion

        #region methods

        bool TryIncreaseYield(MapRegion region, RegionData regionData, YieldType type, out YieldSummary yieldAdded);

        bool TryIncreaseScore(MapRegion region, RegionData regionData, out float scoreAdded);
        bool TryDecreaseScore(MapRegion region, RegionData regionData, out float scoreRemoved);

        #endregion

    }

}
