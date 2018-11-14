using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation;
using Assets.Simulation.WorkerSlots;
using Assets.Simulation.MapManagement;
using Assets.Simulation.Cities;
using Assets.Simulation.Cities.Buildings;
using Assets.Simulation.Cities.Distribution;
using Assets.Simulation.Cities.Production;
using Assets.Simulation.Civilizations;
using Assets.Simulation.HexMap;
using Assets.Simulation.Units;

namespace Assets.Tests.Simulation.MapManagement {

    [TestFixture]
    public class CityComposerTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IHexGrid>                                      MockGrid;
        private Mock<ICityFactory>                                  MockCityFactory;
        private Mock<IPossessionRelationship<ICivilization, ICity>> MockCityPossessionCanon;
        private Mock<IPossessionRelationship<IHexCell, ICity>>      MockCityLocationCanon;
        private Mock<ICivilizationFactory>                          MockCivilizationFactory;
        private Mock<IProductionProjectFactory>                     MockProjectFactory;

        private List<IBuildingTemplate> AvailableBuildingTemplates = new List<IBuildingTemplate>();
        private List<IUnitTemplate>     AvailableUnitTemplates     = new List<IUnitTemplate>();

        private List<ICity>         AllCities        = new List<ICity>();
        private List<ICivilization> AllCivilizations = new List<ICivilization>();

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            AvailableBuildingTemplates.Clear();
            AvailableUnitTemplates    .Clear();

            AllCities       .Clear();
            AllCivilizations.Clear();

            MockGrid                    = new Mock<IHexGrid>();
            MockCityFactory             = new Mock<ICityFactory>();
            MockCityPossessionCanon     = new Mock<IPossessionRelationship<ICivilization, ICity>>();
            MockCityLocationCanon       = new Mock<IPossessionRelationship<IHexCell, ICity>>();
            MockCivilizationFactory     = new Mock<ICivilizationFactory>();
            MockProjectFactory          = new Mock<IProductionProjectFactory>();

            MockCityFactory        .Setup(factory => factory.AllCities)       .Returns(AllCities       .AsReadOnly());
            MockCivilizationFactory.Setup(factory => factory.AllCivilizations).Returns(AllCivilizations.AsReadOnly());

            MockCityFactory.Setup(
                factory => factory.Create(It.IsAny<IHexCell>(), It.IsAny<ICivilization>(), It.IsAny<string>())
            ).Returns<IHexCell, ICivilization, string>(
                (location, owner, name) => BuildCity(location, owner, name, BuildUnit())
            );

            MockProjectFactory.Setup(factory => factory.ConstructProject(It.IsAny<IBuildingTemplate>()))
                              .Returns<IBuildingTemplate>(template => BuildProject(template, null, 0));

            MockProjectFactory.Setup(factory => factory.ConstructProject(It.IsAny<IUnitTemplate>()))
                              .Returns<IUnitTemplate>(template => BuildProject(null, template, 0));

            Container.Bind<IHexGrid>                                     ().FromInstance(MockGrid                   .Object);
            Container.Bind<ICityFactory>                                 ().FromInstance(MockCityFactory            .Object);
            Container.Bind<IPossessionRelationship<ICivilization, ICity>>().FromInstance(MockCityPossessionCanon    .Object);
            Container.Bind<IPossessionRelationship<IHexCell, ICity>>     ().FromInstance(MockCityLocationCanon      .Object);
            Container.Bind<ICivilizationFactory>                         ().FromInstance(MockCivilizationFactory    .Object);
            Container.Bind<IProductionProjectFactory>                    ().FromInstance(MockProjectFactory         .Object);

            Container.Bind<List<IBuildingTemplate>>().FromInstance(AvailableBuildingTemplates);

            Container.Bind<IEnumerable<IUnitTemplate>>()
                     .WithId("Available Unit Templates")
                     .FromInstance(AvailableUnitTemplates);

            Container.Bind<CityComposer>().AsSingle();
        }

        #endregion

        #region test

        [Test]
        public void ClearRuntime_AllCitesRemovedFromLocation() {
            var cityOne   = BuildCity(BuildHexCell(new HexCoordinates(0, 0)), BuildCivilization(), "City One",   BuildUnit());
            var cityTwo   = BuildCity(BuildHexCell(new HexCoordinates(0, 0)), BuildCivilization(), "City Two",   BuildUnit());
            var cityThree = BuildCity(BuildHexCell(new HexCoordinates(0, 0)), BuildCivilization(), "City Three", BuildUnit());

            var composer = Container.Resolve<CityComposer>();

            composer.ClearRuntime();

            MockCityLocationCanon.Verify(
                canon => canon.ChangeOwnerOfPossession(cityOne, null), Times.Once,
                "CityOne was not removed from its location"
            );

            MockCityLocationCanon.Verify(
                canon => canon.ChangeOwnerOfPossession(cityTwo, null), Times.Once,
                "CityTwo was not removed from its location"
            );

            MockCityLocationCanon.Verify(
                canon => canon.ChangeOwnerOfPossession(cityThree, null), Times.Once,
                "CityThree was not removed from its location"
            );
        }

        [Test]
        public void ClearRuntime_AllCitiesDestroyed() {
            Mock<ICity> mockCityOne, mockCityTwo, mockCityThree;

            BuildCity(BuildHexCell(new HexCoordinates(0, 0)), BuildCivilization(), "City One",   BuildUnit(), out mockCityOne);
            BuildCity(BuildHexCell(new HexCoordinates(0, 0)), BuildCivilization(), "City Two",   BuildUnit(), out mockCityTwo);
            BuildCity(BuildHexCell(new HexCoordinates(0, 0)), BuildCivilization(), "City Three", BuildUnit(), out mockCityThree);

            var composer = Container.Resolve<CityComposer>();

            composer.ClearRuntime();

            mockCityOne  .Verify(city => city.Destroy(), Times.Once, "CityOne was not destroyed as expected");
            mockCityTwo  .Verify(city => city.Destroy(), Times.Once, "CityTwo was not destroyed as expected");
            mockCityThree.Verify(city => city.Destroy(), Times.Once, "CityThree was not destroyed as expected");
        }

        [Test]
        public void ComposeCities_StoresLocationAsCoordinates() {
            BuildCity(BuildHexCell(new HexCoordinates(0, 1)), BuildCivilization(), "City One",   BuildUnit());
            BuildCity(BuildHexCell(new HexCoordinates(2, 3)), BuildCivilization(), "City Two",   BuildUnit());
            BuildCity(BuildHexCell(new HexCoordinates(4, 5)), BuildCivilization(), "City Three", BuildUnit());

            var mapData = new SerializableMapData();

            var composer = Container.Resolve<CityComposer>();

            composer.ComposeCities(mapData);

            Assert.That(
                mapData.Cities[0].Location.Equals(new HexCoordinates(0, 1)),
                "First city data does not have the expected coordinates"
            );

            Assert.That(
                mapData.Cities[1].Location.Equals(new HexCoordinates(2, 3)),
                "Second city data does not have the expected coordinates"
            );

            Assert.That(
                mapData.Cities[2].Location.Equals(new HexCoordinates(4, 5)),
                "Third city data does not have the expected coordinates"
            );
        }

        [Test]
        public void ComposeCities_StoresOwnerAsName() {
            BuildCity(BuildHexCell(new HexCoordinates(0, 1)), BuildCivilization("Civ One"),   "City One",   BuildUnit());
            BuildCity(BuildHexCell(new HexCoordinates(2, 3)), BuildCivilization("Civ Two"),   "City Two",   BuildUnit());
            BuildCity(BuildHexCell(new HexCoordinates(4, 5)), BuildCivilization("Civ Three"), "City Three", BuildUnit());

            var mapData = new SerializableMapData();

            var composer = Container.Resolve<CityComposer>();

            composer.ComposeCities(mapData);

            Assert.That(
                mapData.Cities[0].Owner.Equals("Civ One"),
                "First city data has an unexpected Owner"
            );

            Assert.That(
                mapData.Cities[1].Owner.Equals("Civ Two"),
                "Second city data has an unexpected Owner"
            );

            Assert.That(
                mapData.Cities[2].Owner.Equals("Civ Three"),
                "Third city data has an unexpected Owner"
            );
        }

        [Test]
        public void ComposeCities_StoresIntrinsicFieldsProperly() {
            var cityOne   = BuildCity(BuildHexCell(new HexCoordinates(0, 1)), BuildCivilization(), "City One",   BuildUnit());
            var cityTwo   = BuildCity(BuildHexCell(new HexCoordinates(2, 3)), BuildCivilization(), "City Two",   BuildUnit());
            var cityThree = BuildCity(BuildHexCell(new HexCoordinates(4, 5)), BuildCivilization(), "City Three", BuildUnit());

            SetCityFields(cityOne,    1,  11, -1,  YieldFocusType.TotalYield);
            SetCityFields(cityTwo,    2, -2,   22, YieldFocusType.Culture);
            SetCityFields(cityThree, -3,  33,  3,  YieldFocusType.Production);

            var mapData = new SerializableMapData();

            var composer = Container.Resolve<CityComposer>();

            composer.ComposeCities(mapData);

            Assert.AreEqual(1,                         mapData.Cities[0].Population,       "CityOne data has an unexpected Population");
            Assert.AreEqual(11,                        mapData.Cities[0].FoodStockpile,    "CityOne data has an unexpected FoodStockpile");
            Assert.AreEqual(-1,                        mapData.Cities[0].CultureStockpile, "CityOne data has an unexpected CultureStockpile");
            Assert.AreEqual(YieldFocusType.TotalYield, mapData.Cities[0].YieldFocus,       "CityOne data has an unexpected ResourceFocus");

            Assert.AreEqual(2,                      mapData.Cities[1].Population,       "CityTwo data has an unexpected Population");
            Assert.AreEqual(-2,                     mapData.Cities[1].FoodStockpile,    "CityTwo data has an unexpected FoodStockpile");
            Assert.AreEqual(22,                     mapData.Cities[1].CultureStockpile, "CityTwo data has an unexpected CultureStockpile");
            Assert.AreEqual(YieldFocusType.Culture, mapData.Cities[1].YieldFocus,       "CityTwo data has an unexpected ResourceFocus");

            Assert.AreEqual(-3,                        mapData.Cities[2].Population,       "CityThree data has an unexpected Population");
            Assert.AreEqual(33,                        mapData.Cities[2].FoodStockpile,    "CityThree data has an unexpected FoodStockpile");
            Assert.AreEqual(3,                         mapData.Cities[2].CultureStockpile, "CityThree data has an unexpected CultureStockpile");
            Assert.AreEqual(YieldFocusType.Production, mapData.Cities[2].YieldFocus,       "CityThree data has an unexpected ResourceFocus");
        }

        [Test]
        public void ComposeCities_StoresCombatFacadeFieldsProperly() {
            var cityOne   = BuildCity(BuildHexCell(new HexCoordinates(0, 1)), BuildCivilization(), "City One",   BuildUnit());
            var cityTwo   = BuildCity(BuildHexCell(new HexCoordinates(2, 3)), BuildCivilization(), "City Two",   BuildUnit());
            var cityThree = BuildCity(BuildHexCell(new HexCoordinates(4, 5)), BuildCivilization(), "City Three", BuildUnit());

            cityOne.CombatFacade.CurrentHitpoints       = 100;
            cityOne.CombatFacade.CurrentMovement = 1;

            cityTwo.CombatFacade.CurrentHitpoints       = 200;
            cityTwo.CombatFacade.CurrentMovement = 2;

            cityThree.CombatFacade.CurrentHitpoints       = 300;
            cityThree.CombatFacade.CurrentMovement = 3;

            var mapData = new SerializableMapData();

            var composer = Container.Resolve<CityComposer>();

            composer.ComposeCities(mapData);

            Assert.AreEqual(100, mapData.Cities[0].Hitpoints,       "CityOne data has an unexpected Hitpoints value");
            Assert.AreEqual(1,   mapData.Cities[0].CurrentMovement, "CityOne data has an unexpected CurrentMovement value");

            Assert.AreEqual(200, mapData.Cities[1].Hitpoints,       "CityTwo data has an unexpected Hitpoints value");
            Assert.AreEqual(2,   mapData.Cities[1].CurrentMovement, "CityTwo data has an unexpected CurrentMovement value");

            Assert.AreEqual(300, mapData.Cities[2].Hitpoints,       "CityThree data has an unexpected Hitpoints value");
            Assert.AreEqual(3,   mapData.Cities[2].CurrentMovement, "CityThree data has an unexpected CurrentMovement value");
        }

        [Test]
        public void ComposeCities_StoresActiveProjectProperly() {
            Mock<ICity> mockCityOne, mockCityTwo, mockCityThree;

            BuildCity(BuildHexCell(new HexCoordinates(0, 0)), BuildCivilization(), "City One",   BuildUnit(), out mockCityOne);
            BuildCity(BuildHexCell(new HexCoordinates(0, 0)), BuildCivilization(), "City Two",   BuildUnit(), out mockCityTwo);
            BuildCity(BuildHexCell(new HexCoordinates(0, 0)), BuildCivilization(), "City Three", BuildUnit(), out mockCityThree);

            var buildingTemplateOne = BuildBuildingTemplate("Building Template One");
            var buildingTemplateTwo = BuildBuildingTemplate("Building Template Two");

            var unitTemplateOne = BuildUnitTemplate("Unit Template One");
            var unitTemplateTwo = BuildUnitTemplate("Unit Template Two");

            mockCityOne  .Setup(city => city.ActiveProject).Returns(BuildProject(buildingTemplateOne, null, 0));
            mockCityTwo  .Setup(city => city.ActiveProject).Returns(BuildProject(null, unitTemplateOne, 0));
            mockCityThree.Setup(city => city.ActiveProject).Returns(BuildProject(buildingTemplateTwo, unitTemplateTwo, 0));

            var mapData = new SerializableMapData();

            var composer = Container.Resolve<CityComposer>();

            composer.ComposeCities(mapData);

            var projectOne   = mapData.Cities[0].ActiveProject;
            var projectTwo   = mapData.Cities[1].ActiveProject;
            var projectThree = mapData.Cities[2].ActiveProject;

            Assert.AreEqual("Building Template One", projectOne.BuildingToConstruct,   "ProjectOne has an unexpected BuildingToConstruct value");
            Assert.AreEqual(null,                    projectOne.UnitToConstruct,       "ProjectOne has an unexpected UnitToConstruct value");

            Assert.AreEqual(null,                    projectTwo.BuildingToConstruct,   "ProjectTwo has an unexpected BuildingToConstruct value");
            Assert.AreEqual("Unit Template One",     projectTwo.UnitToConstruct,       "ProjectTwo has an unexpected UnitToConstruct value");

            Assert.AreEqual("Building Template Two", projectThree.BuildingToConstruct, "ProjectThree has an unexpected BuildingToConstruct value");
            Assert.AreEqual("Unit Template Two",     projectThree.UnitToConstruct,     "ProjectThree has an unexpected UnitToConstruct value");
        }




        [Test]
        public void DecomposeCities_PassesCorrectLocationAndOwnerToCityFactory() {
            var cellOne   = BuildHexCell(new HexCoordinates(1, 1));
            var cellTwo   = BuildHexCell(new HexCoordinates(2, 2));
            var cellThree = BuildHexCell(new HexCoordinates(3, 3));

            var civOne   = BuildCivilization("Civ One");
            var civTwo   = BuildCivilization("Civ Two");
            var civThree = BuildCivilization("Civ Three");

            var mapData = new SerializableMapData() {
                Cities = new List<SerializableCityData>() {
                    new SerializableCityData() { Name = "City One",   Location = new HexCoordinates(1, 1), Owner = "Civ One" },
                    new SerializableCityData() { Name = "City Two",   Location = new HexCoordinates(2, 2), Owner = "Civ Two" },
                    new SerializableCityData() { Name = "City Three", Location = new HexCoordinates(3, 3), Owner = "Civ Three" },
                }
            };

            var composer = Container.Resolve<CityComposer>();

            composer.DecomposeCities(mapData);

            Assert.AreEqual(3, AllCities.Count, "An unexpected number of cities were created");

            MockCityFactory.Verify(
                factory => factory.Create(cellOne, civOne, "City One"), Times.Once,
                "The expected creation of CityOne did not happen as expected"
            );

            MockCityFactory.Verify(
                factory => factory.Create(cellTwo, civTwo, "City Two"), Times.Once,
                "The expected creation of CityTwo did not happen as expected"
            );

            MockCityFactory.Verify(
                factory => factory.Create(cellThree, civThree, "City Three"), Times.Once,
                "The expected creation of CityThree did not happen as expected"
            );
        }

        [Test]
        public void DecomposeCities_SetsIntrinsicFieldsProperly() {
            BuildHexCell(new HexCoordinates(1, 1));
            BuildHexCell(new HexCoordinates(2, 2));
            BuildHexCell(new HexCoordinates(3, 3));

            BuildCivilization("Civ One");
            BuildCivilization("Civ Two");
            BuildCivilization("Civ Three");

            var mapData = new SerializableMapData() {
                Cities = new List<SerializableCityData>() {
                    new SerializableCityData() {
                        Location = new HexCoordinates(1, 1), Owner = "Civ One",
                        Population = 1, CultureStockpile = 10, FoodStockpile = 100,
                        YieldFocus = YieldFocusType.TotalYield
                    },
                    new SerializableCityData() {
                        Location = new HexCoordinates(2, 2), Owner = "Civ Two",
                        Population = 2, CultureStockpile = 20, FoodStockpile = 200,
                        YieldFocus = YieldFocusType.Production
                    },
                    new SerializableCityData() {
                        Location = new HexCoordinates(3, 3), Owner = "Civ Three",
                        Population = 3, CultureStockpile = 30, FoodStockpile = 300,
                        YieldFocus = YieldFocusType.Culture
                    },
                }
            };

            var composer = Container.Resolve<CityComposer>();

            composer.DecomposeCities(mapData);

            Assert.AreEqual(3, AllCities.Count, "An unexpected number of cities were created");

            Assert.AreEqual(1, AllCities[0].Population, "CityOne has an unexpected Population");
            Assert.AreEqual(2, AllCities[1].Population, "CityTwo has an unexpected Population");
            Assert.AreEqual(3, AllCities[2].Population, "CityThree has an unexpected Population");

            Assert.AreEqual(10, AllCities[0].CultureStockpile, "CityOne has an unexpected CultureStockpile");
            Assert.AreEqual(20, AllCities[1].CultureStockpile, "CityTwo has an unexpected CultureStockpile");
            Assert.AreEqual(30, AllCities[2].CultureStockpile, "CityThree has an unexpected CultureStockpile");

            Assert.AreEqual(100, AllCities[0].FoodStockpile, "CityOne has an unexpected FoodStockpile");
            Assert.AreEqual(200, AllCities[1].FoodStockpile, "CityTwo has an unexpected FoodStockpile");
            Assert.AreEqual(300, AllCities[2].FoodStockpile, "CityThree has an unexpected FoodStockpile");

            Assert.AreEqual(YieldFocusType.TotalYield, AllCities[0].YieldFocus, "CityOne has an unexpected ResourceFocus");
            Assert.AreEqual(YieldFocusType.Production, AllCities[1].YieldFocus, "CityTwo has an unexpected ResourceFocus");
            Assert.AreEqual(YieldFocusType.Culture,    AllCities[2].YieldFocus, "CityThree has an unexpected ResourceFocus");
        }

        [Test]
        public void DecomposeCities_SetCombatFacadeFieldsProperly() {
            BuildHexCell(new HexCoordinates(1, 1));
            BuildHexCell(new HexCoordinates(2, 2));
            BuildHexCell(new HexCoordinates(3, 3));

            BuildCivilization("Civ One");
            BuildCivilization("Civ Two");
            BuildCivilization("Civ Three");

            var mapData = new SerializableMapData() {
                Cities = new List<SerializableCityData>() {
                    new SerializableCityData() {
                        Location = new HexCoordinates(1, 1), Owner = "Civ One",
                        CurrentMovement = 1, Hitpoints = 100
                    },
                    new SerializableCityData() {
                        Location = new HexCoordinates(2, 2), Owner = "Civ Two",
                        CurrentMovement = 2, Hitpoints = 200
                    },
                    new SerializableCityData() {
                        Location = new HexCoordinates(3, 3), Owner = "Civ Three",
                        CurrentMovement = 3, Hitpoints = 300
                    },
                }
            };

            var composer = Container.Resolve<CityComposer>();

            composer.DecomposeCities(mapData);

            Assert.AreEqual(3, AllCities.Count, "An unexpected number of cities were created");

            Assert.AreEqual(1, AllCities[0].CombatFacade.CurrentMovement, "CityOne.CombatFacade has an unexpected CurrentMovement");
            Assert.AreEqual(2, AllCities[1].CombatFacade.CurrentMovement, "CityTwo.CombatFacade has an unexpected CurrentMovement");
            Assert.AreEqual(3, AllCities[2].CombatFacade.CurrentMovement, "CityThree.CombatFacade has an unexpected CurrentMovement");

            Assert.AreEqual(100, AllCities[0].CombatFacade.CurrentHitpoints, "CityOne.CombatFacade has an unexpected CurrentMovement");
            Assert.AreEqual(200, AllCities[1].CombatFacade.CurrentHitpoints, "CityTwo.CombatFacade has an unexpected CurrentMovement");
            Assert.AreEqual(300, AllCities[2].CombatFacade.CurrentHitpoints, "CityThree.CombatFacade has an unexpected CurrentMovement");
        }

        [Test]
        public void DecomposeCities_ConstructsAndSetsProjectsProperly() {
            BuildHexCell(new HexCoordinates(1, 1));
            BuildHexCell(new HexCoordinates(2, 2));
            BuildHexCell(new HexCoordinates(3, 3));

            BuildCivilization("Civ One");
            BuildCivilization("Civ Two");
            BuildCivilization("Civ Three");

            var buildingTemplateOne = BuildBuildingTemplate("Building Template One");
            var buildingTemplateTwo = BuildBuildingTemplate("Building Template Two");

            BuildUnitTemplate("Unit Template One");
            var unitTemplateTwo = BuildUnitTemplate("Unit Template Two");

            var mapData = new SerializableMapData() {
                Cities = new List<SerializableCityData>() {
                    new SerializableCityData() {
                        Location = new HexCoordinates(1, 1), Owner = "Civ One",
                        ActiveProject = new SerializableProjectData() {
                            BuildingToConstruct = "Building Template One", UnitToConstruct = null,
                            Progress = 10
                        }
                    },
                    new SerializableCityData() {
                        Location = new HexCoordinates(2, 2), Owner = "Civ Two",
                        ActiveProject = new SerializableProjectData() {
                            BuildingToConstruct = "Building Template Two", UnitToConstruct = "Unit Template One",
                            Progress = 20
                        }
                    },
                    new SerializableCityData() {
                        Location = new HexCoordinates(3, 3), Owner = "Civ Three",
                        ActiveProject = new SerializableProjectData() {
                            BuildingToConstruct = null, UnitToConstruct = "Unit Template Two",
                            Progress = 20
                        }
                    },
                }
            };

            var composer = Container.Resolve<CityComposer>();

            composer.DecomposeCities(mapData);

            var projectOne   = AllCities[0].ActiveProject;
            var projectTwo   = AllCities[1].ActiveProject;
            var projectThree = AllCities[2].ActiveProject;

            Assert.AreEqual(buildingTemplateOne, projectOne.BuildingToConstruct, "Project of CityOne has an incorrect BuildingToConstruct");
            Assert.AreEqual(null,                projectOne.UnitToConstruct,     "Project of CityOne has an incorrect UnitToConstruct");
            Assert.AreEqual(10,                  projectOne.Progress,            "Project of CityOne has an incorrect Progress");

            Assert.AreEqual(buildingTemplateTwo, projectTwo.BuildingToConstruct, "Project of CityTwo has an incorrect BuildingToConstruct");
            Assert.AreEqual(null,                projectTwo.UnitToConstruct,     "Project of CityTwo has an incorrect UnitToConstruct");
            Assert.AreEqual(20,                  projectTwo.Progress,            "Project of CityTwo has an incorrect Progress");

            Assert.AreEqual(null,            projectThree.BuildingToConstruct, "Project of CityThree has an incorrect BuildingToConstruct");
            Assert.AreEqual(unitTemplateTwo, projectThree.UnitToConstruct,     "Project of CityThree has an incorrect UnitToConstruct");
            Assert.AreEqual(20,              projectThree.Progress,            "Project of CityThree has an incorrect Progress");
        }

        #endregion

        #region utilities

        private IHexCell BuildHexCell(HexCoordinates coordinates) {
            var mockCell = new Mock<IHexCell>();

            mockCell.Setup(cell => cell.Coordinates).Returns(coordinates);

            var newCell = mockCell.Object;

            MockGrid.Setup(grid => grid.GetCellAtCoordinates(coordinates)).Returns(newCell);

            return newCell;
        }

        private ICivilization BuildCivilization(string name = "") {
            var mockCiv = new Mock<ICivilization>();

            var mockTemplate = new Mock<ICivilizationTemplate>();

            mockTemplate.Setup(template => template.Name).Returns(name);

            mockCiv.Setup(civ => civ.Template).Returns(mockTemplate.Object);

            var newCiv = mockCiv.Object;

            AllCivilizations.Add(newCiv);

            return newCiv;
        }

        private IUnit BuildUnit() {
            var mockUnit = new Mock<IUnit>();

            mockUnit.SetupAllProperties();

            var newUnit = mockUnit.Object;

            return newUnit;
        }

        private ICity BuildCity(IHexCell location, ICivilization owner, string name, IUnit combatFacade) {
            Mock<ICity> mock;
            return BuildCity(location, owner, name, combatFacade, out mock);
        }

        private ICity BuildCity(IHexCell location, ICivilization owner, string name, IUnit combatFacade, out Mock<ICity> mock) {
            mock = new Mock<ICity>();

            mock.SetupAllProperties();

            mock.Setup(city => city.CombatFacade).Returns(combatFacade);
            mock.Setup(city => city.Name).Returns(name);

            var newCity = mock.Object;

            MockCityLocationCanon  .Setup(canon => canon.GetOwnerOfPossession(newCity)).Returns(location);
            MockCityPossessionCanon.Setup(canon => canon.GetOwnerOfPossession(newCity)).Returns(owner);

            AllCities.Add(newCity);

            return newCity;
        }

        private void SetCityFields(
            ICity city, int population, int foodStockpile, int cultureStockpile, YieldFocusType resourceFocus
        ){
            city.Population       = population;
            city.FoodStockpile    = foodStockpile;
            city.CultureStockpile = cultureStockpile;
            city.YieldFocus       = resourceFocus;
        }

        private IBuildingTemplate BuildBuildingTemplate(string name) {
            var mockTemplate = new Mock<IBuildingTemplate>();

            mockTemplate.Name = name;
            mockTemplate.Setup(template => template.name).Returns(name);

            var newTemplate = mockTemplate.Object;

            AvailableBuildingTemplates.Add(newTemplate);

            return newTemplate;
        }

        private IUnitTemplate BuildUnitTemplate(string name) {
            var mockTemplate = new Mock<IUnitTemplate>();

            mockTemplate.Name = name;
            mockTemplate.Setup(template => template.name).Returns(name);

            var newTemplate = mockTemplate.Object;

            AvailableUnitTemplates.Add(newTemplate);

            return newTemplate;
        }

        private IProductionProject BuildProject(IBuildingTemplate building, IUnitTemplate unit, int progress) {
            var mockProject = new Mock<IProductionProject>();

            mockProject.SetupAllProperties();

            mockProject.Setup(project => project.BuildingToConstruct).Returns(building);
            mockProject.Setup(project => project.UnitToConstruct)    .Returns(unit);

            var newProject = mockProject.Object;

            newProject.Progress = progress;

            return newProject;
        }

        #endregion

        #endregion

    }

}
