using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.HexMap;
using Assets.Simulation.MapResources;

namespace Assets.Simulation.MapGeneration {

    public interface IResourceDistributor {

        #region methods

        void DistributeLuxuryResourcesAcrossRegion(
            MapRegion region, IRegionGenerationTemplate template,
            IEnumerable<IHexCell> oceanCells
        );

        #endregion

    }

}
