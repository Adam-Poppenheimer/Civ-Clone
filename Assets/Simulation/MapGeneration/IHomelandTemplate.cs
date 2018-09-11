using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.MapGeneration {

    public interface IHomelandTemplate {

        #region properties

        IEnumerable<LuxuryResourceData> LuxuryResourceData { get; }

        int RegionCount { get; }

        IYieldAndResourcesTemplate YieldAndResources { get; }

        int StartingRegionRadius { get; }

        #endregion

    }

}
