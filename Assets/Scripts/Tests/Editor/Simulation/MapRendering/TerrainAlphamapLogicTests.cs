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

namespace Assets.Tests.Simulation.MapRendering {

    public class TerrainAlphamapLogicTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IMapRenderConfig>   MockRenderConfig;
        private Mock<ICellAlphamapLogic> MockCellAlphamapLogic;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockRenderConfig      = new Mock<IMapRenderConfig>();
            MockCellAlphamapLogic = new Mock<ICellAlphamapLogic>();

            Container.Bind<IMapRenderConfig>  ().FromInstance(MockRenderConfig     .Object);
            Container.Bind<ICellAlphamapLogic>().FromInstance(MockCellAlphamapLogic.Object);

            Container.Bind<TerrainAlphamapLogic>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void GetAlphamapFromOrientation_NotOnGrid_ReturnsEmptyAlphamap() {
            var orientationData = new PointOrientationData() {
                IsOnGrid = false
            };

            var returnMap       = new float[5];
            var intermediateMap = new float[5];

            var alphamapLogic = Container.Resolve<TerrainAlphamapLogic>();

            alphamapLogic.GetAlphamapFromOrientation(returnMap, intermediateMap, orientationData);

            CollectionAssert.AreEqual(new float[5], returnMap);
        }

        [Test]
        public void GetAlphamapFromOrientation_RiverWeightAboveZero_AndFloodPlains_AppliesAlphamapWithWeight() {
            var orientationData = new PointOrientationData() {
                IsOnGrid    = true,
                Center      = BuildCell(CellTerrain.FloodPlains),
                RiverWeight = 0.5f
            };

            var returnMap       = new float[5];
            var intermediateMap = new float[5];

            var alphamap = new float[] { 1f, 2f, 3f, 4f, 5f };

            MockRenderConfig.Setup(config => config.FloodPlainsAlphamap).Returns(alphamap);

            var alphamapLogic = Container.Resolve<TerrainAlphamapLogic>();

            alphamapLogic.GetAlphamapFromOrientation(returnMap, intermediateMap, orientationData);

            CollectionAssert.AreEqual(
                new float[] { 0.5f, 1f, 3f * 0.5f, 2f, 5f * 0.5f }, returnMap
            );
        }

        [Test]
        public void GetAlphamapFromOrientation_RiverWeightAboveZero_AndNotFloodPlains_AppliesAlphamapWithWeight() {
            var orientationData = new PointOrientationData() {
                IsOnGrid    = true,
                Center      = BuildCell(CellTerrain.Grassland),
                RiverWeight = 0.5f
            };

            var returnMap       = new float[5];
            var intermediateMap = new float[5];

            var alphamap = new float[] { 1f, 2f, 3f, 4f, 5f };

            MockRenderConfig.Setup(config => config.RiverAlphamap).Returns(alphamap);

            var alphamapLogic = Container.Resolve<TerrainAlphamapLogic>();

            alphamapLogic.GetAlphamapFromOrientation(returnMap, intermediateMap, orientationData);

            CollectionAssert.AreEqual(
                new float[] { 0.5f, 1f, 3f * 0.5f, 2f, 5f * 0.5f }, returnMap
            );
        }

        [Test]
        public void GetAlphamapFromOrientation_AndCenterWeightAboveZero_AppliesWeightToCenterAlphamap() {
            var orientationData = new PointOrientationData() {
                IsOnGrid     = true,
                Center       = BuildCell(CellTerrain.Grassland),
                CenterWeight = 3f
            };

            var returnMap       = new float[5];
            var intermediateMap = new float[5];

            MockCellAlphamapLogic.Setup(
                logic => logic.GetAlphamapForCell(intermediateMap, orientationData.Center)
            ).Callback<float[], IHexCell>((array, cell) => {
                array[0] = 1f;
                array[1] = 2f;
                array[2] = 3f;
                array[3] = 4f;
                array[4] = 5f;
            });

            var alphamapLogic = Container.Resolve<TerrainAlphamapLogic>();

            alphamapLogic.GetAlphamapFromOrientation(returnMap, intermediateMap, orientationData);

            CollectionAssert.AreEqual(
                new float[] { 3f, 6f, 9f, 12f, 15f }, returnMap
            );
        }

        [Test]
        public void GetAlphamapFromOrientation_AndLeftWeightAboveZero_AppliesWeightToLeftAlphamap() {
            var orientationData = new PointOrientationData() {
                IsOnGrid = true, Left = BuildCell(CellTerrain.Grassland), LeftWeight = 3f
            };

            var returnMap       = new float[5];
            var intermediateMap = new float[5];

            MockCellAlphamapLogic.Setup(
                logic => logic.GetAlphamapForCell(intermediateMap, orientationData.Left)
            ).Callback<float[], IHexCell>((array, cell) => {
                array[0] = 1f;
                array[1] = 2f;
                array[2] = 3f;
                array[3] = 4f;
                array[4] = 5f;
            });

            var alphamapLogic = Container.Resolve<TerrainAlphamapLogic>();

            alphamapLogic.GetAlphamapFromOrientation(returnMap, intermediateMap, orientationData);

            CollectionAssert.AreEqual(
                new float[] { 3f, 6f, 9f, 12f, 15f }, returnMap
            );
        }

        [Test]
        public void GetAlphamapFromOrientation_AndLeftWeightAboveZero_IgnoresLeftWeightIfLeftNull() {
            var orientationData = new PointOrientationData() {
                IsOnGrid = true, Left = null, LeftWeight = 3f
            };

            var returnMap       = new float[5];
            var intermediateMap = new float[5];

            MockCellAlphamapLogic.Setup(
                logic => logic.GetAlphamapForCell(intermediateMap, orientationData.Left)
            ).Callback<float[], IHexCell>((array, cell) => {
                array[0] = 1f;
                array[1] = 2f;
                array[2] = 3f;
                array[3] = 4f;
                array[4] = 5f;
            });

            var alphamapLogic = Container.Resolve<TerrainAlphamapLogic>();

            alphamapLogic.GetAlphamapFromOrientation(returnMap, intermediateMap, orientationData);

            CollectionAssert.AreEqual(
                new float[] { 0f, 0f, 0f, 0f, 0f }, returnMap
            );
        }

        [Test]
        public void GetAlphamapFromOrientation_AndRightWeightAboveZero_AppliesWeightToRightAlphamap() {
            var point = new Vector2(1f, -2f);

            var orientationData = new PointOrientationData() {
                IsOnGrid = true, Right = BuildCell(CellTerrain.Grassland), RightWeight = 3f
            };

            var returnMap       = new float[5];
            var intermediateMap = new float[5];

            MockCellAlphamapLogic.Setup(
                logic => logic.GetAlphamapForCell(intermediateMap, orientationData.Right)
            ).Callback<float[], IHexCell>((array, cell) => {
                array[0] = 1f;
                array[1] = 2f;
                array[2] = 3f;
                array[3] = 4f;
                array[4] = 5f;
            });

            var alphamapLogic = Container.Resolve<TerrainAlphamapLogic>();

            alphamapLogic.GetAlphamapFromOrientation(returnMap, intermediateMap, orientationData);

            CollectionAssert.AreEqual(
                new float[] { 3f, 6f, 9f, 12f, 15f }, returnMap
            );
        }

        [Test]
        public void GetAlphamapFromOrientation_AndRightWeightAboveZero_IgnoresRightWeightIfRightNull() {
            var orientationData = new PointOrientationData() {
                IsOnGrid = true, Right = null, RightWeight = 3f
            };

            var returnMap       = new float[5];
            var intermediateMap = new float[5];

            MockCellAlphamapLogic.Setup(
                logic => logic.GetAlphamapForCell(intermediateMap, orientationData.Right)
            ).Callback<float[], IHexCell>((array, cell) => {
                array[0] = 1f;
                array[1] = 2f;
                array[2] = 3f;
                array[3] = 4f;
                array[4] = 5f;
            });

            var alphamapLogic = Container.Resolve<TerrainAlphamapLogic>();

            alphamapLogic.GetAlphamapFromOrientation(returnMap, intermediateMap, orientationData);

            CollectionAssert.AreEqual(
                new float[] { 0f, 0f, 0f, 0f, 0f }, returnMap
            );
        }

        [Test]
        public void GetAlphamapFromOrientation_AndNextRightWeightAboveZero_AppliesWeightToNextRightAlphamap() {
            var orientationData = new PointOrientationData() {
                IsOnGrid = true, NextRight = BuildCell(CellTerrain.Grassland), NextRightWeight = 3f
            };

            var returnMap       = new float[5];
            var intermediateMap = new float[5];

            MockCellAlphamapLogic.Setup(
                logic => logic.GetAlphamapForCell(intermediateMap, orientationData.NextRight)
            ).Callback<float[], IHexCell>((array, cell) => {
                array[0] = 1f;
                array[1] = 2f;
                array[2] = 3f;
                array[3] = 4f;
                array[4] = 5f;
            });

            var alphamapLogic = Container.Resolve<TerrainAlphamapLogic>();

            alphamapLogic.GetAlphamapFromOrientation(returnMap, intermediateMap, orientationData);

            CollectionAssert.AreEqual(
                new float[] { 3f, 6f, 9f, 12f, 15f }, returnMap
            );
        }

        [Test]
        public void GetAlphamapFromOrientation_AndNextRightWeightAboveZero_IgnoresNextRightWeightIfNextRightNull() {
            var orientationData = new PointOrientationData() {
                IsOnGrid = true, NextRight = null, NextRightWeight = 3f
            };

            var returnMap       = new float[5];
            var intermediateMap = new float[5];

            MockCellAlphamapLogic.Setup(
                logic => logic.GetAlphamapForCell(intermediateMap, orientationData.NextRight)
            ).Callback<float[], IHexCell>((array, cell) => {
                array[0] = 1f;
                array[1] = 2f;
                array[2] = 3f;
                array[3] = 4f;
                array[4] = 5f;
            });

            var alphamapLogic = Container.Resolve<TerrainAlphamapLogic>();

            alphamapLogic.GetAlphamapFromOrientation(returnMap, intermediateMap, orientationData);

            CollectionAssert.AreEqual(
                new float[] { 0f, 0f, 0f, 0f, 0f }, returnMap
            );
        }

        [Test]
        public void GetAlphamapFromOrientation_AndMultipleWeightsAboveZero_SumsWeightsWithAlphamaps() {
            var orientationData = new PointOrientationData() {
                IsOnGrid = true,
                Center    = BuildCell(CellTerrain.Grassland), CenterWeight    = 2f,
                Left      = BuildCell(CellTerrain.Grassland), LeftWeight      = 3f,
                Right     = BuildCell(CellTerrain.Grassland), RightWeight     = 4f,
                NextRight = BuildCell(CellTerrain.Grassland), NextRightWeight = 5f
            };

            var returnMap       = new float[5];
            var intermediateMap = new float[5];

            MockCellAlphamapLogic.Setup(
                logic => logic.GetAlphamapForCell(intermediateMap, orientationData.Center)

            ).Callback<float[], IHexCell>((array, cell) => {
                array[0] = array[1] = array[2] = array[3] = 1f;
            });

            MockCellAlphamapLogic.Setup(
                logic => logic.GetAlphamapForCell(intermediateMap, orientationData.Left)
            ).Callback<float[], IHexCell>((array, cell) => {
                array[0] = array[1] = array[2] = array[3] = 2f;
            });

            MockCellAlphamapLogic.Setup(
                logic => logic.GetAlphamapForCell(intermediateMap, orientationData.Right)
            ).Callback<float[], IHexCell>((array, cell) => {
                array[0] = array[1] = array[2] = array[3] = 3f;
            });

            MockCellAlphamapLogic.Setup(
                logic => logic.GetAlphamapForCell(intermediateMap, orientationData.NextRight)
            ).Callback<float[], IHexCell>((array, cell) => {
                array[0] = array[1] = array[2] = array[3] = 4f;
            });

            var alphamapLogic = Container.Resolve<TerrainAlphamapLogic>();

            alphamapLogic.GetAlphamapFromOrientation(returnMap, intermediateMap, orientationData);

            CollectionAssert.AreEqual(new float[] { 40f, 40f, 40f, 40f, 0f }, returnMap);
        }

        #endregion

        #region utilities

        private IHexCell BuildCell(CellTerrain terrain) {
            var mockCell = new Mock<IHexCell>();

            mockCell.Setup(cell => cell.Terrain).Returns(terrain);

            return mockCell.Object;
        }

        #endregion

        #endregion

    }

}
