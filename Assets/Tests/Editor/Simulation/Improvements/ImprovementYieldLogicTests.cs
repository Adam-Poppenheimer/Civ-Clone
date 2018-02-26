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
using Assets.Simulation.SpecialtyResources;

using UnityCustomUtilities.Extensions;

namespace Assets.Tests.Simulation.Improvements {

    public class ImprovementYieldLogicTests : ZenjectUnitTestFixture {

        #region internal types

        public class GetExpectedYieldTestData {

            public ImprovementTemplateTestData Improvement;

            public ResourceNodeTestData ResourceNodeAtCell;

            public CityTestData CityOwningCell;

            public CivilizationTestData CivOwningCity;

        }

        public class ImprovementTemplateTestData {

            public ResourceSummary BonusYieldNormal;

        }

        public class ResourceNodeTestData {

            public ResourceSummary DefinitionImprovementYield;

            public bool IsExtractedByImprovement;

        }

        public class CityTestData {



        }

        public class CivilizationTestData {

            public List<TechTestData> TechsDiscovered;

        }

        public class TechTestData {

            public List<Tuple<ResourceSummary, bool>> ImprovementModifications;

        }

        #endregion

        #region static fields and properties

        public static IEnumerable GetExpectedYieldTestCases {
            get {
                yield return new TestCaseData(new GetExpectedYieldTestData() {
                    Improvement = new ImprovementTemplateTestData() {
                        BonusYieldNormal = new ResourceSummary(food: 1)
                    },
                    ResourceNodeAtCell = new ResourceNodeTestData() {
                        DefinitionImprovementYield = new ResourceSummary(production: 2),
                        IsExtractedByImprovement = false
                    },
                    CityOwningCell = new CityTestData(),
                    CivOwningCity = new CivilizationTestData() {
                        TechsDiscovered = new List<TechTestData>()
                    }
                }).SetName("Non-extracting improvement, no techs").Returns(
                    new ResourceSummary(food: 1)
                );

                yield return new TestCaseData(new GetExpectedYieldTestData() {
                    Improvement = new ImprovementTemplateTestData() {
                        BonusYieldNormal = new ResourceSummary(food: 1)
                    },
                    ResourceNodeAtCell = new ResourceNodeTestData() {
                        DefinitionImprovementYield = new ResourceSummary(production: 2),
                        IsExtractedByImprovement = true
                    },
                    CityOwningCell = new CityTestData(),
                    CivOwningCity = new CivilizationTestData() {
                        TechsDiscovered = new List<TechTestData>()
                    }
                }).SetName("Extracting improvement, no techs").Returns(
                    new ResourceSummary(production: 2)
                );

                yield return new TestCaseData(new GetExpectedYieldTestData() {
                    Improvement = new ImprovementTemplateTestData() {
                        BonusYieldNormal = new ResourceSummary(food: 1)
                    },
                    ResourceNodeAtCell = new ResourceNodeTestData() {
                        DefinitionImprovementYield = new ResourceSummary(production: 2),
                        IsExtractedByImprovement = false
                    },
                    CityOwningCell = new CityTestData(),
                    CivOwningCity = new CivilizationTestData() {
                        TechsDiscovered = new List<TechTestData>() {
                            new TechTestData() {
                                ImprovementModifications = new List<Tuple<ResourceSummary, bool>>() {
                                    new Tuple<ResourceSummary, bool>(new ResourceSummary(gold: 1), true),
                                    new Tuple<ResourceSummary, bool>(new ResourceSummary(culture: 2), true),
                                }
                            },
                            new TechTestData() {
                                ImprovementModifications = new List<Tuple<ResourceSummary, bool>>() {
                                    new Tuple<ResourceSummary, bool>(new ResourceSummary(science: 3), true)
                                }
                            }
                        }
                    }
                }).SetName("Non-extracting improvement, techs that modify yield").Returns(
                    new ResourceSummary(food: 1, gold: 1, culture: 2, science: 3)
                );

                yield return new TestCaseData(new GetExpectedYieldTestData() {
                    Improvement = new ImprovementTemplateTestData() {
                        BonusYieldNormal = new ResourceSummary(food: 1)
                    },
                    ResourceNodeAtCell = new ResourceNodeTestData() {
                        DefinitionImprovementYield = new ResourceSummary(production: 2),
                        IsExtractedByImprovement = false
                    },
                    CityOwningCell = new CityTestData(),
                    CivOwningCity = new CivilizationTestData() {
                        TechsDiscovered = new List<TechTestData>() {
                            new TechTestData() {
                                ImprovementModifications = new List<Tuple<ResourceSummary, bool>>() {
                                    new Tuple<ResourceSummary, bool>(new ResourceSummary(gold: 1), false),
                                    new Tuple<ResourceSummary, bool>(new ResourceSummary(culture: 2), false),
                                }
                            },
                            new TechTestData() {
                                ImprovementModifications = new List<Tuple<ResourceSummary, bool>>() {
                                    new Tuple<ResourceSummary, bool>(new ResourceSummary(science: 3), false)
                                }
                            }
                        }
                    }
                }).SetName("Non-extracting improvement, techs that don't modify yield").Returns(
                    new ResourceSummary(food: 1)
                );

                yield return new TestCaseData(new GetExpectedYieldTestData() {
                    Improvement = new ImprovementTemplateTestData() {
                        BonusYieldNormal = new ResourceSummary(food: 1)
                    },
                    ResourceNodeAtCell = new ResourceNodeTestData() {
                        DefinitionImprovementYield = new ResourceSummary(production: 2),
                        IsExtractedByImprovement = false
                    },
                    CityOwningCell = new CityTestData(),
                    CivOwningCity = new CivilizationTestData() {
                        TechsDiscovered = new List<TechTestData>() {
                            new TechTestData() {
                                ImprovementModifications = new List<Tuple<ResourceSummary, bool>>() {
                                    new Tuple<ResourceSummary, bool>(new ResourceSummary(gold: 1), false),
                                    new Tuple<ResourceSummary, bool>(new ResourceSummary(culture: 2), true),
                                }
                            },
                            new TechTestData() {
                                ImprovementModifications = new List<Tuple<ResourceSummary, bool>>() {
                                    new Tuple<ResourceSummary, bool>(new ResourceSummary(science: 3), false)
                                }
                            }
                        }
                    }
                }).SetName("Non-extracting improvement, only some tech modifications affect template").Returns(
                    new ResourceSummary(food: 1, culture: 2)
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

            Container.Bind<IPossessionRelationship<IHexCell, IResourceNode>>().FromInstance(MockResourceNodeLocationCanon.Object);
            Container.Bind<IImprovementLocationCanon>                       ().FromInstance(MockImprovementLocationCanon .Object);
            Container.Bind<ITechCanon>                                      ().FromInstance(MockTechCanon                .Object);
            Container.Bind<IPossessionRelationship<ICity, IHexCell>>        ().FromInstance(MockCellPossessionCanon      .Object);
            Container.Bind<IPossessionRelationship<ICivilization, ICity>>   ().FromInstance(MockCityPossessionCanon      .Object);

            Container.Bind<ImprovementYieldLogic>().AsSingle();
        }

        #endregion

        #region tests

        [Test(Description = "")]
        [TestCaseSource("GetExpectedYieldTestCases")]
        public ResourceSummary GetExpectedYieldTests(GetExpectedYieldTestData testData) {
            var location = BuildHexCell();

            var improvementTemplate = BuildImprovementTemplate(testData.Improvement);

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

        #endregion

        #region utilities

        private IHexCell BuildHexCell() {
            return new Mock<IHexCell>().Object;
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

            var mockDefinition = new Mock<ISpecialtyResourceDefinition>();

            mockDefinition.Setup(definition => definition.BonusYieldWhenImproved)
                .Returns(testData.DefinitionImprovementYield);

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
                    tuple => BuildModificationData(tuple, candidateImprovementTemplate)
                ));

            return mockTech.Object;
        }

        private IImprovementModificationData BuildModificationData(
            Tuple<ResourceSummary, bool> dataTuple,
            IImprovementTemplate candidateImprovementTemplate
        ) {
            var mockData = new Mock<IImprovementModificationData>();

            mockData.Setup(data => data.BonusYield).Returns(dataTuple.Item1);

            if(dataTuple.Item2) {
                mockData.Setup(data => data.Template).Returns(candidateImprovementTemplate);
            }

            return mockData.Object;
        }

        #endregion

        #endregion

    }

}
