using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.HexMap;
using Assets.Util;

using UnityCustomUtilities.Extensions;

namespace Assets.Simulation.MapRendering {

    public class OrientationTriangulator : IOrientationTriangulator {

        #region instance fields and properties

        private ICellEdgeContourCanon CellContourCanon;
        private IRiverCanon           RiverCanon;
        private IHexGrid              Grid;
        private IContourTriangulator  ContourTriangulator;

        #endregion

        #region constructors

        [Inject]
        public OrientationTriangulator(
            ICellEdgeContourCanon cellContourCanon, IRiverCanon riverCanon, IHexGrid grid,
            IContourTriangulator contourTriangulator
        ) {
            CellContourCanon    = cellContourCanon;
            RiverCanon          = riverCanon;
            Grid                = grid;
            ContourTriangulator = contourTriangulator;
        }

        #endregion

        #region instance methods

        #region from IOrientationTriangulator

        public void TriangulateOrientation(IHexCell center, IHexMesh orientationMesh) {
            short indexOffset = (short)(center.Index + 1);

            foreach(var direction in EnumUtil.GetValues<HexDirection>()) {
                byte[] rg = BitConverter.GetBytes(indexOffset);
                byte b  = (byte)direction;

                var cellColor = new Color32(rg[0], rg[1], b, 0);
                
                var centerContour = CellContourCanon.GetContourForCellEdge(center, direction);

                for(int i = 1; i < centerContour.Count; i++) {
                    orientationMesh.AddTriangle(
                        center.AbsolutePosition, centerContour[i - 1].ToXYZ(), centerContour[i].ToXYZ()
                    );

                    orientationMesh.AddTriangleColor(cellColor);
                }

                if(direction <= HexDirection.SE && RiverCanon.HasRiverAlongEdge(center, direction)) {
                    var right = Grid.GetNeighbor(center, direction);

                    ContourTriangulator.TriangulateContoursBetween(
                        center, right, direction, cellColor, cellColor, orientationMesh
                    );

                    if(direction <= HexDirection.E && RiverCanon.HasRiverAlongEdge(right, direction.Previous2())) {

                    }
                }
            }
        }

        #endregion

        #endregion

    }

}
