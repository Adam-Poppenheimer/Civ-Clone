using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using UnityCustomUtilities.Extensions;

namespace Assets.Simulation.HexMap {

    public class DifferentRiverTriangulator : IRiverTriangulator {

        #region instance fields and properties

        private IHexGrid            Grid;
        private IHexGridMeshBuilder MeshBuilder;
        private IRiverCanon         RiverCanon;
        private INoiseGenerator     NoiseGenerator;

        #endregion

        #region constructors

        [Inject]
        public DifferentRiverTriangulator(
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

        public void TriangulateConnectionAsRiver(
            HexDirection direction, IHexCell cell, EdgeVertices nearEdge
        ) {
            HexEdgeType thisToNeighborEdgeType, thisToPreviousNeighborEdgeType, previousNeighborToNeighborEdgeType;

            IHexCell neighbor         = Grid.GetNeighbor(cell, direction);
            IHexCell previousNeighbor = Grid.GetNeighbor(cell, direction.Previous());

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

                CreateRiverTroughSlope(direction, cell, nearEdge, neighbor, troughEdge);

                if(thisToPreviousNeighborEdgeType == HexEdgeType.River) {

                    if(previousNeighborToNeighborEdgeType == HexEdgeType.River) {
                        CreateRiverConfluenceSection(
                            direction, cell, neighbor, previousNeighbor,
                            nearEdge, farEdge, troughEdge
                        );
                    }else {
                        CreateRiverCurveCorner(cell, previousNeighbor, neighbor, direction, nearEdge);
                    }
                }else {
                    CreateRiverEndpoint(cell, neighbor, previousNeighbor);
                }
            }else if(thisToPreviousNeighborEdgeType == HexEdgeType.River) {

                if(previousNeighborToNeighborEdgeType == HexEdgeType.River) {
                    //CreateRiverCurveCorner(previousNeighbor, cell, neighbor, direction);
                }else {
                    CreateRiverEndpoint(previousNeighbor, cell, neighbor);
                }

            }else if(previousNeighborToNeighborEdgeType == HexEdgeType.River){
                CreateRiverEndpoint(neighbor, previousNeighbor, cell);
            }
        }

        private void CreateRiverTroughSlope(
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

        //Draws six triangles connecting the three inward-facing solid corners of the triangles, the
        //troughs of the rivers on all three edges, and the confluence point at the middle of the
        //intersecting rivers
        private void CreateRiverConfluenceSection(
            HexDirection direction, IHexCell focus, IHexCell neighbor, IHexCell previous,
            EdgeVertices focusNeighborEdge, EdgeVertices neighborFocusEdge, EdgeVertices focusNeighborTrough
        ){
            //We're building out the entire corner section, so we can ignore the corners
            //in the same way normal corner triangulation functions
            if(direction > HexDirection.E) {
                return;
            }

            var focusPreviousEdge    = GetYAdjustedEdge(focus, direction.Previous());
            var previousFocusEdge    = GetYAdjustedEdge(previous, direction.Previous().Opposite());
            var previousNeighborEdge = GetYAdjustedEdge(previous, direction.Previous().Opposite().Previous());
            var neighborPreviousEdge = GetYAdjustedEdge(neighbor, direction.Opposite().Next());

            var focusPreviousTrough    = GetRiverTroughEdge(focusPreviousEdge,    previousFocusEdge, true);
            var previousNeighborTrough = GetRiverTroughEdge(previousNeighborEdge, neighborPreviousEdge, true);

            Vector3 focusNeighborTroughPoint    = focusNeighborTrough.V1;
            Vector3 focusPreviousTroughPoint    = focusPreviousTrough.V1;
            Vector3 previousNeighborTroughPoint = previousNeighborTrough.V1;

            Vector3 confluencePoint = (focusNeighborTroughPoint + focusPreviousTroughPoint + previousNeighborTroughPoint) / 3f;            

            MeshBuilder.AddTerrainTriangleUnperturbed(
                NoiseGenerator.Perturb(focusNeighborEdge.V1,     focus.Shape == TerrainShape.Hills), focus.Index,    MeshBuilder.Weights1,
                NoiseGenerator.Perturb(focusPreviousTroughPoint, false),                             previous.Index, MeshBuilder.Weights12,
                NoiseGenerator.Perturb(confluencePoint,          false),                             neighbor.Index, MeshBuilder.Weights123
            );

            MeshBuilder.AddTerrainTriangleUnperturbed(
                NoiseGenerator.Perturb(focusNeighborEdge.V1,     focus.Shape == TerrainShape.Hills), focus.Index,    MeshBuilder.Weights1,
                NoiseGenerator.Perturb(confluencePoint,          false),                             previous.Index, MeshBuilder.Weights123,
                NoiseGenerator.Perturb(focusNeighborTroughPoint, false),                             neighbor.Index, MeshBuilder.Weights13
            );

            MeshBuilder.AddTerrainTriangleUnperturbed(
                NoiseGenerator.Perturb(previousFocusEdge.V5,     previous.Shape == TerrainShape.Hills), focus.Index,    MeshBuilder.Weights2,
                NoiseGenerator.Perturb(confluencePoint,          false),                                previous.Index, MeshBuilder.Weights123,
                NoiseGenerator.Perturb(focusPreviousTroughPoint, false),                                neighbor.Index, MeshBuilder.Weights12
            );

            MeshBuilder.AddTerrainTriangleUnperturbed(
                NoiseGenerator.Perturb(previousFocusEdge.V5,        previous.Shape == TerrainShape.Hills), focus.Index,    MeshBuilder.Weights2,
                NoiseGenerator.Perturb(previousNeighborTroughPoint, false),                                previous.Index, MeshBuilder.Weights23,
                NoiseGenerator.Perturb(confluencePoint,             false),                                neighbor.Index, MeshBuilder.Weights123
            );

            MeshBuilder.AddTerrainTriangleUnperturbed(
                NoiseGenerator.Perturb(neighborFocusEdge.V1,     neighbor.Shape == TerrainShape.Hills), focus.Index,    MeshBuilder.Weights3,
                NoiseGenerator.Perturb(focusNeighborTroughPoint, false),                                previous.Index, MeshBuilder.Weights13,
                NoiseGenerator.Perturb(confluencePoint,          false),                                neighbor.Index, MeshBuilder.Weights123
            );

            MeshBuilder.AddTerrainTriangleUnperturbed(
                NoiseGenerator.Perturb(neighborFocusEdge.V1,        neighbor.Shape == TerrainShape.Hills), focus.Index,    MeshBuilder.Weights3,
                NoiseGenerator.Perturb(confluencePoint,             false),                                previous.Index, MeshBuilder.Weights123,
                NoiseGenerator.Perturb(previousNeighborTroughPoint, false),                                neighbor.Index, MeshBuilder.Weights123
            );
        }

        //Non-rivered edge is between right and left. Outer river bank is opposite center
        private void CreateRiverCurveCorner(
            IHexCell center, IHexCell left, IHexCell right,
            HexDirection direction, EdgeVertices centerEdge
        ){
            CreateRiverInnerCurve(center, left, right, direction, centerEdge);

            //We've already taken care of River and Void edge types, so we don't need to check them again here
            HexEdgeType leftRightEdge = HexMetrics.GetEdgeType(left, right);

            if(leftRightEdge == HexEdgeType.Flat) {
                CreateRiverOuterCurveFlat(center, left, right, direction, centerEdge);
            }else if(leftRightEdge == HexEdgeType.Slope) {
                if(left.EdgeElevation < right.EdgeElevation) {
                    CreateRiverOuterCurveSloped_TerracesClockwiseUp(center, left, right, direction, centerEdge);
                }else {
                    CreateRiverOuterCurveSloped_TerracesClockwiseDown(center, left, right, direction, centerEdge);
                }                
            }else if(leftRightEdge == HexEdgeType.Cliff) {
                CreateRiverOuterCurveCliffed(center, left, right, direction, centerEdge);
            }
        }

        private void CreateRiverInnerCurve(
            IHexCell center, IHexCell left, IHexCell right,
            HexDirection direction, EdgeVertices centerRightEdge
        ){
            EdgeVertices centerLeftEdge  = GetYAdjustedEdge(center, direction.Previous());          

            EdgeVertices leftEdge  = GetYAdjustedEdge(left,  direction.Previous().Opposite());
            EdgeVertices rightEdge = GetYAdjustedEdge(right, direction.Opposite());

            EdgeVertices centerLeftTrough  = GetRiverTroughEdge(centerLeftEdge,  leftEdge,  true);
            EdgeVertices centerRightTrough = GetRiverTroughEdge(centerRightEdge, rightEdge, false);

            Vector3 centerPoint = centerRightEdge.V1;
            Vector3 centerLeftTroughPoint  = centerLeftTrough .V1;
            Vector3 centerRightTroughPoint = centerRightTrough.V1;

            MeshBuilder.AddTerrainTriangleUnperturbed(
                NoiseGenerator.Perturb(centerPoint,            center.Shape == TerrainShape.Hills), center.Index, MeshBuilder.Weights1,
                NoiseGenerator.Perturb(centerLeftTroughPoint,  false),                              left.Index,   MeshBuilder.Weights12,
                NoiseGenerator.Perturb(centerRightTroughPoint, false),                              right.Index,  MeshBuilder.Weights13
            );
        }

        //Non-rivered edge is between right and left. Outer river bank is opposite center
        private void CreateRiverOuterCurveFlat(
            IHexCell center, IHexCell left, IHexCell right,
            HexDirection direction, EdgeVertices centerRightEdge
        ){
            EdgeVertices centerLeftEdge  = GetYAdjustedEdge(center, direction.Previous());          

            EdgeVertices leftEdge  = GetYAdjustedEdge(left,  direction.Previous().Opposite());
            EdgeVertices rightEdge = GetYAdjustedEdge(right, direction.Opposite());

            EdgeVertices centerLeftTrough  = GetRiverTroughEdge(centerLeftEdge,  leftEdge,  true);
            EdgeVertices centerRightTrough = GetRiverTroughEdge(centerRightEdge, rightEdge, false);

            Vector3 leftPoint              = NoiseGenerator.Perturb(leftEdge.V5,  left.Shape  == TerrainShape.Hills);
            Vector3 rightPoint             = NoiseGenerator.Perturb(rightEdge.V1, right.Shape == TerrainShape.Hills);
            Vector3 centerLeftTroughPoint  = NoiseGenerator.Perturb(centerLeftTrough .V1);
            Vector3 centerRightTroughPoint = NoiseGenerator.Perturb(centerRightTrough.V1);

            MeshBuilder.AddTerrainTriangleUnperturbed(
                leftPoint,             left.Index,   MeshBuilder.Weights1,
                rightPoint,            right.Index,  MeshBuilder.Weights12,
                centerLeftTroughPoint, center.Index, MeshBuilder.Weights13
            );
            MeshBuilder.AddTerrainTriangleUnperturbed(
                centerLeftTroughPoint,  left.Index,   MeshBuilder.Weights13,
                rightPoint,             right.Index,  MeshBuilder.Weights2,
                centerRightTroughPoint, center.Index, MeshBuilder.Weights23
            );
        }

        //Non-rivered edge is between right and left. Outer river bank is opposite center
        private void CreateRiverOuterCurveSloped_TerracesClockwiseUp(
            IHexCell center, IHexCell left, IHexCell right, HexDirection direction,
            Vector3 leftPoint, Vector3 rightPoint,
            Vector3 leftTroughPoint, Vector3 rightTroughPoint
        ) {
            var lerpParam = 0.5f;

            var convergencePoint = Vector3.Lerp(rightTroughPoint, rightPoint, lerpParam);

            

            var convergenceWeights = Color.Lerp(MeshBuilder.Weights123, MeshBuilder.Weights2, lerpParam);

            //Quad between leftPoint, convergencePoint, rightTroughPoint, leftTroughPoint

            Vector3 v2 = NoiseGenerator.Perturb(HexMetrics.TerraceLerp(leftPoint, rightPoint, 1));
            Color w2 = HexMetrics.TerraceLerp(MeshBuilder.Weights1, MeshBuilder.Weights2, 1);

            MeshBuilder.AddTerrainTriangleUnperturbed(
                perturbedPrevious, left.Index,   MeshBuilder.Weights1,
                v2,                right.Index,  w2,
                convergencePoint,  center.Index, convergenceWeights
            );

            for(int i = 2; i < HexMetrics.TerraceSteps; i++) {
                Vector3 v1 = v2;
                Color w1 = w2;

                v2 = NoiseGenerator.Perturb(HexMetrics.TerraceLerp(previousPoint, farPoint, i));
                w2 = HexMetrics.TerraceLerp(MeshBuilder.Weights1, MeshBuilder.Weights2, i);

                MeshBuilder.AddTerrainTriangleUnperturbed(
                    v1,                previousNeighbor.Index, w1,
                    v2,                neighbor.Index,         w2,
                    convergencePoint,  cell.Index,             convergenceWeights
                );
            }

            MeshBuilder.AddTerrainTriangleUnperturbed(
                v2,               previousNeighbor.Index, w2,
                perturbedFar,     neighbor.Index,         MeshBuilder.Weights2,
                convergencePoint, cell.Index,             convergenceWeights
            );
        }

        //Non-rivered edge is between right and left. Outer river bank is opposite center
        private void CreateRiverOuterCurveSloped_TerracesClockwiseDown(
            IHexCell center, IHexCell left, IHexCell right,
            HexDirection direction, EdgeVertices centerEdge
        ) {
            
        }

        //Non-rivered edge is between right and left. Outer river bank is opposite center
        private void CreateRiverOuterCurveCliffed(
            IHexCell center, IHexCell left, IHexCell right,
            HexDirection direction, EdgeVertices centerEdge
        ) {
            
        }

        //Rivered edge is between leftCell and rightCell. River endpoint is pointing at endpointCell
        private void CreateRiverEndpoint(IHexCell leftCell, IHexCell rightCell, IHexCell endpointCell) {
            
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
