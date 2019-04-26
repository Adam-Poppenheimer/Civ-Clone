using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.Profiling;

using Zenject;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapRendering {

    public class BetterPointOrientationLogic : IBetterPointOrientationLogic {

        #region instance fields and properties

        private IHexGrid Grid;

        #endregion

        #region constructors

        [Inject]
        public BetterPointOrientationLogic(IHexGrid grid) {
            Grid = grid;
        }

        #endregion

        #region instance methods

        #region from IBetterPointOrientationLogic

        /* The current construction tries to reduce GC allocation
         * by reusing certain information. ReusedOrientationData
         * in particular can save a lot of allocations.
         * However, this will only function if we're only ever using
         * a single PointOrientationData at a time. It will fail if
         * we attempt to acquire multiple orientation data and use them
         * in tandem.
         */ 
        private byte[] indexBytes = new byte[2];
        private PointOrientationData ReusedOrientationData = new PointOrientationData();

        public PointOrientationData GetOrientationDataFromTextures(
            Vector2 textureNormal, Texture2D orientationTexture, Texture2D weightsTexture
        ) {
            Profiler.BeginSample("GetOrientationDataFromTextures");

            int texelX = Mathf.RoundToInt(orientationTexture.width  * textureNormal.x);
            int texelY = Mathf.RoundToInt(orientationTexture.height * textureNormal.y);

            Color32 orientationColor = orientationTexture.GetPixel(texelX, texelY);
            Color weightsColor       = weightsTexture.    GetPixel(texelX, texelY);

            indexBytes[0] = orientationColor.r;
            indexBytes[1] = orientationColor.g;

            int index = BitConverter.ToInt16(indexBytes, 0) - 1;

            var center = index >= 0 && index < Grid.Cells.Count ? Grid.Cells[index] : null;

            HexDirection sextant = (HexDirection)orientationColor.b;

            Profiler.BeginSample("Data creation");

            if(center != null) {
                ReusedOrientationData.IsOnGrid = true;
                ReusedOrientationData.Sextant = sextant;

                Profiler.BeginSample("Neighbor acquisition");

                ReusedOrientationData.Center    = center;
                ReusedOrientationData.Left      = Grid.GetNeighbor(center, sextant.Previous());
                ReusedOrientationData.Right     = Grid.GetNeighbor(center, sextant);
                ReusedOrientationData.NextRight = Grid.GetNeighbor(center, sextant.Next());

                Profiler.EndSample();

                ReusedOrientationData.CenterWeight    = weightsColor.r;
                ReusedOrientationData.LeftWeight      = weightsColor.g;
                ReusedOrientationData.RightWeight     = weightsColor.b;
                ReusedOrientationData.NextRightWeight = weightsColor.a;

                ReusedOrientationData.RiverWeight = Mathf.Clamp01(1f - weightsColor.r - weightsColor.g - weightsColor.b - weightsColor.a);
            }else {
                ReusedOrientationData.Clear();
            }   

            Profiler.EndSample();
            
            Profiler.EndSample();
            
            return ReusedOrientationData;         
        }

        #endregion

        #endregion
        
    }

}
