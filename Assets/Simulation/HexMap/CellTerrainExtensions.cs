using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.HexMap {

    public static class CellTerrainExtensions {

        #region static methods

        public static bool IsWater(this CellTerrain type) {
            return type == CellTerrain.ShallowWater ||
                   type == CellTerrain.DeepWater    ||
                   type == CellTerrain.FreshWater;
        }

        public static bool IsArctic(this CellTerrain type) {
            return type == CellTerrain.Tundra ||
                   type == CellTerrain.Snow;
        }

        public static bool IsDesert(this CellTerrain type) {
            return type == CellTerrain.Desert ||
                   type == CellTerrain.FloodPlains;
        }

        #endregion

    }

}
