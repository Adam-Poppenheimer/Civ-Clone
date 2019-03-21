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
        private IGeometry2D                 Geometry2D;
        private ITerrainConformTriangulator TerrainConformTriangulator;

        #endregion

        #region constructors

        [Inject]
        public CultureTriangulator(
            IHexGrid grid, ICivilizationTerritoryLogic civTerritoryLogic,
            ICellEdgeContourCanon cellEdgeContourCanon, IMapRenderConfig renderConfig,
            IGeometry2D geometry2D, ITerrainConformTriangulator terrainConformTriangulator
        ) {
            Grid                       = grid;
            CivTerritoryLogic          = civTerritoryLogic;
            CellEdgeContourCanon       = cellEdgeContourCanon;
            RenderConfig               = renderConfig;
            Geometry2D                 = geometry2D;
            TerrainConformTriangulator = terrainConformTriangulator;
        }

        #endregion

        #region instance methods

        #region from ICultureTriangulator

        public void TriangulateCultureInDirection(
            IHexCell center, HexDirection direction, IHexMesh cultureMesh
        ) {
            var centerOwner = CivTerritoryLogic.GetCivClaimingCell(center);

            if(centerOwner == null) {
                return;
            }

            IHexCell left      = Grid.GetNeighbor(center, direction.Previous());
            IHexCell right     = Grid.GetNeighbor(center, direction);
            IHexCell nextRight = Grid.GetNeighbor(center, direction.Next());

            if(right == null) {
                return;
            }

            if(CivTerritoryLogic.GetCivClaimingCell(right) != centerOwner) {
                TriangulateCultureAlongContour(center, direction, centerOwner.Template.Color, cultureMesh);

            }else {
                var centerRightContour = CellEdgeContourCanon.GetContourForCellEdge(center, direction);
                var rightCenterContour = CellEdgeContourCanon.GetContourForCellEdge(right,  direction.Opposite());

                if(rightCenterContour.Count <= 3) {
                    TriangulateCultureCorners_FlatEdge(
                        center, left, right, nextRight, direction, centerOwner, centerRightContour, rightCenterContour, cultureMesh
                    );
                }else {
                    TriangulateCultureCorners_River(
                        center, left, right, nextRight, direction, centerOwner, centerRightContour, cultureMesh
                    );
                }
            }
        }

        #endregion

        private void TriangulateCultureCorners_FlatEdge(
            IHexCell center, IHexCell left, IHexCell right, IHexCell nextRight, HexDirection direction,
            ICivilization centerOwner, ReadOnlyCollection<Vector2> centerRightContour,
            ReadOnlyCollection<Vector2> rightCenterContour, IHexMesh cultureMesh
        ) {
            if(left != null && CivTerritoryLogic.GetCivClaimingCell(left) != centerOwner) {
                Color cultureColor = centerOwner.Template.Color;

                var centerLeftContour = CellEdgeContourCanon.GetContourForCellEdge(center, direction.Previous ());
                var rightLeftContour  = CellEdgeContourCanon.GetContourForCellEdge(right,  direction.Previous2());

                Vector2 centerLeftFirstInner = Vector2.Lerp(centerLeftContour.First(), center.AbsolutePositionXZ, RenderConfig.CultureWidthPercent);
                Vector2 centerLeftLastInner  = Vector2.Lerp(centerLeftContour.Last (), center.AbsolutePositionXZ, RenderConfig.CultureWidthPercent);

                Vector2 rightLeftFirstInner = Vector2.Lerp(rightLeftContour.First(), right.AbsolutePositionXZ, RenderConfig.CultureWidthPercent);
                Vector2 rightleftLastInner  = Vector2.Lerp(rightLeftContour.Last (), right.AbsolutePositionXZ, RenderConfig.CultureWidthPercent);

                Vector2 rayAlongCenterLeft = (centerLeftLastInner - centerLeftFirstInner).normalized;
                Vector2 rayAlongRightLeft  = (rightLeftFirstInner - rightleftLastInner).normalized;

                Vector2 bezierControl;

                if(!Geometry2D.ClosestPointsOnTwoLines(
                    centerLeftLastInner, rayAlongCenterLeft, rightLeftFirstInner, rayAlongRightLeft,
                    out bezierControl, out bezierControl
                )) {
                    Debug.LogError("TriangulateCultureCorners_FlatEdge failed to find a valid control point");
                    return;
                }
                

                Vector3 pivotXYZ = new Vector3(centerLeftContour.Last().x, 0f, centerLeftContour.Last().y);

                float paramDelta = 5f / RenderConfig.RiverQuadsPerCurve;

                for(float t = 0; t < 1f; t = Mathf.Clamp01(t + paramDelta)) {
                    float nextT = Mathf.Clamp01(t + paramDelta);

                    Vector2 bezierOne = BezierQuadratic.GetPoint(centerLeftLastInner, bezierControl, rightLeftFirstInner, nextT);
                    Vector2 bezierTwo = BezierQuadratic.GetPoint(centerLeftLastInner, bezierControl, rightLeftFirstInner, t);

                    TerrainConformTriangulator.AddConformingTriangle(
                        pivotXYZ,                                  new Vector2(0f, 1f), cultureColor,
                        new Vector3(bezierOne.x, 0f, bezierOne.y), Vector2.zero,        cultureColor,
                        new Vector3(bezierTwo.x, 0f, bezierTwo.y), Vector2.zero,        cultureColor,
                        cultureMesh
                    );
                }

                if(rightCenterContour.Count == 3) {
                    Vector2 innerPoint          = Vector2.Lerp(rightCenterContour.Last(), right.AbsolutePositionXZ, RenderConfig.CultureWidthPercent);
                    Vector2 secondToLastContour = rightCenterContour[rightCenterContour.Count - 2];
                    Vector2 lastContour         = rightCenterContour.Last();

                    TerrainConformTriangulator.AddConformingTriangle(
                        new Vector3(innerPoint         .x, 0f, innerPoint         .y), new Vector2(0f, 0f), cultureColor,
                        new Vector3(secondToLastContour.x, 0f, secondToLastContour.y), new Vector2(0f, 1f), cultureColor,
                        new Vector3(lastContour        .x, 0f, lastContour        .y), new Vector2(0f, 1f), cultureColor,
                        cultureMesh
                    );
                }
            }
        }

        private void TriangulateCultureCorners_River(
            IHexCell center, IHexCell left, IHexCell right, IHexCell nextRight, HexDirection direction,
            ICivilization centerOwner, ReadOnlyCollection<Vector2> centerRightContour, IHexMesh cultureMesh
        ) {
            Color cultureColor = centerOwner.Template.Color;

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

                    TerrainConformTriangulator.AddConformingQuad(
                        new Vector3(innerCCW.x, 0f, innerCCW.y), new Vector2(0f, 0f),              cultureColor,
                        new Vector3(innerCW .x, 0f, innerCW .y), new Vector2(0f, 0f),              cultureColor,
                        new Vector3(outerCCW.x, 0f, outerCCW.y), new Vector2(0f, ccwTransparency), cultureColor,
                        new Vector3(outerCW .x, 0f, outerCW .y), new Vector2(0f, cwTransparency),  cultureColor,
                        cultureMesh
                    );

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

                    TerrainConformTriangulator.AddConformingQuad(
                        new Vector3(innerCCW.x, 0f, innerCCW.y), new Vector2(0f, 0f),              cultureColor,
                        new Vector3(innerCW .x, 0f, innerCW .y), new Vector2(0f, 0f),              cultureColor,
                        new Vector3(outerCCW.x, 0f, outerCCW.y), new Vector2(0f, ccwTransparency), cultureColor,
                        new Vector3(outerCW .x, 0f, outerCW .y), new Vector2(0f, cwTransparency),  cultureColor,
                        cultureMesh
                    );

                    cwTransparency = ccwTransparency;
                    i--;
                }while(ccwTransparency > 0f && i > 0);
            }
        }

        private void TriangulateCultureAlongContour(
            IHexCell center, HexDirection direction, Color color, IHexMesh cultureMesh
        ) {
            var contour = CellEdgeContourCanon.GetContourForCellEdge(center, direction);

            Vector2 innerCCW, innerCW, outerCCW, outerCW;

            for(int i = 0; i < contour.Count - 1; i++) {
                outerCCW = contour[i];
                outerCW  = contour[i + 1];

                innerCCW = Vector2.Lerp(outerCCW, center.AbsolutePositionXZ, RenderConfig.CultureWidthPercent);
                innerCW  = Vector2.Lerp(outerCW,  center.AbsolutePositionXZ, RenderConfig.CultureWidthPercent);

                TerrainConformTriangulator.AddConformingQuad(
                    new Vector3(innerCCW.x, 0f, innerCCW.y), new Vector2(0f, 0f), color,
                    new Vector3(innerCW .x, 0f, innerCW .y), new Vector2(0f, 0f), color,
                    new Vector3(outerCCW.x, 0f, outerCCW.y), new Vector2(0f, 1f), color,
                    new Vector3(outerCW .x, 0f, outerCW .y), new Vector2(0f, 1f), color,
                    cultureMesh
                );
            }
        }

        #endregion

    }

}
