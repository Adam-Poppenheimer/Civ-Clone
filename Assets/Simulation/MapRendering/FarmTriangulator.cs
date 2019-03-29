using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.HexMap;
using Assets.Simulation.Improvements;
using Assets.Util;

using UnityCustomUtilities.Extensions;

namespace Assets.Simulation.MapRendering {

    public class FarmTriangulator : IFarmTriangulator {

        #region internal types

        private class FarmQuad {

            #region instance fields and properties

            public Vector2 BottomLeft,   BottomRight,   TopLeft,   TopRight;
            public Vector2 BottomLeftUV, BottomRightUV, TopLeftUV, TopRightUV;

            #endregion

            #region constructors

            public FarmQuad() { }

            public FarmQuad(Vector2 bottomleft, Vector2 bottomRight, Vector2 topLeft, Vector2 topRight) {
                BottomLeft  = bottomleft;
                BottomRight = bottomRight;
                TopLeft     = topLeft;
                TopRight    = topRight;
            }

            #endregion

        }

        #endregion

        #region instance fields and properties

        private IImprovementLocationCanon   ImprovementLocationCanon;
        private ICellEdgeContourCanon       CellContourCanon;
        private ITerrainConformTriangulator TerrainConformTriangulator;
        private INoiseGenerator             NoiseGenerator;
        private IMapRenderConfig            RenderConfig;
        private IHexGrid                    Grid;
        private IRiverCanon                 RiverCanon;

        #endregion

        #region constructors

        [Inject]
        public FarmTriangulator(
            IImprovementLocationCanon improvementLocationCanon, ICellEdgeContourCanon cellContourCanon,
            ITerrainConformTriangulator terrainConformTriangulator, INoiseGenerator noiseGenerator,
            IMapRenderConfig renderConfig, IHexGrid grid, IRiverCanon riverCanon
        ) {
            ImprovementLocationCanon   = improvementLocationCanon;
            CellContourCanon           = cellContourCanon;
            TerrainConformTriangulator = terrainConformTriangulator;
            NoiseGenerator             = noiseGenerator;
            RenderConfig               = renderConfig;
            Grid                       = grid;
            RiverCanon                 = riverCanon;
        }

        #endregion

        #region instance methods

        #region from IFarmTriangulator

        public void TriangulateFarmland(IHexCell cell, IHexMesh mesh) {
            var quadQueue = new Queue<FarmQuad>();

            if(!HasFarmland(cell)) {
                return;
            }

            AddFarmlandCorridor(cell, quadQueue);

            var northEastNeighbor = Grid.GetNeighbor(cell, HexDirection.NE);

            if(northEastNeighbor != null) {
                AddNorthEastFarms(cell, northEastNeighbor, quadQueue);
            }

            var eastNeighbor = Grid.GetNeighbor(cell, HexDirection.E);

            if(eastNeighbor != null) {
                AddEastFarms(cell, eastNeighbor, quadQueue);
            }

            TriangulateQuadQueue(quadQueue, mesh);
        }

        #endregion

        private bool HasFarmland(IHexCell cell) {
            return ImprovementLocationCanon.GetPossessionsOfOwner(cell).Any(improvement => improvement.Template.ProducesFarmland);
        }

        private void AddFarmlandCorridor(IHexCell cell, Queue<FarmQuad> quadQueue) {
            var northwestContour = CellContourCanon.GetContourForCellEdge(cell, HexDirection.NW);
            var southeastContour = CellContourCanon.GetContourForCellEdge(cell, HexDirection.SE);

            quadQueue.Enqueue(new FarmQuad(
                southeastContour.Last(), southeastContour.First(), northwestContour.First(), northwestContour.Last()
            ));
        }

        private void AddNorthEastFarms(IHexCell cell, IHexCell northEastNeighbor, Queue<FarmQuad> quadQueue) {
            var northEastContour = CellContourCanon.GetContourForCellEdge(cell,              HexDirection.NE);
            var southWestContour = CellContourCanon.GetContourForCellEdge(northEastNeighbor, HexDirection.SW);

            if(HasFarmland(northEastNeighbor) && !RiverCanon.HasRiverAlongEdge(cell, HexDirection.NE)) {
                quadQueue.Enqueue(new FarmQuad(
                    northEastContour.First(),                                              (cell.AbsolutePositionXZ + northEastContour.Last()) / 2f,
                    (southWestContour.Last() + northEastNeighbor.AbsolutePositionXZ) / 2f, southWestContour.First()
                ));
            }else {
                Vector2 nextSextantMidline = (cell.AbsolutePositionXZ + northEastContour.Last()) / 2f;

                Vector2 pointAOnContour = CellContourCanon.GetClosestPointOnContour(
                    Vector2.Lerp(northEastContour.First(), northEastContour.Last(), 1f / 3f), northEastContour
                );

                Vector2 pointBOnContour = CellContourCanon.GetClosestPointOnContour(
                    Vector2.Lerp(northEastContour.First(), northEastContour.Last(), 2f / 3f), northEastContour
                );

                quadQueue.Enqueue(new FarmQuad(
                    Vector2.Lerp(northEastContour.First(), nextSextantMidline, 1f / 3f),
                    Vector2.Lerp(northEastContour.First(), nextSextantMidline, 2f / 3f),
                    pointAOnContour,
                    pointBOnContour
                ));

                quadQueue.Enqueue(new FarmQuad(
                    Vector2.Lerp(northEastContour.First(), nextSextantMidline, 2f / 3f),
                    nextSextantMidline,
                    pointBOnContour,
                    northEastContour.Last()
                ));
            }
        }

        private void AddEastFarms(IHexCell cell, IHexCell eastNeighbor, Queue<FarmQuad> quadQueue) {
            var eastContour = CellContourCanon.GetContourForCellEdge(cell,         HexDirection.E);
            var westContour = CellContourCanon.GetContourForCellEdge(eastNeighbor, HexDirection.W);

            if(HasFarmland(eastNeighbor) && !RiverCanon.HasRiverAlongEdge(cell, HexDirection.E)) {
                quadQueue.Enqueue(new FarmQuad(
                    (eastContour.First() + cell.AbsolutePositionXZ) / 2f, eastContour.Last(),
                    eastContour.First(),                                  (westContour.First() + eastNeighbor.AbsolutePositionXZ) / 2f
                ));
            }else {
                Vector2 previousSextantMidline = (cell.AbsolutePositionXZ + eastContour.First()) / 2f;

                Vector2 pointAOnContour = CellContourCanon.GetClosestPointOnContour(
                    Vector2.Lerp(eastContour.First(), eastContour.Last(), 1f / 3f), eastContour
                );

                Vector2 pointBOnContour = CellContourCanon.GetClosestPointOnContour(
                    Vector2.Lerp(eastContour.First(), eastContour.Last(), 2f / 3f), eastContour
                );

                quadQueue.Enqueue(new FarmQuad(
                    previousSextantMidline,
                    Vector2.Lerp(previousSextantMidline, eastContour.Last(), 1f / 3f),
                    eastContour.First(),
                    pointAOnContour
                ));
                quadQueue.Enqueue(new FarmQuad(
                    Vector2.Lerp(previousSextantMidline, eastContour.Last(), 1f / 3f),
                    Vector2.Lerp(previousSextantMidline, eastContour.Last(), 2f / 3f),
                    pointAOnContour,
                    pointBOnContour
                ));
            }
        }

        private void TriangulateQuadQueue(Queue<FarmQuad> quadQueue, IHexMesh mesh) {
            HexHash quadHash;

            float patchWidthSqr = RenderConfig.FarmPatchMaxWidth * RenderConfig.FarmPatchMaxWidth;

            while(quadQueue.Count > 0) {
                var activeQuad = quadQueue.Dequeue();

                if( (activeQuad.BottomLeft - activeQuad.BottomRight).sqrMagnitude > patchWidthSqr ||
                    (activeQuad.TopLeft    - activeQuad.TopRight   ).sqrMagnitude > patchWidthSqr
                ) {
                    //Left quad
                    quadQueue.Enqueue(new FarmQuad(
                        activeQuad.BottomLeft, (activeQuad.BottomLeft + activeQuad.BottomRight) / 2f,
                        activeQuad.TopLeft,    (activeQuad.TopLeft    + activeQuad.TopRight   ) / 2f
                    ));

                    //Right quad
                    quadQueue.Enqueue(new FarmQuad(
                        (activeQuad.BottomLeft + activeQuad.BottomRight) / 2f, activeQuad.BottomRight,
                        (activeQuad.TopLeft    + activeQuad.TopRight   ) / 2f, activeQuad.TopRight
                    ));
                }else if(
                    (activeQuad.BottomLeft  - activeQuad.TopLeft ).sqrMagnitude > patchWidthSqr ||
                    (activeQuad.BottomRight - activeQuad.TopRight).sqrMagnitude > patchWidthSqr
                ) {
                    //Bottom quad
                    quadQueue.Enqueue(new FarmQuad(
                        activeQuad.BottomLeft,                             activeQuad.BottomRight,
                        (activeQuad.BottomLeft + activeQuad.TopLeft) / 2f, (activeQuad.BottomRight + activeQuad.TopRight) / 2f
                    ));

                    //Top quad
                    quadQueue.Enqueue(new FarmQuad(
                        (activeQuad.BottomLeft + activeQuad.TopLeft) / 2f, (activeQuad.BottomRight + activeQuad.TopRight) / 2f,
                        activeQuad.TopLeft,                                activeQuad.TopRight
                    ));
                }else {
                    quadHash = NoiseGenerator.SampleHashGrid(activeQuad.BottomLeft);

                    GetUVPatch(quadHash, activeQuad);

                    var farmColor = RenderConfig.FarmColors.Random();

                    TerrainConformTriangulator.AddConformingQuad(
                        activeQuad.BottomLeft .ToXYZ(), activeQuad.BottomLeftUV,  farmColor,
                        activeQuad.BottomRight.ToXYZ(), activeQuad.BottomRightUV, farmColor,
                        activeQuad.TopLeft    .ToXYZ(), activeQuad.TopLeftUV,     farmColor,
                        activeQuad.TopRight   .ToXYZ(), activeQuad.TopRightUV,    farmColor,
                        RenderConfig.FarmTriangleSideLength, mesh
                    );
                }
            }
        }

        private void GetUVPatch(
            HexHash hash, FarmQuad quad
        ) {
            float hexPatchX = Mathf.Floor(hash.A * RenderConfig.FarmTexturePatchCountSqrt);
            float hexPatchY = Mathf.Floor(hash.B * RenderConfig.FarmTexturePatchCountSqrt);

            quad.BottomLeftUV  = new Vector2(hexPatchX        / RenderConfig.FarmTexturePatchCountSqrt, hexPatchY        / RenderConfig.FarmTexturePatchCountSqrt);
            quad.BottomRightUV = new Vector2((hexPatchX + 1f) / RenderConfig.FarmTexturePatchCountSqrt, hexPatchY        / RenderConfig.FarmTexturePatchCountSqrt);
            quad.TopLeftUV     = new Vector2(hexPatchX        / RenderConfig.FarmTexturePatchCountSqrt, (hexPatchY + 1f) / RenderConfig.FarmTexturePatchCountSqrt);            
            quad.TopRightUV    = new Vector2((hexPatchX + 1f) / RenderConfig.FarmTexturePatchCountSqrt, (hexPatchY + 1f) / RenderConfig.FarmTexturePatchCountSqrt);
        }

        #endregion

    }

}
