using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.HexMap {

    public static class CellVegetationExtensions {

        #region static methods

        public static bool HasTrees(this CellVegetation vegetation) {
            return vegetation == CellVegetation.Forest || vegetation == CellVegetation.Jungle;
        }

        #endregion

    }

}
