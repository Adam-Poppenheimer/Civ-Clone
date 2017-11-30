using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation;
using Assets.Simulation.Civilizations;
using Assets.Simulation.Cities;

namespace Assets.Tests.Simulation.Civilizations {

    [TestFixture]
    public class CivilizationTests : ZenjectUnitTestFixture {

        private Mock<IPossessionRelationship<ICivilization, ICity>> CityCanonMock;

        private List<ICity> AllCities;

        [SetUp]
        public void CommonInstall() {
            AllCities = new List<ICity>();

            CityCanonMock = new Mock<IPossessionRelationship<ICivilization, ICity>>();
            CityCanonMock
                .Setup(canon => canon.GetPossessionsOfOwner(It.IsAny<ICivilization>()))
                .Returns(() => AllCities);

            Container.Bind<IPossessionRelationship<ICivilization, ICity>>().FromInstance(CityCanonMock.Object);

            Container.Bind<ICivilizationConfig>().FromMock();

            Container.Bind<Civilization>().AsSingle();
        }

        [Test(Description = "When PerformIncome is called, Civilization checks CityPossessionCanon " +
            "for all cities belonging to it and increases its gold stockpile based on their last income.")]
        public void PerformIncome_GoldStockpileChanged() {
            var civilization = Container.Resolve<Civilization>();

            CreateCity(new ResourceSummary(gold: 2));
            CreateCity(new ResourceSummary(gold: 5, production: 3));
            CreateCity(new ResourceSummary(gold: 0, culture: 4));

            civilization.PerformIncome();

            Assert.AreEqual(7, civilization.GoldStockpile, "Civilization has an unexpected GoldStockpile");
        }

        [Test(Description = "When PerformIncome is called, Civilization checks CityPossessionCanon " +
            "for all cities belonging to it and increases its culture stockpile based on their last income.")]
        public void PerformIncome_CultureStockpileChanged() {
            var civilization = Container.Resolve<Civilization>();

            CreateCity(new ResourceSummary(gold: 2, culture: 1));
            CreateCity(new ResourceSummary(culture: 2, production: 3));
            CreateCity(new ResourceSummary(gold: 10, culture: 1));

            civilization.PerformIncome();

            Assert.AreEqual(4, civilization.CultureStockpile, "Civilization has an unexpected CultureStockpile");
        }

        private Mock<ICity> CreateCity(ResourceSummary lastIncome) {
            var cityMock = new Mock<ICity>();
            cityMock.Setup(city => city.LastIncome).Returns(lastIncome);

            AllCities.Add(cityMock.Object);
            return cityMock;
        }

    }

}
