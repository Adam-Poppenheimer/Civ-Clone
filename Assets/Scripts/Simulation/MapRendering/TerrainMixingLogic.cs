using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.HexMap;
using Assets.Util;

namespace Assets.Simulation.MapRendering {

    public class TerrainMixingLogic : ITerrainMixingLogic {

        #region instance fields and properties

        private IMapRenderConfig RenderConfig;
        private IGeometry2D      Geometry2D;

        #endregion

        #region constructors

        [Inject]
        public TerrainMixingLogic(
            IMapRenderConfig renderConfig, IGeometry2D geometry2D
        ) {
            RenderConfig = renderConfig;
            Geometry2D   = geometry2D;
        }

        #endregion

        #region instance methods

        #region from ITerrainMixingLogic

        //We take CenterSolidMidpoint as the origin and use that to figure out how
        //close to Center's solid edge our Point is. Once we have that, we use that
        //to linearly interpolate between the height for Center and the height for
        //Right.
        //Local here means points defined in terms of CenterSolidMidpoint
        public T GetMixForEdgeAtPoint<T>(
            IHexCell center, IHexCell right, HexDirection direction, Vector3 point,
            DataSelectorCallback<T> dataSelector, Func<T, T, T> aggregator
        ) {
            Vector3 world_CenterSolidMidpoint = center.AbsolutePosition + RenderConfig.GetSolidEdgeMidpoint(direction);

            //A line going from CenterSolidMidpoint to the corresponding solid midpoint on Right
            Vector3 local_RightSolidMidpoint = right.AbsolutePosition - world_CenterSolidMidpoint + RenderConfig.GetSolidEdgeMidpoint(direction.Opposite());

            Vector3 local_Point = point - world_CenterSolidMidpoint;

            Vector3 local_PointOntoMidline = Vector3.Project(local_Point, local_RightSolidMidpoint);

            float percentRight  = local_PointOntoMidline.magnitude / local_RightSolidMidpoint.magnitude;
            float percentCenter = 1f - percentRight;

            T centerContribution = dataSelector(point, center, direction,            percentCenter);
            T rightContribution  = dataSelector(point, right,  direction.Opposite(), percentRight);

            return aggregator(centerContribution, rightContribution);
        }

        public T GetMixForPreviousCornerAtPoint<T>(
            IHexCell center, IHexCell left, IHexCell right, HexDirection direction, Vector3 point,
            DataSelectorCallback<T> dataSelector, Func<T, T, T> aggregator
        ) {
            Vector2 centerCorner = center.AbsolutePositionXZ + RenderConfig.GetFirstSolidCornerXZ(direction);
            Vector2 leftCorner   = left  .AbsolutePositionXZ + RenderConfig.GetFirstSolidCornerXZ(direction.Next2());
            Vector2 rightCorner  = right .AbsolutePositionXZ + RenderConfig.GetFirstSolidCornerXZ(direction.Previous2());

            float percentCenter, percentLeft, percentRight;

            Geometry2D.GetBarycentric2D(
                new Vector2(point.x, point.z), centerCorner, leftCorner, rightCorner,
                out percentCenter, out percentLeft, out percentRight
            );

            T centerContribution = dataSelector(point, center, direction,             percentCenter);
            T leftContribution   = dataSelector(point, left,   direction.Next2(),     percentLeft);
            T rightContribution  = dataSelector(point, right,  direction.Previous2(), percentRight);

            return aggregator(aggregator(centerContribution, leftContribution), rightContribution);
        }

        public T GetMixForNextCornerAtPoint<T>(
            IHexCell center, IHexCell right, IHexCell nextRight, HexDirection direction, Vector3 point,
            DataSelectorCallback<T> dataSelector, Func<T, T, T> aggregator
        ) {
            Vector2 centerCorner     = center   .AbsolutePositionXZ + RenderConfig.GetSecondSolidCornerXZ(direction);
            Vector2 rightCorner      = right    .AbsolutePositionXZ + RenderConfig.GetSecondSolidCornerXZ(direction.Next2());
            Vector2 nextRightCorner  = nextRight.AbsolutePositionXZ + RenderConfig.GetSecondSolidCornerXZ(direction.Previous2());

            float percentCenter, percentRight, percentNextRight;

            Geometry2D.GetBarycentric2D(
                new Vector2(point.x, point.z), centerCorner, rightCorner, nextRightCorner,
                out percentCenter, out percentRight, out percentNextRight
            );

            T centerContribution    = dataSelector(point, center,    direction,             percentCenter);
            T rightContribution     = dataSelector(point, right,     direction.Next2(),     percentRight);
            T nextRightContribution = dataSelector(point, nextRight, direction.Previous2(), percentNextRight);

            return aggregator(aggregator(centerContribution, rightContribution), nextRightContribution);
        }

        #endregion

        #endregion

    }

}
