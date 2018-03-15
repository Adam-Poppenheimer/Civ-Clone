﻿using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation;
using Assets.Simulation.Cities.Buildings;
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

            public List<BuildingTestData> Buildings = new List<BuildingTestData>();

        }

        public class BuildingTestData {

            public int LocalHappiness;
            public int GlobalHappiness;
            public int Unhappiness;

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

                yield return new TestCaseData(new CityHappinessLogicTestData() {
                    City = new CityTestData() {
                        Population = 0,
                        Buildings = new List<BuildingTestData>() {
                            new BuildingTestData() { LocalHappiness = 2,  GlobalHappiness = -3, Unhappiness = 4 },
                            new BuildingTestData() { LocalHappiness = 3,  GlobalHappiness = 10, Unhappiness = 4 },
                            new BuildingTestData() { LocalHappiness = -1, GlobalHappiness = 11, Unhappiness = -5 }
                        }
                    },
                    Config = new ConfigTestData() {
                        UnhappinessPerCity = 0, UnhappinessPerPopulation = 0
                    }
                }).SetName("Considers unhappiness of buildings").Returns(3);
            }
        }

        public static IEnumerable GetLocalHappinessOfCityTestCases {
            get {
                yield return new TestCaseData(new CityHappinessLogicTestData() {
                    City = new CityTestData() { },
                    Config = new ConfigTestData() { }
                }).SetName("Defaults to 0").Returns(0);

                yield return new TestCaseData(new CityHappinessLogicTestData() {
                    City = new CityTestData() {
                        Buildings = new List<BuildingTestData>() {
                            new BuildingTestData() { LocalHappiness = 2,  GlobalHappiness = -3, Unhappiness = 4 },
                            new BuildingTestData() { LocalHappiness = 3,  GlobalHappiness = 10, Unhappiness = 4 },
                            new BuildingTestData() { LocalHappiness = -1, GlobalHappiness = 11, Unhappiness = 5 }
                        }
                    },
                    Config = new ConfigTestData() { }
                }).SetName("Considers local happiness of buildings").Returns(4);
            }
        }

        public static IEnumerable GetGlobalHappinessOfCityTestCases {
            get {
                yield return new TestCaseData(new CityHappinessLogicTestData() {
                    City = new CityTestData() { },
                    Config = new ConfigTestData() { }
                }).SetName("Defaults to 0").Returns(0);

                yield return new TestCaseData(new CityHappinessLogicTestData() {
                    City = new CityTestData() {
                        Buildings = new List<BuildingTestData>() {
                            new BuildingTestData() { LocalHappiness = 2,  GlobalHappiness = -3, Unhappiness = 4 },
                            new BuildingTestData() { LocalHappiness = 3,  GlobalHappiness = 10, Unhappiness = 4 },
                            new BuildingTestData() { LocalHappiness = -1, GlobalHappiness = 11, Unhappiness = 5 }
                        }
                    },
                    Config = new ConfigTestData() { }
                }).SetName("Considers global happiness of buildings").Returns(18);
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

            Container.Bind<ICityConfig>().FromInstance(MockConfig.Object);
            Container.Bind<IPossessionRelationship<ICity, IBuilding>>().FromInstance(MockBuildingPossessionCanon.Object);

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
        [TestCaseSource("GetGlobalHappinessOfCityTestCases")]
        public int GetGlobalHappinessOfCityTests(CityHappinessLogicTestData testData) {
            var city = BuildCity(testData.City);

            SetupConfig(testData.Config);

            var happinessLogic = Container.Resolve<CityHappinessLogic>();

            return happinessLogic.GetGlobalHappinessOfCity(city);
        }

        #endregion

        #region utilities

        private ICity BuildCity(CityTestData cityData) {
            var mockCity = new Mock<ICity>();

            mockCity.Setup(city => city.Population).Returns(cityData.Population);

            MockBuildingPossessionCanon
                .Setup(canon => canon.GetPossessionsOfOwner(mockCity.Object))
                .Returns(cityData.Buildings.Select(buildingData => BuildBuilding(buildingData)));

            return mockCity.Object;
        }

        private IBuilding BuildBuilding(BuildingTestData buildingData) {
            var mockBuilding = new Mock<IBuilding>();

            var mockTemplate = new Mock<IBuildingTemplate>();

            mockTemplate.Setup(template => template.LocalHappiness ).Returns(buildingData.LocalHappiness);
            mockTemplate.Setup(template => template.GlobalHappiness).Returns(buildingData.GlobalHappiness);
            mockTemplate.Setup(template => template.Unhappiness    ).Returns(buildingData.Unhappiness);

            mockBuilding.Setup(building => building.Template).Returns(mockTemplate.Object);

            return mockBuilding.Object;
        }

        #endregion

        #endregion

    }

}
