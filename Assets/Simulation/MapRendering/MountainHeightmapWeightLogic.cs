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
            Vector2 pointXZ, IHexCell cell, HexDirection sextant,
            out float peakWeight, out float ridgeWeight, out float hillsWeight
        ) {
            Vector2 cellFirstSolidCorner  = cell.AbsolutePositionXZ + RenderConfig.GetFirstSolidCornerXZ (sextant);
            Vector2 cellSecondSolidCorner = cell.AbsolutePositionXZ + RenderConfig.GetSecondSolidCornerXZ(sextant);
            Vector2 cellSolidEdgeMidpoint = cell.AbsolutePositionXZ + RenderConfig.GetSolidEdgeMidpointXZ(sextant);

            IHexCell neighbor = Grid.HasNeighbor(cell, sextant) ? Grid.GetNeighbor(cell, sextant) : null;

            //These triangles check the solid sextant. Both the peak and the ridge should drop to zero as they approach
            //the solid corners of the hex.
            if(Geometry2D.IsPointWithinTriangle(pointXZ, cell.AbsolutePositionXZ, cellFirstSolidCorner, cellSolidEdgeMidpoint)) {                
                Geometry2D.GetBarycentric2D(
                    pointXZ, cell.AbsolutePositionXZ, cellFirstSolidCorner, cellSolidEdgeMidpoint,
                    out peakWeight, out hillsWeight, out ridgeWeight
                );
            }else if(Geometry2D.IsPointWithinTriangle(pointXZ, cell.AbsolutePositionXZ, cellSolidEdgeMidpoint, cellSecondSolidCorner)) {
                Geometry2D.GetBarycentric2D(
                    pointXZ, cell.AbsolutePositionXZ, cellSolidEdgeMidpoint, cellSecondSolidCorner,
                    out peakWeight, out ridgeWeight, out hillsWeight
                );
            }else if(neighbor != null) {
                peakWeight = 0f;

                Vector2 neighborFirstSolidCorner  = neighbor.AbsolutePositionXZ + RenderConfig.GetFirstSolidCornerXZ (sextant.Opposite());
                Vector2 neighborSecondSolidCorner = neighbor.AbsolutePositionXZ + RenderConfig.GetSecondSolidCornerXZ(sextant.Opposite());

                float weightA, weightB, weightC;

                //The first two triangles check the surfaces adjacent to the corners.
                //The third spans the middle
                if(Geometry2D.IsPointWithinTriangle(pointXZ, cellSolidEdgeMidpoint, cellFirstSolidCorner, neighborSecondSolidCorner)) {
                    Geometry2D.GetBarycentric2D(
                        pointXZ, cellSolidEdgeMidpoint, cellFirstSolidCorner, neighborSecondSolidCorner,
                        out weightA, out weightB, out weightC
                    );

                    ridgeWeight = weightA;
                    hillsWeight = weightB + weightC;

                }else if(Geometry2D.IsPointWithinTriangle(pointXZ, cellSolidEdgeMidpoint, neighborFirstSolidCorner, cellSecondSolidCorner)) {
                    Geometry2D.GetBarycentric2D(
                        pointXZ, cellSolidEdgeMidpoint, neighborFirstSolidCorner, cellSecondSolidCorner,
                        out weightA, out weightB, out weightC
                    );

                    ridgeWeight = weightA;
                    hillsWeight = weightB + weightC;

                }else if(Geometry2D.IsPointWithinTriangle(pointXZ, cellSolidEdgeMidpoint, neighborFirstSolidCorner, neighborSecondSolidCorner)) {
                    Geometry2D.GetBarycentric2D(
                        pointXZ, cellSolidEdgeMidpoint, neighborFirstSolidCorner, neighborSecondSolidCorner,
                        out weightA, out weightB, out weightC
                    );

                    hillsWeight = Mathf.Abs(weightB - weightC);
                    ridgeWeight = 1f - hillsWeight;                    

                }else {
                    ridgeWeight = 0f;
                    hillsWeight = 1f;
                }

            }else {
                peakWeight  = 0f;
                ridgeWeight = 0f;
                hillsWeight = 1f;
            }
        }

        #endregion

        #endregion

    }

}
