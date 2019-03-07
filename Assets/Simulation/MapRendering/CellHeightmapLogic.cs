using System;
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
        private IMountainHeightmapLogic MountainHeightmapLogic;
        private INoiseGenerator         NoiseGenerator;
        private IHillsHeightmapLogic    HillsHeightmapLogic;

        #endregion

        #region constructors

        [Inject]
        public CellHeightmapLogic(
            IMapRenderConfig renderConfig, IMountainHeightmapLogic mountainHeightmapLogic,
            INoiseGenerator noiseGenerator, IHillsHeightmapLogic hillsHeightmapLogic
        ) {
            RenderConfig           = renderConfig;
            MountainHeightmapLogic = mountainHeightmapLogic;
            NoiseGenerator         = noiseGenerator;
            HillsHeightmapLogic    = hillsHeightmapLogic;
        }

        #endregion

        #region instance methods

        #region from ICellHeightmapLogic

        public float GetHeightForPointForCell(Vector2 xzPoint, IHexCell cell, HexDirection sextant) {
            if(cell.Terrain.IsWater()) {
                return RenderConfig.SeaFloorElevation;

            }if(cell.Shape == CellShape.Flatlands) {
                return RenderConfig.FlatlandsBaseElevation + NoiseGenerator.SampleNoise(xzPoint, NoiseType.FlatlandsHeight).x;

            }else if(cell.Shape == CellShape.Hills) {
                return HillsHeightmapLogic.GetHeightForPoint(xzPoint, cell, sextant);

            }else if(cell.Shape == CellShape.Mountains) {
                return MountainHeightmapLogic.GetHeightForPoint(xzPoint, cell, sextant);

            }else {
                throw new NotImplementedException();
            }
        }

        #endregion

        #endregion
        
    }

}
