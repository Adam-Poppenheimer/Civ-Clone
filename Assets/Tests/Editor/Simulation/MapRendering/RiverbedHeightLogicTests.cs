using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using Zenject;
using Moq;

using Assets.Simulation.HexMap;
using Assets.Simulation.MapRendering;
using Assets.Util;

namespace Assets.Tests.Simulation.MapRendering {

    public class RiverbedHeightLogicTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IMapRenderConfig>    MockRenderConfig;
        private Mock<ICellHeightmapLogic> MockCellHeightmapLogic;
        private Mock<IGeometry2D>         MockGeometry2D;
        private Mock<IRiverCanon>         MockRiverCanon;
        private Mock<ITerrainMixingLogic> MockTerrainMixingLogic;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockRenderConfig       = new Mock<IMapRenderConfig>();
            MockCellHeightmapLogic = new Mock<ICellHeightmapLogic>();
            MockGeometry2D         = new Mock<IGeometry2D>();
            MockRiverCanon         = new Mock<IRiverCanon>();
            MockTerrainMixingLogic = new Mock<ITerrainMixingLogic>();

            Container.Bind<IMapRenderConfig>   ().FromInstance(MockRenderConfig      .Object);
            Container.Bind<ICellHeightmapLogic>().FromInstance(MockCellHeightmapLogic.Object);
            Container.Bind<IGeometry2D>        ().FromInstance(MockGeometry2D        .Object);
            Container.Bind<IRiverCanon>        ().FromInstance(MockRiverCanon        .Object);
            Container.Bind<ITerrainMixingLogic>().FromInstance(MockTerrainMixingLogic.Object);

            Container.Bind<RiverbedHeightLogic>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void MissingTests() {
            throw new NotImplementedException();
        }

        #endregion

        #region utilities



        #endregion

        #endregion

    }

}
