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

    public class MountainHeightmapWeightLogicTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IMapRenderConfig> MockRenderConfig;
        private Mock<IHexGrid>         MockGrid;
        private Mock<IGeometry2D>      MockGeometry2D;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockRenderConfig = new Mock<IMapRenderConfig>();
            MockGrid         = new Mock<IHexGrid>();
            MockGeometry2D   = new Mock<IGeometry2D>();

            Container.Bind<IMapRenderConfig>().FromInstance(MockRenderConfig.Object);
            Container.Bind<IHexGrid>        ().FromInstance(MockGrid        .Object);
            Container.Bind<IGeometry2D>     ().FromInstance(MockGeometry2D  .Object);

            Container.Bind<MountainHeightmapWeightLogic>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void GetHeightWeightsForPosition_AndPointInCCWHalfOfSolidRegion_ReturnsWeightsFromBarycentricCoords() {
            var cell = BuildCell(new Vector2(10f, 10f));

            var position = new Vector3(-1000f, 0f, -1000f);

            MockRenderConfig.Setup(config => config.GetFirstSolidCornerXZ (HexDirection.E)).Returns(new Vector2(10f, 10f));
            MockRenderConfig.Setup(config => config.GetSecondSolidCornerXZ(HexDirection.E)).Returns(new Vector2(10f, -10f));
            MockRenderConfig.Setup(config => config.GetSolidEdgeMidpointXZ(HexDirection.E)).Returns(new Vector2(10f, 0f));

            MockGeometry2D.Setup(
                geometry => geometry.IsPointWithinTriangle(
                    new Vector2(-1000f, -1000f), new Vector2(10f, 10f), new Vector2(20f, 20f), new Vector2(20f, 10f)
                )
            ).Returns(true);

            float aCoord = -1f, bCoord = -20f, cCoord = -300f;
            MockGeometry2D.Setup(
                geometry => geometry.GetBarycentric2D(
                    new Vector2(-1000f, -1000f), new Vector2(10f, 10f), new Vector2(20f, 20f), new Vector2(20f, 10f),
                    out aCoord, out bCoord, out cCoord
                )
            );

            var weightLogic = Container.Resolve<MountainHeightmapWeightLogic>();

            float peakWeight, ridgeWeight, hillsWeight;

            weightLogic.GetHeightWeightsForPoint(position, cell, HexDirection.E, out peakWeight, out ridgeWeight, out hillsWeight);

            Assert.AreEqual(aCoord, peakWeight,  "PeakWeight has an unexpected value");
            Assert.AreEqual(cCoord, ridgeWeight, "RidgeWeight has an unexpected value");
            Assert.AreEqual(bCoord, hillsWeight, "HillsWeight has an unexpected value");
        }

        [Test]
        public void GetHeightWeightsForPosition_AndPointInCWHalfOfSolidRegion_ReturnsWeightsFromBarycentricCoords() {
            var cell = BuildCell(new Vector2(10f, 10f));

            var position = new Vector3(-1000f, 0f, -1000f);

            MockRenderConfig.Setup(config => config.GetFirstSolidCornerXZ (HexDirection.E)).Returns(new Vector2(10f, 10f));
            MockRenderConfig.Setup(config => config.GetSecondSolidCornerXZ(HexDirection.E)).Returns(new Vector2(10f, -10f));
            MockRenderConfig.Setup(config => config.GetSolidEdgeMidpointXZ(HexDirection.E)).Returns(new Vector2(10f, 0f));

            MockGeometry2D.Setup(
                geometry => geometry.IsPointWithinTriangle(
                    new Vector2(-1000f, -1000f), new Vector2(10f, 10f), new Vector2(20f, 10f), new Vector2(20f, 0f)
                )
            ).Returns(true);

            float aCoord = -1f, bCoord = -20f, cCoord = -300f;
            MockGeometry2D.Setup(
                geometry => geometry.GetBarycentric2D(
                    new Vector2(-1000f, -1000f), new Vector2(10f, 10f), new Vector2(20f, 10f), new Vector2(20f, 0f),
                    out aCoord, out bCoord, out cCoord
                )
            );

            var weightLogic = Container.Resolve<MountainHeightmapWeightLogic>();

            float peakWeight, ridgeWeight, hillsWeight;

            weightLogic.GetHeightWeightsForPoint(position, cell, HexDirection.E, out peakWeight, out ridgeWeight, out hillsWeight);

            Assert.AreEqual(aCoord, peakWeight,  "PeakWeight has an unexpected value");
            Assert.AreEqual(bCoord, ridgeWeight, "RidgeWeight has an unexpected value");
            Assert.AreEqual(cCoord, hillsWeight, "HillsWeight has an unexpected value");
        }

        [Test]
        public void GetHeightWeightsForPosition_AndPointInCCWTriangleOfEdge_ReturnsWeightsFromBarycentricCoords() {
            var cell = BuildCell(new Vector2(10f, 10f));

            var neighbor = BuildCell(new Vector2(40f, 10f));

            MockGrid.Setup(grid => grid.HasNeighbor(cell, HexDirection.E)).Returns(true);
            MockGrid.Setup(grid => grid.GetNeighbor(cell, HexDirection.E)).Returns(neighbor);

            var position = new Vector3(-1000f, 0f, -1000f);

            MockRenderConfig.Setup(config => config.GetFirstSolidCornerXZ (HexDirection.E)).Returns(new Vector2(10f, 10f));
            MockRenderConfig.Setup(config => config.GetSecondSolidCornerXZ(HexDirection.E)).Returns(new Vector2(10f, -10f));
            MockRenderConfig.Setup(config => config.GetSolidEdgeMidpointXZ(HexDirection.E)).Returns(new Vector2(10f, 0f));

            MockRenderConfig.Setup(config => config.GetFirstSolidCornerXZ (HexDirection.W)).Returns(new Vector2(-10f, -10f));
            MockRenderConfig.Setup(config => config.GetSecondSolidCornerXZ(HexDirection.W)).Returns(new Vector2(-10f, 10f));

            MockGeometry2D.Setup(
                geometry => geometry.IsPointWithinTriangle(
                    new Vector2(-1000f, -1000f), new Vector2(20f, 10f), new Vector2(20f, 20f), new Vector2(30f, 20f)
                )
            ).Returns(true);

            float aCoord = -1f, bCoord = -20f, cCoord = -300f;
            MockGeometry2D.Setup(
                geometry => geometry.GetBarycentric2D(
                    new Vector2(-1000f, -1000f), new Vector2(20f, 10f), new Vector2(20f, 20f), new Vector2(30f, 20f),
                    out aCoord, out bCoord, out cCoord
                )
            );

            var weightLogic = Container.Resolve<MountainHeightmapWeightLogic>();

            float peakWeight, ridgeWeight, hillsWeight;

            weightLogic.GetHeightWeightsForPoint(position, cell, HexDirection.E, out peakWeight, out ridgeWeight, out hillsWeight);

            Assert.AreEqual(0f,              peakWeight,  "PeakWeight has an unexpected value");
            Assert.AreEqual(aCoord,          ridgeWeight, "RidgeWeight has an unexpected value");
            Assert.AreEqual(bCoord + cCoord, hillsWeight, "HillsWeight has an unexpected value");
        }

        [Test]
        public void GetHeightWeightsForPosition_AndPointInMiddleTriangleOfEdge_ReturnsWeightsFromBarycentricCoords() {
            var cell = BuildCell(new Vector2(10f, 10f));

            var neighbor = BuildCell(new Vector2(40f, 10f));

            MockGrid.Setup(grid => grid.HasNeighbor(cell, HexDirection.E)).Returns(true);
            MockGrid.Setup(grid => grid.GetNeighbor(cell, HexDirection.E)).Returns(neighbor);

            var position = new Vector3(-1000f, 0f, -1000f);

            MockRenderConfig.Setup(config => config.GetFirstSolidCornerXZ (HexDirection.E)).Returns(new Vector2(10f, 10f));
            MockRenderConfig.Setup(config => config.GetSecondSolidCornerXZ(HexDirection.E)).Returns(new Vector2(10f, -10f));
            MockRenderConfig.Setup(config => config.GetSolidEdgeMidpointXZ(HexDirection.E)).Returns(new Vector2(10f, 0f));

            MockRenderConfig.Setup(config => config.GetFirstSolidCornerXZ (HexDirection.W)).Returns(new Vector2(-10f, -10f));
            MockRenderConfig.Setup(config => config.GetSecondSolidCornerXZ(HexDirection.W)).Returns(new Vector2(-10f, 10f));

            MockGeometry2D.Setup(
                geometry => geometry.IsPointWithinTriangle(
                    new Vector2(-1000f, -1000f), new Vector2(20f, 10f), new Vector2(30f, 0f), new Vector2(30f, 20f)
                )
            ).Returns(true);

            float aCoord = -1f, bCoord = -20f, cCoord = -300f;
            MockGeometry2D.Setup(
                geometry => geometry.GetBarycentric2D(
                    new Vector2(-1000f, -1000f), new Vector2(20f, 10f), new Vector2(30f, 0f), new Vector2(30f, 20f),
                    out aCoord, out bCoord, out cCoord
                )
            );

            var weightLogic = Container.Resolve<MountainHeightmapWeightLogic>();

            float peakWeight, ridgeWeight, hillsWeight;

            weightLogic.GetHeightWeightsForPoint(position, cell, HexDirection.E, out peakWeight, out ridgeWeight, out hillsWeight);

            Assert.AreEqual(0f,                         peakWeight,  "PeakWeight has an unexpected value");
            Assert.AreEqual(1f - hillsWeight,           ridgeWeight, "RidgeWeight has an unexpected value");
            Assert.AreEqual(Mathf.Abs(bCoord - cCoord), hillsWeight, "HillsWeight has an unexpected value");
        }

        [Test]
        public void GetHeightWeightsForPosition_AndPointInCWTriangleOfEdge_ReturnsWeightsFromBarycentricCoords() {
            var cell = BuildCell(new Vector2(10f, 10f));

            var neighbor = BuildCell(new Vector2(40f, 10f));

            MockGrid.Setup(grid => grid.HasNeighbor(cell, HexDirection.E)).Returns(true);
            MockGrid.Setup(grid => grid.GetNeighbor(cell, HexDirection.E)).Returns(neighbor);

            var position = new Vector3(-1000f, 0f, -1000f);

            MockRenderConfig.Setup(config => config.GetFirstSolidCornerXZ (HexDirection.E)).Returns(new Vector2(10f, 10f));
            MockRenderConfig.Setup(config => config.GetSecondSolidCornerXZ(HexDirection.E)).Returns(new Vector2(10f, -10f));
            MockRenderConfig.Setup(config => config.GetSolidEdgeMidpointXZ(HexDirection.E)).Returns(new Vector2(10f, 0f));

            MockRenderConfig.Setup(config => config.GetFirstSolidCornerXZ (HexDirection.W)).Returns(new Vector2(-10f, -10f));
            MockRenderConfig.Setup(config => config.GetSecondSolidCornerXZ(HexDirection.W)).Returns(new Vector2(-10f, 10f));

            MockGeometry2D.Setup(
                geometry => geometry.IsPointWithinTriangle(
                    new Vector2(-1000f, -1000f), new Vector2(20f, 10f), new Vector2(30f, 0f), new Vector2(20f, 0f)
                )
            ).Returns(true);

            float aCoord = -1f, bCoord = -20f, cCoord = -300f;
            MockGeometry2D.Setup(
                geometry => geometry.GetBarycentric2D(
                    new Vector2(-1000f, -1000f), new Vector2(20f, 10f), new Vector2(30f, 0f), new Vector2(20f, 0f),
                    out aCoord, out bCoord, out cCoord
                )
            );

            var weightLogic = Container.Resolve<MountainHeightmapWeightLogic>();

            float peakWeight, ridgeWeight, hillsWeight;

            weightLogic.GetHeightWeightsForPoint(position, cell, HexDirection.E, out peakWeight, out ridgeWeight, out hillsWeight);

            Assert.AreEqual(0f,              peakWeight,  "PeakWeight has an unexpected value");
            Assert.AreEqual(aCoord,          ridgeWeight, "RidgeWeight has an unexpected value");
            Assert.AreEqual(bCoord + cCoord, hillsWeight, "HillsWeight has an unexpected value");
        }

        [Test]
        public void GetHeightWeightsForPosition_PointIsInEdge_ButNoNeighborExistsOnThatEdge_ReturnsOnlyHillWeight() {
            var cell = BuildCell(new Vector2(10f, 10f));

            var neighbor = BuildCell(new Vector2(40f, 10f));

            MockGrid.Setup(grid => grid.HasNeighbor(cell, HexDirection.E)).Returns(false);
            MockGrid.Setup(grid => grid.GetNeighbor(cell, HexDirection.E)).Returns(neighbor);

            var position = new Vector3(-1000f, 0f, -1000f);

            MockRenderConfig.Setup(config => config.GetFirstSolidCornerXZ (HexDirection.E)).Returns(new Vector2(10f, 10f));
            MockRenderConfig.Setup(config => config.GetSecondSolidCornerXZ(HexDirection.E)).Returns(new Vector2(10f, -10f));
            MockRenderConfig.Setup(config => config.GetSolidEdgeMidpointXZ(HexDirection.E)).Returns(new Vector2(10f, 0f));

            MockRenderConfig.Setup(config => config.GetFirstSolidCornerXZ (HexDirection.W)).Returns(new Vector2(-10f, -10f));
            MockRenderConfig.Setup(config => config.GetSecondSolidCornerXZ(HexDirection.W)).Returns(new Vector2(-10f, 10f));

            MockGeometry2D.Setup(
                geometry => geometry.IsPointWithinTriangle(
                    new Vector2(-1000f, -1000f), new Vector2(20f, 10f), new Vector2(20f, 20f), new Vector2(30f, 20f)
                )
            ).Returns(true);

            float aCoord = -1f, bCoord = -20f, cCoord = -300f;
            MockGeometry2D.Setup(
                geometry => geometry.GetBarycentric2D(
                    new Vector2(-1000f, -1000f), new Vector2(20f, 10f), new Vector2(20f, 20f), new Vector2(30f, 20f),
                    out aCoord, out bCoord, out cCoord
                )
            );

            var weightLogic = Container.Resolve<MountainHeightmapWeightLogic>();

            float peakWeight, ridgeWeight, hillsWeight;

            weightLogic.GetHeightWeightsForPoint(position, cell, HexDirection.E, out peakWeight, out ridgeWeight, out hillsWeight);

            Assert.AreEqual(0f, peakWeight,  "PeakWeight has an unexpected value");
            Assert.AreEqual(0f, ridgeWeight, "RidgeWeight has an unexpected value");
            Assert.AreEqual(1f, hillsWeight, "HillsWeight has an unexpected value");
        }

        [Test]
        public void GetHeightWeightsForPosition_AndPointOutsideAllCheckedTriangles_ReturnsOnlyHillWeight() {
            var cell = BuildCell(new Vector2(10f, 10f));

            var neighbor = BuildCell(new Vector2(40f, 10f));

            MockGrid.Setup(grid => grid.HasNeighbor(cell, HexDirection.E)).Returns(true);
            MockGrid.Setup(grid => grid.GetNeighbor(cell, HexDirection.E)).Returns(neighbor);

            var position = new Vector3(-1000f, 0f, -1000f);

            var weightLogic = Container.Resolve<MountainHeightmapWeightLogic>();

            float peakWeight, ridgeWeight, hillsWeight;

            weightLogic.GetHeightWeightsForPoint(position, cell, HexDirection.E, out peakWeight, out ridgeWeight, out hillsWeight);

            Assert.AreEqual(0f, peakWeight,  "PeakWeight has an unexpected value");
            Assert.AreEqual(0f, ridgeWeight, "RidgeWeight has an unexpected value");
            Assert.AreEqual(1f, hillsWeight, "HillsWeight has an unexpected value");
        }

        #endregion

        #region utilities

        private IHexCell BuildCell(Vector2 absolutePositionXZ) {
            var mockCell = new Mock<IHexCell>();

            mockCell.Setup(cell => cell.AbsolutePositionXZ).Returns(absolutePositionXZ);

            return mockCell.Object;
        }

        #endregion

        #endregion

    }

}
