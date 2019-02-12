using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using NUnit.Framework;
using Zenject;
using Moq;

using Assets.Simulation.HexMap;
using Assets.Simulation.MapRendering;
using Assets.Util;

namespace Assets.Tests.Simulation.MapRendering {

    public class TerrainMixingLogicTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IMapRenderConfig>    MockRenderConfig;
        private Mock<IGeometry2D>         MockGeometry2D;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockRenderConfig = new Mock<IMapRenderConfig>();
            MockGeometry2D   = new Mock<IGeometry2D>();

            Container.Bind<IMapRenderConfig>().FromInstance(MockRenderConfig.Object);
            Container.Bind<IGeometry2D>     ().FromInstance(MockGeometry2D  .Object);

            Container.Bind<TerrainMixingLogic>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void GetMixForEdgeAtPoint_AndPointAtCenterSolidEdge_ReturnsExclusivelyValueFromCenter() {
            var center = BuildCell(new Vector3(10f, 0f, 0f));
            var right  = BuildCell(new Vector3(40f, 0f, 0f));

            MockRenderConfig.Setup(config => config.GetSolidEdgeMidpoint(HexDirection.E)).Returns(new Vector3(10f, 0f, 0f));
            MockRenderConfig.Setup(config => config.GetSolidEdgeMidpoint(HexDirection.W)).Returns(new Vector3(-10f, 0f, 0f));

            var point = new Vector3(20f, 15f, 60.5f);

            DataSelectorCallback<float> dataSelector = delegate(Vector3 pos, IHexCell cell, HexDirection sextant, float weight) {
                if(point != pos) {
                    Assert.Fail("Unexpected point passed into selector");
                }

                if(cell == center && sextant == HexDirection.E) {
                    return 20f * weight;
                }else if(cell == right && sextant == HexDirection.W) {
                    return 100f * weight;
                }else {
                    return 0f;
                }
            };

            var mixingLogic = Container.Resolve<TerrainMixingLogic>();

            Assert.AreEqual(
                20f, mixingLogic.GetMixForEdgeAtPoint(center, right, HexDirection.E, point, dataSelector, (a, b) => a + b)
            );
        }

        [Test]
        public void GetMixForEdgeAtPoint_AndPointAtRightSolidEdge_ReturnsExclusivelyHeightFromRight() {
            var center = BuildCell(new Vector3(10f, 0f, 0f));
            var right  = BuildCell(new Vector3(40f, 0f, 0f));

            MockRenderConfig.Setup(config => config.GetSolidEdgeMidpoint(HexDirection.E)).Returns(new Vector3(10f, 0f, 0f));
            MockRenderConfig.Setup(config => config.GetSolidEdgeMidpoint(HexDirection.W)).Returns(new Vector3(-10f, 0f, 0f));

            var point = new Vector3(30f, 15f, 60.5f);

            DataSelectorCallback<float> dataSelector = delegate(Vector3 pos, IHexCell cell, HexDirection sextant, float weight) {
                if(point != pos) {
                    Assert.Fail("Unexpected point passed into selector");
                }

                if(cell == center && sextant == HexDirection.E) {
                    return 20f * weight;
                }else if(cell == right && sextant == HexDirection.W) {
                    return 100f * weight;
                }else {
                    return 0f;
                }
            };

            var mixingLogic = Container.Resolve<TerrainMixingLogic>();

            Assert.AreEqual(
                100f, mixingLogic.GetMixForEdgeAtPoint(center, right, HexDirection.E, point, dataSelector, (a, b) => a + b)
            );
        }

        [Test]
        public void GetMixForEdgeAtPoint_AndPointAtBoundaryBetweenCells_ReturnsEqualWeightsFromBothCenterAndRight() {
            var center = BuildCell(new Vector3(10f, 0f, 0f));
            var right  = BuildCell(new Vector3(40f, 0f, 0f));

            MockRenderConfig.Setup(config => config.GetSolidEdgeMidpoint(HexDirection.E)).Returns(new Vector3(10f, 0f, 0f));
            MockRenderConfig.Setup(config => config.GetSolidEdgeMidpoint(HexDirection.W)).Returns(new Vector3(-10f, 0f, 0f));

            var point = new Vector3(25f, 15f, 60.5f);

            DataSelectorCallback<float> dataSelector = delegate(Vector3 pos, IHexCell cell, HexDirection sextant, float weight) {
                if(point != pos) {
                    Assert.Fail("Unexpected point passed into selector");
                }

                if(cell == center && sextant == HexDirection.E) {
                    return 20f * weight;
                }else if(cell == right && sextant == HexDirection.W) {
                    return 100f * weight;
                }else {
                    return 0f;
                }
            };

            var mixingLogic = Container.Resolve<TerrainMixingLogic>();

            Assert.AreEqual(
                60f, mixingLogic.GetMixForEdgeAtPoint(center, right, HexDirection.E, point, dataSelector, (a, b) => a + b)
            );
        }

        [Test]
        public void GetMixForEdgeAtPoint_AndPointSomewhereInEdge_WeightsCenterAndRightHeight_BasedOnRelativeDistance() {
            var center = BuildCell(new Vector3(10f, 0f, 0f));
            var right  = BuildCell(new Vector3(40f, 0f, 0f));

            MockRenderConfig.Setup(config => config.GetSolidEdgeMidpoint(HexDirection.E)).Returns(new Vector3(10f, 0f, 0f));
            MockRenderConfig.Setup(config => config.GetSolidEdgeMidpoint(HexDirection.W)).Returns(new Vector3(-10f, 0f, 0f));

            var point = new Vector3(27.5f, 15f, 60.5f);

            DataSelectorCallback<float> dataSelector = delegate(Vector3 pos, IHexCell cell, HexDirection sextant, float weight) {
                if(point != pos) {
                    Assert.Fail("Unexpected point passed into selector");
                }

                if(cell == center && sextant == HexDirection.E) {
                    return 20f * weight;
                }else if(cell == right && sextant == HexDirection.W) {
                    return 100f * weight;
                }else {
                    return 0f;
                }
            };

            var mixingLogic = Container.Resolve<TerrainMixingLogic>();

            Assert.AreEqual(
                20f * 0.25f + 100f * 0.75f, mixingLogic.GetMixForEdgeAtPoint(center, right, HexDirection.E, point, dataSelector, (a, b) => a + b)
            );
        }

        [Test]
        public void GetMixForPreviousCornerAtPoint_WeightsHeightsFromAllThreeCells_WithBarycentricCoordinatesInCornerTriangle() {
            var center = BuildCell(new Vector3(10f, 0f,  0f));
            var left   = BuildCell(new Vector3(0f,  20f, 0f));
            var right  = BuildCell(new Vector3(0f,  0f,  30f));

            MockRenderConfig.Setup(config => config.GetFirstSolidCornerXZ(HexDirection.E )).Returns(new Vector2(1.5f, 0f));
            MockRenderConfig.Setup(config => config.GetFirstSolidCornerXZ(HexDirection.SW)).Returns(new Vector2(0f,   2.5f));
            MockRenderConfig.Setup(config => config.GetFirstSolidCornerXZ(HexDirection.NW)).Returns(new Vector2(3.5f, 3.5f));

            var point = new Vector3(100f, 200f, 300f);

            float expectedCenterWeight = 0.5f, expectedLeftWeight = 0.3f, expectedRightWeight = 0.2f;
            MockGeometry2D.Setup(geometry => geometry.GetBarycentric2D(
                new Vector2(100f, 300f), new Vector2(11.5f, 0f), new Vector2(0f, 2.5f), new Vector2(3.5f, 33.5f),
                out expectedCenterWeight, out expectedLeftWeight, out expectedRightWeight
            ));

            DataSelectorCallback<float> dataSelector = delegate(Vector3 pos, IHexCell cell, HexDirection sextant, float weight) {
                if(point != pos) {
                    Assert.Fail("Unexpected point passed into selector");
                }

                if(cell == center && sextant == HexDirection.E) {
                    return 10f * weight;
                }else if(cell == left && sextant == HexDirection.SW) {
                    return 100f * weight;
                }else if(cell == right && sextant == HexDirection.NW) {
                    return 1000f * weight;
                }else {
                    return 0f;
                }
            };

            var mixingLogic = Container.Resolve<TerrainMixingLogic>();

            Assert.AreEqual(
                10f * 0.5f + 100f * 0.3f + 1000f * 0.2f,
                mixingLogic.GetMixForPreviousCornerAtPoint(center, left, right, HexDirection.E, point, dataSelector, (a, b) => a + b)
            );
        }

        [Test]
        public void GetMixForNextCornerAtPoint_WeightsHeightsFromAllThreeCells_WithBarycentricCoordinatesInCornerTriangle() {
            var center    = BuildCell(new Vector3(10f, 0f,  0f));
            var right     = BuildCell(new Vector3(0f,  20f, 0f));
            var nextRight = BuildCell(new Vector3(0f,  0f,  30f));

            MockRenderConfig.Setup(config => config.GetSecondSolidCornerXZ(HexDirection.E )).Returns(new Vector2(1.5f, 0f));
            MockRenderConfig.Setup(config => config.GetSecondSolidCornerXZ(HexDirection.SW)).Returns(new Vector2(0f,   2.5f));
            MockRenderConfig.Setup(config => config.GetSecondSolidCornerXZ(HexDirection.NW)).Returns(new Vector2(3.5f, 3.5f));

            var point = new Vector3(100f, 200f, 300f);

            float expectedCenterWeight = 0.5f, expectedRightWeight = 0.3f, expectedNextRightWeight = 0.2f;
            MockGeometry2D.Setup(geometry => geometry.GetBarycentric2D(
                new Vector2(100f, 300f), new Vector2(11.5f, 0f), new Vector2(0f, 2.5f), new Vector2(3.5f, 33.5f),
                out expectedCenterWeight, out expectedRightWeight, out expectedNextRightWeight
            ));

            DataSelectorCallback<float> dataSelector = delegate(Vector3 pos, IHexCell cell, HexDirection sextant, float weight) {
                if(point != pos) {
                    Assert.Fail("Unexpected point passed into selector");
                }

                if(cell == center && sextant == HexDirection.E) {
                    return 10f * weight;
                }else if(cell == right && sextant == HexDirection.SW) {
                    return 100f * weight;
                }else if(cell == nextRight && sextant == HexDirection.NW) {
                    return 1000f * weight;
                }else {
                    return 0f;
                }
            };

            var mixingLogic = Container.Resolve<TerrainMixingLogic>();

            Assert.AreEqual(
                10f * 0.5f + 100f * 0.3f + 1000f * 0.2f,
                mixingLogic.GetMixForNextCornerAtPoint(center, right, nextRight, HexDirection.E, point, dataSelector, (a, b) => a + b)
            );
        }

        #endregion

        #region utilities

        private IHexCell BuildCell(Vector3 absolutePosition) {
            var mockCell = new Mock<IHexCell>();

            mockCell.Setup(cell => cell.AbsolutePosition  ).Returns(absolutePosition);
            mockCell.Setup(cell => cell.AbsolutePositionXZ).Returns(new Vector2(absolutePosition.x, absolutePosition.z));

            return mockCell.Object;
        }

        #endregion

        #endregion

    }

}
