using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.Profiling;

using Zenject;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapRendering {

    public class PointOrientationLogic : IPointOrientationLogic {

        #region instance fields and properties

        private float SolidRadiusSq;



        private IHexGrid                        Grid;
        private IPointOrientationInSextantLogic PointOrientationInSextantLogic;
        private IMapRenderConfig                RenderConfig;

        #endregion

        #region constructors

        [Inject]
        public PointOrientationLogic(
            IHexGrid grid, IPointOrientationInSextantLogic pointOrientationInSextantLogic,
            IMapRenderConfig renderConfig
        ) {
            Grid                           = grid;
            PointOrientationInSextantLogic = pointOrientationInSextantLogic;
            RenderConfig                   = renderConfig;

            SolidRadiusSq = RenderConfig.SolidFactor * RenderConfig.InnerRadius;
            SolidRadiusSq *= SolidRadiusSq;
        }

        #endregion

        #region instance methods

        #region from IPointOrientationLogic

        public PointOrientationData GetOrientationDataForPoint(Vector2 xzPoint) {
            Vector3 xyzPoint = new Vector3(xzPoint.x, 0f, xzPoint.y);

            if(!Grid.HasCellAtLocation(xyzPoint)) {
                return new PointOrientationData();
            }

            IHexCell gridCenter = Grid.GetCellAtLocation(xyzPoint);

            HexDirection gridSextant;
            Grid.TryGetSextantOfPointInCell(xzPoint, gridCenter, out gridSextant);

            PointOrientationData retval;

            if( PointOrientationInSextantLogic.TryFindValidOrientation(xzPoint, gridCenter, gridSextant,            out retval) ||
                PointOrientationInSextantLogic.TryFindValidOrientation(xzPoint, gridCenter, gridSextant.Previous(), out retval) ||
                PointOrientationInSextantLogic.TryFindValidOrientation(xzPoint, gridCenter, gridSextant.Next(),     out retval)
            ) {
                return retval;
            }
            
            IHexCell gridRight = Grid.GetNeighbor(gridCenter, gridSextant);
            
            if(gridRight != null) {
                if( PointOrientationInSextantLogic.TryFindValidOrientation(xzPoint, gridRight, gridSextant.Opposite (), out retval) ||
                    PointOrientationInSextantLogic.TryFindValidOrientation(xzPoint, gridRight, gridSextant.Next2    (), out retval) ||
                    PointOrientationInSextantLogic.TryFindValidOrientation(xzPoint, gridRight, gridSextant.Previous2(), out retval)
                ) {
                    return retval;
                }
            }

            return new PointOrientationData();
        }

        #endregion

        #endregion

    }

}
