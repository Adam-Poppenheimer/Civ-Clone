using System.Collections.Generic;

namespace Assets.Simulation.MapGeneration {

    public interface IHomelandTemplate {

        #region properties

        int RegionCount { get; }

        int StartingRegionRadius { get; }

        IEnumerable<LuxuryResourceData> LuxuryResourceData { get; }
        
        IYieldAndResourcesTemplate YieldAndResources { get; }

        #endregion

    }

}