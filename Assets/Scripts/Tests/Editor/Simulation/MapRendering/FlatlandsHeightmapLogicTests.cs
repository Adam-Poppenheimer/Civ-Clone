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
            var xzPoint = new Vector2(1f, 2f);

            var noiseTexture = new AsyncTextureUnsafe<Color32>();

            MockRenderConfig.Setup(config => config.FlatlandsElevationNoiseStrength).Returns(-1f);
            MockRenderConfig.Setup(config => config.FlatlandsBaseElevation)         .Returns(100f);

            MockNoiseGenerator.Setup(generator => generator.SampleNoise(xzPoint, noiseTexture, -1f, NoiseType.ZeroToOne))
                              .Returns(new Vector4(10f, 20f, 30f, 40f));

            MockRenderConfig.Setup(config => config.FlatlandsBaseElevation).Returns(100f);

            var cell = new Mock<IHexCell>().Object;

            var heightmapLogic = Container.Resolve<FlatlandsHeightmapLogic>();

            Assert.AreEqual(110f, heightmapLogic.GetHeightForPoint(xzPoint, noiseTexture));
        }

        #endregion

        #region utilities

        

        #endregion

        #endregion

    }

}
