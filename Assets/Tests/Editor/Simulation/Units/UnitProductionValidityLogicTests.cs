using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation;
using Assets.Simulation.Civilizations;
using Assets.Simulation.Units;
using Assets.Simulation.Cities;
using Assets.Simulation.HexMap;
using Assets.Simulation.MapResources;

using UnityCustomUtilities.Extensions;

namespace Assets.Tests.Simulation.Units {

    [TestFixture]
    public class UnitProductionValidityLogicTests : ZenjectUnitTestFixture {

        #region internal types

        public class IsTemplateValidForCityTestData {

            public List<string> AllResourceTypes = new List<string>();

            public UnitTemplateTestData TemplateToCheck;

            public CityTestData CityToCheck;

        }

        public struct UnitTemplateTestData {

            public bool CanBePlacedInCity;

            public List<string> ResourcesRequired;

        }

        public class CityTestData {

            public List<string> ResourcesAvailableToOwner = new List<string>();

        }

        #endregion

        #region static fields and properties

        private static IEnumerable IsTemplateValidForCityTestCases {
            get {
                yield return new TestCaseData(new IsTemplateValidForCityTestData() {
                    TemplateToCheck = new UnitTemplateTestData() {
                        CanBePlacedInCity = true,
                        ResourcesRequired = new List<string>()
                    }
                }).SetName("Template can be placed at city location").Returns(true);

                yield return new TestCaseData(new IsTemplateValidForCityTestData() {
                    TemplateToCheck = new UnitTemplateTestData() {
                        CanBePlacedInCity = false,
                        ResourcesRequired = new List<string>()
                    }
                }).SetName("Template cannot be placed at city location").Returns(false);


                yield return new TestCaseData(new IsTemplateValidForCityTestData() {
                    AllResourceTypes = new List<string>() { "Resource1", "Resource2", "Resource3" },
                    TemplateToCheck = new UnitTemplateTestData() {
                        CanBePlacedInCity = true,
                        ResourcesRequired = new List<string>() { "Resource1" }
                    },
                    CityToCheck = new CityTestData() {
                        ResourcesAvailableToOwner = new List<string>() { "Resource1" }
                    }
                }).SetName("Unit requires 1 resource, city's owner has access to it").Returns(true);

                yield return new TestCaseData(new IsTemplateValidForCityTestData() {
                    AllResourceTypes = new List<string>() { "Resource1", "Resource2", "Resource3" },
                    TemplateToCheck = new UnitTemplateTestData() {
                        CanBePlacedInCity = true,
                        ResourcesRequired = new List<string>() { "Resource1" }
                    },
                    CityToCheck = new CityTestData() {
                        ResourcesAvailableToOwner = new List<string>() { "Resource2" }
                    }
                }).SetName("Unit requires 1 resource, city's owner does not have access to it").Returns(false);


                yield return new TestCaseData(new IsTemplateValidForCityTestData() {
                    AllResourceTypes = new List<string>() { "Resource1", "Resource2", "Resource3" },
                    TemplateToCheck = new UnitTemplateTestData() {
                        CanBePlacedInCity = true,
                        ResourcesRequired = new List<string>() { "Resource1", "Resource2" }
                    },
                    CityToCheck = new CityTestData() {
                        ResourcesAvailableToOwner = new List<string>() { "Resource1", "Resource2" }
                    }
                }).SetName("Unit requires 2 resources, city's owner has access to both").Returns(true);

                yield return new TestCaseData(new IsTemplateValidForCityTestData() {
                    AllResourceTypes = new List<string>() { "Resource1", "Resource2", "Resource3" },
                    TemplateToCheck = new UnitTemplateTestData() {
                        CanBePlacedInCity = true,
                        ResourcesRequired = new List<string>() { "Resource1", "Resource2" }
                    },
                    CityToCheck = new CityTestData() {
                        ResourcesAvailableToOwner = new List<string>() { "Resource1" }
                    }
                }).SetName("Unit requires 2 resources, city's owner has access to only one").Returns(false);

                yield return new TestCaseData(new IsTemplateValidForCityTestData() {
                    AllResourceTypes = new List<string>() { "Resource1", "Resource2", "Resource3" },
                    TemplateToCheck = new UnitTemplateTestData() {
                        CanBePlacedInCity = true,
                        ResourcesRequired = new List<string>() { "Resource1", "Resource2" }
                    },
                    CityToCheck = new CityTestData() {
                        ResourcesAvailableToOwner = new List<string>() {  }
                    }
                }).SetName("Unit requires 2 resources, city's owner has access to none").Returns(false);
            }
        }

        #endregion

        #region instance fields and properties

        private Mock<IUnitPositionCanon>                            MockPositionCanon;
        private Mock<IFreeResourcesLogic>                           MockFreeResourcesLogic;
        private Mock<IPossessionRelationship<ICivilization, ICity>> MockCityPossessionCanon;
        private Mock<IPossessionRelationship<IHexCell, ICity>>      MockCityLocationCanon;

        private List<IUnitTemplate> AllTemplates = new List<IUnitTemplate>();

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            AllTemplates.Clear();

            MockPositionCanon       = new Mock<IUnitPositionCanon>();
            MockFreeResourcesLogic  = new Mock<IFreeResourcesLogic>();
            MockCityPossessionCanon = new Mock<IPossessionRelationship<ICivilization, ICity>>();
            MockCityLocationCanon   = new Mock<IPossessionRelationship<IHexCell, ICity>>();
            
            Container.Bind<IUnitPositionCanon>                           ().FromInstance(MockPositionCanon      .Object);
            Container.Bind<IFreeResourcesLogic>                          ().FromInstance(MockFreeResourcesLogic .Object);
            Container.Bind<IPossessionRelationship<ICivilization, ICity>>().FromInstance(MockCityPossessionCanon.Object);
            Container.Bind<IPossessionRelationship<IHexCell, ICity>>     ().FromInstance(MockCityLocationCanon  .Object);

            Container.Bind<IEnumerable<IUnitTemplate>>().WithId("Available Unit Templates").FromInstance(AllTemplates);

            Container.Bind<UnitProductionValidityLogic>().AsSingle();
        }

        #endregion

        #region tests

        [Test(Description = "")]
        [TestCaseSource("IsTemplateValidForCityTestCases")]
        public bool IsTemplateValidForCityTests(IsTemplateValidForCityTestData data){
            var allResourceDefinitions = data.AllResourceTypes.Select(resourceName => BuildResourceDefinition(resourceName)).ToList();

            var template = BuildTemplate(
                allResourceDefinitions.Where(resource => data.TemplateToCheck.ResourcesRequired.Contains(resource.name)).ToList()
            );

            var city = BuildCity(
                BuildCivilization(), BuildCell(data.TemplateToCheck.CanBePlacedInCity),
                allResourceDefinitions.Where(resource => data.CityToCheck.ResourcesAvailableToOwner.Contains(resource.name)).ToList()
            );

            var validityLogic = Container.Resolve<UnitProductionValidityLogic>();

            return validityLogic.IsTemplateValidForCity(template, city);
        }

        #endregion

        #region utilities

        private IUnitTemplate BuildTemplate(IEnumerable<IResourceDefinition> requiredResources) {
            var mockTemplate = new Mock<IUnitTemplate>();

            mockTemplate.Setup(template => template.RequiredResources).Returns(requiredResources);

            AllTemplates.Add(mockTemplate.Object);

            return mockTemplate.Object;
        }

        private ICity BuildCity(ICivilization owner, IHexCell location,
            IEnumerable<IResourceDefinition> resourcesAvailableToOwner
        ){
            var mockCity = new Mock<ICity>();

            var newCity = mockCity.Object;

            MockCityLocationCanon.Setup(canon => canon.GetOwnerOfPossession(newCity)).Returns(location);
            MockCityPossessionCanon.Setup(canon => canon.GetOwnerOfPossession(newCity)).Returns(owner);

            foreach(var resource in resourcesAvailableToOwner) {
                MockFreeResourcesLogic
                    .Setup(canon => canon.GetFreeCopiesOfResourceForCiv(resource, owner)).Returns(1);
            }

            return newCity;
        }

        private IResourceDefinition BuildResourceDefinition(string name) {
            var mockDefinition = new Mock<IResourceDefinition>();

            mockDefinition.Setup(definition => definition.name).Returns(name);
            mockDefinition.Name = name;

            return mockDefinition.Object;
        }

        private ICivilization BuildCivilization() {
            return new Mock<ICivilization>().Object;
        }

        private IHexCell BuildCell(bool acceptsAllTemplates) {
            var newCell = new Mock<IHexCell>().Object;

            MockPositionCanon.Setup(
                canon => canon.CanPlaceUnitTemplateAtLocation(It.IsAny<IUnitTemplate>(), newCell, It.IsAny<ICivilization>())
            ).Returns(acceptsAllTemplates);

            return newCell;
        }

        #endregion

        #endregion

    }

}
