using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation;
using Assets.Simulation.HexMap;

namespace Assets.Tests.Simulation.GameMap {

    [TestFixture]
    public class TileResourceLogicTests : ZenjectUnitTestFixture {

        [SetUp]
        public void CommonInstall() {
            var mockConfig = new Mock<ITileConfig>();

            mockConfig.Setup(config => config.GrasslandYield).Returns(new ResourceSummary(food: 1));
            mockConfig.Setup(config => config.PlainsYield    ).Returns(new ResourceSummary(food: 2));
            mockConfig.Setup(config => config.DesertYield    ).Returns(new ResourceSummary(food: 3));
            mockConfig.Setup(config => config.ForestYield    ).Returns(new ResourceSummary(food: 4));
            mockConfig.Setup(config => config.HillsYield     ).Returns(new ResourceSummary(food: 5));

            Container.Bind<ITileConfig>().FromInstance(mockConfig.Object);

            Container.Bind<TileResourceLogic>().AsSingle();
        }

        /*[Test(Description = "When GetYieldOfTile is called on an empty grassland, the yield of the grassland should apply")]
        public void OnEmptyGrassland_GrasslandYieldApplies() {
            var mockTile = new Mock<IMapTile>();
            mockTile.SetupAllProperties();

            mockTile.Object.Terrain = TerrainType.Grassland;
            mockTile.Object.Shape   = TerrainShape.Flat;
            mockTile.Object.Feature = TerrainFeatureType.None;

            var config = Container.Resolve<ITileResourceConfig>();

            var resourceLogic = Container.Resolve<TileResourceLogic>();

            Assert.AreEqual(config.GrasslandsYield, resourceLogic.GetYieldOfTile(mockTile.Object),
                "GetYieldOfTile did not return the expected yield");
        }*/

        [Test(Description = "When GetYieldOfTile is called on an empty grassland, the yield of the plains should apply")]
        public void OnEmptyPlains_PlainsYieldApplies() {
            var mockTile = new Mock<IHexCell>();
            mockTile.SetupAllProperties();

            mockTile.Object.Terrain = TerrainType.Plains;
            mockTile.Object.Shape   = TerrainShape.Flat;
            mockTile.Object.Feature = TerrainFeature.None;

            var config = Container.Resolve<ITileConfig>();

            var resourceLogic = Container.Resolve<TileResourceLogic>();

            Assert.AreEqual(config.PlainsYield, resourceLogic.GetYieldOfTile(mockTile.Object),
                "GetYieldOfTile did not return the expected yield");
        }

        [Test(Description = "When GetYieldOfTile is called on an empty grassland, the yield of the desert should apply")]
        public void OnEmptyDesert_DesertYieldApplies() {
            var mockTile = new Mock<IHexCell>();
            mockTile.SetupAllProperties();

            mockTile.Object.Terrain = TerrainType.Desert;
            mockTile.Object.Shape   = TerrainShape.Flat;
            mockTile.Object.Feature = TerrainFeature.None;

            var config = Container.Resolve<ITileConfig>();

            var resourceLogic = Container.Resolve<TileResourceLogic>();

            Assert.AreEqual(config.DesertYield, resourceLogic.GetYieldOfTile(mockTile.Object),
                "GetYieldOfTile did not return the expected yield");
        }

        [Test(Description = "When GetYieldOfTile is called on a forested grassland, the yield of the forest should apply")]
        public void OnForestedGrassland_ForestYieldApplies() {
            var mockTile = new Mock<IHexCell>();
            mockTile.SetupAllProperties();

            mockTile.Object.Terrain = TerrainType.Grassland;
            mockTile.Object.Shape   = TerrainShape.Flat;
            mockTile.Object.Feature = TerrainFeature.Forest;

            var config = Container.Resolve<ITileConfig>();

            var resourceLogic = Container.Resolve<TileResourceLogic>();

            Assert.AreEqual(config.ForestYield, resourceLogic.GetYieldOfTile(mockTile.Object),
                "GetYieldOfTile did not return the expected yield");
        }

        [Test(Description = "When GetYieldOfTile is called on a forested plains, the yield of the forest should apply")]
        public void OnForestedPlains_ForestYieldApplies() {
            var mockTile = new Mock<IHexCell>();
            mockTile.SetupAllProperties();

            mockTile.Object.Terrain = TerrainType.Plains;
            mockTile.Object.Shape   = TerrainShape.Flat;
            mockTile.Object.Feature = TerrainFeature.Forest;

            var config = Container.Resolve<ITileConfig>();

            var resourceLogic = Container.Resolve<TileResourceLogic>();

            Assert.AreEqual(config.ForestYield, resourceLogic.GetYieldOfTile(mockTile.Object),
                "GetYieldOfTile did not return the expected yield");
        }

        [Test(Description = "When GetYieldOfTile is called on a forested desert, the yield of the forest should apply")]
        public void OnForestedDesert_ForestYieldApplies() {
            var mockTile = new Mock<IHexCell>();
            mockTile.SetupAllProperties();

            mockTile.Object.Terrain = TerrainType.Desert;
            mockTile.Object.Shape   = TerrainShape.Flat;
            mockTile.Object.Feature = TerrainFeature.Forest;

            var config = Container.Resolve<ITileConfig>();

            var resourceLogic = Container.Resolve<TileResourceLogic>();

            Assert.AreEqual(config.ForestYield, resourceLogic.GetYieldOfTile(mockTile.Object),
                "GetYieldOfTile did not return the expected yield");
        }

        [Test(Description = "When GetYieldOfTile is called on a hilled grassland, the yield of the hill should apply")]
        public void OnHilledGrassland_HillYieldApplies() {
            var mockTile = new Mock<IHexCell>();
            mockTile.SetupAllProperties();

            mockTile.Object.Terrain = TerrainType.Grassland;
            mockTile.Object.Shape   = TerrainShape.Hills;
            mockTile.Object.Feature = TerrainFeature.None;

            var config = Container.Resolve<ITileConfig>();

            var resourceLogic = Container.Resolve<TileResourceLogic>();

            Assert.AreEqual(config.HillsYield, resourceLogic.GetYieldOfTile(mockTile.Object),
                "GetYieldOfTile did not return the expected yield");
        }

        [Test(Description = "When GetYieldOfTile is called on a hilled plains, the yield of the hill should apply")]
        public void OnHilledPlains_HillYieldApplies() {
            var mockTile = new Mock<IHexCell>();
            mockTile.SetupAllProperties();

            mockTile.Object.Terrain = TerrainType.Plains;
            mockTile.Object.Shape   = TerrainShape.Hills;
            mockTile.Object.Feature = TerrainFeature.None;

            var config = Container.Resolve<ITileConfig>();

            var resourceLogic = Container.Resolve<TileResourceLogic>();

            Assert.AreEqual(config.HillsYield, resourceLogic.GetYieldOfTile(mockTile.Object),
                "GetYieldOfTile did not return the expected yield");
        }

        [Test(Description = "When GetYieldOfTile is called on a hilled desert, the yield of the hill should apply")]
        public void OnHilledDesert_HillYieldApplies() {
            var mockTile = new Mock<IHexCell>();
            mockTile.SetupAllProperties();

            mockTile.Object.Terrain = TerrainType.Desert;
            mockTile.Object.Shape   = TerrainShape.Hills;
            mockTile.Object.Feature = TerrainFeature.None;

            var config = Container.Resolve<ITileConfig>();

            var resourceLogic = Container.Resolve<TileResourceLogic>();

            Assert.AreEqual(config.HillsYield, resourceLogic.GetYieldOfTile(mockTile.Object),
                "GetYieldOfTile did not return the expected yield");
        }

        [Test(Description = "When GetYieldOfTile is called on a hilled grassland with a forest, the "
            + "yield of the forest should apply")]
        public void OnHilledForestedGrassland_ForestYieldApplies() {
            var mockTile = new Mock<IHexCell>();
            mockTile.SetupAllProperties();

            mockTile.Object.Terrain = TerrainType.Grassland;
            mockTile.Object.Shape   = TerrainShape.Hills;
            mockTile.Object.Feature = TerrainFeature.Forest;

            var config = Container.Resolve<ITileConfig>();

            var resourceLogic = Container.Resolve<TileResourceLogic>();

            Assert.AreEqual(config.ForestYield, resourceLogic.GetYieldOfTile(mockTile.Object),
                "GetYieldOfTile did not return the expected yield");
        }

        [Test(Description = "When GetYieldOfTile is called on a hilled plains with a forest, the "
            + "yield of the forest should apply")]
        public void OnHilledForestedPlains_ForestYieldApplies() {
            var mockTile = new Mock<IHexCell>();
            mockTile.SetupAllProperties();

            mockTile.Object.Terrain = TerrainType.Plains;
            mockTile.Object.Shape   = TerrainShape.Hills;
            mockTile.Object.Feature = TerrainFeature.Forest;

            var config = Container.Resolve<ITileConfig>();

            var resourceLogic = Container.Resolve<TileResourceLogic>();

            Assert.AreEqual(config.ForestYield, resourceLogic.GetYieldOfTile(mockTile.Object),
                "GetYieldOfTile did not return the expected yield");
        }

        [Test(Description = "When GetYieldOfTile is called on a hilled desert with a forest, the " +
            "yield of the forest should apply")]
        public void OnHilledForestedDesert_ForestYieldApplies() {
            var mockTile = new Mock<IHexCell>();
            mockTile.SetupAllProperties();

            mockTile.Object.Terrain = TerrainType.Desert;
            mockTile.Object.Shape   = TerrainShape.Hills;
            mockTile.Object.Feature = TerrainFeature.Forest;

            var config = Container.Resolve<ITileConfig>();

            var resourceLogic = Container.Resolve<TileResourceLogic>();

            Assert.AreEqual(config.ForestYield, resourceLogic.GetYieldOfTile(mockTile.Object),
                "GetYieldOfTile did not return the expected yield");
        }

        [Test(Description = "When GetYieldOfTile is called on a null MapTile argument, an " + 
            "ArgumentNullException should be thrown")]
        public void OnNullMapTile_ThrowsException() {
           var resourceLogic = Container.Resolve<TileResourceLogic>();

            Assert.Throws<ArgumentNullException>(() => resourceLogic.GetYieldOfTile(null),
                "GetYieldOfTile failed to throw and ArgumentNullException on a null MapTile argument");
        }

    }

}
