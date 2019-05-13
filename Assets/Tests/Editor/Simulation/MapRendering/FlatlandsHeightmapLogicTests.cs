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

    public class FlatlandsHeightmapLogicTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IMapRenderConfig> MockRenderConfig;
        private Mock<INoiseGenerator>  MockNoiseGenerator;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockRenderConfig   = new Mock<IMapRenderConfig>();
            MockNoiseGenerator = new Mock<INoiseGenerator>();

            Container.Bind<IMapRenderConfig>().FromInstance(MockRenderConfig  .Object);
            Container.Bind<INoiseGenerator> ().FromInstance(MockNoiseGenerator.Object);
            
            Container.Bind<FlatlandsHeightmapLogic>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void GetHeightForPoint_SamplesFlatlandNoise_AndFlatlandBaseElevation() {
            throw new NotImplementedException();

            /*var xzPoint = new Vector2(1f, 2f);

            var noiseSource = new Mock<INoiseTexture>().Object;

            MockRenderConfig.Setup(config => config.FlatlandsElevationNoiseSource)  .Returns(noiseSource);
            MockRenderConfig.Setup(config => config.FlatlandsElevationNoiseStrength).Returns(-1f);
            MockRenderConfig.Setup(config => config.FlatlandsBaseElevation)         .Returns(100f);

            MockNoiseGenerator.Setup(generator => generator.SampleNoise(xzPoint, noiseSource, -1f, NoiseType.ZeroToOne))
                              .Returns(new Vector4(10f, 20f, 30f, 40f));

            MockRenderConfig.Setup(config => config.FlatlandsBaseElevation).Returns(100f);

            var cell = new Mock<IHexCell>().Object;

            var heightmapLogic = Container.Resolve<FlatlandsHeightmapLogic>();

            Assert.AreEqual(110f, heightmapLogic.GetHeightForPoint(xzPoint, cell, HexDirection.E));*/
        }

        #endregion

        #region utilities

        

        #endregion

        #endregion

    }

}
