using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

namespace Assets.Simulation.HexMap {

    public class FreshWaterCanon : IFreshWaterCanon {

        #region instance fields and properties

        private IHexGrid    Grid;
        private IRiverCanon RiverCanon;

        #endregion

        #region constructors

        [Inject]
        public FreshWaterCanon(IHexGrid grid, IRiverCanon riverCanon) {
            Grid       = grid;
            RiverCanon = riverCanon;
        }

        #endregion

        #region instance methods

        #region from IFreshWaterCanon

        public bool HasAccessToFreshWater(IHexCell cell) {
            bool isFreshWater          = cell.Terrain == CellTerrain.FreshWater;
            bool isSaltWater           = cell.Terrain.IsWater() && !isFreshWater;
            bool hasOasis              = cell.Feature == CellFeature.Oasis;
            bool hasRiver              = RiverCanon.HasRiver(cell);
            bool hasFreshWaterNeighbor = Grid.GetNeighbors(cell).Exists(neighbor => neighbor.Terrain == CellTerrain.FreshWater);
            

            return !isSaltWater && (isFreshWater || hasOasis || hasRiver || hasFreshWaterNeighbor);
        }

        #endregion

        #endregion
        
    }

}
