using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Util {

    public static class Vector3Extensions {

        #region static methods

        public static float InverseLerp(Vector3 a, Vector3 b, Vector3 value) {
            Vector3 AB = b - a;
            Vector3 AV = value - a;
            return Vector3.Dot(AV, AB) / Vector3.Dot(AB, AB);
        }

        #endregion

    }

}
