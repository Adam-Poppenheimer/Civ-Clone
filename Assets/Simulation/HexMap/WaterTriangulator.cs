﻿using System;
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
        private IHexMapRenderConfig RenderConfig;

        #endregion

        #region constructors

        [Inject]
        public WaterTriangulator(
            IHexGridMeshBuilder meshBuilder, IRiverCanon riverCanon,
            INoiseGenerator noiseGenerator, IHexMapRenderConfig renderConfig
        ) {
            MeshBuilder    = meshBuilder;
            RiverCanon     = riverCanon;
            NoiseGenerator = noiseGenerator;
            RenderConfig   = renderConfig;
        }

        #endregion

        #region instance methods

        #region from IWaterTriangulator

        public bool ShouldTriangulateWater(CellTriangulationData data) {
            return data.Center.Terrain.IsWater();
        }

        public void TriangulateWater(CellTriangulationData data) {
            if(data.Right == null) {
                return;
            }

            if(!data.Right.Terrain.IsWater()) {
                TriangulateWaterShoreCenter(data);

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

        //There is a river not adjacent to Center leading into the corner between Center, Left, and Right
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

            bool riverIsFeedingWater = RiverCanon.GetFlowOfRiverAtEdge(data.Left, data.Direction.Next()) == RiverFlow.Clockwise;

            TriangulateEstuarySection(data, leftShoreEdge, rightShoreEdge, riverIsFeedingWater);
        }

        //There is a river not adjacent to Center leading into the corner between Center, Right, and NextRight
        private void TriangulateWaterShoreEdgeWithEstuary_Next(CellTriangulationData data) {
            var rightShoreEdge = data.RightToCenterEdge;            

            rightShoreEdge.V1.y = rightShoreEdge.V2.y = rightShoreEdge.V3.y = rightShoreEdge.V4.y = rightShoreEdge.V5.y = data.Center.WaterSurfaceY;

            MeshBuilder.TriangulateEdgeStripPartial(
                data.CenterToRightWaterEdge, MeshBuilder.Weights1, data.Center.Index, 0f, false,
                rightShoreEdge,              MeshBuilder.Weights2, data.Right .Index, 1f, false,
                MeshBuilder.WaterShore, true, false
            );

            TriangulateShoreCorner(data, rightShoreEdge);
        }

        //There are rivers not adjacent to Center that are leading into both of the corners flanking
        //Right.
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

            bool riverIsFeedingWater = RiverCanon.GetFlowOfRiverAtEdge(data.Left, data.Direction.Next()) == RiverFlow.Clockwise;

            TriangulateEstuarySection(data, leftShoreEdge, rightShoreEdge, riverIsFeedingWater);
        }

        //Center is a water cell, Left and Right are not. There is a river between
        //Left and Right.
        //The first set of UV coordinates is for the water shore effect. The second
        //is for rivers.
        private void TriangulateEstuarySection(
            CellTriangulationData data, EdgeVertices leftShoreEdge, EdgeVertices rightShoreEdge,
            bool riverIsFeedingWater
        ) {
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

            //Adds the two quads on either side of the triangle to complete
            //the estuary's triangulation. We build the quads manually
            //instead of through AddQuadUnperturbed so we can make sure that
            //the estuary is symmetrical, and to make figuring out river UVs
            //easier

            MeshBuilder.AddTriangle(
                data.CenterToLeftWaterEdge.V5, MeshBuilder.Weights1, new Vector2(0f, 0f),
                data.CenterToLeftWaterEdge.V4, MeshBuilder.Weights1, new Vector2(0f, 0f),
                leftShoreEdge.V5,              MeshBuilder.Weights2, new Vector2(1f, 1f),
                indices, MeshBuilder.Estuaries
            );            

            MeshBuilder.AddTriangle(
                data.CenterToLeftWaterEdge.V4, MeshBuilder.Weights1, new Vector2(0f, 0f),
                leftShoreEdge.V4,              MeshBuilder.Weights2, new Vector2(0f, 1f),
                leftShoreEdge.V5,              MeshBuilder.Weights2, new Vector2(1f, 1f),
                indices, MeshBuilder.Estuaries
            );


            MeshBuilder.AddTriangle(
                data.CenterToRightWaterEdge.V2, MeshBuilder.Weights1, new Vector2(0f, 0f),
                data.CenterToRightWaterEdge.V1, MeshBuilder.Weights1, new Vector2(0f, 0f),
                rightShoreEdge.V1,              MeshBuilder.Weights3, new Vector2(1f, 1f),
                indices, MeshBuilder.Estuaries
            );            

            MeshBuilder.AddTriangle(
                data.CenterToRightWaterEdge.V2, MeshBuilder.Weights1, new Vector2(0f, 0f),
                rightShoreEdge.V1,              MeshBuilder.Weights3, new Vector2(1f, 1f),
                rightShoreEdge.V2,              MeshBuilder.Weights3, new Vector2(0f, 1f),
                indices, MeshBuilder.Estuaries
            );

            //We want the river to either spread out as it's draining into the standing water
            //or compress as it's funnelling into the river. We do this by manipulating the river
            //UV2s. The U coordinate controls the left/right orientation of the river, while the V
            //controls its flow. To get the inflow/outflow to curve, we need to manipulate the V
            //coordinates.

            //Consider the midline of this segment of the river, assuming that it extends out into
            //our open water. When the river is in a trough, the flow only marches along the midline
            //so we only modify V based on our position there. When we go into open water, our V
            //coordinate changes based on the distance away from the midline, since the flow of the
            //water either spreads out or contracts as it gets farther away from the center. V values
            //are thus manipulated based on the distance from the midline, increasing if the river is
            //feeding the water and decreasing if the water is feeding the river.

            if(riverIsFeedingWater) {
                float farLeftU  = 1.5f;  float leftU  = 1f; float nearLeftU  = 0.7f;
                float farRightU = -0.5f; float rightU = 0f; float nearRightU = 0.3f;

                //For the middle triangle
                //Points are CenterToRightWaterEdge.V1, LeftCorner, and RightCorner
                MeshBuilder.Estuaries.AddTriangleUV2(
                    new Vector2(0.5f,   RenderConfig.RiverEdgeEndV + 0.45f),
                    new Vector2(leftU,  RenderConfig.RiverEdgeEndV),
                    new Vector2(rightU, RenderConfig.RiverEdgeEndV)
                );

                //For the left quad

                //Points are CenterToLeftWaterEdge.V5, CenterToLeftWaterEdge.V4, and leftShoreEdge.V5
                MeshBuilder.Estuaries.AddTriangleUV2(
                    new Vector2(0.5f,      RenderConfig.RiverEdgeEndV + 0.45f),
                    new Vector2(nearLeftU, RenderConfig.RiverEdgeEndV + 0.6f),
                    new Vector2(leftU,     RenderConfig.RiverEdgeEndV)
                );

                //Points are CenterToLeftWaterEdge.V4, leftShoreEdge.V4, and leftShoreEdge.V5
                MeshBuilder.Estuaries.AddTriangleUV2(
                    new Vector2(nearLeftU, RenderConfig.RiverEdgeEndV + 0.6f),
                    new Vector2(farLeftU,  RenderConfig.RiverEdgeEndV + 0.4f),
                    new Vector2(leftU,     RenderConfig.RiverEdgeEndV)
                );

                //For the right quad

                //Points are CenterToRightWaterEdge.V2, CenterToRightWaterEdge.V1, and rightShoreEdge.V1
                MeshBuilder.Estuaries.AddTriangleUV2(
                    new Vector2(nearRightU, RenderConfig.RiverEdgeEndV + 0.6f),
                    new Vector2(0.5f,       RenderConfig.RiverEdgeEndV + 0.45f),
                    new Vector2(rightU,     RenderConfig.RiverEdgeEndV)
                );

                //Points are CenterToRightWaterEdge.V2, rightShoreEdge.V1, and rightShoreEdge.V2
                MeshBuilder.Estuaries.AddTriangleUV2(
                    new Vector2(nearRightU, RenderConfig.RiverEdgeEndV + 0.6f),
                    new Vector2(rightU,     RenderConfig.RiverEdgeEndV),
                    new Vector2(farRightU,  RenderConfig.RiverEdgeEndV + 0.4f)
                );

            }else {
                float farLeftU  = -0.5f; float leftU  = 0f; float nearLeftU  = 0.3f;
                float farRightU = 1.5f;  float rightU = 1f; float nearRightU = 0.7f;

                //For the middle triangle
                //Points are CenterToRightWaterEdge.V1, LeftCorner, and RightCorner
                MeshBuilder.Estuaries.AddTriangleUV2(
                    new Vector2(0.5f,   RenderConfig.RiverEdgeStartV - 0.45f),
                    new Vector2(leftU,  RenderConfig.RiverEdgeStartV),
                    new Vector2(rightU, RenderConfig.RiverEdgeStartV)
                );

                //For the left quad

                //Points are CenterToLeftWaterEdge.V5, CenterToLeftWaterEdge.V4, and leftShoreEdge.V5
                MeshBuilder.Estuaries.AddTriangleUV2(
                    new Vector2(0.5f,      RenderConfig.RiverEdgeStartV - 0.45f),
                    new Vector2(nearLeftU, RenderConfig.RiverEdgeStartV - 0.6f),
                    new Vector2(leftU,     RenderConfig.RiverEdgeStartV)
                );

                //Points are CenterToLeftWaterEdge.V4, leftShoreEdge.V4, and leftShoreEdge.V5
                MeshBuilder.Estuaries.AddTriangleUV2(
                    new Vector2(nearLeftU, RenderConfig.RiverEdgeStartV - 0.6f),
                    new Vector2(-farLeftU, RenderConfig.RiverEdgeStartV - 0.4f),
                    new Vector2(leftU,     RenderConfig.RiverEdgeStartV)
                );

                //For the right quad

                //Points are CenterToRightWaterEdge.V2, CenterToRightWaterEdge.V1, and rightShoreEdge.V1
                MeshBuilder.Estuaries.AddTriangleUV2(
                    new Vector2(nearRightU, RenderConfig.RiverEdgeStartV - 0.6f),
                    new Vector2(0.5f,       RenderConfig.RiverEdgeStartV - 0.45f),
                    new Vector2(rightU,     RenderConfig.RiverEdgeStartV)
                );

                //Points are CenterToRightWaterEdge.V2, rightShoreEdge.V1, and rightShoreEdge.V2
                MeshBuilder.Estuaries.AddTriangleUV2(
                    new Vector2(nearRightU, RenderConfig.RiverEdgeStartV - 0.6f),
                    new Vector2(rightU,     RenderConfig.RiverEdgeStartV),
                    new Vector2(farRightU,  RenderConfig.RiverEdgeStartV - 0.4f)
                );
            }
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
                Vector3 v3 = data.Left.Terrain.IsWater() ? data.LeftToCenterWaterEdge.V1 : data.LeftCorner;
                v3.y = data.Center.WaterSurfaceY;

                MeshBuilder.AddTriangle(
                    data.CenterToRightWaterEdge.V1, MeshBuilder.Weights1, Vector2.zero,
                    v3,                             MeshBuilder.Weights2, new Vector2(0f, data.Left.Terrain.IsWater() ? 0f : 1f),
                    shoreEdgeTwo.V1,                MeshBuilder.Weights3, new Vector2(0f, 1f),
                    new Vector3(data.Center.Index, data.Left.Index, data.Right.Index), MeshBuilder.WaterShore
                );
            }
        }

        private void TriangulateOpenWater(CellTriangulationData data) {
            MeshBuilder.AddTriangle(
                data.CenterWaterMidpoint,       data.Center.Index, MeshBuilder.Weights1,
                data.CenterToRightWaterEdge.V1, data.Right .Index, MeshBuilder.Weights1,
                data.CenterToRightWaterEdge.V5, data.Right .Index, MeshBuilder.Weights1,
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

                if(data.Direction <= HexDirection.E && data.Left != null && data.Left.Terrain.IsWater()) {
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
