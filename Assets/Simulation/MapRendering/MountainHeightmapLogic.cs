using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.HexMap;
using Assets.Util;

namespace Assets.Simulation.MapRendering {

    public class MountainHeightmapLogic : IMountainHeightmapLogic {

        #region instance fields and properties

        private IMapRenderConfig              RenderConfig;
        private INoiseGenerator               NoiseGenerator;
        private IHexGrid                      Grid;
        private IMountainHeightmapWeightLogic MountainHeightmapWeightLogic;

        #endregion

        #region constructors

        [Inject]
        public MountainHeightmapLogic(
            IMapRenderConfig renderConfig, INoiseGenerator noiseGenerator,
            IHexGrid grid, IMountainHeightmapWeightLogic mountainHeightmapWeightLogic
        ) {
            RenderConfig                 = renderConfig;
            NoiseGenerator               = noiseGenerator;
            Grid                         = grid;
            MountainHeightmapWeightLogic = mountainHeightmapWeightLogic;
        }

        #endregion

        #region instance methods

        #region from IMountainHeightmapLogic

        
        public float GetHeightForPosition(Vector3 position, IHexCell cell, HexDirection sextant) {
            float peakWeight, ridgeWeight, hillsWeight;

            MountainHeightmapWeightLogic.GetHeightWeightsForPosition(
                position, cell, sextant, out peakWeight, out ridgeWeight, out hillsWeight
            );

            var neighbor = Grid.HasNeighbor(cell, sextant) ? Grid.GetNeighbor(cell, sextant) : null;

            float hillsHeight = NoiseGenerator.SampleNoise(position, NoiseType.HillsHeight).x;

            float ridgeHeight = hillsHeight;

            if(neighbor != null && neighbor.Shape == CellShape.Mountains) {
                ridgeHeight = RenderConfig.MountainRidgeElevation;
            }

            return peakWeight  * RenderConfig.MountainPeakElevation
                 + ridgeWeight * ridgeHeight 
                 + hillsWeight * hillsHeight;
        }

        #endregion

        #endregion
        
    }

}
