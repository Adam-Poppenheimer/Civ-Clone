using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation.WorkerSlots;
using Assets.Simulation.MapManagement;
using Assets.Simulation.HexMap;

namespace Assets.Tests.Simulation.MapManagement {

    [TestFixture]
    public class HexCellComposerTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IHexGrid>               MockGrid;
        private Mock<IRiverCanon>            MockRiverCanon;
        private Mock<ICellModificationLogic> MockCellModificationLogic;

        private List<IHexCell> AllCells = new List<IHexCell>();

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            AllCells.Clear();

            MockGrid                  = new Mock<IHexGrid>();
            MockRiverCanon            = new Mock<IRiverCanon>();
            MockCellModificationLogic = new Mock<ICellModificationLogic>();

            MockGrid.Setup(grid => grid.Cells).Returns(AllCells.AsReadOnly());

            MockGrid.Setup(grid => grid.GetCellAtCoordinates(It.IsAny<HexCoordinates>()))
                    .Returns<HexCoordinates>(coords => BuildHexCell(coords));

            Container.Bind<IHexGrid>              ().FromInstance(MockGrid                 .Object);
            Container.Bind<IRiverCanon>           ().FromInstance(MockRiverCanon           .Object);
            Container.Bind<ICellModificationLogic>().FromInstance(MockCellModificationLogic.Object);

            Container.Bind<HexCellComposer>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void ClearRuntime_GridCleared() {
            var composer = Container.Resolve<HexCellComposer>();

            composer.ClearRuntime();

            MockGrid.Verify(grid => grid.Clear(), Times.Once);
        }

        [Test]
        public void ComposeCells_ChunkCountRecorded() {
            MockGrid.Setup(grid => grid.ChunkCountX).Returns(10);
            MockGrid.Setup(grid => grid.ChunkCountZ).Returns(7);

            var mapData = new SerializableMapData();

            var composer = Container.Resolve<HexCellComposer>();

            composer.ComposeCells(mapData);

            Assert.AreEqual(10, mapData.ChunkCountX, "Incorrect ChunkCountX");
            Assert.AreEqual(7,  mapData.ChunkCountZ, "Incorrect ChunkCountZ");
        }

        [Test]
        public void ComposeCells_CoordinatesRecorded() {
            var cellOne   = BuildHexCell(new HexCoordinates(0, 1));
            var cellTwo   = BuildHexCell(new HexCoordinates(2, 3));
            var cellThree = BuildHexCell(new HexCoordinates(4, 5));

            var composer = Container.Resolve<HexCellComposer>();

            var mapData = new SerializableMapData();

            composer.ComposeCells(mapData);

            var dataLikeCellOne   = mapData.HexCells.Where(data => data.Coordinates == cellOne  .Coordinates);
            var dataLikeCellTwo   = mapData.HexCells.Where(data => data.Coordinates == cellTwo  .Coordinates);
            var dataLikeCellThree = mapData.HexCells.Where(data => data.Coordinates == cellThree.Coordinates);

            Assert.AreEqual(1, dataLikeCellOne.Count(),   "An unexpected number of cell data had CellOne's coordinates");
            Assert.AreEqual(1, dataLikeCellTwo.Count(),   "An unexpected number of cell data had CellTwo's coordinates");
            Assert.AreEqual(1, dataLikeCellThree.Count(), "An unexpected number of cell data had CellThree's coordinates");
        }

        [Test]
        public void ComposeCells_TerrainRecorded() {
            var cellOne   = BuildHexCell(new HexCoordinates(0, 1));
            var cellTwo   = BuildHexCell(new HexCoordinates(2, 3));
            var cellThree = BuildHexCell(new HexCoordinates(4, 5));

            cellOne  .Terrain = CellTerrain.Grassland;
            cellTwo  .Terrain = CellTerrain.Desert;
            cellThree.Terrain = CellTerrain.Snow;

            var composer = Container.Resolve<HexCellComposer>();

            var mapData = new SerializableMapData();

            composer.ComposeCells(mapData);

            var dataLikeCellOne = mapData.HexCells.Where(
                data => data.Coordinates == cellOne.Coordinates
                     && data.Terrain     == cellOne.Terrain
            );

            var dataLikeCellTwo = mapData.HexCells.Where(
                data => data.Coordinates == cellTwo.Coordinates
                     && data.Terrain     == cellTwo.Terrain
            );

            var dataLikeCellThree = mapData.HexCells.Where(
                data => data.Coordinates == cellThree.Coordinates
                     && data.Terrain     == cellThree.Terrain
            );

            Assert.AreEqual(1, dataLikeCellOne.Count(),   "Unexpected number of data representing CellOne");
            Assert.AreEqual(1, dataLikeCellTwo.Count(),   "Unexpected number of data representing CellTwo");
            Assert.AreEqual(1, dataLikeCellThree.Count(), "Unexpected number of data representing CellThree");
        }

        [Test]
        public void ComposeCells_ShapeRecorded() {
            var cellOne   = BuildHexCell(new HexCoordinates(0, 1));
            var cellTwo   = BuildHexCell(new HexCoordinates(2, 3));
            var cellThree = BuildHexCell(new HexCoordinates(4, 5));

            cellOne  .Shape = CellShape.Mountains;
            cellTwo  .Shape = CellShape.Flatlands;
            cellThree.Shape = CellShape.Hills;

            var composer = Container.Resolve<HexCellComposer>();

            var mapData = new SerializableMapData();

            composer.ComposeCells(mapData);

            var dataLikeCellOne = mapData.HexCells.Where(
                data => data.Coordinates == cellOne.Coordinates
                     && data.Shape       == cellOne.Shape
            );

            var dataLikeCellTwo = mapData.HexCells.Where(
                data => data.Coordinates == cellTwo.Coordinates
                     && data.Shape       == cellTwo.Shape
            );

            var dataLikeCellThree = mapData.HexCells.Where(
                data => data.Coordinates == cellThree.Coordinates
                     && data.Shape       == cellThree.Shape
            );

            Assert.AreEqual(1, dataLikeCellOne.Count(),   "Unexpected number of data representing CellOne");
            Assert.AreEqual(1, dataLikeCellTwo.Count(),   "Unexpected number of data representing CellTwo");
            Assert.AreEqual(1, dataLikeCellThree.Count(), "Unexpected number of data representing CellThree");
        }

        [Test]
        public void ComposeCells_VegetationRecorded() {
            var cellOne   = BuildHexCell(new HexCoordinates(0, 1));
            var cellTwo   = BuildHexCell(new HexCoordinates(2, 3));
            var cellThree = BuildHexCell(new HexCoordinates(4, 5));

            cellOne  .Vegetation = CellVegetation.Forest;
            cellTwo  .Vegetation = CellVegetation.Jungle;
            cellThree.Vegetation = CellVegetation.Forest;

            var composer = Container.Resolve<HexCellComposer>();

            var mapData = new SerializableMapData();

            composer.ComposeCells(mapData);

            var dataLikeCellOne = mapData.HexCells.Where(
                data => data.Coordinates == cellOne.Coordinates
                     && data.Vegetation  == cellOne.Vegetation
            );

            var dataLikeCellTwo = mapData.HexCells.Where(
                data => data.Coordinates == cellTwo.Coordinates
                     && data.Vegetation  == cellTwo.Vegetation
            );

            var dataLikeCellThree = mapData.HexCells.Where(
                data => data.Coordinates == cellThree.Coordinates
                     && data.Vegetation  == cellThree.Vegetation
            );

            Assert.AreEqual(1, dataLikeCellOne.Count(),   "Unexpected number of data representing CellOne");
            Assert.AreEqual(1, dataLikeCellTwo.Count(),   "Unexpected number of data representing CellTwo");
            Assert.AreEqual(1, dataLikeCellThree.Count(), "Unexpected number of data representing CellThree");
        }

        [Test]
        public void ComposeCells_FeatureRecorded() {
            var cellOne   = BuildHexCell(new HexCoordinates(0, 1));
            var cellTwo   = BuildHexCell(new HexCoordinates(2, 3));
            var cellThree = BuildHexCell(new HexCoordinates(4, 5));

            cellOne  .Feature = CellFeature.Oasis;
            cellTwo  .Feature = CellFeature.None;
            cellThree.Feature = CellFeature.Oasis;

            var composer = Container.Resolve<HexCellComposer>();

            var mapData = new SerializableMapData();

            composer.ComposeCells(mapData);

            var dataLikeCellOne = mapData.HexCells.Where(
                data => data.Coordinates == cellOne.Coordinates
                     && data.Feature     == cellOne.Feature
            );

            var dataLikeCellTwo = mapData.HexCells.Where(
                data => data.Coordinates == cellTwo.Coordinates
                     && data.Feature     == cellTwo.Feature
            );

            var dataLikeCellThree = mapData.HexCells.Where(
                data => data.Coordinates == cellThree.Coordinates
                     && data.Feature     == cellThree.Feature
            );

            Assert.AreEqual(1, dataLikeCellOne.Count(),   "Unexpected number of data representing CellOne");
            Assert.AreEqual(1, dataLikeCellTwo.Count(),   "Unexpected number of data representing CellTwo");
            Assert.AreEqual(1, dataLikeCellThree.Count(), "Unexpected number of data representing CellThree");
        }

        [Test]
        public void ComposeCells_SuprpessSlotRecorded() {
            var cellOne   = BuildHexCell(new HexCoordinates(0, 1));
            var cellTwo   = BuildHexCell(new HexCoordinates(2, 3));
            var cellThree = BuildHexCell(new HexCoordinates(4, 5));

            cellOne  .SuppressSlot = true;
            cellTwo  .SuppressSlot = false;
            cellThree.SuppressSlot = false;

            var composer = Container.Resolve<HexCellComposer>();

            var mapData = new SerializableMapData();

            composer.ComposeCells(mapData);

            var dataLikeCellOne = mapData.HexCells.Where(
                data => data.Coordinates  == cellOne.Coordinates
                     && data.SuppressSlot == cellOne.SuppressSlot
            );

            var dataLikeCellTwo = mapData.HexCells.Where(
                data => data.Coordinates  == cellTwo.Coordinates
                     && data.SuppressSlot == cellTwo.SuppressSlot
            );

            var dataLikeCellThree = mapData.HexCells.Where(
                data => data.Coordinates  == cellThree.Coordinates
                     && data.SuppressSlot == cellThree.SuppressSlot
            );

            Assert.AreEqual(1, dataLikeCellOne.Count(),   "Unexpected number of data representing CellOne");
            Assert.AreEqual(1, dataLikeCellTwo.Count(),   "Unexpected number of data representing CellTwo");
            Assert.AreEqual(1, dataLikeCellThree.Count(), "Unexpected number of data representing CellThree");
        }

        [Test]
        public void ComposeCells_HasRoadsRecorded() {
            var cellOne   = BuildHexCell(new HexCoordinates(0, 1));
            var cellTwo   = BuildHexCell(new HexCoordinates(2, 3));
            var cellThree = BuildHexCell(new HexCoordinates(4, 5));

            cellOne  .HasRoads = true;
            cellTwo  .HasRoads = false;
            cellThree.HasRoads = false;

            var composer = Container.Resolve<HexCellComposer>();

            var mapData = new SerializableMapData();

            composer.ComposeCells(mapData);

            var dataLikeCellOne = mapData.HexCells.Where(
                data => data.Coordinates == cellOne.Coordinates
                     && data.HasRoads    == cellOne.HasRoads
            );

            var dataLikeCellTwo = mapData.HexCells.Where(
                data => data.Coordinates == cellTwo.Coordinates
                     && data.HasRoads    == cellTwo.HasRoads
            );

            var dataLikeCellThree = mapData.HexCells.Where(
                data => data.Coordinates == cellThree.Coordinates
                     && data.HasRoads    == cellThree.HasRoads
            );

            Assert.AreEqual(1, dataLikeCellOne.Count(),   "Unexpected number of data representing CellOne");
            Assert.AreEqual(1, dataLikeCellTwo.Count(),   "Unexpected number of data representing CellTwo");
            Assert.AreEqual(1, dataLikeCellThree.Count(), "Unexpected number of data representing CellThree");
        }

        [Test(Description =
            "River data should only be stored along the NE, E, and SE directions of the cell. " +
            "Other directions will be covered by the cell's neighbors."
        )]
        public void ComposeCells_RiverDataRecorded() {
            var cellOne = BuildHexCell(new HexCoordinates(0, 0));

            MockRiverCanon.Setup(canon => canon.GetEdgesWithRivers(cellOne))
                          .Returns(new List<HexDirection>() { HexDirection.E, HexDirection.W, HexDirection.SE });

            MockRiverCanon.Setup(canon => canon.GetFlowOfRiverAtEdge(cellOne, HexDirection.E)) .Returns(RiverFlow.Clockwise);
            MockRiverCanon.Setup(canon => canon.GetFlowOfRiverAtEdge(cellOne, HexDirection.W)) .Returns(RiverFlow.Counterclockwise);
            MockRiverCanon.Setup(canon => canon.GetFlowOfRiverAtEdge(cellOne, HexDirection.SE)).Returns(RiverFlow.Counterclockwise);

            var composer = Container.Resolve<HexCellComposer>();

            var mapData = new SerializableMapData();

            composer.ComposeCells(mapData);

            CollectionAssert.AreEqual(
                new List<bool>() { false, true, true, false, false, false },
                mapData.HexCells[0].HasRiverAtEdge,
                "HexCells[0].HasRiverAtEdge not populated as expected"
            );

            Assert.AreEqual(
                RiverFlow.Clockwise, mapData.HexCells[0].DirectionOfRiverAtEdge[1],
                "Unexpected flow for river along HexDirection.E"
            );

            Assert.AreEqual(
                RiverFlow.Counterclockwise, mapData.HexCells[0].DirectionOfRiverAtEdge[2],
                "Unexpected flow for river along HexDirection.SE"
            );
        }

        [Test]
        public void ComposeCells_SlotDataRecorded() {
            var cellOne   = BuildHexCell(new HexCoordinates(0, 1));
            var cellTwo   = BuildHexCell(new HexCoordinates(2, 3));
            var cellThree = BuildHexCell(new HexCoordinates(4, 5));

            cellOne  .WorkerSlot.IsLocked   = true;
            cellOne  .WorkerSlot.IsOccupied = true;

            cellTwo  .WorkerSlot.IsLocked   = false;
            cellTwo  .WorkerSlot.IsOccupied = true;

            cellThree.WorkerSlot.IsLocked   = true;
            cellThree.WorkerSlot.IsOccupied = false;

            var composer = Container.Resolve<HexCellComposer>();

            var mapData = new SerializableMapData();

            composer.ComposeCells(mapData);

            var dataLikeCellOne = mapData.HexCells.Where(
                data => data.Coordinates    == cellOne.Coordinates
                     && data.IsSlotLocked   == cellOne.WorkerSlot.IsLocked
                     && data.IsSlotOccupied == cellOne.WorkerSlot.IsOccupied
            );

            var dataLikeCellTwo = mapData.HexCells.Where(
                data => data.Coordinates    == cellTwo.Coordinates
                     && data.IsSlotLocked   == cellTwo.WorkerSlot.IsLocked
                     && data.IsSlotOccupied == cellTwo.WorkerSlot.IsOccupied
            );

            var dataLikeCellThree = mapData.HexCells.Where(
                data => data.Coordinates    == cellThree.Coordinates
                     && data.IsSlotLocked   == cellThree.WorkerSlot.IsLocked
                     && data.IsSlotOccupied == cellThree.WorkerSlot.IsOccupied
            );

            Assert.AreEqual(1, dataLikeCellOne.Count(),   "Unexpected number of data representing CellOne");
            Assert.AreEqual(1, dataLikeCellTwo.Count(),   "Unexpected number of data representing CellTwo");
            Assert.AreEqual(1, dataLikeCellThree.Count(), "Unexpected number of data representing CellThree");
        }




        [Test]
        public void DecomposeCells_GridBuiltWithProperChunkCount() {
            var mapData = new SerializableMapData() {
                ChunkCountX = 10, ChunkCountZ = 7,
                HexCells = new List<SerializableHexCellData>()
            };

            var composer = Container.Resolve<HexCellComposer>();

            composer.DecomposeCells(mapData);

            MockGrid.Verify(grid => grid.Build(10, 7));
        }

        [Test]
        public void DecomposeCells_TerrainChangedThroughCellModificationLogic() {
            var mapData = new SerializableMapData() {
                HexCells = new List<SerializableHexCellData>() {
                    new SerializableHexCellData() {
                        Coordinates = new HexCoordinates(0, 0), Terrain = CellTerrain.Desert
                    },
                    new SerializableHexCellData() {
                        Coordinates = new HexCoordinates(1, 1), Terrain = CellTerrain.Tundra
                    },
                }
            };

            var composer = Container.Resolve<HexCellComposer>();

            composer.DecomposeCells(mapData);

            var cellOne = AllCells.Where(cell => cell.Coordinates.Equals(new HexCoordinates(0, 0))).First();
            var cellTwo = AllCells.Where(cell => cell.Coordinates.Equals(new HexCoordinates(1, 1))).First();

            MockCellModificationLogic.Verify(
                logic => logic.ChangeTerrainOfCell(cellOne, CellTerrain.Desert), Times.Once,
                "ChangeTerrainOfCellTo not called properly on cellOne"
            );

            MockCellModificationLogic.Verify(
                logic => logic.ChangeTerrainOfCell(cellTwo, CellTerrain.Tundra), Times.Once,
                "ChangeTerrainOfCellTo not called properly on cellTwo"
            );
        }

        [Test]
        public void DecomposeCells_ShapeChangedThroughCellModificationLogic() {
            var mapData = new SerializableMapData() {
                HexCells = new List<SerializableHexCellData>() {
                    new SerializableHexCellData() {
                        Coordinates = new HexCoordinates(0, 0),Shape = CellShape.Mountains
                    },
                    new SerializableHexCellData() {
                        Coordinates = new HexCoordinates(1, 1), Shape = CellShape.Flatlands
                    },
                }
            };

            var composer = Container.Resolve<HexCellComposer>();

            composer.DecomposeCells(mapData);

            var cellOne = AllCells.Where(cell => cell.Coordinates.Equals(new HexCoordinates(0, 0))).First();
            var cellTwo = AllCells.Where(cell => cell.Coordinates.Equals(new HexCoordinates(1, 1))).First();

            MockCellModificationLogic.Verify(
                logic => logic.ChangeShapeOfCell(cellOne, CellShape.Mountains), Times.Once,
                "ChangeShapeOfCellTo not called properly on cellOne"
            );

            MockCellModificationLogic.Verify(
                logic => logic.ChangeShapeOfCell(cellTwo, CellShape.Flatlands), Times.Once,
                "ChangeShapeOfCellTo not called properly on cellTwo"
            );
        }

        [Test]
        public void DecomposeCells_VegetationChangedThroughCellModificationLogic() {
            var mapData = new SerializableMapData() {
                HexCells = new List<SerializableHexCellData>() {
                    new SerializableHexCellData() {
                        Coordinates = new HexCoordinates(0, 0), Vegetation = CellVegetation.Forest
                    },
                    new SerializableHexCellData() {
                        Coordinates = new HexCoordinates(1, 1), Vegetation = CellVegetation.Marsh
                    },
                }
            };

            var composer = Container.Resolve<HexCellComposer>();

            composer.DecomposeCells(mapData);

            var cellOne = AllCells.Where(cell => cell.Coordinates.Equals(new HexCoordinates(0, 0))).First();
            var cellTwo = AllCells.Where(cell => cell.Coordinates.Equals(new HexCoordinates(1, 1))).First();

            MockCellModificationLogic.Verify(
                logic => logic.ChangeVegetationOfCell(cellOne, CellVegetation.Forest), Times.Once,
                "ChangeVegetationOfCellTo not called properly on cellOne"
            );

            MockCellModificationLogic.Verify(
                logic => logic.ChangeVegetationOfCell(cellTwo, CellVegetation.Marsh), Times.Once,
                "ChangeVegetationOfCellTo not called properly on cellTwo"
            );
        }

        [Test]
        public void DecomposeCells_FeatureChangedThroughCellModificationLogic() {
            var mapData = new SerializableMapData() {
                HexCells = new List<SerializableHexCellData>() {
                    new SerializableHexCellData() {
                        Coordinates = new HexCoordinates(0, 0), Feature = CellFeature.Oasis
                    },
                    new SerializableHexCellData() {
                        Coordinates = new HexCoordinates(1, 1), Feature = CellFeature.None
                    },
                }
            };

            var composer = Container.Resolve<HexCellComposer>();

            composer.DecomposeCells(mapData);

            var cellOne = AllCells.Where(cell => cell.Coordinates.Equals(new HexCoordinates(0, 0))).First();
            var cellTwo = AllCells.Where(cell => cell.Coordinates.Equals(new HexCoordinates(1, 1))).First();

            MockCellModificationLogic.Verify(
                logic => logic.ChangeFeatureOfCell(cellOne, CellFeature.Oasis), Times.Once,
                "ChangeFeatureOfCell not called properly on cellOne"
            );

            MockCellModificationLogic.Verify(
                logic => logic.ChangeFeatureOfCell(cellTwo, CellFeature.None), Times.Once,
                "ChangeFeatureOfCell not called properly on cellTwo"
            );
        }

        [Test]
        public void DecomposeCells_SuppressSlotSetProperly() {
            var mapData = new SerializableMapData() {
                HexCells = new List<SerializableHexCellData>() {
                    new SerializableHexCellData() {
                        Coordinates = new HexCoordinates(0, 0), SuppressSlot = false
                    },
                    new SerializableHexCellData() {
                        Coordinates = new HexCoordinates(1, 1), SuppressSlot = true
                    },
                }
            };

            var composer = Container.Resolve<HexCellComposer>();

            composer.DecomposeCells(mapData);

            var cellOne = AllCells.Where(cell => cell.Coordinates.Equals(new HexCoordinates(0, 0))).First();
            var cellTwo = AllCells.Where(cell => cell.Coordinates.Equals(new HexCoordinates(1, 1))).First();

            Assert.AreEqual(false, cellOne.SuppressSlot, "CellOne has an unexpected SuppressSlot value");
            Assert.AreEqual(true,  cellTwo.SuppressSlot, "CellTwo has an unexpected SuppressSlot value");
        }

        [Test]
        public void DecomposeCells_HasRoadsChangedThroughCellModificationLogic () {
            var mapData = new SerializableMapData() {
                HexCells = new List<SerializableHexCellData>() {
                    new SerializableHexCellData() {
                        Coordinates = new HexCoordinates(0, 0), HasRoads = false
                    },
                    new SerializableHexCellData() {
                        Coordinates = new HexCoordinates(1, 1), HasRoads = true
                    },
                }
            };

            var composer = Container.Resolve<HexCellComposer>();

            composer.DecomposeCells(mapData);

            var cellOne = AllCells.Where(cell => cell.Coordinates.Equals(new HexCoordinates(0, 0))).First();
            var cellTwo = AllCells.Where(cell => cell.Coordinates.Equals(new HexCoordinates(1, 1))).First();

            MockCellModificationLogic.Verify(
                logic => logic.ChangeHasRoadsOfCell(cellOne, false), Times.Once,
                "ChangeHasRoadsOfCellTo not called properly on cellOne"
            );

            MockCellModificationLogic.Verify(
                logic => logic.ChangeHasRoadsOfCell(cellTwo, true), Times.Once,
                "ChangeHasRoadsOfCellTo not called properly on cellTwo"
            );
        }

        [Test]
        public void DecomposeCells_RiverDataSetProperly() {
            var mapData = new SerializableMapData() {
                HexCells = new List<SerializableHexCellData>() {
                    new SerializableHexCellData() {
                        Coordinates = new HexCoordinates(0, 0),
                        HasRiverAtEdge = new List<bool>() { true, false, false, false, false, false },
                        DirectionOfRiverAtEdge = new List<RiverFlow> { RiverFlow.Counterclockwise }
                    },
                    new SerializableHexCellData() {
                        Coordinates = new HexCoordinates(1, 1),
                        HasRiverAtEdge = new List<bool> { true, false, true, false, false, false },
                        DirectionOfRiverAtEdge = new List<RiverFlow> {
                            RiverFlow.Counterclockwise, RiverFlow.Counterclockwise, RiverFlow.Clockwise
                        }
                    }
                }
            };

            MockRiverCanon.Setup(canon => canon.CanAddRiverToCell(It.IsAny<IHexCell>(), HexDirection.NE, RiverFlow.Counterclockwise)).Returns(true);

            MockRiverCanon.SetupSequence(canon => canon.CanAddRiverToCell(It.IsAny<IHexCell>(), HexDirection.SE, RiverFlow.Clockwise))
                          .Returns(false)
                          .Returns(true);

            var composer = Container.Resolve<HexCellComposer>();

            composer.DecomposeCells(mapData);

            var cellOne = AllCells.Where(cell => cell.Coordinates.Equals(new HexCoordinates(0, 0))).First();
            var cellTwo = AllCells.Where(cell => cell.Coordinates.Equals(new HexCoordinates(1, 1))).First();

            MockRiverCanon.Verify(
                canon => canon.AddRiverToCell(It.IsAny<IHexCell>(), It.IsAny<HexDirection>(), It.IsAny<RiverFlow>()),
                Times.Exactly(3), "Unexpected number of calls to RiverCanon.AddRiverToCell"
            );

            MockRiverCanon.Verify(
                canon => canon.AddRiverToCell(cellOne, HexDirection.NE, RiverFlow.Counterclockwise),
                Times.Once, "River not added to cellOne's northeastern edge as expected"
            );

            MockRiverCanon.Verify(
                canon => canon.AddRiverToCell(cellTwo, HexDirection.NE, RiverFlow.Counterclockwise),
                Times.Once, "River not added to cellTwo's northeastern edge as expected"
            );

            MockRiverCanon.Verify(
                canon => canon.AddRiverToCell(cellTwo, HexDirection.SE, RiverFlow.Clockwise),
                Times.Once, "River not added to cellTwo's southeastern edge as expected"
            );
        }

        [Test]
        public void DecomposeCells_SlotDataSetProperly() {
            var mapData = new SerializableMapData() {
                HexCells = new List<SerializableHexCellData>() {
                    new SerializableHexCellData() {
                        Coordinates = new HexCoordinates(0, 0), IsSlotLocked = true,
                        IsSlotOccupied = false
                    },
                    new SerializableHexCellData() {
                        Coordinates = new HexCoordinates(1, 1), IsSlotLocked = false,
                        IsSlotOccupied = true
                    },
                }
            };

            var composer = Container.Resolve<HexCellComposer>();

            composer.DecomposeCells(mapData);

            var cellOne = AllCells.Where(cell => cell.Coordinates.Equals(new HexCoordinates(0, 0))).First();
            var cellTwo = AllCells.Where(cell => cell.Coordinates.Equals(new HexCoordinates(1, 1))).First();

            Assert.AreEqual(true, cellOne.WorkerSlot.IsLocked,    "CellOne.WorkerSlot.IsLocked has an unexpected value");
            Assert.AreEqual(false, cellOne.WorkerSlot.IsOccupied, "CellOne.WorkerSlot.IsOccupied has an unexpected value");

            Assert.AreEqual(false, cellTwo.WorkerSlot.IsLocked,  "CellTwo.WorkerSlot.IsLocked has an unexpected value");
            Assert.AreEqual(true, cellTwo.WorkerSlot.IsOccupied, "CellTwo.WorkerSlot.IsOccupied has an unexpected value");
        }

        #endregion

        #region utilities

        private IHexCell BuildHexCell(HexCoordinates coordinates) {
            var mockCell = new Mock<IHexCell>();

            mockCell.SetupAllProperties();

            var mockSlot = new Mock<IWorkerSlot>();

            mockSlot.SetupAllProperties();

            mockCell.Setup(cell => cell.WorkerSlot) .Returns(mockSlot.Object);
            mockCell.Setup(cell => cell.Coordinates).Returns(coordinates);

            var newCell = mockCell.Object;

            AllCells.Add(newCell);

            return newCell;
        }

        #endregion

        #endregion

    }

}
