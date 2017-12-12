using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation.Units;
using Assets.Simulation.GameMap;

namespace Assets.Tests.Simulation.Units {

    [TestFixture]
    public class UnitTerrainCostLogicTests : ZenjectUnitTestFixture {

        #region static fields and properties

        private static IEnumerable TestCases {
            get {
                yield return new TestCaseData(TerrainType.Grassland,    TerrainShape.Flat, TerrainFeatureType.None).SetName("Grassland/Flat/None")   .Returns(1);
                yield return new TestCaseData(TerrainType.Plains,       TerrainShape.Flat, TerrainFeatureType.None).SetName("Plains/Flat/None")      .Returns(1);
                yield return new TestCaseData(TerrainType.Desert,       TerrainShape.Flat, TerrainFeatureType.None).SetName("Desert/Flat/None")      .Returns(1);
                yield return new TestCaseData(TerrainType.ShallowWater, TerrainShape.Flat, TerrainFeatureType.None).SetName("ShallowWater/Flat/None").Returns(1);
                yield return new TestCaseData(TerrainType.DeepWater,    TerrainShape.Flat, TerrainFeatureType.None).SetName("DeepWater/Flat/None")   .Returns(2);

                yield return new TestCaseData(TerrainType.Grassland,    TerrainShape.Hills, TerrainFeatureType.None).SetName("Grassland/Hills/None")   .Returns(2);
                yield return new TestCaseData(TerrainType.Plains,       TerrainShape.Hills, TerrainFeatureType.None).SetName("Plains/Hills/None")      .Returns(2);
                yield return new TestCaseData(TerrainType.Desert,       TerrainShape.Hills, TerrainFeatureType.None).SetName("Desert/Hills/None")      .Returns(2);
                yield return new TestCaseData(TerrainType.ShallowWater, TerrainShape.Hills, TerrainFeatureType.None).SetName("ShallowWater/Hills/None").Returns(1);
                yield return new TestCaseData(TerrainType.DeepWater,    TerrainShape.Hills, TerrainFeatureType.None).SetName("DeepWater/Hills/None")   .Returns(2);
                 
                yield return new TestCaseData(TerrainType.Grassland,    TerrainShape.Flat, TerrainFeatureType.Forest).SetName("Grassland/Flat/Forest")   .Returns(2);
                yield return new TestCaseData(TerrainType.Plains,       TerrainShape.Flat, TerrainFeatureType.Forest).SetName("Plains/Flat/Forest")      .Returns(2);
                yield return new TestCaseData(TerrainType.Desert,       TerrainShape.Flat, TerrainFeatureType.Forest).SetName("Desert/Flat/Forest")      .Returns(2);
                yield return new TestCaseData(TerrainType.ShallowWater, TerrainShape.Flat, TerrainFeatureType.Forest).SetName("ShallowWater/Flat/Forest").Returns(1);
                yield return new TestCaseData(TerrainType.DeepWater,    TerrainShape.Flat, TerrainFeatureType.Forest).SetName("DeepWater/Flat/Forest")   .Returns(2);

                yield return new TestCaseData(TerrainType.Grassland,    TerrainShape.Hills, TerrainFeatureType.Forest).SetName("Grassland/Hills/Forest")   .Returns(3);
                yield return new TestCaseData(TerrainType.Plains,       TerrainShape.Hills, TerrainFeatureType.Forest).SetName("Plains/Hills/Forest")      .Returns(3);
                yield return new TestCaseData(TerrainType.Desert,       TerrainShape.Hills, TerrainFeatureType.Forest).SetName("Desert/Hills/Forest")      .Returns(3);
                yield return new TestCaseData(TerrainType.ShallowWater, TerrainShape.Hills, TerrainFeatureType.Forest).SetName("ShallowWater/Hills/Forest").Returns(1);
                yield return new TestCaseData(TerrainType.DeepWater,    TerrainShape.Hills, TerrainFeatureType.Forest).SetName("DeepWater/Hills/Forest")   .Returns(2);
            }
        }

        #endregion

        #region instance fields and properties

        private Mock<ITileConfig> MockConfig;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockConfig = new Mock<ITileConfig>();

            MockConfig.Setup(config => config.GrasslandMoveCost)   .Returns(1);
            MockConfig.Setup(config => config.PlainsMoveCost)      .Returns(1);
            MockConfig.Setup(config => config.DesertMoveCost)      .Returns(1);
            MockConfig.Setup(config => config.ShallowWaterMoveCost).Returns(1);
            MockConfig.Setup(config => config.DeepWaterMoveCost)   .Returns(2);
            MockConfig.Setup(config => config.HillsMoveCost)       .Returns(1);
            MockConfig.Setup(config => config.ForestMoveCost)      .Returns(1);

            Container.Bind<ITileConfig>().FromInstance(MockConfig.Object);

            Container.Bind<UnitTerrainCostLogic>().AsSingle();
        }

        #endregion

        #region tests

        [Test(Description = "GetCostToMoveUnitIntoTile should consider only the properties of " +
            "the tile in question, summing up the movement costs of the terrain, shape and features " +
            "as defined in ITileConfig")]
        [TestCaseSource("TestCases")]
        public int GetCostToMoveUnitIntoTile_ConsidersTileCosts(
            TerrainType terrain, TerrainShape shape, TerrainFeatureType feature
        ){
            var tile = BuildTile(terrain, shape, feature);
            var unit = BuildUnit();

            var costLogic = Container.Resolve<UnitTerrainCostLogic>();

            return costLogic.GetCostToMoveUnitIntoTile(unit, tile);
        }

        #endregion

        #region utilities

        private IMapTile BuildTile(TerrainType terrain, TerrainShape shape, TerrainFeatureType feature) {
            var mockTile = new Mock<IMapTile>();

            mockTile.SetupAllProperties();

            var newTile = mockTile.Object;

            newTile.Terrain = terrain;
            newTile.Shape = shape;
            newTile.Feature = feature;

            return newTile;
        }

        private IUnit BuildUnit() {
            return new Mock<IUnit>().Object;
        }

        #endregion

        #endregion

    }

}
