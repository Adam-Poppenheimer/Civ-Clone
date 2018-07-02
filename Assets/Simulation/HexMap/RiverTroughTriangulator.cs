using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

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
                MeshBuilder.Terrain
            );

            MeshBuilder.TriangulateEdgeStripUnperturbed(
                troughEdge, MeshBuilder.Weights12, data.Center.Index,
                farEdge,    MeshBuilder.Weights2,  data.Right .Index,
                MeshBuilder.Terrain
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
                MeshBuilder.Terrain
            );

            //Triangle pointing at left, which CenterRight river flows towards
            MeshBuilder.AddTriangleUnperturbed(
                data.PerturbedLeftCorner,   data.Center.Index, MeshBuilder.Weights2,
                data.LeftRightTroughPoint,  data.Left  .Index, MeshBuilder.Weights23,
                data.CenterLeftTroughPoint, data.Right .Index, MeshBuilder.Weights12,
                MeshBuilder.Terrain
            );

            //Triangle pointing at right, which CenterLeft river flows towards
            MeshBuilder.AddTriangleUnperturbed(
                data.PerturbedRightCorner,   data.Center.Index, MeshBuilder.Weights3,
                data.CenterRightTroughPoint, data.Left  .Index, MeshBuilder.Weights13,
                data.LeftRightTroughPoint,   data.Right .Index, MeshBuilder.Weights23,
                MeshBuilder.Terrain
            );

            //Triangle in the middle, touching each of the trough points and which all rivers flow towards
            MeshBuilder.AddTriangleUnperturbed(
                data.CenterLeftTroughPoint,  data.Center.Index, MeshBuilder.Weights12,
                data.LeftRightTroughPoint,   data.Left  .Index, MeshBuilder.Weights23,
                data.CenterRightTroughPoint, data.Right .Index, MeshBuilder.Weights13,
                MeshBuilder.Terrain
            );
        }

        //There is a river between Center and Right as well as Center and Left, but none
        //between Left and Right. We build only the inner edge of this curve here.
        public void CreateRiverTrough_Curve_InnerEdge(CellTriangulationData data) {            
            if( data.Center.EdgeElevation > data.Right.EdgeElevation &&
                data.Left.EdgeElevation > data.Right.EdgeElevation
            ) {
                //This case applies when the river curve results in a waterfall.
                //We divide the inside of the curve into two triangles so we can
                //give the waterfall more room to flow.
                var lowerCenterLeftTroughPoint  = data.CenterLeftTroughPoint;
                var lowerCenterRightTroughPoint = data.CenterRightTroughPoint;

                float lowerTroughPointY = Mathf.Min(data.CenterLeftTroughPoint.y, data.CenterRightTroughPoint.y);

                lowerCenterLeftTroughPoint.y = lowerCenterRightTroughPoint.y = lowerTroughPointY;
            
                MeshBuilder.AddTriangleUnperturbed(
                    data.PerturbedCenterCorner,  data.Center.Index, MeshBuilder.Weights1,
                    lowerCenterLeftTroughPoint,  data.Left  .Index, MeshBuilder.Weights12,
                    lowerCenterRightTroughPoint, data.Right .Index, MeshBuilder.Weights13,
                    MeshBuilder.Terrain
                );

                if(data.CenterLeftTroughPoint.y > data.CenterRightTroughPoint.y) {
                    MeshBuilder.AddTriangleUnperturbed(
                        data.PerturbedCenterCorner, data.Center.Index, MeshBuilder.Weights1,
                        data.CenterLeftTroughPoint, data.Left  .Index, MeshBuilder.Weights12,
                        lowerCenterLeftTroughPoint, data.Right .Index, MeshBuilder.Weights13,
                        MeshBuilder.Terrain
                    );
                }else if(data.CenterRightTroughPoint.y > data.CenterLeftTroughPoint.y) {
                    MeshBuilder.AddTriangleUnperturbed(
                        data.PerturbedCenterCorner,  data.Center.Index, MeshBuilder.Weights1,
                        lowerCenterRightTroughPoint, data.Left  .Index, MeshBuilder.Weights12,
                        data.CenterRightTroughPoint, data.Right .Index, MeshBuilder.Weights13,
                        MeshBuilder.Terrain
                    );
                }
            }else {
                //If our curve isn't a waterfall, we can build a triangle straight across
                //and make it look smoother.
                MeshBuilder.AddTriangleUnperturbed(
                    data.PerturbedCenterCorner,  data.Center.Index, MeshBuilder.Weights1,
                    data.CenterLeftTroughPoint,  data.Left  .Index, MeshBuilder.Weights12,
                    data.CenterRightTroughPoint, data.Right .Index, MeshBuilder.Weights13,
                    MeshBuilder.Terrain
                );
            }            
        }

        //Triangulates the outer edge of a curve. The non-rivered edge is between Left and Right.
        //The outer river bank is opposite center, and the edge between Left and Right is flat.
        public void CreateRiverTrough_Curve_OuterFlat(CellTriangulationData data) {
            MeshBuilder.AddTriangleUnperturbed(
                data.PerturbedLeftCorner,   data.Left  .Index, MeshBuilder.Weights1,
                data.PerturbedRightCorner,  data.Right .Index, MeshBuilder.Weights2,
                data.CenterLeftTroughPoint, data.Center.Index, MeshBuilder.Weights13,
                MeshBuilder.Terrain
            );
            MeshBuilder.AddTriangleUnperturbed(
                data.CenterLeftTroughPoint,  data.Left  .Index, MeshBuilder.Weights13,
                data.PerturbedRightCorner,   data.Right .Index, MeshBuilder.Weights2,
                data.CenterRightTroughPoint, data.Center.Index, MeshBuilder.Weights23,
                MeshBuilder.Terrain
            );
        }

        //Triangulates the outer edge of a curve. The non-rivered edge is between Left and Right.
        //The outer river bank is opposite center, and the edge between Left and Right is a slope,
        //where Right is above Left
        public void CreateRiverTrough_Curve_TerracesClockwiseUp(CellTriangulationData data) {
            //We need to converge the terraces to the convergence point, which is set at some point
            //between the right trough and the right point
            var convergencePoint = Vector3.Lerp(
                data.CenterRightTroughPoint, data.PerturbedRightCorner, HexMetrics.RiverSlopedCurveLerp
            );

            var convergenceWeights = Color.Lerp(
                MeshBuilder.Weights13, MeshBuilder.Weights3, HexMetrics.RiverSlopedCurveLerp
            );

            //Quad between leftPoint, convergencePoint, rightTroughPoint, leftTroughPoint,
            //which happens below the terrace convergence
            MeshBuilder.AddTriangleUnperturbed(
                data.CenterLeftTroughPoint,  data.Center.Index, MeshBuilder.Weights12,
                data.PerturbedLeftCorner,    data.Left  .Index, MeshBuilder.Weights2,
                data.CenterRightTroughPoint, data.Right .Index, MeshBuilder.Weights13,
                MeshBuilder.Terrain
            );

            MeshBuilder.AddTriangleUnperturbed(
                data.CenterRightTroughPoint, data.Center.Index, MeshBuilder.Weights13,
                data.PerturbedLeftCorner,    data.Left  .Index, MeshBuilder.Weights2,
                convergencePoint,            data.Right .Index, convergenceWeights,
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

        //Triangulates the outer edge of a curve. The non-rivered edge is between Left and Right.
        //The outer river bank is opposite center, and the edge between Left and Right is a slope,
        //where Right is below Left
        public void CreateRiverTrough_Curve_TerracesClockwiseDown(CellTriangulationData data) {
            //We need to converge the terraces to the convergence point, which is set 
            //to the trough point of the upper river
            var convergencePoint = Vector3.Lerp(data.PerturbedLeftCorner, data.PerturbedRightCorner, 0.35f);

            convergencePoint.y = data.PerturbedRightCorner.y;

            var convergenceWeights = MeshBuilder.Weights23;

            Vector3 stepVertex2 = NoiseGenerator.Perturb(
                HexMetrics.TerraceLerp(data.RightCorner, data.LeftCorner, 1)
            );

            Color stepWeights2 = HexMetrics.TerraceLerp(MeshBuilder.Weights2, MeshBuilder.Weights3, 1);

            //Performs the terrace convergence
            MeshBuilder.AddTriangleUnperturbed(
                convergencePoint,          data.Center.Index, convergenceWeights,
                stepVertex2,               data.Right .Index, stepWeights2,
                data.PerturbedRightCorner, data.Left  .Index, MeshBuilder.Weights2,
                MeshBuilder.Terrain
            );

            Vector3 stepVertex1;
            Color stepWeights1;
            int i = 2;

            for(; i < HexMetrics.TerraceSteps - 1; i++) {
                stepVertex1  = stepVertex2;
                stepWeights1 = stepWeights2;

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

            //We don't complete the terrace triangulation as normal, since this corner's got
            //wierd stuff going on. Instead we add a triangle from the last terrace point to
            //to the point on the port side of the trough where the upper river intersects it.
            float upperRiverSurfaceY = Mathf.Min(data.Center.RiverSurfaceY, data.Left.RiverSurfaceY);
            float portWaterIntersectionParam = 
                (upperRiverSurfaceY - data.PerturbedLeftCorner.y) /
                (data.CenterLeftTroughPoint.y - data.PerturbedLeftCorner.y);

            Vector3 portWaterIntersectionPoint = Vector3.Lerp(
                data.PerturbedLeftCorner, data.CenterLeftTroughPoint, portWaterIntersectionParam
            );

            Color portWaterIntersectionWeights = Color.Lerp(
                MeshBuilder.Weights3, MeshBuilder.Weights13, portWaterIntersectionParam
            );

            stepVertex1  = stepVertex2;
            stepWeights1 = stepWeights2;

            stepVertex2 = NoiseGenerator.Perturb(
                HexMetrics.TerraceLerp(data.RightCorner, data.LeftCorner, i)
            );

            stepWeights2 = HexMetrics.TerraceLerp(MeshBuilder.Weights2, MeshBuilder.Weights3, i);

            MeshBuilder.AddTriangleUnperturbed(
                convergencePoint,           data.Center.Index, convergenceWeights,
                portWaterIntersectionPoint, data.Right .Index, portWaterIntersectionWeights,
                stepVertex1,                data.Left  .Index, stepWeights1,
                MeshBuilder.Terrain
            );

            MeshBuilder.AddTriangleUnperturbed(
                portWaterIntersectionPoint, data.Center.Index, portWaterIntersectionWeights,
                stepVertex2,                data.Right .Index, stepWeights2,
                stepVertex1,                data.Left  .Index, stepWeights1,
                MeshBuilder.Terrain
            );

            MeshBuilder.AddTriangleUnperturbed(
                portWaterIntersectionPoint, data.Center.Index, portWaterIntersectionWeights,
                data.PerturbedLeftCorner,   data.Right .Index, MeshBuilder.Weights3,
                stepVertex2,                data.Left  .Index, stepWeights2,
                MeshBuilder.Terrain
            );

            //We define another point lined up with the convergence and just below our
            //water intersection point. Triangulating with this point in mind lets us
            //preserve river width and bank slope more easily.
            Vector3 portPointAlignedWithConvergence = portWaterIntersectionPoint;
            portPointAlignedWithConvergence.y = convergencePoint.y;

            MeshBuilder.AddTriangleUnperturbed(
                portPointAlignedWithConvergence, data.Center.Index, portWaterIntersectionWeights,
                portWaterIntersectionPoint,      data.Right .Index, portWaterIntersectionWeights,
                convergencePoint,                data.Left  .Index, convergenceWeights,
                MeshBuilder.Terrain
            );

            //Builds the triangle beneath the converged terraces that attaches to
            //CenterRightTroughPoint
            MeshBuilder.AddTriangleUnperturbed(
                data.CenterRightTroughPoint, data.Center.Index, MeshBuilder.Weights12,
                convergencePoint,            data.Right.Index,  convergenceWeights,
                data.PerturbedRightCorner,   data.Left.Index,   MeshBuilder.Weights2,
                MeshBuilder.Terrain
            );

            //Finishes converging the far bank by filling in triangles that mostly
            //appear below the water's surface
            MeshBuilder.AddTriangleUnperturbed(
                data.CenterRightTroughPoint,     data.Center.Index, MeshBuilder.Weights12,
                portPointAlignedWithConvergence, data.Right .Index, portWaterIntersectionWeights,
                convergencePoint,                data.Left  .Index, convergenceWeights,
                MeshBuilder.Terrain
            );

            //We need to use a similar convergence strategy as used on the inner curve, though
            //the lower trough point connects to nearby points more complexly
            var lowerCenterLeftTroughPoint  = data.CenterLeftTroughPoint;

            float lowerTroughPointY = Mathf.Min(data.CenterLeftTroughPoint.y, data.CenterRightTroughPoint.y);

            lowerCenterLeftTroughPoint.y = lowerTroughPointY;

            MeshBuilder.AddTriangleUnperturbed(
                data.CenterRightTroughPoint,     data.Center.Index, MeshBuilder.Weights12,
                lowerCenterLeftTroughPoint,      data.Right .Index, MeshBuilder.Weights13,
                portPointAlignedWithConvergence, data.Left  .Index, portWaterIntersectionWeights,
                MeshBuilder.Terrain
            );

            MeshBuilder.AddTriangleUnperturbed(
                lowerCenterLeftTroughPoint,      data.Center.Index, MeshBuilder.Weights13,
                data.CenterLeftTroughPoint,      data.Right .Index, MeshBuilder.Weights13,
                portPointAlignedWithConvergence, data.Left  .Index, portWaterIntersectionWeights,
                MeshBuilder.Terrain
            );
            
            MeshBuilder.AddTriangleUnperturbed(
                data.CenterLeftTroughPoint,      data.Center.Index, MeshBuilder.Weights13,
                portWaterIntersectionPoint,      data.Right .Index, portWaterIntersectionWeights,
                portPointAlignedWithConvergence, data.Left .Index,  portWaterIntersectionWeights,
                MeshBuilder.Terrain
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
                MeshBuilder.Terrain
            );

            MeshBuilder.AddTriangleUnperturbed(
                data.PerturbedLeftCorner,    data.Left  .Index, MeshBuilder.Weights1,
                data.CenterRightTroughPoint, data.Right .Index, MeshBuilder.Weights23,
                data.PerturbedCenterCorner,  data.Center.Index, MeshBuilder.Weights3,
                MeshBuilder.Terrain
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
                MeshBuilder.Terrain
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
                MeshBuilder.Terrain
            );

            Vector3 terraceVertexTwo  = HexMetrics.TerraceLerp(data.RightCorner,     data.LeftCorner,      1);
            Color   terraceWeightsTwo = HexMetrics.TerraceLerp(MeshBuilder.Weights2, MeshBuilder.Weights1, 1);

            //Builds out the terrace convergence between Left and Right
            MeshBuilder.AddTriangleUnperturbed(
                convergencePoint,                         data.Left  .Index, convergenceWeights,
                NoiseGenerator.Perturb(terraceVertexTwo), data.Right .Index, terraceWeightsTwo,
                data.PerturbedRightCorner,                data.Center.Index, MeshBuilder.Weights2,
                MeshBuilder.Terrain
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
                    MeshBuilder.Terrain
                );
            }

            MeshBuilder.AddTriangleUnperturbed(
                convergencePoint,                         data.Left  .Index, convergenceWeights,
                data.PerturbedLeftCorner,                 data.Right .Index, MeshBuilder.Weights1,
                NoiseGenerator.Perturb(terraceVertexTwo), data.Center.Index, terraceWeightsTwo,
                MeshBuilder.Terrain
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
                MeshBuilder.Terrain
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
                NoiseGenerator.Perturb(terraceVertexTwo), data.Left  .Index, convergenceWeights,
                data.PerturbedRightCorner,                data.Right .Index, MeshBuilder.Weights2,
                convergencePoint,                         data.Center.Index, terraceWeightsTwo,
                MeshBuilder.Terrain
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
                MeshBuilder.Terrain
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
                MeshBuilder.Terrain
            );

            //Builds out the terrace convergence from the terraced slope on Center/Left
            //to the convergence point between CenterRightTrough and CenterCorner
            Vector3 terracePointTwo   = HexMetrics.TerraceLerp(data.LeftCorner,      data.CenterCorner,    1);
            Color   terraceWeightsTwo = HexMetrics.TerraceLerp(MeshBuilder.Weights1, MeshBuilder.Weights3, 1);

            MeshBuilder.AddTriangleUnperturbed(
                NoiseGenerator.Perturb(terracePointTwo), data.Left  .Index, terraceWeightsTwo,
                data.PerturbedLeftCorner,                data.Right .Index, MeshBuilder.Weights1,
                convergencePoint,                        data.Center.Index, convergenceWeights,
                MeshBuilder.Terrain
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
                    MeshBuilder.Terrain
                );
            }

            MeshBuilder.AddTriangleUnperturbed(
                data.PerturbedCenterCorner,              data.Left  .Index, MeshBuilder.Weights3,
                NoiseGenerator.Perturb(terracePointTwo), data.Right .Index, terraceWeightsTwo,
                convergencePoint,                        data.Center.Index, convergenceWeights,
                MeshBuilder.Terrain
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
                MeshBuilder.Terrain
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
                MeshBuilder.Terrain
            );

            //Builds out the terrace convergence from the terraced slope on Center/Left
            //to the convergence point between CenterRightTrough and LeftCorner
            Vector3 terracePointTwo   = HexMetrics.TerraceLerp(data.CenterCorner,    data.LeftCorner ,     1);
            Color   terraceWeightsTwo = HexMetrics.TerraceLerp(MeshBuilder.Weights3, MeshBuilder.Weights1, 1);

            MeshBuilder.AddTriangleUnperturbed(
                data.PerturbedCenterCorner,              data.Left  .Index, MeshBuilder.Weights3,
                NoiseGenerator.Perturb(terracePointTwo), data.Right .Index, terraceWeightsTwo,
                convergencePoint,                        data.Center.Index, convergenceWeights,
                MeshBuilder.Terrain
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
                    MeshBuilder.Terrain
                );
            }

            MeshBuilder.AddTriangleUnperturbed(
                NoiseGenerator.Perturb(terracePointTwo), data.Left  .Index, terraceWeightsTwo,
                data.PerturbedLeftCorner,                data.Right .Index, MeshBuilder.Weights1,
                convergencePoint,                        data.Center.Index, convergenceWeights,
                MeshBuilder.Terrain
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
                MeshBuilder.Terrain
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
                    MeshBuilder.Terrain
                );
            }

            MeshBuilder.AddQuadUnperturbed(
                NoiseGenerator.Perturb(toRightTerraceVertexTwo),  toRightTerraceWeightsTwo,
                NoiseGenerator.Perturb(toCenterTerraceVertexTwo), toCenterTerraceWeightsTwo,
                data.PerturbedRightCorner,                        MeshBuilder.Weights2,
                data.PerturbedCenterCorner,                       MeshBuilder.Weights3,
                data.Left.Index, data.Right.Index, data.Center.Index,
                MeshBuilder.Terrain
            );

            //Draws the triangle that the river collides into.
            MeshBuilder.AddTriangleUnperturbed(
                data.PerturbedCenterCorner,  data.Left  .Index, MeshBuilder.Weights3,
                data.PerturbedRightCorner,   data.Right .Index, MeshBuilder.Weights2,
                data.CenterRightTroughPoint, data.Center.Index, MeshBuilder.Weights23,
                MeshBuilder.Terrain
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
        public void CreateRiverTrough_Endpoint_CliffTerraces(CellTriangulationData data) {
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

        //Rivered edge is between Center and Right. River endpoint is pointing at Left,
        //and Center/Left and Left/Right are both cliffed edges
        public void CreateRiverTrough_Endpoint_DoubleCliff(CellTriangulationData data) {
            MeshBuilder.AddTriangleUnperturbed(
                data.PerturbedCenterCorner,  data.Center.Index, MeshBuilder.Weights1,
                data.PerturbedLeftCorner,    data.Left  .Index, MeshBuilder.Weights2,
                data.CenterRightTroughPoint, data.Right .Index, MeshBuilder.Weights23,
                MeshBuilder.Terrain
            );

            MeshBuilder.AddTriangleUnperturbed(
                data.CenterRightTroughPoint, data.Center.Index, MeshBuilder.Weights23,
                data.PerturbedLeftCorner,    data.Left  .Index, MeshBuilder.Weights2,
                data.PerturbedRightCorner,   data.Right .Index, MeshBuilder.Weights3,
                MeshBuilder.Terrain
            );
        }

        #endregion

        #endregion

    }

}
