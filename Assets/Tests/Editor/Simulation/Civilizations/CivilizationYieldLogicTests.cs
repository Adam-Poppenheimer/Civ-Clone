using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation;
using Assets.Simulation.Civilizations;
using Assets.Simulation.Cities;
using Assets.Simulation.Cities.ResourceGeneration;

namespace Assets.Tests.Simulation.Civilizations {

    [TestFixture]
    public class CivilizationYieldLogicTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IPossessionRelationship<ICivilization, ICity>> MockCityPossessionCanon;
        private Mock<IYieldGenerationLogic>                         MockGenerationLogic;
        private Mock<IUnitMaintenanceLogic>                         MockUnitMaintenanceLogic;

        private List<ICity> AllCities = new List<ICity>();

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            AllCities.Clear();

            MockCityPossessionCanon  = new Mock<IPossessionRelationship<ICivilization, ICity>>();
            MockGenerationLogic      = new Mock<IYieldGenerationLogic>();
            MockUnitMaintenanceLogic = new Mock<IUnitMaintenanceLogic>();

            MockCityPossessionCanon.Setup(canon => canon.GetPossessionsOfOwner(It.IsAny<ICivilization>()))
                .Returns(AllCities);

            Container.Bind<IPossessionRelationship<ICivilization, ICity>>().FromInstance(MockCityPossessionCanon .Object);
            Container.Bind<IYieldGenerationLogic>                        ().FromInstance(MockGenerationLogic     .Object);
            Container.Bind<IUnitMaintenanceLogic>                        ().FromInstance(MockUnitMaintenanceLogic.Object);

            Container.Bind<CivilizationYieldLogic>().AsSingle();
        }

        #endregion

        #region tests

        [Test(Description = "GetYieldOfCivilization should sum the total yields of all " +
            "cities bestowed to it by CityPossessionCanon. Total yields should be " +
            "determined by IResourceGenerationLogic")]
        public void GetYieldOfCivilization_SumsOwnedCityYields() {
            BuildCity(new YieldSummary(food: 1f));
            BuildCity(new YieldSummary(gold: 2f));
            BuildCity(new YieldSummary(culture: 3f));

            var civilization = BuildCivilization();

            var yieldLogic = Container.Resolve<CivilizationYieldLogic>();

            Assert.AreEqual(
                new YieldSummary(food: 1f, gold: 2f, culture: 3f),
                yieldLogic.GetYieldOfCivilization(civilization),
                "GetYieldOfCivilization returned an unexpected value"
            );
        }

        [Test]
        public void GetYieldOfCivilization_SubtractsUnitMaintenanceFromGold() {
            BuildCity(new YieldSummary(food: 1f));
            BuildCity(new YieldSummary(gold: 2f));
            BuildCity(new YieldSummary(culture: 3f));

            var civilization = BuildCivilization();

            MockUnitMaintenanceLogic.Setup(logic => logic.GetMaintenanceOfUnitsForCiv(civilization)).Returns(20f);

            var yieldLogic = Container.Resolve<CivilizationYieldLogic>();

            Assert.AreEqual(
                new YieldSummary(food: 1f, gold: -18f, culture: 3f),
                yieldLogic.GetYieldOfCivilization(civilization),
                "GetYieldOfCivilization returned an unexpected value"
            );
        }

        #endregion

        #region utilities

        private ICity BuildCity(YieldSummary yield) {
            var mockCity = new Mock<ICity>();

            var newCity = mockCity.Object;

            MockGenerationLogic.Setup(logic => logic.GetTotalYieldForCity(newCity)).Returns(yield);
            AllCities.Add(newCity);

            return newCity;
        }

        private ICivilization BuildCivilization() {
            return new Mock<ICivilization>().Object;
        }

        #endregion

        #endregion

    }

}
