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
using Assets.Simulation.HexMap;

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

            public bool RequiresAdjacentRiver = false;

            public List<int> RequiredResources = new List<int>();

            public List<int> PrerequisiteBuildings = new List<int>();

        }

        public class CityTestData {

            public HexCellTestData Location = new HexCellTestData();

            public List<HexCellTestData> LocationNeighbors = new List<HexCellTestData>();

            public List<int> ResourcesAvailableToOwner = new List<int>();

            public List<int> TemplatesAlreadyConstructed = new List<int>();

        }

        public class HexCellTestData {

            public bool HasRiver;

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
                            RequiredResources = new List<int>() { 0, 1 },
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
                            RequiredResources = new List<int>() { 0, 1 },
                            PrerequisiteBuildings = new List<int>() { }
                        }
                    },
                    TemplateToConsider = 0,
                    CityToConsider = new CityTestData() {
                        ResourcesAvailableToOwner = new List<int>() { }
                    }
                }).SetName("Template requiring 2 resources in a city with neither resource").Returns(false);


                yield return new TestCaseData(new IsTemplateValidForCityTestData() {
                    AvailableResources = new List<string>() { },
                    AvailableTemplates = new List<BuildingTemplateTestData>() {
                        new BuildingTemplateTestData() {
                            Name = "Template1",
                            RequiredResources = new List<int>() { },
                        },
                        new BuildingTemplateTestData() {
                            Name = "Template2",
                            RequiredResources = new List<int>() { }
                        },
                        new BuildingTemplateTestData() {
                            Name = "Template3",
                            RequiredResources = new List<int>() { },
                            PrerequisiteBuildings = new List<int>() { 0, 1 }
                        },
                    },
                    TemplateToConsider = 2,
                    CityToConsider = new CityTestData() {
                        
                    }
                }).SetName("Template with two prerequisites, neither of which is present").Returns(false);


                yield return new TestCaseData(new IsTemplateValidForCityTestData() {
                    AvailableResources = new List<string>() { },
                    AvailableTemplates = new List<BuildingTemplateTestData>() {
                        new BuildingTemplateTestData() {
                            Name = "Template1",
                            RequiredResources = new List<int>() { },
                        },
                        new BuildingTemplateTestData() {
                            Name = "Template2",
                            RequiredResources = new List<int>() { }
                        },
                        new BuildingTemplateTestData() {
                            Name = "Template3",
                            RequiredResources = new List<int>() { },
                            PrerequisiteBuildings = new List<int>() { 0, 1 }
                        },
                    },
                    TemplateToConsider = 2,
                    CityToConsider = new CityTestData() {
                        TemplatesAlreadyConstructed = new List<int>() { 0 }
                    }
                }).SetName("Template with two prerequisites, one of which is present").Returns(false);


                yield return new TestCaseData(new IsTemplateValidForCityTestData() {
                    AvailableResources = new List<string>() { },
                    AvailableTemplates = new List<BuildingTemplateTestData>() {
                        new BuildingTemplateTestData() {
                            Name = "Template1",
                            RequiredResources = new List<int>() { },
                        },
                        new BuildingTemplateTestData() {
                            Name = "Template2",
                            RequiredResources = new List<int>() { }
                        },
                        new BuildingTemplateTestData() {
                            Name = "Template3",
                            RequiredResources = new List<int>() { },
                            PrerequisiteBuildings = new List<int>() { 0, 1 }
                        },
                    },
                    TemplateToConsider = 2,
                    CityToConsider = new CityTestData() {
                        TemplatesAlreadyConstructed = new List<int>() { 0, 1 }
                    }
                }).SetName("Template with two prerequisites, both of which are present").Returns(true);


                yield return new TestCaseData(new IsTemplateValidForCityTestData() {
                    AvailableResources = new List<string>() { },
                    AvailableTemplates = new List<BuildingTemplateTestData>() {
                        new BuildingTemplateTestData() {
                            Name = "Template1",
                            RequiresAdjacentRiver = true,
                            RequiredResources = new List<int>() { }
                        }
                    },
                    TemplateToConsider = 0,
                    CityToConsider = new CityTestData() {
                        Location = new HexCellTestData(),
                        LocationNeighbors = new List<HexCellTestData>() {
                            new HexCellTestData() { HasRiver = false },
                            new HexCellTestData() { HasRiver = true },
                        }
                    }
                }).SetName("Template requires river adjacency, adjacent river is present").Returns(true);

                yield return new TestCaseData(new IsTemplateValidForCityTestData() {
                    AvailableResources = new List<string>() { },
                    AvailableTemplates = new List<BuildingTemplateTestData>() {
                        new BuildingTemplateTestData() {
                            Name = "Template1",
                            RequiresAdjacentRiver = true,
                            RequiredResources = new List<int>() { }
                        }
                    },
                    TemplateToConsider = 0,
                    CityToConsider = new CityTestData() {
                        Location = new HexCellTestData(),
                        LocationNeighbors = new List<HexCellTestData>() {
                            new HexCellTestData() { HasRiver = false },
                            new HexCellTestData() { HasRiver = false },
                        }
                    }
                }).SetName("Template requires river adjacency, no adjacent river is present").Returns(false);
            }
        }

        #endregion

        #region instance fields and properties

        private List<IBuildingTemplate> AvailableTemplates = new List<IBuildingTemplate>();

        private Mock<IPossessionRelationship<ICity, IBuilding>>     MockBuildingPossessionCanon;
        private Mock<IPossessionRelationship<ICivilization, ICity>> MockCityPossessionCanon;
        private Mock<IResourceAssignmentCanon>                      MockResourceAssignmentCanon;
        private Mock<IPossessionRelationship<IHexCell, ICity>>      MockCityLocationCanon;
        private Mock<IHexGrid>                                      MockGrid;
        private Mock<IRiverCanon>                                   MockRiverCanon;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            AvailableTemplates.Clear();

            MockBuildingPossessionCanon = new Mock<IPossessionRelationship<ICity, IBuilding>>();
            MockCityPossessionCanon     = new Mock<IPossessionRelationship<ICivilization, ICity>>();
            MockResourceAssignmentCanon = new Mock<IResourceAssignmentCanon>();
            MockCityLocationCanon       = new Mock<IPossessionRelationship<IHexCell, ICity>>();
            MockGrid                    = new Mock<IHexGrid>();
            MockRiverCanon              = new Mock<IRiverCanon>();

            Container.Bind<List<IBuildingTemplate>>().FromInstance(AvailableTemplates);

            Container.Bind<IPossessionRelationship<ICity, IBuilding>>    ().FromInstance(MockBuildingPossessionCanon.Object);
            Container.Bind<IPossessionRelationship<ICivilization, ICity>>().FromInstance(MockCityPossessionCanon    .Object);
            Container.Bind<IResourceAssignmentCanon>                     ().FromInstance(MockResourceAssignmentCanon.Object);
            Container.Bind<IPossessionRelationship<IHexCell, ICity>>     ().FromInstance(MockCityLocationCanon      .Object);
            Container.Bind<IHexGrid>                                     ().FromInstance(MockGrid                   .Object);
            Container.Bind<IRiverCanon>                                  ().FromInstance(MockRiverCanon             .Object);

            Container.Bind<BuildingProductionValidityLogic>().AsSingle();
        }

        #endregion

        #region tests

        [Test(Description = "")]
        [TestCaseSource("IsTemplateValidForCityTestCases")]
        public bool IsTemplateValidForCityTests(IsTemplateValidForCityTestData data) {
            var allResources = data.AvailableResources.Select(name => BuildResourceDefinition(name)).ToList();

            var allTemplates = new List<IBuildingTemplate>();
            foreach(var templateData in data.AvailableTemplates) {
                allTemplates.Add(BuildTemplate(templateData, allTemplates, allResources));
            }

            var templatesInCity = data.CityToConsider.TemplatesAlreadyConstructed.Select(
                templateIndex => allTemplates[templateIndex]
            );

            var civilization = BuildCivilization(
                data.CityToConsider.ResourcesAvailableToOwner.Select(resourceIndex => allResources[resourceIndex])
            );

            var cityToConsider = BuildCity(data.CityToConsider, civilization, templatesInCity.Select(template => BuildBuilding(template)));

            var validityLogic = Container.Resolve<BuildingProductionValidityLogic>();

            return validityLogic.IsTemplateValidForCity(allTemplates[data.TemplateToConsider], cityToConsider);
        }

        #endregion

        #region utilities

        private ISpecialtyResourceDefinition BuildResourceDefinition(string name) {
            var mockDefinition = new Mock<ISpecialtyResourceDefinition>();

            mockDefinition.Name = name;
            mockDefinition.Setup(definition => definition.name).Returns(name);

            return mockDefinition.Object;
        }

        private IBuildingTemplate BuildTemplate(
            BuildingTemplateTestData templateData,
            List<IBuildingTemplate> allTemplates,
            List<ISpecialtyResourceDefinition> allResources
        ){
            var mockTemplate = new Mock<IBuildingTemplate>();
            mockTemplate.Name = templateData.Name;

            mockTemplate.Setup(template => template.name                 ).Returns(templateData.Name);
            mockTemplate.Setup(template => template.RequiresAdjacentRiver).Returns(templateData.RequiresAdjacentRiver);

            
            mockTemplate
                .Setup(template => template.RequiredResources)
                .Returns(templateData.RequiredResources.Select(index => allResources[index]));

            mockTemplate
                .Setup(template => template.PrerequisiteBuildings)
                .Returns(templateData.PrerequisiteBuildings.Select(index => allTemplates[index]));

            AvailableTemplates.Add(mockTemplate.Object);

            return mockTemplate.Object;
        }

        private IBuilding BuildBuilding(IBuildingTemplate template) {
            var mockBuilding = new Mock<IBuilding>();

            mockBuilding.Name = template.name;
            mockBuilding.Setup(building => building.Template).Returns(template);

            return mockBuilding.Object;
        }

        private ICity BuildCity(
            CityTestData cityData, ICivilization owner, IEnumerable<IBuilding> buildings
        ){
            var mockCity = new Mock<ICity>();

            var newCity = mockCity.Object;

            var cityLocation     = BuildCell(cityData.Location);
            var locationNeighbrs = cityData.LocationNeighbors.Select(data => BuildCell(data));

            MockCityLocationCanon.Setup(canon => canon.GetOwnerOfPossession(newCity)).Returns(cityLocation);
            MockGrid             .Setup(grid => grid.GetNeighbors(cityLocation)).Returns(locationNeighbrs.ToList());

            MockCityPossessionCanon.Setup(canon => canon.GetOwnerOfPossession(newCity)).Returns(owner);

            MockBuildingPossessionCanon.Setup(canon => canon.GetPossessionsOfOwner(newCity)).Returns(buildings);

            return newCity;
        }

        private ICivilization BuildCivilization(IEnumerable<ISpecialtyResourceDefinition> availableResources) {
            var newCiv = new Mock<ICivilization>().Object;

            foreach(var resource in availableResources) {
                MockResourceAssignmentCanon.Setup(canon => canon.GetFreeCopiesOfResourceForCiv(resource, newCiv)).Returns(1);
            }
            
            return newCiv;
        }

        private IHexCell BuildCell(HexCellTestData cellData) {
            var newCell = new Mock<IHexCell>().Object;

            MockRiverCanon.Setup(canon => canon.HasRiver(newCell)).Returns(cellData.HasRiver);

            return newCell;
        }

        #endregion

        #endregion

    }

}
