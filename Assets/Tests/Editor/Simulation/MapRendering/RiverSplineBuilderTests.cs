using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using Zenject;
using Moq;

using Assets.Simulation.HexMap;
using Assets.Simulation.MapRendering;

namespace Assets.Tests.Simulation.MapRendering {

    public class RiverSplineBuilderTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IMapRenderConfig>    MockRenderConfig;
        private Mock<IRiverAssemblyCanon> MockRiverAssemblyCanon;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockRenderConfig       = new Mock<IMapRenderConfig>();
            MockRiverAssemblyCanon = new Mock<IRiverAssemblyCanon>();

            Container.Bind<IMapRenderConfig>   ().FromInstance(MockRenderConfig      .Object);
            Container.Bind<IRiverAssemblyCanon>().FromInstance(MockRiverAssemblyCanon.Object);

            Container.Bind<RiverSplineBuilder>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void MissingTests() {
            Assert.Ignore("It's not currently clear how to productively unit test this class");
        }

        #endregion

        #region utilities



        #endregion

        #endregion

    }

}
