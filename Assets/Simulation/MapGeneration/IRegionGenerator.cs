using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.MapResources;
using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapGeneration {

    public interface IRegionGenerator {

        #region methods

        void GenerateTopologyAndEcology (MapRegion region, RegionData regionData);
        void DistributeYieldAndResources(MapRegion region, RegionData regionData);

        #endregion

    }

}
