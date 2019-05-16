using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.Cities.Buildings {

    public static class BuildingTypeExtensions {

        #region static methods

        public static bool IsWonder(this BuildingType type) {
            return type == BuildingType.WorldWonder || type == BuildingType.NationalWonder;
        }

        #endregion

    }

}
