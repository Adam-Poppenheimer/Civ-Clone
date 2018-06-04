using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.HexMap {

    public static class RiverDirectionExtensions {

        #region static methods

        public static RiverFlow Opposite(this RiverFlow direction) {
            return (RiverFlow)(((int)direction + 1) % 2);
        }

        #endregion

    }

}
