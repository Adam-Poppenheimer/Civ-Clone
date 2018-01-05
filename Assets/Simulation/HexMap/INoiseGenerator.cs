using UnityEngine;

namespace Assets.Simulation.HexMap {

    public interface INoiseGenerator {

        #region methods

        Vector4 SampleNoise(Vector3 position);

        #endregion

    }

}