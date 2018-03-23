using System;
using System.Collections;
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
using Assets.Simulation.WorkerSlots;

namespace Assets.Tests.Simulation.Cities {

    [TestFixture]
    public class UnemploymentLogicTests : ZenjectUnitTestFixture {

        #region internal types

        public class CityTestData {

            public int Population;

            public List<HexCellTestData> Territory = new List<HexCellTestData>();

            public List<BuildingTestData> Buildings = new List<BuildingTestData>();

        }

        public class HexCellTestData {

            public bool SuppressSlot;

            public WorkerSlotTestData Slot = new WorkerSlotTestData();

        }

        public class BuildingTestData {

            public List<WorkerSlotTestData> Slots = new List<WorkerSlotTestData>();

        }

        public class WorkerSlotTestData {

            public bool IsOccupied;

        }

        #endregion

        #region static fields and properties

        public static IEnumerable GetUnemployedPeopleInCityTestCases {
            get {
                yield return new TestCaseData(
                    new CityTestData() {
                        Population = 10,
                        Territory = new List<HexCellTestData>() {
                            new HexCellTestData() { Slot = new WorkerSlotTestData() { IsOccupied = true  } },
                            new HexCellTestData() { Slot = new WorkerSlotTestData() { IsOccupied = true  } },
                            new HexCellTestData() { Slot = new WorkerSlotTestData() { IsOccupied = true  } },
                            new HexCellTestData() { Slot = new WorkerSlotTestData() { IsOccupied = true  } },
                            new HexCellTestData() { Slot = new WorkerSlotTestData() { IsOccupied = true  } },
                            new HexCellTestData() { Slot = new WorkerSlotTestData() { IsOccupied = false } },
                            new HexCellTestData() { Slot = new WorkerSlotTestData() { IsOccupied = false } },
                            new HexCellTestData() { Slot = new WorkerSlotTestData() { IsOccupied = false } },
                            new HexCellTestData() { Slot = new WorkerSlotTestData() { IsOccupied = false } }
                        }
                    }
                ).SetName("Occupied cell slots reduce unemployed count").Returns(5);

                yield return new TestCaseData(
                    new CityTestData() {
                        Population = 10,
                        Buildings = new List<BuildingTestData>() {
                            new BuildingTestData() {
                                Slots = new List<WorkerSlotTestData>() {
                                    new WorkerSlotTestData() { IsOccupied = true },
                                    new WorkerSlotTestData() { IsOccupied = true },
                                }
                            },
                            new BuildingTestData() {
                                Slots = new List<WorkerSlotTestData>() {
                                    new WorkerSlotTestData() { IsOccupied = false },
                                    new WorkerSlotTestData() { IsOccupied = true },
                                }
                            },
                            new BuildingTestData() {
                                Slots = new List<WorkerSlotTestData>() {
                                    new WorkerSlotTestData() { IsOccupied = true },
                                    new WorkerSlotTestData() { IsOccupied = false },
                                }
                            },
                            new BuildingTestData() {
                                Slots = new List<WorkerSlotTestData>() {
                                    new WorkerSlotTestData() { IsOccupied = false },
                                    new WorkerSlotTestData() { IsOccupied = false },
                                }
                            }
                        }
                    }
                ).SetName("Occupied building slots reduce unemployed count").Returns(6);
            }
        }

        #endregion

        #region instance fields and properties

        private Mock<IPossessionRelationship<ICity, IHexCell>>  MockCellPossessionCanon;
        private Mock<IPossessionRelationship<ICity, IBuilding>> MockBuildingPossessionCanon;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockCellPossessionCanon     = new Mock<IPossessionRelationship<ICity, IHexCell>>();
            MockBuildingPossessionCanon = new Mock<IPossessionRelationship<ICity, IBuilding>>();

            Container.Bind<IPossessionRelationship<ICity, IHexCell>> ().FromInstance(MockCellPossessionCanon    .Object);
            Container.Bind<IPossessionRelationship<ICity, IBuilding>>().FromInstance(MockBuildingPossessionCanon.Object);

            Container.Bind<UnemploymentLogic>().AsSingle();
        }

        #endregion

        #region tests

        [TestCaseSource("GetUnemployedPeopleInCityTestCases")]
        [Test(Description = "")]
        public int GetUnemployedPeopleInCityTests(CityTestData cityData) {
            var city = BuildCity(cityData);

            var unemploymentLogic = Container.Resolve<UnemploymentLogic>();

            return unemploymentLogic.GetUnemployedPeopleInCity(city);
        }

        [Test(Description = "Any city whose territory and buildings contain a number of " +
            "occupied slots that exceed the city's population should throw a " +
            "NegativePopulationException when passed into GetUnemployedPeopleInCity")]
        public void GetUnemployedPeopleInCity_ThrowsOnNegativeUnemployment() {
            var city = BuildCity(new CityTestData() {
                Population = 5,
                Territory = new List<HexCellTestData>() {
                    new HexCellTestData() { Slot = new WorkerSlotTestData() { IsOccupied = true } },
                    new HexCellTestData() { Slot = new WorkerSlotTestData() { IsOccupied = true } },
                    new HexCellTestData() { Slot = new WorkerSlotTestData() { IsOccupied = true } },
                    new HexCellTestData() { Slot = new WorkerSlotTestData() { IsOccupied = true } },
                    new HexCellTestData() { Slot = new WorkerSlotTestData() { IsOccupied = true } },
                },
                Buildings = new List<BuildingTestData>() {
                    new BuildingTestData() {
                        Slots = new List<WorkerSlotTestData>() {
                            new WorkerSlotTestData() { IsOccupied = true }
                        }
                    }
                }
            });

            var distributionLogic = Container.Resolve<UnemploymentLogic>();

            Assert.Throws<NegativeUnemploymentException>(() => distributionLogic.GetUnemployedPeopleInCity(city));
        }

        #endregion

        #region utilities

        private ICity BuildCity(CityTestData cityData) {
            var mockCity = new Mock<ICity>();

            mockCity.Setup(city => city.Population).Returns(cityData.Population);

            var newCity = mockCity.Object;

            var territory = cityData.Territory.Select(cellData     => BuildHexCell (cellData    )).ToList();
            var buildings = cityData.Buildings.Select(buildingData => BuildBuilding(buildingData)).ToList();

            MockCellPossessionCanon    .Setup(canon => canon.GetPossessionsOfOwner(newCity)).Returns(territory);
            MockBuildingPossessionCanon.Setup(canon => canon.GetPossessionsOfOwner(newCity)).Returns(buildings);

            return newCity;
        }

        private IHexCell BuildHexCell(HexCellTestData hexData) {
            var mockCell = new Mock<IHexCell>();

            mockCell.Setup(cell => cell.SuppressSlot).Returns(hexData.SuppressSlot);
            mockCell.Setup(cell => cell.WorkerSlot).Returns(BuildSlot(hexData.Slot));

            return mockCell.Object;
        }

        private IBuilding BuildBuilding(BuildingTestData buildingData) {
            var mockBuilding = new Mock<IBuilding>();

            var slots = buildingData.Slots.Select(slotData => BuildSlot(slotData)).ToList();

            mockBuilding.Setup(building => building.Slots).Returns(slots.AsReadOnly());

            return mockBuilding.Object;
        }

        private IWorkerSlot BuildSlot(WorkerSlotTestData slotData) {
            var mockSlot = new Mock<IWorkerSlot>();

            mockSlot.Setup(slot => slot.IsOccupied).Returns(slotData.IsOccupied);

            return mockSlot.Object;
        }

        #endregion

        #endregion

    }

}
