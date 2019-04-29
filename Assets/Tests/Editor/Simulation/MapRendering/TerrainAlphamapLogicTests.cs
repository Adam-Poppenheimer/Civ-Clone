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

            MockRenderConfig.Setup(config => config.MapTextures).Returns(new Texture2D[5]);

            var alphamapLogic = Container.Resolve<TerrainAlphamapLogic>();

            CollectionAssert.AreEqual(
                new float[5], alphamapLogic.GetAlphamapFromOrientation(orientationData)
            );
        }

        [Test]
        public void GetAlphamapFromOrientation_RiverWeightAboveZero_AndFloodPlains_AppliesAlphamapWithWeight() {
            var orientationData = new PointOrientationData() {
                IsOnGrid = true,
                Center = BuildCell(CellTerrain.FloodPlains),
                RiverWeight = 0.5f
            };

            var alphamap = new float[6] { 1f, 2f, 3f, 4f, 5f, 6f };

            MockRenderConfig.Setup(config => config.MapTextures)        .Returns(new Texture2D[6]);
            MockRenderConfig.Setup(config => config.FloodPlainsAlphamap).Returns(alphamap);

            var alphamapLogic = Container.Resolve<TerrainAlphamapLogic>();

            CollectionAssert.AreEqual(
                new float[] { 0.5f, 1f, 3f * 0.5f, 2f, 5f * 0.5f, 3f },
                alphamapLogic.GetAlphamapFromOrientation(orientationData)
            );
        }

        [Test]
        public void GetAlphamapFromOrientation_RiverWeightAboveZero_AndNotFloodPlains_AppliesAlphamapWithWeight() {
            var orientationData = new PointOrientationData() {
                IsOnGrid = true,
                Center = BuildCell(CellTerrain.Grassland),
                RiverWeight = 0.5f
            };

            var alphamap = new float[6] { 1f, 2f, 3f, 4f, 5f, 6f };

            MockRenderConfig.Setup(config => config.MapTextures)  .Returns(new Texture2D[6]);
            MockRenderConfig.Setup(config => config.RiverAlphamap).Returns(alphamap);

            var alphamapLogic = Container.Resolve<TerrainAlphamapLogic>();

            CollectionAssert.AreEqual(
                new float[] { 0.5f, 1f, 3f * 0.5f, 2f, 5f * 0.5f, 3f },
                alphamapLogic.GetAlphamapFromOrientation(orientationData)
            );
        }

        [Test]
        public void GetAlphamapFromOrientation_AndCenterWeightAboveZero_AppliesWeightToCenterAlphamap() {
            var orientationData = new PointOrientationData() {
                IsOnGrid = true, Sextant = HexDirection.E,
                Center = BuildCell(CellTerrain.Grassland), CenterWeight = 3f
            };

            MockRenderConfig.Setup(config => config.MapTextures).Returns(new Texture2D[4]);

            MockCellAlphamapLogic.Setup(
                logic => logic.GetAlphamapForCell(orientationData.Center, HexDirection.E)
            ).Returns(
                new float[4] { 1f, 2f, 3f, 4f }
            );

            var alphamapLogic = Container.Resolve<TerrainAlphamapLogic>();

            CollectionAssert.AreEqual(
                new float[4] { 3f, 6f, 9f, 12f }, alphamapLogic.GetAlphamapFromOrientation(orientationData)
            );
        }

        [Test]
        public void GetAlphamapFromOrientation_AndLeftWeightAboveZero_AppliesWeightToLeftAlphamap() {
            var orientationData = new PointOrientationData() {
                IsOnGrid = true, Sextant = HexDirection.E,
                Left = BuildCell(CellTerrain.Grassland), LeftWeight = 3f
            };

            MockRenderConfig.Setup(config => config.MapTextures).Returns(new Texture2D[4]);

            MockCellAlphamapLogic.Setup(
                logic => logic.GetAlphamapForCell(orientationData.Left, HexDirection.SW)
            ).Returns(
                new float[4] { 1f, 2f, 3f, 4f }
            );

            var alphamapLogic = Container.Resolve<TerrainAlphamapLogic>();

            CollectionAssert.AreEqual(
                new float[4] { 3f, 6f, 9f, 12f }, alphamapLogic.GetAlphamapFromOrientation(orientationData)
            );
        }

        [Test]
        public void GetAlphamapFromOrientation_AndLeftWeightAboveZero_IgnoresLeftWeightIfLeftNull() {
            var orientationData = new PointOrientationData() {
                IsOnGrid = true, Sextant = HexDirection.E, Left = null, LeftWeight = 3f
            };

            MockRenderConfig.Setup(config => config.MapTextures).Returns(new Texture2D[4]);

            MockCellAlphamapLogic.Setup(
                logic => logic.GetAlphamapForCell(orientationData.Left, HexDirection.SW)
            ).Returns(
                new float[4] { 1f, 2f, 3f, 4f }
            );

            var alphamapLogic = Container.Resolve<TerrainAlphamapLogic>();

            CollectionAssert.AreEqual(
                new float[4] { 0f, 0f, 0f, 0f }, alphamapLogic.GetAlphamapFromOrientation(orientationData)
            );
        }

        [Test]
        public void GetAlphamapFromOrientation_AndRightWeightAboveZero_AppliesWeightToRightAlphamap() {
            var point = new Vector2(1f, -2f);

            var orientationData = new PointOrientationData() {
                IsOnGrid = true, Sextant = HexDirection.E,
                Right = BuildCell(CellTerrain.Grassland), RightWeight = 3f
            };

            MockRenderConfig.Setup(config => config.MapTextures).Returns(new Texture2D[4]);

            MockCellAlphamapLogic.Setup(
                logic => logic.GetAlphamapForCell(orientationData.Right, HexDirection.W)
            ).Returns(
                new float[4] { 1f, 2f, 3f, 4f }
            );

            var alphamapLogic = Container.Resolve<TerrainAlphamapLogic>();

            CollectionAssert.AreEqual(
                new float[4] { 3f, 6f, 9f, 12f }, alphamapLogic.GetAlphamapFromOrientation(orientationData)
            );
        }

        [Test]
        public void GetAlphamapFromOrientation_AndRightWeightAboveZero_IgnoresRightWeightIfRightNull() {
            var orientationData = new PointOrientationData() {
                IsOnGrid = true, Sextant = HexDirection.E, Right = null, RightWeight = 3f
            };

            MockRenderConfig.Setup(config => config.MapTextures).Returns(new Texture2D[4]);

            MockCellAlphamapLogic.Setup(
                logic => logic.GetAlphamapForCell(orientationData.Right, HexDirection.W)
            ).Returns(
                new float[4] { 1f, 2f, 3f, 4f }
            );

            var alphamapLogic = Container.Resolve<TerrainAlphamapLogic>();

            CollectionAssert.AreEqual(
                new float[4] { 0f, 0f, 0f, 0f }, alphamapLogic.GetAlphamapFromOrientation(orientationData)
            );
        }

        [Test]
        public void GetAlphamapFromOrientation_AndNextRightWeightAboveZero_AppliesWeightToNextRightAlphamap() {
            var orientationData = new PointOrientationData() {
                IsOnGrid = true, Sextant = HexDirection.E,
                NextRight = BuildCell(CellTerrain.Grassland), NextRightWeight = 3f
            };
            MockRenderConfig.Setup(config => config.MapTextures).Returns(new Texture2D[4]);

            MockCellAlphamapLogic.Setup(
                logic => logic.GetAlphamapForCell(orientationData.NextRight, HexDirection.NW)
            ).Returns(
                new float[4] { 1f, 2f, 3f, 4f }
            );

            var alphamapLogic = Container.Resolve<TerrainAlphamapLogic>();

            CollectionAssert.AreEqual(
                new float[4] { 3f, 6f, 9f, 12f }, alphamapLogic.GetAlphamapFromOrientation(orientationData)
            );
        }

        [Test]
        public void GetAlphamapFromOrientation_AndNextRightWeightAboveZero_IgnoresNextRightWeightIfNextRightNull() {
            var orientationData = new PointOrientationData() {
                IsOnGrid = true, Sextant = HexDirection.E, NextRight = null, NextRightWeight = 3f
            };

            MockRenderConfig.Setup(config => config.MapTextures).Returns(new Texture2D[4]);

            MockCellAlphamapLogic.Setup(
                logic => logic.GetAlphamapForCell(orientationData.NextRight, HexDirection.NW)
            ).Returns(
                new float[4] { 1f, 2f, 3f, 4f }
            );

            var alphamapLogic = Container.Resolve<TerrainAlphamapLogic>();

            CollectionAssert.AreEqual(
                new float[4] { 0f, 0f, 0f, 0f }, alphamapLogic.GetAlphamapFromOrientation(orientationData)
            );
        }

        [Test]
        public void GetAlphamapFromOrientation_AndMultipleWeightsAboveZero_SumsWeightsWithAlphamaps() {
            var orientationData = new PointOrientationData() {
                IsOnGrid = true, Sextant = HexDirection.E,
                Center    = BuildCell(CellTerrain.Grassland), CenterWeight    = 2f,
                Left      = BuildCell(CellTerrain.Grassland), LeftWeight      = 3f,
                Right     = BuildCell(CellTerrain.Grassland), RightWeight     = 4f,
                NextRight = BuildCell(CellTerrain.Grassland), NextRightWeight = 5f
            };

            MockRenderConfig.Setup(config => config.MapTextures).Returns(new Texture2D[4]);

            MockCellAlphamapLogic.Setup(
                logic => logic.GetAlphamapForCell(orientationData.Center, HexDirection.E)
            ).Returns(
                new float[4] { 1f, 1f, 1f, 1f }
            );

            MockCellAlphamapLogic.Setup(
                logic => logic.GetAlphamapForCell(orientationData.Left, HexDirection.SW)
            ).Returns(
                new float[4] { 2f, 2f, 2f, 2f }
            );

            MockCellAlphamapLogic.Setup(
                logic => logic.GetAlphamapForCell(orientationData.Right, HexDirection.W)
            ).Returns(
                new float[4] { 3f, 3f, 3f, 3f }
            );

            MockCellAlphamapLogic.Setup(
                logic => logic.GetAlphamapForCell(orientationData.NextRight, HexDirection.NW)
            ).Returns(
                new float[4] { 4f, 4f, 4f, 4f }
            );

            var alphamapLogic = Container.Resolve<TerrainAlphamapLogic>();

            CollectionAssert.AreEqual(
                new float[4] { 40f, 40f, 40f, 40f }, alphamapLogic.GetAlphamapFromOrientation(orientationData)
            );
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
