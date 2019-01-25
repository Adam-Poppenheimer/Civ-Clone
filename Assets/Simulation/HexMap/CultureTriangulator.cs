using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.Civilizations;
using Assets.Simulation.Cities;
using Assets.Util;

namespace Assets.Simulation.HexMap {

    public class CultureTriangulator : ICultureTriangulator {

        #region instance fields and properties

        private ICivilizationTerritoryLogic CivTerritoryLogic;
        private INoiseGenerator             NoiseGenerator;
        private IHexGridMeshBuilder         MeshBuilder;
        private IHexMapRenderConfig         RenderConfig;

        #endregion

        #region constructors

        [Inject]
        public CultureTriangulator(
            ICivilizationTerritoryLogic civTerritoryLogic,
            INoiseGenerator noiseGenerator, IHexGridMeshBuilder meshBuilder,
            IHexMapRenderConfig renderConfig
        ) {
            CivTerritoryLogic = civTerritoryLogic;
            NoiseGenerator    = noiseGenerator;
            MeshBuilder       = meshBuilder;
            RenderConfig      = renderConfig;
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

            TriangulateRiverEdge(data, owner);

            if(data.Left == null) {
                return;
            }

            TriangulateCultureCorner(data, owner);
        }

        //Unfortunately, given the divergent behavior between
        //flat and non-flat edges, edge triangulation needs to
        //be aware of adjacent edges. We need to concern ourselves
        //with the edge type and ownership of both Left and NextRight,
        //adding appropriate connecting triangles when necessary.
        private void TriangulateRiverEdge(
            CellTriangulationData data, ICivilization centerOwner
        ) {
            Vector3 indices = new Vector3(
                data.Center.Index, data.Left.Index, data.Right.Index
            );

            ICivilization leftOwner      = CivTerritoryLogic.GetCivClaimingCell(data.Left);
            ICivilization nextRightOwner = CivTerritoryLogic.GetCivClaimingCell(data.NextRight);

            //This exists to serve the difference in triangulation
            //strategies between flat and non-flat edges. Culture on
            //flat edges extends to the middle of the edge boundary
            //between hexes, while culture on non-flat edges only
            //extends to the edge of the hex.
            if(data.CenterToRightEdgeType != HexEdgeType.Flat) {
                //If CenterRightEdge is flat but CenterLeftEdge is non-flat,
                //we can add a simple triangle to connect up with the triangulation
                //results produced when we're aiming left.
                if(data.CenterToLeftEdgeType == HexEdgeType.Flat) {
                    MeshBuilder.AddTriangleUnperturbed(
                        data.CenterToRightCultureStartEdgePerturbed.V1, MeshBuilder.Weights1, new Vector2(0f, 0f),
                        data.CenterToLeftEdgePerturbed.V4,              MeshBuilder.Weights1, new Vector2(0f, 0f),
                        data.PerturbedCenterCorner,                     MeshBuilder.Weights1, new Vector2(0f, 0.5f),
                        centerOwner.Template.Color, indices, MeshBuilder.Culture
                    );

                    //If Center and Left have the same owner, the connection
                    //is a fairly simple quad across CenterLeftEdge, which
                    //we deal with here.
                    if(leftOwner == centerOwner) {
                        MeshBuilder.AddQuadUnperturbed(
                            data.CenterToLeftEdgePerturbed.V4, MeshBuilder.Weights1, new Vector2(0f, 0f),
                            data.CenterToLeftEdgePerturbed.V5, MeshBuilder.Weights1, new Vector2(0f, 0.5f),
                            data.LeftToCenterEdgePerturbed.V4, MeshBuilder.Weights2, new Vector2(0f, 0f),
                            data.LeftToCenterEdgePerturbed.V5, MeshBuilder.Weights2, new Vector2(0f, 0.5f),
                            centerOwner.Template.Color, indices, MeshBuilder.Culture
                        );
                    }
                }

                //If CenterNextRightEdge is flat and CenterLeftEdge isn't,
                //we can do the same sort of connection technique, though
                //with a slightly different orientation. We don't include
                //the extra quad as in the CenterToLeft case because that
                //case covers it for us.
                if(data.CenterToNextRightEdgeType == HexEdgeType.Flat) {
                    MeshBuilder.AddTriangleUnperturbed(
                        data.CenterToRightCultureStartEdgePerturbed.V5, MeshBuilder.Weights1, new Vector2(0f, 0f),
                        data.CenterToNextRightEdgePerturbed.V1,         MeshBuilder.Weights1, new Vector2(0f, 0.5f),
                        data.CenterToNextRightEdgePerturbed.V2,         MeshBuilder.Weights1, new Vector2(0f, 0f),
                        centerOwner.Template.Color, indices, MeshBuilder.Culture
                    );
                }
            }

            //For compatibility with rivers, we decide not to have
            //culture ascend or descend terraced edges. Instead, the
            //culture makes its way to the edge of the terraces and stops.
            if(data.CenterToRightEdgeType == HexEdgeType.Slope || data.CenterToRightEdgeType == HexEdgeType.River) {
                MeshBuilder.TriangulateEdgeStripUnperturbed(
                    data.CenterToRightCultureStartEdgePerturbed, MeshBuilder.Weights1, data.Center.Index, 0f,
                    data.CenterToRightEdgePerturbed,             MeshBuilder.Weights1, data.Center.Index, 0.5f,
                    centerOwner.Template.Color, MeshBuilder.Culture
                );

            }else {
                //We can't simply draw flat edges. Doing so will lead to
                //awkward, disconnected borders when we're near non-flat edges.  
                //We thus need to replace the outermost segments of this edge
                //with triangles if those segments connect to non-flat
                //culture boundaries

                bool modifyleftEnd  = data.CenterToLeftEdgeType      != HexEdgeType.Flat;
                bool modifyRightEnd = data.CenterToNextRightEdgeType != HexEdgeType.Flat;

                if(modifyleftEnd) {
                    Vector3 v1Midpoint = (data.CenterToRightEdgePerturbed.V1 + data.RightToCenterEdgePerturbed.V1) / 2f;
                    Vector3 v2Midpoint = (data.CenterToRightEdgePerturbed.V2 + data.RightToCenterEdgePerturbed.V2) / 2f;

                    float v1BackAlpha = leftOwner == centerOwner ? 0f : 0.5f;

                    MeshBuilder.AddQuadUnperturbed(
                        data.CenterToRightEdgePerturbed.V1, MeshBuilder.Weights1,  new Vector2(0f, v1BackAlpha),
                        data.CenterToRightEdgePerturbed.V2, MeshBuilder.Weights1,  new Vector2(0f, 0f),
                        v1Midpoint,                         MeshBuilder.Weights13, new Vector2(0f, 0.5f),
                        v2Midpoint,                         MeshBuilder.Weights13, new Vector2(0f, 0.5f),
                        centerOwner.Template.Color, indices, MeshBuilder.Culture
                    );
                }

                MeshBuilder.TriangulateEdgeStripPartial(
                    data.CenterToRightEdge, MeshBuilder.Weights1, data.Center.Index, 0f, data.Center.RequiresYPerturb,
                    data.RightToCenterEdge, MeshBuilder.Weights2, data.Right.Index,  1f, data.Right .RequiresYPerturb,
                    centerOwner.Template.Color, MeshBuilder.Culture, !modifyleftEnd, !modifyRightEnd
                );

                if(modifyRightEnd) {
                    Vector3 v4Midpoint = (data.CenterToRightEdgePerturbed.V4 + data.RightToCenterEdgePerturbed.V4) / 2f;
                    Vector3 v5Midpoint = (data.CenterToRightEdgePerturbed.V5 + data.RightToCenterEdgePerturbed.V5) / 2f;

                    float v5BackAlpha = CivTerritoryLogic.GetCivClaimingCell(data.NextRight) == centerOwner ? 0f : 0.5f;

                    MeshBuilder.AddQuadUnperturbed(
                        data.CenterToRightEdgePerturbed.V4, MeshBuilder.Weights1,  new Vector2(0f, 0f),
                        data.CenterToRightEdgePerturbed.V5, MeshBuilder.Weights1,  new Vector2(0f, v5BackAlpha),
                        v4Midpoint,                         MeshBuilder.Weights13, new Vector2(0f, 0.5f),
                        v5Midpoint,                         MeshBuilder.Weights13, new Vector2(0f, 0.5f),
                        centerOwner.Template.Color, indices, MeshBuilder.Culture
                    );
                }
            }
        }

        //Border to be cultured is between Center and Right.
        //The culture hugs the terraced edge, increasing its UV
        //from 0 at CenterToRightEdge to 1 at RightToCenterEdge
        private void TriangulateCultureTerraces(
            CellTriangulationData data, ICivilization owner
        ) {
            EdgeVertices edgeTwo  = RenderConfig.TerraceLerp(data.CenterToRightEdge, data.RightToCenterEdge, 1);
            float        v2       = RenderConfig.TerraceLerp(0f, 1f, 1);
            Color        weights2 = RenderConfig.TerraceLerp(MeshBuilder.Weights1, MeshBuilder.Weights2, 1);

            MeshBuilder.TriangulateEdgeStrip(
                data.CenterToRightEdge, MeshBuilder.Weights1, data.Center.Index, 0f, data.Center.RequiresYPerturb,
                edgeTwo,                weights2,             data.Right .Index, v2, false,
                owner.Template.Color, MeshBuilder.Culture
            );

            for(int i = 2; i < RenderConfig.TerraceSteps; i++) {
                EdgeVertices edgeOne  = edgeTwo;
                float        v1       = v2;
                Color        weights1 = weights2;

                edgeTwo  = RenderConfig.TerraceLerp(data.CenterToRightEdge, data.RightToCenterEdge, i);
                v2       = RenderConfig.TerraceLerp(0f, 1f, i);
                weights2 = RenderConfig.TerraceLerp(MeshBuilder.Weights1, MeshBuilder.Weights2, i);

                MeshBuilder.TriangulateEdgeStrip(
                    edgeOne, weights1, data.Center.Index, v1, false,
                    edgeTwo, weights2, data.Right .Index, v2, false,
                    owner.Template.Color, MeshBuilder.Culture
                );
            }

            MeshBuilder.TriangulateEdgeStrip(
                edgeTwo,                weights2,             data.Center.Index, v2, false,
                data.RightToCenterEdge, MeshBuilder.Weights2, data.Right .Index, 1f, data.Right.RequiresYPerturb,
                owner.Template.Color, MeshBuilder.Culture
            );
        }

        private void TriangulateCultureCorner(
            CellTriangulationData data, ICivilization centerOwner
        ){
            var leftOwner = CivTerritoryLogic.GetCivClaimingCell(data.Left);

            if(data.CenterToLeftEdgeType == HexEdgeType.Slope) {

                if(data.CenterToRightEdgeType == HexEdgeType.Slope && leftOwner == centerOwner) {
                    if(data.LeftToRightEdgeType == HexEdgeType.Flat) {
                        TriangulateCultureCornerTerraces_CutThroughCorner(data, centerOwner);
                    }else {
                        TriangulateCultureCornerTerraces_RunAlongCorner(data, centerOwner);
                    }
                }
            }else if(data.CenterToLeftEdgeType == HexEdgeType.Flat && data.CenterToRightEdgeType == HexEdgeType.Flat) {
                TriangulateCultureCornerSimple(data, centerOwner);
            }
        }

        //Despite its name, we're not actually triangulating culture in the
        //traditional corner here. Instead, we're building out our culture
        //just to the side of our corner, connecting a border below a
        //terraced slop and one above and across a terraced slope.
        private void TriangulateCultureCornerTerraces_RunAlongCorner(
            CellTriangulationData data, ICivilization owner
        ){
            Vector3 indices = new Vector3(
                data.Center.Index, data.Left.Index, data.Right.Index
            );

            //This triangle attaches us to the terraces we'll be ascending
            MeshBuilder.AddTriangleUnperturbed(
                data.CenterToRightCultureStartEdgePerturbed.V1, MeshBuilder.Weights1, new Vector2(0f, 0f),
                data.CenterToLeftEdgePerturbed.V4,              MeshBuilder.Weights1, new Vector2(0f, 0f),
                data.CenterToLeftEdgePerturbed.V5,              MeshBuilder.Weights1, new Vector2(0f, 0.5f),
                owner.Template.Color, indices, MeshBuilder.Culture
            );

            //We only ascend up the nearest segment of CenterLeftEdge's terraces
            Vector3 leftVertexTwo  = RenderConfig.TerraceLerp(data.CenterToLeftEdge.V4, data.LeftToCenterEdge.V4, 1);
            Vector3 rightVertexTwo = RenderConfig.TerraceLerp(data.CenterToLeftEdge.V5, data.LeftToCenterEdge.V5, 1);

            Color weightsTwo = RenderConfig.TerraceLerp(MeshBuilder.Weights1, MeshBuilder.Weights2, 1);

            MeshBuilder.AddQuadUnperturbed(
                data.CenterToLeftEdgePerturbed.V4,      MeshBuilder.Weights1, new Vector2(0f, 0f),
                data.CenterToLeftEdgePerturbed.V5,      MeshBuilder.Weights1, new Vector2(0f, 0.5f),
                NoiseGenerator.Perturb(leftVertexTwo),  weightsTwo,           new Vector2(0f, 0f),
                NoiseGenerator.Perturb(rightVertexTwo), weightsTwo,           new Vector2(0f, 0.5f),
                owner.Template.Color, indices, MeshBuilder.Culture
            );

            for(int i = 2; i < RenderConfig.TerraceSteps; i++) {
                Vector3 leftVertexOne  = leftVertexTwo;
                Vector3 rightVertexOne = rightVertexTwo;

                Color weightsOne = weightsTwo;

                leftVertexTwo  = RenderConfig.TerraceLerp(data.CenterToLeftEdge.V4, data.LeftToCenterEdge.V4, i);
                rightVertexTwo = RenderConfig.TerraceLerp(data.CenterToLeftEdge.V5, data.LeftToCenterEdge.V5, i);

                weightsTwo = RenderConfig.TerraceLerp(MeshBuilder.Weights1, MeshBuilder.Weights2, i);

                MeshBuilder.AddQuad(
                    leftVertexOne,  weightsOne, new Vector2(0f, 0f),
                    rightVertexOne, weightsOne, new Vector2(0f, 0.5f),
                    leftVertexTwo,  weightsTwo, new Vector2(0f, 0f),
                    rightVertexTwo, weightsTwo, new Vector2(0f, 0.5f),
                    owner.Template.Color, indices, MeshBuilder.Culture
                );
            }

            MeshBuilder.AddQuadUnperturbed(
                NoiseGenerator.Perturb(leftVertexTwo),  weightsTwo,           new Vector2(0f, 0f),
                NoiseGenerator.Perturb(rightVertexTwo), weightsTwo,           new Vector2(0f, 0.5f),
                data.LeftToCenterEdgePerturbed.V4,      MeshBuilder.Weights2, new Vector2(0f, 0f),
                data.LeftToCenterEdgePerturbed.V5,      MeshBuilder.Weights2, new Vector2(0f, 0.5f),
                owner.Template.Color, indices, MeshBuilder.Culture
            );

            //There's still a missing triangle above the terraces that we need to fill in
            MeshBuilder.AddTriangleUnperturbed(
                data.LeftToCenterEdgePerturbed.V4,            MeshBuilder.Weights2, new Vector2(0f, 0f),
                data.LeftToRightCultureStartEdgePerturbed.V5, MeshBuilder.Weights2, new Vector2(0f, 0f),
                data.LeftToCenterEdgePerturbed.V5,            MeshBuilder.Weights2, new Vector2(0f, 0.5f),
                owner.Template.Color, indices, MeshBuilder.Culture
            );
        }

        //This method requires some complexity. In order to conserve (at least a little bit)
        //the width of the border, we need to triangulate from the middle of the corner
        //to somewhere along the terraced edge. To do this, we create a line from
        //CenterToLeftEdgePerturbed.V4 and PerturbedLeftCorner, then find the closest
        //point to that line for each of our nearest terrace segments
        //
        //We actually need two quads here: one for the terraces along CenterLeftEdge
        //and another for the terraces at the corner
        private void TriangulateCultureCornerTerraces_CutThroughCorner(
            CellTriangulationData data, ICivilization owner
        ) {           
            Vector3 indices = new Vector3(
                data.Center.Index, data.Left.Index, data.Right.Index
            );
             
            //As in the corner-avoiding case, we build a triangle up to the terraces
            MeshBuilder.AddTriangleUnperturbed(
                data.CenterToRightCultureStartEdgePerturbed.V1, MeshBuilder.Weights1, new Vector2(0f, 0f),
                data.CenterToLeftEdgePerturbed.V4,              MeshBuilder.Weights1, new Vector2(0f, 0f),
                data.CenterToLeftEdgePerturbed.V5,              MeshBuilder.Weights1, new Vector2(0f, 0.5f),
                owner.Template.Color, indices, MeshBuilder.Culture
            );

            Vector3 v4ToLeftCorner = data.PerturbedLeftCorner - data.CenterToLeftEdgePerturbed.V4;

            //Each section of the triangulation has three important points on the terrace:
            //the terrace edge's v4 (which helps define where the border starts), the
            //vertex at the left edge of the corner proper, and the vertex at the right edge
            //of the corner proper
            Vector3 v4TerracePointTwo = NoiseGenerator.Perturb(RenderConfig.TerraceLerp(data.CenterToLeftEdge.V4, data.LeftToCenterEdge.V4, 1));
            Vector3 cornerLeftTerracePointTwo  = NoiseGenerator.Perturb(RenderConfig.TerraceLerp(data.CenterCorner, data.LeftCorner, 1));
            Vector3 cornerRightTerracePointTwo = NoiseGenerator.Perturb(RenderConfig.TerraceLerp(data.CenterCorner, data.RightCorner, 1));

            Vector3 cultureStartTwo, guidePoint;

            //Find the location on the CenterLeftEdge terraces where our culture should start
            Geometry3D.ClosestPointsOnTwoLines(
                data.PerturbedLeftCorner, v4ToLeftCorner,
                v4TerracePointTwo, cornerLeftTerracePointTwo - v4TerracePointTwo,
                out guidePoint, out cultureStartTwo
            );

            Vector3 cultureMiddleTwo = cornerLeftTerracePointTwo;

            //The end of the border is right in the middle of the corner
            Vector3 cultureEndTwo = (cornerLeftTerracePointTwo + cornerRightTerracePointTwo) / 2f;

            Color weightsTwo = RenderConfig.TerraceLerp(MeshBuilder.Weights1, MeshBuilder.Weights2, 1);

            //Alpha calculations require some thought. All of the CultureEnd vertices have
            //a V of 0.5f, and all of the CultureStart a V of 0f. CultureMiddle lies somewhere
            //between the two. Its value is based on the distance between cultureStart/cultureMiddle
            //and cultureMiddle/cultureEnd.

            float startMiddleDistance = Vector3.Distance(cultureStartTwo, cultureMiddleTwo);
            float middleEndDistance   = Vector3.Distance(cultureMiddleTwo, cultureEndTwo);

            float middleV = Mathf.Lerp(0f, 0.5f, startMiddleDistance / (startMiddleDistance + middleEndDistance));

            //This quad gets us up the terrace on CenterLeftEdge
            MeshBuilder.AddQuadUnperturbed(
                data.CenterToLeftEdgePerturbed.V4, MeshBuilder.Weights1, new Vector2(0f, 0f),
                data.CenterToLeftEdgePerturbed.V5, MeshBuilder.Weights1, new Vector2(0f, 0.5f),
                cultureStartTwo,                   weightsTwo,           new Vector2(0f, 0f),
                cultureMiddleTwo,                  weightsTwo,           new Vector2(0f, middleV),
                owner.Template.Color, indices, MeshBuilder.Culture
            );

            //Normally we'd need a quad placed over the corner itself,
            //but we can make do with a triangle for the first step
            MeshBuilder.AddTriangleUnperturbed(
                data.PerturbedCenterCorner, MeshBuilder.Weights1, new Vector2(0f, 0.5f),
                cultureMiddleTwo,           weightsTwo,           new Vector2(0f, 0.5f),
                cultureEndTwo,              weightsTwo,           new Vector2(0f, 0.5f),
                owner.Template.Color, indices, MeshBuilder.Culture
            );

            
            for(int i = 2; i < RenderConfig.TerraceSteps; i++) {                
                Vector3 cultureStartOne  = cultureStartTwo;
                Vector3 cultureMiddleOne = cultureMiddleTwo;
                Vector3 cultureEndOne    = cultureEndTwo;

                Color weightsOne = weightsTwo;

                v4TerracePointTwo          = NoiseGenerator.Perturb(RenderConfig.TerraceLerp(data.CenterToLeftEdge.V4, data.LeftToCenterEdge.V4, i));
                cornerLeftTerracePointTwo  = NoiseGenerator.Perturb(RenderConfig.TerraceLerp(data.CenterToLeftEdge.V5, data.LeftToCenterEdge.V5, i));
                cornerRightTerracePointTwo = NoiseGenerator.Perturb(RenderConfig.TerraceLerp(data.CenterCorner, data.RightCorner, i));

                Geometry3D.ClosestPointsOnTwoLines(
                    data.PerturbedLeftCorner, v4ToLeftCorner,
                    v4TerracePointTwo, cornerLeftTerracePointTwo - v4TerracePointTwo,
                    out guidePoint, out cultureStartTwo
                );

                cultureMiddleTwo = cornerLeftTerracePointTwo;

                cultureEndTwo = (cornerLeftTerracePointTwo + cornerRightTerracePointTwo) / 2f;

                weightsTwo = RenderConfig.TerraceLerp(MeshBuilder.Weights1, MeshBuilder.Weights2, i);

                startMiddleDistance = Vector3.Distance(cultureStartTwo, cultureMiddleTwo);
                middleEndDistance   = Vector3.Distance(cultureMiddleTwo, cultureEndTwo);

                middleV = Mathf.Lerp(0f, 0.5f, startMiddleDistance / (startMiddleDistance + middleEndDistance));

                //This quad lies along the CenterLeft terraces
                MeshBuilder.AddQuadUnperturbed(
                    cultureStartOne,  weightsOne, new Vector2(0f, 0f),
                    cultureMiddleOne, weightsOne, new Vector2(0f, middleV),
                    cultureStartTwo,  weightsTwo, new Vector2(0f, 0f),
                    cultureMiddleTwo, weightsTwo, new Vector2(0f, middleV),
                    owner.Template.Color, indices, MeshBuilder.Culture
                );

                //And this one lies on the corner's terraces
                MeshBuilder.AddQuadUnperturbed(
                    cultureMiddleOne, weightsOne, new Vector2(0f, middleV),
                    cultureEndOne,    weightsOne, new Vector2(0f, 0.5f),
                    cultureMiddleTwo, weightsTwo, new Vector2(0f, middleV),
                    cultureEndTwo,    weightsTwo, new Vector2(0f, 0.5f),
                    owner.Template.Color, indices, MeshBuilder.Culture
                );
            }

            //The last segment, much like the first, consists of a quad
            //and a triangle, though this time the triangle lies on
            //CenterLeftEdge
            MeshBuilder.AddTriangleUnperturbed(
                cultureStartTwo,          weightsTwo,           new Vector2(0f, 0f),
                data.PerturbedLeftCorner, MeshBuilder.Weights2, new Vector2(0f, 0f),
                cultureMiddleTwo,         weightsTwo,           new Vector2(0f, middleV),
                owner.Template.Color, indices, MeshBuilder.Culture
            );

            Vector3 leftRightCornerMidpoint = (data.PerturbedLeftCorner + data.PerturbedRightCorner) / 2f;

            MeshBuilder.AddQuadUnperturbed(
                cultureMiddleTwo,         weightsTwo,            new Vector2(0f, middleV),
                cultureEndTwo,            weightsTwo,            new Vector2(0f, 0.5f),
                data.PerturbedLeftCorner, MeshBuilder.Weights2,  new Vector2(0f, 0f),
                leftRightCornerMidpoint,  MeshBuilder.Weights23, new Vector2(0f, 0.5f),
                owner.Template.Color, indices, MeshBuilder.Culture
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
                owner.Template.Color, indices, MeshBuilder.Culture
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
