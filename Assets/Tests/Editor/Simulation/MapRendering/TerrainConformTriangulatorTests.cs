using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using NUnit.Framework;
using Zenject;
using Moq;

using Assets.Simulation.MapRendering;

namespace Assets.Tests.Simulation.MapRendering {

    public class TerrainConformTriangulatorTests : ZenjectUnitTestFixture {

        #region internal types

        public class AddConformingTriangleTestData {

            public Vector3 VertexOne, VertexTwo, VertexThree;
            public Vector2 UVOne,     UVTwo,     UVThree;
            public Color   ColorOne,  ColorTwo,  ColorThree;

            public float MaxSideLength;

        }

        #endregion

        #region static fields and properties

        public static IEnumerable LargeTriangleTestCases {
            get {
                yield return new TestCaseData(new AddConformingTriangleTestData() {
                    VertexOne   = new Vector3(5f,    7f,  -2f),  UVOne   = new Vector2(0f, 0f), ColorOne   = Color.red,
                    VertexTwo   = new Vector3(100f, -64f,  18f), UVTwo   = new Vector2(1f, 0f), ColorTwo   = Color.green,
                    VertexThree = new Vector3(80f,   200f, 6f),  UVThree = new Vector2(0f, 1f), ColorThree = Color.blue,
                    MaxSideLength = 10f
                }).SetName("(5f, 7f, -2f), (100f, -64, 18f), (80f, 200f, 6f) with side length 10f");
            }
        }

        #endregion

        #region instance fields and properties

        private Mock<IMapCollisionLogic> MockMapCollisionLogic;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockMapCollisionLogic = new Mock<IMapCollisionLogic>();

            Container.Bind<IMapCollisionLogic>().FromInstance(MockMapCollisionLogic.Object);

            Container.Bind<TerrainConformTriangulator>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void AddConformingTriangle_AndAllEdgesShorterThanMaxSideLength_AddsTriangle() {
            Vector3 vertexOne   = new Vector3(1f, 1f, 0f);
            Vector3 vertexTwo   = new Vector3(1f, 4f, 0f);
            Vector3 vertexThree = new Vector3(4f, 1f, 0f);

            Vector3 onTerrainVertexOne   = new Vector3(1f, 11f, 111f);
            Vector3 onTerrainVertexTwo   = new Vector3(2f, 22f, 222f);
            Vector3 onTerrainVertexThree = new Vector3(3f, 33f, 333f);

            MockMapCollisionLogic.Setup(logic => logic.GetNearestMapPointToPoint(vertexOne  )).Returns(onTerrainVertexOne);
            MockMapCollisionLogic.Setup(logic => logic.GetNearestMapPointToPoint(vertexTwo  )).Returns(onTerrainVertexTwo);
            MockMapCollisionLogic.Setup(logic => logic.GetNearestMapPointToPoint(vertexThree)).Returns(onTerrainVertexThree);

            Vector2 uvOne   = new Vector2(1f, 1f);
            Vector2 uvTwo   = new Vector2(2f, 2f);
            Vector2 uvThree = new Vector2(3f, 3f);

            Color colorOne   = Color.red;
            Color colorTwo   = Color.green;
            Color colorThree = Color.blue;

            var mockMesh = new Mock<IHexMesh>();

            var triangulator = Container.Resolve<TerrainConformTriangulator>();

            triangulator.AddConformingTriangle(
                vertexOne,   uvOne,   colorOne,
                vertexTwo,   uvTwo,   colorTwo,
                vertexThree, uvThree, colorThree,
                5f, mockMesh.Object
            );

            mockMesh.Verify(
                mesh => mesh.AddTriangle(onTerrainVertexOne, onTerrainVertexTwo, onTerrainVertexThree),
                Times.Once, "Failed to add correct vertices"
            );

            mockMesh.Verify(
                mesh => mesh.AddTriangleUV(uvOne, uvTwo, uvThree), Times.Once, "Failed to add correct UVs"
            );

            mockMesh.Verify(
                mesh => mesh.AddTriangleColor(colorOne, colorTwo, colorThree), Times.Once, "Failed to add correct colors"
            );
        }

        [Test]
        public void AddConformingTriangle_AndSomeSideTooLong_SplitsTriangleInHalf() {
            Vector3 vertexOne   = new Vector3(1f, 1f, 0f);
            Vector3 vertexTwo   = new Vector3(1f, 5f, 0f);
            Vector3 vertexThree = new Vector3(5f, 1f, 0f);

            Vector3 onTerrainVertexOne   = new Vector3(1f, 11f, 111f);
            Vector3 onTerrainVertexTwo   = new Vector3(2f, 22f, 222f);
            Vector3 onTerrainVertexThree = new Vector3(3f, 33f, 333f);

            Vector3 onTerrainVertexTwoThree = new Vector3(4f, 44f, 444f);

            MockMapCollisionLogic.Setup(logic => logic.GetNearestMapPointToPoint(vertexOne  )).Returns(onTerrainVertexOne);
            MockMapCollisionLogic.Setup(logic => logic.GetNearestMapPointToPoint(vertexTwo  )).Returns(onTerrainVertexTwo);
            MockMapCollisionLogic.Setup(logic => logic.GetNearestMapPointToPoint(vertexThree)).Returns(onTerrainVertexThree);

            MockMapCollisionLogic.Setup(
                logic => logic.GetNearestMapPointToPoint((vertexTwo + vertexThree) / 2f)
            ).Returns(onTerrainVertexTwoThree);

            Vector2 uvOne   = new Vector2(1f, 1f);
            Vector2 uvTwo   = new Vector2(2f, 2f);
            Vector2 uvThree = new Vector2(3f, 3f);

            Color colorOne   = Color.red;
            Color colorTwo   = Color.green;
            Color colorThree = Color.blue;

            var mockMesh = new Mock<IHexMesh>();

            var triangulator = Container.Resolve<TerrainConformTriangulator>();

            triangulator.AddConformingTriangle(
                vertexOne,   uvOne,   colorOne,
                vertexTwo,   uvTwo,   colorTwo,
                vertexThree, uvThree, colorThree,
                5f, mockMesh.Object
            );

            mockMesh.Verify(
                mesh => mesh.AddTriangle(onTerrainVertexOne, onTerrainVertexTwo, onTerrainVertexTwoThree),
                Times.Once, "Failed to add vertices for first expected triangle"
            );

            mockMesh.Verify(
                mesh => mesh.AddTriangleUV(uvOne, uvTwo, (uvTwo + uvThree) / 2f),
                Times.Once, "Failed to add UVs for first expected triangle"
            );

            mockMesh.Verify(
                mesh => mesh.AddTriangleColor(colorOne, colorTwo, (colorTwo + colorThree) / 2f),
                Times.Once, "Failed to add colors for first expected triangle"
            );

            mockMesh.Verify(
                mesh => mesh.AddTriangle(onTerrainVertexOne, onTerrainVertexTwoThree, onTerrainVertexThree),
                Times.Once, "Failed to add vertices for second expected triangle"
            );

            mockMesh.Verify(
                mesh => mesh.AddTriangleUV(uvOne, (uvTwo + uvThree) / 2f, uvThree),
                Times.Once, "Failed to add UVs for second expected triangle"
            );

            mockMesh.Verify(
                mesh => mesh.AddTriangleColor(colorOne, (colorTwo + colorThree) / 2f, colorThree),
                Times.Once, "Failed to add colors for second expected triangle"
            );
        }

        [Test]
        [TestCaseSource("LargeTriangleTestCases")]
        public void AddConformingTriangle_AndTriangleVeryLarge_AllResultingTrianglesHaveShortEdges(AddConformingTriangleTestData data) {
            MockMapCollisionLogic.Setup(logic => logic.GetNearestMapPointToPoint(It.IsAny<Vector3>()))
                                 .Returns<Vector3>(point => point);

            var mockMesh = new Mock<IHexMesh>();

            mockMesh.Setup(
                mesh => mesh.AddTriangle(It.IsAny<Vector3>(), It.IsAny<Vector3>(), It.IsAny<Vector3>())
            ).Callback<Vector3, Vector3, Vector3>((vertexOne, vertexTwo, vertexThree) => {
                Assert.LessOrEqual(
                    (vertexOne - vertexTwo).magnitude, data.MaxSideLength,
                    string.Format("Side ({0} - {1}) has a side length greater than the argued maximum", vertexOne, vertexTwo)
                );

                Assert.LessOrEqual(
                    (vertexOne - vertexThree).magnitude, data.MaxSideLength,
                    string.Format("Side ({0} - {1}) has a side length greater than the argued maximum", vertexOne, vertexThree)
                );

                Assert.LessOrEqual(
                    (vertexTwo - vertexThree).magnitude, data.MaxSideLength,
                    string.Format("Side ({0} - {1}) has a side length greater than the argued maximum", vertexTwo, vertexThree)
                );
            });

            var triangulator = Container.Resolve<TerrainConformTriangulator>();

            triangulator.AddConformingTriangle(
                data.VertexOne,   data.UVOne,   data.ColorOne,
                data.VertexTwo,   data.UVTwo,   data.ColorTwo,
                data.VertexThree, data.UVThree, data.ColorThree,
                data.MaxSideLength, mockMesh.Object
            );
        }

        [Test]
        public void AddConformingTriangle_ThrowsExceptionIfMaxSideLengthVerySmall() {
            var mesh = new Mock<IHexMesh>().Object;

            var triangulator = Container.Resolve<TerrainConformTriangulator>();

            Assert.Throws<ArgumentOutOfRangeException>(
                () => triangulator.AddConformingTriangle(
                    Vector3.zero, Vector2.zero, Color.black,
                    Vector3.zero, Vector2.zero, Color.black,
                    Vector3.zero, Vector2.zero, Color.black,
                    0f, mesh
                ), "Failed to throw when maxSideLength was zero"
            );

            Assert.Throws<ArgumentOutOfRangeException>(
                () => triangulator.AddConformingTriangle(
                    Vector3.zero, Vector2.zero, Color.black,
                    Vector3.zero, Vector2.zero, Color.black,
                    Vector3.zero, Vector2.zero, Color.black,
                    0.001f, mesh
                ), "Failed to throw when maxSideLength was 0.001f"
            );

            Assert.Throws<ArgumentOutOfRangeException>(
                () => triangulator.AddConformingTriangle(
                    Vector3.zero, Vector2.zero, Color.black,
                    Vector3.zero, Vector2.zero, Color.black,
                    Vector3.zero, Vector2.zero, Color.black,
                    0.01f, mesh
                ), "Failed to throw when maxSideLength was 0.01f"
            );

            Assert.DoesNotThrow(
                () => triangulator.AddConformingTriangle(
                    Vector3.zero, Vector2.zero, Color.black,
                    Vector3.zero, Vector2.zero, Color.black,
                    Vector3.zero, Vector2.zero, Color.black,
                    0.012f, mesh
                ), "Unexpectedly threw when maxSideLength was 0.012f"
            );
        }

        [Test]
        public void AddConformingTriangle_ThrowsWhenMeshIsNull() {
            var triangulator = Container.Resolve<TerrainConformTriangulator>();

            Assert.Throws<ArgumentNullException>(() => triangulator.AddConformingTriangle(
                Vector3.zero, Vector2.zero, Color.black,
                Vector3.zero, Vector2.zero, Color.black,
                Vector3.zero, Vector2.zero, Color.black,
                1f, null
            ));
        }

        #endregion

        #region utilities



        #endregion

        #endregion

    }

}
