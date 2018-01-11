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

namespace Assets.Tests.Simulation.HexMap {

    [TestFixture]
    public class TileResourceLogicTests : ZenjectUnitTestFixture {

        #region static fields and properties

        #region test cases

        private static IEnumerable<TestCaseData> TestCases {
            get {
                yield return new TestCaseData(TerrainType.Grassland, TerrainFeature.None).Returns(TerrainYields[0]).SetName("Grassland/None");
                yield return new TestCaseData(TerrainType.Plains,    TerrainFeature.None).Returns(TerrainYields[1]).SetName("Plans/None");
                yield return new TestCaseData(TerrainType.Desert,    TerrainFeature.None).Returns(TerrainYields[2]).SetName("Desert/None");
                yield return new TestCaseData(TerrainType.Tundra,    TerrainFeature.None).Returns(TerrainYields[3]).SetName("Tundra/None");
                yield return new TestCaseData(TerrainType.Snow,      TerrainFeature.None).Returns(TerrainYields[4]).SetName("Snow/None");

                yield return new TestCaseData(TerrainType.Grassland, TerrainFeature.Forest).Returns(FeatureYields[1]).SetName("Grassland/Forest");
                yield return new TestCaseData(TerrainType.Plains,    TerrainFeature.Forest).Returns(FeatureYields[1]).SetName("Plans/Forest");
                yield return new TestCaseData(TerrainType.Desert,    TerrainFeature.Forest).Returns(FeatureYields[1]).SetName("Desert/Forest");
                yield return new TestCaseData(TerrainType.Tundra,    TerrainFeature.Forest).Returns(FeatureYields[1]).SetName("Tundra/Forest");
                yield return new TestCaseData(TerrainType.Snow,      TerrainFeature.Forest).Returns(FeatureYields[1]).SetName("Snow/Forest");
            }
        }

        private static List<ResourceSummary> TerrainYields = new List<ResourceSummary>() {
            new ResourceSummary(1f), //Grassland
            new ResourceSummary(2f), //Plains
            new ResourceSummary(3f), //Desert
            new ResourceSummary(4f), //Tundra
            new ResourceSummary(5f), //Snow
        };

        private static List<ResourceSummary> FeatureYields =  new List<ResourceSummary>() {
            new ResourceSummary(6f), //None
            new ResourceSummary(7f), //Forest
        };

        #endregion

        #endregion

        #region instance fields and properties

        private Mock<IHexGridConfig> MockConfig;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockConfig = new Mock<IHexGridConfig>();

            MockConfig.Setup(config => config.TerrainYields).Returns(TerrainYields.AsReadOnly());
            MockConfig.Setup(config => config.FeatureYields).Returns(FeatureYields.AsReadOnly());

            Container.Bind<IHexGridConfig>().FromInstance(MockConfig.Object);

            Container.Bind<TileResourceLogic>().AsSingle();
        }

        #endregion

        #region tests

        [Test(Description = "GetYieldOfCell should return the yield of the cell's terrain " +
            "if it has no feature, and the yield of the feature if it does")]
        [TestCaseSource("TestCases")]
        public ResourceSummary GetYieldOfCellTests(TerrainType terrain, TerrainFeature feature) {
            var cell = BuildCell(terrain, feature);

            var resourceLogic = Container.Resolve<TileResourceLogic>();

            return resourceLogic.GetYieldOfCell(cell);
        }

        #endregion

        #region utilities

        private IHexCell BuildCell(TerrainType terrain, TerrainFeature feature) {
            var mockCell = new Mock<IHexCell>();

            mockCell.SetupAllProperties();

            var newCell = mockCell.Object;

            newCell.Terrain = terrain;
            newCell.Feature = feature;

            return newCell;
        }

        #endregion

        #endregion

    }

}
