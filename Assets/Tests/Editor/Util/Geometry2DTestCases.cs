using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using NUnit.Framework;

using Assets.Util;

namespace Assets.Tests.Util {

    public static class Geometry2DTestCases {

        #region internal types

        public class IsPointWithinTriangleData {

            public Vector2 Point;

            public Vector2 V1;
            public Vector2 V2;
            public Vector2 V3;

        }

        public class GetBarycentric2DData {

            public Vector2 Point;

            public Vector2 A;
            public Vector2 B;
            public Vector2 C;

            public float ExpectedACoord;
            public float ExpectedBCoord;
            public float ExpectedCCoord;

        }

        public class AreLineSegmentsCrossingData {

            public Vector2 PointA1;
            public Vector2 PointA2;
            
            public Vector2 PointB1;
            public Vector2 PointB2;

            public bool IgnoreExpectedPoints;

            public Vector2 ExpectedClosestPointA;
            public Vector2 ExpectedClosestPointB;

        }

        public class ClosestPointsOnTwoLinesData {

            public Vector2 LineOnePoint;
            public Vector2 LineOneVector;

            public Vector2 LineTwoPoint;
            public Vector2 LineTwoVector;

            public Vector2 ExpectedClosestPointLineOne;
            public Vector2 ExpectedClosestPointLineTwo;

        }

        public class GetClosestPointOnLineSegmentData {

            public Vector2 Start;
            public Vector2 End;
            public Vector2 Point;

            public Vector2 ExpectedResult;

        }

        #endregion

        #region static fields and properties

        public static IEnumerable IsPointWithinTriangleTestCases {
            get {
                yield return new TestCaseData(new IsPointWithinTriangleData() {
                    V1 = new Vector2(5f, 5f),
                    V2 = new Vector2(7f, 40f),
                    V3 = new Vector2(10f, 20f),
                    Point = new Vector2(7.3f, 22f)
                }).SetName("True if point on correct side of all vertices").Returns(true);

                yield return new TestCaseData(new IsPointWithinTriangleData() {
                    V1 = new Vector2(5f, 5f),
                    V2 = new Vector2(7f, 40f),
                    V3 = new Vector2(10f, 20f),
                    Point = new Vector2(5f, 5f)
                }).SetName("True if point at V1").Returns(true);

                yield return new TestCaseData(new IsPointWithinTriangleData() {
                    V1 = new Vector2(5f, 5f),
                    V2 = new Vector2(7f, 40f),
                    V3 = new Vector2(10f, 20f),
                    Point = new Vector2(7f, 40f)
                }).SetName("True if point at V2").Returns(true);

                yield return new TestCaseData(new IsPointWithinTriangleData() {
                    V1 = new Vector2(5f, 5f),
                    V2 = new Vector2(7f, 40f),
                    V3 = new Vector2(10f, 20f),
                    Point = new Vector2(10f, 20f)
                }).SetName("True if point at V3").Returns(true);

                yield return new TestCaseData(new IsPointWithinTriangleData() {
                    V1 = new Vector2(5f, 5f),
                    V2 = new Vector2(7f, 40f),
                    V3 = new Vector2(10f, 20f),
                    Point = Vector2.Lerp(new Vector2(5f, 5f), new Vector2(7f, 40f), 0.72f)
                }).SetName("True if point on V1/V2 edge").Returns(true);

                yield return new TestCaseData(new IsPointWithinTriangleData() {
                    V1 = new Vector2(5f, 5f),
                    V2 = new Vector2(7f, 40f),
                    V3 = new Vector2(10f, 20f),
                    Point = Vector2.Lerp(new Vector2(5f, 5f), new Vector2(10f, 20f), 0.68f)
                }).SetName("True if point on V1/V3 edge").Returns(true);

                yield return new TestCaseData(new IsPointWithinTriangleData() {
                    V1 = new Vector2(5f, 5f),
                    V2 = new Vector2(7f, 40f),
                    V3 = new Vector2(10f, 20f),
                    Point = Vector2.Lerp(new Vector2(7f, 40f), new Vector2(10f, 20f), 0.72f)
                }).SetName("True if point on V2/V3 edge").Returns(true);

                yield return new TestCaseData(new IsPointWithinTriangleData() {
                    V1 = new Vector2(5f, 5f),
                    V2 = new Vector2(7f, 40f),
                    V3 = new Vector2(10f, 20f),
                    Point = (new Vector2(5f, 5f) + new Vector2(7f, 40f)) / 2f + Vector2.left
                }).SetName("False if point beyond V1/V2 edge").Returns(false);

                yield return new TestCaseData(new IsPointWithinTriangleData() {
                    V1 = new Vector2(5f, 5f),
                    V2 = new Vector2(7f, 40f),
                    V3 = new Vector2(10f, 20f),
                    Point = (new Vector2(5f, 5f) + new Vector2(10f, 20f)) / 2f + Vector2.down
                }).SetName("False if point beyond V1/V3 edge").Returns(false);

                yield return new TestCaseData(new IsPointWithinTriangleData() {
                    V1 = new Vector2(5f, 5f),
                    V2 = new Vector2(7f, 40f),
                    V3 = new Vector2(10f, 20f),
                    Point = (new Vector2(7f, 40f) + new Vector2(10f, 20f)) / 2f + Vector2.right
                }).SetName("False if point beyond V2/V3 edge").Returns(false);
            }
        }

        public static IEnumerable GetBarycentric2DTestCases {
            get {
                yield return new TestCaseData(new GetBarycentric2DData() {
                    Point = new Vector2(1f, 1f),
                    A = new Vector2(1f, 1f),
                    B = new Vector2(4f, 4f),
                    C = new Vector2(7f, 1f),

                    ExpectedACoord = 1f, ExpectedBCoord = 0f, ExpectedCCoord = 0f
                }).SetName("Full weight on A coord if point at A");

                yield return new TestCaseData(new GetBarycentric2DData() {
                    Point = new Vector2(4f, 4f),
                    A = new Vector2(1f, 1f),
                    B = new Vector2(4f, 4f),
                    C = new Vector2(7f, 1f),

                    ExpectedACoord = 0f, ExpectedBCoord = 1f, ExpectedCCoord = 0f
                }).SetName("Full weight on B coord if point at B");

                yield return new TestCaseData(new GetBarycentric2DData() {
                    Point = new Vector2(7f, 1f),
                    A = new Vector2(1f, 1f),
                    B = new Vector2(4f, 4f),
                    C = new Vector2(7f, 1f),

                    ExpectedACoord = 0f, ExpectedBCoord = 0f, ExpectedCCoord = 1f
                }).SetName("Full weight on C coord if point at C");

                yield return new TestCaseData(new GetBarycentric2DData() {
                    Point = Vector2.Lerp(new Vector2(1f, 1f), new Vector2(4f, 4f), 0.6f),
                    A = new Vector2(1f, 1f),
                    B = new Vector2(4f, 4f),
                    C = new Vector2(7f, 1f),

                    ExpectedACoord = 0.4f, ExpectedBCoord = 0.6f, ExpectedCCoord = 0f
                }).SetName("Weight split between A and B if point on AB edge");

                yield return new TestCaseData(new GetBarycentric2DData() {
                    Point = Vector2.Lerp(new Vector2(1f, 1f), new Vector2(7f, 1f), 0.6f),
                    A = new Vector2(1f, 1f),
                    B = new Vector2(4f, 4f),
                    C = new Vector2(7f, 1f),

                    ExpectedACoord = 0.4f, ExpectedBCoord = 0f, ExpectedCCoord = 0.6f
                }).SetName("Weight split between A and C if point on AC edge");

                yield return new TestCaseData(new GetBarycentric2DData() {
                    Point = Vector2.Lerp(new Vector2(4f, 4f), new Vector2(7f, 1f), 0.65f),
                    A = new Vector2(1f, 1f),
                    B = new Vector2(4f, 4f),
                    C = new Vector2(7f, 1f),

                    ExpectedACoord = 0f, ExpectedBCoord = 0.35f, ExpectedCCoord = 0.65f
                }).SetName("Weight split between B and C if point on BC edge");

                yield return new TestCaseData(new GetBarycentric2DData() {
                    Point = (new Vector2(1f, 1f) + new Vector2(4f, 4f) + new Vector2(7f, 1f)) / 3f,
                    A = new Vector2(1f, 1f),
                    B = new Vector2(4f, 4f),
                    C = new Vector2(7f, 1f),

                    ExpectedACoord = 1f / 3f, ExpectedBCoord = 1f / 3f, ExpectedCCoord = 1f / 3f
                }).SetName("Weights split evenly between points if point in barycenter");

                yield return new TestCaseData(new GetBarycentric2DData() {
                    Point = Vector2.zero,
                    A = new Vector2(1f, 1f),
                    B = new Vector2(4f, 4f),
                    C = new Vector2(7f, 1f),

                    ExpectedACoord = 4f / 3f, ExpectedBCoord = -1f / 3f, ExpectedCCoord = 0f
                }).SetName("Produces weights less than zero and greater than one if outside the triangle");
            }
        }

        public static IEnumerable AreLineSegmentsCrossingTestCases {
            get {
                yield return new TestCaseData(new AreLineSegmentsCrossingData() {
                    PointA1 = new Vector2(1f, 1f), PointA2 = new Vector2(8f, 1f),
                    PointB1 = new Vector2(2f, 1f), PointB2 = new Vector2(8f, 1f),

                    IgnoreExpectedPoints = true
                }).SetName("False if line segments parallel").Returns(false);

                yield return new TestCaseData(new AreLineSegmentsCrossingData() {
                    PointA1 = new Vector2(1f, 1f), PointA2 = new Vector2(5f, 5f),
                    PointB2 = new Vector2(5f, 1f), PointB1 = new Vector2(1f, 5f),

                    IgnoreExpectedPoints = true
                }).SetName("True if segments overlapping").Returns(true);

                yield return new TestCaseData(new AreLineSegmentsCrossingData() {
                    PointA1 = new Vector2(1f, 1f), PointA2 = new Vector2(5f, 1f),
                    PointB2 = new Vector2(6f, 6f), PointB1 = new Vector2(6f, 0f),

                    IgnoreExpectedPoints = true
                }).SetName("False if line intersection is beyond a segment's endpoints").Returns(false);

                yield return new TestCaseData(new AreLineSegmentsCrossingData() {
                    PointA1 = new Vector2(1f, 1f), PointA2 = new Vector2(5f, 5f),
                    PointB1 = new Vector2(5f, 5f), PointB2 = new Vector2(10f, 1f),

                    IgnoreExpectedPoints = true
                }).SetName("True if segments share an endpoint").Returns(true);

                yield return new TestCaseData(new AreLineSegmentsCrossingData() {
                    PointA1 = new Vector2(1f, 1f), PointA2 = new Vector2(5f, 5f),
                    PointB2 = new Vector2(5f, 1f), PointB1 = new Vector2(1f, 5f),

                    ExpectedClosestPointA = new Vector2(3f, 3f), ExpectedClosestPointB = new Vector2(3f, 3f)
                }).SetName("Closest points identical if segments cross").Returns(true);
            }
        }

        public static IEnumerable ClosestPointsOnTwoLinesTestCases {
            get {
                yield return new TestCaseData(new ClosestPointsOnTwoLinesData() {
                    LineOnePoint = new Vector2(1f, 1f),   LineOneVector = new Vector2(2f, -3.5f),
                    LineTwoPoint = new Vector2(3f, 6.2f), LineTwoVector = new Vector2(2f, -3.5f),

                    ExpectedClosestPointLineOne = Vector2.zero,
                    ExpectedClosestPointLineTwo = Vector2.zero
                }).SetName("Returns false and sets points to zero if lines parallel").Returns(false);

                yield return new TestCaseData(new ClosestPointsOnTwoLinesData() {
                    LineOnePoint = new Vector2(1f,  0f), LineOneVector = new Vector2(1f,  1f),
                    LineTwoPoint = new Vector2(11f, 0f), LineTwoVector = new Vector2(-1f, 1f),

                    ExpectedClosestPointLineOne = new Vector2(6f, 5f),
                    ExpectedClosestPointLineTwo = new Vector2(6f, 5f)
                }).SetName("Returns true and sets points to intersect if lines intersect").Returns(true);
            }
        }

        public static IEnumerable GetClosestPointOnLineSegmentTestCases {
            get {
                yield return new TestCaseData(new GetClosestPointOnLineSegmentData() {
                    Start = new Vector2(1f, 1f), End = new Vector2(3f, 6.5f), Point = new Vector2(1f, 1f),
                    
                    ExpectedResult = new Vector2(1f, 1f)
                }).SetName("Returns start if point is start");

                yield return new TestCaseData(new GetClosestPointOnLineSegmentData() {
                    Start = new Vector2(1f, 1f), End = new Vector2(3f, 6.5f), Point = new Vector2(3f, 6.5f),
                    
                    ExpectedResult = new Vector2(3f, 6.5f)
                }).SetName("Returns end if point is end");

                yield return new TestCaseData(new GetClosestPointOnLineSegmentData() {
                    Start = new Vector2(1f, 1f), End = new Vector2(3f, 6.5f),
                    Point = Vector2.Lerp(new Vector2(1f, 1f), new Vector2(3f, 6.5f), 0.82f),

                    ExpectedResult = Vector2.Lerp(new Vector2(1f, 1f), new Vector2(3f, 6.5f), 0.82f)
                }).SetName("Returns intermediate point if point on segment");

                yield return new TestCaseData(new GetClosestPointOnLineSegmentData() {
                    Start = new Vector2(1f, 1f), End = new Vector2(5f, 5f),
                    Point = new Vector2(13f, -7f),

                    ExpectedResult = new Vector2(3f, 3f)
                }).SetName("Returns nearest intermediate point if point not on segment");

                yield return new TestCaseData(new GetClosestPointOnLineSegmentData() {
                    Start = new Vector2(1f, 1f), End = new Vector2(5f, 5f),
                    Point = new Vector2(-1f, -2f),

                    ExpectedResult = new Vector2(1f, 1f)
                }).SetName("Returns start if point beyond start");

                yield return new TestCaseData(new GetClosestPointOnLineSegmentData() {
                    Start = new Vector2(1f, 1f), End = new Vector2(5f, 5f),
                    Point = new Vector2(10f, 11f),

                    ExpectedResult = new Vector2(5f, 5f)
                }).SetName("Returns end if point beyond end");
            }
        }

        #endregion

    }

}
