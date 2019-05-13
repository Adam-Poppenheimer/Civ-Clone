using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using Unity.Jobs;
using Unity.Collections;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapRendering {

    public struct HeightmapJobParallelFor : IJobParallelFor {

        #region instance fields and properties

        //Array representation of the three control textures used for heightmap calculations
        [ReadOnly] public NativeArray<Color32> OrientationArray;
        [ReadOnly] public NativeArray<Color32> WeightsArray;
        [ReadOnly] public NativeArray<Color32> DuckArray;

        //Representations of various pieces of cell data we might need. Index into the arrays
        //with the cell's index to get the correct information
        [ReadOnly] public NativeArray<Vector2>   PositionXZForCellOfIndex;
        [ReadOnly] public NativeArray<CellShape> ShapeForCellOfIndex;
        [ReadOnly] public NativeArray<int>       IsUnderwaterForCellOfIndex;

        //Per-chunk constants, mostly used to sample textures and determine the world position of the sample
        public int ControlTextureWidth;
        public int ControlTextureHeight;

        public Vector3 TerrainSize;
        public Vector3 ChunkPosition;

        public int MapWidth;
        public int MapHeight;

        public float MaxTextureNormalX;
        public float MaxTextureNormalZ;

        public float IndexToNormalX;
        public float IndexToNormalZ;

        //Data pulled from RenderConfig to inform heightmap calculations
        public float OffGridElevation;
        public float OnGridElevation;

        //The heightmap we're looking to fill
        public NativeArray<float> HeightmapResults;

        #endregion

        #region instance methods

        #region from IJobParallelFor

        public void Execute(int heightmapIndex) {
            Color32 orientationColor = OrientationArray[heightmapIndex];
            Color32 weightsColor     = WeightsArray    [heightmapIndex];
            Color32 duckColor        = DuckArray       [heightmapIndex];

            HeightmapResults[heightmapIndex] = GetHeightAtPoint(Vector2.zero, orientationColor, weightsColor, duckColor);
        }

        #endregion

        private float GetHeightAtPoint(Vector2 xzPoint, Color32 orientationColor, Color32 weightsColor, Color32 duckColor) {
            int cellIndex = BitConverter.ToInt16(new byte[] { orientationColor.g, orientationColor.b }, 0) - 1;

            if(cellIndex < 0 || cellIndex >= PositionXZForCellOfIndex.Length) {
                return OffGridElevation;
            }else {
                return OnGridElevation;
            }
        }

        #endregion
        
    }

}
