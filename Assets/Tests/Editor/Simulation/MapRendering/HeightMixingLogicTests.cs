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

    public class HeightMixingLogicTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IMapRenderConfig>    MockRenderConfig;
        private Mock<ICellHeightmapLogic> MockCellHeightmapLogic;
        private Mock<IGeometry2D>         MockGeometry2D;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockRenderConfig       = new Mock<IMapRenderConfig>();
            MockCellHeightmapLogic = new Mock<ICellHeightmapLogic>();
            MockGeometry2D         = new Mock<IGeometry2D>();

            Container.Bind<IMapRenderConfig>   ().FromInstance(MockRenderConfig      .Object);
            Container.Bind<ICellHeightmapLogic>().FromInstance(MockCellHeightmapLogic.Object);
            Container.Bind<IGeometry2D>        ().FromInstance(MockGeometry2D        .Object);

            Container.Bind<HeightMixingLogic>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void GetMixForEdgeAtPoint_AndPointAtCenterSolidEdge_ReturnsExclusivelyHeightFromCenter() {
            var center = BuildCell(new Vector3(10f, 0f, 0f));
            var right  = BuildCell(new Vector3(40f, 0f, 0f));

            MockRenderConfig.Setup(config => config.GetSolidEdgeMidpoint(HexDirection.E)).Returns(new Vector3(10f, 0f, 0f));
            MockRenderConfig.Setup(config => config.GetSolidEdgeMidpoint(HexDirection.W)).Returns(new Vector3(-10f, 0f, 0f));

            var point = new Vector3(20f, 15f, 60.5f);

            MockCellHeightmapLogic.Setup(logic => logic.GetHeightForPositionForCell(point, center, HexDirection.E)).Returns(20f);
            MockCellHeightmapLogic.Setup(logic => logic.GetHeightForPositionForCell(point, right,  HexDirection.W)).Returns(100f);

            var mixingLogic = Container.Resolve<HeightMixingLogic>();

            Assert.AreEqual(20f, mixingLogic.GetMixForEdgeAtPoint(center, right, HexDirection.E, point));
        }

        [Test]
        public void GetMixForEdgeAtPoint_AndPointAtRightSolidEdge_ReturnsExclusivelyHeightFromRight() {
            var center = BuildCell(new Vector3(10f, 0f, 0f));
            var right  = BuildCell(new Vector3(40f, 0f, 0f));

            MockRenderConfig.Setup(config => config.GetSolidEdgeMidpoint(HexDirection.E)).Returns(new Vector3(10f, 0f, 0f));
            MockRenderConfig.Setup(config => config.GetSolidEdgeMidpoint(HexDirection.W)).Returns(new Vector3(-10f, 0f, 0f));

            var point = new Vector3(30f, 15f, 60.5f);

            MockCellHeightmapLogic.Setup(logic => logic.GetHeightForPositionForCell(point, center, HexDirection.E)).Returns(20f);
            MockCellHeightmapLogic.Setup(logic => logic.GetHeightForPositionForCell(point, right,  HexDirection.W)).Returns(100f);

            var mixingLogic = Container.Resolve<HeightMixingLogic>();

            Assert.AreEqual(100f, mixingLogic.GetMixForEdgeAtPoint(center, right, HexDirection.E, point));
        }

        [Test]
        public void GetMixForEdgeAtPoint_AndPointAtBoundaryBetweenCells_ReturnsEqualWeightsFromBothCenterAndRight() {
            var center = BuildCell(new Vector3(10f, 0f, 0f));
            var right  = BuildCell(new Vector3(40f, 0f, 0f));

            MockRenderConfig.Setup(config => config.GetSolidEdgeMidpoint(HexDirection.E)).Returns(new Vector3(10f, 0f, 0f));
            MockRenderConfig.Setup(config => config.GetSolidEdgeMidpoint(HexDirection.W)).Returns(new Vector3(-10f, 0f, 0f));

            var point = new Vector3(25f, 15f, 60.5f);

            MockCellHeightmapLogic.Setup(logic => logic.GetHeightForPositionForCell(point, center, HexDirection.E)).Returns(20f);
            MockCellHeightmapLogic.Setup(logic => logic.GetHeightForPositionForCell(point, right,  HexDirection.W)).Returns(100f);

            var mixingLogic = Container.Resolve<HeightMixingLogic>();

            Assert.AreEqual(60f, mixingLogic.GetMixForEdgeAtPoint(center, right, HexDirection.E, point));
        }

        [Test]
        public void GetMixForEdgeAtPoint_AndPointSomewhereInEdge_WeightsCenterAndRightHeight_BasedOnRelativeDistance() {
            var center = BuildCell(new Vector3(10f, 0f, 0f));
            var right  = BuildCell(new Vector3(40f, 0f, 0f));

            MockRenderConfig.Setup(config => config.GetSolidEdgeMidpoint(HexDirection.E)).Returns(new Vector3(10f, 0f, 0f));
            MockRenderConfig.Setup(config => config.GetSolidEdgeMidpoint(HexDirection.W)).Returns(new Vector3(-10f, 0f, 0f));

            var point = new Vector3(27.5f, 15f, 60.5f);

            MockCellHeightmapLogic.Setup(logic => logic.GetHeightForPositionForCell(point, center, HexDirection.E)).Returns(20f);
            MockCellHeightmapLogic.Setup(logic => logic.GetHeightForPositionForCell(point, right,  HexDirection.W)).Returns(100f);

            var mixingLogic = Container.Resolve<HeightMixingLogic>();

            Assert.AreEqual(20f * 0.25f + 100f * 0.75f, mixingLogic.GetMixForEdgeAtPoint(center, right, HexDirection.E, point));
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

            MockCellHeightmapLogic.Setup(logic => logic.GetHeightForPositionForCell(point, center, HexDirection.E )).Returns(10f);
            MockCellHeightmapLogic.Setup(logic => logic.GetHeightForPositionForCell(point, left,   HexDirection.SW)).Returns(100f);
            MockCellHeightmapLogic.Setup(logic => logic.GetHeightForPositionForCell(point, right,  HexDirection.NW)).Returns(1000f);

            var mixingLogic = Container.Resolve<HeightMixingLogic>();

            Assert.AreEqual(
                10f * 0.5f + 100f * 0.3f + 1000f * 0.2f,
                mixingLogic.GetMixForPreviousCornerAtPoint(center, left, right, HexDirection.E, point)
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

            MockCellHeightmapLogic.Setup(logic => logic.GetHeightForPositionForCell(point, center,    HexDirection.E )).Returns(10f);
            MockCellHeightmapLogic.Setup(logic => logic.GetHeightForPositionForCell(point, right,     HexDirection.SW)).Returns(100f);
            MockCellHeightmapLogic.Setup(logic => logic.GetHeightForPositionForCell(point, nextRight, HexDirection.NW)).Returns(1000f);

            var mixingLogic = Container.Resolve<HeightMixingLogic>();

            Assert.AreEqual(
                10f * 0.5f + 100f * 0.3f + 1000f * 0.2f,
                mixingLogic.GetMixForNextCornerAtPoint(center, right, nextRight, HexDirection.E, point)
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
