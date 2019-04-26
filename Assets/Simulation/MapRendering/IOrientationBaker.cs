using UnityEngine;

namespace Assets.Simulation.MapRendering {

    public interface IOrientationBaker {

        #region methods

        void RenderOrientationFromMesh(Texture2D orientationTexture, Texture2D weightsTexture, Transform chunkTransform);

        #endregion

    }

}