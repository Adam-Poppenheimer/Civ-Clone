using System;
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

        #region internal types

        private class CachedData {

            public Vector3 CenterCellMiddle;
            public Vector2 CenterCellCenter2D;

            public HexDirection Sextant;

            public Vector2 Point2D;

            public Vector3 CenterFirst;
            public Vector3 CenterSecond;

            public Vector3 CenterFirstSolid;
            public Vector3 CenterSecondSolid;

            public Vector2 CenterFirst2D;
            public Vector2 CenterSecond2D;

            public Vector2 CenterFirstSolid2D;
            public Vector2 CenterSecondSolid2D;

            public IHexCell RightCell;

            public Vector3 RightCellMiddle;

            public Vector3 RightFirstSolid;
            public Vector3 RightSecondSolid;

            public Vector2 RightFirstSolid2D;
            public Vector2 RightSecondSolid2D;

            public Vector2 FirstCenterRightMidpoint2D;
            public Vector2 SecondCenterRightMidpoint2D;

        }

        #endregion

        #region instance fields and properties

        private HexDirection[] AllDirections;

        private CachedData Cache;



        private IMapRenderConfig RenderConfig;
        private IHexGrid         Grid;
        private IGeometry2D      Geometry2D;

        #endregion

        #region constructors

        [Inject]
        public PointOrientationLogic(
            IMapRenderConfig renderConfig, IHexGrid grid, IGeometry2D geometry2D
        ) {
            RenderConfig = renderConfig;
            Grid         = grid;
            Geometry2D   = geometry2D;

            AllDirections = EnumUtil.GetValues<HexDirection>().ToArray();

            Cache = new CachedData();
        }

        #endregion

        #region instance methods

        #region from IPointOrientationLogic

        public HexDirection GetSextantOfPointForCell(Vector3 point, IHexCell cell) {
            HexDirection bestDirection = HexDirection.NE;
            float smallestDistance = float.MaxValue;

            foreach(var direction in AllDirections) {
                float distanceToMidpoint = Vector3.Distance(point, cell.AbsolutePosition + RenderConfig.GetEdgeMidpoint(direction));

                if(distanceToMidpoint < smallestDistance) {
                    smallestDistance = distanceToMidpoint;
                    bestDirection = direction;
                }
            }

            return bestDirection;
        }

        //It's a lot easier to check orientation if we know the sextant.
        //For the inner case, we can check if the point is in a simple triangle.
        //For the edge case, we can check to see if it's in a rectangle.
        //For the corner cases, we can check to see if it's in a quad.
        public PointOrientation GetOrientationOfPointInCell(Vector3 point, IHexCell cell, HexDirection sextant) {
            Cache.Sextant = sextant;

            Cache.Point2D = new Vector2(point.x, point.z);

            if(IsPointInSolidCenter(cell)) {
                return PointOrientation.Center;
            }

            if(!Grid.HasNeighbor(cell, Cache.Sextant)) {
                return PointOrientation.Void;
            }

            Cache.RightCell = Grid.GetNeighbor(cell, Cache.Sextant);

            if(IsPointInEdge(cell)) {
                return PointOrientation.Edge;
            }

            if(Grid.HasNeighbor(cell, Cache.Sextant.Previous()) && IsPointInPreviousCorner(cell)) {
                return PointOrientation.PreviousCorner;

            }else if(Grid.HasNeighbor(cell, Cache.Sextant.Next()) && IsPointInNextCorner(cell)) {
                return PointOrientation.NextCorner;

            }else {
                return PointOrientation.Void;
            }
        }
        
        private bool IsPointInSolidCenter(IHexCell cell) {
            Cache.CenterCellMiddle = cell.AbsolutePosition;

            Cache.CenterCellCenter2D = new Vector2(Cache.CenterCellMiddle.x, Cache.CenterCellMiddle.z);

            Cache.CenterFirstSolid  = Cache.CenterCellMiddle + RenderConfig.GetFirstSolidCorner (Cache.Sextant);
            Cache.CenterSecondSolid = Cache.CenterCellMiddle + RenderConfig.GetSecondSolidCorner(Cache.Sextant);

            Cache.CenterFirstSolid2D  = new Vector2(Cache.CenterFirstSolid .x, Cache.CenterFirstSolid .z);
            Cache.CenterSecondSolid2D = new Vector2(Cache.CenterSecondSolid.x, Cache.CenterSecondSolid.z);

            return Geometry2D.IsPointWithinTriangle(Cache.Point2D, Cache.CenterCellCenter2D, Cache.CenterFirstSolid2D, Cache.CenterSecondSolid2D);
        }

        //Outer corners won't do because we need to treat tri-hex confluences differently
        //Instead, we find the midpoint between the near and far solid corners
        //of the edge and use that for our quadrilateral
        private bool IsPointInEdge(IHexCell cell) {
            Cache.RightCellMiddle = Cache.RightCell.AbsolutePosition;

            Cache.RightFirstSolid  = Cache.RightCellMiddle + RenderConfig.GetFirstSolidCorner (Cache.Sextant.Opposite());
            Cache.RightSecondSolid = Cache.RightCellMiddle + RenderConfig.GetSecondSolidCorner(Cache.Sextant.Opposite());

            Cache.RightFirstSolid2D  = new Vector2(Cache.RightFirstSolid .x, Cache.RightFirstSolid .z);
            Cache.RightSecondSolid2D = new Vector2(Cache.RightSecondSolid.x, Cache.RightSecondSolid.z);

            Cache.FirstCenterRightMidpoint2D  = (Cache.CenterFirstSolid2D  + Cache.RightSecondSolid2D) / 2f;
            Cache.SecondCenterRightMidpoint2D = (Cache.CenterSecondSolid2D + Cache.RightFirstSolid2D)  / 2f;

            return Geometry2D.IsPointWithinTriangle(
                Cache.Point2D, Cache.CenterSecondSolid2D, Cache.CenterFirstSolid2D, Cache.FirstCenterRightMidpoint2D

            ) || Geometry2D.IsPointWithinTriangle(
                Cache.Point2D, Cache.CenterSecondSolid2D, Cache.FirstCenterRightMidpoint2D, Cache.SecondCenterRightMidpoint2D
            );
        }

        //We can get away with checking a single triangle here, since we'll be checking
        //each corner 6 times (twice per cell, checking the half of the corner in the
        //current sextant each time). 
        private bool IsPointInPreviousCorner(IHexCell cell) {
            Cache.CenterFirst = Cache.CenterCellMiddle + RenderConfig.GetFirstCorner(Cache.Sextant);

            Cache.CenterFirst2D = new Vector2(Cache.CenterFirst.x, Cache.CenterFirst.z);

            return Geometry2D.IsPointWithinTriangle(
                Cache.Point2D, Cache.CenterFirstSolid2D, Cache.CenterFirst2D, Cache.FirstCenterRightMidpoint2D
            );
        }

        private bool IsPointInNextCorner(IHexCell cell) {
            Cache.CenterSecond = Cache.CenterCellMiddle + RenderConfig.GetSecondCorner(Cache.Sextant);

            Cache.CenterSecond2D = new Vector2(Cache.CenterSecond.x, Cache.CenterSecond.z);

            return Geometry2D.IsPointWithinTriangle(
                Cache.Point2D, Cache.CenterSecondSolid2D, Cache.SecondCenterRightMidpoint2D, Cache.CenterSecond2D
            );
        }

        #endregion

        #endregion
                
    }

}
