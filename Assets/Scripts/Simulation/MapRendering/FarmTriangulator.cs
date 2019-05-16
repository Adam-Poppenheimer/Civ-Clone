using System;
using System.Collections;
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

            public Color Color;

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

        private class FarmLine {

            public Vector2 Point, Direction;

            public FarmLine(Vector2 point, Vector2 direction) {
                Point     = point;
                Direction = direction;
            }

        }

        #endregion

        #region instance fields and properties

        private IHexGrid                  Grid;
        private IImprovementLocationCanon ImprovementLocationCanon;
        private INoiseGenerator           NoiseGenerator;
        private IMapRenderConfig          RenderConfig;
        private IGeometry2D               Geometry2D;
        private ICellEdgeContourCanon     CellContourCanon;
        private IRiverCanon               RiverCanon;
        private MapRenderingSignals       MapRenderingSignals;

        #endregion

        #region constructors

        [Inject]
        public FarmTriangulator(
            IHexGrid grid, IImprovementLocationCanon improvementLocationCanon, INoiseGenerator noiseGenerator,
            IMapRenderConfig renderConfig, IGeometry2D geometry2D, ICellEdgeContourCanon cellContourCanon,
            IRiverCanon riverCanon, MapRenderingSignals mapRenderingSignals
        ) {
            Grid                     = grid;
            ImprovementLocationCanon = improvementLocationCanon;
            NoiseGenerator           = noiseGenerator;
            RenderConfig             = renderConfig;
            Geometry2D               = geometry2D;
            CellContourCanon         = cellContourCanon;
            RiverCanon               = riverCanon;
            MapRenderingSignals      = mapRenderingSignals;
        }

        #endregion

        #region instance methods

        #region from IFarmTriangulator

        public void TriangulateFarmland() {
            Grid.FarmMesh.Clear();

            List<List<IHexCell>> farmBlobs = AssembleFarmBlobs();

            foreach(var farmBlob in farmBlobs) {
                List<FarmLine> toNortheastLines, toNorthwestLines;

                DrawLines(farmBlob, out toNortheastLines, out toNorthwestLines);

                List<FarmQuad> farmQuads = AssembleQuadsFromLines(farmBlob, toNortheastLines, toNorthwestLines);

                foreach(var quad in farmQuads) {
                    Grid.FarmMesh.AddQuad  (quad.BottomLeft.ToXYZ(), quad.BottomRight.ToXYZ(), quad.TopLeft.ToXYZ(), quad.TopRight.ToXYZ());
                    Grid.FarmMesh.AddQuadUV(quad.BottomLeftUV,       quad.BottomRightUV,       quad.TopLeftUV,       quad.TopRightUV);

                    Grid.FarmMesh.AddQuadColor(quad.Color);
                }
            }

            Grid.FarmMesh.Apply();

            MapRenderingSignals.FarmlandsTriangulated.OnNext(new UniRx.Unit());
        }

        #endregion

        private List<List<IHexCell>> AssembleFarmBlobs() {
            var retval = new List<List<IHexCell>>();

            var unassignedFarms = new HashSet<IHexCell>(Grid.Cells.Where(FarmCellFilter));

            var candidateQueue = new Queue<IHexCell>();

            while(unassignedFarms.Count > 0) {
                var blobCenter = unassignedFarms.First();

                var newBlob = new List<IHexCell>();

                candidateQueue.Enqueue(blobCenter);

                while(candidateQueue.Count > 0) {
                    var activeFarm = candidateQueue.Dequeue();

                    newBlob.Add(activeFarm);

                    unassignedFarms.Remove(activeFarm);

                    foreach(var direction in EnumUtil.GetValues<HexDirection>()) {
                        var neighbor = Grid.GetNeighbor(activeFarm, direction);

                        if(unassignedFarms.Contains(neighbor) && BlobExpandFilter(activeFarm, neighbor, direction)) {
                            candidateQueue.Enqueue(neighbor);
                        }
                    }
                }

                retval.Add(newBlob);
            }

            return retval;
        }

        private bool FarmCellFilter(IHexCell cell) {
            return ImprovementLocationCanon.GetPossessionsOfOwner(cell).Any(improvement => improvement.Template.ProducesFarmland);
        }

        private bool BlobExpandFilter(IHexCell inBlob, IHexCell candidate, HexDirection edgeBetween) {
            return FarmCellFilter(candidate)
                && !RiverCanon.HasRiverAlongEdge(inBlob, edgeBetween);
        }



        private Rect GetBlobRect(List<IHexCell> farmBlob) {
            float xMin = float.MaxValue, xMax = float.MinValue;
            float yMin = float.MaxValue, yMax = float.MinValue;

            foreach(var corner in farmBlob.SelectMany(cell => RenderConfig.CornersXZ.Select(corner => corner + cell.AbsolutePositionXZ))) {
                xMin = Mathf.Min(xMin, corner.x);
                xMax = Mathf.Max(xMax, corner.x);

                yMin = Mathf.Min(yMin, corner.y);
                yMax = Mathf.Max(yMax, corner.y);
            }

            return Rect.MinMaxRect(xMin, yMin, xMax, yMax);
        }



        /* The intention here is to find the AABB of the blob
         * and then draw another quad that contains the AABB but
         * is aligned to the edges of the hexagons rather than
         * world axes. We then use that box's edges to draw lines
         * that go across the hex diagonally.
         */ 
        private void DrawLines(
            List<IHexCell> farmBlob, out List<FarmLine> toNortheastLines, out List<FarmLine> toNorthwestLines
        ) {
            if(RenderConfig.FarmPatchMaxWidth <= 0f) {
                throw new InvalidOperationException("RenderConfig.FarmPatchMaxWidth must be greater than zero");
            }

            toNortheastLines = new List<FarmLine>();
            toNorthwestLines = new List<FarmLine>();

            Rect boundingRect = GetBlobRect(farmBlob);

            Vector2 aabbBottomLeft  = new Vector2(boundingRect.xMin, boundingRect.yMin);
            Vector2 aabbBottomRight = new Vector2(boundingRect.xMax, boundingRect.yMin);
            Vector2 aabbTopLeft     = new Vector2(boundingRect.xMin, boundingRect.yMax);
            Vector2 aabbTopRight    = new Vector2(boundingRect.xMax, boundingRect.yMax);

            Vector2 toNortheastDirection = RenderConfig.GetEdgeMidpointXZ(HexDirection.NE).normalized;

            Vector2 toNorthwestDirection = new Vector2(toNortheastDirection.y, -toNortheastDirection.x).normalized;

            Vector2 eastPoint, southPoint, westPoint;

            Geometry2D.ClosestPointsOnTwoLines(
                aabbBottomRight, toNortheastDirection,
                aabbTopRight,    toNorthwestDirection,
                out eastPoint, out eastPoint
            );

            Geometry2D.ClosestPointsOnTwoLines(
                aabbBottomLeft,  toNorthwestDirection,
                aabbBottomRight, toNortheastDirection,
                out southPoint, out southPoint
            );

            Geometry2D.ClosestPointsOnTwoLines(
                aabbBottomLeft, toNorthwestDirection,
                aabbTopLeft,    toNortheastDirection,
                out westPoint, out westPoint
            );

            float southToWestDistance = (southPoint - westPoint).magnitude;
            float southToEastDistance = (southPoint - eastPoint).magnitude;

            //Draws the line segments going northeast
            for(float t = 0; t <= 1f; ) {
                Vector2 start = Vector2.Lerp(southPoint, westPoint, t);

                toNortheastLines.Add(new FarmLine(start, toNortheastDirection));

                HexHash hash = NoiseGenerator.SampleHashGrid(start);

                t += Mathf.Lerp(RenderConfig.FarmPatchMinWidth, RenderConfig.FarmPatchMaxWidth, hash.A) / southToWestDistance;
            }

            //Draws the line segments going northwest
            for(float t = 0; t <= 1f; ) {
                Vector2 start = Vector2.Lerp(southPoint, eastPoint, t);

                toNorthwestLines.Add(new FarmLine(start, toNorthwestDirection));

                HexHash hash = NoiseGenerator.SampleHashGrid(start);

                t += Mathf.Lerp(RenderConfig.FarmPatchMinWidth, RenderConfig.FarmPatchMaxWidth, hash.A) / southToEastDistance;
            }
        }



        private List<FarmQuad> AssembleQuadsFromLines(
            List<IHexCell> farmBlob, List<FarmLine> toNortheastLines, List<FarmLine> toNorthwestLines
        ) {
            var retval = new List<FarmQuad>();

            for(int northeastIndex = 0; northeastIndex < toNortheastLines.Count - 1; northeastIndex++) {
                for(int northwestIndex = 0; northwestIndex < toNorthwestLines.Count - 1; northwestIndex++) {
                    Vector2 northVertex, eastVertex, southVertex, westVertex;

                    Geometry2D.ClosestPointsOnTwoLines(
                        toNortheastLines[northeastIndex + 1].Point, toNortheastLines[northeastIndex + 1].Direction,
                        toNorthwestLines[northwestIndex + 1].Point, toNorthwestLines[northwestIndex + 1].Direction,
                        out northVertex, out northVertex
                    );

                    Geometry2D.ClosestPointsOnTwoLines(
                        toNortheastLines[northeastIndex    ].Point, toNortheastLines[northeastIndex    ].Direction,
                        toNorthwestLines[northwestIndex + 1].Point, toNorthwestLines[northwestIndex + 1].Direction,
                        out eastVertex, out eastVertex
                    );

                    Geometry2D.ClosestPointsOnTwoLines(
                        toNortheastLines[northeastIndex].Point, toNortheastLines[northeastIndex].Direction,
                        toNorthwestLines[northwestIndex].Point, toNorthwestLines[northwestIndex].Direction,
                        out southVertex, out southVertex
                    );

                    Geometry2D.ClosestPointsOnTwoLines(
                        toNortheastLines[northeastIndex + 1].Point, toNortheastLines[northeastIndex + 1].Direction,
                        toNorthwestLines[northwestIndex    ].Point, toNorthwestLines[northwestIndex    ].Direction,
                        out westVertex, out westVertex
                    );

                    if(ShouldAddQuad(farmBlob, northVertex, eastVertex, southVertex, westVertex)) {
                        var newQuad = new FarmQuad(southVertex, eastVertex, westVertex, northVertex);

                        var hexHash = NoiseGenerator.SampleHashGrid((southVertex + eastVertex + westVertex + northVertex) / 4f);

                        GetUVPatch(hexHash, newQuad);

                        retval.Add(newQuad);
                    }
                }
            }

            return retval;
        }

        private bool ShouldAddQuad(
            List<IHexCell> farmBlob, Vector2 northVertex, Vector2 eastVertex, Vector2 southVertex, Vector2 westVertex
        ) {
            bool northValid = false, eastValid = false, southValid = false, westValid = false;

            foreach(var cell in farmBlob) {
                foreach(var direction in EnumUtil.GetValues<HexDirection>()) {
                    if(RiverCanon.HasRiverAlongEdge(cell, direction)) {
                        var contour = CellContourCanon.GetContourForCellEdge(cell, direction);

                        if( CellContourCanon.DoesSegmentCrossContour(northVertex, eastVertex, contour) ||
                            CellContourCanon.DoesSegmentCrossContour(northVertex, westVertex, contour) ||
                            CellContourCanon.DoesSegmentCrossContour(southVertex, eastVertex, contour) ||
                            CellContourCanon.DoesSegmentCrossContour(southVertex, westVertex, contour)
                        ) {
                            return false;
                        }
                    }
                    
                    northValid |= CellContourCanon.IsPointWithinContour(northVertex, cell, direction);
                    eastValid  |= CellContourCanon.IsPointWithinContour(eastVertex,  cell, direction);
                    southValid |= CellContourCanon.IsPointWithinContour(southVertex, cell, direction);
                    westValid  |= CellContourCanon.IsPointWithinContour(westVertex,  cell, direction);
                }
            }

            return northValid && eastValid && southValid && westValid;
        }



        private void GetUVPatch(
            HexHash hash, FarmQuad quad
        ) {
            if(hash.A <= 0.5f) {
                quad.BottomLeftUV  = new Vector2(0f, 0f);
                quad.BottomRightUV = new Vector2(1f, 0f);
                quad.TopLeftUV     = new Vector2(0f, 1f);
                quad.TopRightUV    = new Vector2(1f, 1f);
            }else {
                quad.BottomLeftUV  = new Vector2(0f, 0f);
                quad.BottomRightUV = new Vector2(0f, 1f);
                quad.TopLeftUV     = new Vector2(1f, 0f);
                quad.TopRightUV    = new Vector2(1f, 1f);
            }

            quad.Color = RenderConfig.FarmColors[Mathf.FloorToInt(hash.C * RenderConfig.FarmColors.Count)];
        }

        #endregion

    }

}
