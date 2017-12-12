﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

namespace Assets.Simulation.GameMap {

    public class TileResourceLogic : ITileResourceLogic {

        #region instance fields and properties

        private ITileConfig Config;

        #endregion

        #region constructors

        [Inject]
        public TileResourceLogic(ITileConfig config) {
            Config = config;
        }

        #endregion

        #region instance methods

        #region from ITileResourceLogic

        public ResourceSummary GetYieldOfTile(IMapTile tile) {
            if(tile == null) {
                throw new ArgumentNullException("tile");
            }

            if(tile.Feature == TerrainFeatureType.Forest) {
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
