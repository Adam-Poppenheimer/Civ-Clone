using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation;

using Assets.Simulation.GameMap;

using Assets.Simulation.Cities;
using Assets.Simulation.Cities.Buildings;
using Assets.Simulation.Cities.ResourceGeneration;
using Assets.Simulation.Cities.Territory;
using Assets.Simulation.Civilizations;

namespace Assets.Tests.Simulation.Cities {

    [TestFixture]
    public class ResourceGenerationLogicTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<ICityConfig>                                   MockConfig;
        private Mock<ITilePossessionCanon>                          MockTileCanon;
        private Mock<IBuildingPossessionCanon>                      MockBuildingCanon;
        private Mock<IIncomeModifierLogic>                          MockIncomeLogic;
        private Mock<IPossessionRelationship<ICivilization, ICity>> MockCityPossessionCanon;

        private Mock<ICity>       CityMock;
        private Mock<IMapTile>    CityLocationMock;
        private Mock<IWorkerSlot> LocationSlotMock;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockConfig              = new Mock<ICityConfig>();
            MockTileCanon           = new Mock<ITilePossessionCanon>();
            MockBuildingCanon       = new Mock<IBuildingPossessionCanon>();     
            MockIncomeLogic         = new Mock<IIncomeModifierLogic>();   
            MockCityPossessionCanon = new Mock<IPossessionRelationship<ICivilization, ICity>>();    

            CityLocationMock = new Mock<IMapTile>();
            LocationSlotMock = new Mock<IWorkerSlot>();
            LocationSlotMock.Setup(slot => slot.BaseYield).Returns(ResourceSummary.Empty);

            CityLocationMock.Setup(tile => tile.WorkerSlot).Returns(LocationSlotMock.Object);

            CityMock = new Mock<ICity>();

            CityMock.Setup(city => city.Location).Returns(CityLocationMock.Object);

            Container.Bind<ICityConfig>()             .FromInstance(MockConfig       .Object);
            Container.Bind<ITilePossessionCanon>()    .FromInstance(MockTileCanon    .Object);
            Container.Bind<IBuildingPossessionCanon>().FromInstance(MockBuildingCanon.Object);
            Container.Bind<IIncomeModifierLogic>()    .FromInstance(MockIncomeLogic  .Object);

            Container.Bind<ResourceGenerationLogic>().AsSingle();
        }

        #endregion

        #region test

        [Test(Description = "GetYieldOfSlotForCity returns the base yield of any slot " +
            "that is occupied")]
        public void GetYieldOfSlotForCity_ReturnsBaseYieldIfOccupied() {
            var city = CityMock.Object;

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
            var city = CityMock.Object;

            var mockSlot = new Mock<IWorkerSlot>();
            var slotYield = new ResourceSummary(food: 1, gold: 3, production: 2);
            mockSlot.SetupGet(slot => slot.BaseYield).Returns(slotYield);
            mockSlot.SetupGet(slot => slot.IsOccupied).Returns(false);

            var logic = Container.Resolve<ResourceGenerationLogic>();

            Assert.AreEqual(ResourceSummary.Empty, logic.GetYieldOfSlotForCity(mockSlot.Object, city),
                "GetYieldOfSlotForCity did not return the expected value");
        }

        [Test(Description = "When GetYieldOfSlotForCity is called, it multiplies its base calculation " +
            "by income modifiers on the slot itself, the city that controls it, and the civilization " +
            "the city belongs to")]
        public void GetYieldOfSlotForCity_ConsidersIncomeModifiers() {
            var city = CityMock.Object;
            var civilization = BuildCivilization();

            var mockSlot = new Mock<IWorkerSlot>();
            var slotYield = new ResourceSummary(food: 1, gold: 1, production: 1, culture: 0);
            mockSlot.SetupGet(slot => slot.BaseYield).Returns(slotYield);
            mockSlot.SetupGet(slot => slot.IsOccupied).Returns(false);

            MockIncomeLogic
                .Setup(logic => logic.GetYieldMultipliersForSlot(mockSlot.Object))
                .Returns(new ResourceSummary(food: 1, gold: 2, production: 0.5f, culture: 1));

            MockIncomeLogic
                .Setup(logic => logic.GetYieldMultipliersForCity(city))
                .Returns(new ResourceSummary(food: 2, gold: 3, production: 1f, culture: 2));

            MockIncomeLogic
                .Setup(logic => logic.GetYieldMultipliersForCivilization(civilization))
                .Returns(new ResourceSummary(food: 1, gold: 1, production: 2, culture: 1));                

            var generationLogic = Container.Resolve<ResourceGenerationLogic>();

            throw new NotImplementedException();
        }


        [Test(Description = "GetYieldOfUnemployedForCity should return the value stored in " +
            "ResourceGenerationConfig")]
        public void GetYieldOfUnemployedForCity_ReturnsConfiguredValue() {
            var city = CityMock.Object;

            var unemployedYield = new ResourceSummary(gold: 2, production: 2);
            MockConfig.Setup(config => config.UnemployedYield).Returns(unemployedYield);

            var logic = Container.Resolve<ResourceGenerationLogic>();

            Assert.AreEqual(unemployedYield, logic.GetYieldOfUnemployedForCity(city),
                "GetYieldOfUnemployedForCity did not return the expected value");
        }

        [Test(Description = "GetTotalYieldOfCity should consider the yield of all " +
            "occupied slots in all tiles possessed by the argued city")]
        public void GetTotalYieldOfCity_ConsidersOccupiedTileSlots() {
            var city = CityMock.Object;

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

        [Test(Description = "GetTotalYieldOfCity should only consider the yield of all " +
            "occupied slots in all buildings possessed by the argued city")]
        public void GetTotalYieldOfCity_ConsidersOccupiedBuildingSlots() {
            var city = CityMock.Object;

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
            var city = CityMock.Object;

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
            CityMock.SetupAllProperties();
            
            var city = CityMock.Object;
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
            "tile that the city itself lies upon")]
        public void GetTotalYieldOfCity_ConsidersCityCenter() {
            CityMock.SetupAllProperties();

            var city = CityMock.Object;
            city.Population = 0;

            LocationSlotMock.Setup(slot => slot.BaseYield).Returns(new ResourceSummary(food: 2, production: 3));

            MockTileCanon.Setup(canon => canon.GetTilesOfCity(It.IsAny<ICity>())).Returns(new List<IMapTile>().AsReadOnly());
            MockBuildingCanon.Setup(canon => canon.GetBuildingsInCity(It.IsAny<ICity>())).Returns(new List<IBuilding>().AsReadOnly());

            var logic = Container.Resolve<ResourceGenerationLogic>();

            var totalYield = logic.GetTotalYieldForCity(city);

            Assert.AreEqual(new ResourceSummary(food: 2, production: 3), totalYield, "GetTotalYieldForCity returned an unexpected value");
        }

        [Test(Description = "GetTotalYieldOfCity should not take into account tiles that are " +
            "suppressing their slots, even when those slots are flagged as occupied")]
        public void GetTotalYieldOfCity_IgnoresSuppressedSlots() {
            var city = CityMock.Object;

            var tiles = new List<IMapTile>();
            for(int i = 0; i < 3; ++i) {
                var mockSlot = new Mock<IWorkerSlot>();
                mockSlot.SetupGet(slot => slot.BaseYield).Returns(new ResourceSummary(food: i, production: i + 1));
                mockSlot.SetupGet(slot => slot.IsOccupied).Returns(i >= 1);

                var mockTile = new Mock<IMapTile>();
                mockTile.SetupGet(tile => tile.WorkerSlot).Returns(mockSlot.Object);
                mockTile.Setup(tile => tile.SuppressSlot).Returns(true);

                tiles.Add(mockTile.Object);
            }

            MockTileCanon.Setup(canon => canon.GetTilesOfCity(city)).Returns(tiles);
            MockBuildingCanon.Setup(canon => canon.GetBuildingsInCity(city)).Returns(new List<IBuilding>().AsReadOnly());

            var logic = Container.Resolve<ResourceGenerationLogic>();

            var totalYield = logic.GetTotalYieldForCity(city);

            Assert.AreEqual(ResourceSummary.Empty, totalYield, "GetTotalYieldForCity returned an unexpected value");
        }

        [Test(Description = "All methods should throw an ArgumentNullException when passed " + 
            "any null argument")]
        public void AllMethods_ThrowExceptionsOnNullArguments() {
            var city = CityMock.Object;
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

        #endregion

        #region utilities

        private ICivilization BuildCivilization() {
            var mockCivilization = new Mock<ICivilization>();


            return mockCivilization.Object;
        }

        #endregion

        #endregion

    }

}
