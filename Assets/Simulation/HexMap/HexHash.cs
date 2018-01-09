using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Simulation.HexMap {

    public struct HexHash {

        #region instance fields and properties

        public float A, B;

        #endregion

        #region static methods

        public static HexHash Create() {
            HexHash hash;
            hash.A = Random.value;
            hash.B = Random.value;
            return hash;
        }

        #endregion

    }

}
