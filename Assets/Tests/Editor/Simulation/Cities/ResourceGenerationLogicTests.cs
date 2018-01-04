using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation;

using Assets.Simulation.HexMap;

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

            MockIncomeLogic
                .Setup(logic => logic.GetYieldMultipliersForCity(It.IsAny<ICity>()))
                .Returns(ResourceSummary.Empty);

            MockIncomeLogic
                .Setup(logic => logic.GetYieldMultipliersForCivilization(It.IsAny<ICivilization>()))
                .Returns(ResourceSummary.Empty);

            MockIncomeLogic
                .Setup(logic => logic.GetYieldMultipliersForSlot(It.IsAny<IWorkerSlot>()))
                .Returns(ResourceSummary.Empty);

            Container.Bind<ICityConfig>()                                  .FromInstance(MockConfig             .Object);
            Container.Bind<ITilePossessionCanon>()                         .FromInstance(MockTileCanon          .Object);
            Container.Bind<IBuildingPossessionCanon>()                     .FromInstance(MockBuildingCanon      .Object);
            Container.Bind<IIncomeModifierLogic>()                         .FromInstance(MockIncomeLogic        .Object);
            Container.Bind<IPossessionRelationship<ICivilization, ICity>>().FromInstance(MockCityPossessionCanon.Object);

            Container.Bind<ResourceGenerationLogic>().AsSingle();
        }

        #endregion

        #region test

        [Test(Description = "GetYieldOfSlotForCity should return IncomeModifierLogic.GetRealBaseYieldForSlot " + 
            "if there are no modifiers applied to the slot")]
        public void GetYieldOfSlotForCity_ReturnsBaseYieldIfNoModifiers() {
            var city = BuildCity(null, new List<IHexCell>(), new List<IBuilding>());

            var slotYield = new ResourceSummary(food: 1, gold: 3, production: 2);
            var slot = BuildSlot(slotYield, true);

            var logic = Container.Resolve<ResourceGenerationLogic>();

            Assert.AreEqual(slotYield, logic.GetYieldOfSlotForCity(slot, city),
                "GetYieldOfSlotForCity did not return the expected value");
        }

        [Test(Description = "When GetYieldOfSlotForCity is called, it multiplies its base calculation " +
            "by income modifiers on the slot itself, the city that controls it, and the civilization " +
            "the city belongs to. All of the multipliers should be added together, added to ResourceSummary.Ones, " +
            "and then multiplied against the real base yield as determined by IncomeModifierLogic.GetRealBaseYieldForSlot")]
        public void GetYieldOfSlotForCity_ConsidersIncomeModifiers() {            
            var slotYield = new ResourceSummary(food: 1, gold: 1, production: 1, culture: 0);
            var slot = BuildSlot(slotYield, true);

            var tiles = new List<IHexCell>() { BuildTile(slot, false) };
            var location = BuildTile(BuildSlot(ResourceSummary.Empty, false), true);

            var city = BuildCity(location, tiles, new List<IBuilding>());
            var civilization = BuildCivilization(city);

            MockIncomeLogic
                .Setup(logic => logic.GetYieldMultipliersForSlot(slot))
                .Returns(new ResourceSummary(food: 0, gold: 1, production: -0.5f, culture: 0));

            MockIncomeLogic
                .Setup(logic => logic.GetYieldMultipliersForCity(city))
                .Returns(new ResourceSummary(food: 1, gold: 2, production: 0.5f, culture: 1));

            MockIncomeLogic
                .Setup(logic => logic.GetYieldMultipliersForCivilization(civilization))
                .Returns(new ResourceSummary(food: 0, gold: 0, production: 1, culture: 0));                

            var generationLogic = Container.Resolve<ResourceGenerationLogic>();

            Assert.AreEqual(
                new ResourceSummary(food: 2, gold: 4, production: 2, culture: 0),
                generationLogic.GetYieldOfSlotForCity(slot, city),
                "GetYieldOfSlotForCity returned an unexpected value"
            );
        }

        [Test(Description = "GetYieldOfSlotForCity should not throw an exception when passed " +
            "a city belonging to no civilization")]
        public void GetYieldOfSlotForCity_HandlesNullCivGracefully() {
            var slotYield = new ResourceSummary(food: 1, gold: 1, production: 1, culture: 0);
            var slot = BuildSlot(slotYield, true);

            var tiles = new List<IHexCell>() { BuildTile(slot, false) };
            var location = BuildTile(BuildSlot(ResourceSummary.Empty, false), true);

            var city = BuildCity(location, tiles, new List<IBuilding>());

            MockIncomeLogic.Setup(logic => logic.GetYieldMultipliersForCivilization(null))
                .Throws(new ArgumentNullException("civilization"));

            var generationLogic = Container.Resolve<ResourceGenerationLogic>();

            Assert.DoesNotThrow(() => generationLogic.GetYieldOfSlotForCity(slot, city),
                "GetYieldOfSlotForCity threw an unexpected exception on a city belonging to no civilization");
        }

        [Test(Description = "GetYieldOfUnemployedForCity should return the value stored in " +
            "ResourceGenerationConfig if there are no city or civilizational modifiers applying " +
            "to resource yield")]
        public void GetYieldOfUnemployedForCity_ReturnsConfiguredValue() {
            var city = BuildCity(null, new List<IHexCell>(), new List<IBuilding>());

            var unemployedYield = new ResourceSummary(gold: 2, production: 2);
            MockConfig.Setup(config => config.UnemployedYield).Returns(unemployedYield);

            var logic = Container.Resolve<ResourceGenerationLogic>();

            Assert.AreEqual(unemployedYield, logic.GetYieldOfUnemployedForCity(city),
                "GetYieldOfUnemployedForCity did not return the expected value");
        }

        [Test(Description = "GetYieldOfUnemployedForCity should have its value modified " +
            "by city and civilizational income modifiers from IncomeModifierLogic")]
        public void GetYieldOfUnemployedForCity_ModifiedByIncomeModifiers() {
            var city = BuildCity(null, new List<IHexCell>(), new List<IBuilding>());

            var civilization = BuildCivilization(city);

            MockIncomeLogic.Setup(logic => logic.GetYieldMultipliersForCity(city))
                .Returns(new ResourceSummary(food: 1, production: 0, gold: -1, culture: 2));

            MockIncomeLogic.Setup(logic => logic.GetYieldMultipliersForCivilization(civilization))
                .Returns(new ResourceSummary(food: 2, production: 1, gold: 0, culture: 3));

            var unemployedYield = new ResourceSummary(food: 1, production: 1, gold: 1, culture: 0);
            MockConfig.Setup(config => config.UnemployedYield).Returns(unemployedYield);

            var generationLogic = Container.Resolve<ResourceGenerationLogic>();

            Assert.AreEqual(
                new ResourceSummary(food: 4, production: 2, gold: 0, culture: 0),
                generationLogic.GetYieldOfUnemployedForCity(city),
                "GetYieldOfUnemployedForCity returned an unexpected value"
            );
        }

        [Test(Description = "GetYieldOfUnemployedForCity should not throw an exception " +
            "when the passed city is not owned by any civilization")]
        public void GetYieldOfUnemployedForCity_HandlesNullCivGracefully() {
            var city = BuildCity(null, new List<IHexCell>(), new List<IBuilding>());

            MockIncomeLogic.Setup(logic => logic.GetYieldMultipliersForCivilization(null))
                .Throws(new ArgumentNullException("civilization"));

            var generationLogic = Container.Resolve<ResourceGenerationLogic>();

            Assert.DoesNotThrow(() => generationLogic.GetYieldOfUnemployedForCity(city),
                "GetYieldOfUnemployedForCity threw an unexpected exception when passed a city with no civilization");
        }

        [Test(Description = "GetTotalYieldOfCity should consider the yield of only " +
            "occupied slots in tiles possessed by the argued city")]
        public void GetTotalYieldOfCity_ConsidersOnlyOccupiedTileSlots() {
            var tiles = new List<IHexCell>();
            var buildings = new List<IBuilding>();

            for(int i = 0; i < 3; ++i) {
                tiles.Add(BuildTile(BuildSlot(new ResourceSummary(food: i, production : i + 1), i >= 1), false));
            }

            var city = BuildCity(BuildTile(BuildSlot(ResourceSummary.Empty, false), true), tiles, buildings);

            MockTileCanon.Setup(canon => canon.GetTilesOfCity(city)).Returns(tiles);
            MockBuildingCanon.Setup(canon => canon.GetBuildingsInCity(city)).Returns(new List<IBuilding>().AsReadOnly());

            var logic = Container.Resolve<ResourceGenerationLogic>();

            var totalYield = logic.GetTotalYieldForCity(city);

            Assert.AreEqual(new ResourceSummary(food: 3, production: 5), totalYield, "GetTotalYieldForCity returned an unexpected value");
        }

        [Test(Description = "GetTotalYieldOfCity should consider the yield of only " +
            "occupied slots in buildings possessed by the argued city")]
        public void GetTotalYieldOfCity_ConsidersOnlyOccupiedBuildingSlots() {
            var buildings = new List<IBuilding>();
            for(int i = 0; i < 3; ++i) {
                var slot = BuildSlot(new ResourceSummary(food: i, production: i + 1), i >= 1);

                var building = BuildBuilding(ResourceSummary.Empty, slot);

                buildings.Add(building);
            }

            var location = BuildTile(BuildSlot(ResourceSummary.Empty, false), true);

            var city = BuildCity(location, new List<IHexCell>(), buildings);

            var logic = Container.Resolve<ResourceGenerationLogic>();

            var totalYield = logic.GetTotalYieldForCity(city);

            Assert.AreEqual(new ResourceSummary(food: 3, production: 5), totalYield, "GetTotalYieldForCity returned an unexpected value");
        }

        [Test(Description = "GetTotalYieldOfCity should consider the static yield of " +
            "all buildings possessed by the argued city")]
        public void GetTotalYieldOfCity_ConsiderBuildingStaticYield() {
            var buildings = new List<IBuilding>();
            for(int i = 0; i < 3; ++i) {
                var newBuilding = BuildBuilding(new ResourceSummary(food: i, production: i + 1));
                buildings.Add(newBuilding);
            }

            var location = BuildTile(BuildSlot(ResourceSummary.Empty, false), true);
            var tiles = new List<IHexCell>();

            var city = BuildCity(location, tiles, buildings);

            var logic = Container.Resolve<ResourceGenerationLogic>();

            var totalYield = logic.GetTotalYieldForCity(city);

            Assert.AreEqual(new ResourceSummary(food: 3, production: 6), totalYield, "GetTotalYieldForCity returned an unexpected value");
        }

        [Test(Description = "GetTotalYieldOfCity should consider the total yield of " +
            "all unemployed people in the city, as determined by the total number of occupied " +
            "slots the city has access to")]
        public void GetTotalYieldOfCity_ConsidersUnemployedPeople() {
            var buildings = new List<IBuilding>();
            var tiles = new List<IHexCell>() {
                BuildTile(BuildSlot(ResourceSummary.Empty, true), false),
                BuildTile(BuildSlot(ResourceSummary.Empty, true), false),
                BuildTile(BuildSlot(ResourceSummary.Empty, true), false)
            };

            var location = BuildTile(BuildSlot(ResourceSummary.Empty, false), true);

            var city = BuildCity(location, tiles, buildings);
            city.Population = 5;

            MockConfig.SetupGet(config => config.UnemployedYield).Returns(new ResourceSummary(production: 1));

            var logic = Container.Resolve<ResourceGenerationLogic>();

            var totalYield = logic.GetTotalYieldForCity(city);

            Assert.AreEqual(new ResourceSummary(production: 2), totalYield, "GetTotalYieldForCity returned an unexpected value");
        }

        [Test(Description = "GetTotalYieldOfCity should always consider the yield of the " +
            "tile that the city itself lies upon")]
        public void GetTotalYieldOfCity_ConsidersCityCenter() {
            var location = BuildTile(BuildSlot(new ResourceSummary(food: 2, production: 3), false), true);

            var city = BuildCity(location, new List<IHexCell>(), new List<IBuilding>());

            var logic = Container.Resolve<ResourceGenerationLogic>();

            var totalYield = logic.GetTotalYieldForCity(city);

            Assert.AreEqual(new ResourceSummary(food: 2, production: 3), totalYield, "GetTotalYieldForCity returned an unexpected value");
        }

        [Test(Description = "GetTotalYieldOfCity should not take into account tiles that are " +
            "suppressing their slots, even when those slots are flagged as occupied")]
        public void GetTotalYieldOfCity_IgnoresSuppressedSlots() {
            var location = BuildTile(BuildSlot(ResourceSummary.Empty, false), true);

            var tileOne   = BuildTile(BuildSlot(new ResourceSummary(food: 1, production: 1), true), true);
            var tileTwo   = BuildTile(BuildSlot(new ResourceSummary(food: 1, production: 1), false), true);
            var tileThree = BuildTile(BuildSlot(new ResourceSummary(food: 1, production: 1), true), true);

            var tiles = new List<IHexCell>() { tileOne, tileTwo, tileThree };
            var buildings = new List<IBuilding>();

            var city = BuildCity(location, tiles, buildings);

            BuildCivilization(city);

            var logic = Container.Resolve<ResourceGenerationLogic>();

            var totalYield = logic.GetTotalYieldForCity(city);

            Assert.AreEqual(ResourceSummary.Empty, totalYield, "GetTotalYieldForCity returned an unexpected value");
        }

        [Test(Description = "All methods should throw an ArgumentNullException when passed " + 
            "any null argument")]
        public void AllMethods_ThrowExceptionsOnNullArguments() {
            var city = BuildCity(null, new List<IHexCell>(), new List<IBuilding>());
            var slot = BuildSlot(ResourceSummary.Empty, true);

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

        private ICivilization BuildCivilization(params ICity[] cities) {
            var mockCivilization = new Mock<ICivilization>();

            MockCityPossessionCanon.Setup(canon => canon.GetPossessionsOfOwner(mockCivilization.Object)).Returns(cities);

            foreach(var city in cities) {
                MockCityPossessionCanon.Setup(canon => canon.GetOwnerOfPossession(city)).Returns(mockCivilization.Object);
            }

            return mockCivilization.Object;
        }

        private ICity BuildCity(IHexCell location, IEnumerable<IHexCell> tiles, IEnumerable<IBuilding> buildings) {
            var cityMock = new Mock<ICity>();

            cityMock.SetupAllProperties();
            cityMock.Setup(city => city.Location).Returns(location);
            MockTileCanon.Setup(canon => canon.GetTilesOfCity(cityMock.Object)).Returns(tiles);

            MockBuildingCanon.Setup(canon => canon.GetBuildingsInCity(cityMock.Object)).Returns(buildings.ToList().AsReadOnly());

            return cityMock.Object;
        }

        private IHexCell BuildTile(IWorkerSlot slot, bool suppressSlot) {
            var tileMock = new Mock<IHexCell>();
            tileMock.Setup(tile => tile.SuppressSlot).Returns(suppressSlot);
            tileMock.Setup(tile => tile.WorkerSlot).Returns(slot);

            return tileMock.Object;
        }

        private IWorkerSlot BuildSlot(ResourceSummary yield, bool isOccupied) {
            var slotMock = new Mock<IWorkerSlot>();

            slotMock.SetupAllProperties();
            MockIncomeLogic.Setup(logic => logic.GetRealBaseYieldForSlot(slotMock.Object)).Returns(yield);

            slotMock.Object.IsOccupied = isOccupied;

            return slotMock.Object;
        }

        private IBuilding BuildBuilding(ResourceSummary staticYield, params IWorkerSlot[] slots) {
            return BuildBuilding(staticYield, slots.ToList());
        }

        private IBuilding BuildBuilding(ResourceSummary staticYield, IEnumerable<IWorkerSlot> slots) {
            var buildingMock = new Mock<IBuilding>();

            var mockTemplate = new Mock<IBuildingTemplate>();

            mockTemplate.Setup(template => template.StaticYield).Returns(staticYield);

            buildingMock.Setup(building => building.Slots).Returns(slots.ToList().AsReadOnly());
            buildingMock.Setup(building => building.Template).Returns(mockTemplate.Object);

            return buildingMock.Object;
        }

        #endregion

        #endregion

    }

}
