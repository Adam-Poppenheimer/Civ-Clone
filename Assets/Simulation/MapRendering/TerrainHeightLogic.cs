using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

namespace Assets.Simulation.MapRendering {

    public class TerrainHeightLogic : ITerrainHeightLogic {

        #region instance fields and properties

        private INoiseGenerator NoiseGenerator;

        #endregion

        #region constructors

        [Inject]
        public TerrainHeightLogic(INoiseGenerator noiseGenerator) {
            NoiseGenerator = noiseGenerator;
        }

        #endregion

        #region instance methods

        #region from ITerrainHeightLogic

        public float GetHeightForPosition(Vector3 position) {
            return NoiseGenerator.SampleElevationNoise(position);
        }

        #endregion

        #endregion
        
    }

}
