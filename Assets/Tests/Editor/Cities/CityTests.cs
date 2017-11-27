using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Text;

using UnityEngine;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation;

using Assets.Simulation.GameMap;

using Assets.Simulation.Cities;
using Assets.Simulation.Cities.Growth;
using Assets.Simulation.Cities.ResourceGeneration;
using Assets.Simulation.Cities.Territory;
using Assets.Simulation.Cities.Distribution;
using Assets.Simulation.Cities.Buildings;
using Assets.Simulation.Cities.Production;

namespace Assets.Tests.Cities {

    [TestFixture]
    public class CityTests : ZenjectUnitTestFixture {

        private Mock<IPopulationGrowthLogic>    GrowthMock;
        private Mock<IProductionLogic>          ProductionMock;
        private Mock<IResourceGenerationLogic>  ResourceGenerationMock;
        private Mock<IBorderExpansionLogic>     ExpansionMock;
        private Mock<ITilePossessionCanon>      TilePossessionCanonMock;
        private Mock<IWorkerDistributionLogic>  DistributionMock;
        private Mock<IBuildingPossessionCanon>  BuildingPossessionCanonMock;
        private Mock<ICityEventBroadcaster>     EventBroadcasterMock;
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
            EventBroadcasterMock        = new Mock<ICityEventBroadcaster>();
            ProjectFactoryMock          = new Mock<IProductionProjectFactory>();

            Container.Bind<IPopulationGrowthLogic>   ().FromInstance(GrowthMock                 .Object);
            Container.Bind<IProductionLogic>         ().FromInstance(ProductionMock             .Object);
            Container.Bind<IResourceGenerationLogic> ().FromInstance(ResourceGenerationMock     .Object);
            Container.Bind<IBorderExpansionLogic>    ().FromInstance(ExpansionMock              .Object);
            Container.Bind<ITilePossessionCanon>     ().FromInstance(TilePossessionCanonMock    .Object);
            Container.Bind<IWorkerDistributionLogic> ().FromInstance(DistributionMock           .Object);
            Container.Bind<IBuildingPossessionCanon> ().FromInstance(BuildingPossessionCanonMock.Object);
            Container.Bind<ICityEventBroadcaster>    ().FromInstance(EventBroadcasterMock       .Object);
            Container.Bind<IProductionProjectFactory>().FromInstance(ProjectFactoryMock         .Object);

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
            city.DistributionPreferences = new DistributionPreferences(false, ResourceType.Food);

            TilePossessionCanonMock.Setup(canon => canon.GetTilesOfCity(city))
                .Returns(new List<IMapTile>() { new Mock<IMapTile>().Object }.AsReadOnly());

            BuildingPossessionCanonMock.Setup(canon => canon.GetBuildingsInCity(city)).Returns(new List<IBuilding>().AsReadOnly());

            city.PerformDistribution();

            DistributionMock.Verify(
                logic => logic.DistributeWorkersIntoSlots(
                    city.Population, It.IsAny<List<IWorkerSlot>>(), city, city.DistributionPreferences
                ),
                Times.Once,
                "DistributionLogic's DistributeWorkersIntoSlots was not called with the correct " +
                "population, city, and DistributionPreferences"
            );
        }

        [Test(Description = "When PerformDistribution is called on a city, that city should " +
            "send DistributionLogic all of the slots it receives from the tiles TilePossessionCanon " +
            "says it possesses")]
        public void PerformDistribution_DistributionLogicGivenAllTileSlots() {
            var city = Container.Resolve<City>();

            var tileMockOne = new Mock<IMapTile>();
            tileMockOne.Setup(tile => tile.WorkerSlot).Returns(new Mock<IWorkerSlot>().Object);

            var tileMockTwo = new Mock<IMapTile>();
            tileMockTwo.Setup(tile => tile.WorkerSlot).Returns(new Mock<IWorkerSlot>().Object);

            var tileMockThree = new Mock<IMapTile>();
            tileMockThree.Setup(tile => tile.WorkerSlot).Returns(new Mock<IWorkerSlot>().Object);

            var tiles = new List<IMapTile>() {tileMockOne.Object, tileMockTwo.Object, tileMockThree.Object};
            var tileSlots = tiles.Select(tile => tile.WorkerSlot);

            TilePossessionCanonMock.Setup(canon => canon.GetTilesOfCity(city)).Returns(tiles);

            BuildingPossessionCanonMock.Setup(canon => canon.GetBuildingsInCity(city)).Returns(new List<IBuilding>().AsReadOnly());

            city.PerformDistribution();

            DistributionMock.Verify(
                logic => logic.DistributeWorkersIntoSlots(
                    It.IsAny<int>(),
                    It.Is<IEnumerable<IWorkerSlot>>(enumerable => new HashSet<IWorkerSlot>(enumerable).IsSupersetOf(tileSlots)),
                    It.IsAny<ICity>(),
                    It.IsAny<DistributionPreferences>()
                ),
                Times.Once, "The Enumerable passed into DistributionLogic's DistributeWorkersIntoSlots method " + 
                "is not a superset of all slots contained within the city's tiles"
            );
        }

        [Test(Description = "When PerformDistribution is called on a city, that city should " +
            "send DistributionLogic all of the slots it receives from the buildings BuildingPossessionCanon " +
            "says it possesses")]
        public void PerformDistribution_DistributionLogicGivenAllBuildingSlots() {
            var city = Container.Resolve<City>();

            TilePossessionCanonMock.Setup(canon => canon.GetTilesOfCity(city)).Returns(new List<IMapTile>().AsReadOnly());

            var buildingMockOne = new Mock<IBuilding>();
            var firstSlots = new List<IWorkerSlot>() { new Mock<IWorkerSlot>().Object };
            buildingMockOne.Setup(building => building.Slots).Returns(firstSlots.AsReadOnly());

            var buildingMockTwo = new Mock<IBuilding>();
            var secondSlots = new List<IWorkerSlot>() { new Mock<IWorkerSlot>().Object, new Mock<IWorkerSlot>().Object };
            buildingMockTwo.Setup(building => building.Slots).Returns(secondSlots.AsReadOnly());

            var buildingMockThree = new Mock<IBuilding>();
            var thirdSlots = new List<IWorkerSlot>() { new Mock<IWorkerSlot>().Object, new Mock<IWorkerSlot>().Object, new Mock<IWorkerSlot>().Object };
            buildingMockThree.Setup(building => building.Slots).Returns(thirdSlots.AsReadOnly());

            var allBuildings = new List<IBuilding>() { buildingMockOne.Object, buildingMockTwo.Object, buildingMockThree.Object };
            var allSlots = new List<IWorkerSlot>(firstSlots.Concat(secondSlots).Concat(thirdSlots));

            BuildingPossessionCanonMock.Setup(canon => canon.GetBuildingsInCity(city)).Returns(allBuildings.AsReadOnly());

            DistributionMock.Setup(
                logic => logic.DistributeWorkersIntoSlots(
                    It.IsAny<int>(),
                    It.IsAny<IEnumerable<IWorkerSlot>>(),
                    It.IsAny<ICity>(),
                    It.IsAny<DistributionPreferences>()
                )
            ).Callback(
                delegate(int workers, IEnumerable<IWorkerSlot> availableSlots, ICity calledCity, DistributionPreferences preferences) {
                    CollectionAssert.IsSupersetOf(availableSlots, allSlots);
                }
            );                

            city.PerformDistribution();
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

        [Test(Description = "When OnPointerClick is called, City should call EventBroadcaster.BroadcastCityClick " +
            "on itself and the argued PointerEventData")]
        public void OnPointerClickCalled_SendsEventToBroadcaster() {
            throw new NotImplementedException();
        }

    }

}
