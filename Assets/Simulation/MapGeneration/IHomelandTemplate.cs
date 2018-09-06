using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.MapGeneration {

    public interface IHomelandTemplate {

        #region properties

        IRegionResourceTemplate StartingResources { get; }
        IRegionResourceTemplate OtherResources    { get; }

        IEnumerable<LuxuryResourceData> LuxuryResourceData { get; }

        int RegionCount { get; }

        HomelandYieldData YieldData { get; }

        #endregion

    }

}
