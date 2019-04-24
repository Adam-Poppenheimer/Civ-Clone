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

        private Mock<IMapRenderConfig>         MockRenderConfig;
        private Mock<IPointOrientationLogic>   MockPointOrientationLogic;
        private Mock<ICellAlphamapLogic>       MockCellAlphamapLogic;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockRenderConfig          = new Mock<IMapRenderConfig>();
            MockPointOrientationLogic = new Mock<IPointOrientationLogic>();
            MockCellAlphamapLogic     = new Mock<ICellAlphamapLogic>();

            Container.Bind<IMapRenderConfig>      ().FromInstance(MockRenderConfig         .Object);
            Container.Bind<IPointOrientationLogic>().FromInstance(MockPointOrientationLogic.Object);
            Container.Bind<ICellAlphamapLogic>    ().FromInstance(MockCellAlphamapLogic    .Object);

            Container.Bind<TerrainAlphamapLogic>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void BrokenTests() {
            throw new NotImplementedException();
        }

        /*[Test]
        public void GetAlphamapForPoint_PointNotOnGrid_ReturnsConfigueredOffGridAlphamap() {

            var point = new Vector2(1f, -2f);

            var orientationData = new PointOrientationData() {
                IsOnGrid = false
            };

            MockPointOrientationLogic.Setup(logic => logic.GetOrientationDataForPoint(point)).Returns(orientationData);

            var offGridAlphamap = new float[5] { 1, 2, 3, 4, 5 };
            MockRenderConfig.Setup(config => config.OffGridAlphamap).Returns(offGridAlphamap);

            var alphamapLogic = Container.Resolve<TerrainAlphamapLogic>();

            CollectionAssert.AreEqual(
                offGridAlphamap, alphamapLogic.GetAlphamapForPoint(point)
            );
        }

        [Test]
        public void GetAlphamapForPoint_AndRiverAlphaWeightAboveZero_ReturnsRiverAlphamap() {
            var point = new Vector2(1f, -2f);

            var orientationData = new PointOrientationData() {
                IsOnGrid = true, RiverAlphaWeight = 0.001f
            };

            MockPointOrientationLogic.Setup(logic => logic.GetOrientationDataForPoint(point)).Returns(orientationData);

            var riverAlphamap = new float[6] { 1f, 1f, 1f, 1f, 1f, 1f };

            MockRenderConfig.Setup(config => config.MapTextures  ).Returns(new List<Texture2D>() { null, null, null, null });
            MockRenderConfig.Setup(config => config.RiverAlphamap).Returns(riverAlphamap);

            var alphamapLogic = Container.Resolve<TerrainAlphamapLogic>();

            CollectionAssert.AreEqual(
                riverAlphamap, alphamapLogic.GetAlphamapForPoint(point)
            );
        }

        [Test]
        public void GetAlphamapForPoint_RiverAlphaWeightDrownsAllOtherWeights() {
            var point = new Vector2(1f, -2f);

            var orientationData = new PointOrientationData() {
                IsOnGrid = true, Sextant = HexDirection.E, RiverAlphaWeight = 0.001f,
                Center = BuildCell(), Left = BuildCell(), Right = BuildCell(), NextRight = BuildCell(),
                CenterWeight = 1f, LeftWeight = 1f, RightWeight = 1f, NextRightWeight = 1f
            };

            MockPointOrientationLogic.Setup(logic => logic.GetOrientationDataForPoint(point)).Returns(orientationData);

            var riverAlphamap = new float[6] { 1f, 1f, 1f, 1f, 1f, 1f };

            MockRenderConfig.Setup(config => config.MapTextures  ).Returns(new List<Texture2D>() { null, null, null, null });
            MockRenderConfig.Setup(config => config.RiverAlphamap).Returns(riverAlphamap);

            MockCellAlphamapLogic.Setup(
                logic => logic.GetAlphamapForPointForCell(point, It.IsAny<IHexCell>(), It.IsAny<HexDirection>())
            ).Returns(
                new float[4] { 2f, 2f, 2f, 2f }
            );

            var alphamapLogic = Container.Resolve<TerrainAlphamapLogic>();

            CollectionAssert.AreEqual(
                riverAlphamap, alphamapLogic.GetAlphamapForPoint(point)
            );
        }

        [Test]
        public void GetAlphamapForPoint_AndCenterWeightAboveZero_AppliesWeightToCenterAlphamap() {
            var point = new Vector2(1f, -2f);

            var orientationData = new PointOrientationData() {
                IsOnGrid = true, Sextant = HexDirection.E, Center = BuildCell(), CenterWeight = 3f
            };

            MockPointOrientationLogic.Setup(logic => logic.GetOrientationDataForPoint(point)).Returns(orientationData);

            MockRenderConfig.Setup(config => config.MapTextures).Returns(new List<Texture2D>() { null, null, null, null });

            MockCellAlphamapLogic.Setup(
                logic => logic.GetAlphamapForPointForCell(point, orientationData.Center, HexDirection.E)
            ).Returns(
                new float[4] { 1f, 2f, 3f, 4f }
            );

            var alphamapLogic = Container.Resolve<TerrainAlphamapLogic>();

            CollectionAssert.AreEqual(
                new float[4] { 3f, 6f, 9f, 12f }, alphamapLogic.GetAlphamapForPoint(point)
            );
        }

        [Test]
        public void GetAlphamapForPoint_AndLeftWeightAboveZero_AppliesWeightToLeftAlphamap() {
            var point = new Vector2(1f, -2f);

            var orientationData = new PointOrientationData() {
                IsOnGrid = true, Sextant = HexDirection.E, Left = BuildCell(), LeftWeight = 3f
            };

            MockPointOrientationLogic.Setup(logic => logic.GetOrientationDataForPoint(point)).Returns(orientationData);

            MockRenderConfig.Setup(config => config.MapTextures).Returns(new List<Texture2D>() { null, null, null, null });

            MockCellAlphamapLogic.Setup(
                logic => logic.GetAlphamapForPointForCell(point, orientationData.Left, HexDirection.SW)
            ).Returns(
                new float[4] { 1f, 2f, 3f, 4f }
            );

            var alphamapLogic = Container.Resolve<TerrainAlphamapLogic>();

            CollectionAssert.AreEqual(
                new float[4] { 3f, 6f, 9f, 12f }, alphamapLogic.GetAlphamapForPoint(point)
            );
        }

        [Test]
        public void GetAlphamapForPoint_AndLeftWeightAboveZero_IgnoresLeftWeightIfLeftNull() {
            var point = new Vector2(1f, -2f);

            var orientationData = new PointOrientationData() {
                IsOnGrid = true, Sextant = HexDirection.E, Left = null, LeftWeight = 3f
            };

            MockPointOrientationLogic.Setup(logic => logic.GetOrientationDataForPoint(point)).Returns(orientationData);

            MockRenderConfig.Setup(config => config.MapTextures).Returns(new List<Texture2D>() { null, null, null, null });

            MockCellAlphamapLogic.Setup(
                logic => logic.GetAlphamapForPointForCell(point, orientationData.Left, HexDirection.SW)
            ).Returns(
                new float[4] { 1f, 2f, 3f, 4f }
            );

            var alphamapLogic = Container.Resolve<TerrainAlphamapLogic>();

            CollectionAssert.AreEqual(
                new float[4] { 0f, 0f, 0f, 0f }, alphamapLogic.GetAlphamapForPoint(point)
            );
        }

        [Test]
        public void GetAlphamapForPoint_AndRightWeightAboveZero_AppliesWeightToRightAlphamap() {
            var point = new Vector2(1f, -2f);

            var orientationData = new PointOrientationData() {
                IsOnGrid = true, Sextant = HexDirection.E, Right = BuildCell(), RightWeight = 3f
            };

            MockPointOrientationLogic.Setup(logic => logic.GetOrientationDataForPoint(point)).Returns(orientationData);

            MockRenderConfig.Setup(config => config.MapTextures).Returns(new List<Texture2D>() { null, null, null, null });

            MockCellAlphamapLogic.Setup(
                logic => logic.GetAlphamapForPointForCell(point, orientationData.Right, HexDirection.W)
            ).Returns(
                new float[4] { 1f, 2f, 3f, 4f }
            );

            var alphamapLogic = Container.Resolve<TerrainAlphamapLogic>();

            CollectionAssert.AreEqual(
                new float[4] { 3f, 6f, 9f, 12f }, alphamapLogic.GetAlphamapForPoint(point)
            );
        }

        [Test]
        public void GetAlphamapForPoint_AndRightWeightAboveZero_IgnoresRightWeightIfRightNull() {
            var point = new Vector2(1f, -2f);

            var orientationData = new PointOrientationData() {
                IsOnGrid = true, Sextant = HexDirection.E, Right = null, RightWeight = 3f
            };

            MockPointOrientationLogic.Setup(logic => logic.GetOrientationDataForPoint(point)).Returns(orientationData);

            MockRenderConfig.Setup(config => config.MapTextures).Returns(new List<Texture2D>() { null, null, null, null });

            MockCellAlphamapLogic.Setup(
                logic => logic.GetAlphamapForPointForCell(point, orientationData.Right, HexDirection.W)
            ).Returns(
                new float[4] { 1f, 2f, 3f, 4f }
            );

            var alphamapLogic = Container.Resolve<TerrainAlphamapLogic>();

            CollectionAssert.AreEqual(
                new float[4] { 0f, 0f, 0f, 0f }, alphamapLogic.GetAlphamapForPoint(point)
            );
        }

        [Test]
        public void GetAlphamapForPoint_AndNextRightWeightAboveZero_AppliesWeightToNextRightAlphamap() {
            var point = new Vector2(1f, -2f);

            var orientationData = new PointOrientationData() {
                IsOnGrid = true, Sextant = HexDirection.E, NextRight = BuildCell(), NextRightWeight = 3f
            };

            MockPointOrientationLogic.Setup(logic => logic.GetOrientationDataForPoint(point)).Returns(orientationData);

            MockRenderConfig.Setup(config => config.MapTextures).Returns(new List<Texture2D>() { null, null, null, null });

            MockCellAlphamapLogic.Setup(
                logic => logic.GetAlphamapForPointForCell(point, orientationData.NextRight, HexDirection.NW)
            ).Returns(
                new float[4] { 1f, 2f, 3f, 4f }
            );

            var alphamapLogic = Container.Resolve<TerrainAlphamapLogic>();

            CollectionAssert.AreEqual(
                new float[4] { 3f, 6f, 9f, 12f }, alphamapLogic.GetAlphamapForPoint(point)
            );
        }

        [Test]
        public void GetAlphamapForPoint_AndNextRightWeightAboveZero_IgnoresNextRightWeightIfNextRightNull() {
            var point = new Vector2(1f, -2f);

            var orientationData = new PointOrientationData() {
                IsOnGrid = true, Sextant = HexDirection.E, NextRight = null, NextRightWeight = 3f
            };

            MockPointOrientationLogic.Setup(logic => logic.GetOrientationDataForPoint(point)).Returns(orientationData);

            MockRenderConfig.Setup(config => config.MapTextures).Returns(new List<Texture2D>() { null, null, null, null });

            MockCellAlphamapLogic.Setup(
                logic => logic.GetAlphamapForPointForCell(point, orientationData.NextRight, HexDirection.NW)
            ).Returns(
                new float[4] { 1f, 2f, 3f, 4f }
            );

            var alphamapLogic = Container.Resolve<TerrainAlphamapLogic>();

            CollectionAssert.AreEqual(
                new float[4] { 0f, 0f, 0f, 0f }, alphamapLogic.GetAlphamapForPoint(point)
            );
        }

        [Test]
        public void GetAlphamapForPoint_AndMultipleWeightsAboveZero_SumsWeightsWithAlphamaps() {
            var point = new Vector2(1f, -2f);

            var orientationData = new PointOrientationData() {
                IsOnGrid = true, Sextant = HexDirection.E,
                Center    = BuildCell(), CenterWeight    = 2f,
                Left      = BuildCell(), LeftWeight      = 3f,
                Right     = BuildCell(), RightWeight     = 4f,
                NextRight = BuildCell(), NextRightWeight = 5f
            };

            MockPointOrientationLogic.Setup(logic => logic.GetOrientationDataForPoint(point)).Returns(orientationData);

            MockRenderConfig.Setup(config => config.MapTextures).Returns(new List<Texture2D>() { null, null, null, null });

            MockCellAlphamapLogic.Setup(
                logic => logic.GetAlphamapForPointForCell(point, orientationData.Center, HexDirection.E)
            ).Returns(
                new float[4] { 1f, 1f, 1f, 1f }
            );

            MockCellAlphamapLogic.Setup(
                logic => logic.GetAlphamapForPointForCell(point, orientationData.Left, HexDirection.SW)
            ).Returns(
                new float[4] { 2f, 2f, 2f, 2f }
            );

            MockCellAlphamapLogic.Setup(
                logic => logic.GetAlphamapForPointForCell(point, orientationData.Right, HexDirection.W)
            ).Returns(
                new float[4] { 3f, 3f, 3f, 3f }
            );

            MockCellAlphamapLogic.Setup(
                logic => logic.GetAlphamapForPointForCell(point, orientationData.NextRight, HexDirection.NW)
            ).Returns(
                new float[4] { 4f, 4f, 4f, 4f }
            );

            var alphamapLogic = Container.Resolve<TerrainAlphamapLogic>();

            CollectionAssert.AreEqual(
                new float[4] { 40f, 40f, 40f, 40f }, alphamapLogic.GetAlphamapForPoint(point)
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
