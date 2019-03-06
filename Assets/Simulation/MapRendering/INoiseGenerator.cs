using UnityEngine;

namespace Assets.Simulation.MapRendering {

    public interface INoiseGenerator {

        #region methods

        Vector4 SampleNoise(Vector2 xzPosition, NoiseType type);

        Vector3 Perturb(Vector3 position);
        Vector3 Perturb(Vector3 position, float strength);

        HexHash SampleHashGrid(Vector2 xzPosition);

        #endregion

    }

}