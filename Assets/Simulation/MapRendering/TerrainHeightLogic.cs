using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapRendering {

    public class TerrainHeightLogic : ITerrainHeightLogic {

        #region instance fields and properties

        private IPointOrientationLogic     PointOrientationLogic;
        private ICellHeightmapLogic        CellHeightmapLogic;
        private IMapRenderConfig           RenderConfig;

        #endregion

        #region constructors

        [Inject]
        public TerrainHeightLogic(
            IPointOrientationLogic pointOrientationLogic,
            ICellHeightmapLogic cellHeightmapLogic, IMapRenderConfig renderConfig
        ) {
            PointOrientationLogic = pointOrientationLogic;
            CellHeightmapLogic    = cellHeightmapLogic;
            RenderConfig          = renderConfig;
        }

        #endregion

        #region instance methods

        #region from ITerrainHeightLogic

        public float GetHeightForPoint(Vector2 xzPoint) {
            float retval = 0f;

            var orientationData = PointOrientationLogic.GetOrientationDataForPoint(xzPoint);

            if(!orientationData.IsOnGrid) {
                return retval;
            }

            if(orientationData.CenterWeight > 0f) {
                retval += orientationData.CenterWeight * CellHeightmapLogic.GetHeightForPointForCell(
                    xzPoint, orientationData.Center, orientationData.Sextant
                );
            }

            if(orientationData.Left != null && orientationData.LeftWeight > 0f) {
                retval += orientationData.LeftWeight * CellHeightmapLogic.GetHeightForPointForCell(
                    xzPoint, orientationData.Left, orientationData.Sextant.Next2()
                );
            }

            if(orientationData.Right != null && orientationData.RightWeight > 0f) {
                retval += orientationData.RightWeight * CellHeightmapLogic.GetHeightForPointForCell(
                    xzPoint, orientationData.Right, orientationData.Sextant.Opposite()
                );
            }

            if(orientationData.NextRight != null && orientationData.NextRightWeight > 0f) {
                retval += orientationData.NextRightWeight * CellHeightmapLogic.GetHeightForPointForCell(
                    xzPoint, orientationData.NextRight, orientationData.Sextant.Previous2()
                );
            }

            if(orientationData.RiverWeight > 0f) {
                retval += orientationData.RiverWeight * RenderConfig.RiverTroughElevation;
            }

            return retval;
        }

        #endregion

        #endregion
        
    }

}
