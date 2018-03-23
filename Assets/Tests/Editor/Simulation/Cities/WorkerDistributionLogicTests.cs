using System;
using System.Collections;
using System.Linq;
using System.Collections.Generic;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation;

using Assets.Simulation.HexMap;

using Assets.Simulation.Cities;
using Assets.Simulation.Cities.ResourceGeneration;
using Assets.Simulation.Cities.Growth;
using Assets.Simulation.Cities.Distribution;
using Assets.Simulation.Cities.Buildings;
using Assets.Simulation.WorkerSlots;

namespace Assets.Tests.Simulation.Cities {

    [TestFixture]
    public class WorkerDistributionLogicTests : ZenjectUnitTestFixture {

        #region internal types

        public class WorkerDistributionTestData {

            public int WorkersToDistribute;

            public CityTestData City;

            public ResourceFocusType ResourceFocus;

        }

        public class GetSlotsAvailableToCityTestData {

        }

        public class CityTestData {

            public int Population;

            public int FoodConsumptionPerTurn;

            public List<HexCellTestData> Territory = new List<HexCellTestData>();

            public List<BuildingTestData> Buildings = new List<BuildingTestData>();

        }

        public class HexCellTestData {

            public bool SuppressSlot;

            public SlotTestData Slot;

            public ResourceSummary Yield;

        }

        public class SlotTestData {

            public bool IsOccupied;
            
            public bool IsLocked;

            public bool ExpectsOccupation;

        }

        public class BuildingTestData {

            public List<SlotTestData> Slots;

            public ResourceSummary SlotYield;

        }

        #endregion

        #region static fields and properties

        public static IEnumerable DistributeWorkersIntoSlotsTestCases {
            get {
                yield return new TestCaseData(new WorkerDistributionTestData() {
                    WorkersToDistribute = 3,
                    City = new CityTestData() {
                        Population = 3,
                        FoodConsumptionPerTurn = 0,
                        Territory = new List<HexCellTestData>() {
                            new HexCellTestData() {
                                Slot = new SlotTestData() { IsOccupied = true, ExpectsOccupation = false },
                                Yield = new ResourceSummary(food: 1)
                            },
                            new HexCellTestData() {
                                Slot = new SlotTestData() { IsOccupied = true, ExpectsOccupation = false },
                                Yield = new ResourceSummary(food: 2)
                            },
                            new HexCellTestData() {
                                Slot = new SlotTestData() { IsOccupied = true, ExpectsOccupation = true },
                                Yield = new ResourceSummary(food: 3)
                            },
                            new HexCellTestData() {
                                Slot = new SlotTestData() { IsOccupied = false, ExpectsOccupation = true },
                                Yield = new ResourceSummary(food: 4)
                            },
                            new HexCellTestData() {
                                Slot = new SlotTestData() { IsOccupied = false, ExpectsOccupation = true },
                                Yield = new ResourceSummary(food: 5)
                            }
                        },
                        Buildings = new List<BuildingTestData>() {

                        }
                    },
                    ResourceFocus = ResourceFocusType.TotalYield
                }).SetName("Total yield focus maximizes total yield");

                yield return new TestCaseData(new WorkerDistributionTestData() {
                    WorkersToDistribute = 3,
                    City = new CityTestData() {
                        Population = 3,
                        FoodConsumptionPerTurn = 0,
                        Territory = new List<HexCellTestData>() {
                            new HexCellTestData() {
                                Slot = new SlotTestData() { IsOccupied = true, ExpectsOccupation = false },
                                Yield = new ResourceSummary(gold: 1)
                            },
                            new HexCellTestData() {
                                Slot = new SlotTestData() { IsOccupied = true, ExpectsOccupation = false },
                                Yield = new ResourceSummary(food: 2)
                            },
                            new HexCellTestData() {
                                Slot = new SlotTestData() { IsOccupied = true, ExpectsOccupation = true },
                                Yield = new ResourceSummary(production: 1, culture: 1, gold: 1)
                            },
                            new HexCellTestData() {
                                Slot = new SlotTestData() { IsOccupied = false, ExpectsOccupation = true },
                                Yield = new ResourceSummary(food: 1, production: 2, science: 1)
                            },
                            new HexCellTestData() {
                                Slot = new SlotTestData() { IsOccupied = false, ExpectsOccupation = true },
                                Yield = new ResourceSummary(culture: 3, science: 2)
                            }
                        },
                        Buildings = new List<BuildingTestData>() {

                        }
                    },
                    ResourceFocus = ResourceFocusType.TotalYield
                }).SetName("Total yield handles mixed yields well");

                yield return new TestCaseData(new WorkerDistributionTestData() {
                    WorkersToDistribute = 3,
                    City = new CityTestData() {
                        Population = 3,
                        FoodConsumptionPerTurn = 0,
                        Territory = new List<HexCellTestData>() {
                            new HexCellTestData() {
                                Slot = new SlotTestData() { IsOccupied = true, ExpectsOccupation = true },
                                Yield = new ResourceSummary(food: 1, culture: 1)
                            },
                            new HexCellTestData() {
                                Slot = new SlotTestData() { IsOccupied = true, ExpectsOccupation = true },
                                Yield = new ResourceSummary(food: 1, culture: 2)
                            },
                            new HexCellTestData() {
                                Slot = new SlotTestData() { IsOccupied = true, ExpectsOccupation = false },
                                Yield = new ResourceSummary(food: 10,  production: 4)
                            },
                            new HexCellTestData() {
                                Slot = new SlotTestData() { IsOccupied = false, ExpectsOccupation = true },
                                Yield = new ResourceSummary(food: 0, culture: 1)
                            },
                            new HexCellTestData() {
                                Slot = new SlotTestData() { IsOccupied = false, ExpectsOccupation = false },
                                Yield = new ResourceSummary(food: 10, gold: 5)
                            }
                        },
                        Buildings = new List<BuildingTestData>() {

                        }
                    },
                    ResourceFocus = ResourceFocusType.Culture
                }).SetName("Specific yield focus maximizes that yield");

                yield return new TestCaseData(new WorkerDistributionTestData() {
                    WorkersToDistribute = 3,
                    City = new CityTestData() {
                        Population = 3,
                        FoodConsumptionPerTurn = 0,
                        Territory = new List<HexCellTestData>() {
                            new HexCellTestData() {
                                Slot = new SlotTestData() { IsOccupied = true, ExpectsOccupation = false },
                                Yield = new ResourceSummary(food: 1, culture: 1)
                            },
                            new HexCellTestData() {
                                Slot = new SlotTestData() { IsOccupied = true, ExpectsOccupation = true },
                                Yield = new ResourceSummary(food: 1, production: 3)
                            },
                            new HexCellTestData() {
                                Slot = new SlotTestData() { IsOccupied = true, ExpectsOccupation = false },
                                Yield = new ResourceSummary(food: 1,  science: 2)
                            },
                            new HexCellTestData() {
                                Slot = new SlotTestData() { IsOccupied = false, ExpectsOccupation = true },
                                Yield = new ResourceSummary(food: 1, gold: 4)
                            },
                            new HexCellTestData() {
                                Slot = new SlotTestData() { IsOccupied = false, ExpectsOccupation = true },
                                Yield = new ResourceSummary(food: 1, culture: 2, science: 3)
                            }
                        },
                        Buildings = new List<BuildingTestData>() {

                        }
                    },
                    ResourceFocus = ResourceFocusType.Food
                }).SetName("Specific yield tie-breaks with total yield");

                yield return new TestCaseData(new WorkerDistributionTestData() {
                    WorkersToDistribute = 3,
                    City = new CityTestData() {
                        Population = 3,
                        FoodConsumptionPerTurn = 0,
                        Territory = new List<HexCellTestData>() {
                            new HexCellTestData() {
                                Slot = new SlotTestData() { IsOccupied = true, ExpectsOccupation = false },
                                Yield = new ResourceSummary(food: 1)
                            },
                            new HexCellTestData() {
                                Slot = new SlotTestData() { IsOccupied = true, ExpectsOccupation = false },
                                Yield = new ResourceSummary(food: 2)
                            },
                            new HexCellTestData() {
                                Slot = new SlotTestData() { IsOccupied = true, ExpectsOccupation = false },
                                Yield = new ResourceSummary(food: 3)
                            },
                            new HexCellTestData() {
                                Slot = new SlotTestData() { IsOccupied = false, ExpectsOccupation = false },
                                Yield = new ResourceSummary(food: 4)
                            },
                            new HexCellTestData() {
                                Slot = new SlotTestData() { IsOccupied = false, ExpectsOccupation = true },
                                Yield = new ResourceSummary(food: 5)
                            }
                        },
                        Buildings = new List<BuildingTestData>() {
                            new BuildingTestData() {
                                Slots = new List<SlotTestData>() {
                                    new SlotTestData() { IsOccupied = false, ExpectsOccupation = true },
                                    new SlotTestData() { IsOccupied = false, ExpectsOccupation = true },
                                },
                                SlotYield = new ResourceSummary(food: 6)
                            }
                        }
                    },
                    ResourceFocus = ResourceFocusType.Food
                }).SetName("Distribution considers slots in buildings");

                yield return new TestCaseData(new WorkerDistributionTestData() {
                    WorkersToDistribute = 5,
                    City = new CityTestData() {
                        Population = 5,
                        FoodConsumptionPerTurn = 10,
                        Territory = new List<HexCellTestData>() {
                            new HexCellTestData() {
                                Slot = new SlotTestData() { IsOccupied = false, ExpectsOccupation = false },
                                Yield = new ResourceSummary(food: 2)
                            },
                            new HexCellTestData() {
                                Slot = new SlotTestData() { IsOccupied = false, ExpectsOccupation = true },
                                Yield = new ResourceSummary(food: 3)
                            },
                            new HexCellTestData() {
                                Slot = new SlotTestData() { IsOccupied = false, ExpectsOccupation = true },
                                Yield = new ResourceSummary(food: 3)
                            },
                            new HexCellTestData() {
                                Slot = new SlotTestData() { IsOccupied = true, ExpectsOccupation = true },
                                Yield = new ResourceSummary(food: 4)
                            },
                            new HexCellTestData() {
                                Slot = new SlotTestData() { IsOccupied = true, ExpectsOccupation = false },
                                Yield = new ResourceSummary(production: 21)
                            },
                            new HexCellTestData() {
                                Slot = new SlotTestData() { IsOccupied = true, ExpectsOccupation = false },
                                Yield = new ResourceSummary(production: 22)
                            },
                            new HexCellTestData() {
                                Slot = new SlotTestData() { IsOccupied = true, ExpectsOccupation = true },
                                Yield = new ResourceSummary(production: 23)
                            },
                            new HexCellTestData() {
                                Slot = new SlotTestData() { IsOccupied = true, ExpectsOccupation = true },
                                Yield = new ResourceSummary(production: 24)
                            }
                        },
                        Buildings = new List<BuildingTestData>() {

                        }
                    },
                    ResourceFocus = ResourceFocusType.Production
                }).SetName("Non-food distributions try to prevent starvation");

                yield return new TestCaseData(new WorkerDistributionTestData() {
                    WorkersToDistribute = 3,
                    City = new CityTestData() {
                        Population = 3,
                        FoodConsumptionPerTurn = 0,
                        Territory = new List<HexCellTestData>() {
                            new HexCellTestData() {
                                Slot = new SlotTestData() { IsOccupied = true, ExpectsOccupation = true },
                                Yield = new ResourceSummary(food: 1)
                            },
                        },
                        Buildings = new List<BuildingTestData>() {

                        }
                    },
                    ResourceFocus = ResourceFocusType.TotalYield
                }).SetName("Handles insufficient slots gracefully");

                yield return new TestCaseData(new WorkerDistributionTestData() {
                    WorkersToDistribute = 3,
                    City = new CityTestData() {
                        Population = 3,
                        FoodConsumptionPerTurn = 0,
                        Territory = new List<HexCellTestData>() {
                            new HexCellTestData() {
                                Slot = new SlotTestData() { IsOccupied = true, ExpectsOccupation = true },
                                Yield = new ResourceSummary(food: 1)
                            },
                            new HexCellTestData() {
                                Slot = new SlotTestData() { IsOccupied = true, ExpectsOccupation = true },
                                Yield = new ResourceSummary(food: 2)
                            },
                            new HexCellTestData() {
                                Slot = new SlotTestData() { IsOccupied = true, ExpectsOccupation = true },
                                Yield = new ResourceSummary(food: 3)
                            },
                            new HexCellTestData() {
                                Slot = new SlotTestData() { IsOccupied = false, ExpectsOccupation = false },
                                Yield = new ResourceSummary(food: 4),
                                SuppressSlot = true
                            },
                            new HexCellTestData() {
                                Slot = new SlotTestData() { IsOccupied = false, ExpectsOccupation = false },
                                Yield = new ResourceSummary(food: 5),
                                SuppressSlot = true
                            }
                        },
                        Buildings = new List<BuildingTestData>() {

                        }
                    },
                    ResourceFocus = ResourceFocusType.TotalYield
                }).SetName("Ignores suppressed slots");
            }
        }

        public static IEnumerable GetSlotsAvailableToCityTestCases {
            get {
                yield return new TestCaseData(new WorkerDistributionTestData() {
                    City = new CityTestData() {
                        Population = 10,
                        Territory = new List<HexCellTestData>() {
                            new HexCellTestData() { Slot = new SlotTestData() { IsOccupied = true  } },
                            new HexCellTestData() { Slot = new SlotTestData() { IsOccupied = true  } },
                            new HexCellTestData() { Slot = new SlotTestData() { IsOccupied = true  } },
                            new HexCellTestData() { Slot = new SlotTestData() { IsOccupied = true  } },
                            new HexCellTestData() { Slot = new SlotTestData() { IsOccupied = true  } },
                        }
                    }
                }).SetName("Finds all territory slots").Returns(5);

                yield return new TestCaseData(new WorkerDistributionTestData() {
                    City = new CityTestData() {
                        Population = 10,
                        Buildings = new List<BuildingTestData>() {
                            new BuildingTestData() {
                                Slots = new List<SlotTestData>() {
                                    new SlotTestData() { IsOccupied = true },
                                    new SlotTestData() { IsOccupied = true },
                                }
                            },
                            new BuildingTestData() {
                                Slots = new List<SlotTestData>() {
                                    new SlotTestData() { IsOccupied = false },
                                    new SlotTestData() { IsOccupied = true },
                                }
                            },
                            new BuildingTestData() {
                                Slots = new List<SlotTestData>() {
                                    new SlotTestData() { IsOccupied = true },
                                    new SlotTestData() { IsOccupied = false },
                                }
                            },
                            new BuildingTestData() {
                                Slots = new List<SlotTestData>() {
                                    new SlotTestData() { IsOccupied = false },
                                    new SlotTestData() { IsOccupied = false },
                                }
                            }
                        }
                    }
                }).SetName("Finds all building slots").Returns(8);

                yield return new TestCaseData(new WorkerDistributionTestData() {
                    City = new CityTestData() {
                        Population = 10,
                        Territory = new List<HexCellTestData>() {
                            new HexCellTestData() { Slot = new SlotTestData() { IsOccupied = true  }, SuppressSlot = true },
                            new HexCellTestData() { Slot = new SlotTestData() { IsOccupied = true  }, SuppressSlot = true },
                            new HexCellTestData() { Slot = new SlotTestData() { IsOccupied = true  }, SuppressSlot = false },
                            new HexCellTestData() { Slot = new SlotTestData() { IsOccupied = true  }, SuppressSlot = false },
                            new HexCellTestData() { Slot = new SlotTestData() { IsOccupied = true  }, SuppressSlot = true },
                        }
                    }
                }).SetName("Ignores suppressed cells").Returns(2);
            }
        }

        #endregion

        #region instance fields and properties

        private Mock<IPopulationGrowthLogic>                    MockGrowthLogic;
        private Mock<IResourceGenerationLogic>                  MockResourceGenerationLogic;
        private Mock<IPossessionRelationship<ICity, IBuilding>> MockBuildingPossessionCanon;
        private Mock<IPossessionRelationship<ICity, IHexCell>>  MockCellPossessionCanon;

        private Dictionary<IWorkerSlot, bool> OccupationExpectations = new Dictionary<IWorkerSlot, bool>();

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            OccupationExpectations.Clear();

            MockGrowthLogic             = new Mock<IPopulationGrowthLogic>();
            MockResourceGenerationLogic = new Mock<IResourceGenerationLogic>();
            MockCellPossessionCanon     = new Mock<IPossessionRelationship<ICity, IHexCell>>();
            MockBuildingPossessionCanon = new Mock<IPossessionRelationship<ICity, IBuilding>>();

            MockResourceGenerationLogic
                .Setup(logic => logic.GetTotalYieldForCity(It.IsAny<ICity>()))
                .Returns<ICity>(MockTotalYieldForCity);

            Container.Bind<IPopulationGrowthLogic>()                   .FromInstance(MockGrowthLogic.Object);
            Container.Bind<IResourceGenerationLogic>()                 .FromInstance(MockResourceGenerationLogic.Object);
            Container.Bind<IPossessionRelationship<ICity, IHexCell>>() .FromInstance(MockCellPossessionCanon.Object);
            Container.Bind<IPossessionRelationship<ICity, IBuilding>>().FromInstance(MockBuildingPossessionCanon.Object);

            Container.Bind<WorkerDistributionLogic>().AsSingle();
        }

        #endregion

        #region tests

        [TestCaseSource("DistributeWorkersIntoSlotsTestCases")]
        [Test(Description = "")]
        public void DistributeWorkersIntoSlotsTests(WorkerDistributionTestData testData) {
            var city = BuildCity(testData.City);

            var distributionLogic = Container.Resolve<WorkerDistributionLogic>();

            var availableSlots = distributionLogic.GetSlotsAvailableToCity(city);

            distributionLogic.DistributeWorkersIntoSlots(
                testData.WorkersToDistribute, availableSlots, city, testData.ResourceFocus
            );

            foreach(var slot in OccupationExpectations.Keys) {
                Assert.AreEqual(
                    OccupationExpectations[slot], slot.IsOccupied,
                    string.Format("IsOccupied property of slot {0} has an unexpected value.", slot)
                );
            }
        }

        [TestCaseSource("GetSlotsAvailableToCityTestCases")]
        [Test(Description = "")]
        public int GetSlotsAvailableToCityTests(WorkerDistributionTestData testData) {
            var city = BuildCity(testData.City);

            var distributionLogic = Container.Resolve<WorkerDistributionLogic>();

            return distributionLogic.GetSlotsAvailableToCity(city).Count();
        }

        #endregion

        #region utilities

        private ICity BuildCity(CityTestData cityData) {
            var mockCity = new Mock<ICity>();

            mockCity.Setup(city => city.Population).Returns(cityData.Population);

            var newCity = mockCity.Object;

            var territory = cityData.Territory.Select(cellData => BuildHexCell(cellData)).ToList();
            var buildings = cityData.Buildings.Select(buildingData => BuildBuilding(buildingData)).ToList();

            MockCellPossessionCanon.Setup(canon => canon.GetPossessionsOfOwner(newCity)).Returns(territory);

            MockBuildingPossessionCanon.Setup(canon => canon.GetPossessionsOfOwner(newCity)).Returns(buildings);

            MockGrowthLogic.Setup(logic => logic.GetFoodConsumptionPerTurn(newCity))
                .Returns(cityData.FoodConsumptionPerTurn);

            return newCity;
        }

        private IWorkerSlot BuildSlot(SlotTestData slotData, IHexCell parentCell, string name) {
            var mockSlot = BuildMockSlot(slotData, name);

            mockSlot.Setup(slot => slot.ParentCell).Returns(parentCell);

            return mockSlot.Object;
        }

        private IWorkerSlot BuildSlot(SlotTestData slotData, IBuilding parentBuilding, string name) {
            var mockSlot = BuildMockSlot(slotData, name);

            mockSlot.Setup(slot => slot.ParentBuilding).Returns(parentBuilding);

            return mockSlot.Object;
        }

        private Mock<IWorkerSlot> BuildMockSlot(SlotTestData slotData, string name) {
            var mockSlot = new Mock<IWorkerSlot>();

            mockSlot.Name = name;

            mockSlot.SetupAllProperties();

            mockSlot.Object.IsOccupied = slotData.IsOccupied;
            mockSlot.Object.IsLocked   = slotData.IsLocked;

            OccupationExpectations[mockSlot.Object] = slotData.ExpectsOccupation;

            return mockSlot;
        }

        private IHexCell BuildHexCell(HexCellTestData cellData) {
            var mockCell = new Mock<IHexCell>();

            var newCell = mockCell.Object;

            mockCell.Setup(cell => cell.SuppressSlot).Returns(cellData.SuppressSlot);

            mockCell.Setup(cell => cell.WorkerSlot  ).Returns(
                BuildSlot(cellData.Slot, newCell, "Slot " + cellData.Yield.ToString())
            );

            MockResourceGenerationLogic
                .Setup(logic => logic.GetYieldOfCellForCity(newCell, It.IsAny<ICity>()))
                .Returns(cellData.Yield);

            return newCell;
        }

        private IBuilding BuildBuilding(BuildingTestData buildingData) {
            var mockBuilding = new Mock<IBuilding>();

            var newBuilding = mockBuilding.Object;

            mockBuilding
                .Setup(building => building.Slots)
                .Returns(buildingData.Slots.Select(
                    slotData => BuildSlot(slotData, newBuilding, "Slot " + buildingData.SlotYield.ToString())
                ).ToList().AsReadOnly());

            MockResourceGenerationLogic
                .Setup(logic => logic.GetYieldOfBuildingSlotsForCity(newBuilding, It.IsAny<ICity>()))
                .Returns(buildingData.SlotYield);

            return newBuilding;
        }

        private ResourceSummary MockTotalYieldForCity(ICity city) {
            var retval = ResourceSummary.Empty;

            foreach(var cell in MockCellPossessionCanon.Object.GetPossessionsOfOwner(city)) {
                if(!cell.SuppressSlot && cell.WorkerSlot.IsOccupied) {
                    retval += MockResourceGenerationLogic.Object.GetYieldOfCellForCity(cell, city);
                }
            }

            foreach(var building in MockBuildingPossessionCanon.Object.GetPossessionsOfOwner(city)) {
                int occupiedSlots = building.Slots.Where(slot => slot.IsOccupied).Count();

                retval += MockResourceGenerationLogic.Object.GetYieldOfBuildingSlotsForCity(building, city) * occupiedSlots;
            }

            return retval;
        }

        #endregion

        #endregion

    }

}
