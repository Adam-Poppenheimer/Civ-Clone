using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.HexMap {

    public static class TerrainTypeExtensions {

        #region static methods

        public static bool IsWater(this TerrainType type) {
            return type == TerrainType.ShallowWater ||
                   type == TerrainType.DeepWater    ||
                   type == TerrainType.FreshWater;
        }

        #endregion

    }

}
