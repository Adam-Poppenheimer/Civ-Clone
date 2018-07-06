using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Util;

namespace Assets.Simulation.HexMap {

    public class RiverTroughTriangulator : IRiverTroughTriangulator {

        #region instance fields and properties

        private IHexGridMeshBuilder MeshBuilder;
        private INoiseGenerator     NoiseGenerator;

        #endregion

        #region constructors

        [Inject]
        public RiverTroughTriangulator(
            IHexGridMeshBuilder meshBuilder, INoiseGenerator noiseGenerator
        ){
            MeshBuilder    = meshBuilder;
            NoiseGenerator = noiseGenerator;
        }

        #endregion

        #region instance methods

        #region from IRiverTroughTriangulationLogic

        //Creates a river trough between Center and Right, which dips down to the lowest
        //streambed elevation between Center and Right and connects to both of the
        //solid edges of those cells.
        public void CreateRiverTrough_Edge(CellTriangulationData data) {
            EdgeVertices nearEdge   = data.CenterToRightEdgePerturbed;
            EdgeVertices troughEdge = data.CenterRightTrough;
            EdgeVertices farEdge    = data.RightToCenterEdgePerturbed;

            MeshBuilder.TriangulateEdgeStripUnperturbed(
                nearEdge,   MeshBuilder.Weights1,  data.Center.Index,
                troughEdge, MeshBuilder.Weights12, data.Right.Index,
                MeshBuilder.SmoothTerrain
            );

            MeshBuilder.TriangulateEdgeStripUnperturbed(
                troughEdge, MeshBuilder.Weights12, data.Center.Index,
                farEdge,    MeshBuilder.Weights2,  data.Right .Index,
                MeshBuilder.SmoothTerrain
            );
        }

        //Draws four triangles: one to connect each of the troughs to their neighboring troughs,
        //and another between all three troughs
        public void CreateRiverTrough_Confluence(CellTriangulationData data) {
            //Triangle pointing at center, which LeftRight river flows towards
            MeshBuilder.AddTriangleUnperturbed(
                data.PerturbedCenterCorner,  data.Center.Index, MeshBuilder.Weights1,
                data.CenterLeftTroughPoint,  data.Left  .Index, MeshBuilder.Weights12,
                data.CenterRightTroughPoint, data.Right .Index, MeshBuilder.Weights13,
                MeshBuilder.SmoothTerrain
            );

            //Triangle pointing at left, which CenterRight river flows towards
            MeshBuilder.AddTriangleUnperturbed(
                data.PerturbedLeftCorner,   data.Center.Index, MeshBuilder.Weights2,
                data.LeftRightTroughPoint,  data.Left  .Index, MeshBuilder.Weights23,
                data.CenterLeftTroughPoint, data.Right .Index, MeshBuilder.Weights12,
                MeshBuilder.SmoothTerrain
            );

            //Triangle pointing at right, which CenterLeft river flows towards
            MeshBuilder.AddTriangleUnperturbed(
                data.PerturbedRightCorner,   data.Center.Index, MeshBuilder.Weights3,
                data.CenterRightTroughPoint, data.Left  .Index, MeshBuilder.Weights13,
                data.LeftRightTroughPoint,   data.Right .Index, MeshBuilder.Weights23,
                MeshBuilder.SmoothTerrain
            );

            //Triangle in the middle, touching each of the trough points and which all rivers flow towards
            MeshBuilder.AddTriangleUnperturbed(
                data.CenterLeftTroughPoint,  data.Center.Index, MeshBuilder.Weights12,
                data.LeftRightTroughPoint,   data.Left  .Index, MeshBuilder.Weights23,
                data.CenterRightTroughPoint, data.Right .Index, MeshBuilder.Weights13,
                MeshBuilder.SmoothTerrain
            );
        }

        //There is a river between Center and Right as well as Center and Left, but none
        //between Left and Right. We build only the inner edge of this curve here.
        public void CreateRiverTrough_Curve_InnerEdge(CellTriangulationData data) {            
            if( data.Center.EdgeElevation > data.Right.EdgeElevation &&
                data.Left  .EdgeElevation > data.Right.EdgeElevation
            ) {
                CreateRiverTrough_Curve_Inner_Down(data);

            }else if(
                data.Center.EdgeElevation > data.Left.EdgeElevation &&
                data.Right .EdgeElevation > data.Left.EdgeElevation
            ) {
                CreateRiverTrough_Curve_Inner_Up(data);

            }else {
                //If our curve isn't a waterfall, we can build a triangle straight across
                //and make it look smoother.
                CreateRiverTrough_Curve_Inner_Flat(data);
            }            
        }

        //There is a river between Center and Right as well as Center and Left, but none
        //between Left and Right. Center and Left have a higher EdgeElevation than Right
        private void CreateRiverTrough_Curve_Inner_Down(CellTriangulationData data) {
            //Normally we would draw a single triangle across this entire corner.
            //But since the waterfall needs to be clipped in such a way as to preserve
            //its width, we must use a more complicated solution here. We create a small
            //protrusion along the starboard bank of the waterfall to obscure it, making
            //sure to align this protrusion to the starboard side of the river corner.
            //We must also push back the inner edge to give the river corner more room to flow.
            var lowerCenterLeftTroughPoint  = data.CenterLeftTroughPoint;

            lowerCenterLeftTroughPoint.y = data.CenterRightTroughPoint.y;
            
            MeshBuilder.AddTriangleUnperturbed(
                data.PerturbedCenterCorner,  data.Center.Index, MeshBuilder.Weights1,
                lowerCenterLeftTroughPoint,  data.Left  .Index, MeshBuilder.Weights12,
                data.CenterRightTroughPoint, data.Right .Index, MeshBuilder.Weights13,
                MeshBuilder.JaggedTerrain
            );

            MeshBuilder.AddTriangleUnperturbed(
                lowerCenterLeftTroughPoint, data.Center.Index, MeshBuilder.Weights12,
                data.PerturbedCenterCorner, data.Left  .Index, MeshBuilder.Weights1,
                data.CenterLeftTroughPoint, data.Right .Index, MeshBuilder.Weights12,
                MeshBuilder.JaggedTerrain
            );

            //Creates the protrusion
            float upperRiverSurfaceY = Mathf.Min(data.Center.RiverSurfaceY, data.Left.RiverSurfaceY);

            float upperRiverIntersectionParam;
            Vector3 upperRiverIntersectionPoint = Vector3Extensions.GetPointBetweenWithY(
                data.PerturbedCenterCorner, data.CenterLeftTroughPoint,
                upperRiverSurfaceY, out upperRiverIntersectionParam
            );

            Color upperRiverIntersectionWeights = Color.Lerp(
                MeshBuilder.Weights1, MeshBuilder.Weights12, upperRiverIntersectionParam
            );

            float centerToBottomTroughParam;
            Vector3 centerToBottomTroughPoint = Vector3Extensions.GetPointBetweenWithY(
                data.PerturbedCenterCorner, lowerCenterLeftTroughPoint,
                upperRiverSurfaceY, out centerToBottomTroughParam
            );

            Color centerToBottomTroughWeights = Color.Lerp(
                MeshBuilder.Weights1, MeshBuilder.Weights12, centerToBottomTroughParam
            );

            float lowerRiverSurfaceY = Mathf.Min(data.Center.RiverSurfaceY, data.Right.RiverSurfaceY);

            float lowerRiverIntersectionParam;
            Vector3 lowerRiverIntersectionPoint = Vector3Extensions.GetPointBetweenWithY(
                data.PerturbedCenterCorner, lowerCenterLeftTroughPoint,
                lowerRiverSurfaceY, out lowerRiverIntersectionParam
            );

            Color lowerRiverIntersectionWeights = Color.Lerp(
                MeshBuilder.Weights1, MeshBuilder.Weights12, lowerRiverIntersectionParam
            );

            Vector3 offsetVector = (data.CenterRightTroughPoint - lowerCenterLeftTroughPoint).normalized *
                                   HexMetrics.CornerWaterfallTroughProtrusion;

            MeshBuilder.AddTriangleUnperturbed(
                lowerRiverIntersectionPoint + offsetVector, data.Center.Index, lowerRiverIntersectionWeights,
                centerToBottomTroughPoint   + offsetVector, data.Left  .Index, MeshBuilder.Weights1,
                upperRiverIntersectionPoint + offsetVector, data.Right .Index, upperRiverIntersectionWeights,
                MeshBuilder.JaggedTerrain
            );

            MeshBuilder.AddQuadUnperturbed(
                lowerRiverIntersectionPoint + offsetVector, lowerRiverIntersectionWeights,
                lowerRiverIntersectionPoint,                lowerRiverIntersectionWeights,
                upperRiverIntersectionPoint + offsetVector, upperRiverIntersectionWeights,
                upperRiverIntersectionPoint,                upperRiverIntersectionWeights,
                data.Center.Index, data.Left.Index, data.Right.Index,
                MeshBuilder.JaggedTerrain
            );

            MeshBuilder.AddTriangleUnperturbed(
                centerToBottomTroughPoint   + offsetVector, data.Center.Index, centerToBottomTroughWeights,
                data.PerturbedCenterCorner,                 data.Left  .Index, MeshBuilder.Weights1,
                upperRiverIntersectionPoint + offsetVector, data.Right .Index, upperRiverIntersectionWeights,
                MeshBuilder.JaggedTerrain
            );

            MeshBuilder.AddTriangleUnperturbed(
                upperRiverIntersectionPoint + offsetVector, data.Center.Index, upperRiverIntersectionWeights,
                data.PerturbedCenterCorner,                 data.Left  .Index, MeshBuilder.Weights1,
                upperRiverIntersectionPoint,                data.Right .Index, upperRiverIntersectionWeights,
                MeshBuilder.JaggedTerrain
            );
        }

        //There is a river between Center and Right as well as Center and Left, but none
        //between Left and Right. Center and Right have a higher EdgeElevation than Left
        private void CreateRiverTrough_Curve_Inner_Up(CellTriangulationData data) {
            //Normally we would draw a single triangle across this entire corner.
            //But since the waterfall needs to be clipped in such a way as to preserve
            //its width, we must use a more complicated solution here. We create a small
            //protrusion along the starboard bank of the waterfall to obscure it, making
            //sure to align this protrusion to the starboard side of the river corner.
            //We must also push back the inner edge to give the river corner more room to flow.
            var lowerCenterRightTroughPoint = data.CenterRightTroughPoint;

            lowerCenterRightTroughPoint.y = data.CenterLeftTroughPoint.y;

            MeshBuilder.AddTriangleUnperturbed(
                data.PerturbedCenterCorner,  data.Center.Index, MeshBuilder.Weights1,
                data.CenterLeftTroughPoint,  data.Left  .Index, MeshBuilder.Weights12,
                lowerCenterRightTroughPoint, data.Right .Index, MeshBuilder.Weights13,
                MeshBuilder.JaggedTerrain
            );

            MeshBuilder.AddTriangleUnperturbed(
                data.PerturbedCenterCorner,  data.Center.Index, MeshBuilder.Weights1,
                lowerCenterRightTroughPoint, data.Left  .Index, MeshBuilder.Weights13,
                data.CenterRightTroughPoint, data.Right .Index, MeshBuilder.Weights13,
                MeshBuilder.JaggedTerrain
            );

            //Creates the protrusion
            float upperRiverSurfaceY = Mathf.Min(data.Center.RiverSurfaceY, data.Right.RiverSurfaceY);

            float upperRiverIntersectionParam;
            Vector3 upperRiverIntersectionPoint = Vector3Extensions.GetPointBetweenWithY(
                data.PerturbedCenterCorner, data.CenterRightTroughPoint,
                upperRiverSurfaceY, out upperRiverIntersectionParam
            );

            Color upperRiverIntersectionWeights = Color.Lerp(
                MeshBuilder.Weights1, MeshBuilder.Weights13, upperRiverIntersectionParam
            );

            float centerToBottomTroughParam;
            Vector3 centerToBottomTroughPoint = Vector3Extensions.GetPointBetweenWithY(
                data.PerturbedCenterCorner, lowerCenterRightTroughPoint,
                upperRiverSurfaceY, out centerToBottomTroughParam
            );

            Color centerToBottomTroughWeights = Color.Lerp(
                MeshBuilder.Weights1, MeshBuilder.Weights13, centerToBottomTroughParam
            );

            float lowerRiverSurfaceY = Mathf.Min(data.Center.RiverSurfaceY, data.Left.RiverSurfaceY);

            float lowerRiverIntersectionParam;
            Vector3 lowerRiverIntersectionPoint = Vector3Extensions.GetPointBetweenWithY(
                data.PerturbedCenterCorner, lowerCenterRightTroughPoint,
                lowerRiverSurfaceY, out lowerRiverIntersectionParam
            );

            Color lowerRiverIntersectionWeights = Color.Lerp(
                MeshBuilder.Weights1, MeshBuilder.Weights12, lowerRiverIntersectionParam
            );

            Vector3 offsetVector = (data.CenterLeftTroughPoint - lowerCenterRightTroughPoint).normalized *
                                   HexMetrics.CornerWaterfallTroughProtrusion;

            MeshBuilder.AddTriangleUnperturbed(
                lowerRiverIntersectionPoint + offsetVector, data.Center.Index, lowerRiverIntersectionWeights,
                upperRiverIntersectionPoint + offsetVector, data.Left  .Index, upperRiverIntersectionWeights,
                centerToBottomTroughPoint   + offsetVector, data.Right .Index, centerToBottomTroughWeights,
                MeshBuilder.JaggedTerrain
            );

            MeshBuilder.AddQuadUnperturbed(
                lowerRiverIntersectionPoint,                lowerRiverIntersectionWeights,
                lowerRiverIntersectionPoint + offsetVector, lowerRiverIntersectionWeights,
                upperRiverIntersectionPoint,                upperRiverIntersectionWeights,
                upperRiverIntersectionPoint + offsetVector, upperRiverIntersectionWeights,
                data.Center.Index, data.Left.Index, data.Right.Index, MeshBuilder.JaggedTerrain
            );

            MeshBuilder.AddTriangleUnperturbed(
                upperRiverIntersectionPoint,                data.Center.Index, upperRiverIntersectionWeights,
                data.PerturbedCenterCorner,                 data.Left  .Index, MeshBuilder.Weights1,
                upperRiverIntersectionPoint + offsetVector, data.Right .Index, upperRiverIntersectionWeights,
                MeshBuilder.JaggedTerrain
            );

            MeshBuilder.AddTriangleUnperturbed(
                upperRiverIntersectionPoint + offsetVector, data.Center.Index, upperRiverIntersectionWeights,
                data.PerturbedCenterCorner,                 data.Left  .Index, MeshBuilder.Weights1,
                centerToBottomTroughPoint   + offsetVector, data.Right .Index, centerToBottomTroughWeights,
                MeshBuilder.JaggedTerrain
            );
        }

        //There is a river between Center and Right as well as Center and Left, but none
        //between Left and Right. Center, Left, and Right all have the same EdgeElevation
        private void CreateRiverTrough_Curve_Inner_Flat(CellTriangulationData data) {
            MeshBuilder.AddTriangleUnperturbed(
                data.PerturbedCenterCorner,  data.Center.Index, MeshBuilder.Weights1,
                data.CenterLeftTroughPoint,  data.Left  .Index, MeshBuilder.Weights12,
                data.CenterRightTroughPoint, data.Right .Index, MeshBuilder.Weights13,
                MeshBuilder.SmoothTerrain
            );
        }

        //Triangulates the outer edge of a curve. The non-rivered edge is between Left and Right.
        //The outer river bank is opposite center, and the edge between Left and Right is flat.
        public void CreateRiverTrough_Curve_OuterFlat(CellTriangulationData data) {
            MeshBuilder.AddTriangleUnperturbed(
                data.PerturbedLeftCorner,   MeshBuilder.Weights2,
                data.PerturbedRightCorner,  MeshBuilder.Weights3,
                data.CenterLeftTroughPoint, MeshBuilder.Weights12,
                data.StandardIndicies, MeshBuilder.SmoothTerrain
            );
            MeshBuilder.AddTriangleUnperturbed(
                data.CenterLeftTroughPoint,  MeshBuilder.Weights12,
                data.PerturbedRightCorner,   MeshBuilder.Weights3,
                data.CenterRightTroughPoint, MeshBuilder.Weights13,
                data.StandardIndicies, MeshBuilder.SmoothTerrain
            );
        }

        //Triangulates the outer edge of a curve. The non-rivered edge is between Left and Right.
        //The outer river bank is opposite center, and the edge between Left and Right is a slope,
        //where Right is above Left
        public void CreateRiverTrough_Curve_TerracesClockwiseUp(CellTriangulationData data) {
            var convergencePoint = Vector3.Lerp(
                data.PerturbedRightCorner, data.PerturbedLeftCorner, 0.35f
            );
            convergencePoint.y = data.PerturbedLeftCorner.y;

            var convergenceWeights = Color.Lerp(
                MeshBuilder.Weights3, MeshBuilder.Weights2, 0.35f
            );

            Vector3 stepVertex2 = NoiseGenerator.Perturb(
                HexMetrics.TerraceLerp(data.LeftCorner, data.RightCorner, 1)
            );

            Color stepWeights2 = HexMetrics.TerraceLerp(
                MeshBuilder.Weights2, MeshBuilder.Weights3, 1
            );

            Vector3 lowerCenterRightTroughPoint = data.CenterRightTroughPoint;
            lowerCenterRightTroughPoint.y = data.CenterLeftTroughPoint.y;

            //Performs the terrace convergence, which turns the terraces into a flat wall
            //along the outside of the curve
            MeshBuilder.AddTriangleUnperturbed(
                convergencePoint,         data.Center.Index, convergenceWeights,
                data.PerturbedLeftCorner, data.Left  .Index, MeshBuilder.Weights2,
                stepVertex2,              data.Right .Index, stepWeights2,
                MeshBuilder.JaggedTerrain
            );

            for(int i = 2; i < HexMetrics.TerraceSteps; i++) {
                Vector3 stepVertex1  = stepVertex2;
                Color   stepWeights1 = stepWeights2;

                stepVertex2 = NoiseGenerator.Perturb(
                    HexMetrics.TerraceLerp(data.LeftCorner, data.RightCorner, i)
                );

                stepWeights2 = HexMetrics.TerraceLerp(
                    MeshBuilder.Weights2, MeshBuilder.Weights3, i
                );

                MeshBuilder.AddTriangleUnperturbed(
                    convergencePoint, data.Center.Index, convergenceWeights,
                    stepVertex1,      data.Left  .Index, stepWeights1,
                    stepVertex2,      data.Right .Index, stepWeights2,
                    MeshBuilder.JaggedTerrain
                );
            }

            MeshBuilder.AddTriangleUnperturbed(
                convergencePoint,          data.Center.Index, convergenceWeights,
                stepVertex2,               data.Left  .Index, stepWeights2,
                data.PerturbedRightCorner, data.Right .Index, MeshBuilder.Weights3,
                MeshBuilder.JaggedTerrain
            );

            //We need to finish the upper side of the opposite wall, which requires an
            //additional triangle in excess of that needed for terrace convergence
            Vector3 rightCornerAlignedWithConvergence = data.PerturbedRightCorner;
            rightCornerAlignedWithConvergence.y = convergencePoint.y;

            MeshBuilder.AddTriangleUnperturbed(
                convergencePoint,                  data.Center.Index, convergenceWeights,
                data.PerturbedRightCorner,         data.Left  .Index, MeshBuilder.Weights3,
                rightCornerAlignedWithConvergence, data.Right .Index, MeshBuilder.Weights3,
                MeshBuilder.JaggedTerrain
            );

            //Creates the the vertical surface that connects the upper river's
            //trough to the corner's trough
            MeshBuilder.AddTriangleUnperturbed(
                data.PerturbedRightCorner,   data.Center.Index, MeshBuilder.Weights3,
                data.CenterRightTroughPoint, data.Left  .Index, MeshBuilder.Weights13,
                lowerCenterRightTroughPoint, data.Right .Index, MeshBuilder.Weights13,
                MeshBuilder.JaggedTerrain
            );

            MeshBuilder.AddTriangleUnperturbed(
                rightCornerAlignedWithConvergence, data.Center.Index, convergenceWeights,
                data.PerturbedRightCorner,         data.Left  .Index, MeshBuilder.Weights3,
                lowerCenterRightTroughPoint,       data.Right .Index, MeshBuilder.Weights13,
                MeshBuilder.JaggedTerrain
            );

            //Creates the lower outer section of the trough, lined up with the trough
            //between Center and Left.
            MeshBuilder.AddQuadUnperturbed(
                data.CenterLeftTroughPoint,        MeshBuilder.Weights12,
                lowerCenterRightTroughPoint,       MeshBuilder.Weights13,
                data.PerturbedLeftCorner,          MeshBuilder.Weights2,
                rightCornerAlignedWithConvergence, MeshBuilder.Weights3,
                data.Center.Index, data.Right.Index, data.Left.Index,
                MeshBuilder.JaggedTerrain
            );

            //To make sure the terrain mesh continues to obscure the outer edges of the river
            //(and thus preserves the river's width) we need to extend the upper trough
            //slightly. To do that, we need create a small protrusion from the terrain mesh
            //that covers the outer edge of the waterfall
            float upperRiverSurfaceY = Mathf.Min(data.Center.RiverSurfaceY, data.Right.RiverSurfaceY);

            float upperRiverIntersectionParam;
            Vector3 upperRiverIntersectionPoint = Vector3Extensions.GetPointBetweenWithY(
                data.PerturbedRightCorner, data.CenterRightTroughPoint, upperRiverSurfaceY,
                out upperRiverIntersectionParam
            );

            Color upperRiverIntersectionWeights = Color.Lerp(
                MeshBuilder.Weights3, MeshBuilder.Weights13, upperRiverIntersectionParam
            );

            Vector3 rightCornerAlignedWithUpperRiverSurface = data.PerturbedRightCorner;
            rightCornerAlignedWithUpperRiverSurface.y = upperRiverSurfaceY;

            float lowerRiverSurfaceY = Mathf.Min(data.Center.RiverSurfaceY, data.Left.RiverSurfaceY);

            float lowerRiverIntersectionParam;
            Vector3 lowerRiverIntersectionPoint = Vector3Extensions.GetPointBetweenWithY(
                rightCornerAlignedWithConvergence, lowerCenterRightTroughPoint, lowerRiverSurfaceY,
                out lowerRiverIntersectionParam
            );

            Color lowerRiverIntersectionWeights = Color.Lerp(
                MeshBuilder.Weights3, MeshBuilder.Weights13, lowerRiverIntersectionParam
            );

            Vector3 offsetVector = (data.PerturbedLeftCorner - rightCornerAlignedWithConvergence).normalized *
                                   HexMetrics.CornerWaterfallTroughProtrusion;

            MeshBuilder.AddQuadUnperturbed(
                rightCornerAlignedWithConvergence       + offsetVector, MeshBuilder.Weights3,
                lowerRiverIntersectionPoint             + offsetVector, lowerRiverIntersectionWeights,
                rightCornerAlignedWithUpperRiverSurface + offsetVector, MeshBuilder.Weights3,
                upperRiverIntersectionPoint             + offsetVector, upperRiverIntersectionWeights,
                data.Center.Index, data.Left.Index, data.Right.Index, MeshBuilder.JaggedTerrain
            );

            MeshBuilder.AddQuadUnperturbed(
                lowerRiverIntersectionPoint + offsetVector, lowerRiverIntersectionWeights,
                lowerRiverIntersectionPoint,                lowerRiverIntersectionWeights,
                upperRiverIntersectionPoint + offsetVector, upperRiverIntersectionWeights,
                upperRiverIntersectionPoint,                upperRiverIntersectionWeights,
                data.Center.Index, data.Left.Index, data.Right.Index, MeshBuilder.JaggedTerrain
            );

            MeshBuilder.AddTriangleUnperturbed(
                upperRiverIntersectionPoint + offsetVector, data.Center.Index, upperRiverIntersectionWeights,
                data.PerturbedRightCorner,                  data.Left  .Index, MeshBuilder.Weights3,
                upperRiverIntersectionPoint,                data.Right .Index, upperRiverIntersectionWeights,
                MeshBuilder.JaggedTerrain
            );

            MeshBuilder.AddTriangleUnperturbed(
                rightCornerAlignedWithUpperRiverSurface + offsetVector, data.Center.Index, MeshBuilder.Weights3,
                data.PerturbedRightCorner,                              data.Left  .Index, MeshBuilder.Weights3,
                upperRiverIntersectionPoint + offsetVector,             data.Right .Index, upperRiverIntersectionWeights,
                MeshBuilder.JaggedTerrain
            );
        }

        //Triangulates the outer edge of a curve. The non-rivered edge is between Left and Right.
        //The outer river bank is opposite center, and the edge between Left and Right is a slope,
        //where Right is below Left
        public void CreateRiverTrough_Curve_TerracesClockwiseDown(CellTriangulationData data) {
            //We need to converge the terraces into a flat cliff face. This cliff face
            //extends all the way down to Right's surface level so that we can maintain
            //a sloped bank on the outer edge
            var convergencePoint = Vector3.Lerp(
                data.PerturbedLeftCorner, data.PerturbedRightCorner, 0.35f
            );
            convergencePoint.y = data.PerturbedRightCorner.y;

            var convergenceWeights = Color.Lerp(
                MeshBuilder.Weights3, MeshBuilder.Weights2, 0.35f
            );

            Vector3 stepVertex2 = NoiseGenerator.Perturb(
                HexMetrics.TerraceLerp(data.RightCorner, data.LeftCorner, 1)
            );

            Color stepWeights2 = HexMetrics.TerraceLerp(
                MeshBuilder.Weights2, MeshBuilder.Weights3, 1
            );

            Vector3 lowerCenterLeftTroughPoint = data.CenterLeftTroughPoint;
            lowerCenterLeftTroughPoint.y = data.CenterRightTroughPoint.y;

            //Performs the terrace convergence, which turns the terraces into a flat wall
            //along the outside of the curve
            MeshBuilder.AddTriangleUnperturbed(
                convergencePoint,          data.Center.Index, convergenceWeights,
                stepVertex2,               data.Right .Index, stepWeights2,
                data.PerturbedRightCorner, data.Left  .Index, MeshBuilder.Weights2,
                MeshBuilder.JaggedTerrain
            );

            for(int i = 2; i < HexMetrics.TerraceSteps; i++) {
                Vector3 stepVertex1  = stepVertex2;
                Color   stepWeights1 = stepWeights2;

                stepVertex2 = NoiseGenerator.Perturb(
                    HexMetrics.TerraceLerp(data.RightCorner, data.LeftCorner, i)
                );

                stepWeights2 = HexMetrics.TerraceLerp(
                    MeshBuilder.Weights2, MeshBuilder.Weights3, i
                );

                MeshBuilder.AddTriangleUnperturbed(
                    convergencePoint, data.Center.Index, convergenceWeights,
                    stepVertex2,      data.Right .Index, stepWeights2,
                    stepVertex1,      data.Left  .Index, stepWeights1,
                    MeshBuilder.JaggedTerrain
                );
            }

            MeshBuilder.AddTriangleUnperturbed(
                convergencePoint,         data.Center.Index, convergenceWeights,
                data.PerturbedLeftCorner, data.Right .Index, MeshBuilder.Weights3,
                stepVertex2,              data.Left  .Index, stepWeights2,
                MeshBuilder.JaggedTerrain
            );

            //We need to finish the upper side of the opposite wall, which requires an
            //additional triangle in excess of that needed for terrace convergence
            Vector3 leftCornerAlignedWithConvergence = data.PerturbedLeftCorner;
            leftCornerAlignedWithConvergence.y = convergencePoint.y;

            MeshBuilder.AddTriangleUnperturbed(
                leftCornerAlignedWithConvergence, data.Center.Index, MeshBuilder.Weights3,
                data.PerturbedLeftCorner,         data.Right .Index, MeshBuilder.Weights3,
                convergencePoint,                 data.Left  .Index, convergenceWeights,
                MeshBuilder.JaggedTerrain
            );

            //Creates the the vertical surface that connects the upper river's
            //trough to the corner's trough
            MeshBuilder.AddTriangleUnperturbed(
                lowerCenterLeftTroughPoint, data.Center.Index, MeshBuilder.Weights13,
                data.CenterLeftTroughPoint, data.Right .Index, MeshBuilder.Weights13,
                data.PerturbedLeftCorner,   data.Left  .Index, MeshBuilder.Weights3,
                MeshBuilder.JaggedTerrain
            );

            MeshBuilder.AddTriangleUnperturbed(
                lowerCenterLeftTroughPoint,       data.Center.Index, MeshBuilder.Weights13,
                data.PerturbedLeftCorner,         data.Right .Index, MeshBuilder.Weights3,
                leftCornerAlignedWithConvergence, data.Left  .Index, MeshBuilder.Weights3,
                MeshBuilder.JaggedTerrain
            );

            //Creates the lower outer section of the trough, lined up with the trough
            //between Center and Right.
            MeshBuilder.AddQuadUnperturbed(
                lowerCenterLeftTroughPoint,       MeshBuilder.Weights13,
                data.CenterRightTroughPoint,      MeshBuilder.Weights12,
                leftCornerAlignedWithConvergence, MeshBuilder.Weights3,
                data.PerturbedRightCorner,        MeshBuilder.Weights2,
                data.Center.Index, data.Right.Index, data.Left.Index,
                MeshBuilder.JaggedTerrain
            );

            //To make sure the terrain mesh continues to obscure the outer edges of the river
            //(and thus preserves the river's width) we need to extend the upper trough
            //slightly. To do that, we need create a small protrusion from the terrain mesh
            //that covers the outer edge of the waterfall
            float upperRiverSurfaceY = Mathf.Min(data.Center.RiverSurfaceY, data.Left.RiverSurfaceY);

            float upperRiverIntersectionParam;
            Vector3 upperRiverIntersectionPoint = Vector3Extensions.GetPointBetweenWithY(
                data.PerturbedLeftCorner, data.CenterLeftTroughPoint, upperRiverSurfaceY,
                out upperRiverIntersectionParam
            );

            Color upperRiverIntersectionWeights = Color.Lerp(
                MeshBuilder.Weights3, MeshBuilder.Weights13, upperRiverIntersectionParam
            );

            Vector3 leftCornerAlignedWithUpperRiverSurface = data.PerturbedLeftCorner;
            leftCornerAlignedWithUpperRiverSurface.y = upperRiverSurfaceY;

            float lowerRiverSurfaceY = Mathf.Min(data.Center.RiverSurfaceY, data.Right.RiverSurfaceY);

            float lowerRiverIntersectionParam;
            Vector3 lowerRiverIntersectionPoint = Vector3Extensions.GetPointBetweenWithY(
                leftCornerAlignedWithConvergence, lowerCenterLeftTroughPoint, lowerRiverSurfaceY,
                out lowerRiverIntersectionParam
            );

            Color lowerRiverIntersectionWeights = Color.Lerp(
                MeshBuilder.Weights3, MeshBuilder.Weights13, lowerRiverIntersectionParam
            );

            Vector3 offsetVector = (data.PerturbedRightCorner - leftCornerAlignedWithConvergence).normalized *
                                   HexMetrics.CornerWaterfallTroughProtrusion;

            MeshBuilder.AddQuadUnperturbed(
                lowerRiverIntersectionPoint            + offsetVector, lowerRiverIntersectionWeights,
                leftCornerAlignedWithConvergence       + offsetVector, MeshBuilder.Weights3,
                upperRiverIntersectionPoint            + offsetVector, upperRiverIntersectionWeights,
                leftCornerAlignedWithUpperRiverSurface + offsetVector, MeshBuilder.Weights3,
                data.Center.Index, data.Right.Index, data.Left.Index, MeshBuilder.JaggedTerrain
            );

            MeshBuilder.AddQuadUnperturbed(
                lowerRiverIntersectionPoint,                lowerRiverIntersectionWeights,
                lowerRiverIntersectionPoint + offsetVector, lowerRiverIntersectionWeights,
                upperRiverIntersectionPoint,                upperRiverIntersectionWeights,
                upperRiverIntersectionPoint + offsetVector, upperRiverIntersectionWeights,
                data.Center.Index, data.Right.Index, data.Left.Index, MeshBuilder.JaggedTerrain
            );

            MeshBuilder.AddTriangleUnperturbed(
                upperRiverIntersectionPoint + offsetVector,            data.Center.Index, upperRiverIntersectionWeights,
                data.PerturbedLeftCorner,                              data.Right .Index, MeshBuilder.Weights3,
                leftCornerAlignedWithUpperRiverSurface + offsetVector, data.Left  .Index, MeshBuilder.Weights3,
                MeshBuilder.JaggedTerrain
            );

            MeshBuilder.AddTriangleUnperturbed(
                upperRiverIntersectionPoint,                data.Center.Index, upperRiverIntersectionWeights,
                data.PerturbedLeftCorner,                   data.Right .Index, MeshBuilder.Weights3,
                upperRiverIntersectionPoint + offsetVector, data.Left  .Index, upperRiverIntersectionWeights,
                MeshBuilder.JaggedTerrain
            );
        }

        //Rivered edge is between Center and Right. River endpoint is pointing at Left,
        //and Center/Left and Left/Right are both flat edges. This leads to a river running
        //into the back of a terraced slope.
        public void CreateRiverTrough_Endpoint_DoubleFlat(CellTriangulationData data) {
            MeshBuilder.AddTriangleUnperturbed(
                data.PerturbedLeftCorner,    data.Left  .Index, MeshBuilder.Weights1,
                data.PerturbedRightCorner,   data.Right .Index, MeshBuilder.Weights2,
                data.CenterRightTroughPoint, data.Center.Index, MeshBuilder.Weights23,
                MeshBuilder.SmoothTerrain
            );

            MeshBuilder.AddTriangleUnperturbed(
                data.PerturbedLeftCorner,    data.Left  .Index, MeshBuilder.Weights1,
                data.CenterRightTroughPoint, data.Right .Index, MeshBuilder.Weights23,
                data.PerturbedCenterCorner,  data.Center.Index, MeshBuilder.Weights3,
                MeshBuilder.SmoothTerrain
            );
        }

        //Rivered edge is between Center and Right. River endpoint is pointing at Left,
        //Center/Left is flat, and Left/Right is a terraced edge, with Left having a
        //higher elevation than Right
        public void CreateRiverTrough_Endpoint_FlatTerraces_ElevatedLeft(CellTriangulationData data) {
            //Adds the triangle between Center and Left opposite the terraced edge,
            //equivalent to half of a flat endpoint
            MeshBuilder.AddTriangleUnperturbed(
                data.PerturbedLeftCorner,             data.Left  .Index, MeshBuilder.Weights1,
                data.CenterRightTroughPoint, data.Right .Index, MeshBuilder.Weights23,
                data.PerturbedCenterCorner,           data.Center.Index, MeshBuilder.Weights3,
                MeshBuilder.JaggedTerrain
            );

            var convergencePoint = Vector3.Lerp(
                data.CenterRightTroughPoint, data.PerturbedLeftCorner, 0.5f
            );

            var convergenceWeights = Color.Lerp(MeshBuilder.Weights3, MeshBuilder.Weights23, 0.5f);

            //Adds the bottom half of the terraces converging towards the Center/Left edge
            MeshBuilder.AddTriangleUnperturbed(
                convergencePoint,            data.Left.Index,   convergenceWeights,
                data.PerturbedRightCorner,   data.Right.Index,  MeshBuilder.Weights2,
                data.CenterRightTroughPoint, data.Center.Index, MeshBuilder.Weights23,
                MeshBuilder.JaggedTerrain
            );

            Vector3 terraceVertexTwo  = HexMetrics.TerraceLerp(data.RightCorner,     data.LeftCorner,      1);
            Color   terraceWeightsTwo = HexMetrics.TerraceLerp(MeshBuilder.Weights2, MeshBuilder.Weights1, 1);

            //Builds out the terrace convergence between Left and Right
            MeshBuilder.AddTriangleUnperturbed(
                convergencePoint,                         data.Left  .Index, convergenceWeights,
                NoiseGenerator.Perturb(terraceVertexTwo), data.Right .Index, terraceWeightsTwo,
                data.PerturbedRightCorner,                data.Center.Index, MeshBuilder.Weights2,
                MeshBuilder.JaggedTerrain
            );

            for(int i = 2; i < HexMetrics.TerraceSteps; i++) {
                Vector3 terraceVertexOne  = terraceVertexTwo;
                Color   terraceWeightsOne = terraceWeightsTwo;

                terraceVertexTwo  = HexMetrics.TerraceLerp(data.RightCorner,     data.LeftCorner,      i);
                terraceWeightsTwo = HexMetrics.TerraceLerp(MeshBuilder.Weights2, MeshBuilder.Weights1, i);

                MeshBuilder.AddTriangleUnperturbed(
                    convergencePoint,                         data.Left  .Index, convergenceWeights,
                    NoiseGenerator.Perturb(terraceVertexTwo), data.Right .Index, terraceWeightsTwo,
                    NoiseGenerator.Perturb(terraceVertexOne), data.Center.Index, terraceWeightsOne,
                    MeshBuilder.JaggedTerrain
                );
            }

            MeshBuilder.AddTriangleUnperturbed(
                convergencePoint,                         data.Left  .Index, convergenceWeights,
                data.PerturbedLeftCorner,                 data.Right .Index, MeshBuilder.Weights1,
                NoiseGenerator.Perturb(terraceVertexTwo), data.Center.Index, terraceWeightsTwo,
                MeshBuilder.JaggedTerrain
            );
        }

        //Rivered edge is between Center and Right. River endpoint is pointing at Left,
        //Center/Left is flat, and Left/Right is a terraced edge, with Right having a
        //higher elevation than Left
        public void CreateRiverTrough_Endpoint_FlatTerraces_ElevatedRight(CellTriangulationData data) {
            //Adds the triangle between Center and Left opposite the terraced edge,
            //equivalent to half of a flat endpoint
            MeshBuilder.AddTriangleUnperturbed(
                data.PerturbedLeftCorner,    data.Left  .Index, MeshBuilder.Weights1,
                data.CenterRightTroughPoint, data.Right .Index, MeshBuilder.Weights23,
                data.PerturbedCenterCorner,  data.Center.Index, MeshBuilder.Weights3,
                MeshBuilder.JaggedTerrain
            );

            var convergencePoint = Vector3.Lerp(
                data.CenterRightTroughPoint, data.PerturbedRightCorner, 0.5f
            );

            var convergenceWeights = Color.Lerp(MeshBuilder.Weights3, MeshBuilder.Weights23, 0.5f);

            //Adds the bottom half of the terraces converging towards the Center/Left edge
            MeshBuilder.AddTriangleUnperturbed(
                data.PerturbedLeftCorner,    data.Left.Index,   MeshBuilder.Weights1,
                convergencePoint,            data.Right.Index,  convergenceWeights,
                data.CenterRightTroughPoint, data.Center.Index, MeshBuilder.Weights23,
                MeshBuilder.JaggedTerrain
            );

            Vector3 terraceVertexTwo  = HexMetrics.TerraceLerp(data.LeftCorner,      data.RightCorner,     1);
            Color   terraceWeightsTwo = HexMetrics.TerraceLerp(MeshBuilder.Weights1, MeshBuilder.Weights2, 1);

            //Builds out the terrace convergence between Left and Right
            MeshBuilder.AddTriangleUnperturbed(
                data.PerturbedLeftCorner,                 data.Left  .Index, MeshBuilder.Weights1,
                NoiseGenerator.Perturb(terraceVertexTwo), data.Right .Index, terraceWeightsTwo,
                convergencePoint,                         data.Center.Index, convergenceWeights,
                MeshBuilder.JaggedTerrain
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
                    MeshBuilder.JaggedTerrain
                );
            }

            MeshBuilder.AddTriangleUnperturbed(
                NoiseGenerator.Perturb(terraceVertexTwo), data.Left  .Index, convergenceWeights,
                data.PerturbedRightCorner,                data.Right .Index, MeshBuilder.Weights2,
                convergencePoint,                         data.Center.Index, terraceWeightsTwo,
                MeshBuilder.JaggedTerrain
            );
        }

        //Rivered edge is between Center and Right. River endpoint is pointing at left,
        //Center/Left is a terraced edge going down from Center to Left and Left/Right is flat
        public void CreateRiverTrough_Endpoint_TerracesFlat_ElevatedCenter(CellTriangulationData data) {
            //Builds out the flat, cliff-like surface opposite Center
            MeshBuilder.AddTriangleUnperturbed(
                data.PerturbedLeftCorner,    data.Left  .Index, MeshBuilder.Weights1,
                data.PerturbedRightCorner,   data.Right .Index, MeshBuilder.Weights2,
                data.CenterRightTroughPoint, data.Center.Index, MeshBuilder.Weights23,
                MeshBuilder.JaggedTerrain
            );

            //We want to converge Center/Left's terraces to the Center/Right edge, so we need
            //to set the convergence point to lie between CenterCorner and CenterRightTroughPoint
            var convergencePoint   = Vector3.Lerp(
                data.PerturbedCenterCorner, data.CenterRightTroughPoint, 0.5f
            );

            var convergenceWeights = Color.Lerp(MeshBuilder.Weights3, MeshBuilder.Weights2, 0.5f);

            //Builds out the triangle below the terraces
            MeshBuilder.AddTriangleUnperturbed(
                data.PerturbedLeftCorner,    data.Left  .Index, MeshBuilder.Weights1,
                data.CenterRightTroughPoint, data.Right .Index, MeshBuilder.Weights23,
                convergencePoint,            data.Center.Index, convergenceWeights,
                MeshBuilder.JaggedTerrain
            );

            //Builds out the terrace convergence from the terraced slope on Center/Left
            //to the convergence point between CenterRightTrough and CenterCorner
            Vector3 terracePointTwo   = HexMetrics.TerraceLerp(data.LeftCorner,      data.CenterCorner,    1);
            Color   terraceWeightsTwo = HexMetrics.TerraceLerp(MeshBuilder.Weights1, MeshBuilder.Weights3, 1);

            MeshBuilder.AddTriangleUnperturbed(
                NoiseGenerator.Perturb(terracePointTwo), data.Left  .Index, terraceWeightsTwo,
                data.PerturbedLeftCorner,                data.Right .Index, MeshBuilder.Weights1,
                convergencePoint,                        data.Center.Index, convergenceWeights,
                MeshBuilder.JaggedTerrain
            );

            for(int i = 2; i < HexMetrics.TerraceSteps; i++) {
                Vector3 terracePointOne   = terracePointTwo;
                Color   terraceWeightsOne = terraceWeightsTwo;

                terracePointTwo   = HexMetrics.TerraceLerp(data.LeftCorner,      data.CenterCorner,    i);
                terraceWeightsTwo = HexMetrics.TerraceLerp(MeshBuilder.Weights1, MeshBuilder.Weights3, i);

                MeshBuilder.AddTriangleUnperturbed(
                    NoiseGenerator.Perturb(terracePointTwo), data.Left  .Index, terraceWeightsTwo,
                    NoiseGenerator.Perturb(terracePointOne), data.Right .Index, terraceWeightsOne,
                    convergencePoint,                        data.Center.Index, convergenceWeights,
                    MeshBuilder.JaggedTerrain
                );
            }

            MeshBuilder.AddTriangleUnperturbed(
                data.PerturbedCenterCorner,              data.Left  .Index, MeshBuilder.Weights3,
                NoiseGenerator.Perturb(terracePointTwo), data.Right .Index, terraceWeightsTwo,
                convergencePoint,                        data.Center.Index, convergenceWeights,
                MeshBuilder.JaggedTerrain
            );
        }

        //Rivered edge is between Center and Right. River endpoint is pointing at left,
        //Center/Left is a terraced edge going up from Center to Left, and Left/Right is flat
        public void CreateRiverTrough_Endpoint_TerracesFlat_ElevatedLeft(CellTriangulationData data) {
            //Builds out the flat, cliff-like surface opposite Center
            MeshBuilder.AddTriangleUnperturbed(
                NoiseGenerator.Perturb(data.LeftCorner,  data.Left.RequiresYPerturb),  data.Left  .Index, MeshBuilder.Weights1,
                NoiseGenerator.Perturb(data.RightCorner, data.Right.RequiresYPerturb), data.Right .Index, MeshBuilder.Weights2,
                data.CenterRightTroughPoint,                                  data.Center.Index, MeshBuilder.Weights23,
                MeshBuilder.JaggedTerrain
            );

            //We want to converge Center/Left's terraces to the surface opposite Left, so we need
            //to find the midpoint between the trough and the left corner
            var convergencePoint = Vector3.Lerp(
                data.CenterRightTroughPoint, data.PerturbedLeftCorner, 0.5f
            );

            var convergenceWeights = Color.Lerp(MeshBuilder.Weights23, MeshBuilder.Weights1, 0.5f);

            //Adds the triangle below the terraces between Center and Left
            MeshBuilder.AddTriangleUnperturbed(
                data.PerturbedCenterCorner,           data.Left  .Index, MeshBuilder.Weights3,
                convergencePoint,                     data.Right .Index, convergenceWeights,
                data.CenterRightTroughPoint, data.Center.Index, MeshBuilder.Weights23,
                MeshBuilder.JaggedTerrain
            );

            //Builds out the terrace convergence from the terraced slope on Center/Left
            //to the convergence point between CenterRightTrough and LeftCorner
            Vector3 terracePointTwo   = HexMetrics.TerraceLerp(data.CenterCorner,    data.LeftCorner ,     1);
            Color   terraceWeightsTwo = HexMetrics.TerraceLerp(MeshBuilder.Weights3, MeshBuilder.Weights1, 1);

            MeshBuilder.AddTriangleUnperturbed(
                data.PerturbedCenterCorner,              data.Left  .Index, MeshBuilder.Weights3,
                NoiseGenerator.Perturb(terracePointTwo), data.Right .Index, terraceWeightsTwo,
                convergencePoint,                        data.Center.Index, convergenceWeights,
                MeshBuilder.JaggedTerrain
            );

            for(int i = 2; i < HexMetrics.TerraceSteps; i++) {
                Vector3 terracePointOne   = terracePointTwo;
                Color   terraceWeightsOne = terraceWeightsTwo;

                terracePointTwo   = HexMetrics.TerraceLerp(data.CenterCorner,    data.LeftCorner,      i);
                terraceWeightsTwo = HexMetrics.TerraceLerp(MeshBuilder.Weights3, MeshBuilder.Weights1, i);

                MeshBuilder.AddTriangleUnperturbed(
                    NoiseGenerator.Perturb(terracePointOne), data.Left  .Index, terraceWeightsOne,
                    NoiseGenerator.Perturb(terracePointTwo), data.Right .Index, terraceWeightsTwo,
                    convergencePoint,                        data.Center.Index, convergenceWeights,
                    MeshBuilder.JaggedTerrain
                );
            }

            MeshBuilder.AddTriangleUnperturbed(
                NoiseGenerator.Perturb(terracePointTwo), data.Left  .Index, terraceWeightsTwo,
                data.PerturbedLeftCorner,                data.Right .Index, MeshBuilder.Weights1,
                convergencePoint,                        data.Center.Index, convergenceWeights,
                MeshBuilder.JaggedTerrain
            );
        }

        //There's a river between Center and Right, and both Center and Right are
        //above Left, with slopes going down towards Left from both sides.
        //Left is assumed to be some sort of open water cell, and the river is flowing 
        //out into that open water. This method converges all of the terraced edges
        //to the center trough of the river.
        public void CreateRiverTrough_Endpoint_ShallowWaterRiverDelta(CellTriangulationData data) {
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
                data.PerturbedLeftCorner,                                 data.Left  .Index, MeshBuilder.Weights1,
                data.CenterRightTroughPoint,                              data.Right .Index, MeshBuilder.Weights23,
                NoiseGenerator.Perturb(leftCenterTerracePointTwo, false), data.Center.Index, leftCenterTerraceWeightsTwo,
                MeshBuilder.JaggedTerrain
            );

            MeshBuilder.AddTriangleUnperturbed(
                data.PerturbedLeftCorner,                                data.Left  .Index, MeshBuilder.Weights1,
                NoiseGenerator.Perturb(leftRightTerracePointTwo, false), data.Right .Index, leftRightTerraceWeightsTwo,
                data.CenterRightTroughPoint,                             data.Center.Index, MeshBuilder.Weights23,
                MeshBuilder.JaggedTerrain
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

                MeshBuilder.AddTriangleUnperturbed(
                    NoiseGenerator.Perturb(leftCenterTerracePointOne, false), data.Left  .Index, leftCenterTerraceWeightsOne,
                    data.CenterRightTroughPoint,                              data.Right .Index, MeshBuilder.Weights23,
                    NoiseGenerator.Perturb(leftCenterTerracePointTwo, false), data.Center.Index, leftCenterTerraceWeightsTwo,
                    MeshBuilder.JaggedTerrain
                );

                MeshBuilder.AddTriangleUnperturbed(
                    NoiseGenerator.Perturb(leftRightTerracePointOne, false), data.Left  .Index, leftRightTerraceWeightsOne,
                    NoiseGenerator.Perturb(leftRightTerracePointTwo, false), data.Right .Index, leftRightTerraceWeightsTwo,
                    data.CenterRightTroughPoint,                             data.Center.Index, MeshBuilder.Weights23,
                    MeshBuilder.JaggedTerrain
                );
            }

            MeshBuilder.AddTriangleUnperturbed(
                NoiseGenerator.Perturb(leftCenterTerracePointTwo, false), data.Left  .Index, leftCenterTerraceWeightsTwo,
                data.CenterRightTroughPoint,                              data.Right .Index, MeshBuilder.Weights23,
                data.PerturbedCenterCorner,                               data.Center.Index, MeshBuilder.Weights3,
                MeshBuilder.JaggedTerrain
            );

            MeshBuilder.AddTriangleUnperturbed(
                NoiseGenerator.Perturb(leftRightTerracePointTwo, false), data.Left  .Index, leftRightTerraceWeightsTwo,
                data.PerturbedRightCorner,                               data.Right .Index, MeshBuilder.Weights2,
                data.CenterRightTroughPoint,                             data.Center.Index, MeshBuilder.Weights23,
                MeshBuilder.JaggedTerrain
            );
        }

        //Rivered edge is between Center and Right. River endpoint is pointing at Left,
        //and Center/Left and Left/Right are both terraced edges, pointing either up or
        //down. This method simply connects the two terraces together and then creates a
        //vertical wall for the river to run into.
        public void CreateRiverTrough_Endpoint_DoubleTerraces(CellTriangulationData data) {
            Vector3 toCenterTerraceVertexTwo  = HexMetrics.TerraceLerp(data.LeftCorner,      data.CenterCorner,    1);
            Color   toCenterTerraceWeightsTwo = HexMetrics.TerraceLerp(MeshBuilder.Weights1, MeshBuilder.Weights3, 1);

            Vector3 toRightTerraceVertexTwo  = HexMetrics.TerraceLerp(data.LeftCorner,      data.RightCorner,     1);
            Color   toRightTerraceWeightsTwo = HexMetrics.TerraceLerp(MeshBuilder.Weights1, MeshBuilder.Weights2, 1);

            //Builds the first triangle between the terraces
            MeshBuilder.AddTriangleUnperturbed(
                data.PerturbedLeftCorner,                         data.Left  .Index, MeshBuilder.Weights1,
                NoiseGenerator.Perturb(toRightTerraceVertexTwo),  data.Right .Index, toRightTerraceWeightsTwo,
                NoiseGenerator.Perturb(toCenterTerraceVertexTwo), data.Center.Index, toCenterTerraceWeightsTwo,
                MeshBuilder.JaggedTerrain
            );

            //Builds the quads between the terraces
            for(int i = 2; i < HexMetrics.TerraceSteps; i++) {
                Vector3 toCenterTerraceVertexOne  = toCenterTerraceVertexTwo;
                Color   toCenterTerraceWeightsOne = toCenterTerraceWeightsTwo;

                Vector3 toRightTerraceVertexOne  = toRightTerraceVertexTwo;
                Color   toRightTerraceWeightsOne = toRightTerraceWeightsTwo;

                toCenterTerraceVertexTwo  = HexMetrics.TerraceLerp(data.LeftCorner,      data.CenterCorner,    i);
                toCenterTerraceWeightsTwo = HexMetrics.TerraceLerp(MeshBuilder.Weights1, MeshBuilder.Weights3, i);

                toRightTerraceVertexTwo  = HexMetrics.TerraceLerp(data.LeftCorner,      data.RightCorner,     i);
                toRightTerraceWeightsTwo = HexMetrics.TerraceLerp(MeshBuilder.Weights1, MeshBuilder.Weights2, i);

                MeshBuilder.AddQuad(
                    toRightTerraceVertexOne, toRightTerraceWeightsOne,
                    toCenterTerraceVertexOne, toCenterTerraceWeightsOne,
                    toRightTerraceVertexTwo, toRightTerraceWeightsTwo,
                    toCenterTerraceVertexTwo, toCenterTerraceWeightsTwo,
                    data.Left.Index, data.Right.Index, data.Center.Index,
                    MeshBuilder.JaggedTerrain
                );
            }

            MeshBuilder.AddQuadUnperturbed(
                NoiseGenerator.Perturb(toRightTerraceVertexTwo),  toRightTerraceWeightsTwo,
                NoiseGenerator.Perturb(toCenterTerraceVertexTwo), toCenterTerraceWeightsTwo,
                data.PerturbedRightCorner,                        MeshBuilder.Weights2,
                data.PerturbedCenterCorner,                       MeshBuilder.Weights3,
                data.Left.Index, data.Right.Index, data.Center.Index,
                MeshBuilder.JaggedTerrain
            );

            //Draws the triangle that the river collides into.
            MeshBuilder.AddTriangleUnperturbed(
                data.PerturbedCenterCorner,  data.Left  .Index, MeshBuilder.Weights3,
                data.PerturbedRightCorner,   data.Right .Index, MeshBuilder.Weights2,
                data.CenterRightTroughPoint, data.Center.Index, MeshBuilder.Weights23,
                MeshBuilder.JaggedTerrain
            );
        }

        //This method handles only the case when Left is below Center and Right, and thus the terraced
        //slope is going down towards Left. The only way the terrace could go up is if Right
        //were Deep Water, which would prevent the existence of a river between Center and Right.
        public void CreateRiverTrough_Endpoint_TerracesCliff(CellTriangulationData data) {
            //Builds the triangle that deals with the cliff edge attached to Right
            MeshBuilder.AddTriangle(
                data.LeftCorner,             data.Left  .Index, MeshBuilder.Weights1,
                data.RightCorner,            data.Right .Index, MeshBuilder.Weights2,
                data.CenterRightTroughPoint, data.Center.Index, MeshBuilder.Weights23,
                MeshBuilder.JaggedTerrain
            );

            //Builds out the terraced edges attached to Center by converging them to the
            //trough point opposite Left
            Vector3 terracePointTwo   = HexMetrics.TerraceLerp(data.LeftCorner,      data.CenterCorner,    1);
            Color   terraceWeightsTwo = HexMetrics.TerraceLerp(MeshBuilder.Weights1, MeshBuilder.Weights3, 1);

            MeshBuilder.AddTriangle(
                data.LeftCorner,             data.Left  .Index, MeshBuilder.Weights1,
                data.CenterRightTroughPoint, data.Right .Index, MeshBuilder.Weights23,
                terracePointTwo,             data.Center.Index, terraceWeightsTwo,
                MeshBuilder.JaggedTerrain
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
                    MeshBuilder.JaggedTerrain
                );
            }

            MeshBuilder.AddTriangle(
                terracePointTwo,             data.Left  .Index, terraceWeightsTwo,
                data.CenterRightTroughPoint, data.Right .Index, MeshBuilder.Weights23,
                data.CenterCorner,           data.Center.Index, MeshBuilder.Weights3,
                MeshBuilder.JaggedTerrain
            );
        }

        //This method handles only the case where Left is below Center and Right, and thus the
        //terraced slope is going down from Right to Left. The only way the terrace could go up is
        //if Right were Deep Water, which would prevent the existence of a river between Center and Right.
        public void CreateRiverTrough_Endpoint_CliffTerraces(CellTriangulationData data) {
            //Builds the triangle that deals with the cliff edge attached to Center
            MeshBuilder.AddTriangle(
                data.LeftCorner,             data.Left  .Index, MeshBuilder.Weights1,
                data.CenterRightTroughPoint, data.Right .Index, MeshBuilder.Weights23,
                data.CenterCorner,           data.Center.Index, MeshBuilder.Weights3,
                MeshBuilder.JaggedTerrain
            );

            //Builds out the terraced edges attached to Right by converging them to the
            //trough point opposite Left
            Vector3 terracePointTwo   = HexMetrics.TerraceLerp(data.LeftCorner,      data.RightCorner,     1);
            Color   terraceWeightsTwo = HexMetrics.TerraceLerp(MeshBuilder.Weights1, MeshBuilder.Weights2, 1);

            MeshBuilder.AddTriangle(
                data.LeftCorner,             data.Left  .Index, MeshBuilder.Weights1,
                terracePointTwo,             data.Right .Index, terraceWeightsTwo,
                data.CenterRightTroughPoint, data.Center.Index, MeshBuilder.Weights23,
                MeshBuilder.JaggedTerrain
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
                    MeshBuilder.JaggedTerrain
                );
            }

            MeshBuilder.AddTriangle(
                terracePointTwo,             data.Left  .Index, terraceWeightsTwo,
                data.RightCorner,            data.Right .Index, MeshBuilder.Weights2,
                data.CenterRightTroughPoint, data.Center.Index, MeshBuilder.Weights23,
                MeshBuilder.JaggedTerrain
            );
        }

        //Rivered edge is between Center and Right. River endpoint is pointing at Left,
        //and Center/Left and Left/Right are both cliffed edges
        public void CreateRiverTrough_Endpoint_DoubleCliff(CellTriangulationData data) {
            MeshBuilder.AddTriangleUnperturbed(
                data.PerturbedCenterCorner,  data.Center.Index, MeshBuilder.Weights1,
                data.PerturbedLeftCorner,    data.Left  .Index, MeshBuilder.Weights2,
                data.CenterRightTroughPoint, data.Right .Index, MeshBuilder.Weights23,
                MeshBuilder.JaggedTerrain
            );

            MeshBuilder.AddTriangleUnperturbed(
                data.CenterRightTroughPoint, data.Center.Index, MeshBuilder.Weights23,
                data.PerturbedLeftCorner,    data.Left  .Index, MeshBuilder.Weights2,
                data.PerturbedRightCorner,   data.Right .Index, MeshBuilder.Weights3,
                MeshBuilder.JaggedTerrain
            );
        }

        #endregion

        #endregion

    }

}
