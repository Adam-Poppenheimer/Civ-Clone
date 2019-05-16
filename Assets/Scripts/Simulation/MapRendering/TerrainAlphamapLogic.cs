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
        }

        #endregion

        #region instance methods

        #region from ITerrainAlphamapLogic

        public void GetAlphamapFromOrientation(float[] returnMap, float[] intermediateMap, PointOrientationData orientationData) {
            for(int i = 0; i < returnMap.Length; i++) {
                returnMap[i] = 0f;
            }

            if(orientationData.IsOnGrid) {
                if(orientationData.RiverWeight > 0f) {
                    AddToMap(
                        returnMap,
                        orientationData.Center.Terrain == CellTerrain.FloodPlains ? RenderConfig.FloodPlainsAlphamap : RenderConfig.RiverAlphamap,
                        orientationData.RiverWeight
                    );
                }

                if(orientationData.CenterWeight > 0f) {
                    CellAlphamapLogic.GetAlphamapForCell(intermediateMap, orientationData.Center);

                    AddToMap(returnMap,intermediateMap, orientationData.CenterWeight);
                }

                if(orientationData.Left != null && orientationData.LeftWeight > 0f) {
                    CellAlphamapLogic.GetAlphamapForCell(intermediateMap, orientationData.Left);

                    AddToMap(returnMap, intermediateMap, orientationData.LeftWeight);
                }

                if(orientationData.Right != null && orientationData.RightWeight > 0f) {
                    CellAlphamapLogic.GetAlphamapForCell(intermediateMap, orientationData.Right);

                    AddToMap(returnMap, intermediateMap, orientationData.RightWeight);
                }

                if(orientationData.NextRight != null && orientationData.NextRightWeight > 0f) {
                    CellAlphamapLogic.GetAlphamapForCell(intermediateMap, orientationData.NextRight);

                    AddToMap(returnMap, intermediateMap, orientationData.NextRightWeight);
                }
            }
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
