using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.Civilizations;
using Assets.Simulation.Cities;

namespace Assets.Simulation.HexMap {

    public class CultureTriangulator : ICultureTriangulator {

        #region instance fields and properties

        private IHexGrid                    Grid;
        private ICivilizationTerritoryLogic CivTerritoryLogic;
        private INoiseGenerator             NoiseGenerator;
        private IHexGridMeshBuilder         MeshBuilder;

        #endregion

        #region constructors

        [Inject]
        public CultureTriangulator(
            IHexGrid grid, ICivilizationTerritoryLogic civTerritoryLogic,
            INoiseGenerator noiseGenerator, IHexGridMeshBuilder meshBuilder
        ) {
            Grid              = grid;
            CivTerritoryLogic = civTerritoryLogic;
            NoiseGenerator    = noiseGenerator;
            MeshBuilder       = meshBuilder;
        }

        #endregion

        #region instance methods

        #region from ICultureTriangulator

        public bool ShouldTriangulateCulture(CellTriangulationData data) {
            return CivTerritoryLogic.GetCivClaimingCell(data.Center) != null;
        }

        public void TriangulateCulture(CellTriangulationData data) {
            var owner = CivTerritoryLogic.GetCivClaimingCell(data.Center);

            TriangulateCultureConnection(data, owner);
        }

        #endregion

        //Creates the connection between Center and Right, and possibly
        //the previous corner between Center, Left, and Right
        private void TriangulateCultureConnection(
            CellTriangulationData data, ICivilization owner
        ){
            if(data.Right == null) {
                return;
            }

            ICivilization ownerOfNeighbor = CivTerritoryLogic.GetCivClaimingCell(data.Right);
            if(ownerOfNeighbor == owner) {
                return;
            }

            if(data.CenterToRightEdgeType == HexEdgeType.Slope) {
                TriangulateCultureTerraces(data, owner);
            }else {
                TriangulateCultureEdgeStrip(
                    data.CenterToRightEdge, MeshBuilder.Weights1, data.Center.Index, data.Center.RequiresYPerturb,
                    data.RightToCenterEdge, MeshBuilder.Weights2, data.Right.Index,  data.Right .RequiresYPerturb,
                    owner, 0f, 1f
                );
            }

            if(data.Left == null) {
                return;
            }

            if(data.Center.EdgeElevation <= data.Left.EdgeElevation) {
                if(data.Center.EdgeElevation <= data.Right.EdgeElevation) {
                    TriangulateCultureCorner(data, owner);
                }else {
                    TriangulateCultureCorner(
                        MeshBuilder.GetTriangulationData(
                            data.Right, data.Center, data.Left, data.Direction.Previous2()
                        ),
                        owner
                    );
                }
            }else if(data.Left.EdgeElevation <= data.Right.EdgeElevation) {
                TriangulateCultureCorner(
                    MeshBuilder.GetTriangulationData(
                        data.Left, data.Right, data.Center, data.Direction.Next2()
                    ),
                    owner
                );
            }else {
                TriangulateCultureCorner(
                    MeshBuilder.GetTriangulationData(
                        data.Right, data.Center, data.Left, data.Direction.Previous2()
                    ),
                    owner
                );
            }
        }

        //Border to be cultured is between Center and Right.
        //The culture hugs the terraced edge, increasing its UV
        //from 0 at CenterToRightEdge to 1 at RightToCenterEdge
        private void TriangulateCultureTerraces(
            CellTriangulationData data, ICivilization owner
        ) {
            EdgeVertices edgeTwo  = EdgeVertices.TerraceLerp(data.CenterToRightEdge, data.RightToCenterEdge, 1);
            float        uv2      = HexMetrics.TerraceLerp(0f, 1f, 1);
            Color        weights2 = HexMetrics.TerraceLerp(MeshBuilder.Weights1, MeshBuilder.Weights2, 1);

            TriangulateCultureEdgeStrip(
                data.CenterToRightEdge, MeshBuilder.Weights1, data.Center.Index, data.Center.RequiresYPerturb,
                edgeTwo,                weights2,             data.Right .Index, false,
                owner, 0f, uv2
            );

            for(int i = 2; i < HexMetrics.TerraceSteps; i++) {
                EdgeVertices edgeOne  = edgeTwo;
                float        uv1      = uv2;
                Color        weights1 = weights2;

                edgeTwo  = EdgeVertices.TerraceLerp(data.CenterToRightEdge, data.RightToCenterEdge, i);
                uv2      = HexMetrics.TerraceLerp(0f, 1f, i);
                weights2 = HexMetrics.TerraceLerp(MeshBuilder.Weights1, MeshBuilder.Weights2, i);

                TriangulateCultureEdgeStrip(
                    edgeOne, weights1, data.Center.Index, false,
                    edgeTwo, weights2, data.Right .Index, false,
                    owner, uv1, uv2
                );
            }

            TriangulateCultureEdgeStrip(
                edgeTwo,                weights2,             data.Center.Index, false,
                data.RightToCenterEdge, MeshBuilder.Weights2, data.Right .Index, data.Right .RequiresYPerturb,
                owner, uv2, 1f
            );
        }

        private void TriangulateCultureEdgeStrip(
            EdgeVertices edgeOne, Color w1, float index1, bool perturbEdgeOneY,
            EdgeVertices edgeTwo, Color w2, float index2, bool perturbEdgeTwoY,
            ICivilization owner, float vMin, float vMax
        ) {
            MeshBuilder.Culture.AddQuadUnperturbed(
                NoiseGenerator.Perturb(edgeOne.V1, perturbEdgeOneY), NoiseGenerator.Perturb(edgeOne.V2, perturbEdgeOneY),
                NoiseGenerator.Perturb(edgeTwo.V1, perturbEdgeTwoY), NoiseGenerator.Perturb(edgeTwo.V2, perturbEdgeTwoY)
            );

            MeshBuilder.Culture.AddQuadUnperturbed(
                NoiseGenerator.Perturb(edgeOne.V2, perturbEdgeOneY), NoiseGenerator.Perturb(edgeOne.V3, perturbEdgeOneY),
                NoiseGenerator.Perturb(edgeTwo.V2, perturbEdgeTwoY), NoiseGenerator.Perturb(edgeTwo.V3, perturbEdgeTwoY)
            );

            MeshBuilder.Culture.AddQuadUnperturbed(
                NoiseGenerator.Perturb(edgeOne.V3, perturbEdgeOneY), NoiseGenerator.Perturb(edgeOne.V4, perturbEdgeOneY),
                NoiseGenerator.Perturb(edgeTwo.V3, perturbEdgeTwoY), NoiseGenerator.Perturb(edgeTwo.V4, perturbEdgeTwoY)
            );

            MeshBuilder.Culture.AddQuadUnperturbed(
                NoiseGenerator.Perturb(edgeOne.V4, perturbEdgeOneY), NoiseGenerator.Perturb(edgeOne.V5, perturbEdgeOneY),
                NoiseGenerator.Perturb(edgeTwo.V4, perturbEdgeTwoY), NoiseGenerator.Perturb(edgeTwo.V5, perturbEdgeTwoY)
            );

            MeshBuilder.Culture.AddQuadColor(owner.Color);
            MeshBuilder.Culture.AddQuadColor(owner.Color);
            MeshBuilder.Culture.AddQuadColor(owner.Color);
            MeshBuilder.Culture.AddQuadColor(owner.Color);

            MeshBuilder.Culture.AddQuadUV(0f, 0f, vMin, vMax);
            MeshBuilder.Culture.AddQuadUV(0f, 0f, vMin, vMax);
            MeshBuilder.Culture.AddQuadUV(0f, 0f, vMin, vMax);
            MeshBuilder.Culture.AddQuadUV(0f, 0f, vMin, vMax);

            Vector3 indices;
            indices.x = indices.z = index1;
            indices.y = index2;

            MeshBuilder.Culture.AddQuadCellData(indices, w1, w2);
            MeshBuilder.Culture.AddQuadCellData(indices, w1, w2);
            MeshBuilder.Culture.AddQuadCellData(indices, w1, w2);
            MeshBuilder.Culture.AddQuadCellData(indices, w1, w2);
        }

        private void TriangulateCultureCorner(
            CellTriangulationData data, ICivilization owner
        ){
            var dataCenteredOnLeft = MeshBuilder.GetTriangulationData(
                data.Left, data.Right, data.Center, data.Direction.Next2()
            );

            var dataCenteredOnRight = MeshBuilder.GetTriangulationData(
                data.Right, data.Center, data.Left, data.Direction.Previous2()
            );

            if(data.CenterToLeftEdgeType == HexEdgeType.Slope) {

                if(data.CenterToRightEdgeType == HexEdgeType.Slope) {
                    TriangulateCultureCornerTerraces(data, owner);

                }else if(data.CenterToRightEdgeType == HexEdgeType.Flat) {
                    TriangulateCultureCornerTerraces(dataCenteredOnLeft, owner);

                }else {
                    TriangulateCultureCornerTerracesCliff(data, owner);
                }

            }else if(data.CenterToRightEdgeType == HexEdgeType.Slope) {

                if(data.CenterToLeftEdgeType == HexEdgeType.Flat) {
                    TriangulateCultureCornerTerraces(dataCenteredOnRight, owner);

                }else {
                    TriangulateCultureCornerCliffTerraces(data, owner);
                }

            }else if(data.LeftToRightEdgeType == HexEdgeType.Slope) {

                if(data.Left.EdgeElevation < data.Right.EdgeElevation) {
                    TriangulateCultureCornerCliffTerraces(dataCenteredOnRight, owner);

                }else {
                    TriangulateCultureCornerTerracesCliff(dataCenteredOnLeft, owner);
                }

            }else {
                TriangulateCultureCornerSimple(data, owner);
            }
        }

        private void TriangulateCultureCornerTerraces(
            CellTriangulationData data, ICivilization owner
        ){
            Vector3 v3 = HexMetrics.TerraceLerp(data.CenterCorner, data.LeftCorner,  1);
            Vector3 v4 = HexMetrics.TerraceLerp(data.CenterCorner, data.RightCorner, 1);

            float uvMin, leftUVMax, rightUVMax;
            CalculateCultureUVs(
                data.Center, data.Left, data.Right, owner,
                out uvMin, out leftUVMax, out rightUVMax
            );

            float uv3 = HexMetrics.TerraceLerp(uvMin, leftUVMax,  1);
            float uv4 = HexMetrics.TerraceLerp(uvMin, rightUVMax, 1);

            Color w3 = HexMetrics.TerraceLerp(MeshBuilder.Weights1, MeshBuilder.Weights2, 1);
            Color w4 = HexMetrics.TerraceLerp(MeshBuilder.Weights1, MeshBuilder.Weights3, 1);

            Vector3 indices;
            indices.x = data.Center.Index;
            indices.y = data.Left  .Index;
            indices.z = data.Right .Index;

            MeshBuilder.Culture.AddTriangleUnperturbed(
                data.PerturbedCenterCorner, NoiseGenerator.Perturb(v3),
                NoiseGenerator.Perturb(v4)
            );

            MeshBuilder.Culture.AddTriangleColor(owner.Color);
            MeshBuilder.Culture.AddTriangleUV(
                new Vector2(0f, uvMin),
                new Vector2(0f, uv3),
                new Vector2(0f, uv4)
            );

            MeshBuilder.Culture.AddTriangleCellData(indices, MeshBuilder.Weights1, w3, w4);

            for(int i = 2; i < HexMetrics.TerraceSteps; i++) {
                Vector3 v1 = v3;
                Vector3 v2 = v4;
                float uv1 = uv3;
                float uv2 = uv4;
                Color w1 = w3;
                Color w2 = w4;

                v3 = HexMetrics.TerraceLerp(data.CenterCorner, data.LeftCorner,  i);
                v4 = HexMetrics.TerraceLerp(data.CenterCorner, data.RightCorner, i);

                uv3 = HexMetrics.TerraceLerp(uvMin, leftUVMax, i);
                uv4 = HexMetrics.TerraceLerp(uvMin, rightUVMax, i);

                w3 = HexMetrics.TerraceLerp(MeshBuilder.Weights1, MeshBuilder.Weights2, i);
                w4 = HexMetrics.TerraceLerp(MeshBuilder.Weights1, MeshBuilder.Weights3, i);

                MeshBuilder.Culture.AddQuad(v1, v2, v3, v4);
                MeshBuilder.Culture.AddQuadColor(owner.Color);
                MeshBuilder.Culture.AddQuadUV(
                    new Vector2(0f, uv1), new Vector2(0f, uv2),
                    new Vector2(0f, uv3), new Vector2(0f, uv4)
                );

                MeshBuilder.Culture.AddQuadCellData(indices, w1, w2, w3, w4);
            }

            MeshBuilder.Culture.AddQuadUnperturbed(
                NoiseGenerator.Perturb(v3), NoiseGenerator.Perturb(v4),
                data.PerturbedLeftCorner, data.PerturbedRightCorner
            );

            MeshBuilder.Culture.AddQuadColor(owner.Color);
            MeshBuilder.Culture.AddQuadUV(
                new Vector2(0f, uv3), new Vector2(0f, uv4),
                new Vector2(0f, leftUVMax), new Vector2(0f, rightUVMax)
            );

            MeshBuilder.Culture.AddQuadCellData(indices, w3, w4, MeshBuilder.Weights2, MeshBuilder.Weights3);
        }

        private void TriangulateCultureCornerTerracesCliff(
            CellTriangulationData data, ICivilization owner
        ){
            float uvMin, leftUVMax, rightUVMax;
            CalculateCultureUVs
                (data.Center, data.Left, data.Right, owner,
                out uvMin, out leftUVMax, out rightUVMax
            );

            float b = Mathf.Abs(1f / (data.Right.EdgeElevation - data.Center.EdgeElevation));
            float boundaryUV = Mathf.Lerp(uvMin, rightUVMax, b);

            Vector3 boundary = Vector3.Lerp(data.PerturbedCenterCorner, data.PerturbedRightCorner, b);

            Color boundaryWeights = Color.Lerp(MeshBuilder.Weights1, MeshBuilder.Weights3, b);
            Vector3 indices;
            indices.x = data.Center.Index;
            indices.y = data.Left  .Index;
            indices.z = data.Right .Index;

            TriangulateCultureBoundaryTriangle(
                data.CenterCorner, data.Center, uvMin,     MeshBuilder.Weights1,
                data.LeftCorner,   data.Left,   leftUVMax, MeshBuilder.Weights2,
                boundary, boundaryUV,
                boundaryWeights, indices,
                owner
            );

            if(data.LeftToRightEdgeType == HexEdgeType.Slope) {
                TriangulateCultureBoundaryTriangle(
                    data.LeftCorner,  data.Left,  leftUVMax,  MeshBuilder.Weights2,
                    data.RightCorner, data.Right, rightUVMax, MeshBuilder.Weights3,
                    boundary, boundaryUV,
                    boundaryWeights, indices,
                    owner
                );
            }else {
                MeshBuilder.Culture.AddTriangleUnperturbed(data.PerturbedLeftCorner, data.PerturbedRightCorner, boundary);
                MeshBuilder.Culture.AddTriangleColor(owner.Color);
                MeshBuilder.Culture.AddTriangleUV(
                    new Vector2(0f, leftUVMax),
                    new Vector2(0f, rightUVMax),
                    new Vector2(0f, boundaryUV)
                );

                MeshBuilder.Culture.AddTriangleCellData(indices, MeshBuilder.Weights2, MeshBuilder.Weights3, boundaryWeights);
            }
        }

        private void TriangulateCultureCornerCliffTerraces(
            CellTriangulationData data, ICivilization owner
        ){
            float uvMin, leftUVMax, rightUVMax;
            CalculateCultureUVs(
                data.Center, data.Left, data.Right, owner,
                out uvMin, out leftUVMax, out rightUVMax
            );

            float b = Mathf.Abs(1f / (data.Left.EdgeElevation - data.Center.EdgeElevation));
            float boundaryUV = Mathf.Lerp(uvMin, leftUVMax, b);

            Vector3 boundary = Vector3.Lerp(data.PerturbedCenterCorner, data.PerturbedLeftCorner, b);

            Color boundaryWeights = Color.Lerp(MeshBuilder.Weights1, MeshBuilder.Weights2, b);
            Vector3 indices;
            indices.x = data.Center.Index;
            indices.y = data.Left  .Index;
            indices.z = data.Right .Index;

            TriangulateCultureBoundaryTriangle(
                data.RightCorner,  data.Right,  rightUVMax, MeshBuilder.Weights3,
                data.CenterCorner, data.Center, uvMin,      MeshBuilder.Weights1,
                boundary, boundaryUV,
                boundaryWeights, indices,
                owner
            );

            if(data.LeftToRightEdgeType == HexEdgeType.Slope) {
                TriangulateCultureBoundaryTriangle(
                    data.LeftCorner,  data.Left,  leftUVMax,  MeshBuilder.Weights2,
                    data.RightCorner, data.Right, rightUVMax, MeshBuilder.Weights3,
                    boundary, boundaryUV,
                    boundaryWeights, indices,
                    owner
                );
            }else {
                MeshBuilder.Culture.AddTriangleUnperturbed(data.PerturbedLeftCorner, data.PerturbedRightCorner, boundary);
                MeshBuilder.Culture.AddTriangleColor(owner.Color);
                MeshBuilder.Culture.AddTriangleUV(
                    new Vector2(0f, leftUVMax),
                    new Vector2(0f, rightUVMax),
                    new Vector2(0f, boundaryUV)
                );
                MeshBuilder.Culture.AddTriangleCellData(indices, MeshBuilder.Weights2, MeshBuilder.Weights3, boundaryWeights);
            }
        }

        private void TriangulateCultureBoundaryTriangle(
            Vector3 begin, IHexCell beginCell, float beginUV, Color beginWeights,
            Vector3 left,  IHexCell leftCell,  float leftUV,  Color leftWeights,
            Vector3 boundary, float boundaryUV, Color boundaryWeights,
            Vector3 indices,
            ICivilization owner
        ) {
            Vector3 v2 = NoiseGenerator.Perturb(HexMetrics.TerraceLerp(begin, left, 1));
            float uv2 = HexMetrics.TerraceLerp(beginUV, leftUV, 1);
            Color w2 = HexMetrics.TerraceLerp(beginWeights, leftWeights, 1);

            MeshBuilder.Culture.AddTriangleUnperturbed(
                NoiseGenerator.Perturb(begin, beginCell.RequiresYPerturb), v2, boundary
            );

            MeshBuilder.Culture.AddTriangleColor(owner.Color);
            MeshBuilder.Culture.AddTriangleUV(
                new Vector2(0f, beginUV),
                new Vector2(0f, uv2),
                new Vector2(0f, boundaryUV)
            );
            MeshBuilder.Culture.AddTriangleCellData(indices, beginWeights, w2, boundaryWeights);

            for(int i = 2; i < HexMetrics.TerraceSteps; i++) {
                Vector3 v1 = v2;
                float uv1 = uv2;
                Color w1 = w2;

                v2 = NoiseGenerator.Perturb(HexMetrics.TerraceLerp(begin, left, i));
                uv2 = HexMetrics.TerraceLerp(beginUV, leftUV, i);
                w2 = HexMetrics.TerraceLerp(beginWeights, leftWeights, i);

                MeshBuilder.Culture.AddTriangleUnperturbed(v1, v2, boundary);
                MeshBuilder.Culture.AddTriangleColor(owner.Color);
                MeshBuilder.Culture.AddTriangleUV(
                    new Vector2(0f, uv1),
                    new Vector2(0f, uv2),
                    new Vector2(0f, boundaryUV)
                );
                MeshBuilder.Culture.AddTriangleCellData(indices, w1, w2, boundaryWeights);
            }

            MeshBuilder.Culture.AddTriangleUnperturbed(
                v2, NoiseGenerator.Perturb(left, leftCell.RequiresYPerturb), boundary
            );
            MeshBuilder.Culture.AddTriangleColor(owner.Color);
            MeshBuilder.Culture.AddTriangleUV(
                new Vector2(0f, uv2),
                new Vector2(0f, leftUV),
                new Vector2(0f, boundaryUV)
            );
            MeshBuilder.Culture.AddTriangleCellData(indices, w2, leftWeights, boundaryWeights);
        }

        //Creates a single triangle between the three corners.
        //Used when all edges surrounding the corner are either flats
        //or cliffs
        private void TriangulateCultureCornerSimple(
            CellTriangulationData data, ICivilization owner
        ) {
            MeshBuilder.Culture.AddTriangleUnperturbed(
                data.PerturbedCenterCorner, data.PerturbedLeftCorner,
                data.PerturbedRightCorner
            );

            MeshBuilder.Culture.AddTriangleColor(owner.Color);

            float uvMin, leftUVMax, rightUVMax;
            CalculateCultureUVs(
                data.Center, data.Left, data.Right, owner,
                out uvMin, out leftUVMax, out rightUVMax
            );

            MeshBuilder.Culture.AddTriangleUV(
                new Vector2(0f, uvMin), new Vector2(0f, leftUVMax), new Vector2(0f, rightUVMax)
            );

            Vector3 indices;
            indices.x = data.Center.Index;
            indices.y = data.Left.Index;
            indices.z = data.Right.Index;

            MeshBuilder.Culture.AddTriangleCellData(indices, MeshBuilder.Weights1, MeshBuilder.Weights2, MeshBuilder.Weights3);
        }

        //Figures out how to arrange culture UVs for corner cases,
        //given the three cells that make up the corner and the
        //owner for which we're drawing culture.
        private void CalculateCultureUVs(
            IHexCell beginCell, IHexCell leftCell,
            IHexCell rightCell, ICivilization owner,
            out float uvMin, out float leftUVMax, out float rightUVMax
        ){
            if(CivTerritoryLogic.GetCivClaimingCell(beginCell) == owner) {
                if(CivTerritoryLogic.GetCivClaimingCell(leftCell) == owner) {
                    //Bottom owned, left owned, right unowned
                    uvMin = leftUVMax = 0f;
                    rightUVMax = 1f;

                }else if(CivTerritoryLogic.GetCivClaimingCell(rightCell) == owner) {
                    //Bottom owned, left unowned, right owned
                    uvMin = rightUVMax = 0f;
                    leftUVMax = 1f;

                }else {
                    //Bottom owned, left unowned, right unowned
                    uvMin = 0f;
                    leftUVMax = rightUVMax = 1f;

                }
            }else if(CivTerritoryLogic.GetCivClaimingCell(leftCell) == owner) {
                if(CivTerritoryLogic.GetCivClaimingCell(rightCell) == owner) {
                    //bottom unowned, left owned, right owned
                    uvMin = 1f;
                    leftUVMax = rightUVMax = 0f;

                }else {
                    //bottom unowned, left owned, right unowned
                    uvMin = rightUVMax = 1f;
                    leftUVMax = 0f;
                }
            }else {
                //bottom unowned, left unowned, right owned
                uvMin = leftUVMax = 1f;
                rightUVMax = 0f;
            }
        }

        #endregion

    }

}
