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
using Assets.Simulation.Cities.Production;

namespace Assets.Tests.Simulation.Cities {

    public class WonderBuildingRestrictionTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IPossessionRelationship<ICivilization, ICity>> MockCityPossessionCanon;
        private Mock<IPossessionRelationship<ICity, IBuilding>>     MockBuildingPossessionCanon;
        private Mock<ICityFactory>                                  MockCityFactory;

        private List<ICity> AllCities = new List<ICity>();

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            AllCities.Clear();

            MockCityPossessionCanon     = new Mock<IPossessionRelationship<ICivilization, ICity>>();
            MockBuildingPossessionCanon = new Mock<IPossessionRelationship<ICity, IBuilding>>();
            MockCityFactory             = new Mock<ICityFactory>();

            MockCityFactory.Setup(factory => factory.AllCities).Returns(AllCities.AsReadOnly());

            Container.Bind<IPossessionRelationship<ICivilization, ICity>>().FromInstance(MockCityPossessionCanon    .Object);
            Container.Bind<IPossessionRelationship<ICity, IBuilding>>    ().FromInstance(MockBuildingPossessionCanon.Object);
            Container.Bind<ICityFactory>                                 ().FromInstance(MockCityFactory            .Object);

            Container.Bind<WonderBuildingRestriction>().AsSingle();
        }

        #endregion

        #region tests

        #region national wonders

        [Test]
        public void IsTemplateVaidForCity_AndBuildingNationalWonder_FalseIfSomeDomesticCityAlreadyBuildingTemplate() {
            var nationalWonderTemplate = BuildTemplate(BuildingType.NationalWonder);

            var cityOne   = BuildCity(null);
            var cityTwo   = BuildCity(BuildProject(nationalWonderTemplate));
            var cityThree = BuildCity(null);

            var civ  = BuildCiv(cityOne, cityTwo, cityThree);

            var restriction = Container.Resolve<WonderBuildingRestriction>();

            Assert.IsFalse(restriction.IsTemplateValidForCity(nationalWonderTemplate, cityOne, civ));
        }

        [Test]
        public void IsTemplateValidForCity_AndBuildingNationalWonder_TrueIfSomeForeignCityAlreadyBuildingTemplate() {
            var nationalWonderTemplate = BuildTemplate(BuildingType.NationalWonder);

            var domesticCity = BuildCity(null);
            var foreignCity  = BuildCity(BuildProject(nationalWonderTemplate));

            var domesticCiv = BuildCiv(domesticCity);
            BuildCiv(foreignCity);

            var restriction = Container.Resolve<WonderBuildingRestriction>();

            Assert.IsTrue(restriction.IsTemplateValidForCity(nationalWonderTemplate, domesticCity, domesticCiv));
        }

        [Test]
        public void IsTemplateValidForCity_AndBuildingNationalWonder_FalseIfBuildingExistsInSomeDomesticCity() {
            var normalTemplate         = BuildTemplate(BuildingType.Normal);
            var nationalWonderTemplate = BuildTemplate(BuildingType.NationalWonder);

            var normalBuilding = BuildBuilding(normalTemplate);
            var nationalWonder = BuildBuilding(nationalWonderTemplate);

            var cityOne   = BuildCity(null);
            var cityTwo   = BuildCity(null, normalBuilding);
            var cityThree = BuildCity(null, nationalWonder);

            var civ  = BuildCiv(cityOne, cityTwo, cityThree);

            var restriction = Container.Resolve<WonderBuildingRestriction>();

            Assert.IsFalse(restriction.IsTemplateValidForCity(nationalWonderTemplate, cityOne, civ));
        }

        [Test]
        public void IsTemplateValidForCity_AndBuildingNationalWonder_TrueIfBuildingExistsInSomeForeignCity() {
            var nationalWonderTemplate = BuildTemplate(BuildingType.NationalWonder);

            var nationalWonder = BuildBuilding(nationalWonderTemplate);

            var domesticCity = BuildCity(null);
            var foreignCity  = BuildCity(null, nationalWonder);

            var domesticCiv = BuildCiv(domesticCity);
            BuildCiv(foreignCity);

            var restriction = Container.Resolve<WonderBuildingRestriction>();

            Assert.IsTrue(restriction.IsTemplateValidForCity(nationalWonderTemplate, domesticCity, domesticCiv));
        }

        [Test]
        public void IsTemplateValidForCity_AndBuildingNationalWonder_TrueIfBuildingDoesntExistAndIsntBeingConstructed() {
            var normalTemplate         = BuildTemplate(BuildingType.Normal);
            var nationalWonderTemplate = BuildTemplate(BuildingType.NationalWonder);

            var normalBuilding = BuildBuilding(normalTemplate);

            var city = BuildCity(BuildProject(normalTemplate), normalBuilding);
            var civ  = BuildCiv(city);

            var restriction = Container.Resolve<WonderBuildingRestriction>();

            Assert.IsTrue(restriction.IsTemplateValidForCity(nationalWonderTemplate, city, civ));
        }

        #endregion

        #region world wonders

        [Test]
        public void IsTemplateValidForCity_AndBuildingWorldWonder_FalseIfSomeDomesticCityAlreadyBuildingTemplate() {
            var worldWonderTemplate = BuildTemplate(BuildingType.WorldWonder);
            
            var domesticCityOne = BuildCity(null);
            var domesticCityTwo = BuildCity(BuildProject(worldWonderTemplate));

            var domesticCiv = BuildCiv(domesticCityOne, domesticCityTwo);

            var restriction = Container.Resolve<WonderBuildingRestriction>();

            Assert.IsFalse(restriction.IsTemplateValidForCity(worldWonderTemplate, domesticCityOne, domesticCiv));
        }

        [Test]
        public void IsTemplateValidForCity_AndBuildingWorldWonder_TrueIfSomeForeignCityAlreadyBuildingTemplate() {
            var worldWonderTemplate = BuildTemplate(BuildingType.WorldWonder);
            
            var domesticCityOne = BuildCity(null);
            var foreignCityOne  = BuildCity(BuildProject(worldWonderTemplate));

            var domesticCiv = BuildCiv(domesticCityOne);
            BuildCiv(foreignCityOne);

            var restriction = Container.Resolve<WonderBuildingRestriction>();

            Assert.IsTrue(restriction.IsTemplateValidForCity(worldWonderTemplate, domesticCityOne, domesticCiv));
        }

        [Test]
        public void IsTemplatevalidForCity_AndBuildingWorldWonder_FalseIfBuildingExistsInSomeDomesticCity() {
            var worldWonderTemplate = BuildTemplate(BuildingType.WorldWonder);

            var worldWonder = BuildBuilding(worldWonderTemplate);

            var domesticCityOne = BuildCity(null);
            var domesticCityTwo = BuildCity(null, worldWonder);
            var foreignCity     = BuildCity(null);

            var domesticCiv = BuildCiv(domesticCityOne, domesticCityTwo);
            BuildCiv(foreignCity);

            var restriction = Container.Resolve<WonderBuildingRestriction>();

            Assert.IsFalse(restriction.IsTemplateValidForCity(worldWonderTemplate, domesticCityOne, domesticCiv));
        }

        [Test]
        public void IsTemplateValidForCity_AndBuildingWorldWonder_FalseIfBuildingExistsInSomeForeignCity() {
            var worldWonderTemplate = BuildTemplate(BuildingType.WorldWonder);

            var worldWonder = BuildBuilding(worldWonderTemplate);

            var domesticCity = BuildCity(null);
            var foreignCity  = BuildCity(null, worldWonder);

            var domesticCiv = BuildCiv(domesticCity);
            BuildCiv(foreignCity);

            var restriction = Container.Resolve<WonderBuildingRestriction>();

            Assert.IsFalse(restriction.IsTemplateValidForCity(worldWonderTemplate, domesticCity, domesticCiv));
        }

        [Test]
        public void IsTemplatevalidForCity_AndBuildingWorldWonder_TrueIfBuildingDoesntExistAndIsntBeingConstructed() {
            var worldWonderTemplate = BuildTemplate(BuildingType.WorldWonder);

            var domesticCity = BuildCity(null);

            var domesticCiv = BuildCiv(domesticCity);

            var restriction = Container.Resolve<WonderBuildingRestriction>();

            Assert.IsTrue(restriction.IsTemplateValidForCity(worldWonderTemplate, domesticCity, domesticCiv));
        }

        #endregion

        [Test]
        public void IsTemplateValidForCity_TrueIfTemplateTypeNotNationalWonderOrWorldWonder() {
            var normalTemplate = BuildTemplate(BuildingType.Normal);

            var normalBuilding = BuildBuilding(normalTemplate);

            var cityOne   = BuildCity(BuildProject(normalTemplate), normalBuilding);
            var cityTwo   = BuildCity(BuildProject(normalTemplate), normalBuilding);
            var cityThree = BuildCity(BuildProject(normalTemplate), normalBuilding);

            var civ  = BuildCiv(cityOne, cityTwo, cityThree);

            var restriction = Container.Resolve<WonderBuildingRestriction>();

            Assert.IsTrue(restriction.IsTemplateValidForCity(normalTemplate, cityOne, civ));
        }

        #endregion

        #region utilities

        private IBuildingTemplate BuildTemplate(BuildingType type) {
            var mockTemplate = new Mock<IBuildingTemplate>();

            mockTemplate.Setup(template => template.Type).Returns(type);

            return mockTemplate.Object;
        }

        private IBuilding BuildBuilding(IBuildingTemplate template) {
            var mockBuilding = new Mock<IBuilding>();

            mockBuilding.Setup(building => building.Template).Returns(template);

            return mockBuilding.Object;
        }

        private IProductionProject BuildProject(IBuildingTemplate buildingToConstruct) {
            var mockProject = new Mock<IProductionProject>();

            mockProject.Setup(project => project.BuildingToConstruct).Returns(buildingToConstruct);

            return mockProject.Object;
        }

        private ICity BuildCity(IProductionProject project, params IBuilding[] buildings) {
            var mockCity = new Mock<ICity>();

            mockCity.Setup(city => city.ActiveProject).Returns(project);

            var newCity = mockCity.Object;

            MockBuildingPossessionCanon.Setup(canon => canon.GetPossessionsOfOwner(newCity)).Returns(buildings);

            AllCities.Add(newCity);

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
