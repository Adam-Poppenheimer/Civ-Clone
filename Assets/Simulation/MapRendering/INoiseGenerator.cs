using UnityEngine;

namespace Assets.Simulation.MapRendering {

    public interface INoiseGenerator {

        #region methods

        Vector4 SampleNoise(Vector3 position);

        float SampleElevationNoise(Vector3 position);

        Vector3 Perturb(Vector3 position);

        HexHash SampleHashGrid(Vector3 position);

        #endregion

    }

}