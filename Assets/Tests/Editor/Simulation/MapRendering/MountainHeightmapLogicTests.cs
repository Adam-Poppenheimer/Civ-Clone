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

    public class MountainHeightmapLogicTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IMapRenderConfig>              MockRenderConfig;
        private Mock<INoiseGenerator>               MockNoiseGenerator;
        private Mock<IHexGrid>                      MockGrid;
        private Mock<IMountainHeightmapWeightLogic> MockMountainHeightmapWeightLogic;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockRenderConfig                 = new Mock<IMapRenderConfig>();
            MockNoiseGenerator               = new Mock<INoiseGenerator>();
            MockGrid                         = new Mock<IHexGrid>();
            MockMountainHeightmapWeightLogic = new Mock<IMountainHeightmapWeightLogic>();

            Container.Bind<IMapRenderConfig>             ().FromInstance(MockRenderConfig                .Object);
            Container.Bind<INoiseGenerator>              ().FromInstance(MockNoiseGenerator              .Object);
            Container.Bind<IHexGrid>                     ().FromInstance(MockGrid                        .Object);
            Container.Bind<IMountainHeightmapWeightLogic>().FromInstance(MockMountainHeightmapWeightLogic.Object);

            Container.Bind<MountainHeightmapLogic>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void GetHeightForPosition_AndOnlyPeakWeightNonzero_ReturnsConfiguredPeakElevation() {
            var cell = BuildCell(CellShape.Flatlands);

            var position = new Vector3(1f, 2f, 3f);

            float peakWeight = 0.75f, ridgeWeight = 0f, hillsWeight = 0f;
            MockMountainHeightmapWeightLogic.Setup(
                logic => logic.GetHeightWeightsForPosition(
                    position, cell, HexDirection.E, out peakWeight, out ridgeWeight, out hillsWeight
                )
            );

            MockRenderConfig.Setup(config => config.MountainPeakElevation).Returns(15.5f);

            var heightmapLogic = Container.Resolve<MountainHeightmapLogic>();

            Assert.AreEqual(0.75f * 15.5f, heightmapLogic.GetHeightForPosition(position, cell, HexDirection.E));
        }

        [Test]
        public void GetHeightForPosition_OnlyRidgeWeightNonzero_AndNeighborIsMountains_ReturnsConfiguredRidgeElevation() {
            var cell = BuildCell(CellShape.Flatlands);

            MockGrid.Setup(grid => grid.HasNeighbor(cell, HexDirection.E)).Returns(true);
            MockGrid.Setup(grid => grid.GetNeighbor(cell, HexDirection.E)).Returns(BuildCell(CellShape.Mountains));

            var position = new Vector3(1f, 2f, 3f);

            float peakWeight = 0f, ridgeWeight = 0.75f, hillsWeight = 0f;
            MockMountainHeightmapWeightLogic.Setup(
                logic => logic.GetHeightWeightsForPosition(
                    position, cell, HexDirection.E, out peakWeight, out ridgeWeight, out hillsWeight
                )
            );

            MockRenderConfig.Setup(config => config.MountainRidgeElevation).Returns(15.5f);

            var heightmapLogic = Container.Resolve<MountainHeightmapLogic>();

            Assert.AreEqual(0.75f * 15.5f, heightmapLogic.GetHeightForPosition(position, cell, HexDirection.E));
        }

        [Test]
        public void GetHeightForPosition_OnlyRidgeWeightNonzero_AndNeighborIsNotMountains_ReturnsSampledHillNoise() {
            var cell = BuildCell(CellShape.Flatlands);

            MockGrid.Setup(grid => grid.HasNeighbor(cell, HexDirection.E)).Returns(true);
            MockGrid.Setup(grid => grid.GetNeighbor(cell, HexDirection.E)).Returns(BuildCell(CellShape.Hills));

            var position = new Vector3(1f, 2f, 3f);

            float peakWeight = 0f, ridgeWeight = 0.75f, hillsWeight = 0f;
            MockMountainHeightmapWeightLogic.Setup(
                logic => logic.GetHeightWeightsForPosition(
                    position, cell, HexDirection.E, out peakWeight, out ridgeWeight, out hillsWeight
                )
            );

            MockNoiseGenerator.Setup(noise => noise.SampleNoise(position, NoiseType.HillsHeight))
                              .Returns(new Vector4(15.5f, 0f, 0f, 0f));

            var heightmapLogic = Container.Resolve<MountainHeightmapLogic>();

            Assert.AreEqual(0.75f * 15.5f, heightmapLogic.GetHeightForPosition(position, cell, HexDirection.E));
        }

        [Test]
        public void GetHeightForPosition_OnlyRidgeWeightNonzero_AndNoNeighbor_ReturnsSampledHillNoise() {
            var cell = BuildCell(CellShape.Flatlands);

            MockGrid.Setup(grid => grid.HasNeighbor(cell, HexDirection.E)).Returns(false);
            MockGrid.Setup(grid => grid.GetNeighbor(cell, HexDirection.E)).Returns(BuildCell(CellShape.Mountains));

            var position = new Vector3(1f, 2f, 3f);

            float peakWeight = 0f, ridgeWeight = 0.75f, hillsWeight = 0f;
            MockMountainHeightmapWeightLogic.Setup(
                logic => logic.GetHeightWeightsForPosition(
                    position, cell, HexDirection.E, out peakWeight, out ridgeWeight, out hillsWeight
                )
            );

            MockNoiseGenerator.Setup(noise => noise.SampleNoise(position, NoiseType.HillsHeight))
                              .Returns(new Vector4(15.5f, 0f, 0f, 0f));

            var heightmapLogic = Container.Resolve<MountainHeightmapLogic>();

            Assert.AreEqual(0.75f * 15.5f, heightmapLogic.GetHeightForPosition(position, cell, HexDirection.E));
        }

        [Test]
        public void GetHeightForPosition_AndOnlyHillsWeightNonzero_ReturnsSampledHillNoise() {
            var cell = BuildCell(CellShape.Flatlands);

            var position = new Vector3(1f, 2f, 3f);

            float peakWeight = 0f, ridgeWeight = 0f, hillsWeight = 0.75f;
            MockMountainHeightmapWeightLogic.Setup(
                logic => logic.GetHeightWeightsForPosition(
                    position, cell, HexDirection.E, out peakWeight, out ridgeWeight, out hillsWeight
                )
            );

            MockNoiseGenerator.Setup(noise => noise.SampleNoise(position, NoiseType.HillsHeight))
                              .Returns(new Vector4(15.5f, 0f, 0f, 0f));

            var heightmapLogic = Container.Resolve<MountainHeightmapLogic>();

            Assert.AreEqual(0.75f * 15.5f, heightmapLogic.GetHeightForPosition(position, cell, HexDirection.E));
        }

        [Test]
        public void GetHeightForPosition_AndMultipleWeights_SumsComponents() {
            var cell = BuildCell(CellShape.Flatlands);

            MockGrid.Setup(grid => grid.HasNeighbor(cell, HexDirection.E)).Returns(true);
            MockGrid.Setup(grid => grid.GetNeighbor(cell, HexDirection.E)).Returns(BuildCell(CellShape.Mountains));

            var position = new Vector3(1f, 2f, 3f);

            float peakWeight = 0.6f, ridgeWeight = 0.4f, hillsWeight = 0.3f;
            MockMountainHeightmapWeightLogic.Setup(
                logic => logic.GetHeightWeightsForPosition(
                    position, cell, HexDirection.E, out peakWeight, out ridgeWeight, out hillsWeight
                )
            );

            MockRenderConfig.Setup(config => config.MountainPeakElevation) .Returns(10f);
            MockRenderConfig.Setup(config => config.MountainRidgeElevation).Returns(20f);

            MockNoiseGenerator.Setup(noise => noise.SampleNoise(position, NoiseType.HillsHeight))
                              .Returns(new Vector4(30f, 0f, 0f, 0f));

            var heightmapLogic = Container.Resolve<MountainHeightmapLogic>();

            Assert.AreEqual(10f * 0.6f + 20f * 0.4f, + 30f * 0.3f, heightmapLogic.GetHeightForPosition(position, cell, HexDirection.E));
        }

        #endregion

        #region utilities

        private IHexCell BuildCell(CellShape shape) {
            var mockCell = new Mock<IHexCell>();

            mockCell.Setup(cell => cell.Shape).Returns(shape);

            return mockCell.Object;
        }

        #endregion

        #endregion

    }

}
