using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.Profiling;

using Zenject;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapRendering {

    public class TerrainAlphamapLogic : ITerrainAlphamapLogic {

        #region instance fields and properties

        private IMapRenderConfig         RenderConfig;
        private ICellAlphamapLogic       CellAlphamapLogic;

        #endregion

        #region constructors

        [Inject]
        public TerrainAlphamapLogic(
            IMapRenderConfig renderConfig, ICellAlphamapLogic cellAlphamapLogic
        ) {
            RenderConfig      = renderConfig;
            CellAlphamapLogic = cellAlphamapLogic;

            ReusedAlphamap = new float[RenderConfig.MapTextures.Count()];
        }

        #endregion

        #region instance methods

        #region from ITerrainAlphamapLogic

        /* As elsewhere in the code, we reuse the same array here
         * to save on memory requirements. Make sure that multiple
         * uses of GetAlphamapFromOrientation are not being used
         * at the same time.
         */ 
        private float[] ReusedAlphamap;

        public float[] GetAlphamapFromOrientation(PointOrientationData orientationData) {
            for(int i = 0; i < ReusedAlphamap.Length; i++) {
                ReusedAlphamap[i] = 0f;
            }

            if(orientationData.IsOnGrid) {
                if(orientationData.RiverWeight > 0f) {
                    AddToMap(
                        ReusedAlphamap,
                        orientationData.Center.Terrain == CellTerrain.FloodPlains ? RenderConfig.FloodPlainsAlphamap : RenderConfig.RiverAlphamap,
                        orientationData.RiverWeight
                    );
                }

                if(orientationData.CenterWeight > 0f) {
                    AddToMap(
                        ReusedAlphamap,
                        CellAlphamapLogic.GetAlphamapForCell(orientationData.Center, orientationData.Sextant),
                        orientationData.CenterWeight
                    );
                }

                if(orientationData.Left != null && orientationData.LeftWeight > 0f) {
                    AddToMap(
                        ReusedAlphamap,
                        CellAlphamapLogic.GetAlphamapForCell(orientationData.Left, orientationData.Sextant.Next2()),
                        orientationData.LeftWeight
                    );
                }

                if(orientationData.Right != null && orientationData.RightWeight > 0f) {
                    AddToMap(
                        ReusedAlphamap,
                        CellAlphamapLogic.GetAlphamapForCell(orientationData.Right, orientationData.Sextant.Opposite()),
                        orientationData.RightWeight
                    );
                }

                if(orientationData.NextRight != null && orientationData.NextRightWeight > 0f) {
                    AddToMap(
                        ReusedAlphamap,
                        CellAlphamapLogic.GetAlphamapForCell(orientationData.NextRight, orientationData.Sextant.Previous2()),
                        orientationData.NextRightWeight
                    );
                }
            }

            return ReusedAlphamap;
        }

        #endregion

        private void AddToMap(float[] thisMap, float[] otherMap, float otherWeight) {
            for(int i = 0; i < thisMap.Length; i++) {
                thisMap[i] += otherMap[i] * otherWeight;
            }
        }

        #endregion
        
    }

}
