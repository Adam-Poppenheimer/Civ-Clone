using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapGeneration {

    public interface IResourceDistributor {

        #region methods

        void DistributeLuxuryResourcesAcrossRegion(
            MapRegion region, IRegionTemplate template
        );

        void DistributeStrategicResourcesAcrossRegion(
            MapRegion region, IRegionTemplate template
        );

        #endregion

    }

}
