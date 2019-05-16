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

    public class CellHeightmapLogicTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IMapRenderConfig>         MockRenderConfig;
        private Mock<IMountainHeightmapLogic>  MockMountainHeightmapLogic;
        private Mock<INoiseGenerator>          MockNoiseGenerator;
        private Mock<IHillsHeightmapLogic>     MockHillsHeightmapLogic;
        private Mock<IFlatlandsHeightmapLogic> MockFlatlandsHeightmapLogic;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockRenderConfig            = new Mock<IMapRenderConfig>();
            MockMountainHeightmapLogic  = new Mock<IMountainHeightmapLogic>();
            MockNoiseGenerator          = new Mock<INoiseGenerator>();
            MockHillsHeightmapLogic     = new Mock<IHillsHeightmapLogic>();
            MockFlatlandsHeightmapLogic = new Mock<IFlatlandsHeightmapLogic>();

            Container.Bind<IMapRenderConfig>        ().FromInstance(MockRenderConfig           .Object);
            Container.Bind<IMountainHeightmapLogic> ().FromInstance(MockMountainHeightmapLogic .Object);
            Container.Bind<INoiseGenerator>         ().FromInstance(MockNoiseGenerator         .Object);
            Container.Bind<IHillsHeightmapLogic>    ().FromInstance(MockHillsHeightmapLogic    .Object);
            Container.Bind<IFlatlandsHeightmapLogic>().FromInstance(MockFlatlandsHeightmapLogic.Object);

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

            Assert.AreEqual(15.25f, heightmapLogic.GetHeightForPointForCell(position, cell, 7.27f, null, null));
        }

        [Test]
        public void GetHeightForPositionForCell_AndCellIsFlatlands_ReturnsHeightFromFlatlandsHeightmapLogic() {
            var position = new Vector3(1f, 2f, 3f);

            var cell = BuildCell(CellTerrain.Grassland, CellShape.Flatlands);

            var flatlandsNoise = new AsyncTextureUnsafe<Color32>();

            MockFlatlandsHeightmapLogic.Setup(logic => logic.GetHeightForPoint(position, flatlandsNoise)).Returns(15.25f);

            var heightmapLogic = Container.Resolve<CellHeightmapLogic>();

            Assert.AreEqual(15.25f, heightmapLogic.GetHeightForPointForCell(position, cell, 7.27f, flatlandsNoise, null));
        }

        [Test]
        public void GetHeightForPositionForCell_AndCellHills_ReturnsHeightFromHillsHeightmapLogic() {
            var position = new Vector3(1f, 2f, 3f);

            var cell = BuildCell(CellTerrain.Grassland, CellShape.Hills);

            var flatlandsNoise = new AsyncTextureUnsafe<Color32>();
            var hillsNoise     = new AsyncTextureUnsafe<Color32>();

            MockHillsHeightmapLogic.Setup(logic => logic.GetHeightForPoint(position, 7.27f, flatlandsNoise, hillsNoise)).Returns(15.25f);

            var heightmapLogic = Container.Resolve<CellHeightmapLogic>();

            Assert.AreEqual(15.25f, heightmapLogic.GetHeightForPointForCell(position, cell, 7.27f, flatlandsNoise, hillsNoise));
        }

        [Test]
        public void GetHeightForPositionForCell_AndCellMountains_ReturnsHeightFromMountainHeightmapLogic() {
            var position = new Vector3(1f, 2f, 3f);

            var cell = BuildCell(CellTerrain.Grassland, CellShape.Mountains);

            var flatlandsNoise = new AsyncTextureUnsafe<Color32>();
            var hillsNoise     = new AsyncTextureUnsafe<Color32>();

            MockMountainHeightmapLogic.Setup(logic => logic.GetHeightForPoint(position, cell, 7.27f, flatlandsNoise, hillsNoise))
                                      .Returns(15.25f);

            var heightmapLogic = Container.Resolve<CellHeightmapLogic>();

            Assert.AreEqual(15.25f, heightmapLogic.GetHeightForPointForCell(position, cell, 7.27f, flatlandsNoise, hillsNoise));
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
