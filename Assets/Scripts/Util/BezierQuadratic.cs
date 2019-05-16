using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Util {

    public static class BezierQuadratic {

        #region static methods

        public static Vector3 GetPoint(Vector3 a, Vector3 b, Vector3 c, float t) {
            float r = 1f - t;
            return r * r * a + 2f * r * t * b + t * t * c;
        }

        public static Vector3 GetFirstDerivative(Vector3 a, Vector3 b, Vector3 c, float t) {
            return 2f * ((1f - t) * (b - a) + t * (c - b));
        }

        #endregion

    }

}
