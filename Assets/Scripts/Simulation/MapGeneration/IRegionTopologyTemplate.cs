using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.MapResources;

using UnityCustomUtilities.Extensions;

namespace Assets.Simulation.MapGeneration {

    public interface IRegionTopologyTemplate {

        #region properties

        string name { get; }

        int HillsPercentage     { get; }
        int MountainsPercentage { get; }

        IEnumerable<RegionBalanceStrategyData> BalanceStrategyWeights { get; }
        IEnumerable<RegionResourceData>        ResourceWeights        { get; }

        #endregion

    }

}
