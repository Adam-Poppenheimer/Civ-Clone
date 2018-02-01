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

namespace Assets.Tests.Simulation.Cities {
    
    [TestFixture]
    public class HealthLogicTests : ZenjectUnitTestFixture {

        #region internal types

        public struct GetHealthOfCityTestData {

            public int CityPopulation;
            public int BaseHealth;

            public List<BuildingTestData> Buildings;

        }

        public struct BuildingTestData {

            public int Health;

        }

        #endregion

        #region static fields and properties

        public static IEnumerable GetHealthOfCityTestCases {
            get {
                yield return new TestCaseData(new GetHealthOfCityTestData() {
                    CityPopulation = 0,
                    BaseHealth = 4,
                    Buildings = new List<BuildingTestData>() { }

                }).SetName("Starts at configured base health").Returns(4);

                yield return new TestCaseData(new GetHealthOfCityTestData() {
                    CityPopulation = 3,
                    BaseHealth = 4,
                    Buildings = new List<BuildingTestData>() { }

                }).SetName("Subtracts one for every citizen").Returns(1);

                yield return new TestCaseData(new GetHealthOfCityTestData() {
                    CityPopulation = 6,
                    BaseHealth = 4,
                    Buildings = new List<BuildingTestData>() { }

                }).SetName("Can become negative").Returns(-2);

                yield return new TestCaseData(new GetHealthOfCityTestData() {
                    CityPopulation = 3,
                    BaseHealth = 4,
                    Buildings = new List<BuildingTestData>() {
                        new BuildingTestData() { Health = 1 },
                        new BuildingTestData() { Health = 2 },
                    }
                }).SetName("Considers health of buildings").Returns(4);

                yield return new TestCaseData(new GetHealthOfCityTestData() {
                    CityPopulation = 3,
                    BaseHealth = 4,
                    Buildings = new List<BuildingTestData>() {
                        new BuildingTestData() { Health = 1 },
                        new BuildingTestData() { Health = -4 },
                    }
                }).SetName("Buildings can cause unhealthiness").Returns(-2);
            }
        }

        #endregion

        #region instance fields and properties

        private Mock<ICityConfig> MockConfig;

        private Mock<IPossessionRelationship<ICity, IBuilding>> MockBuildingPossessionCanon;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockConfig                  = new Mock<ICityConfig>();
            MockBuildingPossessionCanon = new Mock<IPossessionRelationship<ICity, IBuilding>>();

            Container.Bind<ICityConfig>                              ().FromInstance(MockConfig                 .Object);
            Container.Bind<IPossessionRelationship<ICity, IBuilding>>().FromInstance(MockBuildingPossessionCanon.Object);

            Container.Bind<HealthLogic>().AsSingle();
        }

        #endregion

        #region tests

        [Test(Description = "GetHealthOfCity should take into account CityConfig's BaseHealth property, " +
            "the population of the city in question, and any buildings that modify health")]
        [TestCaseSource("GetHealthOfCityTestCases")]
        public int GetHealthOfCityTests(GetHealthOfCityTestData data) {
            List<IBuilding> buildings = data.Buildings.Select(buildingData => BuildBuilding(buildingData)).ToList();

            MockConfig.Setup(config => config.BaseHealth).Returns(data.BaseHealth);

            var city = BuildCity(data.CityPopulation, buildings);

            var healthLogic = Container.Resolve<HealthLogic>();

            return healthLogic.GetHealthOfCity(city);
        }

        #endregion

        #region utilities

        private IBuilding BuildBuilding(BuildingTestData data) {
            var mockBuilding = new Mock<IBuilding>();

            mockBuilding.Setup(building => building.Health).Returns(data.Health);

            return mockBuilding.Object;
        }

        private ICity BuildCity(int population, List<IBuilding> buildings) {
            var mockCity = new Mock<ICity>();

            mockCity.Setup(city => city.Population).Returns(population);

            var newCity = mockCity.Object;

            MockBuildingPossessionCanon.Setup(canon => canon.GetPossessionsOfOwner(newCity)).Returns(buildings);

            return newCity;
        }

        #endregion

        #endregion

    }
}
