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

namespace Assets.Tests.Simulation.Civilizations {

    public class CapitalCityCanonTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IPossessionRelationship<ICity, IBuilding>>     MockBuildingPossessionCanon;
        private Mock<ICityConfig>                                   MockCityConfig;
        private Mock<IBuildingFactory>                              MockBuildingFactory;
        private Mock<IPossessionRelationship<ICivilization, ICity>> MockCityPossessionCanon;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockBuildingPossessionCanon = new Mock<IPossessionRelationship<ICity, IBuilding>>();
            MockCityConfig              = new Mock<ICityConfig>();
            MockBuildingFactory         = new Mock<IBuildingFactory>();
            MockCityPossessionCanon     = new Mock<IPossessionRelationship<ICivilization, ICity>>();

            Container.Bind<IPossessionRelationship<ICity, IBuilding>>    ().FromInstance(MockBuildingPossessionCanon.Object);
            Container.Bind<ICityConfig>                                  ().FromInstance(MockCityConfig             .Object);
            Container.Bind<IBuildingFactory>                             ().FromInstance(MockBuildingFactory        .Object);
            Container.Bind<IPossessionRelationship<ICivilization, ICity>>().FromInstance(MockCityPossessionCanon    .Object);

            Container.Bind<CapitalCityCanon>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void GetCapitalOfCiv_ReturnsNullByDefault() {
            var civ = BuildCiv();

            var capitalCanon = Container.Resolve<CapitalCityCanon>();

            Assert.IsNull(capitalCanon.GetCapitalOfCiv(civ));
        }

        [Test]
        public void CanSetCapitalOfCiv_FalseIfCityAlreadyCapital() {
            var civ  = BuildCiv();
            var city = BuildCity(civ);

            var capitalCanon = Container.Resolve<CapitalCityCanon>();

            capitalCanon.SetCapitalOfCiv(civ, city);

            Assert.IsFalse(capitalCanon.CanSetCapitalOfCiv(civ, city));
        }

        [Test]
        public void CanSetCapitalOfCiv_FalseIfCityDoesntBelongToCiv() {
            var civ = BuildCiv();
            var city = BuildCity(null);

            var capitalCanon = Container.Resolve<CapitalCityCanon>();

            Assert.IsFalse(capitalCanon.CanSetCapitalOfCiv(civ, city));
        }

        [Test]
        public void SetCapitalOfCiv_ReflectedInGetCapitalOfCiv() {
            var civ = BuildCiv();
            var city = BuildCity(civ);

            var capitalCanon = Container.Resolve<CapitalCityCanon>();

            capitalCanon.SetCapitalOfCiv(civ, city);

            Assert.AreEqual(city, capitalCanon.GetCapitalOfCiv(civ));
        }

        [Test]
        public void SetCapitalOfCiv_AddsCapitalTemplatesToNewCapital() {
            var templateOne   = BuildTemplate();
            var templateTwo   = BuildTemplate();
            var templateThree = BuildTemplate();

            MockCityConfig.Setup(config => config.CapitalTemplates).Returns(
                new List<IBuildingTemplate>() { templateOne, templateTwo, templateThree }
            );

            var civ = BuildCiv();
            var city = BuildCity(civ);

            var capitalCanon = Container.Resolve<CapitalCityCanon>();

            capitalCanon.SetCapitalOfCiv(civ, city);

            MockBuildingFactory.Verify(factory => factory.BuildBuilding(templateOne,   city), Times.Once, "TemplateOne was not built as expected");
            MockBuildingFactory.Verify(factory => factory.BuildBuilding(templateTwo,   city), Times.Once, "TemplateTwo was not built as expected");
            MockBuildingFactory.Verify(factory => factory.BuildBuilding(templateThree, city), Times.Once, "TemplateThree was not built as expected");
        }

        [Test]
        public void SetCapitalOfCiv_DoesntAddDuplicatesOfCapitalTemplatesToNewCapital() {
            var templateOne   = BuildTemplate();
            var templateTwo   = BuildTemplate();
            var templateThree = BuildTemplate();

            var buildingOne = BuildBuilding(templateOne);
            var buildingTwo = BuildBuilding(templateTwo);

            MockCityConfig.Setup(config => config.CapitalTemplates).Returns(
                new List<IBuildingTemplate>() { templateOne, templateTwo, templateThree }
            );

            var civ  = BuildCiv();
            var city = BuildCity(civ, buildingOne, buildingTwo);

            var capitalCanon = Container.Resolve<CapitalCityCanon>();

            capitalCanon.SetCapitalOfCiv(civ, city);

            MockBuildingFactory.Verify(factory => factory.BuildBuilding(templateOne,   city), Times.Never, "TemplateOne was unexpectedly build");
            MockBuildingFactory.Verify(factory => factory.BuildBuilding(templateTwo,   city), Times.Never, "TemplateTwo was unexpectedly build");
            MockBuildingFactory.Verify(factory => factory.BuildBuilding(templateThree, city), Times.Once, "TemplateThree not built as expected");
        }

        [Test]
        public void SetCapitalOfCiv_DestroysCapitalTemplatesFromOldCapital() {
            var templateOne   = BuildTemplate();
            var templateTwo   = BuildTemplate();
            var templateThree = BuildTemplate();

            var buildingOne   = BuildBuilding(templateOne);
            var buildingTwo   = BuildBuilding(templateTwo);
            var buildingThree = BuildBuilding(templateThree);

            MockCityConfig.Setup(config => config.CapitalTemplates).Returns(
                new List<IBuildingTemplate>() { templateOne, templateTwo }
            );

            var civ  = BuildCiv();
            var oldCapital = BuildCity(civ, buildingOne, buildingTwo, buildingThree);
            var newCapital = BuildCity(civ);

            var capitalCanon = Container.Resolve<CapitalCityCanon>();

            capitalCanon.SetCapitalOfCiv(civ, oldCapital);
            capitalCanon.SetCapitalOfCiv(civ, newCapital);

            MockBuildingFactory.Verify(factory => factory.DestroyBuilding(buildingOne),   Times.Once,  "BuildingOne was not destroyed as expected");
            MockBuildingFactory.Verify(factory => factory.DestroyBuilding(buildingTwo),   Times.Once,  "BuildingTwo was not destroyed as expected");
            MockBuildingFactory.Verify(factory => factory.DestroyBuilding(buildingThree), Times.Never, "BuildingThree was unexpectedly destroyed");
        }

        #endregion

        #region utilities

        private ICivilization BuildCiv() {
            return new Mock<ICivilization>().Object;
        }

        private ICity BuildCity(ICivilization owner, params IBuilding[] buildings) {
            var newCity = new Mock<ICity>().Object;

            MockCityPossessionCanon.Setup(canon => canon.GetOwnerOfPossession(newCity)).Returns(owner);

            MockBuildingPossessionCanon.Setup(canon => canon.GetPossessionsOfOwner(newCity)).Returns(buildings);

            return newCity;
        }

        private IBuildingTemplate BuildTemplate() {
            return new Mock<IBuildingTemplate>().Object;
        }

        private IBuilding BuildBuilding(IBuildingTemplate template) {
            var mockBuilding = new Mock<IBuilding>();

            mockBuilding.Setup(building => building.Template).Returns(template);

            return mockBuilding.Object;
        }

        #endregion

        #endregion

    }

}
