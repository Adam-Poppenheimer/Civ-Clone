﻿using System;
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

        public void GetBarycentric2D(
            Vector2 point, Vector2 a, Vector2 b, Vector2 c, out float aCoord, out float bCoord, out float cCoord
        ) {
            Vector2 v0 = b - a, v1 = c - a, v2 = point - a;

            float denom = v0.x * v1.y - v1.x * v0.y;

            bCoord = (v2.x * v1.y - v1.x * v2.y) / denom;
            cCoord = (v0.x * v2.y - v2.x * v0.y) / denom;
            aCoord = 1.0f - bCoord - cCoord;
        }

        //Copied from http://wiki.unity3d.com/index.php/3d_Math_functions
        public bool AreLineSegmentsCrossing(
            Vector2 pointA1, Vector2 pointA2, out Vector2 closestPointA,
            Vector2 pointB1, Vector2 pointB2, out Vector2 closestPointB
        ) {
            int sideA;
            int sideB;

            Vector2 lineVecA = pointA2 - pointA1;
            Vector2 lineVecB = pointB2 - pointB1;

            bool valid = ClosestPointsOnTwoLines(
                pointA1, lineVecA.normalized, pointB1, lineVecB.normalized, out closestPointA, out closestPointB
            );

            if(valid) {
                sideA = PointOnWhichSideOfLineSegment(pointA1, pointA2, closestPointA);
                sideB = PointOnWhichSideOfLineSegment(pointB1, pointB2, closestPointB);

                return (sideA == 0) && (sideB == 0);

            }else {
                return false;
            }
        }

        //Copied from http://wiki.unity3d.com/index.php/3d_Math_functions
        //Originally created by Bit Barrel Media
        public bool ClosestPointsOnTwoLines(
            Vector2 lineOnePoint, Vector2 lineOneVector,
            Vector2 lineTwoPoint, Vector2 lineTwoVector,
            out Vector2 closestPointLineOne, out Vector2 closestPointLineTwo
        ) {
            closestPointLineOne = Vector2.zero;
            closestPointLineTwo = Vector2.zero;

            float a = Vector2.Dot(lineOneVector, lineOneVector);
            float b = Vector2.Dot(lineOneVector, lineTwoVector);
            float e = Vector2.Dot(lineTwoVector, lineTwoVector);

            float d = a * e - b * b;

            if(d != 0f) {

                Vector2 r = lineOnePoint - lineTwoPoint;
                float c = Vector2.Dot(lineOneVector, r);
                float f = Vector2.Dot(lineTwoVector, r);

                float s = (b * f - c * e) / d;
                float t = (a * f - c * b) / d;

                closestPointLineOne = lineOnePoint + lineOneVector * s;
                closestPointLineTwo = lineTwoPoint + lineTwoVector * t;

                return true;

            }else {
                return false;
            }
        }

        public Vector2 GetClosestPointOnLineSegment(Vector2 start, Vector2 end, Vector2 point) {
            Vector2 heading = (end - start);
            float magnitudeMax = heading.magnitude;
            heading.Normalize();

            Vector2 lhs = point - start;
            float dotP = Vector2.Dot(lhs, heading);
            dotP = Mathf.Clamp(dotP, 0f, magnitudeMax);
            return start + heading * dotP;
        }

        #endregion

        private static float Sign(Vector2 v1, Vector2 v2, Vector2 v3) {
            return (v1.x - v3.x) * (v2.y - v3.y) - (v2.x - v3.x) * (v1.y - v3.y);
        }

        //Copied from http://wiki.unity3d.com/index.php/3d_Math_functions
        //This function finds out on which side of a line segment the point is located.
	    //The point is assumed to be on a line created by linePoint1 and linePoint2. If the point is not on
	    //the line segment, project it on the line using ProjectPointOnLine() first.
	    //Returns 0 if point is on the line segment.
	    //Returns 1 if point is outside of the line segment and located on the side of linePoint1.
	    //Returns 2 if point is outside of the line segment and located on the side of linePoint2.
        private static int PointOnWhichSideOfLineSegment(Vector2 linePoint1, Vector2 linePoint2, Vector2 point) {
            Vector2 lineVec = linePoint2 - linePoint1;
            Vector2 pointVec = point - linePoint1;

            float dot = Vector2.Dot(pointVec, lineVec);
            //point is on side of linePoint2, compared to linePoint1
            if(dot >= 0) {

                //point is on the line segment
                if(pointVec.magnitude <= lineVec.magnitude) {
                    return 0;

                //point is not on the line segment and it is on the side of LinePoint2
                }else {
                    return 2;
                }
            
            //Point is not on side of linePoint2, compared to linePoint1.
		    //Point is not on the line segment and it is on the side of linePoint1.
            }else {                
                return 1;
            }
        }

        #endregion

    }

}
