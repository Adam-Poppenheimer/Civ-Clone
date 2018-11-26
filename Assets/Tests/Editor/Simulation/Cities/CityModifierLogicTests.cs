using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using Zenject;
using Moq;

using Assets.Simulation;
using Assets.Simulation.Cities;
using Assets.Simulation.Civilizations;
using Assets.Simulation.SocialPolicies;
using Assets.Simulation.Modifiers;

namespace Assets.Tests.Simulation.Cities {

    public class CityModifierLogicTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<ISocialPolicyBonusLogic>                       MockSocialPolicyBonusLogic;
        private Mock<ICapitalCityCanon>                             MockCapitalCityCanon;
        private Mock<IPossessionRelationship<ICivilization, ICity>> MockCityPossessionCanon;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockSocialPolicyBonusLogic = new Mock<ISocialPolicyBonusLogic>();
            MockCapitalCityCanon       = new Mock<ICapitalCityCanon>();
            MockCityPossessionCanon    = new Mock<IPossessionRelationship<ICivilization, ICity>>();

            Container.Bind<ISocialPolicyBonusLogic>                      ().FromInstance(MockSocialPolicyBonusLogic.Object);
            Container.Bind<ICapitalCityCanon>                            ().FromInstance(MockCapitalCityCanon      .Object);
            Container.Bind<IPossessionRelationship<ICivilization, ICity>>().FromInstance(MockCityPossessionCanon   .Object);
        }

        #endregion

        #region tests

        [Test]
        public void GetModifierForCity_DefaultsToTypeDefault() {
            var city = BuildCity();

            BuildCiv(city);

            var modLogic = new CityModifier<int>(new CityModifier<int>.ExtractionData() {
                CapitalBonusesExtractor = bonuses => 0, CityBonusesExtractor = bonuses => 0,
                Aggregator = (a, b) => a + b, UnitaryValue = 1
            });

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

            var modLogic = new CityModifier<int>(new CityModifier<int>.ExtractionData() {
                CapitalBonusesExtractor = capitalExtractor, CityBonusesExtractor = cityExtractor,
                Aggregator = aggregator, UnitaryValue = 1
            });

            MockSocialPolicyBonusLogic.Setup(logic => logic.ExtractBonusFromCiv(civ, capitalExtractor, aggregator))
                                      .Returns(300);

            Container.Inject(modLogic);

            Assert.AreEqual(301, modLogic.GetValueForCity(city));
        }

        [Test]
        public void GetModifierForCity_AddsSocialPolicyCityBonuses() {
            var city = BuildCity();

            var civ = BuildCiv(null, city);

            Func<ISocialPolicyBonusesData, int> capitalExtractor = bonuses => 0;
            Func<ISocialPolicyBonusesData, int> cityExtractor    = bonuses => 0;
            Func<int, int, int>                 aggregator       = (a, b) => a + b;

            var modLogic = new CityModifier<int>(new CityModifier<int>.ExtractionData() {
                CapitalBonusesExtractor = capitalExtractor, CityBonusesExtractor = cityExtractor,
                Aggregator = aggregator, UnitaryValue = 1
            });

            MockSocialPolicyBonusLogic.Setup(logic => logic.ExtractBonusFromCiv(civ, cityExtractor, aggregator))
                                      .Returns(300);

            Container.Inject(modLogic);

            Assert.AreEqual(301, modLogic.GetValueForCity(city));
        }

        #endregion

        #region utilities

        private ICity BuildCity() {
            return new Mock<ICity>().Object;
        }

        private ICivilization BuildCiv(ICity capital, params ICity[] cities) {
            var newCiv = new Mock<ICivilization>().Object;

            MockCapitalCityCanon.Setup(canon => canon.GetCapitalOfCiv(newCiv)).Returns(capital);

            MockCityPossessionCanon.Setup(canon => canon.GetOwnerOfPossession(capital)).Returns(newCiv);

            foreach(var city in cities) {
                MockCityPossessionCanon.Setup(canon => canon.GetOwnerOfPossession(city)).Returns(newCiv);
            }

            return newCiv;
        }

        #endregion

        #endregion

    }

}
