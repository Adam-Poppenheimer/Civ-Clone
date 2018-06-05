using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

namespace Assets.Simulation.HexMap {

    public class BasicTerrainTriangulator : IBasicTerrainTriangulator {

        #region instance fields and properties

        private IHexGrid            Grid;
        private IHexGridMeshBuilder MeshBuilder;
        private INoiseGenerator     NoiseGenerator;
        private HexMesh             Terrain;
        private HexMesh             Roads;

        #endregion

        #region constructors

        [Inject]
        public BasicTerrainTriangulator(
            IHexGrid grid, IHexGridMeshBuilder meshBuilder,
            INoiseGenerator noiseGenerator,
            [Inject(Id = "Terrain")] HexMesh terrain,
            [Inject(Id = "Roads")] HexMesh roads
        ) {
            Grid           = grid;
            MeshBuilder    = meshBuilder;
            NoiseGenerator = noiseGenerator;
            Terrain        = terrain;
            Roads          = roads;
        }

        #endregion

        #region instance methods

        #region from IBasicTerrainTriangulator

        public void TriangulateTerrainCenter(CellTriangulationData data){

            if(data.Center.Shape == TerrainShape.Hills) {
                TriangulateCenterWithHills(data);
            }else {
                TriangulateCenterWithoutHills(data);
            }
        }

        public bool ShouldTriangulateTerrainConnection(CellTriangulationData data) {
            return data.CenterToRightEdgeType != HexEdgeType.River && data.Direction <= HexDirection.SE;
        }

        public void TriangulateTerrainConnection(CellTriangulationData data) {
            if(data.Right == null) {
                return;
            }

            if(data.CenterToRightEdgeType == HexEdgeType.Slope) {
                TriangulateEdgeTerraces(
                    data.CenterToRightEdge, data.Center,
                    data.RightToCenterEdge, data.Right,
                    ShouldRoadBeRendered(data.Center, data.Right)
                );
            }else if(data.Center.Shape == TerrainShape.Hills || data.Right.Shape == TerrainShape.Hills){
                TriangulateEdgeStrip_Hills(
                    data.CenterToRightEdge, MeshBuilder.Weights1, data.Center.Index, data.Center.RequiresYPerturb,
                    data.RightToCenterEdge, MeshBuilder.Weights2, data.Right .Index, data.Right .RequiresYPerturb,
                    ShouldRoadBeRendered(data.Center, data.Right)
                );
            }else{
                MeshBuilder.TriangulateEdgeStrip(
                    data.CenterToRightEdge, MeshBuilder.Weights1, data.Center.Index,
                    data.RightToCenterEdge, MeshBuilder.Weights2, data.Right .Index,
                    ShouldRoadBeRendered(data.Center, data.Right)
                );
            }

            if( data.Direction > HexDirection.E || data.Left == null || data.IsRiverCorner) {
                return;
            }

            var leftOrientedData = MeshBuilder.GetTriangulationData(
                data.Left, data.Right, data.Center, data.Direction.Next2()
            );

            var rightOrientedData = MeshBuilder.GetTriangulationData(
                data.Right, data.Center, data.Left, data.Direction.Previous2()
            );

            if(data.Center.EdgeElevation <= data.Left.EdgeElevation) {
                if(data.Center.EdgeElevation <= data.Right.EdgeElevation) {
                    TriangulateCorner(data);
                }else {
                    TriangulateCorner(rightOrientedData);
                }
            }else if(data.Left.EdgeElevation <= data.Right.EdgeElevation) {
                TriangulateCorner(leftOrientedData);
            }else {
                TriangulateCorner(rightOrientedData);
            }
        }

        #endregion

        private void TriangulateCenterWithHills(CellTriangulationData data) {
            MeshBuilder.TriangulateEdgeFan(
                data.CenterPeak, data.CenterToRightInnerEdge,
                data.Center.Index, true
            );

            TriangulateEdgeStrip_Hills(
                data.CenterToRightInnerEdge, MeshBuilder.Weights1, data.Center.Index, true,
                data.CenterToRightEdge,      MeshBuilder.Weights1, data.Center.Index, true
            );
        }

        private void TriangulateCenterWithoutHills(CellTriangulationData data){
            MeshBuilder.TriangulateEdgeFan(
                data.CenterPeak, data.CenterToRightEdge, data.Center.Index
            );

            if(data.Center.HasRoads) {
                Vector2 interpolators = GetRoadInterpolators(data.Direction, data.Center);

                TriangulateRoad(
                    data.CenterPeak,
                    Vector3.Lerp(data.CenterPeak, data.CenterToRightEdge.V1, interpolators.x),
                    Vector3.Lerp(data.CenterPeak, data.CenterToRightEdge.V5, interpolators.y),
                    data.CenterToRightEdge, ShouldRoadBeRendered(data.Center, data.Right),
                    data.Center.Index
                );
            }
        }

        private Vector2 GetRoadInterpolators(HexDirection direction, IHexCell cell) {
            var previousNeighbor = Grid.GetNeighbor(cell, direction.Previous());
            var neighbor         = Grid.GetNeighbor(cell, direction);
            var nextNeighbor     = Grid.GetNeighbor(cell, direction.Next());

            Vector2 interpolators;
            if(ShouldRoadBeRendered(cell, neighbor)) {
                interpolators.x = interpolators.y = 0.5f;

            }else {
                interpolators.x = ShouldRoadBeRendered(cell, previousNeighbor) ? 0.5f : 0.25f;
                interpolators.y = ShouldRoadBeRendered(cell, nextNeighbor)     ? 0.5f : 0.25f;
            }
            return interpolators;
        }

        private void TriangulateRoad(
            Vector3 center, Vector3 middleLeft, Vector3 middleRight, EdgeVertices e,
            bool hasRoadThroughCellEdge, float index
        ) {
            if(hasRoadThroughCellEdge) {
                Vector3 middleCenter = Vector3.Lerp(middleLeft, middleRight, 0.5f);

                Vector3 indices;
                indices.x = indices.y = indices.z = index;

                MeshBuilder.TriangulateRoadSegment(
                    middleLeft, middleCenter, middleRight,
                    e.V2, e.V3, e.V4,
                    MeshBuilder.Weights1, MeshBuilder.Weights2, indices
                );

                Roads.AddTriangle(center, middleLeft, middleCenter);
                Roads.AddTriangle(center, middleCenter, middleRight);
                Roads.AddTriangleUV(
                    new Vector2(1f, 0f), new Vector2(0f, 0f), new Vector2(1f, 0f)
                );
                Roads.AddTriangleUV(
                    new Vector2(1f, 0f), new Vector2(1f, 0f), new Vector2(0f, 0f)
                );

                Roads.AddTriangleCellData(indices, MeshBuilder.Weights1);
                Roads.AddTriangleCellData(indices, MeshBuilder.Weights1);
            }else {
                TriangulateRoadEdge(center, middleLeft, middleRight, index);
            }            
        }

        private void TriangulateRoadEdge(Vector3 center, Vector3 middleLeft, Vector3 middleRight, float index) {
            Roads.AddTriangle(center, middleLeft, middleRight);
            Roads.AddTriangleUV(
                new Vector2(1f, 0f), new Vector2(0f, 0f), new Vector2(0f, 0f)
            );

            Vector3 indices;
            indices.x = indices.y = indices.z = index;
            Roads.AddTriangleCellData(indices, MeshBuilder.Weights1);
        }

        private void TriangulateEdgeTerraces(
            EdgeVertices begin, IHexCell beginCell,
            EdgeVertices end, IHexCell endCell,
            bool hasRoad
        ) {
            EdgeVertices edgeTwo = EdgeVertices.TerraceLerp(begin, end, 1);
            Color weights2 = HexMetrics.TerraceLerp(MeshBuilder.Weights1, MeshBuilder.Weights2, 1);
            float i1 = beginCell.Index;
            float i2 = endCell.Index;

            TriangulateEdgeStrip_Hills(
                begin,   MeshBuilder.Weights1, i1, beginCell.RequiresYPerturb,
                edgeTwo, weights2,             i2, false,
                hasRoad
            );

            for(int i = 2; i < HexMetrics.TerraceSteps; i++) {
                EdgeVertices edgeOne = edgeTwo;
                Color weights1 = weights2;

                edgeTwo  = EdgeVertices.TerraceLerp(begin, end, i);
                weights2 = HexMetrics.TerraceLerp(MeshBuilder.Weights1, MeshBuilder.Weights2, i);

                TriangulateEdgeStrip_Hills(
                    edgeOne, weights1, i1, false,
                    edgeTwo, weights2, i2, false,
                    hasRoad
                );
            }

            TriangulateEdgeStrip_Hills(
                edgeTwo, weights2,             i1, false,
                end,     MeshBuilder.Weights2, i2, endCell.RequiresYPerturb,
                hasRoad
            );
        }

        private void TriangulateEdgeStrip_Hills(
            EdgeVertices e1, Color w1, float index1, bool perturbEdgeOne,
            EdgeVertices e2, Color w2, float index2, bool perturbEdgeTwo,
            bool hasRoad = false
        ) {
            Terrain.AddQuadUnperturbed(
                NoiseGenerator.Perturb(e1.V1, perturbEdgeOne), NoiseGenerator.Perturb(e1.V2, perturbEdgeOne),
                NoiseGenerator.Perturb(e2.V1, perturbEdgeTwo), NoiseGenerator.Perturb(e2.V2, perturbEdgeTwo)
            );

            Terrain.AddQuadUnperturbed(
                NoiseGenerator.Perturb(e1.V2, perturbEdgeOne), NoiseGenerator.Perturb(e1.V3, perturbEdgeOne),
                NoiseGenerator.Perturb(e2.V2, perturbEdgeTwo), NoiseGenerator.Perturb(e2.V3, perturbEdgeTwo)
            );

            Terrain.AddQuadUnperturbed(
                NoiseGenerator.Perturb(e1.V3, perturbEdgeOne), NoiseGenerator.Perturb(e1.V4, perturbEdgeOne),
                NoiseGenerator.Perturb(e2.V3, perturbEdgeTwo), NoiseGenerator.Perturb(e2.V4, perturbEdgeTwo)
            );

            Terrain.AddQuadUnperturbed(
                NoiseGenerator.Perturb(e1.V4, perturbEdgeOne), NoiseGenerator.Perturb(e1.V5, perturbEdgeOne),
                NoiseGenerator.Perturb(e2.V4, perturbEdgeTwo), NoiseGenerator.Perturb(e2.V5, perturbEdgeTwo)
            );

            Vector3 indices;
            indices.x = indices.z = index1;
            indices.y = index2;
            Terrain.AddQuadCellData(indices, w1, w2);
            Terrain.AddQuadCellData(indices, w1, w2);
            Terrain.AddQuadCellData(indices, w1, w2);
            Terrain.AddQuadCellData(indices, w1, w2);

            if(hasRoad) {
                MeshBuilder.TriangulateRoadSegment(e1.V2, e1.V3, e1.V4, e2.V2, e2.V3, e2.V4, w1, w2, indices);
            }
        }

        private void TriangulateCorner(CellTriangulationData data) {

            var leftOrientedData = MeshBuilder.GetTriangulationData(
                data.Left, data.Right, data.Center, data.Direction.Next2()
            );

            var rightOrientedData = MeshBuilder.GetTriangulationData(
                data.Right, data.Center, data.Left, data.Direction.Previous2()
            );

            if(data.CenterToLeftEdgeType == HexEdgeType.Slope) {

                if(data.CenterToRightEdgeType == HexEdgeType.Slope) {
                    TriangulateCornerTerraces(data);

                }else if(data.CenterToRightEdgeType == HexEdgeType.Flat) {
                    TriangulateCornerTerraces(leftOrientedData);
                }else {
                    TriangulateCornerTerracesCliff(data);
                }

            }else if(data.CenterToRightEdgeType == HexEdgeType.Slope) {

                if(data.CenterToLeftEdgeType == HexEdgeType.Flat) {
                    TriangulateCornerTerraces(rightOrientedData);
                }else {
                    TriangulateCornerCliffTerraces(data);
                }

            }else if(data.LeftToRightEdgeType == HexEdgeType.Slope) {

                if(data.Left.EdgeElevation < data.Right.EdgeElevation) {
                    TriangulateCornerCliffTerraces(rightOrientedData);
                }else {
                    TriangulateCornerTerracesCliff(leftOrientedData);
                }

            }else {
                Terrain.AddTriangleUnperturbed(
                    NoiseGenerator.Perturb(data.CenterCorner, data.Center.RequiresYPerturb),
                    NoiseGenerator.Perturb(data.LeftCorner,   data.Left  .RequiresYPerturb),
                    NoiseGenerator.Perturb(data.RightCorner,  data.Right .RequiresYPerturb)
                );

                Vector3 indices;
                indices.x = data.Center.Index;
                indices.y = data.Left  .Index;
                indices.z = data.Right .Index;

                Terrain.AddTriangleCellData(
                    indices, MeshBuilder.Weights1, MeshBuilder.Weights2, MeshBuilder.Weights3
                );
            }            
        }

        private void TriangulateCornerTerraces(CellTriangulationData data){
            Vector3 v3 = HexMetrics.TerraceLerp(data.CenterCorner, data.LeftCorner,  1);
            Vector3 v4 = HexMetrics.TerraceLerp(data.CenterCorner, data.RightCorner, 1);

            Color w3 = HexMetrics.TerraceLerp(MeshBuilder.Weights1, MeshBuilder.Weights2, 1);
            Color w4 = HexMetrics.TerraceLerp(MeshBuilder.Weights1, MeshBuilder.Weights3, 1);

            Vector3 indices;
            indices.x = data.Center.Index;
            indices.y = data.Left  .Index;
            indices.z = data.Right .Index;

            Terrain.AddTriangleUnperturbed(
                data.PerturbedCenterCorner,
                NoiseGenerator.Perturb(v3),
                NoiseGenerator.Perturb(v4)
            );

            Terrain.AddTriangleCellData(indices, MeshBuilder.Weights1, w3, w4);

            for(int i = 2; i < HexMetrics.TerraceSteps; i++) {
                Vector3 v1 = v3;
                Vector3 v2 = v4;
                Color w1 = w3;
                Color w2 = w4;

                v3 = HexMetrics.TerraceLerp(data.CenterCorner, data.LeftCorner,  i);
                v4 = HexMetrics.TerraceLerp(data.CenterCorner, data.RightCorner, i);
                w3 = HexMetrics.TerraceLerp(MeshBuilder.Weights1, MeshBuilder.Weights2, i);
                w4 = HexMetrics.TerraceLerp(MeshBuilder.Weights1, MeshBuilder.Weights3, i);

                Terrain.AddQuad(v1, v2, v3, v4);

                Terrain.AddQuadCellData(indices, w1, w2, w3, w4);
            }

            Terrain.AddQuadUnperturbed(
                NoiseGenerator.Perturb(v3, false), NoiseGenerator.Perturb(v4, false),
                data.PerturbedLeftCorner, data.PerturbedRightCorner
            );

            Terrain.AddQuadCellData(indices, w3, w4, MeshBuilder.Weights2, MeshBuilder.Weights3);
        }

        private void TriangulateCornerTerracesCliff(CellTriangulationData data){
            float b = Mathf.Abs(1f / (data.Right.EdgeElevation - data.Center.EdgeElevation));

            Vector3 boundary = Vector3.Lerp(data.PerturbedCenterCorner, data.PerturbedRightCorner, b);

            Color boundaryWeights = Color.Lerp(MeshBuilder.Weights1, MeshBuilder.Weights3, b);

            Vector3 indices;
            indices.x = data.Center.Index;
            indices.y = data.Left  .Index;
            indices.z = data.Right .Index;

            TriangulateBoundaryTriangle(
                data.CenterCorner, MeshBuilder.Weights1, data.Center.RequiresYPerturb,
                data.LeftCorner,   MeshBuilder.Weights2, data.Left  .RequiresYPerturb,
                boundary, boundaryWeights,
                indices
            );

            if(data.LeftToRightEdgeType == HexEdgeType.Slope) {
                TriangulateBoundaryTriangle(
                    data.LeftCorner,     MeshBuilder.Weights2, data.Left .RequiresYPerturb,
                    data.RightCorner,    MeshBuilder.Weights3, data.Right.RequiresYPerturb,
                    boundary, boundaryWeights,
                    indices
                );
            }else {
                Terrain.AddTriangleUnperturbed(
                    NoiseGenerator.Perturb(data.LeftCorner,  data.Left .RequiresYPerturb),
                    NoiseGenerator.Perturb(data.RightCorner, data.Right.RequiresYPerturb),
                    boundary
                );
                Terrain.AddTriangleCellData(
                    indices, MeshBuilder.Weights2, MeshBuilder.Weights3, boundaryWeights
                );
            }
        }

        private void TriangulateCornerCliffTerraces(CellTriangulationData data) {
            float b = Mathf.Abs(1f / (data.Left.EdgeElevation - data.Center.EdgeElevation));

            Vector3 boundary = Vector3.Lerp(data.PerturbedCenterCorner, data.PerturbedLeftCorner, b);

            Color boundaryWeights = Color.Lerp(MeshBuilder.Weights1, MeshBuilder.Weights2, b);

            Vector3 indices;
            indices.x = data.Center.Index;
            indices.y = data.Left  .Index;
            indices.z = data.Right .Index;

            TriangulateBoundaryTriangle(
                data.RightCorner,  MeshBuilder.Weights3, data.Right .RequiresYPerturb,
                data.CenterCorner, MeshBuilder.Weights1, data.Center.RequiresYPerturb,
                boundary, boundaryWeights, indices
            );

            if(data.LeftToRightEdgeType == HexEdgeType.Slope) {
                TriangulateBoundaryTriangle(
                    data.LeftCorner,  MeshBuilder.Weights2, data.Left .RequiresYPerturb,
                    data.RightCorner, MeshBuilder.Weights3, data.Right.RequiresYPerturb,
                    boundary, boundaryWeights, indices
                );
            }else {
                Terrain.AddTriangleUnperturbed(
                    data.PerturbedLeftCorner, data.PerturbedRightCorner, boundary
                );

                Terrain.AddTriangleCellData(
                    indices, MeshBuilder.Weights2, MeshBuilder.Weights3, boundaryWeights
                );
            }
        }

        private void TriangulateBoundaryTriangle(
            Vector3 begin, Color beginWeights, bool perturbBeginY,
            Vector3 left,  Color leftWeights,  bool perturbLeftY,
            Vector3 boundary, Color boundaryWeights,
            Vector3 indices
        ) {
            Vector3 v2 = NoiseGenerator.Perturb(HexMetrics.TerraceLerp(begin, left, 1));
            Color w2 = HexMetrics.TerraceLerp(beginWeights, leftWeights, 1);

            Terrain.AddTriangleUnperturbed(NoiseGenerator.Perturb(begin, perturbBeginY), v2, boundary);
            Terrain.AddTriangleCellData(indices, beginWeights, w2, boundaryWeights);

            for(int i = 2; i < HexMetrics.TerraceSteps; i++) {
                Vector3 v1 = v2;
                Color w1 = w2;
                v2 = NoiseGenerator.Perturb(HexMetrics.TerraceLerp(begin, left, i));
                w2 = HexMetrics.TerraceLerp(beginWeights, leftWeights, i);
                Terrain.AddTriangleUnperturbed(v1, v2, boundary);
                Terrain.AddTriangleCellData(indices, w1, w2, boundaryWeights);
            }

            Terrain.AddTriangleUnperturbed(v2, NoiseGenerator.Perturb(left, perturbLeftY), boundary);
            Terrain.AddTriangleCellData(indices, w2, leftWeights, boundaryWeights);
        }

        private bool ShouldRoadBeRendered(IHexCell cell, IHexCell neighbor) {
            return neighbor != null
                && neighbor.HasRoads
                && cell.HasRoads
                && HexMetrics.GetEdgeType(cell, neighbor) != HexEdgeType.Cliff;
        }

        #endregion

    }

}
