using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using NUnit.Framework;
using Zenject;
using Moq;

using Assets.Simulation.MapRendering;

namespace Assets.Tests.Simulation.MapRendering {

    public class NoiseGeneratorTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IMapRenderConfig> MockRenderConfig;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockRenderConfig = new Mock<IMapRenderConfig>();

            Container.Bind<IMapRenderConfig>().FromInstance(MockRenderConfig.Object);

            Container.Bind<NoiseGenerator>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void SampleNoise_AndTypeIsGeneric_SamplesFromGenericNoiseSource() {
            var position = new Vector3(1f, 2f, 3f);

            var mockNoiseSource = new Mock<INoiseTexture>();

            var noiseSample = new Vector4(1f, -20f, 300f, -4000f);

            mockNoiseSource.Setup(source => source.SampleBilinear(1f, 3f)).Returns(noiseSample);

            MockRenderConfig.Setup(config => config.GenericNoiseSource).Returns(mockNoiseSource.Object);
            MockRenderConfig.Setup(config => config.NoiseScale).Returns(1f);

            var noiseGenerator = Container.Resolve<NoiseGenerator>();

            Assert.AreEqual(noiseSample, noiseGenerator.SampleNoise(position, NoiseType.Generic));
        }

        [Test]
        public void SampleNoise_AndTypeIsFlatlandsHeight_SamplesFlatlandsElevationHeightmap() {
            var position = new Vector3(1f, 2f, 3f);

            var mockNoiseSource = new Mock<INoiseTexture>();

            var noiseSample = new Vector4(1f, -20f, 300f, -4000f);

            mockNoiseSource.Setup(source => source.SampleBilinear(1f, 3f)).Returns(noiseSample);

            MockRenderConfig.Setup(config => config.FlatlandsElevationHeightmap).Returns(mockNoiseSource.Object);
            MockRenderConfig.Setup(config => config.NoiseScale).Returns(1f);

            var noiseGenerator = Container.Resolve<NoiseGenerator>();

            Assert.AreEqual(noiseSample, noiseGenerator.SampleNoise(position, NoiseType.FlatlandsHeight));
        }

        [Test]
        public void SampleNoise_AndTypeIsHillsHeight_SamplesFromHillsElevationHeightmap() {
            var position = new Vector3(1f, 2f, 3f);

            var mockNoiseSource = new Mock<INoiseTexture>();

            var noiseSample = new Vector4(1f, -20f, 300f, -4000f);

            mockNoiseSource.Setup(source => source.SampleBilinear(1f, 3f)).Returns(noiseSample);

            MockRenderConfig.Setup(config => config.HillsElevationHeightmap).Returns(mockNoiseSource.Object);
            MockRenderConfig.Setup(config => config.NoiseScale).Returns(1f);

            var noiseGenerator = Container.Resolve<NoiseGenerator>();

            Assert.AreEqual(noiseSample, noiseGenerator.SampleNoise(position, NoiseType.HillsHeight));
        }

        [Test]
        public void SampleNoise_SampleModifiedByConfiguredNoiseScale() {
            var position = new Vector3(1f, 2f, 3f);

            var mockNoiseSource = new Mock<INoiseTexture>();

            var noiseSample = new Vector4(1f, -20f, 300f, -4000f);

            mockNoiseSource.Setup(source => source.SampleBilinear(2.5f, 3f * 2.5f)).Returns(noiseSample);

            MockRenderConfig.Setup(config => config.GenericNoiseSource).Returns(mockNoiseSource.Object);
            MockRenderConfig.Setup(config => config.NoiseScale).Returns(2.5f);

            var noiseGenerator = Container.Resolve<NoiseGenerator>();

            Assert.AreEqual(noiseSample, noiseGenerator.SampleNoise(position, NoiseType.Generic));
        }

        [Test]
        public void Perturb_TakesGenericSample_AddsExpandedXAndZSamplesOfGenericNoiseToPosition() {
            var position = new Vector3(-5f, -5f, -6f);

            var mockNoiseSource = new Mock<INoiseTexture>();

            var noiseSample = new Vector4(0.6f, 1000f, 0.4f, 1000f);

            mockNoiseSource.Setup(source => source.SampleBilinear(-5f, -6f)).Returns(noiseSample);

            MockRenderConfig.Setup(config => config.GenericNoiseSource).Returns(mockNoiseSource.Object);
            MockRenderConfig.Setup(config => config.NoiseScale).Returns(1f);
            MockRenderConfig.Setup(config => config.CellPerturbStrengthXZ).Returns(1f);

            var noiseGenerator = Container.Resolve<NoiseGenerator>();

            Assert.AreEqual(new Vector3(-5f + (0.6f * 2f - 1f), -5f, -6f + (0.4f * 2f - 1f)), noiseGenerator.Perturb(position));
        }

        [Test]
        public void Perturb_ChangeMultipliedByCellPerturbStrengthXZ() {
            var position = new Vector3(-5f, -5f, -6f);

            var mockNoiseSource = new Mock<INoiseTexture>();

            var noiseSample = new Vector4(0.6f, 1000f, 0.4f, 1000f);

            mockNoiseSource.Setup(source => source.SampleBilinear(-5f, -6f)).Returns(noiseSample);

            MockRenderConfig.Setup(config => config.GenericNoiseSource).Returns(mockNoiseSource.Object);
            MockRenderConfig.Setup(config => config.NoiseScale).Returns(1f);
            MockRenderConfig.Setup(config => config.CellPerturbStrengthXZ).Returns(10f);

            var noiseGenerator = Container.Resolve<NoiseGenerator>();

            Assert.AreEqual(new Vector3(-5f + (0.6f * 2f - 1f) * 10f, -5f, -6f + (0.4f * 2f - 1f) * 10f), noiseGenerator.Perturb(position));
        }

        [Test]
        public void MissingHashGridTests() {
            Assert.Ignore("It's not clear what unit tests would be useful for hash grid sampling");
        }

        #endregion

        #region utilities



        #endregion

        #endregion

    }

}
