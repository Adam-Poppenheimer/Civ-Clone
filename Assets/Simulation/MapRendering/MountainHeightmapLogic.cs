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
        private IHillsHeightmapLogic          HillsHeightmapLogic;
        private IRiverCanon                   RiverCanon;

        #endregion

        #region constructors

        [Inject]
        public MountainHeightmapLogic(
            IMapRenderConfig renderConfig, INoiseGenerator noiseGenerator,
            IHexGrid grid, IMountainHeightmapWeightLogic mountainHeightmapWeightLogic,
            IHillsHeightmapLogic hillsHeightmapLogic, IRiverCanon riverCanon
        ) {
            RenderConfig                 = renderConfig;
            NoiseGenerator               = noiseGenerator;
            Grid                         = grid;
            MountainHeightmapWeightLogic = mountainHeightmapWeightLogic;
            HillsHeightmapLogic          = hillsHeightmapLogic;
            RiverCanon                   = riverCanon;
        }

        #endregion

        #region instance methods

        #region from IMountainHeightmapLogic

        
        public float GetHeightForPoint(Vector2 xzPoint, IHexCell cell, HexDirection sextant) {
            float peakWeight, ridgeWeight, hillsWeight;

            MountainHeightmapWeightLogic.GetHeightWeightsForPoint(
                xzPoint, cell, sextant, out peakWeight, out ridgeWeight, out hillsWeight
            );

            var neighbor = Grid.GetNeighbor(cell, sextant);

            float hillsHeight = HillsHeightmapLogic.GetHeightForPoint(xzPoint, cell, sextant);

            float ridgeHeight = hillsHeight;

            if(neighbor != null && neighbor.Shape == CellShape.Mountains && !RiverCanon.HasRiverAlongEdge(cell, sextant)) {
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
