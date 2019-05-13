﻿using System;
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

        private Mock<ICellHeightmapLogic> MockCellHeightmapLogic;
        private Mock<IMapRenderConfig>    MockMapRenderConfig;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockCellHeightmapLogic = new Mock<ICellHeightmapLogic>();
            MockMapRenderConfig    = new Mock<IMapRenderConfig>();

            Container.Bind<ICellHeightmapLogic>().FromInstance(MockCellHeightmapLogic.Object);
            Container.Bind<IMapRenderConfig>   ().FromInstance(MockMapRenderConfig   .Object);

            Container.Bind<TerrainHeightLogic>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void BrokenTests() {
            throw new NotImplementedException();
        }

        /*[Test]
        public void GetTerrainHeightForPoint_AndPointNotOnGrid_ReturnsZero() {
            var point = new Vector2(1f, 2f);

            var orientationData = new PointOrientationData() {
                IsOnGrid = false
            };

            var heightLogic = Container.Resolve<TerrainHeightLogic>();

            Assert.AreEqual(0f, heightLogic.GetHeightForPoint(point, orientationData));
        }

        [Test]
        public void GetTerrainHeightForPoint_AndCenterWeightGreaterThanZero_ReturnsCenterWeightTimesCenterHeight() {
            var point = new Vector2(1f, 2f);

            var center = BuildCell();

            var orientationData = new PointOrientationData() {
                IsOnGrid = true, Sextant = HexDirection.E,
                Center = center, CenterWeight = 12f,
                ElevationDuck = 5.5f
            };

            MockCellHeightmapLogic.Setup(logic => logic.GetHeightForPointForCell(point, center, HexDirection.E, 5.5f)).Returns(8f);

            var heightLogic = Container.Resolve<TerrainHeightLogic>();

            Assert.AreEqual(96, heightLogic.GetHeightForPoint(point, orientationData));
        }

        [Test]
        public void GetTerrainHeightForPoint_AndLeftWeightGreaterThanZero_ReturnsLeftWeightTimesLeftHeight() {
            var point = new Vector2(1f, 2f);

            var left = BuildCell();

            var orientationData = new PointOrientationData() {
                IsOnGrid = true, Sextant = HexDirection.E,
                Left = left, LeftWeight = 12f,
                ElevationDuck = 5.5f
            };

            MockCellHeightmapLogic.Setup(logic => logic.GetHeightForPointForCell(point, left, HexDirection.SW, 5.5f)).Returns(8f);

            var heightLogic = Container.Resolve<TerrainHeightLogic>();

            Assert.AreEqual(96, heightLogic.GetHeightForPoint(point, orientationData));
        }

        [Test]
        public void GetTerrainHeightForPoint_AndLeftNull_IgnoresLeftContribution() {
            var point = new Vector2(1f, 2f);

            var orientationData = new PointOrientationData() {
                IsOnGrid = true, Sextant = HexDirection.E,
                Left = null, LeftWeight = 12f,
                ElevationDuck = 5.5f
            };

            MockCellHeightmapLogic.Setup(logic => logic.GetHeightForPointForCell(point, It.IsAny<IHexCell>(), HexDirection.SW, 5.5f)).Returns(8f);

            var heightLogic = Container.Resolve<TerrainHeightLogic>();

            Assert.AreEqual(0, heightLogic.GetHeightForPoint(point, orientationData));
        }

        [Test]
        public void GetTerrainHeightForPoint_AndRightWeightGreaterThanZero_ReturnsRightWeightTimesRightHeight() {
            var point = new Vector2(1f, 2f);

            var right = BuildCell();

            var orientationData = new PointOrientationData() {
                IsOnGrid = true, Sextant = HexDirection.E,
                Right = right, RightWeight = 12f,
                ElevationDuck = 5.5f
            };

            MockCellHeightmapLogic.Setup(logic => logic.GetHeightForPointForCell(point, right, HexDirection.W, 5.5f)).Returns(8f);

            var heightLogic = Container.Resolve<TerrainHeightLogic>();

            Assert.AreEqual(96, heightLogic.GetHeightForPoint(point, orientationData));
        }

        [Test]
        public void GetTerrainHeightForPoint_AndRightNull_IgnoresRightContribution() {
            var point = new Vector2(1f, 2f);

            var orientationData = new PointOrientationData() {
                IsOnGrid = true, Sextant = HexDirection.E,
                Right = null, RightWeight = 12f,
                ElevationDuck = 5.5f
            };

            MockCellHeightmapLogic.Setup(logic => logic.GetHeightForPointForCell(point, It.IsAny<IHexCell>(), HexDirection.W, 5.5f)).Returns(8f);

            var heightLogic = Container.Resolve<TerrainHeightLogic>();

            Assert.AreEqual(0, heightLogic.GetHeightForPoint(point, orientationData));
        }

        [Test]
        public void GetTerrainHeightForPoint_AndNextRightWeightGreaterThanZero_ReturnsNextRightWeightTimesNextRightHeight() {
            var point = new Vector2(1f, 2f);

            var nextRight = BuildCell();

            var orientationData = new PointOrientationData() {
                IsOnGrid = true, Sextant = HexDirection.E,
                NextRight = nextRight, NextRightWeight = 12f,
                ElevationDuck = 5.5f
            };

            MockCellHeightmapLogic.Setup(logic => logic.GetHeightForPointForCell(point, nextRight, HexDirection.NW, 5.5f)).Returns(8f);

            var heightLogic = Container.Resolve<TerrainHeightLogic>();

            Assert.AreEqual(96, heightLogic.GetHeightForPoint(point, orientationData));
        }

        [Test]
        public void GetTerrainHeightForPoint_AndNextRightNull_IgnoresNextRightContribution() {
            var point = new Vector2(1f, 2f);

            var orientationData = new PointOrientationData() {
                IsOnGrid = true, Sextant = HexDirection.E,
                NextRight = null, NextRightWeight = 12f,
                ElevationDuck = 5.5f
            };

            MockCellHeightmapLogic.Setup(logic => logic.GetHeightForPointForCell(point, It.IsAny<IHexCell>(), HexDirection.NW, 5.5f)).Returns(8f);

            var heightLogic = Container.Resolve<TerrainHeightLogic>();

            Assert.AreEqual(0, heightLogic.GetHeightForPoint(point, orientationData));
        }

        [Test]
        public void GetTerrainHeightForPoint_AndRiverWeightGreaterThanZero_ReturnsRiverWeightTimesTroughElevation() {
            var point = new Vector2(1f, 2f);

            var orientationData = new PointOrientationData() {
                IsOnGrid = true, Sextant = HexDirection.E,
                RiverWeight = 12f
            };

            MockMapRenderConfig.Setup(config => config.RiverTroughElevation).Returns(8f);

            var heightLogic = Container.Resolve<TerrainHeightLogic>();

            Assert.AreEqual(96, heightLogic.GetHeightForPoint(point, orientationData));
        }

        [Test]
        public void GetTerrainHeightForPoint_AndMultipleWeightsGreaterThanZero_SumsAllWeightContributions() {
            var point = new Vector2(1f, 2f);

            var center    = BuildCell();
            var left      = BuildCell();
            var right     = BuildCell();
            var nextRight = BuildCell();

            var orientationData = new PointOrientationData() {
                IsOnGrid  = true,      Sextant         = HexDirection.E,
                Center    = center,    CenterWeight    = 1f,
                Left      = left,      LeftWeight      = 5f,
                Right     = right,     RightWeight     = 1.5f,
                NextRight = nextRight, NextRightWeight = 0.6f,
                RiverWeight = 2f,      ElevationDuck   = 5.5f
            };

            MockCellHeightmapLogic.Setup(logic => logic.GetHeightForPointForCell(point, center,    HexDirection.E,  5.5f)).Returns(10f);
            MockCellHeightmapLogic.Setup(logic => logic.GetHeightForPointForCell(point, left,      HexDirection.SW, 5.5f)).Returns(11f);
            MockCellHeightmapLogic.Setup(logic => logic.GetHeightForPointForCell(point, right,     HexDirection.W,  5.5f)).Returns(12f);
            MockCellHeightmapLogic.Setup(logic => logic.GetHeightForPointForCell(point, nextRight, HexDirection.NW, 5.5f)).Returns(13f);

            MockMapRenderConfig.Setup(config => config.RiverTroughElevation).Returns(6.2f);

            var heightLogic = Container.Resolve<TerrainHeightLogic>();

            float expectedValue = 10f + 55f + 1.5f * 12f + 0.6f * 13f + 2f * 6.2f;

            float pointHeight = heightLogic.GetHeightForPoint(point, orientationData);

            Assert.IsTrue(
                Mathf.Approximately(expectedValue, pointHeight), string.Format(
                    "Resulting height not approximately equal to expected results (Expected {0}, got {1})",
                    expectedValue, pointHeight
                )
            );
        }*/

        #endregion

        #region utilities

        private IHexCell BuildCell() {
            return new Mock<IHexCell>().Object;
        }

        #endregion

        #endregion

    }

}
