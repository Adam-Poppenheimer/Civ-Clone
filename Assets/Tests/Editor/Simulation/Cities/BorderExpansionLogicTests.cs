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

using Assets.Simulation.Cities;
using Assets.Simulation.Cities.Distribution;
using Assets.Simulation.Cities.Territory;
using Assets.Simulation.Cities.ResourceGeneration;

namespace Assets.Tests.Simulation.Cities {

    [TestFixture]
    public class BorderExpansionLogicTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IHexGrid>                                 MockHexGrid;
        private Mock<IPossessionRelationship<ICity, IHexCell>> MockTerritoryPossessionCanon;
        private Mock<ICityConfig>                              MockConfig;
        private Mock<IResourceGenerationLogic>                 MockResourceGenerationLogic;
        private Mock<IPossessionRelationship<IHexCell, ICity>> MockCityLocationCanon;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockHexGrid                  = new Mock<IHexGrid>();
            MockTerritoryPossessionCanon = new Mock<IPossessionRelationship<ICity, IHexCell>>();
            MockConfig                   = new Mock<ICityConfig>();
            MockResourceGenerationLogic  = new Mock<IResourceGenerationLogic>();
            MockCityLocationCanon        = new Mock<IPossessionRelationship<IHexCell, ICity>>();

            Container.Bind<IHexGrid>                                ().FromInstance(MockHexGrid                 .Object);
            Container.Bind<IPossessionRelationship<ICity, IHexCell>>().FromInstance(MockTerritoryPossessionCanon.Object);
            Container.Bind<ICityConfig>                             ().FromInstance(MockConfig                  .Object);
            Container.Bind<IResourceGenerationLogic>                ().FromInstance(MockResourceGenerationLogic .Object);
            Container.Bind<IPossessionRelationship<IHexCell, ICity>>().FromInstance(MockCityLocationCanon       .Object);

            Container.Bind<BorderExpansionLogic>().AsSingle();
        }

        #endregion

        #region tests

        [Test(Description = "When a city expresses a preference for a particular resource type, " + 
            "GetNextTileToPursue should return a tile that maximizes yield on that resource for that city")]
        public void GetNextTileToPursue_FocusedCityMaximization() {
            var homeTileMock       = new Mock<IHexCell>();
            var neutralTileOneMock = new Mock<IHexCell>();
            var neutralTileTwoMock = new Mock<IHexCell>();

            homeTileMock      .Name = "Home Tile";
            neutralTileOneMock.Name = "Neutral Tile One";
            neutralTileTwoMock.Name = "Neutral Tile Two";

            var homeTile       = homeTileMock      .Object;
            var neutralTileOne = neutralTileOneMock.Object;
            var neutralTileTwo = neutralTileTwoMock.Object;

            var tiles = new List<IHexCell>() { homeTile, neutralTileOne, neutralTileTwo };

            var cityMock = new Mock<ICity>();
            cityMock.SetupAllProperties();

            var homeCity = cityMock.Object;

            MockCityLocationCanon.Setup(canon => canon.GetOwnerOfPossession(homeCity)).Returns(homeTile);

            homeCity.ResourceFocus = ResourceFocusType.Food;

            MockHexGrid.Setup(grid => grid.GetDistance(It.IsAny<IHexCell>(), It.IsAny<IHexCell>())).Returns(0);
            MockHexGrid.Setup(grid => grid.GetNeighbors(It.IsAny<IHexCell>())).Returns(tiles);

            MockResourceGenerationLogic.Setup(
                logic => logic.GetYieldOfSlotForCity(neutralTileOne.WorkerSlot, homeCity)
            ).Returns(new ResourceSummary(food: 2));

            MockResourceGenerationLogic.Setup(
                logic => logic.GetYieldOfSlotForCity(neutralTileTwo.WorkerSlot, homeCity)
            ).Returns(new ResourceSummary(food: 3));

            MockTerritoryPossessionCanon.Setup(canon => canon.GetPossessionsOfOwner(homeCity)).Returns(new List<IHexCell>() { homeTile });
            MockTerritoryPossessionCanon.Setup(canon => canon.GetOwnerOfPossession(homeTile)).Returns(homeCity);

            var expansionLogic = Container.Resolve<BorderExpansionLogic>();

            Assert.AreEqual(neutralTileTwo, expansionLogic.GetNextCellToPursue(homeCity), 
                "GetNextTileToPursue did not return the tile that maximizes food");
        }

        [Test(Description = "When a city does not express a preference for a particular resource type, " +
            "GetNextTileToPursue should return a tile that maximizes total yield for that city")]
        public void GetNextTileToPursue_UnfocusedCityMaximization() {
            var homeTileMock       = new Mock<IHexCell>();
            var neutralTileOneMock = new Mock<IHexCell>();
            var neutralTileTwoMock = new Mock<IHexCell>();

            homeTileMock      .Name = "Home Tile";
            neutralTileOneMock.Name = "Neutral Tile One";
            neutralTileTwoMock.Name = "Neutral Tile Two";

            var homeTile       = homeTileMock      .Object;
            var neutralTileOne = neutralTileOneMock.Object;
            var neutralTileTwo = neutralTileTwoMock.Object;

            var tiles = new List<IHexCell>() { homeTile, neutralTileOne, neutralTileTwo };

            var cityMock = new Mock<ICity>();
            cityMock.SetupAllProperties();

            var homeCity = cityMock.Object;

            MockCityLocationCanon.Setup(canon => canon.GetOwnerOfPossession(homeCity)).Returns(homeTile);

            homeCity.ResourceFocus = ResourceFocusType.TotalYield;

            MockHexGrid.Setup(grid => grid.GetDistance(It.IsAny<IHexCell>(), It.IsAny<IHexCell>())).Returns(0);
            MockHexGrid.Setup(grid => grid.GetNeighbors(It.IsAny<IHexCell>())).Returns(tiles);

            MockResourceGenerationLogic.Setup(
                logic => logic.GetYieldOfSlotForCity(neutralTileOne.WorkerSlot, homeCity)
            ).Returns(new ResourceSummary(food: 2, production: 2));

            MockResourceGenerationLogic.Setup(
                logic => logic.GetYieldOfSlotForCity(neutralTileTwo.WorkerSlot, homeCity)
            ).Returns(new ResourceSummary(food: 3));

            MockTerritoryPossessionCanon.Setup(canon => canon.GetPossessionsOfOwner(homeCity)).Returns(new List<IHexCell>() { homeTile });
            MockTerritoryPossessionCanon.Setup(canon => canon.GetOwnerOfPossession(homeTile)).Returns(homeCity);

            var expansionLogic = Container.Resolve<BorderExpansionLogic>();

            Assert.AreEqual(neutralTileOne, expansionLogic.GetNextCellToPursue(homeCity), 
                "GetNextTileToPursue did not return the tile that maximizes food and is also available");
        }

        [Test(Description = "GetNextTileToPursue should only ever return a tile that is available, " +
            "even if it doesn't maximize yield")]
        public void GetNextTileToPursue_IsAlwaysAvailable() {
            var homeTileMock       = new Mock<IHexCell>();
            var neutralTileOneMock = new Mock<IHexCell>();
            var neutralTileTwoMock = new Mock<IHexCell>();

            homeTileMock      .Name = "Home Tile";
            neutralTileOneMock.Name = "Neutral Tile One";
            neutralTileTwoMock.Name = "Neutral Tile Two";

            var homeTile       = homeTileMock      .Object;
            var neutralTileOne = neutralTileOneMock.Object;
            var neutralTileTwo = neutralTileTwoMock.Object;

            var tiles = new List<IHexCell>() { homeTile, neutralTileOne, neutralTileTwo };

            var cityMock = new Mock<ICity>();
            cityMock.SetupAllProperties();

            var homeCity = cityMock.Object;

            MockCityLocationCanon.Setup(canon => canon.GetOwnerOfPossession(homeCity)).Returns(homeTile);

            homeCity.ResourceFocus = ResourceFocusType.Food;

            MockHexGrid.Setup(grid => grid.GetDistance(It.IsAny<IHexCell>(), It.IsAny<IHexCell>())).Returns(0);
            MockHexGrid.Setup(grid => grid.GetNeighbors(It.IsAny<IHexCell>())).Returns(tiles);

            MockResourceGenerationLogic.Setup(
                logic => logic.GetYieldOfSlotForCity(homeTile.WorkerSlot, homeCity)
            ).Returns(new ResourceSummary(food: 4));

            MockResourceGenerationLogic.Setup(
                logic => logic.GetYieldOfSlotForCity(neutralTileOne.WorkerSlot, homeCity)
            ).Returns(new ResourceSummary(food: 2));

            MockResourceGenerationLogic.Setup(
                logic => logic.GetYieldOfSlotForCity(neutralTileTwo.WorkerSlot, homeCity)
            ).Returns(new ResourceSummary(food: 3));

            MockTerritoryPossessionCanon.Setup(canon => canon.GetPossessionsOfOwner(homeCity)).Returns(new List<IHexCell>() { homeTile });
            MockTerritoryPossessionCanon.Setup(canon => canon.GetOwnerOfPossession(homeTile)).Returns(homeCity);

            var expansionLogic = Container.Resolve<BorderExpansionLogic>();

            Assert.AreEqual(neutralTileTwo, expansionLogic.GetNextCellToPursue(homeCity), 
                "GetNextTileToPursue did not return the tile that maximizes food and is also available");
        }

        [Test(Description = "A tile should not be available if it is beyond the max range " +
            "specified in the logic's Config")]
        public void IsTileAvailable_MustBeWithinMaxRange() {
            var centerTile = new Mock<IHexCell>().Object;
            var nearTile = new Mock<IHexCell>().Object;
            var farTile = new Mock<IHexCell>().Object;

            var mockCity = new Mock<ICity>();
            var homeCity = mockCity.Object;

            MockCityLocationCanon.Setup(canon => canon.GetOwnerOfPossession(homeCity)).Returns(centerTile);

            MockTerritoryPossessionCanon.Setup(canon => canon.GetOwnerOfPossession(centerTile)).Returns(homeCity);

            MockConfig.SetupGet(config => config.MaxBorderRange).Returns(2);

            MockHexGrid.Setup(grid => grid.GetNeighbors(nearTile)).Returns(new List<IHexCell>(){ centerTile });
            MockHexGrid.Setup(grid => grid.GetNeighbors(farTile)).Returns(new List<IHexCell>(){ centerTile });

            MockHexGrid.Setup(grid => grid.GetDistance(centerTile, nearTile)).Returns(2);
            MockHexGrid.Setup(grid => grid.GetDistance(nearTile, centerTile)).Returns(2);

            MockHexGrid.Setup(grid => grid.GetDistance(centerTile, farTile)).Returns(3);
            MockHexGrid.Setup(grid => grid.GetDistance(farTile, centerTile)).Returns(3);

            var expansionLogic = Container.Resolve<BorderExpansionLogic>();

            Assert.IsTrue(expansionLogic.IsCellAvailable(homeCity, nearTile),
                "NearTile was falsely flagged as unavailable for expansion");

            Assert.IsFalse(expansionLogic.IsCellAvailable(homeCity, farTile),
                "FarTile was falsely flagged as available for expansion");
        }

        [Test(Description = "A tile should not be available if it is already owned by a city")]
        public void IsTileAvailable_MustBeUnowned() {
            var homeTile = new Mock<IHexCell>().Object;
            var neutralTile = new Mock<IHexCell>().Object;
            var foreignTile = new Mock<IHexCell>().Object;

            var mockCity = new Mock<ICity>();

            var homeCity = mockCity.Object;
            var foreignCity = mockCity.Object;

            MockCityLocationCanon.Setup(canon => canon.GetOwnerOfPossession(homeCity)).Returns(homeTile);

            MockTerritoryPossessionCanon.Setup(canon => canon.GetOwnerOfPossession(homeTile)).Returns(homeCity);
            MockTerritoryPossessionCanon.Setup(canon => canon.GetOwnerOfPossession(foreignTile)).Returns(foreignCity);

            MockHexGrid.Setup(grid => grid.GetNeighbors(neutralTile)).Returns(new List<IHexCell>(){ homeTile });
            MockHexGrid.Setup(grid => grid.GetNeighbors(foreignTile)).Returns(new List<IHexCell>(){ homeTile });

            var expansionLogic = Container.Resolve<BorderExpansionLogic>();

            Assert.IsTrue(expansionLogic.IsCellAvailable(homeCity, neutralTile),
                "NeutralTile was falsely flagged as unavailable for expansion");

            Assert.IsFalse(expansionLogic.IsCellAvailable(homeCity, foreignTile),
                "ForeignTile was falsely flagged as available for expansion");
        }

        [Test(Description = "A tile should not be available if it isn't adjacent to a tile " +
            "the city has already claimed")]
        public void IsTileAvailable_MustBeAdjacentToTerritory() {
            var ownedTile = new Mock<IHexCell>().Object;
            var neighboringTile = new Mock<IHexCell>().Object;
            var nonNeighboringTile = new Mock<IHexCell>().Object;

            var mockCity = new Mock<ICity>();

            var owningCity = mockCity.Object;

            MockCityLocationCanon.Setup(canon => canon.GetOwnerOfPossession(owningCity)).Returns(ownedTile);

            MockTerritoryPossessionCanon.Setup(canon => canon.GetOwnerOfPossession(ownedTile)).Returns(owningCity);

            MockConfig.SetupGet(config => config.MaxBorderRange).Returns(2);

            MockHexGrid.Setup(grid => grid.GetNeighbors(neighboringTile)).Returns(new List<IHexCell>(){ ownedTile });
            MockHexGrid.Setup(grid => grid.GetNeighbors(nonNeighboringTile)).Returns(new List<IHexCell>());

            var expansionLogic = Container.Resolve<BorderExpansionLogic>();

            Assert.IsTrue(expansionLogic.IsCellAvailable(owningCity, neighboringTile),
                "NeighboringTile was falsely flagged as unavailable for expansion");

            Assert.IsFalse(expansionLogic.IsCellAvailable(owningCity, nonNeighboringTile),
                "NonNeighboringTile was falsely flagged as available for expansion");
        }

        [Test(Description = "GetCultureCostOfAcquiringTile should respond to Config and " +
            "operate under the following equation, where t is the current number of tiles:\n\n" +
            "\tTileCostBase + (PreviousTileCountCoefficient * (t - 1)) ^ PreviousTileCountExponen\n\n" + 
            "The returned value should be rounded down")]
        public void GetCultureCostOfAcquiringTile_FollowsEquationAndConfig() {
            var city = new Mock<ICity>().Object;
            var newTile = new Mock<IHexCell>().Object;

            MockTerritoryPossessionCanon.Setup(canon => canon.GetPossessionsOfOwner(city)).Returns(new List<IHexCell>() {
               new Mock<IHexCell>().Object,
               new Mock<IHexCell>().Object,
               new Mock<IHexCell>().Object,
               new Mock<IHexCell>().Object,
               new Mock<IHexCell>().Object,
               new Mock<IHexCell>().Object,
            });

            MockConfig.SetupGet(config => config.TileCostBase).Returns(20);
            MockConfig.SetupGet(config => config.PreviousTileCountCoefficient).Returns(10);
            MockConfig.SetupGet(config => config.PreviousTileCountExponent).Returns(1.1f);

            var logic = Container.Resolve<BorderExpansionLogic>();

            Assert.AreEqual(93, logic.GetCultureCostOfAcquiringCell(city, newTile),
                "GetCultureCostOfAcquiringTile returned an incorrect value on a city with 6 tiles");
        }

        [Test(Description = "GetGoldCostOfAcquiringTile should return the same value as " +
            "GetCultureCostOfAcquiringTile")]
        public void GetGoldCostOfAcquiringTile_IdenticalToCultureCost() {
            var city = new Mock<ICity>().Object;
            var newTile = new Mock<IHexCell>().Object;

            MockTerritoryPossessionCanon.Setup(canon => canon.GetPossessionsOfOwner(city)).Returns(new List<IHexCell>() {
               new Mock<IHexCell>().Object,
               new Mock<IHexCell>().Object,
               new Mock<IHexCell>().Object,
               new Mock<IHexCell>().Object,
               new Mock<IHexCell>().Object,
               new Mock<IHexCell>().Object,
            });

            MockConfig.SetupGet(config => config.TileCostBase).Returns(20);
            MockConfig.SetupGet(config => config.PreviousTileCountCoefficient).Returns(10);
            MockConfig.SetupGet(config => config.PreviousTileCountExponent).Returns(1.1f);

            var logic = Container.Resolve<BorderExpansionLogic>();

            Assert.AreEqual(93, logic.GetGoldCostOfAcquiringCell(city, newTile),
                "GetCultureCostOfAcquiringTile returned an incorrect value on a city with 6 tiles");
        }

        [Test(Description = "GetAllTilesAvailableToCity should return all tiles for which " +
            "IsTileAvailable is true and no others")]
        public void GetAllTilesAvailableToCity_ReturnsAllAndOnlyAllCorrectTiles() {
            var homeTileMock                      = new Mock<IHexCell>(); 
            var nearNeighboringNeutralTileMock    = new Mock<IHexCell>(); 
            var farNeighboringNeutralTileMock     = new Mock<IHexCell>(); 
            var nearNonNeighboringNeutralTileMock = new Mock<IHexCell>(); 
            var nearNeighboringForeignTileMock    = new Mock<IHexCell>(); 

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

            MockCityLocationCanon.Setup(canon => canon.GetOwnerOfPossession(homeCityMock.Object)).Returns(homeTile);

            var foreignCityMock = new Mock<ICity>();
            foreignCityMock.Name = "Foreign City";

            MockTerritoryPossessionCanon.Setup(canon => canon.GetPossessionsOfOwner(homeCityMock.Object)).Returns(new List<IHexCell>() { homeTile });

            MockTerritoryPossessionCanon.Setup(canon => canon.GetOwnerOfPossession(homeTile)).Returns(homeCityMock.Object);
            MockTerritoryPossessionCanon.Setup(canon => canon.GetOwnerOfPossession(nearNeighboringForeignTile)).Returns(foreignCityMock.Object);

            MockHexGrid.Setup(grid => grid.GetNeighbors(homeTile)).Returns(
                new List<IHexCell>() { nearNeighboringNeutralTile, farNeighboringNeutralTile, nearNeighboringForeignTile }
            );
            MockHexGrid.Setup(grid => grid.GetNeighbors(nearNeighboringNeutralTile)).Returns(new List<IHexCell>() { homeTile });
            MockHexGrid.Setup(grid => grid.GetNeighbors(farNeighboringNeutralTile)).Returns(new List<IHexCell>() { homeTile });
            MockHexGrid.Setup(grid => grid.GetNeighbors(nearNeighboringForeignTile)).Returns(new List<IHexCell>() { homeTile });

            MockHexGrid.Setup(grid => grid.GetDistance(homeTile, nearNeighboringNeutralTile)).Returns(2);
            MockHexGrid.Setup(grid => grid.GetDistance(nearNeighboringNeutralTile, homeTile)).Returns(2);

            MockHexGrid.Setup(grid => grid.GetDistance(homeTile, farNeighboringNeutralTile)).Returns(3);
            MockHexGrid.Setup(grid => grid.GetDistance(farNeighboringNeutralTile, homeTile)).Returns(3);

            MockHexGrid.Setup(grid => grid.GetDistance(homeTile, nearNonNeighboringNeutralTile)).Returns(2);
            MockHexGrid.Setup(grid => grid.GetDistance(nearNonNeighboringNeutralTile, homeTile)).Returns(2);

            MockHexGrid.Setup(grid => grid.GetDistance(homeTile, nearNeighboringForeignTile)).Returns(2);
            MockHexGrid.Setup(grid => grid.GetDistance(nearNeighboringForeignTile, homeTile)).Returns(2);

            MockConfig.SetupGet(config => config.MaxBorderRange).Returns(2);

            var expansionLogic = Container.Resolve<BorderExpansionLogic>();

            var availableTiles = expansionLogic.GetAllCellsAvailableToCity(homeCityMock.Object);

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

            var testTile = new Mock<IHexCell>().Object;
            var testCity = new Mock<ICity>().Object;

            Assert.Throws<ArgumentNullException>(() => logic.GetNextCellToPursue(null),
                "GetNextTileToPursue failed to throw on a null city argument");

            Assert.Throws<ArgumentNullException>(() => logic.IsCellAvailable(null, testTile),
                "IsTileAvailable failed to throw on a null city argument");

            Assert.Throws<ArgumentNullException>(() => logic.IsCellAvailable(testCity, null),
                "IsTileAvailable failed to throw on a null tile argument");

            Assert.Throws<ArgumentNullException>(() => logic.GetCultureCostOfAcquiringCell(null, testTile),
                "GetCultureCostOfAcquiringTile failed to throw on a null city argument");

            Assert.Throws<ArgumentNullException>(() => logic.GetCultureCostOfAcquiringCell(testCity, null),
                "GetCultureCostOfAcquiringTile failed to throw on a null tile argument");

            Assert.Throws<ArgumentNullException>(() => logic.GetGoldCostOfAcquiringCell(null, testTile),
                "GetGoldCostOfAcquiringTile failed to throw on a null city argument");

            Assert.Throws<ArgumentNullException>(() => logic.GetGoldCostOfAcquiringCell(testCity, null),
                "GetGoldCostOfAcquiringTile failed to throw on a null tile argument");
        }

        #endregion

        #endregion

    }

}
