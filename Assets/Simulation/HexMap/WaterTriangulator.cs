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
        private IRiverCanon         RiverCanon;
        private INoiseGenerator     NoiseGenerator;

        #endregion

        #region constructors

        [Inject]
        public WaterTriangulator(
            IHexGridMeshBuilder meshBuilder, IRiverCanon riverCanon,
            INoiseGenerator noiseGenerator
        ) {
            MeshBuilder    = meshBuilder;
            RiverCanon     = riverCanon;
            NoiseGenerator = noiseGenerator;
        }

        #endregion

        #region instance methods

        #region from IWaterTriangulator

        public bool ShouldTriangulateWater(CellTriangulationData data) {
            return data.Center.IsUnderwater;
        }

        public void TriangulateWater(CellTriangulationData data) {
            TriangulateWaterShoreCenter(data);

            if(data.Right != null && !data.Right.IsUnderwater) {
                var previousCornerHasEstuary = data.IsRiverCorner;
                var nextCornerHasEstuary     = RiverCanon.HasRiverAlongEdge(data.Right, data.Direction.Next2());

                if(previousCornerHasEstuary) {
                    if(nextCornerHasEstuary) {
                        TriangulateWaterShoreEdgeWithEstuary_PreviousAndNext(data);
                    }else {
                        TriangulateWaterShoreEdgeWithEstuary_Previous(data);
                    }
                }else if(nextCornerHasEstuary){
                    TriangulateWaterShoreEdgeWithEstuary_Next(data);

                }else {
                    TriangulateWaterShoreEdge_NoRivers(data);
                }
                
            }else {
                TriangulateOpenWater(data);
            }
        }

        #endregion

        private void TriangulateWaterShoreEdgeWithEstuary_Previous(CellTriangulationData data) {
            var leftShoreEdge  = data.LeftToCenterEdge;
            var rightShoreEdge = data.RightToCenterEdge;            

            rightShoreEdge.V1.y = rightShoreEdge.V2.y = rightShoreEdge.V3.y = rightShoreEdge.V4.y = rightShoreEdge.V5.y = data.Center.WaterSurfaceY;
            leftShoreEdge .V1.y = leftShoreEdge .V2.y = leftShoreEdge .V3.y = leftShoreEdge .V4.y = leftShoreEdge .V5.y = data.Center.WaterSurfaceY;

            MeshBuilder.TriangulateEdgeStripPartial(
                data.CenterToRightWaterEdge, MeshBuilder.Weights1, data.Center.Index, 0f, false,
                rightShoreEdge,              MeshBuilder.Weights2, data.Right .Index, 1f, false,
                MeshBuilder.WaterShore, false, true
            );

            TriangulateEstuarySection(data, leftShoreEdge, rightShoreEdge);
        }

        private void TriangulateWaterShoreEdgeWithEstuary_Next(CellTriangulationData data) {
            var leftShoreEdge  = data.LeftToCenterEdge;
            var rightShoreEdge = data.RightToCenterEdge;            

            rightShoreEdge.V1.y = rightShoreEdge.V2.y = rightShoreEdge.V3.y = rightShoreEdge.V4.y = rightShoreEdge.V5.y = data.Center.WaterSurfaceY;
            leftShoreEdge .V1.y = leftShoreEdge .V2.y = leftShoreEdge .V3.y = leftShoreEdge .V4.y = leftShoreEdge .V5.y = data.Center.WaterSurfaceY;

            MeshBuilder.TriangulateEdgeStripPartial(
                data.CenterToRightWaterEdge, MeshBuilder.Weights1, data.Center.Index, 0f, false,
                rightShoreEdge,              MeshBuilder.Weights2, data.Right .Index, 1f, false,
                MeshBuilder.WaterShore, true, false
            );

            TriangulateShoreCorner(data, rightShoreEdge);
        }

        private void TriangulateWaterShoreEdgeWithEstuary_PreviousAndNext(CellTriangulationData data) {
            var leftShoreEdge  = data.LeftToCenterEdge;
            var rightShoreEdge = data.RightToCenterEdge;            

            rightShoreEdge.V1.y = rightShoreEdge.V2.y = rightShoreEdge.V3.y = rightShoreEdge.V4.y = rightShoreEdge.V5.y = data.Center.WaterSurfaceY;
            leftShoreEdge .V1.y = leftShoreEdge .V2.y = leftShoreEdge .V3.y = leftShoreEdge .V4.y = leftShoreEdge .V5.y = data.Center.WaterSurfaceY;

            MeshBuilder.TriangulateEdgeStripPartial(
                data.CenterToRightWaterEdge, MeshBuilder.Weights1, data.Center.Index, 0f, false,
                rightShoreEdge,              MeshBuilder.Weights2, data.Right .Index, 1f, false,
                MeshBuilder.WaterShore, false, false
            );

            TriangulateEstuarySection(data, leftShoreEdge, rightShoreEdge);
        }

        //Center is a water cell, Left and Right are not. There is a river between
        //Left and Right.
        //The first set of UV coordinates is for the water shore effect. The second
        //is for rivers.
        private void TriangulateEstuarySection(CellTriangulationData data, EdgeVertices leftShoreEdge, EdgeVertices rightShoreEdge) {
            var indices = new Vector3(data.Center.Index, data.Left.Index, data.Right.Index);

            Vector3 yAdjustedLeft = data.PerturbedLeftCorner, yAdjustedRight = data.PerturbedRightCorner;

            yAdjustedLeft.y = yAdjustedRight.y = data.Center.WaterSurfaceY;            

            //Adds the triangle from the tip of the normal water's center to the
            //opposite edges of the corner we're working on (which are LeftCorner and RightCorner)
            MeshBuilder.Estuaries.AddTriangleUnperturbed(
                NoiseGenerator.Perturb(data.CenterToRightWaterEdge.V1),
                yAdjustedLeft, yAdjustedRight
            );

            MeshBuilder.Estuaries.AddTriangleCellData(
                indices, MeshBuilder.Weights1, MeshBuilder.Weights2, MeshBuilder.Weights3
            );

            MeshBuilder.Estuaries.AddTriangleUV (new Vector2(0f, 0f), new Vector2(1f, 1f), new Vector2(1f, 1f));
            MeshBuilder.Estuaries.AddTriangleUV2(new Vector2(0.5f, 0.45f), new Vector2(0f, 0f), new Vector2(1f, 0f));

            //Adds the two quads on either side of the triangle to complete
            //the estuary's triangulation. We build the quads manually
            //instead of through AddQuadUnperturbed so we can make sure that
            //the estuary is symmetrical, and to make figuring out river UVs
            //easier

            MeshBuilder.AddTriangleUnperturbed(
                data.CenterToLeftWaterEdge.V5, MeshBuilder.Weights1, new Vector2(0f, 0f),
                data.CenterToLeftWaterEdge.V4, MeshBuilder.Weights1, new Vector2(0f, 0f),
                leftShoreEdge.V5,              MeshBuilder.Weights1, new Vector2(1f, 1f),
                indices, MeshBuilder.Estuaries
            );
            MeshBuilder.Estuaries.AddTriangleUV2(new Vector2(0.5f, 0.45f), new Vector2(0.3f, 0.6f), new Vector2(0f, 0f));

            MeshBuilder.AddTriangleUnperturbed(
                data.CenterToLeftWaterEdge.V4, MeshBuilder.Weights1, new Vector2(0f, 0f),
                leftShoreEdge.V4,              MeshBuilder.Weights1, new Vector2(0f, 1f),
                leftShoreEdge.V5,              MeshBuilder.Weights1, new Vector2(1f, 1f),
                indices, MeshBuilder.Estuaries
            );
            MeshBuilder.Estuaries.AddTriangleUV2(new Vector2(0.3f, 0.6f), new Vector2(-0.5f, 0.4f), new Vector2(0f, 0f));


            MeshBuilder.AddTriangleUnperturbed(
                data.CenterToRightWaterEdge.V2, MeshBuilder.Weights1, new Vector2(0f, 0f),
                data.CenterToRightWaterEdge.V1, MeshBuilder.Weights1, new Vector2(0f, 0f),
                rightShoreEdge.V1,              MeshBuilder.Weights1, new Vector2(1f, 1f),
                indices, MeshBuilder.Estuaries
            );
            MeshBuilder.Estuaries.AddTriangleUV2( new Vector2(0.7f, 0.6f), new Vector2(0.5f, 0.45f), new Vector2(1f, 0f));

            MeshBuilder.AddTriangleUnperturbed(
                data.CenterToRightWaterEdge.V2, MeshBuilder.Weights1, new Vector2(0f, 0f),
                rightShoreEdge.V1,              MeshBuilder.Weights1, new Vector2(1f, 1f),
                rightShoreEdge.V2,              MeshBuilder.Weights1, new Vector2(0f, 1f),
                indices, MeshBuilder.Estuaries
            );
            MeshBuilder.Estuaries.AddTriangleUV2(new Vector2(0.7f, 0.6f), new Vector2(1f, 0f), new Vector2(1.5f, 0.4f));
        }

        private void TriangulateWaterShoreEdge_NoRivers(CellTriangulationData data) {
            var shoreEdgeTwo = data.RightToCenterEdge;
            shoreEdgeTwo.V1.y = shoreEdgeTwo.V2.y = shoreEdgeTwo.V3.y = shoreEdgeTwo.V4.y = shoreEdgeTwo.V5.y = data.Center.WaterSurfaceY;

            MeshBuilder.TriangulateEdgeStrip(
                data.CenterToRightWaterEdge, MeshBuilder.Weights1, data.Center.Index, 0f, false,
                shoreEdgeTwo,                MeshBuilder.Weights2, data.Right.Index,  1f, false,
                MeshBuilder.WaterShore
            );

            TriangulateShoreCorner(data, shoreEdgeTwo);
        }

        private void TriangulateWaterShoreCenter(CellTriangulationData data) {
            MeshBuilder.TriangulateEdgeFan(
                data.CenterWaterMidpoint, data.CenterToRightWaterEdge, data.Center.Index,
                MeshBuilder.Water, false
            );
        }

        private void TriangulateShoreCorner(CellTriangulationData data, EdgeVertices shoreEdgeTwo) {
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
