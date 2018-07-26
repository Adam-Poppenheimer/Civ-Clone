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
using Assets.Simulation.MapResources;
using Assets.Simulation.Improvements;
using Assets.Simulation.Cities;
using Assets.Simulation.HexMap;
using Assets.Simulation.Technology;

namespace Assets.Tests.Simulation.Civilizations {

    [TestFixture]
    public class ResourceExtractionLogicTests : ZenjectUnitTestFixture {

        #region internal types

        public class ExtractedCopiesTestData {

            public List<ImprovementTemplateTestData> AvailableTemplates = new List<ImprovementTemplateTestData>();

            public List<ResourceTestData> AvailableResources = new List<ResourceTestData>();

            public CivilizationTestData CivToTest = new CivilizationTestData();

            public int ResourceToTestIndex;

        }

        public class CivilizationTestData {

            public List<CityTestData> Cities = new List<CityTestData>();

        }

        public class CityTestData {

            public List<HexCellTestData> Territory = new List<HexCellTestData>();

        }

        public class HexCellTestData {

            public ResourceNodeTestData Node = new ResourceNodeTestData();

            public ImprovementTestData Improvement = new ImprovementTestData();

        }

        public class ResourceNodeTestData {

            public int Copies;

            public int ResourceIndex = -1;

        }

        public class ResourceTestData {

            public Assets.Simulation.MapResources.ResourceType Type;

            public bool IsVisibleToCiv;

            public int ExtractorIndex = -1;

        }

        public class ImprovementTestData {

            public int TemplateIndex;

            public bool IsConstructed;
            public bool IsPillaged;

        }

        public class ImprovementTemplateTestData {

        }

        #endregion

        #region static fields and properties

        public static IEnumerable GetExtractedCopiesOfResourceForCivTestCases {
            get {
                yield return new TestCaseData(new ExtractedCopiesTestData() {
                    AvailableResources = new List<ResourceTestData>() {
                        new ResourceTestData()
                    }
                }).SetName("Defaults to zero").Returns(0);

                yield return new TestCaseData(new ExtractedCopiesTestData() {
                    AvailableResources = new List<ResourceTestData>() {
                        new ResourceTestData() {
                            ExtractorIndex = 0, IsVisibleToCiv = true, Type = Assets.Simulation.MapResources.ResourceType.Strategic
                        }
                    },
                    AvailableTemplates = new List<ImprovementTemplateTestData>() {
                        new ImprovementTemplateTestData()
                    },
                    CivToTest = new CivilizationTestData() {
                        Cities = new List<CityTestData>() {
                            new CityTestData() {
                                Territory = new List<HexCellTestData>() {
                                    new HexCellTestData() {
                                        Improvement = new ImprovementTestData() {
                                            IsConstructed = true, TemplateIndex = 0
                                        },
                                        Node = new ResourceNodeTestData() {
                                            Copies = 1, ResourceIndex = 0
                                        }
                                    }
                                }
                            },
                            new CityTestData() {
                                Territory = new List<HexCellTestData>() {
                                    new HexCellTestData(),
                                    new HexCellTestData(),
                                    new HexCellTestData() {
                                        Improvement = new ImprovementTestData() {
                                            IsConstructed = true, TemplateIndex = 0
                                        },
                                        Node = new ResourceNodeTestData() {
                                            Copies = 3, ResourceIndex = 0
                                        }
                                    }
                                }
                            }
                        }
                    }
                }).SetName("Pulls copies of strategic resources from cells of possessed cities").Returns(4);

                yield return new TestCaseData(new ExtractedCopiesTestData() {
                    AvailableResources = new List<ResourceTestData>() {
                        new ResourceTestData() {
                            ExtractorIndex = 0, IsVisibleToCiv = true, Type = Assets.Simulation.MapResources.ResourceType.Luxury
                        }
                    },
                    AvailableTemplates = new List<ImprovementTemplateTestData>() {
                        new ImprovementTemplateTestData()
                    },
                    CivToTest = new CivilizationTestData() {
                        Cities = new List<CityTestData>() {
                            new CityTestData() {
                                Territory = new List<HexCellTestData>() {
                                    new HexCellTestData() {
                                        Improvement = new ImprovementTestData() {
                                            IsConstructed = true, TemplateIndex = 0
                                        },
                                        Node = new ResourceNodeTestData() {
                                            Copies = 1, ResourceIndex = 0
                                        }
                                    }
                                }
                            },
                            new CityTestData() {
                                Territory = new List<HexCellTestData>() {
                                    new HexCellTestData(),
                                    new HexCellTestData(),
                                    new HexCellTestData() {
                                        Improvement = new ImprovementTestData() {
                                            IsConstructed = true, TemplateIndex = 0
                                        },
                                        Node = new ResourceNodeTestData() {
                                            Copies = 3, ResourceIndex = 0
                                        }
                                    }
                                }
                            }
                        }
                    }
                }).SetName("Luxury nodes never add more than 1 copy").Returns(2);

                yield return new TestCaseData(new ExtractedCopiesTestData() {
                    AvailableResources = new List<ResourceTestData>() {
                        new ResourceTestData() {
                            ExtractorIndex = 0, IsVisibleToCiv = true, Type = Assets.Simulation.MapResources.ResourceType.Bonus
                        }
                    },
                    AvailableTemplates = new List<ImprovementTemplateTestData>() {
                        new ImprovementTemplateTestData()
                    },
                    CivToTest = new CivilizationTestData() {
                        Cities = new List<CityTestData>() {
                            new CityTestData() {
                                Territory = new List<HexCellTestData>() {
                                    new HexCellTestData() {
                                        Improvement = new ImprovementTestData() {
                                            IsConstructed = true, TemplateIndex = 0
                                        },
                                        Node = new ResourceNodeTestData() {
                                            Copies = 1, ResourceIndex = 0
                                        }
                                    }
                                }
                            },
                            new CityTestData() {
                                Territory = new List<HexCellTestData>() {
                                    new HexCellTestData(),
                                    new HexCellTestData(),
                                    new HexCellTestData() {
                                        Improvement = new ImprovementTestData() {
                                            IsConstructed = true, TemplateIndex = 0
                                        },
                                        Node = new ResourceNodeTestData() {
                                            Copies = 3, ResourceIndex = 0
                                        }
                                    }
                                }
                            }
                        }
                    }
                }).SetName("Bonus nodes never provide any copies").Returns(0);

                yield return new TestCaseData(new ExtractedCopiesTestData() {
                    AvailableResources = new List<ResourceTestData>() {
                        new ResourceTestData() {
                            ExtractorIndex = 0, IsVisibleToCiv = false, Type = Assets.Simulation.MapResources.ResourceType.Luxury
                        }
                    },
                    AvailableTemplates = new List<ImprovementTemplateTestData>() {
                        new ImprovementTemplateTestData()
                    },
                    CivToTest = new CivilizationTestData() {
                        Cities = new List<CityTestData>() {
                            new CityTestData() {
                                Territory = new List<HexCellTestData>() {
                                    new HexCellTestData() {
                                        Improvement = new ImprovementTestData() {
                                            IsConstructed = true, TemplateIndex = 0
                                        },
                                        Node = new ResourceNodeTestData() {
                                            Copies = 1, ResourceIndex = 0
                                        }
                                    }
                                }
                            }
                        }
                    }
                }).SetName("No copies returned if resource is invisible to civ").Returns(0);

                yield return new TestCaseData(new ExtractedCopiesTestData() {
                    AvailableResources = new List<ResourceTestData>() {
                        new ResourceTestData() {
                            ExtractorIndex = 0, IsVisibleToCiv = true, Type = Assets.Simulation.MapResources.ResourceType.Luxury
                        }
                    },
                    AvailableTemplates = new List<ImprovementTemplateTestData>() {
                        new ImprovementTemplateTestData()
                    },
                    CivToTest = new CivilizationTestData() {
                        Cities = new List<CityTestData>() {
                            new CityTestData() {
                                Territory = new List<HexCellTestData>() {
                                    new HexCellTestData() {
                                        Improvement = new ImprovementTestData() {
                                            IsConstructed = false, TemplateIndex = 0
                                        },
                                        Node = new ResourceNodeTestData() {
                                            Copies = 1, ResourceIndex = 0
                                        }
                                    }
                                }
                            }
                        }
                    }
                }).SetName("No copies returned from unconstructed improvements").Returns(0);

                yield return new TestCaseData(new ExtractedCopiesTestData() {
                    AvailableResources = new List<ResourceTestData>() {
                        new ResourceTestData() {
                            ExtractorIndex = 0, IsVisibleToCiv = true, Type = Assets.Simulation.MapResources.ResourceType.Luxury
                        }
                    },
                    AvailableTemplates = new List<ImprovementTemplateTestData>() {
                        new ImprovementTemplateTestData()
                    },
                    CivToTest = new CivilizationTestData() {
                        Cities = new List<CityTestData>() {
                            new CityTestData() {
                                Territory = new List<HexCellTestData>() {
                                    new HexCellTestData() {
                                        Improvement = new ImprovementTestData() {
                                            IsConstructed = true, IsPillaged = true, TemplateIndex = 0
                                        },
                                        Node = new ResourceNodeTestData() {
                                            Copies = 1, ResourceIndex = 0
                                        }
                                    }
                                }
                            }
                        }
                    }
                }).SetName("No copies returned from pillaged improvements").Returns(0);

                yield return new TestCaseData(new ExtractedCopiesTestData() {
                    AvailableResources = new List<ResourceTestData>() {
                        new ResourceTestData() {
                            ExtractorIndex = 0, IsVisibleToCiv = true, Type = Assets.Simulation.MapResources.ResourceType.Luxury
                        }
                    },
                    AvailableTemplates = new List<ImprovementTemplateTestData>() {
                        new ImprovementTemplateTestData(), new ImprovementTemplateTestData()
                    },
                    CivToTest = new CivilizationTestData() {
                        Cities = new List<CityTestData>() {
                            new CityTestData() {
                                Territory = new List<HexCellTestData>() {
                                    new HexCellTestData() {
                                        Improvement = new ImprovementTestData() {
                                            IsConstructed = true, TemplateIndex = 1
                                        },
                                        Node = new ResourceNodeTestData() {
                                            Copies = 1, ResourceIndex = 0
                                        }
                                    }
                                }
                            }
                        }
                    }
                }).SetName("No copies returned if improvement isn't an extractor").Returns(0);

                yield return new TestCaseData(new ExtractedCopiesTestData() {
                    AvailableResources = new List<ResourceTestData>() {
                        new ResourceTestData() {
                            ExtractorIndex = 0, IsVisibleToCiv = true, Type = Assets.Simulation.MapResources.ResourceType.Luxury
                        }
                    },
                    AvailableTemplates = new List<ImprovementTemplateTestData>() {
                        new ImprovementTemplateTestData(), new ImprovementTemplateTestData()
                    },
                    CivToTest = new CivilizationTestData() {
                        Cities = new List<CityTestData>() {
                            new CityTestData() {
                                Territory = new List<HexCellTestData>() {
                                    new HexCellTestData() {
                                        Node = new ResourceNodeTestData() {
                                            Copies = 1, ResourceIndex = 0
                                        }
                                    }
                                }
                            }
                        }
                    }
                }).SetName("No copies returned if no improvement, but the resource needs and extractor").Returns(0);

                yield return new TestCaseData(new ExtractedCopiesTestData() {
                    AvailableResources = new List<ResourceTestData>() {
                        new ResourceTestData() {
                            ExtractorIndex = 0, IsVisibleToCiv = true, Type = Assets.Simulation.MapResources.ResourceType.Luxury
                        }
                    },
                    AvailableTemplates = new List<ImprovementTemplateTestData>() {
                        new ImprovementTemplateTestData(), new ImprovementTemplateTestData()
                    },
                    CivToTest = new CivilizationTestData() {
                        Cities = new List<CityTestData>() {
                            new CityTestData() {
                                Territory = new List<HexCellTestData>() {
                                    new HexCellTestData() {
                                        Node = new ResourceNodeTestData() {
                                            Copies = 1, ResourceIndex = -1
                                        }
                                    }
                                }
                            }
                        }
                    }
                }).SetName("Copies returned if no improvement, but the resource doesn't need an extractor").Returns(0);
            }
        }

        #endregion

        #region instance fields and properties

        private Mock<IPossessionRelationship<ICivilization, ICity>>    MockCityPossessionCanon;
        private Mock<IPossessionRelationship<ICity, IHexCell>>         MockCellPossessionCanon;
        private Mock<IPossessionRelationship<IHexCell, IResourceNode>> MockNodePositionCanon;
        private Mock<IImprovementLocationCanon>                        MockImprovementLocationCanon;
        private Mock<ITechCanon>                                       MockTechCanon;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockCityPossessionCanon      = new Mock<IPossessionRelationship<ICivilization, ICity>>();
            MockCellPossessionCanon      = new Mock<IPossessionRelationship<ICity, IHexCell>>();
            MockNodePositionCanon        = new Mock<IPossessionRelationship<IHexCell, IResourceNode>>();
            MockImprovementLocationCanon = new Mock<IImprovementLocationCanon>();
            MockTechCanon                = new Mock<ITechCanon>();

            Container.Bind<IPossessionRelationship<ICivilization, ICity>>   ().FromInstance(MockCityPossessionCanon     .Object);
            Container.Bind<IPossessionRelationship<ICity, IHexCell>>        ().FromInstance(MockCellPossessionCanon     .Object);
            Container.Bind<IPossessionRelationship<IHexCell, IResourceNode>>().FromInstance(MockNodePositionCanon       .Object);
            Container.Bind<IImprovementLocationCanon>                       ().FromInstance(MockImprovementLocationCanon.Object);
            Container.Bind<ITechCanon>                                      ().FromInstance(MockTechCanon               .Object);

            Container.Bind<ResourceExtractionLogic>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        [TestCaseSource("GetExtractedCopiesOfResourceForCivTestCases")]
        public int GetExtractedCopiesOfResourceForCivTests(ExtractedCopiesTestData testData) {
            var availableTemplates = testData.AvailableTemplates.Select(
                templateData => BuildImprovementTemplate(templateData)
            ).ToList();

            var availableResources = testData.AvailableResources.Select(
                resourceData => BuildResourceDefinition(resourceData, availableTemplates)
            ).ToList();

            var resourceToTest = availableResources[testData.ResourceToTestIndex];
            var civToTest      = BuildCiv(testData.CivToTest, availableResources, availableTemplates);

            var extractionLogic = Container.Resolve<ResourceExtractionLogic>();

            return extractionLogic.GetExtractedCopiesOfResourceForCiv(resourceToTest, civToTest);
        }

        #endregion

        #region utilities

        private ICivilization BuildCiv(
            CivilizationTestData data, List<IResourceDefinition> resources,
            List<IImprovementTemplate> availableTemplates
        ){
            var newCiv = new Mock<ICivilization>().Object;

            MockCityPossessionCanon.Setup(canon => canon.GetPossessionsOfOwner(newCiv))
                .Returns(data.Cities.Select(cityData => BuildCity(cityData, resources, availableTemplates)));

            return newCiv;
        }

        private ICity BuildCity(
            CityTestData data, List<IResourceDefinition> resources,
            List<IImprovementTemplate> availableTemplates
        ){
            var newCity = new Mock<ICity>().Object;

            MockCellPossessionCanon.Setup(canon => canon.GetPossessionsOfOwner(newCity))
                .Returns(data.Territory.Select(cellData => BuildCell(cellData, resources, availableTemplates)));

            return newCity;
        }

        private IHexCell BuildCell(
            HexCellTestData data, List<IResourceDefinition> resources,
            List<IImprovementTemplate> availableTemplates
        ){
            var mockCell = new Mock<IHexCell>();

            var newCell = mockCell.Object;

            if(data.Improvement != null) {
                var improvement = BuildImprovement(data.Improvement, availableTemplates);

                MockImprovementLocationCanon
                    .Setup(canon => canon.GetPossessionsOfOwner(newCell))
                    .Returns(improvement != null ? new List<IImprovement>() { improvement } : new List<IImprovement>());
            }

            if(data.Node != null) {
                var node = BuildResourceNode(data.Node, data.Node.ResourceIndex >= 0 ? resources[data.Node.ResourceIndex] : null);

                MockNodePositionCanon
                    .Setup(canon => canon.GetPossessionsOfOwner(newCell))
                    .Returns(node != null ? new List<IResourceNode>() { node } : new List<IResourceNode>());
            }

            return newCell;
        }

        private IImprovement BuildImprovement(
            ImprovementTestData data, List<IImprovementTemplate> availableTemplates
        ){
            var mockImprovement = new Mock<IImprovement>();

            mockImprovement.Setup(improvement => improvement.IsConstructed).Returns(data.IsConstructed);
            mockImprovement.Setup(improvement => improvement.IsPillaged)   .Returns(data.IsPillaged);
            mockImprovement.Setup(improvement => improvement.Template)     .Returns(availableTemplates[data.TemplateIndex]);

            return mockImprovement.Object;
        }

        private IResourceNode BuildResourceNode(ResourceNodeTestData data, IResourceDefinition resource) {
            var mockNode = new Mock<IResourceNode>();

            mockNode.Setup(node => node.Copies)  .Returns(data.Copies);
            mockNode.Setup(node => node.Resource).Returns(resource);

            return mockNode.Object;
        }

        private IResourceDefinition BuildResourceDefinition(
            ResourceTestData data, List<IImprovementTemplate> availableTemplates
        ){
            var mockResource = new Mock<IResourceDefinition>();

            mockResource.Setup(resource => resource.Type).Returns(data.Type);

            mockResource.Setup(resource => resource.Extractor).Returns(
                data.ExtractorIndex >= 0 ? availableTemplates[data.ExtractorIndex] : null
            );

            var newResource = mockResource.Object;

            MockTechCanon.Setup(canon => canon.IsResourceVisibleToCiv(newResource, It.IsAny<ICivilization>()))
                         .Returns(data.IsVisibleToCiv);

            return newResource;
        }

        private IImprovementTemplate BuildImprovementTemplate(ImprovementTemplateTestData testData) {
            return new Mock<IImprovementTemplate>().Object;
        }

        #endregion

        #endregion

    }

}
