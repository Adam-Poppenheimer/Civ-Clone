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

namespace Assets.Tests.Simulation.Cities {

    public class OtherBuildingsBuildingRestrictionTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IPossessionRelationship<ICity, IBuilding>> MockBuildingPossessionCanon;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockBuildingPossessionCanon = new Mock<IPossessionRelationship<ICity, IBuilding>>();

            Container.Bind<IPossessionRelationship<ICity, IBuilding>>().FromInstance(MockBuildingPossessionCanon.Object);

            Container.Bind<OtherBuildingsBuildingRestriction>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void IsTemplateValidForCity_TrueIfAllPrerequisitesPresent() {
            var prereqTemplateOne = BuildTemplate();
            var prereqTemplateTwo = BuildTemplate();

            var templateToTest = BuildTemplate(prereqTemplateOne, prereqTemplateTwo);

            var city = BuildCity(
                BuildBuilding(prereqTemplateOne), BuildBuilding(prereqTemplateTwo)
            );

            var civ = BuildCiv();

            var restriction = Container.Resolve<OtherBuildingsBuildingRestriction>();

            Assert.IsTrue(restriction.IsTemplateValidForCity(templateToTest, city, civ));
        }

        [Test]
        public void IsTemplateValidForCity_FalseIfAnyPrerequisiteNotPresent() {
            var prereqTemplateOne = BuildTemplate();
            var prereqTemplateTwo = BuildTemplate();

            var templateToTest = BuildTemplate(prereqTemplateOne, prereqTemplateTwo);

            var city = BuildCity(BuildBuilding(prereqTemplateOne));

            var civ = BuildCiv();

            var restriction = Container.Resolve<OtherBuildingsBuildingRestriction>();

            Assert.IsFalse(restriction.IsTemplateValidForCity(templateToTest, city, civ));
        }

        [Test]
        public void IsTemplateValidForCity_FalseIfTemplateAlreadyThere() {
            var prereqTemplateOne = BuildTemplate();
            var prereqTemplateTwo = BuildTemplate();

            var templateToTest = BuildTemplate(prereqTemplateOne, prereqTemplateTwo);

            var city = BuildCity(
                BuildBuilding(prereqTemplateOne), BuildBuilding(prereqTemplateTwo),
                BuildBuilding(templateToTest)
            );

            var civ = BuildCiv();

            var restriction = Container.Resolve<OtherBuildingsBuildingRestriction>();

            Assert.IsFalse(restriction.IsTemplateValidForCity(templateToTest, city, civ));
        }

        #endregion

        #region utilities

        private IBuildingTemplate BuildTemplate(params IBuildingTemplate[] buildingPrereqs) {
            var mockTemplate = new Mock<IBuildingTemplate>();

            mockTemplate.Setup(template => template.PrerequisiteBuildings).Returns(buildingPrereqs);

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

        private ICivilization BuildCiv() {
            return new Mock<ICivilization>().Object;
        }

        #endregion

        #endregion

    }

}
