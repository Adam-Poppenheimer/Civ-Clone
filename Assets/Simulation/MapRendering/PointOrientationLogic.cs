﻿using System;
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

        private byte[] indexBytes = new byte[2];
        public void GetOrientationDataFromColors(
            PointOrientationData dataToUse, Color32 orientationColor, Color weightsColor, Color duckColor, bool shiftChannels
        ) {
            Profiler.BeginSample("GetOrientationDataFromColors()");

            var realOrientation = shiftChannels ? new Color32(orientationColor.g, orientationColor.b, orientationColor.a, orientationColor.r) : orientationColor;
            var realWeights     = shiftChannels ? new Color  (weightsColor    .g, weightsColor    .b, weightsColor    .a, weightsColor    .r) : weightsColor;
            var realDuck        = shiftChannels ? new Color  (duckColor       .g, duckColor       .b, duckColor       .a, duckColor       .r) : duckColor;

            indexBytes[0] = realOrientation.r;
            indexBytes[1] = realOrientation.g;

            int index = BitConverter.ToInt16(indexBytes, 0) - 1;

            var center = index >= 0 && index < Grid.Cells.Count ? Grid.Cells[index] : null;

            HexDirection sextant = (HexDirection)realOrientation.b;

            if(center != null) {
                dataToUse.IsOnGrid = true;

                dataToUse.Center    = center;
                dataToUse.Left      = Grid.GetNeighbor(center, sextant.Previous());
                dataToUse.Right     = Grid.GetNeighbor(center, sextant);
                dataToUse.NextRight = Grid.GetNeighbor(center, sextant.Next());

                dataToUse.CenterWeight    = realWeights.r;
                dataToUse.LeftWeight      = realWeights.g;
                dataToUse.RightWeight     = realWeights.b;
                dataToUse.NextRightWeight = realWeights.a;

                dataToUse.RiverWeight = Mathf.Clamp01(1f - realWeights.r - realWeights.g - realWeights.b - realWeights.a);

                dataToUse.ElevationDuck = realDuck.r;

                Profiler.EndSample();

            } else {
                dataToUse.Clear();
            }      
        }

        #endregion

        #endregion
        
    }

}
