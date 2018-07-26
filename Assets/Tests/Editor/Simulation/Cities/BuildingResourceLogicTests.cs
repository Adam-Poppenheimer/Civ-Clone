using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation;
using Assets.Simulation.Cities;
using Assets.Simulation.Cities.Buildings;
using Assets.Simulation.WorkerSlots;

namespace Assets.Tests.Simulation.Cities {

    [TestFixture]
    public class BuildingResourceLogicTests : ZenjectUnitTestFixture {

        #region internal types

        public class BuildingTestData {

            public YieldSummary StaticYield = YieldSummary.Empty;

            public YieldSummary SlotYield = YieldSummary.Empty;

            public YieldSummary BonusYieldPerPopulation = YieldSummary.Empty;

            public CityTestData Owner = new CityTestData();

            public List<SlotTestData> Slots = new List<SlotTestData>();

        }

        public class CityTestData {

            public int Population;

        }

        public class SlotTestData {

            public bool IsOccupied;

        }

        #endregion

        #region static fields and properties

        public static IEnumerable GetYieldOfBuildingTestCases {
            get {
                yield return new TestCaseData(
                    new BuildingTestData() {
                        StaticYield = new YieldSummary(food: 1f, production: 2f, gold: 3f, culture: 4f)
                    }
                ).SetName("Considers static yield").Returns(new YieldSummary(
                    food: 1f, production: 2f, gold: 3f, culture: 4f
                ));

                yield return new TestCaseData(
                    new BuildingTestData() {
                        BonusYieldPerPopulation = new YieldSummary(food: 1f, production: 1f, gold: 2f, culture: 3f),
                        Owner = new CityTestData() { Population = 5 }
                    }
                ).SetName("Considers per-population yield").Returns(new YieldSummary(
                    food: 5f, production: 5f, gold: 10f, culture: 15f
                ));

                yield return new TestCaseData(
                    new BuildingTestData() {
                        SlotYield = new YieldSummary(food: 1f, production: 1f),
                        Slots = new List<SlotTestData>() {
                            new SlotTestData() { IsOccupied = true },
                            new SlotTestData() { IsOccupied = true },
                            new SlotTestData() { IsOccupied = false },
                            new SlotTestData() { IsOccupied = true },
                            new SlotTestData() { IsOccupied = false },
                        }
                    }
                ).SetName("Considers yield of occupied slots").Returns(new YieldSummary(
                    food: 3f, production: 3f
                ));
            }
        }

        #endregion

        #region instance fields and properties

        private Mock<IPossessionRelationship<ICity, IBuilding>> MockBuildingPossessionCanon;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockBuildingPossessionCanon = new Mock<IPossessionRelationship<ICity, IBuilding>>();

            Container.Bind<IPossessionRelationship<ICity, IBuilding>>().FromInstance(MockBuildingPossessionCanon.Object);

            Container.Bind<BuildingYieldLogic>().AsSingle();
        }

        #endregion

        #region tests

        [TestCaseSource("GetYieldOfBuildingTestCases")]
        [Test(Description = "")]
        public YieldSummary GetYieldOfBuildingTests(BuildingTestData buildingData) {
            var building = BuildBuilding(buildingData);

            var resourceLogic = Container.Resolve<BuildingYieldLogic>();

            return resourceLogic.GetYieldOfBuilding(building);
        }

        #endregion

        #region utilities

        private IBuilding BuildBuilding(BuildingTestData buildingData) {
            var mockBuilding = new Mock<IBuilding>();

            var mockTemplate = new Mock<IBuildingTemplate>();

            mockTemplate.Setup(template => template.StaticYield            ).Returns(buildingData.StaticYield);
            mockTemplate.Setup(template => template.BonusYieldPerPopulation).Returns(buildingData.BonusYieldPerPopulation);
            mockTemplate.Setup(template => template.SlotYield              ).Returns(buildingData.SlotYield);

            mockBuilding.Setup(building => building.Template).Returns(mockTemplate.Object);

            var slots = buildingData.Slots.Select(slotData => BuildSlot(slotData)).ToList();

            mockBuilding.Setup(building => building.Slots).Returns(slots.AsReadOnly());

            var newBuilding = mockBuilding.Object;

            var owner = BuildCity(buildingData.Owner);

            MockBuildingPossessionCanon.Setup(canon => canon.GetOwnerOfPossession(newBuilding)).Returns(owner);

            return newBuilding;
        }

        private IWorkerSlot BuildSlot(SlotTestData slotData) {
            var mockSlot = new Mock<IWorkerSlot>();

            mockSlot.Setup(slot => slot.IsOccupied).Returns(slotData.IsOccupied);

            return mockSlot.Object;
        }

        private ICity BuildCity(CityTestData cityData) {
            var mockCity = new Mock<ICity>();

            mockCity.Setup(city => city.Population).Returns(cityData.Population);

            return mockCity.Object;
        }

        #endregion

        #endregion

    }

}
