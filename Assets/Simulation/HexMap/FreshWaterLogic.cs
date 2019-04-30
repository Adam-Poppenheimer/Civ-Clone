using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using UnityCustomUtilities.Extensions;

namespace Assets.Simulation.HexMap {

    public class FreshWaterLogic : IFreshWaterLogic {

        #region instance fields and properties

        private IHexGrid    Grid;
        private IRiverCanon RiverCanon;

        #endregion

        #region constructors

        [Inject]
        public FreshWaterLogic(IHexGrid grid, IRiverCanon riverCanon) {
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
            
            if(!isSaltWater && (isFreshWater || hasOasis || hasRiver)) {
                foreach(var direction in EnumUtil.GetValues<HexDirection>()) {
                    var neighbor = Grid.GetNeighbor(cell, direction);

                    if(neighbor != null && neighbor.Terrain == CellTerrain.FreshWater || neighbor.Feature == CellFeature.Oasis) {
                        return true;
                    }
                }
            }

            return false;
        }

        #endregion

        #endregion
        
    }

}
