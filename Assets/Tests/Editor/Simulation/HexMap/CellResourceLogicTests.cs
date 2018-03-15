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

        }

        public class ResourceNodeData {

            public ResourceSummary BonusYield;

        }

        public class ImprovementData {

            public ResourceSummary Yield;

        }

        public class CityData {

            public List<BuildingData> Buildings;

        }

        public class BuildingData {

            public List<Tuple<ResourceSummary, bool>> ResourceYieldModifications = new List<Tuple<ResourceSummary, bool>>();

            public List<CellYieldModificationData> CellYieldModifications = new List<CellYieldModificationData>();

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
                        BonusYield = new ResourceSummary(0f, 2f)
                    }
                }).SetName("Grassland/Flat/no feature/Has a resource").Returns(new ResourceSummary(1f, 2f));

                yield return new TestCaseData(new GetYieldOfCellTestData() {
                    Terrain = TerrainType.Grassland,
                    Feature = TerrainFeature.Forest,
                    Shape   = TerrainShape.Flatlands,
                    ResourceNodeOnCell = new ResourceNodeData() {
                        BonusYield = new ResourceSummary(0f, 2f)
                    }
                }).SetName("Grassland/Flat/Forest/Has a resource").Returns(new ResourceSummary(2f, 2f));




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
                        BonusYield = new ResourceSummary(0f, 2f)
                    },
                    ImprovementOnCell = new ImprovementData() {
                        Yield = new ResourceSummary(0f, 0f, 3f)
                    }
                }).SetName("Grassland/Flat/no feature/Has a resource and an improvement").Returns(new ResourceSummary(1f, 2f, 3f));

                yield return new TestCaseData(new GetYieldOfCellTestData() {
                    Terrain = TerrainType.Grassland,
                    Feature = TerrainFeature.Forest,
                    Shape   = TerrainShape.Flatlands,
                    ResourceNodeOnCell = new ResourceNodeData() {
                        BonusYield = new ResourceSummary(0f, 2f)
                    },
                    ImprovementOnCell = new ImprovementData() {
                        Yield = new ResourceSummary(0f, 0f, 3f)
                    }
                }).SetName("Grassland/Flat/Forest/Has a resource and an improvement").Returns(new ResourceSummary(2f, 2f, 3f));


                yield return new TestCaseData(new GetYieldOfCellTestData() {
                    Terrain = TerrainType.Grassland,
                    Feature = TerrainFeature.None,
                    Shape   = TerrainShape.Flatlands,
                    ResourceNodeOnCell = new ResourceNodeData() {
                        BonusYield = new ResourceSummary(food: 2)
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
                    }
                }).SetName("Grassland/Flat/No feature/Has resource and modifying buildings").Returns(new ResourceSummary(food: 3, production: 2, science: 2));


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

        private static List<ResourceSummary> TerrainYields = new List<ResourceSummary>() {
            new ResourceSummary(1f),
            new ResourceSummary(1.1f),
            new ResourceSummary(1.2f),
            new ResourceSummary(1.3f),
            new ResourceSummary(1.4f),
        };

        private static List<ResourceSummary> FeatureYields = new List<ResourceSummary>() {
            new ResourceSummary(0f),
            new ResourceSummary(2f)
        };

        private static List<ResourceSummary> ShapeYields = new List<ResourceSummary>() {
            new ResourceSummary(0f),
            new ResourceSummary(10f),
            new ResourceSummary(20f)
        };

        #endregion

        #region instance fields and properties

        private Mock<IHexGridConfig> MockConfig;

        private Mock<IPossessionRelationship<IHexCell, IResourceNode>> MockNodePositionCanon;

        private Mock<IImprovementLocationCanon> MockImprovementLocationCanon;

        private Mock<IImprovementYieldLogic> MockImprovementYieldLogic;

        private Mock<IPossessionRelationship<ICity, IHexCell>> MockCellPossessionCanon;

        private Mock<IPossessionRelationship<ICity, IBuilding>> MockBuildingPossessionCanon;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockConfig                   = new Mock<IHexGridConfig>();
            MockNodePositionCanon        = new Mock<IPossessionRelationship<IHexCell, IResourceNode>>();
            MockImprovementLocationCanon = new Mock<IImprovementLocationCanon>();
            MockImprovementYieldLogic    = new Mock<IImprovementYieldLogic>();
            MockCellPossessionCanon      = new Mock<IPossessionRelationship<ICity, IHexCell>>();
            MockBuildingPossessionCanon  = new Mock<IPossessionRelationship<ICity, IBuilding>>();

            MockConfig.Setup(config => config.TerrainYields).Returns(TerrainYields.AsReadOnly());
            MockConfig.Setup(config => config.FeatureYields).Returns(FeatureYields.AsReadOnly());
            MockConfig.Setup(config => config.ShapeYields  ).Returns(ShapeYields  .AsReadOnly());

            Container.Bind<IHexGridConfig>                                  ().FromInstance(MockConfig                  .Object);
            Container.Bind<IPossessionRelationship<IHexCell, IResourceNode>>().FromInstance(MockNodePositionCanon       .Object);
            Container.Bind<IImprovementLocationCanon>                       ().FromInstance(MockImprovementLocationCanon.Object);
            Container.Bind<IImprovementYieldLogic>                          ().FromInstance(MockImprovementYieldLogic   .Object);
            Container.Bind<IPossessionRelationship<ICity, IHexCell>>        ().FromInstance(MockCellPossessionCanon     .Object);
            Container.Bind<IPossessionRelationship<ICity, IBuilding>>       ().FromInstance(MockBuildingPossessionCanon .Object);

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
                node = BuildResourceNode(cell, data.ResourceNodeOnCell.BonusYield);
            }

            if(data.CityOwningCell != null) {
                BuildCity(cell, data.CityOwningCell, node != null ? node.Resource : null);
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

        private IResourceNode BuildResourceNode(IHexCell location, ResourceSummary bonusYield) {
            var mockNode = new Mock<IResourceNode>();

            var mockDefinition = new Mock<ISpecialtyResourceDefinition>();

            mockDefinition.Setup(resource => resource.BonusYieldBase).Returns(bonusYield);

            mockNode.Setup(node => node.Resource).Returns(mockDefinition.Object);

            var newNode = mockNode.Object;

            MockNodePositionCanon.Setup(canon => canon.GetPossessionsOfOwner(location))
                .Returns(new List<IResourceNode>() { newNode });

            return newNode;
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
