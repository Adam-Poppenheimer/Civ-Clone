using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

namespace Assets.Simulation.HexMap {

    public class WaterTriangulator : IWaterTriangulator {

        #region instance fields and properties

        private IHexGridMeshBuilder MeshBuilder;

        #endregion

        #region constructors

        [Inject]
        public WaterTriangulator(
            IHexGridMeshBuilder meshBuilder
        ) {
            MeshBuilder = meshBuilder;
        }

        #endregion

        #region instance methods

        #region from IWaterTriangulator

        public bool ShouldTriangulateWater(CellTriangulationData data) {
            return data.Center.IsUnderwater;
        }

        public void TriangulateWater(CellTriangulationData data) {
            if(data.Right != null && !data.Right.IsUnderwater) {
                TriangulateWaterWithShore(data);
            }else {
                TriangulateOpenWater(data);
            }
        }

        #endregion

        private void TriangulateWaterWithShore(CellTriangulationData data) {
            MeshBuilder.TriangulateEdgeFan(
                data.CenterWaterMidpoint, data.CenterToRightWaterEdge, data.Center.Index,
                MeshBuilder.Water, false
            );

            var shoreEdgeTwo = data.RightToCenterEdge;
            shoreEdgeTwo.V1.y = shoreEdgeTwo.V2.y = shoreEdgeTwo.V3.y = shoreEdgeTwo.V4.y = shoreEdgeTwo.V5.y = data.Center.WaterSurfaceY;

            MeshBuilder.TriangulateEdgeStrip(
                data.CenterToRightWaterEdge, MeshBuilder.Weights1, data.Center.Index, 0f, false,
                shoreEdgeTwo,                MeshBuilder.Weights2, data.Right.Index,  1f, false,
                MeshBuilder.WaterShore
            );

            if(data.Left != null) {
                Vector3 v3 = data.Left.IsUnderwater ? data.LeftToCenterWaterEdge.V1 : data.LeftCorner;
                v3.y = data.Center.WaterSurfaceY;

                MeshBuilder.AddTriangle(
                    data.CenterToRightWaterEdge.V1, MeshBuilder.Weights1, Vector2.zero,
                    v3,                             MeshBuilder.Weights2, new Vector2(0f, data.Left.IsUnderwater ? 0f : 1f),
                    shoreEdgeTwo.V1,                MeshBuilder.Weights3, new Vector2(0f, 1f),
                    new Vector3(data.Center.Index, data.Left.Index, data.Right.Index), MeshBuilder.WaterShore
                );
            }
        }

        private void TriangulateOpenWater(CellTriangulationData data) {
            MeshBuilder.AddTriangle(
                data.CenterWaterMidpoint,       data.Center.Index, MeshBuilder.Weights1,
                data.CenterToRightWaterEdge.V1, data.Center.Index, MeshBuilder.Weights1,
                data.CenterToRightWaterEdge.V5, data.Center.Index, MeshBuilder.Weights1,
                MeshBuilder.Water
            );

            if(data.Direction <= HexDirection.SE && data.Right != null) {
                MeshBuilder.AddQuad(
                    data.CenterToRightWaterEdge.V1, MeshBuilder.Weights1,
                    data.CenterToRightWaterEdge.V5, MeshBuilder.Weights1,
                    data.RightToCenterWaterEdge.V1, MeshBuilder.Weights2,
                    data.RightToCenterWaterEdge.V5, MeshBuilder.Weights2,
                    data.Center.Index, data.Right.Index, data.Right.Index,
                    MeshBuilder.Water
                );

                if(data.Direction <= HexDirection.E && data.Left != null && data.Left.IsUnderwater) {
                    MeshBuilder.AddTriangle(
                        data.CenterToRightWaterEdge.V1, data.Center.Index, MeshBuilder.Weights1,
                        data.LeftToCenterWaterEdge .V1, data.Left  .Index, MeshBuilder.Weights2,
                        data.RightToCenterWaterEdge.V1, data.Right .Index, MeshBuilder.Weights3,
                        MeshBuilder.Water
                    );
                }
            }
        }

        #endregion

    }

}
