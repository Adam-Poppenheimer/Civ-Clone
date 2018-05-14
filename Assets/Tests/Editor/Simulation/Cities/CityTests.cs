using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

using UnityEngine;
using UnityEngine.EventSystems;

using Zenject;
using NUnit.Framework;
using Moq;
using UniRx;

using Assets.Simulation;

using Assets.Simulation.HexMap;

using Assets.Simulation.Cities;
using Assets.Simulation.Cities.Growth;
using Assets.Simulation.Cities.ResourceGeneration;
using Assets.Simulation.Cities.Territory;
using Assets.Simulation.Cities.Distribution;
using Assets.Simulation.Cities.Buildings;
using Assets.Simulation.Cities.Production;
using Assets.Simulation.WorkerSlots;

using Assets.Simulation.Units;

namespace Assets.Tests.Simulation.Cities {

    [TestFixture]
    public class CityTests : ZenjectUnitTestFixture {

        #region internal types

        private struct CellMockData {

            public bool SuppressSlot;
            public bool SlotIsLocked;
            public bool SlotIsOccupied;
            public ResourceSummary BaseYield;

        }

        #endregion

        #region instance fields and properties

        private Mock<IPopulationGrowthLogic>                    MockGrowthLogic;
        private Mock<IProductionLogic>                          MockProductionLogic;
        private Mock<IResourceGenerationLogic>                  MockResourceGenerationLogic;
        private Mock<IBorderExpansionLogic>                     MockExpansionLogic;
        private Mock<IPossessionRelationship<ICity, IHexCell>>  MockCellPossessionCanon;
        private Mock<IWorkerDistributionLogic>                  MockDistributionLogic;
        private Mock<IPossessionRelationship<ICity, IBuilding>> MockBuildingPossessionCanon;
        private Mock<ICityConfig>                               MockCityConfig;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockGrowthLogic             = new Mock<IPopulationGrowthLogic>();
            MockProductionLogic         = new Mock<IProductionLogic>();
            MockResourceGenerationLogic = new Mock<IResourceGenerationLogic>();
            MockExpansionLogic          = new Mock<IBorderExpansionLogic>();
            MockCellPossessionCanon     = new Mock<IPossessionRelationship<ICity, IHexCell>>();
            MockDistributionLogic       = new Mock<IWorkerDistributionLogic>();
            MockBuildingPossessionCanon = new Mock<IPossessionRelationship<ICity, IBuilding>>();
            MockCityConfig              = new Mock<ICityConfig>();


            Container.Bind<IPopulationGrowthLogic>                   ().FromInstance(MockGrowthLogic            .Object);
            Container.Bind<IProductionLogic>                         ().FromInstance(MockProductionLogic        .Object);
            Container.Bind<IResourceGenerationLogic>                 ().FromInstance(MockResourceGenerationLogic.Object);
            Container.Bind<IBorderExpansionLogic>                    ().FromInstance(MockExpansionLogic         .Object);
            Container.Bind<IPossessionRelationship<ICity, IHexCell>> ().FromInstance(MockCellPossessionCanon    .Object);
            Container.Bind<IWorkerDistributionLogic>                 ().FromInstance(MockDistributionLogic      .Object);
            Container.Bind<IPossessionRelationship<ICity, IBuilding>>().FromInstance(MockBuildingPossessionCanon.Object);
            Container.Bind<ICityConfig>                              ().FromInstance(MockCityConfig             .Object);

            Container.Bind<SignalManager>().AsSingle();

            Container.Bind<ISubject<ICity>>().WithId("City Clicked Subject").FromInstance(new Subject<ICity>());

            Container.DeclareSignal<CityProjectChangedSignal>();
            Container.DeclareSignal<CityDistributionPerformedSignal>();

            Container.Bind<CitySignals>().AsSingle();

            Container.Bind<City>().FromNewComponentOnNewGameObject().AsSingle();
        }

        #endregion

        #region tests

        [Test(Description = "When PerformGrowth is called on a city with a negative food stockpile " +
            "and more than one person, its population should decrease by 1")]
        public void PerformGrowth_NegativeStockpileDecreasesPopulation() {
            var city = Container.Resolve<City>();

            city.Population = 2;
            city.FoodStockpile = -1;

            city.PerformGrowth();

            Assert.AreEqual(1, city.Population, "Population did not decrease as expected");
        }

        [Test(Description = "When PerformGrowth is called on a city that is set to starve, " + 
            "GrowthLogic should be called to determine the new food stockpile of that city")]
        public void PerformGrowth_ChecksGrowthLogicForStarvationStockpileReset() {
            var city = Container.Resolve<City>();

            city.Population = 2;
            city.FoodStockpile = -1;

            MockGrowthLogic.Setup(logic => logic.GetFoodStockpileAfterStarvation(city)).Returns(9);

            city.PerformGrowth();

            MockGrowthLogic.Verify(logic => logic.GetFoodStockpileAfterStarvation(city), Times.AtLeastOnce, 
                "GrowthLogic did not have its GetFoodStockpileAfterStarvation method called");

            Assert.AreEqual(9, city.FoodStockpile, "After starvation, City has an unexpected food stockpile");
        }

        [Test(Description = "When PerformGrowth is called on a city with one population " +
            "that consumes more food than it produces, FoodStockpile should remain at zero and " +
            "never become negative")]
        public void PerformGrowth_SinglePopulationDoesntCauseNegativeFood() {
            var city = Container.Resolve<City>();
            
            MockGrowthLogic.Setup(logic => logic.GetFoodConsumptionPerTurn(city)).Returns(10);

            MockResourceGenerationLogic.Setup(logic => logic.GetTotalYieldForCity(city)).Returns(ResourceSummary.Empty);

            city.Population = 1;

            for(int i = 0; i < 10; ++i) {
                city.PerformGrowth();
                Assert.That(city.FoodStockpile >= 0, "City's FoodStockpile became negative after PerformGrowth call " + i);
            }
        }

        [Test(Description = "When PerformGrowth is called on a city that isn't starving, " + 
            "GrowthLogic should be called to determine the food stockpile necessary for the city to grow")]
        public void PerformGrowth_ChecksGrowthLogicForPopulationGrowth() {
            var city = Container.Resolve<City>();

            city.PerformGrowth();

            MockGrowthLogic.Verify(logic => logic.GetFoodStockpileToGrow(city), Times.AtLeastOnce, 
                "GrowthLogic did not have its GetFoodStockpileToGrow method called");
        }

        [Test(Description = "When PerformGrowth is called on a city with a sufficient food stockpile, " +
            "its population should increase by 1")]
        public void PerformGrowth_SufficientStockpileIncreasesPopulation() {
            var city = Container.Resolve<City>();
            city.Population = 1;
            city.FoodStockpile = 15;

            MockGrowthLogic.Setup(logic => logic.GetFoodStockpileToGrow(city)).Returns(14);

            city.PerformGrowth();
            Assert.AreEqual(2, city.Population, "Population did not increase as expected");
        }

        [Test(Description = "When PerformGrowth is called on a city with a sufficient food stockpile, " + 
            "GrowthLogic should be called to figure what it's new food stockpile is")]
        public void PerformGrowth_ChecksGrowthLogicForStockpileReduction() {
            var city = Container.Resolve<City>();

            city.Population = 1;
            city.FoodStockpile = 15;

            MockGrowthLogic.Setup(logic => logic.GetFoodStockpileAfterGrowth(city)).Returns(9);

            city.PerformGrowth();

            MockGrowthLogic.Verify(logic => logic.GetFoodStockpileAfterGrowth(city), Times.AtLeastOnce, 
                "GrowthLogic did not have its GetFoodStockpileAfterGrowth method called");

            Assert.AreEqual(9, city.FoodStockpile, "After growth, City has an unexpected food stockpile");
        }

        [Test(Description = "When PerformProduction is called on a city with a non-null CurrentProject, " + 
            "that project's progress should be increased by a level determined by a call to ProductionLogic")]
        public void PerformProduction_ChecksProductionLogicForProjectProgress() {
            var mockProject = new Mock<IProductionProject>();
            mockProject.SetupAllProperties();
            mockProject.Object.Progress = 5;
            mockProject.SetupGet(project => project.ProductionToComplete).Returns(30);

            var city = Container.Resolve<City>();
            city.ActiveProject = mockProject.Object;

            MockProductionLogic.Setup(logic => logic.GetProductionProgressPerTurnOnProject(city, mockProject.Object)).Returns(9);

            city.PerformProduction();

            MockProductionLogic.Verify(logic => logic.GetProductionProgressPerTurnOnProject(city, mockProject.Object),
                "ProductionLogic did not have its GetProductionProgressPerTurnOnProject method called");

            Assert.AreEqual(14, mockProject.Object.Progress, "CurrentProject has an incorrect Progress");
        }

        [Test(Description = "When PerformProduction is called on a city whose CurrentProject has " +
            "sufficient progress to complete, CurrentProject should have its ExecuteProject method called")]
        public void PerformProduction_ExecutesProjectWhenComplete() {
            var mockProject = new Mock<IProductionProject>();
            mockProject.SetupAllProperties();
            mockProject.Object.Progress = 5;
            mockProject.SetupGet(project => project.ProductionToComplete).Returns(5);

            var city = Container.Resolve<City>();
            city.ActiveProject = mockProject.Object;

            city.PerformProduction();

            mockProject.Verify(project => project.Execute(city), Times.Once, "ExecuteProject on CurrentProject was not called");
        }

        [Test(Description = "When PerformProduction is called on a city whose CurrentProject has " +
            "sufficient progress to complete, CurrentProject should be reset to null")]
        public void PerformProduction_ClearsCurrentProjectWhenCompleted() {
            var mockProject = new Mock<IProductionProject>();
            mockProject.SetupAllProperties();
            mockProject.Object.Progress = 5;
            mockProject.SetupGet(project => project.ProductionToComplete).Returns(5);

            var city = Container.Resolve<City>();
            city.ActiveProject = mockProject.Object;

            city.PerformProduction();

            Assert.Null(city.ActiveProject, "CurrentProject was not set to null");
        }

        [Test(Description = "When PerformExpansion is called on a city, that city should call " + 
            "ExpansionLogic's GetNextTileToPursue method to determine the tile it's pursuing")]
        public void PerformExpansion_ChecksExpansionLogicForTileToPursue() {
            var city = Container.Resolve<City>();

            var tile = new Mock<IHexCell>().Object;

            MockExpansionLogic.Setup(logic => logic.GetNextCellToPursue(city)).Returns(tile);

            city.PerformExpansion();

            MockExpansionLogic.Verify(logic => logic.GetNextCellToPursue(city), Times.AtLeastOnce,
                "ExpansionLogic's GetNextTileToPursue method was never called");
            Assert.AreEqual(tile, city.CellBeingPursued, "City has an incorrect TileBeingPursued");
        }

        [Test(Description = "When PerformExpansion is called on a city, that city should call " +
            "ExpansionLogic's GetCultureCostOfAcquiringTile method to determine whether it can " +
            "seize its TileBeingPursued or not")]
        public void PerformExpansion_ChecksExpansionLogicForTileAcquisition() {
            var city = Container.Resolve<City>();

            var tile = new Mock<IHexCell>().Object;

            MockExpansionLogic.Setup(logic => logic.GetNextCellToPursue(city)).Returns(tile);

            city.PerformExpansion();

            MockExpansionLogic.Verify(logic => logic.GetCultureCostOfAcquiringCell(city, tile), Times.AtLeastOnce,
                "ExpansionLogic's GetCultureCostOfAcquiringTile method was never called");
        }

        [Test(Description = "When PerformExpansion is called on a city, that city should call " + 
            "PossessionCanon's CanChangeOwnerOfTile method to determine whether it can seize " + 
            "its TileBeingPursued or not")]
        public void PerformExpansion_ChecksPosessionCanonForTileAcquisition() {
            var city = Container.Resolve<City>();

            var tile = new Mock<IHexCell>().Object;

            MockExpansionLogic.Setup(logic => logic.GetNextCellToPursue(city)).Returns(tile);
            MockExpansionLogic.Setup(logic => logic.IsCellAvailable(city, tile)).Returns(true);

            city.PerformExpansion();

            MockCellPossessionCanon.Verify(canon => canon.CanChangeOwnerOfPossession(tile, city), Times.AtLeastOnce, 
                "PossessionCanon's CanChangeOwnerOfTile method was never called");
        }

        [Test(Description = "When PerformExpansion is called on a city that can acquire its " +
            "TileBeingPursued, it should check ExpansionLogic to determine how much its culture " +
            "stockpile should be decreased, and decrease the stockpile accordingly")]
        public void PerformExpansion_ChecksExpansionLogicForCultureStockpileDecrease() {
            var city = Container.Resolve<City>();
            city.CultureStockpile = 10;

            var tile = new Mock<IHexCell>().Object;

            MockExpansionLogic.Setup(logic => logic.GetNextCellToPursue(city)).Returns(tile);
            MockExpansionLogic.Setup(logic => logic.IsCellAvailable(city, tile)).Returns(true);
            MockExpansionLogic.Setup(logic => logic.GetCultureCostOfAcquiringCell(city, tile)).Returns(7);

            MockCellPossessionCanon.Setup(canon => canon.CanChangeOwnerOfPossession(tile, city)).Returns(true);

            city.PerformExpansion();

            Assert.AreEqual(3, city.CultureStockpile, "CultureStockpile has an incorrect value");
        }

        [Test(Description = "When PerformExpansion is called on a city that can acquire its " +
            "TileBeingPursued, it should request the ownership change of that tile from " +
            "PossessionCanon")]
        public void PerformExpansion_RequestsOwnershipChangeFromPossessionCanon() {
            var city = Container.Resolve<City>();

            var tile = new Mock<IHexCell>().Object;

            MockExpansionLogic.Setup(logic => logic.GetNextCellToPursue(city)).Returns(tile);
            MockExpansionLogic.Setup(logic => logic.IsCellAvailable(city, tile)).Returns(true);
            MockExpansionLogic.Setup(logic => logic.GetCultureCostOfAcquiringCell(city, tile)).Returns(0);

            MockCellPossessionCanon.Setup(canon => canon.CanChangeOwnerOfPossession(tile, city)).Returns(true);

            city.PerformExpansion();

            MockCellPossessionCanon.Verify(canon => canon.ChangeOwnerOfPossession(tile, city), Times.Once, 
                "Did not receive the expected ChangeOwnerOfTile call on PossessionCanon");
        }

        [Test(Description = "When PerformExpansion is called on a city that can acquire its " +
            "TileBeingPursued, it should check ExpansionLogic to determine the new tile it " +
            "should be pursuing")]
        public void PerformExpansion_ChecksExpansionLogicAfterAcquisition() {
            var city = Container.Resolve<City>();

            var firstTile = new Mock<IHexCell>().Object;
            var secondTile = new Mock<IHexCell>().Object;

            MockExpansionLogic.SetupSequence(logic => logic.GetNextCellToPursue(city))
                .Returns(firstTile)
                .Returns(secondTile);

            MockExpansionLogic.Setup(logic => logic.IsCellAvailable(city, firstTile)).Returns(true);
            MockExpansionLogic.Setup(logic => logic.GetCultureCostOfAcquiringCell(city, firstTile)).Returns(0);

            MockCellPossessionCanon.Setup(canon => canon.CanChangeOwnerOfPossession(firstTile, city)).Returns(true);

            city.PerformExpansion();

            Assert.AreEqual(secondTile, city.CellBeingPursued, "A configuration that should've led to a successful " +
                "tile acquisition did not change the TileBeingPursued of the city to the next tile to pursue");
        }

        [Test(Description = "When PerformExpansion is called and ExpansionLogic returns a " +
            "null Tile to pursue, PerformExpansion should handle this gracefully and not throw any exceptions")]
        public void PerformExpansion_DoesNotThrowOnNullPursuit() {
            var city = Container.Resolve<City>();

            MockExpansionLogic.Setup(logic => logic.GetNextCellToPursue(city)).Returns<IHexCell>(null);

            Assert.DoesNotThrow(() => city.PerformExpansion(),
                "An exception was thrown when calling PerformExpansion on a null TileToPursue, even though this should be valid");

            Assert.IsNull(city.CellBeingPursued, "TileBeingPursued has an incorrect value");
        }

        [Test(Description = "When PerformDistribution is called on a city, that city should " + 
            "call into DistributionLogic with the correct city, worker count, and preferences")]
        public void PerformDistribution_SendsRequestToDistributionLogic() {
            var city = Container.Resolve<City>();
            city.Population = 7;
            city.ResourceFocus = ResourceFocusType.TotalYield;

            MockCellPossessionCanon.Setup(canon => canon.GetPossessionsOfOwner(city))
                .Returns(new List<IHexCell>() { BuildMockCell(new CellMockData()) }.AsReadOnly());

            MockBuildingPossessionCanon.Setup(canon => canon.GetPossessionsOfOwner(city)).Returns(new List<IBuilding>().AsReadOnly());

            city.PerformDistribution();

            MockDistributionLogic.Verify(
                logic => logic.DistributeWorkersIntoSlots(
                    city.Population, It.IsAny<IEnumerable<IWorkerSlot>>(), city, city.ResourceFocus
                ),
                Times.Once,
                "DistributionLogic's DistributeWorkersIntoSlots was not called with the correct " +
                "population, city, and DistributionPreferences"
            );
        }

        [Test(Description = "When PerformDistribution is called on a city, that city should " +
            "send DistributionLogic all of the slots that distributionLogic claims are available to it")]
        public void PerformDistribution_CallsIntoGetSlotsAvailableToCity() {
            var city = Container.Resolve<City>();

            var buildingMockOne = new Mock<IBuilding>();
            var firstSlots = new List<IWorkerSlot>() { new Mock<IWorkerSlot>().Object };
            buildingMockOne.Setup(building => building.Slots).Returns(firstSlots.AsReadOnly());

            var buildingMockTwo = new Mock<IBuilding>();
            var secondSlots = new List<IWorkerSlot>() { new Mock<IWorkerSlot>().Object, new Mock<IWorkerSlot>().Object };
            buildingMockTwo.Setup(building => building.Slots).Returns(secondSlots.AsReadOnly());

            var buildingMockThree = new Mock<IBuilding>();
            var thirdSlots = new List<IWorkerSlot>() { new Mock<IWorkerSlot>().Object, new Mock<IWorkerSlot>().Object, new Mock<IWorkerSlot>().Object };
            buildingMockThree.Setup(building => building.Slots).Returns(thirdSlots.AsReadOnly());

            var allSlots = new List<IWorkerSlot>(firstSlots.Concat(secondSlots).Concat(thirdSlots));

            MockDistributionLogic.Setup(logic => logic.GetSlotsAvailableToCity(city)).Returns(allSlots);

            MockDistributionLogic.Setup(
                logic => logic.DistributeWorkersIntoSlots(
                    It.IsAny<int>(),
                    It.IsAny<IEnumerable<IWorkerSlot>>(),
                    It.IsAny<ICity>(),
                    It.IsAny<ResourceFocusType>()
                )
            ).Callback(
                delegate(int workers, IEnumerable<IWorkerSlot> availableSlots, ICity calledCity, ResourceFocusType preferences) {
                    CollectionAssert.IsSupersetOf(availableSlots, allSlots);
                }
            );                

            city.PerformDistribution();

            MockDistributionLogic.Verify(logic => logic.GetSlotsAvailableToCity(city), Times.Once,
                "DistributionLogic.GetSlotsAvailableToCity was not called");
        }

        [Test(Description = "When PerformDistribution is called, City does not pass any tile slots " +
            "whose tiles have been marked for slot suppression")]
        public void PerformDistribution_SuppressedSlotsNotPassed() {
            var city = Container.Resolve<City>();

            var tileMockOne = new Mock<IHexCell>();
            tileMockOne.Setup(tile => tile.WorkerSlot).Returns(new Mock<IWorkerSlot>().Object);

            var tileMockTwo = new Mock<IHexCell>();
            tileMockTwo.Setup(tile => tile.WorkerSlot).Returns(new Mock<IWorkerSlot>().Object);
            tileMockTwo.Setup(tile => tile.SuppressSlot).Returns(true);

            var tileMockThree = new Mock<IHexCell>();
            tileMockThree.Setup(tile => tile.WorkerSlot).Returns(new Mock<IWorkerSlot>().Object);

            var tiles = new List<IHexCell>() {tileMockOne.Object, tileMockTwo.Object, tileMockThree.Object};

            MockCellPossessionCanon.Setup(canon => canon.GetPossessionsOfOwner(city)).Returns(tiles);

            MockBuildingPossessionCanon.Setup(canon => canon.GetPossessionsOfOwner(city)).Returns(new List<IBuilding>().AsReadOnly());

            MockDistributionLogic.Setup(
                logic => logic.DistributeWorkersIntoSlots(
                    It.IsAny<int>(),
                    It.IsAny<IEnumerable<IWorkerSlot>>(),
                    It.IsAny<ICity>(),
                    It.IsAny<ResourceFocusType>()
                )
            ).Callback(
                delegate(int workers, IEnumerable<IWorkerSlot> availableSlots, ICity calledCity, ResourceFocusType preferences) {
                    CollectionAssert.DoesNotContain(availableSlots, tileMockTwo.Object.WorkerSlot,
                        "DistributionLogic was incorrectly passed a slot from a suppressed tile");
                }
            ); 

            city.PerformDistribution();
        }

        [Test(Description = "When PerformDistribution is called, City does not pass any slots " +
            "that are locked, regardless of whether they're occupied or not. Locked slots that " + 
            "are occupied should also reduce WorkerCount")]
        public void PerformDistribution_LockedSlotsNotPassed() {
            var city = Container.Resolve<City>();
            city.Population = 2;

            var unlockedSlotTile         = BuildMockCell(new CellMockData());
            var lockedUnoccupiedSlotTile = BuildMockCell(new CellMockData() { SlotIsLocked = true });
            var lockedOccupiedSlotTile   = BuildMockCell(new CellMockData() { SlotIsLocked = true, SlotIsOccupied = true });

            var tiles = new List<IHexCell>() { unlockedSlotTile, lockedUnoccupiedSlotTile, lockedOccupiedSlotTile };

            MockCellPossessionCanon
                .Setup(canon => canon.GetPossessionsOfOwner(city))
                .Returns(tiles);

            MockBuildingPossessionCanon
                .Setup(canon => canon.GetPossessionsOfOwner(city))
                .Returns(new List<IBuilding>().AsReadOnly());

            MockDistributionLogic.Setup(
                logic => logic.DistributeWorkersIntoSlots(1, It.IsAny<IEnumerable<IWorkerSlot>>(), city, It.IsAny<ResourceFocusType>())
            ).Callback(
                delegate(int workerCount, IEnumerable<IWorkerSlot> availableSlots, ICity calledCity, ResourceFocusType preferences) {
                    Assert.AreEqual(1, workerCount, "Incorrect workerCount passed to DistributionLogic");

                    CollectionAssert.Contains      (availableSlots, unlockedSlotTile        .WorkerSlot, "unlockedSlot not passed to DistributionLogic");
                    CollectionAssert.DoesNotContain(availableSlots, lockedUnoccupiedSlotTile.WorkerSlot, "lockedUnoccupiedSlot falsely passed to DistributionLogic");
                    CollectionAssert.DoesNotContain(availableSlots, lockedOccupiedSlotTile  .WorkerSlot, "lockedOccupiedSlot falsely passed to DistributionLogic");
                }
            );

            city.PerformDistribution();
        }

        [Test(Description = "When PerformDistribution is called, City fires the DistributionPerformedSignal")]
        public void PerformDistribution_DistributionPerformedSignalFired() {
            var city = Container.Resolve<City>();

            var signal = Container.Resolve<CityDistributionPerformedSignal>();
            signal.Listen(delegate(ICity sourceCity) {
                Assert.AreEqual(city, sourceCity, "Signal was not fired on the correct city");
                Assert.Pass();
            });

            city.PerformDistribution();
            Assert.Fail("CityDistributionPerformedSignal was never fired");
        }

        [Test(Description = "When PerformIncome is called on a city, that city should " +
            "call into ResourceGenerationLogic to determine its LastIncome and update its " +
            "culture and food stockpiles")]
        public void PerformIncome_ChecksResourceGenerationLogicForIncome() {
            var city = Container.Resolve<City>();
            city.FoodStockpile = 5;
            city.CultureStockpile = 4;

            var income = new ResourceSummary(food: -2, production: 1, gold: 7, culture: 2);

            MockResourceGenerationLogic.Setup(logic => logic.GetTotalYieldForCity(city)).Returns(income);

            city.PerformIncome();

            MockResourceGenerationLogic.Verify(logic => logic.GetTotalYieldForCity(city), Times.AtLeastOnce,
                "ResourceGenerationLogic's GetTotalYieldForCity was not called as expected");

            Assert.AreEqual(income, city.LastIncome, "City did not correctly update its LastIncome");
            Assert.AreEqual(3, city.FoodStockpile, "City did not correctly update its food stockpile");
            Assert.AreEqual(6, city.CultureStockpile, "City did not correctly update its culture stockpile");
        }

        [Test(Description = "When PerformIncome is called on a city, that city should " +
            "call into GrowthLogic's GetFoodConsumptionPerTurn and subtract that value " +
            "from its food stockpile")]
        public void PerformIncome_ChecksGrowthLogicForFoodConsumption() {
            var city = Container.Resolve<City>();
            city.FoodStockpile = 5;

            MockResourceGenerationLogic.Setup(logic => logic.GetTotalYieldForCity(city)).Returns(ResourceSummary.Empty);

            MockGrowthLogic.Setup(logic => logic.GetFoodConsumptionPerTurn(city)).Returns(4);

            city.PerformIncome();

            MockGrowthLogic.Verify(logic => logic.GetFoodConsumptionPerTurn(city), Times.AtLeastOnce,
                "GrowthLogic's GetFoodConsumptionPerTurn was not called as expected");

            Assert.AreEqual(1, city.FoodStockpile, "City did not correctly update its FoodStockpile");
        }

        [Test(Description = "When PerformIncome is called on a city, that city should " +
            "call GrowthLogic's GetFoodStockpileAdditionFromIncome to determine how much " +
            "to increase the city's food stockpile by. It should only do this if the " +
            "city's net food income is positive")]
        public void PerformIncome_ChecksGrowthLogicForFoodStockpileAddition() {
            var city = Container.Resolve<City>();

            city.FoodStockpile = 0;

            MockResourceGenerationLogic
                .Setup(logic => logic.GetTotalYieldForCity(city))
                .Returns(new ResourceSummary(food: 10));

            MockGrowthLogic.Setup(logic => logic.GetFoodConsumptionPerTurn(city)).Returns(8);
            MockGrowthLogic.Setup(logic => logic.GetFoodStockpileAdditionFromIncome(city, 2)).Returns(2);

            city.PerformIncome();

            Assert.AreEqual(2, city.FoodStockpile, "FoodStockpile has an unexpected value");

            MockGrowthLogic.Verify(logic => logic.GetFoodStockpileAdditionFromIncome(city, 2), Times.AtLeastOnce,
                "GetFoodStockpileAdditionFromIncome wasn't called on the expected arguments");

            MockGrowthLogic.ResetCalls();

            MockGrowthLogic.Setup(logic => logic.GetFoodConsumptionPerTurn(city)).Returns(12);

            city.PerformIncome();

            MockGrowthLogic.Verify(logic => logic.GetFoodStockpileAdditionFromIncome(city, It.IsAny<int>()),
                Times.Never, "GetFoodStockpileAdditionFromIncome was unexpectedly called");
        }

        [Test(Description = "When SetActiveProductionProject is called on an IBuildingTemplate, " +
            "City should fire ProjectChangedSignal with the appropriate arguments")]
        public void SetActiveProductionProject_OnBuildingTemplate_FiresProjectChangedSignal() {
            var cityToTest = Container.Resolve<City>();

            var newTemplateMock = new Mock<IBuildingTemplate>();
            newTemplateMock.Setup(template => template.name).Returns("New Template");

            var mockProject = new Mock<IProductionProject>();
            mockProject.Setup(project => project.Name).Returns(newTemplateMock.Name);

            var citySignals = Container.Resolve<CitySignals>();

            citySignals.ProjectChangedSignal.Listen(delegate(ICity city, IProductionProject project) {
                Assert.AreEqual(city, cityToTest, "ClickedSignal was passed the wrong city");

                Assert.AreEqual(mockProject.Object, project, "ClickedSignal was passed an unexpected project");
                Assert.Pass();
            });

            cityToTest.ActiveProject = mockProject.Object;
        }

        [Test(Description = "When SetActiveProductionProject is called on an IUnitTemplate, " +
            "City should fire ProjectChangedSignal with the appropriate arguments")]
        public void SetActiveProductionProject_OnUnitTemplate_FiresProjectChangedSignal() {
            var cityToTest = Container.Resolve<City>();

            var newTemplateMock = new Mock<IUnitTemplate>();
            newTemplateMock.Setup(template => template.name).Returns("New Template");

            var mockProject = new Mock<IProductionProject>();
            mockProject.Setup(project => project.Name).Returns(newTemplateMock.Name);

            var citySignals = Container.Resolve<CitySignals>();

            citySignals.ProjectChangedSignal.Listen(delegate(ICity city, IProductionProject project) {
                Assert.AreEqual(city, cityToTest, "ClickedSignal was passed the wrong city");

                Assert.AreEqual(mockProject.Object, project, "ClickedSignal was passed an unexpected project");
                Assert.Pass();
            });

            cityToTest.ActiveProject = mockProject.Object;
        }

        [Test(Description = "When PerformHealing is called, CombatFacade should be healed by an amount " +
            "configured in CityConfig and have its current movement set to 1")]
        public void PerformHealing_FacadeHealedAndGivenMovement() {
            MockCityConfig.Setup(config => config.HitPointRegenPerRound).Returns(20);

            var city = Container.Resolve<City>();
            city.CombatFacade = BuildUnit();
            city.CombatFacade.CurrentHitpoints = 0;
            city.CombatFacade.CurrentMovement = 0;

            city.PerformHealing();

            Assert.AreEqual(20, city.CombatFacade.CurrentHitpoints, "CombatFacade was not healed properly");
            Assert.AreEqual(1, city.CombatFacade.CurrentMovement, "CombatFacade's movement wasn't restored properly");
        }

        #endregion

        #region utilities

        private IHexCell BuildMockCell(CellMockData mockData) {
            var mockCell = new Mock<IHexCell>();

            var mockSlot = new Mock<IWorkerSlot>();

            mockSlot.Setup(slot => slot.ParentCell).Returns(mockCell.Object);

            mockSlot.SetupAllProperties();

            mockSlot.Object.IsLocked = mockData.SlotIsLocked;
            mockSlot.Object.IsOccupied = mockData.SlotIsOccupied;

            mockCell.Setup(tile => tile.WorkerSlot  ).Returns(mockSlot.Object);
            mockCell.Setup(tile => tile.SuppressSlot).Returns(mockData.SuppressSlot);

            return mockCell.Object;
        }

        private IUnit BuildUnit() {
            var mockUnit = new Mock<IUnit>();

            mockUnit.SetupAllProperties();

            return mockUnit.Object;
        }

        #endregion

        #endregion

    }

}
