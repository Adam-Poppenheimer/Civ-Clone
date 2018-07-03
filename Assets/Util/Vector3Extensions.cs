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

        public static Vector3 GetPointBetweenWithY(Vector3 a, Vector3 b, float y) {
            float lerpParam = (y - a.y) / (b.y - a.y);
            return Vector3.Lerp(a, b, lerpParam);
        }

        public static Vector3 GetPointBetweenWithY(Vector3 a, Vector3 b, float y, out float lerpParam) {
            lerpParam = (y - a.y) / (b.y - a.y);
            return Vector3.Lerp(a, b, lerpParam);
        }

        #endregion

    }

}
