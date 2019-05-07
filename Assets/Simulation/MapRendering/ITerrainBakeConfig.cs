using UnityEngine;

namespace Assets.Simulation.MapRendering {

    public interface ITerrainBakeConfig {

        #region properties

        int                 RenderTextureDepth  { get; }
        RenderTextureFormat RenderTextureFormat { get; }

        Shader TerrainBakeOcclusionShader { get; }

        BakedElementFlags BakeElementsHighRes   { get; }
        BakedElementFlags BakeElementsMediumRes { get; }
        BakedElementFlags BakeElementsLowRes    { get; }
        

        Vector2 BakeTextureDimensionsHighRes   { get; }
        Vector2 BakeTextureDimensionsMediumRes { get; }
        Vector2 BakeTextureDimensionsLowRes    { get; }

        #endregion

    }

}