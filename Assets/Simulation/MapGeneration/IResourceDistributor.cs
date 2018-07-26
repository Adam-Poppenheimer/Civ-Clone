using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.HexMap;
using Assets.Simulation.MapResources;

namespace Assets.Simulation.MapGeneration {

    public interface IResourceDistributor {

        #region methods
        
        List<List<IResourceDefinition>> SubdivideResources(
            IEnumerable<IResourceDefinition> resources, int groupCount,
            int minResourcesPerGroup
        );

        void DistributeResourcesAcrossRegion(
            MapRegion region, IRegionGenerationTemplate template,
            IEnumerable<IHexCell> oceanCells
        );

        #endregion

    }

}
