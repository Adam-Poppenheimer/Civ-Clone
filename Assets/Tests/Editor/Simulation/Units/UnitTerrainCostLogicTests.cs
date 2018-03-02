using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation.Units;
using Assets.Simulation.HexMap;
using Assets.Simulation.Cities;

namespace Assets.Tests.Simulation.Units {

    [TestFixture]
    public class UnitTerrainCostLogicTests : ZenjectUnitTestFixture {

        #region internal types

        public struct TestData {

            public HexCellTestData CurrentCell;

            public HexCellTestData NextCell;

            public UnitTestData Unit;

        }

        public struct HexCellTestData {

            public TerrainType    Terrain;
            public TerrainFeature Feature;
            public TerrainShape   Shape;

            public int  Elevation;
            public bool IsUnderwater;

            public bool HasCity;

        }

        public struct UnitTestData {

            public bool IsAquatic;

        }

        #endregion

        #region static fields and properties

        private static IEnumerable TestCases {
            get {
                yield return new TestCaseData(new TestData() {
                    CurrentCell = new HexCellTestData() {
                        Elevation = 0
                    },
                    NextCell = new HexCellTestData() {
                        Terrain = TerrainType.Grassland, Feature = TerrainFeature.None,
                        Shape = TerrainShape.Flatlands, Elevation = 0, IsUnderwater = false
                    },
                    Unit = new UnitTestData() {
                        IsAquatic = false
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
                        Elevation = 0
                    },
                    NextCell = new HexCellTestData() {
                        Terrain = TerrainType.Grassland, Feature = TerrainFeature.Forest,
                        Elevation = 0, IsUnderwater = false, Shape = TerrainShape.Flatlands
                    },
                    Unit = new UnitTestData() {
                        IsAquatic = false
                    }
                }).Returns(2).SetName("Non-aquatic into flat forested grassland, no elevation change, no water");

                yield return new TestCaseData(new TestData() {
                    CurrentCell = new HexCellTestData() {
                        Elevation = 0
                    },
                    NextCell = new HexCellTestData() {
                        Terrain = TerrainType.Grassland, Feature = TerrainFeature.None,
                        Shape = TerrainShape.Flatlands, Elevation = 1, IsUnderwater = false
                    },
                    Unit = new UnitTestData() {
                        IsAquatic = false
                    }
                }).Returns(2).SetName("Non-aquatic into flat empty grassland, up a slope, no water");

                yield return new TestCaseData(new TestData() {
                    CurrentCell = new HexCellTestData() {
                        Elevation = 0
                    },
                    NextCell = new HexCellTestData() {
                        Terrain = TerrainType.Grassland, Feature = TerrainFeature.None,
                        Shape = TerrainShape.Flatlands, Elevation = 2, IsUnderwater = false
                    },
                    Unit = new UnitTestData() {
                        IsAquatic = false
                    }
                }).Returns(-1).SetName("Non-aquatic into flat empty grassland, up a cliff, no water");

                yield return new TestCaseData(new TestData() {
                    CurrentCell = new HexCellTestData() {
                        Elevation = 1
                    },
                    NextCell = new HexCellTestData() {
                        Terrain = TerrainType.Grassland, Feature = TerrainFeature.None,
                        Shape = TerrainShape.Flatlands, Elevation = 0, IsUnderwater = false
                    },
                    Unit = new UnitTestData() {
                        IsAquatic = false
                    }
                }).Returns(1).SetName("Non-aquatic into flat empty grassland, down a slope, no water");

                yield return new TestCaseData(new TestData() {
                    CurrentCell = new HexCellTestData() {
                        Elevation = 2
                    },
                    NextCell = new HexCellTestData() {
                        Terrain = TerrainType.Grassland, Feature = TerrainFeature.None,
                        Shape = TerrainShape.Flatlands, Elevation = 0, IsUnderwater = false
                    },
                    Unit = new UnitTestData() {
                        IsAquatic = false
                    }
                }).Returns(-1).SetName("Non-aquatic into flat empty grassland, down a cliff, no water");

                yield return new TestCaseData(new TestData() {
                    CurrentCell = new HexCellTestData() {
                        Elevation = 0
                    },
                    NextCell = new HexCellTestData() {
                        Terrain = TerrainType.Grassland, Feature = TerrainFeature.Forest,
                        Shape = TerrainShape.Flatlands, Elevation = 1, IsUnderwater = false
                    },
                    Unit = new UnitTestData() {
                        IsAquatic = false
                    }
                }).Returns(3).SetName("Non-aquatic into flat forest, up a slope, no water");

                yield return new TestCaseData(new TestData() {
                    CurrentCell = new HexCellTestData() {
                        Elevation = 0
                    },
                    NextCell = new HexCellTestData() {
                        Terrain = TerrainType.Grassland, Feature = TerrainFeature.None,
                        Shape = TerrainShape.Hills, Elevation = 0, IsUnderwater = false
                    },
                    Unit = new UnitTestData() {
                        IsAquatic = false
                    }
                }).Returns(2).SetName("Non-aquatic into empty hills, no water");

                yield return new TestCaseData(new TestData() {
                    CurrentCell = new HexCellTestData() {
                        Elevation = 0
                    },
                    NextCell = new HexCellTestData() {
                        Terrain = TerrainType.Grassland, Feature = TerrainFeature.Forest,
                        Shape = TerrainShape.Hills, Elevation = 0, IsUnderwater = false
                    },
                    Unit = new UnitTestData() {
                        IsAquatic = false
                    }
                }).Returns(3).SetName("Non-aquatic into forested hills, no water");

                yield return new TestCaseData(new TestData() {
                    CurrentCell = new HexCellTestData() {
                        Elevation = 0
                    },
                    NextCell = new HexCellTestData() {
                        Terrain = TerrainType.Grassland, Feature = TerrainFeature.None,
                        Shape = TerrainShape.Mountains, Elevation = 0, IsUnderwater = false
                    },
                    Unit = new UnitTestData() {
                        IsAquatic = false
                    }
                }).Returns(-1).SetName("Non-aquatic into empty mountains, no water");

                yield return new TestCaseData(new TestData() {
                    CurrentCell = new HexCellTestData() {
                        Elevation = 0
                    },
                    NextCell = new HexCellTestData() {
                        Terrain = TerrainType.Grassland, Feature = TerrainFeature.Forest,
                        Shape = TerrainShape.Hills, Elevation = 1, IsUnderwater = false
                    },
                    Unit = new UnitTestData() {
                        IsAquatic = false
                    }
                }).Returns(-1).SetName("Non-aquatic into forested hills, up a slope, no water");
            }
        }

        #endregion

        #region instance fields and properties

        private Mock<IHexGridConfig> MockConfig;

        private Mock<ICityFactory> MockCityFactory;

        private Mock<IUnitPositionCanon> MockUnitPositionCanon;

        private List<ICity> AllCities = new List<ICity>();

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            AllCities.Clear();

            MockConfig            = new Mock<IHexGridConfig>();
            MockCityFactory       = new Mock<ICityFactory>();
            MockUnitPositionCanon = new Mock<IUnitPositionCanon>();

            MockConfig.Setup(config => config.BaseLandMoveCost).Returns(1);
            MockConfig.Setup(config => config.WaterMoveCost)   .Returns(1);
            MockConfig.Setup(config => config.SlopeMoveCost)   .Returns(1);

            MockConfig.Setup(config => config.FeatureMoveCosts).Returns(new List<int>() {
                0, //None cost
                1, //Forest cost
            }.AsReadOnly());

            MockConfig.Setup(config => config.ShapeMoveCosts).Returns(new List<int>() {
                0, //Flatland cost
                1, // Hills cost
                -1 // Mountains cost
            }.AsReadOnly());

            MockCityFactory.Setup(factory => factory.AllCities).Returns(AllCities.AsReadOnly());

            Container.Bind<IHexGridConfig>    ().FromInstance(MockConfig           .Object);
            Container.Bind<ICityFactory>      ().FromInstance(MockCityFactory      .Object);
            Container.Bind<IUnitPositionCanon>().FromInstance(MockUnitPositionCanon.Object);

            Container.Bind<UnitTerrainCostLogic>().AsSingle();
        }

        #endregion

        #region tests

        [Test(Description = "GetTraversalCostForUnit should consider the terrain and features of NextCell, " +
            "as well as whether that cell is underwater, to determine unit traversal. It should also consider " +
            "the EdgeType between currentCell and nextCell and whether the argued unit is aquatic or not")]
        [TestCaseSource("TestCases")]
        public int GetTraversalCostForUnitTests(TestData data){
            var currentCell = BuildCell(data.CurrentCell);

            var nextCell = BuildCell(data.NextCell);

            var unit = BuildUnit(data.Unit);

            var costLogic = Container.Resolve<UnitTerrainCostLogic>();

            return costLogic.GetTraversalCostForUnit(unit, currentCell, nextCell);
        }

        #endregion

        #region utilities

        private IHexCell BuildCell(HexCellTestData testData){
            var mockCell = new Mock<IHexCell>();

            mockCell.SetupAllProperties();
            mockCell.Setup(cell => cell.IsUnderwater).Returns(testData.IsUnderwater);

            var newCell = mockCell.Object;

            newCell.Terrain = testData.Terrain;
            newCell.Feature = testData.Feature;
            newCell.Shape   = testData.Shape;
            newCell.FoundationElevation = testData.Elevation;

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

            unitMock.Setup(unit => unit.IsAquatic).Returns(testData.IsAquatic);

            return unitMock.Object;
        }

        private ICity BuildCity(IHexCell location) {
            var mockCity = new Mock<ICity>();

            mockCity.Setup(city => city.Location).Returns(location);

            AllCities.Add(mockCity.Object);

            return mockCity.Object;
        }

        #endregion

        #endregion

    }

}
