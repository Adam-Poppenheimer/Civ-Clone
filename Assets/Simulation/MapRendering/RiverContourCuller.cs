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

    public class RiverContourCuller : IRiverContourCuller {

        #region instance fields and properties

        private IHexGrid              Grid;
        private IRiverCanon           RiverCanon;
        private ICellEdgeContourCanon CellEdgeContourCanon;
        private IRiverSectionCanon    RiverSectionCanon;
        private IRiverAssemblyCanon   RiverAssemblyCanon;
        private IGeometry2D           Geometry2D;

        #endregion

        #region constructors

        [Inject]
        public RiverContourCuller(
            IHexGrid grid, IRiverCanon riverCanon, ICellEdgeContourCanon cellEdgeContourCanon,
            IRiverSectionCanon riverSectionCanon, IRiverAssemblyCanon riverAssemblyCanon,
            IGeometry2D geometry2D
        ) {
            Grid                 = grid;
            RiverCanon           = riverCanon;
            CellEdgeContourCanon = cellEdgeContourCanon;
            RiverSectionCanon    = riverSectionCanon;
            RiverAssemblyCanon   = riverAssemblyCanon;
            Geometry2D           = geometry2D;
        }

        #endregion

        #region instance methods

        #region from IRiverContourCuller

        public void CullConfluenceContours() {
            foreach(var center in Grid.Cells) {
                for(var direction = HexDirection.NE; direction <= HexDirection.NW; direction++) {
                    IHexCell left = Grid.GetNeighbor(center, direction.Previous());

                    if( RiverCanon.HasRiverAlongEdge(center, direction.Previous()) &&
                        RiverCanon.HasRiverAlongEdge(center, direction           ) &&
                        RiverCanon.HasRiverAlongEdge(left,   direction.Next()    )
                    ) {
                        IHexCell right = Grid.GetNeighbor(center, direction);

                        RiverSection centerLeftSection  = RiverSectionCanon.GetSectionBetweenCells(center, left);
                        RiverSection centerRightSection = RiverSectionCanon.GetSectionBetweenCells(center, right);

                        bool shouldCullContours = true;

                        foreach(var river in RiverAssemblyCanon.Rivers) {
                            int leftIndex  = river.IndexOf(centerLeftSection);
                            int rightIndex = river.IndexOf(centerRightSection);

                            bool areAdjacentInRiver = ((leftIndex + 1) == rightIndex) || ((rightIndex + 1) == leftIndex);

                            if(leftIndex != -1 && rightIndex != -1 && areAdjacentInRiver) {
                                shouldCullContours = false;
                                break;
                            }
                        }

                        if(shouldCullContours) {
                            CullContours(center, left, right, direction);
                        }
                    }
                }
            }
        }

        #endregion

        private void CullContours(
            IHexCell center, IHexCell left, IHexCell right, HexDirection direction
        ) {
            List<Vector2> centerLeftContour  = CellEdgeContourCanon.GetContourForCellEdge(center, direction.Previous()).ToList();
            List<Vector2> centerRightContour = CellEdgeContourCanon.GetContourForCellEdge(center, direction           ).ToList();

            Vector2 middleToLeftStart = centerLeftContour.First() - center.AbsolutePositionXZ;
            Vector2 middleToLeftEnd   = centerLeftContour.Last () - center.AbsolutePositionXZ;

            Vector2 middleToRightStart = centerRightContour.First() - center.AbsolutePositionXZ;
            Vector2 middleToRightEnd   = centerRightContour.Last () - center.AbsolutePositionXZ;

            Vector3 middleToLeftStartXYZ = new Vector3(middleToLeftStart.x, 0f, middleToLeftStart.y);
            Vector3 middleToLeftEndXYZ   = new Vector3(middleToLeftEnd  .x, 0f, middleToLeftEnd  .y);

            Vector3 middleToRightStartXYZ = new Vector3(middleToRightStart.x, 0f, middleToRightStart.y);
            Vector3 middleToRightEndXYZ   = new Vector3(middleToRightEnd  .x, 0f, middleToRightEnd  .y);

            //Because our contours lie on the XZ plane, this allows us to determine
            //whether our contour is pointing clockwise or counterclockwise. We want
            //to make sure that centerLeftContour ends near the Center/Left/Right
            //corner and that centerRightContour begins there            
            if(Vector3.Cross(middleToLeftStartXYZ, middleToLeftEndXYZ).y < 0) {
                centerLeftContour.Reverse();
            }

            if(Vector3.Cross(middleToRightStartXYZ, middleToRightEndXYZ).y < 0) {
                centerRightContour.Reverse();
            }

            int lastOutsideLeftIndex  = centerLeftContour.Count - 1;
            int lastOutsideRightIndex = 0;

            Vector2 contourIntersect = Vector2.zero;

            bool hasFoundIntersect = false;

            for(; lastOutsideRightIndex < centerRightContour.Count - 1; lastOutsideRightIndex++) {
                if(Geometry2D.AreLineSegmentsCrossing(
                    centerLeftContour[0],                      centerLeftContour[0] + (centerLeftContour[0] - centerLeftContour[1]), out contourIntersect,
                    centerRightContour[lastOutsideRightIndex], centerRightContour[lastOutsideRightIndex + 1],                        out contourIntersect
                )) {
                    hasFoundIntersect = true;
                    break;
                }
            }

            if(!hasFoundIntersect) {
                for(; lastOutsideLeftIndex > 0; lastOutsideLeftIndex--) {
                    if(Geometry2D.AreLineSegmentsCrossing(
                        centerLeftContour[lastOutsideLeftIndex ], centerLeftContour [lastOutsideLeftIndex  - 1],                           out contourIntersect,
                        centerRightContour[0],                    centerRightContour[0] + (centerRightContour[0] - centerRightContour[1]), out contourIntersect
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

                    }else {
                        Vector2 lastRight = centerRightContour.Last();
                        Vector2 secondLastRight = centerRightContour[centerRightContour.Count - 2];

                        if(Geometry2D.AreLineSegmentsCrossing(
                            centerLeftContour [lastOutsideLeftIndex ], centerLeftContour [lastOutsideLeftIndex  - 1], out contourIntersect,
                            lastRight,                                 lastRight + (lastRight - secondLastRight),     out contourIntersect
                        )) {
                            hasFoundIntersect = true;
                            break;
                        }
                    }
                }
            }

            if(!hasFoundIntersect) {
                for(lastOutsideRightIndex = 0; lastOutsideRightIndex < centerRightContour.Count - 1; lastOutsideRightIndex++) {
                    Vector2 lastLeft = centerLeftContour.Last();
                    Vector2 secondLastLeft = centerLeftContour[centerLeftContour.Count - 2];

                    if(Geometry2D.AreLineSegmentsCrossing(
                        lastLeft,                                  lastLeft + (lastLeft - secondLastLeft),        out contourIntersect,
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

    }

}
