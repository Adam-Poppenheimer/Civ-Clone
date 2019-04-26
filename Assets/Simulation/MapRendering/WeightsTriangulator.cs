using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.HexMap;

using UnityCustomUtilities.Extensions;

namespace Assets.Simulation.MapRendering {

    public class WeightsTriangulator : IWeightsTriangulator {

        #region static fields and properties

        //Weights are Center, Left, Right, and NextRight in that order
        private static Color CenterWeights               = new Color(1f,      0f,      0f,      0f     );
        private static Color CenterRightWeights          = new Color(0.5f,    0f,      0.5f,    0f     );
        private static Color CenterLeftRightWeights      = new Color(1f / 3f, 1f / 3f, 1f / 3f, 0f     );
        private static Color CenterRightNextRightWeights = new Color(1f / 3f, 0f,      1f / 3f, 1f / 3f);

        #endregion

        #region instance fields and properties

        private IMapRenderConfig RenderConfig;
        private IHexGrid         Grid;

        #endregion

        #region constructors

        [Inject]
        public WeightsTriangulator(IMapRenderConfig renderConfig, IHexGrid grid) {
            RenderConfig = renderConfig;
            Grid         = grid;
        }

        #endregion

        #region instance methods

        #region from IWeightsTriangulator

        public void TriangulateCellWeights(IHexCell center, IHexMesh weightsMesh) {
            foreach(var direction in EnumUtil.GetValues<HexDirection>()) {
                TriangulateWeights_Center(center, direction, weightsMesh);

                var right = Grid.GetNeighbor(center, direction);

                if(right == null) {
                    TriangulateWeights_EmptyEdge(center, direction, weightsMesh);
                    continue;
                }

                TriangulateWeights_Edge(center, right, direction, weightsMesh);

                var left      = Grid.GetNeighbor(center, direction.Previous());
                var nextRight = Grid.GetNeighbor(center, direction.Next());

                
                TriangulateWeights_PreviousCorner(center, left,  right,     direction, weightsMesh);
                TriangulateWeights_NextCorner    (center, right, nextRight, direction, weightsMesh);
            }
        }

        #endregion

        private void TriangulateWeights_Center(IHexCell center, HexDirection direction, IHexMesh weightsMesh) {
            weightsMesh.AddTriangle(
                Vector3.down + center.AbsolutePosition,
                Vector3.down + center.AbsolutePosition + RenderConfig.GetFirstSolidCorner (direction),
                Vector3.down + center.AbsolutePosition + RenderConfig.GetSecondSolidCorner(direction)
            );

            weightsMesh.AddTriangleColor(CenterWeights);
        }

        private void TriangulateWeights_EmptyEdge(IHexCell center, HexDirection direction, IHexMesh weightsMesh) {
            weightsMesh.AddQuad(
                Vector3.down + center.AbsolutePosition + RenderConfig.GetFirstSolidCorner (direction),
                Vector3.down + center.AbsolutePosition + RenderConfig.GetSecondSolidCorner(direction),
                Vector3.down + center.AbsolutePosition + RenderConfig.GetFirstCorner      (direction),
                Vector3.down + center.AbsolutePosition + RenderConfig.GetSecondCorner     (direction)
            );

            weightsMesh.AddQuadColor(CenterWeights);
        }

        private void TriangulateWeights_Edge(IHexCell center, IHexCell right, HexDirection direction, IHexMesh weightsMesh) {
            Vector3 centerOne = Vector3.down + center.AbsolutePosition + RenderConfig.GetFirstSolidCorner (direction);
            Vector3 centerTwo = Vector3.down + center.AbsolutePosition + RenderConfig.GetSecondSolidCorner(direction);

            Vector3 rightOne = Vector3.down + right.AbsolutePosition + RenderConfig.GetSecondSolidCorner(direction.Opposite());
            Vector3 rightTwo = Vector3.down + right.AbsolutePosition + RenderConfig.GetFirstSolidCorner (direction.Opposite());

            weightsMesh.AddQuad(
                centerOne, centerTwo, (centerOne + rightOne) / 2f, (centerTwo + rightTwo) / 2f
            );  

            weightsMesh.AddQuadColor(CenterWeights, CenterRightWeights);
        }

        private void TriangulateWeights_PreviousCorner(
            IHexCell center, IHexCell left, IHexCell right, HexDirection direction, IHexMesh weightsMesh
        ) {
            Vector3 intermediate = (
                center.AbsolutePosition + RenderConfig.GetFirstSolidCorner (direction) + 
                right .AbsolutePosition + RenderConfig.GetSecondSolidCorner(direction.Opposite())
            ) / 2f;

            weightsMesh.AddTriangle(
                Vector3.down + center.AbsolutePosition + RenderConfig.GetFirstSolidCorner(direction),
                Vector3.down + center.AbsolutePosition + RenderConfig.GetFirstCorner     (direction),
                Vector3.down + intermediate
            );

            if(left != null) {
                weightsMesh.AddTriangleColor(CenterWeights, CenterLeftRightWeights, CenterRightWeights);
            }else {
                weightsMesh.AddTriangleColor(CenterWeights, CenterRightWeights, CenterRightWeights);
            }
        }

        private void TriangulateWeights_NextCorner(
            IHexCell center, IHexCell right, IHexCell nextRight, HexDirection direction, IHexMesh weightsMesh
        ) {
            Vector3 intermediate = (
                center.AbsolutePosition + RenderConfig.GetSecondSolidCorner(direction) + 
                right .AbsolutePosition + RenderConfig.GetFirstSolidCorner (direction.Opposite())
            ) / 2f;

            weightsMesh.AddTriangle(
                Vector3.down + center.AbsolutePosition + RenderConfig.GetSecondSolidCorner(direction),
                Vector3.down + intermediate,
                Vector3.down + center.AbsolutePosition + RenderConfig.GetSecondCorner     (direction)
            );

            if(nextRight != null) {
                weightsMesh.AddTriangleColor(CenterWeights, CenterRightWeights, CenterRightNextRightWeights);
            }else {
                weightsMesh.AddTriangleColor(CenterWeights, CenterRightWeights, CenterRightWeights);
            }
        }

        #endregion

    }

}
