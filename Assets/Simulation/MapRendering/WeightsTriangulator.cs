using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.HexMap;
using Assets.Util;

using UnityCustomUtilities.Extensions;

namespace Assets.Simulation.MapRendering {

    public class WeightsTriangulator : IWeightsTriangulator {

        #region static fields and properties

        //Weights are Center, Left, Right, and NextRight in that order
        private static Color CenterWeights               = new Color(1f,      0f,      0f,      0f     );
        private static Color CenterRightWeights          = new Color(0.5f,    0f,      0.5f,    0f     );
        private static Color CenterLeftRightWeights      = new Color(1f / 3f, 1f / 3f, 1f / 3f, 0f     );
        private static Color CenterRightNextRightWeights = new Color(1f / 3f, 0f,      1f / 3f, 1f / 3f);
        private static Color RightWeights                = new Color(0f,      0f,      1f,      0f     );

        #endregion

        #region instance fields and properties

        private IMapRenderConfig      RenderConfig;
        private IHexGrid              Grid;
        private IRiverCanon           RiverCanon;
        private ICellEdgeContourCanon CellContourCanon;
        private IContourTriangulator  ContourTriangulator;

        #endregion

        #region constructors

        [Inject]
        public WeightsTriangulator(
            IMapRenderConfig renderConfig, IHexGrid grid, IRiverCanon riverCanon, ICellEdgeContourCanon cellContourCanon,
            IContourTriangulator contourTriangulator
        ) {
            RenderConfig        = renderConfig;
            Grid                = grid;
            RiverCanon          = riverCanon;
            CellContourCanon    = cellContourCanon;
            ContourTriangulator = contourTriangulator;
        }

        #endregion

        #region instance methods

        #region from IWeightsTriangulator

        public void TriangulateCellWeights(IHexCell center, IHexMesh weightsMesh) {
            foreach(var direction in EnumUtil.GetValues<HexDirection>()) {
                bool hasCenterRightRiver = RiverCanon.HasRiverAlongEdge(center, direction);

                IHexCell right = Grid.GetNeighbor(center, direction);

                if(hasCenterRightRiver) {
                    TriangulateCellWeights_River(center, right, direction, hasCenterRightRiver, weightsMesh);
                }else {
                    TriangulateCellWeights_NoRiver(center, right, direction, weightsMesh);
                }
            }
        }

        #endregion

        private void TriangulateCellWeights_River(
            IHexCell center, IHexCell right, HexDirection direction, bool hasCenterRightRiver, IHexMesh weightsMesh
        ) {
            var centerRightContour = CellContourCanon.GetContourForCellEdge(center, direction);

            Vector3 innerOne, innerTwo, contourOneXYZ, contourTwoXYZ;

            for(int i = 1; i < centerRightContour.Count; i++) {
                contourOneXYZ = centerRightContour[i - 1].ToXYZ();
                contourTwoXYZ = centerRightContour[i]    .ToXYZ();

                innerOne = Vector3.Lerp(center.AbsolutePosition, contourOneXYZ, RenderConfig.SolidFactor);
                innerTwo = Vector3.Lerp(center.AbsolutePosition, contourTwoXYZ, RenderConfig.SolidFactor);

                weightsMesh.AddTriangle(center.AbsolutePosition, innerOne, innerTwo);
                weightsMesh.AddTriangleColor(CenterWeights);

                weightsMesh.AddQuad(innerOne, innerTwo, contourOneXYZ, contourTwoXYZ);

                if(hasCenterRightRiver) {
                    weightsMesh.AddQuadColor(CenterWeights);
                }else {
                    weightsMesh.AddQuadColor(CenterWeights, CenterRightWeights);
                }
            }

            if(hasCenterRightRiver && direction <= HexDirection.SE) {
                ContourTriangulator.TriangulateContoursBetween(
                    center, right, direction, CenterWeights, RightWeights, weightsMesh
                );
            }
        }

        private void TriangulateCellWeights_NoRiver(
            IHexCell center, IHexCell right, HexDirection direction, IHexMesh weightsMesh
        ) {
            if(right == null) {
                weightsMesh.AddTriangle(
                    center.AbsolutePosition,
                    center.AbsolutePosition + RenderConfig.GetFirstCorner (direction),
                    center.AbsolutePosition + RenderConfig.GetSecondCorner(direction)
                );
                weightsMesh.AddTriangleColor(CenterWeights);

                return;
            }

            Vector3 firstEdgeInner  = center.AbsolutePosition + RenderConfig.GetFirstSolidCorner (direction);
            Vector3 secondEdgeInner = center.AbsolutePosition + RenderConfig.GetSecondSolidCorner(direction);

            //Solid center region
            weightsMesh.AddTriangle(center.AbsolutePosition, firstEdgeInner, secondEdgeInner);
            weightsMesh.AddTriangleColor(CenterWeights);

            Vector3 firstEdgeOuter  = (firstEdgeInner  + right.AbsolutePosition + RenderConfig.GetSecondSolidCorner(direction.Opposite())) / 2f;
            Vector3 secondEdgeOuter = (secondEdgeInner + right.AbsolutePosition + RenderConfig.GetFirstSolidCorner (direction.Opposite())) / 2f;

            //The edge between Center and Right, going up to the dividing line
            weightsMesh.AddQuad(firstEdgeInner, secondEdgeInner, firstEdgeOuter, secondEdgeOuter);
            weightsMesh.AddQuadColor(CenterWeights, CenterRightWeights);

            //Previous corner out to the edge of the cell
            weightsMesh.AddTriangle     (firstEdgeInner, center.AbsolutePosition + RenderConfig.GetFirstCorner(direction), firstEdgeOuter    );
            weightsMesh.AddTriangleColor(CenterWeights,  CenterLeftRightWeights,                                           CenterRightWeights);

            //Next corner out to the edge of the cell
            weightsMesh.AddTriangle     (secondEdgeInner, secondEdgeOuter,    center.AbsolutePosition + RenderConfig.GetSecondCorner(direction));
            weightsMesh.AddTriangleColor(CenterWeights,   CenterRightWeights, CenterRightNextRightWeights);
        }

        #endregion

    }

}
