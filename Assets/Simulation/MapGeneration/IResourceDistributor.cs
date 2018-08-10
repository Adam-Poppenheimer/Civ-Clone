using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapGeneration {

    public interface IResourceDistributor {

        #region methods

        void DistributeLuxuryResourcesAcrossRegion(
            MapRegion region, IRegionGenerationTemplate template
        );

        void DistributeStrategicResourcesAcrossRegion(
            MapRegion region, IRegionGenerationTemplate template
        );

        #endregion

    }

}
