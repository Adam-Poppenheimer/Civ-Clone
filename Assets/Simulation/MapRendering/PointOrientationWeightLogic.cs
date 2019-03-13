using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.HexMap;
using Assets.Util;

namespace Assets.Simulation.MapRendering {

    public class PointOrientationWeightLogic : IPointOrientationWeightLogic {

        #region instance fields and properties

        private IMapRenderConfig      RenderConfig;
        private IGeometry2D           Geometry2D;
        private ICellEdgeContourCanon CellEdgeContourCanon;

        #endregion

        #region constructors

        [Inject]
        public PointOrientationWeightLogic(
            IMapRenderConfig renderConfig, IGeometry2D geometry2D, ICellEdgeContourCanon cellEdgeContourCanon
        ) {
            RenderConfig         = renderConfig;
            Geometry2D           = geometry2D;
            CellEdgeContourCanon = cellEdgeContourCanon;
        }

        #endregion

        #region instance methods

        #region from IPointOrientationWeightLogic

        public void ApplyLandBesideRiverWeights(Vector2 xzPoint, PointOrientationData data, bool takeFromRight) {
            if(takeFromRight) {
                data.RightWeight = 1f;
            }else {
                data.CenterWeight = 1f;
            }
        }

        public void ApplyLandBesideLandWeights(Vector2 xzPoint, PointOrientationData data) {
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

        public void ApplyLandBesideLandWeights_Edge(Vector2 xzPoint, PointOrientationData data) {
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

        public void ApplyRiverWeights(Vector2 xzPoint, PointOrientationData data) {
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

        #endregion

        #endregion

    }

}
