using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation.Cities;

namespace Assets.Tests.Simulation.Cities {

    [TestFixture]
    public class CityHappinessLogicTests : ZenjectUnitTestFixture {

        #region internal types

        public class CityHappinessLogicTestData {

            public CityTestData City;

            public ConfigTestData Config;

        }

        public class CityTestData {

            public int Population;

        }

        public class ConfigTestData {

            public int UnhappinessPerCity;

            public float UnhappinessPerPopulation;

        }

        #endregion

        #region static fields and properties

        public static IEnumerable GetUnhappinessOfCityTestCases {
            get {
                yield return new TestCaseData(new CityHappinessLogicTestData() {
                    City = new CityTestData() {
                        Population = 0
                    },
                    Config = new ConfigTestData() {
                        UnhappinessPerCity = 3
                    }
                }).SetName("Considers configured base unhappiness").Returns(3);

                yield return new TestCaseData(new CityHappinessLogicTestData() {
                    City = new CityTestData() {
                        Population = 5
                    },
                    Config = new ConfigTestData() {
                        UnhappinessPerPopulation = 2
                    }
                }).SetName("Considers configured unhappiness per population").Returns(10);

                yield return new TestCaseData(new CityHappinessLogicTestData() {
                    City = new CityTestData() {
                        Population = 5
                    },
                    Config = new ConfigTestData() {
                        UnhappinessPerPopulation = 1.5f
                    }
                }).SetName("Unhappiness per population rounds").Returns(8);
            }
        }

        public static IEnumerable GetLocalHappinessOfCityTestCases {
            get {
                yield return new TestCaseData(new CityHappinessLogicTestData() {
                    City = new CityTestData() { },
                    Config = new ConfigTestData() { }
                }).SetName("Defaults to 0").Returns(0);
            }
        }

        public static IEnumerable GetGlobalHappinessOfCityTestCases {
            get {
                yield return new TestCaseData(new CityHappinessLogicTestData() {
                    City = new CityTestData() { },
                    Config = new ConfigTestData() { }
                }).SetName("Defaults to 0").Returns(0);
            }
        }

        #endregion

        #region instance fields and properties

        private Mock<ICityConfig> MockConfig;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockConfig = new Mock<ICityConfig>();

            Container.Bind<ICityConfig>().FromInstance(MockConfig.Object);

            Container.Bind<CityHappinessLogic>().AsSingle();
        }

        private void SetupConfig(ConfigTestData data) {
            MockConfig.Setup(config => config.UnhappinessPerCity      ).Returns(data.UnhappinessPerCity);
            MockConfig.Setup(config => config.UnhappinessPerPopulation).Returns(data.UnhappinessPerPopulation);
        }

        #endregion

        #region tests

        [Test(Description = "")]
        [TestCaseSource("GetUnhappinessOfCityTestCases")]
        public int GetUnhappinessOfCityTests(CityHappinessLogicTestData testData) {
            var city = BuildCity(testData.City);

            SetupConfig(testData.Config);

            var happinessLogic = Container.Resolve<CityHappinessLogic>();

            return happinessLogic.GetUnhappinessOfCity(city);
        }

        [Test(Description = "")]
        [TestCaseSource("GetLocalHappinessOfCityTestCases")]
        public int GetLocalHappinessOfCityTests(CityHappinessLogicTestData testData) {
            var city = BuildCity(testData.City);

            SetupConfig(testData.Config);

            var happinessLogic = Container.Resolve<CityHappinessLogic>();

            return happinessLogic.GetLocalHappinessOfCity(city);
        }

        [Test(Description = "")]
        [TestCaseSource("GetLocalHappinessOfCityTestCases")]
        public int GetGlobalHappinessOfCityTests(CityHappinessLogicTestData testData) {
            var city = BuildCity(testData.City);

            SetupConfig(testData.Config);

            var happinessLogic = Container.Resolve<CityHappinessLogic>();

            return happinessLogic.GetGlobalHappinessOfCity(city);
        }

        #endregion

        #region utilities

        private ICity BuildCity(CityTestData data) {
            var mockCity = new Mock<ICity>();

            mockCity.Setup(city => city.Population).Returns(data.Population);

            return mockCity.Object;
        }

        #endregion

        #endregion

    }

}
