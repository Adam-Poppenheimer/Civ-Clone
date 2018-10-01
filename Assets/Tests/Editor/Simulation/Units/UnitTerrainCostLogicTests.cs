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

    /*[TestFixture]
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
            public CellShape      Shape;

            public bool HasDomesticCity;
            public bool HasForeignCity;

            public bool HasRoads;

        }

        public class UnitTestData {

            public int MaxMovement;

            public UnitMovementSummary MovementSummary = new UnitMovementSummary();

        }

        #endregion

        #region static fields and properties

        private static IEnumerable TestCases {
            get {
                yield return new TestCaseData(new TestData() {
                    CurrentCell = new HexCellTestData(),
                    NextCell = new HexCellTestData() {
                        Terrain = CellTerrain.Grassland, Vegetation = CellVegetation.None,
                        Shape = CellShape.Flatlands
                    },
                    Unit = new UnitTestData() {
                        MovementSummary = new UnitMovementSummary() { CanTraverseLand = true }
                    }
                }).Returns(1).SetName("flat empty grassland, CanTraverseLand is true");

                yield return new TestCaseData(new TestData() {
                    CurrentCell = new HexCellTestData(),
                    NextCell = new HexCellTestData() {
                        Terrain = CellTerrain.Grassland, Vegetation = CellVegetation.None,
                        Shape = CellShape.Flatlands
                    },
                    Unit = new UnitTestData() {
                        MovementSummary = new UnitMovementSummary() { CanTraverseLand = false }
                    }
                }).Returns(-1).SetName("flat empty grassland, CanTraverseLand is false");

                yield return new TestCaseData(new TestData() {
                    CurrentCell = new HexCellTestData(),
                    NextCell = new HexCellTestData() {
                        Terrain = CellTerrain.ShallowWater, Vegetation = CellVegetation.None,
                        Shape = CellShape.Flatlands
                    },
                    Unit = new UnitTestData() {
                        MovementSummary = new UnitMovementSummary() { CanTraverseShallowWater = false }
                    }
                }).Returns(-1).SetName("Shallow water, CanTraverseShallowWater is false");

                yield return new TestCaseData(new TestData() {
                    CurrentCell = new HexCellTestData(),
                    NextCell = new HexCellTestData() {
                        Terrain = CellTerrain.ShallowWater, Vegetation = CellVegetation.None,
                        Shape = CellShape.Flatlands
                    },
                    Unit = new UnitTestData() {
                        MovementSummary = new UnitMovementSummary() { CanTraverseShallowWater = true }
                    }
                }).Returns(1).SetName("Shallow water, CanTraverseShallowWater is true");

                yield return new TestCaseData(new TestData() {
                    CurrentCell = new HexCellTestData(),
                    NextCell = new HexCellTestData() {
                        Terrain = CellTerrain.FreshWater, Vegetation = CellVegetation.None,
                        Shape = CellShape.Flatlands
                    },
                    Unit = new UnitTestData() {
                        MovementSummary = new UnitMovementSummary() { CanTraverseShallowWater = true }
                    }
                }).Returns(1).SetName("Fresh water, CanTraverseShallowWater is true");

                yield return new TestCaseData(new TestData() {
                    CurrentCell = new HexCellTestData(),
                    NextCell = new HexCellTestData() {
                        Terrain = CellTerrain.DeepWater, Vegetation = CellVegetation.None,
                        Shape = CellShape.Flatlands
                    },
                    Unit = new UnitTestData() {
                        MovementSummary = new UnitMovementSummary() { CanTraverseDeepWater = true }
                    }
                }).Returns(1).SetName("Deep water, CanTraverseDeepWater is true");

                yield return new TestCaseData(new TestData() {
                    CurrentCell = new HexCellTestData(),
                    NextCell = new HexCellTestData() {
                        Terrain = CellTerrain.DeepWater, Vegetation = CellVegetation.None,
                        Shape = CellShape.Flatlands
                    },
                    Unit = new UnitTestData() {
                        MovementSummary = new UnitMovementSummary() {
                            CanTraverseShallowWater = true, CanTraverseDeepWater = false
                        }
                    }
                }).Returns(-1).SetName("Deep water, CanTraverseDeepWater is false, CanTraverseShallowWater is true");



                yield return new TestCaseData(new TestData() {
                    CurrentCell = new HexCellTestData(),
                    NextCell = new HexCellTestData() {
                        Terrain = CellTerrain.Grassland,
                        HasDomesticCity = true
                    },
                    Unit = new UnitTestData() {
                        MovementSummary = new UnitMovementSummary() { CanTraverseLand = false }
                    }
                }).Returns(1).SetName("Land cell with domestic city, unit cannot normally traverse land");

                yield return new TestCaseData(new TestData() {
                    CurrentCell = new HexCellTestData(),
                    NextCell = new HexCellTestData() {
                        Terrain = CellTerrain.Grassland,
                        HasForeignCity = true
                    },
                    Unit = new UnitTestData() {
                        MovementSummary = new UnitMovementSummary() { CanTraverseLand = true }
                    }
                }).Returns(-1).SetName("Land cell with foreign city, unit can normally traverse land");




                yield return new TestCaseData(new TestData() {
                    CurrentCell = new HexCellTestData(),
                    NextCell = new HexCellTestData() {
                        Terrain = CellTerrain.Grassland, Vegetation = CellVegetation.Forest,
                        Shape = CellShape.Flatlands
                    },
                    Unit = new UnitTestData() {
                        MovementSummary = new UnitMovementSummary() { CanTraverseLand = true }
                    }
                }).Returns(2).SetName("Permitted unit into flat forested grassland");

                yield return new TestCaseData(new TestData() {
                    CurrentCell = new HexCellTestData(),
                    NextCell = new HexCellTestData() {
                        Terrain = CellTerrain.Grassland, Vegetation = CellVegetation.Forest,
                        Shape = CellShape.Flatlands
                    },
                    Unit = new UnitTestData() {
                        MovementSummary = new UnitMovementSummary() { CanTraverseLand = true }
                    }
                }).Returns(2).SetName("Permitted unit into flat forest");

                yield return new TestCaseData(new TestData() {
                    CurrentCell = new HexCellTestData(),
                    NextCell = new HexCellTestData() {
                        Terrain = CellTerrain.Grassland, Vegetation = CellVegetation.None,
                        Shape = CellShape.Hills
                    },
                    Unit = new UnitTestData() {
                        MovementSummary = new UnitMovementSummary() { CanTraverseLand = true }
                    }
                }).Returns(2).SetName("Permitted unit into empty hills");

                yield return new TestCaseData(new TestData() {
                    CurrentCell = new HexCellTestData(),
                    NextCell = new HexCellTestData() {
                        Terrain = CellTerrain.Grassland, Vegetation = CellVegetation.Forest,
                        Shape = CellShape.Hills
                    },
                    Unit = new UnitTestData() {
                        MovementSummary = new UnitMovementSummary() { CanTraverseLand = true }
                    }
                }).Returns(3).SetName("Permitted unit into forested hills");

                yield return new TestCaseData(new TestData() {
                    CurrentCell = new HexCellTestData(),
                    NextCell = new HexCellTestData() {
                        Terrain = CellTerrain.Grassland, Vegetation = CellVegetation.None,
                        Shape = CellShape.Mountains
                    },
                    Unit = new UnitTestData() {
                        MovementSummary = new UnitMovementSummary() { CanTraverseLand = true }
                    }
                }).Returns(-1).SetName("Permitted unit into empty mountains");



                yield return new TestCaseData(new TestData() {
                    CurrentCell = new HexCellTestData(),
                    NextCell = new HexCellTestData() {
                        Terrain = CellTerrain.Grassland, Vegetation = CellVegetation.None,
                        Shape = CellShape.Hills
                    },
                    Unit = new UnitTestData() {
                        MaxMovement = 10,
                        MovementSummary = new UnitMovementSummary() {
                            CanTraverseLand = true,
                            ShapesConsumingFullMovement = new HashSet<CellShape>() { CellShape.Hills }
                        }
                    }
                }).Returns(10).SetName("Permitted unit into empty hills, hills consume full movement");

                yield return new TestCaseData(new TestData() {
                    CurrentCell = new HexCellTestData(),
                    NextCell = new HexCellTestData() {
                        Terrain = CellTerrain.Grassland, Vegetation = CellVegetation.Forest,
                        Shape = CellShape.Flatlands
                    },
                    Unit = new UnitTestData() {
                        MaxMovement = 10,
                        MovementSummary = new UnitMovementSummary() {
                            CanTraverseLand = true,
                            VegetationConsumingFullMovement = new HashSet<CellVegetation>() { CellVegetation.Forest }
                        }
                    }
                }).Returns(10).SetName("Permitted unit into flat forest, forests consume full movement");



                yield return new TestCaseData(new TestData() {
                    CurrentCell = new HexCellTestData(),
                    NextCell = new HexCellTestData() {
                        Terrain = CellTerrain.Grassland, Vegetation = CellVegetation.Forest,
                        Shape = CellShape.Hills
                    },
                    Unit = new UnitTestData() {
                        MaxMovement = 2,
                        MovementSummary = new UnitMovementSummary() {
                            CanTraverseLand = true,
                            TerrainsWithIgnoredCosts = new HashSet<CellTerrain>() { CellTerrain.Grassland }
                        }
                    }
                }).Returns(1).SetName("Permitted unit into forested hills, grassland causes movement costs to be ignored");

                yield return new TestCaseData(new TestData() {
                    CurrentCell = new HexCellTestData(),
                    NextCell = new HexCellTestData() {
                        Terrain = CellTerrain.Grassland, Vegetation = CellVegetation.Forest,
                        Shape = CellShape.Hills
                    },
                    Unit = new UnitTestData() {
                        MaxMovement = 2,
                        MovementSummary = new UnitMovementSummary() {
                            CanTraverseLand = true,
                            ShapesWithIgnoredCosts = new HashSet<CellShape>() { CellShape.Hills }
                        }
                    }
                }).Returns(1).SetName("Permitted unit into forested hills, hills cause movement costs to be ignored");

                yield return new TestCaseData(new TestData() {
                    CurrentCell = new HexCellTestData(),
                    NextCell = new HexCellTestData() {
                        Terrain = CellTerrain.Grassland, Vegetation = CellVegetation.Forest,
                        Shape = CellShape.Hills
                    },
                    Unit = new UnitTestData() {
                        MaxMovement = 2,
                        MovementSummary = new UnitMovementSummary() {
                            CanTraverseLand = true,
                            VegetationsWithIgnoredCosts = new HashSet<CellVegetation>() { CellVegetation.Forest }
                        }
                    }
                }).Returns(1).SetName("Permitted unit into forested hills, forests cause movement costs to be ignored");
                



                yield return new TestCaseData(new TestData() {
                    CurrentCell = new HexCellTestData() {
                        HasRoads = true
                    },
                    NextCell = new HexCellTestData() {
                        Terrain = CellTerrain.Grassland, Vegetation = CellVegetation.Forest,
                        Shape = CellShape.Hills, HasRoads = true
                    },
                    Unit = new UnitTestData() {
                        MovementSummary = new UnitMovementSummary() { CanTraverseLand = true }
                    }
                }).Returns(1f * 0.5f).SetName("Permitted unit into forested hills, current and next have roads");

                yield return new TestCaseData(new TestData() {
                    CurrentCell = new HexCellTestData() {
                        HasRoads = true
                    },
                    NextCell = new HexCellTestData() {
                        Terrain = CellTerrain.Grassland, Vegetation = CellVegetation.Forest,
                        Shape = CellShape.Hills
                    },
                    Unit = new UnitTestData() {
                        MovementSummary = new UnitMovementSummary() { CanTraverseLand = true }
                    }
                }).Returns(3).SetName("Permitted unit into forested hills, current has roads");

                yield return new TestCaseData(new TestData() {
                    CurrentCell = new HexCellTestData(),
                    NextCell = new HexCellTestData() {
                        Terrain = CellTerrain.Grassland, Vegetation = CellVegetation.Forest,
                        Shape = CellShape.Hills, HasRoads = true
                    },
                    Unit = new UnitTestData() {
                        MovementSummary = new UnitMovementSummary() { CanTraverseLand = true }
                    }
                }).Returns(3).SetName("Permitted unit into forested hills, next has roads");
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

        private Mock<IHexMapSimulationConfig>                       MockConfig;
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
            MockConfig              = new Mock<IHexMapSimulationConfig>();
            MockUnitPositionCanon   = new Mock<IUnitPositionCanon>();
            MockCityLocationCanon   = new Mock<IPossessionRelationship<IHexCell, ICity>>();
            MockCityPossessionCanon = new Mock<IPossessionRelationship<ICivilization, ICity>>();
            MockUnitPossessionCanon = new Mock<IPossessionRelationship<ICivilization, IUnit>>();

            MockConfig.Setup(config => config.SlopeMoveCost).Returns(1);

            MockConfig.Setup(config => config.GetBaseMoveCostOfTerrain(It.IsAny<CellTerrain>())).Returns(1);

            MockConfig.Setup(config => config.GetBaseMoveCostOfVegetation(It.IsAny<CellVegetation>()))
                      .Returns<CellVegetation>(feature => VegetationMoveCosts[feature]);

            MockConfig.Setup(config => config.GetBaseMoveCostOfShape(It.IsAny<CellShape>()))
                      .Returns<CellShape>(shape => ShapeMoveCosts[shape]);

            MockConfig.Setup(config => config.RoadMoveCostMultiplier).Returns(0.5f);

            MockConfig.Setup(config => config.CityMoveCost).Returns(1);

            Container.Bind<IHexMapSimulationConfig>                      ().FromInstance(MockConfig             .Object);
            Container.Bind<IUnitPositionCanon>                           ().FromInstance(MockUnitPositionCanon  .Object);
            Container.Bind<IPossessionRelationship<IHexCell, ICity>>     ().FromInstance(MockCityLocationCanon  .Object);
            Container.Bind<IPossessionRelationship<ICivilization, ICity>>().FromInstance(MockCityPossessionCanon.Object);
            Container.Bind<IPossessionRelationship<ICivilization, IUnit>>().FromInstance(MockUnitPossessionCanon.Object);

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

            var unit = BuildUnit(data.Unit, data.Unit.MovementSummary);

            var costLogic = Container.Resolve<UnitTerrainCostLogic>();

            return costLogic.GetTraversalCostForUnit(unit, currentCell, nextCell);
        }

        #endregion

        #region utilities

        private IHexCell BuildCell(HexCellTestData testData){
            var mockCell = new Mock<IHexCell>();

            mockCell.SetupAllProperties();

            var newCell = mockCell.Object;

            newCell.Terrain    = testData.Terrain;
            newCell.Vegetation = testData.Vegetation;
            newCell.Shape      = testData.Shape;
            newCell.HasRoads   = testData.HasRoads;

            MockUnitPositionCanon
                .Setup(canon => canon.CanChangeOwnerOfPossession(It.IsAny<IUnit>(), newCell))
                .Returns(true);

            if(testData.HasDomesticCity) {
                BuildCity(newCell, null);

            }else if(testData.HasForeignCity) {
                BuildCity(newCell, new Mock<ICivilization>().Object);
            }

            return newCell;
        }

        private IUnit BuildUnit(UnitTestData testData, IUnitMovementSummary movementSummary) {
            var mockUnit = new Mock<IUnit>();

            var mockTemplate = new Mock<IUnitTemplate>();

            mockTemplate.Setup(template => template.MaxMovement).Returns(testData.MaxMovement);

            mockUnit.Setup(unit => unit.Template)       .Returns(mockTemplate.Object);
            mockUnit.Setup(unit => unit.MovementSummary).Returns(movementSummary);

            return mockUnit.Object;
        }

        private ICity BuildCity(IHexCell location, ICivilization owner) {
            var newCity = new Mock<ICity>().Object;

            MockCityLocationCanon.Setup(canon => canon.GetOwnerOfPossession(newCity)).Returns(location);
            MockCityLocationCanon.Setup(canon => canon.GetPossessionsOfOwner(location)).Returns(new List<ICity>() { newCity });

            MockCityPossessionCanon.Setup(canon => canon.GetOwnerOfPossession(newCity)).Returns(owner);

            return newCity;
        }

        #endregion

        #endregion

    }*/

}
