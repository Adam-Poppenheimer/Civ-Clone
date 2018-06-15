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
                mesh.AddTriangleUV(new Vector2(1f, 0f), new Vector2(0f, 0f), new Vector2(0.5f, 0.3f));
            }else {
                mesh.AddTriangleUV(new Vector2(1f, 0f), new Vector2(0f, 0f), new Vector2(0.5f, -0.3f));
            }
            
            if(RiverCanon.GetFlowOfRiverAtEdge(data.Center, data.Direction) == RiverFlow.Clockwise) {
                mesh.AddTriangleUV2(new Vector2(1f, 0f), new Vector2(0.5f, -0.3f), new Vector2(0f, 0f));
            }else {
                mesh.AddTriangleUV2(new Vector2(1f, 0f), new Vector2(0.5f, 0.3f), new Vector2(0f, 0f));
            }

            if(RiverCanon.GetFlowOfRiverAtEdge(data.Left, data.Direction.Next()) == RiverFlow.Clockwise) {
                mesh.AddTriangleUV3(new Vector2(0.5f, 0.3f), new Vector2(1f, 0f), new Vector2(0f, 0f));                
            }else {
                mesh.AddTriangleUV3(new Vector2(0.5f, -0.3f), new Vector2(1f, 0f), new Vector2(0f, 0f)); 
            }
        }

        //This method creates the river surface for edges. It extends these edges into any
        //applicable river corners so that UV interpolation lets the river flow properly.
        //Resolving the corners independently led to problems that I couldn't resolve.
        //In order to handle rivers of different elevation, this method averages the Y values
        //of nearby river edges. This can sometimes cause rivers to flow uphill, though
        //the effect is usually unnoticeable as long as elevation perturbation is small.
        public void CreateRiverSurface_EdgesAndCorners(
            CellTriangulationData thisData, CellTriangulationData nextData
        ){
            float upRiverSurfaceY, midRiverSurfaceY, downRiverSurfaceY;

            Vector3 upRiverLeftBank, upRiverRightBank, downRiverLeftBank, downRiverRightBank;

            float upRiverParam, downRiverParam;

            EdgeVertices leftBank = new EdgeVertices(
                thisData.Right.LocalPosition + HexMetrics.GetSecondOuterSolidCorner(thisData.Direction.Opposite()),
                thisData.Right.LocalPosition + HexMetrics.GetFirstOuterSolidCorner (thisData.Direction.Opposite())
            );

            EdgeVertices rightBank = new EdgeVertices(
                thisData.Center.LocalPosition + HexMetrics.GetFirstOuterSolidCorner (thisData.Direction),
                thisData.Center.LocalPosition + HexMetrics.GetSecondOuterSolidCorner(thisData.Direction)
            );

            CalculateRiverSurfaceEdgeProperties(
                thisData, false, out upRiverSurfaceY, out upRiverLeftBank, out upRiverRightBank,
                out upRiverParam
            );

            CalculateRiverSurfaceEdgeProperties(
                nextData, true,  out downRiverSurfaceY, out downRiverLeftBank, out downRiverRightBank,
                out downRiverParam
            );

            midRiverSurfaceY = Mathf.Min(thisData.Center.RiverSurfaceY, thisData.Right.RiverSurfaceY);

            leftBank.V1 = upRiverLeftBank;
            leftBank.V5 = downRiverLeftBank;

            rightBank.V1 = upRiverRightBank;
            rightBank.V5 = downRiverRightBank;

            leftBank.V1.y = upRiverSurfaceY;
            leftBank.V2.y = (upRiverSurfaceY + midRiverSurfaceY) / 2f;
            leftBank.V3.y = midRiverSurfaceY;
            leftBank.V4.y = (midRiverSurfaceY + downRiverSurfaceY) / 2f;
            leftBank.V5.y = downRiverSurfaceY;

            rightBank.V1.y = upRiverSurfaceY;
            rightBank.V2.y = (upRiverSurfaceY + midRiverSurfaceY) / 2f;
            rightBank.V3.y = midRiverSurfaceY;
            rightBank.V4.y = (midRiverSurfaceY + downRiverSurfaceY) / 2f;
            rightBank.V5.y = downRiverSurfaceY;

            bool isReversed = RiverCanon.GetFlowOfRiverAtEdge(thisData.Center, thisData.Direction) == RiverFlow.Counterclockwise;

            Vector3 indices = new Vector3(
                thisData.Center.Index, thisData.Center.Index, thisData.Center.Index
            );
            
            MeshBuilder.TriangulateRiverQuadUnperturbed(
                leftBank.V1, rightBank.V1,
                NoiseGenerator.Perturb(leftBank.V2), NoiseGenerator.Perturb(rightBank.V2),
                0f, 0.25f, isReversed, indices
            );

            MeshBuilder.TriangulateRiverQuadUnperturbed(
                NoiseGenerator.Perturb(leftBank.V2), NoiseGenerator.Perturb(rightBank.V2),
                NoiseGenerator.Perturb(leftBank.V3), NoiseGenerator.Perturb(rightBank.V3),
                0.25f, 0.5f, isReversed, indices
            );

            MeshBuilder.TriangulateRiverQuadUnperturbed(
                NoiseGenerator.Perturb(leftBank.V3), NoiseGenerator.Perturb(rightBank.V3),
                NoiseGenerator.Perturb(leftBank.V4), NoiseGenerator.Perturb(rightBank.V4),
                0.5f, 0.75f, isReversed, indices
            );

            MeshBuilder.TriangulateRiverQuadUnperturbed(
                NoiseGenerator.Perturb(leftBank.V4), NoiseGenerator.Perturb(rightBank.V4),
                leftBank.V5, rightBank.V5,
                0.75f, 1f, isReversed, indices
            );
        }

        public void CreateRiverSurface_Waterfall(CellTriangulationData data, float yAdjustment) {
            CreateRiverSurface_Waterfall(data, yAdjustment, yAdjustment, yAdjustment);
        }

        //Center and Left both have a higher elevation than Right, which means a
        //waterfall should flow from the river between Center and Left into the
        //corner, towards Right.
        public void CreateRiverSurface_Waterfall(
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

            //UV may need to be synchronized at some point. 0.2 is just there to make sure the waterfall flows.
            MeshBuilder.TriangulateRiverQuadUnperturbed(
                waterfallUpRiverLeft, waterfallUpRiverRight,
                waterfallDownRiverLeft, waterfallDownRiverRight,
                0f, 0.2f, isReversed, indices
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
                    yAdjustedCenter, MeshBuilder.Weights1, new Vector2(1f, 0f), 
                    yAdjustedLeft,   MeshBuilder.Weights2, new Vector2(0.5f, -HexMetrics.RiverEndpointVMax),
                    yAdjustedRight,  MeshBuilder.Weights3, Vector2.zero,
                    indices, MeshBuilder.Rivers
                );
            }else {
                MeshBuilder.AddTriangleUnperturbed(
                    yAdjustedCenter, MeshBuilder.Weights1, Vector2.zero, 
                    yAdjustedLeft,   MeshBuilder.Weights2, new Vector2(0.5f, HexMetrics.RiverEndpointVMax),
                    yAdjustedRight,  MeshBuilder.Weights3, new Vector2(1f, 0f),
                    indices, MeshBuilder.Rivers
                );
            }
        }

        #endregion

        //Determines what the endpoints of a particular river surface segment should look like.
        //Unlike other triangulations, river surface triangulation needs to be simultaneously
        //aware of a particular edge and both of its corners in order to function properly.
        //A large number of corner cases are handled by extending the edge's surface into the corners,
        //so that UVs wrap properly.
        //Confluences and endpoints are handled separately.
        private void CalculateRiverSurfaceEdgeProperties(
            CellTriangulationData data, bool targetingNextCorner,
            out float riverSurfaceY, out Vector3 riverLeftBank, out Vector3 riverRightBank,
            out float param
        ) {
            param = HexMetrics.RiverCurveOffsetDefault;

            if( RiverCanon.HasRiverAlongEdge(data.Center, data.Direction.Previous()) &&
                RiverCanon.HasRiverAlongEdge(data.Center, data.Direction)
            ) {

                if(RiverCanon.HasRiverAlongEdge(data.Right, data.Direction.Opposite().Next())) {

                    //Data represents a corner that's part of a river confluence.

                    //By default, the river Y should align itself with the Y of the confluence.
                    riverSurfaceY = Mathf.Min(
                        data.Center.RiverSurfaceY, data.Left.RiverSurfaceY, data.Right.RiverSurfaceY
                    );

                    //Waterfalls occur when two of the cells surrounding the confluence are at the same
                    //edge elevation and the third has a different elevation, so we need to check those
                    //to see if this is true and set the parameter to zero if it is. That'll tell the
                    //calling method to build a river right to the edge but not drop it down to the
                    //confluence triangle. We also need to make sure that the river surface isn't
                    //dropped down to the confluence's y coordinate.
                    if( data.Center.EdgeElevation == data.Left .EdgeElevation &&
                        data.Center.EdgeElevation != data.Right.EdgeElevation
                    ) {

                        param = 0f;
                        //If this is the next corner, then that means this edge is at the top of the waterfall
                        //between center and left and needs to be aligned to the Y that river would normally have
                        if(targetingNextCorner) {
                            riverSurfaceY = Mathf.Min(data.Center.RiverSurfaceY, data.Left .RiverSurfaceY);
                        }                        

                    }else if(
                        data.Center.EdgeElevation == data.Right.EdgeElevation &&
                        data.Center.EdgeElevation != data.Left .EdgeElevation
                    ) {

                        param = 0f;
                        
                        //If this is the current corner, then that means this edge is at the top of the waterfall
                        //between center and right and needs to be aligned to the Y that river would normally have
                        if(!targetingNextCorner) {
                            riverSurfaceY = Mathf.Min(data.Center.RiverSurfaceY, data.Right.RiverSurfaceY);
                        }                        

                    }else if(
                        data.Left.EdgeElevation == data.Right.EdgeElevation &&
                        data.Left.EdgeElevation != data.Center.EdgeElevation
                    ) {

                        param = 0f;

                        //If this is either corner, that means this edge is not part of the waterfall
                        //and needs to remain aligned with the confluence. Since that's the default
                        //behavior, we don't need to do anything here
                    }

                    if(targetingNextCorner) {
                        riverLeftBank  = data.PerturbedLeftCorner;
                        riverRightBank = data.PerturbedCenterCorner;
                    }else {
                        riverLeftBank  = data.PerturbedRightCorner;
                        riverRightBank = data.PerturbedCenterCorner;
                    }

                }else {
                    //Center is on the inside of a curve
                    riverSurfaceY = (
                        Mathf.Min(data.Center.RiverSurfaceY, data.Left .RiverSurfaceY) + 
                        Mathf.Min(data.Center.RiverSurfaceY, data.Right.RiverSurfaceY)
                    ) / 2f;

                    riverLeftBank  = (data.PerturbedLeftCorner + data.PerturbedRightCorner) / 2f;
                    riverRightBank = data.PerturbedCenterCorner;
                }

            }else if(
                data.Left != null && data.Right != null &&
                RiverCanon.HasRiverAlongEdge(data.Right, data.Direction.Opposite().Next())
            ) {

                if(!RiverCanon.HasRiverAlongEdge(data.Center, data.Direction)) {
                    //This case only occurs when invoked on the next corner of a valid river.
                    //There should be a river between Center and Left and Left and Right,
                    //but none between Center and Right.
                    riverSurfaceY = (
                        Mathf.Min(data.Left.RiverSurfaceY, data.Right.RiverSurfaceY) +
                        Mathf.Min(data.Left.RiverSurfaceY, data.Center.RiverSurfaceY)
                    ) / 2f;

                    riverLeftBank  = data.PerturbedLeftCorner;
                    riverRightBank = (data.PerturbedCenterCorner + data.PerturbedRightCorner) / 2f;

                }else {
                    //Center is on the outside of a curve, and the second segment is between
                    //Left and Right
                    riverSurfaceY = (
                        Math.Min(data.Center.RiverSurfaceY, data.Right.RiverSurfaceY) + 
                        Math.Min(data.Left  .RiverSurfaceY, data.Right.RiverSurfaceY)
                    ) / 2f;

                    riverLeftBank = data.PerturbedRightCorner;
                    riverRightBank = (data.PerturbedLeftCorner + data.PerturbedCenterCorner) / 2f;
                }

            }else if(!RiverCanon.HasRiverAlongEdge(data.Center, data.Direction)){
                //There's a river between Left and Center, which has an endpoint leading into Right
                riverSurfaceY = Math.Min(data.Center.RiverSurfaceY, data.Left.RiverSurfaceY);

                riverLeftBank  = data.PerturbedLeftCorner;
                riverRightBank = data.PerturbedCenterCorner;

            }else {                

                //There's a river between Right and Center, which has an endpoint leading into Left
                riverSurfaceY = Math.Min(data.Center.RiverSurfaceY, data.Right.RiverSurfaceY);

                riverLeftBank  = data.PerturbedRightCorner;
                riverRightBank = data.PerturbedCenterCorner;

            }
        }

        #endregion

    }

}
