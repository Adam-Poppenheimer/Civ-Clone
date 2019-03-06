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
            List<Vector2>[] contourArray;

            if(!ContourOfEdgeOfCell.TryGetValue(cell, out contourArray)) {
                contourArray = new List<Vector2>[6];

                ContourOfEdgeOfCell[cell] = contourArray;
            }

            contourArray[(int)edge] = contour;
        }

        public ReadOnlyCollection<Vector2> GetContourForCellEdge(IHexCell cell, HexDirection edge) {
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

        public bool IsPointWithinContour(Vector2 xzPoint, ReadOnlyCollection<Vector2> contour, Vector2 midpoint) {
            for(int i = 1; i < contour.Count; i++) {
                if(Geometry2D.IsPointWithinTriangle(xzPoint, midpoint, contour[i - 1], contour[i])) {
                    return true;
                }
            }

            return false;
        }

        //This method assumes that the contours have the same number
        //of points, which is a safe assumption for its current use
        //cases. It returns false if they don't.
        public bool IsPointBetweenContours(
            Vector2 xzPoint, ReadOnlyCollection<Vector2> contourOne, ReadOnlyCollection<Vector2> contourTwo
        ) {
            if(contourOne.Count != contourTwo.Count) {
                return false;
            }

            for(int i = 1; i < contourOne.Count; i++) {
                Vector2 contourOneP1 = contourOne[i - 1];
                Vector2 contourOneP2 = contourOne[i];

                Vector2 contourTwoP1 = contourTwo[contourTwo.Count - i];
                Vector2 contourTwoP2 = contourTwo[contourTwo.Count - i - 1];

                if( Geometry2D.IsPointWithinTriangle(xzPoint, contourOneP1, contourTwoP1, contourOneP2) ||
                    Geometry2D.IsPointWithinTriangle(xzPoint, contourOneP2, contourTwoP1, contourTwoP2)
                ) {
                    return true;
                }
            }

            return false;
        }

        public Vector2 GetClosestPointOnContour(Vector2 xzPoint, ReadOnlyCollection<Vector2> contour) {
            Vector2 closestPoint = contour[0];
            float shortestDistanceSquared = (xzPoint - closestPoint).sqrMagnitude;

            for(int i = 1; i < contour.Count; i++) {
                Vector2 candidatePoint = Geometry2D.GetClosestPointOnLineSegment(contour[i - 1], contour[i], xzPoint);

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
