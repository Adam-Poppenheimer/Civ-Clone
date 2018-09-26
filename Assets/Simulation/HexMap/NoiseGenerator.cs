using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

namespace Assets.Simulation.HexMap {

    public class NoiseGenerator : INoiseGenerator {

        #region instance fields and properties

        private HexHash[] HashGrid;



        private IHexMapRenderConfig RenderConfig;

        #endregion

        #region constructors

        public NoiseGenerator(IHexMapRenderConfig renderConfig) {
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

        public Vector3 Perturb(Vector3 position, bool perturbY = false) {
            Vector4 sample = SampleNoise(position);

            position.x += (sample.x * 2f - 1f) * RenderConfig.CellPerturbStrengthXZ;
            position.z += (sample.z * 2f - 1f) * RenderConfig.CellPerturbStrengthXZ;

            if(perturbY) {
                position.y += Mathf.Max(sample.y * 2f - 1f, RenderConfig.MinHillPerturbation) * RenderConfig.CellPerturbStrengthY;
            }

            return position;
        }

        public EdgeVertices Perturb(EdgeVertices edge, bool includeY = false) {
            return new EdgeVertices(
                Perturb(edge.V1, includeY), Perturb(edge.V2, includeY), Perturb(edge.V3, includeY),
                Perturb(edge.V4, includeY), Perturb(edge.V5, includeY)
            );
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
