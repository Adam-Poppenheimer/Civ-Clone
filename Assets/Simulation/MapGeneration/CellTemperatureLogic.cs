using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapGeneration {

    public class CellTemperatureLogic : ICellTemperatureLogic {

        #region instance fields and properties

        private IHexGrid             Grid;
        private IMapGenerationConfig Config;
        private INoiseGenerator      NoiseGenerator;

        #endregion

        #region constructors

        [Inject]
        public CellTemperatureLogic(
            IHexGrid grid, IMapGenerationConfig config, INoiseGenerator noiseGenerator
        ) {
            Grid           = grid;
            Config         = config;
            NoiseGenerator = noiseGenerator;
        }

        #endregion

        #region instance methods

        #region from ICellTemperatureLogic

        public float GetTemperatureOfCell(IHexCell cell, int jitterChannel) {
            float latitude = (float)cell.Coordinates.Z / Grid.CellCountZ;
            if(Config.Hemispheres == HemisphereMode.Both) {
                latitude *= 2;
                if(latitude > 1f) {
                    latitude = 2f - latitude;
                }
            }else if(Config.Hemispheres == HemisphereMode.North) {
                latitude = 1f - latitude;
            }

            float temperature = Mathf.LerpUnclamped(Config.LowTemperature, Config.HighTemperature, latitude);

            float jitter = NoiseGenerator.SampleNoise(cell.Position * 0.1f)[jitterChannel];

            temperature += (jitter * 2f - 1f) * Config.TemperatureJitter;

            return temperature;
        }

        #endregion

        #endregion
        
    }

}
