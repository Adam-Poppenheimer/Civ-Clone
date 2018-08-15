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
using Assets.Simulation.MapResources;

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

            public bool IgnoreOwnership;

        }

        public struct HexCellTestData {

            public CellTerrain    Terrain;
            public CellVegetation Vegetation;
            public CellShape      Shape;
            public CellFeature    Feature;

            public bool HasAccessToFreshWater;

        }

        public struct ImprovementTemplateTestData {

            public List<CellTerrain> RestrictedToTerrains;

            public List<CellVegetation> RestrictedToVegetations;

            public List<CellShape> RestrictedToShapes;

            public bool RequiresResourceToExtract;

            public bool FreshWaterAlwaysEnables;

        }

        public class ResourceNodeTestData {

            public bool TemplateToTestIsExtractor;

        }

        #endregion

        #region static fields and properties

        #region test cases

        private static IEnumerable IsTemplateValidForCellTestCases {
            get {
                yield return new TestCaseData(new IsTemplateValidForCellData() {
                    Cell = new HexCellTestData() {
                        Terrain = CellTerrain.Grassland, Vegetation = CellVegetation.None,
                        Shape = CellShape.Flatlands
                    },
                    Neighbors = new List<HexCellTestData>(),
                    CellHasCity = false,
                    ImprovementTemplate = new ImprovementTemplateTestData() {
                        RestrictedToShapes      = new List<CellShape>     () { CellShape.Flatlands, CellShape.Hills },
                        RestrictedToTerrains    = new List<CellTerrain>   () { CellTerrain.Grassland, CellTerrain.Plains },
                        RestrictedToVegetations = new List<CellVegetation>() { CellVegetation.None, CellVegetation.Forest }
                    }
                }).SetName("Everything is valid").Returns(true);

                yield return new TestCaseData(new IsTemplateValidForCellData() {
                    Cell = new HexCellTestData() {
                        Terrain = CellTerrain.Desert, Vegetation = CellVegetation.None,
                        Shape = CellShape.Flatlands
                    },
                    Neighbors = new List<HexCellTestData>(),
                    CellHasCity = false,
                    ImprovementTemplate = new ImprovementTemplateTestData() {
                        RestrictedToShapes   = new List<CellShape>  () { CellShape.Flatlands, CellShape.Hills },
                        RestrictedToTerrains = new List<CellTerrain>() { CellTerrain.Grassland, CellTerrain.Plains },
                        RestrictedToVegetations = new List<CellVegetation>() { CellVegetation.None, CellVegetation.Forest }
                    }
                }).SetName("Invalid terrain").Returns(false);

                yield return new TestCaseData(new IsTemplateValidForCellData() {
                    Cell = new HexCellTestData() {
                        Terrain = CellTerrain.Grassland, Vegetation = CellVegetation.None,
                        Shape = CellShape.Flatlands
                    },
                    Neighbors = new List<HexCellTestData>(),
                    CellHasCity = false,
                    ImprovementTemplate = new ImprovementTemplateTestData() {
                        RestrictedToShapes   = new List<CellShape>  () { CellShape.Flatlands, CellShape.Hills },
                        RestrictedToTerrains = new List<CellTerrain>() { CellTerrain.Grassland, CellTerrain.Plains },
                        RestrictedToVegetations = new List<CellVegetation>() { CellVegetation.Forest }
                    }
                }).SetName("Invalid feature").Returns(false);

                yield return new TestCaseData(new IsTemplateValidForCellData() {
                    Cell = new HexCellTestData() {
                        Terrain = CellTerrain.Desert, Vegetation = CellVegetation.None,
                        Shape = CellShape.Mountains
                    },
                    Neighbors = new List<HexCellTestData>() {
                        new HexCellTestData()
                    },
                    CellHasCity = false,
                    ImprovementTemplate = new ImprovementTemplateTestData() {
                        RestrictedToShapes   = new List<CellShape>  () { CellShape.Flatlands, CellShape.Hills },
                        RestrictedToTerrains = new List<CellTerrain>   () { CellTerrain.Grassland, CellTerrain.Plains },
                        RestrictedToVegetations = new List<CellVegetation>() { CellVegetation.None, CellVegetation.Forest }
                    }
                }).SetName("Invalid Shape").Returns(false);

                yield return new TestCaseData(new IsTemplateValidForCellData() {
                    Cell = new HexCellTestData() {
                        Terrain = CellTerrain.Desert, Vegetation = CellVegetation.None,
                        Shape = CellShape.Flatlands
                    },
                    Neighbors = new List<HexCellTestData>() {
                        new HexCellTestData()
                    },
                    CellHasCity = false,
                    ImprovementTemplate = new ImprovementTemplateTestData() {
                        RestrictedToShapes   = new List<CellShape>  () { CellShape.Flatlands, CellShape.Hills },
                        RestrictedToTerrains = new List<CellTerrain>   () {  },
                        RestrictedToVegetations = new List<CellVegetation>() { CellVegetation.None, CellVegetation.Forest }
                    }
                }).SetName("Template specifies no terrain types").Returns(true);

                yield return new TestCaseData(new IsTemplateValidForCellData() {
                    Cell = new HexCellTestData() {
                        Terrain = CellTerrain.Grassland, Vegetation = CellVegetation.None,
                        Shape = CellShape.Flatlands
                    },
                    Neighbors = new List<HexCellTestData>() {
                        new HexCellTestData()
                    },
                    CellHasCity = false,
                    ImprovementTemplate = new ImprovementTemplateTestData() {
                        RestrictedToShapes   = new List<CellShape>  () {  },
                        RestrictedToTerrains = new List<CellTerrain>   () { CellTerrain.Grassland, CellTerrain.Plains },
                        RestrictedToVegetations = new List<CellVegetation>() { CellVegetation.None, CellVegetation.Forest }
                    }
                }).SetName("Template specifies no terrain shapes").Returns(true);

                yield return new TestCaseData(new IsTemplateValidForCellData() {
                    Cell = new HexCellTestData() {
                        Terrain = CellTerrain.Grassland, Vegetation = CellVegetation.None,
                        Shape = CellShape.Flatlands
                    },
                    Neighbors = new List<HexCellTestData>() {
                        new HexCellTestData()
                    },
                    CellHasCity = false,
                    ImprovementTemplate = new ImprovementTemplateTestData() {
                        RestrictedToShapes   = new List<CellShape>  () { CellShape.Flatlands, CellShape.Hills },
                        RestrictedToTerrains = new List<CellTerrain>   () { CellTerrain.Grassland, CellTerrain.Plains },
                        RestrictedToVegetations = new List<CellVegetation>() {  }
                    }
                }).SetName("Template specifies no terrain features").Returns(true);

                yield return new TestCaseData(new IsTemplateValidForCellData() {
                    Cell = new HexCellTestData() {
                        Terrain = CellTerrain.Grassland, Vegetation = CellVegetation.None
                    },
                    Neighbors = new List<HexCellTestData>(),
                    CellHasCity = true,
                    ImprovementTemplate = new ImprovementTemplateTestData() {
                        RestrictedToShapes   = new List<CellShape>  () { CellShape.Flatlands, CellShape.Hills },
                        RestrictedToTerrains = new List<CellTerrain>   () { CellTerrain.Grassland, CellTerrain.Plains },
                        RestrictedToVegetations = new List<CellVegetation>() { CellVegetation.None, CellVegetation.Forest }
                    }
                }).SetName("City on otherwise valid cell").Returns(false);

                yield return new TestCaseData(new IsTemplateValidForCellData() {
                    Cell = new HexCellTestData() {
                        Terrain = CellTerrain.Grassland, Vegetation = CellVegetation.None
                    },
                    Neighbors = new List<HexCellTestData>(),
                    CellHasCity = true,
                    ImprovementTemplate = new ImprovementTemplateTestData() {
                        RestrictedToShapes      = new List<CellShape>     () { CellShape.Flatlands,   CellShape.Hills },
                        RestrictedToTerrains    = new List<CellTerrain>   () { CellTerrain.Grassland, CellTerrain.Plains },
                        RestrictedToVegetations = new List<CellVegetation>() { CellVegetation.None,   CellVegetation.Forest }
                    },
                    IgnoreOwnership = true
                }).SetName("City on otherwise valid cell, and ownership ignored").Returns(true);

                yield return new TestCaseData(new IsTemplateValidForCellData() {
                    Cell = new HexCellTestData() {
                        Terrain = CellTerrain.Desert, Vegetation = CellVegetation.Forest
                    },
                    Neighbors = new List<HexCellTestData>(),
                    CellHasCity = false,
                    ImprovementTemplate = new ImprovementTemplateTestData() {
                        RestrictedToShapes   = new List<CellShape>  () { CellShape.Flatlands, CellShape.Hills },
                        RestrictedToTerrains = new List<CellTerrain>   () { CellTerrain.Grassland, CellTerrain.Plains },
                        RestrictedToVegetations = new List<CellVegetation>() { CellVegetation.None }
                    },
                    NodeOnCell = new ResourceNodeTestData() {
                        TemplateToTestIsExtractor = true
                    }
                }).SetName("An otherwise invalid cell, but node with valid extractor").Returns(true);

                yield return new TestCaseData(new IsTemplateValidForCellData() {
                    Cell = new HexCellTestData() {
                        Terrain = CellTerrain.Desert, Vegetation = CellVegetation.Forest
                    },
                    Neighbors = new List<HexCellTestData>(),
                    CellHasCity = false,
                    ImprovementTemplate = new ImprovementTemplateTestData() {
                        RestrictedToShapes   = new List<CellShape>  () { CellShape.Flatlands, CellShape.Hills },
                        RestrictedToTerrains = new List<CellTerrain>   () { CellTerrain.Grassland, CellTerrain.Plains },
                        RestrictedToVegetations = new List<CellVegetation>() { CellVegetation.None }
                    },
                    NodeOnCell = new ResourceNodeTestData() {
                        TemplateToTestIsExtractor = false
                    }
                }).SetName("An otherwise invalid cell, but node with invalid extractor").Returns(false);

                yield return new TestCaseData(new IsTemplateValidForCellData() {
                    Cell = new HexCellTestData() {
                        Terrain = CellTerrain.Grassland, Vegetation = CellVegetation.None
                    },
                    Neighbors = new List<HexCellTestData>(),
                    CellHasCity = false,
                    ImprovementTemplate = new ImprovementTemplateTestData() {
                        RestrictedToTerrains = new List<CellTerrain>(),
                        RestrictedToShapes   = new List<CellShape>(),
                        RestrictedToVegetations = new List<CellVegetation>(),
                        RequiresResourceToExtract = true
                    },

                }).SetName("No terrain, shape, or feature restrictions, no resource, and RequiresResourceToExtract is true").Returns(false);




                yield return new TestCaseData(new IsTemplateValidForCellData() {
                    Cell = new HexCellTestData() {
                        Terrain = CellTerrain.Desert, Vegetation = CellVegetation.None,
                        Shape = CellShape.Flatlands, HasAccessToFreshWater = true
                    },
                    Neighbors = new List<HexCellTestData>(),
                    CellHasCity = false,
                    ImprovementTemplate = new ImprovementTemplateTestData() {
                        RestrictedToShapes   = new List<CellShape>  () { CellShape.Flatlands, CellShape.Hills },
                        RestrictedToTerrains = new List<CellTerrain>() { CellTerrain.Grassland, CellTerrain.Plains },
                        RestrictedToVegetations = new List<CellVegetation>() { CellVegetation.None, CellVegetation.Forest },
                        FreshWaterAlwaysEnables = true
                    }
                }).SetName("Invalid terrain, but has access to fresh water and FreshWaterAlwaysEnables is true").Returns(true);

                yield return new TestCaseData(new IsTemplateValidForCellData() {
                    Cell = new HexCellTestData() {
                        Terrain = CellTerrain.Grassland, Vegetation = CellVegetation.None,
                        Shape = CellShape.Flatlands, HasAccessToFreshWater = true
                    },
                    Neighbors = new List<HexCellTestData>(),
                    CellHasCity = false,
                    ImprovementTemplate = new ImprovementTemplateTestData() {
                        RestrictedToShapes   = new List<CellShape>() { CellShape.Flatlands, CellShape.Hills },
                        RestrictedToTerrains = new List<CellTerrain>() { CellTerrain.Grassland, CellTerrain.Plains },
                        RestrictedToVegetations = new List<CellVegetation>() { CellVegetation.Forest },
                        FreshWaterAlwaysEnables = true
                    }
                }).SetName("Invalid feature, but has access to fresh water and FreshWaterAlwaysEnables is true").Returns(true);

                yield return new TestCaseData(new IsTemplateValidForCellData() {
                    Cell = new HexCellTestData() {
                        Terrain = CellTerrain.Desert, Vegetation = CellVegetation.None,
                        Shape = CellShape.Mountains, HasAccessToFreshWater = true
                    },
                    Neighbors = new List<HexCellTestData>() {
                        new HexCellTestData()
                    },
                    CellHasCity = false,
                    ImprovementTemplate = new ImprovementTemplateTestData() {
                        RestrictedToShapes      = new List<CellShape>     () { CellShape.Flatlands, CellShape.Hills },
                        RestrictedToTerrains    = new List<CellTerrain>   () { CellTerrain.Grassland, CellTerrain.Plains },
                        RestrictedToVegetations = new List<CellVegetation>() { CellVegetation.None, CellVegetation.Forest },
                        FreshWaterAlwaysEnables = true
                    }
                }).SetName("Invalid Shape, but has access to fresh water and FreshWaterAlwaysEnables is true").Returns(true);



                yield return new TestCaseData(new IsTemplateValidForCellData() {
                    Cell = new HexCellTestData() {
                        Terrain = CellTerrain.Grassland, Vegetation = CellVegetation.None,
                        Shape = CellShape.Flatlands, Feature = CellFeature.Oasis,
                        HasAccessToFreshWater = true
                    },
                    Neighbors = new List<HexCellTestData>(),
                    CellHasCity = false,
                    ImprovementTemplate = new ImprovementTemplateTestData() {
                        RestrictedToShapes      = new List<CellShape>     () { CellShape.Flatlands },
                        RestrictedToTerrains    = new List<CellTerrain>   () { CellTerrain.Grassland },
                        RestrictedToVegetations = new List<CellVegetation>() { CellVegetation.None },
                    }
                }).SetName("Otherwise valid cell, but has an oasis").Returns(false);
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

        [Test()]
        [TestCaseSource("IsTemplateValidForCellTestCases")]
        public bool IsTemplateValidForCellTests(IsTemplateValidForCellData data){
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

            return validityLogic.IsTemplateValidForCell(templateToTest, cellToTest, data.IgnoreOwnership);
        }

        #endregion

        #region utilities

        private IHexCell BuildCell(HexCellTestData data) {
            var mockCell = new Mock<IHexCell>();

            mockCell.SetupAllProperties();

            var newCell = mockCell.Object;

            newCell.Terrain    = data.Terrain;
            newCell.Vegetation = data.Vegetation;
            newCell.Shape      = data.Shape;
            newCell.Feature    = data.Feature;

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
            mockTemplate.Setup(template => template.RestrictedToVegetations)     .Returns(data.RestrictedToVegetations);
            mockTemplate.Setup(template => template.RestrictedToShapes )      .Returns(data.RestrictedToShapes);
            mockTemplate.Setup(template => template.RequiresResourceToExtract).Returns(data.RequiresResourceToExtract);
            mockTemplate.Setup(template => template.FreshWaterAlwaysEnables)  .Returns(data.FreshWaterAlwaysEnables);

            return mockTemplate.Object;
        }

        private IResourceNode BuildResourceNode(IHexCell location, IImprovementTemplate extractor) {
            var mockNode = new Mock<IResourceNode>();

            var mockDefinition = new Mock<IResourceDefinition>();
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
