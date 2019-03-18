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

        private IMapRenderConfig         RenderConfig;
        private IFlatlandsHeightmapLogic FlatlandsHeightmapLogic;
        private IHillsHeightmapLogic     HillsHeightmapLogic;
        private IMountainHeightmapLogic  MountainHeightmapLogic;

        #endregion

        #region constructors

        [Inject]
        public CellHeightmapLogic(
            IMapRenderConfig renderConfig, IFlatlandsHeightmapLogic flatlandsHeightmapLogic,
            IHillsHeightmapLogic hillsHeightmapLogic, IMountainHeightmapLogic mountainHeightmapLogic
        ) {
            RenderConfig            = renderConfig;
            FlatlandsHeightmapLogic = flatlandsHeightmapLogic;
            HillsHeightmapLogic     = hillsHeightmapLogic;
            MountainHeightmapLogic  = mountainHeightmapLogic;
        }

        #endregion

        #region instance methods

        #region from ICellHeightmapLogic

        public float GetHeightForPointForCell(Vector2 xzPoint, IHexCell cell, HexDirection sextant) {
            if(cell.Terrain.IsWater()) {
                return RenderConfig.SeaFloorElevation;

            }if(cell.Shape == CellShape.Flatlands) {
                return FlatlandsHeightmapLogic.GetHeightForPoint(xzPoint, cell, sextant);

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
