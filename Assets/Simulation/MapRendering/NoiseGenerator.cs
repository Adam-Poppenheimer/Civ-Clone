using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

namespace Assets.Simulation.MapRendering {

    public class NoiseGenerator : INoiseGenerator {

        #region instance fields and properties

        private HexHash[] HashGrid;



        private IMapRenderConfig RenderConfig;

        #endregion

        #region constructors

        public NoiseGenerator(IMapRenderConfig renderConfig) {
            RenderConfig = renderConfig;

            InitializeHashGrid(renderConfig.RandomSeed);
        }

        #endregion

        #region instance methods

        #region from INoiseGenerator

        public Vector4 SampleNoise(Vector3 position) {
            return RenderConfig.NoiseSource.GetPixelBilinear(
                position.x * RenderConfig.NoiseScale,
                position.z * RenderConfig.NoiseScale
            );
        }

        public Vector3 Perturb(Vector3 position) {
            Vector4 sample = SampleNoise(position);

            position.x += (sample.x * 2f - 1f) * RenderConfig.CellPerturbStrengthXZ;
            position.z += (sample.z * 2f - 1f) * RenderConfig.CellPerturbStrengthXZ;

            return position;
        }

        public HexHash SampleHashGrid(Vector3 position) {
            int x = (int)(position.x * RenderConfig.NoiseHashGridScale) % RenderConfig.NoiseHashGridSize;
            int z = (int)(position.z * RenderConfig.NoiseHashGridScale) % RenderConfig.NoiseHashGridSize;

            x = x < 0 ? x += RenderConfig.NoiseHashGridSize : x;
            z = z < 0 ? z += RenderConfig.NoiseHashGridSize : z;

            return HashGrid[x + z * RenderConfig.NoiseHashGridSize];
        }

        #endregion

        private void InitializeHashGrid(int seed) {
            HashGrid = new HexHash[RenderConfig.NoiseHashGridSize * RenderConfig.NoiseHashGridSize];

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
