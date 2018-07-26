using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation;
using Assets.Simulation.Improvements;
using Assets.Simulation.Technology;
using Assets.Simulation.Civilizations;
using Assets.Simulation.HexMap;
using Assets.Simulation.Cities;
using Assets.Simulation.MapResources;

using UnityCustomUtilities.Extensions;

namespace Assets.Tests.Simulation.Improvements {

    public class ImprovementYieldLogicTests : ZenjectUnitTestFixture {

        #region internal types

        public class ImprovementYieldTestData {

            public ImprovementTestData Improvement;

            public ResourceNodeTestData ResourceNodeAtCell;

            public bool CellHasAccessToFreshWater;

            public CityTestData CityOwningCell;

            public CivilizationTestData CivOwningCity;

        }

        public class ImprovementTestData {

            public ImprovementTemplateTestData Template;

            public bool IsConstructed = true;

            public bool IsPillaged = false;

        }

        public class ImprovementTemplateTestData {

            public YieldSummary BonusYieldNormal;

        }

        public class ResourceNodeTestData {

            public YieldSummary DefinitionImprovementYield;

            public bool IsExtractedByImprovement;

            public bool IsVisible;

        }

        public class CityTestData {



        }

        public class CivilizationTestData {

            public List<TechTestData> TechsDiscovered;

        }

        public class TechTestData {

            public List<ImprovementModificationTestData> ImprovementModifications;

        }

        public class ImprovementModificationTestData {

            public YieldSummary BonusYield;

            public bool AppliesToImprovement;

            public bool RequiresFreshWater;

        }

        #endregion

        #region static fields and properties

        public static IEnumerable GetExpectedYieldTestCases {
            get {
                yield return new TestCaseData(new ImprovementYieldTestData() {
                    Improvement = new ImprovementTestData() {
                        Template = new ImprovementTemplateTestData() {
                            BonusYieldNormal = new YieldSummary(food: 1)
                        },
                    },                    
                    ResourceNodeAtCell = new ResourceNodeTestData() {
                        DefinitionImprovementYield = new YieldSummary(production: 2),
                        IsExtractedByImprovement = false
                    },
                    CityOwningCell = new CityTestData(),
                    CivOwningCity = new CivilizationTestData() {
                        TechsDiscovered = new List<TechTestData>()
                    }
                }).SetName("Non-extracting improvement, no techs").Returns(
                    new YieldSummary(food: 1)
                );

                yield return new TestCaseData(new ImprovementYieldTestData() {
                    Improvement = new ImprovementTestData() {
                        Template = new ImprovementTemplateTestData() {
                            BonusYieldNormal = new YieldSummary(food: 1)
                        },
                    },
                    ResourceNodeAtCell = new ResourceNodeTestData() {
                        DefinitionImprovementYield = new YieldSummary(production: 2),
                        IsExtractedByImprovement = true,
                        IsVisible = false
                    },
                    CityOwningCell = new CityTestData(),
                    CivOwningCity = new CivilizationTestData() {
                        TechsDiscovered = new List<TechTestData>()
                    }
                }).SetName("Extracting improvement, invisible resource, no techs").Returns(
                    new YieldSummary(food: 1)
                );

                yield return new TestCaseData(new ImprovementYieldTestData() {
                    Improvement = new ImprovementTestData() {
                        Template = new ImprovementTemplateTestData() {
                            BonusYieldNormal = new YieldSummary(food: 1)
                        },
                    },
                    ResourceNodeAtCell = new ResourceNodeTestData() {
                        DefinitionImprovementYield = new YieldSummary(production: 2),
                        IsExtractedByImprovement = true,
                        IsVisible = true
                    },
                    CityOwningCell = new CityTestData(),
                    CivOwningCity = new CivilizationTestData() {
                        TechsDiscovered = new List<TechTestData>()
                    }
                }).SetName("Extracting improvement, visible resource, no techs").Returns(
                    new YieldSummary(production: 2)
                );

                yield return new TestCaseData(new ImprovementYieldTestData() {
                    Improvement = new ImprovementTestData() {
                        Template = new ImprovementTemplateTestData() {
                            BonusYieldNormal = new YieldSummary(food: 1)
                        },
                    },
                    ResourceNodeAtCell = new ResourceNodeTestData() {
                        DefinitionImprovementYield = new YieldSummary(production: 2),
                        IsExtractedByImprovement = false
                    },
                    CityOwningCell = new CityTestData(),
                    CivOwningCity = new CivilizationTestData() {
                        TechsDiscovered = new List<TechTestData>() {
                            new TechTestData() {
                                ImprovementModifications = new List<ImprovementModificationTestData>() {
                                    new ImprovementModificationTestData() {
                                        BonusYield = new YieldSummary(gold: 1), AppliesToImprovement = true
                                    },
                                    new ImprovementModificationTestData() {
                                        BonusYield = new YieldSummary(culture: 2), AppliesToImprovement = true
                                    }
                                }
                            },
                            new TechTestData() {
                                ImprovementModifications = new List<ImprovementModificationTestData>() {
                                    new ImprovementModificationTestData() {
                                        BonusYield = new YieldSummary(science: 3), AppliesToImprovement = true
                                    },
                                }
                            }
                        }
                    }
                }).SetName("Non-extracting improvement, techs that modify yield").Returns(
                    new YieldSummary(food: 1, gold: 1, culture: 2, science: 3)
                );

                yield return new TestCaseData(new ImprovementYieldTestData() {
                    Improvement = new ImprovementTestData() {
                        Template = new ImprovementTemplateTestData() {
                            BonusYieldNormal = new YieldSummary(food: 1)
                        },
                    },
                    ResourceNodeAtCell = new ResourceNodeTestData() {
                        DefinitionImprovementYield = new YieldSummary(production: 2),
                        IsExtractedByImprovement = false
                    },
                    CityOwningCell = new CityTestData(),
                    CivOwningCity = new CivilizationTestData() {
                        TechsDiscovered = new List<TechTestData>() {
                            new TechTestData() {
                                ImprovementModifications = new List<ImprovementModificationTestData>() {
                                    new ImprovementModificationTestData() {
                                        BonusYield = new YieldSummary(gold: 1), AppliesToImprovement = false
                                    },
                                    new ImprovementModificationTestData() {
                                        BonusYield = new YieldSummary(culture: 2f), AppliesToImprovement = false
                                    }
                                }
                            },
                            new TechTestData() {
                                ImprovementModifications = new List<ImprovementModificationTestData>() {
                                    new ImprovementModificationTestData() {
                                        BonusYield = new YieldSummary(science: 3), AppliesToImprovement = false
                                    }
                                }
                            }
                        }
                    }
                }).SetName("Non-extracting improvement, techs that don't modify yield").Returns(
                    new YieldSummary(food: 1)
                );

                yield return new TestCaseData(new ImprovementYieldTestData() {
                    Improvement = new ImprovementTestData() {
                        Template = new ImprovementTemplateTestData() {
                            BonusYieldNormal = new YieldSummary(food: 1)
                        },
                    },
                    ResourceNodeAtCell = new ResourceNodeTestData() {
                        DefinitionImprovementYield = new YieldSummary(production: 2),
                        IsExtractedByImprovement = false
                    },
                    CityOwningCell = new CityTestData(),
                    CivOwningCity = new CivilizationTestData() {
                        TechsDiscovered = new List<TechTestData>() {
                            new TechTestData() {
                                ImprovementModifications = new List<ImprovementModificationTestData>() {
                                    new ImprovementModificationTestData() {
                                        BonusYield = new YieldSummary(gold: 1), AppliesToImprovement = false
                                    },
                                    new ImprovementModificationTestData() {
                                        BonusYield = new YieldSummary(culture: 2f), AppliesToImprovement = true
                                    }
                                }
                            },
                            new TechTestData() {
                                ImprovementModifications = new List<ImprovementModificationTestData>() {
                                    new ImprovementModificationTestData() {
                                        BonusYield = new YieldSummary(science: 3f), AppliesToImprovement = false
                                    }
                                }
                            }
                        }
                    }
                }).SetName("Non-extracting improvement, only some tech modifications affect template").Returns(
                    new YieldSummary(food: 1, culture: 2)
                );

                yield return new TestCaseData(new ImprovementYieldTestData() {
                    Improvement = new ImprovementTestData() {
                        Template = new ImprovementTemplateTestData() {
                            BonusYieldNormal = new YieldSummary(food: 1)
                        },
                    },
                    ResourceNodeAtCell = new ResourceNodeTestData() {
                        DefinitionImprovementYield = new YieldSummary(production: 2),
                        IsExtractedByImprovement = false
                    },
                    CellHasAccessToFreshWater = true,
                    CityOwningCell = new CityTestData(),
                    CivOwningCity = new CivilizationTestData() {
                        TechsDiscovered = new List<TechTestData>() {
                            new TechTestData() {
                                ImprovementModifications = new List<ImprovementModificationTestData>() {
                                    new ImprovementModificationTestData() {
                                        BonusYield = new YieldSummary(gold: 1), AppliesToImprovement = true,
                                        RequiresFreshWater = true
                                    },
                                    new ImprovementModificationTestData() {
                                        BonusYield = new YieldSummary(culture: 2), AppliesToImprovement = true,
                                        RequiresFreshWater = true
                                    }
                                }
                            },
                            new TechTestData() {
                                ImprovementModifications = new List<ImprovementModificationTestData>() {
                                    new ImprovementModificationTestData() {
                                        BonusYield = new YieldSummary(science: 3), AppliesToImprovement = true,
                                        RequiresFreshWater = true
                                    },
                                }
                            }
                        }
                    }
                }).SetName("Non-extracting improvement, fresh water, tech modifications require fresh water").Returns(
                    new YieldSummary(food: 1, gold: 1, culture: 2, science: 3)
                );

                yield return new TestCaseData(new ImprovementYieldTestData() {
                    Improvement = new ImprovementTestData() {
                        Template = new ImprovementTemplateTestData() {
                            BonusYieldNormal = new YieldSummary(food: 1)
                        },
                    },
                    ResourceNodeAtCell = new ResourceNodeTestData() {
                        DefinitionImprovementYield = new YieldSummary(production: 2),
                        IsExtractedByImprovement = false
                    },
                    CellHasAccessToFreshWater = false,
                    CityOwningCell = new CityTestData(),
                    CivOwningCity = new CivilizationTestData() {
                        TechsDiscovered = new List<TechTestData>() {
                            new TechTestData() {
                                ImprovementModifications = new List<ImprovementModificationTestData>() {
                                    new ImprovementModificationTestData() {
                                        BonusYield = new YieldSummary(gold: 1), AppliesToImprovement = true,
                                        RequiresFreshWater = true
                                    },
                                    new ImprovementModificationTestData() {
                                        BonusYield = new YieldSummary(culture: 2), AppliesToImprovement = true,
                                        RequiresFreshWater = true
                                    }
                                }
                            },
                            new TechTestData() {
                                ImprovementModifications = new List<ImprovementModificationTestData>() {
                                    new ImprovementModificationTestData() {
                                        BonusYield = new YieldSummary(science: 3), AppliesToImprovement = true,
                                        RequiresFreshWater = true
                                    },
                                }
                            }
                        }
                    }
                }).SetName("Non-extracting improvement, no fresh water, tech modifications require fresh water").Returns(
                    new YieldSummary(food: 1, gold: 0, culture: 0, science: 0)
                );
            }
        }

        public static IEnumerable GetYieldOfImprovementTestCases {
            get {
                yield return new TestCaseData(new ImprovementYieldTestData() {
                    Improvement = new ImprovementTestData() {
                        Template = new ImprovementTemplateTestData() {
                            BonusYieldNormal = new YieldSummary(food: 1)
                        },
                        IsConstructed = true
                    },
                    ResourceNodeAtCell = new ResourceNodeTestData() {
                        DefinitionImprovementYield = new YieldSummary(production: 2),
                        IsExtractedByImprovement = false
                    },
                    CityOwningCell = new CityTestData(),
                    CivOwningCity = new CivilizationTestData() {
                        TechsDiscovered = new List<TechTestData>()
                    }
                }).SetName("Improvement is constructed").Returns(
                    new YieldSummary(food: 1)
                );

                yield return new TestCaseData(new ImprovementYieldTestData() {
                    Improvement = new ImprovementTestData() {
                        Template = new ImprovementTemplateTestData() {
                            BonusYieldNormal = new YieldSummary(food: 1)
                        },
                        IsConstructed = false
                    },
                    ResourceNodeAtCell = new ResourceNodeTestData() {
                        DefinitionImprovementYield = new YieldSummary(production: 2),
                        IsExtractedByImprovement = false
                    },
                    CityOwningCell = new CityTestData(),
                    CivOwningCity = new CivilizationTestData() {
                        TechsDiscovered = new List<TechTestData>()
                    }
                }).SetName("Improvement is not constructed").Returns(
                    YieldSummary.Empty
                );

                yield return new TestCaseData(new ImprovementYieldTestData() {
                    Improvement = new ImprovementTestData() {
                        Template = new ImprovementTemplateTestData() {
                            BonusYieldNormal = new YieldSummary(food: 1)
                        },
                        IsConstructed = true, IsPillaged = true
                    },
                    ResourceNodeAtCell = new ResourceNodeTestData() {
                        DefinitionImprovementYield = new YieldSummary(production: 2),
                        IsExtractedByImprovement = false
                    },
                    CityOwningCell = new CityTestData(),
                    CivOwningCity = new CivilizationTestData() {
                        TechsDiscovered = new List<TechTestData>()
                    }
                }).SetName("Improvement is constructed but pillaged").Returns(
                    YieldSummary.Empty
                );
            }
        }

        #endregion

        #region instance fields and properties

        private Mock<IPossessionRelationship<IHexCell, IResourceNode>> MockResourceNodeLocationCanon;
        private Mock<IImprovementLocationCanon>                        MockImprovementLocationCanon;
        private Mock<ITechCanon>                                       MockTechCanon;
        private Mock<IPossessionRelationship<ICity, IHexCell>>         MockCellPossessionCanon;
        private Mock<IPossessionRelationship<ICivilization, ICity>>    MockCityPossessionCanon;
        private Mock<IFreshWaterCanon>                                 MockFreshWaterCanon;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockResourceNodeLocationCanon = new Mock<IPossessionRelationship<IHexCell, IResourceNode>>();
            MockImprovementLocationCanon  = new Mock<IImprovementLocationCanon>();
            MockTechCanon                 = new Mock<ITechCanon>();
            MockCellPossessionCanon       = new Mock<IPossessionRelationship<ICity, IHexCell>>();
            MockCityPossessionCanon       = new Mock<IPossessionRelationship<ICivilization, ICity>>();
            MockFreshWaterCanon           = new Mock<IFreshWaterCanon>();

            Container.Bind<IPossessionRelationship<IHexCell, IResourceNode>>().FromInstance(MockResourceNodeLocationCanon.Object);
            Container.Bind<IImprovementLocationCanon>                       ().FromInstance(MockImprovementLocationCanon .Object);
            Container.Bind<ITechCanon>                                      ().FromInstance(MockTechCanon                .Object);
            Container.Bind<IPossessionRelationship<ICity, IHexCell>>        ().FromInstance(MockCellPossessionCanon      .Object);
            Container.Bind<IPossessionRelationship<ICivilization, ICity>>   ().FromInstance(MockCityPossessionCanon      .Object);
            Container.Bind<IFreshWaterCanon>                                ().FromInstance(MockFreshWaterCanon          .Object);

            Container.Bind<ImprovementYieldLogic>().AsSingle();
        }

        #endregion

        #region tests

        [TestCaseSource("GetExpectedYieldTestCases")]
        [Test(Description = "")]
        public YieldSummary GetExpectedYieldTests(ImprovementYieldTestData testData) {
            var location = BuildHexCell(testData.CellHasAccessToFreshWater);

            var improvementTemplate = BuildImprovementTemplate(testData.Improvement.Template);

            if(testData.ResourceNodeAtCell != null) {
                BuildResourceNode(testData.ResourceNodeAtCell, location, improvementTemplate);
            }

            if(testData.CityOwningCell != null) {
                var city = BuildCity(testData.CityOwningCell);

                MockCellPossessionCanon.Setup(canon => canon.GetOwnerOfPossession(location)).Returns(city);

                BuildCivilization(testData.CivOwningCity, city, improvementTemplate);
            }

            var yieldLogic = Container.Resolve<ImprovementYieldLogic>();

            return yieldLogic.GetExpectedYieldOfImprovementOnCell(improvementTemplate, location);
        }

        [TestCaseSource("GetYieldOfImprovementTestCases")]
        [Test(Description = "")]
        public YieldSummary GetYieldOfImprovementTests(ImprovementYieldTestData testData) {
            var location = BuildHexCell(testData.CellHasAccessToFreshWater);

            var improvementTemplate = BuildImprovementTemplate(testData.Improvement.Template);

            var improvement = BuildImprovement(testData.Improvement, location, improvementTemplate);

            if(testData.ResourceNodeAtCell != null) {
                BuildResourceNode(testData.ResourceNodeAtCell, location, improvementTemplate);
            }

            if(testData.CityOwningCell != null) {
                var city = BuildCity(testData.CityOwningCell);

                MockCellPossessionCanon.Setup(canon => canon.GetOwnerOfPossession(location)).Returns(city);

                BuildCivilization(testData.CivOwningCity, city, improvementTemplate);
            }

            var yieldLogic = Container.Resolve<ImprovementYieldLogic>();

            return yieldLogic.GetYieldOfImprovement(improvement);
        }

        #endregion

        #region utilities

        private IHexCell BuildHexCell(bool hasAccessToFreshWater = false) {
            var newCell = new Mock<IHexCell>().Object;

            MockFreshWaterCanon.Setup(canon => canon.HasAccessToFreshWater(newCell)).Returns(hasAccessToFreshWater);

            return newCell;
        }

        private IImprovement BuildImprovement(ImprovementTestData testData, IHexCell location, IImprovementTemplate template) {
            var mockImprovement = new Mock<IImprovement>();

            mockImprovement.Setup(improvement => improvement.IsConstructed).Returns(testData.IsConstructed);
            mockImprovement.Setup(improvement => improvement.IsPillaged)   .Returns(testData.IsPillaged);
            mockImprovement.Setup(improvement => improvement.Template)     .Returns(template);

            var newImprovement = mockImprovement.Object;

            MockImprovementLocationCanon.Setup(canon => canon.GetOwnerOfPossession(newImprovement)).Returns(location);

            return newImprovement;
        }

        private IImprovementTemplate BuildImprovementTemplate(ImprovementTemplateTestData testData) {
            var mockTemplate = new Mock<IImprovementTemplate>();

            mockTemplate.Setup(template => template.BonusYieldNormal).Returns(testData.BonusYieldNormal);

            return mockTemplate.Object;
        }

        private IResourceNode BuildResourceNode(
            ResourceNodeTestData testData, IHexCell location,
            IImprovementTemplate extractorCandidate
        ) {
            var mockNode = new Mock<IResourceNode>();

            var mockDefinition = new Mock<IResourceDefinition>();

            mockDefinition.Setup(definition => definition.BonusYieldWhenImproved)
                .Returns(testData.DefinitionImprovementYield);

            MockTechCanon.Setup(
                canon => canon.IsResourceVisibleToCiv(mockDefinition.Object, It.IsAny<ICivilization>())
            ).Returns(testData.IsVisible);

            if(testData.IsExtractedByImprovement) {
                mockDefinition.Setup(definition => definition.Extractor).Returns(extractorCandidate);
            }

            mockNode.Setup(node => node.Resource).Returns(mockDefinition.Object);

            MockResourceNodeLocationCanon
                .Setup(canon => canon.GetPossessionsOfOwner(location))
                .Returns(new List<IResourceNode>() { mockNode.Object });

            return mockNode.Object;
        }

        private ICity BuildCity(CityTestData testData) {
            return new Mock<ICity>().Object;
        }

        private ICivilization BuildCivilization(
            CivilizationTestData testData, ICity possessedCity,
            IImprovementTemplate candidateImprovementTemplate
        ){
            var newCiv = new Mock<ICivilization>().Object;

            MockTechCanon
                .Setup(canon => canon.GetTechsDiscoveredByCiv(newCiv))
                .Returns(testData.TechsDiscovered.Select(techData => BuildTechDefinition(techData, candidateImprovementTemplate)));

            MockCityPossessionCanon
                .Setup(canon => canon.GetOwnerOfPossession(possessedCity))
                .Returns(newCiv);

            return newCiv;
        }

        private ITechDefinition BuildTechDefinition(TechTestData testData,
            IImprovementTemplate candidateImprovementTemplate
        ) {
            var mockTech = new Mock<ITechDefinition>();

            mockTech
                .Setup(tech => tech.ImprovementYieldModifications)
                .Returns(testData.ImprovementModifications.Select(
                    yieldModData => BuildModificationData(yieldModData, candidateImprovementTemplate)
                ));

            return mockTech.Object;
        }

        private IImprovementModificationData BuildModificationData(
            ImprovementModificationTestData modData,
            IImprovementTemplate candidateImprovementTemplate
        ) {
            var mockData = new Mock<IImprovementModificationData>();

            mockData.Setup(data => data.BonusYield).Returns(modData.BonusYield);
            mockData.Setup(data => data.RequiresFreshWater).Returns(modData.RequiresFreshWater);

            if(modData.AppliesToImprovement) {
                mockData.Setup(data => data.Template).Returns(candidateImprovementTemplate);
            }

            return mockData.Object;
        }

        #endregion

        #endregion

    }

}
