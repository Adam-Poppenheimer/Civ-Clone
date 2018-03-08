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

        public static HexDirection Previous2(this HexDirection direction) {
            direction -= 2;
            return direction >= HexDirection.NE ? direction : (direction + 6);
        }

        public static HexDirection Next2(this HexDirection direction) {
            direction += 2;
            return direction <= HexDirection.NW ? direction : (direction - 6);
        }

        public static HexDirection Opposite(this HexDirection direction) {
            return (HexDirection)(((int)direction + 3) % 6);
        }

        #endregion

    }

}
