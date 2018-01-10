using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

namespace Assets.Simulation.HexMap {

    public class NoiseGenerator : INoiseGenerator {

        #region instance fields and properties

        private const float NoiseScale = 0.003f;

        private const int HashGridSize = 256;

        private const float HashGridScale = 0.25f;

        private Texture2D NoiseSource;

        private HexHash[] HashGrid;

        #endregion

        #region constructors

        public NoiseGenerator(
            [Inject(Id = "Noise Source")] Texture2D noiseSource,
            [Inject(Id = "Random Seed")] int randomSeed
        ) {
            NoiseSource = noiseSource;
            InitializeHashGrid(randomSeed);
        }

        #endregion

        #region instance methods

        #region from INoiseGenerator

        public Vector4 SampleNoise(Vector3 position) {
            return NoiseSource.GetPixelBilinear(
                position.x * NoiseScale,
                position.z * NoiseScale
            );
        }

        public Vector3 Perturb(Vector3 position) {
            Vector4 sample = SampleNoise(position);

            position.x += (sample.x * 2f - 1f) * HexMetrics.CellPerturbStrength;
            position.z += (sample.z * 2f - 1f) * HexMetrics.CellPerturbStrength;

            return position;
        }

        public HexHash SampleHashGrid(Vector3 position) {
            int x = (int)(position.x * HashGridScale) % HashGridSize;
            int z = (int)(position.z * HashGridScale) % HashGridSize;

            x = x < 0 ? x += HashGridSize : x;
            z = z < 0 ? z += HashGridSize : z;

            return HashGrid[x + z * HashGridSize];
        }

        #endregion

        private void InitializeHashGrid(int seed) {
            HashGrid = new HexHash[HashGridSize * HashGridSize];

            Random.State currentState = Random.state;
            Random.InitState(seed);
            for(int i = 0; i < HashGrid.Length; i++) {
                HashGrid[i] = HexHash.Create();
            }
            Random.state = currentState;
        }

        #endregion

    }

}
