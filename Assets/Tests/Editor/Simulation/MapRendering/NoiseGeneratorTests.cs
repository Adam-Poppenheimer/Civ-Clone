using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using NUnit.Framework;
using Zenject;
using UniRx;
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
        public void SampleNoise_AndTypeIsZeroToOne_SamplesPixelsWithNoiseScale_FromArguedSource_MultipliedByStrength() {
            var xzPosition = new Vector3(-5f, -6f);

            var sampledNoise = new Vector4(0.6f, 1000f, 0.4f, 1000f);

            var noiseSource = BuildNoiseTexture(new Tuple<Vector2, Vector4>(new Vector2(-10f, -12f), sampledNoise));

            MockRenderConfig.Setup(config => config.NoiseScale).Returns(2f);

            var noiseGenerator = Container.Resolve<NoiseGenerator>();

            Assert.AreEqual(sampledNoise * 3f, noiseGenerator.SampleNoise(xzPosition, noiseSource, 3f, NoiseType.ZeroToOne));
        }

        [Test]
        public void SampleNoise_AndTypeIsNegativeOneToOne_SampleSpreadIntoNegativeNumbersCorrectly() {
            var xzPosition = new Vector3(-5f, -6f);

            var sampledNoise = new Vector4(0.25f, 0.5f, 0.75f, 1f);

            var noiseSource = BuildNoiseTexture(new Tuple<Vector2, Vector4>(new Vector2(-10f, -12f), sampledNoise));

            MockRenderConfig.Setup(config => config.NoiseScale).Returns(2f);

            var noiseGenerator = Container.Resolve<NoiseGenerator>();

            Assert.AreEqual(
                new Vector4(-0.5f * 3f, 0f, 0.5f * 3f, 3f),
                noiseGenerator.SampleNoise(xzPosition, noiseSource, 3f, NoiseType.NegativeOneToOne)
            );
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

        private INoiseTexture BuildNoiseTexture(params Tuple<Vector2, Vector4>[] samples) {
            var mockTexture = new Mock<INoiseTexture>();

            foreach(var sample in samples) {
                mockTexture.Setup(texture => texture.SampleBilinear(sample.Item1.x, sample.Item1.y)).Returns(sample.Item2);
            }

            return mockTexture.Object;
        }

        #endregion

        #endregion

    }

}
