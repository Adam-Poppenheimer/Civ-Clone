using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Util {

    public class Geometry2D : IGeometry2D {

        #region instance methods

        #region from IGeometry2D

        //Sensitive to floating-point errors
        public bool IsPointWithinTriangle(Vector2 point, Vector2 v1, Vector2 v2, Vector2 v3) {
            bool b1, b2, b3;

            b1 = Sign(point, v1, v2) <= 0.0f;
            b2 = Sign(point, v2, v3) <= 0.0f;
            b3 = Sign(point, v3, v1) <= 0.0f;

            return ((b1 == b2) && (b2 == b3));
        }

        public void GetBarycentric2D(Vector2 point, Vector2 a, Vector2 b, Vector2 c, out float u, out float v, out float w) {
            Vector2 v0 = b - a, v1 = c - a, v2 = point - a;

            float denom = v0.x * v1.y - v1.x * v0.y;

            v = (v2.x * v1.y - v1.x * v2.y) / denom;
            w = (v0.x * v2.y - v2.x * v0.y) / denom;
            u = 1.0f - v - w;
        }

        #endregion

        private static float Sign(Vector2 v1, Vector2 v2, Vector2 v3) {
            return (v1.x - v3.x) * (v2.y - v3.y) - (v2.x - v3.x) * (v1.y - v3.y);
        }

        #endregion

    }

}
