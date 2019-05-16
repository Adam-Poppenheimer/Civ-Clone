using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation;
using Assets.Simulation.Civilizations;
using Assets.Simulation.Cities;
using Assets.Simulation.MapResources;

namespace Assets.Tests.Simulation.Civilizations {

    [TestFixture]
    public class CivilizationHappinessLogicTests : ZenjectUnitTestFixture {

        #region internal types

        public class CivilizationHappinessLogicTestData {

            public CivilizationTestData Civilization;

            public List<CityTestData> Cities;

            public ConfigTestData Config;

            public List<ResourceTestData> AvailableResources;

        }

        public class CivilizationTestData {



        }

        public class CityTestData {

            public int Population;

            public int LocalHappiness;
            public int GlobalHappiness;            
            public int Unhappiness;

        }

        public class ConfigTestData {

            public int BaseHappiness;
            public int HappinessPerLuxury;

        }

        public class ResourceTestData {

            public Assets.Simulation.MapResources.ResourceType Type;

        }

        #endregion

        #region static fields and properties

        private static IEnumerable GetHappinessOfCivTestCases {
            get {
                yield return new TestCaseData(new CivilizationHappinessLogicTestData() {
                    Civilization = new CivilizationTestData(),
                    Cities = new List<CityTestData>() { },
                    AvailableResources = new List<ResourceTestData>() { },
                    Config = new ConfigTestData() {
                        BaseHappiness = 5
                    }
                }).SetName("Considers configured base happiness").Returns(5);

                yield return new TestCaseData(new CivilizationHappinessLogicTestData() {
                    Civilization = new CivilizationTestData(),
                    Cities = new List<CityTestData>() { },
                    AvailableResources = new List<ResourceTestData>() {
                        new ResourceTestData() { Type = Assets.Simulation.MapResources.ResourceType.Luxury },
                        new ResourceTestData() { Type = Assets.Simulation.MapResources.ResourceType.Luxury },
                        new ResourceTestData() { Type = Assets.Simulation.MapResources.ResourceType.Bonus },
                        new ResourceTestData() { Type = Assets.Simulation.MapResources.ResourceType.Strategic },
                    },
                    Config = new ConfigTestData() {
                        BaseHappiness = 0, HappinessPerLuxury = 4
                    }
                }).SetName("Considers luxury resources").Returns(8);

                yield return new TestCaseData(new CivilizationHappinessLogicTestData() {
                    Civilization = new CivilizationTestData(),
                    Cities = new List<CityTestData>() {
                        new CityTestData() { GlobalHappiness = 3 },
                        new CityTestData() { GlobalHappiness = 5 },
                        new CityTestData() { GlobalHappiness = 2 },
                        new CityTestData() { GlobalHappiness = 0 },
                    },
                    AvailableResources = new List<ResourceTestData>() { },
                    Config = new ConfigTestData() {}
                }).SetName("Considers global happiness of cities").Returns(10);

                yield return new TestCaseData(new CivilizationHappinessLogicTestData() {
                    Civilization = new CivilizationTestData(),
                    Cities = new List<CityTestData>() {
                        new CityTestData() { LocalHappiness = 3, Population = 5 },
                        new CityTestData() { LocalHappiness = 5, Population = 5 },
                        new CityTestData() { LocalHappiness = 2, Population = 5 },
                        new CityTestData() { LocalHappiness = 0, Population = 5 },
                    },
                    AvailableResources = new List<ResourceTestData>() { },
                    Config = new ConfigTestData() {}
                }).SetName("Considers local happiness of cities").Returns(10);

                yield return new TestCaseData(new CivilizationHappinessLogicTestData() {
                    Civilization = new CivilizationTestData(),
                    Cities = new List<CityTestData>() {
                        new CityTestData() { LocalHappiness = 3, Population = 3 },
                        new CityTestData() { LocalHappiness = 5, Population = 3 },
                        new CityTestData() { LocalHappiness = 2, Population = 3 },
                        new CityTestData() { LocalHappiness = 0, Population = 3 },
                    },
                    AvailableResources = new List<ResourceTestData>() { },
                    Config = new ConfigTestData() {}
                }).SetName("Local happiness limited by population").Returns(8);

                yield return new TestCaseData(new CivilizationHappinessLogicTestData() {
                    Civilization = new CivilizationTestData(),
                    Cities = new List<CityTestData>() {
                        new CityTestData() { LocalHappiness = 3, GlobalHappiness = 1, Population = 3 },
                        new CityTestData() { LocalHappiness = 5, GlobalHappiness = 2, Population = 3 },
                        new CityTestData() { LocalHappiness = 2, GlobalHappiness = 0, Population = 3 },
                        new CityTestData() { LocalHappiness = 0, GlobalHappiness = 0, Population = 3 },
                    },
                    AvailableResources = new List<ResourceTestData>() {
                        new ResourceTestData() { Type = Assets.Simulation.MapResources.ResourceType.Luxury },
                        new ResourceTestData() { Type = Assets.Simulation.MapResources.ResourceType.Luxury },
                        new ResourceTestData() { Type = Assets.Simulation.MapResources.ResourceType.Bonus },
                        new ResourceTestData() { Type = Assets.Simulation.MapResources.ResourceType.Strategic },
                    },
                    Config = new ConfigTestData() {
                        BaseHappiness = 5, HappinessPerLuxury = 4
                    }
                }).SetName("Integrates components independently").Returns(24);
            }
        }

        private static IEnumerable GetUnhappinessOfCivTestCases {
            get {
                yield return new TestCaseData(new CivilizationHappinessLogicTestData() {
                    Civilization = new CivilizationTestData(),
                    Cities = new List<CityTestData>() {
                        new CityTestData() { Unhappiness = 5 },
                        new CityTestData() { Unhappiness = 2 },
                        new CityTestData() { Unhappiness = 3 },
                        new CityTestData() { Unhappiness = 0 },
                    },
                    AvailableResources = new List<ResourceTestData>() { },
                    Config = new ConfigTestData() { }

                }).SetName("Considers unhappiness of cities").Returns(10);
            }
        }

        #endregion

        #region instance fields and properties

        private Mock<ICivilizationConfig>                           MockConfig;
        private Mock<IFreeResourcesLogic>                           MockFreeResourcesLogic;
        private Mock<IPossessionRelationship<ICivilization, ICity>> MockCityPossessionCanon;
        private Mock<ICityHappinessLogic>                           MockCityHappinessLogic;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockConfig              = new Mock<ICivilizationConfig>();
            MockFreeResourcesLogic  = new Mock<IFreeResourcesLogic>();
            MockCityPossessionCanon = new Mock<IPossessionRelationship<ICivilization, ICity>>();
            MockCityHappinessLogic  = new Mock<ICityHappinessLogic>();

            Container.Bind<ICivilizationConfig>                          ().FromInstance(MockConfig             .Object);
            Container.Bind<IFreeResourcesLogic>                          ().FromInstance(MockFreeResourcesLogic .Object);
            Container.Bind<IPossessionRelationship<ICivilization, ICity>>().FromInstance(MockCityPossessionCanon.Object);
            Container.Bind<ICityHappinessLogic>                          ().FromInstance(MockCityHappinessLogic .Object);

            Container.Bind<CivilizationHappinessLogic>().AsSingle();
        }

        private void SetupConfig(ConfigTestData data) {
            MockConfig.Setup(config => config.BaseHappiness     ).Returns(data.BaseHappiness);
            MockConfig.Setup(config => config.HappinessPerLuxury).Returns(data.HappinessPerLuxury);
        }

        private void SetupTest(
            CivilizationHappinessLogicTestData testData, out ICivilization civ,
            out CivilizationHappinessLogic happinessLogic
        ){
            var cities = testData.Cities.Select(cityData => BuildCity(cityData));

            var resources = testData.AvailableResources.Select(resourceData => BuildResource(resourceData)).ToList();

            civ = BuildCivilization(testData.Civilization, cities, resources);

            SetupConfig(testData.Config);

            Container.Bind<IEnumerable<IResourceDefinition>>()
                .WithId("Available Resources")
                .FromInstance(resources);

            happinessLogic = Container.Resolve<CivilizationHappinessLogic>();
        }

        #endregion

        #region tests

        [Test(Description = "")]
        [TestCaseSource("GetHappinessOfCivTestCases")]
        public int GetHappinessOfCivTests(CivilizationHappinessLogicTestData testData) {
            ICivilization civilization;
            CivilizationHappinessLogic happinessLogic;

            SetupTest(testData, out civilization, out happinessLogic);

            return happinessLogic.GetHappinessOfCiv(civilization);
        }

        [Test(Description = "")]
        [TestCaseSource("GetUnhappinessOfCivTestCases")]
        public int GetUnhappinessOfCivTests(CivilizationHappinessLogicTestData testData) {
            ICivilization civilization;
            CivilizationHappinessLogic happinessLogic;

            SetupTest(testData, out civilization, out happinessLogic);

            return happinessLogic.GetUnhappinessOfCiv(civilization);
        }

        #endregion

        #region utilities

        private ICivilization BuildCivilization(
            CivilizationTestData data, IEnumerable<ICity> cities,
            IEnumerable<IResourceDefinition> resources
        ){
            var mockCiv = new Mock<ICivilization>();

            var newCiv = mockCiv.Object;

            MockCityPossessionCanon.Setup(canon => canon.GetPossessionsOfOwner(newCiv)).Returns(cities);

            foreach(var resource in resources) {
                MockFreeResourcesLogic
                    .Setup(canon => canon.GetFreeCopiesOfResourceForCiv(resource, newCiv))
                    .Returns(1);
            }

            return newCiv;
        }

        private ICity BuildCity(CityTestData data) {
            var mockCity = new Mock<ICity>();

            mockCity.Setup(city => city.Population).Returns(data.Population);

            var newCity = mockCity.Object;

            MockCityHappinessLogic.Setup(logic => logic.GetLocalHappinessOfCity (newCity)).Returns(data.LocalHappiness);
            MockCityHappinessLogic.Setup(logic => logic.GetGlobalHappinessOfCity(newCity)).Returns(data.GlobalHappiness);
            MockCityHappinessLogic.Setup(logic => logic.GetUnhappinessOfCity    (newCity)).Returns(data.Unhappiness);

            return newCity;
        }

        private IResourceDefinition BuildResource(ResourceTestData data) {
            var mockResource = new Mock<IResourceDefinition>();

            mockResource.Setup(resource => resource.Type).Returns(data.Type);

            return mockResource.Object;
        }

        #endregion

        #endregion

    }

}
