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

using Assets.Simulation.GameMap;

using Assets.Simulation.Cities;
using Assets.Simulation.Cities.Growth;
using Assets.Simulation.Cities.ResourceGeneration;
using Assets.Simulation.Cities.Territory;
using Assets.Simulation.Cities.Distribution;
using Assets.Simulation.Cities.Buildings;
using Assets.Simulation.Cities.Production;

using Assets.Simulation.Units;

namespace Assets.Tests.Simulation.Cities {

    [TestFixture]
    public class CityTests : ZenjectUnitTestFixture {

        private Mock<IPopulationGrowthLogic>    GrowthMock;
        private Mock<IProductionLogic>          ProductionMock;
        private Mock<IResourceGenerationLogic>  ResourceGenerationMock;
        private Mock<IBorderExpansionLogic>     ExpansionMock;
        private Mock<ITilePossessionCanon>      TilePossessionCanonMock;
        private Mock<IWorkerDistributionLogic>  DistributionMock;
        private Mock<IBuildingPossessionCanon>  BuildingPossessionCanonMock;
        private Mock<IProductionProjectFactory> ProjectFactoryMock;

        [SetUp]
        public void CommonInstall() {
            GrowthMock                  = new Mock<IPopulationGrowthLogic>();
            ProductionMock              = new Mock<IProductionLogic>();
            ResourceGenerationMock      = new Mock<IResourceGenerationLogic>();
            ExpansionMock               = new Mock<IBorderExpansionLogic>();
            TilePossessionCanonMock     = new Mock<ITilePossessionCanon>();
            DistributionMock            = new Mock<IWorkerDistributionLogic>();
            BuildingPossessionCanonMock = new Mock<IBuildingPossessionCanon>();
            ProjectFactoryMock          = new Mock<IProductionProjectFactory>();

            Container.Bind<IPopulationGrowthLogic>   ().FromInstance(GrowthMock                 .Object);
            Container.Bind<IProductionLogic>         ().FromInstance(ProductionMock             .Object);
            Container.Bind<IResourceGenerationLogic> ().FromInstance(ResourceGenerationMock     .Object);
            Container.Bind<IBorderExpansionLogic>    ().FromInstance(ExpansionMock              .Object);
            Container.Bind<ITilePossessionCanon>     ().FromInstance(TilePossessionCanonMock    .Object);
            Container.Bind<IWorkerDistributionLogic> ().FromInstance(DistributionMock           .Object);
            Container.Bind<IBuildingPossessionCanon> ().FromInstance(BuildingPossessionCanonMock.Object);
            Container.Bind<IProductionProjectFactory>().FromInstance(ProjectFactoryMock         .Object);

            Container.Bind<SignalManager>().AsSingle();

            Container.Bind<ISubject<ICity>>().WithId("City Clicked Subject").FromInstance(new Subject<ICity>());

            Container.DeclareSignal<CityProjectChangedSignal>();
            Container.DeclareSignal<CityDistributionPerformedSignal>();

            Container.Bind<CitySignals>().AsSingle();

            Container.Bind<City>().FromNewComponentOnNewGameObject().AsSingle();
        }

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

            GrowthMock.Setup(logic => logic.GetFoodStockpileAfterStarvation(city)).Returns(9);

            city.PerformGrowth();

            GrowthMock.Verify(logic => logic.GetFoodStockpileAfterStarvation(city), Times.AtLeastOnce, 
                "GrowthLogic did not have its GetFoodStockpileAfterStarvation method called");

            Assert.AreEqual(9, city.FoodStockpile, "After starvation, City has an unexpected food stockpile");
        }

        [Test(Description = "When PerformGrowth is called on a city with one population " +
            "that consumes more food than it produces, FoodStockpile should remain at zero and " +
            "never become negative")]
        public void PerformGrowth_SinglePopulationDoesntCauseNegativeFood() {
            var city = Container.Resolve<City>();
            
            GrowthMock.Setup(logic => logic.GetFoodConsumptionPerTurn(city)).Returns(10);

            ResourceGenerationMock.Setup(logic => logic.GetTotalYieldForCity(city)).Returns(ResourceSummary.Empty);

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

            GrowthMock.Verify(logic => logic.GetFoodStockpileToGrow(city), Times.AtLeastOnce, 
                "GrowthLogic did not have its GetFoodStockpileToGrow method called");
        }

        [Test(Description = "When PerformGrowth is called on a city with a sufficient food stockpile, " +
            "its population should increase by 1")]
        public void PerformGrowth_SufficientStockpileIncreasesPopulation() {
            var city = Container.Resolve<City>();
            city.Population = 1;
            city.FoodStockpile = 15;

            GrowthMock.Setup(logic => logic.GetFoodStockpileToGrow(city)).Returns(14);

            city.PerformGrowth();
            Assert.AreEqual(2, city.Population, "Population did not increase as expected");
        }

        [Test(Description = "When PerformGrowth is called on a city with a sufficient food stockpile, " + 
            "GrowthLogic should be called to figure out how much to reduce the current food stockpile")]
        public void PerformGrowth_ChecksGrowthLogicForStockpileReduction() {
            var city = Container.Resolve<City>();

            city.Population = 1;
            city.FoodStockpile = 15;

            GrowthMock.Setup(logic => logic.GetFoodStockpileSubtractionAfterGrowth(city)).Returns(9);

            city.PerformGrowth();

            GrowthMock.Verify(logic => logic.GetFoodStockpileSubtractionAfterGrowth(city), Times.AtLeastOnce, 
                "GrowthLogic did not have its GetFoodStockpileSubtractionAfterGrowth method called");

            Assert.AreEqual(6, city.FoodStockpile, "After growth, City has an unexpected food stockpile");
        }

        [Test(Description = "When PerformProduction is called on a city with a non-null CurrentProject, " + 
            "that project's progress should be increased by a level determined by a call to ProductionLogic")]
        public void PerformProduction_ChecksProductionLogicForProjectProgress() {
            var mockProject = new Mock<IProductionProject>();
            mockProject.SetupAllProperties();
            mockProject.Object.Progress = 5;
            mockProject.SetupGet(project => project.ProductionToComplete).Returns(30);

            ProjectFactoryMock.Setup(factory => factory.ConstructBuildingProject(It.IsAny<IBuildingTemplate>()))
                .Returns(mockProject.Object);

            var city = Container.Resolve<City>();
            city.SetActiveProductionProject(new Mock<IBuildingTemplate>().Object);

            ProductionMock.Setup(logic => logic.GetProductionProgressPerTurnOnProject(city, mockProject.Object)).Returns(9);

            city.PerformProduction();

            ProductionMock.Verify(logic => logic.GetProductionProgressPerTurnOnProject(city, mockProject.Object),
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

            ProjectFactoryMock.Setup(factory => factory.ConstructBuildingProject(It.IsAny<IBuildingTemplate>()))
                .Returns(mockProject.Object);

            var city = Container.Resolve<City>();
            city.SetActiveProductionProject(new Mock<IBuildingTemplate>().Object);

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

            ProjectFactoryMock.Setup(factory => factory.ConstructBuildingProject(It.IsAny<IBuildingTemplate>()))
                .Returns(mockProject.Object);

            var city = Container.Resolve<City>();
            city.SetActiveProductionProject(new Mock<IBuildingTemplate>().Object);

            city.PerformProduction();

            Assert.Null(city.ActiveProject, "CurrentProject was not set to null");
        }

        [Test(Description = "When PerformExpansion is called on a city, that city should call " + 
            "ExpansionLogic's GetNextTileToPursue method to determine the tile it's pursuing")]
        public void PerformExpansion_ChecksExpansionLogicForTileToPursue() {
            var city = Container.Resolve<City>();

            var tile = new Mock<IMapTile>().Object;

            ExpansionMock.Setup(logic => logic.GetNextTileToPursue(city)).Returns(tile);

            city.PerformExpansion();

            ExpansionMock.Verify(logic => logic.GetNextTileToPursue(city), Times.AtLeastOnce,
                "ExpansionLogic's GetNextTileToPursue method was never called");
            Assert.AreEqual(tile, city.TileBeingPursued, "City has an incorrect TileBeingPursued");
        }

        [Test(Description = "When PerformExpansion is called on a city, that city should call " +
            "ExpansionLogic's GetCultureCostOfAcquiringTile method to determine whether it can " +
            "seize its TileBeingPursued or not")]
        public void PerformExpansion_ChecksExpansionLogicForTileAcquisition() {
            var city = Container.Resolve<City>();

            var tile = new Mock<IMapTile>().Object;

            ExpansionMock.Setup(logic => logic.GetNextTileToPursue(city)).Returns(tile);

            city.PerformExpansion();

            ExpansionMock.Verify(logic => logic.GetCultureCostOfAcquiringTile(city, tile), Times.AtLeastOnce,
                "ExpansionLogic's GetCultureCostOfAcquiringTile method was never called");
        }

        [Test(Description = "When PerformExpansion is called on a city, that city should call " + 
            "PossessionCanon's CanChangeOwnerOfTile method to determine whether it can seize " + 
            "its TileBeingPursued or not")]
        public void PerformExpansion_ChecksPosessionCanonForTileAcquisition() {
            var city = Container.Resolve<City>();

            var tile = new Mock<IMapTile>().Object;

            ExpansionMock.Setup(logic => logic.GetNextTileToPursue(city)).Returns(tile);
            ExpansionMock.Setup(logic => logic.IsTileAvailable(city, tile)).Returns(true);

            city.PerformExpansion();

            TilePossessionCanonMock.Verify(canon => canon.CanChangeOwnerOfTile(tile, city), Times.AtLeastOnce, 
                "PossessionCanon's CanChangeOwnerOfTile method was never called");
        }

        [Test(Description = "When PerformExpansion is called on a city that can acquire its " +
            "TileBeingPursued, it should check ExpansionLogic to determine how much its culture " +
            "stockpile should be decreased, and decrease the stockpile accordingly")]
        public void PerformExpansion_ChecksExpansionLogicForCultureStockpileDecrease() {
            var city = Container.Resolve<City>();
            city.CultureStockpile = 10;

            var tile = new Mock<IMapTile>().Object;

            ExpansionMock.Setup(logic => logic.GetNextTileToPursue(city)).Returns(tile);
            ExpansionMock.Setup(logic => logic.IsTileAvailable(city, tile)).Returns(true);
            ExpansionMock.Setup(logic => logic.GetCultureCostOfAcquiringTile(city, tile)).Returns(7);

            TilePossessionCanonMock.Setup(canon => canon.CanChangeOwnerOfTile(tile, city)).Returns(true);

            city.PerformExpansion();

            Assert.AreEqual(3, city.CultureStockpile, "CultureStockpile has an incorrect value");
        }

        [Test(Description = "When PerformExpansion is called on a city that can acquire its " +
            "TileBeingPursued, it should request the ownership change of that tile from " +
            "PossessionCanon")]
        public void PerformExpansion_RequestsOwnershipChangeFromPossessionCanon() {
            var city = Container.Resolve<City>();

            var tile = new Mock<IMapTile>().Object;

            ExpansionMock.Setup(logic => logic.GetNextTileToPursue(city)).Returns(tile);
            ExpansionMock.Setup(logic => logic.IsTileAvailable(city, tile)).Returns(true);
            ExpansionMock.Setup(logic => logic.GetCultureCostOfAcquiringTile(city, tile)).Returns(0);

            TilePossessionCanonMock.Setup(canon => canon.CanChangeOwnerOfTile(tile, city)).Returns(true);

            city.PerformExpansion();

            TilePossessionCanonMock.Verify(canon => canon.ChangeOwnerOfTile(tile, city), Times.Once, 
                "Did not receive the expected ChangeOwnerOfTile call on PossessionCanon");
        }

        [Test(Description = "When PerformExpansion is called on a city that can acquire its " +
            "TileBeingPursued, it should check ExpansionLogic to determine the new tile it " +
            "should be pursuing")]
        public void PerformExpansion_ChecksExpansionLogicAfterAcquisition() {
            var city = Container.Resolve<City>();

            var firstTile = new Mock<IMapTile>().Object;
            var secondTile = new Mock<IMapTile>().Object;

            ExpansionMock.SetupSequence(logic => logic.GetNextTileToPursue(city))
                .Returns(firstTile)
                .Returns(secondTile);

            ExpansionMock.Setup(logic => logic.IsTileAvailable(city, firstTile)).Returns(true);
            ExpansionMock.Setup(logic => logic.GetCultureCostOfAcquiringTile(city, firstTile)).Returns(0);

            TilePossessionCanonMock.Setup(canon => canon.CanChangeOwnerOfTile(firstTile, city)).Returns(true);

            city.PerformExpansion();

            Assert.AreEqual(secondTile, city.TileBeingPursued, "A configuration that should've led to a successful " +
                "tile acquisition did not change the TileBeingPursued of the city to the next tile to pursue");
        }

        [Test(Description = "When PerformExpansion is called and ExpansionLogic returns a " +
            "null Tile to pursue, PerformExpansion should handle this gracefully and not throw any exceptions")]
        public void PerformExpansion_DoesNotThrowOnNullPursuit() {
            var city = Container.Resolve<City>();

            ExpansionMock.Setup(logic => logic.GetNextTileToPursue(city)).Returns<IMapTile>(null);

            Assert.DoesNotThrow(() => city.PerformExpansion(),
                "An exception was thrown when calling PerformExpansion on a null TileToPursue, even though this should be valid");

            Assert.IsNull(city.TileBeingPursued, "TileBeingPursued has an incorrect value");
        }

        [Test(Description = "When PerformDistribution is called on a city, that city should " + 
            "call into DistributionLogic with the correct city, worker count, and preferences")]
        public void PerformDistribution_SendsRequestToDistributionLogic() {
            var city = Container.Resolve<City>();
            city.Population = 7;
            city.ResourceFocus = ResourceFocusType.TotalYield;

            TilePossessionCanonMock.Setup(canon => canon.GetTilesOfCity(city))
                .Returns(new List<IMapTile>() { BuildMockTile(new TileMockData()) }.AsReadOnly());

            BuildingPossessionCanonMock.Setup(canon => canon.GetBuildingsInCity(city)).Returns(new List<IBuilding>().AsReadOnly());

            city.PerformDistribution();

            DistributionMock.Verify(
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

            DistributionMock.Setup(logic => logic.GetSlotsAvailableToCity(city)).Returns(allSlots);

            DistributionMock.Setup(
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

            DistributionMock.Verify(logic => logic.GetSlotsAvailableToCity(city), Times.Once,
                "DistributionLogic.GetSlotsAvailableToCity was not called");
        }

        [Test(Description = "When PerformDistribution is called, City does not pass any tile slots " +
            "whose tiles have been marked for slot suppression")]
        public void PerformDistribution_SuppressedSlotsNotPassed() {
            var city = Container.Resolve<City>();

            var tileMockOne = new Mock<IMapTile>();
            tileMockOne.Setup(tile => tile.WorkerSlot).Returns(new Mock<IWorkerSlot>().Object);

            var tileMockTwo = new Mock<IMapTile>();
            tileMockTwo.Setup(tile => tile.WorkerSlot).Returns(new Mock<IWorkerSlot>().Object);
            tileMockTwo.Setup(tile => tile.SuppressSlot).Returns(true);

            var tileMockThree = new Mock<IMapTile>();
            tileMockThree.Setup(tile => tile.WorkerSlot).Returns(new Mock<IWorkerSlot>().Object);

            var tiles = new List<IMapTile>() {tileMockOne.Object, tileMockTwo.Object, tileMockThree.Object};

            TilePossessionCanonMock.Setup(canon => canon.GetTilesOfCity(city)).Returns(tiles);

            BuildingPossessionCanonMock.Setup(canon => canon.GetBuildingsInCity(city)).Returns(new List<IBuilding>().AsReadOnly());

            DistributionMock.Setup(
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

            var unlockedSlotTile         = BuildMockTile(new TileMockData());
            var lockedUnoccupiedSlotTile = BuildMockTile(new TileMockData() { SlotIsLocked = true });
            var lockedOccupiedSlotTile   = BuildMockTile(new TileMockData() { SlotIsLocked = true, SlotIsOccupied = true });

            var tiles = new List<IMapTile>() { unlockedSlotTile, lockedUnoccupiedSlotTile, lockedOccupiedSlotTile };

            TilePossessionCanonMock
                .Setup(canon => canon.GetTilesOfCity(city))
                .Returns(tiles);

            BuildingPossessionCanonMock
                .Setup(canon => canon.GetBuildingsInCity(city))
                .Returns(new List<IBuilding>().AsReadOnly());

            DistributionMock.Setup(
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

            ResourceGenerationMock.Setup(logic => logic.GetTotalYieldForCity(city)).Returns(income);

            city.PerformIncome();

            ResourceGenerationMock.Verify(logic => logic.GetTotalYieldForCity(city), Times.AtLeastOnce,
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

            ResourceGenerationMock.Setup(logic => logic.GetTotalYieldForCity(city)).Returns(ResourceSummary.Empty);

            GrowthMock.Setup(logic => logic.GetFoodConsumptionPerTurn(city)).Returns(4);

            city.PerformIncome();

            GrowthMock.Verify(logic => logic.GetFoodConsumptionPerTurn(city), Times.AtLeastOnce,
                "GrowthLogic's GetFoodConsumptionPerTurn was not called as expected");

            Assert.AreEqual(1, city.FoodStockpile, "City did not correctly update its FoodStockpile");
        }

        [Test(Description = "When OnPointerClick is called, City should fire the CityClicked signal with " +
            "the appropriate arguments")]
        public void OnPointerClickCalled_FiresClickedSignal() {
            var cityToTest = Container.Resolve<City>();

            var dataToPass = new PointerEventData(EventSystem.current);

            var citySignals = Container.Resolve<CitySignals>();

            citySignals.CityClickedSignal.Subscribe(delegate(ICity city) {
                Assert.AreEqual(city, cityToTest, "ClickedSignal was passed the wrong city");
                Assert.Pass();
            });

            cityToTest.OnPointerClick(dataToPass);
            Assert.Fail("ClickedSignal was never fired");
        }

        [Test(Description = "When SetActiveProductionProject is called on an IBuildingTemplate, " +
            "City should fire ProjectChangedSignal with the appropriate arguments")]
        public void SetActiveProductionProject_OnBuildingTemplate_FiresProjectChangedSignal() {
            var cityToTest = Container.Resolve<City>();

            var newTemplateMock = new Mock<IBuildingTemplate>();
            newTemplateMock.Setup(template => template.name).Returns("New Template");

            var mockProject = new Mock<IProductionProject>();
            mockProject.Setup(project => project.Name).Returns(newTemplateMock.Name);

            ProjectFactoryMock.Setup(factory => factory.ConstructBuildingProject(newTemplateMock.Object)).Returns(mockProject.Object);

            var citySignals = Container.Resolve<CitySignals>();

            citySignals.ProjectChangedSignal.Listen(delegate(ICity city, IProductionProject project) {
                Assert.AreEqual(city, cityToTest, "ClickedSignal was passed the wrong city");

                Assert.AreEqual(mockProject.Object, project, "ClickedSignal was passed an unexpected project");
                Assert.Pass();
            });

            cityToTest.SetActiveProductionProject(newTemplateMock.Object);
        }

        [Test(Description = "When SetActiveProductionProject is called on an IUnitTemplate, " +
            "City should fire ProjectChangedSignal with the appropriate arguments")]
        public void SetActiveProductionProject_OnUnitTemplate_FiresProjectChangedSignal() {
            var cityToTest = Container.Resolve<City>();

            var newTemplateMock = new Mock<IUnitTemplate>();
            newTemplateMock.Setup(template => template.Name).Returns("New Template");

            var mockProject = new Mock<IProductionProject>();
            mockProject.Setup(project => project.Name).Returns(newTemplateMock.Name);

            ProjectFactoryMock.Setup(factory => factory.ConstructUnitProject(newTemplateMock.Object)).Returns(mockProject.Object);

            var citySignals = Container.Resolve<CitySignals>();

            citySignals.ProjectChangedSignal.Listen(delegate(ICity city, IProductionProject project) {
                Assert.AreEqual(city, cityToTest, "ClickedSignal was passed the wrong city");

                Assert.AreEqual(mockProject.Object, project, "ClickedSignal was passed an unexpected project");
                Assert.Pass();
            });

            cityToTest.SetActiveProductionProject(newTemplateMock.Object);
        }

        #region utilities

        private struct TileMockData {

            public bool SuppressSlot;
            public bool SlotIsLocked;
            public bool SlotIsOccupied;
            public ResourceSummary BaseYield;

        }

        private IMapTile BuildMockTile(TileMockData mockData) {
            var mockTile = new Mock<IMapTile>();

            var mockSlot = new Mock<IWorkerSlot>();
            mockSlot.SetupAllProperties();
            mockSlot.Object.IsLocked = mockData.SlotIsLocked;
            mockSlot.Object.IsOccupied = mockData.SlotIsOccupied;
            mockSlot.Setup(slot => slot.BaseYield).Returns(mockData.BaseYield);

            mockTile.Setup(tile => tile.WorkerSlot).Returns(mockSlot.Object);
            mockTile.Setup(tile => tile.SuppressSlot).Returns(mockData.SuppressSlot);

            return mockTile.Object;
        }

        #endregion

    }

}
