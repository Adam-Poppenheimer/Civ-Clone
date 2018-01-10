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

        public ResourceSummary GetYieldOfTile(IHexCell tile) {
            if(tile == null) {
                throw new ArgumentNullException("tile");
            }

            if(tile.Feature == TerrainFeature.Forest) {
                return Config.ForestYield;

            }else if(tile.Shape == TerrainShape.Hills) {
                return Config.HillsYield;

            }else {
                switch(tile.Terrain) {
                    case TerrainType.Grassland: return Config.GrasslandYield;
                    case TerrainType.Plains:    return Config.PlainsYield;
                    case TerrainType.Desert:    return Config.DesertYield;
                    default: return new ResourceSummary();
                }
            }
        }

        #endregion

        #endregion
        
    }

}
