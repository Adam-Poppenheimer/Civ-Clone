using UnityEngine;

namespace Assets.Simulation.MapRendering {

    public interface ITerrainBaker {

        #region methods

        void BakeIntoChunk(IMapChunk chunk);

        #endregion

    }

}