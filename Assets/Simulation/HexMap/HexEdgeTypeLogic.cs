using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

using UnityCustomUtilities.Extensions;

namespace Assets.Simulation.HexMap {

    public class HexEdgeTypeLogic : IHexEdgeTypeLogic {

        #region instance fields and properties

        private IRiverCanon RiverCanon;
        private IHexGrid    Grid;

        #endregion

        #region constructors

        [Inject]
        public HexEdgeTypeLogic(IRiverCanon riverCanon, IHexGrid grid) {
            RiverCanon = riverCanon;
            Grid       = grid;
        }

        #endregion

        #region instance methods

        #region from IHexEdgeTypeLogic

         public HexEdgeType GetEdgeTypeBetweenCells(IHexCell cellOne, IHexCell cellTwo) {
            if(cellOne == null || cellTwo == null) {
                return HexEdgeType.Void;
            }

            if(cellOne.Terrain.IsWater() || cellTwo.Terrain.IsWater()) {
                return HexEdgeType.Flat;
            }

            foreach(var direction in EnumUtil.GetValues<HexDirection>()) {
                var neighbor = Grid.GetNeighbor(cellOne, direction);

                if(neighbor == cellTwo && RiverCanon.HasRiverAlongEdge(cellOne, direction)) {
                    return HexEdgeType.River;
                }
            }

            int elevationOne = cellOne.EdgeElevation;
            int elevationTwo = cellTwo.EdgeElevation;

            if(elevationOne == elevationTwo) {
                return HexEdgeType.Flat;
            }else if(Math.Abs(elevationOne - elevationTwo) == 1) {
                return HexEdgeType.Slope;
            }else {
                return HexEdgeType.Cliff;
            }
        }

        #endregion

        #endregion
       
    }

}
