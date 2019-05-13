using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.Profiling;

using Zenject;

using Assets.Simulation.HexMap;
using Assets.Util;

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

        public float GetHeightForPoint(
            Vector2 xzPoint, PointOrientationData orientationData, AsyncTextureUnsafe<Color32> flatlandsNoise,
            AsyncTextureUnsafe<Color32> hillsNoise
        ) {
            float retval = 0f;

            if(!orientationData.IsOnGrid) {
                return retval;
            }

            if(orientationData.Center != null && orientationData.CenterWeight > 0f) {
                retval += orientationData.CenterWeight * CellHeightmapLogic.GetHeightForPointForCell(
                    xzPoint, orientationData.Center, orientationData.ElevationDuck, flatlandsNoise, hillsNoise
                );
            }

            if(orientationData.Left != null && orientationData.LeftWeight > 0f) {
                retval += orientationData.LeftWeight * CellHeightmapLogic.GetHeightForPointForCell(
                    xzPoint, orientationData.Left, orientationData.ElevationDuck, flatlandsNoise, hillsNoise
                );
            }

            if(orientationData.Right != null && orientationData.RightWeight > 0f) {
                retval += orientationData.RightWeight * CellHeightmapLogic.GetHeightForPointForCell(
                    xzPoint, orientationData.Right, orientationData.ElevationDuck, flatlandsNoise, hillsNoise
                );
            }

            if(orientationData.NextRight != null && orientationData.NextRightWeight > 0f) {
                retval += orientationData.NextRightWeight * CellHeightmapLogic.GetHeightForPointForCell(
                    xzPoint, orientationData.NextRight, orientationData.ElevationDuck, flatlandsNoise, hillsNoise
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
