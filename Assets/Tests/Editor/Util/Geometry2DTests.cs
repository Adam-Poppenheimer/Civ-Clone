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

    public class Geometry2DTests : ZenjectUnitTestFixture {

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            Container.Bind<Geometry2D>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void IsPointWithinTriangle_TrueIfPointOnCorrectSideOfAllVertices() {
            Vector2 v1 = new Vector2(5f, 5f);
            Vector2 v2 = new Vector2(7f, 40f);
            Vector2 v3 = new Vector2(10f, 20f);

            Vector2 point = (v1 + v2 + v3) / 3f;

            var geometry2D = Container.Resolve<Geometry2D>();

            Assert.IsTrue(geometry2D.IsPointWithinTriangle(point, v1, v2, v3));
        }

        [Test]
        public void IsPointWithinTriangle_TrueIfPointAtV1() {
            Vector2 v1 = new Vector2(5f, 5f);
            Vector2 v2 = new Vector2(7f, 40f);
            Vector2 v3 = new Vector2(10f, 20f);

            Vector2 point = v1;

            var geometry2D = Container.Resolve<Geometry2D>();

            Assert.IsTrue(geometry2D.IsPointWithinTriangle(point, v1, v2, v3));
        }

        [Test]
        public void IsPointWithinTriangle_TrueIfPointAtV2() {
            Vector2 v1 = new Vector2(5f, 5f);
            Vector2 v2 = new Vector2(7f, 40f);
            Vector2 v3 = new Vector2(10f, 20f);

            Vector2 point = v2;

            var geometry2D = Container.Resolve<Geometry2D>();

            Assert.IsTrue(geometry2D.IsPointWithinTriangle(point, v1, v2, v3));
        }

        [Test]
        public void IsPointWithinTriangle_TrueIfPointAtV3() {
            Vector2 v1 = new Vector2(5f, 5f);
            Vector2 v2 = new Vector2(7f, 40f);
            Vector2 v3 = new Vector2(10f, 20f);

            Vector2 point = v3;

            var geometry2D = Container.Resolve<Geometry2D>();

            Assert.IsTrue(geometry2D.IsPointWithinTriangle(point, v1, v2, v3));
        }

        [Test]
        public void IsPointWithinTriangle_TrueIfOnV1V2Edge() {
            Vector2 v1 = new Vector2(5f, 5f);
            Vector2 v2 = new Vector2(7f, 40f);
            Vector2 v3 = new Vector2(10f, 20f);

            Vector2 point = Vector2.Lerp(v1, v2, 0.72f);

            var geometry2D = Container.Resolve<Geometry2D>();

            Assert.IsTrue(geometry2D.IsPointWithinTriangle(point, v1, v2, v3));
        }

        [Test]
        public void IsPointWithinTriangle_TrueIfOnV1V3Edge() {
            Vector2 v1 = new Vector2(5f, 5f);
            Vector2 v2 = new Vector2(7f, 40f);
            Vector2 v3 = new Vector2(10f, 20f);

            Vector2 point = Vector2.Lerp(v1, v3, 0.68f);

            var geometry2D = Container.Resolve<Geometry2D>();

            Assert.IsTrue(geometry2D.IsPointWithinTriangle(point, v1, v2, v3));
        }

        [Test]
        public void IsPointWithinTriangle_TrueIfOnV2V3Edge() {
            Vector2 v1 = new Vector2(5f, 5f);
            Vector2 v2 = new Vector2(7f, 40f);
            Vector2 v3 = new Vector2(10f, 20f);

            Vector2 point = Vector2.Lerp(v2, v3, 0.72f);

            var geometry2D = Container.Resolve<Geometry2D>();

            Assert.IsTrue(geometry2D.IsPointWithinTriangle(point, v1, v2, v3));
        }

        [Test]
        public void IsPointWithinTriangle_FalseIfBeyondV1V2Edge() {
            Vector2 v1 = new Vector2(5f, 5f);
            Vector2 v2 = new Vector2(7f, 40f);
            Vector2 v3 = new Vector2(10f, 20f);

            Vector2 point = (v1 + v2) / 2f + Vector2.left;

            var geometry2D = Container.Resolve<Geometry2D>();

            Assert.IsFalse(geometry2D.IsPointWithinTriangle(point, v1, v2, v3));
        }

        [Test]
        public void IsPointWithinTriangle_FalseIfBeyondV1V3Edge() {
            Vector2 v1 = new Vector2(5f, 5f);
            Vector2 v2 = new Vector2(7f, 40f);
            Vector2 v3 = new Vector2(10f, 20f);

            Vector2 point = (v1 + v3) / 2f + Vector2.down;

            var geometry2D = Container.Resolve<Geometry2D>();

            Assert.IsFalse(geometry2D.IsPointWithinTriangle(point, v1, v2, v3));
        }

        [Test]
        public void IsPointWithinTriangle_FalseIfBeyondV2V3Edge() {
            Vector2 v1 = new Vector2(5f, 5f);
            Vector2 v2 = new Vector2(7f, 40f);
            Vector2 v3 = new Vector2(10f, 20f);

            Vector2 point = (v2 + v3) / 2f + Vector2.right;

            var geometry2D = Container.Resolve<Geometry2D>();

            Assert.IsFalse(geometry2D.IsPointWithinTriangle(point, v1, v2, v3));
        }

        #endregion

        #endregion

    }

}
