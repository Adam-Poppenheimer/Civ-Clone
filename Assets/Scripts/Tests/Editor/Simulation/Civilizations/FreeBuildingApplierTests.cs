using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using Zenject;
using Moq;

using Assets.Simulation.Cities;
using Assets.Simulation.Cities.Buildings;
using Assets.Simulation.Civilizations;

namespace Assets.Tests.Simulation.Civilizations {

    public class FreeBuildingApplierTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IBuildingProductionValidityLogic> MockBuildingProductionValidityLogic;
        private Mock<IBuildingFactory>                 MockBuildingFactory;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockBuildingProductionValidityLogic = new Mock<IBuildingProductionValidityLogic>();
            MockBuildingFactory                 = new Mock<IBuildingFactory>();

            Container.Bind<IBuildingProductionValidityLogic>().FromInstance(MockBuildingProductionValidityLogic.Object);
            Container.Bind<IBuildingFactory>                ().FromInstance(MockBuildingFactory                .Object);

            Container.Bind<FreeBuildingApplier>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void CanApplyFreeBuildingToCity_TrueIfAnyTemplateValidForProduction() {
            var templates = new List<IBuildingTemplate>() {
                BuildTemplate(false),
                BuildTemplate(true),
                BuildTemplate(true),
                BuildTemplate(false),
            };

            var city = BuildCity();

            var buildingApplier = Container.Resolve<FreeBuildingApplier>();

            Assert.IsTrue(buildingApplier.CanApplyFreeBuildingToCity(templates, city));
        }

        [Test]
        public void CanApplyFreeBuildingToCity_FalseIfNoTemplateValidForProduction() {
            var templates = new List<IBuildingTemplate>() {
                BuildTemplate(false),
                BuildTemplate(false),
            };

            var city = BuildCity();

            var buildingApplier = Container.Resolve<FreeBuildingApplier>();

            Assert.IsFalse(buildingApplier.CanApplyFreeBuildingToCity(templates, city));
        }

        [Test]
        public void CanApplyFreeBuildingToCity_FalseIfSomeTemplateAlreadyAppliedToCity() {
            var templateOne = BuildTemplate(true);
            var templateTwo = BuildTemplate(true);

            var city = BuildCity();

            var buildingApplier = Container.Resolve<FreeBuildingApplier>();

            buildingApplier.ApplyFreeBuildingToCity(new List<IBuildingTemplate>() { templateOne }, city);

            Assert.IsFalse(buildingApplier.CanApplyFreeBuildingToCity(
                new List<IBuildingTemplate>() { templateOne, templateTwo }, city
            ));
        }

        [Test]
        public void ApplyFreeBuildingToCity_BuildsFirstValidTemplateInCity() {
            var templates = new List<IBuildingTemplate>() {
                BuildTemplate(false),
                BuildTemplate(true),
                BuildTemplate(true),
                BuildTemplate(false),
            };

            var city = BuildCity();

            var buildingApplier = Container.Resolve<FreeBuildingApplier>();

            buildingApplier.ApplyFreeBuildingToCity(templates, city);

            MockBuildingFactory.Verify(
                factory => factory.BuildBuilding(templates[1], city),
                Times.Once, "Failed to construct templates[1]"
            );
        }

        [Test]
        public void ApplyFreeBuildingToCity_ReflectedInTemplatesAppledToCity() {
            var templates = new List<IBuildingTemplate>() {
                BuildTemplate(false),
                BuildTemplate(true),
                BuildTemplate(true),
                BuildTemplate(false),
            };

            var city = BuildCity();

            var buildingApplier = Container.Resolve<FreeBuildingApplier>();

            buildingApplier.ApplyFreeBuildingToCity(templates, city);

            CollectionAssert.AreEquivalent(
                new List<IBuildingTemplate>() { templates[1] },
                buildingApplier.GetTemplatesAppliedToCity(city)
            );
        }

        [Test]
        public void ApplyFreeBuildingToCity_DoesntBuildValidTemplatesBeyondTheFirst() {
            var templates = new List<IBuildingTemplate>() {
                BuildTemplate(false),
                BuildTemplate(true),
                BuildTemplate(true),
                BuildTemplate(false),
            };

            var city = BuildCity();

            var buildingApplier = Container.Resolve<FreeBuildingApplier>();

            buildingApplier.ApplyFreeBuildingToCity(templates, city);

            MockBuildingFactory.Verify(
                factory => factory.BuildBuilding(templates[2], city),
                Times.Never, "Unexpectedly constructed templates[2]"
            );
        }

        [Test]
        public void ApplyFreeBuildingToCity_DoesntBuildInvalidTemplates() {
            var templates = new List<IBuildingTemplate>() {
                BuildTemplate(false),
                BuildTemplate(true),
                BuildTemplate(true),
                BuildTemplate(false),
            };

            var city = BuildCity();

            var buildingApplier = Container.Resolve<FreeBuildingApplier>();

            buildingApplier.ApplyFreeBuildingToCity(templates, city);

            MockBuildingFactory.Verify(
                factory => factory.BuildBuilding(templates[0], city),
                Times.Never, "Unexpectedly constructed templates[0]"
            );

            MockBuildingFactory.Verify(
                factory => factory.BuildBuilding(templates[3], city),
                Times.Never, "Unexpectedly constructed templates[3]"
            );
        }
        
        [Test]
        public void ApplyFreeBuildingToCity_DoesNothingIfNoTemplatesValid() {
            var templates = new List<IBuildingTemplate>() {
                BuildTemplate(false),
                BuildTemplate(false),
                BuildTemplate(false),
                BuildTemplate(false),
            };

            var city = BuildCity();

            var buildingApplier = Container.Resolve<FreeBuildingApplier>();

            buildingApplier.ApplyFreeBuildingToCity(templates, city);

            MockBuildingFactory.Verify(
                factory => factory.BuildBuilding(It.IsAny<IBuildingTemplate>(), city), Times.Never
            );
        }

        [Test]
        public void OverrideTemplatesAppliedToCity_ReflectedInGetTemplatesAppliedToCity() {
            var templateOne   = BuildTemplate(true);
            var templateTwo   = BuildTemplate(true);
            var templateThree = BuildTemplate(true);

            var city = BuildCity();

            var buildingApplier = Container.Resolve<FreeBuildingApplier>();

            buildingApplier.ApplyFreeBuildingToCity(new List<IBuildingTemplate>() { templateOne }, city);

            var newAppliedList = new List<IBuildingTemplate>() { templateTwo, templateThree };

            buildingApplier.OverrideTemplatesAppliedToCity(city, newAppliedList);

            CollectionAssert.AreEquivalent(
                newAppliedList, buildingApplier.GetTemplatesAppliedToCity(city)
            );
        }

        [Test]
        public void Clear_AppliedBuildingsOfAllCitiesCleared() {
            var templates = new List<IBuildingTemplate>() {
                BuildTemplate(true), BuildTemplate(true), BuildTemplate(true),
            };

            var cityOne = BuildCity();
            var cityTwo = BuildCity();

            var buildingApplier = Container.Resolve<FreeBuildingApplier>();

            buildingApplier.OverrideTemplatesAppliedToCity(cityOne, templates);
            buildingApplier.OverrideTemplatesAppliedToCity(cityTwo, templates);

            buildingApplier.Clear();

            CollectionAssert.IsEmpty(buildingApplier.GetTemplatesAppliedToCity(cityOne), "CityOne unexpectedly has applied templates");
            CollectionAssert.IsEmpty(buildingApplier.GetTemplatesAppliedToCity(cityTwo), "CityTwo unexpectedly has applied templates");
        }

        #endregion

        #region utilities

        private IBuildingTemplate BuildTemplate(bool isValid) {
            var newTemplate = new Mock<IBuildingTemplate>().Object;

            MockBuildingProductionValidityLogic.Setup(
                logic => logic.IsTemplateValidForCity(newTemplate, It.IsAny<ICity>())
            ).Returns(isValid);

            return newTemplate;
        }

        private ICity BuildCity() {
            return new Mock<ICity>().Object;
        }

        #endregion

        #endregion 

    }

}
