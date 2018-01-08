using UnityEngine;

namespace Assets.Simulation.HexMap {

    public interface INoiseGenerator {

        #region methods

        Vector4 SampleNoise(Vector3 position);

        Vector3 Perturb(Vector3 position);

        #endregion

    }

}