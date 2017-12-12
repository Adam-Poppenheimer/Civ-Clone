using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.GameMap;

namespace Assets.Simulation.Units {

    public class UnitTerrainCostLogic : IUnitTerrainCostLogic {

        #region instance fields and properties

        private ITileConfig Config;

        #endregion

        #region constructors

        [Inject]
        public UnitTerrainCostLogic(ITileConfig config) {
            Config = config;
        }

        #endregion

        #region instance methods

        #region from IUnitTerrainCostLogic

        public int GetCostToMoveUnitIntoTile(IUnit unit, IMapTile tile) {
            int retval = 0;

            switch(tile.Terrain) {
                case TerrainType.Grassland:    retval += Config.GrasslandMoveCost;    break;
                case TerrainType.Plains:       retval += Config.PlainsMoveCost;       break;
                case TerrainType.Desert:       retval += Config.DesertMoveCost;       break;
                case TerrainType.ShallowWater: return Config.ShallowWaterMoveCost;
                case TerrainType.DeepWater:    return Config.DeepWaterMoveCost;
                default: break;
            }

            switch(tile.Shape) {
                case TerrainShape.Hills: retval += Config.HillsMoveCost; break;
                default: break;
            }

            switch(tile.Feature) {
                case TerrainFeatureType.Forest: retval += Config.ForestMoveCost; break;
                default: break;
            }

            return retval;
        }

        #endregion

        #endregion
        
    }

}
