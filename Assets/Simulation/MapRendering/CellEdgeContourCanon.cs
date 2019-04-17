using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.HexMap;
using Assets.Util;

namespace Assets.Simulation.MapRendering {

    public class CellEdgeContourCanon : ICellEdgeContourCanon {

        #region instance fields and properties

        private Dictionary<IHexCell, List<Vector2>[]> ContourOfEdgeOfCell =
            new Dictionary<IHexCell, List<Vector2>[]>();



        private IMapRenderConfig RenderConfig;
        private IGeometry2D      Geometry2D;

        #endregion

        #region constructors

        [Inject]
        public CellEdgeContourCanon(IMapRenderConfig renderConfig, IGeometry2D geometry2D) {
            RenderConfig = renderConfig;
            Geometry2D   = geometry2D;
        }

        #endregion

        #region instance methods

        #region from ICellEdgeContourCanon

        public void SetContourForCellEdge(IHexCell cell, HexDirection edge, List<Vector2> contour) {
            if(cell == null) {
                throw new ArgumentNullException("cell");

            }else if(contour == null) {
                throw new ArgumentNullException("contour");

            }else if(contour.Count < 2) {
                throw new ArgumentException("Contour invalid. A valid contour must have at least 2 elements in it");
            }

            List<Vector2>[] contourArray;

            if(!ContourOfEdgeOfCell.TryGetValue(cell, out contourArray)) {
                contourArray = new List<Vector2>[6];

                ContourOfEdgeOfCell[cell] = contourArray;
            }

            var newContour = contour.ToList();

            //We want to make sure that the contours of every cell are
            //defined in a clockwise order. But our input isn't
            //guaranteed to be in that order. So we need to reverse
            //the contours if their starting point is counterclockwise
            //of their endpoint
            Vector2 cellToContourStart = newContour.First() - cell.AbsolutePositionXZ;
            Vector2 cellToContourEnd   = newContour.Last () - cell.AbsolutePositionXZ;

            if(Vector3.Cross(
                new Vector3(cellToContourStart.x, 0f, cellToContourStart.y),
                new Vector3(cellToContourEnd  .x, 0f, cellToContourEnd  .y)
            ).y < 0) {
                newContour.Reverse();
            }

            contourArray[(int)edge] = newContour;
        }

        public ReadOnlyCollection<Vector2> GetContourForCellEdge(IHexCell cell, HexDirection edge) {
            if(cell == null) {
                throw new ArgumentNullException("cell");
            }

            List<Vector2>[] contourArray;

            if(!ContourOfEdgeOfCell.TryGetValue(cell, out contourArray)) {
                contourArray = new List<Vector2>[6];

                ContourOfEdgeOfCell[cell] = contourArray;
            }
            
            if(contourArray[(int)edge] == null) {
                contourArray[(int)edge] = new List<Vector2>() {
                    cell.AbsolutePositionXZ + RenderConfig.GetFirstCornerXZ (edge),
                    cell.AbsolutePositionXZ + RenderConfig.GetSecondCornerXZ(edge)
                };
            }
            
            return contourArray[(int)edge].AsReadOnly();
        }

        public void Clear() {
            ContourOfEdgeOfCell.Clear();
        }

        public bool IsPointWithinContour(Vector2 xzPoint, IHexCell cell, HexDirection direction) {
            var contour = GetContourForCellEdge(cell, direction);
            Vector2 midpoint = cell.AbsolutePositionXZ;

            for(int i = 1; i < contour.Count; i++) {
                if(Geometry2D.IsPointWithinTriangle(xzPoint, midpoint, contour[i - 1], contour[i])) {
                    return true;
                }
            }

            return false;
        }

        //We need to crawl down the contours in opposite directions
        //because contours are always built from the first corner to
        //the second, which means that two contours opposite each-other
        //on an edge are reversed relative to each-other.
        public bool IsPointBetweenContours(
            Vector2 xzPoint, ReadOnlyCollection<Vector2> contourOne, ReadOnlyCollection<Vector2> contourTwo
        ) {
            if(contourOne == null) {
                throw new ArgumentNullException("contourOne");

            }else if(contourTwo == null) {
                throw new ArgumentNullException("contourTwo");

            }else if(contourOne.Count < 2) {
                throw new ArgumentException("contourOne has less than two points, and is therefore invalid");

            }else if(contourTwo.Count < 2) {
                throw new ArgumentException("contourTwo has less than two points, and is therefore invalid");
            }

            int oneIndex = 0;
            int twoIndex = contourTwo.Count - 1;

            for(; oneIndex < contourOne.Count - 1 && twoIndex > 0; oneIndex++, twoIndex--) {
                Vector2 contourOneP1 = contourOne[oneIndex];
                Vector2 contourOneP2 = contourOne[oneIndex + 1];

                Vector2 contourTwoP1 = contourTwo[twoIndex];
                Vector2 contourTwoP2 = contourTwo[twoIndex - 1];

                if( Geometry2D.IsPointWithinTriangle(xzPoint, contourOneP1, contourTwoP1, contourOneP2) ||
                    Geometry2D.IsPointWithinTriangle(xzPoint, contourOneP2, contourTwoP1, contourTwoP2)
                ) {
                    return true;
                }
            }

            if(contourOne.Count > contourTwo.Count) {
                for(; oneIndex < contourOne.Count - 1; oneIndex++) {
                    Vector2 contourOneP1 = contourOne[oneIndex];
                    Vector2 contourOneP2 = contourOne[oneIndex + 1];

                    if(Geometry2D.IsPointWithinTriangle(xzPoint, contourOneP1, contourTwo.First(), contourOneP2)) {
                        return true;
                    }
                }
            }else if(contourTwo.Count > contourOne.Count) {
                for(; twoIndex > 0; twoIndex--) {
                    Vector2 contourTwoP1 = contourTwo[twoIndex];
                    Vector2 contourTwoP2 = contourTwo[twoIndex - 1];

                    if(Geometry2D.IsPointWithinTriangle(xzPoint, contourOne.Last(), contourTwoP1, contourTwoP2)) {
                        return true;
                    }
                }
            }

            return false;
        }

        public bool DoesSegmentCrossContour(Vector2 segmentStart, Vector2 segmentEnd, ReadOnlyCollection<Vector2> contour) {
            if(contour == null) {
                throw new ArgumentNullException("contour");

            }else if(contour.Count < 2) {
                throw new ArgumentException("contour has less than two points, and is therefore invalid");
            }

            for(int i = 0; i < contour.Count - 1; i++) {
                Vector2 dummy;

                if(Geometry2D.AreLineSegmentsCrossing(segmentStart, segmentEnd, out dummy, contour[i], contour[i + 1], out dummy)) {
                    return true;
                }
            }

            return false;
        }

        public Vector2 GetClosestPointOnContour(Vector2 xzPoint, ReadOnlyCollection<Vector2> contour) {
            if(contour == null) {
                throw new ArgumentNullException("contour");
            }

            Vector2 closestPoint = Vector2.zero;
            float shortestDistanceSquared = float.MaxValue;

            for(int i = 0; i < contour.Count - 1; i++) {
                Vector2 candidatePoint = Geometry2D.GetClosestPointOnLineSegment(contour[i], contour[i + 1], xzPoint);

                float candidateDistanceSquared = (xzPoint - candidatePoint).sqrMagnitude;

                if(candidateDistanceSquared < shortestDistanceSquared) {
                    closestPoint = candidatePoint;
                    shortestDistanceSquared = candidateDistanceSquared;
                }
            }

            return closestPoint;
        }

        #endregion

        #endregion

    }

}
