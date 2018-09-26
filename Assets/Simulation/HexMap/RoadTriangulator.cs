using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

namespace Assets.Simulation.HexMap {

    public class RoadTriangulator : IRoadTriangulator {

        #region instance fields and properties

        private IHexGridMeshBuilder MeshBuilder;
        private INoiseGenerator     NoiseGenerator;
        private IHexMapRenderConfig RenderConfig;

        #endregion

        #region constructors

        [Inject]
        public RoadTriangulator(
            IHexGridMeshBuilder meshBuilder, INoiseGenerator noiseGenerator,
            IHexMapRenderConfig renderConfig
        ){
            MeshBuilder    = meshBuilder;
            NoiseGenerator = noiseGenerator;
            RenderConfig   = renderConfig;
        }

        #endregion

        #region instance methods

        #region from IRoadTriangulator

        public bool ShouldTriangulateRoads(CellTriangulationData data) {
            return data.Center.HasRoads;
        }

        public void TriangulateRoads(CellTriangulationData data) {
            if(data.Center.Shape == CellShape.Hills) {
                TriangulateCenterRoads_Hills(data);
            }else {
                TriangulateCenterRoads_NoHills(data);
            }

            if(data.Direction <= HexDirection.SE && data.HasRoadToRight) {
                switch(data.CenterToRightEdgeType) {
                    case HexEdgeType.Flat:  TriangulateRoadConnection_Flat (data); break;
                    case HexEdgeType.Slope: TriangulateRoadConnection_Slope(data); break;
                    case HexEdgeType.River: TriangulateRoadConnection_River(data); break;
                    default: break;
                }
            }
        }

        #endregion

        private void TriangulateCenterRoads_Hills(CellTriangulationData data) {
            //Similarly for the non-hill case, we create triangles to merge the road
            //towards the center. Here, we use the inner solid edge instead of the
            //outer solid edge and need to manually perturb the points to account
            //for the y perturbation that hills have. We also need to address each
            bool perturbY = data.Center.RequiresYPerturb;

            Vector2 interpolators = GetRoadInterpolators(data);

            Vector3 peak  = NoiseGenerator.Perturb(data.CenterPeak,                perturbY);
            Vector3 farV1 = NoiseGenerator.Perturb(data.CenterToRightInnerEdge.V1, perturbY);
            Vector3 farV2 = NoiseGenerator.Perturb(data.CenterToRightInnerEdge.V2, perturbY);
            Vector3 farV3 = NoiseGenerator.Perturb(data.CenterToRightInnerEdge.V3, perturbY);
            Vector3 farV4 = NoiseGenerator.Perturb(data.CenterToRightInnerEdge.V4, perturbY);
            Vector3 farV5 = NoiseGenerator.Perturb(data.CenterToRightInnerEdge.V5, perturbY);

            EdgeVertices fanEdge = new EdgeVertices(
                Vector3.Lerp(peak, farV1, interpolators.x),
                Vector3.Lerp(peak, farV2, Mathf.Lerp(interpolators.x, interpolators.y, 0.25f)),
                Vector3.Lerp(peak, farV3, Mathf.Lerp(interpolators.x, interpolators.y, 0.5f)),
                Vector3.Lerp(peak, farV4, Mathf.Lerp(interpolators.x, interpolators.y, 0.75f)),
                Vector3.Lerp(peak, farV5, interpolators.y)
            );

            Vector3 indices = new Vector3(data.Center.Index, 0f, 0f);

            //We need to triangulate away from the peak regardless of whether there's a road.
            //We use an edge fan to make sure nothing passes beneath the terrain mesh,
            //since hills have y-perturbation that could cause the traditionally-calculated
            //midpoint to fall beneath the surface of the ground. We need to perform UV
            //calculations differently for road-to-edge and non-road-to-edge cases, but
            //that's being handled in TriangulateRoadFan
            TriangulateRoadFan(peak, fanEdge, indices, data.HasRoadToRight);            

            if(data.HasRoadToRight) {
                //Converges the road fan to Center's inner solid edge.
                //We can't go straight to the outer solid edge because hills have steep slopes between
                //their inner and outer solid edges, so a straight shot would pass the road beneath
                //the terrain mesh. We also can't create a road segment here because it wouldn't
                //match the fan at the current end
                TriangulateRoadFanToSegment(
                    fanEdge, MeshBuilder.Weights1,
                    data.CenterToRightInnerEdge.V2, data.CenterToRightInnerEdge.V3,
                    data.CenterToRightInnerEdge.V4, MeshBuilder.Weights1, perturbY,
                    indices
                );

                //Creates a road segment from the inner solid edge of the cell
                //to the outer solid edge. This causes the effective width of the road
                //to change, since the inner solid edge is smaller, but it combines
                //with the inherent noisiness of the road texture acceptably and doesn't
                //look too bad
                TriangulateRoadSegment(
                    data.CenterToRightInnerEdge.V2, data.CenterToRightInnerEdge.V3, data.CenterToRightInnerEdge.V4, MeshBuilder.Weights1, data.Center.RequiresYPerturb,
                    data.CenterToRightEdge.V2,      data.CenterToRightEdge.V3,      data.CenterToRightEdge.V4,      MeshBuilder.Weights1, data.Center.RequiresYPerturb,
                    indices
                );
            }
        }

        //Draws roads from the center of a cell without hills to the
        //edge. We do this by drawing a triangle from the peak of Center
        //to about halfway across our triangular section. If there's a
        //road traveling through this edge, we also build a pair of quads
        //from the end of the triangle to the edge of the cell
        private void TriangulateCenterRoads_NoHills(CellTriangulationData data) {
            Vector2 interpolators = GetRoadInterpolators(data);

            Vector3 middleLeft   = Vector3.Lerp(data.CenterPeak, data.CenterToRightEdge.V1, interpolators.x);
            Vector3 middleRight  = Vector3.Lerp(data.CenterPeak, data.CenterToRightEdge.V5, interpolators.y);
            Vector3 middleCenter = Vector3.Lerp(middleLeft, middleRight, 0.5f);

            Vector3 indices = new Vector3(data.Center.Index, 0f, 0f);

            //We only perform the full road construction if we need to
            if(data.HasRoadToRight) {
                //Builds out the triangular wedge closer to the center of the cell,
                //using two triangles to make sure the UV is one at the center and zero
                //at the edges
                MeshBuilder.AddTriangle(
                    data.CenterPeak, MeshBuilder.Weights1, new Vector2(1f, 0f),
                    middleLeft,      MeshBuilder.Weights1, new Vector2(0f, 0f),
                    middleCenter,    MeshBuilder.Weights1, new Vector2(1f, 0f),
                    indices, MeshBuilder.Roads
                );

                MeshBuilder.AddTriangle(
                    data.CenterPeak, MeshBuilder.Weights1, new Vector2(1f, 0f),
                    middleCenter,    MeshBuilder.Weights1, new Vector2(1f, 0f),
                    middleRight,     MeshBuilder.Weights1, new Vector2(0f, 0f),
                    indices, MeshBuilder.Roads
                );

                //Extends the road to the edge of the cell
                TriangulateRoadSegment(
                    middleLeft,                middleCenter,              middleRight,               MeshBuilder.Weights1, data.Center.RequiresYPerturb,
                    data.CenterToRightEdge.V2, data.CenterToRightEdge.V3, data.CenterToRightEdge.V4, MeshBuilder.Weights1, data.Center.RequiresYPerturb,
                    indices
                );
            }else {
                //If we don't have roads to the edge, we still need to add
                //some smaller road segments in this direction to round out
                //the roads. This requires a single triangle between the peak,
                //middleLeft, and middleRight to make sure the UVs don't
                //create a road with spokes going in every direction
                MeshBuilder.AddTriangle(
                    data.CenterPeak, MeshBuilder.Weights1, new Vector2(1f, 0f),
                    middleLeft,      MeshBuilder.Weights1, new Vector2(0f, 0f),
                    middleRight,     MeshBuilder.Weights1, new Vector2(0f, 0f),
                    indices, MeshBuilder.Roads
                );
            }
        }

        private Vector2 GetRoadInterpolators(CellTriangulationData data) {
            Vector2 interpolators;

            if(data.HasRoadToRight) {
                interpolators.x = interpolators.y = 0.5f;
            }else {
                interpolators.x = data.HasRoadToLeft      ? 0.5f : 0.25f;
                interpolators.y = data.HasRoadToNextRight ? 0.5f : 0.25f;
            }

            return interpolators;
        }

        //Creates a pair of quads out of the middle three vertices on both sides
        //of the Center/Right edge.
        private void TriangulateRoadConnection_Flat(CellTriangulationData data) {
            var nearEdge = data.CenterToRightEdge;
            var farEdge  = data.RightToCenterEdge;

            TriangulateRoadSegment(
                nearEdge.V2, nearEdge.V3, nearEdge.V4, MeshBuilder.Weights1, data.Center.RequiresYPerturb,
                farEdge .V2, farEdge .V3, farEdge .V4, MeshBuilder.Weights2, data.Right .RequiresYPerturb,
                new Vector3(data.Center.Index, data.Right.Index, 0f)
            );
        }

        //Similar to generating terrain slopes, except we only make use
        //of the middle three vertices
        private void TriangulateRoadConnection_Slope(CellTriangulationData data) {
            EdgeVertices nearEdge = data.CenterToRightEdge;
            EdgeVertices farEdge  = data.RightToCenterEdge;

            EdgeVertices edgeTwo    = RenderConfig.TerraceLerp(nearEdge, farEdge, 1);
            Color        weightsTwo = RenderConfig.TerraceLerp(MeshBuilder.Weights1, MeshBuilder.Weights2, 1);

            Vector3 indices = new Vector3(data.Center.Index, data.Right.Index, 0f);

            TriangulateRoadSegment(
                nearEdge.V2, nearEdge.V3, nearEdge.V4, MeshBuilder.Weights1, data.Center.RequiresYPerturb,
                edgeTwo .V2, edgeTwo .V3, edgeTwo .V4, weightsTwo,           false,
                indices
            );

            for(int i = 2; i < RenderConfig.TerraceSteps; i++) {
                EdgeVertices edgeOne    = edgeTwo;
                Color        weightsOne = weightsTwo;

                edgeTwo    = RenderConfig.TerraceLerp(nearEdge, farEdge, i);
                weightsTwo = RenderConfig.TerraceLerp(MeshBuilder.Weights1, MeshBuilder.Weights2, i);

                TriangulateRoadSegment(
                    edgeOne.V2, edgeOne.V3, edgeOne.V4, weightsOne, false,
                    edgeTwo.V2, edgeTwo.V3, edgeTwo.V4, weightsTwo, false,
                    indices
                );
            }

            TriangulateRoadSegment(
                edgeTwo.V2, edgeTwo.V3, edgeTwo.V4, weightsTwo,           false,
                farEdge.V2, farEdge.V3, farEdge.V4, MeshBuilder.Weights2, data.Right.RequiresYPerturb,
                indices
            );
        }

        private void TriangulateRoadConnection_River(CellTriangulationData data) {
            
        }

        private void TriangulateRoadSegment(
            Vector3 nearV1, Vector3 nearV2, Vector3 nearV3, Color nearWeights, bool perturbNearY,
            Vector3 farV1,  Vector3 farV2,  Vector3 farV3,  Color farWeights,  bool perturbFarY,
            Vector3 indices
        ) {
            MeshBuilder.AddQuadUnperturbed(
                NoiseGenerator.Perturb(nearV1, perturbNearY), nearWeights, new Vector2(0f, 0f),
                NoiseGenerator.Perturb(nearV2, perturbNearY), nearWeights, new Vector2(1f, 0f),
                NoiseGenerator.Perturb(farV1,  perturbFarY),  farWeights,  new Vector2(0f, 0f),
                NoiseGenerator.Perturb(farV2,  perturbFarY),  farWeights,  new Vector2(1f, 0f),
                indices, MeshBuilder.Roads
            );

            MeshBuilder.AddQuadUnperturbed(
                NoiseGenerator.Perturb(nearV2, perturbNearY), nearWeights, new Vector2(1f, 0f),
                NoiseGenerator.Perturb(nearV3, perturbNearY), nearWeights, new Vector2(0f, 0f),
                NoiseGenerator.Perturb(farV2,  perturbFarY),  farWeights,  new Vector2(1f, 0f),
                NoiseGenerator.Perturb(farV3,  perturbFarY),  farWeights,  new Vector2(0f, 0f),
                indices, MeshBuilder.Roads
            );
        }

        private void TriangulateRoadFanToSegment(
            EdgeVertices fanEdge, Color fanWeights,
            Vector3 farV1, Vector3 farV2, Vector3 farV3, Color farWeights, bool perturbFarY,
            Vector3 indices
        ) {
            MeshBuilder.AddTriangleUnperturbed(
                fanEdge.V2,                                 fanWeights, new Vector2(0.5f, 0f),
                fanEdge.V1,                                 fanWeights, new Vector2(0f, 0f),
                NoiseGenerator.Perturb(farV1, perturbFarY), farWeights, new Vector2(0f, 0f),
                indices, MeshBuilder.Roads
            );

            MeshBuilder.AddQuadUnperturbed(
                fanEdge.V2, fanWeights, new Vector2(0.5f, 0f),
                fanEdge.V3, fanWeights, new Vector2(1f,   0f),
                NoiseGenerator.Perturb(farV1,  perturbFarY),  farWeights,  new Vector2(0f, 0f),
                NoiseGenerator.Perturb(farV2,  perturbFarY),  farWeights,  new Vector2(1f, 0f),
                indices, MeshBuilder.Roads
            );

            MeshBuilder.AddQuadUnperturbed(
                fanEdge.V3, fanWeights, new Vector2(1f,   0f),
                fanEdge.V4, fanWeights, new Vector2(0.5f, 0f),
                NoiseGenerator.Perturb(farV2,  perturbFarY),  farWeights,  new Vector2(1f, 0f),
                NoiseGenerator.Perturb(farV3,  perturbFarY),  farWeights,  new Vector2(0f, 0f),
                indices, MeshBuilder.Roads
            );

            MeshBuilder.AddTriangleUnperturbed(
                fanEdge.V5,                                 fanWeights, new Vector2(0f, 0f),
                fanEdge.V4,                                 fanWeights, new Vector2(0.5f, 0f),
                NoiseGenerator.Perturb(farV3, perturbFarY), farWeights, new Vector2(0f, 0f),
                indices, MeshBuilder.Roads
            );
        }

        private void TriangulateRoadFan(Vector3 center, EdgeVertices edge, Vector3 indices, bool hasRoadInDirection) {
            var weights = MeshBuilder.Weights1;

            MeshBuilder.AddTriangleUnperturbed(
                center,  weights, new Vector2(1f, 0f),
                edge.V1, weights, new Vector2(0f, 0f),
                edge.V2, weights, hasRoadInDirection ? new Vector2(0.5f, 0f) : new Vector2(0f, 0f),
                indices, MeshBuilder.Roads
            );
            MeshBuilder.AddTriangleUnperturbed(
                center,  weights, new Vector2(1f, 0f),
                edge.V2, weights, hasRoadInDirection ? new Vector2(0.5f, 0f) : new Vector2(0f, 0f),
                edge.V3, weights, hasRoadInDirection ? new Vector2(1f,   0f) : new Vector2(0f, 0f),
                indices, MeshBuilder.Roads
            );

            MeshBuilder.AddTriangleUnperturbed(
                center,  weights, new Vector2(1f, 0f),
                edge.V3, weights, hasRoadInDirection ? new Vector2(1f,   0f) : new Vector2(0f, 0f),
                edge.V4, weights, hasRoadInDirection ? new Vector2(0.5f, 0f) : new Vector2(0f, 0f),
                indices, MeshBuilder.Roads
            );

            MeshBuilder.AddTriangleUnperturbed(
                center,  weights, new Vector2(1f, 0f),
                edge.V4, weights, hasRoadInDirection ? new Vector2(0.5f, 0f) : new Vector2(0f, 0f),
                edge.V5, weights, hasRoadInDirection ? new Vector2(0f,   0f) : new Vector2(0f, 0f),
                indices, MeshBuilder.Roads
            );
        }

        #endregion

    }

}
