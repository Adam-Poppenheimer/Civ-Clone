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
using Assets.Simulation.HexMap;
using Assets.Simulation.Cities;
using Assets.Simulation.SpecialtyResources;

namespace Assets.Tests.Simulation.Improvements {

    [TestFixture]
    public class ImprovementValidityLogicTests : ZenjectUnitTestFixture {

        #region internal types

        public struct IsTemplateValidForCellData {

            public HexCellTestData Cell;

            public List<HexCellTestData> Neighbors;

            public bool CellHasCity;

            public ImprovementTemplateTestData ImprovementTemplate;

            public ResourceNodeTestData NodeOnCell;

        }

        public struct HexCellTestData {

            public TerrainType    Terrain;
            public TerrainFeature Feature;
            public TerrainShape   Shape;

            public int FoundationElevation;

            public bool HasAccessToFreshWater;

        }

        public struct ImprovementTemplateTestData {

            public List<TerrainType> RestrictedToTerrains;

            public List<TerrainFeature> RestrictedToFeatures;

            public List<TerrainShape> RestrictedToShapes;

            public bool RequiresResourceToExtract;

            public bool FreshWaterAlwaysEnables;

        }

        public class ResourceNodeTestData {

            public bool TemplateToTestIsExtractor;

        }

        #endregion

        #region static fields and properties

        #region test cases

        private static IEnumerable ValidityTestCases {
            get {
                yield return new TestCaseData(new IsTemplateValidForCellData() {
                    Cell = new HexCellTestData() {
                        Terrain = TerrainType.Grassland, Feature = TerrainFeature.None,
                        Shape = TerrainShape.Flatlands, FoundationElevation = 0
                    },
                    Neighbors = new List<HexCellTestData>(),
                    CellHasCity = false,
                    ImprovementTemplate = new ImprovementTemplateTestData() {
                        RestrictedToShapes   = new List<TerrainShape>  () { TerrainShape.Flatlands, TerrainShape.Hills },
                        RestrictedToTerrains = new List<TerrainType>   () { TerrainType.Grassland, TerrainType.Plains },
                        RestrictedToFeatures = new List<TerrainFeature>() { TerrainFeature.None, TerrainFeature.Forest }
                    }
                }).SetName("Everything is valid").Returns(true);

                yield return new TestCaseData(new IsTemplateValidForCellData() {
                    Cell = new HexCellTestData() {
                        Terrain = TerrainType.Desert, Feature = TerrainFeature.None,
                        Shape = TerrainShape.Flatlands, FoundationElevation = 0
                    },
                    Neighbors = new List<HexCellTestData>(),
                    CellHasCity = false,
                    ImprovementTemplate = new ImprovementTemplateTestData() {
                        RestrictedToShapes   = new List<TerrainShape>  () { TerrainShape.Flatlands, TerrainShape.Hills },
                        RestrictedToTerrains = new List<TerrainType>() { TerrainType.Grassland, TerrainType.Plains },
                        RestrictedToFeatures = new List<TerrainFeature>() { TerrainFeature.None, TerrainFeature.Forest }
                    }
                }).SetName("Invalid terrain").Returns(false);

                yield return new TestCaseData(new IsTemplateValidForCellData() {
                    Cell = new HexCellTestData() {
                        Terrain = TerrainType.Grassland, Feature = TerrainFeature.None,
                        Shape = TerrainShape.Flatlands, FoundationElevation = 0
                    },
                    Neighbors = new List<HexCellTestData>(),
                    CellHasCity = false,
                    ImprovementTemplate = new ImprovementTemplateTestData() {
                        RestrictedToShapes   = new List<TerrainShape>  () { TerrainShape.Flatlands, TerrainShape.Hills },
                        RestrictedToTerrains = new List<TerrainType>() { TerrainType.Grassland, TerrainType.Plains },
                        RestrictedToFeatures = new List<TerrainFeature>() { TerrainFeature.Forest }
                    }
                }).SetName("Invalid feature").Returns(false);

                yield return new TestCaseData(new IsTemplateValidForCellData() {
                    Cell = new HexCellTestData() {
                        Terrain = TerrainType.Desert, Feature = TerrainFeature.None,
                        Shape = TerrainShape.Mountains, FoundationElevation = 0
                    },
                    Neighbors = new List<HexCellTestData>() {
                        new HexCellTestData() { FoundationElevation = 0 }
                    },
                    CellHasCity = false,
                    ImprovementTemplate = new ImprovementTemplateTestData() {
                        RestrictedToShapes   = new List<TerrainShape>  () { TerrainShape.Flatlands, TerrainShape.Hills },
                        RestrictedToTerrains = new List<TerrainType>   () { TerrainType.Grassland, TerrainType.Plains },
                        RestrictedToFeatures = new List<TerrainFeature>() { TerrainFeature.None, TerrainFeature.Forest }
                    }
                }).SetName("Invalid Shape").Returns(false);

                yield return new TestCaseData(new IsTemplateValidForCellData() {
                    Cell = new HexCellTestData() {
                        Terrain = TerrainType.Desert, Feature = TerrainFeature.None,
                        Shape = TerrainShape.Flatlands, FoundationElevation = 0
                    },
                    Neighbors = new List<HexCellTestData>() {
                        new HexCellTestData() { FoundationElevation = 0 }
                    },
                    CellHasCity = false,
                    ImprovementTemplate = new ImprovementTemplateTestData() {
                        RestrictedToShapes   = new List<TerrainShape>  () { TerrainShape.Flatlands, TerrainShape.Hills },
                        RestrictedToTerrains = new List<TerrainType>   () {  },
                        RestrictedToFeatures = new List<TerrainFeature>() { TerrainFeature.None, TerrainFeature.Forest }
                    }
                }).SetName("Template specifies no terrain types").Returns(true);

                yield return new TestCaseData(new IsTemplateValidForCellData() {
                    Cell = new HexCellTestData() {
                        Terrain = TerrainType.Grassland, Feature = TerrainFeature.None,
                        Shape = TerrainShape.Flatlands, FoundationElevation = 0
                    },
                    Neighbors = new List<HexCellTestData>() {
                        new HexCellTestData() { FoundationElevation = 0 }
                    },
                    CellHasCity = false,
                    ImprovementTemplate = new ImprovementTemplateTestData() {
                        RestrictedToShapes   = new List<TerrainShape>  () {  },
                        RestrictedToTerrains = new List<TerrainType>   () { TerrainType.Grassland, TerrainType.Plains },
                        RestrictedToFeatures = new List<TerrainFeature>() { TerrainFeature.None, TerrainFeature.Forest }
                    }
                }).SetName("Template specifies no terrain shapes").Returns(true);

                yield return new TestCaseData(new IsTemplateValidForCellData() {
                    Cell = new HexCellTestData() {
                        Terrain = TerrainType.Grassland, Feature = TerrainFeature.None,
                        Shape = TerrainShape.Flatlands, FoundationElevation = 0
                    },
                    Neighbors = new List<HexCellTestData>() {
                        new HexCellTestData() { FoundationElevation = 0 }
                    },
                    CellHasCity = false,
                    ImprovementTemplate = new ImprovementTemplateTestData() {
                        RestrictedToShapes   = new List<TerrainShape>  () { TerrainShape.Flatlands, TerrainShape.Hills },
                        RestrictedToTerrains = new List<TerrainType>   () { TerrainType.Grassland, TerrainType.Plains },
                        RestrictedToFeatures = new List<TerrainFeature>() {  }
                    }
                }).SetName("Template specifies no terrain features").Returns(true);

                yield return new TestCaseData(new IsTemplateValidForCellData() {
                    Cell = new HexCellTestData() {
                        Terrain = TerrainType.Grassland, Feature = TerrainFeature.None, FoundationElevation = 0
                    },
                    Neighbors = new List<HexCellTestData>(),
                    CellHasCity = true,
                    ImprovementTemplate = new ImprovementTemplateTestData() {
                        RestrictedToShapes   = new List<TerrainShape>  () { TerrainShape.Flatlands, TerrainShape.Hills },
                        RestrictedToTerrains = new List<TerrainType>   () { TerrainType.Grassland, TerrainType.Plains },
                        RestrictedToFeatures = new List<TerrainFeature>() { TerrainFeature.None, TerrainFeature.Forest }
                    }
                }).SetName("City on otherwise valid cell").Returns(false);

                yield return new TestCaseData(new IsTemplateValidForCellData() {
                    Cell = new HexCellTestData() {
                        Terrain = TerrainType.Desert, Feature = TerrainFeature.Forest, FoundationElevation = 0
                    },
                    Neighbors = new List<HexCellTestData>(),
                    CellHasCity = false,
                    ImprovementTemplate = new ImprovementTemplateTestData() {
                        RestrictedToShapes   = new List<TerrainShape>  () { TerrainShape.Flatlands, TerrainShape.Hills },
                        RestrictedToTerrains = new List<TerrainType>   () { TerrainType.Grassland, TerrainType.Plains },
                        RestrictedToFeatures = new List<TerrainFeature>() { TerrainFeature.None }
                    },
                    NodeOnCell = new ResourceNodeTestData() {
                        TemplateToTestIsExtractor = true
                    }
                }).SetName("An otherwise invalid cell, but node with valid extractor").Returns(true);

                yield return new TestCaseData(new IsTemplateValidForCellData() {
                    Cell = new HexCellTestData() {
                        Terrain = TerrainType.Desert, Feature = TerrainFeature.Forest, FoundationElevation = 0
                    },
                    Neighbors = new List<HexCellTestData>(),
                    CellHasCity = false,
                    ImprovementTemplate = new ImprovementTemplateTestData() {
                        RestrictedToShapes   = new List<TerrainShape>  () { TerrainShape.Flatlands, TerrainShape.Hills },
                        RestrictedToTerrains = new List<TerrainType>   () { TerrainType.Grassland, TerrainType.Plains },
                        RestrictedToFeatures = new List<TerrainFeature>() { TerrainFeature.None }
                    },
                    NodeOnCell = new ResourceNodeTestData() {
                        TemplateToTestIsExtractor = false
                    }
                }).SetName("An otherwise invalid cell, but node with invalid extractor").Returns(false);

                yield return new TestCaseData(new IsTemplateValidForCellData() {
                    Cell = new HexCellTestData() {
                        Terrain = TerrainType.Grassland, Feature = TerrainFeature.None, FoundationElevation = 0
                    },
                    Neighbors = new List<HexCellTestData>(),
                    CellHasCity = false,
                    ImprovementTemplate = new ImprovementTemplateTestData() {
                        RestrictedToTerrains = new List<TerrainType>(),
                        RestrictedToShapes   = new List<TerrainShape>(),
                        RestrictedToFeatures = new List<TerrainFeature>(),
                        RequiresResourceToExtract = true
                    },

                }).SetName("No terrain, shape, or feature restrictions, no resource, and RequiresResourceToExtract is true").Returns(false);




                yield return new TestCaseData(new IsTemplateValidForCellData() {
                    Cell = new HexCellTestData() {
                        Terrain = TerrainType.Desert, Feature = TerrainFeature.None,
                        Shape = TerrainShape.Flatlands, FoundationElevation = 0,
                        HasAccessToFreshWater = true
                    },
                    Neighbors = new List<HexCellTestData>(),
                    CellHasCity = false,
                    ImprovementTemplate = new ImprovementTemplateTestData() {
                        RestrictedToShapes   = new List<TerrainShape>  () { TerrainShape.Flatlands, TerrainShape.Hills },
                        RestrictedToTerrains = new List<TerrainType>() { TerrainType.Grassland, TerrainType.Plains },
                        RestrictedToFeatures = new List<TerrainFeature>() { TerrainFeature.None, TerrainFeature.Forest },
                        FreshWaterAlwaysEnables = true
                    }
                }).SetName("Invalid terrain, but has access to fresh water and FreshWaterAlwaysEnables is true").Returns(true);

                yield return new TestCaseData(new IsTemplateValidForCellData() {
                    Cell = new HexCellTestData() {
                        Terrain = TerrainType.Grassland, Feature = TerrainFeature.None,
                        Shape = TerrainShape.Flatlands, FoundationElevation = 0,
                        HasAccessToFreshWater = true
                    },
                    Neighbors = new List<HexCellTestData>(),
                    CellHasCity = false,
                    ImprovementTemplate = new ImprovementTemplateTestData() {
                        RestrictedToShapes   = new List<TerrainShape>() { TerrainShape.Flatlands, TerrainShape.Hills },
                        RestrictedToTerrains = new List<TerrainType>() { TerrainType.Grassland, TerrainType.Plains },
                        RestrictedToFeatures = new List<TerrainFeature>() { TerrainFeature.Forest },
                        FreshWaterAlwaysEnables = true
                    }
                }).SetName("Invalid feature, but has access to fresh water and FreshWaterAlwaysEnables is true").Returns(true);

                yield return new TestCaseData(new IsTemplateValidForCellData() {
                    Cell = new HexCellTestData() {
                        Terrain = TerrainType.Desert, Feature = TerrainFeature.None,
                        Shape = TerrainShape.Mountains, FoundationElevation = 0,
                        HasAccessToFreshWater = true
                    },
                    Neighbors = new List<HexCellTestData>() {
                        new HexCellTestData() { FoundationElevation = 0 }
                    },
                    CellHasCity = false,
                    ImprovementTemplate = new ImprovementTemplateTestData() {
                        RestrictedToShapes   = new List<TerrainShape>  () { TerrainShape.Flatlands, TerrainShape.Hills },
                        RestrictedToTerrains = new List<TerrainType>   () { TerrainType.Grassland, TerrainType.Plains },
                        RestrictedToFeatures = new List<TerrainFeature>() { TerrainFeature.None, TerrainFeature.Forest },
                        FreshWaterAlwaysEnables = true
                    }
                }).SetName("Invalid Shape, but has access to fresh water and FreshWaterAlwaysEnables is true").Returns(true);
            }
        }

        #endregion

        #endregion

        #region instance fields and properties

        private Mock<ICityFactory>                                     MockCityFactory;
        private Mock<IHexGrid>                                         MockGrid;
        private Mock<IPossessionRelationship<IHexCell, IResourceNode>> MockNodePositionCanon;
        private Mock<IPossessionRelationship<IHexCell, ICity>>         MockCityLocationCanon;
        private Mock<IFreshWaterCanon>                                 MockFreshWaterCanon;

        private List<ICity> AllCities = new List<ICity>();

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            AllCities.Clear();

            MockCityFactory       = new Mock<ICityFactory>();
            MockGrid              = new Mock<IHexGrid>();
            MockNodePositionCanon = new Mock<IPossessionRelationship<IHexCell, IResourceNode>>();
            MockCityLocationCanon = new Mock<IPossessionRelationship<IHexCell, ICity>>();
            MockFreshWaterCanon   = new Mock<IFreshWaterCanon>();

            MockCityFactory.Setup(factory => factory.AllCities).Returns(() => AllCities.AsReadOnly());

            Container.Bind<ICityFactory>                                    ().FromInstance(MockCityFactory      .Object);
            Container.Bind<IHexGrid>                                        ().FromInstance(MockGrid             .Object);
            Container.Bind<IPossessionRelationship<IHexCell, IResourceNode>>().FromInstance(MockNodePositionCanon.Object);
            Container.Bind<IPossessionRelationship<IHexCell, ICity>>        ().FromInstance(MockCityLocationCanon.Object);
            Container.Bind<IFreshWaterCanon>                                ().FromInstance(MockFreshWaterCanon  .Object);

            Container.Bind<ImprovementValidityLogic>().AsSingle();
        }

        #endregion

        #region tests

        [Test(Description = "IsTemplateValidForCell should consider the terrain, features, and " +
            "edge types of the cell in question. It should always forbid placement on cities but allow " +
            "templates on ResourceNodes regardless of other properties if the node requires that " +
            "template as an extractor")]
        [TestCaseSource("ValidityTestCases")]
        public bool IsTemplateValidForCell_ConsidersTerrainAndFeatures(IsTemplateValidForCellData data){
            var cellToTest = BuildCell(data.Cell);

            if(data.CellHasCity) {
                BuildCity(cellToTest);
            }

            MockGrid.Setup(grid => grid.GetNeighbors(cellToTest))
                .Returns(data.Neighbors.Select(neighbor => BuildCell(neighbor)).ToList());

            var templateToTest = BuildTemplate(data.ImprovementTemplate);

            if(data.NodeOnCell != null) {
                BuildResourceNode(cellToTest, data.NodeOnCell.TemplateToTestIsExtractor ? templateToTest : null);
            }

            var validityLogic = Container.Resolve<ImprovementValidityLogic>();

            return validityLogic.IsTemplateValidForCell(templateToTest, cellToTest);
        }

        #endregion

        #region utilities

        private IHexCell BuildCell(HexCellTestData data) {
            var mockCell = new Mock<IHexCell>();

            mockCell.SetupAllProperties();
            mockCell.Setup(cell => cell.FoundationElevation).Returns(data.FoundationElevation);

            var newCell = mockCell.Object;

            newCell.Terrain = data.Terrain;
            newCell.Feature = data.Feature;
            newCell.Shape   = data.Shape;

            MockFreshWaterCanon.Setup(canon => canon.HasAccessToFreshWater(newCell)).Returns(data.HasAccessToFreshWater);

            return newCell;
        }

        private ICity BuildCity(IHexCell location) {
            var cityMock = new Mock<ICity>();

            var newCity = cityMock.Object;

            MockCityLocationCanon.Setup(canon => canon.GetOwnerOfPossession(newCity)).Returns(location);
            MockCityLocationCanon.Setup(canon => canon.GetPossessionsOfOwner(location)).Returns(new List<ICity>() { newCity });

            AllCities.Add(newCity);

            return newCity;
        }

        private IImprovementTemplate BuildTemplate(ImprovementTemplateTestData data){
            var mockTemplate = new Mock<IImprovementTemplate>();

            mockTemplate.Setup(template => template.RestrictedToTerrains)     .Returns(data.RestrictedToTerrains);
            mockTemplate.Setup(template => template.RestrictedToFeatures)     .Returns(data.RestrictedToFeatures);
            mockTemplate.Setup(template => template.RestrictedToShapes )      .Returns(data.RestrictedToShapes);
            mockTemplate.Setup(template => template.RequiresResourceToExtract).Returns(data.RequiresResourceToExtract);
            mockTemplate.Setup(template => template.FreshWaterAlwaysEnables)  .Returns(data.FreshWaterAlwaysEnables);

            return mockTemplate.Object;
        }

        private IResourceNode BuildResourceNode(IHexCell location, IImprovementTemplate extractor) {
            var mockNode = new Mock<IResourceNode>();

            var mockDefinition = new Mock<ISpecialtyResourceDefinition>();
            mockDefinition.Setup(definition => definition.Extractor).Returns(extractor);

            mockNode.Setup(node => node.Resource).Returns(mockDefinition.Object);

            var newNode = mockNode.Object;

            MockNodePositionCanon.Setup(canon => canon.GetPossessionsOfOwner(location))
                .Returns(new List<IResourceNode>() { newNode });

            return newNode;
        }

        #endregion

        #endregion

    }

}
