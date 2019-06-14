using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.Profiling;

using Zenject;

using Assets.Simulation.HexMap;
using Assets.Util;

using UnityCustomUtilities.Extensions;

namespace Assets.Simulation.MapRendering {

    public class PointOrientationLogic : IPointOrientationLogic {

        #region instance fields and properties

        private IHexGrid              Grid;
        private ICellEdgeContourCanon CellContourCanon;

        #endregion

        #region constructors

        [Inject]
        public PointOrientationLogic(IHexGrid grid, ICellEdgeContourCanon cellContourCanon) {
            Grid             = grid;
            CellContourCanon = cellContourCanon;
        }

        #endregion

        #region instance methods

        #region from IBetterPointOrientationLogic

        public IHexCell GetCellAtPoint(Vector3 point) {
            if(!Grid.HasCellAtLocation(point)) {
                return null;
            }

            var gridCell = Grid.GetCellAtLocation(point);

            foreach(var direction in EnumUtil.GetValues<HexDirection>()) {
                if(CellContourCanon.IsPointWithinContour(point.ToXZ(), gridCell, direction)) {
                    return gridCell;
                }
            }

            foreach(var neighbor in Grid.GetNeighbors(gridCell)) {
                foreach(var direction in EnumUtil.GetValues<HexDirection>()) {
                    if(CellContourCanon.IsPointWithinContour(point.ToXZ(), neighbor, direction.Opposite())) {
                        return neighbor;
                    }
                }
            }

            return null;
        }

        public void GetOrientationDataFromColors(
            PointOrientationData dataToUse, byte[] indexBytes, Color32 orientationColor, Color weightsColor, Color duckColor
        ) {
            indexBytes[0] = orientationColor.r;
            indexBytes[1] = orientationColor.g;

            int index = BitConverter.ToInt16(indexBytes, 0) - 1;

            var center = index >= 0 && index < Grid.Cells.Count ? Grid.Cells[index] : null;

            HexDirection sextant = (HexDirection)orientationColor.b;

            if(center != null) {
                dataToUse.IsOnGrid = true;

                dataToUse.Center    = center;
                dataToUse.Left      = Grid.GetNeighbor(center, sextant.Previous());
                dataToUse.Right     = Grid.GetNeighbor(center, sextant);
                dataToUse.NextRight = Grid.GetNeighbor(center, sextant.Next());

                dataToUse.CenterWeight    = weightsColor.r;
                dataToUse.LeftWeight      = weightsColor.g;
                dataToUse.RightWeight     = weightsColor.b;
                dataToUse.NextRightWeight = weightsColor.a;

                float weightSum = weightsColor.r + weightsColor.g + weightsColor.b + weightsColor.a;

                dataToUse.RiverWeight = Mathf.Clamp01(1f - weightSum);

                dataToUse.ElevationDuck = duckColor.r;

            } else {
                dataToUse.Clear();
            }
        }

        #endregion

        #endregion
        
    }

}
