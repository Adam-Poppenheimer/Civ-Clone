using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Cities.Buildings;
using Assets.GameMap;

namespace Assets.Cities.Editor {

    [TestFixture]
    public class ResourceGenerationLogicTests : ZenjectUnitTestFixture {

        private Mock<IResourceGenerationConfig> MockConfig;
        private Mock<ITilePossessionCanon> MockTileCanon;
        private Mock<IBuildingPossessionCanon> MockBuildingCanon;

        [SetUp]
        public void CommonInstall() {
            MockConfig        = new Mock<IResourceGenerationConfig>();
            MockTileCanon     = new Mock<ITilePossessionCanon>();
            MockBuildingCanon = new Mock<IBuildingPossessionCanon>();

            Container.Bind<IResourceGenerationConfig>().FromInstance(MockConfig.Object);
            Container.Bind<ITilePossessionCanon>()     .FromInstance(MockTileCanon.Object);
            Container.Bind<IBuildingPossessionCanon>() .FromInstance(MockBuildingCanon.Object);

            Container.Bind<ResourceGenerationLogic>().AsSingle();
        }

        [Test(Description = "GetYieldOfSlotForCity returns the base yield of any slot " +
            "that is occupied")]
        public void GetYieldOfSlotForCity_ReturnsBaseYieldIfOccupied() {
            var city = new Mock<ICity>().Object;

            var mockSlot = new Mock<IWorkerSlot>();
            var slotYield = new ResourceSummary(food: 1, gold: 3, production: 2);
            mockSlot.SetupGet(slot => slot.BaseYield).Returns(slotYield);
            mockSlot.SetupGet(slot => slot.IsOccupied).Returns(true);

            var logic = Container.Resolve<ResourceGenerationLogic>();

            Assert.AreEqual(slotYield, logic.GetYieldOfSlotForCity(mockSlot.Object, city),
                "GetYieldOfSlotForCity did not return the expected value");
        }

        [Test(Description = "GetYieldOfSlotForCity returns ResourceSummary.Empty for any " +
            "slot that is unoccupied")]
        public void GetYieldOfSlotForCity_ReturnsEmptyIfUnoccupied() {
            var city = new Mock<ICity>().Object;

            var mockSlot = new Mock<IWorkerSlot>();
            var slotYield = new ResourceSummary(food: 1, gold: 3, production: 2);
            mockSlot.SetupGet(slot => slot.BaseYield).Returns(slotYield);
            mockSlot.SetupGet(slot => slot.IsOccupied).Returns(false);

            var logic = Container.Resolve<ResourceGenerationLogic>();

            Assert.AreEqual(ResourceSummary.Empty, logic.GetYieldOfSlotForCity(mockSlot.Object, city),
                "GetYieldOfSlotForCity did not return the expected value");
        }

        [Test(Description = "GetYieldOfUnemployedForCity should return the value stored in " +
            "ResourceGenerationConfig")]
        public void GetYieldOfUnemployedForCity_ReturnsConfiguredValue() {
            var city = new Mock<ICity>().Object;

            var unemployedYield = new ResourceSummary(gold: 2, production: 2);
            MockConfig.Setup(config => config.UnemployedYield).Returns(unemployedYield);

            var logic = Container.Resolve<ResourceGenerationLogic>();

            Assert.AreEqual(unemployedYield, logic.GetYieldOfUnemployedForCity(city),
                "GetYieldOfUnemployedForCity did not return the expected value");
        }

        [Test(Description = "GetTotalYieldOfCity should consider the yield of all " +
            "occupied slots in all tiles possessed by the argued city")]
        public void GetTotalYieldOfCity_ConsidersOccupiedTileSlots() {
            var city = new Mock<ICity>().Object;

            var tiles = new List<IMapTile>();
            for(int i = 0; i < 3; ++i) {
                var mockSlot = new Mock<IWorkerSlot>();
                mockSlot.SetupGet(slot => slot.BaseYield).Returns(new ResourceSummary(food: i, production: i + 1));
                mockSlot.SetupGet(slot => slot.IsOccupied).Returns(i >= 1);

                var mockTile = new Mock<IMapTile>();
                mockTile.SetupGet(tile => tile.WorkerSlot).Returns(mockSlot.Object);

                tiles.Add(mockTile.Object);
            }

            MockTileCanon.Setup(canon => canon.GetTilesOfCity(city)).Returns(tiles);
            MockBuildingCanon.Setup(canon => canon.GetBuildingsInCity(city)).Returns(new List<IBuilding>().AsReadOnly());

            var logic = Container.Resolve<ResourceGenerationLogic>();

            var totalYield = logic.GetTotalYieldForCity(city);

            Assert.AreEqual(new ResourceSummary(food: 3, production: 5), totalYield, "GetTotalYieldForCity returned an unexpected value");
        }

        [Test(Description = "GetTotalYieldOfCity should consider the yield of all " +
            "occupied slots in all buildings possessed by the argued city")]
        public void GetTotalYieldOfCity_ConsidersOccupiedBuildingSlots() {
            var city = new Mock<ICity>().Object;

            var buildings = new List<IBuilding>();
            for(int i = 0; i < 3; ++i) {
                var mockSlot = new Mock<IWorkerSlot>();
                mockSlot.SetupGet(slot => slot.BaseYield).Returns(new ResourceSummary(food: i, production: i + 1));
                mockSlot.SetupGet(slot => slot.IsOccupied).Returns(i >= 1);

                var mockBuilding = new Mock<IBuilding>();
                mockBuilding.SetupGet(building => building.Slots).Returns(new List<IWorkerSlot>() { mockSlot.Object }.AsReadOnly());

                mockBuilding.SetupGet(building => building.Template.StaticYield).Returns(ResourceSummary.Empty);

                buildings.Add(mockBuilding.Object);
            }

            MockBuildingCanon.Setup(canon => canon.GetBuildingsInCity(city)).Returns(buildings.AsReadOnly());

            var logic = Container.Resolve<ResourceGenerationLogic>();

            var totalYield = logic.GetTotalYieldForCity(city);

            Assert.AreEqual(new ResourceSummary(food: 3, production: 5), totalYield, "GetTotalYieldForCity returned an unexpected value");
        }

        [Test(Description = "GetTotalYieldOfCity should consider the static yield of " +
            "all buildings possessed by the argued city")]
        public void GetTotalYieldOfCity_ConsiderBuildingStaticYield() {
            var city = new Mock<ICity>().Object;

            var buildings = new List<IBuilding>();
            for(int i = 0; i < 3; ++i) {
                var mockBuilding = new Mock<IBuilding>();
                mockBuilding.SetupGet(building => building.Slots).Returns(new List<IWorkerSlot>().AsReadOnly());
                mockBuilding.SetupGet(building => building.Template.StaticYield)
                    .Returns(new ResourceSummary(food: i, production: i + 1));

                buildings.Add(mockBuilding.Object);
            }

            MockConfig.SetupGet(config => config.UnemployedYield).Returns(ResourceSummary.Empty);

            MockBuildingCanon.Setup(canon => canon.GetBuildingsInCity(city)).Returns(buildings.AsReadOnly());

            var logic = Container.Resolve<ResourceGenerationLogic>();

            var totalYield = logic.GetTotalYieldForCity(city);

            Assert.AreEqual(new ResourceSummary(food: 3, production: 6), totalYield, "GetTotalYieldForCity returned an unexpected value");
        }

        [Test(Description = "GetTotalYieldOfCity should consider the total yield of " +
            "all unemployed people in the city, as determined by the total number of occupied " +
            "slots the city has access to")]
        public void GetTotalYieldOfCity_ConsidersUnemployedPeople() {
            var mockCity = new Mock<ICity>();
            mockCity.SetupAllProperties();
            
            var city = mockCity.Object;
            city.Population = 5;

            var buildings = new List<IBuilding>();
            for(int i = 0; i < 3; ++i) {
                var mockSlot = new Mock<IWorkerSlot>();
                mockSlot.SetupGet(slot => slot.BaseYield).Returns(ResourceSummary.Empty);
                mockSlot.SetupGet(slot => slot.IsOccupied).Returns(true);

                var mockBuilding = new Mock<IBuilding>();
                mockBuilding.SetupGet(building => building.Slots).Returns(new List<IWorkerSlot>() { mockSlot.Object }.AsReadOnly());

                mockBuilding.SetupGet(building => building.Template.StaticYield).Returns(ResourceSummary.Empty);

                buildings.Add(mockBuilding.Object);
            }

            MockConfig.SetupGet(config => config.UnemployedYield).Returns(ResourceSummary.Empty);

            MockBuildingCanon.Setup(canon => canon.GetBuildingsInCity(city)).Returns(buildings.AsReadOnly());
            MockConfig.SetupGet(config => config.UnemployedYield).Returns(new ResourceSummary(production: 1));

            var logic = Container.Resolve<ResourceGenerationLogic>();

            var totalYield = logic.GetTotalYieldForCity(city);

            Assert.AreEqual(new ResourceSummary(production: 2), totalYield, "GetTotalYieldForCity returned an unexpected value");
        }

        [Test(Description = "GetTotalYieldOfCity should always consider the yield of the " +
            "unoccupiable tile that the city itself lies upon")]
        public void GetTotalYieldForCity_ConsidersCityCenter() {
            throw new NotImplementedException();
        }

        [Test(Description = "All methods should throw an ArgumentNullException when passed " + 
            "any null argument")]
        public void AllMethods_ThrowExceptionsOnNullArguments() {
            var city = new Mock<ICity>().Object;
            var slot = new Mock<IWorkerSlot>().Object;

            var logic = Container.Resolve<ResourceGenerationLogic>();

            Assert.Throws<ArgumentNullException>(() => logic.GetTotalYieldForCity(null),
                "GetTotalYieldForCity failed to throw on a null city argument");

            Assert.Throws<ArgumentNullException>(() => logic.GetYieldOfSlotForCity(null, city),
                "GetYieldOfSlotForCity failed to throw on a null slot argument");

            Assert.Throws<ArgumentNullException>(() => logic.GetYieldOfSlotForCity(slot, null),
                "GetYieldOfSlotForCity failed to throw on a null city argument");

            Assert.Throws<ArgumentNullException>(() => logic.GetYieldOfUnemployedForCity(null),
                "GetYieldOfUnemployedForCity failed to throw on a null city argument");
        }

    }

}
