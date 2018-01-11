using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

namespace Assets.Simulation.HexMap {

    public class TileResourceLogic : ITileResourceLogic {

        #region instance fields and properties

        private IHexGridConfig Config;

        #endregion

        #region constructors

        [Inject]
        public TileResourceLogic(IHexGridConfig config) {
            Config = config;
        }

        #endregion

        #region instance methods

        #region from ITileResourceLogic

        public ResourceSummary GetYieldOfCell(IHexCell tile) {
            if(tile == null) {
                throw new ArgumentNullException("tile");
            }

            if(tile.Feature != TerrainFeature.None) {
                return Config.FeatureYields[(int)tile.Feature];

            }else {
                return Config.TerrainYields[(int)tile.Terrain];
            }
        }

        #endregion

        #endregion
        
    }

}
