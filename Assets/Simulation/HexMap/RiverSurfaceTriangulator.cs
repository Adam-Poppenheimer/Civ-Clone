using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Util;

namespace Assets.Simulation.HexMap {

    public class RiverSurfaceTriangulator : IRiverSurfaceTriangulator {

        #region instance fields and properties

        private IHexGridMeshBuilder MeshBuilder;
        private INoiseGenerator     NoiseGenerator;
        private IRiverCanon         RiverCanon;

        #endregion

        #region constructors

        [Inject]
        public RiverSurfaceTriangulator(
            IHexGridMeshBuilder meshBuilder, INoiseGenerator noiseGenerator,
            IRiverCanon riverCanon
        ) {
            MeshBuilder    = meshBuilder;
            NoiseGenerator = noiseGenerator;
            RiverCanon     = riverCanon;
        }

        #endregion

        #region instance methods

        #region from IRiverSurfaceTriangulator

        
        public void CreateRiverSurface_Confluence(CellTriangulationData data) {
            float confluenceY = Mathf.Min(
                data.Center.RiverSurfaceY, data.Left.RiverSurfaceY, data.Right.RiverSurfaceY
            );

            Vector3 yAdjustedCenter = data.PerturbedCenterCorner,
                    yAdjustedLeft   = data.PerturbedLeftCorner,
                    yAdjustedRight  = data.PerturbedRightCorner;

            yAdjustedCenter    .y = confluenceY;
            yAdjustedLeft      .y = confluenceY;
            yAdjustedRight     .y = confluenceY;

            var mesh = MeshBuilder.RiverConfluences;

            mesh.AddTriangleUnperturbed(yAdjustedCenter, yAdjustedLeft, yAdjustedRight);
            mesh.AddTriangleColor(new Color(1f, 0f, 0f), new Color(0f, 1f, 0f), new Color(0f, 0f, 1f));

            mesh.AddTriangleCellData(
                new Vector3(data.Center.Index, data.Left.Index, data.Right.Index),
                MeshBuilder.Weights1, MeshBuilder.Weights2, MeshBuilder.Weights123
            );

            if(RiverCanon.GetFlowOfRiverAtEdge(data.Center, data.Direction.Previous()) == RiverFlow.Clockwise) {
                mesh.AddTriangleUV(
                    new Vector2(HexMetrics.RiverStarboardU, HexMetrics.RiverEdgeEndV),
                    new Vector2(HexMetrics.RiverPortU,      HexMetrics.RiverEdgeEndV),
                    new Vector2(0.5f,                       HexMetrics.RiverEdgeEndV + HexMetrics.RiverConfluenceV)
                );
            }else {
                mesh.AddTriangleUV(
                    new Vector2(HexMetrics.RiverPortU,      HexMetrics.RiverEdgeStartV),
                    new Vector2(HexMetrics.RiverStarboardU, HexMetrics.RiverEdgeStartV),
                    new Vector2(0.5f,                       HexMetrics.RiverEdgeStartV - HexMetrics.RiverConfluenceV)
                );
            }
            
            if(RiverCanon.GetFlowOfRiverAtEdge(data.Center, data.Direction) == RiverFlow.Clockwise) {
                mesh.AddTriangleUV2(
                    new Vector2(HexMetrics.RiverStarboardU, HexMetrics.RiverEdgeStartV),
                    new Vector2(0.5f,                       HexMetrics.RiverEdgeStartV - HexMetrics.RiverConfluenceV),
                    new Vector2(HexMetrics.RiverPortU,      HexMetrics.RiverEdgeStartV)
                );
            }else {
                mesh.AddTriangleUV2(
                    new Vector2(HexMetrics.RiverPortU,      HexMetrics.RiverEdgeEndV),
                    new Vector2(0.5f,                       HexMetrics.RiverEdgeEndV + HexMetrics.RiverConfluenceV),
                    new Vector2(HexMetrics.RiverStarboardU, HexMetrics.RiverEdgeEndV)
                );
            }

            if(RiverCanon.GetFlowOfRiverAtEdge(data.Left, data.Direction.Next()) == RiverFlow.Clockwise) {
                mesh.AddTriangleUV3(
                    new Vector2(0.5f,                       HexMetrics.RiverEdgeEndV + HexMetrics.RiverConfluenceV),
                    new Vector2(HexMetrics.RiverStarboardU, HexMetrics.RiverEdgeEndV),
                    new Vector2(HexMetrics.RiverPortU,      HexMetrics.RiverEdgeEndV)
                );                
            }else {
                mesh.AddTriangleUV3(
                    new Vector2(0.5f,                       HexMetrics.RiverEdgeStartV - HexMetrics.RiverConfluenceV),
                    new Vector2(HexMetrics.RiverPortU,      HexMetrics.RiverEdgeStartV),
                    new Vector2(HexMetrics.RiverStarboardU, HexMetrics.RiverEdgeStartV)
                );
            }
        }

        public void CreateRiverSurface_ThisCornerToMiddle(CellTriangulationData thisData) {
            float riverSurfaceY = Mathf.Min(thisData.Center.RiverSurfaceY, thisData.Right.RiverSurfaceY);

            Vector3 leftOne   = NoiseGenerator.Perturb(thisData.RightToCenterEdge.V1);
            Vector3 leftTwo   = NoiseGenerator.Perturb(thisData.RightToCenterEdge.V2);
            Vector3 leftThree = NoiseGenerator.Perturb(thisData.RightToCenterEdge.V3);

            Vector3 rightOne   = NoiseGenerator.Perturb(thisData.CenterToRightEdge.V1);
            Vector3 rightTwo   = NoiseGenerator.Perturb(thisData.CenterToRightEdge.V2);
            Vector3 rightThree = NoiseGenerator.Perturb(thisData.CenterToRightEdge.V3);

            leftOne.y = leftTwo.y = leftThree.y = rightOne.y = rightTwo.y = rightThree.y = riverSurfaceY;

            bool isReversed = RiverCanon.GetFlowOfRiverAtEdge(thisData.Center, thisData.Direction) == RiverFlow.Counterclockwise;

            Vector3 indices = new Vector3(
                thisData.Center.Index, thisData.Center.Index, thisData.Center.Index
            );

            MeshBuilder.TriangulateRiverQuadUnperturbed(
                leftOne, rightOne, leftTwo, rightTwo,
                HexMetrics.GetRiverEdgeV(0), HexMetrics.GetRiverEdgeV(1),
                isReversed, indices
            );

            MeshBuilder.TriangulateRiverQuadUnperturbed(
                leftTwo, rightTwo, leftThree, rightThree,
                HexMetrics.GetRiverEdgeV(1), HexMetrics.GetRiverEdgeV(2),
                isReversed, indices
            );
        }

        public void CreateRiverSurface_MiddleToNextCorner(CellTriangulationData nextData) {
            float riverSurfaceY = Mathf.Min(nextData.Center.RiverSurfaceY, nextData.Left.RiverSurfaceY);

            Vector3 leftThree = NoiseGenerator.Perturb(nextData.LeftToCenterEdge.V3);
            Vector3 leftFour  = NoiseGenerator.Perturb(nextData.LeftToCenterEdge.V4);
            Vector3 leftFive  = NoiseGenerator.Perturb(nextData.LeftToCenterEdge.V5);

            Vector3 rightThree = NoiseGenerator.Perturb(nextData.CenterToLeftEdge.V3);
            Vector3 rightFour  = NoiseGenerator.Perturb(nextData.CenterToLeftEdge.V4);
            Vector3 rightFive  = NoiseGenerator.Perturb(nextData.CenterToLeftEdge.V5);

            leftThree.y = leftFour.y = leftFive.y = rightThree.y = rightFour.y = rightFive.y = riverSurfaceY;

            bool isReversed = RiverCanon.GetFlowOfRiverAtEdge(nextData.Center, nextData.Direction.Previous()) == RiverFlow.Counterclockwise;

            Vector3 indices = new Vector3(
                nextData.Center.Index, nextData.Center.Index, nextData.Center.Index
            );

            MeshBuilder.TriangulateRiverQuadUnperturbed(
                leftThree, rightThree, leftFour, rightFour,
                HexMetrics.GetRiverEdgeV(2), HexMetrics.GetRiverEdgeV(3),
                isReversed, indices
            );

            MeshBuilder.TriangulateRiverQuadUnperturbed(
                leftFour, rightFour, leftFive, rightFive,
                HexMetrics.GetRiverEdgeV(3), HexMetrics.GetRiverEdgeV(4),
                isReversed, indices
            );            
        }

        public void CreateRiverSurface_CurveCorner(CellTriangulationData data) {
            float riverSurfaceY = Mathf.Min(data.Center.RiverSurfaceY, data.Left.RiverSurfaceY, data.Right.RiverSurfaceY);

            bool isReversed = false;
            bool waterfallHasOppositeReversal = false;

            Vector3 centerCorner = Vector3.zero, leftCorner = Vector3.zero, rightCorner = Vector3.zero;

            bool buildCorner    = false;
            bool buildWaterfall = false;

            Vector3 waterfallUpRiverLeft   = Vector3.zero, waterfallUpRiverRight   = Vector3.zero;
            Vector3 waterfallDownRiverLeft = Vector3.zero, waterfallDownRiverRight = Vector3.zero;

            if(data.CenterToRightEdgeType == HexEdgeType.River) {
                if(data.CenterToLeftEdgeType == HexEdgeType.River) {
                    buildCorner = true;

                    centerCorner = data.PerturbedCenterCorner;
                    leftCorner   = data.PerturbedLeftCorner;
                    rightCorner  = data.PerturbedRightCorner;

                    centerCorner.y = leftCorner.y = rightCorner.y = riverSurfaceY;

                    isReversed = RiverCanon.GetFlowOfRiverAtEdge(data.Center, data.Direction) == RiverFlow.Counterclockwise;

                    if(data.Center.EdgeElevation > data.Right.EdgeElevation && data.Left.EdgeElevation > data.Right.EdgeElevation) {
                        buildWaterfall = true;

                        waterfallUpRiverLeft    = data.PerturbedLeftCorner;
                        waterfallUpRiverRight   = data.PerturbedCenterCorner;
                        waterfallDownRiverLeft  = leftCorner;
                        waterfallDownRiverRight = centerCorner;

                        waterfallUpRiverLeft.y = waterfallUpRiverRight.y = Mathf.Min(data.Center.RiverSurfaceY, data.Left.RiverSurfaceY);

                    }else if(data.Center.EdgeElevation > data.Left.EdgeElevation && data.Right.EdgeElevation > data.Left.EdgeElevation) {
                        buildWaterfall = true;

                        waterfallUpRiverLeft    = data.PerturbedCenterCorner;
                        waterfallUpRiverRight   = data.PerturbedRightCorner;
                        waterfallDownRiverLeft  = centerCorner;
                        waterfallDownRiverRight = rightCorner;

                        waterfallUpRiverLeft.y = waterfallUpRiverRight.y = Mathf.Min(data.Center.RiverSurfaceY, data.Right.RiverSurfaceY);
                    }

                }else if(data.LeftToRightEdgeType == HexEdgeType.River) {
                    buildCorner = true;

                    centerCorner = data.PerturbedRightCorner;
                    leftCorner   = data.PerturbedCenterCorner;
                    rightCorner  = data.PerturbedLeftCorner;

                    centerCorner.y = leftCorner.y = rightCorner.y = riverSurfaceY;

                    isReversed = RiverCanon.GetFlowOfRiverAtEdge(data.Right, data.Direction.Previous2()) == RiverFlow.Counterclockwise;

                    if(data.Center.EdgeElevation > data.Left.EdgeElevation && data.Right.EdgeElevation > data.Left.EdgeElevation) {
                        buildWaterfall = true;

                        waterfallUpRiverLeft    = data.PerturbedCenterCorner;
                        waterfallUpRiverRight   = data.PerturbedRightCorner;
                        waterfallDownRiverLeft  = leftCorner;
                        waterfallDownRiverRight = centerCorner;

                        waterfallUpRiverLeft.y = waterfallUpRiverRight.y = Mathf.Min(data.Center.RiverSurfaceY, data.Right.RiverSurfaceY);

                    }else if(data.Center.EdgeElevation < data.Right.EdgeElevation && data.Center.EdgeElevation < data.Left.EdgeElevation) {
                        buildWaterfall = true;
                        waterfallHasOppositeReversal = true;

                        waterfallUpRiverLeft    = data.PerturbedRightCorner;
                        waterfallUpRiverRight   = data.PerturbedLeftCorner;
                        waterfallDownRiverLeft  = centerCorner;
                        waterfallDownRiverRight = rightCorner;

                        waterfallUpRiverLeft.y = waterfallUpRiverRight.y = Mathf.Min(data.Left.RiverSurfaceY, data.Right.RiverSurfaceY);
                    }
                }

            }else if(data.CenterToLeftEdgeType == HexEdgeType.River && data.LeftToRightEdgeType == HexEdgeType.River) {
                buildCorner = true;

                centerCorner = data.PerturbedLeftCorner;
                leftCorner   = data.PerturbedRightCorner;
                rightCorner  = data.PerturbedCenterCorner;

                centerCorner.y = leftCorner.y = rightCorner.y = riverSurfaceY;

                isReversed = RiverCanon.GetFlowOfRiverAtEdge(data.Left, data.Direction.Next()) == RiverFlow.Counterclockwise;

                if(data.Center.EdgeElevation > data.Right.EdgeElevation && data.Left.EdgeElevation > data.Right.EdgeElevation) {
                    buildWaterfall = true;
                    waterfallHasOppositeReversal = true;

                    waterfallUpRiverLeft    = data.PerturbedLeftCorner;
                    waterfallUpRiverRight   = data.PerturbedCenterCorner;
                    waterfallDownRiverLeft  = centerCorner;
                    waterfallDownRiverRight = rightCorner;

                    waterfallUpRiverLeft.y = waterfallUpRiverRight.y = Mathf.Min(data.Center.RiverSurfaceY, data.Left.RiverSurfaceY);

                }else if(data.Center.EdgeElevation < data.Left.EdgeElevation && data.Center.EdgeElevation < data.Right.EdgeElevation) {
                    buildWaterfall = true;

                    waterfallUpRiverLeft    = data.PerturbedRightCorner;
                    waterfallUpRiverRight   = data.PerturbedLeftCorner;
                    waterfallDownRiverLeft  = leftCorner;
                    waterfallDownRiverRight = centerCorner;

                    waterfallUpRiverLeft.y = waterfallUpRiverRight.y = Mathf.Min(data.Left.RiverSurfaceY, data.Right.RiverSurfaceY);

                }
            }

            if(!buildCorner) {
                return;
            }

            if(isReversed) {
                MeshBuilder.AddTriangleUnperturbed(
                    centerCorner, data.Center.Index, MeshBuilder.Weights1,
                    leftCorner,   data.Left  .Index, MeshBuilder.Weights3,
                    rightCorner,  data.Right .Index, MeshBuilder.Weights2,
                    MeshBuilder.RiverCorners 
                );
                MeshBuilder.RiverCorners.AddTriangleUV(new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f));
            }else {
                MeshBuilder.AddTriangleUnperturbed(
                    centerCorner, data.Center.Index, MeshBuilder.Weights1,
                    leftCorner,   data.Left  .Index, MeshBuilder.Weights2,
                    rightCorner,  data.Right .Index, MeshBuilder.Weights3,
                    MeshBuilder.RiverCorners 
                );
                MeshBuilder.RiverCorners.AddTriangleUV(new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f));
            }

            if(buildWaterfall) {
                Vector3 indices = new Vector3(data.Center.Index, data.Left.Index, data.Right.Index);

                MeshBuilder.TriangulateRiverQuadUnperturbed(
                    waterfallUpRiverLeft,   waterfallUpRiverRight,
                    waterfallDownRiverLeft, waterfallDownRiverRight,
                    0.9f, 1f, waterfallHasOppositeReversal ? !isReversed : isReversed, indices
                );
            }
        }

        public void CreateRiverSurface_ConfluenceWaterfall(CellTriangulationData data, float yAdjustment) {
            CreateRiverSurface_ConfluenceWaterfall(data, yAdjustment, yAdjustment, yAdjustment);
        }

        //Center and Left both have a higher elevation than Right, which means a
        //waterfall should flow from the river between Center and Left into the
        //corner, towards Right.
        public void CreateRiverSurface_ConfluenceWaterfall(
            CellTriangulationData data, float centerYAdjustment,
            float leftYAdjustment, float rightYAdjustment
        ) {
            Vector3 yAdjustedCenter = data.PerturbedCenterCorner;
            Vector3 yAdjustedLeft   = data.PerturbedLeftCorner;
            Vector3 yAdjustedRight  = data.PerturbedRightCorner;

            yAdjustedCenter.y = centerYAdjustment;
            yAdjustedLeft  .y = leftYAdjustment;
            yAdjustedRight .y = rightYAdjustment;

            float edgeY = Mathf.Min(data.Center.RiverSurfaceY, data.Left.RiverSurfaceY);

            bool isReversed = RiverCanon.GetFlowOfRiverAtEdge(data.Center, data.Direction.Previous()) == RiverFlow.Counterclockwise;
            var indices = new Vector3(data.Center.Index, data.Center.Index, data.Center.Index);

            var waterfallUpRiverLeft  = data.PerturbedLeftCorner;
            var waterfallUpRiverRight = data.PerturbedCenterCorner;

            waterfallUpRiverLeft .y = edgeY;
            waterfallUpRiverRight.y = edgeY;

            //We want the waterfall to stop when it hits the water triangle at the confluence.
            //Rather than performing a ray-triangle intersection test (which can miss because
            //our ray may pass very close to the triangle's edge) we can use
            //ClosestPointsOnTwoLines to find our "intersection" with the water triangle
            //and use those as our down-river points. This section tests between one of the
            //water triangle's edges and a line that goes between the river's edge and one
            //of the trough points.

            Vector3 waterfallDownRiverLeft = Vector3.zero, waterfallDownRiverRight = Vector3.zero;
            Vector3 dummyVector;

            Vector3 waterfallVectorLeft  = data.PertrubedLeftRightTroughPoint   - waterfallUpRiverLeft;
            Vector3 waterfallVectorRight = data.PerturbedCenterRightTroughPoint - waterfallUpRiverRight;

            Vector3 rightLeftTriangleEdge   = yAdjustedLeft   - yAdjustedRight;
            Vector3 rightCenterTriangleEdge = yAdjustedCenter - yAdjustedRight;

            Geometry3D.ClosestPointsOnTwoLines(
                waterfallUpRiverLeft, waterfallVectorLeft,
                yAdjustedRight, rightLeftTriangleEdge,
                out waterfallDownRiverLeft, out dummyVector
            );

            Geometry3D.ClosestPointsOnTwoLines(
                waterfallUpRiverRight, waterfallVectorRight,
                yAdjustedRight, rightCenterTriangleEdge,
                out waterfallDownRiverRight, out dummyVector
            );

            MeshBuilder.TriangulateRiverQuadUnperturbed(
                waterfallUpRiverLeft, waterfallUpRiverRight,
                waterfallDownRiverLeft, waterfallDownRiverRight,
                HexMetrics.RiverEdgeEndV, HexMetrics.RiverEdgeEndV + HexMetrics.RiverWaterfallV,
                isReversed, indices
            );
        }

        //There is a river between Center and Left.
        //Center and Left are both above Right, which is underwater.
        //Creates a waterfall that flows from the river towards Right.
        //This waterfall consists of a single quad that goes from the
        //end of the river towards the solid edges of Right.
        public void CreateRiverSurface_EstuaryWaterfall(CellTriangulationData data, float yAdjustment) {
            float edgeY = Mathf.Min(data.Center.RiverSurfaceY, data.Left.RiverSurfaceY);

            bool isReversed = RiverCanon.GetFlowOfRiverAtEdge(data.Center, data.Direction.Previous()) == RiverFlow.Counterclockwise;
            var indices = new Vector3(data.Center.Index, data.Center.Index, data.Center.Index);

            Vector3 waterfallUpRiverLeft  = data.PerturbedLeftCorner;
            Vector3 waterfallUpRiverRight = data.PerturbedCenterCorner;

            waterfallUpRiverLeft .y = edgeY;
            waterfallUpRiverRight.y = edgeY;

            Vector3 downRiverLeftTarget  = Vector3.Lerp(
                NoiseGenerator.Perturb(data.RightToLeftEdge.V1), NoiseGenerator.Perturb(data.RightToLeftEdge.V2), 
                1f / 4f
            );

            Vector3 downRiverRightTarget = Vector3.Lerp(
                NoiseGenerator.Perturb(data.RightToCenterEdge.V1), NoiseGenerator.Perturb(data.RightToCenterEdge.V2),
                1f / 4f
            );

            float leftParam  = (data.Right.WaterSurfaceY - waterfallUpRiverLeft .y) / (downRiverLeftTarget  .y - waterfallUpRiverLeft .y);
            float rightParam = (data.Right.WaterSurfaceY - waterfallUpRiverRight.y) / (downRiverRightTarget .y - waterfallUpRiverRight.y);

            Vector3 waterfallDownRiverLeft  = Vector3.Lerp(waterfallUpRiverLeft,  downRiverLeftTarget,  leftParam);
            Vector3 waterfallDownRiverRight = Vector3.Lerp(waterfallUpRiverRight, downRiverRightTarget, rightParam);

            MeshBuilder.TriangulateRiverQuadUnperturbed(
                waterfallUpRiverLeft,   waterfallUpRiverRight,
                waterfallDownRiverLeft, waterfallDownRiverRight,
                HexMetrics.RiverEdgeEndV, HexMetrics.RiverEdgeEndV + HexMetrics.EstuaryWaterfallV, isReversed, indices
            );
        }

        //Creates the water surface for a river endpoint. The river lies between
        //Center and Right and is facing Left.
        public void CreateRiverEndpointSurface_Default(CellTriangulationData data) {
            var indices = new Vector3(data.Center.Index, data.Left.Index, data.Right.Index);

            Vector3 yAdjustedCenter = data.PerturbedCenterCorner,
                    yAdjustedLeft   = data.PerturbedLeftCorner,
                    yAdjustedRight  = data.PerturbedRightCorner;

            float riverY = Mathf.Min(data.Center.RiverSurfaceY, data.Right.RiverSurfaceY);

            yAdjustedCenter.y = riverY;
            yAdjustedLeft  .y = riverY;
            yAdjustedRight .y = riverY;

            if(RiverCanon.GetFlowOfRiverAtEdge(data.Center, data.Direction) == RiverFlow.Clockwise) {
                MeshBuilder.AddTriangleUnperturbed(
                    yAdjustedCenter, MeshBuilder.Weights1, new Vector2(1f,   HexMetrics.RiverEdgeStartV), 
                    yAdjustedLeft,   MeshBuilder.Weights2, new Vector2(0.5f, HexMetrics.RiverEdgeStartV - HexMetrics.RiverEndpointV),
                    yAdjustedRight,  MeshBuilder.Weights3, new Vector2(0f,   HexMetrics.RiverEdgeStartV),
                    indices, MeshBuilder.Rivers
                );
            }else {
                MeshBuilder.AddTriangleUnperturbed(
                    yAdjustedCenter, MeshBuilder.Weights1, new Vector2(0f,   HexMetrics.RiverEdgeEndV), 
                    yAdjustedLeft,   MeshBuilder.Weights2, new Vector2(0.5f, HexMetrics.RiverEdgeEndV + HexMetrics.RiverEndpointV),
                    yAdjustedRight,  MeshBuilder.Weights3, new Vector2(1f,   HexMetrics.RiverEdgeEndV),
                    indices, MeshBuilder.Rivers
                );
            }
        }

        #endregion

        #endregion

    }

}
