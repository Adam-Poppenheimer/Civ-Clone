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
        private HexMesh             Water;
        private HexMesh             WaterShore;

        #endregion

        #region constructors

        [Inject]
        public WaterTriangulator(
            IHexGridMeshBuilder meshBuilder,
            [Inject(Id = "Water")] HexMesh water,
            [Inject(Id = "Water Shore")] HexMesh waterShore
        ) {
            MeshBuilder = meshBuilder;
            Water       = water;
            WaterShore  = waterShore;
        }

        #endregion

        #region instance methods

        #region from IWaterTriangulator

        public bool ShouldTriangulateWater(CellTriangulationData data) {
            return data.Center.IsUnderwater;
        }

        public void TriangulateWater(CellTriangulationData data) {
            if(data.Right != null && !data.Right.IsUnderwater) {
                TriangulateWaterShore(data);
            }else {
                TriangulateOpenWater(data);
            }
        }

        #endregion

        private void TriangulateWaterShore(CellTriangulationData data) {
            Water.AddTriangle(data.CenterWaterMidpoint, data.CenterToRightWaterEdge.V1, data.CenterToRightWaterEdge.V2);
            Water.AddTriangle(data.CenterWaterMidpoint, data.CenterToRightWaterEdge.V2, data.CenterToRightWaterEdge.V3);
            Water.AddTriangle(data.CenterWaterMidpoint, data.CenterToRightWaterEdge.V3, data.CenterToRightWaterEdge.V4);
            Water.AddTriangle(data.CenterWaterMidpoint, data.CenterToRightWaterEdge.V4, data.CenterToRightWaterEdge.V5);

            Vector3 indices;
            indices.x = indices.z = data.Center.Index;
            indices.y = data.Right.Index;
            Water.AddTriangleCellData(indices, MeshBuilder.Weights1);
            Water.AddTriangleCellData(indices, MeshBuilder.Weights1);
            Water.AddTriangleCellData(indices, MeshBuilder.Weights1);
            Water.AddTriangleCellData(indices, MeshBuilder.Weights1);

            var shoreEdgeTwo = data.RightToCenterEdge;
            shoreEdgeTwo.V1.y = shoreEdgeTwo.V2.y = shoreEdgeTwo.V3.y = shoreEdgeTwo.V4.y = shoreEdgeTwo.V5.y = data.Center.WaterSurfaceY;

            WaterShore.AddQuad(
                data.CenterToRightWaterEdge.V1, data.CenterToRightWaterEdge.V2,
                shoreEdgeTwo.V1,                shoreEdgeTwo.V2
            );

            WaterShore.AddQuad(
                data.CenterToRightWaterEdge.V2, data.CenterToRightWaterEdge.V3,
                shoreEdgeTwo.V2,                shoreEdgeTwo.V3
            );

            WaterShore.AddQuad(
                data.CenterToRightWaterEdge.V3, data.CenterToRightWaterEdge.V4,
                shoreEdgeTwo.V3,                shoreEdgeTwo.V4
            );

            WaterShore.AddQuad(
                data.CenterToRightWaterEdge.V4, data.CenterToRightWaterEdge.V5,
                shoreEdgeTwo.V4,                shoreEdgeTwo.V5
            );

            WaterShore.AddQuadUV(0f, 0f, 0f, 1f);
            WaterShore.AddQuadUV(0f, 0f, 0f, 1f);
            WaterShore.AddQuadUV(0f, 0f, 0f, 1f);
            WaterShore.AddQuadUV(0f, 0f, 0f, 1f);

            WaterShore.AddQuadCellData(indices, MeshBuilder.Weights1, MeshBuilder.Weights2);
            WaterShore.AddQuadCellData(indices, MeshBuilder.Weights1, MeshBuilder.Weights2);
            WaterShore.AddQuadCellData(indices, MeshBuilder.Weights1, MeshBuilder.Weights2);
            WaterShore.AddQuadCellData(indices, MeshBuilder.Weights1, MeshBuilder.Weights2);

            if(data.Left != null) {
                Vector3 v3 = data.Left.IsUnderwater ? data.LeftToCenterWaterEdge.V1 : data.LeftCorner;
                v3.y = data.Center.WaterSurfaceY;

                WaterShore.AddTriangle(data.CenterToRightWaterEdge.V1, v3, shoreEdgeTwo.V1);

                WaterShore.AddTriangleUV(
                    new Vector2(0f, 0f),
                    new Vector2(0f, data.Left.IsUnderwater ? 0f : 1f),
                    new Vector2(0f, 1f)
                );

                indices.z = data.Left.Index;
                WaterShore.AddTriangleCellData(
                    indices, MeshBuilder.Weights1, MeshBuilder.Weights3, MeshBuilder.Weights2
                );
            }
        }

        private void TriangulateOpenWater(CellTriangulationData data) {
            Water.AddTriangle(
                data.CenterWaterMidpoint, data.CenterToRightWaterEdge.V1, data.CenterToRightWaterEdge.V5
            );

            Vector3 indices;
            indices.x = indices.y = indices.z = data.Center.Index;
            Water.AddTriangleCellData(indices, MeshBuilder.Weights1);

            if(data.Direction <= HexDirection.SE && data.Right != null) {

                Water.AddQuad(
                    data.CenterToRightWaterEdge.V1, data.CenterToRightWaterEdge.V5,
                    data.RightToCenterWaterEdge.V1, data.RightToCenterWaterEdge.V5
                );

                indices.y = data.Right.Index;
                Water.AddQuadCellData(indices, MeshBuilder.Weights1, MeshBuilder.Weights2);

                if(data.Direction <= HexDirection.E && data.Left != null && data.Left.IsUnderwater) {
                    Water.AddTriangle(
                        data.CenterToRightWaterEdge.V1, data.LeftToCenterWaterEdge.V1,
                        data.RightToCenterWaterEdge.V1
                    );

                    indices.z = data.Left.Index;
                    Water.AddTriangleCellData(
                        indices, MeshBuilder.Weights1, MeshBuilder.Weights3, MeshBuilder.Weights2
                    );
                }
            }
        }

        #endregion

    }

}
