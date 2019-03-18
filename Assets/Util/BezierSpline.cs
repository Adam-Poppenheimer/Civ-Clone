using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Util {

    public class BezierSpline {

        #region instance fields and properties

        public ReadOnlyCollection<Vector3> Points {
            get { return points.AsReadOnly(); }
        }
        private List<Vector3> points;

        public int CurveCount {
            get { return (points.Count - 1) / 3; }
        }

        #endregion

        #region constructors

        public BezierSpline(Vector3 start) {
            points = new List<Vector3>();

            points.Add(start);
        }

        #endregion

        #region instance methods

        public void AddCubicCurve(Vector3 controlOne, Vector3 controlTwo, Vector3 end) {
            points.Add(controlOne);
            points.Add(controlTwo);
            points.Add(end);
        }

        public Vector3 GetPoint(float t) {
            int i = GetStartingIndex(ref t);

            return BezierCubic.GetPoint(points[i], points[i + 1], points[i + 2], points[i + 3], t);
        }

        public Vector3 GetVelocity(float t) {
            int i = GetStartingIndex(ref t);

            return BezierCubic.GetFirstDerivative(points[i], points[i + 1], points[i + 2], points[i + 3], t);
        }

        public Vector3 GetDirection(float t) {
            return GetVelocity(t).normalized;
        }

        public Vector3 GetNormalXZ(float t) {
            Vector3 tangent = GetDirection(t);

            return new Vector3(-tangent.z, tangent.y, tangent.x);
        }

        public void Reset() {
            points.Clear();
        }

        private int GetStartingIndex(ref float t) {
            int i;

            if(t >= 1f) {
                t = 1f;
                i = points.Count - 4;

            }else {
                t = Mathf.Clamp01(t) * CurveCount;
                i = (int)t;
                t -= i;
                i *= 3;
            }

            return i;
        }

        #endregion

    }

}
