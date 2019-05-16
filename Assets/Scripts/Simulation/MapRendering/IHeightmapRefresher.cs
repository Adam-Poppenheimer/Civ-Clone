using System.Collections;

namespace Assets.Simulation.MapRendering {

    public interface IHeightmapRefresher {

        #region methods

        void RefreshHeightmapOfChunk(IMapChunk chunk, ChunkOrientationData orientationData);

        #endregion

    }

}