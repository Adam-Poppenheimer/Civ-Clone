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
                    TerrainType.Grassland, TerrainFeature.None,
                    new List<TerrainType>       () { TerrainType.Grassland },
                    new List<TerrainFeature>() { TerrainFeature.None })
                .SetName("All aspects are valid")
                .Returns(true);

                yield return new TestCaseData(
                    TerrainType.Grassland, TerrainFeature.None,
                    new List<TerrainType>       () { TerrainType.Plains },
                    new List<TerrainFeature>() { TerrainFeature.None })
                .SetName("Only terrain is invalid")
                .Returns(false);

                yield return new TestCaseData(
                    TerrainType.Grassland, TerrainFeature.None,
                    new List<TerrainType>       () { TerrainType.Grassland },
                    new List<TerrainFeature>() { TerrainFeature.Forest })
                .SetName("Only feature is invalid")
                .Returns(false);

                yield return new TestCaseData(
                    TerrainType.Grassland, TerrainFeature.None,
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

        private Mock<IHexGrid> MockGrid;

        private List<ICity> AllCities = new List<ICity>();

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            AllCities.Clear();

            MockCityFactory = new Mock<ICityFactory>();
            MockGrid        = new Mock<IHexGrid>();

            MockCityFactory.Setup(factory => factory.AllCities).Returns(() => AllCities.AsReadOnly());

            Container.Bind<ICityFactory>().FromInstance(MockCityFactory.Object);
            Container.Bind<IHexGrid>    ().FromInstance(MockGrid       .Object);

            Container.Bind<ImprovementValidityLogic>().AsSingle();
        }

        #endregion

        #region tests

        [Test(Description = "IsTemplateValidForTile should return true when the given " +
            "tile has a terrain type, shape, and feature that the template considers " +
            "valid and should return false otherwise")]
        [TestCaseSource("ValidityTestCases")]
        public bool IsTemplateValidForTile_ConsidersTerrainAndFeatures(
            TerrainType tileTerrain, TerrainFeature tileFeature,
            IEnumerable<TerrainType> validTerrains, IEnumerable<TerrainFeature> validFeatures
        ){
            var template = BuildTemplate(validTerrains, validFeatures);
            var tile = BuildCell(tileTerrain, tileFeature);            

            var validityLogic = Container.Resolve<ImprovementValidityLogic>();

            return validityLogic.IsTemplateValidForCell(template, tile);
        }

        [Test(Description = "IsTemplateValidForTile should return false when the " +
            "given tile has a city on it")]
        public void IsTemplateValidForTile_FalseIfTileHasCity() {
            var template = BuildTemplate(
                new List<TerrainType>   () { TerrainType.Grassland },
                new List<TerrainFeature>() { TerrainFeature.Forest }
            );

            var tile = BuildCell(TerrainType.Grassland, TerrainFeature.Forest);

            BuildCity(tile);

            var validityLogic = Container.Resolve<ImprovementValidityLogic>();

            Assert.IsFalse(validityLogic.IsTemplateValidForCell(template, tile),
                "IsTemplateValidForTile falsely returns true on a tile with a city");
        }
        
        [Test(Description = "IsTemplateValidForTile should throw an ArgumentNullException " +
            "whenever passed a null argument")]
        public void IsTemplateValidForTile_ThrowsOnNullArguments() {
            var template = BuildTemplate(null, null);
            var tile = BuildCell(TerrainType.Desert, TerrainFeature.Forest);

            var validityLogic = Container.Resolve<ImprovementValidityLogic>();

            Assert.Throws<ArgumentNullException>(() => validityLogic.IsTemplateValidForCell(null, tile),
                "IsTemplateValidForTile did not throw on a null template argument");

            Assert.Throws<ArgumentNullException>(() => validityLogic.IsTemplateValidForCell(template, null),
                "IsTemplateValidForTile did not throw on a null tile argument");
        }

        [Test(Description = "IsTemplateValidForTile should return false if the template " +
            "requires cliffs and the cell isn't at the bottom of one")]
        public void IsTemplateValidForTile_ProperlyConsidersCliffRequirements() {
            var template = BuildTemplate(
                new List<TerrainType>   () { TerrainType.Grassland },
                new List<TerrainFeature>() { TerrainFeature.Forest },
                true
            );

            var cellOne = BuildCell(TerrainType.Grassland, TerrainFeature.Forest);

            var cellTwo = BuildCell(TerrainType.Grassland, TerrainFeature.None, 2);

            MockGrid.Setup(grid => grid.GetNeighbors(cellOne)).Returns(new List<IHexCell>() { cellTwo });
            MockGrid.Setup(grid => grid.GetNeighbors(cellTwo)).Returns(new List<IHexCell>() { cellOne });

            var validityLogic = Container.Resolve<ImprovementValidityLogic>();

            Assert.IsTrue(
                validityLogic.IsTemplateValidForCell(template, cellOne),
                "Logic fails to permit a cliff-requiring improvement on cellOne"
            );

            Assert.IsFalse(
                validityLogic.IsTemplateValidForCell(template, cellTwo),
                "Logic falsely permits a cliff-requiring improvement on cellTwo"
            );
        }

        #endregion

        #region utilities

        private IHexCell BuildCell(TerrainType terrain, TerrainFeature feature, int elevation = 0) {
            var mockCell = new Mock<IHexCell>();
            mockCell.SetupAllProperties();

            var newcell = mockCell.Object;

            newcell.Terrain   = terrain;
            newcell.Feature   = feature;
            newcell.Elevation = elevation;

            return newcell;
        }

        private IImprovementTemplate BuildTemplate(
            IEnumerable<TerrainType> validTerrains,
            IEnumerable<TerrainFeature> validFeatures,
            bool requiresAdjacentUpwardCliff = false
        ){
            var mockTemplate = new Mock<IImprovementTemplate>();

            mockTemplate.Setup(template => template.ValidTerrains).Returns(validTerrains);
            mockTemplate.Setup(template => template.ValidFeatures).Returns(validFeatures);
            
            mockTemplate.Setup(template => template.RequiresAdjacentUpwardCliff).Returns(requiresAdjacentUpwardCliff);

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
