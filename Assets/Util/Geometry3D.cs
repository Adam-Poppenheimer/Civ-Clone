using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Util {

    public static class Geometry3D {

        #region static methods

        //Copied from http://wiki.unity3d.com/index.php/3d_Math_functions
        //Originally created by Bit Barrel Media
        public static bool ClosestPointsOnTwoLines(
            Vector3 lineOnePoint, Vector3 lineOneVector,
            Vector3 lineTwoPoint, Vector3 lineTwoVector,
            out Vector3 closestPointLineOne, out Vector3 closestPointLineTwo
        ) {
            closestPointLineOne = Vector3.zero;
            closestPointLineTwo = Vector3.zero;

            float a = Vector3.Dot(lineOneVector, lineOneVector);
            float b = Vector3.Dot(lineOneVector, lineTwoVector);
            float e = Vector3.Dot(lineTwoVector, lineTwoVector);

            float d = a * e - b * b;

            if(d != 0f) {

                Vector3 r = lineOnePoint - lineTwoPoint;
                float c = Vector3.Dot(lineOneVector, r);
                float f = Vector3.Dot(lineTwoVector, r);

                float s = (b * f - c * e) / d;
                float t = (a * f - c * b) / d;

                closestPointLineOne = lineOnePoint + lineOneVector * s;
                closestPointLineTwo = lineTwoPoint + lineTwoVector * t;

                return true;

            }else {
                return false;
            }
        }

        #endregion

    }

}
