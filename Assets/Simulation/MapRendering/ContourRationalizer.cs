using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.HexMap;
using Assets.Util;

namespace Assets.Simulation.MapRendering {

    public class ContourRationalizer : IContourRationalizer {

        #region instance fields and properties

        private ICellEdgeContourCanon CellEdgeContourCanon;
        private IGeometry2D           Geometry2D;

        #endregion

        #region constructors

        [Inject]
        public ContourRationalizer(ICellEdgeContourCanon cellEdgeContourCanon, IGeometry2D geometry2D) {
            CellEdgeContourCanon = cellEdgeContourCanon;
            Geometry2D           = geometry2D;
        }

        #endregion

        #region instance methods

        #region from IContourRationalizer
        /*
         * There are a handful of possible cases for this method.
         * 
         * In the first case, CenterLeftContour and CenterRightContour intersect each-other.
         * In that case, we want to remove all of the points in each contour beyond the other
         * and then join them at the intersect. We do this by checking the contours in opposite
         * directions. Because all contours are oriented clockwise around their cell, if we
         * check for line segment collision carefully we'll know exactly which points need to
         * be removed. Any intersection will appear close to the beginning of CenterRightContour
         * and the end of CenterLeftContour. Thus we work up CenterRightContour and down
         * CenterLeftContour. Once we find an intersection, we strip points from the beginning of
         * CenterRightContour and the end of CenterLeftContour and add the intersection point at
         * the beginning and end, respectively.
         * 
         * In the second case, CenterLeftContour doesn't touch CenterRightContour but CenterRightContour
         * extends undesirably into the rivers. To handle this, we extrapolate forward from
         * CenterLeftContour using its last and second to last points, check for an intersection,
         * and then cull points and add the intersection as above.
         * 
         * In the third case, CenterRightContour doesn't touch CenterLeftContour but CenterLeftContour
         * extends undesirably into the rivers. We handle this much the same way, extrapolating
         * backwards from the first and second points of CenterRightContour, checking for an intersecton,
         * and culling as needed.
         * 
         * In the fourth case, neither contours touch each-other or extend into the rivers. In that case,
         * we extrapolate on both contours (forward for CenterLeft, backward for CenterRight) and add
         * their intersection without culling.
         */
        public void RationalizeCellContours(
            IHexCell center, HexDirection direction
        ) {
            List<Vector2> centerLeftContour  = CellEdgeContourCanon.GetContourForCellEdge(center, direction.Previous()).ToList();
            List<Vector2> centerRightContour = CellEdgeContourCanon.GetContourForCellEdge(center, direction           ).ToList();

            int lastOutsideLeftIndex  = centerLeftContour.Count - 1;
            int lastOutsideRightIndex = 0;

            Vector2 contourIntersect = Vector2.zero;

            bool hasFoundIntersect = false;

            //We multiply our extrapolating vector by some relatively large number to make it more likely
            //that they intersect properly.
            for(; lastOutsideLeftIndex > 0; lastOutsideLeftIndex--) {
                if(Geometry2D.AreLineSegmentsCrossing(
                    centerLeftContour[lastOutsideLeftIndex ], centerLeftContour [lastOutsideLeftIndex  - 1],                                out contourIntersect,
                    centerRightContour[0],                    centerRightContour[0] + (centerRightContour[0] - centerRightContour[1]) * 10, out contourIntersect
                )) {
                    hasFoundIntersect = true;
                    break;
                }

                for(lastOutsideRightIndex = 0; lastOutsideRightIndex < centerRightContour.Count - 1; lastOutsideRightIndex++) {
                    if(Geometry2D.AreLineSegmentsCrossing(
                        centerLeftContour [lastOutsideLeftIndex ], centerLeftContour [lastOutsideLeftIndex  - 1], out contourIntersect,
                        centerRightContour[lastOutsideRightIndex], centerRightContour[lastOutsideRightIndex + 1], out contourIntersect
                    )) {
                        hasFoundIntersect = true;
                        break;
                    }
                }

                if(hasFoundIntersect) {
                    break;
                }
            }

            if(!hasFoundIntersect) {
                Vector2 lastLeft = centerLeftContour.Last();
                Vector2 secondLastLeft = centerLeftContour[centerLeftContour.Count - 2];

                if(Geometry2D.AreLineSegmentsCrossing(
                    lastLeft,              lastLeft + (lastLeft - secondLastLeft) * 10,                                  out contourIntersect,
                    centerRightContour[0], centerRightContour[0] + (centerRightContour[0] - centerRightContour[1]) * 10, out contourIntersect
                )) {
                    hasFoundIntersect = true;
                }

                for(lastOutsideRightIndex = 0; lastOutsideRightIndex < centerRightContour.Count - 1; lastOutsideRightIndex++) {
                    if(Geometry2D.AreLineSegmentsCrossing(
                        lastLeft,                                  lastLeft + (lastLeft - secondLastLeft) * 10,   out contourIntersect,
                        centerRightContour[lastOutsideRightIndex], centerRightContour[lastOutsideRightIndex + 1], out contourIntersect
                    )) {
                        hasFoundIntersect = true;
                        break;
                    }
                }
            }

            if(lastOutsideLeftIndex > 0) {
                centerLeftContour.RemoveRange(lastOutsideLeftIndex, centerLeftContour.Count - lastOutsideLeftIndex);
            }

            if(lastOutsideRightIndex < centerRightContour.Count - 1) {
                centerRightContour.RemoveRange(0, lastOutsideRightIndex + 1);
            }

            if(hasFoundIntersect) {
                centerLeftContour.Add(contourIntersect);

                centerRightContour.Insert(0, contourIntersect);
            }

            CellEdgeContourCanon.SetContourForCellEdge(center, direction.Previous(), centerLeftContour);
            CellEdgeContourCanon.SetContourForCellEdge(center, direction,            centerRightContour);
        }

        #endregion

        #endregion

    }

}
