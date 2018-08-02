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

        public class GetYieldOfImprovementTemplateTestData {

            public ImprovementTemplateTestData ImprovementTemplate;

            public ResourceNodeTestData ResourceNodeAtLocation;

            public List<TechTestData> DiscoveredTechs = new List<TechTestData>();

            public bool HasFreshWater;

        }

        public class ImprovementTemplateTestData {

            public YieldSummary BonusYieldNormal;

        }

        public class ResourceNodeTestData {

            public bool IsExtractedByImprovement;

            public bool ResourceIsVisible;

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

        public static IEnumerable GetYieldOfImprovementTemplateTestCases {
            get {
                yield return new TestCaseData(new GetYieldOfImprovementTemplateTestData() {
                    ImprovementTemplate = new ImprovementTemplateTestData() {
                        BonusYieldNormal = new YieldSummary(food: 1, production: 2)
                    },
                    ResourceNodeAtLocation = null
                }).SetName("nodeAtLocation null | adds template's bonus yield").Returns(
                    new YieldSummary(food: 1, production: 2)
                );

                yield return new TestCaseData(new GetYieldOfImprovementTemplateTestData() {
                    ImprovementTemplate = new ImprovementTemplateTestData() {
                        BonusYieldNormal = new YieldSummary(food: 1, production: 2)
                    },
                    ResourceNodeAtLocation = new ResourceNodeTestData() {
                        IsExtractedByImprovement = false,
                        ResourceIsVisible = true
                    }
                }).SetName("nodeAtLocation's resource has invalid extractor | adds template's bonus yield").Returns(
                    new YieldSummary(food: 1, production: 2)
                );

                yield return new TestCaseData(new GetYieldOfImprovementTemplateTestData() {
                    ImprovementTemplate = new ImprovementTemplateTestData() {
                        BonusYieldNormal = new YieldSummary(food: 1, production: 2)
                    },
                    ResourceNodeAtLocation = new ResourceNodeTestData() {
                        IsExtractedByImprovement = true,
                        ResourceIsVisible = false
                    }
                }).SetName("nodeAtLocation's resource not visible | adds template's bonus yield").Returns(
                    new YieldSummary(food: 1, production: 2)
                );

                yield return new TestCaseData(new GetYieldOfImprovementTemplateTestData() {
                    ImprovementTemplate = new ImprovementTemplateTestData() {
                        BonusYieldNormal = new YieldSummary(food: 1, production: 2)
                    },
                    ResourceNodeAtLocation = new ResourceNodeTestData() {
                        IsExtractedByImprovement = true,
                        ResourceIsVisible = true
                    }
                }).SetName("nodeAtLocation exists and has valid visible resource | ignores template's bonus yield").Returns(
                    new YieldSummary()
                );





                yield return new TestCaseData(new GetYieldOfImprovementTemplateTestData() {
                    ImprovementTemplate = new ImprovementTemplateTestData() {
                        BonusYieldNormal = new YieldSummary(food: 1, production: 2)
                    },
                    HasFreshWater = false,
                    DiscoveredTechs = new List<TechTestData>() {
                        new TechTestData() {
                            ImprovementModifications = new List<ImprovementModificationTestData>() {
                                new ImprovementModificationTestData() {
                                    AppliesToImprovement = true, RequiresFreshWater = false,
                                    BonusYield = new YieldSummary(production: 4, gold: 10)
                                },
                                new ImprovementModificationTestData() {
                                    AppliesToImprovement = true, RequiresFreshWater = false,
                                    BonusYield = new YieldSummary(culture: 5)
                                },
                            }
                        }
                    }
                }).SetName("Some tech mods don't require fresh water, and no fresh water present | mods included").Returns(
                    new YieldSummary(food: 1, production: 6, gold: 10, culture: 5)
                );

                yield return new TestCaseData(new GetYieldOfImprovementTemplateTestData() {
                    ImprovementTemplate = new ImprovementTemplateTestData() {
                        BonusYieldNormal = new YieldSummary(food: 1, production: 2)
                    },
                    HasFreshWater = true,
                    DiscoveredTechs = new List<TechTestData>() {
                        new TechTestData() {
                            ImprovementModifications = new List<ImprovementModificationTestData>() {
                                new ImprovementModificationTestData() {
                                    AppliesToImprovement = true, RequiresFreshWater = false,
                                    BonusYield = new YieldSummary(production: 4, gold: 10)
                                },
                                new ImprovementModificationTestData() {
                                    AppliesToImprovement = true, RequiresFreshWater = false,
                                    BonusYield = new YieldSummary(culture: 5)
                                },
                            }
                        }
                    }
                }).SetName("Some tech mods don't require fresh water, and fresh water present | mods included").Returns(
                    new YieldSummary(food: 1, production: 6, gold: 10, culture: 5)
                );

                yield return new TestCaseData(new GetYieldOfImprovementTemplateTestData() {
                    ImprovementTemplate = new ImprovementTemplateTestData() {
                        BonusYieldNormal = new YieldSummary(food: 1, production: 2)
                    },
                    HasFreshWater = false,
                    DiscoveredTechs = new List<TechTestData>() {
                        new TechTestData() {
                            ImprovementModifications = new List<ImprovementModificationTestData>() {
                                new ImprovementModificationTestData() {
                                    AppliesToImprovement = true, RequiresFreshWater = true,
                                    BonusYield = new YieldSummary(production: 4, gold: 10)
                                },
                                new ImprovementModificationTestData() {
                                    AppliesToImprovement = true, RequiresFreshWater = true,
                                    BonusYield = new YieldSummary(culture: 5)
                                },
                            }
                        }
                    }
                }).SetName("Some tech mods require fresh water, and no fresh water present | mods not included").Returns(
                    new YieldSummary(food: 1, production: 2)
                );

                yield return new TestCaseData(new GetYieldOfImprovementTemplateTestData() {
                    ImprovementTemplate = new ImprovementTemplateTestData() {
                        BonusYieldNormal = new YieldSummary(food: 1, production: 2)
                    },
                    HasFreshWater = true,
                    DiscoveredTechs = new List<TechTestData>() {
                        new TechTestData() {
                            ImprovementModifications = new List<ImprovementModificationTestData>() {
                                new ImprovementModificationTestData() {
                                    AppliesToImprovement = true, RequiresFreshWater = true,
                                    BonusYield = new YieldSummary(production: 4, gold: 10)
                                },
                                new ImprovementModificationTestData() {
                                    AppliesToImprovement = true, RequiresFreshWater = true,
                                    BonusYield = new YieldSummary(culture: 5)
                                },
                            }
                        }
                    }
                }).SetName("Some tech mods require fresh water, and fresh water present | mods included").Returns(
                    new YieldSummary(food: 1, production: 6, gold: 10, culture: 5)
                );



                yield return new TestCaseData(new GetYieldOfImprovementTemplateTestData() {
                    ImprovementTemplate = new ImprovementTemplateTestData() {
                        BonusYieldNormal = new YieldSummary(food: 1, production: 2)
                    },
                    HasFreshWater = false,
                    DiscoveredTechs = new List<TechTestData>() {
                        new TechTestData() {
                            ImprovementModifications = new List<ImprovementModificationTestData>() {
                                new ImprovementModificationTestData() {
                                    AppliesToImprovement = false, RequiresFreshWater = false,
                                    BonusYield = new YieldSummary(production: 4, gold: 10)
                                },
                                new ImprovementModificationTestData() {
                                    AppliesToImprovement = false, RequiresFreshWater = false,
                                    BonusYield = new YieldSummary(culture: 5)
                                },
                            }
                        }
                    }
                }).SetName("Some tech mods don't apply to improvement | mods not included").Returns(
                    new YieldSummary(food: 1, production: 2)
                );
            }
        }

        #endregion

        #region instance fields and properties

        

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            Container.Bind<ImprovementYieldLogic>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        [TestCaseSource("GetYieldOfImprovementTemplateTestCases")]
        public YieldSummary GetYieldOfImprovementTemplateTests(GetYieldOfImprovementTemplateTestData testData) {
            var templateToTest = BuildImprovementTemplate(testData.ImprovementTemplate);

            var resourceExtractedBy    = BuildResourceDefinition(templateToTest);
            var resourceNotExtractedBy = BuildResourceDefinition(null);

            IResourceNode nodeAtLocation = null;
            var visibleResources = new List<IResourceDefinition>();

            if(testData.ResourceNodeAtLocation != null) {
                var resourceInNode = testData.ResourceNodeAtLocation.IsExtractedByImprovement ? resourceExtractedBy : resourceNotExtractedBy;

                nodeAtLocation = BuildResourceNode(resourceInNode);

                if(testData.ResourceNodeAtLocation.ResourceIsVisible) {
                    visibleResources.Add(resourceInNode);
                }
            }
            
            var discoveredTechs = testData.DiscoveredTechs.Select(techData => BuildTechDefinition(techData, templateToTest));

            var improvementYieldLogic = Container.Resolve<ImprovementYieldLogic>();

            return improvementYieldLogic.GetYieldOfImprovementTemplate(
                templateToTest, nodeAtLocation, visibleResources, discoveredTechs, testData.HasFreshWater
            );
        }

        [Test]
        public void GetYieldOfImprovement_ReturnsEmptyIfImprovementNotConstructed() {
            var template = BuildImprovementTemplate(new ImprovementTemplateTestData() {
                BonusYieldNormal = new YieldSummary(food: 1, production: 2)
            });

            var improvement = BuildImprovement(template, false, false);

            var yieldLogic = Container.Resolve<ImprovementYieldLogic>();

            Assert.AreEqual(
                new YieldSummary(),
                yieldLogic.GetYieldOfImprovement(
                    improvement, null, new List<IResourceDefinition>(),
                    new List<ITechDefinition>(), false
                )
            );
        }

        [Test]
        public void GetYieldOfImprovement_ReturnsEmptyIfImprovementPillaged() {
            var template = BuildImprovementTemplate(new ImprovementTemplateTestData() {
                BonusYieldNormal = new YieldSummary(food: 1, production: 2)
            });

            var improvement = BuildImprovement(template, true, true);

            var yieldLogic = Container.Resolve<ImprovementYieldLogic>();

            Assert.AreEqual(
                new YieldSummary(),
                yieldLogic.GetYieldOfImprovement(
                    improvement, null, new List<IResourceDefinition>(),
                    new List<ITechDefinition>(), false
                )
            );
        }

        [Test]
        public void GetYieldOfImprovement_ReturnsTemplateYieldIfConstructedAndNotPillaged() {
            var template = BuildImprovementTemplate(new ImprovementTemplateTestData() {
                BonusYieldNormal = new YieldSummary(food: 1, production: 2)
            });

            var improvement = BuildImprovement(template, true, false);

            var yieldLogic = Container.Resolve<ImprovementYieldLogic>();

            Assert.AreEqual(
                new YieldSummary(food: 1, production: 2),
                yieldLogic.GetYieldOfImprovement(
                    improvement, null, new List<IResourceDefinition>(),
                    new List<ITechDefinition>(), false
                )
            );
        }

        #endregion

        #region utilities

        private IImprovement BuildImprovement(
            IImprovementTemplate template, bool isConstructed, bool isPillaged
        ) {
            var mockImprovement = new Mock<IImprovement>();

            mockImprovement.Setup(improvement => improvement.IsConstructed).Returns(isConstructed);
            mockImprovement.Setup(improvement => improvement.IsPillaged)   .Returns(isPillaged);
            mockImprovement.Setup(improvement => improvement.Template)     .Returns(template);

            var newImprovement = mockImprovement.Object;

            return newImprovement;
        }

        private IImprovementTemplate BuildImprovementTemplate(ImprovementTemplateTestData testData) {
            var mockTemplate = new Mock<IImprovementTemplate>();

            mockTemplate.Setup(template => template.BonusYieldNormal).Returns(testData.BonusYieldNormal);

            return mockTemplate.Object;
        }

        private IResourceDefinition BuildResourceDefinition(IImprovementTemplate extractor) {
            var mockResource = new Mock<IResourceDefinition>();

            mockResource.Setup(resource => resource.Extractor).Returns(extractor);

            return mockResource.Object;
        }

        private IResourceNode BuildResourceNode(IResourceDefinition resource) {
            var mockNode = new Mock<IResourceNode>();

            mockNode.Setup(node => node.Resource).Returns(resource);

            return mockNode.Object;
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
