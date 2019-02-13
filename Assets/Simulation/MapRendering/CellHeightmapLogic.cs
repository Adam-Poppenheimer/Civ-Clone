﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.HexMap;

using Assets.Util;

namespace Assets.Simulation.MapRendering {

    public class CellHeightmapLogic : ICellHeightmapLogic {

        #region instance fields and properties

        private IMapRenderConfig        RenderConfig;
        private INoiseGenerator         NoiseGenerator;
        private IMountainHeightmapLogic MountainHeightmapLogic;

        #endregion

        #region constructors

        [Inject]
        public CellHeightmapLogic(
            IMapRenderConfig renderConfig, INoiseGenerator noiseGenerator,
            IMountainHeightmapLogic mountainHeightmapLogic
        ) {
            RenderConfig           = renderConfig;
            NoiseGenerator         = noiseGenerator;
            MountainHeightmapLogic = mountainHeightmapLogic;
        }

        #endregion

        #region instance methods

        #region from ICellHeightmapLogic

        public float GetHeightForPositionForCell(Vector3 position, IHexCell cell, HexDirection sextant) {
            if(cell.Terrain.IsWater()) {
                return RenderConfig.SeaFloorElevation;

            }else {
                switch(cell.Shape) {
                    case CellShape.Flatlands: return RenderConfig.FlatlandsBaseElevation + NoiseGenerator.SampleNoise(position, NoiseType.FlatlandsHeight).x;
                    case CellShape.Hills:     return RenderConfig.HillsBaseElevation     + NoiseGenerator.SampleNoise(position, NoiseType.HillsHeight)    .x;
                    case CellShape.Mountains: return MountainHeightmapLogic.GetHeightForPosition(position, cell, sextant);
                    default: throw new NotImplementedException();
                }
            }
        }

        #endregion

        #endregion
        
    }

}
