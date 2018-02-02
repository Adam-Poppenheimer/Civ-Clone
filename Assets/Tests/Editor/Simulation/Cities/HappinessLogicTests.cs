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
using Assets.Simulation.SpecialtyResources;
using Assets.Simulation.Cities.Buildings;

namespace Assets.Tests.Simulation.Cities {

    [TestFixture]
    public class HappinessLogicTests : ZenjectUnitTestFixture {

        #region internal types

        public struct GetHappinessOfCityTestData {

            public int Population;

            public int BaseHappiness;

            public List<ResourceTestData> ResourcesAssignedToCity;

            public List<BuildingTestData> BuildingsInCity;

        }

        public struct ResourceTestData {

            public SpecialtyResourceType Type;

        }

        public struct BuildingTestData {

            public int Happiness;

        }

        #endregion

        #region static fields and properties

        private static IEnumerable GetHappinessOfCityTestCases {
            get {
                yield return new TestCaseData(new GetHappinessOfCityTestData() {
                    Population = 0,
                    BaseHappiness = 4,
                    ResourcesAssignedToCity = new List<ResourceTestData>(),
                    BuildingsInCity = new List<BuildingTestData>()
                }).SetName("Only base happiness applies").Returns(4);

                yield return new TestCaseData(new GetHappinessOfCityTestData() {
                    Population = 3,
                    BaseHappiness = 4,
                    ResourcesAssignedToCity = new List<ResourceTestData>(),
                    BuildingsInCity = new List<BuildingTestData>()
                }).SetName("Each population reduces happiness by 1").Returns(1);

                yield return new TestCaseData(new GetHappinessOfCityTestData() {
                    Population = 0,
                    BaseHappiness = 4,
                    ResourcesAssignedToCity = new List<ResourceTestData>() {
                        new ResourceTestData() { Type = SpecialtyResourceType.Luxury },
                        new ResourceTestData() { Type = SpecialtyResourceType.Luxury },
                        new ResourceTestData() { Type = SpecialtyResourceType.Luxury },
                    },
                    BuildingsInCity = new List<BuildingTestData>()
                }).SetName("Luxury resources increase happiness by 1").Returns(7);

                yield return new TestCaseData(new GetHappinessOfCityTestData() {
                    Population = 0,
                    BaseHappiness = 4,
                    ResourcesAssignedToCity = new List<ResourceTestData>() {
                        new ResourceTestData() { Type = SpecialtyResourceType.Luxury },
                        new ResourceTestData() { Type = SpecialtyResourceType.Strategic },
                        new ResourceTestData() { Type = SpecialtyResourceType.Bonus },
                    },
                    BuildingsInCity = new List<BuildingTestData>()
                }).SetName("Strategic and bonus resources have no effect").Returns(5);

                yield return new TestCaseData(new GetHappinessOfCityTestData() {
                    Population = 0,
                    BaseHappiness = 4,
                    ResourcesAssignedToCity = new List<ResourceTestData>(),
                    BuildingsInCity = new List<BuildingTestData>() {
                        new BuildingTestData() { Happiness = 1 },
                        new BuildingTestData() { Happiness = -2 },
                        new BuildingTestData() { Happiness = 4 },
                    }
                }).SetName("Buildings increase and decrease happiness").Returns(7);

                yield return new TestCaseData(new GetHappinessOfCityTestData() {
                    Population = 6,
                    BaseHappiness = 4,
                    ResourcesAssignedToCity = new List<ResourceTestData>(),
                    BuildingsInCity = new List<BuildingTestData>()
                }).SetName("Happiness can become negative").Returns(-2);

                yield return new TestCaseData(new GetHappinessOfCityTestData() {
                    Population = 7,
                    BaseHappiness = 4,
                    ResourcesAssignedToCity = new List<ResourceTestData>() {
                        new ResourceTestData() { Type = SpecialtyResourceType.Luxury },
                        new ResourceTestData() { Type = SpecialtyResourceType.Luxury },
                        new ResourceTestData() { Type = SpecialtyResourceType.Luxury },
                    },
                    BuildingsInCity = new List<BuildingTestData>() {
                        new BuildingTestData() { Happiness = 2 },
                        new BuildingTestData() { Happiness = -1 },
                        new BuildingTestData() { Happiness = 1 },
                    }
                }).SetName("Mix of sources considers all factors").Returns(2);
            }
        }

        #endregion

        #region instance fields and properties

        private Mock<ICityConfig> MockConfig;

        private Mock<ICityResourceAssignmentCanon> MockResourceAssignmentCanon;

        private Mock<IPossessionRelationship<ICity, IBuilding>> MockBuildingPossessionCanon;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockConfig                  = new Mock<ICityConfig>();
            MockResourceAssignmentCanon = new Mock<ICityResourceAssignmentCanon>();
            MockBuildingPossessionCanon = new Mock<IPossessionRelationship<ICity, IBuilding>>();

            Container.Bind<ICityConfig>                              ().FromInstance(MockConfig                 .Object);
            Container.Bind<ICityResourceAssignmentCanon>             ().FromInstance(MockResourceAssignmentCanon.Object);
            Container.Bind<IPossessionRelationship<ICity, IBuilding>>().FromInstance(MockBuildingPossessionCanon.Object);

            Container.Bind<HappinessLogic>().AsSingle();
        }

        #endregion

        #region tests

        [Test(Description = "GetHappinessOfCity should consider the population of the argued " +
            "city, any luxury resources it has assigned to it, and the happiness of any " +
            "buildings it contains.")]
        [TestCaseSource("GetHappinessOfCityTestCases")]
        public int GetHappinessOfCityTests(GetHappinessOfCityTestData data) {
            MockConfig.Setup(config => config.BaseHappiness).Returns(data.BaseHappiness);

            var buildings = data.BuildingsInCity.Select(buildingData => BuildBuilding(buildingData));
            var resources = data.ResourcesAssignedToCity.Select(resourceData => BuildResource(resourceData));

            var cityToTest = BuildCity(data.Population, buildings, resources);
            
            var happinessLogic = Container.Resolve<HappinessLogic>();

            return happinessLogic.GetHappinessOfCity(cityToTest);
        }

        #endregion

        #region utilities

        private IBuilding BuildBuilding(BuildingTestData data) {
            var mockBuilding = new Mock<IBuilding>();

            mockBuilding.Setup(building => building.Happiness).Returns(data.Happiness);

            return mockBuilding.Object;
        }

        private ISpecialtyResourceDefinition BuildResource(ResourceTestData data) {
            var mockResource = new Mock<ISpecialtyResourceDefinition>();

            mockResource.Setup(resource => resource.Type).Returns(data.Type);

            return mockResource.Object;
        }

        private ICity BuildCity(
            int population, IEnumerable<IBuilding> buildings, 
            IEnumerable<ISpecialtyResourceDefinition> resources
        ){
            var mockCity = new Mock<ICity>();

            mockCity.Setup(city => city.Population).Returns(population);

            var newCity = mockCity.Object;

            MockBuildingPossessionCanon.Setup(canon => canon.GetPossessionsOfOwner(newCity)).Returns(buildings);

            MockResourceAssignmentCanon.Setup(canon => canon.GetAllResourcesAssignedToCity(newCity)).Returns(resources);

            return newCity;
        }

        #endregion

        #endregion

    }

}
