using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation;
using Assets.Simulation.Cities;
using Assets.Simulation.Cities.Buildings;
using Assets.Simulation.Units;
using Assets.Simulation.Units.Promotions;

namespace Assets.Tests.Simulation.Units.Promotions {

    [TestFixture]
    public class StartingExperienceLogicTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IPossessionRelationship<ICity, IBuilding>> MockBuildingPossessionCanon;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockBuildingPossessionCanon = new Mock<IPossessionRelationship<ICity, IBuilding>>();

            Container.Bind<IPossessionRelationship<ICity, IBuilding>>()
                     .FromInstance(MockBuildingPossessionCanon.Object);

            Container.Bind<StartingExperienceLogic>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void GetStartingExperienceForUnit_DefaultsToZero() {
            var unit = BuildUnit();

            var city = BuildCity(new List<IBuilding>());

            var experienceLogic = Container.Resolve<StartingExperienceLogic>();

            Assert.AreEqual(0, experienceLogic.GetStartingExperienceForUnit(unit, city));
        }

        [Test]
        public void GetStartingExperienceForUnit_AddsBonusExperienceFromBuildings() {
            var unit = BuildUnit();

            var city = BuildCity(new List<IBuilding>() {
                BuildBuilding(BuildBuildingTemplate(5)), BuildBuilding(BuildBuildingTemplate(4)),
                BuildBuilding(BuildBuildingTemplate(-2)), BuildBuilding(BuildBuildingTemplate(0)),
            });

            var experienceLogic = Container.Resolve<StartingExperienceLogic>();

            Assert.AreEqual(7, experienceLogic.GetStartingExperienceForUnit(unit, city));
        }

        #endregion

        #region utilities

        private IUnit BuildUnit() {
            var mockUnit = new Mock<IUnit>();

            mockUnit.SetupAllProperties();

            return mockUnit.Object;
        }

        private ICity BuildCity(IEnumerable<IBuilding> buildings) {
            var newCity = new Mock<ICity>().Object;

            MockBuildingPossessionCanon.Setup(canon => canon.GetPossessionsOfOwner(newCity)).Returns(buildings);

            return newCity;
        }

        private IBuilding BuildBuilding(IBuildingTemplate template) {
            var mockBuilding = new Mock<IBuilding>();

            mockBuilding.Setup(building => building.Template).Returns(template);

            return mockBuilding.Object;
        }

        private IBuildingTemplate BuildBuildingTemplate(int bonusExperience) {
            var mockTemplate = new Mock<IBuildingTemplate>();

            mockTemplate.Setup(template => template.BonusExperience).Returns(bonusExperience);

            return mockTemplate.Object;
        }

        #endregion

        #endregion

    }

}
