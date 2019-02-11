using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.HexMap;
using Assets.Util;

namespace Assets.Simulation.MapRendering {

    public class HeightMixingLogic : IHeightMixingLogic {

        #region instance fields and properties

        private IMapRenderConfig    RenderConfig;
        private ICellHeightmapLogic CellHeightmapLogic;
        private IGeometry2D         Geometry2D;

        #endregion

        #region constructors

        [Inject]
        public HeightMixingLogic(
            IMapRenderConfig renderConfig, ICellHeightmapLogic cellHeightmapLogic, IGeometry2D geometry2D
        ) {
            RenderConfig       = renderConfig;
            CellHeightmapLogic = cellHeightmapLogic;
            Geometry2D         = geometry2D;
        }

        #endregion

        #region instance methods

        #region from IHeightMixingLogic

        //We take CenterSolidMidpoint as the origin and use that to figure out how
        //close to Center's solid edge our Point is. Once we have that, we use that
        //to linearly interpolate between the height for Center and the height for
        //Right.
        //Local here means points defined in terms of CenterSolidMidpoint
        public float GetMixForEdgeAtPoint(IHexCell center, IHexCell right, HexDirection direction, Vector3 point) {
            Vector3 world_CenterSolidMidpoint = center.AbsolutePosition + RenderConfig.GetSolidEdgeMidpoint(direction);

            //A line going from CenterSolidMidpoint to the corresponding solid midpoint on Right
            Vector3 local_RightSolidMidpoint = right.AbsolutePosition - world_CenterSolidMidpoint + RenderConfig.GetSolidEdgeMidpoint(direction.Opposite());

            Vector3 local_Point = point - world_CenterSolidMidpoint;

            Vector3 local_PointOntoMidline = Vector3.Project(local_Point, local_RightSolidMidpoint);

            float percentRight  = local_PointOntoMidline.magnitude / local_RightSolidMidpoint.magnitude;
            float percentCenter = 1f - percentRight;

            return CellHeightmapLogic.GetHeightForPositionForCell(point, center, direction)            * percentCenter +
                   CellHeightmapLogic.GetHeightForPositionForCell(point, right,  direction.Opposite()) * percentRight;
        }

        public float GetMixForPreviousCornerAtPoint(IHexCell center, IHexCell left, IHexCell right, HexDirection direction, Vector3 point) {
            Vector2 centerCorner = center.AbsolutePositionXZ + RenderConfig.GetFirstSolidCornerXZ(direction);
            Vector2 leftCorner   = left  .AbsolutePositionXZ + RenderConfig.GetFirstSolidCornerXZ(direction.Next2());
            Vector2 rightCorner  = right .AbsolutePositionXZ + RenderConfig.GetFirstSolidCornerXZ(direction.Previous2());

            float percentCenter, percentLeft, percentRight;

            Geometry2D.GetBarycentric2D(
                new Vector2(point.x, point.z), centerCorner, leftCorner, rightCorner,
                out percentCenter, out percentLeft, out percentRight
            );

            return CellHeightmapLogic.GetHeightForPositionForCell(point, center, direction)             * percentCenter + 
                   CellHeightmapLogic.GetHeightForPositionForCell(point, left,   direction.Next2())     * percentLeft   + 
                   CellHeightmapLogic.GetHeightForPositionForCell(point, right,  direction.Previous2()) * percentRight;
        }

        public float GetMixForNextCornerAtPoint(IHexCell center, IHexCell right, IHexCell nextRight, HexDirection direction, Vector3 point) {
            Vector2 centerCorner     = center   .AbsolutePositionXZ + RenderConfig.GetSecondSolidCornerXZ(direction);
            Vector2 rightCorner      = right    .AbsolutePositionXZ + RenderConfig.GetSecondSolidCornerXZ(direction.Next2());
            Vector2 nextRightCorner  = nextRight.AbsolutePositionXZ + RenderConfig.GetSecondSolidCornerXZ(direction.Previous2());

            float percentCenter, percentRight, percentNextRight;

            Geometry2D.GetBarycentric2D(
                new Vector2(point.x, point.z), centerCorner, rightCorner, nextRightCorner,
                out percentCenter, out percentRight, out percentNextRight
            );

            return CellHeightmapLogic.GetHeightForPositionForCell(point, center,    direction)             * percentCenter + 
                   CellHeightmapLogic.GetHeightForPositionForCell(point, right,     direction.Next2())     * percentRight  + 
                   CellHeightmapLogic.GetHeightForPositionForCell(point, nextRight, direction.Previous2()) * percentNextRight;
        }

        #endregion

        #endregion
        
    }

}
