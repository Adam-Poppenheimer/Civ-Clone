using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.HexMap;
using Assets.Util;

namespace Assets.Simulation.MapRendering {

    public class MountainHeightmapWeightLogic : IMountainHeightmapWeightLogic {

        #region instance fields and properties

        private IMapRenderConfig      RenderConfig;
        private IHexGrid              Grid;
        private IGeometry2D           Geometry2D;
        private ICellEdgeContourCanon CellEdgeContourCanon;

        #endregion

        #region constructors

        [Inject]
        public MountainHeightmapWeightLogic(
            IMapRenderConfig renderConfig, IHexGrid grid, IGeometry2D geometry2D,
            ICellEdgeContourCanon cellEdgeContourCanon
        ) {
            RenderConfig         = renderConfig;
            Grid                 = grid;
            Geometry2D           = geometry2D;
            CellEdgeContourCanon = cellEdgeContourCanon;
        }

        #endregion

        #region instance methods

        #region from IMountainHeightmapWeightLogic

        //The goal here is to have the mountain rise to a point at the center
        //and taper off to hill elevation along the edges. Edges with adjacent
        //mountains should form ridges, shorter than the peak but taller than
        //the edges, that connect mountains to each-other to form ranges.

        //The solid region of each mountain sextant into a pair of triangles, with one
        //vertex at the center of the cell, one vertex at the midpoint of the
        //outer edge, and one at each of the non-solid corners (each triangle goes
        //to a different corner). We then figure out which triangle the point is
        //in and figure out its barycentric coordinates in that triangle.
        //We use those coordinates to weight its elevation, with the peak,
        //the edge midpoint, and the corners all having different heights.

        //We divide the non-corner edges into three triangles and use them
        //to inform the edge and midpoint weights, setting the peak height to
        //zero.

        //And we give corner edges fully over to the edge behavior, which for
        //now is equivalent to the height algorithm used for hills
        public void GetHeightWeightsForPoint(
            Vector2 xzPoint, IHexCell cell, HexDirection sextant,
            out float peakWeight, out float ridgeWeight, out float hillsWeight
        ) {
            Vector2 nearestContourPoint = GetNearestContourPoint(xzPoint, cell, sextant);

            Vector2 contourToPoint  = xzPoint                 - nearestContourPoint;
            Vector2 contourToCenter = cell.AbsolutePositionXZ - nearestContourPoint;

            peakWeight = contourToPoint.magnitude / contourToCenter.magnitude;


            Vector2 crossbeam = RenderConfig.GetSecondCornerXZ(sextant) - RenderConfig.GetEdgeMidpointXZ(sextant);

            Vector2 edgeMidpointToPoint = xzPoint - (cell.AbsolutePositionXZ + RenderConfig.GetEdgeMidpointXZ(sextant));

            Vector2 pointOntoCrossbeam = edgeMidpointToPoint.Project(crossbeam);

            ridgeWeight = Mathf.Clamp01(1f - (pointOntoCrossbeam.magnitude / crossbeam.magnitude) - peakWeight);

            hillsWeight = Mathf.Clamp01(1f - peakWeight - ridgeWeight);
        }

        #endregion

        private Vector2 GetNearestContourPoint(Vector2 xzPoint, IHexCell cell, HexDirection sextant) {
            Vector2 nearestContourPoint = Vector2.zero;
            float nearestDistance = float.MaxValue;

            for(HexDirection direction = sextant.Previous(); direction != sextant.Next2(); direction = direction.Next()) {
                var contour = CellEdgeContourCanon.GetContourForCellEdge(cell, direction);

                Vector2 candidate = CellEdgeContourCanon.GetClosestPointOnContour(xzPoint, contour);

                float candidateDistance = Vector2.Distance(xzPoint, candidate);

                if(candidateDistance < nearestDistance) {
                    nearestContourPoint = candidate;
                    nearestDistance = candidateDistance;
                }
            }

            return nearestContourPoint;
        }

        #endregion

    }

}
