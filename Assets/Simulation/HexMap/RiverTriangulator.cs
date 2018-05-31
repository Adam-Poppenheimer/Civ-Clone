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

        public void TriangulateConnectionAsRiver(
            HexDirection direction, IHexCell cell, EdgeVertices nearEdge
        ) {
            HexEdgeType thisToNeighborEdgeType, thisToPreviousNeighborEdgeType, previousNeighborToNeighborEdgeType;

            IHexCell previousNeighbor = Grid.GetNeighbor(cell, direction.Previous());
            IHexCell neighbor         = Grid.GetNeighbor(cell, direction);            
            IHexCell nextNeighbor     = Grid.GetNeighbor(cell, direction.Next());

            var thisCornerData = new RiverData(cell, previousNeighbor, neighbor,     direction,        NoiseGenerator);
            var nextCornerData = new RiverData(cell, neighbor,         nextNeighbor, direction.Next(), NoiseGenerator);            

            if(neighbor == null) {
                thisToNeighborEdgeType             = HexEdgeType.Void;
                previousNeighborToNeighborEdgeType = HexEdgeType.Void;

            }else if(RiverCanon.HasRiverAlongEdge(cell, direction)){
                thisToNeighborEdgeType = HexEdgeType.River;

            }else {
                thisToNeighborEdgeType = HexMetrics.GetEdgeType(cell, neighbor);
            }

            if(previousNeighbor == null) {
                thisToPreviousNeighborEdgeType     = HexEdgeType.Void;
                previousNeighborToNeighborEdgeType = HexEdgeType.Void;

            }else if(RiverCanon.HasRiverAlongEdge(cell, direction.Previous())) {
                thisToPreviousNeighborEdgeType = HexEdgeType.River;

            }else {
                thisToPreviousNeighborEdgeType = HexMetrics.GetEdgeType(cell, previousNeighbor);
            }

            if(previousNeighbor != null && neighbor != null) {
                if(RiverCanon.HasRiverAlongEdge(neighbor, direction.Opposite().Next())) {
                    previousNeighborToNeighborEdgeType = HexEdgeType.River;
                }else {
                    previousNeighborToNeighborEdgeType = HexMetrics.GetEdgeType(previousNeighbor, neighbor);
                }
            }else {
                previousNeighborToNeighborEdgeType = HexEdgeType.Void;
            }

            if(thisToNeighborEdgeType == HexEdgeType.River) {
                EdgeVertices farEdge    = GetYAdjustedEdge(neighbor, direction.Opposite());
                EdgeVertices troughEdge = GetRiverTroughEdge(nearEdge, farEdge, false);

                CreateRiverTrough_Edge(direction, cell, nearEdge, neighbor, troughEdge);

                if(direction <= HexDirection.SE) {
                    CreateRiverSurface_EdgesAndCorners(thisCornerData, nextCornerData);
                }

                if(thisToPreviousNeighborEdgeType == HexEdgeType.River) {

                    if(previousNeighborToNeighborEdgeType == HexEdgeType.River) {
                        CreateRiverTrough_Confluence (thisCornerData);
                        CreateRiverSurface_Confluence(thisCornerData);
                    }else {
                        CreateRiverTrough_Curve(thisCornerData);
                    }
                }else if(previousNeighborToNeighborEdgeType != HexEdgeType.River){
                    CreateRiverTrough_Endpoint(thisCornerData);
                }
            }
        }

        public bool HasRiverCorner(IHexCell cell, HexDirection direction) {
            IHexCell neighbor     = Grid.GetNeighbor(cell, direction);
            IHexCell nextNeighbor = Grid.GetNeighbor(cell, direction.Next());

            if(neighbor != null && RiverCanon.HasRiverAlongEdge(cell, direction)) {
                return true;

            }else if(nextNeighbor != null && RiverCanon.HasRiverAlongEdge(cell, direction.Next())) {
                return true;

            }else if(
                neighbor != null && nextNeighbor != null &&
                RiverCanon.HasRiverAlongEdge(neighbor, direction.Opposite().Previous())
            ){
                return true;
            }else {
                return false;
            }
        }

        #endregion

        private void CreateRiverTrough_Edge(
            HexDirection direction, IHexCell cell, EdgeVertices nearEdge,
            IHexCell neighbor, EdgeVertices troughEdge
        ){
            Color troughWeights = Color.Lerp(MeshBuilder.Weights1, MeshBuilder.Weights2, 0.5f);

            MeshBuilder.TriangulateEdgeStrip(
                nearEdge,   MeshBuilder.Weights1, cell.Index,     cell.Shape == TerrainShape.Hills,
                troughEdge, troughWeights,        neighbor.Index, false,
                false
            );
        }

        //This method creates the river surface for edges. It extends these edges into any
        //applicable river corners so that UV interpolation lets the river flow properly.
        //Resolving the corners independently led to problems that I couldn't resolve.
        //In order to handle rivers of different elevation, this method averages the Y values
        //of nearby river edges. This can sometimes cause rivers to flow uphill, though
        //the effect is usually unnoticeable as long as elevation perturbation is small
        //and the terrain is flat.
        private void CreateRiverSurface_EdgesAndCorners(RiverData thisData, RiverData nextData){
            float upRiverSurfaceY, intermediateUpRiverSurfaceY,
                  intermediateDownRiverSurfaceY, downRiverSurfaceY;

            Vector3 upRiverLeftBank, upRiverRightBank, intermediateUpRiverLeftBank, intermediateUpRiverRightBank;
            Vector3 intermediateDownRiverLeftBank, intermediateDownRiverRightBank, downRiverLeftBank, downRiverRightBank;

            float upRiverParam, downRiverParam;

            CalculateRiverSurfaceEdgeProperties(
                thisData, false, out upRiverSurfaceY, out upRiverLeftBank, out upRiverRightBank,
                out upRiverParam
            );

            CalculateRiverSurfaceEdgeProperties(
                nextData, true,  out downRiverSurfaceY, out downRiverLeftBank, out downRiverRightBank,
                out downRiverParam
            );

            intermediateUpRiverSurfaceY   = Mathf.Min(thisData.Center.RiverSurfaceY, thisData.Right.RiverSurfaceY);
            intermediateDownRiverSurfaceY = Mathf.Min(thisData.Center.RiverSurfaceY, thisData.Right.RiverSurfaceY);

            intermediateUpRiverLeftBank  = Vector3.Lerp(upRiverLeftBank,  downRiverLeftBank,  upRiverParam);
            intermediateUpRiverRightBank = Vector3.Lerp(upRiverRightBank, downRiverRightBank, upRiverParam);

            intermediateDownRiverLeftBank  = Vector3.Lerp(upRiverLeftBank,  downRiverLeftBank,  1f - downRiverParam);
            intermediateDownRiverRightBank = Vector3.Lerp(upRiverRightBank, downRiverRightBank, 1f - downRiverParam);

            bool isReversed = RiverCanon.GetFlowOfRiverAtEdge(thisData.Center, thisData.Direction) == RiverFlow.Counterclockwise;

            Vector3 indices = new Vector3(
                thisData.Center.Index, thisData.Center.Index, thisData.Center.Index
            );

            //If the intermediate up-river section will start at the corner, there's
            //no reason to create another segment of water. This happens when the
            //corner is a confluence and this edge is supposed to make a waterfall into it.
            //That triangulation needs to be handled as a special case elsewhere, however.
            if(upRiverParam > 0f) {
                MeshBuilder.TriangulateRiverQuadUnperturbed(
                    upRiverLeftBank, upRiverRightBank,
                    intermediateUpRiverLeftBank, intermediateUpRiverRightBank,
                    upRiverSurfaceY, intermediateUpRiverSurfaceY,
                    0f, upRiverParam,
                    isReversed, indices
                );
            }else {
                intermediateUpRiverSurfaceY = upRiverSurfaceY;
            }

            //We ignore the final down-river section for the same reasons we ignore
            //the first section; if our intermediate quad got us all the way to the
            //corner
            if(downRiverParam > 0f) {
                MeshBuilder.TriangulateRiverQuadUnperturbed(
                    intermediateDownRiverLeftBank, intermediateDownRiverRightBank,
                    downRiverLeftBank, downRiverRightBank,
                    intermediateUpRiverSurfaceY, downRiverSurfaceY,
                    1f - downRiverParam, 1f,
                    isReversed, indices
                );
            }else {
                intermediateDownRiverSurfaceY = downRiverSurfaceY;
            }

            MeshBuilder.TriangulateRiverQuadUnperturbed(
                intermediateUpRiverLeftBank, intermediateUpRiverRightBank,
                intermediateDownRiverLeftBank, intermediateDownRiverRightBank,
                intermediateUpRiverSurfaceY, intermediateDownRiverSurfaceY,
                upRiverParam, 1f - downRiverParam,
                isReversed, indices
            );
            
        }

        //Determines what the endpoints of a particular river surface segment should look like.
        //Unlike other triangulations, river surface triangulation needs to be simultaneously
        //aware of a particular edge and both of its corners in order to function properly.
        //A large number of corner cases are handled by extending the edge's surface into the corners,
        //so that UVs wrap properly.
        //Confluences and endpoints are handled separately.
        private void CalculateRiverSurfaceEdgeProperties(
            RiverData data, bool targetingNextCorner,
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
        private void CreateRiverSurface_Confluence(RiverData data) {
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
            MeshBuilder.AddWaterTriangleUnperturbed(
                yAdjustedCenter, data.Center.Index, MeshBuilder.Weights1,
                yAdjustedLeft,   data.Left  .Index, MeshBuilder.Weights2,
                yAdjustedRight,  data.Right .Index, MeshBuilder.Weights3
            );

            //We need to handle waterfalls into confluences as a special case. Edge and corner
            //surface generation should account for this by not building its river edge to the
            //confluence's water triangle
            if( data.Center.EdgeElevation > data.Right.EdgeElevation &&
                data.Left  .EdgeElevation > data.Right.EdgeElevation
            ) {
                //There's a waterfall flowing from CenterLeft river into the confluence
                float edgeY = Mathf.Min(data.Center.RiverSurfaceY, data.Left.RiverSurfaceY);

                bool isReversed = RiverCanon.GetFlowOfRiverAtEdge(data.Center, data.Direction.Previous()) == RiverFlow.Counterclockwise;
                var indices = new Vector3(data.Center.Index, data.Center.Index, data.Center.Index);

                var waterfallUpRiverLeft  = data.PerturbedLeftCorner;
                var waterfallUpRiverRight = data.PerturbedCenterCorner;

                waterfallUpRiverLeft .y = edgeY;
                waterfallUpRiverRight.y = edgeY;

                //We want the waterfall to stop when it hits the water triangle at the confluence.
                //Rather than performing a ray-triangle intersection test (which can miss because
                //our ray will pass very close to the triangle's edge) we can use
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
                    0f, 0.2f, isReversed, indices
                );
            }
        }

        //Draws six triangles connecting the three inward-facing solid corners of the triangles, the
        //troughs of the rivers on all three edges, and the confluence point at the middle of the
        //intersecting rivers
        private void CreateRiverTrough_Confluence(RiverData data){
            //We're building out the entire corner section, so we can ignore the corners
            //in the same way normal corner triangulation functions
            if(data.Direction > HexDirection.E) {
                return;
            }

            //Triangle pointing at center, which LeftRight river flows towards
            MeshBuilder.AddTerrainTriangleUnperturbed(
                NoiseGenerator.Perturb(data.CenterCorner,           data.Center.RequiresYPerturb), data.Center.Index, MeshBuilder.Weights1,
                NoiseGenerator.Perturb(data.CenterLeftTroughPoint,  false),                        data.Left  .Index, MeshBuilder.Weights12,
                NoiseGenerator.Perturb(data.CenterRightTroughPoint, false),                        data.Right .Index, MeshBuilder.Weights13
            );

            //Triangle pointing at left, which CenterRight river flows towards
            MeshBuilder.AddTerrainTriangleUnperturbed(
                NoiseGenerator.Perturb(data.LeftCorner,            data.Left.RequiresYPerturb), data.Center.Index, MeshBuilder.Weights2,
                NoiseGenerator.Perturb(data.LeftRightTroughPoint,  false),                      data.Left  .Index, MeshBuilder.Weights23,
                NoiseGenerator.Perturb(data.CenterLeftTroughPoint, false),                      data.Right .Index, MeshBuilder.Weights12
            );

            //Triangle pointing at right, which CenterLeft river flows towards
            MeshBuilder.AddTerrainTriangleUnperturbed(
                NoiseGenerator.Perturb(data.RightCorner,            data.Right.RequiresYPerturb), data.Center.Index, MeshBuilder.Weights3,
                NoiseGenerator.Perturb(data.CenterRightTroughPoint, false),                       data.Left  .Index, MeshBuilder.Weights13,
                NoiseGenerator.Perturb(data.LeftRightTroughPoint,   false),                       data.Right .Index, MeshBuilder.Weights23
            );

            //Triangle in the middle, touching each of the trough points and which all rivers flow towards
            MeshBuilder.AddTerrainTriangleUnperturbed(
                data.PerturbedCenterLeftTroughPoint,  data.Center.Index, MeshBuilder.Weights12,
                data.PertrubedLeftRightTroughPoint,   data.Left  .Index, MeshBuilder.Weights23,
                data.PerturbedCenterRightTroughPoint, data.Right .Index, MeshBuilder.Weights13
            );
        }

        //Non-rivered edge is between right and left. Outer river bank is opposite center
        private void CreateRiverTrough_Curve(RiverData data){
            CreateRiverInnerCurve(data);

            //We've already taken care of River and Void edge types, so we don't need to check them again here
            HexEdgeType leftRightEdge = HexMetrics.GetEdgeType(data.Left, data.Right);

            if(leftRightEdge == HexEdgeType.Flat) {
                CreateRiverOuterCurveFlat(data);
            }else if(leftRightEdge == HexEdgeType.Slope) {
                if(data.Left.EdgeElevation < data.Right.EdgeElevation) {
                    CreateRiverOuterCurveSloped_TerracesClockwiseUp(data);
                }else {
                    CreateRiverOuterCurveSloped_TerracesClockwiseDown(data);
                }                
            }
        }

        private void CreateRiverInnerCurve(RiverData data){
            MeshBuilder.AddTerrainTriangleUnperturbed(
                data.PerturbedCenterCorner,    data.Center.Index, MeshBuilder.Weights1,
                data.PerturbedCenterLeftTroughPoint,  data.Left.Index,   MeshBuilder.Weights12,
                data.PerturbedCenterRightTroughPoint, data.Right.Index,  MeshBuilder.Weights13
            );
        }

        //Non-rivered edge is between right and left. Outer river bank is opposite center
        private void CreateRiverOuterCurveFlat(RiverData data){
            MeshBuilder.AddTerrainTriangleUnperturbed(
                data.PerturbedLeftCorner,    data.Left  .Index, MeshBuilder.Weights1,
                data.PerturbedRightCorner,   data.Right .Index, MeshBuilder.Weights2,
                data.PerturbedCenterLeftTroughPoint, data.Center.Index, MeshBuilder.Weights13
            );
            MeshBuilder.AddTerrainTriangleUnperturbed(
                data.PerturbedCenterLeftTroughPoint,  data.Left  .Index, MeshBuilder.Weights13,
                data.PerturbedRightCorner,    data.Right .Index, MeshBuilder.Weights2,
                data.PerturbedCenterRightTroughPoint, data.Center.Index, MeshBuilder.Weights23
            );
        }

        //Non-rivered edge is between right and left. Outer river bank is opposite center
        private void CreateRiverOuterCurveSloped_TerracesClockwiseUp(RiverData data) {
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
            MeshBuilder.AddTerrainTriangleUnperturbed(
                data.PerturbedCenterLeftTroughPoint,  data.Center.Index, MeshBuilder.Weights12,
                data.PerturbedLeftCorner,     data.Left  .Index, MeshBuilder.Weights2,
                data.PerturbedCenterRightTroughPoint, data.Right .Index, MeshBuilder.Weights13
            );

            MeshBuilder.AddTerrainTriangleUnperturbed(
                data.PerturbedCenterRightTroughPoint, data.Center.Index, MeshBuilder.Weights13,
                data.PerturbedLeftCorner,     data.Left  .Index, MeshBuilder.Weights2,
                convergencePoint,                     data.Right .Index, convergenceWeights
            );

            Vector3 stepVertex2 = NoiseGenerator.Perturb(
                HexMetrics.TerraceLerp(data.LeftCorner, data.RightCorner, 1)
            );

            Color stepWeights2 = HexMetrics.TerraceLerp(MeshBuilder.Weights2, MeshBuilder.Weights3, 1);

            MeshBuilder.AddTerrainTriangleUnperturbed(
                data.LeftCorner, data.Center.Index, MeshBuilder.Weights2,
                stepVertex2,             data.Left  .Index, stepWeights2,
                convergencePoint,        data.Right .Index, convergenceWeights
            );

            for(int i = 2; i < HexMetrics.TerraceSteps; i++) {
                Vector3 stepVertex1 = stepVertex2;
                Color stepWeights1  = stepWeights2;

                stepVertex2 = NoiseGenerator.Perturb(
                    HexMetrics.TerraceLerp(data.LeftCorner, data.RightCorner, i)
                );

                stepWeights2 = HexMetrics.TerraceLerp(MeshBuilder.Weights2, MeshBuilder.Weights3, i);

                MeshBuilder.AddTerrainTriangleUnperturbed(
                    stepVertex1,       data.Center.Index, stepWeights1,
                    stepVertex2,       data.Left  .Index, stepWeights2,
                    convergencePoint,  data.Right .Index, convergenceWeights
                );
            }

            MeshBuilder.AddTerrainTriangleUnperturbed(
                stepVertex2,                       data.Center.Index, stepWeights2,
                data.PerturbedRightCorner, data.Left  .Index, MeshBuilder.Weights3,
                convergencePoint,                  data.Right .Index, convergenceWeights
            );
        }

        //Non-rivered edge is between right and left. Outer river bank is opposite center
        private void CreateRiverOuterCurveSloped_TerracesClockwiseDown(RiverData data) {
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
            MeshBuilder.AddTerrainTriangleUnperturbed(
                data.PerturbedCenterLeftTroughPoint,  data.Center.Index, MeshBuilder.Weights13,
                data.PerturbedRightCorner,            data.Right .Index, MeshBuilder.Weights2,
                data.PerturbedCenterRightTroughPoint, data.Left  .Index, MeshBuilder.Weights12
            );

            MeshBuilder.AddTerrainTriangleUnperturbed(
                data.PerturbedCenterLeftTroughPoint, data.Center.Index, MeshBuilder.Weights13,
                convergencePoint,                    data.Right .Index, convergenceWeights,
                data.PerturbedRightCorner,   data.Left  .Index, MeshBuilder.Weights2
            );

            Vector3 stepVertex2 = NoiseGenerator.Perturb(
                HexMetrics.TerraceLerp(data.RightCorner, data.LeftCorner, 1)
            );

            Color stepWeights2 = HexMetrics.TerraceLerp(MeshBuilder.Weights2, MeshBuilder.Weights3, 1);

            MeshBuilder.AddTerrainTriangleUnperturbed(
                convergencePoint,         data.Center.Index, convergenceWeights,
                stepVertex2,              data.Right .Index, stepWeights2,
                data.RightCorner, data.Left  .Index, MeshBuilder.Weights2
            );

            for(int i = 2; i < HexMetrics.TerraceSteps; i++) {
                Vector3 stepVertex1 = stepVertex2;
                Color stepWeights1  = stepWeights2;

                stepVertex2 = NoiseGenerator.Perturb(
                    HexMetrics.TerraceLerp(data.RightCorner, data.LeftCorner, i)
                );

                stepWeights2 = HexMetrics.TerraceLerp(MeshBuilder.Weights2, MeshBuilder.Weights3, i);

                MeshBuilder.AddTerrainTriangleUnperturbed(
                    convergencePoint, data.Center.Index, convergenceWeights,
                    stepVertex2,      data.Right .Index, stepWeights2,
                    stepVertex1,      data.Left  .Index, stepWeights1
                );
            }

            MeshBuilder.AddTerrainTriangleUnperturbed(
                convergencePoint,                 data.Center.Index, convergenceWeights,
                data.PerturbedLeftCorner, data.Right .Index, MeshBuilder.Weights3,
                stepVertex2,                      data.Left  .Index, stepWeights2
            );
        }

        //Rivered edge is between Center and Right. River endpoint is pointing at Left
        private void CreateRiverTrough_Endpoint(RiverData data) {
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
                    CreateRiverEndpointTrough_FlatCliff(data);
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
                    CreateRiverEndpointTrough_CliffFlat(data);
                }else if(leftRightEdgeType == HexEdgeType.Slope) {
                    CreateRiverEndpointTrough_CliffTerraces(data);
                }else if(leftRightEdgeType == HexEdgeType.Cliff) {
                    CreateRiverEndpointTrough_DoubleCliff(data);
                }
            }
        }

        //Rivered edge is between Center and Right. River endpoint is pointing at Left,
        //and Center/Left and Left/Right are both flat edges
        private void CreateRiverEndpointTrough_DoubleFlat(RiverData data) {
            MeshBuilder.AddTerrainTriangleUnperturbed(
                data.PerturbedLeftCorner,             data.Left  .Index, MeshBuilder.Weights1,
                data.PerturbedRightCorner,            data.Right .Index, MeshBuilder.Weights2,
                data.PerturbedCenterRightTroughPoint, data.Center.Index, MeshBuilder.Weights23
            );

            MeshBuilder.AddTerrainTriangleUnperturbed(
                data.PerturbedLeftCorner,             data.Left  .Index, MeshBuilder.Weights1,
                data.PerturbedCenterRightTroughPoint, data.Right .Index, MeshBuilder.Weights23,
                data.PerturbedCenterCorner,           data.Center.Index, MeshBuilder.Weights3
            );

            CreateRiverEndpointSurface_Default(data);
        }

        //Rivered edge is between Center and Right. River endpoint is pointing at Left,
        //and Center/Left and Left/Right are both terraced edges
        private void CreateRiverEndpointTrough_DoubleTerracesUp(RiverData data) {
            Vector3 toCenterTerraceVertexTwo  = HexMetrics.TerraceLerp(data.PerturbedLeftCorner, data.PerturbedCenterCorner, 1);
            Color   toCenterTerraceWeightsTwo = HexMetrics.TerraceLerp(MeshBuilder.Weights1,     MeshBuilder.Weights2,       1);

            Vector3 toRightTerraceVertexTwo  = HexMetrics.TerraceLerp(data.PerturbedLeftCorner, data.PerturbedRightCorner, 1);
            Color   toRightTerraceWeightsTwo = HexMetrics.TerraceLerp(MeshBuilder.Weights1,     MeshBuilder.Weights3,      1);

            //Builds the first triangle between the terraces
            MeshBuilder.AddTerrainTriangleUnperturbed(
                data.PerturbedLeftCorner, data.Left  .Index, MeshBuilder.Weights1,
                toRightTerraceVertexTwo,  data.Center.Index, toRightTerraceWeightsTwo,
                toCenterTerraceVertexTwo, data.Right .Index, toCenterTerraceWeightsTwo
            );

            //Builds the quads between the terraces
            for(int i = 2; i < HexMetrics.TerraceSteps; i++) {
                Vector3 toCenterTerraceVertexOne  = toCenterTerraceVertexTwo;
                Color   toCenterTerraceWeightsOne = toCenterTerraceWeightsTwo;

                Vector3 toRightTerraceVertexOne  = toRightTerraceVertexTwo;
                Color   toRightTerraceWeightsOne = toRightTerraceWeightsTwo;

                toCenterTerraceVertexTwo  = HexMetrics.TerraceLerp(data.PerturbedLeftCorner, data.PerturbedCenterCorner, i);
                toCenterTerraceWeightsTwo = HexMetrics.TerraceLerp(MeshBuilder.Weights1,     MeshBuilder.Weights2,       i);

                toRightTerraceVertexTwo  = HexMetrics.TerraceLerp(data.PerturbedLeftCorner, data.PerturbedRightCorner, i);
                toRightTerraceWeightsTwo = HexMetrics.TerraceLerp(MeshBuilder.Weights1,     MeshBuilder.Weights3,      i);

                MeshBuilder.AddTerrainTriangleUnperturbed(
                    toCenterTerraceVertexTwo, data.Left.Index,   toCenterTerraceWeightsTwo,
                    toCenterTerraceVertexOne, data.Center.Index, toCenterTerraceWeightsOne,
                    toRightTerraceVertexOne,  data.Right.Index,  toRightTerraceWeightsOne
                );

                MeshBuilder.AddTerrainTriangleUnperturbed(
                    toRightTerraceVertexOne,  data.Left  .Index, toRightTerraceWeightsOne,
                    toRightTerraceVertexTwo,  data.Center.Index, toRightTerraceWeightsTwo,
                    toCenterTerraceVertexTwo, data.Right .Index, toCenterTerraceWeightsTwo
                );
            }

            MeshBuilder.AddTerrainTriangleUnperturbed(
                data.PerturbedCenterCorner, data.Left  .Index, MeshBuilder.Weights2,
                toCenterTerraceVertexTwo,   data.Center.Index, toCenterTerraceWeightsTwo,
                data.PerturbedRightCorner,  data.Right .Index, MeshBuilder.Weights3
            );

            MeshBuilder.AddTerrainTriangleUnperturbed(
                toCenterTerraceVertexTwo,  data.Left  .Index, toCenterTerraceWeightsTwo,
                toRightTerraceVertexTwo,   data.Center.Index, toRightTerraceWeightsTwo,
                data.PerturbedRightCorner, data.Right .Index, MeshBuilder.Weights3
            );

            //Draws the triangle below all the terraces
            MeshBuilder.AddTerrainTriangleUnperturbed(
                data.PerturbedCenterCorner,           data.Center.Index, MeshBuilder.Weights1,
                data.PerturbedRightCorner,            data.Right .Index, MeshBuilder.Weights2,
                data.PerturbedCenterRightTroughPoint, data.Left  .Index, MeshBuilder.Weights12
            );
        }

        //This converges all of the terraced edges to the center trough of the river.
        //Consider figuring out a better policy.
        private void CreateRiverEndpointTrough_DoubleTerracesDown(RiverData data) {
            Vector3 leftCenterTerracePointTwo = HexMetrics.TerraceLerp(
                data.PerturbedLeftCorner, data.PerturbedCenterCorner, 1
            );
            Color leftCenterTerraceWeightsTwo = HexMetrics.TerraceLerp(
                MeshBuilder.Weights1, MeshBuilder.Weights3, 1
            );

            Vector3 leftRightTerracePointTwo = HexMetrics.TerraceLerp(
                data.PerturbedLeftCorner, data.PerturbedRightCorner, 1
            );
            Color leftRightTerraceWeightsTwo = HexMetrics.TerraceLerp(
                MeshBuilder.Weights1, MeshBuilder.Weights2, 1
            );

            MeshBuilder.AddTerrainTriangleUnperturbed(
                data.PerturbedLeftCorner,             data.Left  .Index, MeshBuilder.Weights1,
                data.PerturbedCenterRightTroughPoint, data.Right .Index, MeshBuilder.Weights23,
                leftCenterTerracePointTwo,            data.Center.Index, leftCenterTerraceWeightsTwo
            );

            MeshBuilder.AddTerrainTriangleUnperturbed(
                data.PerturbedLeftCorner,             data.Left  .Index, MeshBuilder.Weights1,
                leftRightTerracePointTwo,             data.Right .Index, leftRightTerraceWeightsTwo,
                data.PerturbedCenterRightTroughPoint, data.Center.Index, MeshBuilder.Weights23
            );

            for(int i = 2; i < HexMetrics.TerraceSteps; i++) {
                Vector3 leftCenterTerracePointOne   = leftCenterTerracePointTwo;
                Color   leftCenterTerraceWeightsOne = leftCenterTerraceWeightsTwo;

                Vector3 leftRightTerracePointOne   = leftRightTerracePointTwo;
                Color   leftRightTerraceWeightsOne = leftRightTerraceWeightsTwo;

                leftCenterTerracePointTwo = HexMetrics.TerraceLerp(
                    data.PerturbedLeftCorner, data.PerturbedCenterCorner, i
                );
                leftCenterTerraceWeightsTwo = HexMetrics.TerraceLerp(
                    MeshBuilder.Weights1, MeshBuilder.Weights3, i
                );

                leftRightTerracePointTwo = HexMetrics.TerraceLerp(
                    data.PerturbedLeftCorner, data.PerturbedRightCorner, i
                );
                leftRightTerraceWeightsTwo = HexMetrics.TerraceLerp(
                    MeshBuilder.Weights1, MeshBuilder.Weights2, i
                );

                MeshBuilder.AddTerrainTriangleUnperturbed(
                    leftCenterTerracePointOne,            data.Left  .Index, leftCenterTerraceWeightsOne,
                    data.PerturbedCenterRightTroughPoint, data.Right .Index, MeshBuilder.Weights23,
                    leftCenterTerracePointTwo,            data.Center.Index, leftCenterTerraceWeightsTwo
                );

                MeshBuilder.AddTerrainTriangleUnperturbed(
                    leftRightTerracePointOne,             data.Left  .Index, leftRightTerraceWeightsOne,
                    leftRightTerracePointTwo,             data.Right .Index, leftRightTerraceWeightsTwo,
                    data.PerturbedCenterRightTroughPoint, data.Center.Index, MeshBuilder.Weights23
                );
            }

            MeshBuilder.AddTerrainTriangleUnperturbed(
                leftCenterTerracePointTwo,            data.Left  .Index, leftCenterTerraceWeightsTwo,
                data.PerturbedCenterRightTroughPoint, data.Right .Index, MeshBuilder.Weights23,
                data.PerturbedCenterCorner,           data.Center.Index, MeshBuilder.Weights3
            );

            MeshBuilder.AddTerrainTriangleUnperturbed(
                leftRightTerracePointTwo,             data.Left  .Index, leftRightTerraceWeightsTwo,
                data.PerturbedRightCorner,            data.Right .Index, MeshBuilder.Weights2,
                data.PerturbedCenterRightTroughPoint, data.Center.Index, MeshBuilder.Weights23
            );
        }

        //Rivered edge is between Center and Right. River endpoint is pointing at Left,
        //and Center/Left and Left/Right are both cliffed edges
        private void CreateRiverEndpointTrough_DoubleCliff(RiverData data) {
            MeshBuilder.AddTerrainTriangleUnperturbed(
                data.PerturbedCenterCorner,           data.Center.Index, MeshBuilder.Weights1,
                data.PerturbedLeftCorner,             data.Left  .Index, MeshBuilder.Weights2,
                data.PerturbedCenterRightTroughPoint, data.Right .Index, MeshBuilder.Weights23
            );

            MeshBuilder.AddTerrainTriangleUnperturbed(
                data.PerturbedCenterRightTroughPoint, data.Center.Index, MeshBuilder.Weights23,
                data.PerturbedLeftCorner,             data.Left  .Index, MeshBuilder.Weights2,
                data.PerturbedRightCorner,            data.Right .Index, MeshBuilder.Weights3
            );
        }

        private void CreateRiverEndpointTrough_FlatTerraces(RiverData data) {
            //Adds the triangle opposite the terraced edge, equivalent to half of a flat endpoint
            MeshBuilder.AddTerrainTriangleUnperturbed(
                data.PerturbedLeftCorner,             data.Left  .Index, MeshBuilder.Weights1,
                data.PerturbedCenterRightTroughPoint, data.Right .Index, MeshBuilder.Weights23,
                data.PerturbedCenterCorner,           data.Center.Index, MeshBuilder.Weights3
            );

            var convergencePoint = Vector3.Lerp(
                data.PerturbedCenterRightTroughPoint, data.PerturbedRightCorner, 0.5f
            );

            var convergenceWeights = Color.Lerp(MeshBuilder.Weights3, MeshBuilder.Weights23, 0.5f);

            //Adds the bottom half of the right-facing side of the endpoint
            MeshBuilder.AddTerrainTriangleUnperturbed(
                data.PerturbedLeftCorner,             data.Left.Index,   MeshBuilder.Weights1,
                convergencePoint,                     data.Right.Index,  convergenceWeights,
                data.PerturbedCenterRightTroughPoint, data.Center.Index, MeshBuilder.Weights23
            );

            Vector3 terraceVertexTwo  = HexMetrics.TerraceLerp(data.PerturbedLeftCorner, data.PerturbedRightCorner, 1);
            Color   terraceWeightsTwo = HexMetrics.TerraceLerp(MeshBuilder.Weights1,     MeshBuilder.Weights2,      1);

            //Builds out the terrace convergence between Left and Right
            MeshBuilder.AddTerrainTriangleUnperturbed(
                data.PerturbedLeftCorner, data.Left  .Index, MeshBuilder.Weights1,
                terraceVertexTwo,         data.Right .Index, terraceWeightsTwo,
                convergencePoint,         data.Center.Index, convergenceWeights
            );

            for(int i = 2; i < HexMetrics.TerraceSteps; i++) {
                Vector3 terraceVertexOne  = terraceVertexTwo;
                Color   terraceWeightsOne = terraceWeightsTwo;

                terraceVertexTwo  = HexMetrics.TerraceLerp(data.PerturbedLeftCorner, data.PerturbedRightCorner, i);
                terraceWeightsTwo = HexMetrics.TerraceLerp(MeshBuilder.Weights1,     MeshBuilder.Weights2,      i);

                MeshBuilder.AddTerrainTriangleUnperturbed(
                    terraceVertexOne, data.Left  .Index, terraceWeightsOne,
                    terraceVertexTwo, data.Right .Index, terraceWeightsTwo,
                    convergencePoint, data.Center.Index, convergenceWeights
                );
            }

            MeshBuilder.AddTerrainTriangleUnperturbed(
                terraceVertexTwo,          data.Left  .Index, terraceWeightsTwo,
                data.PerturbedRightCorner, data.Right .Index, MeshBuilder.Weights2,
                convergencePoint,          data.Center.Index, convergenceWeights
            );

            CreateRiverEndpointSurface_Default(data);
        }

        private void CreateRiverEndpointTrough_TerracesFlat(RiverData data) {
            //Builds out the endpoint trough
            MeshBuilder.AddTerrainTriangleUnperturbed(
                data.PerturbedLeftCorner,             data.Left  .Index, MeshBuilder.Weights1,
                data.PerturbedRightCorner,            data.Right .Index, MeshBuilder.Weights2,
                data.PerturbedCenterRightTroughPoint, data.Center.Index, MeshBuilder.Weights23
            );

            var convergencePoint   = Vector3.Lerp(data.PerturbedCenterRightTroughPoint, data.PerturbedCenterCorner, 0.5f);
            var convergenceWeights = Color  .Lerp(MeshBuilder.Weights23,                MeshBuilder.Weights2,       0.5f);

            MeshBuilder.AddTerrainTriangleUnperturbed(
                data.PerturbedLeftCorner,             data.Left  .Index, MeshBuilder.Weights1,
                data.PerturbedCenterRightTroughPoint, data.Right .Index, MeshBuilder.Weights23,
                convergencePoint,                     data.Center.Index, convergenceWeights
            );

            //Builds out the terrace convergence about the endpoint trough
            Vector3 terracePointTwo   = HexMetrics.TerraceLerp(data.PerturbedLeftCorner, data.PerturbedCenterCorner, 1);
            Color   terraceWeightsTwo = HexMetrics.TerraceLerp(MeshBuilder.Weights1,     MeshBuilder.Weights3,       1);

            MeshBuilder.AddTerrainTriangleUnperturbed(
                terracePointTwo,          data.Left  .Index, terraceWeightsTwo,
                data.PerturbedLeftCorner, data.Right .Index, MeshBuilder.Weights1,
                convergencePoint,         data.Center.Index, convergenceWeights
            );

            for(int i = 2; i < HexMetrics.TerraceSteps; i++) {
                Vector3 terracePointOne   = terracePointTwo;
                Color   terraceWeightsOne = terraceWeightsTwo;

                terracePointTwo   = HexMetrics.TerraceLerp(data.PerturbedLeftCorner, data.PerturbedCenterCorner, i);
                terraceWeightsTwo = HexMetrics.TerraceLerp(MeshBuilder.Weights1,     MeshBuilder.Weights3,       i);

                MeshBuilder.AddTerrainTriangleUnperturbed(
                    terracePointTwo,  data.Left  .Index, terraceWeightsTwo,
                    terracePointOne,  data.Right .Index, terraceWeightsOne,
                    convergencePoint, data.Center.Index, convergenceWeights
                );
            }

            MeshBuilder.AddTerrainTriangleUnperturbed(
                data.PerturbedCenterCorner, data.Left  .Index, MeshBuilder.Weights3,
                terracePointTwo,            data.Right .Index, terraceWeightsTwo,
                convergencePoint,           data.Center.Index, convergenceWeights
            );

            CreateRiverEndpointSurface_Default(data);
        }

        private void CreateRiverEndpointTrough_FlatCliff(RiverData data) {
            Debug.LogWarning("CreateRiverEndpoint_FlatCliff is unimplemented");
        }

        private void CreateRiverEndpointTrough_CliffFlat(RiverData data) {
            Debug.LogWarning("CreateRiverEndpoint_CliffFlat is unimplemented");
        }

        //This method handles only the case when Left is below Center and Right, and thus the terraced
        //slope is going down towards Left. The only way the terrace could go up is if Right
        //were Deep Water, which would prevent the existence of a river between Center and Right.
        private void CreateRiverEndpointTrough_TerracesCliff(RiverData data) {
            //Builds the triangle that deals with the cliff edge attached to Right
            MeshBuilder.AddTerrainTriangleUnperturbed(
                data.PerturbedLeftCorner,             data.Left  .Index, MeshBuilder.Weights1,
                data.PerturbedRightCorner,            data.Right .Index, MeshBuilder.Weights2,
                data.PerturbedCenterRightTroughPoint, data.Center.Index, MeshBuilder.Weights23
            );

            //Builds out the terraced edges attached to Center by converging them to the
            //trough point opposite Left
            Vector3 terracePointTwo   = HexMetrics.TerraceLerp(data.PerturbedLeftCorner, data.PerturbedCenterCorner, 1);
            Color   terraceWeightsTwo = HexMetrics.TerraceLerp(MeshBuilder.Weights1,     MeshBuilder.Weights3,       1);

            MeshBuilder.AddTerrainTriangleUnperturbed(
                data.PerturbedLeftCorner,             data.Left  .Index, MeshBuilder.Weights1,
                data.PerturbedCenterRightTroughPoint, data.Right .Index, MeshBuilder.Weights23,
                terracePointTwo,                      data.Center.Index, terraceWeightsTwo
            );

            for(int i = 2; i < HexMetrics.TerraceSteps; i++) {
                Vector3 terracePointOne   = terracePointTwo;
                Color   terraceWeightsOne = terraceWeightsTwo;

                terracePointTwo   = HexMetrics.TerraceLerp(data.PerturbedLeftCorner, data.PerturbedCenterCorner, i);
                terraceWeightsTwo = HexMetrics.TerraceLerp(MeshBuilder.Weights1,     MeshBuilder.Weights3,       i);

                MeshBuilder.AddTerrainTriangleUnperturbed(
                    terracePointOne,                      data.Left  .Index, terraceWeightsOne,
                    data.PerturbedCenterRightTroughPoint, data.Right .Index, MeshBuilder.Weights23,
                    terracePointTwo,                      data.Center.Index, terraceWeightsTwo
                );
            }

            MeshBuilder.AddTerrainTriangleUnperturbed(
                terracePointTwo,                      data.Left  .Index, terraceWeightsTwo,
                data.PerturbedCenterRightTroughPoint, data.Right .Index, MeshBuilder.Weights23,
                data.PerturbedCenterCorner,           data.Center.Index, MeshBuilder.Weights3
            );
        }

        //This method handles only the case where Left is below Center and Right, and thus the
        //terraced slope is going down from Right to Left. The only way the terrace could go up is
        //if Right were Deep Water, which would prevent the existence of a river between Center and Right.
        private void CreateRiverEndpointTrough_CliffTerraces(RiverData data) {
            //Builds the triangle that deals with the cliff edge attached to Center
            MeshBuilder.AddTerrainTriangleUnperturbed(
                data.PerturbedLeftCorner,             data.Left  .Index, MeshBuilder.Weights1,
                data.PerturbedCenterRightTroughPoint, data.Right .Index, MeshBuilder.Weights23,
                data.PerturbedCenterCorner,           data.Center.Index, MeshBuilder.Weights3
            );

            //Builds out the terraced edges attached to Right by converging them to the
            //trough point opposite Left
            Vector3 terracePointTwo   = HexMetrics.TerraceLerp(data.PerturbedLeftCorner, data.PerturbedRightCorner, 1);
            Color   terraceWeightsTwo = HexMetrics.TerraceLerp(MeshBuilder.Weights1,     MeshBuilder.Weights2,      1);

            MeshBuilder.AddTerrainTriangleUnperturbed(
                data.PerturbedLeftCorner,             data.Left  .Index, MeshBuilder.Weights1,
                terracePointTwo,                      data.Right .Index, terraceWeightsTwo,
                data.PerturbedCenterRightTroughPoint, data.Center.Index, MeshBuilder.Weights23
            );

            for(int i = 2; i < HexMetrics.TerraceSteps; i++) {
                Vector3 terracePointOne   = terracePointTwo;
                Color   terraceWeightsOne = terraceWeightsTwo;

                terracePointTwo   = HexMetrics.TerraceLerp(data.PerturbedLeftCorner, data.PerturbedRightCorner, i);
                terraceWeightsTwo = HexMetrics.TerraceLerp(MeshBuilder.Weights1,     MeshBuilder.Weights2,      i);

                MeshBuilder.AddTerrainTriangleUnperturbed(
                    terracePointOne,                      data.Left  .Index, terraceWeightsOne,
                    terracePointTwo,                      data.Right .Index, terraceWeightsTwo,
                    data.PerturbedCenterRightTroughPoint, data.Center.Index, MeshBuilder.Weights23
                );
            }

            MeshBuilder.AddTerrainTriangleUnperturbed(
                terracePointTwo,                      data.Left  .Index, terraceWeightsTwo,
                data.PerturbedRightCorner,            data.Right .Index, MeshBuilder.Weights2,
                data.PerturbedCenterRightTroughPoint, data.Center.Index, MeshBuilder.Weights23
            );
        }

        private void CreateRiverEndpointSurface_Default(RiverData data) {
            var indices = new Vector3(data.Center.Index, data.Left.Index, data.Right.Index);

            Vector3 yAdjustedCenter = data.PerturbedCenterCorner,
                    yAdjustedLeft   = data.PerturbedLeftCorner,
                    yAdjustedRight  = data.PerturbedRightCorner;

            float riverY = Mathf.Min(data.Center.RiverSurfaceY, data.Right.RiverSurfaceY);

            yAdjustedCenter.y = riverY;
            yAdjustedLeft  .y = riverY;
            yAdjustedRight .y = riverY;

            if(RiverCanon.GetFlowOfRiverAtEdge(data.Center, data.Direction) == RiverFlow.Clockwise) {
                MeshBuilder.AddRiverTriangleUnperturbed(
                    yAdjustedCenter, yAdjustedLeft, yAdjustedRight,
                    new Vector2(1f, 0f), new Vector2(0.5f, -HexMetrics.RiverEndpointVMax),
                    new Vector2(0f, 0f), indices
                );
            }else {
                MeshBuilder.AddRiverTriangleUnperturbed(
                    yAdjustedCenter, yAdjustedLeft, yAdjustedRight,
                    new Vector2(0f, 0f), new Vector2(0.5f, HexMetrics.RiverEndpointVMax),
                    new Vector2(1f, 0f), indices
                );
            }
        }

        private EdgeVertices GetYAdjustedEdge(IHexCell cell, HexDirection direction) {
            var center = cell.transform.localPosition;

            var retval = new EdgeVertices(
                center + HexMetrics.GetSecondOuterSolidCorner(direction),
                center + HexMetrics.GetFirstOuterSolidCorner (direction)
            );

            retval.V1.y = cell.EdgeY;
            retval.V2.y = cell.EdgeY;
            retval.V3.y = cell.EdgeY;
            retval.V4.y = cell.EdgeY;
            retval.V5.y = cell.EdgeY;

            return retval;
        }

        private EdgeVertices GetRiverTroughEdge(EdgeVertices nearEdge, EdgeVertices farEdge, bool invertFarEdge) {
            var troughEdge = new EdgeVertices(
                (nearEdge.V1 + (invertFarEdge ? farEdge.V5 : farEdge.V1)) / 2f,
                (nearEdge.V2 + (invertFarEdge ? farEdge.V4 : farEdge.V2)) / 2f,
                (nearEdge.V3 + farEdge.V3)                                / 2f,
                (nearEdge.V4 + (invertFarEdge ? farEdge.V2 : farEdge.V4)) / 2f,
                (nearEdge.V5 + (invertFarEdge ? farEdge.V1 : farEdge.V5)) / 2f
            );

            troughEdge.V1.y = Mathf.Min(nearEdge.V1.y, (invertFarEdge ? farEdge.V5 : farEdge.V1).y) + HexMetrics.StreamBedElevationOffset;
            troughEdge.V2.y = Mathf.Min(nearEdge.V2.y, (invertFarEdge ? farEdge.V4 : farEdge.V2).y) + HexMetrics.StreamBedElevationOffset;
            troughEdge.V3.y = Mathf.Min(nearEdge.V3.y, farEdge.V3.y)                                + HexMetrics.StreamBedElevationOffset;
            troughEdge.V4.y = Mathf.Min(nearEdge.V4.y, (invertFarEdge ? farEdge.V2 : farEdge.V4).y) + HexMetrics.StreamBedElevationOffset;
            troughEdge.V5.y = Mathf.Min(nearEdge.V5.y, (invertFarEdge ? farEdge.V1 : farEdge.V5).y) + HexMetrics.StreamBedElevationOffset;

            return troughEdge;
        }

        #endregion

    }

}
