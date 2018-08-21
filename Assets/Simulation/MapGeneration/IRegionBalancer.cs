using System.Collections.Generic;
using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapGeneration {

    public interface IRegionBalancer {

        #region methods

        void BalanceRegionYields(
            MapRegion region, IRegionTemplate template
        );

        #endregion

    }
}