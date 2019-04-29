using UnityEngine;

namespace Assets.Simulation.MapRendering {

    public interface ITerrainBaker {

        #region methods

        void BakeIntoTextures(Texture2D landTexture, Texture2D waterTexture, Transform chunkTransform);

        #endregion

    }

}