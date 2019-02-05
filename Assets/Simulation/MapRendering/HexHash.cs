using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Simulation.MapRendering {

    public struct HexHash {

        #region instance fields and properties

        public float A, B, C;

        #endregion

        #region static methods

        public static HexHash Create() {
            HexHash hash;
            hash.A = Random.value;
            hash.B = Random.value;
            hash.C = Random.value;
            return hash;
        }

        #endregion

    }

}
