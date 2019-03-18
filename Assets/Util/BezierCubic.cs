using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Util {

    public class BezierCubic {

        #region static methods

        public static Vector3 GetPoint(Vector3 start, Vector3 controlOne, Vector3 controlTwo, Vector3 end, float t) {
            t = Mathf.Clamp01(t);

            float oneMinusT = 1f - t;

            return oneMinusT * oneMinusT * oneMinusT * start
                 + oneMinusT * oneMinusT * t         * controlOne * 3f
                 + oneMinusT * t         * t         * controlTwo * 3f
                 + t         * t         * t         * end;
        }

        public static Vector3 GetFirstDerivative(Vector3 start, Vector3 controlOne, Vector3 controlTwo, Vector3 end, float t) {
            t = Mathf.Clamp01(t);

            float oneMinusT = 1f - t;

            return oneMinusT * oneMinusT * (controlOne - start)      * 3f
                 + oneMinusT * t         * (controlTwo - controlOne) * 6f
                 + t         * t         * (end        - controlTwo) * 3f;
        }

        #endregion

    }

}
