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

        #endregion

    }

}
