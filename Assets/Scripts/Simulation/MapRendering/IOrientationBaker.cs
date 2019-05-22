using UnityEngine;

namespace Assets.Simulation.MapRendering {

    public interface IOrientationBaker {

        #region methods

        ChunkOrientationData MakeOrientationRequestForChunk(IMapChunk chunk);

        void ReleaseOrientationData(ChunkOrientationData data);

        #endregion

    }

}