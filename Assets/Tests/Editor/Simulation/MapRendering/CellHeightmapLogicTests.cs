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

    public class CellHeightmapLogicTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IMapRenderConfig>        MockRenderConfig;
        private Mock<INoiseGenerator>         MockNoiseGenerator;
        private Mock<IMountainHeightmapLogic> MockMountainHeightmapLogic;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockRenderConfig           = new Mock<IMapRenderConfig>();
            MockNoiseGenerator         = new Mock<INoiseGenerator>();
            MockMountainHeightmapLogic = new Mock<IMountainHeightmapLogic>();

            Container.Bind<IMapRenderConfig>       ().FromInstance(MockRenderConfig          .Object);
            Container.Bind<INoiseGenerator>        ().FromInstance(MockNoiseGenerator        .Object);
            Container.Bind<IMountainHeightmapLogic>().FromInstance(MockMountainHeightmapLogic.Object);

            Container.Bind<CellHeightmapLogic>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void GetHeightForPositionForCell_AndCellIsWater_ReturnsSeaFloorElevation() {
            var position = new Vector3(1f, 2f, 3f);

            var cell = BuildCell(CellTerrain.FreshWater, CellShape.Mountains);

            MockRenderConfig.Setup(config => config.SeaFloorElevation).Returns(15.25f);

            var heightmapLogic = Container.Resolve<CellHeightmapLogic>();

            Assert.AreEqual(15.25f, heightmapLogic.GetHeightForPointForCell(position, cell, HexDirection.E));
        }

        [Test]
        public void GetHeightForPositionForCell_AndCellFlatlands_ReturnsNoiseSampledFromFlatlandsHeightmap() {
            var position = new Vector3(1f, 2f, 3f);

            var cell = BuildCell(CellTerrain.Grassland, CellShape.Flatlands);

            MockNoiseGenerator.Setup(generator => generator.SampleNoise(position, NoiseType.FlatlandsHeight))
                              .Returns(new Vector4(15.25f, 0f, 0f, 0f));

            var heightmapLogic = Container.Resolve<CellHeightmapLogic>();

            Assert.AreEqual(15.25f, heightmapLogic.GetHeightForPointForCell(position, cell, HexDirection.E));
        }

        [Test]
        public void GetHeightForPositionForCell_AndCellHills_ReturnsNoiseSampledFromHillsHeightmap() {
            var position = new Vector3(1f, 2f, 3f);

            var cell = BuildCell(CellTerrain.Grassland, CellShape.Hills);

            MockNoiseGenerator.Setup(generator => generator.SampleNoise(position, NoiseType.HillsHeight))
                              .Returns(new Vector4(15.25f, 0f, 0f, 0f));

            var heightmapLogic = Container.Resolve<CellHeightmapLogic>();

            Assert.AreEqual(15.25f, heightmapLogic.GetHeightForPointForCell(position, cell, HexDirection.E));
        }

        [Test]
        public void GetHeightForPositionForCell_AndCellMountains_ReturnsHeightFromMountainHeightmapLogic() {
            var position = new Vector3(1f, 2f, 3f);

            var cell = BuildCell(CellTerrain.Grassland, CellShape.Mountains);

            MockMountainHeightmapLogic.Setup(logic => logic.GetHeightForPoint(position, cell, HexDirection.E))
                                      .Returns(15.25f);

            var heightmapLogic = Container.Resolve<CellHeightmapLogic>();

            Assert.AreEqual(15.25f, heightmapLogic.GetHeightForPointForCell(position, cell, HexDirection.E));
        }

        #endregion

        #region utilities

        private IHexCell BuildCell(CellTerrain terrain, CellShape shape) {
            var mockCell = new Mock<IHexCell>();

            mockCell.Setup(cell => cell.Terrain).Returns(terrain);
            mockCell.Setup(cell => cell.Shape)  .Returns(shape);

            return mockCell.Object;
        }

        #endregion

        #endregion

    }

}
