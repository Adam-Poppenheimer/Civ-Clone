using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using NUnit.Framework;
using Zenject;
using Moq;

using Assets.Simulation.MapRendering;
using Assets.Simulation.HexMap;

namespace Assets.Tests.Simulation.MapRendering {

    public class TerrainHeightLogicTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IPointOrientationLogic> MockPointOrientationLogic;
        private Mock<ICellHeightmapLogic>    MockCellHeightmapLogic;
        private Mock<IMapRenderConfig>       MockMapRenderConfig;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockPointOrientationLogic = new Mock<IPointOrientationLogic>();
            MockCellHeightmapLogic    = new Mock<ICellHeightmapLogic>();
            MockMapRenderConfig       = new Mock<IMapRenderConfig>();

            Container.Bind<IPointOrientationLogic>().FromInstance(MockPointOrientationLogic.Object);
            Container.Bind<ICellHeightmapLogic>   ().FromInstance(MockCellHeightmapLogic   .Object);
            Container.Bind<IMapRenderConfig>      ().FromInstance(MockMapRenderConfig      .Object);

            Container.Bind<TerrainHeightLogic>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void GetTerrainHeightForPoint_AndPointNotOnGrid_ReturnsZero() {
            var point = new Vector2(1f, 2f);

            var orientationData = new PointOrientationData() {
                IsOnGrid = false
            };

            MockPointOrientationLogic.Setup(logic => logic .GetOrientationDataForPoint(point)).Returns(orientationData);

            var heightLogic = Container.Resolve<TerrainHeightLogic>();

            Assert.AreEqual(0f, heightLogic.GetHeightForPoint(point));
        }

        [Test]
        public void GetTerrainHeightForPoint_AndCenterWeightGreaterThanZero_ReturnsCenterWeightTimesCenterHeight() {
            var point = new Vector2(1f, 2f);

            var center = BuildCell();

            var orientationData = new PointOrientationData() {
                IsOnGrid = true, Sextant = HexDirection.E,
                Center = center, CenterWeight = 12f
            };

            MockPointOrientationLogic.Setup(logic => logic .GetOrientationDataForPoint(point)).Returns(orientationData);

            MockCellHeightmapLogic.Setup(logic => logic.GetHeightForPointForCell(point, center, HexDirection.E)).Returns(8f);

            var heightLogic = Container.Resolve<TerrainHeightLogic>();

            Assert.AreEqual(96, heightLogic.GetHeightForPoint(point));
        }

        [Test]
        public void GetTerrainHeightForPoint_AndLeftWeightGreaterThanZero_ReturnsLeftWeightTimesLeftHeight() {
            var point = new Vector2(1f, 2f);

            var left = BuildCell();

            var orientationData = new PointOrientationData() {
                IsOnGrid = true, Sextant = HexDirection.E,
                Left = left, LeftWeight = 12f
            };

            MockPointOrientationLogic.Setup(logic => logic .GetOrientationDataForPoint(point)).Returns(orientationData);

            MockCellHeightmapLogic.Setup(logic => logic.GetHeightForPointForCell(point, left, HexDirection.SW)).Returns(8f);

            var heightLogic = Container.Resolve<TerrainHeightLogic>();

            Assert.AreEqual(96, heightLogic.GetHeightForPoint(point));
        }

        [Test]
        public void GetTerrainHeightForPoint_AndLeftNull_IgnoresLeftContribution() {
            var point = new Vector2(1f, 2f);

            var orientationData = new PointOrientationData() {
                IsOnGrid = true, Sextant = HexDirection.E,
                Left = null, LeftWeight = 12f
            };

            MockPointOrientationLogic.Setup(logic => logic .GetOrientationDataForPoint(point)).Returns(orientationData);

            MockCellHeightmapLogic.Setup(logic => logic.GetHeightForPointForCell(point, It.IsAny<IHexCell>(), HexDirection.SW)).Returns(8f);

            var heightLogic = Container.Resolve<TerrainHeightLogic>();

            Assert.AreEqual(0, heightLogic.GetHeightForPoint(point));
        }

        [Test]
        public void GetTerrainHeightForPoint_AndRightWeightGreaterThanZero_ReturnsRightWeightTimesRightHeight() {
            var point = new Vector2(1f, 2f);

            var right = BuildCell();

            var orientationData = new PointOrientationData() {
                IsOnGrid = true, Sextant = HexDirection.E,
                Right = right, RightWeight = 12f
            };

            MockPointOrientationLogic.Setup(logic => logic .GetOrientationDataForPoint(point)).Returns(orientationData);

            MockCellHeightmapLogic.Setup(logic => logic.GetHeightForPointForCell(point, right, HexDirection.W)).Returns(8f);

            var heightLogic = Container.Resolve<TerrainHeightLogic>();

            Assert.AreEqual(96, heightLogic.GetHeightForPoint(point));
        }

        [Test]
        public void GetTerrainHeightForPoint_AndRightNull_IgnoresRightContribution() {
            var point = new Vector2(1f, 2f);

            var orientationData = new PointOrientationData() {
                IsOnGrid = true, Sextant = HexDirection.E,
                Right = null, RightWeight = 12f
            };

            MockPointOrientationLogic.Setup(logic => logic .GetOrientationDataForPoint(point)).Returns(orientationData);

            MockCellHeightmapLogic.Setup(logic => logic.GetHeightForPointForCell(point, It.IsAny<IHexCell>(), HexDirection.W)).Returns(8f);

            var heightLogic = Container.Resolve<TerrainHeightLogic>();

            Assert.AreEqual(0, heightLogic.GetHeightForPoint(point));
        }

        [Test]
        public void GetTerrainHeightForPoint_AndNextRightWeightGreaterThanZero_ReturnsNextRightWeightTimesNextRightHeight() {
            var point = new Vector2(1f, 2f);

            var nextRight = BuildCell();

            var orientationData = new PointOrientationData() {
                IsOnGrid = true, Sextant = HexDirection.E,
                NextRight = nextRight, NextRightWeight = 12f
            };

            MockPointOrientationLogic.Setup(logic => logic .GetOrientationDataForPoint(point)).Returns(orientationData);

            MockCellHeightmapLogic.Setup(logic => logic.GetHeightForPointForCell(point, nextRight, HexDirection.NW)).Returns(8f);

            var heightLogic = Container.Resolve<TerrainHeightLogic>();

            Assert.AreEqual(96, heightLogic.GetHeightForPoint(point));
        }

        [Test]
        public void GetTerrainHeightForPoint_AndNextRightNull_IgnoresNextRightContribution() {
            var point = new Vector2(1f, 2f);

            var orientationData = new PointOrientationData() {
                IsOnGrid = true, Sextant = HexDirection.E,
                NextRight = null, NextRightWeight = 12f
            };

            MockPointOrientationLogic.Setup(logic => logic .GetOrientationDataForPoint(point)).Returns(orientationData);

            MockCellHeightmapLogic.Setup(logic => logic.GetHeightForPointForCell(point, It.IsAny<IHexCell>(), HexDirection.NW)).Returns(8f);

            var heightLogic = Container.Resolve<TerrainHeightLogic>();

            Assert.AreEqual(0, heightLogic.GetHeightForPoint(point));
        }

        [Test]
        public void GetTerrainHeightForPoint_AndRiverWeightGreaterThanZero_ReturnsRiverWeightTimesTroughElevation() {
            var point = new Vector2(1f, 2f);

            var orientationData = new PointOrientationData() {
                IsOnGrid = true, Sextant = HexDirection.E,
                RiverHeightWeight = 12f
            };

            MockPointOrientationLogic.Setup(logic => logic .GetOrientationDataForPoint(point)).Returns(orientationData);

            MockMapRenderConfig.Setup(config => config.RiverTroughElevation).Returns(8f);

            var heightLogic = Container.Resolve<TerrainHeightLogic>();

            Assert.AreEqual(96, heightLogic.GetHeightForPoint(point));
        }

        [Test]
        public void GetTerrainHeightForPoint_AndMultipleWeightsGreaterThanZero_SumsAllWeightContributions() {
            var point = new Vector2(1f, 2f);

            var center    = BuildCell();
            var left      = BuildCell();
            var right     = BuildCell();
            var nextRight = BuildCell();

            var orientationData = new PointOrientationData() {
                IsOnGrid = true, Sextant = HexDirection.E,
                Center    = center,    CenterWeight    = 1f,
                Left      = left,      LeftWeight      = 5f,
                Right     = right,     RightWeight     = 1.5f,
                NextRight = nextRight, NextRightWeight = 0.6f,
                RiverHeightWeight = 2f
            };

            MockPointOrientationLogic.Setup(logic => logic .GetOrientationDataForPoint(point)).Returns(orientationData);

            MockCellHeightmapLogic.Setup(logic => logic.GetHeightForPointForCell(point, center,    HexDirection.E )).Returns(10f);
            MockCellHeightmapLogic.Setup(logic => logic.GetHeightForPointForCell(point, left,      HexDirection.SW)).Returns(11f);
            MockCellHeightmapLogic.Setup(logic => logic.GetHeightForPointForCell(point, right,     HexDirection.W )).Returns(12f);
            MockCellHeightmapLogic.Setup(logic => logic.GetHeightForPointForCell(point, nextRight, HexDirection.NW)).Returns(13f);

            MockMapRenderConfig.Setup(config => config.RiverTroughElevation).Returns(6.2f);

            var heightLogic = Container.Resolve<TerrainHeightLogic>();

            float expectedValue = 10f + 55f + 1.5f * 12f + 0.6f * 13f + 2f * 6.2f;

            float pointHeight = heightLogic.GetHeightForPoint(point);

            Assert.IsTrue(
                Mathf.Approximately(expectedValue, pointHeight), string.Format(
                    "Resulting height not approximately equal to expected results (Expected {0}, got {1})",
                    expectedValue, pointHeight
                )
            );
        }

        #endregion

        #region utilities

        private IHexCell BuildCell() {
            return new Mock<IHexCell>().Object;
        }

        #endregion

        #endregion

    }

}
