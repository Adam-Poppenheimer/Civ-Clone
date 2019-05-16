using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using Zenject;
using Moq;

using Assets.Simulation;
using Assets.Simulation.Civilizations;
using Assets.Simulation.Cities;
using Assets.Simulation.Cities.Buildings;
using Assets.Simulation.SocialPolicies;

namespace Assets.Tests.Simulation.Civilizations {

    public class CivModifier_T_Tests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<ISocialPolicyCanon>                            MockSocialPolicyCanon;
        private Mock<IPossessionRelationship<ICivilization, ICity>> MockCityPossessionCanon;
        private Mock<IPossessionRelationship<ICity, IBuilding>>     MockBuildingPossessionCanon;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockSocialPolicyCanon       = new Mock<ISocialPolicyCanon>();
            MockCityPossessionCanon     = new Mock<IPossessionRelationship<ICivilization, ICity>>();
            MockBuildingPossessionCanon = new Mock<IPossessionRelationship<ICity, IBuilding>>();

            Container.Bind<ISocialPolicyCanon>                           ().FromInstance(MockSocialPolicyCanon      .Object);
            Container.Bind<IPossessionRelationship<ICivilization, ICity>>().FromInstance(MockCityPossessionCanon    .Object);
            Container.Bind<IPossessionRelationship<ICity, IBuilding>>    ().FromInstance(MockBuildingPossessionCanon.Object);
        }

        #endregion

        #region tests

        [Test]
        public void GetValueForCiv_ExtractsFromSocialPolicyBonuses() {
            var civ = BuildCiv();

            Func<ISocialPolicyBonusesData, int> policyExtractor         = bonuses => 1;
            Func<IBuildingTemplate, int>        globalBuildingExtractor = template => 0;
            Func<int, int, int>                 aggregator              = (a, b) => a + b;

            var modifier = new CivModifier<int>(
                new CivModifier<int>.ExtractionData() {
                    PolicyExtractor = policyExtractor, GlobalBuildingExtractor = globalBuildingExtractor,
                    Aggregator = aggregator, UnitaryValue = 1
                }
            );

            MockSocialPolicyCanon.Setup(canon => canon.GetPolicyBonusesForCiv(civ)).Returns(
                new List<ISocialPolicyBonusesData>() {
                    BuildPolicyBonuses(), BuildPolicyBonuses(),
                    BuildPolicyBonuses(),
                }
            );

            Container.Inject(modifier);

            Assert.AreEqual(4, modifier.GetValueForCiv(civ));
        }

        [Test]
        public void GetValueForCiv_ExtractsFromAllBuildingsInAllCities() {
            var civ = BuildCiv(
                BuildCity(BuildBuilding(BuildTemplate())),
                BuildCity(BuildBuilding(BuildTemplate()), BuildBuilding(BuildTemplate())),
                BuildCity(BuildBuilding(BuildTemplate()), BuildBuilding(BuildTemplate()), BuildBuilding(BuildTemplate()))
            );

            Func<ISocialPolicyBonusesData, int> policyExtractor         = bonuses => 0;
            Func<IBuildingTemplate, int>        globalBuildingExtractor = template => 1;
            Func<int, int, int>                 aggregator              = (a, b) => a + b;

            var modifier = new CivModifier<int>(
                new CivModifier<int>.ExtractionData() {
                    PolicyExtractor = policyExtractor,
                    GlobalBuildingExtractor = globalBuildingExtractor,
                    Aggregator = aggregator, UnitaryValue = 1
                }
            );

            Container.Inject(modifier);

            Assert.AreEqual(7, modifier.GetValueForCiv(civ));
        }

        #endregion

        #region utilities

        private ISocialPolicyBonusesData BuildPolicyBonuses() {
            return new Mock<ISocialPolicyBonusesData>().Object;
        }

        private IBuildingTemplate BuildTemplate() {
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

        private ICivilization BuildCiv(params ICity[] cities) {
            var newCiv = new Mock<ICivilization>().Object;

            MockCityPossessionCanon.Setup(canon => canon.GetPossessionsOfOwner(newCiv)).Returns(cities);

            return newCiv;
        }

        #endregion

        #endregion

    }

}
