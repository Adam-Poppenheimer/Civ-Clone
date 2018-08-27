using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapGeneration {

    public interface IResourceDistributor {

        #region methods

        void DistributeLuxuryResourcesAcrossRegion   (MapRegion region, RegionData regionData);
        void DistributeStrategicResourcesAcrossRegion(MapRegion region, RegionData regionData);

        #endregion

    }

}
