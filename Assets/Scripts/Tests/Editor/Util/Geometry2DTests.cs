using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using NUnit.Framework;
using Zenject;
using Moq;

using Assets.Util;

namespace Assets.Tests.Util {

    [TestFixture]
    public class Geometry2DTests : ZenjectUnitTestFixture {

        #region static fields and properties

        private static float MaxDelta = 0.000001f;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            Container.Bind<Geometry2D>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        [TestCaseSource(typeof(Geometry2DTestCases), "IsPointWithinTriangleTestCases")]
        public bool IsPointWithinTriangleTests(Geometry2DTestCases.IsPointWithinTriangleData data) {
            var geometry2D = Container.Resolve<Geometry2D>();

            return geometry2D.IsPointWithinTriangle(data.Point, data.V1, data.V2, data.V3);
        }

        [Test]
        [TestCaseSource(typeof(Geometry2DTestCases), "GetBarycentric2DTestCases")]
        public void GetBarycentric2DTests(Geometry2DTestCases.GetBarycentric2DData data) {
            var geometry2D = Container.Resolve<Geometry2D>();

            float aCoord, bCoord, cCoord;

            geometry2D.GetBarycentric2D(data.Point, data.A, data.B, data.C, out aCoord, out bCoord, out cCoord);

            Assert.That(Mathf.Abs(data.ExpectedACoord - aCoord) <= MaxDelta, string.Format("Unexpected ACoord returned. Expected {0} but got {1}", data.ExpectedACoord, aCoord));
            Assert.That(Mathf.Abs(data.ExpectedBCoord - bCoord) <= MaxDelta, string.Format("Unexpected BCoord returned. Expected {0} but got {1}", data.ExpectedBCoord, bCoord));
            Assert.That(Mathf.Abs(data.ExpectedCCoord - cCoord) <= MaxDelta, string.Format("Unexpected CCoord returned. Expected {0} but got {1}", data.ExpectedCCoord, cCoord));
        }

        [Test]
        [TestCaseSource(typeof(Geometry2DTestCases), "AreLineSegmentsCrossingTestCases")]
        public bool AreLineSegmentsCrossingTests(Geometry2DTestCases.AreLineSegmentsCrossingData data) {
            var geometry2D = Container.Resolve<Geometry2D>();

            Vector2 closestPointA, closestPointB;

            bool retval = geometry2D.AreLineSegmentsCrossing(
                data.PointA1, data.PointA2, out closestPointA,
                data.PointB1, data.PointB2, out closestPointB
            );

            if(!data.IgnoreExpectedPoints) {
                Assert.That(
                    Vector2.Distance(data.ExpectedClosestPointA, closestPointA) <= MaxDelta,
                    string.Format("Unexpected ClosestPointA returned. Expected {0} but got {1}", data.ExpectedClosestPointA, closestPointA)
                );

                Assert.That(
                    Vector2.Distance(data.ExpectedClosestPointB, closestPointB) <= MaxDelta,
                    string.Format("Unexpected ClosestPointB returned. Expected {0} but got {1}", data.ExpectedClosestPointB, closestPointB)
                );            
            }

            return retval;
        }

        [Test]
        [TestCaseSource(typeof(Geometry2DTestCases), "ClosestPointsOnTwoLinesTestCases")]
        public bool ClosestPointsOnTwoLinesTests(Geometry2DTestCases.ClosestPointsOnTwoLinesData data) {
            var geometry2D = Container.Resolve<Geometry2D>();

            Vector2 closestPointLineOne, closestPointLineTwo;

            bool retval = geometry2D.ClosestPointsOnTwoLines(
                data.LineOnePoint, data.LineOneVector, data.LineTwoPoint, data.LineTwoVector,
                out closestPointLineOne, out closestPointLineTwo
            );

            Assert.That(
                Vector2.Distance(data.ExpectedClosestPointLineOne, closestPointLineOne) <= MaxDelta,
                string.Format(
                    "Unexpected ClosestPointLineOne returned. Expected {0} but got {1}",
                    data.ExpectedClosestPointLineOne, closestPointLineOne
                )
            );

            Assert.That(
                Vector2.Distance(data.ExpectedClosestPointLineTwo, closestPointLineTwo) <= MaxDelta,
                string.Format(
                    "Unexpected ClosestPointLineTwo returned. Expected {0} but got {1}",
                    data.ExpectedClosestPointLineTwo, closestPointLineTwo
                )
            );

            return retval;
        }

        [Test]
        [TestCaseSource(typeof(Geometry2DTestCases), "GetClosestPointOnLineSegmentTestCases")]
        public void GetClosestPointOnLineSegmentTests(Geometry2DTestCases.GetClosestPointOnLineSegmentData data) {
            var geometry2D = Container.Resolve<Geometry2D>();

            Vector2 nearestPoint = geometry2D.GetClosestPointOnLineSegment(data.Start, data.End, data.Point);

            Assert.That(
                Vector2.Distance(data.ExpectedResult, nearestPoint) <= MaxDelta,
                string.Format("Unexpected value returned. Expected {0} but got {1}", data.ExpectedResult, nearestPoint)
            );
        }

        #endregion

        #endregion

    }

}
