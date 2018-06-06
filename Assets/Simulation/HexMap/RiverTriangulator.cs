using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Util;

using UnityCustomUtilities.Extensions;

namespace Assets.Simulation.HexMap {

    public class RiverTriangulator : IRiverTriangulator {

        #region instance fields and properties

        private IHexGrid            Grid;
        private IHexGridMeshBuilder MeshBuilder;
        private IRiverCanon         RiverCanon;
        private INoiseGenerator     NoiseGenerator;

        #endregion

        #region constructors

        [Inject]
        public RiverTriangulator(
            IHexGrid grid, IHexGridMeshBuilder meshBuilder,
            IRiverCanon riverCanon, INoiseGenerator noiseGenerator
        ){
            Grid           = grid;
            MeshBuilder    = meshBuilder;
            RiverCanon     = riverCanon;
            NoiseGenerator = noiseGenerator;
        }

        #endregion

        #region instance methods

        #region from IRiverTriangulator

        public bool ShouldTriangulateRiverConnection(CellTriangulationData thisData) {
            return thisData.Direction <= HexDirection.SE && thisData.IsRiverCorner;
        }

        public void TriangulateRiverConnection(CellTriangulationData thisData) {          
            IHexCell nextNeighbor = Grid.GetNeighbor(thisData.Center, thisData.Direction.Next());

            var nextData = MeshBuilder.GetTriangulationData(
                thisData.Center, thisData.Right, nextNeighbor, thisData.Direction.Next()
            );            

            if(thisData.Direction > HexDirection.SE) {
                return;
            }

            //Creates river edge troughs and surfaces in the direction if needed
            if(thisData.CenterToRightEdgeType == HexEdgeType.River) {
                CreateRiverTrough_Edge(thisData);
                CreateRiverSurface_EdgesAndCorners(thisData, nextData);
            }

            //Creates river corner troughs and surfaces at the previous corner if needed
            if(thisData.Direction <= HexDirection.SE && thisData.IsRiverCorner) {
                CreateRiverTrough_Corner(thisData);
            }
        }

        private void CreateRiverTrough_Corner(CellTriangulationData data) {
            if(data.AllEdgesHaveRivers) {
                //There is a confluence at the corner
                CreateRiverTrough_Confluence (data);
                CreateRiverSurface_Confluence(data);

            }else if(data.TwoEdgesHaveRivers) {
                if(data.CenterToLeftEdgeType != HexEdgeType.River) {
                    //There is a river corner and Right is on its inside edge. Since
                    //CreateRiverTrough_Curve draws relative to the inside of the curve,
                    //We rotate our river data so that Right is in the center, Center
                    //is on the left, and Left is on the right
                    CreateRiverTrough_Curve(MeshBuilder.GetTriangulationData(
                        data.Right, data.Center, data.Left, data.Direction.Opposite().Next()
                    ));
                }else if(data.CenterToRightEdgeType != HexEdgeType.River) {
                    //There is a river corner and Left is on its inside edge. We must
                    //rotate so that Left is in the center, Right is on the left, and
                    //Center is on the right
                    CreateRiverTrough_Curve(MeshBuilder.GetTriangulationData(
                        data.Left, data.Right, data.Center, data.Direction.Next2()
                    ));
                }else {
                    //There is a river corner and Center is on its inside edge
                    CreateRiverTrough_Curve(data);
                }

            //We need to rotate endpoint cases to make sure that the endpoint is always
            //pointing towards Left
            }else if(data.CenterToLeftEdgeType == HexEdgeType.River) {
                //There is an endpoint pointing towards Right
                CreateRiverTrough_Endpoint(MeshBuilder.GetTriangulationData(
                    data.Left, data.Right, data.Center, data.Direction.Next2()
                ));

            }else if(data.CenterToRightEdgeType == HexEdgeType.River) {
                //There is an endpoint pointing towards Left
                CreateRiverTrough_Endpoint(data);

            }else if(data.LeftToRightEdgeType == HexEdgeType.River) {
                //There is an endpoint pointing towards Center
                CreateRiverTrough_Endpoint(MeshBuilder.GetTriangulationData(
                    data.Right, data.Center, data.Left, data.Direction.Opposite().Next()
                ));
            }
        }

        #endregion

        private void CreateRiverTrough_Edge(CellTriangulationData data){
            MeshBuilder.TriangulateEdgeStrip(
                data.CenterToRightEdge, MeshBuilder.Weights1,  data.Center.Index, data.Center.RequiresYPerturb,
                data.CenterRightTrough, MeshBuilder.Weights12, data.Right.Index,  false,
                MeshBuilder.Terrain
            );

            MeshBuilder.TriangulateEdgeStrip(
                data.CenterRightTrough, MeshBuilder.Weights12, data.Center.Index, false,
                data.RightToCenterEdge, MeshBuilder.Weights2,  data.Right .Index, data.Right.RequiresYPerturb,
                MeshBuilder.Terrain
            );
        }

        //This method creates the river surface for edges. It extends these edges into any
        //applicable river corners so that UV interpolation lets the river flow properly.
        //Resolving the corners independently led to problems that I couldn't resolve.
        //In order to handle rivers of different elevation, this method averages the Y values
        //of nearby river edges. This can sometimes cause rivers to flow uphill, though
        //the effect is usually unnoticeable as long as elevation perturbation is small.
        private void CreateRiverSurface_EdgesAndCorners(CellTriangulationData thisData, CellTriangulationData nextData){
            float upRiverSurfaceY, midRiverSurfaceY, downRiverSurfaceY;

            Vector3 upRiverLeftBank, upRiverRightBank, downRiverLeftBank, downRiverRightBank;

            float upRiverParam, downRiverParam;

            EdgeVertices leftBank = new EdgeVertices(
                thisData.Right.transform.localPosition + HexMetrics.GetSecondOuterSolidCorner(thisData.Direction.Opposite()),
                thisData.Right.transform.localPosition + HexMetrics.GetFirstOuterSolidCorner (thisData.Direction.Opposite())
            );

            EdgeVertices rightBank = new EdgeVertices(
                thisData.Center.transform.localPosition + HexMetrics.GetFirstOuterSolidCorner (thisData.Direction),
                thisData.Center.transform.localPosition + HexMetrics.GetSecondOuterSolidCorner(thisData.Direction)
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

        //This method currently places a static water triangle at the confluence as a first approximation.
        //Managing the UV for proper river flow is quite complex and is being deferred to a later date.
        private void CreateRiverSurface_Confluence(CellTriangulationData data) {
            float confluenceY = Mathf.Min(
                data.Center.RiverSurfaceY, data.Left.RiverSurfaceY, data.Right.RiverSurfaceY
            );

            Vector3 yAdjustedCenter = data.PerturbedCenterCorner,
                    yAdjustedLeft   = data.PerturbedLeftCorner,
                    yAdjustedRight  = data.PerturbedRightCorner;

            yAdjustedCenter.y = confluenceY;
            yAdjustedLeft  .y = confluenceY;
            yAdjustedRight .y = confluenceY;

            //Every confluence consists of some triangle of still water where the rivers
            //converge/diverge.
            MeshBuilder.AddTriangleUnperturbed(
                yAdjustedCenter, data.Center.Index, MeshBuilder.Weights1,
                yAdjustedLeft,   data.Left  .Index, MeshBuilder.Weights2,
                yAdjustedRight,  data.Right .Index, MeshBuilder.Weights3,
                MeshBuilder.Water
            );

            //We need to handle waterfalls into confluences as a special case. Edge and corner
            //surface generation should account for this by not building its river edge to the
            //confluence's water triangle
            if( data.Center.EdgeElevation > data.Right.EdgeElevation &&
                data.Left  .EdgeElevation > data.Right.EdgeElevation
            ) {
                //There's a waterfall flowing from CenterLeft river into the confluence
                CreateRiverSurface_Waterfall(data, yAdjustedCenter, yAdjustedLeft, yAdjustedRight);
            }else if(
                data.Center.EdgeElevation > data.Left.EdgeElevation &&
                data.Right .EdgeElevation > data.Left.EdgeElevation
            ) {
                //There's a waterfall flowing from CenterRight river into the confluence.
                //We need to rotate our data to make sure that Right is the new center and
                //Left is the new right
                CreateRiverSurface_Waterfall(
                    MeshBuilder.GetTriangulationData(
                        data.Right, data.Center, data.Left, data.Direction.Previous2()
                    ),
                    yAdjustedRight, yAdjustedCenter, yAdjustedLeft
                );
            }else if(
                data.Left .EdgeElevation > data.Center.EdgeElevation &&
                data.Right.EdgeElevation > data.Center.EdgeElevation
            ) {
                //There's a waterwall flowing from LeftRight river into the confluence.
                //we need to rotate our data to make sure that Center is the new Right
                //and Left is the new center
                CreateRiverSurface_Waterfall(
                    MeshBuilder.GetTriangulationData(
                        data.Left, data.Right, data.Center, data.Direction.Next2()
                    ),
                    yAdjustedLeft, yAdjustedRight, yAdjustedCenter
                );
            }
        }

        //Center and Left both have a higher elevation than Right, which means a
        //waterfall should flow from the river between Center and Left into the
        //corner, towards Right.
        private void CreateRiverSurface_Waterfall(
            CellTriangulationData data, Vector3 yAdjustedCenter, Vector3 yAdjustedLeft, Vector3 yAdjustedRight
        ){
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

        //Draws four triangles connecting the three inward-facing solid corners of the triangles and the
        //troughs of the rivers on all three edges
        private void CreateRiverTrough_Confluence(CellTriangulationData data){
            //Triangle pointing at center, which LeftRight river flows towards
            MeshBuilder.AddTriangleUnperturbed(
                NoiseGenerator.Perturb(data.CenterCorner,           data.Center.RequiresYPerturb), data.Center.Index, MeshBuilder.Weights1,
                NoiseGenerator.Perturb(data.CenterLeftTroughPoint,  false),                        data.Left  .Index, MeshBuilder.Weights12,
                NoiseGenerator.Perturb(data.CenterRightTroughPoint, false),                        data.Right .Index, MeshBuilder.Weights13,
                MeshBuilder.Terrain
            );

            //Triangle pointing at left, which CenterRight river flows towards
            MeshBuilder.AddTriangleUnperturbed(
                NoiseGenerator.Perturb(data.LeftCorner,            data.Left.RequiresYPerturb), data.Center.Index, MeshBuilder.Weights2,
                NoiseGenerator.Perturb(data.LeftRightTroughPoint,  false),                      data.Left  .Index, MeshBuilder.Weights23,
                NoiseGenerator.Perturb(data.CenterLeftTroughPoint, false),                      data.Right .Index, MeshBuilder.Weights12,
                MeshBuilder.Terrain
            );

            //Triangle pointing at right, which CenterLeft river flows towards
            MeshBuilder.AddTriangleUnperturbed(
                NoiseGenerator.Perturb(data.RightCorner,            data.Right.RequiresYPerturb), data.Center.Index, MeshBuilder.Weights3,
                NoiseGenerator.Perturb(data.CenterRightTroughPoint, false),                       data.Left  .Index, MeshBuilder.Weights13,
                NoiseGenerator.Perturb(data.LeftRightTroughPoint,   false),                       data.Right .Index, MeshBuilder.Weights23,
                MeshBuilder.Terrain
            );

            //Triangle in the middle, touching each of the trough points and which all rivers flow towards
            MeshBuilder.AddTriangleUnperturbed(
                data.PerturbedCenterLeftTroughPoint,  data.Center.Index, MeshBuilder.Weights12,
                data.PertrubedLeftRightTroughPoint,   data.Left  .Index, MeshBuilder.Weights23,
                data.PerturbedCenterRightTroughPoint, data.Right .Index, MeshBuilder.Weights13,
                MeshBuilder.Terrain
            );
        }

        //Non-rivered edge is between right and left. Center is on the inside of the curve
        private void CreateRiverTrough_Curve(CellTriangulationData data){
            //The inside of the curve is always a single triangle, since the two
            //riverine edges prevent anything tricky from happening
            MeshBuilder.AddTriangleUnperturbed(
                data.PerturbedCenterCorner,           data.Center.Index, MeshBuilder.Weights1,
                data.PerturbedCenterLeftTroughPoint,  data.Left.Index,   MeshBuilder.Weights12,
                data.PerturbedCenterRightTroughPoint, data.Right.Index,  MeshBuilder.Weights13,
                MeshBuilder.Terrain
            );

            //We've already taken care of River and Void edge types, so we don't need to check them again here
            if(data.LeftToRightEdgeType == HexEdgeType.Flat) {
                CreateRiverTrough_OuterCurveFlat(data);
            }else if(data.LeftToRightEdgeType == HexEdgeType.Slope) {
                if(data.Left.EdgeElevation < data.Right.EdgeElevation) {
                    CreateRiverOuterCurveSloped_TerracesClockwiseUp(data);
                }else {
                    CreateRiverOuterCurveSloped_TerracesClockwiseDown(data);
                }                
            }
        }

        //Non-rivered edge is between right and left. Outer river bank is opposite center
        private void CreateRiverTrough_OuterCurveFlat(CellTriangulationData data){
            MeshBuilder.AddTriangleUnperturbed(
                data.PerturbedLeftCorner,    data.Left  .Index, MeshBuilder.Weights1,
                data.PerturbedRightCorner,   data.Right .Index, MeshBuilder.Weights2,
                data.PerturbedCenterLeftTroughPoint, data.Center.Index, MeshBuilder.Weights13,
                MeshBuilder.Terrain
            );
            MeshBuilder.AddTriangleUnperturbed(
                data.PerturbedCenterLeftTroughPoint,  data.Left  .Index, MeshBuilder.Weights13,
                data.PerturbedRightCorner,    data.Right .Index, MeshBuilder.Weights2,
                data.PerturbedCenterRightTroughPoint, data.Center.Index, MeshBuilder.Weights23,
                MeshBuilder.Terrain
            );
        }

        //Non-rivered edge is between right and left. Outer river bank is opposite center
        private void CreateRiverOuterCurveSloped_TerracesClockwiseUp(CellTriangulationData data) {
            //We need to converge the terraces to the convergence point, which is set at some point
            //between the right trough and the right point
            var convergencePoint = Vector3.Lerp(
                data.PerturbedCenterRightTroughPoint, data.PerturbedRightCorner, HexMetrics.RiverSlopedCurveLerp
            );

            var convergenceWeights = Color.Lerp(
                MeshBuilder.Weights13, MeshBuilder.Weights3, HexMetrics.RiverSlopedCurveLerp
            );

            //Quad between leftPoint, convergencePoint, rightTroughPoint, leftTroughPoint,
            //which happens below the terrace convergence
            MeshBuilder.AddTriangleUnperturbed(
                data.PerturbedCenterLeftTroughPoint,  data.Center.Index, MeshBuilder.Weights12,
                data.PerturbedLeftCorner,             data.Left  .Index, MeshBuilder.Weights2,
                data.PerturbedCenterRightTroughPoint, data.Right .Index, MeshBuilder.Weights13,
                MeshBuilder.Terrain
            );

            MeshBuilder.AddTriangleUnperturbed(
                data.PerturbedCenterRightTroughPoint, data.Center.Index, MeshBuilder.Weights13,
                data.PerturbedLeftCorner,             data.Left  .Index, MeshBuilder.Weights2,
                convergencePoint,                     data.Right .Index, convergenceWeights,
                MeshBuilder.Terrain
            );

            //Then we need to create the corner terraces, which follows the same basic pattern
            //as it does in the rest of the codebase
            Vector3 stepVertex2 = NoiseGenerator.Perturb(
                HexMetrics.TerraceLerp(data.LeftCorner, data.RightCorner, 1)
            );

            Color stepWeights2 = HexMetrics.TerraceLerp(MeshBuilder.Weights2, MeshBuilder.Weights3, 1);

            MeshBuilder.AddTriangleUnperturbed(
                data.PerturbedLeftCorner, data.Center.Index, MeshBuilder.Weights2,
                stepVertex2,              data.Left  .Index, stepWeights2,
                convergencePoint,         data.Right .Index, convergenceWeights,
                MeshBuilder.Terrain
            );

            for(int i = 2; i < HexMetrics.TerraceSteps; i++) {
                Vector3 stepVertex1 = stepVertex2;
                Color stepWeights1  = stepWeights2;

                stepVertex2 = NoiseGenerator.Perturb(
                    HexMetrics.TerraceLerp(data.LeftCorner, data.RightCorner, i)
                );

                stepWeights2 = HexMetrics.TerraceLerp(MeshBuilder.Weights2, MeshBuilder.Weights3, i);

                MeshBuilder.AddTriangleUnperturbed(
                    stepVertex1,       data.Center.Index, stepWeights1,
                    stepVertex2,       data.Left  .Index, stepWeights2,
                    convergencePoint,  data.Right .Index, convergenceWeights,
                    MeshBuilder.Terrain
                );
            }

            MeshBuilder.AddTriangleUnperturbed(
                stepVertex2,               data.Center.Index, stepWeights2,
                data.PerturbedRightCorner, data.Left  .Index, MeshBuilder.Weights3,
                convergencePoint,          data.Right .Index, convergenceWeights,
                MeshBuilder.Terrain
            );
        }

        //Non-rivered edge is between right and left. Outer river bank is opposite center
        private void CreateRiverOuterCurveSloped_TerracesClockwiseDown(CellTriangulationData data) {
            //We need to converge the terraces to the convergence point, which is set at some point
            //between the left trough and the left point
            var convergencePoint = Vector3.Lerp(
                data.PerturbedCenterLeftTroughPoint, data.PerturbedLeftCorner, HexMetrics.RiverSlopedCurveLerp
            );

            var convergenceWeights = Color.Lerp(
                MeshBuilder.Weights13, MeshBuilder.Weights3, HexMetrics.RiverSlopedCurveLerp
            );

            //Quad between rightPoint, convergencePoint, rightTroughPoint, leftTroughPoint,
            //which happens below the terrace convergence
            MeshBuilder.AddTriangleUnperturbed(
                data.PerturbedCenterLeftTroughPoint,  data.Center.Index, MeshBuilder.Weights13,
                data.PerturbedRightCorner,            data.Right .Index, MeshBuilder.Weights2,
                data.PerturbedCenterRightTroughPoint, data.Left  .Index, MeshBuilder.Weights12,
                MeshBuilder.Terrain
            );

            MeshBuilder.AddTriangleUnperturbed(
                data.PerturbedCenterLeftTroughPoint, data.Center.Index, MeshBuilder.Weights13,
                convergencePoint,                    data.Right .Index, convergenceWeights,
                data.PerturbedRightCorner,           data.Left  .Index, MeshBuilder.Weights2,
                MeshBuilder.Terrain
            );

            Vector3 stepVertex2 = NoiseGenerator.Perturb(
                HexMetrics.TerraceLerp(data.RightCorner, data.LeftCorner, 1)
            );

            Color stepWeights2 = HexMetrics.TerraceLerp(MeshBuilder.Weights2, MeshBuilder.Weights3, 1);

            MeshBuilder.AddTriangleUnperturbed(
                convergencePoint,          data.Center.Index, convergenceWeights,
                stepVertex2,               data.Right .Index, stepWeights2,
                data.PerturbedRightCorner, data.Left  .Index, MeshBuilder.Weights2,
                MeshBuilder.Terrain
            );

            for(int i = 2; i < HexMetrics.TerraceSteps; i++) {
                Vector3 stepVertex1 = stepVertex2;
                Color stepWeights1  = stepWeights2;

                stepVertex2 = NoiseGenerator.Perturb(
                    HexMetrics.TerraceLerp(data.RightCorner, data.LeftCorner, i)
                );

                stepWeights2 = HexMetrics.TerraceLerp(MeshBuilder.Weights2, MeshBuilder.Weights3, i);

                MeshBuilder.AddTriangleUnperturbed(
                    convergencePoint, data.Center.Index, convergenceWeights,
                    stepVertex2,      data.Right .Index, stepWeights2,
                    stepVertex1,      data.Left  .Index, stepWeights1,
                    MeshBuilder.Terrain
                );
            }

            MeshBuilder.AddTriangleUnperturbed(
                convergencePoint,         data.Center.Index, convergenceWeights,
                data.PerturbedLeftCorner, data.Right .Index, MeshBuilder.Weights3,
                stepVertex2,              data.Left  .Index, stepWeights2,
                MeshBuilder.Terrain
            );
        }

        //Rivered edge is between Center and Right. River endpoint is pointing at Left
        private void CreateRiverTrough_Endpoint(CellTriangulationData data) {
            if(data.Left == null) {
                return;
            }

            var centerLeftEdgeType = HexMetrics.GetEdgeType(data.Center, data.Left);
            var leftRightEdgeType  = HexMetrics.GetEdgeType(data.Left,   data.Right);

            if(centerLeftEdgeType == HexEdgeType.Flat) {
                if(leftRightEdgeType == HexEdgeType.Flat) {
                    CreateRiverEndpointTrough_DoubleFlat(data);
                }else if(leftRightEdgeType == HexEdgeType.Slope) {
                    CreateRiverEndpointTrough_FlatTerraces(data);
                }else if(leftRightEdgeType == HexEdgeType.Cliff) {
                    //This case should never come up, since cliffs only appear when DeepWater
                    //is in play
                }
            }else if(centerLeftEdgeType == HexEdgeType.Slope) {
                if(leftRightEdgeType == HexEdgeType.Flat) {
                    CreateRiverEndpointTrough_TerracesFlat(data);
                }else if(leftRightEdgeType == HexEdgeType.Slope) {

                    if(data.Center.FoundationElevation > data.Left.FoundationElevation) {
                        CreateRiverEndpointTrough_DoubleTerracesDown(data);
                    }else {
                        CreateRiverEndpointTrough_DoubleTerracesUp(data);
                    }

                }else if(leftRightEdgeType == HexEdgeType.Cliff) {
                    CreateRiverEndpointTrough_TerracesCliff(data);
                }
            }else if(centerLeftEdgeType == HexEdgeType.Cliff) {
                if(leftRightEdgeType == HexEdgeType.Flat) {
                    //This case should never come up, since cliffs only appear when DeepWater
                    //is in play
                }else if(leftRightEdgeType == HexEdgeType.Slope) {
                    CreateRiverEndpointTrough_CliffTerraces(data);
                }else if(leftRightEdgeType == HexEdgeType.Cliff) {
                    CreateRiverEndpointTrough_DoubleCliff(data);
                }
            }else if(centerLeftEdgeType == HexEdgeType.River) {
                if(leftRightEdgeType == HexEdgeType.Slope) {
                    
                }
            }
        }

        //Rivered edge is between Center and Right. River endpoint is pointing at Left,
        //and Center/Left and Left/Right are both flat edges. This leads to a river running
        //into the back of a terraced slope.
        private void CreateRiverEndpointTrough_DoubleFlat(CellTriangulationData data) {
            MeshBuilder.AddTriangleUnperturbed(
                data.PerturbedLeftCorner,             data.Left  .Index, MeshBuilder.Weights1,
                data.PerturbedRightCorner,            data.Right .Index, MeshBuilder.Weights2,
                data.PerturbedCenterRightTroughPoint, data.Center.Index, MeshBuilder.Weights23,
                MeshBuilder.Terrain
            );

            MeshBuilder.AddTriangleUnperturbed(
                data.PerturbedLeftCorner,             data.Left  .Index, MeshBuilder.Weights1,
                data.PerturbedCenterRightTroughPoint, data.Right .Index, MeshBuilder.Weights23,
                data.PerturbedCenterCorner,           data.Center.Index, MeshBuilder.Weights3,
                MeshBuilder.Terrain
            );

            CreateRiverEndpointSurface_Default(data);
        }

        //Rivered edge is between Center and Right. River endpoint is pointing at Left,
        //and Center/Left and Left/Right are both terraced edges
        private void CreateRiverEndpointTrough_DoubleTerracesUp(CellTriangulationData data) {
            Vector3 toCenterTerraceVertexTwo  = HexMetrics.TerraceLerp(data.LeftCorner,      data.CenterCorner,    1);
            Color   toCenterTerraceWeightsTwo = HexMetrics.TerraceLerp(MeshBuilder.Weights1, MeshBuilder.Weights2, 1);

            Vector3 toRightTerraceVertexTwo  = HexMetrics.TerraceLerp(data.LeftCorner,      data.RightCorner,     1);
            Color   toRightTerraceWeightsTwo = HexMetrics.TerraceLerp(MeshBuilder.Weights1, MeshBuilder.Weights3, 1);

            //Builds the first triangle between the terraces
            MeshBuilder.AddTriangleUnperturbed(
                NoiseGenerator.Perturb(data.LeftCorner,          data.Left.RequiresYPerturb), data.Left  .Index, MeshBuilder.Weights1,
                NoiseGenerator.Perturb(toRightTerraceVertexTwo,  false),                      data.Center.Index, toRightTerraceWeightsTwo,
                NoiseGenerator.Perturb(toCenterTerraceVertexTwo, false),                      data.Right .Index, toCenterTerraceWeightsTwo,
                MeshBuilder.Terrain
            );

            //Builds the quads between the terraces
            for(int i = 2; i < HexMetrics.TerraceSteps; i++) {
                Vector3 toCenterTerraceVertexOne  = toCenterTerraceVertexTwo;
                Color   toCenterTerraceWeightsOne = toCenterTerraceWeightsTwo;

                Vector3 toRightTerraceVertexOne  = toRightTerraceVertexTwo;
                Color   toRightTerraceWeightsOne = toRightTerraceWeightsTwo;

                toCenterTerraceVertexTwo  = HexMetrics.TerraceLerp(data.LeftCorner,      data.CenterCorner,    i);
                toCenterTerraceWeightsTwo = HexMetrics.TerraceLerp(MeshBuilder.Weights1, MeshBuilder.Weights2, i);

                toRightTerraceVertexTwo  = HexMetrics.TerraceLerp(data.LeftCorner,      data.RightCorner,     i);
                toRightTerraceWeightsTwo = HexMetrics.TerraceLerp(MeshBuilder.Weights1, MeshBuilder.Weights3, i);

                MeshBuilder.AddTriangle(
                    toCenterTerraceVertexTwo, data.Left.Index,   toCenterTerraceWeightsTwo,
                    toCenterTerraceVertexOne, data.Center.Index, toCenterTerraceWeightsOne,
                    toRightTerraceVertexOne,  data.Right.Index,  toRightTerraceWeightsOne,
                    MeshBuilder.Terrain
                );

                MeshBuilder.AddTriangle(
                    toRightTerraceVertexOne,  data.Left  .Index, toRightTerraceWeightsOne,
                    toRightTerraceVertexTwo,  data.Center.Index, toRightTerraceWeightsTwo,
                    toCenterTerraceVertexTwo, data.Right .Index, toCenterTerraceWeightsTwo,
                    MeshBuilder.Terrain
                );
            }

            //We need to check for Y perturbation on CenterCorner and RightCorner, since they're
            //probably going to be hills and hills Y perturb.
            MeshBuilder.AddTriangleUnperturbed(
                NoiseGenerator.Perturb(data.CenterCorner,        data.Center.RequiresYPerturb), data.Left  .Index, MeshBuilder.Weights2,
                NoiseGenerator.Perturb(toCenterTerraceVertexTwo, false),                        data.Center.Index, toCenterTerraceWeightsTwo,
                NoiseGenerator.Perturb(data.RightCorner,         data.Right.RequiresYPerturb),  data.Right .Index, MeshBuilder.Weights3,
                MeshBuilder.Terrain
            );

            MeshBuilder.AddTriangleUnperturbed(
                NoiseGenerator.Perturb(toCenterTerraceVertexTwo, false),                       data.Left  .Index, toCenterTerraceWeightsTwo,
                NoiseGenerator.Perturb(toRightTerraceVertexTwo,  false),                       data.Center.Index, toRightTerraceWeightsTwo,
                NoiseGenerator.Perturb(data.RightCorner,         data.Right.RequiresYPerturb), data.Right .Index, MeshBuilder.Weights3,
                MeshBuilder.Terrain
            );

            //Draws the triangle that the river collides into.
            MeshBuilder.AddTriangleUnperturbed(
                NoiseGenerator.Perturb(data.CenterCorner,           data.Center.RequiresYPerturb), data.Center.Index, MeshBuilder.Weights1,
                NoiseGenerator.Perturb(data.RightCorner,            data.Right.RequiresYPerturb),  data.Right .Index, MeshBuilder.Weights2,
                NoiseGenerator.Perturb(data.CenterRightTroughPoint, false),                        data.Left  .Index, MeshBuilder.Weights12,
                MeshBuilder.Terrain
            );
        }

        //There's a river between Center and Right, and both Center and Right are
        //above Left, with slopes going down towards Left from both sides.
        //This converges all of the terraced edges to the center trough of the river.
        //Consider figuring out a better policy.
        private void CreateRiverEndpointTrough_DoubleTerracesDown(CellTriangulationData data) {
            Vector3 leftCenterTerracePointTwo = HexMetrics.TerraceLerp(
                data.LeftCorner, data.CenterCorner, 1
            );
            Color leftCenterTerraceWeightsTwo = HexMetrics.TerraceLerp(
                MeshBuilder.Weights1, MeshBuilder.Weights3, 1
            );

            Vector3 leftRightTerracePointTwo = HexMetrics.TerraceLerp(
                data.LeftCorner, data.RightCorner, 1
            );
            Color leftRightTerraceWeightsTwo = HexMetrics.TerraceLerp(
                MeshBuilder.Weights1, MeshBuilder.Weights2, 1
            );

            MeshBuilder.AddTriangleUnperturbed(
                NoiseGenerator.Perturb(data.LeftCorner,             data.Left.RequiresYPerturb), data.Left  .Index, MeshBuilder.Weights1,
                NoiseGenerator.Perturb(data.CenterRightTroughPoint, false),                      data.Right .Index, MeshBuilder.Weights23,
                NoiseGenerator.Perturb(leftCenterTerracePointTwo,   false),                      data.Center.Index, leftCenterTerraceWeightsTwo,
                MeshBuilder.Terrain
            );

            MeshBuilder.AddTriangleUnperturbed(
                NoiseGenerator.Perturb(data.LeftCorner,             data.Left.RequiresYPerturb), data.Left  .Index, MeshBuilder.Weights1,
                NoiseGenerator.Perturb(leftRightTerracePointTwo,    false),                      data.Right .Index, leftRightTerraceWeightsTwo,
                NoiseGenerator.Perturb(data.CenterRightTroughPoint, false),                      data.Center.Index, MeshBuilder.Weights23,
                MeshBuilder.Terrain
            );

            for(int i = 2; i < HexMetrics.TerraceSteps; i++) {
                Vector3 leftCenterTerracePointOne   = leftCenterTerracePointTwo;
                Color   leftCenterTerraceWeightsOne = leftCenterTerraceWeightsTwo;

                Vector3 leftRightTerracePointOne   = leftRightTerracePointTwo;
                Color   leftRightTerraceWeightsOne = leftRightTerraceWeightsTwo;

                leftCenterTerracePointTwo = HexMetrics.TerraceLerp(
                    data.LeftCorner, data.CenterCorner, i
                );
                leftCenterTerraceWeightsTwo = HexMetrics.TerraceLerp(
                    MeshBuilder.Weights1, MeshBuilder.Weights3, i
                );

                leftRightTerracePointTwo = HexMetrics.TerraceLerp(
                    data.LeftCorner, data.RightCorner, i
                );
                leftRightTerraceWeightsTwo = HexMetrics.TerraceLerp(
                    MeshBuilder.Weights1, MeshBuilder.Weights2, i
                );

                MeshBuilder.AddTriangle(
                    leftCenterTerracePointOne,   data.Left  .Index, leftCenterTerraceWeightsOne,
                    data.CenterRightTroughPoint, data.Right .Index, MeshBuilder.Weights23,
                    leftCenterTerracePointTwo,   data.Center.Index, leftCenterTerraceWeightsTwo,
                    MeshBuilder.Terrain
                );

                MeshBuilder.AddTriangle(
                    leftRightTerracePointOne,    data.Left  .Index, leftRightTerraceWeightsOne,
                    leftRightTerracePointTwo,    data.Right .Index, leftRightTerraceWeightsTwo,
                    data.CenterRightTroughPoint, data.Center.Index, MeshBuilder.Weights23,
                    MeshBuilder.Terrain
                );
            }

            MeshBuilder.AddTriangleUnperturbed(
                NoiseGenerator.Perturb(leftCenterTerracePointTwo,   false),                        data.Left  .Index, leftCenterTerraceWeightsTwo,
                NoiseGenerator.Perturb(data.CenterRightTroughPoint, false),                        data.Right .Index, MeshBuilder.Weights23,
                NoiseGenerator.Perturb(data.CenterCorner,           data.Center.RequiresYPerturb), data.Center.Index, MeshBuilder.Weights3,
                MeshBuilder.Terrain
            );

            MeshBuilder.AddTriangleUnperturbed(
                NoiseGenerator.Perturb(leftRightTerracePointTwo,    false),                       data.Left  .Index, leftRightTerraceWeightsTwo,
                NoiseGenerator.Perturb(data.RightCorner,            data.Right.RequiresYPerturb), data.Right .Index, MeshBuilder.Weights2,
                NoiseGenerator.Perturb(data.CenterRightTroughPoint, false),                       data.Center.Index, MeshBuilder.Weights23,
                MeshBuilder.Terrain
            );

            //The current policy is to drop a waterfall from this edge, since
            //The only way to have two terraces sloping down is to have the river
            //flow into a shallow water cell.
            Vector3 yAdjustedCenter = data.PerturbedCenterCorner,
                    yAdjustedLeft   = data.PerturbedLeftCorner,
                    yAdjustedRight  = data.PerturbedRightCorner;

            //The waterfall is supposed to drop down to the water level,
            //so we need to adjust the corner points differently than when
            //creating a river confluence.
            yAdjustedCenter.y = data.Left.WaterSurfaceY;
            yAdjustedLeft  .y = data.Left.WaterSurfaceY;
            yAdjustedRight .y = data.Left.WaterSurfaceY;

            //We need to rotate to make sure that our water cell is on the Right
            CreateRiverSurface_Waterfall(
                MeshBuilder.GetTriangulationData(
                    data.Right, data.Center, data.Left, data.Direction.Previous2()
                ),
                yAdjustedRight, yAdjustedCenter, yAdjustedLeft
            );
        }

        //Rivered edge is between Center and Right. River endpoint is pointing at Left,
        //and Center/Left and Left/Right are both cliffed edges
        private void CreateRiverEndpointTrough_DoubleCliff(CellTriangulationData data) {
            MeshBuilder.AddTriangleUnperturbed(
                data.PerturbedCenterCorner,           data.Center.Index, MeshBuilder.Weights1,
                data.PerturbedLeftCorner,             data.Left  .Index, MeshBuilder.Weights2,
                data.PerturbedCenterRightTroughPoint, data.Right .Index, MeshBuilder.Weights23,
                MeshBuilder.Terrain
            );

            MeshBuilder.AddTriangleUnperturbed(
                data.PerturbedCenterRightTroughPoint, data.Center.Index, MeshBuilder.Weights23,
                data.PerturbedLeftCorner,             data.Left  .Index, MeshBuilder.Weights2,
                data.PerturbedRightCorner,            data.Right .Index, MeshBuilder.Weights3,
                MeshBuilder.Terrain
            );

            //The current policy is to drop a waterfall from this edge, since
            //The only way to have two terraces sloping down is to have the river
            //flow into a shallow water cell.
            Vector3 yAdjustedCenter = data.PerturbedCenterCorner,
                    yAdjustedLeft   = data.PerturbedLeftCorner,
                    yAdjustedRight  = data.PerturbedRightCorner;

            //The waterfall is supposed to drop down to the water level,
            //so we need to adjust the corner points differently than when
            //creating a river confluence.
            yAdjustedCenter.y = data.Left.WaterSurfaceY;
            yAdjustedLeft  .y = data.Left.WaterSurfaceY;
            yAdjustedRight .y = data.Left.WaterSurfaceY;

            //We need to rotate to make sure that our water cell is on the Right
            CreateRiverSurface_Waterfall(
                MeshBuilder.GetTriangulationData(
                    data.Right, data.Center, data.Left, data.Direction.Previous2()
                ),
                yAdjustedRight, yAdjustedCenter, yAdjustedLeft
            );
        }

        //Rivered edge is between Center and Right. River endpoint is pointing at Left,
        //Center/Left is flat, and Left/Right is a terraced edge, which could only
        //Possible be pointing up
        private void CreateRiverEndpointTrough_FlatTerraces(CellTriangulationData data) {
            //Adds the triangle opposite the terraced edge, equivalent to half of a flat endpoint
            MeshBuilder.AddTriangleUnperturbed(
                data.PerturbedLeftCorner,             data.Left  .Index, MeshBuilder.Weights1,
                data.PerturbedCenterRightTroughPoint, data.Right .Index, MeshBuilder.Weights23,
                data.PerturbedCenterCorner,           data.Center.Index, MeshBuilder.Weights3,
                MeshBuilder.Terrain
            );

            var convergencePoint = Vector3.Lerp(
                data.PerturbedCenterRightTroughPoint, data.PerturbedRightCorner, 0.5f
            );

            var convergenceWeights = Color.Lerp(MeshBuilder.Weights3, MeshBuilder.Weights23, 0.5f);

            //Adds the bottom half of the right-facing side of the endpoint
            MeshBuilder.AddTriangleUnperturbed(
                data.PerturbedLeftCorner,             data.Left.Index,   MeshBuilder.Weights1,
                convergencePoint,                     data.Right.Index,  convergenceWeights,
                data.PerturbedCenterRightTroughPoint, data.Center.Index, MeshBuilder.Weights23,
                MeshBuilder.Terrain
            );

            Vector3 terraceVertexTwo  = HexMetrics.TerraceLerp(data.LeftCorner,      data.RightCorner,     1);
            Color   terraceWeightsTwo = HexMetrics.TerraceLerp(MeshBuilder.Weights1, MeshBuilder.Weights2, 1);

            //Builds out the terrace convergence between Left and Right
            MeshBuilder.AddTriangleUnperturbed(
                data.PerturbedLeftCorner,                 data.Left  .Index, MeshBuilder.Weights1,
                NoiseGenerator.Perturb(terraceVertexTwo), data.Right .Index, terraceWeightsTwo,
                convergencePoint,                         data.Center.Index, convergenceWeights,
                MeshBuilder.Terrain
            );

            for(int i = 2; i < HexMetrics.TerraceSteps; i++) {
                Vector3 terraceVertexOne  = terraceVertexTwo;
                Color   terraceWeightsOne = terraceWeightsTwo;

                terraceVertexTwo  = HexMetrics.TerraceLerp(data.LeftCorner,      data.RightCorner,     i);
                terraceWeightsTwo = HexMetrics.TerraceLerp(MeshBuilder.Weights1, MeshBuilder.Weights2, i);

                MeshBuilder.AddTriangleUnperturbed(
                    NoiseGenerator.Perturb(terraceVertexOne), data.Left  .Index, terraceWeightsOne,
                    NoiseGenerator.Perturb(terraceVertexTwo), data.Right .Index, terraceWeightsTwo,
                    convergencePoint,                         data.Center.Index, convergenceWeights,
                    MeshBuilder.Terrain
                );
            }

            MeshBuilder.AddTriangleUnperturbed(
                NoiseGenerator.Perturb(terraceVertexTwo), data.Left  .Index, terraceWeightsTwo,
                data.PerturbedRightCorner,                data.Right .Index, MeshBuilder.Weights2,
                convergencePoint,                         data.Center.Index, convergenceWeights,
                MeshBuilder.Terrain
            );

            CreateRiverEndpointSurface_Default(data);
        }

        //Rivered edge is between Center and Right. River endpoint is pointing at left,
        //Center/Left is a terraced edge going down from Center to Left,
        //and Left/Right is flat
        private void CreateRiverEndpointTrough_TerracesFlat(CellTriangulationData data) {
            //Builds out the flat, cliff-like surface opposite Center
            MeshBuilder.AddTriangleUnperturbed(
                NoiseGenerator.Perturb(data.LeftCorner,  data.Left.RequiresYPerturb),  data.Left  .Index, MeshBuilder.Weights1,
                NoiseGenerator.Perturb(data.RightCorner, data.Right.RequiresYPerturb), data.Right .Index, MeshBuilder.Weights2,
                data.PerturbedCenterRightTroughPoint,                                  data.Center.Index, MeshBuilder.Weights23,
                MeshBuilder.Terrain
            );

            //We want to converge Center/Left's terraces to the surface opposite Left, so we need
            //to find the midpoint between the trough and the left corner
            var convergencePoint   = Vector3.Lerp(data.CenterRightTroughPoint, data.CenterCorner,    0.5f);
            var convergenceWeights = Color  .Lerp(MeshBuilder.Weights23,       MeshBuilder.Weights2, 0.5f);

            //ConvergencePoint has the potential to deviate from the above triangle, so we need to make
            //sure to fill any space that could appear
            MeshBuilder.AddTriangleUnperturbed(
                NoiseGenerator.Perturb(data.LeftCorner, data.Left.RequiresYPerturb), data.Left  .Index, MeshBuilder.Weights1,
                data.PerturbedCenterRightTroughPoint,                                data.Right .Index, MeshBuilder.Weights23,
                NoiseGenerator.Perturb(convergencePoint),                            data.Center.Index, convergenceWeights,
                MeshBuilder.Terrain
            );

            //Builds out the terrace convergence from the terraced slope on Center/Left
            //to the convergence point between CenterRightTrough and LeftCorner
            Vector3 terracePointTwo   = HexMetrics.TerraceLerp(data.LeftCorner,      data.CenterCorner,    1);
            Color   terraceWeightsTwo = HexMetrics.TerraceLerp(MeshBuilder.Weights1, MeshBuilder.Weights3, 1);

            MeshBuilder.AddTriangleUnperturbed(
                NoiseGenerator.Perturb(terracePointTwo),                             data.Left  .Index, terraceWeightsTwo,
                NoiseGenerator.Perturb(data.LeftCorner, data.Left.RequiresYPerturb), data.Right .Index, MeshBuilder.Weights1,
                NoiseGenerator.Perturb(convergencePoint),                            data.Center.Index, convergenceWeights,
                MeshBuilder.Terrain
            );

            for(int i = 2; i < HexMetrics.TerraceSteps; i++) {
                Vector3 terracePointOne   = terracePointTwo;
                Color   terraceWeightsOne = terraceWeightsTwo;

                terracePointTwo   = HexMetrics.TerraceLerp(data.LeftCorner,      data.CenterCorner,    i);
                terraceWeightsTwo = HexMetrics.TerraceLerp(MeshBuilder.Weights1, MeshBuilder.Weights3, i);

                MeshBuilder.AddTriangle(
                    terracePointTwo,  data.Left  .Index, terraceWeightsTwo,
                    terracePointOne,  data.Right .Index, terraceWeightsOne,
                    convergencePoint, data.Center.Index, convergenceWeights,
                    MeshBuilder.Terrain
                );
            }

            MeshBuilder.AddTriangleUnperturbed(
                NoiseGenerator.Perturb(data.CenterCorner, data.Center.RequiresYPerturb), data.Left  .Index, MeshBuilder.Weights3,
                NoiseGenerator.Perturb(terracePointTwo,   false),                        data.Right .Index, terraceWeightsTwo,
                NoiseGenerator.Perturb(convergencePoint,  false),                        data.Center.Index, convergenceWeights,
                MeshBuilder.Terrain
            );

            //After converging the terraces, there remains a triangular surface between CenterCorner,
            //convergencePoint, and CenterRightTroughPoint that needs to be filled
            MeshBuilder.AddTriangleUnperturbed(
                NoiseGenerator.Perturb(data.CenterCorner, data.Center.RequiresYPerturb), data.Left.Index,   MeshBuilder.Weights3,
                NoiseGenerator.Perturb(convergencePoint,  false),                        data.Right.Index,  convergenceWeights,
                data.PerturbedCenterRightTroughPoint,                                    data.Center.Index, MeshBuilder.Weights23,
                MeshBuilder.Terrain
            );

            CreateRiverEndpointSurface_Default(data);
        }

        //This method handles only the case when Left is below Center and Right, and thus the terraced
        //slope is going down towards Left. The only way the terrace could go up is if Right
        //were Deep Water, which would prevent the existence of a river between Center and Right.
        private void CreateRiverEndpointTrough_TerracesCliff(CellTriangulationData data) {
            //Builds the triangle that deals with the cliff edge attached to Right
            MeshBuilder.AddTriangle(
                data.LeftCorner,             data.Left  .Index, MeshBuilder.Weights1,
                data.RightCorner,            data.Right .Index, MeshBuilder.Weights2,
                data.CenterRightTroughPoint, data.Center.Index, MeshBuilder.Weights23,
                MeshBuilder.Terrain
            );

            //Builds out the terraced edges attached to Center by converging them to the
            //trough point opposite Left
            Vector3 terracePointTwo   = HexMetrics.TerraceLerp(data.LeftCorner,      data.CenterCorner,    1);
            Color   terraceWeightsTwo = HexMetrics.TerraceLerp(MeshBuilder.Weights1, MeshBuilder.Weights3, 1);

            MeshBuilder.AddTriangle(
                data.LeftCorner,             data.Left  .Index, MeshBuilder.Weights1,
                data.CenterRightTroughPoint, data.Right .Index, MeshBuilder.Weights23,
                terracePointTwo,             data.Center.Index, terraceWeightsTwo,
                MeshBuilder.Terrain
            );

            for(int i = 2; i < HexMetrics.TerraceSteps; i++) {
                Vector3 terracePointOne   = terracePointTwo;
                Color   terraceWeightsOne = terraceWeightsTwo;

                terracePointTwo   = HexMetrics.TerraceLerp(data.LeftCorner,      data.CenterCorner,    i);
                terraceWeightsTwo = HexMetrics.TerraceLerp(MeshBuilder.Weights1, MeshBuilder.Weights3, i);

                MeshBuilder.AddTriangle(
                    terracePointOne,             data.Left  .Index, terraceWeightsOne,
                    data.CenterRightTroughPoint, data.Right .Index, MeshBuilder.Weights23,
                    terracePointTwo,             data.Center.Index, terraceWeightsTwo,
                    MeshBuilder.Terrain
                );
            }

            MeshBuilder.AddTriangle(
                terracePointTwo,             data.Left  .Index, terraceWeightsTwo,
                data.CenterRightTroughPoint, data.Right .Index, MeshBuilder.Weights23,
                data.CenterCorner,           data.Center.Index, MeshBuilder.Weights3,
                MeshBuilder.Terrain
            );
        }

        //This method handles only the case where Left is below Center and Right, and thus the
        //terraced slope is going down from Right to Left. The only way the terrace could go up is
        //if Right were Deep Water, which would prevent the existence of a river between Center and Right.
        private void CreateRiverEndpointTrough_CliffTerraces(CellTriangulationData data) {
            //Builds the triangle that deals with the cliff edge attached to Center
            MeshBuilder.AddTriangle(
                data.LeftCorner,             data.Left  .Index, MeshBuilder.Weights1,
                data.CenterRightTroughPoint, data.Right .Index, MeshBuilder.Weights23,
                data.CenterCorner,           data.Center.Index, MeshBuilder.Weights3,
                MeshBuilder.Terrain
            );

            //Builds out the terraced edges attached to Right by converging them to the
            //trough point opposite Left
            Vector3 terracePointTwo   = HexMetrics.TerraceLerp(data.LeftCorner,      data.RightCorner,     1);
            Color   terraceWeightsTwo = HexMetrics.TerraceLerp(MeshBuilder.Weights1, MeshBuilder.Weights2, 1);

            MeshBuilder.AddTriangle(
                data.LeftCorner,             data.Left  .Index, MeshBuilder.Weights1,
                terracePointTwo,             data.Right .Index, terraceWeightsTwo,
                data.CenterRightTroughPoint, data.Center.Index, MeshBuilder.Weights23,
                MeshBuilder.Terrain
            );

            for(int i = 2; i < HexMetrics.TerraceSteps; i++) {
                Vector3 terracePointOne   = terracePointTwo;
                Color   terraceWeightsOne = terraceWeightsTwo;

                terracePointTwo   = HexMetrics.TerraceLerp(data.LeftCorner,      data.RightCorner,     i);
                terraceWeightsTwo = HexMetrics.TerraceLerp(MeshBuilder.Weights1, MeshBuilder.Weights2, i);

                MeshBuilder.AddTriangle(
                    terracePointOne,             data.Left  .Index, terraceWeightsOne,
                    terracePointTwo,             data.Right .Index, terraceWeightsTwo,
                    data.CenterRightTroughPoint, data.Center.Index, MeshBuilder.Weights23,
                    MeshBuilder.Terrain
                );
            }

            MeshBuilder.AddTriangle(
                terracePointTwo,             data.Left  .Index, terraceWeightsTwo,
                data.RightCorner,            data.Right .Index, MeshBuilder.Weights2,
                data.CenterRightTroughPoint, data.Center.Index, MeshBuilder.Weights23,
                MeshBuilder.Terrain
            );
        }

        private void CreateRiverEndpointSurface_Default(CellTriangulationData data) {
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

    }

}
