using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.Profiling;

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

        public void ApplyLandBesideRiverWeights(Vector2 xzPoint, PointOrientationData data) {
            data.CenterWeight = 1f;
        }

        public void ApplyLandBesideLandWeights(
            Vector2 xzPoint, PointOrientationData data, bool hasPreviousRiver, bool hasNextRiver
        ) {
            Profiler.BeginSample("PointOrientationWeightLogic.ApplyLandBesideLandWeights()");

            if(data.Right == null) {
                data.CenterWeight = 1f;
                Profiler.EndSample();
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

            Profiler.EndSample();
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

        //We solve this problem by dividing the corner into six triangles,
        //then using the barycentric coordinates within these triangles to
        //determine the relative center, left, right, and river weights
        public void GetRiverCornerWeights(
            Vector2 xzPoint, IHexCell center, IHexCell left, IHexCell right, HexDirection sextant,
            out float centerWeight, out float leftWeight, out float rightWeight, out float riverHeightWeight,
            out float riverAlphaWeight
        ) {
            centerWeight = 0f; leftWeight = 0f; rightWeight = 0f; riverHeightWeight = 0f; riverAlphaWeight = 1f;

            var centerRightContour = CellEdgeContourCanon.GetContourForCellEdge(center, sextant           );
            var leftCenterContour  = CellEdgeContourCanon.GetContourForCellEdge(left,   sextant.Next2   ());
            var rightCenterContour = CellEdgeContourCanon.GetContourForCellEdge(right,  sextant.Opposite());

            Vector2 centerCorner = centerRightContour.First();
            Vector2 leftCorner   = leftCenterContour .First();
            Vector2 rightCorner  = rightCenterContour.Last();
            
            Vector2 centerLeftMidpoint  = (centerCorner + leftCorner ) / 2f;
            Vector2 centerRightMidpoint = (centerCorner + rightCorner) / 2f;
            Vector2 leftRightMidpoint   = (leftCorner   + rightCorner) / 2f;

            Vector2 riverMidpoint = (centerCorner + leftCorner + rightCorner) / 3f;

            float coordA, coordB, coordC;

            //Triangles spanning the CenterLeft edge
            if(Geometry2D.IsPointWithinTriangle(xzPoint, centerCorner, centerLeftMidpoint, riverMidpoint)) {

                Geometry2D.GetBarycentric2D(
                    xzPoint, centerCorner, centerLeftMidpoint, riverMidpoint,
                    out coordA, out coordB, out coordC
                );

                centerWeight = coordA;
                riverHeightWeight = Mathf.Max(coordB, coordC);

            }else if(Geometry2D.IsPointWithinTriangle(xzPoint, centerLeftMidpoint, leftCorner, riverMidpoint)) {

                Geometry2D.GetBarycentric2D(
                    xzPoint, centerLeftMidpoint, leftCorner, riverMidpoint,
                    out coordA, out coordB, out coordC
                );

                leftWeight  = coordB;
                riverHeightWeight = Mathf.Max(coordA, coordC);

            //Triangles spanning the CenterRight edge
            }else if(Geometry2D.IsPointWithinTriangle(xzPoint, centerCorner, riverMidpoint, centerRightMidpoint)) {

                Geometry2D.GetBarycentric2D(
                    xzPoint, centerCorner, riverMidpoint, centerRightMidpoint,
                    out coordA, out coordB, out coordC
                );

                centerWeight = coordA;
                riverHeightWeight  = Mathf.Max(coordB, coordC);

            }else if(Geometry2D.IsPointWithinTriangle(xzPoint, centerRightMidpoint, riverMidpoint, rightCorner)) {

                Geometry2D.GetBarycentric2D(
                    xzPoint, centerRightMidpoint, riverMidpoint, rightCorner,
                    out coordA, out coordB, out coordC
                );

                rightWeight = coordC;
                riverHeightWeight = Mathf.Max(coordA, coordB);

            //Triangles spanning the LeftRight edge
            }else if(Geometry2D.IsPointWithinTriangle(xzPoint, rightCorner, riverMidpoint, leftRightMidpoint)) {

                Geometry2D.GetBarycentric2D(
                    xzPoint, rightCorner, riverMidpoint, leftRightMidpoint,
                    out coordA, out coordB, out coordC
                );

                rightWeight = coordA;
                riverHeightWeight = Mathf.Max(coordB, coordC);

            }else if(Geometry2D.IsPointWithinTriangle(xzPoint, riverMidpoint, leftCorner, leftRightMidpoint)) {

                Geometry2D.GetBarycentric2D(
                    xzPoint, riverMidpoint, leftCorner, leftRightMidpoint,
                    out coordA, out coordB, out coordC
                );

                leftWeight  = coordB;
                riverHeightWeight = Mathf.Max(coordA, coordC);
            }
        }

        #endregion

        #endregion

    }

}
