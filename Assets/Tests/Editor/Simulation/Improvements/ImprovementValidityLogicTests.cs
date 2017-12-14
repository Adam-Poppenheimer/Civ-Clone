using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation.Improvements;
using Assets.Simulation.GameMap;
using Assets.Simulation.Cities;

namespace Assets.Tests.Simulation.Improvements {

    [TestFixture]
    public class ImprovementValidityLogicTests : ZenjectUnitTestFixture {

        #region static fields and properties

        #region test cases

        private static IEnumerable ValidityTestCases {
            get {
                yield return new TestCaseData(
                    TerrainType.Grassland, TerrainShape.Flat, TerrainFeatureType.None,
                    new List<TerrainType>       () { TerrainType.Grassland },
                    new List<TerrainShape>      () { TerrainShape.Flat },
                    new List<TerrainFeatureType>() { TerrainFeatureType.None })
                .SetName("All aspects are valid")
                .Returns(true);

                yield return new TestCaseData(
                    TerrainType.Grassland, TerrainShape.Flat, TerrainFeatureType.None,
                    new List<TerrainType>       () { TerrainType.Plains },
                    new List<TerrainShape>      () { TerrainShape.Flat },
                    new List<TerrainFeatureType>() { TerrainFeatureType.None })
                .SetName("Only terrain is invalid")
                .Returns(false);

                yield return new TestCaseData(
                    TerrainType.Grassland, TerrainShape.Flat, TerrainFeatureType.None,
                    new List<TerrainType>       () { TerrainType.Grassland },
                    new List<TerrainShape>      () { TerrainShape.Hills },
                    new List<TerrainFeatureType>() { TerrainFeatureType.None })
                .SetName("Only shape is invalid")
                .Returns(false);

                yield return new TestCaseData(
                    TerrainType.Grassland, TerrainShape.Flat, TerrainFeatureType.None,
                    new List<TerrainType>       () { TerrainType.Grassland },
                    new List<TerrainShape>      () { TerrainShape.Hills },
                    new List<TerrainFeatureType>() { TerrainFeatureType.Forest })
                .SetName("Only feature is invalid")
                .Returns(false);

                yield return new TestCaseData(
                    TerrainType.Grassland, TerrainShape.Flat, TerrainFeatureType.None,
                    new List<TerrainType>       () { TerrainType.Grassland, TerrainType.Plains },
                    new List<TerrainShape>      () { TerrainShape.Hills, TerrainShape.Flat },
                    new List<TerrainFeatureType>() { TerrainFeatureType.Forest, TerrainFeatureType.None })
                .SetName("All aspects are valid, and template has options not represented by tile")
                .Returns(true);
            }
        }

        #endregion

        #endregion

        #region instance fields and properties

        private Mock<IRecordkeepingCityFactory> MockCityFactory;

        private List<ICity> AllCities = new List<ICity>();

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            AllCities.Clear();

            MockCityFactory = new Mock<IRecordkeepingCityFactory>();
            MockCityFactory.Setup(factory => factory.AllCities).Returns(() => AllCities.AsReadOnly());

            Container.Bind<IRecordkeepingCityFactory>().FromInstance(MockCityFactory.Object);

            Container.Bind<ImprovementValidityLogic>().AsSingle();
        }

        #endregion

        #region tests

        [Test(Description = "IsTemplateValidForTile should return true when the given " +
            "tile has a terrain type, shape, and feature that the template considers " +
            "valid and should return false otherwise")]
        [TestCaseSource("ValidityTestCases")]
        public bool IsTemplateValidForTile_ConsidersTerrain_Shape_AndFeatures(
            TerrainType tileTerrain, TerrainShape tileShape, TerrainFeatureType tileFeature,
            IEnumerable<TerrainType> validTerrains, IEnumerable<TerrainShape> validShapes,
            IEnumerable<TerrainFeatureType> validFeatures
        ){
            var template = BuildTemplate(validTerrains, validShapes, validFeatures);
            var tile = BuildTile(tileTerrain, tileShape, tileFeature);            

            var validityLogic = Container.Resolve<ImprovementValidityLogic>();

            return validityLogic.IsTemplateValidForTile(template, tile);
        }

        [Test(Description = "IsTemplateValidForTile should return false when the " +
            "given tile has a city on it")]
        public void IsTemplateValidForTile_FalseIfTileHasCity() {
            var template = BuildTemplate(
                new List<TerrainType>       () { TerrainType.Grassland },
                new List<TerrainShape>      () { TerrainShape.Flat },
                new List<TerrainFeatureType>() { TerrainFeatureType.Forest }
            );

            var tile = BuildTile(TerrainType.Grassland, TerrainShape.Flat, TerrainFeatureType.Forest);

            BuildCity(tile);

            var validityLogic = Container.Resolve<ImprovementValidityLogic>();

            Assert.IsFalse(validityLogic.IsTemplateValidForTile(template, tile),
                "IsTemplateValidForTile falsely returns true on a tile with a city");
        }
        
        [Test(Description = "IsTemplateValidForTile should throw an ArgumentNullException " +
            "whenever passed a null argument")]
        public void IsTemplateValidForTile_ThrowsOnNullArguments() {
            var template = BuildTemplate(null, null, null);
            var tile = BuildTile(TerrainType.DeepWater, TerrainShape.Flat, TerrainFeatureType.Forest);

            var validityLogic = Container.Resolve<ImprovementValidityLogic>();

            Assert.Throws<ArgumentNullException>(() => validityLogic.IsTemplateValidForTile(null, tile),
                "IsTemplateValidForTile did not throw on a null template argument");

            Assert.Throws<ArgumentNullException>(() => validityLogic.IsTemplateValidForTile(template, null),
                "IsTemplateValidForTile did not throw on a null tile argument");
        }

        #endregion

        #region utilities

        private IMapTile BuildTile(TerrainType terrain, TerrainShape shape, TerrainFeatureType feature) {
            var mockTile = new Mock<IMapTile>();
            mockTile.SetupAllProperties();

            var newTile = mockTile.Object;

            newTile.Terrain = terrain;
            newTile.Shape   = shape;
            newTile.Feature = feature;

            return newTile;
        }

        private IImprovementTemplate BuildTemplate(IEnumerable<TerrainType> validTerrains,
            IEnumerable<TerrainShape> validShapes, IEnumerable<TerrainFeatureType> validFeatures
        ){
            var mockTemplate = new Mock<IImprovementTemplate>();

            mockTemplate.Setup(template => template.ValidTerrains).Returns(validTerrains);
            mockTemplate.Setup(template => template.ValidShapes  ).Returns(validShapes);
            mockTemplate.Setup(template => template.ValidFeatures).Returns(validFeatures);

            return mockTemplate.Object;
        }

        private ICity BuildCity(IMapTile location) {
            var cityMock = new Mock<ICity>();
            cityMock.Setup(city => city.Location).Returns(location);

            var newCity = cityMock.Object;

            AllCities.Add(newCity);

            return newCity;
        }

        #endregion

        #endregion

    }

}
