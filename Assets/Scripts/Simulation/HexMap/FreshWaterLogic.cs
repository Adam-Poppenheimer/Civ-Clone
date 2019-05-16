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
            bool isFreshWater = cell.Terrain == CellTerrain.FreshWater;
            bool isSaltWater  = cell.Terrain.IsWater() && !isFreshWater;
            bool hasOasis     = cell.Feature == CellFeature.Oasis;
            bool hasRiver     = RiverCanon.HasRiver(cell);
            
            if(!isSaltWater) {
                if(isFreshWater || hasOasis || hasRiver) {
                    return true;
                }

                foreach(var neighbor in Grid.GetNeighbors(cell)) {
                    if(neighbor.Terrain == CellTerrain.FreshWater || neighbor.Feature == CellFeature.Oasis) {
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
