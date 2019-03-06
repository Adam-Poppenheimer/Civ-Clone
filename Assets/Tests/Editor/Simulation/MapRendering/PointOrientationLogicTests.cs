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

    public class PointOrientationLogicTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IMapRenderConfig> MockRenderConfig;
        private Mock<IHexGrid>         MockGrid;
        private Mock<IGeometry2D>      MockGeometry2D;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockRenderConfig = new Mock<IMapRenderConfig>();
            MockGrid         = new Mock<IHexGrid>();
            MockGeometry2D   = new Mock<IGeometry2D>();

            Container.Bind<IMapRenderConfig>().FromInstance(MockRenderConfig.Object);
            Container.Bind<IHexGrid>        ().FromInstance(MockGrid        .Object);
            Container.Bind<IGeometry2D>     ().FromInstance(MockGeometry2D  .Object);

            Container.Bind<PointOrientationLogic>().AsSingle();
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
