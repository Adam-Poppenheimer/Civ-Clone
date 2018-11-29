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
using Assets.Simulation.Cities.Territory;
using Assets.Simulation.Civilizations;
using Assets.Simulation.SocialPolicies;

namespace Assets.Tests.Simulation.Cities {

    public class BorderExpansionModifierLogicTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IPossessionRelationship<ICity, IBuilding>>     MockBuildingPossessionCanon;
        private Mock<IPossessionRelationship<ICivilization, ICity>> MockCityPossessionCanon;
        private Mock<ISocialPolicyCanon>                            MockSocialPolicyCanon;
        private Mock<ICapitalCityCanon>                             MockCapitalCityCanon;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockBuildingPossessionCanon = new Mock<IPossessionRelationship<ICity, IBuilding>>();
            MockCityPossessionCanon     = new Mock<IPossessionRelationship<ICivilization, ICity>>();
            MockSocialPolicyCanon       = new Mock<ISocialPolicyCanon>();
            MockCapitalCityCanon        = new Mock<ICapitalCityCanon>();

            Container.Bind<IPossessionRelationship<ICity, IBuilding>>    ().FromInstance(MockBuildingPossessionCanon.Object);
            Container.Bind<IPossessionRelationship<ICivilization, ICity>>().FromInstance(MockCityPossessionCanon    .Object);
            Container.Bind<ISocialPolicyCanon>                           ().FromInstance(MockSocialPolicyCanon      .Object);
            Container.Bind<ICapitalCityCanon>                            ().FromInstance(MockCapitalCityCanon       .Object);

            Container.Bind<BorderExpansionModifierLogic>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void GetBorderExpansionModifierForCity_DefaultsToOne() {
            var city = BuildCity();

            BuildCiv(new List<ICity>() { city }, city, new List<ISocialPolicyBonusesData>());

            var modifierLogic = Container.Resolve<BorderExpansionModifierLogic>();

            Assert.AreEqual(1f, modifierLogic.GetBorderExpansionModifierForCity(city));
        }

        [Test]
        public void GetBorderExpansionModifierForCity_IncludesLocalModifierForBuildingsInCity() {
            var city = BuildCity(
                BuildBuilding(BuildBuildingTemplate(localExpansionModifier: 2f)),
                BuildBuilding(BuildBuildingTemplate(localExpansionModifier: 5f)),
                BuildBuilding(BuildBuildingTemplate(localExpansionModifier: -1f)),
                BuildBuilding(BuildBuildingTemplate(localExpansionModifier: 0f))
            );

            BuildCiv(new List<ICity>() { city }, city, new List<ISocialPolicyBonusesData>());

            var modifierLogic = Container.Resolve<BorderExpansionModifierLogic>();

            Assert.AreEqual(7f, modifierLogic.GetBorderExpansionModifierForCity(city));
        }

        [Test]
        public void GetBorderExpansionModifierForCity_IncludesGlobalModifierForBuildingsInOwnersCities() {
            var cityOne = BuildCity(
                BuildBuilding(BuildBuildingTemplate(globalExpansionModifier: 1f))
            );

            var cityTwo = BuildCity(
                BuildBuilding(BuildBuildingTemplate(globalExpansionModifier: 2f))
            );

            var cityThree = BuildCity(
                BuildBuilding(BuildBuildingTemplate(globalExpansionModifier: 3f)),
                BuildBuilding(BuildBuildingTemplate(globalExpansionModifier: -1f))
            );

            BuildCiv(new List<ICity>() { cityOne, cityTwo, cityThree }, cityOne, new List<ISocialPolicyBonusesData>());

            var modifierLogic = Container.Resolve<BorderExpansionModifierLogic>();

            Assert.AreEqual(6f, modifierLogic.GetBorderExpansionModifierForCity(cityOne));
        }

        [Test]
        public void GetBorderExpansionModifierForCity_IncludesCityModifierForPolicyBonusesOfOwner() {
            var city = BuildCity();

            BuildCiv(
                new List<ICity>() { city }, city,
                new List<ISocialPolicyBonusesData>() {
                    BuildPolicyBonuses(cityBorderExpansionModifier: 1f),
                    BuildPolicyBonuses(cityBorderExpansionModifier: 2f),
                    BuildPolicyBonuses(cityBorderExpansionModifier: 3f),
                    BuildPolicyBonuses(cityBorderExpansionModifier: -1f),
                }
            );

            var modifierLogic = Container.Resolve<BorderExpansionModifierLogic>();

            Assert.AreEqual(6f, modifierLogic.GetBorderExpansionModifierForCity(city));
        }

        [Test]
        public void GetBorderExpansionModifierForCity_AndCityCapital_IncludesCapitalModifierForPolicyBonusesOfOwner() {
            var city = BuildCity();

            BuildCiv(
                new List<ICity>() { city }, city,
                new List<ISocialPolicyBonusesData>() {
                    BuildPolicyBonuses(capitalBorderExpansionModifier: 1f),
                    BuildPolicyBonuses(capitalBorderExpansionModifier: 2f),
                    BuildPolicyBonuses(capitalBorderExpansionModifier: 3f),
                    BuildPolicyBonuses(capitalBorderExpansionModifier: -1f),
                }
            );

            var modifierLogic = Container.Resolve<BorderExpansionModifierLogic>();

            Assert.AreEqual(6f, modifierLogic.GetBorderExpansionModifierForCity(city));
        }

        [Test]
        public void GetBorderExpansionModifierForCity_AndCityNotCapital_DoesNotIncludeCapitalModifierForPolicyBonusesOfOwner() {
            var city = BuildCity();

            var capital = BuildCity();

            BuildCiv(
                new List<ICity>() { city }, capital,
                new List<ISocialPolicyBonusesData>() {
                    BuildPolicyBonuses(capitalBorderExpansionModifier: 1f),
                    BuildPolicyBonuses(capitalBorderExpansionModifier: 2f),
                    BuildPolicyBonuses(capitalBorderExpansionModifier: 3f),
                    BuildPolicyBonuses(capitalBorderExpansionModifier: -1f),
                }
            );

            var modifierLogic = Container.Resolve<BorderExpansionModifierLogic>();

            Assert.AreEqual(1f, modifierLogic.GetBorderExpansionModifierForCity(city));
        }

        #endregion

        #region utilities

        private ISocialPolicyBonusesData BuildPolicyBonuses(
            float cityBorderExpansionModifier = 0f, float capitalBorderExpansionModifier = 0f
        ) {
            var mockBonuses = new Mock<ISocialPolicyBonusesData>();

            mockBonuses.Setup(bonuses => bonuses.CapitalBorderExpansionModifier).Returns(capitalBorderExpansionModifier);
            mockBonuses.Setup(bonuses => bonuses.CityBorderExpansionModifier)   .Returns(cityBorderExpansionModifier);

            return mockBonuses.Object;
        }

        private IBuildingTemplate BuildBuildingTemplate(
            float localExpansionModifier = 0f, float globalExpansionModifier = 0f
        ) {
            var mockTemplate = new Mock<IBuildingTemplate>();

            mockTemplate.Setup(template => template.LocalBorderExpansionModifier) .Returns(localExpansionModifier);
            mockTemplate.Setup(template => template.GlobalBorderExpansionModifier).Returns(globalExpansionModifier);

            return mockTemplate.Object;
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

        private ICivilization BuildCiv(
            List<ICity> cities, ICity capital, List<ISocialPolicyBonusesData> policyBonuses
        ) {
            var newCiv = new Mock<ICivilization>().Object;

            MockCityPossessionCanon.Setup(canon => canon.GetPossessionsOfOwner(newCiv)).Returns(cities);

            foreach(var city in cities) {
                MockCityPossessionCanon.Setup(canon => canon.GetOwnerOfPossession(city)).Returns(newCiv);
            }

            MockCapitalCityCanon.Setup(canon => canon.GetCapitalOfCiv(newCiv)).Returns(capital);

            MockSocialPolicyCanon.Setup(canon => canon.GetPolicyBonusesForCiv(newCiv)).Returns(policyBonuses);
            
            return newCiv;
        }

        #endregion

        #endregion

    }

}
