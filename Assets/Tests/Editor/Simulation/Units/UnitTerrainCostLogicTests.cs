using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation.Units;
using Assets.Simulation.HexMap;

namespace Assets.Tests.Simulation.Units {

    [TestFixture]
    public class UnitTerrainCostLogicTests : ZenjectUnitTestFixture {

        #region static fields and properties

        private static IEnumerable TestCases {
            get {
                yield return new TestCaseData(TerrainType.Grassland,    TerrainShape.Flat, TerrainFeature.None).SetName("Grassland/Flat/None")   .Returns(1);
                yield return new TestCaseData(TerrainType.Plains,       TerrainShape.Flat, TerrainFeature.None).SetName("Plains/Flat/None")      .Returns(1);
                yield return new TestCaseData(TerrainType.Desert,       TerrainShape.Flat, TerrainFeature.None).SetName("Desert/Flat/None")      .Returns(1);
                yield return new TestCaseData(TerrainType.ShallowWater, TerrainShape.Flat, TerrainFeature.None).SetName("ShallowWater/Flat/None").Returns(1);
                yield return new TestCaseData(TerrainType.DeepWater,    TerrainShape.Flat, TerrainFeature.None).SetName("DeepWater/Flat/None")   .Returns(2);

                yield return new TestCaseData(TerrainType.Grassland,    TerrainShape.Hills, TerrainFeature.None).SetName("Grassland/Hills/None")   .Returns(2);
                yield return new TestCaseData(TerrainType.Plains,       TerrainShape.Hills, TerrainFeature.None).SetName("Plains/Hills/None")      .Returns(2);
                yield return new TestCaseData(TerrainType.Desert,       TerrainShape.Hills, TerrainFeature.None).SetName("Desert/Hills/None")      .Returns(2);
                yield return new TestCaseData(TerrainType.ShallowWater, TerrainShape.Hills, TerrainFeature.None).SetName("ShallowWater/Hills/None").Returns(1);
                yield return new TestCaseData(TerrainType.DeepWater,    TerrainShape.Hills, TerrainFeature.None).SetName("DeepWater/Hills/None")   .Returns(2);
                 
                yield return new TestCaseData(TerrainType.Grassland,    TerrainShape.Flat, TerrainFeature.Forest).SetName("Grassland/Flat/Forest")   .Returns(2);
                yield return new TestCaseData(TerrainType.Plains,       TerrainShape.Flat, TerrainFeature.Forest).SetName("Plains/Flat/Forest")      .Returns(2);
                yield return new TestCaseData(TerrainType.Desert,       TerrainShape.Flat, TerrainFeature.Forest).SetName("Desert/Flat/Forest")      .Returns(2);
                yield return new TestCaseData(TerrainType.ShallowWater, TerrainShape.Flat, TerrainFeature.Forest).SetName("ShallowWater/Flat/Forest").Returns(1);
                yield return new TestCaseData(TerrainType.DeepWater,    TerrainShape.Flat, TerrainFeature.Forest).SetName("DeepWater/Flat/Forest")   .Returns(2);

                yield return new TestCaseData(TerrainType.Grassland,    TerrainShape.Hills, TerrainFeature.Forest).SetName("Grassland/Hills/Forest")   .Returns(3);
                yield return new TestCaseData(TerrainType.Plains,       TerrainShape.Hills, TerrainFeature.Forest).SetName("Plains/Hills/Forest")      .Returns(3);
                yield return new TestCaseData(TerrainType.Desert,       TerrainShape.Hills, TerrainFeature.Forest).SetName("Desert/Hills/Forest")      .Returns(3);
                yield return new TestCaseData(TerrainType.ShallowWater, TerrainShape.Hills, TerrainFeature.Forest).SetName("ShallowWater/Hills/Forest").Returns(1);
                yield return new TestCaseData(TerrainType.DeepWater,    TerrainShape.Hills, TerrainFeature.Forest).SetName("DeepWater/Hills/Forest")   .Returns(2);
            }
        }

        #endregion

        #region instance fields and properties

        private Mock<IHexGridConfig> MockConfig;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockConfig = new Mock<IHexGridConfig>();

            MockConfig.Setup(config => config.GrasslandMoveCost)   .Returns(1);
            MockConfig.Setup(config => config.PlainsMoveCost)      .Returns(1);
            MockConfig.Setup(config => config.DesertMoveCost)      .Returns(1);
            MockConfig.Setup(config => config.ShallowWaterMoveCost).Returns(1);
            MockConfig.Setup(config => config.DeepWaterMoveCost)   .Returns(2);
            MockConfig.Setup(config => config.HillsMoveCost)       .Returns(1);
            MockConfig.Setup(config => config.ForestMoveCost)      .Returns(1);

            Container.Bind<IHexGridConfig>().FromInstance(MockConfig.Object);

            Container.Bind<UnitTerrainCostLogic>().AsSingle();
        }

        #endregion

        #region tests

        [Test(Description = "GetCostToMoveUnitIntoTile should consider only the properties of " +
            "the tile in question, summing up the movement costs of the terrain, shape and features " +
            "as defined in ITileConfig")]
        [TestCaseSource("TestCases")]
        public int GetCostToMoveUnitIntoTile_ConsidersTileCosts(
            TerrainType terrain, TerrainShape shape, TerrainFeature feature
        ){
            var tile = BuildTile(terrain, shape, feature);
            var unit = BuildUnit();

            var costLogic = Container.Resolve<UnitTerrainCostLogic>();

            return costLogic.GetCostToMoveUnitIntoTile(unit, tile);
        }

        #endregion

        #region utilities

        private IHexCell BuildTile(TerrainType terrain, TerrainShape shape, TerrainFeature feature) {
            var mockTile = new Mock<IHexCell>();

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
