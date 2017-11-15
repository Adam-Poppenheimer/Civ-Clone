using System;
using System.Linq;

using Zenject;
using NUnit.Framework;
using Moq;

namespace Assets.Cities.Editor {

    [TestFixture]
    public class WorkerDistributionLogicTests : ZenjectUnitTestFixture {

        private Mock<IPopulationGrowthLogic> GrowthLogicMock;
        private Mock<IResourceGenerationLogic> GenerationLogicMock;

        private IWorkerSlot SlotOne;
        private IWorkerSlot SlotTwo;
        private IWorkerSlot SlotThree;
        private IWorkerSlot SlotFour;
        private IWorkerSlot SlotFive;

        [SetUp]
        public void CommonInstall() {
            var slotMockOne   = new Mock<IWorkerSlot>();
            var slotMockTwo   = new Mock<IWorkerSlot>();
            var slotMockThree = new Mock<IWorkerSlot>();
            var slotMockFour  = new Mock<IWorkerSlot>();
            var slotMockFive  = new Mock<IWorkerSlot>();

            slotMockOne  .SetupAllProperties();
            slotMockTwo  .SetupAllProperties();
            slotMockThree.SetupAllProperties();
            slotMockFour .SetupAllProperties();
            slotMockFive .SetupAllProperties();

            SlotOne   = slotMockOne.Object;
            SlotTwo   = slotMockTwo.Object;
            SlotThree = slotMockThree.Object;
            SlotFour  = slotMockFour.Object;
            SlotFive  = slotMockFive.Object;

            Container.Bind<IWorkerSlot>().FromInstance(SlotOne  );
            Container.Bind<IWorkerSlot>().FromInstance(SlotTwo  );
            Container.Bind<IWorkerSlot>().FromInstance(SlotThree);
            Container.Bind<IWorkerSlot>().FromInstance(SlotFour );
            Container.Bind<IWorkerSlot>().FromInstance(SlotFive );

            GrowthLogicMock = new Mock<IPopulationGrowthLogic>();
            GrowthLogicMock.Setup(logic => logic.GetFoodConsumptionPerTurn(It.IsAny<ICity>())).Returns(0);

            GenerationLogicMock = new Mock<IResourceGenerationLogic>();

            Container.Bind<IPopulationGrowthLogic>().FromInstance(GrowthLogicMock.Object);
            Container.Bind<IResourceGenerationLogic>().FromInstance(GenerationLogicMock.Object);

            Container.Bind<WorkplaceDistributionLogic>().AsSingle();
        }

        [Test(Description = "When a resource focus is being requested, the distribution should attempt to maximize the total " +
            "yield for that resource")]
        public void ResourceFocusRequested_MaximizesYieldOfResource() {
            var preferences = new DistributionPreferences(true, ResourceType.Culture);

            GenerationLogicMock.Setup(logic => logic.GetYieldOfSlotForCity(SlotOne, It.IsAny<ICity>())).Returns(
                new ResourceSummary(culture: 3)
            );

            GenerationLogicMock.Setup(logic => logic.GetYieldOfSlotForCity(SlotTwo, It.IsAny<ICity>())).Returns(
                new ResourceSummary(culture: 2)
            );

            GenerationLogicMock.Setup(logic => logic.GetYieldOfSlotForCity(SlotThree, It.IsAny<ICity>())).Returns(
                new ResourceSummary(culture: 1, gold: 2)
            );

            GenerationLogicMock.Setup(logic => logic.GetYieldOfSlotForCity(SlotFour, It.IsAny<ICity>())).Returns(
                new ResourceSummary(culture: 0, gold: 3)
            );

            GenerationLogicMock.Setup(logic => logic.GetYieldOfSlotForCity(SlotFive, It.IsAny<ICity>())).Returns(
                new ResourceSummary(culture: 0, gold: 3, production: 3)
            );

            var distributionLogic = Container.Resolve<WorkplaceDistributionLogic>();
            var slots = Container.ResolveAll<IWorkerSlot>();

            distributionLogic.DistributeWorkersIntoSlots(3, slots, preferences);

            Assert.IsTrue (SlotOne.IsOccupied,   "Unexpected SlotOne IsOccupied value");
            Assert.IsTrue (SlotTwo.IsOccupied,   "Unexpected SlotTwo IsOccupied value");
            Assert.IsTrue (SlotThree.IsOccupied, "Unexpected SlotThree IsOccupied value");
            Assert.IsFalse(SlotFour.IsOccupied,  "Unexpected SlotFour IsOccupied value");
            Assert.IsFalse(SlotFive.IsOccupied,  "Unexpected SlotFive IsOccupied value");
        }

        [Test(Description = "When a resource focus is being requested and there are several ways to maximize yield for that resource " +
            "the distribution should pick the maximal configuration with the greatest total yield")]
        public void ResourceFocusRequested_ConsidersTotalYield() {
            var preferences = new DistributionPreferences(true, ResourceType.Culture);

            GenerationLogicMock.Setup(logic => logic.GetYieldOfSlotForCity(SlotOne, It.IsAny<ICity>())).Returns(
                new ResourceSummary(culture: 3)
            );

            GenerationLogicMock.Setup(logic => logic.GetYieldOfSlotForCity(SlotTwo, It.IsAny<ICity>())).Returns(
                new ResourceSummary(culture: 2)
            );

            GenerationLogicMock.Setup(logic => logic.GetYieldOfSlotForCity(SlotThree, It.IsAny<ICity>())).Returns(
                new ResourceSummary(culture: 1, gold: 0)
            );

            GenerationLogicMock.Setup(logic => logic.GetYieldOfSlotForCity(SlotFour, It.IsAny<ICity>())).Returns(
                new ResourceSummary(culture: 1, gold: 1)
            );

            GenerationLogicMock.Setup(logic => logic.GetYieldOfSlotForCity(SlotFive, It.IsAny<ICity>())).Returns(
                new ResourceSummary(culture: 1, gold: 1, production: 1)
            );

            var distributionLogic = Container.Resolve<WorkplaceDistributionLogic>();
            var slots = Container.ResolveAll<IWorkerSlot>();

            distributionLogic.DistributeWorkersIntoSlots(3, slots, preferences);

            Assert.IsTrue (SlotOne.IsOccupied,   "Unexpected SlotOne IsOccupied value");
            Assert.IsTrue (SlotTwo.IsOccupied,   "Unexpected SlotTwo IsOccupied value");
            Assert.IsFalse(SlotThree.IsOccupied, "Unexpected SlotThree IsOccupied value");
            Assert.IsFalse(SlotFour.IsOccupied,  "Unexpected SlotFour IsOccupied value");
            Assert.IsTrue (SlotFive.IsOccupied,  "Unexpected SlotFive IsOccupied value");
        }

        [Test(Description = "When no resource focus is being requested, the distribution should attempt to maximize the total " + 
            "yield of all resources")]
        public void ResourceFocusNotRequested_MaximizesTotalYield() {
            var preferences = new DistributionPreferences(false, ResourceType.Culture);

            GenerationLogicMock.Setup(logic => logic.GetYieldOfSlotForCity(SlotOne, It.IsAny<ICity>())).Returns(
                new ResourceSummary(culture: 3)
            );

            GenerationLogicMock.Setup(logic => logic.GetYieldOfSlotForCity(SlotTwo, It.IsAny<ICity>())).Returns(
                new ResourceSummary(culture: 2)
            );

            GenerationLogicMock.Setup(logic => logic.GetYieldOfSlotForCity(SlotThree, It.IsAny<ICity>())).Returns(
                new ResourceSummary(culture: 1, food: 2, production: 1, gold: 2)
            );

            GenerationLogicMock.Setup(logic => logic.GetYieldOfSlotForCity(SlotFour, It.IsAny<ICity>())).Returns(
                new ResourceSummary(culture: 1, gold: 4)
            );

            GenerationLogicMock.Setup(logic => logic.GetYieldOfSlotForCity(SlotFive, It.IsAny<ICity>())).Returns(
                new ResourceSummary(culture: 1, gold: 3, production: 1)
            );

            var distributionLogic = Container.Resolve<WorkplaceDistributionLogic>();
            var slots = Container.ResolveAll<IWorkerSlot>();

            distributionLogic.DistributeWorkersIntoSlots(3, slots, preferences);

            Assert.IsFalse(SlotOne.IsOccupied,   "Unexpected SlotOne IsOccupied value");
            Assert.IsFalse(SlotTwo.IsOccupied,   "Unexpected SlotTwo IsOccupied value");
            Assert.IsTrue (SlotThree.IsOccupied, "Unexpected SlotThree IsOccupied value");
            Assert.IsTrue (SlotFour.IsOccupied,  "Unexpected SlotFour IsOccupied value");
            Assert.IsTrue (SlotFive.IsOccupied,  "Unexpected SlotFive IsOccupied value");
        }

        [Test(Description = "Focused distribution should attempt to provide enough food for the given population to sustain itself")]
        public void FocusedDistribution_TriesToPreventStarvation() {
            var preferences = new DistributionPreferences(true, ResourceType.Culture);

            GenerationLogicMock.Setup(logic => logic.GetYieldOfSlotForCity(SlotOne, It.IsAny<ICity>())).Returns(
                new ResourceSummary(culture: 3)
            );

            GenerationLogicMock.Setup(logic => logic.GetYieldOfSlotForCity(SlotTwo, It.IsAny<ICity>())).Returns(
                new ResourceSummary(culture: 2)
            );

            GenerationLogicMock.Setup(logic => logic.GetYieldOfSlotForCity(SlotThree, It.IsAny<ICity>())).Returns(
                new ResourceSummary(culture: 1, food: 1)
            );

            GenerationLogicMock.Setup(logic => logic.GetYieldOfSlotForCity(SlotFour, It.IsAny<ICity>())).Returns(
                new ResourceSummary(culture: 0, food: 1)
            );

            GenerationLogicMock.Setup(logic => logic.GetYieldOfSlotForCity(SlotFive, It.IsAny<ICity>())).Returns(
                new ResourceSummary(culture: 0, food: 2)
            );

            GrowthLogicMock.Setup(logic => logic.GetFoodConsumptionPerTurn(It.IsAny<ICity>())).Returns(3);

            var distributionLogic = Container.Resolve<WorkplaceDistributionLogic>();
            var slots = Container.ResolveAll<IWorkerSlot>();

            distributionLogic.DistributeWorkersIntoSlots(3, slots, preferences);

            Assert.IsTrue (SlotOne.IsOccupied,   "Unexpected SlotOne IsOccupied value");
            Assert.IsFalse(SlotTwo.IsOccupied,   "Unexpected SlotTwo IsOccupied value");
            Assert.IsTrue (SlotThree.IsOccupied, "Unexpected SlotThree IsOccupied value");
            Assert.IsFalse(SlotFour.IsOccupied,  "Unexpected SlotFour IsOccupied value");
            Assert.IsTrue (SlotFive.IsOccupied,  "Unexpected SlotFive IsOccupied value");
        }

        [Test(Description = "Unfocused distribution should attempt to provide enough food for the given population to sustain itself")]
        public void UnfocusedDistribution_TriesToPreventStarvation() {
            var preferences = new DistributionPreferences(false, ResourceType.Culture);

            GenerationLogicMock.Setup(logic => logic.GetYieldOfSlotForCity(SlotOne, It.IsAny<ICity>())).Returns(
                new ResourceSummary(culture: 3, food: 1)
            );

            GenerationLogicMock.Setup(logic => logic.GetYieldOfSlotForCity(SlotTwo, It.IsAny<ICity>())).Returns(
                new ResourceSummary(culture: 2, food: 1)
            );

            GenerationLogicMock.Setup(logic => logic.GetYieldOfSlotForCity(SlotThree, It.IsAny<ICity>())).Returns(
                new ResourceSummary(culture: 1, food: 2, production: 1)
            );

            GenerationLogicMock.Setup(logic => logic.GetYieldOfSlotForCity(SlotFour, It.IsAny<ICity>())).Returns(
                new ResourceSummary(culture: 1, gold: 5)
            );

            GenerationLogicMock.Setup(logic => logic.GetYieldOfSlotForCity(SlotFive, It.IsAny<ICity>())).Returns(
                new ResourceSummary(culture: 1, gold: 3, production: 1)
            );

            GrowthLogicMock.Setup(logic => logic.GetFoodConsumptionPerTurn(It.IsAny<ICity>())).Returns(3);

            var distributionLogic = Container.Resolve<WorkplaceDistributionLogic>();
            var slots = Container.ResolveAll<IWorkerSlot>();

            distributionLogic.DistributeWorkersIntoSlots(3, slots, preferences);

            Assert.IsTrue (SlotOne.IsOccupied,   "Unexpected SlotOne IsOccupied value");
            Assert.IsFalse(SlotTwo.IsOccupied,   "Unexpected SlotTwo IsOccupied value");
            Assert.IsTrue (SlotThree.IsOccupied, "Unexpected SlotThree IsOccupied value");
            Assert.IsTrue (SlotFour.IsOccupied,  "Unexpected SlotFour IsOccupied value");
            Assert.IsFalse(SlotFive.IsOccupied,  "Unexpected SlotFive IsOccupied value");
        }

        [Test(Description = "The previous distribution of workers should have no effect on the calculated distribution")]
        public void AllDistributions_IgnorePreviousDistribution() {
            var preferences = new DistributionPreferences(true, ResourceType.Culture);

            GenerationLogicMock.Setup(logic => logic.GetYieldOfSlotForCity(SlotOne, It.IsAny<ICity>())).Returns(
                new ResourceSummary(culture: 3)
            );

            GenerationLogicMock.Setup(logic => logic.GetYieldOfSlotForCity(SlotTwo, It.IsAny<ICity>())).Returns(
                new ResourceSummary(culture: 2)
            );

            GenerationLogicMock.Setup(logic => logic.GetYieldOfSlotForCity(SlotThree, It.IsAny<ICity>())).Returns(
                new ResourceSummary(culture: 1)
            );

            GenerationLogicMock.Setup(logic => logic.GetYieldOfSlotForCity(SlotFour, It.IsAny<ICity>())).Returns(
                new ResourceSummary(culture: 0)
            );

            GenerationLogicMock.Setup(logic => logic.GetYieldOfSlotForCity(SlotFive, It.IsAny<ICity>())).Returns(
                new ResourceSummary(culture: 0)
            );

            SlotOne  .IsOccupied = false;
            SlotTwo  .IsOccupied = true;
            SlotThree.IsOccupied = true;
            SlotFour .IsOccupied = true;
            SlotFive .IsOccupied = true;

            var distributionLogic = Container.Resolve<WorkplaceDistributionLogic>();
            var slots = Container.ResolveAll<IWorkerSlot>();

            distributionLogic.DistributeWorkersIntoSlots(3, slots, preferences);

            Assert.IsTrue (SlotOne.IsOccupied,   "Unexpected SlotOne IsOccupied value");
            Assert.IsTrue (SlotTwo.IsOccupied,   "Unexpected SlotTwo IsOccupied value");
            Assert.IsTrue (SlotThree.IsOccupied, "Unexpected SlotThree IsOccupied value");
            Assert.IsFalse(SlotFour.IsOccupied,  "Unexpected SlotFour IsOccupied value");
            Assert.IsFalse(SlotFive.IsOccupied,  "Unexpected SlotFive IsOccupied value");
        }

        [Test(Description = "No distribution should result in more occupied slots than there are people to fill them")]
        public void AllDistributions_FillCorrectNumberOfSlots() {
            Container.Unbind<IWorkerSlot>();

            for(int i = 0; i < 15; ++i) {
                var mockSlot = new Mock<IWorkerSlot>();
                mockSlot.SetupAllProperties();

                Container.Bind<IWorkerSlot>().FromInstance(mockSlot.Object);
            }

            var distributionLogic = Container.Resolve<WorkplaceDistributionLogic>();
            var slots = Container.ResolveAll<IWorkerSlot>();

            distributionLogic.DistributeWorkersIntoSlots(8, slots, new DistributionPreferences(true, ResourceType.Culture));

            Assert.AreEqual(8, slots.Where(slot => slot.IsOccupied).Count(), "Unexpected number of occupied slots");

            distributionLogic.DistributeWorkersIntoSlots(7, slots, new DistributionPreferences(false, ResourceType.Culture));

            Assert.AreEqual(7, slots.Where(slot => slot.IsOccupied).Count(), "Unexpected number of occupied slots");
        }

        [Test(Description = "When there are fewer slots than people to fill them, the distribution should consider this " +
            "a valid state and throw no errors")]
        public void AllDistributions_HandleInsufficientSlotsGracefully() {
            Container.Unbind<IWorkerSlot>();

            for(int i = 0; i < 10; ++i) {
                var mockSlot = new Mock<IWorkerSlot>();
                mockSlot.SetupAllProperties();

                Container.Bind<IWorkerSlot>().FromInstance(mockSlot.Object);
            }

            var distributionLogic = Container.Resolve<WorkplaceDistributionLogic>();
            var slots = Container.ResolveAll<IWorkerSlot>();

            Assert.DoesNotThrow(
                () => distributionLogic.DistributeWorkersIntoSlots(20, slots, new DistributionPreferences(false, ResourceType.Culture)),
                "DistributeWorkersIntoSlots falsely threw an error when given more people than slots to fill"
            );

            Assert.AreEqual(10, slots.Where(slot => slot.IsOccupied).Count(), "Unexpected number of occupied slots");
        }

        [Test(Description = "Distributions should take into account the yield of an unemployed person when maximizing yield")]
        public void AllDistributions_ConsiderUnemploymentYield() {
            throw new NotImplementedException();
        }

    }

}

