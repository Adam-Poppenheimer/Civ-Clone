using System;
using System.Linq;
using System.Collections.Generic;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation;

using Assets.Simulation.GameMap;

using Assets.Simulation.Cities;
using Assets.Simulation.Cities.ResourceGeneration;
using Assets.Simulation.Cities.Growth;
using Assets.Simulation.Cities.Distribution;
using Assets.Simulation.Cities.Buildings;
using Assets.Simulation.Cities.Territory;

namespace Assets.Tests.Simulation.Cities {

    [TestFixture]
    public class WorkerDistributionLogicTests : ZenjectUnitTestFixture {

        private Mock<IPopulationGrowthLogic> GrowthLogicMock;
        private Mock<IResourceGenerationLogic> GenerationLogicMock;

        private Mock<IBuildingPossessionCanon> BuildingCanonMock;
        private Mock<ITilePossessionCanon> TileCanonMock;

        private List<IWorkerSlot> AllSlots = new List<IWorkerSlot>();

        private List<IMapTile> AllTiles = new List<IMapTile>();

        private List<IBuilding> AllBuildings = new List<IBuilding>();

        [SetUp]
        public void CommonInstall() {
            AllSlots.Clear();
            AllTiles.Clear();
            AllBuildings.Clear();

            GrowthLogicMock = new Mock<IPopulationGrowthLogic>();
            GrowthLogicMock.Setup(logic => logic.GetFoodConsumptionPerTurn(It.IsAny<ICity>())).Returns(0);

            GenerationLogicMock = new Mock<IResourceGenerationLogic>();
            GenerationLogicMock
                .Setup(logic => logic.GetYieldOfSlotForCity(It.IsAny<IWorkerSlot>(), It.IsAny<ICity>()))
                .Returns<IWorkerSlot, ICity>((slot, city) => slot.BaseYield);

            GenerationLogicMock
                .Setup(logic => logic.GetTotalYieldForCity(It.IsAny<ICity>()))
                .Returns(
                    () => AllSlots
                        .Where(slot => slot.IsOccupied)
                        .Select(slot => slot.BaseYield)
                        .Aggregate((x, y) => x + y)
                );

            TileCanonMock = new Mock<ITilePossessionCanon>();
            TileCanonMock
                .Setup(canon => canon.GetTilesOfCity(It.IsAny<ICity>()))
                .Returns(() => AllTiles);

            BuildingCanonMock = new Mock<IBuildingPossessionCanon>();
            BuildingCanonMock
                .Setup(canon => canon.GetBuildingsInCity(It.IsAny<ICity>()))
                .Returns(() => AllBuildings.AsReadOnly());

            Container.Bind<IPopulationGrowthLogic>()  .FromInstance(GrowthLogicMock.Object);
            Container.Bind<IResourceGenerationLogic>().FromInstance(GenerationLogicMock.Object);
            Container.Bind<ITilePossessionCanon>()    .FromInstance(TileCanonMock.Object);
            Container.Bind<IBuildingPossessionCanon>().FromInstance(BuildingCanonMock.Object);

            var mockCity = new Mock<ICity>();
            mockCity.SetupAllProperties();

            Container.Bind<ICity>().FromInstance(mockCity.Object);

            Container.Bind<WorkerDistributionLogic>().AsSingle();
        }

        [Test(Description = "When a resource focus is being requested, the distribution should attempt to maximize the total " +
            "yield for that resource")]
        public void ResourceFocusRequested_MaximizesYieldOfResource() {
            var preferences = ResourceFocusType.Culture;

            var slotOne   = BuildSlot(false, new ResourceSummary(culture: 3));
            var slotTwo   = BuildSlot(false, new ResourceSummary(culture: 2));
            var slotThree = BuildSlot(false, new ResourceSummary(culture: 1, gold: 2));
            var slotFour  = BuildSlot(false, new ResourceSummary(culture: 0, gold: 3));
            var slotFive  = BuildSlot(false, new ResourceSummary(culture: 0, gold: 3, production: 3));

            var distributionLogic = Container.Resolve<WorkerDistributionLogic>();
            var city = Container.Resolve<ICity>();

            distributionLogic.DistributeWorkersIntoSlots(3, AllSlots, city, preferences);

            Assert.IsTrue (slotOne.IsOccupied,   "Unexpected SlotOne IsOccupied value");
            Assert.IsTrue (slotTwo.IsOccupied,   "Unexpected SlotTwo IsOccupied value");
            Assert.IsTrue (slotThree.IsOccupied, "Unexpected SlotThree IsOccupied value");
            Assert.IsFalse(slotFour.IsOccupied,  "Unexpected SlotFour IsOccupied value");
            Assert.IsFalse(slotFive.IsOccupied,  "Unexpected SlotFive IsOccupied value");
        }

        [Test(Description = "When a resource focus is being requested and there are several ways to maximize yield for that resource " +
            "the distribution should pick the maximal configuration with the greatest total yield")]
        public void ResourceFocusRequested_ConsidersTotalYield() {
            var preferences = ResourceFocusType.Culture;

            var slotOne   = BuildSlot(false, new ResourceSummary(culture: 3));
            var slotTwo   = BuildSlot(false, new ResourceSummary(culture: 2));
            var slotThree = BuildSlot(false, new ResourceSummary(culture: 1, gold: 0));
            var slotFour  = BuildSlot(false, new ResourceSummary(culture: 1, gold: 1));
            var slotFive  = BuildSlot(false, new ResourceSummary(culture: 1, gold: 1, production: 1));

            var distributionLogic = Container.Resolve<WorkerDistributionLogic>();
            var city = Container.Resolve<ICity>();

            distributionLogic.DistributeWorkersIntoSlots(3, AllSlots, city, preferences);

            Assert.IsTrue (slotOne.IsOccupied,   "Unexpected SlotOne IsOccupied value");
            Assert.IsTrue (slotTwo.IsOccupied,   "Unexpected SlotTwo IsOccupied value");
            Assert.IsFalse(slotThree.IsOccupied, "Unexpected SlotThree IsOccupied value");
            Assert.IsFalse(slotFour.IsOccupied,  "Unexpected SlotFour IsOccupied value");
            Assert.IsTrue (slotFive.IsOccupied,  "Unexpected SlotFive IsOccupied value");
        }

        [Test(Description = "When no resource focus is being requested, the distribution should attempt to maximize the total " + 
            "yield of all resources")]
        public void ResourceFocusNotRequested_MaximizesTotalYield() {
            var preferences = ResourceFocusType.TotalYield;

            var slotOne   = BuildSlot(false, new ResourceSummary(culture: 3));
            var slotTwo   = BuildSlot(false, new ResourceSummary(culture: 2));
            var slotThree = BuildSlot(false, new ResourceSummary(culture: 1, food: 2, production: 1, gold: 2));
            var slotFour  = BuildSlot(false, new ResourceSummary(culture: 1, gold: 4));
            var slotFive  = BuildSlot(false, new ResourceSummary(culture: 1, gold: 3, production: 1));

            var distributionLogic = Container.Resolve<WorkerDistributionLogic>();
            var city = Container.Resolve<ICity>();

            distributionLogic.DistributeWorkersIntoSlots(3, AllSlots, city, preferences);

            Assert.IsFalse(slotOne.IsOccupied,   "Unexpected SlotOne IsOccupied value");
            Assert.IsFalse(slotTwo.IsOccupied,   "Unexpected SlotTwo IsOccupied value");
            Assert.IsTrue (slotThree.IsOccupied, "Unexpected SlotThree IsOccupied value");
            Assert.IsTrue (slotFour.IsOccupied,  "Unexpected SlotFour IsOccupied value");
            Assert.IsTrue (slotFive.IsOccupied,  "Unexpected SlotFive IsOccupied value");
        }

        [Test(Description = "Focused distribution should attempt to provide enough food for the given population to sustain itself")]
        public void FocusedDistribution_TriesToPreventStarvation() {
            var preferences = ResourceFocusType.Culture;

            var slotOne   = BuildSlot(false, new ResourceSummary(culture: 3));
            var slotTwo   = BuildSlot(false, new ResourceSummary(culture: 2));
            var slotThree = BuildSlot(false, new ResourceSummary(culture: 1, food: 1));
            var slotFour  = BuildSlot(false, new ResourceSummary(culture: 0, food: 1));
            var slotFive  = BuildSlot(false, new ResourceSummary(culture: 0, food: 2));

            GrowthLogicMock.Setup(logic => logic.GetFoodConsumptionPerTurn(It.IsAny<ICity>())).Returns(3);

            var distributionLogic = Container.Resolve<WorkerDistributionLogic>();
            var city = Container.Resolve<ICity>();

            distributionLogic.DistributeWorkersIntoSlots(3, AllSlots, city, preferences);

            Assert.IsTrue (slotOne.IsOccupied,   "Unexpected SlotOne IsOccupied value");
            Assert.IsFalse(slotTwo.IsOccupied,   "Unexpected SlotTwo IsOccupied value");
            Assert.IsTrue (slotThree.IsOccupied, "Unexpected SlotThree IsOccupied value");
            Assert.IsFalse(slotFour.IsOccupied,  "Unexpected SlotFour IsOccupied value");
            Assert.IsTrue (slotFive.IsOccupied,  "Unexpected SlotFive IsOccupied value");
        }

        [Test(Description = "Unfocused distribution should attempt to provide enough food for the given population to sustain itself")]
        public void UnfocusedDistribution_TriesToPreventStarvation() {
            var preferences = ResourceFocusType.TotalYield;

            var slotOne   = BuildSlot(false, new ResourceSummary(culture: 3, food: 1));
            var slotTwo   = BuildSlot(false, new ResourceSummary(culture: 2, food: 1));
            var slotThree = BuildSlot(false, new ResourceSummary(culture: 1, food: 2, production: 1));
            var slotFour  = BuildSlot(false, new ResourceSummary(culture: 1, gold: 5));
            var slotFive  = BuildSlot(false, new ResourceSummary(culture: 1, gold: 3, production: 1));

            GrowthLogicMock.Setup(logic => logic.GetFoodConsumptionPerTurn(It.IsAny<ICity>())).Returns(3);

            var distributionLogic = Container.Resolve<WorkerDistributionLogic>();
            var city = Container.Resolve<ICity>();

            distributionLogic.DistributeWorkersIntoSlots(3, AllSlots, city, preferences);

            Assert.IsTrue (slotOne.IsOccupied,   "Unexpected SlotOne IsOccupied value");
            Assert.IsFalse(slotTwo.IsOccupied,   "Unexpected SlotTwo IsOccupied value");
            Assert.IsTrue (slotThree.IsOccupied, "Unexpected SlotThree IsOccupied value");
            Assert.IsTrue (slotFour.IsOccupied,  "Unexpected SlotFour IsOccupied value");
            Assert.IsFalse(slotFive.IsOccupied,  "Unexpected SlotFive IsOccupied value");
        }

        [Test(Description = "The previous distribution of workers should have no effect on the calculated distribution")]
        public void AllDistributions_IgnorePreviousDistribution() {
            var preferences = ResourceFocusType.Culture;

            var slotOne   = BuildSlot(false, new ResourceSummary(culture: 3));
            var slotTwo   = BuildSlot(true,  new ResourceSummary(culture: 2));
            var slotThree = BuildSlot(true,  new ResourceSummary(culture: 1));
            var slotFour  = BuildSlot(true,  new ResourceSummary(culture: 0));
            var slotFive  = BuildSlot(true,  new ResourceSummary(culture: 0));

            var distributionLogic = Container.Resolve<WorkerDistributionLogic>();
            var city = Container.Resolve<ICity>();

            distributionLogic.DistributeWorkersIntoSlots(3, AllSlots, city, preferences);

            Assert.IsTrue (slotOne.IsOccupied,   "Unexpected SlotOne IsOccupied value");
            Assert.IsTrue (slotTwo.IsOccupied,   "Unexpected SlotTwo IsOccupied value");
            Assert.IsTrue (slotThree.IsOccupied, "Unexpected SlotThree IsOccupied value");
            Assert.IsFalse(slotFour.IsOccupied,  "Unexpected SlotFour IsOccupied value");
            Assert.IsFalse(slotFive.IsOccupied,  "Unexpected SlotFive IsOccupied value");
        }

        [Test(Description = "No distribution should result in more occupied slots than there are people to fill them")]
        public void AllDistributions_FillCorrectNumberOfSlots() {
            for(int i = 0; i < 15; ++i) {
                BuildSlot(false, ResourceSummary.Empty);
            }

            var distributionLogic = Container.Resolve<WorkerDistributionLogic>();
            var city = Container.Resolve<ICity>();

            distributionLogic.DistributeWorkersIntoSlots(8, AllSlots, city, ResourceFocusType.Culture);

            Assert.AreEqual(8, AllSlots.Where(slot => slot.IsOccupied).Count(), "Unexpected number of occupied slots after focused distribution");

            distributionLogic.DistributeWorkersIntoSlots(7, AllSlots, city, ResourceFocusType.TotalYield);

            Assert.AreEqual(7, AllSlots.Where(slot => slot.IsOccupied).Count(), "Unexpected number of occupied slots after unfocused distribution");
        }

        [Test(Description = "When there are fewer slots than people to fill them, the distribution should consider this " +
            "a valid state and throw no errors")]
        public void AllDistributions_HandleInsufficientSlotsGracefully() {
            for(int i = 0; i < 10; ++i) {
                BuildSlot(false, ResourceSummary.Empty);
            }

            var distributionLogic = Container.Resolve<WorkerDistributionLogic>();
            var city = Container.Resolve<ICity>();

            Assert.DoesNotThrow(
                () => distributionLogic.DistributeWorkersIntoSlots(20, AllSlots, city, ResourceFocusType.Culture),
                "DistributeWorkersIntoSlots falsely threw an error when given focused distribution orders"
            );

            Assert.AreEqual(10, AllSlots.Where(slot => slot.IsOccupied).Count(), "Unexpected number of occupied slots after focused distribution");

            Assert.DoesNotThrow(
                () => distributionLogic.DistributeWorkersIntoSlots(20, AllSlots, city, ResourceFocusType.TotalYield),
                "DistributeWorkersIntoSlots falsely threw an error when given unfocused distribution orders"
            );

            Assert.AreEqual(10, AllSlots.Where(slot => slot.IsOccupied).Count(), "Unexpected number of occupied slots after unfocused distribution");
        }

        [Test(Description = "When GetUnemployedPeopleInCity is called, it returns the population of " +
            "the city minus the number of occupied slots in tiles and buildings under its possession")]
        public void GetUnemployedPeopleInCity_ConsidersTileAndBuildingSlots() {
            BuildMapTile(false, BuildSlot(true,  ResourceSummary.Empty));
            BuildMapTile(false, BuildSlot(false, ResourceSummary.Empty));

            BuildBuilding(BuildSlot(true,  ResourceSummary.Empty));
            BuildBuilding(BuildSlot(false, ResourceSummary.Empty));
            BuildBuilding(BuildSlot(true,  ResourceSummary.Empty), BuildSlot(false, ResourceSummary.Empty));

            var city = Container.Resolve<ICity>();
            city.Population = 5;

            var distributionLogic = Container.Resolve<WorkerDistributionLogic>();

            Assert.AreEqual(2, distributionLogic.GetUnemployedPeopleInCity(city),
                "GetUnemployedPeopleInCity returned an unexpected value");
        }

        [Test(Description = "When GetUnemployedPeopleInCity is called, it throws a NegativeUnemploymentException if " +
            "there are more occupied slots than there are people in the city")]
        public void GetUnemployedPeopleInCity_ThrowsOnOverAssignment() {
            BuildMapTile(false, BuildSlot(true,  ResourceSummary.Empty));

            BuildBuilding(BuildSlot(true,  ResourceSummary.Empty));

            var city = Container.Resolve<ICity>();
            city.Population = 1;

            var distributionLogic = Container.Resolve<WorkerDistributionLogic>();

            Assert.Throws<NegativeUnemploymentException>(() => distributionLogic.GetUnemployedPeopleInCity(city),
                "GetUnemployedPeopleInCity failed to throw a NegativeUnemploymentException");
        }

        [Test(Description = "When GetSlotsAvailableToCity is called, it returns a list containing " +
            "all slots from all buildings and all slots from tiles that aren't suppressing their slots")]
        public void GetSlotsAvailableToCity_ConsidersTilesAndBuildings() {
            BuildMapTile(false, BuildSlot(true,  ResourceSummary.Empty));
            BuildMapTile(false, BuildSlot(false, ResourceSummary.Empty));
            var suppressingTile = BuildMapTile(true,  BuildSlot(true,  ResourceSummary.Empty));

            BuildBuilding(BuildSlot(true,  ResourceSummary.Empty));
            BuildBuilding(BuildSlot(false, ResourceSummary.Empty));
            BuildBuilding(BuildSlot(true,  ResourceSummary.Empty), BuildSlot(false, ResourceSummary.Empty));

            var city = Container.Resolve<ICity>();

            var distributionLogic = Container.Resolve<WorkerDistributionLogic>();

            var availableSlots = distributionLogic.GetSlotsAvailableToCity(city);

            CollectionAssert.AreEquivalent(
                AllSlots.Except(new List<IWorkerSlot>(){ suppressingTile.WorkerSlot }),
                availableSlots,
                "GetSlotsAvailableToCity did not contain the expected slots"
            );
        }

        private IWorkerSlot BuildSlot(bool isOccupied, ResourceSummary yield) {
            var mockSlot = new Mock<IWorkerSlot>();
            mockSlot.SetupAllProperties();

            mockSlot.Object.IsOccupied = isOccupied;
            mockSlot.Setup(slot => slot.BaseYield).Returns(yield);

            AllSlots.Add(mockSlot.Object);

            return mockSlot.Object;
        }

        private IMapTile BuildMapTile(bool suppressSlot, IWorkerSlot slot) {
            var mockTile = new Mock<IMapTile>();
            mockTile.SetupAllProperties();

            mockTile.Object.SuppressSlot = suppressSlot;
            mockTile.Setup(tile => tile.WorkerSlot).Returns(slot);

            AllTiles.Add(mockTile.Object);

            return mockTile.Object;
        }

        private IBuilding BuildBuilding(params IWorkerSlot[] slots) {
            var mockBuilding = new Mock<IBuilding>();

            mockBuilding.Setup(building => building.Slots).Returns(slots.ToList().AsReadOnly());

            AllBuildings.Add(mockBuilding.Object);

            return mockBuilding.Object;
        }

    }

}

