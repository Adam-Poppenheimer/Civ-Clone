﻿using System;
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

            public YieldSummary CityYieldMultipliers = YieldSummary.Empty;

        }

        public class CellTestData {

            public YieldSummary CellYield = YieldSummary.Empty;

            public SlotTestData Slot = new SlotTestData() { };

            public bool SuppressSlot;

        }

        public class BuildingTestData {

            public YieldSummary BuildingYield;

            public YieldSummary SlotYield;

        }

        public class SlotTestData {

            public bool IsOccupied;

        }

        public class CivilizationTestData {

            public YieldSummary YieldMultipliers = YieldSummary.Empty;

        }

        public class CityConfigTestData {

            public YieldSummary UnemployedYield = YieldSummary.Empty;

            public YieldSummary CityCenterYield = YieldSummary.Empty;

        }

        #endregion

        #region static fields and properties

        public static IEnumerable GetYieldOfCellForCityTestCases {
            get {
                yield return new TestCaseData(
                    new CellTestData() {
                        CellYield = new YieldSummary(food: 1f, production: 2f, gold: 3f),
                        Slot = new SlotTestData() {
                            IsOccupied = true
                        }
                    },
                    new CityTestData() { }
                ).SetName("Occupied slot, no additional modifiers").Returns(new YieldSummary(
                    food: 1f, production: 2f, gold: 3f
                ));

                yield return new TestCaseData(
                    new CellTestData() {
                        CellYield = new YieldSummary(food: 1f, production: 2f, gold: 3f),
                        Slot = new SlotTestData() { IsOccupied = false }
                    },
                    new CityTestData() { }
                ).SetName("Unoccupied slot, no additional modifiers").Returns(new YieldSummary(
                    food: 1f, production: 2f, gold: 3f
                ));

                yield return new TestCaseData(
                    new CellTestData() {
                        CellYield = new YieldSummary(food: 1f, production: 2f, gold: 3f),
                        Slot = new SlotTestData() { IsOccupied = true }
                    },
                    new CityTestData() {
                        CityYieldMultipliers = new YieldSummary(food: 0f, production: -0.5f, gold: 2f, culture: 3f),
                    }                    
                ).SetName("Occupied slot, city has yield modifiers").Returns(new YieldSummary(
                    food: 1f, production: 1f, gold: 9f
                ));

                yield return new TestCaseData(
                    new CellTestData() {
                        CellYield = new YieldSummary(food: 1f, production: 2f, gold: 3f),
                        Slot = new SlotTestData() { IsOccupied = true }
                    },
                    new CityTestData() {
                        OwningCivilization   = new CivilizationTestData() {
                            YieldMultipliers = new YieldSummary(food: 0f, production: -0.5f, gold: 2f, culture: 3f)
                        },
                    }                    
                ).SetName("Occupied slot, civilization has yield modifiers").Returns(new YieldSummary(
                    food: 1f, production: 1f, gold: 9f
                ));

                yield return new TestCaseData(
                    new CellTestData() {
                        CellYield = new YieldSummary(food: 1f, production: 1f, gold: 1f, culture: 1f),
                        Slot = new SlotTestData() { IsOccupied = true }
                    },
                    new CityTestData() {
                        OwningCivilization   = new CivilizationTestData() {
                            YieldMultipliers = new YieldSummary(food: 2f, gold: 1f)
                        },
                        CityYieldMultipliers = new YieldSummary(food: 1f, production: -1f),
                    }
                ).SetName("Occupied slot, city and civilization modifiers in play").Returns(new YieldSummary(
                    food: 4f, production: 0f, gold: 2f, culture: 1f
                ));
            }
        }

        public static IEnumerable GetYieldOfUnemployedPersonForCityTestCases {
            get {
                yield return new TestCaseData(
                    new CityTestData() { },
                    new CityConfigTestData() {
                        UnemployedYield = new YieldSummary(food: 1f, production: 1f)
                    }
                ).SetName("No modifiers in play returns configured base").Returns(
                    new YieldSummary(food: 1f, production: 1f)
                );

                yield return new TestCaseData(
                    new CityTestData() {
                        CityYieldMultipliers = new YieldSummary(food: 2f, production: 3f, gold: -4f),
                    },
                    new CityConfigTestData() {
                        UnemployedYield = new YieldSummary(food: 1f, production: 1f)
                    }
                ).SetName("city modifiers in play").Returns(
                    new YieldSummary(food: 3f, production: 4f)
                );

                yield return new TestCaseData(
                    new CityTestData() {
                        OwningCivilization = new CivilizationTestData() {
                            YieldMultipliers = new YieldSummary(food: 2f, production: 3f, gold: -4f)
                        }
                    },
                    new CityConfigTestData() {
                        UnemployedYield = new YieldSummary(food: 1f, production: 1f)
                    }
                ).SetName("Civilization modifiers in play").Returns(
                    new YieldSummary(food: 3f, production: 4f)
                );

                yield return new TestCaseData(
                    new CityTestData() {
                        OwningCivilization = new CivilizationTestData() {
                            YieldMultipliers = new YieldSummary(food: 2f, production: 3f, gold: -4f)
                        },
                        CityYieldMultipliers = new YieldSummary(food: -1f, production: 0f, culture: 0.5f)
                    },
                    new CityConfigTestData() {
                        UnemployedYield = new YieldSummary(food: 1f, production: 1f, gold: 1f, culture: 1f)
                    }
                ).SetName("City and civilization modifiers in play").Returns(
                    new YieldSummary(food: 2f, production: 4f, gold: -3f, culture: 1.5f)
                );
            }
        }

        public static IEnumerable GetYieldOfBuildingForCityTestCases {
            get {
                yield return new TestCaseData(
                    new BuildingTestData() {
                        BuildingYield = new YieldSummary(food: 4f, production: 2f, gold: 1f, culture: 6f),
                    },
                    new CityTestData() { }
                ).SetName("Building with yield, no additional modifiers").Returns(
                    new YieldSummary(food: 4f, production: 2f, gold: 1f, culture: 6f)
                );

                yield return new TestCaseData(
                    new BuildingTestData() {
                        BuildingYield = new YieldSummary(food: 4f, production: 2f, gold: 1f, culture: 6f),
                    },
                    new CityTestData() {
                        CityYieldMultipliers = new YieldSummary(production: 2f, gold: 0.5f, culture: -1f)
                    }
                ).SetName("Building with yield, city modifiers present").Returns(
                    new YieldSummary(food: 4f, production: 6f, gold: 1f * 1.5f, culture: 0f)
                );

                yield return new TestCaseData(
                    new BuildingTestData() {
                        BuildingYield = new YieldSummary(food: 4f, production: 2f, gold: 1f, culture: 6f),
                    },
                    new CityTestData() {
                        OwningCivilization = new CivilizationTestData() {
                            YieldMultipliers = new YieldSummary(production: 2f, gold: 0.5f, culture: -1f)
                        }
                    }
                ).SetName("Building with yield, civilization modifiers present").Returns(
                    new YieldSummary(food: 4f, production: 6f, gold: 1f * 1.5f, culture: 0f)
                );

                yield return new TestCaseData(
                    new BuildingTestData() {
                        BuildingYield = new YieldSummary(food: 1f, production: 1f, gold: 1f, culture: 1f, science: 1f),
                    },
                    new CityTestData() {
                        CityYieldMultipliers = new YieldSummary(food: 1f, production: -1f, gold: 2f),
                        OwningCivilization = new CivilizationTestData() {
                            YieldMultipliers = new YieldSummary(food: 0.5f, production: -1f, gold: 0.5f, culture: -1f)
                        }
                    }
                ).SetName("Building with yield, city and civilization modifiers present").Returns(
                    new YieldSummary(food: 2.5f, production: -1f, gold: 3.5f, culture: 0f, science: 1f)
                );
            }
        }

        public static IEnumerable GetYieldOfBuildingSlotsForCityTestCases {
            get {
                yield return new TestCaseData(
                    new BuildingTestData() {
                        SlotYield = new YieldSummary(food: 4f, production: 2f, gold: 1f, culture: 6f),
                    },
                    new CityTestData() { }
                ).SetName("Building with slot yields, no additional modifiers").Returns(
                    new YieldSummary(food: 4f, production: 2f, gold: 1f, culture: 6f)
                );

                yield return new TestCaseData(
                    new BuildingTestData() {
                        SlotYield = new YieldSummary(food: 4f, production: 2f, gold: 1f, culture: 6f),
                    },
                    new CityTestData() {
                        CityYieldMultipliers = new YieldSummary(production: 2f, gold: 0.5f, culture: -1f)
                    }
                ).SetName("Building with slot yields, city modifiers present").Returns(
                    new YieldSummary(food: 4f, production: 6f, gold: 1f * 1.5f, culture: 0f)
                );

                yield return new TestCaseData(
                    new BuildingTestData() {
                        SlotYield = new YieldSummary(food: 4f, production: 2f, gold: 1f, culture: 6f),
                    },
                    new CityTestData() {
                        OwningCivilization = new CivilizationTestData() {
                            YieldMultipliers = new YieldSummary(production: 2f, gold: 0.5f, culture: -1f)
                        }
                    }
                ).SetName("Building with slot yields, civilization modifiers present").Returns(
                    new YieldSummary(food: 4f, production: 6f, gold: 1f * 1.5f, culture: 0f)
                );

                yield return new TestCaseData(
                    new BuildingTestData() {
                        SlotYield = new YieldSummary(food: 1f, production: 1f, gold: 1f, culture: 1f, science: 1f),
                    },
                    new CityTestData() {
                        CityYieldMultipliers = new YieldSummary(food: 1f, production: -1f, gold: 2f),
                        OwningCivilization = new CivilizationTestData() {
                            YieldMultipliers = new YieldSummary(food: 0.5f, production: -1f, gold: 0.5f, culture: -1f)
                        }
                    }
                ).SetName("Building with slot yields, city and civilization modifiers present").Returns(
                    new YieldSummary(food: 2.5f, production: -1f, gold: 3.5f, culture: 0f, science: 1f)
                );
            }
        }

        public static IEnumerable GetTotalYieldForCityTestCases {
            get {
                yield return new TestCaseData(
                    new CityTestData() {
                        Population = 0,
                        Territory = new List<CellTestData>() {
                            new CellTestData() {
                                CellYield = new YieldSummary(food: 1),
                                Slot = new SlotTestData() { IsOccupied = true }
                            },
                            new CellTestData() {
                                CellYield = new YieldSummary(production: 2),
                                Slot = new SlotTestData() { IsOccupied = false }
                            },
                            new CellTestData() {
                                CellYield = new YieldSummary(gold: 3),
                                Slot = new SlotTestData() { IsOccupied = true }
                            },
                            new CellTestData() {
                                CellYield = new YieldSummary(culture: 4),
                                Slot = new SlotTestData() { IsOccupied = false }
                            },
                            new CellTestData() {
                                CellYield = new YieldSummary(science: 5),
                                Slot = new SlotTestData() { IsOccupied = true }
                            }
                        }
                    },
                    new CityConfigTestData() {
                        UnemployedYield = new YieldSummary()
                    }
                ).SetName("Accounts for yield of all occupied territory").Returns(new YieldSummary(
                    food: 1f, gold: 3f, science: 5f
                ));

                yield return new TestCaseData(
                    new CityTestData() {
                        Population = 0,
                        Territory = new List<CellTestData>() {
                            new CellTestData() {
                                CellYield = new YieldSummary(food: 1),
                                Slot = new SlotTestData() { IsOccupied = true }
                            },
                            new CellTestData() {
                                CellYield = new YieldSummary(production: 2),
                                SuppressSlot = true,
                                Slot = new SlotTestData() { IsOccupied = false }
                            },
                            new CellTestData() {
                                CellYield = new YieldSummary(gold: 3),
                                SuppressSlot = true,
                                Slot = new SlotTestData() { IsOccupied = true }
                            },
                            new CellTestData() {
                                CellYield = new YieldSummary(culture: 4),
                                Slot = new SlotTestData() { IsOccupied = false }
                            },
                            new CellTestData() {
                                CellYield = new YieldSummary(science: 5),
                                Slot = new SlotTestData() { IsOccupied = true }
                            }
                        }
                    },
                    new CityConfigTestData() {
                        UnemployedYield = new YieldSummary()
                    }
                ).SetName("Ignores suppressed slots").Returns(new YieldSummary(
                    food: 1f, science: 5f
                ));

                yield return new TestCaseData(
                    new CityTestData() {
                        Population = 0,
                        Buildings = new List<BuildingTestData>() {
                            new BuildingTestData() { BuildingYield = new YieldSummary(food:       1f) },
                            new BuildingTestData() { BuildingYield = new YieldSummary(production: 2f) },
                            new BuildingTestData() { BuildingYield = new YieldSummary(gold:       3f) },
                            new BuildingTestData() { BuildingYield = new YieldSummary(culture:    4f) },
                            new BuildingTestData() { BuildingYield = new YieldSummary(science:    5f) },
                        }
                    },
                    new CityConfigTestData() {
                        UnemployedYield = new YieldSummary()
                    }
                ).SetName("Accounts for yield of all buildings").Returns(new YieldSummary(
                    food: 1f, production: 2f, gold: 3f, culture: 4f, science: 5f
                ));

                yield return new TestCaseData(
                    new CityTestData() {
                        Population = 0,
                        UnemployedPeople = 3
                    },
                    new CityConfigTestData() {
                        UnemployedYield = new YieldSummary(production: 1f)
                    }
                ).SetName("Accounts for yield of unemployed people").Returns(new YieldSummary(
                    production: 3f
                ));

                yield return new TestCaseData(
                    new CityTestData() {
                        Population = 5
                    },
                    new CityConfigTestData() {
                        CityCenterYield = new YieldSummary(food: 2f, production: 1f, gold: 1f)
                    }
                ).SetName("Accounts for city center yield").Returns(new YieldSummary(
                    food: 2f, production: 1f, gold: 1f
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
        private Mock<ICellYieldLogic>                               MockCellResourceLogic;
        private Mock<IBuildingInherentYieldLogic>                   MockBuildingResourceLogic;
        private Mock<IUnemploymentLogic>                            MockUnemploymentLogic;
        private Mock<ICityCenterYieldLogic>                         MockCityCenterYieldLogic;

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
            MockCellResourceLogic       = new Mock<ICellYieldLogic>();
            MockBuildingResourceLogic   = new Mock<IBuildingInherentYieldLogic>();
            MockUnemploymentLogic       = new Mock<IUnemploymentLogic>();
            MockCityCenterYieldLogic    = new Mock<ICityCenterYieldLogic>();

            Container.Bind<ICityConfig>                                  ().FromInstance(MockConfig                 .Object);
            Container.Bind<IPossessionRelationship<ICity, IHexCell>>     ().FromInstance(MockCellPossessionCanon    .Object);
            Container.Bind<IPossessionRelationship<ICity, IBuilding>>    ().FromInstance(MockBuildingPossessionCanon.Object);
            Container.Bind<IIncomeModifierLogic>                         ().FromInstance(MockIncomeModifierLogic    .Object);
            Container.Bind<IPossessionRelationship<ICivilization, ICity>>().FromInstance(MockCityPossessionCanon    .Object);
            Container.Bind<ICellYieldLogic>                              ().FromInstance(MockCellResourceLogic      .Object);
            Container.Bind<IBuildingInherentYieldLogic>                  ().FromInstance(MockBuildingResourceLogic  .Object);
            Container.Bind<IUnemploymentLogic>                           ().FromInstance(MockUnemploymentLogic      .Object);
            Container.Bind<ICityCenterYieldLogic>                        ().FromInstance(MockCityCenterYieldLogic   .Object);

            Container.Bind<YieldGenerationLogic>().AsSingle();
        }

        private void SetupConfig(CityConfigTestData configData) {
            MockConfig.Setup(config => config.UnemployedYield).Returns(configData.UnemployedYield);

            MockCityCenterYieldLogic.Setup(logic => logic.GetYieldOfCityCenter(It.IsAny<ICity>()))
                                    .Returns(configData.CityCenterYield);
        }

        #endregion

        #region test

        [TestCaseSource("GetYieldOfCellForCityTestCases")]
        [Test(Description = "")]
        public YieldSummary GetYieldOfCellForCityTests(CellTestData cellData, CityTestData cityData) {
            var cell = BuildCell(cellData);
            var city = BuildCity(cityData);

            var resourceLogic = Container.Resolve<YieldGenerationLogic>();

            return resourceLogic.GetYieldOfCellForCity(cell, city);
        }

        [TestCaseSource("GetYieldOfUnemployedPersonForCityTestCases")]
        [Test(Description = "")]
        public YieldSummary GetYieldOfUnemployedPersonForCityTests(CityTestData cityData, CityConfigTestData configData) {
            var city = BuildCity(cityData);

            SetupConfig(configData);

            var resourceLogic = Container.Resolve<YieldGenerationLogic>();

            return resourceLogic.GetYieldOfUnemployedPersonForCity(city);
        }

        [TestCaseSource("GetYieldOfBuildingForCityTestCases")]
        [Test(Description = "")]
        public YieldSummary GetYieldOfBuildingForCityTests(BuildingTestData buildingData, CityTestData cityData) {
            var building = BuildBuilding(buildingData);
            var city = BuildCity(cityData);

            var resourceLogic = Container.Resolve<YieldGenerationLogic>();

            return resourceLogic.GetYieldOfBuildingForCity(building, city);
        }

        [TestCaseSource("GetYieldOfBuildingSlotsForCityTestCases")]
        [Test(Description = "")]
        public YieldSummary GetYieldOfBuildingSlotsForCityTests(BuildingTestData testData, CityTestData cityData) {
            var building = BuildBuilding(testData);
            var city = BuildCity(cityData);

            var resourceLogic = Container.Resolve<YieldGenerationLogic>();

            return resourceLogic.GetYieldOfBuildingSlotsForCity(building, city);
        }

        [TestCaseSource("GetTotalYieldForCityTestCases")]
        [Test(Description = "")]
        public YieldSummary GetTotalYieldForCityTests(CityTestData cityData, CityConfigTestData configData) {
            SetupConfig(configData);

            var city = BuildCity(cityData);

            var resourceLogic = Container.Resolve<YieldGenerationLogic>();

            return resourceLogic.GetTotalYieldForCity(city);
        }

        [Test]
        public void GetTotalYieldForCityTests_ResultMultipliedByOnePlusAdditionalBonuses() {
            SetupConfig(new CityConfigTestData() {
                CityCenterYield = YieldSummary.Empty,
                UnemployedYield = new YieldSummary(food: 1, production: 1)
            });

            var city = BuildCity(new CityTestData() {
                Population = 2, UnemployedPeople = 2
            });

            var resourceLogic = Container.Resolve<YieldGenerationLogic>();
            
            Assert.AreEqual(
                new YieldSummary(food: 4, production: 10),
                resourceLogic.GetTotalYieldForCity(city, new YieldSummary(food: 1, production: 4, science: -1))
            );
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

            MockCellResourceLogic.Setup(logic => logic.GetYieldOfCell(mockCell.Object, It.IsAny<ICivilization>()))
                                 .Returns(cellData.CellYield);

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
