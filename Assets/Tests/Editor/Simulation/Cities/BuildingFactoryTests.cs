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

namespace Assets.Tests.Simulation.Cities {

    [TestFixture]
    public class BuildingFactoryTests : ZenjectUnitTestFixture {

        #region static fields and properties

        private static IEnumerable TemplateValidityCases {
            get {
                yield return new TestCaseData("Template", true, false).Returns(true)
                    .SetName("Template is valid and no building with it exists");

                yield return new TestCaseData("Template", true, true).Returns(false)
                    .SetName("Template is valid and a building with it exists");

                yield return new TestCaseData("Template", false, false).Returns(false)
                    .SetName("Template is invalid and no building with it exists");

                yield return new TestCaseData("Template", false, true).Returns(false)
                    .SetName("Template is invalid and a building with it exists");
            }
        }

        #endregion

        #region instance fields and properties

        private Mock<IBuildingProductionValidityLogic> MockValidityLogic;
        private Mock<IPossessionRelationship<ICity, IBuilding>> MockPossessionCanon;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockValidityLogic   = new Mock<IBuildingProductionValidityLogic>();
            MockPossessionCanon = new Mock<IPossessionRelationship<ICity, IBuilding>>();

            Container.Bind<IBuildingProductionValidityLogic>().FromInstance(MockValidityLogic  .Object);
            Container.Bind<IPossessionRelationship<ICity, IBuilding>>().FromInstance(MockPossessionCanon.Object);

            Container.Bind<BuildingFactory>().AsSingle();
        }

        #endregion

        #region tests

        [Test(Description = "CanConstructTemplateInCity should return true with the argued template " +
            "is considered valid by IBuildingProductionValidityLogic and when IBuildingPossessionCanon " +
            "registers no building with the same template. Otherwise, it should return false")]
        [TestCaseSource("TemplateValidityCases")]
        public bool CanConstructTemplateInCity_MustBeValidAndNonExistent(string templateName,
            bool templateIsValid, bool templateAlreadyExists
        ){
            var template = BuildTemplate(templateName, templateIsValid);

            ICity city;
            if(templateAlreadyExists) {
                city = BuildCity(BuildBuilding(template));
            }else {
                city = BuildCity();
            }

            var factory = Container.Resolve<BuildingFactory>();

            return factory.CanConstructTemplateInCity(template, city);
        }

        [Test(Description = "When a new building is created, it should be initialized with " +
            "the argued IBuildingTemplate")]
        public void BuildingCreated_InitializedWithTemplate() {
            var template = BuildTemplate("Template", true);

            var city = BuildCity();

            var factory = Container.Resolve<BuildingFactory>();

            var newBuilding = factory.Create(template, city);

            Assert.AreEqual(template, newBuilding.Template, "newBuilding has an unexpected template");
        }

        [Test(Description = "When a new building is created, BuildingFactory should call " +
            "into BuildingPossessionCanon and assign the new building to the argued city")]
        public void BuildingCreated_PlacedInCity() {
            var template = BuildTemplate("Template", true);

            var city = BuildCity();

            var factory = Container.Resolve<BuildingFactory>();

            var newBuilding = factory.Create(template, city);

            MockPossessionCanon.Verify(canon => canon.CanChangeOwnerOfPossession(newBuilding, city),
                Times.AtLeastOnce, "BuildingFactory did not check BuildingPossessionCanon before " +
                "placing newBuilding into the argued city"
            );

            MockPossessionCanon.Verify(canon => canon.ChangeOwnerOfPossession(newBuilding, city),
                Times.Once, "BuildingFactory did not place newBuilding into the argued city as expected");
        }

        [Test(Description = "All methods should throw an ArgumentNullException on any null arguments")]
        public void AllMethods_ThrowOnNullArguments() {
            var template = BuildTemplate("Template", true);

            var city = BuildCity();

            var factory = Container.Resolve<BuildingFactory>();

            Assert.Throws<ArgumentNullException>(() => factory.CanConstructTemplateInCity(null, city),
                "CanConstructTemplateInCity failed to throw on a null template argument");

            Assert.Throws<ArgumentNullException>(() => factory.CanConstructTemplateInCity(template, null),
                "CanConstructTemplateInCity failed to throw on a null city argument");

            Assert.Throws<ArgumentNullException>(() => factory.Create(null, city),
                "Create failed to throw on a null template argument");

            Assert.Throws<ArgumentNullException>(() => factory.Create(template, null),
                "Create failed to throw on a null city argument");
        }

        [Test(Description = "If Create is called and CanConstructTemplateInCity would return false, " +
            "CityFactory should throw a BuildingConstructionException")]
        public void CreateCalled_ThrowsIfTemplateInvalid() {
            var template = BuildTemplate("Template", false);

            ICity city = BuildCity();

            var factory = Container.Resolve<BuildingFactory>();
            
            Assert.Throws<BuildingCreationException>(() => factory.Create(template, city),
                "BuildingFactory.Create failed to throw as expected");
        }

        [Test(Description = "If Create is called and BuildingPossessionCanon.CanChangeOwnerOfPossession" +
            "would return false, CityFactory should throw a BuildingConstructionException")]
        public void CreateCalled_ThrowsIfCityLocationInvalid() {
            var template = BuildTemplate("Template", true);

            ICity city = BuildCity();

            MockPossessionCanon.Setup(canon => canon.CanChangeOwnerOfPossession(It.IsAny<IBuilding>(), city)).Returns(false);

            var factory = Container.Resolve<BuildingFactory>();

            Assert.Throws<BuildingCreationException>(() => factory.Create(template, city),
                "BuildingFactory.Create failed to throw as expected");
        }

        #endregion

        #region utilities

        private IBuildingTemplate BuildTemplate(string name, bool isValid) {
            var mockTemplate = new Mock<IBuildingTemplate>();

            mockTemplate.Setup(template => template.name).Returns(name);
            mockTemplate.Setup(template => template.SlotYields).Returns(new List<ResourceSummary>().AsReadOnly());

            MockValidityLogic.Setup(logic => logic.IsTemplateValidForCity(mockTemplate.Object, It.IsAny<ICity>()))
                .Returns(isValid);

            return mockTemplate.Object;
        }

        private IBuilding BuildBuilding(IBuildingTemplate template) {
            var mockBuilding = new Mock<IBuilding>();

            mockBuilding.Setup(building => building.Template).Returns(template);

            return mockBuilding.Object;
        }

        private ICity BuildCity(params IBuilding[] buildings) {
            var newCity = new Mock<ICity>().Object;

            MockPossessionCanon.Setup(canon => canon.GetPossessionsOfOwner(newCity))
                .Returns(buildings.ToList().AsReadOnly());

            MockPossessionCanon.Setup(canon => canon.CanChangeOwnerOfPossession(It.IsAny<IBuilding>(), newCity))
                .Returns(true);

            return newCity;
        }

        #endregion

        #endregion

    }

}
