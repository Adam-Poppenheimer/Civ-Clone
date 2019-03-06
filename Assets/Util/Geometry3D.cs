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

        //Copied from https://stackoverflow.com/questions/3120357/get-closest-point-to-a-line
        //Written by N Schilke
        public static Vector3 GeClosestPointOnLineSegment(Vector3 segmentStart, Vector3 segmentEnd, Vector3 point) {
            Vector3 startToPoint = point        - segmentStart;
            Vector3 startToEnd   = segmentStart - segmentEnd;

            float magnitudeSegment = startToEnd.sqrMagnitude;

            float segmentPointDot = Vector3.Dot(startToPoint, startToEnd);

            float distance = segmentPointDot / magnitudeSegment;

            if(distance < 0) {
                return segmentStart;

            }else if(distance > 1) {
                return segmentEnd;

            }else {
                return segmentStart + startToEnd * distance;
            }
        }

        //Copied from http://wiki.unity3d.com/index.php/3d_Math_functions
        public static bool AreLineSegmentsCrossing(
            Vector3 pointA1, Vector3 pointA2, out Vector3 closestPointA,
            Vector3 pointB1, Vector3 pointB2, out Vector3 closestPointB
        ) {
            int sideA;
            int sideB;

            Vector3 lineVecA = pointA2 - pointA1;
            Vector3 lineVecB = pointB2 - pointB1;

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
        //This function finds out on which side of a line segment the point is located.
	    //The point is assumed to be on a line created by linePoint1 and linePoint2. If the point is not on
	    //the line segment, project it on the line using ProjectPointOnLine() first.
	    //Returns 0 if point is on the line segment.
	    //Returns 1 if point is outside of the line segment and located on the side of linePoint1.
	    //Returns 2 if point is outside of the line segment and located on the side of linePoint2.
        public static int PointOnWhichSideOfLineSegment(Vector3 linePoint1, Vector3 linePoint2, Vector3 point) {
            Vector3 lineVec = linePoint2 - linePoint1;
            Vector3 pointVec = point - linePoint1;

            float dot = Vector3.Dot(pointVec, lineVec);
            //point is on side of linePoint2, compared to linePoint1
            if(dot > 0) {

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
