using UnityEngine;

namespace Assets.Simulation.MapRendering {

    public interface IOrientationBaker {

        #region properties

        Texture2D OrientationTexture { get; }
        Texture2D WeightsTexture     { get; }        
        Texture2D RiverDuckTexture   { get; }

        IHexMesh OrientationMesh { get; }
        IHexMesh WeightsMesh     { get; }

        #endregion

        #region methods

        void RenderOrientationFromChunk(IMapChunk chunk);

        #endregion

    }

}