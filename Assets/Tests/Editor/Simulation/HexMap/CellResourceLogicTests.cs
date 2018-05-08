using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation;
using Assets.Simulation.HexMap;
using Assets.Simulation.SpecialtyResources;
using Assets.Simulation.Improvements;
using Assets.Simulation.Cities;
using Assets.Simulation.Cities.Buildings;
using Assets.Simulation.Civilizations;
using Assets.Simulation.Core;
using Assets.Simulation.Technology;

using UnityCustomUtilities.Extensions;

namespace Assets.Tests.Simulation.HexMap {

    [TestFixture]
    public class CellResourceLogicTests : ZenjectUnitTestFixture {

        #region internal types

        public struct GetYieldOfCellTestData {

            public TerrainType    Terrain;
            public TerrainFeature Feature;
            public TerrainShape   Shape;

            public ResourceNodeData ResourceNodeOnCell;

            public ImprovementData ImprovementOnCell;

            public CityData CityOwningCell;

            public CivilizationData CivOwningCell;

        }

        public class ResourceNodeData {

            public ResourceDefinitionData Resource;            

        }

        public class ResourceDefinitionData {

            public ResourceSummary BonusYield;

            public bool IsVisible;

        }

        public class ImprovementData {

            public ResourceSummary Yield;

        }

        public class CityData {

            public List<BuildingData> Buildings = new List<BuildingData>();

        }

        public class BuildingData {

            public List<Tuple<ResourceSummary, bool>> ResourceYieldModifications = new List<Tuple<ResourceSummary, bool>>();

            public List<CellYieldModificationData> CellYieldModifications = new List<CellYieldModificationData>();

        }

        public class CivilizationData {



        }

        #endregion

        #region static fields and properties

        private static IEnumerable<TestCaseData> TestCases {
            get {
                yield return new TestCaseData(new GetYieldOfCellTestData() {
                    Terrain = TerrainType.Grassland,
                    Feature = TerrainFeature.None,
                    Shape   = TerrainShape.Flatlands,
                    ResourceNodeOnCell = null
                }).SetName("Grassland/Flat/no feature/no resource").Returns(new ResourceSummary(1f));

                yield return new TestCaseData(new GetYieldOfCellTestData() {
                    Terrain = TerrainType.Plains,
                    Feature = TerrainFeature.None,
                    Shape   = TerrainShape.Flatlands,
                    ResourceNodeOnCell = null
                }).SetName("Plains/Flat/no feature/no resource").Returns(new ResourceSummary(1.1f));

                yield return new TestCaseData(new GetYieldOfCellTestData() {
                    Terrain = TerrainType.Desert,
                    Feature = TerrainFeature.None,
                    Shape   = TerrainShape.Flatlands,
                    ResourceNodeOnCell = null
                }).SetName("Desert/Flat/no feature/no resource").Returns(new ResourceSummary(1.2f));

                yield return new TestCaseData(new GetYieldOfCellTestData() {
                    Terrain = TerrainType.Tundra,
                    Feature = TerrainFeature.None,
                    Shape   = TerrainShape.Flatlands,
                    ResourceNodeOnCell = null
                }).SetName("Tundra/Flat/no feature/no resource").Returns(new ResourceSummary(1.3f));

                yield return new TestCaseData(new GetYieldOfCellTestData() {
                    Terrain = TerrainType.Snow,
                    Feature = TerrainFeature.None,
                    Shape   = TerrainShape.Flatlands,
                    ResourceNodeOnCell = null
                }).SetName("Snow/Flat/no feature/no resource").Returns(new ResourceSummary(1.4f));




                yield return new TestCaseData(new GetYieldOfCellTestData() {
                    Terrain = TerrainType.Grassland,
                    Feature = TerrainFeature.Forest,
                    Shape   = TerrainShape.Flatlands,
                    ResourceNodeOnCell = null
                }).SetName("Grassland/Flat/Forest/no resource").Returns(new ResourceSummary(2f));

                yield return new TestCaseData(new GetYieldOfCellTestData() {
                    Terrain = TerrainType.Plains,
                    Feature = TerrainFeature.Forest,
                    Shape   = TerrainShape.Flatlands,
                    ResourceNodeOnCell = null
                }).SetName("Plains/Flat/Forest/no resource").Returns(new ResourceSummary(2f));

                yield return new TestCaseData(new GetYieldOfCellTestData() {
                    Terrain = TerrainType.Desert,
                    Feature = TerrainFeature.Forest,
                    Shape   = TerrainShape.Flatlands,
                    ResourceNodeOnCell = null
                }).SetName("Desert/Flat/Forest/no resource").Returns(new ResourceSummary(2f));

                yield return new TestCaseData(new GetYieldOfCellTestData() {
                    Terrain = TerrainType.Tundra,
                    Feature = TerrainFeature.Forest,
                    Shape   = TerrainShape.Flatlands,
                    ResourceNodeOnCell = null
                }).SetName("Tundra/Flat/Forest/no resource").Returns(new ResourceSummary(2f));

                yield return new TestCaseData(new GetYieldOfCellTestData() {
                    Terrain = TerrainType.Snow,
                    Feature = TerrainFeature.Forest,
                    Shape   = TerrainShape.Flatlands,
                    ResourceNodeOnCell = null
                }).SetName("Snow/Flat/Forest/no resource").Returns(new ResourceSummary(2f));



                yield return new TestCaseData(new GetYieldOfCellTestData() {
                    Terrain = TerrainType.Grassland,
                    Feature = TerrainFeature.None,
                    Shape   = TerrainShape.Flatlands,
                    ResourceNodeOnCell = new ResourceNodeData() {
                        Resource = new ResourceDefinitionData() {
                            BonusYield = new ResourceSummary(0f, 2f), IsVisible = true
                        }
                    },
                    CityOwningCell = new CityData(),
                    CivOwningCell = new CivilizationData()
                }).SetName("Grassland/Flat/no feature/Has a visible resource").Returns(new ResourceSummary(1f, 2f));

                yield return new TestCaseData(new GetYieldOfCellTestData() {
                    Terrain = TerrainType.Grassland,
                    Feature = TerrainFeature.None,
                    Shape   = TerrainShape.Flatlands,
                    ResourceNodeOnCell = new ResourceNodeData() {
                        Resource = new ResourceDefinitionData() {
                            BonusYield = new ResourceSummary(0f, 2f), IsVisible = false
                        }
                    },
                    CityOwningCell = new CityData(),
                    CivOwningCell = new CivilizationData()
                }).SetName("Grassland/Flat/no feature/Has an invisible resource").Returns(new ResourceSummary(1f));

                yield return new TestCaseData(new GetYieldOfCellTestData() {
                    Terrain = TerrainType.Grassland,
                    Feature = TerrainFeature.None,
                    Shape   = TerrainShape.Flatlands,
                    ResourceNodeOnCell = new ResourceNodeData() {
                        Resource = new ResourceDefinitionData() {
                            BonusYield = new ResourceSummary(0f, 2f), IsVisible = true
                        }
                    }
                }).SetName("Grassland/Flat/no feature/Has a visible resource but no civ").Returns(new ResourceSummary(1f));

                yield return new TestCaseData(new GetYieldOfCellTestData() {
                    Terrain = TerrainType.Grassland,
                    Feature = TerrainFeature.Forest,
                    Shape   = TerrainShape.Flatlands,
                    ResourceNodeOnCell = new ResourceNodeData() {
                        Resource = new ResourceDefinitionData() {
                            BonusYield = new ResourceSummary(0f, 2f), IsVisible = true
                        }
                    },
                    CityOwningCell = new CityData(),
                    CivOwningCell = new CivilizationData()
                }).SetName("Grassland/Flat/Forest/Has a visible resource").Returns(new ResourceSummary(2f, 2f));




                yield return new TestCaseData(new GetYieldOfCellTestData() {
                    Terrain = TerrainType.Grassland,
                    Feature = TerrainFeature.None,
                    Shape   = TerrainShape.Flatlands,
                    ImprovementOnCell = new ImprovementData() {
                        Yield = new ResourceSummary(0f, 0f, 3f)
                    }
                }).SetName("Grassland/Flat/no feature/Has an improvement").Returns(new ResourceSummary(1f, 0f, 3f));

                yield return new TestCaseData(new GetYieldOfCellTestData() {
                    Terrain = TerrainType.Grassland,
                    Feature = TerrainFeature.Forest,
                    Shape   = TerrainShape.Flatlands,
                    ImprovementOnCell = new ImprovementData() {
                        Yield = new ResourceSummary(0f, 0f, 3f)
                    }
                }).SetName("Grassland/Flat/Forest/Has an improvement").Returns(new ResourceSummary(2f, 0f, 3f));


                yield return new TestCaseData(new GetYieldOfCellTestData() {
                    Terrain = TerrainType.Grassland,
                    Feature = TerrainFeature.None,
                    Shape   = TerrainShape.Flatlands,
                    ResourceNodeOnCell = new ResourceNodeData() {
                        Resource = new ResourceDefinitionData() {
                            BonusYield = new ResourceSummary(0f, 2f), IsVisible = true
                        }
                    },
                    ImprovementOnCell = new ImprovementData() {
                        Yield = new ResourceSummary(0f, 0f, 3f)
                    },
                    CityOwningCell = new CityData(),
                    CivOwningCell = new CivilizationData()
                }).SetName("Grassland/Flat/no feature/Has a visible resource and an improvement").Returns(new ResourceSummary(1f, 2f, 3f));

                yield return new TestCaseData(new GetYieldOfCellTestData() {
                    Terrain = TerrainType.Grassland,
                    Feature = TerrainFeature.Forest,
                    Shape   = TerrainShape.Flatlands,
                    ResourceNodeOnCell = new ResourceNodeData() {
                        Resource = new ResourceDefinitionData() {
                            BonusYield = new ResourceSummary(0f, 2f), IsVisible = true
                        }
                    },
                    ImprovementOnCell = new ImprovementData() {
                        Yield = new ResourceSummary(0f, 0f, 3f)
                    },
                    CityOwningCell = new CityData(),
                    CivOwningCell = new CivilizationData()
                }).SetName("Grassland/Flat/Forest/Has a visible resource and an improvement").Returns(new ResourceSummary(2f, 2f, 3f));


                yield return new TestCaseData(new GetYieldOfCellTestData() {
                    Terrain = TerrainType.Grassland,
                    Feature = TerrainFeature.None,
                    Shape   = TerrainShape.Flatlands,
                    ResourceNodeOnCell = new ResourceNodeData() {
                        Resource = new ResourceDefinitionData() {
                            BonusYield = new ResourceSummary(food: 2), IsVisible = true
                        }
                    },
                    CityOwningCell = new CityData() {
                        Buildings = new List<BuildingData>() {
                            new BuildingData() {
                                ResourceYieldModifications = new List<Tuple<ResourceSummary, bool>>() {
                                    new Tuple<ResourceSummary, bool>(new ResourceSummary(production: 2), true),
                                    new Tuple<ResourceSummary, bool>(new ResourceSummary(gold: 2), false),
                                }
                            },
                            new BuildingData() {
                                ResourceYieldModifications = new List<Tuple<ResourceSummary, bool>>() {
                                    new Tuple<ResourceSummary, bool>(new ResourceSummary(culture: 2), false),
                                    new Tuple<ResourceSummary, bool>(new ResourceSummary(science: 2), true),
                                }
                            }
                        }
                    },
                    CivOwningCell = new CivilizationData()
                }).SetName("Grassland/Flat/No feature/Has visible resource and modifying buildings")
                .Returns(new ResourceSummary(food: 3, production: 2, science: 2));


                yield return new TestCaseData(new GetYieldOfCellTestData() {
                    Terrain = TerrainType.Grassland,
                    Feature = TerrainFeature.None,
                    Shape   = TerrainShape.Hills,
                    ResourceNodeOnCell = null
                }).SetName("Grassland/Hills/No feature").Returns(new ResourceSummary(10f));

                yield return new TestCaseData(new GetYieldOfCellTestData() {
                    Terrain = TerrainType.Grassland,
                    Feature = TerrainFeature.None,
                    Shape   = TerrainShape.Mountains,
                    ResourceNodeOnCell = null
                }).SetName("Grassland/Mountains/No feature").Returns(new ResourceSummary(20f));

                yield return new TestCaseData(new GetYieldOfCellTestData() {
                    Terrain = TerrainType.Grassland,
                    Feature = TerrainFeature.Forest,
                    Shape   = TerrainShape.Hills,
                    ResourceNodeOnCell = null
                }).SetName("Grassland/Hills/Forest").Returns(new ResourceSummary(2f));


                yield return new TestCaseData(new GetYieldOfCellTestData() {
                    Terrain = TerrainType.Grassland,
                    Feature = TerrainFeature.None,
                    Shape = TerrainShape.Flatlands,
                    ResourceNodeOnCell = null,
                    CityOwningCell = new CityData() {
                        Buildings = new List<BuildingData>() {
                            new BuildingData() {
                                CellYieldModifications = new List<CellYieldModificationData>() {
                                    new CellYieldModificationData(TerrainType.Grassland, new ResourceSummary(culture: 1)),
                                    new CellYieldModificationData(TerrainShape.Hills,    new ResourceSummary(culture: 2)),
                                    new CellYieldModificationData(TerrainFeature.None,   new ResourceSummary(culture: 3)),
                                    new CellYieldModificationData(true,                  new ResourceSummary(culture: 4))
                                }
                            },
                            new BuildingData() {
                                CellYieldModifications = new List<CellYieldModificationData>() {
                                    new CellYieldModificationData(TerrainType.Desert,     new ResourceSummary(gold: 1)),
                                    new CellYieldModificationData(TerrainShape.Flatlands, new ResourceSummary(gold: 2)),
                                    new CellYieldModificationData(TerrainFeature.Forest,  new ResourceSummary(gold: 3)),
                                    new CellYieldModificationData(false,                  new ResourceSummary(gold: 4))
                                }
                            }
                        }
                    }
                }).SetName("Grassland/Flat/No feature, has buildings that modify based on cell properties")
                .Returns(new ResourceSummary(food: 1, gold: 6, culture: 4));
            }
        }

        private static Dictionary<TerrainType, ResourceSummary> TerrainYields = new Dictionary<TerrainType, ResourceSummary>() {
            { TerrainType.Grassland, new ResourceSummary(1f)   },
            { TerrainType.Plains,    new ResourceSummary(1.1f) },
            { TerrainType.Desert,    new ResourceSummary(1.2f) },
            { TerrainType.Tundra,    new ResourceSummary(1.3f) },
            { TerrainType.Snow,      new ResourceSummary(1.4f) },
        };

        private static Dictionary<TerrainFeature, ResourceSummary> FeatureYields = new Dictionary<TerrainFeature, ResourceSummary>() {
            { TerrainFeature.None,   new ResourceSummary(0f) },
            { TerrainFeature.Forest, new ResourceSummary(2f) },
        };

        private static Dictionary<TerrainShape, ResourceSummary> ShapeYields = new Dictionary<TerrainShape, ResourceSummary>() {
            { TerrainShape.Flatlands, new ResourceSummary(0f) },
            { TerrainShape.Hills,     new ResourceSummary(10f) },
            { TerrainShape.Mountains, new ResourceSummary(20f) },
        };

        #endregion

        #region instance fields and properties

        private Mock<IHexMapConfig>                                    MockConfig;
        private Mock<IPossessionRelationship<IHexCell, IResourceNode>> MockNodePositionCanon;
        private Mock<IImprovementLocationCanon>                        MockImprovementLocationCanon;
        private Mock<IImprovementYieldLogic>                           MockImprovementYieldLogic;
        private Mock<IPossessionRelationship<ICity, IHexCell>>         MockCellPossessionCanon;
        private Mock<IPossessionRelationship<ICity, IBuilding>>        MockBuildingPossessionCanon;
        private Mock<IPossessionRelationship<ICivilization, ICity>>    MockCityPossessionCanon;
        private Mock<ITechCanon>                                       MockTechCanon;
        private Mock<IGameCore>                                        MockGameCore;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockConfig                   = new Mock<IHexMapConfig>();
            MockNodePositionCanon        = new Mock<IPossessionRelationship<IHexCell, IResourceNode>>();
            MockImprovementLocationCanon = new Mock<IImprovementLocationCanon>();
            MockImprovementYieldLogic    = new Mock<IImprovementYieldLogic>();
            MockCellPossessionCanon      = new Mock<IPossessionRelationship<ICity, IHexCell>>();
            MockBuildingPossessionCanon  = new Mock<IPossessionRelationship<ICity, IBuilding>>();
            MockCityPossessionCanon      = new Mock<IPossessionRelationship<ICivilization, ICity>>();
            MockTechCanon                = new Mock<ITechCanon>();
            MockGameCore                 = new Mock<IGameCore>();

            MockConfig.Setup(config => config.GetYieldOfFeature(It.IsAny<TerrainFeature>()))
                      .Returns<TerrainFeature>(feature => FeatureYields[feature]);

            MockConfig.Setup(config => config.GetYieldOfTerrain(It.IsAny<TerrainType>()))
                      .Returns<TerrainType>(feature => TerrainYields[feature]);

            MockConfig.Setup(config => config.GetYieldOfShape(It.IsAny<TerrainShape>()))
                      .Returns<TerrainShape>(feature => ShapeYields[feature]);

            Container.Bind<IHexMapConfig>                                  ().FromInstance(MockConfig                  .Object);
            Container.Bind<IPossessionRelationship<IHexCell, IResourceNode>>().FromInstance(MockNodePositionCanon       .Object);
            Container.Bind<IImprovementLocationCanon>                       ().FromInstance(MockImprovementLocationCanon.Object);
            Container.Bind<IImprovementYieldLogic>                          ().FromInstance(MockImprovementYieldLogic   .Object);
            Container.Bind<IPossessionRelationship<ICity, IHexCell>>        ().FromInstance(MockCellPossessionCanon     .Object);
            Container.Bind<IPossessionRelationship<ICity, IBuilding>>       ().FromInstance(MockBuildingPossessionCanon .Object);
            Container.Bind<IPossessionRelationship<ICivilization, ICity>>   ().FromInstance(MockCityPossessionCanon     .Object);
            Container.Bind<ITechCanon>                                      ().FromInstance(MockTechCanon               .Object);
            Container.Bind<IGameCore>                                       ().FromInstance(MockGameCore                .Object);

            Container.Bind<CellResourceLogic>().AsSingle();
        }

        #endregion

        #region tests

        [Test(Description = "GetYieldOfCell should return the yield of the cell's terrain " +
            "if it has no feature, and the yield of the feature if it does. If the cell has a " +
            "ResourceNode, it should also add that resource's bonus yield to the result returned.")]
        [TestCaseSource("TestCases")]
        public ResourceSummary GetYieldOfCellTests(GetYieldOfCellTestData data) {
            var cell = BuildCell(data.Terrain, data.Feature, data.Shape);

            IResourceNode node = null;
            if(data.ResourceNodeOnCell != null) {
                var resource = BuildResourceDefinition(data.ResourceNodeOnCell.Resource);
                node = BuildResourceNode(cell, resource);
            }

            if(data.CityOwningCell != null) {
                BuildCity(cell, data.CityOwningCell, node != null ? node.Resource : null);
            }

            if(data.CivOwningCell != null) {
                BuildCivilization(data.CivOwningCell);
            }

            if(data.ImprovementOnCell != null) {
                BuildImprovement(cell, data.ImprovementOnCell.Yield);
            }

            var resourceLogic = Container.Resolve<CellResourceLogic>();

            return resourceLogic.GetYieldOfCell(cell);
        }

        #endregion

        #region utilities

        private IHexCell BuildCell(TerrainType terrain, TerrainFeature feature, TerrainShape shape) {
            var mockCell = new Mock<IHexCell>();

            mockCell.SetupAllProperties();

            var newCell = mockCell.Object;

            newCell.Terrain = terrain;
            newCell.Feature = feature;
            newCell.Shape   = shape;

            return newCell;
        }

        private IResourceNode BuildResourceNode(
            IHexCell location, ISpecialtyResourceDefinition resource
        ) {
            var mockNode = new Mock<IResourceNode>();

            mockNode.Setup(node => node.Resource).Returns(resource);

            var newNode = mockNode.Object;

            MockNodePositionCanon.Setup(canon => canon.GetPossessionsOfOwner(location))
                .Returns(new List<IResourceNode>() { newNode });

            

            return newNode;
        }

        private ISpecialtyResourceDefinition BuildResourceDefinition(ResourceDefinitionData resourceData) {
            var mockDefinition = new Mock<ISpecialtyResourceDefinition>();

            mockDefinition.Setup(resource => resource.BonusYieldBase).Returns(resourceData.BonusYield);

            var newDefinition = mockDefinition.Object;

            MockTechCanon.Setup(canon => canon.IsResourceVisibleToCiv(newDefinition, It.IsAny<ICivilization>())).Returns(resourceData.IsVisible);

            return newDefinition;
        }

        private IImprovement BuildImprovement(IHexCell location, ResourceSummary bonusYield) {
            var mockImprovement = new Mock<IImprovement>();

            
            MockImprovementYieldLogic
                .Setup(logic => logic.GetYieldOfImprovement(mockImprovement.Object)).Returns(bonusYield);

            var newImprovement = mockImprovement.Object;

            MockImprovementLocationCanon.Setup(canon => canon.GetPossessionsOfOwner(location))
                .Returns(new List<IImprovement>() { newImprovement });
            
            return newImprovement;
        }

        private ICity BuildCity(IHexCell territory, CityData cityData,
            ISpecialtyResourceDefinition candidateResource
        ) {
            var newCity = new Mock<ICity>().Object;

            MockCellPossessionCanon.Setup(canon => canon.GetOwnerOfPossession(territory)).Returns(newCity);

            MockBuildingPossessionCanon
                .Setup(canon => canon.GetPossessionsOfOwner(newCity))
                .Returns(cityData.Buildings.Select(
                    buildingData => BuildBuilding(newCity, buildingData, candidateResource)
                ));


            return newCity;
        }

        private IBuilding BuildBuilding(
            ICity city, BuildingData buildingData,
            ISpecialtyResourceDefinition candidateResource
        ){
            var mockBuilding = new Mock<IBuilding>();

            var mockTemplate = new Mock<IBuildingTemplate>();

            if(candidateResource != null) {
                mockTemplate
                    .Setup(template => template.ResourceYieldModifications)
                    .Returns(buildingData.ResourceYieldModifications.Select(
                        tuple => BuildModificationData(tuple, candidateResource)
                    ));
            }

            mockTemplate
                .Setup(template => template.CellYieldModifications)
                .Returns(buildingData.CellYieldModifications.Cast<ICellYieldModificationData>());
            
            mockBuilding.Setup(building => building.Template).Returns(mockTemplate.Object);

            return mockBuilding.Object;
        }

        private ICivilization BuildCivilization(CivilizationData civData) {
            var newCivilization = new Mock<ICivilization>().Object;

            MockCityPossessionCanon.Setup(canon => canon.GetOwnerOfPossession(It.IsAny<ICity>())).Returns(newCivilization);

            return newCivilization;
        }

        private IResourceYieldModificationData BuildModificationData(
            Tuple<ResourceSummary, bool> dataTuple,
            ISpecialtyResourceDefinition candidateResource
        ) {
            var mockData = new Mock<IResourceYieldModificationData>();

            mockData.Setup(data => data.BonusYield).Returns(dataTuple.Item1);

            if(dataTuple.Item2) {
                mockData.Setup(data => data.Resource).Returns(candidateResource);
            }

            return mockData.Object;
        }

        #endregion

        #endregion

    }

}
