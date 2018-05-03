using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation;
using Assets.Simulation.Cities;
using Assets.Simulation.Cities.Buildings;
using Assets.Simulation.HexMap;
using Assets.Simulation.MapManagement;
using Assets.Simulation.WorkerSlots;

namespace Assets.Tests.Simulation.MapManagement {

    [TestFixture]
    public class BuildingComposerTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IHexGrid>                                  MockGrid;
        private Mock<IBuildingFactory>                          MockBuildingFactory;
        private Mock<IPossessionRelationship<ICity, IBuilding>> MockBuildingPossessionCanon;
        private Mock<IPossessionRelationship<IHexCell, ICity>>  MockCityLocationCanon;

        private List<IBuildingTemplate> AllBuildingTemplates = new List<IBuildingTemplate>();

        private List<IBuilding> AllBuildings = new List<IBuilding>();

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            AllBuildingTemplates.Clear();

            AllBuildings.Clear();

            MockGrid                    = new Mock<IHexGrid>();
            MockBuildingFactory         = new Mock<IBuildingFactory>();
            MockBuildingPossessionCanon = new Mock<IPossessionRelationship<ICity, IBuilding>>();
            MockCityLocationCanon       = new Mock<IPossessionRelationship<IHexCell, ICity>>();

            MockBuildingFactory.Setup(factory => factory.AllBuildings).Returns(AllBuildings);

            MockBuildingFactory.Setup(
                factory => factory.Create(It.IsAny<IBuildingTemplate>(), It.IsAny<ICity>())
            ).Returns<IBuildingTemplate, ICity>(
                (template, city) => BuildBuilding(template, city)
            );

            Container.Bind<IHexGrid>                                 ().FromInstance(MockGrid                   .Object);
            Container.Bind<IBuildingFactory>                         ().FromInstance(MockBuildingFactory        .Object);
            Container.Bind<IPossessionRelationship<ICity, IBuilding>>().FromInstance(MockBuildingPossessionCanon.Object);
            Container.Bind<IPossessionRelationship<IHexCell, ICity>> ().FromInstance(MockCityLocationCanon      .Object);

            Container.Bind<List<IBuildingTemplate>>().FromInstance(AllBuildingTemplates);

            Container.Bind<BuildingComposer>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void ClearRuntime_AllBuildingsRemovedFromLocation() {
            var buildingOne   = BuildBuilding(BuildTemplate("Template One"));
            var buildingTwo   = BuildBuilding(BuildTemplate("Template Two"));
            var buildingThree = BuildBuilding(BuildTemplate("Template Three"));

            var composer = Container.Resolve<BuildingComposer>();

            composer.ClearRuntime();

            MockBuildingPossessionCanon.Verify(
                canon => canon.ChangeOwnerOfPossession(buildingOne, null), Times.Once,
                "BuildingOne was not removed from its location"
            );

            MockBuildingPossessionCanon.Verify(
                canon => canon.ChangeOwnerOfPossession(buildingTwo, null), Times.Once,
                "BuildingTwo was not removed from its location"
            );

            MockBuildingPossessionCanon.Verify(
                canon => canon.ChangeOwnerOfPossession(buildingThree, null), Times.Once,
                "BuildingThree was not removed from its location"
            );
        }

        [Test]
        public void ComposeBuildings_TemplateStoredAsName() {
            BuildBuilding(BuildTemplate("Template One"));
            BuildBuilding(BuildTemplate("Template Two"));
            BuildBuilding(BuildTemplate("Template Three"));

            var mapData = new SerializableMapData();

            var composer = Container.Resolve<BuildingComposer>();

            composer.ComposeBuildings(mapData);

            Assert.AreEqual("Template One",   mapData.Buildings[0].Template, "First building data has an unexpected Template value");
            Assert.AreEqual("Template Two",   mapData.Buildings[1].Template, "Second building data has an unexpected Template value");
            Assert.AreEqual("Template Three", mapData.Buildings[2].Template, "Third building data has an unexpected Template value");
        }

        [Test]
        public void ComposeBuildings_OwningCityStoredAsLocationCoordinates() {
            BuildBuilding(BuildTemplate(""), BuildCity(BuildHexCell(new HexCoordinates(0, 1))));
            BuildBuilding(BuildTemplate(""), BuildCity(BuildHexCell(new HexCoordinates(2, 3))));
            BuildBuilding(BuildTemplate(""), BuildCity(BuildHexCell(new HexCoordinates(4, 5))));

            var mapData = new SerializableMapData();

            var composer = Container.Resolve<BuildingComposer>();

            composer.ComposeBuildings(mapData);

            Assert.AreEqual(new HexCoordinates(0, 1), mapData.Buildings[0].CityLocation, "First building data has an unexpected CityLocation value");
            Assert.AreEqual(new HexCoordinates(2, 3), mapData.Buildings[1].CityLocation, "Second building data has an unexpected CityLocation value");
            Assert.AreEqual(new HexCoordinates(4, 5), mapData.Buildings[2].CityLocation, "Third building data has an unexpected CityLocation value");
        }

        [Test]
        public void ComposeBuildings_LockedStatusOfSlotsStoredAsListOfBools() {
            var buildingOne = BuildBuilding(BuildTemplate("Template One", 1));
            var buildingTwo = BuildBuilding(BuildTemplate("Template Two", 2));
            BuildBuilding(BuildTemplate("Template Three", 0));

            buildingOne.Slots[0].IsLocked = true;

            buildingTwo.Slots[0].IsLocked = false;
            buildingTwo.Slots[1].IsLocked = true;

            var mapData = new SerializableMapData();

            var composer = Container.Resolve<BuildingComposer>();

            composer.ComposeBuildings(mapData);

            CollectionAssert.AreEqual(
                new List<bool>() { true }, mapData.Buildings[0].IsSlotLocked,
                "First building data has an unexpected IsSlotLocked collection"
            );            

            CollectionAssert.AreEqual(
                new List<bool>() { false, true }, mapData.Buildings[1].IsSlotLocked,
                "Second building data has an unexpected IsSlotLocked collection"
            );            

            CollectionAssert.AreEqual(
                new List<bool>() {  }, mapData.Buildings[2].IsSlotLocked,
                "Third building data has an unexpected IsSlotLocked collection"
            );
        }

        [Test]
        public void ComposeBuildings_OccupiedStatusOfSlotsStoredAsListOfBools() {
            var buildingOne = BuildBuilding(BuildTemplate("Template One", 1));
            var buildingTwo = BuildBuilding(BuildTemplate("Template Two", 2));
            BuildBuilding(BuildTemplate("Template Three", 0));

            buildingOne.Slots[0].IsOccupied = true;

            buildingTwo.Slots[0].IsOccupied = false;
            buildingTwo.Slots[1].IsOccupied = true;

            var mapData = new SerializableMapData();

            var composer = Container.Resolve<BuildingComposer>();

            composer.ComposeBuildings(mapData);

            CollectionAssert.AreEqual(
                new List<bool>() { true }, mapData.Buildings[0].IsSlotOccupied,
                "First building data has an unexpected IsSlotOccupied collection"
            );

            CollectionAssert.AreEqual(
                new List<bool>() { false, true }, mapData.Buildings[1].IsSlotOccupied,
                "Second building data has an unexpected IsSlotOccupied collection"
            );

            CollectionAssert.AreEqual(
                new List<bool>() { }, mapData.Buildings[2].IsSlotOccupied,
                "Third building data has an unexpected IsSlotOccupied collection"
            );
        }

        [Test]
        public void DecomposeBuildings_BuildingFactoryCalledOnCorrectTemplateAndCity() {
            var templateOne   = BuildTemplate("Template One");
            var templateTwo   = BuildTemplate("Template Two");
            var templateThree = BuildTemplate("Template Three");

            var cityOne   = BuildCity(BuildHexCell(new HexCoordinates(0, 1)));
            var cityTwo   = BuildCity(BuildHexCell(new HexCoordinates(2, 3)));
            var cityThree = BuildCity(BuildHexCell(new HexCoordinates(4, 5)));

            var mapData = new SerializableMapData() {
                Buildings = new List<SerializableBuildingData>() {
                    new SerializableBuildingData() { Template = "Template One",   CityLocation = new HexCoordinates(0, 1) },
                    new SerializableBuildingData() { Template = "Template Two",   CityLocation = new HexCoordinates(2, 3) },
                    new SerializableBuildingData() { Template = "Template Three", CityLocation = new HexCoordinates(4, 5) },
                }
            };

            var composer = Container.Resolve<BuildingComposer>();

            composer.DecomposeBuildings(mapData);

            MockBuildingFactory.Verify(
                factory => factory.Create(templateOne, cityOne), Times.Once,
                "Factory was not called as expected on TemplateOne and CityOne"
            );

            MockBuildingFactory.Verify(
                factory => factory.Create(templateTwo, cityTwo), Times.Once,
                "Factory was not called as expected on TemplateOne and CityOne"
            );

            MockBuildingFactory.Verify(
                factory => factory.Create(templateThree, cityThree), Times.Once,
                "Factory was not called as expected on TemplateOne and CityOne"
            );
        }

        [Test]
        public void DecomposeBuildings_LockedStatusOfSlotsLoadedCorrectly() {
            BuildTemplate("Template One", 1);
            BuildTemplate("Template Two", 2);
            BuildTemplate("Template Three");

            BuildCity(BuildHexCell(new HexCoordinates(0, 1)));
            BuildCity(BuildHexCell(new HexCoordinates(2, 3)));
            BuildCity(BuildHexCell(new HexCoordinates(4, 5)));

            var mapData = new SerializableMapData() {
                Buildings = new List<SerializableBuildingData>() {
                    new SerializableBuildingData() {
                        Template = "Template One", CityLocation = new HexCoordinates(0, 1),
                        IsSlotLocked   = new List<bool>() { true },
                        IsSlotOccupied = new List<bool>() { false }
                    },
                    new SerializableBuildingData() {
                        Template = "Template Two", CityLocation = new HexCoordinates(2, 3),
                        IsSlotLocked   = new List<bool>() { false, true },
                        IsSlotOccupied = new List<bool>() { true, false },
                    },
                    new SerializableBuildingData() {
                        Template = "Template Three", CityLocation = new HexCoordinates(4, 5),
                        IsSlotLocked   = new List<bool>() {  },
                        IsSlotOccupied = new List<bool>() {  },
                    },
                }
            };

            var composer = Container.Resolve<BuildingComposer>();

            composer.DecomposeBuildings(mapData);

            CollectionAssert.AreEqual(
                mapData.Buildings[0].IsSlotLocked, AllBuildings[0].Slots.Select(slot => slot.IsLocked),
                "BuildingOne's slots have unexpected IsLocked values"
            );

            CollectionAssert.AreEqual(
                mapData.Buildings[1].IsSlotLocked, AllBuildings[1].Slots.Select(slot => slot.IsLocked),
                "BuildingTwo's slots have unexpected IsLocked values"
            );

            CollectionAssert.AreEqual(
                mapData.Buildings[2].IsSlotLocked, AllBuildings[2].Slots.Select(slot => slot.IsLocked),
                "BuildingThree's slots have unexpected IsLocked values"
            );
        }

        [Test]
        public void DecomposeBuildings_OccupiedStatusOfSlotsLoadedCorrectly() {
            BuildTemplate("Template One", 1);
            BuildTemplate("Template Two", 2);
            BuildTemplate("Template Three");

            BuildCity(BuildHexCell(new HexCoordinates(0, 1)));
            BuildCity(BuildHexCell(new HexCoordinates(2, 3)));
            BuildCity(BuildHexCell(new HexCoordinates(4, 5)));

            var mapData = new SerializableMapData() {
                Buildings = new List<SerializableBuildingData>() {
                    new SerializableBuildingData() {
                        Template = "Template One", CityLocation = new HexCoordinates(0, 1),
                        IsSlotLocked   = new List<bool>() { true },
                        IsSlotOccupied = new List<bool>() { false }
                    },
                    new SerializableBuildingData() {
                        Template = "Template Two", CityLocation = new HexCoordinates(2, 3),
                        IsSlotLocked   = new List<bool>() { false, true },
                        IsSlotOccupied = new List<bool>() { true, false },
                    },
                    new SerializableBuildingData() {
                        Template = "Template Three", CityLocation = new HexCoordinates(4, 5),
                        IsSlotLocked   = new List<bool>() {  },
                        IsSlotOccupied = new List<bool>() {  },
                    },
                }
            };

            var composer = Container.Resolve<BuildingComposer>();

            composer.DecomposeBuildings(mapData);

            CollectionAssert.AreEqual(
                mapData.Buildings[0].IsSlotOccupied, AllBuildings[0].Slots.Select(slot => slot.IsOccupied),
                "BuildingOne's slots have unexpected IsOccupied values"
            );

            CollectionAssert.AreEqual(
                mapData.Buildings[1].IsSlotOccupied, AllBuildings[1].Slots.Select(slot => slot.IsOccupied),
                "BuildingTwo's slots have unexpected IsOccupied values"
            );

            CollectionAssert.AreEqual(
                mapData.Buildings[2].IsSlotOccupied, AllBuildings[2].Slots.Select(slot => slot.IsOccupied),
                "BuildingThree's slots have unexpected IsOccupied values"
            );
        }

        #endregion

        #region utilities

        private IBuildingTemplate BuildTemplate(string name, int slotCount = 0) {
            var mockTemplate = new Mock<IBuildingTemplate>();

            mockTemplate.Name = name;
            mockTemplate.Setup(template => template.name)     .Returns(name);
            mockTemplate.Setup(template => template.SlotCount).Returns(slotCount);

            var newTemplate = mockTemplate.Object;

            AllBuildingTemplates.Add(newTemplate);

            return newTemplate;
        }

        private IBuilding BuildBuilding (IBuildingTemplate template) {
            return BuildBuilding(template, BuildCity(BuildHexCell(new HexCoordinates(0, 0))));
        }

        private IBuilding BuildBuilding(IBuildingTemplate template, ICity owner) {
            var mockBuilding = new Mock<IBuilding>();

            var slots = new List<IWorkerSlot>();
            for(int i = 0; i < template.SlotCount; i++) {
                slots.Add(BuildWorkerSlot());
            }

            mockBuilding.Setup(building => building.Template).Returns(template);
            mockBuilding.Setup(building => building.Slots)   .Returns(slots.AsReadOnly());

            var newBuilding = mockBuilding.Object;

            MockBuildingPossessionCanon.Setup(canon => canon.GetOwnerOfPossession(newBuilding)).Returns(owner);
            AllBuildings.Add(newBuilding);

            return newBuilding;
        }

        private ICity BuildCity(IHexCell location) {
            var newCity = new Mock<ICity>().Object;

            MockCityLocationCanon.Setup(canon => canon.GetOwnerOfPossession(newCity)).Returns(location);

            MockCityLocationCanon.Setup(canon => canon.GetPossessionsOfOwner(location))
                                 .Returns(new List<ICity>() { newCity }.AsReadOnly());

            return newCity;
        }

        private IHexCell BuildHexCell(HexCoordinates coordinates) {
            var mockCell = new Mock<IHexCell>();

            mockCell.Setup(cell => cell.Coordinates).Returns(coordinates);

            var newCell = mockCell.Object;

            MockGrid.Setup(grid => grid.GetCellAtCoordinates(coordinates)).Returns(newCell);

            return newCell;
        }

        private IWorkerSlot BuildWorkerSlot() {
            var mockSlot = new Mock<IWorkerSlot>();

            mockSlot.SetupAllProperties();

            return mockSlot.Object;
        }

        #endregion

        #endregion

    }

}
