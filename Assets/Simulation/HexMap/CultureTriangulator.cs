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

        private ICivilizationTerritoryLogic CivTerritoryLogic;
        private INoiseGenerator             NoiseGenerator;
        private IHexGridMeshBuilder         MeshBuilder;

        #endregion

        #region constructors

        [Inject]
        public CultureTriangulator(
            ICivilizationTerritoryLogic civTerritoryLogic,
            INoiseGenerator noiseGenerator, IHexGridMeshBuilder meshBuilder
        ) {
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
                MeshBuilder.TriangulateEdgeStrip(
                    data.CenterToRightEdge, MeshBuilder.Weights1, data.Center.Index, 0f, data.Center.RequiresYPerturb,
                    data.RightToCenterEdge, MeshBuilder.Weights2, data.Right.Index,  1f, data.Right .RequiresYPerturb,
                    owner.Color, MeshBuilder.Culture
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
            float        v2       = HexMetrics.TerraceLerp(0f, 1f, 1);
            Color        weights2 = HexMetrics.TerraceLerp(MeshBuilder.Weights1, MeshBuilder.Weights2, 1);

            MeshBuilder.TriangulateEdgeStrip(
                data.CenterToRightEdge, MeshBuilder.Weights1, data.Center.Index, 0f, data.Center.RequiresYPerturb,
                edgeTwo,                weights2,             data.Right .Index, v2, false,
                owner.Color, MeshBuilder.Culture
            );

            for(int i = 2; i < HexMetrics.TerraceSteps; i++) {
                EdgeVertices edgeOne  = edgeTwo;
                float        v1       = v2;
                Color        weights1 = weights2;

                edgeTwo  = EdgeVertices.TerraceLerp(data.CenterToRightEdge, data.RightToCenterEdge, i);
                v2       = HexMetrics.TerraceLerp(0f, 1f, i);
                weights2 = HexMetrics.TerraceLerp(MeshBuilder.Weights1, MeshBuilder.Weights2, i);

                MeshBuilder.TriangulateEdgeStrip(
                    edgeOne, weights1, data.Center.Index, v1, false,
                    edgeTwo, weights2, data.Right .Index, v2, false,
                    owner.Color, MeshBuilder.Culture
                );
            }

            MeshBuilder.TriangulateEdgeStrip(
                edgeTwo,                weights2,             data.Center.Index, v2, false,
                data.RightToCenterEdge, MeshBuilder.Weights2, data.Right .Index, 1f, data.Right.RequiresYPerturb,
                owner.Color, MeshBuilder.Culture
            );
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
            Vector3 vertex3 = HexMetrics.TerraceLerp(data.CenterCorner, data.LeftCorner,  1);
            Vector3 vertex4 = HexMetrics.TerraceLerp(data.CenterCorner, data.RightCorner, 1);

            float alphaMin, leftAlphaMax, rightAlphaMax;
            CalculateCultureAlphas(
                data.Center, data.Left, data.Right, owner,
                out alphaMin, out leftAlphaMax, out rightAlphaMax
            );

            float alpha3 = HexMetrics.TerraceLerp(alphaMin, leftAlphaMax,  1);
            float alpha4 = HexMetrics.TerraceLerp(alphaMin, rightAlphaMax, 1);

            Color weights3 = HexMetrics.TerraceLerp(MeshBuilder.Weights1, MeshBuilder.Weights2, 1);
            Color weights4 = HexMetrics.TerraceLerp(MeshBuilder.Weights1, MeshBuilder.Weights3, 1);

            Vector3 indices;
            indices.x = data.Center.Index;
            indices.y = data.Left  .Index;
            indices.z = data.Right .Index;

            MeshBuilder.AddTriangleUnperturbed(
                data.PerturbedCenterCorner, MeshBuilder.Weights1, new Vector2(0f, alphaMin),
                NoiseGenerator.Perturb(vertex3), weights3,        new Vector2(0f, alpha3),
                NoiseGenerator.Perturb(vertex4), weights4,        new Vector2(0f, alpha4),
                owner.Color, indices, MeshBuilder.Culture
            );

            for(int i = 2; i < HexMetrics.TerraceSteps; i++) {
                Vector3 vertex1 = vertex3;
                Vector3 vertex2 = vertex4;
                float alpha1 = alpha3;
                float alpha2 = alpha4;
                Color weights1 = weights3;
                Color weights2 = weights4;

                vertex3 = HexMetrics.TerraceLerp(data.CenterCorner, data.LeftCorner,  i);
                vertex4 = HexMetrics.TerraceLerp(data.CenterCorner, data.RightCorner, i);

                alpha3 = HexMetrics.TerraceLerp(alphaMin, leftAlphaMax,  i);
                alpha4 = HexMetrics.TerraceLerp(alphaMin, rightAlphaMax, i);

                weights3 = HexMetrics.TerraceLerp(MeshBuilder.Weights1, MeshBuilder.Weights2, i);
                weights4 = HexMetrics.TerraceLerp(MeshBuilder.Weights1, MeshBuilder.Weights3, i);

                MeshBuilder.AddQuad(
                    vertex1, weights1, new Vector2(0f, alpha1),
                    vertex2, weights2, new Vector2(0f, alpha2),
                    vertex3, weights3, new Vector2(0f, alpha3),
                    vertex4, weights4, new Vector2(0f, alpha4),
                    owner.Color, indices, MeshBuilder.Culture
                );
            }

            MeshBuilder.AddQuadUnperturbed(
                NoiseGenerator.Perturb(vertex3), weights3,             new Vector2(0f, alpha3),
                NoiseGenerator.Perturb(vertex4), weights4,             new Vector2(0f, alpha4),
                data.PerturbedLeftCorner,        MeshBuilder.Weights2, new Vector2(0f, leftAlphaMax),
                data.PerturbedRightCorner,       MeshBuilder.Weights3, new Vector2(0f, rightAlphaMax),
                owner.Color, indices, MeshBuilder.Culture
            );
        }

        private void TriangulateCultureCornerTerracesCliff(
            CellTriangulationData data, ICivilization owner
        ){
            float alphaMin, leftAlphaMax, rightAlphaMax;
            CalculateCultureAlphas(
                data.Center, data.Left, data.Right, owner,
                out alphaMin, out leftAlphaMax, out rightAlphaMax
            );

            float b = Mathf.Abs(1f / (data.Right.EdgeElevation - data.Center.EdgeElevation));
            float boundaryAlpha = Mathf.Lerp(alphaMin, rightAlphaMax, b);

            Vector3 boundary = Vector3.Lerp(data.PerturbedCenterCorner, data.PerturbedRightCorner, b);

            Color boundaryWeights = Color.Lerp(MeshBuilder.Weights1, MeshBuilder.Weights3, b);
            Vector3 indices;
            indices.x = data.Center.Index;
            indices.y = data.Left  .Index;
            indices.z = data.Right .Index;

            TriangulateCultureBoundaryTriangle(
                data.CenterCorner, data.Center, alphaMin,     MeshBuilder.Weights1,
                data.LeftCorner,   data.Left,   leftAlphaMax, MeshBuilder.Weights2,
                boundary, boundaryAlpha,
                boundaryWeights, indices,
                owner
            );

            if(data.LeftToRightEdgeType == HexEdgeType.Slope) {
                TriangulateCultureBoundaryTriangle(
                    data.LeftCorner,  data.Left,  leftAlphaMax,  MeshBuilder.Weights2,
                    data.RightCorner, data.Right, rightAlphaMax, MeshBuilder.Weights3,
                    boundary, boundaryAlpha,
                    boundaryWeights, indices,
                    owner
                );
            }else {
                MeshBuilder.AddTriangleUnperturbed(
                    data.PerturbedLeftCorner,  MeshBuilder.Weights2, new Vector2(0f, leftAlphaMax),
                    data.PerturbedRightCorner, MeshBuilder.Weights3, new Vector2(0f, rightAlphaMax),
                    boundary,                  boundaryWeights,      new Vector2(0f, boundaryAlpha),
                    owner.Color, indices, MeshBuilder.Culture
                );
            }
        }

        private void TriangulateCultureCornerCliffTerraces(
            CellTriangulationData data, ICivilization owner
        ){
            float alphaMin, leftAlphaMax, rightAlphaMax;
            CalculateCultureAlphas(
                data.Center, data.Left, data.Right, owner,
                out alphaMin, out leftAlphaMax, out rightAlphaMax
            );

            float b = Mathf.Abs(1f / (data.Left.EdgeElevation - data.Center.EdgeElevation));
            float boundaryAlpha = Mathf.Lerp(alphaMin, leftAlphaMax, b);

            Vector3 boundary = Vector3.Lerp(data.PerturbedCenterCorner, data.PerturbedLeftCorner, b);

            Color boundaryWeights = Color.Lerp(MeshBuilder.Weights1, MeshBuilder.Weights2, b);
            Vector3 indices;
            indices.x = data.Center.Index;
            indices.y = data.Left  .Index;
            indices.z = data.Right .Index;

            TriangulateCultureBoundaryTriangle(
                data.RightCorner,  data.Right,  rightAlphaMax, MeshBuilder.Weights3,
                data.CenterCorner, data.Center, alphaMin,      MeshBuilder.Weights1,
                boundary, boundaryAlpha, boundaryWeights,
                indices,
                owner
            );

            if(data.LeftToRightEdgeType == HexEdgeType.Slope) {
                TriangulateCultureBoundaryTriangle(
                    data.LeftCorner,  data.Left,  leftAlphaMax,  MeshBuilder.Weights2,
                    data.RightCorner, data.Right, rightAlphaMax, MeshBuilder.Weights3,
                    boundary, boundaryAlpha,
                    boundaryWeights, indices,
                    owner
                );
            }else {
                MeshBuilder.AddTriangleUnperturbed(
                    data.PerturbedLeftCorner,  MeshBuilder.Weights2, new Vector2(0f, leftAlphaMax),
                    data.PerturbedRightCorner, MeshBuilder.Weights3, new Vector2(0f, rightAlphaMax),
                    boundary,                  boundaryWeights,      new Vector2(0f, boundaryAlpha),
                    owner.Color, indices, MeshBuilder.Culture
                );
            }
        }

        private void TriangulateCultureBoundaryTriangle(
            Vector3 begin, IHexCell beginCell, float beginAlpha, Color beginWeights,
            Vector3 left,  IHexCell leftCell,  float leftAlpha,  Color leftWeights,
            Vector3 boundary, float boundaryAlpha, Color boundaryWeights,
            Vector3 indices, ICivilization owner
        ) {
            Vector3 vertex2 = NoiseGenerator.Perturb(HexMetrics.TerraceLerp(begin, left, 1));
            float alpha2    = HexMetrics.TerraceLerp(beginAlpha,   leftAlpha,   1);
            Color weights2  = HexMetrics.TerraceLerp(beginWeights, leftWeights, 1);

            MeshBuilder.AddTriangleUnperturbed(
                NoiseGenerator.Perturb(begin, beginCell.RequiresYPerturb), beginWeights,    new Vector2(0f, beginAlpha),
                vertex2,                                                   weights2,        new Vector2(0f, alpha2),
                boundary,                                                  boundaryWeights, new Vector2(0f, boundaryAlpha),
                owner.Color, indices, MeshBuilder.Culture
            );

            for(int i = 2; i < HexMetrics.TerraceSteps; i++) {
                Vector3 vertex1 = vertex2;
                float alpha1    = alpha2;
                Color weights1  = weights2;

                vertex2 = NoiseGenerator.Perturb(HexMetrics.TerraceLerp(begin, left, i));
                alpha2 = HexMetrics.TerraceLerp(beginAlpha, leftAlpha, i);
                weights2 = HexMetrics.TerraceLerp(beginWeights, leftWeights, i);

                MeshBuilder.AddTriangleUnperturbed(
                    vertex1,  weights1,        new Vector2(0f, alpha1),
                    vertex2,  weights2,        new Vector2(0f, alpha2),
                    boundary, boundaryWeights, new Vector2(0f, boundaryAlpha),
                    owner.Color, indices, MeshBuilder.Culture
                );
            }

            MeshBuilder.AddTriangleUnperturbed(
                vertex2,                                                 weights2,        new Vector2(0f, alpha2),
                NoiseGenerator.Perturb(left, leftCell.RequiresYPerturb), leftWeights,     new Vector2(0f, leftAlpha),
                boundary,                                                boundaryWeights, new Vector2(0f, boundaryAlpha),
                owner.Color, indices, MeshBuilder.Culture
            );
        }

        //Creates a single triangle between the three corners.
        //Used when all edges surrounding the corner are either flats
        //or cliffs
        private void TriangulateCultureCornerSimple(
            CellTriangulationData data, ICivilization owner
        ) {
            float alphaMin, leftAlphaMax, rightAlphaMax;
            CalculateCultureAlphas(
                data.Center, data.Left, data.Right, owner,
                out alphaMin, out leftAlphaMax, out rightAlphaMax
            );

            Vector3 indices;
            indices.x = data.Center.Index;
            indices.y = data.Left.Index;
            indices.z = data.Right.Index;

            MeshBuilder.AddTriangleUnperturbed(
                data.PerturbedCenterCorner, MeshBuilder.Weights1, new Vector2(0f, alphaMin),
                data.PerturbedLeftCorner,   MeshBuilder.Weights2, new Vector2(0f, leftAlphaMax),
                data.PerturbedRightCorner,  MeshBuilder.Weights3, new Vector2(0f, rightAlphaMax),
                owner.Color, indices, MeshBuilder.Culture
            );
        }

        //Figures out how to arrange culture alpha for corner cases,
        //given the three cells that make up the corner and the
        //owner for which we're drawing culture.
        private void CalculateCultureAlphas(
            IHexCell beginCell, IHexCell leftCell,
            IHexCell rightCell, ICivilization owner,
            out float alphaMin, out float leftAlphaMax, out float rightAlphaMax
        ){
            if(CivTerritoryLogic.GetCivClaimingCell(beginCell) == owner) {
                if(CivTerritoryLogic.GetCivClaimingCell(leftCell) == owner) {
                    //Bottom owned, left owned, right unowned
                    alphaMin = leftAlphaMax = 0f;
                    rightAlphaMax = 1f;

                }else if(CivTerritoryLogic.GetCivClaimingCell(rightCell) == owner) {
                    //Bottom owned, left unowned, right owned
                    alphaMin = rightAlphaMax = 0f;
                    leftAlphaMax = 1f;

                }else {
                    //Bottom owned, left unowned, right unowned
                    alphaMin = 0f;
                    leftAlphaMax = rightAlphaMax = 1f;

                }
            }else if(CivTerritoryLogic.GetCivClaimingCell(leftCell) == owner) {
                if(CivTerritoryLogic.GetCivClaimingCell(rightCell) == owner) {
                    //bottom unowned, left owned, right owned
                    alphaMin = 1f;
                    leftAlphaMax = rightAlphaMax = 0f;

                }else {
                    //bottom unowned, left owned, right unowned
                    alphaMin = rightAlphaMax = 1f;
                    leftAlphaMax = 0f;
                }
            }else {
                //bottom unowned, left unowned, right owned
                alphaMin = leftAlphaMax = 1f;
                rightAlphaMax = 0f;
            }
        }

        #endregion

    }

}
