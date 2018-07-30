using System;
using System.Collections.Generic;

using Assets.Simulation.MapResources;

namespace Assets.Simulation.MapGeneration {

    public interface IResourceSampler {

        #region methods

        void Reset();

        ResourceSamplerResults GetLuxuryForRegion(
            MapRegion region, int nodesNeeded,
            List<IResourceDefinition> luxuriesSoFar
        );

        #endregion

    }

}