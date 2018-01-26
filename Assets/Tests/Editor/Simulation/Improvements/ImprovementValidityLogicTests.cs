using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation.Improvements;
using Assets.Simulation.HexMap;
using Assets.Simulation.Cities;

namespace Assets.Tests.Simulation.Improvements {

    [TestFixture]
    public class ImprovementValidityLogicTests : ZenjectUnitTestFixture {

        #region static fields and properties

        #region test cases

        private static IEnumerable ValidityTestCases {
            get {
                yield return new TestCaseData(
                    TerrainType.Grassland, TerrainShape.Flat, TerrainFeature.None,
                    new List<TerrainType>       () { TerrainType.Grassland },
                    new List<TerrainFeature>() { TerrainFeature.None })
                .SetName("All aspects are valid")
                .Returns(true);

                yield return new TestCaseData(
                    TerrainType.Grassland, TerrainShape.Flat, TerrainFeature.None,
                    new List<TerrainType>       () { TerrainType.Plains },
                    new List<TerrainFeature>() { TerrainFeature.None })
                .SetName("Only terrain is invalid")
                .Returns(false);

                yield return new TestCaseData(
                    TerrainType.Grassland, TerrainShape.Flat, TerrainFeature.None,
                    new List<TerrainType>       () { TerrainType.Grassland },
                    new List<TerrainFeature>() { TerrainFeature.Forest })
                .SetName("Only feature is invalid")
                .Returns(false);

                yield return new TestCaseData(
                    TerrainType.Grassland, TerrainShape.Flat, TerrainFeature.None,
                    new List<TerrainType>       () { TerrainType.Grassland, TerrainType.Plains },
                    new List<TerrainFeature>() { TerrainFeature.Forest, TerrainFeature.None })
                .SetName("All aspects are valid, and template has options not represented by tile")
                .Returns(true);
            }
        }

        #endregion

        #endregion

        #region instance fields and properties

        private Mock<ICityFactory> MockCityFactory;

        private List<ICity> AllCities = new List<ICity>();

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            AllCities.Clear();

            MockCityFactory = new Mock<ICityFactory>();
            MockCityFactory.Setup(factory => factory.AllCities).Returns(() => AllCities.AsReadOnly());

            Container.Bind<ICityFactory>().FromInstance(MockCityFactory.Object);

            Container.Bind<ImprovementValidityLogic>().AsSingle();
        }

        #endregion

        #region tests

        [Test(Description = "IsTemplateValidForTile should return true when the given " +
            "tile has a terrain type, shape, and feature that the template considers " +
            "valid and should return false otherwise")]
        [TestCaseSource("ValidityTestCases")]
        public bool IsTemplateValidForTile_ConsidersTerrain_Shape_AndFeatures(
            TerrainType tileTerrain, TerrainShape tileShape, TerrainFeature tileFeature,
            IEnumerable<TerrainType> validTerrains, IEnumerable<TerrainFeature> validFeatures
        ){
            var template = BuildTemplate(validTerrains, validFeatures);
            var tile = BuildTile(tileTerrain, tileShape, tileFeature);            

            var validityLogic = Container.Resolve<ImprovementValidityLogic>();

            return validityLogic.IsTemplateValidForCell(template, tile);
        }

        [Test(Description = "IsTemplateValidForTile should return false when the " +
            "given tile has a city on it")]
        public void IsTemplateValidForTile_FalseIfTileHasCity() {
            var template = BuildTemplate(
                new List<TerrainType>       () { TerrainType.Grassland },
                new List<TerrainFeature>() { TerrainFeature.Forest }
            );

            var tile = BuildTile(TerrainType.Grassland, TerrainShape.Flat, TerrainFeature.Forest);

            BuildCity(tile);

            var validityLogic = Container.Resolve<ImprovementValidityLogic>();

            Assert.IsFalse(validityLogic.IsTemplateValidForCell(template, tile),
                "IsTemplateValidForTile falsely returns true on a tile with a city");
        }
        
        [Test(Description = "IsTemplateValidForTile should throw an ArgumentNullException " +
            "whenever passed a null argument")]
        public void IsTemplateValidForTile_ThrowsOnNullArguments() {
            var template = BuildTemplate(null, null);
            var tile = BuildTile(TerrainType.Desert, TerrainShape.Flat, TerrainFeature.Forest);

            var validityLogic = Container.Resolve<ImprovementValidityLogic>();

            Assert.Throws<ArgumentNullException>(() => validityLogic.IsTemplateValidForCell(null, tile),
                "IsTemplateValidForTile did not throw on a null template argument");

            Assert.Throws<ArgumentNullException>(() => validityLogic.IsTemplateValidForCell(template, null),
                "IsTemplateValidForTile did not throw on a null tile argument");
        }

        [Test(Description = "")]
        public void MissingCliffRequirementTests() {
            throw new NotImplementedException();
        }

        #endregion

        #region utilities

        private IHexCell BuildTile(TerrainType terrain, TerrainShape shape, TerrainFeature feature) {
            var mockTile = new Mock<IHexCell>();
            mockTile.SetupAllProperties();

            var newTile = mockTile.Object;

            newTile.Terrain = terrain;
            newTile.Shape   = shape;
            newTile.Feature = feature;

            return newTile;
        }

        private IImprovementTemplate BuildTemplate(IEnumerable<TerrainType> validTerrains,
            IEnumerable<TerrainFeature> validFeatures
        ){
            var mockTemplate = new Mock<IImprovementTemplate>();

            mockTemplate.Setup(template => template.ValidTerrains).Returns(validTerrains);
            mockTemplate.Setup(template => template.ValidFeatures).Returns(validFeatures);

            return mockTemplate.Object;
        }

        private ICity BuildCity(IHexCell location) {
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
