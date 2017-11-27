using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation;

using Assets.Simulation.GameMap;

using Assets.Simulation.Cities;
using Assets.Simulation.Cities.Distribution;
using Assets.Simulation.Cities.Territory;
using Assets.Simulation.Cities.ResourceGeneration;

namespace Assets.Tests.Cities {

    [TestFixture]
    public class BorderExpansionLogicTests : ZenjectUnitTestFixture {

        private Mock<IMapHexGrid> MockHexGrid;
        private Mock<ITilePossessionCanon> MockPossessionCanon;
        private Mock<IBorderExpansionConfig> MockConfig;
        private Mock<IResourceGenerationLogic> MockResourceGenerationLogic;

        [SetUp]
        public void CommonInstall() {
            MockHexGrid                 = new Mock<IMapHexGrid>();
            MockPossessionCanon         = new Mock<ITilePossessionCanon>();
            MockConfig                  = new Mock<IBorderExpansionConfig>();
            MockResourceGenerationLogic = new Mock<IResourceGenerationLogic>();

            Container.Bind<IMapHexGrid>()             .FromInstance(MockHexGrid.Object);
            Container.Bind<ITilePossessionCanon>()    .FromInstance(MockPossessionCanon.Object);
            Container.Bind<IBorderExpansionConfig>()  .FromInstance(MockConfig.Object);
            Container.Bind<IResourceGenerationLogic>().FromInstance(MockResourceGenerationLogic.Object);

            Container.Bind<BorderExpansionLogic>().AsSingle();
        }

        [Test(Description = "When a city expresses a preference for a particular resource type, " + 
            "GetNextTileToPursue should return a tile that maximizes yield on that resource for that city")]
        public void GetNextTileToPursue_FocusedCityMaximization() {
            var homeTileMock       = new Mock<IMapTile>();
            var neutralTileOneMock = new Mock<IMapTile>();
            var neutralTileTwoMock = new Mock<IMapTile>();

            homeTileMock      .Name = "Home Tile";
            neutralTileOneMock.Name = "Neutral Tile One";
            neutralTileTwoMock.Name = "Neutral Tile Two";

            var homeTile       = homeTileMock      .Object;
            var neutralTileOne = neutralTileOneMock.Object;
            var neutralTileTwo = neutralTileTwoMock.Object;

            var tiles = new List<IMapTile>() { homeTile, neutralTileOne, neutralTileTwo };

            var cityMock = new Mock<ICity>();
            cityMock.SetupAllProperties();
            cityMock.SetupGet(city => city.Location).Returns(homeTile);

            var homeCity = cityMock.Object;

            homeCity.DistributionPreferences = new DistributionPreferences(true, ResourceType.Food);

            MockHexGrid.Setup(grid => grid.GetDistance(It.IsAny<IMapTile>(), It.IsAny<IMapTile>())).Returns(0);
            MockHexGrid.Setup(grid => grid.GetNeighbors(It.IsAny<IMapTile>())).Returns(tiles);

            MockResourceGenerationLogic.Setup(
                logic => logic.GetYieldOfSlotForCity(neutralTileOne.WorkerSlot, homeCity)
            ).Returns(new ResourceSummary(food: 2));

            MockResourceGenerationLogic.Setup(
                logic => logic.GetYieldOfSlotForCity(neutralTileTwo.WorkerSlot, homeCity)
            ).Returns(new ResourceSummary(food: 3));

            MockPossessionCanon.Setup(canon => canon.GetTilesOfCity(homeCity)).Returns(new List<IMapTile>() { homeTile });
            MockPossessionCanon.Setup(canon => canon.GetCityOfTile(homeTile)).Returns(homeCity);

            var expansionLogic = Container.Resolve<BorderExpansionLogic>();

            Assert.AreEqual(neutralTileTwo, expansionLogic.GetNextTileToPursue(homeCity), 
                "GetNextTileToPursue did not return the tile that maximizes food");
        }

        [Test(Description = "When a city does not express a preference for a particular resource type, " +
            "GetNextTileToPursue should return a tile that maximizes total yield for that city")]
        public void GetNextTileToPursue_UnfocusedCityMaximization() {
            var homeTileMock       = new Mock<IMapTile>();
            var neutralTileOneMock = new Mock<IMapTile>();
            var neutralTileTwoMock = new Mock<IMapTile>();

            homeTileMock      .Name = "Home Tile";
            neutralTileOneMock.Name = "Neutral Tile One";
            neutralTileTwoMock.Name = "Neutral Tile Two";

            var homeTile       = homeTileMock      .Object;
            var neutralTileOne = neutralTileOneMock.Object;
            var neutralTileTwo = neutralTileTwoMock.Object;

            var tiles = new List<IMapTile>() { homeTile, neutralTileOne, neutralTileTwo };

            var cityMock = new Mock<ICity>();
            cityMock.SetupAllProperties();
            cityMock.SetupGet(city => city.Location).Returns(homeTile);

            var homeCity = cityMock.Object;

            homeCity.DistributionPreferences = new DistributionPreferences(false, ResourceType.Food);

            MockHexGrid.Setup(grid => grid.GetDistance(It.IsAny<IMapTile>(), It.IsAny<IMapTile>())).Returns(0);
            MockHexGrid.Setup(grid => grid.GetNeighbors(It.IsAny<IMapTile>())).Returns(tiles);

            MockResourceGenerationLogic.Setup(
                logic => logic.GetYieldOfSlotForCity(neutralTileOne.WorkerSlot, homeCity)
            ).Returns(new ResourceSummary(food: 2, production: 2));

            MockResourceGenerationLogic.Setup(
                logic => logic.GetYieldOfSlotForCity(neutralTileTwo.WorkerSlot, homeCity)
            ).Returns(new ResourceSummary(food: 3));

            MockPossessionCanon.Setup(canon => canon.GetTilesOfCity(homeCity)).Returns(new List<IMapTile>() { homeTile });
            MockPossessionCanon.Setup(canon => canon.GetCityOfTile(homeTile)).Returns(homeCity);

            var expansionLogic = Container.Resolve<BorderExpansionLogic>();

            Assert.AreEqual(neutralTileOne, expansionLogic.GetNextTileToPursue(homeCity), 
                "GetNextTileToPursue did not return the tile that maximizes food and is also available");
        }

        [Test(Description = "GetNextTileToPursue should only ever return a tile that is available, " +
            "even if it doesn't maximize yield")]
        public void GetNextTileToPursue_IsAlwaysAvailable() {
            var homeTileMock       = new Mock<IMapTile>();
            var neutralTileOneMock = new Mock<IMapTile>();
            var neutralTileTwoMock = new Mock<IMapTile>();

            homeTileMock      .Name = "Home Tile";
            neutralTileOneMock.Name = "Neutral Tile One";
            neutralTileTwoMock.Name = "Neutral Tile Two";

            var homeTile       = homeTileMock      .Object;
            var neutralTileOne = neutralTileOneMock.Object;
            var neutralTileTwo = neutralTileTwoMock.Object;

            var tiles = new List<IMapTile>() { homeTile, neutralTileOne, neutralTileTwo };

            var cityMock = new Mock<ICity>();
            cityMock.SetupAllProperties();
            cityMock.SetupGet(city => city.Location).Returns(homeTile);

            var homeCity = cityMock.Object;

            homeCity.DistributionPreferences = new DistributionPreferences(true, ResourceType.Food);

            MockHexGrid.Setup(grid => grid.GetDistance(It.IsAny<IMapTile>(), It.IsAny<IMapTile>())).Returns(0);
            MockHexGrid.Setup(grid => grid.GetNeighbors(It.IsAny<IMapTile>())).Returns(tiles);

            MockResourceGenerationLogic.Setup(
                logic => logic.GetYieldOfSlotForCity(homeTile.WorkerSlot, homeCity)
            ).Returns(new ResourceSummary(food: 4));

            MockResourceGenerationLogic.Setup(
                logic => logic.GetYieldOfSlotForCity(neutralTileOne.WorkerSlot, homeCity)
            ).Returns(new ResourceSummary(food: 2));

            MockResourceGenerationLogic.Setup(
                logic => logic.GetYieldOfSlotForCity(neutralTileTwo.WorkerSlot, homeCity)
            ).Returns(new ResourceSummary(food: 3));

            MockPossessionCanon.Setup(canon => canon.GetTilesOfCity(homeCity)).Returns(new List<IMapTile>() { homeTile });
            MockPossessionCanon.Setup(canon => canon.GetCityOfTile(homeTile)).Returns(homeCity);

            var expansionLogic = Container.Resolve<BorderExpansionLogic>();

            Assert.AreEqual(neutralTileTwo, expansionLogic.GetNextTileToPursue(homeCity), 
                "GetNextTileToPursue did not return the tile that maximizes food and is also available");
        }

        [Test(Description = "A tile should not be available if it is beyond the max range " +
            "specified in the logic's Config")]
        public void IsTileAvailable_MustBeWithinMaxRange() {
            var centerTile = new Mock<IMapTile>().Object;
            var nearTile = new Mock<IMapTile>().Object;
            var farTile = new Mock<IMapTile>().Object;

            var mockCity = new Mock<ICity>();
            mockCity.SetupGet(city => city.Location).Returns(centerTile);
            var homeCity = mockCity.Object;

            MockPossessionCanon.Setup(canon => canon.GetCityOfTile(centerTile)).Returns(homeCity);

            MockConfig.SetupGet(config => config.MaxRange).Returns(2);

            MockHexGrid.Setup(grid => grid.GetNeighbors(nearTile)).Returns(new List<IMapTile>(){ centerTile });
            MockHexGrid.Setup(grid => grid.GetNeighbors(farTile)).Returns(new List<IMapTile>(){ centerTile });

            MockHexGrid.Setup(grid => grid.GetDistance(centerTile, nearTile)).Returns(2);
            MockHexGrid.Setup(grid => grid.GetDistance(nearTile, centerTile)).Returns(2);

            MockHexGrid.Setup(grid => grid.GetDistance(centerTile, farTile)).Returns(3);
            MockHexGrid.Setup(grid => grid.GetDistance(farTile, centerTile)).Returns(3);

            var expansionLogic = Container.Resolve<BorderExpansionLogic>();

            Assert.IsTrue(expansionLogic.IsTileAvailable(homeCity, nearTile),
                "NearTile was falsely flagged as unavailable for expansion");

            Assert.IsFalse(expansionLogic.IsTileAvailable(homeCity, farTile),
                "FarTile was falsely flagged as available for expansion");
        }

        [Test(Description = "A tile should not be available if it is already owned by a city")]
        public void IsTileAvailable_MustBeUnowned() {
            var homeTile = new Mock<IMapTile>().Object;
            var neutralTile = new Mock<IMapTile>().Object;
            var foreignTile = new Mock<IMapTile>().Object;

            var mockCity = new Mock<ICity>();
            mockCity.SetupGet(city => city.Location).Returns(homeTile);

            var homeCity = mockCity.Object;
            var foreignCity = mockCity.Object;

            MockPossessionCanon.Setup(canon => canon.GetCityOfTile(homeTile)).Returns(homeCity);
            MockPossessionCanon.Setup(canon => canon.GetCityOfTile(foreignTile)).Returns(foreignCity);

            MockHexGrid.Setup(grid => grid.GetNeighbors(neutralTile)).Returns(new List<IMapTile>(){ homeTile });
            MockHexGrid.Setup(grid => grid.GetNeighbors(foreignTile)).Returns(new List<IMapTile>(){ homeTile });

            var expansionLogic = Container.Resolve<BorderExpansionLogic>();

            Assert.IsTrue(expansionLogic.IsTileAvailable(homeCity, neutralTile),
                "NeutralTile was falsely flagged as unavailable for expansion");

            Assert.IsFalse(expansionLogic.IsTileAvailable(homeCity, foreignTile),
                "ForeignTile was falsely flagged as available for expansion");
        }

        [Test(Description = "A tile should not be available if it isn't adjacent to a tile " +
            "the city has already claimed")]
        public void IsTileAvailable_MustBeAdjacentToTerritory() {
            var ownedTile = new Mock<IMapTile>().Object;
            var neighboringTile = new Mock<IMapTile>().Object;
            var nonNeighboringTile = new Mock<IMapTile>().Object;

            var mockCity = new Mock<ICity>();
            mockCity.SetupGet(city => city.Location).Returns(ownedTile);

            var owningCity = mockCity.Object;

            MockPossessionCanon.Setup(canon => canon.GetCityOfTile(ownedTile)).Returns(owningCity);

            MockConfig.SetupGet(config => config.MaxRange).Returns(2);

            MockHexGrid.Setup(grid => grid.GetNeighbors(neighboringTile)).Returns(new List<IMapTile>(){ ownedTile });
            MockHexGrid.Setup(grid => grid.GetNeighbors(nonNeighboringTile)).Returns(new List<IMapTile>());

            var expansionLogic = Container.Resolve<BorderExpansionLogic>();

            Assert.IsTrue(expansionLogic.IsTileAvailable(owningCity, neighboringTile),
                "NeighboringTile was falsely flagged as unavailable for expansion");

            Assert.IsFalse(expansionLogic.IsTileAvailable(owningCity, nonNeighboringTile),
                "NonNeighboringTile was falsely flagged as available for expansion");
        }

        [Test(Description = "GetCultureCostOfAcquiringTile should respond to Config and " +
            "operate under the following equation, where t is the current number of tiles:\n\n" +
            "\tTileCostBase + (PreviousTileCountCoefficient * (t - 1)) ^ PreviousTileCountExponen\n\n" + 
            "The returned value should be rounded down")]
        public void GetCultureCostOfAcquiringTile_FollowsEquationAndConfig() {
            var city = new Mock<ICity>().Object;
            var newTile = new Mock<IMapTile>().Object;

            MockPossessionCanon.Setup(canon => canon.GetTilesOfCity(city)).Returns(new List<IMapTile>() {
               new Mock<IMapTile>().Object,
               new Mock<IMapTile>().Object,
               new Mock<IMapTile>().Object,
               new Mock<IMapTile>().Object,
               new Mock<IMapTile>().Object,
               new Mock<IMapTile>().Object,
            });

            MockConfig.SetupGet(config => config.TileCostBase).Returns(20);
            MockConfig.SetupGet(config => config.PreviousTileCountCoefficient).Returns(10);
            MockConfig.SetupGet(config => config.PreviousTileCountExponent).Returns(1.1f);

            var logic = Container.Resolve<BorderExpansionLogic>();

            Assert.AreEqual(93, logic.GetCultureCostOfAcquiringTile(city, newTile),
                "GetCultureCostOfAcquiringTile returned an incorrect value on a city with 6 tiles");
        }

        [Test(Description = "GetGoldCostOfAcquiringTile should return the same value as " +
            "GetCultureCostOfAcquiringTile")]
        public void GetGoldCostOfAcquiringTile_IdenticalToCultureCost() {
            var city = new Mock<ICity>().Object;
            var newTile = new Mock<IMapTile>().Object;

            MockPossessionCanon.Setup(canon => canon.GetTilesOfCity(city)).Returns(new List<IMapTile>() {
               new Mock<IMapTile>().Object,
               new Mock<IMapTile>().Object,
               new Mock<IMapTile>().Object,
               new Mock<IMapTile>().Object,
               new Mock<IMapTile>().Object,
               new Mock<IMapTile>().Object,
            });

            MockConfig.SetupGet(config => config.TileCostBase).Returns(20);
            MockConfig.SetupGet(config => config.PreviousTileCountCoefficient).Returns(10);
            MockConfig.SetupGet(config => config.PreviousTileCountExponent).Returns(1.1f);

            var logic = Container.Resolve<BorderExpansionLogic>();

            Assert.AreEqual(93, logic.GetGoldCostOfAcquiringTile(city, newTile),
                "GetCultureCostOfAcquiringTile returned an incorrect value on a city with 6 tiles");
        }

        [Test(Description = "GetAllTilesAvailableToCity should return all tiles for which " +
            "IsTileAvailable is true and no others")]
        public void GetAllTilesAvailableToCity_ReturnsAllAndOnlyAllCorrectTiles() {
            var homeTileMock                      = new Mock<IMapTile>(); 
            var nearNeighboringNeutralTileMock    = new Mock<IMapTile>(); 
            var farNeighboringNeutralTileMock     = new Mock<IMapTile>(); 
            var nearNonNeighboringNeutralTileMock = new Mock<IMapTile>(); 
            var nearNeighboringForeignTileMock    = new Mock<IMapTile>(); 

            homeTileMock.Name                      = "Home Tile";
            nearNeighboringNeutralTileMock.Name    = "Near Neighboring Neutral Tile";
            farNeighboringNeutralTileMock.Name     = "Far Neighboring Neutral Tile";
            nearNonNeighboringNeutralTileMock.Name = "Near Non Neighboring Neutral Tile";
            nearNeighboringForeignTileMock.Name    = "Near Neighboring Foreign Tile";

            var homeTile                      = homeTileMock                     .Object;
            var nearNeighboringNeutralTile    = nearNeighboringNeutralTileMock   .Object;
            var farNeighboringNeutralTile     = farNeighboringNeutralTileMock    .Object;
            var nearNonNeighboringNeutralTile = nearNonNeighboringNeutralTileMock.Object;
            var nearNeighboringForeignTile    = nearNeighboringForeignTileMock   .Object;

            var homeCityMock    = new Mock<ICity>();
            homeCityMock.Name = "Home City";
            homeCityMock.SetupGet(city => city.Location).Returns(homeTile);

            var foreignCityMock = new Mock<ICity>();
            foreignCityMock.Name = "Foreign City";

            MockPossessionCanon.Setup(canon => canon.GetTilesOfCity(homeCityMock.Object)).Returns(new List<IMapTile>() { homeTile });

            MockPossessionCanon.Setup(canon => canon.GetCityOfTile(homeTile)).Returns(homeCityMock.Object);
            MockPossessionCanon.Setup(canon => canon.GetCityOfTile(nearNeighboringForeignTile)).Returns(foreignCityMock.Object);

            MockHexGrid.Setup(grid => grid.GetNeighbors(homeTile)).Returns(
                new List<IMapTile>() { nearNeighboringNeutralTile, farNeighboringNeutralTile, nearNeighboringForeignTile }
            );
            MockHexGrid.Setup(grid => grid.GetNeighbors(nearNeighboringNeutralTile)).Returns(new List<IMapTile>() { homeTile });
            MockHexGrid.Setup(grid => grid.GetNeighbors(farNeighboringNeutralTile)).Returns(new List<IMapTile>() { homeTile });
            MockHexGrid.Setup(grid => grid.GetNeighbors(nearNeighboringForeignTile)).Returns(new List<IMapTile>() { homeTile });

            MockHexGrid.Setup(grid => grid.GetDistance(homeTile, nearNeighboringNeutralTile)).Returns(2);
            MockHexGrid.Setup(grid => grid.GetDistance(nearNeighboringNeutralTile, homeTile)).Returns(2);

            MockHexGrid.Setup(grid => grid.GetDistance(homeTile, farNeighboringNeutralTile)).Returns(3);
            MockHexGrid.Setup(grid => grid.GetDistance(farNeighboringNeutralTile, homeTile)).Returns(3);

            MockHexGrid.Setup(grid => grid.GetDistance(homeTile, nearNonNeighboringNeutralTile)).Returns(2);
            MockHexGrid.Setup(grid => grid.GetDistance(nearNonNeighboringNeutralTile, homeTile)).Returns(2);

            MockHexGrid.Setup(grid => grid.GetDistance(homeTile, nearNeighboringForeignTile)).Returns(2);
            MockHexGrid.Setup(grid => grid.GetDistance(nearNeighboringForeignTile, homeTile)).Returns(2);

            MockConfig.SetupGet(config => config.MaxRange).Returns(2);

            var expansionLogic = Container.Resolve<BorderExpansionLogic>();

            var availableTiles = expansionLogic.GetAllTilesAvailableToCity(homeCityMock.Object);

            Assert.IsFalse(availableTiles.Contains(homeTile),                      "HomeTile is falsely considered an available tile");
            Assert.IsTrue (availableTiles.Contains(nearNeighboringNeutralTile),    "nearNeighboringNeutralTile is not considered an available tile");
            Assert.IsFalse(availableTiles.Contains(farNeighboringNeutralTile),     "farNeighboringNeutralTile is falsely considered an available tile");
            Assert.IsFalse(availableTiles.Contains(nearNonNeighboringNeutralTile), "nearNonNeighboringNeutralTile is falsely considered an available tile");
            Assert.IsFalse(availableTiles.Contains(nearNeighboringForeignTile),    "nearNeighboringForeignTile is falsely considered an available tile");
        }

        [Test(Description = "All methods of BorderExpansionLogic should throw an ArgumentNullException " +
            " on any null argument")]
        public void AllMethods_ThrowExceptionsOnNullArguments() {
            var logic = Container.Resolve<BorderExpansionLogic>();

            var testTile = new Mock<IMapTile>().Object;
            var testCity = new Mock<ICity>().Object;

            Assert.Throws<ArgumentNullException>(() => logic.GetNextTileToPursue(null),
                "GetNextTileToPursue failed to throw on a null city argument");

            Assert.Throws<ArgumentNullException>(() => logic.IsTileAvailable(null, testTile),
                "IsTileAvailable failed to throw on a null city argument");

            Assert.Throws<ArgumentNullException>(() => logic.IsTileAvailable(testCity, null),
                "IsTileAvailable failed to throw on a null tile argument");

            Assert.Throws<ArgumentNullException>(() => logic.GetCultureCostOfAcquiringTile(null, testTile),
                "GetCultureCostOfAcquiringTile failed to throw on a null city argument");

            Assert.Throws<ArgumentNullException>(() => logic.GetCultureCostOfAcquiringTile(testCity, null),
                "GetCultureCostOfAcquiringTile failed to throw on a null tile argument");

            Assert.Throws<ArgumentNullException>(() => logic.GetGoldCostOfAcquiringTile(null, testTile),
                "GetGoldCostOfAcquiringTile failed to throw on a null city argument");

            Assert.Throws<ArgumentNullException>(() => logic.GetGoldCostOfAcquiringTile(testCity, null),
                "GetGoldCostOfAcquiringTile failed to throw on a null tile argument");
        }

    }

}
