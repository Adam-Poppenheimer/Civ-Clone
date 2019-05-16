using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation;
using Assets.Simulation.Cities.Buildings;
using Assets.Simulation.Cities;
using Assets.Simulation.SocialPolicies;
using Assets.Simulation.Civilizations;
using Assets.Simulation.Units;

namespace Assets.Tests.Simulation.Cities {

    [TestFixture]
    public class CityHappinessLogicTests : ZenjectUnitTestFixture {

        #region internal types

        public class CityHappinessLogicTestData {

            public CityTestData City;

            public ConfigTestData Config;

            public float PerPopulationHappinessModifier   = 0f;
            public float PerPopulationUnhappinessModifier = 0f;            

        }

        public class CityTestData {

            public int Population;

            public List<BuildingTestData> Buildings = new List<BuildingTestData>();

            public bool IsConnectedToCapital;
            public bool IsGarrisoned;

            public int ConnectedToCapitalHappiness;
            public int GarrisonedCityHappiness;

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

                yield return new TestCaseData(new CityHappinessLogicTestData() {
                    City = new CityTestData() {
                        Population = 10
                    },
                    Config = new ConfigTestData() { },
                    PerPopulationUnhappinessModifier = -2.5f
                }).SetName("Considers per-population unhappiness modifiers").Returns(Mathf.FloorToInt(-2.5f * 10));
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

                yield return new TestCaseData(new CityHappinessLogicTestData() {
                    City = new CityTestData() {
                        Population = 10
                    },
                    Config = new ConfigTestData() { },
                    PerPopulationHappinessModifier = 2.5f
                }).SetName("Considers per-population happiness modifiers").Returns(Mathf.FloorToInt(2.5f * 10));
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

                yield return new TestCaseData(new CityHappinessLogicTestData() {
                    City = new CityTestData() {
                        IsConnectedToCapital = true, ConnectedToCapitalHappiness = 7
                    },
                    Config = new ConfigTestData(),
                }).SetName("Considers capital connection happiness from owner policies if connected to capital").Returns(7);

                yield return new TestCaseData(new CityHappinessLogicTestData() {
                    City = new CityTestData() {
                        IsConnectedToCapital = false, ConnectedToCapitalHappiness = 7
                    },
                    Config = new ConfigTestData(),
                }).SetName("Ignores capital connection happiness from owner policies if not connected to capital").Returns(0);

                yield return new TestCaseData(new CityHappinessLogicTestData() {
                    City = new CityTestData() {
                        IsGarrisoned = true, GarrisonedCityHappiness = 5
                    },
                    Config = new ConfigTestData(),
                }).SetName("Considers garrisoned city happiness if city is garrisoned").Returns(5);

                yield return new TestCaseData(new CityHappinessLogicTestData() {
                    City = new CityTestData() {
                        IsGarrisoned = false, GarrisonedCityHappiness = 5
                    },
                    Config = new ConfigTestData(),
                }).SetName("Ignores garrisoned city happiness if city is not garrisoned").Returns(0);
            }
        }

        #endregion

        #region instance fields and properties

        private Mock<ICityConfig>                                   MockConfig;
        private Mock<IPossessionRelationship<ICity, IBuilding>>     MockBuildingPossessionCanon;
        private Mock<ICityModifiers>                                MockCityModifiers;
        private Mock<ICapitalConnectionLogic>                       MockCapitalConnectionLogic;
        private Mock<IUnitGarrisonLogic>                            MockUnitGarrisonLogic;

        private Mock<ICityModifier<int>> MockGarrisonedCityHappinessModifier;
        private Mock<ICityModifier<int>> MockCapitalConnectionHappinessModifier;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockConfig                  = new Mock<ICityConfig>();
            MockBuildingPossessionCanon = new Mock<IPossessionRelationship<ICity, IBuilding>>();
            MockCityModifiers           = new Mock<ICityModifiers>();
            MockCapitalConnectionLogic  = new Mock<ICapitalConnectionLogic>();
            MockUnitGarrisonLogic       = new Mock<IUnitGarrisonLogic>();

            MockGarrisonedCityHappinessModifier    = new Mock<ICityModifier<int>>();
            MockCapitalConnectionHappinessModifier = new Mock<ICityModifier<int>>();

            MockCityModifiers.Setup(modifiers => modifiers.GarrisonedHappiness)       .Returns(MockGarrisonedCityHappinessModifier   .Object);
            MockCityModifiers.Setup(modifiers => modifiers.CapitalConnectionHappiness).Returns(MockCapitalConnectionHappinessModifier.Object);

            Container.Bind<ICityConfig>                              ().FromInstance(MockConfig                 .Object);
            Container.Bind<IPossessionRelationship<ICity, IBuilding>>().FromInstance(MockBuildingPossessionCanon.Object);
            Container.Bind<ICityModifiers>                           ().FromInstance(MockCityModifiers          .Object);
            Container.Bind<ICapitalConnectionLogic>                  ().FromInstance(MockCapitalConnectionLogic .Object);
            Container.Bind<IUnitGarrisonLogic>                       ().FromInstance(MockUnitGarrisonLogic      .Object);

            Container.Bind<CityHappinessLogic>().AsSingle();
        }

        private void SetupConfig(CityHappinessLogicTestData testData) {
            MockConfig.Setup(config => config.UnhappinessPerCity      ).Returns(testData.Config.UnhappinessPerCity);
            MockConfig.Setup(config => config.UnhappinessPerPopulation).Returns(testData.Config.UnhappinessPerPopulation);

            var mockHappinessModifier   = new Mock<ICityModifier<float>>();
            var mockUnhappinessModifier = new Mock<ICityModifier<float>>();

            mockHappinessModifier.Setup(modifier => modifier.GetValueForCity(It.IsAny<ICity>()))
                                 .Returns(testData.PerPopulationHappinessModifier);

            mockUnhappinessModifier.Setup(modifier => modifier.GetValueForCity(It.IsAny<ICity>()))
                                   .Returns(testData.PerPopulationUnhappinessModifier);

            MockCityModifiers.Setup(modifiers => modifiers.PerPopulationHappiness)  .Returns(mockHappinessModifier  .Object);
            MockCityModifiers.Setup(modifiers => modifiers.PerPopulationUnhappiness).Returns(mockUnhappinessModifier.Object);
        }

        #endregion

        #region tests

        [Test(Description = "")]
        [TestCaseSource("GetUnhappinessOfCityTestCases")]
        public int GetUnhappinessOfCityTests(CityHappinessLogicTestData testData) {
            var city = BuildCity(testData.City);

            SetupConfig(testData);

            var happinessLogic = Container.Resolve<CityHappinessLogic>();

            return happinessLogic.GetUnhappinessOfCity(city);
        }

        [Test(Description = "")]
        [TestCaseSource("GetLocalHappinessOfCityTestCases")]
        public int GetLocalHappinessOfCityTests(CityHappinessLogicTestData testData) {
            var city = BuildCity(testData.City);

            SetupConfig(testData);

            var happinessLogic = Container.Resolve<CityHappinessLogic>();

            return happinessLogic.GetLocalHappinessOfCity(city);
        }

        [Test(Description = "")]
        [TestCaseSource("GetGlobalHappinessOfCityTestCases")]
        public int GetGlobalHappinessOfCityTests(CityHappinessLogicTestData testData) {
            var city = BuildCity(testData.City);

            SetupConfig(testData);

            var happinessLogic = Container.Resolve<CityHappinessLogic>();

            return happinessLogic.GetGlobalHappinessOfCity(city);
        }

        #endregion

        #region utilities

        private ICity BuildCity(CityTestData cityData) {
            var mockCity = new Mock<ICity>();

            mockCity.Setup(city => city.Population).Returns(cityData.Population);

            var newCity = mockCity.Object;

            MockBuildingPossessionCanon
                .Setup(canon => canon.GetPossessionsOfOwner(newCity))
                .Returns(cityData.Buildings.Select(buildingData => BuildBuilding(buildingData)));

            MockCapitalConnectionLogic.Setup(logic => logic.IsCityConnectedToCapital(newCity))
                                      .Returns(cityData.IsConnectedToCapital);

            MockCapitalConnectionHappinessModifier.Setup(modifier => modifier.GetValueForCity(newCity))
                                                  .Returns(cityData.ConnectedToCapitalHappiness);

            MockUnitGarrisonLogic.Setup(logic => logic.IsCityGarrisoned(newCity)).Returns(cityData.IsGarrisoned);

            MockGarrisonedCityHappinessModifier.Setup(modifier => modifier.GetValueForCity(newCity))
                                               .Returns(cityData.GarrisonedCityHappiness);

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
