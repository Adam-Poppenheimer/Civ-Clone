using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

namespace Assets.Simulation.HexMap {

    public class BasicTerrainTriangulator : IBasicTerrainTriangulator {

        #region instance fields and properties

        private IHexGridMeshBuilder MeshBuilder;
        private INoiseGenerator     NoiseGenerator;

        #endregion

        #region constructors

        [Inject]
        public BasicTerrainTriangulator(
            IHexGridMeshBuilder meshBuilder, INoiseGenerator noiseGenerator
        ) {
            MeshBuilder    = meshBuilder;
            NoiseGenerator = noiseGenerator;
        }

        #endregion

        #region instance methods

        #region from IBasicTerrainTriangulator

        public void TriangulateTerrainCenter(CellTriangulationData data){

            if(data.Center.Shape == CellShape.Hills) {
                TriangulateCenterWithHills(data);
            }else {
                TriangulateCenterWithoutHills(data);
            }
        }

        public bool ShouldTriangulateTerrainEdge(CellTriangulationData data) {
            return data.CenterToRightEdgeType != HexEdgeType.River && data.Direction <= HexDirection.SE;
        }

        public void TriangulateTerrainEdge(CellTriangulationData data) {
            if(data.Right == null) {
                return;
            }

            if(data.CenterToRightEdgeType == HexEdgeType.Slope) {
                TriangulateEdgeTerraces(
                    data.CenterToRightEdge, data.Center,
                    data.RightToCenterEdge, data.Right
                );
            }else if(data.Center.Shape == CellShape.Hills || data.Right.Shape == CellShape.Hills){
                MeshBuilder.TriangulateEdgeStrip(
                    data.CenterToRightEdge, MeshBuilder.Weights1, data.Center.Index, data.Center.RequiresYPerturb,
                    data.RightToCenterEdge, MeshBuilder.Weights2, data.Right .Index, data.Right .RequiresYPerturb,
                    MeshBuilder.JaggedTerrain
                );
            }else{
                MeshBuilder.TriangulateEdgeStrip(
                    data.CenterToRightEdge, MeshBuilder.Weights1, data.Center.Index,
                    data.RightToCenterEdge, MeshBuilder.Weights2, data.Right .Index,
                    MeshBuilder.JaggedTerrain
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
                data.Center.Index, MeshBuilder.SmoothTerrain, true
            );

            MeshBuilder.TriangulateEdgeStrip(
                data.CenterToRightInnerEdge, MeshBuilder.Weights1, data.Center.Index, true,
                data.CenterToRightEdge,      MeshBuilder.Weights1, data.Center.Index, true,
                MeshBuilder.SmoothTerrain
            );
        }

        private void TriangulateCenterWithoutHills(CellTriangulationData data){
            MeshBuilder.TriangulateEdgeFan(
                data.CenterPeak, data.CenterToRightEdge, data.Center.Index,
                MeshBuilder.SmoothTerrain
            );
        }        

        private void TriangulateEdgeTerraces(
            EdgeVertices begin, IHexCell beginCell,
            EdgeVertices end, IHexCell endCell
        ) {
            EdgeVertices edgeTwo = EdgeVertices.TerraceLerp(begin, end, 1);
            Color weights2 = HexMetrics.TerraceLerp(MeshBuilder.Weights1, MeshBuilder.Weights2, 1);
            float i1 = beginCell.Index;
            float i2 = endCell.Index;

            MeshBuilder.TriangulateEdgeStrip(
                begin,   MeshBuilder.Weights1, i1, beginCell.RequiresYPerturb,
                edgeTwo, weights2,             i2, false,
                MeshBuilder.JaggedTerrain
            );

            for(int i = 2; i < HexMetrics.TerraceSteps; i++) {
                EdgeVertices edgeOne = edgeTwo;
                Color weights1 = weights2;

                edgeTwo  = EdgeVertices.TerraceLerp(begin, end, i);
                weights2 = HexMetrics.TerraceLerp(MeshBuilder.Weights1, MeshBuilder.Weights2, i);

                MeshBuilder.TriangulateEdgeStrip(
                    edgeOne, weights1, i1, false,
                    edgeTwo, weights2, i2, false,
                    MeshBuilder.JaggedTerrain
                );
            }

            MeshBuilder.TriangulateEdgeStrip(
                edgeTwo, weights2,             i1, false,
                end,     MeshBuilder.Weights2, i2, endCell.RequiresYPerturb,
                MeshBuilder.JaggedTerrain
            );
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
                MeshBuilder.AddTriangleUnperturbed(
                    data.PerturbedCenterCorner, data.Center.Index, MeshBuilder.Weights1,
                    data.PerturbedLeftCorner,   data.Left  .Index, MeshBuilder.Weights2,
                    data.PerturbedRightCorner,  data.Right .Index, MeshBuilder.Weights3,
                    MeshBuilder.JaggedTerrain
                );
            }            
        }

        private void TriangulateCornerTerraces(CellTriangulationData data){
            Vector3 v3 = HexMetrics.TerraceLerp(data.CenterCorner, data.LeftCorner,  1);
            Vector3 v4 = HexMetrics.TerraceLerp(data.CenterCorner, data.RightCorner, 1);

            Color w3 = HexMetrics.TerraceLerp(MeshBuilder.Weights1, MeshBuilder.Weights2, 1);
            Color w4 = HexMetrics.TerraceLerp(MeshBuilder.Weights1, MeshBuilder.Weights3, 1);

            MeshBuilder.AddTriangleUnperturbed(
                data.PerturbedCenterCorner, data.Center.Index, MeshBuilder.Weights1,
                NoiseGenerator.Perturb(v3), data.Left  .Index, w3,
                NoiseGenerator.Perturb(v4), data.Right .Index, w4,
                MeshBuilder.JaggedTerrain
            );

            for(int i = 2; i < HexMetrics.TerraceSteps; i++) {
                Vector3 v1 = v3;
                Vector3 v2 = v4;
                Color w1 = w3;
                Color w2 = w4;

                v3 = HexMetrics.TerraceLerp(data.CenterCorner, data.LeftCorner,  i);
                v4 = HexMetrics.TerraceLerp(data.CenterCorner, data.RightCorner, i);
                w3 = HexMetrics.TerraceLerp(MeshBuilder.Weights1, MeshBuilder.Weights2, i);
                w4 = HexMetrics.TerraceLerp(MeshBuilder.Weights1, MeshBuilder.Weights3, i);

                MeshBuilder.AddQuad(
                    v1, w1, v2, w2,
                    v3, w3, v4, w4,
                    data.Center.Index, data.Left.Index, data.Right.Index,
                    MeshBuilder.JaggedTerrain
                );
            }

            MeshBuilder.AddQuadUnperturbed(
                NoiseGenerator.Perturb(v3, false), w3,                   NoiseGenerator.Perturb(v4, false), w4,
                data.PerturbedLeftCorner,          MeshBuilder.Weights2, data.PerturbedRightCorner,         MeshBuilder.Weights3,
                data.Center.Index, data.Left.Index, data.Right.Index,
                MeshBuilder.JaggedTerrain
            );
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
                MeshBuilder.AddTriangleUnperturbed(
                    data.PerturbedLeftCorner,  data.Center.Index, MeshBuilder.Weights2,
                    data.PerturbedRightCorner, data.Left  .Index, MeshBuilder.Weights3,
                    boundary,                  data.Right .Index, boundaryWeights,
                    MeshBuilder.JaggedTerrain
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
                MeshBuilder.AddTriangleUnperturbed(
                    data.PerturbedLeftCorner,  data.Center.Index, MeshBuilder.Weights2,
                    data.PerturbedRightCorner, data.Left  .Index, MeshBuilder.Weights3,
                    boundary,                  data.Right .Index, boundaryWeights,
                    MeshBuilder.JaggedTerrain
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

            MeshBuilder.AddTriangleUnperturbed(
                NoiseGenerator.Perturb(begin, perturbBeginY), beginWeights,
                v2, w2, boundary, boundaryWeights,
                indices, MeshBuilder.JaggedTerrain
            );

            for(int i = 2; i < HexMetrics.TerraceSteps; i++) {
                Vector3 v1 = v2;
                Color w1 = w2;
                v2 = NoiseGenerator.Perturb(HexMetrics.TerraceLerp(begin, left, i));
                w2 = HexMetrics.TerraceLerp(beginWeights, leftWeights, i);

                MeshBuilder.AddTriangleUnperturbed(
                    v1, w1, v2, w2, boundary, boundaryWeights,
                    indices, MeshBuilder.JaggedTerrain
                );
            }

            MeshBuilder.AddTriangleUnperturbed(
                v2, w2, NoiseGenerator.Perturb(left, perturbLeftY), leftWeights,
                boundary, boundaryWeights, indices, MeshBuilder.JaggedTerrain
            );
        }

        #endregion

    }

}
