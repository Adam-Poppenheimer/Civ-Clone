using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using Zenject;
using Moq;

using Assets.Simulation;
using Assets.Simulation.Units;
using Assets.Simulation.Units.Combat;
using Assets.Simulation.Civilizations;
using Assets.Simulation.Cities;

namespace Assets.Tests.Simulation.Units.Combat {

    public class CitySackResponderTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IPossessionRelationship<ICivilization, IUnit>> MockUnitPossessionCanon;
        private Mock<ICityFactory>                                  MockCityFactory;
        private Mock<IPossessionRelationship<ICivilization, ICity>> MockCityPossessionCanon;

        private List<ICity> AllCities = new List<ICity>();

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            AllCities.Clear();

            MockUnitPossessionCanon = new Mock<IPossessionRelationship<ICivilization, IUnit>>();
            MockCityFactory         = new Mock<ICityFactory>();
            MockCityPossessionCanon = new Mock<IPossessionRelationship<ICivilization, ICity>>();

            MockCityFactory.Setup(factory => factory.AllCities).Returns(AllCities.AsReadOnly());

            Container.Bind<IPossessionRelationship<ICivilization, IUnit>>().FromInstance(MockUnitPossessionCanon.Object);
            Container.Bind<ICityFactory>                                 ().FromInstance(MockCityFactory        .Object);
            Container.Bind<IPossessionRelationship<ICivilization, ICity>>().FromInstance(MockCityPossessionCanon.Object);

            Container.Bind<CitySackResponder>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void RespondToCombat_DoesNothingIfDefenderStillHasHitpoints() {
            var attacker = BuildUnit(100, UnitType.Melee);
            var defender = BuildUnit(1, UnitType.City);

            var cityBeingAttacked = BuildCity(10, defender);

            BuildCiv(100, new List<ICity>(), BuildCivTemplate(true), attacker);

            var defendingCiv = BuildCiv(
                100, new List<ICity>() { cityBeingAttacked }, BuildCivTemplate(false), defender
            );

            var combatInfo = new CombatInfo() { CombatType = CombatType.Melee };

            var responder = Container.Resolve<CitySackResponder>();

            responder.RespondToCombat(attacker, defender, combatInfo);

            Assert.AreEqual(100, defendingCiv.GoldStockpile);
        }

        [Test]
        public void RespondToCombat_DoesNothingIfDefenderNotACity() {
            var attacker = BuildUnit(100, UnitType.Melee);
            var defender = BuildUnit(0, UnitType.Melee);

            var cityBeingAttacked = BuildCity(10, defender);

            BuildCiv(100, new List<ICity>(), BuildCivTemplate(true), attacker);

            var defendingCiv = BuildCiv(
                100, new List<ICity>() { cityBeingAttacked }, BuildCivTemplate(false), defender
            );

            var combatInfo = new CombatInfo() { CombatType = CombatType.Melee };

            var responder = Container.Resolve<CitySackResponder>();

            responder.RespondToCombat(attacker, defender, combatInfo);

            Assert.AreEqual(100, defendingCiv.GoldStockpile);
        }

        [Test]
        public void RespondToCombat_DoesNothingIfCombatTypeNotMelee() {
            var attacker = BuildUnit(100, UnitType.Melee);
            var defender = BuildUnit(0, UnitType.City);

            var cityBeingAttacked = BuildCity(10, defender);

            BuildCiv(100, new List<ICity>(), BuildCivTemplate(true), attacker);

            var defendingCiv = BuildCiv(
                100, new List<ICity>() { cityBeingAttacked }, BuildCivTemplate(false), defender
            );

            var combatInfo = new CombatInfo() { CombatType = CombatType.Ranged };

            var responder = Container.Resolve<CitySackResponder>();

            responder.RespondToCombat(attacker, defender, combatInfo);

            Assert.AreEqual(100, defendingCiv.GoldStockpile);
        }

        [Test]
        public void RespondToCombat_DoesNothingIfAttackerOwnerNotBarbaric() {
            var attacker = BuildUnit(100, UnitType.Melee);
            var defender = BuildUnit(0, UnitType.City);

            var cityBeingAttacked = BuildCity(10, defender);

            BuildCiv(100, new List<ICity>(), BuildCivTemplate(false), attacker);

            var defendingCiv = BuildCiv(
                100, new List<ICity>() { cityBeingAttacked }, BuildCivTemplate(false), defender
            );

            var combatInfo = new CombatInfo() { CombatType = CombatType.Melee };

            var responder = Container.Resolve<CitySackResponder>();

            responder.RespondToCombat(attacker, defender, combatInfo);

            Assert.AreEqual(100, defendingCiv.GoldStockpile);
        }

        [Test]
        public void RespondToCombat_CityOwnerLosesGold_BasedOnPercentageOfOwnersPopulationInCity() {
            var attacker = BuildUnit(100, UnitType.Melee);
            var defender = BuildUnit(0, UnitType.City);

            var cityBeingAttacked = BuildCity(10, defender);

            var cities = new List<ICity>() {
                cityBeingAttacked, BuildCity(5, null), BuildCity(10, null)
            };

            BuildCiv(100, new List<ICity>(), BuildCivTemplate(true), attacker);

            var defendingCiv = BuildCiv(100, cities, BuildCivTemplate(false), defender);

            var combatInfo = new CombatInfo() { CombatType = CombatType.Melee };

            var responder = Container.Resolve<CitySackResponder>();

            responder.RespondToCombat(attacker, defender, combatInfo);

            Assert.AreEqual(60, defendingCiv.GoldStockpile);
        }

        [Test]
        public void RespondToCombat_CityOwnerCannotLoseNegativeGold() {
            var attacker = BuildUnit(100, UnitType.Melee);
            var defender = BuildUnit(0, UnitType.City);

            var cityBeingAttacked = BuildCity(-10, defender);

            var cities = new List<ICity>() {
                cityBeingAttacked, BuildCity(100, null)
            };

            BuildCiv(100, new List<ICity>(), BuildCivTemplate(true), attacker);

            var defendingCiv = BuildCiv(100, cities, BuildCivTemplate(false), defender);

            var combatInfo = new CombatInfo() { CombatType = CombatType.Melee };

            var responder = Container.Resolve<CitySackResponder>();

            responder.RespondToCombat(attacker, defender, combatInfo);

            Assert.AreEqual(100, defendingCiv.GoldStockpile);
        }

        #endregion

        #region utilites

        private ICity BuildCity(int population, IUnit combatFacade) {
            var mockCity = new Mock<ICity>();

            mockCity.Setup(city => city.Population)  .Returns(population);
            mockCity.Setup(city => city.CombatFacade).Returns(combatFacade);

            var newCity = mockCity.Object;

            AllCities.Add(newCity);

            return mockCity.Object;
        }

        private ICivilizationTemplate BuildCivTemplate(bool isBarbaric) {
            var mockTemplate = new Mock<ICivilizationTemplate>();

            mockTemplate.Setup(template => template.IsBarbaric).Returns(isBarbaric);

            return mockTemplate.Object;
        }

        private ICivilization BuildCiv(
            int goldStockpile, IEnumerable<ICity> cities, ICivilizationTemplate template, params IUnit[] units
        ) {
            var mockCiv = new Mock<ICivilization>();

            mockCiv.SetupAllProperties();
            mockCiv.Setup(civ => civ.Template).Returns(template);

            var newCiv = mockCiv.Object;

            newCiv.GoldStockpile = goldStockpile;

            foreach(var city in cities) {
                MockCityPossessionCanon.Setup(canon => canon.GetOwnerOfPossession(city)).Returns(newCiv);
            }

            MockCityPossessionCanon.Setup(canon => canon.GetPossessionsOfOwner(newCiv)).Returns(cities);

            foreach(var unit in units) {
                MockUnitPossessionCanon.Setup(canon => canon.GetOwnerOfPossession(unit)).Returns(newCiv);
            }

            return newCiv;
        }

        private IUnit BuildUnit(int currentHitpoints, UnitType type) {
            var mockUnit = new Mock<IUnit>();

            mockUnit.Setup(unit => unit.CurrentHitpoints).Returns(currentHitpoints);
            mockUnit.Setup(unit => unit.Type)            .Returns(type);

            return mockUnit.Object;
        }

        #endregion

        #endregion

    }

}
