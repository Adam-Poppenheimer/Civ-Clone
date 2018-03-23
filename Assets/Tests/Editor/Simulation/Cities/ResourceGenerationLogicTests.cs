using System;
using System.Collections;
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
using Assets.Simulation.Cities.Distribution;
using Assets.Simulation.Civilizations;
using Assets.Simulation.WorkerSlots;

namespace Assets.Tests.Simulation.Cities {

    [TestFixture]
    public class ResourceGenerationLogicTests : ZenjectUnitTestFixture {

        #region internal types

        public class CityResourceTestData {

            public CityTestData City;

            public CityConfigTestData Config;

        }

        public class CityTestData {

            public int Population;

            public int UnemployedPeople;

            public List<CellTestData> Territory = new List<CellTestData>();

            public List<BuildingTestData> Buildings = new List<BuildingTestData>();

            public CivilizationTestData OwningCivilization = new CivilizationTestData() { };

            public ResourceSummary CityYieldMultipliers = ResourceSummary.Empty;

        }

        public class CellTestData {

            public ResourceSummary CellYield = ResourceSummary.Empty;

            public SlotTestData Slot = new SlotTestData() { };

            public bool SuppressSlot;

        }

        public class BuildingTestData {

            public ResourceSummary BuildingYield;

            public ResourceSummary SlotYield;

        }

        public class SlotTestData {

            public bool IsOccupied;

        }

        public class CivilizationTestData {

            public ResourceSummary YieldMultipliers = ResourceSummary.Empty;

        }

        public class CityConfigTestData {

            public ResourceSummary UnemployedYield = ResourceSummary.Empty;

            public ResourceSummary LocationYield = ResourceSummary.Empty;

        }

        #endregion

        #region static fields and properties

        public static IEnumerable GetYieldOfCellForCityTestCases {
            get {
                yield return new TestCaseData(
                    new CellTestData() {
                        CellYield = new ResourceSummary(food: 1f, production: 2f, gold: 3f),
                        Slot = new SlotTestData() {
                            IsOccupied = true
                        }
                    },
                    new CityTestData() { }
                ).SetName("Occupied slot, no additional modifiers").Returns(new ResourceSummary(
                    food: 1f, production: 2f, gold: 3f
                ));

                yield return new TestCaseData(
                    new CellTestData() {
                        CellYield = new ResourceSummary(food: 1f, production: 2f, gold: 3f),
                        Slot = new SlotTestData() { IsOccupied = false }
                    },
                    new CityTestData() { }
                ).SetName("Unoccupied slot, no additional modifiers").Returns(new ResourceSummary(
                    food: 1f, production: 2f, gold: 3f
                ));

                yield return new TestCaseData(
                    new CellTestData() {
                        CellYield = new ResourceSummary(food: 1f, production: 2f, gold: 3f),
                        Slot = new SlotTestData() { IsOccupied = true }
                    },
                    new CityTestData() {
                        CityYieldMultipliers = new ResourceSummary(food: 0f, production: -0.5f, gold: 2f, culture: 3f),
                    }                    
                ).SetName("Occupied slot, city has yield modifiers").Returns(new ResourceSummary(
                    food: 1f, production: 1f, gold: 9f
                ));

                yield return new TestCaseData(
                    new CellTestData() {
                        CellYield = new ResourceSummary(food: 1f, production: 2f, gold: 3f),
                        Slot = new SlotTestData() { IsOccupied = true }
                    },
                    new CityTestData() {
                        OwningCivilization   = new CivilizationTestData() {
                            YieldMultipliers = new ResourceSummary(food: 0f, production: -0.5f, gold: 2f, culture: 3f)
                        },
                    }                    
                ).SetName("Occupied slot, civilization has yield modifiers").Returns(new ResourceSummary(
                    food: 1f, production: 1f, gold: 9f
                ));

                yield return new TestCaseData(
                    new CellTestData() {
                        CellYield = new ResourceSummary(food: 1f, production: 1f, gold: 1f, culture: 1f),
                        Slot = new SlotTestData() { IsOccupied = true }
                    },
                    new CityTestData() {
                        OwningCivilization   = new CivilizationTestData() {
                            YieldMultipliers = new ResourceSummary(food: 2f, gold: 1f)
                        },
                        CityYieldMultipliers = new ResourceSummary(food: 1f, production: -1f),
                    }
                ).SetName("Occupied slot, city and civilization modifiers in play").Returns(new ResourceSummary(
                    food: 4f, production: 0f, gold: 2f, culture: 1f
                ));
            }
        }

        public static IEnumerable GetYieldOfUnemployedPersonForCityTestCases {
            get {
                yield return new TestCaseData(
                    new CityTestData() { },
                    new CityConfigTestData() {
                        UnemployedYield = new ResourceSummary(food: 1f, production: 1f)
                    }
                ).SetName("No modifiers in play returns configured base").Returns(
                    new ResourceSummary(food: 1f, production: 1f)
                );

                yield return new TestCaseData(
                    new CityTestData() {
                        CityYieldMultipliers = new ResourceSummary(food: 2f, production: 3f, gold: -4f),
                    },
                    new CityConfigTestData() {
                        UnemployedYield = new ResourceSummary(food: 1f, production: 1f)
                    }
                ).SetName("city modifiers in play").Returns(
                    new ResourceSummary(food: 3f, production: 4f)
                );

                yield return new TestCaseData(
                    new CityTestData() {
                        OwningCivilization = new CivilizationTestData() {
                            YieldMultipliers = new ResourceSummary(food: 2f, production: 3f, gold: -4f)
                        }
                    },
                    new CityConfigTestData() {
                        UnemployedYield = new ResourceSummary(food: 1f, production: 1f)
                    }
                ).SetName("Civilization modifiers in play").Returns(
                    new ResourceSummary(food: 3f, production: 4f)
                );

                yield return new TestCaseData(
                    new CityTestData() {
                        OwningCivilization = new CivilizationTestData() {
                            YieldMultipliers = new ResourceSummary(food: 2f, production: 3f, gold: -4f)
                        },
                        CityYieldMultipliers = new ResourceSummary(food: -1f, production: 0f, culture: 0.5f)
                    },
                    new CityConfigTestData() {
                        UnemployedYield = new ResourceSummary(food: 1f, production: 1f, gold: 1f, culture: 1f)
                    }
                ).SetName("City and civilization modifiers in play").Returns(
                    new ResourceSummary(food: 2f, production: 4f, gold: -3f, culture: 1.5f)
                );
            }
        }

        public static IEnumerable GetYieldOfBuildingForCityTestCases {
            get {
                yield return new TestCaseData(
                    new BuildingTestData() {
                        BuildingYield = new ResourceSummary(food: 4f, production: 2f, gold: 1f, culture: 6f),
                    },
                    new CityTestData() { }
                ).SetName("Building with yield, no additional modifiers").Returns(
                    new ResourceSummary(food: 4f, production: 2f, gold: 1f, culture: 6f)
                );

                yield return new TestCaseData(
                    new BuildingTestData() {
                        BuildingYield = new ResourceSummary(food: 4f, production: 2f, gold: 1f, culture: 6f),
                    },
                    new CityTestData() {
                        CityYieldMultipliers = new ResourceSummary(production: 2f, gold: 0.5f, culture: -1f)
                    }
                ).SetName("Building with yield, city modifiers present").Returns(
                    new ResourceSummary(food: 4f, production: 6f, gold: 1f * 1.5f, culture: 0f)
                );

                yield return new TestCaseData(
                    new BuildingTestData() {
                        BuildingYield = new ResourceSummary(food: 4f, production: 2f, gold: 1f, culture: 6f),
                    },
                    new CityTestData() {
                        OwningCivilization = new CivilizationTestData() {
                            YieldMultipliers = new ResourceSummary(production: 2f, gold: 0.5f, culture: -1f)
                        }
                    }
                ).SetName("Building with yield, civilization modifiers present").Returns(
                    new ResourceSummary(food: 4f, production: 6f, gold: 1f * 1.5f, culture: 0f)
                );

                yield return new TestCaseData(
                    new BuildingTestData() {
                        BuildingYield = new ResourceSummary(food: 1f, production: 1f, gold: 1f, culture: 1f, science: 1f),
                    },
                    new CityTestData() {
                        CityYieldMultipliers = new ResourceSummary(food: 1f, production: -1f, gold: 2f),
                        OwningCivilization = new CivilizationTestData() {
                            YieldMultipliers = new ResourceSummary(food: 0.5f, production: -1f, gold: 0.5f, culture: -1f)
                        }
                    }
                ).SetName("Building with yield, city and civilization modifiers present").Returns(
                    new ResourceSummary(food: 2.5f, production: -1f, gold: 3.5f, culture: 0f, science: 1f)
                );
            }
        }

        public static IEnumerable GetYieldOfBuildingSlotsForCityTestCases {
            get {
                yield return new TestCaseData(
                    new BuildingTestData() {
                        SlotYield = new ResourceSummary(food: 4f, production: 2f, gold: 1f, culture: 6f),
                    },
                    new CityTestData() { }
                ).SetName("Building with slot yields, no additional modifiers").Returns(
                    new ResourceSummary(food: 4f, production: 2f, gold: 1f, culture: 6f)
                );

                yield return new TestCaseData(
                    new BuildingTestData() {
                        SlotYield = new ResourceSummary(food: 4f, production: 2f, gold: 1f, culture: 6f),
                    },
                    new CityTestData() {
                        CityYieldMultipliers = new ResourceSummary(production: 2f, gold: 0.5f, culture: -1f)
                    }
                ).SetName("Building with slot yields, city modifiers present").Returns(
                    new ResourceSummary(food: 4f, production: 6f, gold: 1f * 1.5f, culture: 0f)
                );

                yield return new TestCaseData(
                    new BuildingTestData() {
                        SlotYield = new ResourceSummary(food: 4f, production: 2f, gold: 1f, culture: 6f),
                    },
                    new CityTestData() {
                        OwningCivilization = new CivilizationTestData() {
                            YieldMultipliers = new ResourceSummary(production: 2f, gold: 0.5f, culture: -1f)
                        }
                    }
                ).SetName("Building with slot yields, civilization modifiers present").Returns(
                    new ResourceSummary(food: 4f, production: 6f, gold: 1f * 1.5f, culture: 0f)
                );

                yield return new TestCaseData(
                    new BuildingTestData() {
                        SlotYield = new ResourceSummary(food: 1f, production: 1f, gold: 1f, culture: 1f, science: 1f),
                    },
                    new CityTestData() {
                        CityYieldMultipliers = new ResourceSummary(food: 1f, production: -1f, gold: 2f),
                        OwningCivilization = new CivilizationTestData() {
                            YieldMultipliers = new ResourceSummary(food: 0.5f, production: -1f, gold: 0.5f, culture: -1f)
                        }
                    }
                ).SetName("Building with slot yields, city and civilization modifiers present").Returns(
                    new ResourceSummary(food: 2.5f, production: -1f, gold: 3.5f, culture: 0f, science: 1f)
                );
            }
        }

        public static IEnumerable GetBaseYieldOfCityTestCases {
            get {
                yield return new TestCaseData(
                    new CityTestData() {  },
                    new CityConfigTestData() {
                        LocationYield = new ResourceSummary(food: 2f, production: 1f, gold: 1f)
                    }
                ).SetName("Considers the configured yield of the city's location").Returns(new ResourceSummary(
                    food: 2f, production: 1f, gold: 1f
                ));

                yield return new TestCaseData(
                    new CityTestData() {
                        CityYieldMultipliers = new ResourceSummary(food: 1f, production: -1f),
                        OwningCivilization = new CivilizationTestData() {
                            YieldMultipliers = new ResourceSummary(production: 0.5f, gold: 3f)
                        }
                    },
                    new CityConfigTestData() {
                        LocationYield = new ResourceSummary(food: 2f, production: 1f, gold: 1f)
                    }
                ).SetName("Location yield modified by multipliers").Returns(new ResourceSummary(
                    food: 4f, production: 0.5f, gold: 4f
                ));

                yield return new TestCaseData(
                    new CityTestData() {
                        Population = 5
                    },
                    new CityConfigTestData() {

                    }
                ).SetName("Adds 1 science for each person in the city").Returns(new ResourceSummary(
                    science: 5f
                ));

                yield return new TestCaseData(
                    new CityTestData() {
                        Population = 5,
                        CityYieldMultipliers = new ResourceSummary(science: 1f),
                        OwningCivilization = new CivilizationTestData() {
                            YieldMultipliers = new ResourceSummary(science: 1.5f)
                        }
                    },
                    new CityConfigTestData() {
                        
                    }
                ).SetName("Science per population modified by multipliers").Returns(new ResourceSummary(
                    science: 5f * 3.5f
                ));
            }
        }

        public static IEnumerable GetTotalYieldForCityTestCases {
            get {
                yield return new TestCaseData(
                    new CityTestData() {
                        Population = 0,
                        Territory = new List<CellTestData>() {
                            new CellTestData() {
                                CellYield = new ResourceSummary(food: 1),
                                Slot = new SlotTestData() { IsOccupied = true }
                            },
                            new CellTestData() {
                                CellYield = new ResourceSummary(production: 2),
                                Slot = new SlotTestData() { IsOccupied = false }
                            },
                            new CellTestData() {
                                CellYield = new ResourceSummary(gold: 3),
                                Slot = new SlotTestData() { IsOccupied = true }
                            },
                            new CellTestData() {
                                CellYield = new ResourceSummary(culture: 4),
                                Slot = new SlotTestData() { IsOccupied = false }
                            },
                            new CellTestData() {
                                CellYield = new ResourceSummary(science: 5),
                                Slot = new SlotTestData() { IsOccupied = true }
                            }
                        }
                    },
                    new CityConfigTestData() {
                        UnemployedYield = new ResourceSummary()
                    }
                ).SetName("Accounts for yield of all occupied territory").Returns(new ResourceSummary(
                    food: 1f, gold: 3f, science: 5f
                ));

                yield return new TestCaseData(
                    new CityTestData() {
                        Population = 0,
                        Territory = new List<CellTestData>() {
                            new CellTestData() {
                                CellYield = new ResourceSummary(food: 1),
                                Slot = new SlotTestData() { IsOccupied = true }
                            },
                            new CellTestData() {
                                CellYield = new ResourceSummary(production: 2),
                                SuppressSlot = true,
                                Slot = new SlotTestData() { IsOccupied = false }
                            },
                            new CellTestData() {
                                CellYield = new ResourceSummary(gold: 3),
                                SuppressSlot = true,
                                Slot = new SlotTestData() { IsOccupied = true }
                            },
                            new CellTestData() {
                                CellYield = new ResourceSummary(culture: 4),
                                Slot = new SlotTestData() { IsOccupied = false }
                            },
                            new CellTestData() {
                                CellYield = new ResourceSummary(science: 5),
                                Slot = new SlotTestData() { IsOccupied = true }
                            }
                        }
                    },
                    new CityConfigTestData() {
                        UnemployedYield = new ResourceSummary()
                    }
                ).SetName("Ignores suppressed slots").Returns(new ResourceSummary(
                    food: 1f, science: 5f
                ));

                yield return new TestCaseData(
                    new CityTestData() {
                        Population = 0,
                        Buildings = new List<BuildingTestData>() {
                            new BuildingTestData() { BuildingYield = new ResourceSummary(food:       1f) },
                            new BuildingTestData() { BuildingYield = new ResourceSummary(production: 2f) },
                            new BuildingTestData() { BuildingYield = new ResourceSummary(gold:       3f) },
                            new BuildingTestData() { BuildingYield = new ResourceSummary(culture:    4f) },
                            new BuildingTestData() { BuildingYield = new ResourceSummary(science:    5f) },
                        }
                    },
                    new CityConfigTestData() {
                        UnemployedYield = new ResourceSummary()
                    }
                ).SetName("Accounts for yield of all buildings").Returns(new ResourceSummary(
                    food: 1f, production: 2f, gold: 3f, culture: 4f, science: 5f
                ));

                yield return new TestCaseData(
                    new CityTestData() {
                        Population = 0,
                        UnemployedPeople = 3
                    },
                    new CityConfigTestData() {
                        UnemployedYield = new ResourceSummary(production: 1f)
                    }
                ).SetName("Accounts for yield of unemployed people").Returns(new ResourceSummary(
                    production: 3f
                ));

                yield return new TestCaseData(
                    new CityTestData() {
                        Population = 5
                    },
                    new CityConfigTestData() {
                        LocationYield = new ResourceSummary(food: 2f, production: 1f, gold: 1f)
                    }
                ).SetName("Accounts for base city yield").Returns(new ResourceSummary(
                    food: 2f, production: 1f, gold: 1f, science: 5f
                ));
            }
        }

        #endregion

        #region instance fields and properties

        private Mock<ICityConfig>                                   MockConfig;
        private Mock<IPossessionRelationship<ICity, IHexCell>>      MockCellPossessionCanon;
        private Mock<IPossessionRelationship<ICity, IBuilding>>     MockBuildingPossessionCanon;
        private Mock<IIncomeModifierLogic>                          MockIncomeModifierLogic;
        private Mock<IPossessionRelationship<ICivilization, ICity>> MockCityPossessionCanon;
        private Mock<ICellResourceLogic>                            MockCellResourceLogic;
        private Mock<IBuildingResourceLogic>                        MockBuildingResourceLogic;
        private Mock<IUnemploymentLogic>                            MockUnemploymentLogic;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockConfig                  = new Mock<ICityConfig>();
            MockCellPossessionCanon     = new Mock<IPossessionRelationship<ICity, IHexCell>>();
            MockBuildingPossessionCanon = new Mock<IPossessionRelationship<ICity, IBuilding>>();     
            MockIncomeModifierLogic     = new Mock<IIncomeModifierLogic>();   
            MockCityPossessionCanon     = new Mock<IPossessionRelationship<ICivilization, ICity>>();
            MockCellResourceLogic       = new Mock<ICellResourceLogic>();
            MockBuildingResourceLogic   = new Mock<IBuildingResourceLogic>();
            MockUnemploymentLogic       = new Mock<IUnemploymentLogic>();

            Container.Bind<ICityConfig>                                  ().FromInstance(MockConfig                 .Object);
            Container.Bind<IPossessionRelationship<ICity, IHexCell>>     ().FromInstance(MockCellPossessionCanon    .Object);
            Container.Bind<IPossessionRelationship<ICity, IBuilding>>    ().FromInstance(MockBuildingPossessionCanon.Object);
            Container.Bind<IIncomeModifierLogic>                         ().FromInstance(MockIncomeModifierLogic    .Object);
            Container.Bind<IPossessionRelationship<ICivilization, ICity>>().FromInstance(MockCityPossessionCanon    .Object);
            Container.Bind<ICellResourceLogic>                           ().FromInstance(MockCellResourceLogic      .Object);
            Container.Bind<IBuildingResourceLogic>                       ().FromInstance(MockBuildingResourceLogic  .Object);
            Container.Bind<IUnemploymentLogic>                           ().FromInstance(MockUnemploymentLogic      .Object);

            Container.Bind<ResourceGenerationLogic>().AsSingle();
        }

        private void SetupConfig(CityConfigTestData configData) {
            MockConfig.Setup(config => config.UnemployedYield).Returns(configData.UnemployedYield);
            MockConfig.Setup(config => config.LocationYield  ).Returns(configData.LocationYield);
        }

        #endregion

        #region test

        [TestCaseSource("GetYieldOfCellForCityTestCases")]
        [Test(Description = "")]
        public ResourceSummary GetYieldOfCellForCityTests(CellTestData cellData, CityTestData cityData) {
            var cell = BuildCell(cellData);
            var city = BuildCity(cityData);

            var resourceLogic = Container.Resolve<ResourceGenerationLogic>();

            return resourceLogic.GetYieldOfCellForCity(cell, city);
        }

        [TestCaseSource("GetYieldOfUnemployedPersonForCityTestCases")]
        [Test(Description = "")]
        public ResourceSummary GetYieldOfUnemployedPersonForCityTests(CityTestData cityData, CityConfigTestData configData) {
            var city = BuildCity(cityData);

            SetupConfig(configData);

            var resourceLogic = Container.Resolve<ResourceGenerationLogic>();

            return resourceLogic.GetYieldOfUnemployedPersonForCity(city);
        }

        [TestCaseSource("GetYieldOfBuildingForCityTestCases")]
        [Test(Description = "")]
        public ResourceSummary GetYieldOfBuildingForCityTests(BuildingTestData buildingData, CityTestData cityData) {
            var building = BuildBuilding(buildingData);
            var city = BuildCity(cityData);

            var resourceLogic = Container.Resolve<ResourceGenerationLogic>();

            return resourceLogic.GetYieldOfBuildingForCity(building, city);
        }

        [TestCaseSource("GetYieldOfBuildingSlotsForCityTestCases")]
        [Test(Description = "")]
        public ResourceSummary GetYieldOfBuildingSlotsForCityTests(BuildingTestData testData, CityTestData cityData) {
            var building = BuildBuilding(testData);
            var city = BuildCity(cityData);

            var resourceLogic = Container.Resolve<ResourceGenerationLogic>();

            return resourceLogic.GetYieldOfBuildingSlotsForCity(building, city);
        }

        [TestCaseSource("GetBaseYieldOfCityTestCases")]
        [Test(Description = "")]
        public ResourceSummary GetBaseYieldOfCityTests(CityTestData cityData, CityConfigTestData configData) {
            var city = BuildCity(cityData);

            SetupConfig(configData);

            var resourceLogic = Container.Resolve<ResourceGenerationLogic>();

            return resourceLogic.GetBaseYieldOfCity(city);
        }

        [TestCaseSource("GetTotalYieldForCityTestCases")]
        [Test(Description = "")]
        public ResourceSummary GetTotalYieldForCityTests(CityTestData cityData, CityConfigTestData configData) {
            SetupConfig(configData);

            var city = BuildCity(cityData);

            var resourceLogic = Container.Resolve<ResourceGenerationLogic>();

            return resourceLogic.GetTotalYieldForCity(city);
        }

        #endregion

        #region utilities

        private ICity BuildCity(CityTestData cityData) {
            var mockCity = new Mock<ICity>();

            mockCity.Setup(city => city.Population).Returns(cityData.Population);

            var newCity = mockCity.Object;

            List<IHexCell> territory = cityData.Territory.Select(cellData => BuildCell(cellData)).ToList();

            MockCellPossessionCanon
                .Setup(canon => canon.GetPossessionsOfOwner(newCity))
                .Returns(territory);

            MockBuildingPossessionCanon
                .Setup(canon => canon.GetPossessionsOfOwner(newCity))
                .Returns(cityData.Buildings.Select(buildingData => BuildBuilding(buildingData)));

            var owningCiv = BuildCivilization(cityData.OwningCivilization);

            MockCityPossessionCanon.Setup(canon => canon.GetOwnerOfPossession(newCity)).Returns(owningCiv);

            MockIncomeModifierLogic
                .Setup(canon => canon.GetYieldMultipliersForCity(newCity))
                .Returns(cityData.CityYieldMultipliers);

            MockUnemploymentLogic
                .Setup(canon => canon.GetUnemployedPeopleInCity(newCity))
                .Returns(cityData.UnemployedPeople);

            return newCity;
        }

        private IHexCell BuildCell(CellTestData cellData) {
            var mockCell = new Mock<IHexCell>();

            mockCell.Setup(cell => cell.WorkerSlot  ).Returns(BuildWorkerSlot(cellData.Slot));
            mockCell.Setup(cell => cell.SuppressSlot).Returns(cellData.SuppressSlot);

            MockCellResourceLogic.Setup(logic => logic.GetYieldOfCell(mockCell.Object)).Returns(cellData.CellYield);

            return mockCell.Object;
        }

        private IBuilding BuildBuilding(BuildingTestData buildingData) {
            var mockBuilding = new Mock<IBuilding>();

            var mockTemplate = new Mock<IBuildingTemplate>();

            mockTemplate.Setup(template => template.SlotYield).Returns(buildingData.SlotYield);

            mockBuilding.Setup(building => building.Template).Returns(mockTemplate.Object);

            var newBuilding = mockBuilding.Object;

            MockBuildingResourceLogic.Setup(logic => logic.GetYieldOfBuilding(newBuilding)).Returns(buildingData.BuildingYield);

            return newBuilding;
        }

        private IWorkerSlot BuildWorkerSlot(SlotTestData slotData) {
            var mockSlot = new Mock<IWorkerSlot>();

            mockSlot.Setup(slot => slot.IsOccupied).Returns(slotData.IsOccupied);

            var newSlot = mockSlot.Object;

            return newSlot;
        }

        private ICivilization BuildCivilization(CivilizationTestData civData) {
            var newCivilization = new Mock<ICivilization>().Object;

            MockIncomeModifierLogic
                .Setup(logic => logic.GetYieldMultipliersForCivilization(newCivilization))
                .Returns(civData.YieldMultipliers);

            return newCivilization;
        }

        #endregion

        #endregion

    }

}
