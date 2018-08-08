using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation.HexMap;

namespace Assets.Tests.Simulation.HexMap {

    [TestFixture]
    public class CellModificationLogicTests : ZenjectUnitTestFixture {

        #region internal types

        public class CanChangeTerrainOfCellTestData {

            public CellTerrain Terrain;

            public HexCellTestData Cell;

        }

        public class CanChangeVegetationOfCellTestData {

            public CellVegetation Vegetation;

            public HexCellTestData Cell;

        }

        public class CanChangeHasRoadsOfCellTestData {

            public bool HasRoads;

            public HexCellTestData Cell;

        }

        public class HexCellTestData {

            public CellTerrain    Terrain;
            public CellShape      Shape;
            public CellVegetation Vegetation;
            public bool           HasRoads;
            public bool           HasRiver;

        }

        #endregion

        #region static fields and properties

        public static IEnumerable CanChangeTerrainOfCellTestCases {
            get {
                yield return new TestCaseData(new CanChangeTerrainOfCellTestData() {
                    Terrain = CellTerrain.FloodPlains,
                    Cell = new HexCellTestData() {
                        Shape = CellShape.Flatlands, Terrain = CellTerrain.Desert,
                        HasRiver = true
                    }
                }).SetName("Changing flat desert with river to flood plains").Returns(true);

                yield return new TestCaseData(new CanChangeTerrainOfCellTestData() {
                    Terrain = CellTerrain.FloodPlains,
                    Cell = new HexCellTestData() {
                        Shape = CellShape.Hills, Terrain = CellTerrain.Desert,
                        HasRiver = true
                    }
                }).SetName("Changing hilly desert with river to flood plains").Returns(false);

                yield return new TestCaseData(new CanChangeTerrainOfCellTestData() {
                    Terrain = CellTerrain.FloodPlains,
                    Cell = new HexCellTestData() {
                        Shape = CellShape.Flatlands, Terrain = CellTerrain.Grassland,
                        HasRiver = true
                    }
                }).SetName("Changing flat non-desert with river to flood plains").Returns(false);

                yield return new TestCaseData(new CanChangeTerrainOfCellTestData() {
                    Terrain = CellTerrain.FloodPlains,
                    Cell = new HexCellTestData() {
                        Shape = CellShape.Flatlands, Terrain = CellTerrain.Desert,
                        HasRiver = false
                    }
                }).SetName("Changing flat desert without river to flood plains").Returns(false);
            }
        }

        public static IEnumerable CanChangeVegetationOfCellTestCases {
            get {
                //When argued vegetation is None
                yield return new TestCaseData(new CanChangeVegetationOfCellTestData() {
                    Cell = new HexCellTestData() {
                        Terrain = CellTerrain.ShallowWater
                    },
                    Vegetation = CellVegetation.None
                }).SetName("None valid when cell underwater").Returns(true);

                yield return new TestCaseData(new CanChangeVegetationOfCellTestData() {
                    Cell = new HexCellTestData() {
                        Terrain = CellTerrain.Grassland
                    },
                    Vegetation = CellVegetation.None
                }).SetName("None valid when cell not underwater").Returns(true);

                
                //When argued vegetation is Forest
                yield return new TestCaseData(new CanChangeVegetationOfCellTestData() {
                    Cell = new HexCellTestData() {
                        Terrain = CellTerrain.ShallowWater
                    },
                    Vegetation = CellVegetation.Forest
                }).SetName("Forest invalid when cell underwater").Returns(false);

                yield return new TestCaseData(new CanChangeVegetationOfCellTestData() {
                    Cell = new HexCellTestData() {
                        Terrain = CellTerrain.Grassland
                    },
                    Vegetation = CellVegetation.Forest
                }).SetName("Forest valid on grassland").Returns(true);

                yield return new TestCaseData(new CanChangeVegetationOfCellTestData() {
                    Cell = new HexCellTestData() {
                        Terrain = CellTerrain.Plains
                    },
                    Vegetation = CellVegetation.Forest
                }).SetName("Forest valid on plains").Returns(true);

                yield return new TestCaseData(new CanChangeVegetationOfCellTestData() {
                    Cell = new HexCellTestData() {
                        Terrain = CellTerrain.Tundra
                    },
                    Vegetation = CellVegetation.Forest
                }).SetName("Forest valid on tundra").Returns(true);

                yield return new TestCaseData(new CanChangeVegetationOfCellTestData() {
                    Cell = new HexCellTestData() {
                        Terrain = CellTerrain.Desert
                    },
                    Vegetation = CellVegetation.Forest
                }).SetName("Forest invalid on desert").Returns(false);

                yield return new TestCaseData(new CanChangeVegetationOfCellTestData() {
                    Cell = new HexCellTestData() {
                        Terrain = CellTerrain.FloodPlains
                    },
                    Vegetation = CellVegetation.Forest
                }).SetName("Forest invalid on flood plains").Returns(false);

                yield return new TestCaseData(new CanChangeVegetationOfCellTestData() {
                    Cell = new HexCellTestData() {
                        Terrain = CellTerrain.Snow
                    },
                    Vegetation = CellVegetation.Forest
                }).SetName("Forest invalid on snow").Returns(false);

                yield return new TestCaseData(new CanChangeVegetationOfCellTestData() {
                    Cell = new HexCellTestData() {
                        Terrain = CellTerrain.Grassland, Shape = CellShape.Hills
                    },
                    Vegetation = CellVegetation.Forest
                }).SetName("Forest valid on hills").Returns(true);

                yield return new TestCaseData(new CanChangeVegetationOfCellTestData() {
                    Cell = new HexCellTestData() {
                        Terrain = CellTerrain.Grassland, Shape = CellShape.Mountains
                    },
                    Vegetation = CellVegetation.Forest
                }).SetName("Forest invalid on mountains").Returns(false);




                //When argued vegetation is Jungle
                yield return new TestCaseData(new CanChangeVegetationOfCellTestData() {
                    Cell = new HexCellTestData() {
                        Terrain = CellTerrain.ShallowWater
                    },
                    Vegetation = CellVegetation.Jungle
                }).SetName("Jungle invalid when cell underwater").Returns(false);

                yield return new TestCaseData(new CanChangeVegetationOfCellTestData() {
                    Cell = new HexCellTestData() {
                        Terrain = CellTerrain.Grassland
                    },
                    Vegetation = CellVegetation.Jungle
                }).SetName("Jungle valid on grassland").Returns(true);

                yield return new TestCaseData(new CanChangeVegetationOfCellTestData() {
                    Cell = new HexCellTestData() {
                        Terrain = CellTerrain.Plains
                    },
                    Vegetation = CellVegetation.Jungle
                }).SetName("Jungle valid on plains").Returns(true);

                yield return new TestCaseData(new CanChangeVegetationOfCellTestData() {
                    Cell = new HexCellTestData() {
                        Terrain = CellTerrain.Tundra
                    },
                    Vegetation = CellVegetation.Jungle
                }).SetName("Jungle invalid on tundra").Returns(false);

                yield return new TestCaseData(new CanChangeVegetationOfCellTestData() {
                    Cell = new HexCellTestData() {
                        Terrain = CellTerrain.Desert
                    },
                    Vegetation = CellVegetation.Jungle
                }).SetName("Jungle invalid on desert").Returns(false);

                yield return new TestCaseData(new CanChangeVegetationOfCellTestData() {
                    Cell = new HexCellTestData() {
                        Terrain = CellTerrain.Snow
                    },
                    Vegetation = CellVegetation.Jungle
                }).SetName("Jungle invalid on snow").Returns(false);

                yield return new TestCaseData(new CanChangeVegetationOfCellTestData() {
                    Cell = new HexCellTestData() {
                        Terrain = CellTerrain.Grassland, Shape = CellShape.Hills
                    },
                    Vegetation = CellVegetation.Jungle
                }).SetName("Jungle valid on hills").Returns(true);

                yield return new TestCaseData(new CanChangeVegetationOfCellTestData() {
                    Cell = new HexCellTestData() {
                        Terrain = CellTerrain.Grassland, Shape = CellShape.Mountains
                    },
                    Vegetation = CellVegetation.Jungle
                }).SetName("Jungle invalid on mountains").Returns(false);



                //When argued vegetation is Marsh
                yield return new TestCaseData(new CanChangeVegetationOfCellTestData() {
                    Cell = new HexCellTestData() {
                        Terrain = CellTerrain.ShallowWater
                    },
                    Vegetation = CellVegetation.Marsh
                }).SetName("Marsh valid when cell underwater").Returns(true);

                yield return new TestCaseData(new CanChangeVegetationOfCellTestData() {
                    Cell = new HexCellTestData() {
                        Terrain = CellTerrain.Grassland
                    },
                    Vegetation = CellVegetation.Marsh
                }).SetName("Marsh valid on grassland").Returns(true);

                yield return new TestCaseData(new CanChangeVegetationOfCellTestData() {
                    Cell = new HexCellTestData() {
                        Terrain = CellTerrain.Plains
                    },
                    Vegetation = CellVegetation.Marsh
                }).SetName("Marsh valid on plains").Returns(true);

                yield return new TestCaseData(new CanChangeVegetationOfCellTestData() {
                    Cell = new HexCellTestData() {
                        Terrain = CellTerrain.Tundra
                    },
                    Vegetation = CellVegetation.Marsh
                }).SetName("Marsh valid on tundra").Returns(true);

                yield return new TestCaseData(new CanChangeVegetationOfCellTestData() {
                    Cell = new HexCellTestData() {
                        Terrain = CellTerrain.Desert
                    },
                    Vegetation = CellVegetation.Marsh
                }).SetName("Marsh valid on desert").Returns(true);

                yield return new TestCaseData(new CanChangeVegetationOfCellTestData() {
                    Cell = new HexCellTestData() {
                        Terrain = CellTerrain.Snow
                    },
                    Vegetation = CellVegetation.Marsh
                }).SetName("Marsh valid on snow").Returns(true);

                yield return new TestCaseData(new CanChangeVegetationOfCellTestData() {
                    Cell = new HexCellTestData() {
                        Terrain = CellTerrain.Grassland, Shape = CellShape.Hills
                    },
                    Vegetation = CellVegetation.Marsh
                }).SetName("Marsh valid on hills").Returns(true);

                yield return new TestCaseData(new CanChangeVegetationOfCellTestData() {
                    Cell = new HexCellTestData() {
                        Terrain = CellTerrain.Grassland, Shape = CellShape.Mountains
                    },
                    Vegetation = CellVegetation.Marsh
                }).SetName("Marsh valid on mountains").Returns(true);
            }
        }

        public static IEnumerable CanChangeHasRoadsOfCellTestCases {
            get {
                //hasRoads argument is false (not exhaustive)
                yield return new TestCaseData(new CanChangeHasRoadsOfCellTestData() {
                    Cell = new HexCellTestData() {
                        Terrain = CellTerrain.ShallowWater
                    },
                    HasRoads = false
                }).SetName("False valid when cell is underwater").Returns(true);

                yield return new TestCaseData(new CanChangeHasRoadsOfCellTestData() {
                    Cell = new HexCellTestData() {
                        Terrain = CellTerrain.Grassland
                    },
                    HasRoads = false
                }).SetName("False valid when cell is not underwater").Returns(true);


                //hasRoads argument is true
                yield return new TestCaseData(new CanChangeHasRoadsOfCellTestData() {
                    Cell = new HexCellTestData() {
                        Terrain = CellTerrain.ShallowWater
                    },
                    HasRoads = true
                }).SetName("True invalid when cell is underwater").Returns(false);

                yield return new TestCaseData(new CanChangeHasRoadsOfCellTestData() {
                    Cell = new HexCellTestData() {
                        Terrain = CellTerrain.Grassland, Shape = CellShape.Flatlands
                    },
                    HasRoads = true
                }).SetName("True valid when cell is Flatlands").Returns(true);

                yield return new TestCaseData(new CanChangeHasRoadsOfCellTestData() {
                    Cell = new HexCellTestData() {
                        Terrain = CellTerrain.Grassland, Shape = CellShape.Hills
                    },
                    HasRoads = true
                }).SetName("True valid when cell is Hills").Returns(true);

                yield return new TestCaseData(new CanChangeHasRoadsOfCellTestData() {
                    Cell = new HexCellTestData() {
                        Terrain = CellTerrain.Grassland, Shape = CellShape.Mountains
                    },
                    HasRoads = true
                }).SetName("True invalid when cell is Mountains").Returns(false);
            }
        }

        #endregion

        #region instance fields and properties

        private Mock<IRiverCanon>   MockRiverCanon;        
        private Mock<IHexMapConfig> MockConfig;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockRiverCanon = new Mock<IRiverCanon>();
            MockConfig     = new Mock<IHexMapConfig>();

            Container.Bind<IRiverCanon>  ().FromInstance(MockRiverCanon.Object);
            Container.Bind<IHexMapConfig>().FromInstance(MockConfig    .Object);

            Container.Bind<CellModificationLogic>().AsSingle();
        }

        #endregion

        #region tests

        #region Terrain tests
        [Test]
        [TestCaseSource("CanChangeTerrainOfCellTestCases")]
        public bool CanChangeTerrainOfCellTests(CanChangeTerrainOfCellTestData testData) {
            var cellToTest = BuildCell(testData.Cell);

            var modLogic = Container.Resolve<CellModificationLogic>();

            return modLogic.CanChangeTerrainOfCell(cellToTest, testData.Terrain);
        }

        [Test]
        public void ChangeTerrainOfCell_TerrainSet() {
            var cellToTest = BuildCell();

            var modLogic = Container.Resolve<CellModificationLogic>();

            modLogic.ChangeTerrainOfCell(cellToTest, CellTerrain.Tundra);

            Assert.AreEqual(CellTerrain.Tundra, cellToTest.Terrain);
        }

        [Test]
        public void ChangeTerrainOfCell_FoundationElevationReset() {
            var cellToTest = BuildCell();

            MockConfig.Setup(config => config.GetFoundationElevationForTerrain(CellTerrain.ShallowWater)).Returns(-5);

            var modLogic = Container.Resolve<CellModificationLogic>();

            modLogic.ChangeTerrainOfCell(cellToTest, CellTerrain.ShallowWater);

            Assert.AreEqual(-5, cellToTest.FoundationElevation);
        }

        [Test]
        public void ChangeTerrainOfCell_MarshRemovedIfNewTerrainNotGrassland() {
            var cellToTest = BuildCell(vegetation: CellVegetation.Marsh);

            var modLogic = Container.Resolve<CellModificationLogic>();

            modLogic.ChangeTerrainOfCell(cellToTest, CellTerrain.Plains);

            Assert.AreEqual(CellVegetation.None, cellToTest.Vegetation);
        }

        [Test]
        public void ChangeTerrainOfCell_ForestRemovedIfNewTerrainSnow() {
            var cellToTest = BuildCell(vegetation: CellVegetation.Forest);

            var modLogic = Container.Resolve<CellModificationLogic>();

            modLogic.ChangeTerrainOfCell(cellToTest, CellTerrain.Snow);

            Assert.AreEqual(CellVegetation.None, cellToTest.Vegetation);
        }

        [Test]
        public void ChangeTerrainOfCell_ForestRemovedIfNewTerrainDesert() {
            var cellToTest = BuildCell(vegetation: CellVegetation.Forest);

            var modLogic = Container.Resolve<CellModificationLogic>();

            modLogic.ChangeTerrainOfCell(cellToTest, CellTerrain.Desert);

            Assert.AreEqual(CellVegetation.None, cellToTest.Vegetation);
        }

        [Test]
        public void ChangeTerrainOfCell_ForestRemovedIfNewTerrainFloodPlains() {
            var cellToTest = BuildCell(
                vegetation: CellVegetation.Forest, terrain: CellTerrain.Desert, hasRiver: true
            );

            var modLogic = Container.Resolve<CellModificationLogic>();

            modLogic.ChangeTerrainOfCell(cellToTest, CellTerrain.FloodPlains);

            Assert.AreEqual(CellVegetation.None, cellToTest.Vegetation);
        }

        [Test]
        public void ChangeTerrainOfCell_ForesMaintainedIfNewTerrainTundra() {
            var cellToTest = BuildCell(vegetation: CellVegetation.Forest);

            var modLogic = Container.Resolve<CellModificationLogic>();

            modLogic.ChangeTerrainOfCell(cellToTest, CellTerrain.Tundra);

            Assert.AreEqual(CellVegetation.Forest, cellToTest.Vegetation);
        }

        [Test]
        public void ChangeTerrainOfCell_JungleRemovedIfNewTerrainSnow() {
            var cellToTest = BuildCell(vegetation: CellVegetation.Jungle);

            var modLogic = Container.Resolve<CellModificationLogic>();

            modLogic.ChangeTerrainOfCell(cellToTest, CellTerrain.Snow);

            Assert.AreEqual(CellVegetation.None, cellToTest.Vegetation);
        }

        [Test]
        public void ChangeTerrainOfCell_JungleRemovedIfNewTerrainDesert() {
            var cellToTest = BuildCell(vegetation: CellVegetation.Jungle);

            var modLogic = Container.Resolve<CellModificationLogic>();

            modLogic.ChangeTerrainOfCell(cellToTest, CellTerrain.Desert);

            Assert.AreEqual(CellVegetation.None, cellToTest.Vegetation);
        }

        [Test]
        public void ChangeTerrainOfCell_JungleRemovedIfNewTerrainTundra() {
            var cellToTest = BuildCell(vegetation: CellVegetation.Jungle);

            var modLogic = Container.Resolve<CellModificationLogic>();

            modLogic.ChangeTerrainOfCell(cellToTest, CellTerrain.Tundra);

            Assert.AreEqual(CellVegetation.None, cellToTest.Vegetation);
        }

        [Test]
        public void ChangeTerrainOfCell_VegetationRemovedIfNewTerrainUnderwater() {
            var cellToTest = BuildCell(vegetation: CellVegetation.Jungle);

            var modLogic = Container.Resolve<CellModificationLogic>();

            modLogic.ChangeTerrainOfCell(cellToTest, CellTerrain.ShallowWater);

            Assert.AreEqual(CellVegetation.None, cellToTest.Vegetation);
        }

        [Test]
        public void ChangeTerrainOfCell_ShapeFlattenedIfNewTerrainUnderwater() {
            var cellToTest = BuildCell(shape: CellShape.Mountains);

            var modLogic = Container.Resolve<CellModificationLogic>();

            modLogic.ChangeTerrainOfCell(cellToTest, CellTerrain.ShallowWater);

            Assert.AreEqual(CellShape.Flatlands, cellToTest.Shape);
        }

        [Test]
        public void ChangeTerrainOfCell_RiversRemovedIfNewTerrainUnderwater() {
            var cellToTest = BuildCell();

            var modLogic = Container.Resolve<CellModificationLogic>();

            modLogic.ChangeTerrainOfCell(cellToTest, CellTerrain.ShallowWater);

            MockRiverCanon.Verify(canon => canon.RemoveAllRiversFromCell(cellToTest), Times.Once);
        }

        [Test]
        public void ChangeTerrainOfCell_RoadsRemovedIfNewTerrainUnderwater() {
            var cellToTest = BuildCell(hasRoads: true);

            var modLogic = Container.Resolve<CellModificationLogic>();

            modLogic.ChangeTerrainOfCell(cellToTest, CellTerrain.ShallowWater);

            Assert.IsFalse(cellToTest.HasRoads);
        }

        #endregion

        #region Shape tests
        [Test]
        public void CanChangeShapeOfCellTests() {
            Assert.Ignore("CanChangeShapeOfCell always return true, which is considered a case too trivial to test");
        }

        [Test]
        public void ChangeShapeOfCell_ShapeChanged() {
            var cellToTest = BuildCell();

            var modLogic = Container.Resolve<CellModificationLogic>();

            modLogic.ChangeShapeOfCell(cellToTest, CellShape.Hills);

            Assert.AreEqual(CellShape.Hills, cellToTest.Shape);
        }

        [Test]
        public void ChangeShapeOfCell_HillsRemoveMarsh() {
            var cellToTest = BuildCell(vegetation: CellVegetation.Marsh);

            var modLogic = Container.Resolve<CellModificationLogic>();

            modLogic.ChangeShapeOfCell(cellToTest, CellShape.Hills);

            Assert.AreEqual(CellVegetation.None, cellToTest.Vegetation);
        }

        [Test]
        public void ChangeShapeOfCell_HillsRemoveWater() {
            var cellToTest = BuildCell(terrain: CellTerrain.ShallowWater);

            var modLogic = Container.Resolve<CellModificationLogic>();

            modLogic.ChangeShapeOfCell(cellToTest, CellShape.Hills);

            Assert.AreEqual(CellTerrain.Grassland, cellToTest.Terrain);
        }

        [Test]
        public void ChangeShapeOfCell_HillsRemoveFloodPlains() {
            var cellToTest = BuildCell(terrain: CellTerrain.FloodPlains);

            var modLogic = Container.Resolve<CellModificationLogic>();

            modLogic.ChangeShapeOfCell(cellToTest, CellShape.Hills);

            Assert.AreEqual(CellTerrain.Desert, cellToTest.Terrain);
        }

        [Test]
        public void ChangeShapeOfCell_MountainsRemoveMarsh() {
            var cellToTest = BuildCell(vegetation: CellVegetation.Marsh);

            var modLogic = Container.Resolve<CellModificationLogic>();

            modLogic.ChangeShapeOfCell(cellToTest, CellShape.Mountains);

            Assert.AreEqual(CellVegetation.None, cellToTest.Vegetation);
        }

        [Test]
        public void ChangeShapeOfCell_MountainsRemoveForest() {
            var cellToTest = BuildCell(vegetation: CellVegetation.Forest);

            var modLogic = Container.Resolve<CellModificationLogic>();

            modLogic.ChangeShapeOfCell(cellToTest, CellShape.Mountains);

            Assert.AreEqual(CellVegetation.None, cellToTest.Vegetation);
        }

        [Test]
        public void ChangeShapeOfCell_MountainsRemoveJungle() {
            var cellToTest = BuildCell(vegetation: CellVegetation.Jungle);

            var modLogic = Container.Resolve<CellModificationLogic>();

            modLogic.ChangeShapeOfCell(cellToTest, CellShape.Mountains);

            Assert.AreEqual(CellVegetation.None, cellToTest.Vegetation);
        }

        [Test]
        public void ChangeShapeOfCell_MountainsRemoveWater() {
            var cellToTest = BuildCell(terrain: CellTerrain.ShallowWater);

            var modLogic = Container.Resolve<CellModificationLogic>();

            modLogic.ChangeShapeOfCell(cellToTest, CellShape.Mountains);

            Assert.AreEqual(CellTerrain.Grassland, cellToTest.Terrain);
        }

        [Test]
        public void ChangeShapeOfCell_MountainsRemoveRoads() {
            var cellToTest = BuildCell(hasRoads: true);

            var modLogic = Container.Resolve<CellModificationLogic>();

            modLogic.ChangeShapeOfCell(cellToTest, CellShape.Mountains);

            Assert.IsFalse(cellToTest.HasRoads);
        }

        #endregion

        #region Vegetation tests
        [Test]
        [TestCaseSource("CanChangeVegetationOfCellTestCases")]
        public bool CanChangeVegetationOfCellTests(CanChangeVegetationOfCellTestData testData) {
            var cellToTest = BuildCell(testData.Cell);

            var modLogic = Container.Resolve<CellModificationLogic>();

            return modLogic.CanChangeVegetationOfCell(cellToTest, testData.Vegetation);
        }

        [Test]
        public void ChangeVegetationOfCell_VegetationChanged() {
            var cellToTest = BuildCell();

            var modLogic = Container.Resolve<CellModificationLogic>();

            modLogic.ChangeVegetationOfCell(cellToTest, CellVegetation.Jungle);

            Assert.AreEqual(CellVegetation.Jungle, cellToTest.Vegetation);
        }

        [Test]
        public void ChangeVegetationOfCell_MarshTurnsTerrainToGrassland() {
            var cellToTest = BuildCell(terrain: CellTerrain.DeepWater);

            var modLogic = Container.Resolve<CellModificationLogic>();

            modLogic.ChangeVegetationOfCell(cellToTest, CellVegetation.Marsh);

            Assert.AreEqual(CellTerrain.Grassland, cellToTest.Terrain);
        }

        [Test]
        public void ChangeVegetationOfCell_MarshTurnsShapeToFlatlands() {
            var cellToTest = BuildCell(shape: CellShape.Mountains);

            var modLogic = Container.Resolve<CellModificationLogic>();

            modLogic.ChangeVegetationOfCell(cellToTest, CellVegetation.Marsh);

            Assert.AreEqual(CellShape.Flatlands, cellToTest.Shape);
        }

        [Test]
        public void ChangeVegetationOfCell_ThrowsWhenInvalid() {
            var cellToTest = BuildCell(shape: CellShape.Mountains);

            var modLogic = Container.Resolve<CellModificationLogic>();

            Assert.Throws<InvalidOperationException>(() => modLogic.ChangeVegetationOfCell(cellToTest, CellVegetation.Forest));            
        }

        #endregion

        #region Road tests
        [Test]
        [TestCaseSource("CanChangeHasRoadsOfCellTestCases")]
        public bool CanChangeHasRoadsOfCellTests(CanChangeHasRoadsOfCellTestData testData) {
            var cellToTest = BuildCell(testData.Cell);

            var modLogic = Container.Resolve<CellModificationLogic>();

            return modLogic.CanChangeHasRoadsOfCell(cellToTest, testData.HasRoads);
        }

        [Test]
        public void ChangeHasRoadsOfCell_HasRoadsChanged() {
            var cellToTest = BuildCell();

            var modLogic = Container.Resolve<CellModificationLogic>();

            modLogic.ChangeHasRoadsOfCell(cellToTest, true);

            Assert.IsTrue(cellToTest.HasRoads);
        }

        [Test]
        public void ChangeHasRoadsOfCell_ThrowsWhenInvalid() {
            var cellToTest = BuildCell(terrain: CellTerrain.ShallowWater);

            var modLogic = Container.Resolve<CellModificationLogic>();

            Assert.Throws<InvalidOperationException>(() => modLogic.ChangeHasRoadsOfCell(cellToTest, true));
        }

        #endregion

        #endregion

        #region utilities

        private IHexCell BuildCell(HexCellTestData cellData) {
            return BuildCell(
                cellData.Terrain, cellData.Shape, cellData.Vegetation,
                cellData.HasRoads, cellData.HasRiver
            );
        }

        private IHexCell BuildCell(
            CellTerrain    terrain    = CellTerrain.Grassland,
            CellShape      shape      = CellShape.Flatlands,
            CellVegetation vegetation = CellVegetation.None,
            bool           hasRoads   = false,
            bool           hasRiver   = false
        ) {
            var mockCell = new Mock<IHexCell>();

            mockCell.SetupAllProperties();

            var newCell = mockCell.Object;

            newCell.Terrain    = terrain;
            newCell.Shape      = shape;
            newCell.Vegetation = vegetation;
            newCell.HasRoads   = hasRoads;

            MockRiverCanon.Setup(canon => canon.HasRiver(newCell)).Returns(hasRiver);

            return newCell;
        }

        #endregion

        #endregion

    }
}
