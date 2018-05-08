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

        private Mock<IHexGrid>    MockGrid;
        private Mock<IRiverCanon> MockRiverCanon;

        private List<IHexCell> AllCells = new List<IHexCell>();

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            AllCells.Clear();

            MockGrid       = new Mock<IHexGrid>();
            MockRiverCanon = new Mock<IRiverCanon>();

            MockGrid.Setup(grid => grid.AllCells).Returns(AllCells.AsReadOnly());

            MockGrid.Setup(grid => grid.GetCellAtCoordinates(It.IsAny<HexCoordinates>()))
                    .Returns<HexCoordinates>(coords => BuildHexCell(coords));

            Container.Bind<IHexGrid>   ().FromInstance(MockGrid      .Object);
            Container.Bind<IRiverCanon>().FromInstance(MockRiverCanon.Object);

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

            cellOne  .Terrain = TerrainType.Grassland;
            cellTwo  .Terrain = TerrainType.Desert;
            cellThree.Terrain = TerrainType.Snow;

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
        public void ComposeCells_FeatureRecorded() {
            var cellOne   = BuildHexCell(new HexCoordinates(0, 1));
            var cellTwo   = BuildHexCell(new HexCoordinates(2, 3));
            var cellThree = BuildHexCell(new HexCoordinates(4, 5));

            cellOne  .Feature = TerrainFeature.Forest;
            cellTwo  .Feature = TerrainFeature.Jungle;
            cellThree.Feature = TerrainFeature.Forest;

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
        public void ComposeCells_ShapeRecorded() {
            var cellOne   = BuildHexCell(new HexCoordinates(0, 1));
            var cellTwo   = BuildHexCell(new HexCoordinates(2, 3));
            var cellThree = BuildHexCell(new HexCoordinates(4, 5));

            cellOne  .Shape = TerrainShape.Mountains;
            cellTwo  .Shape = TerrainShape.Flatlands;
            cellThree.Shape = TerrainShape.Hills;

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

        [Test]
        public void ComposeCells_RiverDataRecorded() {
            var cellOne   = BuildHexCell(new HexCoordinates(0, 1));
            var cellTwo   = BuildHexCell(new HexCoordinates(2, 3));
            var cellThree = BuildHexCell(new HexCoordinates(4, 5));

            MockRiverCanon.Setup(canon => canon.HasOutgoingRiver(cellOne))  .Returns(true);
            MockRiverCanon.Setup(canon => canon.HasOutgoingRiver(cellTwo))  .Returns(true);
            MockRiverCanon.Setup(canon => canon.HasOutgoingRiver(cellThree)).Returns(false);

            MockRiverCanon.Setup(canon => canon.GetOutgoingRiver(cellOne))  .Returns(HexDirection.E);
            MockRiverCanon.Setup(canon => canon.GetOutgoingRiver(cellTwo))  .Returns(HexDirection.W);
            MockRiverCanon.Setup(canon => canon.GetOutgoingRiver(cellThree)).Returns(HexDirection.SE);

            var composer = Container.Resolve<HexCellComposer>();

            var mapData = new SerializableMapData();

            composer.ComposeCells(mapData);

            var dataLikeCellOne = mapData.HexCells.Where(
                data => data.Coordinates      == cellOne.Coordinates
                     && data.HasOutgoingRiver == true
                     && data.OutgoingRiver    == HexDirection.E
            );

            var dataLikeCellTwo = mapData.HexCells.Where(
                data => data.Coordinates      == cellTwo.Coordinates
                     && data.HasOutgoingRiver == true
                     && data.OutgoingRiver    == HexDirection.W
            );

            var dataLikeCellThree = mapData.HexCells.Where(
                data => data.Coordinates      == cellThree.Coordinates
                     && data.HasOutgoingRiver == false
                     && data.OutgoingRiver    == HexDirection.SE
            );

            Assert.AreEqual(1, dataLikeCellOne.Count(),   "Unexpected number of data representing CellOne");
            Assert.AreEqual(1, dataLikeCellTwo.Count(),   "Unexpected number of data representing CellTwo");
            Assert.AreEqual(1, dataLikeCellThree.Count(), "Unexpected number of data representing CellThree");
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
        public void DecomposeCells_TerrainSetProperly() {
            var mapData = new SerializableMapData() {
                HexCells = new List<SerializableHexCellData>() {
                    new SerializableHexCellData() {
                        Coordinates = new HexCoordinates(0, 0), Terrain = TerrainType.Desert
                    },
                    new SerializableHexCellData() {
                        Coordinates = new HexCoordinates(1, 1), Terrain = TerrainType.Tundra
                    },
                }
            };

            var composer = Container.Resolve<HexCellComposer>();

            composer.DecomposeCells(mapData);

            var cellOne = AllCells.Where(cell => cell.Coordinates.Equals(new HexCoordinates(0, 0))).First();
            var cellTwo = AllCells.Where(cell => cell.Coordinates.Equals(new HexCoordinates(1, 1))).First();

            Assert.AreEqual(TerrainType.Desert, cellOne.Terrain, "CellOne has an unexpected terrain");
            Assert.AreEqual(TerrainType.Tundra, cellTwo.Terrain, "CellTwo has an unexpected terrain");
        }

        [Test]
        public void DecomposeCells_FeatureSetProperly() {
            var mapData = new SerializableMapData() {
                HexCells = new List<SerializableHexCellData>() {
                    new SerializableHexCellData() {
                        Coordinates = new HexCoordinates(0, 0), Feature = TerrainFeature.Forest
                    },
                    new SerializableHexCellData() {
                        Coordinates = new HexCoordinates(1, 1), Feature = TerrainFeature.Marsh
                    },
                }
            };

            var composer = Container.Resolve<HexCellComposer>();

            composer.DecomposeCells(mapData);

            var cellOne = AllCells.Where(cell => cell.Coordinates.Equals(new HexCoordinates(0, 0))).First();
            var cellTwo = AllCells.Where(cell => cell.Coordinates.Equals(new HexCoordinates(1, 1))).First();

            Assert.AreEqual(TerrainFeature.Forest, cellOne.Feature, "CellOne has an unexpected feature");
            Assert.AreEqual(TerrainFeature.Marsh,  cellTwo.Feature, "CellTwo has an unexpected feature");
        }

        [Test]
        public void DecomposeCells_ShapeSetProperly() {
            var mapData = new SerializableMapData() {
                HexCells = new List<SerializableHexCellData>() {
                    new SerializableHexCellData() {
                        Coordinates = new HexCoordinates(0, 0),Shape = TerrainShape.Mountains
                    },
                    new SerializableHexCellData() {
                        Coordinates = new HexCoordinates(1, 1), Shape = TerrainShape.Flatlands
                    },
                }
            };

            var composer = Container.Resolve<HexCellComposer>();

            composer.DecomposeCells(mapData);

            var cellOne = AllCells.Where(cell => cell.Coordinates.Equals(new HexCoordinates(0, 0))).First();
            var cellTwo = AllCells.Where(cell => cell.Coordinates.Equals(new HexCoordinates(1, 1))).First();

            Assert.AreEqual(TerrainShape.Mountains, cellOne.Shape, "CellOne has an unexpected shape");
            Assert.AreEqual(TerrainShape.Flatlands, cellTwo.Shape, "CellTwo has an unexpected shape");
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
        public void DecomposeCells_HasRoadsSetProperly() {
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

            Assert.AreEqual(false, cellOne.HasRoads, "CellOne has an unexpected HasRoads value");
            Assert.AreEqual(true,  cellTwo.HasRoads, "CellTwo has an unexpected HasRoads value");
        }

        [Test]
        public void DecomposeCells_RiverDataSetProperly() {
            var mapData = new SerializableMapData() {
                HexCells = new List<SerializableHexCellData>() {
                    new SerializableHexCellData() {
                        Coordinates = new HexCoordinates(0, 0), HasOutgoingRiver = false,
                        OutgoingRiver = HexDirection.E
                    },
                    new SerializableHexCellData() {
                        Coordinates = new HexCoordinates(1, 1), HasOutgoingRiver = true,
                        OutgoingRiver = HexDirection.SE
                    },
                }
            };

            var composer = Container.Resolve<HexCellComposer>();

            composer.DecomposeCells(mapData);

            var cellOne = AllCells.Where(cell => cell.Coordinates.Equals(new HexCoordinates(0, 0))).First();
            var cellTwo = AllCells.Where(cell => cell.Coordinates.Equals(new HexCoordinates(1, 1))).First();

            MockRiverCanon.Verify(
                canon => canon.SetOutgoingRiver(cellOne, It.IsAny<HexDirection>()),
                Times.Never, "Unexpected call to SetOutgoingRiver on CellOne"
            );

            MockRiverCanon.Verify(
                canon => canon.SetOutgoingRiver(cellTwo, HexDirection.SE),
                Times.Once, "SetOutgoingRiver not called on cellTwo as expected"
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
