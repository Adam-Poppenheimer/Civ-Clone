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
using Assets.Simulation.Cities;
using Assets.Simulation.Cities.Buildings;

namespace Assets.Tests.Simulation.Cities {

    [TestFixture]
    public class CityCombatLogicTests : ZenjectUnitTestFixture {

        #region internal types

        public class CityCombatLogicTestData {

            public CityTestData City;

            public CityConfigTestData Config;

        }

        public class CityTestData {

            public int Population;

            public List<BuildingTestData> Buildings;

        }

        public class BuildingTestData {

            public int CityCombatStrengthBonus;

            public int CityMaxHitpointBonus;

        }

        public class CityConfigTestData {

            public int BaseCombatStrength;

            public float CombatStrengthPerPopulation;

            public int BaseMaxHitpoints;

            public float MaxHitPointsPerPopulation;

            public int BaseRangedAttackStrength;

            public float RangedAttackStrengthPerPopulation;

        }

        #endregion

        #region static fields and properties

        private static IEnumerable GetCombatStrengthOfCityTestCases {
            get {
                yield return new TestCaseData(new CityCombatLogicTestData() {
                    City = new CityTestData() {
                        Buildings = new List<BuildingTestData>() { },
                        Population = 0
                    },
                    Config = new CityConfigTestData() {
                        BaseCombatStrength = 10
                    }
                }).SetName("Starts at CityConfig.BaseCombatStrength").Returns(10);

                yield return new TestCaseData(new CityCombatLogicTestData() {
                    City = new CityTestData() {
                        Buildings = new List<BuildingTestData>() { },
                        Population = 4
                    },
                    Config = new CityConfigTestData() {
                        BaseCombatStrength = 10,
                        CombatStrengthPerPopulation = 7
                    }
                }).SetName("Modified by population and CityConfig.CombatStrengthPerPopulation").Returns(38);

                yield return new TestCaseData(new CityCombatLogicTestData() {
                    City = new CityTestData() {
                        Buildings = new List<BuildingTestData>() {
                            new BuildingTestData() { CityCombatStrengthBonus = 6 },
                            new BuildingTestData() { CityCombatStrengthBonus = 14 },
                            new BuildingTestData() { CityCombatStrengthBonus = -5 },
                        },
                    },
                    Config = new CityConfigTestData() {

                    }
                }).SetName("Modified by buildings that provide combat strength bonuses").Returns(15);
            }
        }

        private static IEnumerable GetMaxHitpointsOfCityTestCases {
            get {
                yield return new TestCaseData(new CityCombatLogicTestData() {
                    City = new CityTestData() {
                        Buildings = new List<BuildingTestData>() { },
                        Population = 0
                    },
                    Config = new CityConfigTestData() {
                        BaseMaxHitpoints = 10
                    }
                }).SetName("Starts at CityConfig.BaseMaxHitpoints").Returns(10);

                yield return new TestCaseData(new CityCombatLogicTestData() {
                    City = new CityTestData() {
                        Buildings = new List<BuildingTestData>() { },
                        Population = 4
                    },
                    Config = new CityConfigTestData() {
                        BaseMaxHitpoints = 10,
                        MaxHitPointsPerPopulation = 7
                    }
                }).SetName("Modified by population and CityConfig.MaxHitPointsPerPopulation").Returns(38);

                yield return new TestCaseData(new CityCombatLogicTestData() {
                    City = new CityTestData() {
                        Buildings = new List<BuildingTestData>() {
                            new BuildingTestData() { CityMaxHitpointBonus = 6 },
                            new BuildingTestData() { CityMaxHitpointBonus = 14 },
                            new BuildingTestData() { CityMaxHitpointBonus = -5 },
                        },
                    },
                    Config = new CityConfigTestData() {

                    }
                }).SetName("Modified by buildings that provide max hitpoint bonuses").Returns(15);
            }
        }

        private static IEnumerable GetRangedAttackStrengthOfCityTestCases {
            get {
                yield return new TestCaseData(new CityCombatLogicTestData() {
                    City = new CityTestData() {
                        Buildings = new List<BuildingTestData>() { },
                        Population = 0
                    },
                    Config = new CityConfigTestData() {
                        BaseRangedAttackStrength = 10
                    }
                }).SetName("Starts at CityConfig.BaseRangedAttackStrength").Returns(10);

                yield return new TestCaseData(new CityCombatLogicTestData() {
                    City = new CityTestData() {
                        Buildings = new List<BuildingTestData>() { },
                        Population = 4
                    },
                    Config = new CityConfigTestData() {
                        BaseRangedAttackStrength = 10,
                        RangedAttackStrengthPerPopulation = 7
                    }
                }).SetName("Modified by population and CityConfig.RangedAttackStrengthPerPopulation").Returns(38);
            }
        }

        #endregion

        #region instance fields and properties

        private Mock<ICityConfig> MockCityConfig;

        private Mock<IPossessionRelationship<ICity, IBuilding>> MockBuildingPossessionCanon;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockCityConfig              = new Mock<ICityConfig>();
            MockBuildingPossessionCanon = new Mock<IPossessionRelationship<ICity, IBuilding>>();

            Container.Bind<ICityConfig>                              ().FromInstance(MockCityConfig             .Object);
            Container.Bind<IPossessionRelationship<ICity, IBuilding>>().FromInstance(MockBuildingPossessionCanon.Object);

            Container.Bind<CityCombatLogic>().AsSingle();
        }

        #endregion

        #region tests

        [TestCaseSource("GetCombatStrengthOfCityTestCases")]
        [Test(Description = "")]
        public int GetCombatStrengthOfCityTests(CityCombatLogicTestData testData) {
            SetupConfig(testData.Config);

            var city = BuildCity(testData.City);

            var combatLogic = Container.Resolve<CityCombatLogic>();

            return combatLogic.GetCombatStrengthOfCity(city);
        }

        [TestCaseSource("GetMaxHitpointsOfCityTestCases")]
        [Test(Description = "")]
        public int GetMaxHitpointsOfCityTests(CityCombatLogicTestData testData) {
            SetupConfig(testData.Config);

            var city = BuildCity(testData.City);

            var combatLogic = Container.Resolve<CityCombatLogic>();

            return combatLogic.GetMaxHitpointsOfCity(city);
        }

        [TestCaseSource("GetRangedAttackStrengthOfCityTestCases")]
        [Test(Description = "")]
        public int GetRangedAttackStrengthOfCityTests(CityCombatLogicTestData testData) {
            SetupConfig(testData.Config);

            var city = BuildCity(testData.City);

            var combatLogic = Container.Resolve<CityCombatLogic>();

            return combatLogic.GetRangedAttackStrengthOfCity(city);
        }

        #endregion

        #region utilities

        public void SetupConfig(CityConfigTestData data) {
            MockCityConfig.Setup(config => config.BaseCombatStrength         ).Returns(data.BaseCombatStrength);
            MockCityConfig.Setup(config => config.CombatStrengthPerPopulation).Returns(data.CombatStrengthPerPopulation);

            MockCityConfig.Setup(config => config.BaseMaxHitPoints         ).Returns(data.BaseMaxHitpoints);
            MockCityConfig.Setup(config => config.MaxHitPointsPerPopulation).Returns(data.MaxHitPointsPerPopulation);

            MockCityConfig.Setup(config => config.BaseRangedAttackStrength         ).Returns(data.BaseRangedAttackStrength);
            MockCityConfig.Setup(config => config.RangedAttackStrengthPerPopulation).Returns(data.RangedAttackStrengthPerPopulation);
        }

        public IBuilding BuildBuilding(BuildingTestData buildingData) {
            var mockBuilding = new Mock<IBuilding>();

            var mockTemplate = new Mock<IBuildingTemplate>();

            mockTemplate.Setup(template => template.CityCombatStrengthBonus).Returns(buildingData.CityCombatStrengthBonus);
            mockTemplate.Setup(template => template.CityMaxHitpointBonus   ).Returns(buildingData.CityMaxHitpointBonus);

            mockBuilding.Setup(building => building.Template).Returns(mockTemplate.Object);

            return mockBuilding.Object;
        }

        public ICity BuildCity(CityTestData cityData) {
            var mockCity = new Mock<ICity>();

            mockCity.SetupAllProperties();

            var newCity = mockCity.Object;

            newCity.Population = cityData.Population;

            MockBuildingPossessionCanon
                .Setup(canon => canon.GetPossessionsOfOwner(newCity))
                .Returns(cityData.Buildings.Select(buildingData => BuildBuilding(buildingData)));

            return newCity;
        }

        #endregion

        #endregion

    }

}
