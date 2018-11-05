using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation;
using Assets.Simulation.Cities;
using Assets.Simulation.Cities.Buildings;
using Assets.Simulation.Civilizations;

namespace Assets.Tests.Simulation.Cities {

    [TestFixture]
    public class BuildingProductionValidityLogicTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IPossessionRelationship<ICivilization, ICity>> MockCityPossessionCanon;

        private List<IBuildingTemplate>    AvailableTemplates   = new List<IBuildingTemplate>();
        private List<IBuildingRestriction> BuildingRestrictions = new List<IBuildingRestriction>();        

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            AvailableTemplates  .Clear();
            BuildingRestrictions.Clear();

            MockCityPossessionCanon = new Mock<IPossessionRelationship<ICivilization, ICity>>();

            Container.Bind<IPossessionRelationship<ICivilization, ICity>>().FromInstance(MockCityPossessionCanon.Object);

            Container.Bind<List<IBuildingTemplate>>   ().FromInstance(AvailableTemplates);
            Container.Bind<List<IBuildingRestriction>>().FromInstance(BuildingRestrictions);

            Container.Bind<BuildingProductionValidityLogic>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void IsTemplateValidForCity_TrueIfAllRestrictionsMet() {
            var template = BuildTemplate();
            var city = BuildCity();

            BuildRestriction(template);
            BuildRestriction(template);
            BuildRestriction(template);

            var validityLogic = Container.Resolve<BuildingProductionValidityLogic>();

            Assert.IsTrue(validityLogic.IsTemplateValidForCity(template, city));
        }

        [Test]
        public void IsTemplateValidForCity_FalseIfAnyRestrictionUnmet() {
            var template = BuildTemplate();
            var city = BuildCity();

            BuildRestriction(template);
            BuildRestriction();
            BuildRestriction(template);

            var validityLogic = Container.Resolve<BuildingProductionValidityLogic>();

            Assert.IsFalse(validityLogic.IsTemplateValidForCity(template, city));
        }

        [Test]
        public void GetTemplatesValidForCity_ReturnsAllTemplatesThatAreValid() {
            var templateOne   = BuildTemplate();
            var templateTwo   = BuildTemplate();
            var templateThree = BuildTemplate();
            var templateFour  = BuildTemplate();

            BuildRestriction(templateOne, templateTwo);
            BuildRestriction(templateOne, templateTwo, templateThree);
            BuildRestriction(templateOne, templateTwo, templateFour);

            var city = BuildCity();

            var validityLogic = Container.Resolve<BuildingProductionValidityLogic>();
            
            CollectionAssert.AreEquivalent(
                new List<IBuildingTemplate>() { templateOne, templateTwo },
                validityLogic.GetTemplatesValidForCity(city)
            );
        }

        #endregion

        #region utilities

        private IBuildingTemplate BuildTemplate() {
            var newTemplate = new Mock<IBuildingTemplate>().Object;

            AvailableTemplates.Add(newTemplate);

            return newTemplate;
        }

        private IBuildingRestriction BuildRestriction(params IBuildingTemplate[] validTemplates) {
            var mockRestriction = new Mock<IBuildingRestriction>();

            foreach(var template in validTemplates) {
                mockRestriction.Setup(restriction => restriction.IsTemplateValidForCity(
                    template, It.IsAny<ICity>(), It.IsAny<ICivilization>()
                )).Returns(true);
            }
            
            var newRestriction = mockRestriction.Object;

            BuildingRestrictions.Add(newRestriction);

            return newRestriction;
        }

        private ICity BuildCity() {
            return new Mock<ICity>().Object;
        }

        #endregion

        #endregion

    }

}
