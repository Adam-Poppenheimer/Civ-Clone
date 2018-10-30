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
        private IHexMapRenderConfig RenderConfig;

        #endregion

        #region constructors

        [Inject]
        public RiverSurfaceTriangulator(
            IHexGridMeshBuilder meshBuilder, INoiseGenerator noiseGenerator,
            IRiverCanon riverCanon, IHexMapRenderConfig renderConfig
        ) {
            MeshBuilder    = meshBuilder;
            NoiseGenerator = noiseGenerator;
            RiverCanon     = riverCanon;
            RenderConfig   = renderConfig;
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
                    new Vector2(RenderConfig.RiverStarboardU, RenderConfig.RiverEdgeEndV),
                    new Vector2(RenderConfig.RiverPortU,      RenderConfig.RiverEdgeEndV),
                    new Vector2(0.5f,                         RenderConfig.RiverEdgeEndV + RenderConfig.RiverConfluenceV)
                );
            }else {
                mesh.AddTriangleUV(
                    new Vector2(RenderConfig.RiverPortU,      RenderConfig.RiverEdgeStartV),
                    new Vector2(RenderConfig.RiverStarboardU, RenderConfig.RiverEdgeStartV),
                    new Vector2(0.5f,                         RenderConfig.RiverEdgeStartV - RenderConfig.RiverConfluenceV)
                );
            }
            
            if(RiverCanon.GetFlowOfRiverAtEdge(data.Center, data.Direction) == RiverFlow.Clockwise) {
                mesh.AddTriangleUV2(
                    new Vector2(RenderConfig.RiverStarboardU, RenderConfig.RiverEdgeStartV),
                    new Vector2(0.5f,                         RenderConfig.RiverEdgeStartV - RenderConfig.RiverConfluenceV),
                    new Vector2(RenderConfig.RiverPortU,      RenderConfig.RiverEdgeStartV)
                );
            }else {
                mesh.AddTriangleUV2(
                    new Vector2(RenderConfig.RiverPortU,      RenderConfig.RiverEdgeEndV),
                    new Vector2(0.5f,                         RenderConfig.RiverEdgeEndV + RenderConfig.RiverConfluenceV),
                    new Vector2(RenderConfig.RiverStarboardU, RenderConfig.RiverEdgeEndV)
                );
            }

            if(RiverCanon.GetFlowOfRiverAtEdge(data.Left, data.Direction.Next()) == RiverFlow.Clockwise) {
                mesh.AddTriangleUV3(
                    new Vector2(0.5f,                         RenderConfig.RiverEdgeEndV + RenderConfig.RiverConfluenceV),
                    new Vector2(RenderConfig.RiverStarboardU, RenderConfig.RiverEdgeEndV),
                    new Vector2(RenderConfig.RiverPortU,      RenderConfig.RiverEdgeEndV)
                );                
            }else {
                mesh.AddTriangleUV3(
                    new Vector2(0.5f,                         RenderConfig.RiverEdgeStartV - RenderConfig.RiverConfluenceV),
                    new Vector2(RenderConfig.RiverPortU,      RenderConfig.RiverEdgeStartV),
                    new Vector2(RenderConfig.RiverStarboardU, RenderConfig.RiverEdgeStartV)
                );
            }
        }

        public void CreateRiverSurface_StraightEdge(CellTriangulationData thisData) {
            float riverSurfaceY = Mathf.Min(thisData.Center.RiverSurfaceY, thisData.Right.RiverSurfaceY);

            Vector3 leftOne   = NoiseGenerator.Perturb(thisData.RightToCenterEdge.V1);
            Vector3 leftTwo   = NoiseGenerator.Perturb(thisData.RightToCenterEdge.V2);
            Vector3 leftThree = NoiseGenerator.Perturb(thisData.RightToCenterEdge.V3);
            Vector3 leftFour  = NoiseGenerator.Perturb(thisData.RightToCenterEdge.V4);
            Vector3 leftFive  = NoiseGenerator.Perturb(thisData.RightToCenterEdge.V5);

            Vector3 rightOne   = NoiseGenerator.Perturb(thisData.CenterToRightEdge.V1);
            Vector3 rightTwo   = NoiseGenerator.Perturb(thisData.CenterToRightEdge.V2);
            Vector3 rightThree = NoiseGenerator.Perturb(thisData.CenterToRightEdge.V3);
            Vector3 rightFour  = NoiseGenerator.Perturb(thisData.CenterToRightEdge.V4);
            Vector3 rightFive  = NoiseGenerator.Perturb(thisData.CenterToRightEdge.V5);

            leftOne .y = leftTwo .y = leftThree .y = leftFour .y = leftFive .y = riverSurfaceY;
            rightOne.y = rightTwo.y = rightThree.y = rightFour.y = rightFive.y = riverSurfaceY;

            bool isReversed = RiverCanon.GetFlowOfRiverAtEdge(thisData.Center, thisData.Direction) == RiverFlow.Counterclockwise;

            Vector3 indices = new Vector3(
                thisData.Center.Index, thisData.Left.Index, thisData.Right.Index
            );

            MeshBuilder.TriangulateRiverQuadUnperturbed(
                leftOne, rightOne, leftTwo, rightTwo,
                RenderConfig.GetRiverEdgeV(0), RenderConfig.GetRiverEdgeV(1),
                isReversed, indices
            );

            MeshBuilder.TriangulateRiverQuadUnperturbed(
                leftTwo, rightTwo, leftThree, rightThree,
                RenderConfig.GetRiverEdgeV(1), RenderConfig.GetRiverEdgeV(2),
                isReversed, indices
            );

            MeshBuilder.TriangulateRiverQuadUnperturbed(
                leftThree, rightThree, leftFour, rightFour,
                RenderConfig.GetRiverEdgeV(2), RenderConfig.GetRiverEdgeV(3),
                isReversed, indices
            );

            MeshBuilder.TriangulateRiverQuadUnperturbed(
                leftFour, rightFour, leftFive, rightFive,
                RenderConfig.GetRiverEdgeV(3), RenderConfig.GetRiverEdgeV(4),
                isReversed, indices
            );
        }

        /*
         * This method tries to draw a curve triangle with Center on its inside point
         * and Left and Right making up its outer edge. It does this by performing 
         * rotational operations much like corner triangulation in the terrain mesh.
         * 
         * We use UV2 and UV3 to pass in barycentric coordinates to the shader.
         * The shader uses those to determine its UV, since river corners require
         * a somewhat complex UV animation. This is not considered a great solution
         * to the problem.
         * 
         * the current implementation also includes waterfalls. The organization
         * for waterfall creation is unpleasant to say the least, but waterfalls are
         * probably going to be removed from the final product, so it's not clear
         * that a refactor for comprehension is useful.
         */
        public void CreateRiverSurface_CurveCorner(CellTriangulationData data) {
            float riverSurfaceY = Mathf.Min(data.Center.RiverSurfaceY, data.Left.RiverSurfaceY, data.Right.RiverSurfaceY);

            bool isReversed = false;
            bool waterfallHasOppositeReversal = false;

            Vector3 innerCorner  = Vector3.zero, outerLeftCorner  = Vector3.zero, outerRightCorner  = Vector3.zero;
            Color   innerWeights = Color.black,  outerLeftWeights = Color.black,  outerRightWeights = Color.black;

            bool buildCorner = false;
            bool buildWaterfall = false;

            Vector3 waterfallUpRiverLeft   = Vector3.zero, waterfallUpRiverRight   = Vector3.zero;
            Vector3 waterfallDownRiverLeft = Vector3.zero, waterfallDownRiverRight = Vector3.zero;

            //If Center is already on the inside of the corner
            if(data.CenterToRightEdgeType == HexEdgeType.River) {
                if(data.CenterToLeftEdgeType == HexEdgeType.River) {
                    buildCorner = true;

                    innerCorner      = data.PerturbedCenterCorner;
                    outerLeftCorner  = data.PerturbedLeftCorner;
                    outerRightCorner = data.PerturbedRightCorner;

                    innerWeights      = MeshBuilder.Weights1;
                    outerLeftWeights  = MeshBuilder.Weights2;
                    outerRightWeights = MeshBuilder.Weights3;

                    innerCorner.y = outerLeftCorner.y = outerRightCorner.y = riverSurfaceY;

                    isReversed = RiverCanon.GetFlowOfRiverAtEdge(data.Center, data.Direction) == RiverFlow.Counterclockwise;

                    if(data.Center.EdgeElevation > data.Right.EdgeElevation && data.Left.EdgeElevation > data.Right.EdgeElevation) {
                        buildWaterfall = true;

                        waterfallUpRiverLeft    = data.PerturbedLeftCorner;
                        waterfallUpRiverRight   = data.PerturbedCenterCorner;
                        waterfallDownRiverLeft  = outerLeftCorner;
                        waterfallDownRiverRight = innerCorner;

                        waterfallUpRiverLeft.y = waterfallUpRiverRight.y = Mathf.Min(data.Center.RiverSurfaceY, data.Left.RiverSurfaceY);

                    }else if(data.Center.EdgeElevation > data.Left.EdgeElevation && data.Right.EdgeElevation > data.Left.EdgeElevation) {
                        buildWaterfall = true;
                        waterfallHasOppositeReversal = true;

                        waterfallUpRiverLeft    = data.PerturbedCenterCorner;
                        waterfallUpRiverRight   = data.PerturbedRightCorner;
                        waterfallDownRiverLeft  = innerCorner;
                        waterfallDownRiverRight = outerRightCorner;

                        waterfallUpRiverLeft.y = waterfallUpRiverRight.y = Mathf.Min(data.Center.RiverSurfaceY, data.Right.RiverSurfaceY);
                    }

                //If right is on the inside of the corner
                }else if(data.LeftToRightEdgeType == HexEdgeType.River) {
                    buildCorner = true;

                    innerCorner      = data.PerturbedRightCorner;
                    outerLeftCorner  = data.PerturbedCenterCorner;
                    outerRightCorner = data.PerturbedLeftCorner;

                    innerWeights      = MeshBuilder.Weights3;
                    outerLeftWeights  = MeshBuilder.Weights1;
                    outerRightWeights = MeshBuilder.Weights2;

                    innerCorner.y = outerLeftCorner.y = outerRightCorner.y = riverSurfaceY;

                    isReversed = RiverCanon.GetFlowOfRiverAtEdge(data.Right, data.Direction.Previous2()) == RiverFlow.Counterclockwise;

                    if(data.Center.EdgeElevation > data.Left.EdgeElevation && data.Right.EdgeElevation > data.Left.EdgeElevation) {
                        buildWaterfall = true;

                        waterfallUpRiverLeft    = data.PerturbedCenterCorner;
                        waterfallUpRiverRight   = data.PerturbedRightCorner;
                        waterfallDownRiverLeft  = outerLeftCorner;
                        waterfallDownRiverRight = innerCorner;

                        waterfallUpRiverLeft.y = waterfallUpRiverRight.y = Mathf.Min(data.Center.RiverSurfaceY, data.Right.RiverSurfaceY);

                    }else if(data.Center.EdgeElevation < data.Right.EdgeElevation && data.Center.EdgeElevation < data.Left.EdgeElevation) {                        
                        buildWaterfall = true;
                        waterfallHasOppositeReversal = true;

                        waterfallUpRiverLeft    = data.PerturbedRightCorner;
                        waterfallUpRiverRight   = data.PerturbedLeftCorner;
                        waterfallDownRiverLeft  = innerCorner;
                        waterfallDownRiverRight = outerRightCorner;

                        waterfallUpRiverLeft.y = waterfallUpRiverRight.y = Mathf.Min(data.Left.RiverSurfaceY, data.Right.RiverSurfaceY);
                    }
                }

            //If left is on the inside of the corner
            }else if(data.CenterToLeftEdgeType == HexEdgeType.River && data.LeftToRightEdgeType == HexEdgeType.River) {
                buildCorner = true;

                innerCorner      = data.PerturbedLeftCorner;
                outerLeftCorner  = data.PerturbedRightCorner;
                outerRightCorner = data.PerturbedCenterCorner;

                innerWeights      = MeshBuilder.Weights2;
                outerLeftWeights  = MeshBuilder.Weights3;
                outerRightWeights = MeshBuilder.Weights1;

                innerCorner.y = outerLeftCorner.y = outerRightCorner.y = riverSurfaceY;

                isReversed = RiverCanon.GetFlowOfRiverAtEdge(data.Left, data.Direction.Next()) == RiverFlow.Counterclockwise;

                if(data.Center.EdgeElevation > data.Right.EdgeElevation && data.Left.EdgeElevation > data.Right.EdgeElevation) {
                    buildWaterfall = true;
                    waterfallHasOppositeReversal = true;

                    waterfallUpRiverLeft    = data.PerturbedLeftCorner;
                    waterfallUpRiverRight   = data.PerturbedCenterCorner;
                    waterfallDownRiverLeft  = innerCorner;
                    waterfallDownRiverRight = outerRightCorner;

                    waterfallUpRiverLeft.y = waterfallUpRiverRight.y = Mathf.Min(data.Center.RiverSurfaceY, data.Left.RiverSurfaceY);

                }else if(data.Center.EdgeElevation < data.Left.EdgeElevation && data.Center.EdgeElevation < data.Right.EdgeElevation) {
                    buildWaterfall = true;

                    waterfallUpRiverLeft    = data.PerturbedRightCorner;
                    waterfallUpRiverRight   = data.PerturbedLeftCorner;
                    waterfallDownRiverLeft  = outerLeftCorner;
                    waterfallDownRiverRight = innerCorner;

                    waterfallUpRiverLeft.y = waterfallUpRiverRight.y = Mathf.Min(data.Left.RiverSurfaceY, data.Right.RiverSurfaceY);
                }
            }

            if(!buildCorner) {
                return;
            }

            MeshBuilder.AddTriangleUnperturbed(
                innerCorner,      data.Center.Index, innerWeights,
                outerLeftCorner,  data.Left  .Index, outerLeftWeights,
                outerRightCorner, data.Right .Index, outerRightWeights,
                MeshBuilder.RiverCorners 
            );

            //The inner point of the curve is always at barycentric coordinate (1, 0, 0).
            //Upriver is always at (0, 1, 0) and downriver at (0, 0, 1). Because of this,
            //flow direction affects how we assign the barycentric coordinates.
            if(isReversed) {
                MeshBuilder.RiverCorners.AddTriangleUV(new Vector2(1f, 1f), new Vector2(1f, 1f), new Vector2(1f, 1f));

                MeshBuilder.RiverCorners.AddTriangleUV2(new Vector2(1f, 0f), Vector2.zero, new Vector2(0f, 1f));
                MeshBuilder.RiverCorners.AddTriangleUV3(Vector2.zero, new Vector2(1f, 0f), Vector2.zero);
            }else {
                MeshBuilder.RiverCorners.AddTriangleUV(new Vector2(0f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f));

                MeshBuilder.RiverCorners.AddTriangleUV2(new Vector2(1f, 0f), new Vector2(0f, 1f), Vector2.zero);
                MeshBuilder.RiverCorners.AddTriangleUV3(Vector2.zero, Vector2.zero, new Vector2(1f, 0f));
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

            Vector3 waterfallVectorLeft  = data.LeftRightTroughPoint   - waterfallUpRiverLeft;
            Vector3 waterfallVectorRight = data.CenterRightTroughPoint - waterfallUpRiverRight;

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
                RenderConfig.RiverEdgeEndV, RenderConfig.RiverEdgeEndV + RenderConfig.RiverWaterfallV,
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
                RenderConfig.RiverEdgeEndV, RenderConfig.RiverEdgeEndV + RenderConfig.EstuaryWaterfallV,
                isReversed, indices
            );
        }

        //Creates the water surface for a river endpoint. The river lies between
        //Center and Right and is facing Left.
        public void CreateRiverEndpointSurface_Default(CellTriangulationData data) {
            if(data.Left.Terrain.IsWater()) {
                return;
            }

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
                    yAdjustedCenter, MeshBuilder.Weights1, new Vector2(1f,   RenderConfig.RiverEdgeStartV), 
                    yAdjustedLeft,   MeshBuilder.Weights2, new Vector2(0.5f, RenderConfig.RiverEdgeStartV - RenderConfig.RiverEndpointV),
                    yAdjustedRight,  MeshBuilder.Weights3, new Vector2(0f,   RenderConfig.RiverEdgeStartV),
                    indices, MeshBuilder.Rivers
                );
            }else {
                MeshBuilder.AddTriangleUnperturbed(
                    yAdjustedCenter, MeshBuilder.Weights1, new Vector2(0f,   RenderConfig.RiverEdgeEndV), 
                    yAdjustedLeft,   MeshBuilder.Weights2, new Vector2(0.5f, RenderConfig.RiverEdgeEndV + RenderConfig.RiverEndpointV),
                    yAdjustedRight,  MeshBuilder.Weights3, new Vector2(1f,   RenderConfig.RiverEdgeEndV),
                    indices, MeshBuilder.Rivers
                );
            }
        }

        #endregion

        #endregion

    }

}
