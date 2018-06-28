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
using Assets.Simulation.Improvements;

namespace Assets.Tests.Simulation.Cities {

    [TestFixture]
    public class BuildingProductionValidityLogicTests :ZenjectUnitTestFixture {

        #region internal types

        public class IsTemplateValidForCityTestData {

            public List<string> AvailableResources = new List<string>();

            public List<BuildingTemplateTestData> AvailableBuildings;

            public List<ImprovementTemplateTestData> AvailableImprovements =
                new List<ImprovementTemplateTestData>();

            public int TemplateToConsider;

            public CityTestData CityToConsider;

        }

        public class BuildingTemplateTestData {

            public string Name;

            public bool RequiresAdjacentRiver = false;

            public bool RequiresCoastalCity = false;

            public List<int> ConsumedResources = new List<int>();

            public List<int> PrerequisiteBuildings = new List<int>();

            public List<int> PrerequisiteResourcesInCity = new List<int>();

            public List<int> PrerequisiteImprovementsNearCity = new List<int>();

        }

        public class CityTestData {

            public HexCellTestData Location = new HexCellTestData();

            public List<HexCellTestData> LocationNeighbors = new List<HexCellTestData>();

            public List<int> ResourcesAvailableToOwner = new List<int>();

            public List<int> TemplatesAlreadyConstructed = new List<int>();

        }

        public class HexCellTestData {

            public bool HasRiver;

            public bool IsUnderwater;

            public int ResourceIndexInNode = -1;

            public int ImprovementIndex = -1;

        }

        public class ImprovementTemplateTestData {

            public string Name;

        }

        #endregion

        #region static fields and properties

        public static IEnumerable IsTemplateValidForCityTestCases {
            get {
                yield return new TestCaseData(new IsTemplateValidForCityTestData() {
                    AvailableResources = new List<string>() { },
                    AvailableBuildings = new List<BuildingTemplateTestData>() {
                        new BuildingTemplateTestData() {
                            Name = "Template1",
                            ConsumedResources = new List<int>() { }
                        }
                    },
                    TemplateToConsider = 0,
                    CityToConsider = new CityTestData() {
                        
                    }
                }).SetName("Template in empty city").Returns(true);


                yield return new TestCaseData(new IsTemplateValidForCityTestData() {
                    AvailableResources = new List<string>() { },
                    AvailableBuildings = new List<BuildingTemplateTestData>() {
                        new BuildingTemplateTestData() {
                            Name = "Template1",
                            ConsumedResources = new List<int>() { }
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
                    AvailableBuildings = new List<BuildingTemplateTestData>() {
                        new BuildingTemplateTestData() {
                            Name = "Template1",
                            ConsumedResources = new List<int>() { 0 }
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
                    AvailableBuildings = new List<BuildingTemplateTestData>() {
                        new BuildingTemplateTestData() {
                            Name = "Template1",
                            ConsumedResources = new List<int>() { 0 }
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
                    AvailableBuildings = new List<BuildingTemplateTestData>() {
                        new BuildingTemplateTestData() {
                            Name = "Template1",
                            ConsumedResources = new List<int>() { 0, 1 }
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
                    AvailableBuildings = new List<BuildingTemplateTestData>() {
                        new BuildingTemplateTestData() {
                            Name = "Template1",
                            ConsumedResources = new List<int>() { 0, 1 },
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
                    AvailableBuildings = new List<BuildingTemplateTestData>() {
                        new BuildingTemplateTestData() {
                            Name = "Template1",
                            ConsumedResources = new List<int>() { 0, 1 },
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
                    AvailableBuildings = new List<BuildingTemplateTestData>() {
                        new BuildingTemplateTestData() {
                            Name = "Template1",
                            ConsumedResources = new List<int>() { },
                        },
                        new BuildingTemplateTestData() {
                            Name = "Template2",
                            ConsumedResources = new List<int>() { }
                        },
                        new BuildingTemplateTestData() {
                            Name = "Template3",
                            ConsumedResources = new List<int>() { },
                            PrerequisiteBuildings = new List<int>() { 0, 1 }
                        },
                    },
                    TemplateToConsider = 2,
                    CityToConsider = new CityTestData() {
                        
                    }
                }).SetName("Template with two prerequisites, neither of which is present").Returns(false);


                yield return new TestCaseData(new IsTemplateValidForCityTestData() {
                    AvailableResources = new List<string>() { },
                    AvailableBuildings = new List<BuildingTemplateTestData>() {
                        new BuildingTemplateTestData() {
                            Name = "Template1",
                            ConsumedResources = new List<int>() { },
                        },
                        new BuildingTemplateTestData() {
                            Name = "Template2",
                            ConsumedResources = new List<int>() { }
                        },
                        new BuildingTemplateTestData() {
                            Name = "Template3",
                            ConsumedResources = new List<int>() { },
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
                    AvailableBuildings = new List<BuildingTemplateTestData>() {
                        new BuildingTemplateTestData() {
                            Name = "Template1",
                            ConsumedResources = new List<int>() { },
                        },
                        new BuildingTemplateTestData() {
                            Name = "Template2",
                            ConsumedResources = new List<int>() { }
                        },
                        new BuildingTemplateTestData() {
                            Name = "Template3",
                            ConsumedResources = new List<int>() { },
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
                    AvailableBuildings = new List<BuildingTemplateTestData>() {
                        new BuildingTemplateTestData() {
                            Name = "Template1",
                            RequiresAdjacentRiver = true,
                            ConsumedResources = new List<int>() { }
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
                    AvailableBuildings = new List<BuildingTemplateTestData>() {
                        new BuildingTemplateTestData() {
                            Name = "Template1",
                            RequiresAdjacentRiver = true,
                            ConsumedResources = new List<int>() { }
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


                yield return new TestCaseData(new IsTemplateValidForCityTestData() {
                    AvailableResources = new List<string>() { },
                    AvailableBuildings = new List<BuildingTemplateTestData>() {
                        new BuildingTemplateTestData() {
                            Name = "Template1",
                            RequiresCoastalCity = true,
                            ConsumedResources = new List<int>() { }
                        }
                    },
                    TemplateToConsider = 0,
                    CityToConsider = new CityTestData() {
                        Location = new HexCellTestData(),
                        LocationNeighbors = new List<HexCellTestData>() {
                            new HexCellTestData() { IsUnderwater = false },
                            new HexCellTestData() { IsUnderwater = true },
                        }
                    }
                }).SetName("Template requires coastal city, adjacent water is present").Returns(true);

                yield return new TestCaseData(new IsTemplateValidForCityTestData() {
                    AvailableResources = new List<string>() { },
                    AvailableBuildings = new List<BuildingTemplateTestData>() {
                        new BuildingTemplateTestData() {
                            Name = "Template1",
                            RequiresCoastalCity = true,
                            ConsumedResources = new List<int>() { }
                        }
                    },
                    TemplateToConsider = 0,
                    CityToConsider = new CityTestData() {
                        Location = new HexCellTestData(),
                        LocationNeighbors = new List<HexCellTestData>() {
                            new HexCellTestData() { IsUnderwater = false },
                            new HexCellTestData() { IsUnderwater = false },
                        }
                    }
                }).SetName("Template requires coastal city, no adjacent water is present").Returns(false);


                yield return new TestCaseData(new IsTemplateValidForCityTestData() {
                    AvailableResources = new List<string>() {
                        "Resource1", "Resource2"
                    },
                    AvailableBuildings = new List<BuildingTemplateTestData>() {
                        new BuildingTemplateTestData() {
                            Name = "Template1",
                            ConsumedResources = new List<int>() { },
                            PrerequisiteResourcesInCity = new List<int>() { 0, 1 }
                        }
                    },
                    TemplateToConsider = 0,
                    CityToConsider = new CityTestData() {
                        Location = new HexCellTestData(),
                        LocationNeighbors = new List<HexCellTestData>() {
                            new HexCellTestData() { ResourceIndexInNode = -1 },
                            new HexCellTestData() { ResourceIndexInNode = 0 },
                            new HexCellTestData() { ResourceIndexInNode = 1 },
                        }
                    }
                }).SetName("Template requires resources in city, both resources are present").Returns(true);

                yield return new TestCaseData(new IsTemplateValidForCityTestData() {
                    AvailableResources = new List<string>() {
                        "Resource1", "Resource2"
                    },
                    AvailableBuildings = new List<BuildingTemplateTestData>() {
                        new BuildingTemplateTestData() {
                            Name = "Template1",
                            ConsumedResources = new List<int>() { },
                            PrerequisiteResourcesInCity = new List<int>() { 0, 1 }
                        }
                    },
                    TemplateToConsider = 0,
                    CityToConsider = new CityTestData() {
                        Location = new HexCellTestData(),
                        LocationNeighbors = new List<HexCellTestData>() {
                            new HexCellTestData() { ResourceIndexInNode = -1 },
                            new HexCellTestData() { ResourceIndexInNode = 0 },
                            new HexCellTestData() { ResourceIndexInNode = 0 },
                        }
                    }
                }).SetName("Template requires resources in city, one resource is present").Returns(true);

                yield return new TestCaseData(new IsTemplateValidForCityTestData() {
                    AvailableResources = new List<string>() {
                        "Resource1", "Resource2"
                    },
                    AvailableBuildings = new List<BuildingTemplateTestData>() {
                        new BuildingTemplateTestData() {
                            Name = "Template1",
                            ConsumedResources = new List<int>() { },
                            PrerequisiteResourcesInCity = new List<int>() { 0, 1 }
                        }
                    },
                    TemplateToConsider = 0,
                    CityToConsider = new CityTestData() {
                        Location = new HexCellTestData(),
                        LocationNeighbors = new List<HexCellTestData>() {
                            new HexCellTestData() { ResourceIndexInNode = -1 },
                            new HexCellTestData() { ResourceIndexInNode = -1 },
                            new HexCellTestData() { ResourceIndexInNode = -1 },
                        }
                    }
                }).SetName("Template requires resources in city, neither resource is present").Returns(false);


                yield return new TestCaseData(new IsTemplateValidForCityTestData() {
                    AvailableResources = new List<string>() { },
                    AvailableImprovements = new List<ImprovementTemplateTestData>() {
                        new ImprovementTemplateTestData() { Name = "Improvement1" },
                        new ImprovementTemplateTestData() { Name = "Improvement2" },
                    },
                    AvailableBuildings = new List<BuildingTemplateTestData>() {
                        new BuildingTemplateTestData() {
                            Name = "Template1",
                            ConsumedResources = new List<int>() { },
                            PrerequisiteImprovementsNearCity = new List<int>() { 0, 1 }
                        }
                    },
                    TemplateToConsider = 0,
                    CityToConsider = new CityTestData() {
                        Location = new HexCellTestData(),
                        LocationNeighbors = new List<HexCellTestData>() {
                            new HexCellTestData() { ImprovementIndex = -1 },
                            new HexCellTestData() { ImprovementIndex = 0 },
                            new HexCellTestData() { ImprovementIndex = 1 },
                        }
                    }
                }).SetName("Template requires improvements near city, both improvements are present").Returns(true);

                yield return new TestCaseData(new IsTemplateValidForCityTestData() {
                    AvailableResources = new List<string>() { },
                    AvailableImprovements = new List<ImprovementTemplateTestData>() {
                        new ImprovementTemplateTestData() { Name = "Improvement1" },
                        new ImprovementTemplateTestData() { Name = "Improvement2" },
                    },
                    AvailableBuildings = new List<BuildingTemplateTestData>() {
                        new BuildingTemplateTestData() {
                            Name = "Template1",
                            ConsumedResources = new List<int>() { },
                            PrerequisiteImprovementsNearCity = new List<int>() { 0, 1 }
                        }
                    },
                    TemplateToConsider = 0,
                    CityToConsider = new CityTestData() {
                        Location = new HexCellTestData(),
                        LocationNeighbors = new List<HexCellTestData>() {
                            new HexCellTestData() { ImprovementIndex = -1 },
                            new HexCellTestData() { ImprovementIndex = 0 },
                            new HexCellTestData() { ImprovementIndex = -1 },
                        }
                    }
                }).SetName("Template requires improvements near city, one improvement is present").Returns(true);

                yield return new TestCaseData(new IsTemplateValidForCityTestData() {
                    AvailableResources = new List<string>() { },
                    AvailableImprovements = new List<ImprovementTemplateTestData>() {
                        new ImprovementTemplateTestData() { Name = "Improvement1" },
                        new ImprovementTemplateTestData() { Name = "Improvement2" },
                    },
                    AvailableBuildings = new List<BuildingTemplateTestData>() {
                        new BuildingTemplateTestData() {
                            Name = "Template1",
                            ConsumedResources = new List<int>() { },
                            PrerequisiteImprovementsNearCity = new List<int>() { 0, 1 }
                        }
                    },
                    TemplateToConsider = 0,
                    CityToConsider = new CityTestData() {
                        Location = new HexCellTestData(),
                        LocationNeighbors = new List<HexCellTestData>() {
                            new HexCellTestData() { ImprovementIndex = -1 },
                            new HexCellTestData() { ImprovementIndex = -1 },
                            new HexCellTestData() { ImprovementIndex = -1 },
                        }
                    }
                }).SetName("Template requires improvements near city, neither improvements are present").Returns(false);
            }
        }

        #endregion

        #region instance fields and properties

        private List<IBuildingTemplate> AvailableTemplates = new List<IBuildingTemplate>();

        private Mock<IPossessionRelationship<ICity, IBuilding>>        MockBuildingPossessionCanon;
        private Mock<IPossessionRelationship<ICivilization, ICity>>    MockCityPossessionCanon;
        private Mock<IFreeResourcesLogic>                              MockFreeResourcesLogic;
        private Mock<IPossessionRelationship<IHexCell, ICity>>         MockCityLocationCanon;
        private Mock<IHexGrid>                                         MockGrid;
        private Mock<IRiverCanon>                                      MockRiverCanon;
        private Mock<IPossessionRelationship<ICity, IHexCell>>         MockCellPossessionCanon;
        private Mock<IPossessionRelationship<IHexCell, IResourceNode>> MockNodeLocationCanon;
        private Mock<IImprovementLocationCanon>                        MockImprovementLocationCanon;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            AvailableTemplates.Clear();

            MockBuildingPossessionCanon  = new Mock<IPossessionRelationship<ICity, IBuilding>>();
            MockCityPossessionCanon      = new Mock<IPossessionRelationship<ICivilization, ICity>>();
            MockFreeResourcesLogic       = new Mock<IFreeResourcesLogic>();
            MockCityLocationCanon        = new Mock<IPossessionRelationship<IHexCell, ICity>>();
            MockGrid                     = new Mock<IHexGrid>();
            MockRiverCanon               = new Mock<IRiverCanon>();
            MockCellPossessionCanon      = new Mock<IPossessionRelationship<ICity, IHexCell>>();
            MockNodeLocationCanon        = new Mock<IPossessionRelationship<IHexCell, IResourceNode>>();
            MockImprovementLocationCanon = new Mock<IImprovementLocationCanon>();

            Container.Bind<List<IBuildingTemplate>>().FromInstance(AvailableTemplates);

            Container.Bind<IPossessionRelationship<ICity, IBuilding>>       ().FromInstance(MockBuildingPossessionCanon .Object);
            Container.Bind<IPossessionRelationship<ICivilization, ICity>>   ().FromInstance(MockCityPossessionCanon     .Object);
            Container.Bind<IFreeResourcesLogic>                             ().FromInstance(MockFreeResourcesLogic      .Object);
            Container.Bind<IPossessionRelationship<IHexCell, ICity>>        ().FromInstance(MockCityLocationCanon       .Object);
            Container.Bind<IHexGrid>                                        ().FromInstance(MockGrid                    .Object);
            Container.Bind<IRiverCanon>                                     ().FromInstance(MockRiverCanon              .Object);
            Container.Bind<IPossessionRelationship<ICity, IHexCell>>        ().FromInstance(MockCellPossessionCanon     .Object);
            Container.Bind<IPossessionRelationship<IHexCell, IResourceNode>>().FromInstance(MockNodeLocationCanon       .Object);
            Container.Bind<IImprovementLocationCanon>                       ().FromInstance(MockImprovementLocationCanon.Object);

            Container.Bind<BuildingProductionValidityLogic>().AsSingle();
        }

        #endregion

        #region tests

        [Test(Description = "")]
        [TestCaseSource("IsTemplateValidForCityTestCases")]
        public bool IsTemplateValidForCityTests(IsTemplateValidForCityTestData data) {
            var allResources = data.AvailableResources.Select(name => BuildResourceDefinition(name)).ToList();

            var allImprovementTemplates = data.AvailableImprovements.Select(
                improvementData => BuildImprovementTemplate(improvementData)
            ).ToList();

            var allbuildingTemplates = new List<IBuildingTemplate>();
            foreach(var templateData in data.AvailableBuildings) {
                allbuildingTemplates.Add(BuildTemplate(
                    templateData, allbuildingTemplates, allResources, allImprovementTemplates
                ));
            }

            var templatesInCity = data.CityToConsider.TemplatesAlreadyConstructed.Select(
                templateIndex => allbuildingTemplates[templateIndex]
            );

            var civilization = BuildCivilization(
                data.CityToConsider.ResourcesAvailableToOwner.Select(resourceIndex => allResources[resourceIndex])
            );

            var cityToConsider = BuildCity(
                data.CityToConsider, civilization,
                templatesInCity.Select(template => BuildBuilding(template)),
                allResources, allImprovementTemplates
            );

            var validityLogic = Container.Resolve<BuildingProductionValidityLogic>();

            return validityLogic.IsTemplateValidForCity(allbuildingTemplates[data.TemplateToConsider], cityToConsider);
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
            List<IBuildingTemplate> allBuildingTemplates,
            List<ISpecialtyResourceDefinition> allResources,
            List<IImprovementTemplate> allImprovementTemplates
        ){
            var mockTemplate = new Mock<IBuildingTemplate>();
            mockTemplate.Name = templateData.Name;

            mockTemplate.Setup(template => template.name                 ).Returns(templateData.Name);
            mockTemplate.Setup(template => template.RequiresAdjacentRiver).Returns(templateData.RequiresAdjacentRiver);
            mockTemplate.Setup(template => template.RequiresCoastalCity  ).Returns(templateData.RequiresCoastalCity);
            
            mockTemplate
                .Setup(template => template.ResourcesConsumed)
                .Returns(templateData.ConsumedResources.Select(index => allResources[index]));

            mockTemplate
                .Setup(template => template.PrerequisiteResourcesNearCity)
                .Returns(templateData.PrerequisiteResourcesInCity.Select(index => allResources[index]));

            mockTemplate
                .Setup(template => template.PrerequisiteBuildings)
                .Returns(templateData.PrerequisiteBuildings.Select(index => allBuildingTemplates[index]));

            mockTemplate
                .Setup(template => template.PrerequisiteImprovementsNearCity)
                .Returns(templateData.PrerequisiteImprovementsNearCity.Select(index => allImprovementTemplates[index]));

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
            CityTestData cityData, ICivilization owner, IEnumerable<IBuilding> buildings,
            List<ISpecialtyResourceDefinition> allResources,
            List<IImprovementTemplate> allImprovementTemplates
        ){
            var mockCity = new Mock<ICity>();

            var newCity = mockCity.Object;

            var cityLocation      = BuildCell(cityData.Location, allResources, allImprovementTemplates);
            var locationNeighbors = cityData.LocationNeighbors.Select(data => BuildCell(data, allResources, allImprovementTemplates));

            MockCityLocationCanon.Setup(canon => canon.GetOwnerOfPossession(newCity)).Returns(cityLocation);
            MockGrid.Setup(grid => grid.GetNeighbors(cityLocation)).Returns(locationNeighbors.ToList());

            MockCellPossessionCanon.Setup(canon => canon.GetPossessionsOfOwner(newCity)).Returns(locationNeighbors);

            MockCityPossessionCanon.Setup(canon => canon.GetOwnerOfPossession(newCity)).Returns(owner);

            MockBuildingPossessionCanon.Setup(canon => canon.GetPossessionsOfOwner(newCity)).Returns(buildings);

            return newCity;
        }

        private ICivilization BuildCivilization(IEnumerable<ISpecialtyResourceDefinition> availableResources) {
            var newCiv = new Mock<ICivilization>().Object;

            foreach(var resource in availableResources) {
                MockFreeResourcesLogic.Setup(canon => canon.GetFreeCopiesOfResourceForCiv(resource, newCiv)).Returns(1);
            }
            
            return newCiv;
        }

        private IHexCell BuildCell(
            HexCellTestData cellData, List<ISpecialtyResourceDefinition> allResources,
            List<IImprovementTemplate> allImprovementTemplates
        ){
            var mockCell = new Mock<IHexCell>();

            mockCell.Setup(cell => cell.Terrain).Returns(cellData.IsUnderwater ? CellTerrain.FreshWater : CellTerrain.Grassland);

            var newCell = mockCell.Object;

            MockRiverCanon.Setup(canon => canon.HasRiver(newCell)).Returns(cellData.HasRiver);

            if(cellData.ResourceIndexInNode != -1) {
                BuildResourceNode(newCell, allResources[cellData.ResourceIndexInNode]);
            }

            if(cellData.ImprovementIndex != -1) {
                BuildImprovement(newCell, allImprovementTemplates[cellData.ImprovementIndex]);
            }

            return newCell;
        }

        private IResourceNode BuildResourceNode(IHexCell location, ISpecialtyResourceDefinition resource) {
            var mockNode = new Mock<IResourceNode>();

            mockNode.Setup(node => node.Resource).Returns(resource);

            var newNode = mockNode.Object;

            MockNodeLocationCanon
                .Setup(canon => canon.GetPossessionsOfOwner(location))
                .Returns(new List<IResourceNode>() { newNode });

            return newNode;
        }

        private IImprovement BuildImprovement(IHexCell location, IImprovementTemplate template) {
            var mockImprovement = new Mock<IImprovement>();

            mockImprovement.Setup(improvement => improvement.Template).Returns(template);

            var newImprovement = mockImprovement.Object;

            MockImprovementLocationCanon
                .Setup(canon => canon.GetPossessionsOfOwner(location))
                .Returns(new List<IImprovement>() { newImprovement });

            return newImprovement;
        }

        private IImprovementTemplate BuildImprovementTemplate(ImprovementTemplateTestData templateData) {
            var mockTemplate = new Mock<IImprovementTemplate>();

            mockTemplate.Setup(template => template.name).Returns(templateData.Name);

            return mockTemplate.Object;
        }

        #endregion

        #endregion

    }

}
