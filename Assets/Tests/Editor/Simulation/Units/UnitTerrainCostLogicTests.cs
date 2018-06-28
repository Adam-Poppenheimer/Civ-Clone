using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation;
using Assets.Simulation.Units;
using Assets.Simulation.Units.Promotions;
using Assets.Simulation.HexMap;
using Assets.Simulation.Cities;
using Assets.Simulation.Civilizations;

namespace Assets.Tests.Simulation.Units {

    [TestFixture]
    public class UnitTerrainCostLogicTests : ZenjectUnitTestFixture {

        #region internal types

        public class TestData {

            public HexCellTestData CurrentCell;

            public HexCellTestData NextCell;

            public UnitTestData Unit;

        }

        public class HexCellTestData {

            public CellTerrain    Terrain;
            public CellVegetation Vegetation;
            public CellShape   Shape;

            public int  FoundationElevation;
            public bool IsUnderwater;

            public bool HasCity;

            public bool HasRoads;

        }

        public class UnitTestData {

            public bool IsAquatic;

            public int MaxMovement;

            public MovementInfo MovementInfo = new MovementInfo();

        }

        #endregion

        #region static fields and properties

        private static IEnumerable TestCases {
            get {
                yield return new TestCaseData(new TestData() {
                    CurrentCell = new HexCellTestData() {
                        FoundationElevation = 0
                    },
                    NextCell = new HexCellTestData() {
                        Terrain = CellTerrain.Grassland, Vegetation = CellVegetation.None,
                        Shape = CellShape.Flatlands, FoundationElevation = 0, IsUnderwater = false
                    },
                    Unit = new UnitTestData() {
                        IsAquatic = false,
                    }
                }).Returns(1).SetName("Non-aquatic into flat empty grassland, no elevation change, no water");

                yield return new TestCaseData(new TestData() {
                    CurrentCell = new HexCellTestData(),
                    NextCell = new HexCellTestData() {
                        IsUnderwater = true
                    },
                    Unit = new UnitTestData() {
                        IsAquatic = true
                    }
                }).Returns(1).SetName("Aquatic into underwater cell");

                yield return new TestCaseData(new TestData() {
                    CurrentCell = new HexCellTestData(),
                    NextCell = new HexCellTestData() {
                        IsUnderwater = false
                    },
                    Unit = new UnitTestData() {
                        IsAquatic = true
                    },
                }).Returns(-1).SetName("Aquatic into land cell");

                yield return new TestCaseData(new TestData() {
                    CurrentCell = new HexCellTestData(),
                    NextCell = new HexCellTestData() {
                        IsUnderwater = true,
                        HasCity = true
                    },
                    Unit = new UnitTestData() {
                        IsAquatic = true
                    }
                }).Returns(1).SetName("Aquatic into land cell with city");

                yield return new TestCaseData(new TestData() {
                    CurrentCell = new HexCellTestData(),
                    NextCell = new HexCellTestData() {
                        IsUnderwater = true
                    },
                    Unit = new UnitTestData() {
                        IsAquatic = false
                    }
                }).Returns(-1).SetName("Non-aquatic into underwater cell");

                yield return new TestCaseData(new TestData() {
                    CurrentCell = new HexCellTestData() {
                        FoundationElevation = 0
                    },
                    NextCell = new HexCellTestData() {
                        Terrain = CellTerrain.Grassland, Vegetation = CellVegetation.Forest,
                        FoundationElevation = 0, IsUnderwater = false, Shape = CellShape.Flatlands
                    },
                    Unit = new UnitTestData() {
                        IsAquatic = false
                    }
                }).Returns(2).SetName("Non-aquatic into flat forested grassland, no elevation change, no water");

                yield return new TestCaseData(new TestData() {
                    CurrentCell = new HexCellTestData() {
                        FoundationElevation = 0
                    },
                    NextCell = new HexCellTestData() {
                        Terrain = CellTerrain.Grassland, Vegetation = CellVegetation.None,
                        Shape = CellShape.Flatlands, FoundationElevation = 1, IsUnderwater = false
                    },
                    Unit = new UnitTestData() {
                        IsAquatic = false
                    }
                }).Returns(2).SetName("Non-aquatic into flat empty grassland, up a slope, no water");

                yield return new TestCaseData(new TestData() {
                    CurrentCell = new HexCellTestData() {
                        FoundationElevation = 0
                    },
                    NextCell = new HexCellTestData() {
                        Terrain = CellTerrain.Grassland, Vegetation = CellVegetation.None,
                        Shape = CellShape.Flatlands, FoundationElevation = 2, IsUnderwater = false
                    },
                    Unit = new UnitTestData() {
                        IsAquatic = false
                    }
                }).Returns(-1).SetName("Non-aquatic into flat empty grassland, up a cliff, no water");

                yield return new TestCaseData(new TestData() {
                    CurrentCell = new HexCellTestData() {
                        FoundationElevation = 1
                    },
                    NextCell = new HexCellTestData() {
                        Terrain = CellTerrain.Grassland, Vegetation = CellVegetation.None,
                        Shape = CellShape.Flatlands, FoundationElevation = 0, IsUnderwater = false
                    },
                    Unit = new UnitTestData() {
                        IsAquatic = false
                    }
                }).Returns(1).SetName("Non-aquatic into flat empty grassland, down a slope, no water");

                yield return new TestCaseData(new TestData() {
                    CurrentCell = new HexCellTestData() {
                        FoundationElevation = 2
                    },
                    NextCell = new HexCellTestData() {
                        Terrain = CellTerrain.Grassland, Vegetation = CellVegetation.None,
                        Shape = CellShape.Flatlands, FoundationElevation = 0, IsUnderwater = false
                    },
                    Unit = new UnitTestData() {
                        IsAquatic = false
                    }
                }).Returns(-1).SetName("Non-aquatic into flat empty grassland, down a cliff, no water");

                yield return new TestCaseData(new TestData() {
                    CurrentCell = new HexCellTestData() {
                        FoundationElevation = 0
                    },
                    NextCell = new HexCellTestData() {
                        Terrain = CellTerrain.Grassland, Vegetation = CellVegetation.Forest,
                        Shape = CellShape.Flatlands, FoundationElevation = 1, IsUnderwater = false
                    },
                    Unit = new UnitTestData() {
                        IsAquatic = false
                    }
                }).Returns(3).SetName("Non-aquatic into flat forest, up a slope, no water");

                yield return new TestCaseData(new TestData() {
                    CurrentCell = new HexCellTestData() {
                        FoundationElevation = 0
                    },
                    NextCell = new HexCellTestData() {
                        Terrain = CellTerrain.Grassland, Vegetation = CellVegetation.None,
                        Shape = CellShape.Hills, FoundationElevation = 0, IsUnderwater = false
                    },
                    Unit = new UnitTestData() {
                        IsAquatic = false
                    }
                }).Returns(3).SetName("Non-aquatic into empty hills, no water");

                yield return new TestCaseData(new TestData() {
                    CurrentCell = new HexCellTestData() {
                        FoundationElevation = 0
                    },
                    NextCell = new HexCellTestData() {
                        Terrain = CellTerrain.Grassland, Vegetation = CellVegetation.Forest,
                        Shape = CellShape.Hills, FoundationElevation = 0, IsUnderwater = false
                    },
                    Unit = new UnitTestData() {
                        IsAquatic = false
                    }
                }).Returns(4).SetName("Non-aquatic into forested hills, no water");

                yield return new TestCaseData(new TestData() {
                    CurrentCell = new HexCellTestData() {
                        FoundationElevation = 0
                    },
                    NextCell = new HexCellTestData() {
                        Terrain = CellTerrain.Grassland, Vegetation = CellVegetation.None,
                        Shape = CellShape.Mountains, FoundationElevation = 0, IsUnderwater = false
                    },
                    Unit = new UnitTestData() {
                        IsAquatic = false
                    }
                }).Returns(-1).SetName("Non-aquatic into empty mountains, no water");

                yield return new TestCaseData(new TestData() {
                    CurrentCell = new HexCellTestData() {
                        FoundationElevation = 0
                    },
                    NextCell = new HexCellTestData() {
                        Terrain = CellTerrain.Grassland, Vegetation = CellVegetation.Forest,
                        Shape = CellShape.Hills, FoundationElevation = 1, IsUnderwater = false
                    },
                    Unit = new UnitTestData() {
                        IsAquatic = false
                    }
                }).Returns(-1).SetName("Non-aquatic into forested hills, up a slope, no water");

                yield return new TestCaseData(new TestData() {
                    CurrentCell = new HexCellTestData() {
                        FoundationElevation = 0
                    },
                    NextCell = new HexCellTestData() {
                        Terrain = CellTerrain.Grassland, Vegetation = CellVegetation.Forest,
                        Shape = CellShape.Hills, FoundationElevation = 0, IsUnderwater = false
                    },
                    Unit = new UnitTestData() {
                        IsAquatic = false, MovementInfo = new MovementInfo() { IgnoresTerrainCosts = true }
                    }
                }).Returns(1).SetName("Non-aquatic into forested hills, unit ignores terrain costs");



                yield return new TestCaseData(new TestData() {
                    CurrentCell = new HexCellTestData() {
                        FoundationElevation = 0
                    },
                    NextCell = new HexCellTestData() {
                        Terrain = CellTerrain.Grassland, Vegetation = CellVegetation.None,
                        Shape = CellShape.Hills, FoundationElevation = 0, IsUnderwater = false
                    },
                    Unit = new UnitTestData() {
                        IsAquatic = false, MaxMovement = 10,
                        MovementInfo = new MovementInfo() { HasRoughTerrainPenalty = true }
                    }
                }).Returns(10).SetName("Non-aquatic into empty hills, unit has rough terrain penalty");

                yield return new TestCaseData(new TestData() {
                    CurrentCell = new HexCellTestData() {
                        FoundationElevation = 0
                    },
                    NextCell = new HexCellTestData() {
                        Terrain = CellTerrain.Grassland, Vegetation = CellVegetation.Forest,
                        Shape = CellShape.Flatlands, FoundationElevation = 0, IsUnderwater = false
                    },
                    Unit = new UnitTestData() {
                        IsAquatic = false, MaxMovement = 10,
                        MovementInfo = new MovementInfo() { HasRoughTerrainPenalty = true }
                    }
                }).Returns(10).SetName("Non-aquatic into flat forest, unit has rough terrain penalty");

                yield return new TestCaseData(new TestData() {
                    CurrentCell = new HexCellTestData() {
                        FoundationElevation = 0
                    },
                    NextCell = new HexCellTestData() {
                        Terrain = CellTerrain.Grassland, Vegetation = CellVegetation.Forest,
                        Shape = CellShape.Hills, FoundationElevation = 0, IsUnderwater = false
                    },
                    Unit = new UnitTestData() {
                        IsAquatic = false, MaxMovement = 10,
                        MovementInfo = new MovementInfo() { HasRoughTerrainPenalty = true }
                    }
                }).Returns(10).SetName("Non-aquatic into forested hills, unit has rough terrain penalty");

                yield return new TestCaseData(new TestData() {
                    CurrentCell = new HexCellTestData() {
                        FoundationElevation = 0, Shape = CellShape.Hills
                    },
                    NextCell = new HexCellTestData() {
                        Terrain = CellTerrain.Grassland, Vegetation = CellVegetation.None,
                        Shape = CellShape.Hills, FoundationElevation = 0, IsUnderwater = false
                    },
                    Unit = new UnitTestData() {
                        IsAquatic = false, MaxMovement = 10,
                        MovementInfo = new MovementInfo() { HasRoughTerrainPenalty = true }
                    }
                }).Returns(10).SetName("Non-aquatic from hills to hills, unit has rough terrain penalty");



                yield return new TestCaseData(new TestData() {
                    CurrentCell = new HexCellTestData() {
                        FoundationElevation = 0, HasRoads = true
                    },
                    NextCell = new HexCellTestData() {
                        Terrain = CellTerrain.Grassland, Vegetation = CellVegetation.Forest,
                        Shape = CellShape.Hills, IsUnderwater = false, HasRoads = true
                    },
                    Unit = new UnitTestData() {
                        IsAquatic = false, MaxMovement = 2
                    }
                }).Returns(1f * 0.5f).SetName("Non-aquatic into forested hills, current and next have roads");

                yield return new TestCaseData(new TestData() {
                    CurrentCell = new HexCellTestData() {
                        FoundationElevation = 0, HasRoads = true
                    },
                    NextCell = new HexCellTestData() {
                        Terrain = CellTerrain.Grassland, Vegetation = CellVegetation.Forest,
                        Shape = CellShape.Hills, IsUnderwater = false
                    },
                    Unit = new UnitTestData() {
                        IsAquatic = false, MaxMovement = 2
                    }
                }).Returns(4).SetName("Non-aquatic into forested hills, current has roads");

                yield return new TestCaseData(new TestData() {
                    CurrentCell = new HexCellTestData() {
                        FoundationElevation = 0
                    },
                    NextCell = new HexCellTestData() {
                        Terrain = CellTerrain.Grassland, Vegetation = CellVegetation.Forest,
                        Shape = CellShape.Hills, IsUnderwater = false, HasRoads = true
                    },
                    Unit = new UnitTestData() {
                        IsAquatic = false, MaxMovement = 2
                    }
                }).Returns(4).SetName("Non-aquatic into forested hills, next has roads");
            }
        }

        private Dictionary<CellVegetation, int> VegetationMoveCosts = new Dictionary<CellVegetation, int>() {
            { CellVegetation.None,   0 },
            { CellVegetation.Forest, 1 },
        };

        private Dictionary<CellShape, int> ShapeMoveCosts = new Dictionary<CellShape, int>() {
            { CellShape.Flatlands,  0 },
            { CellShape.Hills,      1 },
            { CellShape.Mountains, -1 },
        };

        #endregion

        #region instance fields and properties

        private Mock<IHexMapConfig>                                 MockConfig;
        private Mock<IUnitPositionCanon>                            MockUnitPositionCanon;
        private Mock<IPossessionRelationship<IHexCell, ICity>>      MockCityLocationCanon;
        private Mock<IPossessionRelationship<ICivilization, ICity>> MockCityPossessionCanon;
        private Mock<IPossessionRelationship<ICivilization, IUnit>> MockUnitPossessionCanon;
        private Mock<IPromotionParser>                              MockPromotionParser;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockConfig              = new Mock<IHexMapConfig>();
            MockUnitPositionCanon   = new Mock<IUnitPositionCanon>();
            MockCityLocationCanon   = new Mock<IPossessionRelationship<IHexCell, ICity>>();
            MockCityPossessionCanon = new Mock<IPossessionRelationship<ICivilization, ICity>>();
            MockUnitPossessionCanon = new Mock<IPossessionRelationship<ICivilization, IUnit>>();
            MockPromotionParser     = new Mock<IPromotionParser>();

            MockConfig.Setup(config => config.SlopeMoveCost).Returns(1);

            MockConfig.Setup(config => config.GetBaseMoveCostOfTerrain(It.IsAny<CellTerrain>())).Returns(1);

            MockConfig.Setup(config => config.GetBaseMoveCostOfVegetation(It.IsAny<CellVegetation>()))
                      .Returns<CellVegetation>(feature => VegetationMoveCosts[feature]);

            MockConfig.Setup(config => config.GetBaseMoveCostOfShape(It.IsAny<CellShape>()))
                      .Returns<CellShape>(shape => ShapeMoveCosts[shape]);

            MockConfig.Setup(config => config.RoadMoveCostMultiplier).Returns(0.5f);

            Container.Bind<IHexMapConfig>                                ().FromInstance(MockConfig             .Object);
            Container.Bind<IUnitPositionCanon>                           ().FromInstance(MockUnitPositionCanon  .Object);
            Container.Bind<IPossessionRelationship<IHexCell, ICity>>     ().FromInstance(MockCityLocationCanon  .Object);
            Container.Bind<IPossessionRelationship<ICivilization, ICity>>().FromInstance(MockCityPossessionCanon.Object);
            Container.Bind<IPossessionRelationship<ICivilization, IUnit>>().FromInstance(MockUnitPossessionCanon.Object);
            Container.Bind<IPromotionParser>                             ().FromInstance(MockPromotionParser    .Object);            

            Container.Bind<UnitTerrainCostLogic>().AsSingle();
        }

        #endregion

        #region tests

        [Test(Description = "GetTraversalCostForUnit should consider the terrain and features of NextCell, " +
            "as well as whether that cell is underwater, to determine unit traversal. It should also consider " +
            "the EdgeType between currentCell and nextCell and whether the argued unit is aquatic or not")]
        [TestCaseSource("TestCases")]
        public float GetTraversalCostForUnitTests(TestData data){
            var currentCell = BuildCell(data.CurrentCell);

            var nextCell = BuildCell(data.NextCell);

            var unit = BuildUnit(data.Unit);

            MockPromotionParser.Setup(parser => parser.GetMovementInfo(unit)).Returns(data.Unit.MovementInfo);

            var costLogic = Container.Resolve<UnitTerrainCostLogic>();

            return costLogic.GetTraversalCostForUnit(unit, currentCell, nextCell);
        }

        #endregion

        #region utilities

        private IHexCell BuildCell(HexCellTestData testData){
            var mockCell = new Mock<IHexCell>();

            mockCell.SetupAllProperties();
            mockCell.Setup(cell => cell.Terrain ).Returns(testData.IsUnderwater ? CellTerrain.FreshWater : CellTerrain.Grassland);

            int edgeElevation;

            switch(testData.Shape) {
                case CellShape.Hills: case CellShape.Mountains: edgeElevation = testData.FoundationElevation + 1; break;
                default: edgeElevation = testData.FoundationElevation; break;
            }

            mockCell.Setup(cell => cell.EdgeElevation).Returns(edgeElevation);
            mockCell.Setup(cell => cell.FoundationElevation).Returns(testData.FoundationElevation);

            var newCell = mockCell.Object;

            newCell.Terrain    = testData.Terrain;
            newCell.Vegetation = testData.Vegetation;
            newCell.Shape      = testData.Shape;
            newCell.HasRoads   = testData.HasRoads;

            MockUnitPositionCanon
                .Setup(canon => canon.CanChangeOwnerOfPossession(It.IsAny<IUnit>(), newCell))
                .Returns(true);

            if(testData.HasCity) {
                BuildCity(newCell);
            }

            return newCell;
        }

        private IUnit BuildUnit(UnitTestData testData) {
            var unitMock = new Mock<IUnit>();

            var mockTemplate = new Mock<IUnitTemplate>();

            mockTemplate.Setup(template => template.MaxMovement).Returns(testData.MaxMovement);

            unitMock.Setup(unit => unit.Template).Returns(mockTemplate.Object);

            unitMock.Setup(unit => unit.IsAquatic).Returns(testData.IsAquatic);

            return unitMock.Object;
        }

        private ICity BuildCity(IHexCell location) {
            var newCity = new Mock<ICity>().Object;

            MockCityLocationCanon.Setup(canon => canon.GetOwnerOfPossession(newCity)).Returns(location);

            return newCity;
        }

        #endregion

        #endregion

    }

}
