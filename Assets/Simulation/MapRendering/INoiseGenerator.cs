using UnityEngine;

namespace Assets.Simulation.MapRendering {

    public interface INoiseGenerator {

        #region methods

        Vector4 SampleNoise(Vector2 xzPosition, INoiseTexture source, float strength, NoiseType type);

        Vector3 Perturb(Vector3 position);

        HexHash SampleHashGrid(Vector2 xzPosition);

        #endregion

    }

}