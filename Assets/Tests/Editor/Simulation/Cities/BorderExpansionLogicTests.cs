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
            "GetNextCellToPursue should return a cell that maximizes yield on that resource for that city")]
        public void GetNextCellToPursue_FocusedCityMaximization() {
            var homeCellMock       = new Mock<IHexCell>();
            var neutralCellOneMock = new Mock<IHexCell>();
            var neutralCellTwoMock = new Mock<IHexCell>();

            homeCellMock      .Name = "Home Cell";
            neutralCellOneMock.Name = "Neutral Cell One";
            neutralCellTwoMock.Name = "Neutral Cell Two";

            var homeCell       = homeCellMock      .Object;
            var neutralCellOne = neutralCellOneMock.Object;
            var neutralCellTwo = neutralCellTwoMock.Object;

            var cells = new List<IHexCell>() { homeCell, neutralCellOne, neutralCellTwo };

            var cityMock = new Mock<ICity>();
            cityMock.SetupAllProperties();

            var homeCity = cityMock.Object;

            MockCityLocationCanon.Setup(canon => canon.GetOwnerOfPossession(homeCity)).Returns(homeCell);

            homeCity.ResourceFocus = ResourceFocusType.Food;

            MockHexGrid.Setup(grid => grid.GetDistance(It.IsAny<IHexCell>(), It.IsAny<IHexCell>())).Returns(0);
            MockHexGrid.Setup(grid => grid.GetNeighbors(It.IsAny<IHexCell>())).Returns(cells);

            MockResourceGenerationLogic.Setup(
                logic => logic.GetYieldOfCellForCity(neutralCellOne, homeCity)
            ).Returns(new ResourceSummary(food: 2));

            MockResourceGenerationLogic.Setup(
                logic => logic.GetYieldOfCellForCity(neutralCellTwo, homeCity)
            ).Returns(new ResourceSummary(food: 3));

            MockTerritoryPossessionCanon.Setup(canon => canon.GetPossessionsOfOwner(homeCity)).Returns(new List<IHexCell>() { homeCell });
            MockTerritoryPossessionCanon.Setup(canon => canon.GetOwnerOfPossession(homeCell)).Returns(homeCity);

            var expansionLogic = Container.Resolve<BorderExpansionLogic>();

            Assert.AreEqual(neutralCellTwo, expansionLogic.GetNextCellToPursue(homeCity), 
                "GetNextCellToPursue did not return the cell that maximizes food");
        }

        [Test(Description = "When a city does not express a preference for a particular resource type, " +
            "GetNextCellToPursue should return a cell that maximizes total yield for that city")]
        public void GetNextCellToPursue_UnfocusedCityMaximization() {
            var homeCellMock       = new Mock<IHexCell>();
            var neutralCellOneMock = new Mock<IHexCell>();
            var neutralCellTwoMock = new Mock<IHexCell>();

            homeCellMock      .Name = "Home Cell";
            neutralCellOneMock.Name = "Neutral Cell One";
            neutralCellTwoMock.Name = "Neutral Cell Two";

            var homeCell       = homeCellMock      .Object;
            var neutralCellOne = neutralCellOneMock.Object;
            var neutralCellTwo = neutralCellTwoMock.Object;

            var cells = new List<IHexCell>() { homeCell, neutralCellOne, neutralCellTwo };

            var cityMock = new Mock<ICity>();
            cityMock.SetupAllProperties();

            var homeCity = cityMock.Object;

            MockCityLocationCanon.Setup(canon => canon.GetOwnerOfPossession(homeCity)).Returns(homeCell);

            homeCity.ResourceFocus = ResourceFocusType.TotalYield;

            MockHexGrid.Setup(grid => grid.GetDistance(It.IsAny<IHexCell>(), It.IsAny<IHexCell>())).Returns(0);
            MockHexGrid.Setup(grid => grid.GetNeighbors(It.IsAny<IHexCell>())).Returns(cells);

            MockResourceGenerationLogic.Setup(
                logic => logic.GetYieldOfCellForCity(neutralCellOne, homeCity)
            ).Returns(new ResourceSummary(food: 2, production: 2));

            MockResourceGenerationLogic.Setup(
                logic => logic.GetYieldOfCellForCity(neutralCellTwo, homeCity)
            ).Returns(new ResourceSummary(food: 3));

            MockTerritoryPossessionCanon.Setup(canon => canon.GetPossessionsOfOwner(homeCity)).Returns(new List<IHexCell>() { homeCell });
            MockTerritoryPossessionCanon.Setup(canon => canon.GetOwnerOfPossession(homeCell)).Returns(homeCity);

            var expansionLogic = Container.Resolve<BorderExpansionLogic>();

            Assert.AreEqual(neutralCellOne, expansionLogic.GetNextCellToPursue(homeCity), 
                "GetNextCellToPursue did not return the cell that maximizes food and is also available");
        }

        [Test(Description = "GetNextCellToPursue should only ever return a cell that is available, " +
            "even if it doesn't maximize yield")]
        public void GetNextCellToPursue_IsAlwaysAvailable() {
            var homeCellMock       = new Mock<IHexCell>();
            var neutralCellOneMock = new Mock<IHexCell>();
            var neutralCellTwoMock = new Mock<IHexCell>();

            homeCellMock      .Name = "Home Cell";
            neutralCellOneMock.Name = "Neutral Cell One";
            neutralCellTwoMock.Name = "Neutral Cell Two";

            var homeCell       = homeCellMock      .Object;
            var neutralCellOne = neutralCellOneMock.Object;
            var neutralCellTwo = neutralCellTwoMock.Object;

            var cells = new List<IHexCell>() { homeCell, neutralCellOne, neutralCellTwo };

            var cityMock = new Mock<ICity>();
            cityMock.SetupAllProperties();

            var homeCity = cityMock.Object;

            MockCityLocationCanon.Setup(canon => canon.GetOwnerOfPossession(homeCity)).Returns(homeCell);

            homeCity.ResourceFocus = ResourceFocusType.Food;

            MockHexGrid.Setup(grid => grid.GetDistance(It.IsAny<IHexCell>(), It.IsAny<IHexCell>())).Returns(0);
            MockHexGrid.Setup(grid => grid.GetNeighbors(It.IsAny<IHexCell>())).Returns(cells);

            MockResourceGenerationLogic.Setup(
                logic => logic.GetYieldOfCellForCity(homeCell, homeCity)
            ).Returns(new ResourceSummary(food: 4));

            MockResourceGenerationLogic.Setup(
                logic => logic.GetYieldOfCellForCity(neutralCellOne, homeCity)
            ).Returns(new ResourceSummary(food: 2));

            MockResourceGenerationLogic.Setup(
                logic => logic.GetYieldOfCellForCity(neutralCellTwo, homeCity)
            ).Returns(new ResourceSummary(food: 3));

            MockTerritoryPossessionCanon.Setup(canon => canon.GetPossessionsOfOwner(homeCity)).Returns(new List<IHexCell>() { homeCell });
            MockTerritoryPossessionCanon.Setup(canon => canon.GetOwnerOfPossession(homeCell)).Returns(homeCity);

            var expansionLogic = Container.Resolve<BorderExpansionLogic>();

            Assert.AreEqual(neutralCellTwo, expansionLogic.GetNextCellToPursue(homeCity), 
                "GetNextCellToPursue did not return the cell that maximizes food and is also available");
        }

        [Test(Description = "A cell should not be available if it is beyond the max range " +
            "specified in the logic's Config")]
        public void IsCellAvailable_MustBeWithinMaxRange() {
            var centerCell = new Mock<IHexCell>().Object;
            var nearCell = new Mock<IHexCell>().Object;
            var farCell = new Mock<IHexCell>().Object;

            var mockCity = new Mock<ICity>();
            var homeCity = mockCity.Object;

            MockCityLocationCanon.Setup(canon => canon.GetOwnerOfPossession(homeCity)).Returns(centerCell);

            MockTerritoryPossessionCanon.Setup(canon => canon.GetOwnerOfPossession(centerCell)).Returns(homeCity);

            MockConfig.SetupGet(config => config.MaxBorderRange).Returns(2);

            MockHexGrid.Setup(grid => grid.GetNeighbors(nearCell)).Returns(new List<IHexCell>(){ centerCell });
            MockHexGrid.Setup(grid => grid.GetNeighbors(farCell)).Returns(new List<IHexCell>(){ centerCell });

            MockHexGrid.Setup(grid => grid.GetDistance(centerCell, nearCell)).Returns(2);
            MockHexGrid.Setup(grid => grid.GetDistance(nearCell, centerCell)).Returns(2);

            MockHexGrid.Setup(grid => grid.GetDistance(centerCell, farCell)).Returns(3);
            MockHexGrid.Setup(grid => grid.GetDistance(farCell, centerCell)).Returns(3);

            var expansionLogic = Container.Resolve<BorderExpansionLogic>();

            Assert.IsTrue(expansionLogic.IsCellAvailable(homeCity, nearCell),
                "NearCell was falsely flagged as unavailable for expansion");

            Assert.IsFalse(expansionLogic.IsCellAvailable(homeCity, farCell),
                "FarCell was falsely flagged as available for expansion");
        }

        [Test(Description = "A cell should not be available if it is already owned by a city")]
        public void IsCellAvailable_MustBeUnowned() {
            var homeCell = new Mock<IHexCell>().Object;
            var neutralCell = new Mock<IHexCell>().Object;
            var foreignCell = new Mock<IHexCell>().Object;

            var mockCity = new Mock<ICity>();

            var homeCity = mockCity.Object;
            var foreignCity = mockCity.Object;

            MockCityLocationCanon.Setup(canon => canon.GetOwnerOfPossession(homeCity)).Returns(homeCell);

            MockTerritoryPossessionCanon.Setup(canon => canon.GetOwnerOfPossession(homeCell)).Returns(homeCity);
            MockTerritoryPossessionCanon.Setup(canon => canon.GetOwnerOfPossession(foreignCell)).Returns(foreignCity);

            MockHexGrid.Setup(grid => grid.GetNeighbors(neutralCell)).Returns(new List<IHexCell>(){ homeCell });
            MockHexGrid.Setup(grid => grid.GetNeighbors(foreignCell)).Returns(new List<IHexCell>(){ homeCell });

            var expansionLogic = Container.Resolve<BorderExpansionLogic>();

            Assert.IsTrue(expansionLogic.IsCellAvailable(homeCity, neutralCell),
                "NeutralCell was falsely flagged as unavailable for expansion");

            Assert.IsFalse(expansionLogic.IsCellAvailable(homeCity, foreignCell),
                "ForeignCell was falsely flagged as available for expansion");
        }

        [Test(Description = "A cell should not be available if it isn't adjacent to a cell " +
            "the city has already claimed")]
        public void IsCellAvailable_MustBeAdjacentToTerritory() {
            var ownedCell = new Mock<IHexCell>().Object;
            var neighboringCell = new Mock<IHexCell>().Object;
            var nonNeighboringCell = new Mock<IHexCell>().Object;

            var mockCity = new Mock<ICity>();

            var owningCity = mockCity.Object;

            MockCityLocationCanon.Setup(canon => canon.GetOwnerOfPossession(owningCity)).Returns(ownedCell);

            MockTerritoryPossessionCanon.Setup(canon => canon.GetOwnerOfPossession(ownedCell)).Returns(owningCity);

            MockConfig.SetupGet(config => config.MaxBorderRange).Returns(2);

            MockHexGrid.Setup(grid => grid.GetNeighbors(neighboringCell)).Returns(new List<IHexCell>(){ ownedCell });
            MockHexGrid.Setup(grid => grid.GetNeighbors(nonNeighboringCell)).Returns(new List<IHexCell>());

            var expansionLogic = Container.Resolve<BorderExpansionLogic>();

            Assert.IsTrue(expansionLogic.IsCellAvailable(owningCity, neighboringCell),
                "NeighboringCell was falsely flagged as unavailable for expansion");

            Assert.IsFalse(expansionLogic.IsCellAvailable(owningCity, nonNeighboringCell),
                "NonNeighboringCell was falsely flagged as available for expansion");
        }

        [Test(Description = "GetCultureCostOfAcquiringCell should respond to Config and " +
            "operate under the following equation, where t is the current number of cells:\n\n" +
            "\tCellCostBase + (PreviousCellCountCoefficient * (t - 1)) ^ PreviousCellCountExponen\n\n" + 
            "The returned value should be rounded down")]
        public void GetCultureCostOfAcquiringCell_FollowsEquationAndConfig() {
            var city = new Mock<ICity>().Object;
            var newCell = new Mock<IHexCell>().Object;

            MockTerritoryPossessionCanon.Setup(canon => canon.GetPossessionsOfOwner(city)).Returns(new List<IHexCell>() {
               new Mock<IHexCell>().Object,
               new Mock<IHexCell>().Object,
               new Mock<IHexCell>().Object,
               new Mock<IHexCell>().Object,
               new Mock<IHexCell>().Object,
               new Mock<IHexCell>().Object,
            });

            MockConfig.SetupGet(config => config.CellCostBase).Returns(20);
            MockConfig.SetupGet(config => config.PreviousCellCountCoefficient).Returns(10);
            MockConfig.SetupGet(config => config.PreviousCellCountExponent).Returns(1.1f);

            var logic = Container.Resolve<BorderExpansionLogic>();

            Assert.AreEqual(93, logic.GetCultureCostOfAcquiringCell(city, newCell),
                "GetCultureCostOfAcquiringCell returned an incorrect value on a city with 6 cells");
        }

        [Test(Description = "GetGoldCostOfAcquiringCell should return the same value as " +
            "GetCultureCostOfAcquiringCell")]
        public void GetGoldCostOfAcquiringCell_IdenticalToCultureCost() {
            var city = new Mock<ICity>().Object;
            var newCell = new Mock<IHexCell>().Object;

            MockTerritoryPossessionCanon.Setup(canon => canon.GetPossessionsOfOwner(city)).Returns(new List<IHexCell>() {
               new Mock<IHexCell>().Object,
               new Mock<IHexCell>().Object,
               new Mock<IHexCell>().Object,
               new Mock<IHexCell>().Object,
               new Mock<IHexCell>().Object,
               new Mock<IHexCell>().Object,
            });

            MockConfig.SetupGet(config => config.CellCostBase).Returns(20);
            MockConfig.SetupGet(config => config.PreviousCellCountCoefficient).Returns(10);
            MockConfig.SetupGet(config => config.PreviousCellCountExponent).Returns(1.1f);

            var logic = Container.Resolve<BorderExpansionLogic>();

            Assert.AreEqual(93, logic.GetGoldCostOfAcquiringCell(city, newCell),
                "GetCultureCostOfAcquiringCell returned an incorrect value on a city with 6 cells");
        }

        [Test(Description = "GetAllCellsAvailableToCity should return all cells for which " +
            "IsCellAvailable is true and no others")]
        public void GetAllCellsAvailableToCity_ReturnsAllAndOnlyAllCorrectCells() {
            var homeCellMock                      = new Mock<IHexCell>(); 
            var nearNeighboringNeutralCellMock    = new Mock<IHexCell>(); 
            var farNeighboringNeutralCellMock     = new Mock<IHexCell>(); 
            var nearNonNeighboringNeutralCellMock = new Mock<IHexCell>(); 
            var nearNeighboringForeignCellMock    = new Mock<IHexCell>(); 

            homeCellMock.Name                      = "Home Cell";
            nearNeighboringNeutralCellMock.Name    = "Near Neighboring Neutral Cell";
            farNeighboringNeutralCellMock.Name     = "Far Neighboring Neutral Cell";
            nearNonNeighboringNeutralCellMock.Name = "Near Non Neighboring Neutral Cell";
            nearNeighboringForeignCellMock.Name    = "Near Neighboring Foreign Cell";

            var homeCell                      = homeCellMock                     .Object;
            var nearNeighboringNeutralCell    = nearNeighboringNeutralCellMock   .Object;
            var farNeighboringNeutralCell     = farNeighboringNeutralCellMock    .Object;
            var nearNonNeighboringNeutralCell = nearNonNeighboringNeutralCellMock.Object;
            var nearNeighboringForeignCell    = nearNeighboringForeignCellMock   .Object;

            var homeCityMock    = new Mock<ICity>();
            homeCityMock.Name = "Home City";

            MockCityLocationCanon.Setup(canon => canon.GetOwnerOfPossession(homeCityMock.Object)).Returns(homeCell);

            var foreignCityMock = new Mock<ICity>();
            foreignCityMock.Name = "Foreign City";

            MockTerritoryPossessionCanon.Setup(canon => canon.GetPossessionsOfOwner(homeCityMock.Object)).Returns(new List<IHexCell>() { homeCell });

            MockTerritoryPossessionCanon.Setup(canon => canon.GetOwnerOfPossession(homeCell)).Returns(homeCityMock.Object);
            MockTerritoryPossessionCanon.Setup(canon => canon.GetOwnerOfPossession(nearNeighboringForeignCell)).Returns(foreignCityMock.Object);

            MockHexGrid.Setup(grid => grid.GetNeighbors(homeCell)).Returns(
                new List<IHexCell>() { nearNeighboringNeutralCell, farNeighboringNeutralCell, nearNeighboringForeignCell }
            );
            MockHexGrid.Setup(grid => grid.GetNeighbors(nearNeighboringNeutralCell)).Returns(new List<IHexCell>() { homeCell });
            MockHexGrid.Setup(grid => grid.GetNeighbors(farNeighboringNeutralCell)).Returns(new List<IHexCell>() { homeCell });
            MockHexGrid.Setup(grid => grid.GetNeighbors(nearNeighboringForeignCell)).Returns(new List<IHexCell>() { homeCell });

            MockHexGrid.Setup(grid => grid.GetDistance(homeCell, nearNeighboringNeutralCell)).Returns(2);
            MockHexGrid.Setup(grid => grid.GetDistance(nearNeighboringNeutralCell, homeCell)).Returns(2);

            MockHexGrid.Setup(grid => grid.GetDistance(homeCell, farNeighboringNeutralCell)).Returns(3);
            MockHexGrid.Setup(grid => grid.GetDistance(farNeighboringNeutralCell, homeCell)).Returns(3);

            MockHexGrid.Setup(grid => grid.GetDistance(homeCell, nearNonNeighboringNeutralCell)).Returns(2);
            MockHexGrid.Setup(grid => grid.GetDistance(nearNonNeighboringNeutralCell, homeCell)).Returns(2);

            MockHexGrid.Setup(grid => grid.GetDistance(homeCell, nearNeighboringForeignCell)).Returns(2);
            MockHexGrid.Setup(grid => grid.GetDistance(nearNeighboringForeignCell, homeCell)).Returns(2);

            MockConfig.SetupGet(config => config.MaxBorderRange).Returns(2);

            var expansionLogic = Container.Resolve<BorderExpansionLogic>();

            var availableCells = expansionLogic.GetAllCellsAvailableToCity(homeCityMock.Object);

            Assert.IsFalse(availableCells.Contains(homeCell),                      "HomeCell is falsely considered an available cell");
            Assert.IsTrue (availableCells.Contains(nearNeighboringNeutralCell),    "nearNeighboringNeutralCell is not considered an available cell");
            Assert.IsFalse(availableCells.Contains(farNeighboringNeutralCell),     "farNeighboringNeutralCell is falsely considered an available cell");
            Assert.IsFalse(availableCells.Contains(nearNonNeighboringNeutralCell), "nearNonNeighboringNeutralCell is falsely considered an available cell");
            Assert.IsFalse(availableCells.Contains(nearNeighboringForeignCell),    "nearNeighboringForeignCell is falsely considered an available cell");
        }

        [Test(Description = "All methods of BorderExpansionLogic should throw an ArgumentNullException " +
            " on any null argument")]
        public void AllMethods_ThrowExceptionsOnNullArguments() {
            var logic = Container.Resolve<BorderExpansionLogic>();

            var testCell = new Mock<IHexCell>().Object;
            var testCity = new Mock<ICity>().Object;

            Assert.Throws<ArgumentNullException>(() => logic.GetNextCellToPursue(null),
                "GetNextCellToPursue failed to throw on a null city argument");

            Assert.Throws<ArgumentNullException>(() => logic.IsCellAvailable(null, testCell),
                "IsCellAvailable failed to throw on a null city argument");

            Assert.Throws<ArgumentNullException>(() => logic.IsCellAvailable(testCity, null),
                "IsCellAvailable failed to throw on a null cell argument");

            Assert.Throws<ArgumentNullException>(() => logic.GetCultureCostOfAcquiringCell(null, testCell),
                "GetCultureCostOfAcquiringCell failed to throw on a null city argument");

            Assert.Throws<ArgumentNullException>(() => logic.GetCultureCostOfAcquiringCell(testCity, null),
                "GetCultureCostOfAcquiringCell failed to throw on a null cell argument");

            Assert.Throws<ArgumentNullException>(() => logic.GetGoldCostOfAcquiringCell(null, testCell),
                "GetGoldCostOfAcquiringCell failed to throw on a null city argument");

            Assert.Throws<ArgumentNullException>(() => logic.GetGoldCostOfAcquiringCell(testCity, null),
                "GetGoldCostOfAcquiringCell failed to throw on a null cell argument");
        }

        #endregion

        #endregion

    }

}
