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
using Assets.Simulation.Cities;

namespace Assets.Tests.Simulation.Units {

    [TestFixture]
    public class UnitTerrainCostLogicTests : ZenjectUnitTestFixture {

        #region internal types

        public struct TestData {

            public TerrainType NextCellTerrain;
            public TerrainFeature NextCellFeature;
            public int NextCellElevation;
            public bool NextCellIsUnderwater;

            public int CurrentCellElevation;

            public bool UnitIsAquatic;

            public bool NextHasCity;

        }

        #endregion

        #region static fields and properties

        private static IEnumerable TestCases {
            get {
                yield return new TestCaseData(new TestData() {
                    NextCellTerrain      = TerrainType.Grassland,
                    NextCellFeature      = TerrainFeature.None,
                    NextCellElevation    = 0,
                    NextCellIsUnderwater = false,
                    CurrentCellElevation = 0,
                    UnitIsAquatic        = false
                }).Returns(1).SetName("Non-aquatic into empty grassland, no elevation change, no water");

                yield return new TestCaseData(new TestData() {
                    NextCellTerrain      = TerrainType.Grassland,
                    NextCellFeature      = TerrainFeature.None,
                    NextCellElevation    = 0,
                    NextCellIsUnderwater = false,
                    CurrentCellElevation = 0,
                    UnitIsAquatic        = true
                }).Returns(-1).SetName("Aquatic into empty grassland, no water");

                yield return new TestCaseData(new TestData() {
                    NextCellIsUnderwater = true,
                    UnitIsAquatic        = false
                }).Returns(-1).SetName("Non-aquatic into underwater tile");

                yield return new TestCaseData(new TestData() {
                    NextCellIsUnderwater = true,
                    UnitIsAquatic        = true
                }).Returns(1).SetName("Aquatic into underwater tile");

                yield return new TestCaseData(new TestData() {
                    NextCellIsUnderwater = false,
                    UnitIsAquatic        = true,
                    NextHasCity          = true
                }).Returns(1).SetName("Aquatic into land tile with city");

                yield return new TestCaseData(new TestData() {
                    NextCellTerrain      = TerrainType.Grassland,
                    NextCellFeature      = TerrainFeature.Forest,
                    NextCellElevation    = 0,
                    NextCellIsUnderwater = false,
                    CurrentCellElevation = 0,
                    UnitIsAquatic        = false
                }).Returns(2).SetName("Non-aquatic into forested grassland, no elevation change, no water");

                yield return new TestCaseData(new TestData() {
                    NextCellTerrain      = TerrainType.Grassland,
                    NextCellFeature      = TerrainFeature.None,
                    NextCellElevation    = 1,
                    NextCellIsUnderwater = false,
                    CurrentCellElevation = 0,
                    UnitIsAquatic        = false
                }).Returns(3).SetName("Non-aquatic into empty grassland, elevation increase of 1, no water");

                yield return new TestCaseData(new TestData() {
                    NextCellTerrain      = TerrainType.Grassland,
                    NextCellFeature      = TerrainFeature.None,
                    NextCellElevation    = 2,
                    NextCellIsUnderwater = false,
                    CurrentCellElevation = 0,
                    UnitIsAquatic        = false
                }).Returns(-1).SetName("Non-aquatic into empty grassland, elevation increase of 2, no water");

                yield return new TestCaseData(new TestData() {
                    NextCellTerrain      = TerrainType.Grassland,
                    NextCellFeature      = TerrainFeature.None,
                    NextCellElevation    = 0,
                    NextCellIsUnderwater = false,
                    CurrentCellElevation = 1,
                    UnitIsAquatic        = false
                }).Returns(1).SetName("Non-aquatic into empty grassland, elevation decrease of 1, no water");

                yield return new TestCaseData(new TestData() {
                    NextCellTerrain      = TerrainType.Grassland,
                    NextCellFeature      = TerrainFeature.None,
                    NextCellElevation    = 0,
                    NextCellIsUnderwater = false,
                    CurrentCellElevation = 2,
                    UnitIsAquatic        = false
                }).Returns(-1).SetName("Non-aquatic into empty grassland, elevation decrease of 2, no water");

                yield return new TestCaseData(new TestData() {
                    NextCellTerrain      = TerrainType.Grassland,
                    NextCellFeature      = TerrainFeature.Forest,
                    NextCellElevation    = 1,
                    NextCellIsUnderwater = false,
                    CurrentCellElevation = 0,
                    UnitIsAquatic        = false
                }).Returns(4).SetName("Non-aquatic into forest, elevation increase of 1, no water");
            }
        }

        #endregion

        #region instance fields and properties

        private Mock<IHexGridConfig> MockConfig;

        private Mock<ICityFactory> MockCityFactory;

        private List<ICity> AllCities = new List<ICity>();

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            AllCities.Clear();

            MockConfig      = new Mock<IHexGridConfig>();
            MockCityFactory = new Mock<ICityFactory>();

            MockConfig.Setup(config => config.BaseLandMoveCost).Returns(1);
            MockConfig.Setup(config => config.WaterMoveCost)   .Returns(1);
            MockConfig.Setup(config => config.SlopeMoveCost)   .Returns(2);

            MockConfig.Setup(config => config.FeatureMoveCosts).Returns(new List<int>() {
                -1, //None cost
                1,  //Forest cost
            }.AsReadOnly());

            MockCityFactory.Setup(factory => factory.AllCities).Returns(AllCities.AsReadOnly());

            Container.Bind<IHexGridConfig>().FromInstance(MockConfig.Object);
            Container.Bind<ICityFactory>  ().FromInstance(MockCityFactory.Object);

            Container.Bind<UnitTerrainCostLogic>().AsSingle();
        }

        #endregion

        #region tests

        [Test(Description = "GetTraversalCostForUnit should consider the terrain and features of NextCell, " +
            "as well as whether that cell is underwater, to determine unit traversal. It should also consider " +
            "the EdgeType between currentCell and nextCell and whether the argued unit is aquatic or not")]
        [TestCaseSource("TestCases")]
        public int GetTraversalCostForUnitTests(TestData data){
            var currentCell = BuildCell(
                TerrainType.Grassland, TerrainFeature.None,
                data.CurrentCellElevation, false
            );

            var nextCell = BuildCell(
                data.NextCellTerrain, data.NextCellFeature,
                data.NextCellElevation, data.NextCellIsUnderwater
            );

            var unit = BuildUnit(data.UnitIsAquatic);

            if(data.NextHasCity) {
                BuildCity(nextCell);
            }

            var costLogic = Container.Resolve<UnitTerrainCostLogic>();

            return costLogic.GetTraversalCostForUnit(unit, currentCell, nextCell);
        }

        #endregion

        #region utilities

        private IHexCell BuildCell(
            TerrainType terrain, TerrainFeature feature,
            int elevation, bool isUnderwater
        ){
            var mockCell = new Mock<IHexCell>();

            mockCell.SetupAllProperties();
            mockCell.Setup(cell => cell.IsUnderwater).Returns(isUnderwater);

            var newCell = mockCell.Object;

            newCell.Terrain = terrain;
            newCell.Shape   = TerrainShape.Flat;
            newCell.Feature = feature;
            newCell.Elevation = elevation;

            return newCell;
        }

        private IUnit BuildUnit(bool isAquatic) {
            var unitMock = new Mock<IUnit>();

            var mockTemplate = new Mock<IUnitTemplate>();
            mockTemplate.Setup(template => template.IsAquatic).Returns(isAquatic);

            unitMock.Setup(unit => unit.Template).Returns(mockTemplate.Object);

            return unitMock.Object;
        }

        private ICity BuildCity(IHexCell location) {
            var mockCity = new Mock<ICity>();

            mockCity.Setup(city => city.Location).Returns(location);

            AllCities.Add(mockCity.Object);

            return mockCity.Object;
        }

        #endregion

        #endregion

    }

}
