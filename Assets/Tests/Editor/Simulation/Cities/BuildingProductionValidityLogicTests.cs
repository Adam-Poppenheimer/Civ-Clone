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
using Assets.Simulation.SpecialtyResources;

namespace Assets.Tests.Simulation.Cities {

    [TestFixture]
    public class BuildingProductionValidityLogicTests :ZenjectUnitTestFixture {

        #region internal types

        public class IsTemplateValidForCityTestData {

            public List<string> AvailableResources = new List<string>();

            public List<BuildingTemplateTestData> AvailableTemplates;

            public int TemplateToConsider;

            public CityTestData CityToConsider;

        }

        public class BuildingTemplateTestData {

            public string Name;

            public List<int> RequiredResources = new List<int>();

        }

        public class CityTestData {

            public List<int> ResourcesAvailableToOwner = new List<int>();

            public List<int> TemplatesAlreadyConstructed = new List<int>();

        }

        #endregion

        #region static fields and properties

        public static IEnumerable IsTemplateValidForCityTestCases {
            get {
                yield return new TestCaseData(new IsTemplateValidForCityTestData() {
                    AvailableResources = new List<string>() { },
                    AvailableTemplates = new List<BuildingTemplateTestData>() {
                        new BuildingTemplateTestData() {
                            Name = "Template1",
                            RequiredResources = new List<int>() { }
                        }
                    },
                    TemplateToConsider = 0,
                    CityToConsider = new CityTestData() {
                        
                    }
                }).SetName("Template in empty city").Returns(true);


                yield return new TestCaseData(new IsTemplateValidForCityTestData() {
                    AvailableResources = new List<string>() { },
                    AvailableTemplates = new List<BuildingTemplateTestData>() {
                        new BuildingTemplateTestData() {
                            Name = "Template1",
                            RequiredResources = new List<int>() { }
                        }
                    },
                    TemplateToConsider = 0,
                    CityToConsider = new CityTestData() {
                        TemplatesAlreadyConstructed = new List<int>() { 0 }
                    }
                }).SetName("Template in city that has building of template").Returns(false);


                yield return new TestCaseData(new IsTemplateValidForCityTestData() {
                    AvailableResources = new List<string>() {
                        "Resource1"
                    },
                    AvailableTemplates = new List<BuildingTemplateTestData>() {
                        new BuildingTemplateTestData() {
                            Name = "Template1",
                            RequiredResources = new List<int>() { 0 }
                        }
                    },
                    TemplateToConsider = 0,
                    CityToConsider = new CityTestData() {
                        ResourcesAvailableToOwner = new List<int>() { 0 }
                    }
                }).SetName("Template requiring a resource in a city with that resource").Returns(true);

                yield return new TestCaseData(new IsTemplateValidForCityTestData() {
                    AvailableResources = new List<string>() {
                        "Resource1"
                    },
                    AvailableTemplates = new List<BuildingTemplateTestData>() {
                        new BuildingTemplateTestData() {
                            Name = "Template1",
                            RequiredResources = new List<int>() { 0 }
                        }
                    },
                    TemplateToConsider = 0,
                    CityToConsider = new CityTestData() {
                        ResourcesAvailableToOwner = new List<int>() {  }
                    }
                }).SetName("Template requiring a resource in a city without that resource").Returns(false);


                yield return new TestCaseData(new IsTemplateValidForCityTestData() {
                    AvailableResources = new List<string>() {
                        "Resource1", "Resource2"
                    },
                    AvailableTemplates = new List<BuildingTemplateTestData>() {
                        new BuildingTemplateTestData() {
                            Name = "Template1",
                            RequiredResources = new List<int>() { 0, 1 }
                        }
                    },
                    TemplateToConsider = 0,
                    CityToConsider = new CityTestData() {
                        ResourcesAvailableToOwner = new List<int>() { 0, 1 }
                    }
                }).SetName("Template requiring 2 resources in a city with both resources").Returns(true);

                yield return new TestCaseData(new IsTemplateValidForCityTestData() {
                    AvailableResources = new List<string>() {
                        "Resource1", "Resource2"
                    },
                    AvailableTemplates = new List<BuildingTemplateTestData>() {
                        new BuildingTemplateTestData() {
                            Name = "Template1",
                            RequiredResources = new List<int>() { 0, 1 }
                        }
                    },
                    TemplateToConsider = 0,
                    CityToConsider = new CityTestData() {
                        ResourcesAvailableToOwner = new List<int>() { 1 }
                    }
                }).SetName("Template requiring 2 resources in a city with only one resource").Returns(false);

                yield return new TestCaseData(new IsTemplateValidForCityTestData() {
                    AvailableResources = new List<string>() {
                        "Resource1", "Resource2"
                    },
                    AvailableTemplates = new List<BuildingTemplateTestData>() {
                        new BuildingTemplateTestData() {
                            Name = "Template1",
                            RequiredResources = new List<int>() { 0, 1 }
                        }
                    },
                    TemplateToConsider = 0,
                    CityToConsider = new CityTestData() {
                        ResourcesAvailableToOwner = new List<int>() { }
                    }
                }).SetName("Template requiring 2 resources in a city with neither resource").Returns(false);
            }
        }

        #endregion

        #region instance fields and properties

        private List<IBuildingTemplate> AvailableTemplates = new List<IBuildingTemplate>();

        private Mock<IPossessionRelationship<ICity, IBuilding>> MockBuildingPossessionCanon;

        private Mock<IPossessionRelationship<ICivilization, ICity>> MockCityPossessionCanon;

        private Mock<IResourceAssignmentCanon> MockResourceAssignmentCanon;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            AvailableTemplates.Clear();

            MockBuildingPossessionCanon = new Mock<IPossessionRelationship<ICity, IBuilding>>();
            MockCityPossessionCanon     = new Mock<IPossessionRelationship<ICivilization, ICity>>();
            MockResourceAssignmentCanon = new Mock<IResourceAssignmentCanon>();

            Container.Bind<List<IBuildingTemplate>>().FromInstance(AvailableTemplates);

            Container.Bind<IPossessionRelationship<ICity, IBuilding>>    ().FromInstance(MockBuildingPossessionCanon.Object);
            Container.Bind<IPossessionRelationship<ICivilization, ICity>>().FromInstance(MockCityPossessionCanon    .Object);
            Container.Bind<IResourceAssignmentCanon>                     ().FromInstance(MockResourceAssignmentCanon.Object);

            Container.Bind<BuildingProductionValidityLogic>().AsSingle();
        }

        #endregion

        #region tests

        [Test(Description = "")]
        [TestCaseSource("IsTemplateValidForCityTestCases")]
        public bool IsTemplateValidForCityTests(IsTemplateValidForCityTestData data) {
            var allResources = data.AvailableResources.Select(name => BuildResourceDefinition(name)).ToList();

            var allTemplates = data.AvailableTemplates.Select(templateData => BuildTemplate(
                templateData.Name,
                templateData.RequiredResources.Select(resourceIndex => allResources[resourceIndex])
            )).ToList();

            var templatesInCity = data.CityToConsider.TemplatesAlreadyConstructed.Select(
                templateIndex => allTemplates[templateIndex]
            );

            var civilization = BuildCivilization(
                data.CityToConsider.ResourcesAvailableToOwner.Select(resourceIndex => allResources[resourceIndex])
            );

            var cityToConsider = BuildCity(civilization, templatesInCity.Select(template => BuildBuilding(template)));

            var validityLogic = Container.Resolve<BuildingProductionValidityLogic>();

            return validityLogic.IsTemplateValidForCity(allTemplates[data.TemplateToConsider], cityToConsider);
        }

        #endregion

        #region utilities

        public ISpecialtyResourceDefinition BuildResourceDefinition(string name) {
            var mockDefinition = new Mock<ISpecialtyResourceDefinition>();

            mockDefinition.Name = name;
            mockDefinition.Setup(definition => definition.name).Returns(name);

            return mockDefinition.Object;
        }

        public IBuildingTemplate BuildTemplate(string name, IEnumerable<ISpecialtyResourceDefinition> requiredResources) {
            var mockTemplate = new Mock<IBuildingTemplate>();
            mockTemplate.Name = name;

            mockTemplate.Setup(template => template.name).Returns(name);
            mockTemplate.Setup(template => template.RequiredResources).Returns(requiredResources);

            AvailableTemplates.Add(mockTemplate.Object);

            return mockTemplate.Object;
        }

        public IBuilding BuildBuilding(IBuildingTemplate template) {
            var mockBuilding = new Mock<IBuilding>();

            mockBuilding.Name = template.name;
            mockBuilding.Setup(building => building.Template).Returns(template);

            return mockBuilding.Object;
        }

        public ICity BuildCity(ICivilization owner, IEnumerable<IBuilding> buildings) {
            var mockCity = new Mock<ICity>();

            var newCity = mockCity.Object;

            MockCityPossessionCanon.Setup(canon => canon.GetOwnerOfPossession(newCity)).Returns(owner);

            MockBuildingPossessionCanon.Setup(canon => canon.GetPossessionsOfOwner(newCity)).Returns(buildings);

            return newCity;
        }

        public ICivilization BuildCivilization(IEnumerable<ISpecialtyResourceDefinition> availableResources) {
            var newCiv = new Mock<ICivilization>().Object;

            foreach(var resource in availableResources) {
                MockResourceAssignmentCanon.Setup(canon => canon.GetFreeCopiesOfResourceForCiv(resource, newCiv)).Returns(1);
            }
            
            return newCiv;
        }

        #endregion

        #endregion

    }

}
