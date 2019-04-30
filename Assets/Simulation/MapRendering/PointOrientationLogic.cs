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

        /* The current construction tries to reduce GC allocation
         * by reusing certain information. ReusedOrientationData
         * in particular can save a lot of allocations.
         * However, this will only function if we're only ever using
         * a single PointOrientationData at a time. It will fail if
         * we attempt to acquire multiple orientation data and use them
         * in tandem.
         */ 
        private byte[] indexBytes = new byte[2];
        private PointOrientationData ReusedOrientationData = new PointOrientationData();
        private PointOrientationData EmptyData = new PointOrientationData();

        public PointOrientationData GetOrientationDataFromColors(Color32 orientationColor, Color weightsColor) {
            Profiler.BeginSample("GetOrientationDataFromColors()");

            indexBytes[0] = orientationColor.r;
            indexBytes[1] = orientationColor.g;

            int index = BitConverter.ToInt16(indexBytes, 0) - 1;

            var center = index >= 0 && index < Grid.Cells.Count ? Grid.Cells[index] : null;

            HexDirection sextant = (HexDirection)orientationColor.b;

            if(center != null) {
                ReusedOrientationData.IsOnGrid = true;
                ReusedOrientationData.Sextant = sextant;

                ReusedOrientationData.Center    = center;
                ReusedOrientationData.Left      = Grid.GetNeighbor(center, sextant.Previous());
                ReusedOrientationData.Right     = Grid.GetNeighbor(center, sextant);
                ReusedOrientationData.NextRight = Grid.GetNeighbor(center, sextant.Next());

                ReusedOrientationData.CenterWeight    = weightsColor.r;
                ReusedOrientationData.LeftWeight      = weightsColor.g;
                ReusedOrientationData.RightWeight     = weightsColor.b;
                ReusedOrientationData.NextRightWeight = weightsColor.a;

                ReusedOrientationData.RiverWeight = Mathf.Clamp01(1f - weightsColor.r - weightsColor.g - weightsColor.b - weightsColor.a);

                Profiler.EndSample();

                return ReusedOrientationData;
            }else {
                Profiler.EndSample();

                return EmptyData;
            }      
        }

        #endregion

        #endregion
        
    }

}
