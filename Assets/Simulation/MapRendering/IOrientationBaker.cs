using UnityEngine;

namespace Assets.Simulation.MapRendering {

    public interface IOrientationBaker {

        #region methods

        void RenderOrientationFromMesh(Texture2D orientationTexture, Transform chunkTransform);

        #endregion

    }

}