using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using Zenject;
using Moq;

using Assets.Simulation;
using Assets.Simulation.Cities;
using Assets.Simulation.Cities.Buildings;
using Assets.Simulation.Civilizations;
using Assets.Simulation.SocialPolicies;

namespace Assets.Tests.Simulation.Cities {

    public class CityModifier_T_Tests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<ISocialPolicyCanon>                            MockSocialPolicyCanon;
        private Mock<ICapitalCityCanon>                             MockCapitalCityCanon;
        private Mock<IPossessionRelationship<ICivilization, ICity>> MockCityPossessionCanon;
        private Mock<IPossessionRelationship<ICity, IBuilding>>     MockBuildingPossessionCanon;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockSocialPolicyCanon       = new Mock<ISocialPolicyCanon>();
            MockCapitalCityCanon        = new Mock<ICapitalCityCanon>();
            MockCityPossessionCanon     = new Mock<IPossessionRelationship<ICivilization, ICity>>();
            MockBuildingPossessionCanon = new Mock<IPossessionRelationship<ICity, IBuilding>>();

            Container.Bind<ISocialPolicyCanon>                           ().FromInstance(MockSocialPolicyCanon      .Object);
            Container.Bind<ICapitalCityCanon>                            ().FromInstance(MockCapitalCityCanon       .Object);
            Container.Bind<IPossessionRelationship<ICivilization, ICity>>().FromInstance(MockCityPossessionCanon    .Object);
            Container.Bind<IPossessionRelationship<ICity, IBuilding>>    ().FromInstance(MockBuildingPossessionCanon.Object);
        }

        #endregion

        #region tests

        [Test]
        public void GetModifierForCity_DefaultsToTypeDefault() {
            var city = BuildCity();

            BuildCiv(city);

            var modLogic = new CityModifier<int>(
                new CityModifier<int>.ExtractionData() {
                    PolicyCapitalBonusesExtractor = bonuses => 0, PolicyCityBonusesExtractor = bonuses => 0,
                    Aggregator = (a, b) => a + b, UnitaryValue = 1
                }
            );

            Container.Inject(modLogic);

            Assert.AreEqual(1f, modLogic.GetValueForCity(city));
        }

        [Test]
        public void GetModifierForCity_AndCityCapitalOfItsOwner_AddsSocialPolicyCapitalBonuses() {
            var city = BuildCity();

            var civ = BuildCiv(city, city);

            Func<ISocialPolicyBonusesData, int> capitalExtractor = bonuses => 1;
            Func<ISocialPolicyBonusesData, int> cityExtractor    = bonuses => 0;
            Func<int, int, int>                 aggregator       = (a, b) => a + b;

            var modifier = new CityModifier<int>(
                new CityModifier<int>.ExtractionData() {
                    PolicyCapitalBonusesExtractor = capitalExtractor, PolicyCityBonusesExtractor = cityExtractor,
                    Aggregator = aggregator, UnitaryValue = 1
                }
            );

            MockSocialPolicyCanon.Setup(canon => canon.GetPolicyBonusesForCiv(civ)).Returns(
                new List<ISocialPolicyBonusesData>() {
                    BuildSocialPolicyBonuses(), BuildSocialPolicyBonuses(),
                    BuildSocialPolicyBonuses(),
                }
            );

            Container.Inject(modifier);

            Assert.AreEqual(4, modifier.GetValueForCity(city));
        }

        [Test]
        public void GetModifierForCity_AddsSocialPolicyCityBonuses() {
            var city = BuildCity();

            var civ = BuildCiv(null, city);

            Func<ISocialPolicyBonusesData, int> capitalExtractor = bonuses => 0;
            Func<ISocialPolicyBonusesData, int> cityExtractor    = bonuses => 1;
            Func<int, int, int>                 aggregator       = (a, b) => a + b;

            var modifier = new CityModifier<int>(
                new CityModifier<int>.ExtractionData() {
                    PolicyCapitalBonusesExtractor = capitalExtractor, PolicyCityBonusesExtractor = cityExtractor,
                    Aggregator = aggregator, UnitaryValue = 1
                }
            );

            MockSocialPolicyCanon.Setup(canon => canon.GetPolicyBonusesForCiv(civ)).Returns(
                new List<ISocialPolicyBonusesData>() {
                    BuildSocialPolicyBonuses(), BuildSocialPolicyBonuses(),
                    BuildSocialPolicyBonuses(),
                }
            );

            Container.Inject(modifier);

            Assert.AreEqual(4, modifier.GetValueForCity(city));
        }

        [Test]
        public void GetModifierForCity_GetsLocalBuildingBonusesFromBuildingsInCity() {
            var templateOne   = BuildBuildingTemplate();
            var templateTwo   = BuildBuildingTemplate();
            var templateThree = BuildBuildingTemplate();

            var city = BuildCity(
                BuildBuilding(templateOne), BuildBuilding(templateTwo), BuildBuilding(templateThree)
            );

            Func<ISocialPolicyBonusesData, int> capitalExtractor        = bonuses => 0;
            Func<ISocialPolicyBonusesData, int> cityExtractor           = bonuses => 0;
            Func<IBuildingTemplate, int>        localBuildingExtractor  = template => 1;
            Func<IBuildingTemplate, int>        globalBuildingExtractor = null;
            Func<int, int, int>                 aggregator              = (a, b) => a + b;

            var modifier = new CityModifier<int>(
                new CityModifier<int>.ExtractionData() {
                    PolicyCapitalBonusesExtractor = capitalExtractor,
                    PolicyCityBonusesExtractor = cityExtractor,
                    BuildingLocalBonusesExtractor = localBuildingExtractor,
                    BuildingGlobalBonusesExtractor = globalBuildingExtractor,
                    Aggregator = aggregator, UnitaryValue = 1
                }
            );

            Container.Inject(modifier);

            Assert.AreEqual(4, modifier.GetValueForCity(city));
        }

        [Test]
        public void GetModifierForCity_GetsGlobalBuildingBonusesFromAllBuildingsOfAllOwnersCities() {
            var templateOne   = BuildBuildingTemplate();
            var templateTwo   = BuildBuildingTemplate();
            var templateThree = BuildBuildingTemplate();
            var templateFour  = BuildBuildingTemplate();

            var cityOne   = BuildCity(BuildBuilding(templateOne));
            var cityTwo   = BuildCity(BuildBuilding(templateTwo));
            var cityThree = BuildCity(BuildBuilding(templateThree), BuildBuilding(templateFour));

            BuildCiv(null, cityOne, cityTwo, cityThree);

            Func<ISocialPolicyBonusesData, int> capitalExtractor        = bonuses => 0;
            Func<ISocialPolicyBonusesData, int> cityExtractor           = bonuses => 0;
            Func<IBuildingTemplate, int>        localBuildingExtractor  = null;
            Func<IBuildingTemplate, int>        globalBuildingExtractor = template => 20;
            Func<int, int, int>                 aggregator              = (a, b) => a + b;

            var modifier = new CityModifier<int>(
                new CityModifier<int>.ExtractionData() {
                    PolicyCapitalBonusesExtractor = capitalExtractor,
                    PolicyCityBonusesExtractor = cityExtractor,
                    BuildingLocalBonusesExtractor = localBuildingExtractor,
                    BuildingGlobalBonusesExtractor = globalBuildingExtractor,
                    Aggregator = aggregator, UnitaryValue = 1
                }
            );

            Container.Inject(modifier);

            Assert.AreEqual(81, modifier.GetValueForCity(cityOne));
            Assert.AreEqual(81, modifier.GetValueForCity(cityTwo));
        }

        #endregion

        #region utilities

        private IBuildingTemplate BuildBuildingTemplate() {
            return new Mock<IBuildingTemplate>().Object;
        }

        private IBuilding BuildBuilding(IBuildingTemplate template) {
            var mockBuilding = new Mock<IBuilding>();

            mockBuilding.Setup(building => building.Template).Returns(template);

            return mockBuilding.Object;
        }

        private ICity BuildCity(params IBuilding[] buildings) {
            var newCity = new Mock<ICity>().Object;

            MockBuildingPossessionCanon.Setup(canon => canon.GetPossessionsOfOwner(newCity)).Returns(buildings);

            return newCity;
        }

        private ICivilization BuildCiv(ICity capital, params ICity[] cities) {
            var newCiv = new Mock<ICivilization>().Object;

            MockCapitalCityCanon.Setup(canon => canon.GetCapitalOfCiv(newCiv)).Returns(capital);

            MockCityPossessionCanon.Setup(canon => canon.GetOwnerOfPossession(capital)).Returns(newCiv);

            foreach(var city in cities) {
                MockCityPossessionCanon.Setup(canon => canon.GetOwnerOfPossession(city)).Returns(newCiv);
            }

            MockCityPossessionCanon.Setup(canon => canon.GetPossessionsOfOwner(newCiv))
                                   .Returns(cities.Concat(new ICity[] { capital }));

            return newCiv;
        }

        private ISocialPolicyBonusesData BuildSocialPolicyBonuses() {
            return new Mock<ISocialPolicyBonusesData>().Object;
        }

        #endregion

        #endregion

    }

}
