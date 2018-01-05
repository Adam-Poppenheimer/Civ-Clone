using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

namespace Assets.Simulation.HexMap {

    public class NoiseGenerator : INoiseGenerator {

        #region instance fields and properties

        private const float NoiseScale = 0.003f;

        private Texture2D NoiseSource;

        #endregion

        #region constructors

        public NoiseGenerator([Inject(Id = "Noise Source")] Texture2D noiseSource) {
            NoiseSource = noiseSource;
        }

        #endregion

        #region instance methods

        public Vector4 SampleNoise(Vector3 position) {
            return NoiseSource.GetPixelBilinear(
                position.x * NoiseScale,
                position.z * NoiseScale
            );
        }

        #endregion

    }

}
