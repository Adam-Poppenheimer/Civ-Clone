using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.HexMap;
using Assets.Util;

using UnityCustomUtilities.Extensions;

namespace Assets.Simulation.MapRendering {

    public class PointOrientationLogic : IPointOrientationLogic {

        #region instance fields and properties

        private HexDirection[] AllDirections;




        private IHexGrid              Grid;
        private ICellEdgeContourCanon CellEdgeContourCanon;
        private IRiverCanon           RiverCanon;
        private IGeometry2D           Geometry2D;
        private IMapRenderConfig      RenderConfig;

        #endregion

        #region constructors

        [Inject]
        public PointOrientationLogic(
            IHexGrid grid, ICellEdgeContourCanon cellEdgeContourCanon, IRiverCanon riverCanon,
            IGeometry2D geometry2D, IMapRenderConfig renderConfig
        ) {
            Grid                 = grid;
            CellEdgeContourCanon = cellEdgeContourCanon;
            RiverCanon           = riverCanon;
            Geometry2D           = geometry2D;
            RenderConfig         = renderConfig;

            AllDirections = EnumUtil.GetValues<HexDirection>().ToArray();
        }

        #endregion

        #region instance methods

        #region from IPointOrientationLogic

        public PointOrientationData GetOrientationDataForPoint(Vector2 xzPoint) {
            Vector3 xyzPoint = new Vector3(xzPoint.x, 0f, xzPoint.y);

            if(!Grid.HasCellAtLocation(xyzPoint)) {
                return new PointOrientationData();
            }

            IHexCell gridCenter = Grid.GetCellAtLocation(xyzPoint);

            HexDirection gridSextant = GetSextantOfPointInCell(xzPoint, gridCenter);

            PointOrientationData retval;

            if( TryFindValidOrientation(xzPoint, gridCenter, gridSextant,            out retval) ||
                TryFindValidOrientation(xzPoint, gridCenter, gridSextant.Previous(), out retval) ||
                TryFindValidOrientation(xzPoint, gridCenter, gridSextant.Next(),     out retval)
            ) {
                return retval;
            }else {
                return new PointOrientationData();
            }
        }

        #endregion

        private bool TryFindValidOrientation(Vector2 xzPoint, IHexCell gridCenter, HexDirection gridSextant, out PointOrientationData data) {
            data = new PointOrientationData() {
                IsOnGrid = true,
                Sextant  = gridSextant,

                Center    = gridCenter,
                Left      = Grid.GetNeighbor(gridCenter, gridSextant.Previous()),
                Right     = Grid.GetNeighbor(gridCenter, gridSextant),
                NextRight = Grid.GetNeighbor(gridCenter, gridSextant.Next()),
            };

            var centerRightContour = CellEdgeContourCanon.GetContourForCellEdge(gridCenter, gridSextant);

            if(CellEdgeContourCanon.IsPointWithinContour(xzPoint, centerRightContour, gridCenter.AbsolutePositionXZ)) {
                if(RiverCanon.HasRiverAlongEdge(gridCenter, gridSextant)) {
                    ApplyLandBesideRiverWeights(xzPoint, data, false);

                }else {
                    ApplyLandBesideLandWeights(xzPoint, data);
                }

                return true;
            }

            IHexCell gridRight = Grid.GetNeighbor(gridCenter, gridSextant);

            if(gridRight == null) {
                return false;
            }

            var rightCenterContour = CellEdgeContourCanon.GetContourForCellEdge(gridRight, gridSextant.Opposite());

            if(CellEdgeContourCanon.IsPointWithinContour(xzPoint, rightCenterContour, gridRight.AbsolutePositionXZ)) {
                if(RiverCanon.HasRiverAlongEdge(gridCenter, gridSextant)) {
                    ApplyLandBesideRiverWeights(xzPoint, data, true);

                }else {
                    ApplyLandBesideLandWeights(xzPoint, data);
                }

                return true;
            }

            if(CellEdgeContourCanon.IsPointBetweenContours(xzPoint, centerRightContour, rightCenterContour)) {
                if(RiverCanon.HasRiverAlongEdge(gridCenter, gridSextant)) {
                    ApplyRiverWeights(xzPoint, data);
                }

                return true;
            }

            return false;
        }

        private void ApplyLandBesideRiverWeights(Vector2 xzPoint, PointOrientationData data, bool takeFromRight) {
            if(takeFromRight) {
                data.RightWeight = 1f;
            }else {
                data.CenterWeight = 1f;
            }
        }

        private void ApplyLandBesideLandWeights(Vector2 xzPoint, PointOrientationData data) {
            if(data.Right == null) {
                data.CenterWeight = 1f;
                return;
            }

            if( data.Left != null &&
                Geometry2D.IsPointWithinTriangle(
                    xzPoint,
                    data.Center.AbsolutePositionXZ + RenderConfig.GetFirstSolidCornerXZ(data.Sextant),
                    data.Left  .AbsolutePositionXZ + RenderConfig.GetFirstSolidCornerXZ(data.Sextant.Next2()),
                    data.Right .AbsolutePositionXZ + RenderConfig.GetFirstSolidCornerXZ(data.Sextant.Previous2())
                )
            ) {
                Geometry2D.GetBarycentric2D(
                    xzPoint,
                    data.Center.AbsolutePositionXZ + RenderConfig.GetFirstSolidCornerXZ(data.Sextant),
                    data.Left  .AbsolutePositionXZ + RenderConfig.GetFirstSolidCornerXZ(data.Sextant.Next2()),
                    data.Right .AbsolutePositionXZ + RenderConfig.GetFirstSolidCornerXZ(data.Sextant.Previous2()),
                    out data.CenterWeight, out data.LeftWeight, out data.RightWeight
                );
            }else if(
                data.NextRight != null &&
                Geometry2D.IsPointWithinTriangle(
                    xzPoint,
                    data.Center   .AbsolutePositionXZ + RenderConfig.GetSecondSolidCornerXZ(data.Sextant),
                    data.Right    .AbsolutePositionXZ + RenderConfig.GetSecondSolidCornerXZ(data.Sextant.Next2()),
                    data.NextRight.AbsolutePositionXZ + RenderConfig.GetSecondSolidCornerXZ(data.Sextant.Previous2())
                )
            ) {
                Geometry2D.GetBarycentric2D(
                    xzPoint,
                    data.Center   .AbsolutePositionXZ + RenderConfig.GetSecondSolidCornerXZ(data.Sextant),
                    data.Right    .AbsolutePositionXZ + RenderConfig.GetSecondSolidCornerXZ(data.Sextant.Next2()),
                    data.NextRight.AbsolutePositionXZ + RenderConfig.GetSecondSolidCornerXZ(data.Sextant.Previous2()),
                    out data.CenterWeight, out data.RightWeight, out data.NextRightWeight
                );
            } else {
                ApplyLandBesideLandWeights_Edge(xzPoint, data);
            }
        }

        private void ApplyLandBesideLandWeights_Edge(Vector2 xzPoint, PointOrientationData data) {
            Vector2 centerToRight = data.Right.AbsolutePositionXZ - data.Center.AbsolutePositionXZ;

            Vector2 centerToPoint = xzPoint - data.Center.AbsolutePositionXZ;

            Vector2 pointOntoMidline = centerToPoint.Project(centerToRight);

            float solidCenterDistance = RenderConfig.GetSolidEdgeMidpointXZ(data.Sextant).magnitude;

            Vector2 centerToSolidRight = (
                data.Right.AbsolutePositionXZ + RenderConfig.GetSolidEdgeMidpointXZ(data.Sextant.Opposite())
            ) - data.Center.AbsolutePositionXZ;

            float solidRightDistance = centerToSolidRight.magnitude;

            float rightWeight = Mathf.Clamp01(
                (pointOntoMidline.magnitude - solidCenterDistance) / (solidRightDistance - solidCenterDistance)
            );

            float centerWeight = 1f - rightWeight;

            data.CenterWeight = centerWeight;
            data.RightWeight  = rightWeight;
        }

        private void ApplyRiverWeights(Vector2 xzPoint, PointOrientationData data) {
            var centerRightContour = CellEdgeContourCanon.GetContourForCellEdge(data.Center, data.Sextant);
            var rightCenterContour = CellEdgeContourCanon.GetContourForCellEdge(data.Right,  data.Sextant.Opposite());

            Vector2 pointOnContourCR = CellEdgeContourCanon.GetClosestPointOnContour(xzPoint, centerRightContour);
            Vector2 pointOnContourRC = CellEdgeContourCanon.GetClosestPointOnContour(xzPoint, rightCenterContour);

            Vector2 centerToRightMidline = pointOnContourRC - pointOnContourCR;
            Vector2 rightToCenterMidline = pointOnContourCR - pointOnContourRC;

            Vector2 contourCRToPoint = xzPoint - pointOnContourCR;
            Vector2 contourRCToPoint = xzPoint - pointOnContourRC;

            Vector2 pointCROntoMidline = contourCRToPoint.Project(centerToRightMidline);
            Vector2 pointRCOntoMidline = contourRCToPoint.Project(rightToCenterMidline);

            data.CenterWeight = 1f - Mathf.Clamp01(2 * (pointCROntoMidline.magnitude / centerToRightMidline.magnitude));
            data.RightWeight  = 1f - Mathf.Clamp01(2 * (pointRCOntoMidline.magnitude / rightToCenterMidline.magnitude));
            data.RiverWeight  = 1f - data.CenterWeight - data.RightWeight;
        }

        private HexDirection GetSextantOfPointInCell(Vector2 xzPoint, IHexCell cell) {
            foreach(var direction in AllDirections) {
                if(Geometry2D.IsPointWithinTriangle(
                    xzPoint, cell.AbsolutePositionXZ,
                    cell.AbsolutePositionXZ + RenderConfig.GetFirstCornerXZ (direction),
                    cell.AbsolutePositionXZ + RenderConfig.GetSecondCornerXZ(direction)
                )) {
                    return direction;
                }
            }

            return HexDirection.NW;
        }

        #endregion

    }

}
