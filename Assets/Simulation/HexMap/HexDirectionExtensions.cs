using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.HexMap {

    public static class HexDirectionExtensions {

        #region static methods

        public static HexDirection Previous(this HexDirection direction) {
            return direction == HexDirection.NE ? HexDirection.NW : (direction - 1);
        }

        public static HexDirection Next(this HexDirection direction) {
            return direction == HexDirection.NW ? HexDirection.NE : (direction + 1);
        }

        #endregion

    }

}
