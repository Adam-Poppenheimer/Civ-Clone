using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using NUnit.Framework;
using Moq;

namespace Assets.Cities.Buildings.Editor {

    [TestFixture]
    public class TemplateValidityLogicTests : ZenjectUnitTestFixture {

        private List<IBuildingTemplate> Templates;

        [SetUp]
        public void CommonInstall() {
            Templates = new List<IBuildingTemplate>() {
                new Mock<IBuildingTemplate>().Object,
                new Mock<IBuildingTemplate>().Object,
                new Mock<IBuildingTemplate>().Object
            };

            Container.Bind<List<IBuildingTemplate>>().FromInstance(Templates);

            Container.Bind<TemplateValidityLogic>().AsSingle();
        }

        [Test(Description = "IsTemplateValidForCity should always return true if the " +
            "template is one of the available templates given to TemplateValidityLogic, " +
            "and should return false otherwise.")]
        public void IsTemplateValidForCity_AlwaysTrueIfAvailable() {
            var cityOne = new Mock<ICity>().Object;
            var cityTwo = new Mock<ICity>().Object;

            var logic = Container.Resolve<TemplateValidityLogic>();

            var externalTemplate = new Mock<IBuildingTemplate>().Object;

            foreach(var template in Templates) {
                Assert.IsTrue(logic.IsTemplateValidForCity(template, cityOne),
                    "CityOne is considered invalid for a template");

                Assert.IsTrue(logic.IsTemplateValidForCity(template, cityTwo),
                    "CityOne is considered invalid for a template");
            }

            Assert.IsFalse(logic.IsTemplateValidForCity(externalTemplate, cityOne),
                "Template not within AvailableTemplates is falsely considered valid for cityOne");

            Assert.IsFalse(logic.IsTemplateValidForCity(externalTemplate, cityTwo),
                "Template not within AvailableTemplates is falsely considered valid for cityTwo");
        }

        [Test(Description = "GetTemplatesValidForCity should return every template the " +
            "logic has been assigned, since every template is valid for every city")]
        public void GetTemplatesValidForCity_ReturnsAllAvailableTemplates() {
            var cityOne = new Mock<ICity>().Object;
            var cityTwo = new Mock<ICity>().Object;

            var logic = Container.Resolve<TemplateValidityLogic>();

            CollectionAssert.AreEquivalent(Templates, logic.GetTemplatesValidForCity(cityOne),
                "GetTemplatesValidForCity did not return all available templates for cityOne");

            CollectionAssert.AreEquivalent(Templates, logic.GetTemplatesValidForCity(cityTwo),
                "GetTemplatesValidForCity did not return all available templates for cityTwo");
        }

        [Test(Description = "GetTemplatesValidForCity should not return any templates that " +
            "are already represented by a building in the argued city")]
        public void GetTemplatesValidForCity_ExcludesConstructedTemplates() {
            throw new NotImplementedException();
        }

        [Test(Description = "Every method should throw an ArgumentNullException when passed " +
            "any null argument")]
        public void AllMethods_ThrowOnNullArguments() {
            var city = new Mock<ICity>().Object;
            var template = new Mock<IBuildingTemplate>().Object;

            var logic = Container.Resolve<TemplateValidityLogic>();

            Assert.Throws<ArgumentNullException>(() => logic.GetTemplatesValidForCity(null),
                "GetTemplatesValidForCity did not throw an ArgumentNullException on a null city argument");

            Assert.Throws<ArgumentNullException>(() => logic.IsTemplateValidForCity(null, city),
                "IsTemplateValidForCity did not throw an ArgumentNullException on a null template argument");

            Assert.Throws<ArgumentNullException>(() => logic.IsTemplateValidForCity(template, null),
                "IsTemplateValidForCity did not throw an ArgumentNullException on a null city argument");
        }

    }

}
