using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.Profiling;

using Zenject;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapRendering {

    public class TerrainHeightLogic : ITerrainHeightLogic {

        #region instance fields and properties

        private ICellHeightmapLogic CellHeightmapLogic;
        private IMapRenderConfig    RenderConfig;

        #endregion

        #region constructors

        [Inject]
        public TerrainHeightLogic(
            ICellHeightmapLogic cellHeightmapLogic, IMapRenderConfig renderConfig
        ) {
            CellHeightmapLogic = cellHeightmapLogic;
            RenderConfig       = renderConfig;
        }

        #endregion

        #region instance methods

        #region from ITerrainHeightLogic

        public float GetHeightForPoint(Vector2 xzPoint, PointOrientationData orientationData) {
            float retval = 0f;

            if(!orientationData.IsOnGrid) {
                return retval;
            }

            if(orientationData.Center != null && orientationData.CenterWeight > 0f) {
                retval += orientationData.CenterWeight * CellHeightmapLogic.GetHeightForPointForCell(
                    xzPoint, orientationData.Center, orientationData.Sextant, orientationData.ElevationDuck
                );
            }

            if(orientationData.Left != null && orientationData.LeftWeight > 0f) {
                retval += orientationData.LeftWeight * CellHeightmapLogic.GetHeightForPointForCell(
                    xzPoint, orientationData.Left, orientationData.Sextant.Next2(), orientationData.ElevationDuck
                );
            }

            if(orientationData.Right != null && orientationData.RightWeight > 0f) {
                retval += orientationData.RightWeight * CellHeightmapLogic.GetHeightForPointForCell(
                    xzPoint, orientationData.Right, orientationData.Sextant.Opposite(), orientationData.ElevationDuck
                );
            }

            if(orientationData.NextRight != null && orientationData.NextRightWeight > 0f) {
                retval += orientationData.NextRightWeight * CellHeightmapLogic.GetHeightForPointForCell(
                    xzPoint, orientationData.NextRight, orientationData.Sextant.Previous2(), orientationData.ElevationDuck
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
