using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.MapResources;
using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapGeneration {

    public interface IRegionGenerator {

        #region methods

        void GenerateTopology (MapRegion region, IRegionTopologyTemplate template);
        void PaintTerrain     (MapRegion region, IRegionBiomeTemplate template);
        void AssignFloodPlains(IEnumerable<IHexCell> landCells);

        void DistributeYieldAndResources(MapRegion region, RegionData regionData);

        #endregion

    }

}
