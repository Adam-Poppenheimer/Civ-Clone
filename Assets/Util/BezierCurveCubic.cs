using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Util {

    public class BezierCurveCubic {

        #region instance fields and properties

        public Vector3 V1 { get; private set; }
        public Vector3 V2 { get; private set; }
        public Vector3 V3 { get; private set; }
        public Vector3 V4 { get; private set; }

        #endregion

        #region constructors

        public BezierCurveCubic(Vector3 v1, Vector3 v2, Vector3 v3, Vector3 v4) {
            V1 = v1;
            V2 = v2;
            V3 = v3;
            V4 = v4;
        }

        #endregion

        #region instance methods

        public Vector3 GetPoint(float t) {
            return Bezier.GetPoint(V1, V2, V3, V4, t);
        }

        public Vector3 GetVelocity(float t) {
            return Bezier.GetFirstDerivative(V1, V2, V3, V4, t);
        }

        public Vector3 GetDirection(float t) {
            return GetVelocity(t).normalized;
        }

        public Vector3 GetNormalXZ(float t) {
            Vector3 tangent = GetDirection(t);

            return new Vector3(-tangent.z, tangent.y, tangent.x);
        }

        #endregion

    }
}
