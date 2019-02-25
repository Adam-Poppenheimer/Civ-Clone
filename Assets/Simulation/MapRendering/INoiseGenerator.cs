using UnityEngine;

namespace Assets.Simulation.MapRendering {

    public interface INoiseGenerator {

        #region methods

        Vector4 SampleNoise(Vector3 position, NoiseType type);

        Vector3 Perturb(Vector3 position);
        Vector3 Perturb(Vector3 position, float strength);

        HexHash SampleHashGrid(Vector3 position);

        #endregion

    }

}