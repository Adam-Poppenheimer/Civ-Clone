using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapRendering {

    public class TerrainAlphamapLogic : ITerrainAlphamapLogic {

        #region instance fields and properties

        private IMapRenderConfig         RenderConfig;
        private IPointOrientationLogic   PointOrientationLogic;
        private ICellAlphamapLogic       CellAlphamapLogic;

        #endregion

        #region constructors

        [Inject]
        public TerrainAlphamapLogic(
            IMapRenderConfig renderConfig,IPointOrientationLogic pointOrientationLogic,
            ICellAlphamapLogic cellAlphamapLogic
        ) {
            RenderConfig            = renderConfig;
            PointOrientationLogic   = pointOrientationLogic;
            CellAlphamapLogic       = cellAlphamapLogic;
        }

        #endregion

        #region instance methods

        #region from ITerrainAlphamapLogic

        public float[] GetAlphamapForPoint(Vector2 xzPoint) {
            var orientationData = PointOrientationLogic.GetOrientationDataForPoint(xzPoint);

            if(!orientationData.IsOnGrid) {
                return new float[RenderConfig.MapTextures.Count()];
            }

            if(orientationData.RiverWeight > 0f) {
                return RenderConfig.RiverAlphamap;
            }

            var retval = new float[RenderConfig.MapTextures.Count()];

            if(orientationData.CenterWeight > 0f) {
                AddToMap(
                    retval,
                    CellAlphamapLogic.GetAlphamapForPointForCell(xzPoint, orientationData.Center, orientationData.Sextant),
                    orientationData.CenterWeight
                );
            }

            if(orientationData.Left != null && orientationData.LeftWeight > 0f) {
                AddToMap(
                    retval,
                    CellAlphamapLogic.GetAlphamapForPointForCell(xzPoint, orientationData.Left, orientationData.Sextant.Next2()),
                    orientationData.LeftWeight
                );
            }

            if(orientationData.Right != null && orientationData.RightWeight > 0f) {
                AddToMap(
                    retval,
                    CellAlphamapLogic.GetAlphamapForPointForCell(xzPoint, orientationData.Right, orientationData.Sextant.Opposite()),
                    orientationData.RightWeight
                );
            }

            if(orientationData.NextRight != null && orientationData.NextRightWeight > 0f) {
                AddToMap(
                    retval,
                    CellAlphamapLogic.GetAlphamapForPointForCell(xzPoint, orientationData.NextRight, orientationData.Sextant.Previous2()),
                    orientationData.NextRightWeight
                );
            }

            return retval;
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
