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
        private Mock<ITerrainMixingLogic>    MockTerrainMixingLogic;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockPointOrientationLogic = new Mock<IPointOrientationLogic>();
            MockCellHeightmapLogic    = new Mock<ICellHeightmapLogic>();
            MockTerrainMixingLogic    = new Mock<ITerrainMixingLogic>();

            Container.Bind<IPointOrientationLogic>().FromInstance(MockPointOrientationLogic.Object);
            Container.Bind<ICellHeightmapLogic>   ().FromInstance(MockCellHeightmapLogic   .Object);
            Container.Bind<ITerrainMixingLogic>   ().FromInstance(MockTerrainMixingLogic   .Object);

            Container.Bind<TerrainHeightLogic>().AsSingle();
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
