using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation.Cities;
using Assets.Simulation.Cities.Buildings;

namespace Assets.Tests.Simulation.Cities {

    [TestFixture]
    public class TemplateValidityLogicTests : ZenjectUnitTestFixture {

        private List<IBuildingTemplate> Templates;

        private Mock<IBuildingPossessionCanon> PossessionCanonMock;

        [SetUp]
        public void CommonInstall() {
            Templates = new List<IBuildingTemplate>() {
                new Mock<IBuildingTemplate>().Object,
                new Mock<IBuildingTemplate>().Object,
                new Mock<IBuildingTemplate>().Object
            };

            PossessionCanonMock = new Mock<IBuildingPossessionCanon>();
            PossessionCanonMock
                .Setup(canon => canon.GetBuildingsInCity(It.IsAny<ICity>()))
                .Returns(new List<IBuilding>().AsReadOnly());

            Container.Bind<List<IBuildingTemplate>>().FromInstance(Templates);

            Container.Bind<IBuildingPossessionCanon>().FromInstance(PossessionCanonMock.Object);

            Container.Bind<BuildingProductionValidityLogic>().AsSingle();
        }

        [Test(Description = "IsTemplateValidForCity should return false if the " +
            "template is not of the available templates given to TemplateValidityLogic.")]
        public void IsTemplateValidForCity_FalseIfNotRecognized() {
            var cityOne = new Mock<ICity>().Object;
            var cityTwo = new Mock<ICity>().Object;

            var logic = Container.Resolve<BuildingProductionValidityLogic>();

            var externalTemplate = new Mock<IBuildingTemplate>().Object;

            Assert.IsFalse(logic.IsTemplateValidForCity(externalTemplate, cityOne),
                "Template not within AvailableTemplates is falsely considered valid for cityOne");

            Assert.IsFalse(logic.IsTemplateValidForCity(externalTemplate, cityTwo),
                "Template not within AvailableTemplates is falsely considered valid for cityTwo");
        }

        [Test(Description = "IsTemplateValidForCity should return false if " +
            "BuildingPossessionCanon claims a building with that template is already at the city")]
        public void IsTemplateValidForCity_FalseIfAlreadyThere() {
            var cityOne = new Mock<ICity>().Object;
            var cityTwo = new Mock<ICity>().Object;

            var buildingMockInCityOne = new Mock<IBuilding>();
            buildingMockInCityOne.Setup(building => building.Template).Returns(Templates[0]);

            var buildingMockInCityTwo = new Mock<IBuilding>();
            buildingMockInCityTwo.Setup(building => building.Template).Returns(Templates[1]);

            PossessionCanonMock
                .Setup(canon => canon.GetBuildingsInCity(cityOne))
                .Returns(new List<IBuilding>() { buildingMockInCityOne.Object }.AsReadOnly());

            PossessionCanonMock
                .Setup(canon => canon.GetBuildingsInCity(cityTwo))
                .Returns(new List<IBuilding>() { buildingMockInCityTwo.Object }.AsReadOnly());

            var logic = Container.Resolve<BuildingProductionValidityLogic>();

            Assert.IsFalse(logic.IsTemplateValidForCity(Templates[0], cityOne),
                "IsTemplateValidForCity falsely considers Templates[0] valid for cityOne");
            Assert.IsFalse(logic.IsTemplateValidForCity(Templates[1], cityTwo),
                "IsTemplateValidForCity falsely considers Templates[1] valid for cityTwo");
        }

        [Test(Description = "GetTemplatesValidForCity should return every template the " +
            "logic has been assigned, since every template is valid for every city")]
        public void GetTemplatesValidForCity_ReturnsUnconstructedTemplates() {
            var cityOne = new Mock<ICity>().Object;
            var cityTwo = new Mock<ICity>().Object;

            var logic = Container.Resolve<BuildingProductionValidityLogic>();

            CollectionAssert.AreEquivalent(Templates, logic.GetTemplatesValidForCity(cityOne),
                "GetTemplatesValidForCity did not return all available templates for cityOne");

            CollectionAssert.AreEquivalent(Templates, logic.GetTemplatesValidForCity(cityTwo),
                "GetTemplatesValidForCity did not return all available templates for cityTwo");
        }

        [Test(Description = "GetTemplatesValidForCity should not return any templates that " +
            "are already represented by a building in the argued city")]
        public void GetTemplatesValidForCity_ExcludesConstructedTemplates() {
            var cityOne = new Mock<ICity>().Object;
            var cityTwo = new Mock<ICity>().Object;

            var buildingMockInCityOne = new Mock<IBuilding>();
            buildingMockInCityOne.Setup(building => building.Template).Returns(Templates[0]);

            var firstBuildingMockInCityTwo = new Mock<IBuilding>();
            firstBuildingMockInCityTwo.Setup(building => building.Template).Returns(Templates[1]);

            var secondBuildingMockInCityTwo = new Mock<IBuilding>();
            secondBuildingMockInCityTwo.Setup(building => building.Template).Returns(Templates[2]);

            PossessionCanonMock
                .Setup(canon => canon.GetBuildingsInCity(cityOne))
                .Returns(new List<IBuilding>() { buildingMockInCityOne.Object }.AsReadOnly());

            PossessionCanonMock
                .Setup(canon => canon.GetBuildingsInCity(cityTwo))
                .Returns(new List<IBuilding>() {
                    firstBuildingMockInCityTwo.Object, secondBuildingMockInCityTwo.Object
                }.AsReadOnly());

            var logic = Container.Resolve<BuildingProductionValidityLogic>();

            var validForCityOne = logic.GetTemplatesValidForCity(cityOne);
            var validForCityTwo = logic.GetTemplatesValidForCity(cityTwo);

            CollectionAssert.DoesNotContain(validForCityOne, Templates[0],
                "GetTemplatesValidForCity falsely included Templates[0] when queried on cityOne");
            CollectionAssert.Contains(validForCityOne, Templates[1],
                "GetTemplatesValidForCity fails to include Templates[1] when queried on cityOne");
            CollectionAssert.Contains(validForCityOne, Templates[2],
                "GetTemplatesValidForCity fails to include Templates[2] when queried on cityOne");

            CollectionAssert.Contains(validForCityTwo, Templates[0],
                "GetTemplatesValidForCity fails to include Templates[0] when queried on cityTwo");
            CollectionAssert.DoesNotContain(validForCityTwo, Templates[1],
                "GetTemplatesValidForCity falsely included Templates[1] when queried on cityTwo");
            CollectionAssert.DoesNotContain(validForCityTwo, Templates[2],
                "GetTemplatesValidForCity falsely included Templates[2] when queried on cityTwo");
        }

        [Test(Description = "Every method should throw an ArgumentNullException when passed " +
            "any null argument")]
        public void AllMethods_ThrowOnNullArguments() {
            var city = new Mock<ICity>().Object;
            var template = new Mock<IBuildingTemplate>().Object;

            var logic = Container.Resolve<BuildingProductionValidityLogic>();

            Assert.Throws<ArgumentNullException>(() => logic.GetTemplatesValidForCity(null),
                "GetTemplatesValidForCity did not throw an ArgumentNullException on a null city argument");

            Assert.Throws<ArgumentNullException>(() => logic.IsTemplateValidForCity(null, city),
                "IsTemplateValidForCity did not throw an ArgumentNullException on a null template argument");

            Assert.Throws<ArgumentNullException>(() => logic.IsTemplateValidForCity(template, null),
                "IsTemplateValidForCity did not throw an ArgumentNullException on a null city argument");
        }

    }

}
