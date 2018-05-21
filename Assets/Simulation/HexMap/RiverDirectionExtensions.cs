using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.HexMap {

    public static class RiverDirectionExtensions {

        #region static methods

        public static RiverDirection Opposite(this RiverDirection direction) {
            return (RiverDirection)(((int)direction + 1) % 2);
        }

        #endregion

    }

}
