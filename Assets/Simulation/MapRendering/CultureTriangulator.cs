using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.Civilizations;
using Assets.Simulation.HexMap;
using Assets.Util;

using UnityCustomUtilities.Extensions;

namespace Assets.Simulation.MapRendering {

    public class CultureTriangulator : ICultureTriangulator {

        #region instance fields and properties

        private IHexGrid                    Grid;
        private ICivilizationTerritoryLogic CivTerritoryLogic;
        private ICellEdgeContourCanon       CellEdgeContourCanon;
        private IMapRenderConfig            RenderConfig;
        private IHexMesh                    CultureMesh;
        private IGeometry2D                 Geometry2D;

        #endregion

        #region constructors

        [Inject]
        public CultureTriangulator(
            IHexGrid grid, ICivilizationTerritoryLogic civTerritoryLogic,
            ICellEdgeContourCanon cellEdgeContourCanon, IMapRenderConfig renderConfig,
            [Inject(Id = "Culture Mesh")] IHexMesh cultureMesh, IGeometry2D geometry2D
        ) {
            Grid                 = grid;
            CivTerritoryLogic    = civTerritoryLogic;
            CellEdgeContourCanon = cellEdgeContourCanon;
            RenderConfig         = renderConfig;
            CultureMesh          = cultureMesh;
            Geometry2D           = geometry2D;
        }

        #endregion

        #region instance methods

        #region from ICultureTriangulator

        public void TriangulateCulture() {
            CultureMesh.Clear();

            foreach(var cell in Grid.Cells) {
                var civOwningCell = CivTerritoryLogic.GetCivClaimingCell(cell);

                if(civOwningCell == null) {
                    continue;
                }

                foreach(var direction in EnumUtil.GetValues<HexDirection>()) {
                    TriangulateCultureInDirection(cell, direction, civOwningCell);
                }
            }

            CultureMesh.Apply();
        }

        #endregion

        private void TriangulateCultureInDirection(IHexCell center, HexDirection direction, ICivilization centerOwner) {
            IHexCell left      = Grid.GetNeighbor(center, direction.Previous());
            IHexCell right     = Grid.GetNeighbor(center, direction);
            IHexCell nextRight = Grid.GetNeighbor(center, direction.Next());

            if(right == null) {
                return;
            }

            if(CivTerritoryLogic.GetCivClaimingCell(right) != centerOwner) {
                TriangulateCultureAlongContour(center, direction, centerOwner.Template.Color);

            }else {
                var centerRightContour = CellEdgeContourCanon.GetContourForCellEdge(center, direction);

                if(centerRightContour.Count == 2) {
                    TriangulateCultureCorners_FlatEdge(center, left, nextRight, direction, centerOwner, centerRightContour);

                }else if(centerRightContour.Count == 3) {
                    TriangulateCultureCorners_FlatRiverEndpoint(center, left, nextRight, direction, centerOwner, centerRightContour);

                }else {
                    TriangulateCultureCorners_River(center, left, right, nextRight, direction, centerOwner, centerRightContour);
                }
            }
        }

        private void TriangulateCultureCorners_FlatEdge(
            IHexCell center, IHexCell left, IHexCell nextRight, HexDirection direction,
            ICivilization centerOwner, ReadOnlyCollection<Vector2> centerRightContour
        ) {
            if(left != null && CivTerritoryLogic.GetCivClaimingCell(left) != centerOwner) {
                var centerLeftContour = CellEdgeContourCanon.GetContourForCellEdge(center, direction.Previous());

                Vector2 arcInnerStart = Vector2.Lerp(centerLeftContour .Last (), center.AbsolutePositionXZ, RenderConfig.CultureWidthPercent);
                Vector2 arcInnerEnd   = Vector2.Lerp(centerRightContour.First(), centerRightContour.Last(), RenderConfig.CultureWidthPercent);

                Vector2 arcPivot = centerRightContour.First();

                TriangulateContourSweep(arcInnerStart, arcInnerEnd, arcPivot, centerOwner.Template.Color);
            }

            if(nextRight != null && CivTerritoryLogic.GetCivClaimingCell(nextRight) != centerOwner) {
                var centerNextRightContour = CellEdgeContourCanon.GetContourForCellEdge(center, direction.Next());

                Vector2 arcInnerStart = Vector2.Lerp(centerRightContour    .Last (), centerRightContour.First(), RenderConfig.CultureWidthPercent);
                Vector2 arcInnerEnd   = Vector2.Lerp(centerNextRightContour.First(), center.AbsolutePositionXZ,  RenderConfig.CultureWidthPercent);

                Vector2 arcPivot = centerRightContour.Last();

                TriangulateContourSweep(arcInnerStart, arcInnerEnd, arcPivot, centerOwner.Template.Color);
            }
        }

        private void TriangulateCultureCorners_FlatRiverEndpoint(
            IHexCell center, IHexCell left, IHexCell nextRight, HexDirection direction,
            ICivilization centerOwner, ReadOnlyCollection<Vector2> centerRightContour
        ) {
            if(left != null && CivTerritoryLogic.GetCivClaimingCell(left) != centerOwner) {
                var centerLeftContour = CellEdgeContourCanon.GetContourForCellEdge(center, direction.Previous());

                Vector2 arcInnerStart = Vector2.Lerp(centerLeftContour .Last (), center.AbsolutePositionXZ, RenderConfig.CultureWidthPercent);
                Vector2 arcInnerEnd   = Vector2.Lerp(centerRightContour.First(), centerRightContour.Last(), RenderConfig.CultureWidthPercent);

                Vector2 arcPivot = centerRightContour.First();

                TriangulateContourSweep(arcInnerStart, arcInnerEnd, arcPivot, centerOwner.Template.Color);
            }

            if(nextRight != null && CivTerritoryLogic.GetCivClaimingCell(nextRight) != centerOwner) {
                Vector2 arcInnerStart = Vector2.Lerp(centerRightContour[1], centerRightContour[0],     RenderConfig.CultureWidthPercent);
                Vector2 arcInnerEnd   = Vector2.Lerp(centerRightContour[1], center.AbsolutePositionXZ, RenderConfig.CultureWidthPercent);

                Vector2 arcPivot = centerRightContour[1];

                TriangulateContourSweep(arcInnerStart, arcInnerEnd, arcPivot, centerOwner.Template.Color);

                Vector2 bottomRight = Vector2.Lerp(centerRightContour[2], center.AbsolutePositionXZ, RenderConfig.CultureWidthPercent);

                CultureMesh.AddQuad(
                    new Vector3(arcInnerEnd.x, 0f, arcInnerEnd.y), new Vector3(bottomRight          .x, 0f, bottomRight          .y),
                    new Vector3(arcPivot   .x, 0f, arcPivot   .y), new Vector3(centerRightContour[2].x, 0f, centerRightContour[2].y)
                );

                CultureMesh.AddQuadUV(0f, 0f, 0f, 1f);
                CultureMesh.AddQuadColor(centerOwner.Template.Color);
            }
        }

        private void TriangulateCultureCorners_River(
            IHexCell center, IHexCell left, IHexCell right, IHexCell nextRight, HexDirection direction,
            ICivilization centerOwner, ReadOnlyCollection<Vector2> centerRightContour
        ) {
            if(left != null && CivTerritoryLogic.GetCivClaimingCell(left) != centerOwner) {
                float ccwTransparency = 1f, cwTransparency;

                Vector2 innerCCW, innerCW, outerCCW, outerCW;

                float cultureWidth = (centerRightContour.First() - center.AbsolutePositionXZ).magnitude * RenderConfig.CultureWidthPercent;

                int i = 0;
                do {
                    outerCCW = centerRightContour[i];
                    outerCW  = centerRightContour[i + 1];

                    float distanceFromStart = (centerRightContour.First() - outerCW).magnitude;

                    cwTransparency = Mathf.Clamp01(1f - distanceFromStart / cultureWidth);

                    innerCCW = Vector2.Lerp(outerCCW, center.AbsolutePositionXZ, RenderConfig.CultureWidthPercent);
                    innerCW  = Vector2.Lerp(outerCW,  center.AbsolutePositionXZ, RenderConfig.CultureWidthPercent);

                    CultureMesh.AddQuad(
                        new Vector3(innerCCW.x, 0f, innerCCW.y), new Vector3(innerCW.x, 0f, innerCW.y),
                        new Vector3(outerCCW.x, 0f, outerCCW.y), new Vector3(outerCW.x, 0f, outerCW.y)
                    );

                    CultureMesh.AddQuadUV(
                        new Vector2(0f, 0f),              new Vector2(0f, 0f),
                        new Vector2(0f, ccwTransparency), new Vector2(0f, cwTransparency)
                    );

                    CultureMesh.AddQuadColor(centerOwner.Template.Color);

                    ccwTransparency = cwTransparency;
                    i++;
                } while(cwTransparency > 0f && i < centerRightContour.Count - 1);
            }

            if(nextRight != null && CivTerritoryLogic.GetCivClaimingCell(nextRight) != centerOwner) {
                float cwTransparency = 1f, ccwTransparency;

                Vector2 innerCCW, innerCW, outerCCW, outerCW;

                float cultureWidth = (centerRightContour.Last() - center.AbsolutePositionXZ).magnitude * RenderConfig.CultureWidthPercent;

                int i = centerRightContour.Count - 1;
                do {
                    outerCCW = centerRightContour[i - 1];
                    outerCW  = centerRightContour[i];

                    float distanceFromStart = (centerRightContour.Last() - outerCCW).magnitude;

                    ccwTransparency = Mathf.Clamp01(1f - distanceFromStart / cultureWidth);

                    innerCCW = Vector2.Lerp(outerCCW, center.AbsolutePositionXZ, RenderConfig.CultureWidthPercent);
                    innerCW  = Vector2.Lerp(outerCW,  center.AbsolutePositionXZ, RenderConfig.CultureWidthPercent);

                    CultureMesh.AddQuad(
                        new Vector3(innerCCW.x, 0f, innerCCW.y), new Vector3(innerCW.x, 0f, innerCW.y),
                        new Vector3(outerCCW.x, 0f, outerCCW.y), new Vector3(outerCW.x, 0f, outerCW.y)
                    );

                    CultureMesh.AddQuadUV(
                        new Vector2(0f, 0f),              new Vector2(0f, 0f),
                        new Vector2(0f, ccwTransparency), new Vector2(0f, cwTransparency)
                    );

                    CultureMesh.AddQuadColor(centerOwner.Template.Color);

                    cwTransparency = ccwTransparency;
                    i--;
                }while(ccwTransparency > 0f && i > 0);
            }
        }

        private void TriangulateCultureAlongContour(IHexCell center, HexDirection direction, Color color) {
            var contour = CellEdgeContourCanon.GetContourForCellEdge(center, direction);

            Vector2 innerCCW, innerCW, outerCCW, outerCW;

            for(int i = 0; i < contour.Count - 1; i++) {
                outerCCW = contour[i];
                outerCW  = contour[i + 1];

                innerCCW = Vector2.Lerp(outerCCW, center.AbsolutePositionXZ, RenderConfig.CultureWidthPercent);
                innerCW  = Vector2.Lerp(outerCW,  center.AbsolutePositionXZ, RenderConfig.CultureWidthPercent);

                CultureMesh.AddQuad(
                    new Vector3(innerCCW.x, 0f, innerCCW.y), new Vector3(innerCW.x, 0f, innerCW.y),
                    new Vector3(outerCCW.x, 0f, outerCCW.y), new Vector3(outerCW.x, 0f, outerCW.y)
                );

                CultureMesh.AddQuadUV(0f, 0f, 0f, 1f);
                CultureMesh.AddQuadColor(color);
            }
        }

        private void TriangulateContourSweep(
            Vector2 arcInnerStart, Vector2 arcInnerEnd, Vector2 arcPivot, Color color
        ) {
            Vector3 arcPivotXYZ     = new Vector3(arcPivot.x, 0f, arcPivot.y);
            Vector3 pivotToStartXYZ = new Vector3(arcInnerStart.x - arcPivot.x, 0f, arcInnerStart.y - arcPivot.y);
            Vector3 pivotToEndXYZ   = new Vector3(arcInnerEnd  .x - arcPivot.x, 0f, arcInnerEnd  .y - arcPivot.y);

            float pivotDelta = 5f / RenderConfig.RiverQuadsPerCurve;

            Vector3 arcTwo = arcPivotXYZ + Vector3.Slerp(pivotToStartXYZ, pivotToEndXYZ, pivotDelta);

            CultureMesh.AddTriangle(arcPivotXYZ + pivotToStartXYZ, arcPivotXYZ, arcTwo);
            CultureMesh.AddTriangleUV(new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0f, 0f));
            CultureMesh.AddTriangleColor(color);
                    
            for(float t = pivotDelta; t < 1f; t += pivotDelta) {
                Vector3 arcOne = arcTwo;

                arcTwo = arcPivotXYZ + Vector3.Slerp(pivotToStartXYZ, pivotToEndXYZ, t);

                CultureMesh.AddTriangle(arcOne, arcPivotXYZ, arcTwo);
                CultureMesh.AddTriangleUV(new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0f, 0f));
                CultureMesh.AddTriangleColor(color);
            }

            CultureMesh.AddTriangle(arcTwo, arcPivotXYZ, arcPivotXYZ + pivotToEndXYZ);
            CultureMesh.AddTriangleUV(new Vector2(0f, 0f), new Vector2(0f, 1f), new Vector2(0f, 0f));
            CultureMesh.AddTriangleColor(color);
        }

        #endregion

    }

}
