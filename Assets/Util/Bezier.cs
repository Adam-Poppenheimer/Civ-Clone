using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Util {

    public static class Bezier {

        public static Vector3 GetPoint(Vector3 a, Vector3 b, Vector3 c, float t) {
            float r = 1f - t;
            return r * r * a + 2f * r * t * b + t * t * c;
        }

        public static Vector3 GetPoint(Vector3 start, Vector3 controlOne, Vector3 controlTwo, Vector3 end, float t) {
            t = Mathf.Clamp01(t);

            float oneMinusT = 1f - t;

            return oneMinusT * oneMinusT * oneMinusT * start
                 + oneMinusT * oneMinusT * t         * controlOne * 3f
                 + oneMinusT * t         * t         * controlTwo * 3f
                 + t         * t         * t         * end;
        }

        public static Vector3 GetFirstDerivative(Vector3 a, Vector3 b, Vector3 c, float t) {
            return 2f * ((1f - t) * (b - a) + t * (c - b));
        }

        public static Vector3 GetFirstDerivative(Vector3 start, Vector3 controlOne, Vector3 controlTwo, Vector3 end, float t) {
            t = Mathf.Clamp01(t);

            float oneMinusT = 1f - t;

            return oneMinusT * oneMinusT * (controlOne - start)      * 3f
                 + oneMinusT * t         * (controlTwo - controlOne) * 6f
                 + t         * t         * (end        - controlTwo) * 3f;
        }

    }

}
