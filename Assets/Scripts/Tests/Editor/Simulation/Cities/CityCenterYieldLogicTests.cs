﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using Zenject;
using Moq;

using Assets.Simulation;
using Assets.Simulation.Cities;
using Assets.Simulation.Cities.ResourceGeneration;
using Assets.Simulation.Civilizations;
using Assets.Simulation.Units;

namespace Assets.Tests.Simulation.Cities {

    public class CityCenterYieldLogicTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IIncomeModifierLogic>                          MockIncomeModifierLogic;
        private Mock<ICityConfig>                                   MockCityConfig;
        private Mock<IPossessionRelationship<ICivilization, ICity>> MockCityPossessionCanon;
        private Mock<IUnitGarrisonLogic>                            MockUnitGarrisonLogic;
        private Mock<ICityModifiers>                                MockCityModifiers;

        private Mock<ICityModifier<YieldSummary>> MockBonusYieldModifier;
        private Mock<ICityModifier<YieldSummary>> MockGarrisonedYieldModifier;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockIncomeModifierLogic = new Mock<IIncomeModifierLogic>();
            MockCityConfig          = new Mock<ICityConfig>();
            MockCityPossessionCanon = new Mock<IPossessionRelationship<ICivilization, ICity>>();
            MockUnitGarrisonLogic   = new Mock<IUnitGarrisonLogic>();
            MockCityModifiers       = new Mock<ICityModifiers>();

            MockBonusYieldModifier      = new Mock<ICityModifier<YieldSummary>>();
            MockGarrisonedYieldModifier = new Mock<ICityModifier<YieldSummary>>();

            MockCityModifiers.Setup(modifiers => modifiers.BonusYield)         .Returns(MockBonusYieldModifier     .Object);
            MockCityModifiers.Setup(modifiers => modifiers.GarrisonedYield).Returns(MockGarrisonedYieldModifier.Object);

            Container.Bind<IIncomeModifierLogic>                         ().FromInstance(MockIncomeModifierLogic.Object);
            Container.Bind<ICityConfig>                                  ().FromInstance(MockCityConfig         .Object);
            Container.Bind<IPossessionRelationship<ICivilization, ICity>>().FromInstance(MockCityPossessionCanon.Object);
            Container.Bind<IUnitGarrisonLogic>                           ().FromInstance(MockUnitGarrisonLogic  .Object);
            Container.Bind<ICityModifiers>                               ().FromInstance(MockCityModifiers      .Object);

            Container.Bind<CityCenterYieldLogic>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void GetYieldOfCityCenter_DefaultsToConfiguredCityCenterYield() {
            MockCityConfig.Setup(config => config.CityCenterBaseYield)
                          .Returns(new YieldSummary(food: 1f, gold: 2f));

            var city = BuildCity(0);

            BuildCiv(new List<ICity>() { city });

            var yieldLogic = Container.Resolve<CityCenterYieldLogic>();

            Assert.AreEqual(new YieldSummary(food: 1f, gold: 2f), yieldLogic.GetYieldOfCityCenter(city));
        }

        [Test]
        public void GetYieldOfCityCenter_IncludesOneSciencePerCityPopulation() {
            MockCityConfig.Setup(config => config.CityCenterBaseYield).Returns(new YieldSummary());

            var city = BuildCity(15);

            BuildCiv(new List<ICity>() { city });

            var yieldLogic = Container.Resolve<CityCenterYieldLogic>();

            Assert.AreEqual(new YieldSummary(science: 15), yieldLogic.GetYieldOfCityCenter(city));
        }

        [Test]
        public void GetYieldOfCityCenter_IncludesCityPolicyBonuses() {
            MockCityConfig.Setup(config => config.CityCenterBaseYield).Returns(new YieldSummary());

            var city = BuildCity(0);

            BuildCiv(new List<ICity>() { city });

            MockBonusYieldModifier.Setup(modifier => modifier.GetValueForCity(city))
                                  .Returns(new YieldSummary(gold: 12f));

            var yieldLogic = Container.Resolve<CityCenterYieldLogic>();

            Assert.AreEqual(new YieldSummary(gold: 12f), yieldLogic.GetYieldOfCityCenter(city));
        }

        [Test]
        public void GetYieldOfCityCenter_IncludesGarrisonedYieldIfCityGarrisoned() {
            MockCityConfig.Setup(config => config.CityCenterBaseYield).Returns(new YieldSummary());

            var city = BuildCity(0);

            BuildCiv(new List<ICity>() { city });

            MockGarrisonedYieldModifier.Setup(modifier => modifier.GetValueForCity(city))
                                       .Returns(new YieldSummary(gold: 12f));

            MockUnitGarrisonLogic.Setup(logic => logic.IsCityGarrisoned(city)).Returns(true);

            var yieldLogic = Container.Resolve<CityCenterYieldLogic>();

            Assert.AreEqual(new YieldSummary(gold: 12f), yieldLogic.GetYieldOfCityCenter(city));
        }

        [Test]
        public void GetYieldOfCityCenter_IgnoresGarrisonedYieldIfCityNotGarrisoned() {
            MockCityConfig.Setup(config => config.CityCenterBaseYield).Returns(new YieldSummary());

            var city = BuildCity(0);

            BuildCiv(new List<ICity>() { city });

            MockGarrisonedYieldModifier.Setup(modifier => modifier.GetValueForCity(city))
                                       .Returns(new YieldSummary(gold: 12f));

            MockUnitGarrisonLogic.Setup(logic => logic.IsCityGarrisoned(city)).Returns(false);

            var yieldLogic = Container.Resolve<CityCenterYieldLogic>();

            Assert.AreEqual(YieldSummary.Empty, yieldLogic.GetYieldOfCityCenter(city));
        }

        [Test]
        public void GetYieldOfCityCenter_ModifiedByCityMultiplers() {
            MockCityConfig.Setup(config => config.CityCenterBaseYield).Returns(new YieldSummary());

            var city = BuildCity(15);

            BuildCiv(new List<ICity>() { city });

            MockIncomeModifierLogic.Setup(logic => logic.GetYieldMultipliersForCity(city)).Returns(
                new YieldSummary(science: 1.5f)
            );

            var yieldLogic = Container.Resolve<CityCenterYieldLogic>();

            Assert.AreEqual(new YieldSummary(science: 15 * 2.5f), yieldLogic.GetYieldOfCityCenter(city));
        }

        [Test]
        public void GetYieldOfCityCenter_ModifiedByCivMultipliers() {
            MockCityConfig.Setup(config => config.CityCenterBaseYield).Returns(new YieldSummary());

            var city = BuildCity(15);

            var civ = BuildCiv(new List<ICity>() { city });

            MockIncomeModifierLogic.Setup(logic => logic.GetYieldMultipliersForCivilization(civ)).Returns(
                new YieldSummary(science: 2.5f)
            );

            var yieldLogic = Container.Resolve<CityCenterYieldLogic>();

            Assert.AreEqual(new YieldSummary(science: 15 * 3.5f), yieldLogic.GetYieldOfCityCenter(city));
        }

        #endregion

        #region utilities

        private ICity BuildCity(int population) {
            var mockCity = new Mock<ICity>();

            mockCity.Setup(city => city.Population).Returns(population);

            return mockCity.Object;
        }

        private ICivilization BuildCiv(List<ICity> cities) {
            var newCiv = new Mock<ICivilization>().Object;

            foreach(var city in cities) {
                MockCityPossessionCanon.Setup(canon => canon.GetOwnerOfPossession(city)).Returns(newCiv);
            }

            return newCiv;
        }

        #endregion

        #endregion

    }

}
