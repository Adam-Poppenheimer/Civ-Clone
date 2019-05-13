using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using Unity.Collections;
using Unity.Jobs;

using Zenject;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapRendering {

    public class HeightmapRefresher : IHeightmapRefresher {

        #region instance fields and properties

        private WaitForEndOfFrame SkipFrame = new WaitForEndOfFrame();



        private IMapRenderConfig RenderConfig;
        private IHexGrid         Grid;

        #endregion

        #region constructors

        [Inject]
        public HeightmapRefresher(IMapRenderConfig renderConfig, IHexGrid grid) {
            RenderConfig     = renderConfig;
            Grid             = grid;
        }

        #endregion

        #region instance methods

        #region from IHeightmapRefresher

        public void RefreshHeightmapOfChunk(IMapChunk chunkToRefresh, ChunkOrientationData orientationData) {
            var terrainData = chunkToRefresh.Terrain.terrainData;

            NativeArray<Color32> orientationArray = orientationData.OrientationTexture.GetRawTextureData<Color32>();
            NativeArray<Color32> weightsArray     = orientationData.WeightsTexture    .GetRawTextureData<Color32>();
            NativeArray<Color32> duckArray        = orientationData.DuckTexture       .GetRawTextureData<Color32>();

            var expandedHeights = terrainData.GetHeights(0, 0, terrainData.heightmapWidth, terrainData.heightmapHeight);

            var flatHeights = new float[expandedHeights.Length];

            Buffer.BlockCopy(expandedHeights, 0, flatHeights, 0, flatHeights.Length * sizeof(float));

            NativeArray<float> heightmapArray = new NativeArray<float>(flatHeights, Allocator.TempJob);

            var positionXZForCellOfIndex   = new NativeArray<Vector2>  (Grid.Cells.Count, Allocator.TempJob);
            var shapeForCellOfIndex        = new NativeArray<CellShape>(Grid.Cells.Count, Allocator.TempJob);
            var isUnderwaterForCellOfIndex = new NativeArray<int>      (Grid.Cells.Count, Allocator.TempJob);

            for(int terrainHeight = 0; terrainHeight < terrainData.heightmapHeight; terrainHeight++) {
                for(int terrainWidth = 0; terrainWidth < terrainData.heightmapWidth; terrainWidth++) {
                    Color32 orientationColor = orientationData.OrientationTexture.GetPixel(terrainWidth, terrainHeight);
                    
                    expandedHeights[terrainHeight, terrainWidth] = GetHeightFromColors(orientationColor);
                }
            }

            heightmapArray            .Dispose();
            positionXZForCellOfIndex  .Dispose();
            shapeForCellOfIndex       .Dispose();
            isUnderwaterForCellOfIndex.Dispose();
        }

        #endregion

        private float GetHeightFromColors(Color32 orientationColor) {
            int cellIndex = BitConverter.ToInt16(new byte[] { orientationColor.r, orientationColor.g }, 0) - 1;

            if(cellIndex < 0 || cellIndex >= Grid.Cells.Count) {
                return 0f;
            }else {
                return 1f;
            }
        }

        #endregion

    }

}
