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
        public void MissingTests() {
            throw new NotImplementedException();
        }

        #endregion

        #region utilities

        

        #endregion

        #endregion

    }

}
