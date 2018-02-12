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
using Assets.Simulation.SpecialtyResources;

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

            public UnitType Type;

            public List<string> ResourcesRequired;

        }

        public class CityTestData {

            public List<UnitType> UnitTypesValidForLocation = new List<UnitType>();

            public List<string> ResourcesAvailableToOwner = new List<string>();

        }

        #endregion

        #region static fields and properties

        private static IEnumerable IsTemplateValidForCityTestCases {
            get {
                yield return new TestCaseData(new IsTemplateValidForCityTestData() {
                    TemplateToCheck = new UnitTemplateTestData() { Type = UnitType.LandMilitary },
                    CityToCheck = new CityTestData() {
                        UnitTypesValidForLocation = new List<UnitType>() { UnitType.LandMilitary }
                    }
                }).SetName("Template is LandMilitary, city can accept LandMilitary").Returns(true);

                yield return new TestCaseData(new IsTemplateValidForCityTestData() {
                    TemplateToCheck = new UnitTemplateTestData() { Type = UnitType.LandMilitary },
                    CityToCheck = new CityTestData() {
                        UnitTypesValidForLocation = new List<UnitType>() { UnitType.LandCivilian }
                    }
                }).SetName("Template is LandMilitary, city cannot accept LandMilitary").Returns(false);


                yield return new TestCaseData(new IsTemplateValidForCityTestData() {
                    TemplateToCheck = new UnitTemplateTestData() { Type = UnitType.LandCivilian },
                    CityToCheck = new CityTestData() {
                        UnitTypesValidForLocation = new List<UnitType>() { UnitType.LandCivilian }
                    }
                }).SetName("Template is LandCivilian, city can accept LandCivilian").Returns(true);

                yield return new TestCaseData(new IsTemplateValidForCityTestData() {
                    TemplateToCheck = new UnitTemplateTestData() { Type = UnitType.LandCivilian },
                    CityToCheck = new CityTestData() {
                        UnitTypesValidForLocation = new List<UnitType>() { UnitType.LandMilitary }
                    }
                }).SetName("Template is LandCivilian, city cannot accept LandCivilian").Returns(false);


                yield return new TestCaseData(new IsTemplateValidForCityTestData() {
                    TemplateToCheck = new UnitTemplateTestData() { Type = UnitType.WaterMilitary },
                    CityToCheck = new CityTestData() {
                        UnitTypesValidForLocation = new List<UnitType>() { UnitType.WaterMilitary }
                    }
                }).SetName("Template is WaterMilitary, city can accept WaterMilitary").Returns(true);

                yield return new TestCaseData(new IsTemplateValidForCityTestData() {
                    TemplateToCheck = new UnitTemplateTestData() { Type = UnitType.WaterMilitary },
                    CityToCheck = new CityTestData() {
                        UnitTypesValidForLocation = new List<UnitType>() { UnitType.WaterCivilian }
                    }
                }).SetName("Template is WaterMilitary, city cannot accept WaterMilitary").Returns(false);


                yield return new TestCaseData(new IsTemplateValidForCityTestData() {
                    TemplateToCheck = new UnitTemplateTestData() { Type = UnitType.WaterCivilian },
                    CityToCheck = new CityTestData() {
                        UnitTypesValidForLocation = new List<UnitType>() { UnitType.WaterCivilian }
                    }
                }).SetName("Template is WaterCivilian, city can accept WaterCivilian").Returns(true);

                yield return new TestCaseData(new IsTemplateValidForCityTestData() {
                    TemplateToCheck = new UnitTemplateTestData() { Type = UnitType.WaterCivilian },
                    CityToCheck = new CityTestData() {
                        UnitTypesValidForLocation = new List<UnitType>() { UnitType.LandMilitary }
                    }
                }).SetName("Template is WaterCivilian, city cannot accept WaterCivilian").Returns(false);


                yield return new TestCaseData(new IsTemplateValidForCityTestData() {
                    AllResourceTypes = new List<string>() { "Resource1", "Resource2", "Resource3" },
                    TemplateToCheck = new UnitTemplateTestData() {
                        Type = UnitType.LandMilitary,
                        ResourcesRequired = new List<string>() { "Resource1" }
                    },
                    CityToCheck = new CityTestData() {
                        UnitTypesValidForLocation = new List<UnitType>() { UnitType.LandMilitary },
                        ResourcesAvailableToOwner = new List<string>() { "Resource1" }
                    }
                }).SetName("Unit requires 1 resource, city's owner has access to it").Returns(true);

                yield return new TestCaseData(new IsTemplateValidForCityTestData() {
                    AllResourceTypes = new List<string>() { "Resource1", "Resource2", "Resource3" },
                    TemplateToCheck = new UnitTemplateTestData() {
                        Type = UnitType.LandMilitary,
                        ResourcesRequired = new List<string>() { "Resource1" }
                    },
                    CityToCheck = new CityTestData() {
                        UnitTypesValidForLocation = new List<UnitType>() { UnitType.LandMilitary },
                        ResourcesAvailableToOwner = new List<string>() { "Resource2" }
                    }
                }).SetName("Unit requires 1 resource, city's owner does not have access to it").Returns(false);


                yield return new TestCaseData(new IsTemplateValidForCityTestData() {
                    AllResourceTypes = new List<string>() { "Resource1", "Resource2", "Resource3" },
                    TemplateToCheck = new UnitTemplateTestData() {
                        Type = UnitType.LandMilitary,
                        ResourcesRequired = new List<string>() { "Resource1", "Resource2" }
                    },
                    CityToCheck = new CityTestData() {
                        UnitTypesValidForLocation = new List<UnitType>() { UnitType.LandMilitary },
                        ResourcesAvailableToOwner = new List<string>() { "Resource1", "Resource2" }
                    }
                }).SetName("Unit requires 2 resources, city's owner has access to both").Returns(true);

                yield return new TestCaseData(new IsTemplateValidForCityTestData() {
                    AllResourceTypes = new List<string>() { "Resource1", "Resource2", "Resource3" },
                    TemplateToCheck = new UnitTemplateTestData() {
                        Type = UnitType.LandMilitary,
                        ResourcesRequired = new List<string>() { "Resource1", "Resource2" }
                    },
                    CityToCheck = new CityTestData() {
                        UnitTypesValidForLocation = new List<UnitType>() { UnitType.LandMilitary },
                        ResourcesAvailableToOwner = new List<string>() { "Resource1" }
                    }
                }).SetName("Unit requires 2 resources, city's owner has access to only one").Returns(false);

                yield return new TestCaseData(new IsTemplateValidForCityTestData() {
                    AllResourceTypes = new List<string>() { "Resource1", "Resource2", "Resource3" },
                    TemplateToCheck = new UnitTemplateTestData() {
                        Type = UnitType.LandMilitary,
                        ResourcesRequired = new List<string>() { "Resource1", "Resource2" }
                    },
                    CityToCheck = new CityTestData() {
                        UnitTypesValidForLocation = new List<UnitType>() { UnitType.LandMilitary },
                        ResourcesAvailableToOwner = new List<string>() {  }
                    }
                }).SetName("Unit requires 2 resources, city's owner has access to none").Returns(false);
            }
        }

        #endregion

        #region instance fields and properties

        private Mock<IUnitPositionCanon> MockPositionCanon;

        private Mock<IResourceAssignmentCanon> MockResourceAssignmentCanon;

        private Mock<IPossessionRelationship<ICivilization, ICity>> MockCityPossessionCanon;

        private List<IUnitTemplate> AllTemplates = new List<IUnitTemplate>();

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            AllTemplates.Clear();

            MockPositionCanon           = new Mock<IUnitPositionCanon>();
            MockResourceAssignmentCanon = new Mock<IResourceAssignmentCanon>();
            MockCityPossessionCanon     = new Mock<IPossessionRelationship<ICivilization, ICity>>();
            
            Container.Bind<IUnitPositionCanon>                           ().FromInstance(MockPositionCanon          .Object);
            Container.Bind<IResourceAssignmentCanon>                     ().FromInstance(MockResourceAssignmentCanon.Object);
            Container.Bind<IPossessionRelationship<ICivilization, ICity>>().FromInstance(MockCityPossessionCanon    .Object);

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
                data.TemplateToCheck.Type,
                allResourceDefinitions.Where(resource => data.TemplateToCheck.ResourcesRequired.Contains(resource.name)).ToList()
            );

            var city = BuildCity(
                BuildCivilization(),
                data.CityToCheck.UnitTypesValidForLocation,
                allResourceDefinitions.Where(resource => data.CityToCheck.ResourcesAvailableToOwner.Contains(resource.name)).ToList()
            );

            var validityLogic = Container.Resolve<UnitProductionValidityLogic>();

            return validityLogic.IsTemplateValidForCity(template, city);
        }

        [Test(Description = "GetTemplatesValidForCity should return all templates " +
            "in AllTemplates for which IsTemplateValidForProductionInCity returns true")]
        public void GetTemplatesValidForCity_ReturnsAllValidTemplates() {
            var city = BuildCity(
                BuildCivilization(),
                new List<UnitType>() { UnitType.LandCivilian, UnitType.LandMilitary }, 
                new List<ISpecialtyResourceDefinition>()
            );

            var landCivilianTemplate  = BuildTemplate(UnitType.LandCivilian,  new List<ISpecialtyResourceDefinition>());
            var waterCivilianTemplate = BuildTemplate(UnitType.WaterCivilian, new List<ISpecialtyResourceDefinition>());

            var validityLogic = Container.Resolve<UnitProductionValidityLogic>();

            var validTemplates = validityLogic.GetTemplatesValidForCity(city);

            CollectionAssert.Contains(validTemplates, landCivilianTemplate, "GetTemplatesValidForCity is missing an expected template");
            CollectionAssert.DoesNotContain(validTemplates, waterCivilianTemplate, "GetTemplatesValidForCity contains an unexpected template");
        }

        #endregion

        #region utilities

        private IUnitTemplate BuildTemplate(UnitType type, IEnumerable<ISpecialtyResourceDefinition> requiredResources) {
            var mockTemplate = new Mock<IUnitTemplate>();

            mockTemplate.Setup(template => template.Type).Returns(type);
            mockTemplate.Setup(template => template.RequiredResources).Returns(requiredResources);

            AllTemplates.Add(mockTemplate.Object);

            return mockTemplate.Object;
        }

        private ICity BuildCity(
            ICivilization owner, List<UnitType> typesPermitted,
            IEnumerable<ISpecialtyResourceDefinition> resourcesAvailableToOwner
        ){
            var mockTile = new Mock<IHexCell>();

            MockPositionCanon
                .Setup(canon => canon.CanPlaceUnitOfTypeAtLocation(It.IsAny<UnitType>(), mockTile.Object, It.IsAny<bool>()))
                .Returns<UnitType, IHexCell, bool>((type, tile, ignoreOccupancy) => typesPermitted.Contains(type));

            var mockCity = new Mock<ICity>();
            mockCity.Setup(city => city.Location).Returns(mockTile.Object);

            var newCity = mockCity.Object;

            MockCityPossessionCanon.Setup(canon => canon.GetOwnerOfPossession(newCity)).Returns(owner);

            foreach(var resource in resourcesAvailableToOwner) {
                MockResourceAssignmentCanon
                    .Setup(canon => canon.GetFreeCopiesOfResourceForCiv(resource, owner)).Returns(1);
            }

            return newCity;
        }

        private ISpecialtyResourceDefinition BuildResourceDefinition(string name) {
            var mockDefinition = new Mock<ISpecialtyResourceDefinition>();

            mockDefinition.Setup(definition => definition.name).Returns(name);
            mockDefinition.Name = name;

            return mockDefinition.Object;
        }

        private ICivilization BuildCivilization() {
            return new Mock<ICivilization>().Object;
        }

        #endregion

        #endregion

    }

}
