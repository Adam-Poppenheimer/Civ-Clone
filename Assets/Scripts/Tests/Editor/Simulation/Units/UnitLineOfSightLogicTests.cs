using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using Zenject;
using Moq;

using Assets.Simulation.Units;
using Assets.Simulation.HexMap;

using UnityCustomUtilities.Extensions;

namespace Assets.Tests.Simulation.Units {

    public class UnitLineOfSightLogicTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IHexGrid>           MockGrid;
        private Mock<IUnitPositionCanon> MockUnitPositionCanon;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockGrid              = new Mock<IHexGrid>();
            MockUnitPositionCanon = new Mock<IUnitPositionCanon>();

            Container.Bind<IHexGrid>          ().FromInstance(MockGrid             .Object);
            Container.Bind<IUnitPositionCanon>().FromInstance(MockUnitPositionCanon.Object);

            Container.Bind<UnitVisibilityLogic>().AsSingle();
        }

        #endregion

        #region test

        [Test]
        public void GetCellsVisibleToUnit_GetsCellsOutToVisionRangeOfUnit() {
            var cellsInRange = new List<IHexCell>() {
                BuildCell(CellTerrain.Grassland, CellShape.Flatlands, CellVegetation.None),
                BuildCell(CellTerrain.Grassland, CellShape.Flatlands, CellVegetation.None),
                BuildCell(CellTerrain.Grassland, CellShape.Flatlands, CellVegetation.None)
            };

            var location = cellsInRange[0];

            BuildLineOfCells(location, cellsInRange[1]);
            BuildLineOfCells(location, cellsInRange[2]);

            var unit = BuildUnit(location, 2);

            MockGrid.Setup(grid => grid.GetCellsInRadius(location, 2)).Returns(cellsInRange);
            MockGrid.Setup(grid => grid.GetNeighbors    (location))   .Returns(new List<IHexCell>());
            MockGrid.Setup(grid => grid.GetCellsInRing  (location, 3)).Returns(new List<IHexCell>());

            var lineOfSightLogic = Container.Resolve<UnitVisibilityLogic>();

            CollectionAssert.AreEquivalent(
                cellsInRange, lineOfSightLogic.GetCellsVisibleToUnit(unit)
            );
        }

        [Test]
        public void GetCellsVisibleToUnit_IncludesWaterJustBeyondVisionRangeOfUnit() {
            var cellsJustBeyondRange = new List<IHexCell>() {
                BuildCell(CellTerrain.ShallowWater, CellShape.Flatlands, CellVegetation.None),
                BuildCell(CellTerrain.ShallowWater, CellShape.Flatlands, CellVegetation.None),
                BuildCell(CellTerrain.ShallowWater, CellShape.Flatlands, CellVegetation.None)
            };

            var location = BuildCell(CellTerrain.Grassland, CellShape.Flatlands, CellVegetation.None);

            BuildLineOfCells(location, cellsJustBeyondRange[0]);
            BuildLineOfCells(location, cellsJustBeyondRange[1]);
            BuildLineOfCells(location, cellsJustBeyondRange[2]);

            var cellsInRange = new List<IHexCell>() { location };

            var unit = BuildUnit(location, 2);

            MockGrid.Setup(grid => grid.GetCellsInRadius(location, 2)).Returns(cellsInRange);
            MockGrid.Setup(grid => grid.GetNeighbors    (location))   .Returns(new List<IHexCell>());
            MockGrid.Setup(grid => grid.GetCellsInRing  (location, 3)).Returns(cellsJustBeyondRange);

            var lineOfSightLogic = Container.Resolve<UnitVisibilityLogic>();

            CollectionAssert.AreEquivalent(
                cellsInRange.Concat(cellsJustBeyondRange), lineOfSightLogic.GetCellsVisibleToUnit(unit)
            );
        }

        [Test]
        public void GetCellsVisibleToUnit_DoesNotIncludeLandJustBeyondVisionRangeOfUnit() {
            var cellsJustBeyondRange = new List<IHexCell>() {
                BuildCell(CellTerrain.Grassland,    CellShape.Flatlands, CellVegetation.None),
                BuildCell(CellTerrain.ShallowWater, CellShape.Flatlands, CellVegetation.None),
                BuildCell(CellTerrain.Grassland,    CellShape.Flatlands, CellVegetation.None)
            };

            var location = BuildCell(CellTerrain.Grassland, CellShape.Flatlands, CellVegetation.None);

            BuildLineOfCells(location, cellsJustBeyondRange[0]);
            BuildLineOfCells(location, cellsJustBeyondRange[1]);
            BuildLineOfCells(location, cellsJustBeyondRange[2]);

            var cellsInRange = new List<IHexCell>() { location };

            var unit = BuildUnit(location, 2);

            MockGrid.Setup(grid => grid.GetCellsInRadius(location, 2)).Returns(cellsInRange);
            MockGrid.Setup(grid => grid.GetNeighbors    (location))   .Returns(new List<IHexCell>());
            MockGrid.Setup(grid => grid.GetCellsInRing  (location, 3)).Returns(cellsJustBeyondRange);

            var lineOfSightLogic = Container.Resolve<UnitVisibilityLogic>();

            CollectionAssert.AreEquivalent(
                new List<IHexCell>() { cellsJustBeyondRange[1], location },
                lineOfSightLogic.GetCellsVisibleToUnit(unit)
            );
        }

        [Test]
        public void GetCellsVisibleToUnit_AndUnitOnFlatlands_ForestsObscureVision() {
            var cellsInRange = new List<IHexCell>() {
                BuildCell(CellTerrain.Grassland, CellShape.Flatlands, CellVegetation.None)
            };

            var location = BuildCell(CellTerrain.Grassland, CellShape.Flatlands, CellVegetation.None);

            BuildLineOfCells(
                location,
                BuildCell(CellTerrain.Grassland, CellShape.Flatlands, CellVegetation.None),
                BuildCell(CellTerrain.Grassland, CellShape.Flatlands, CellVegetation.Forest),
                BuildCell(CellTerrain.Grassland, CellShape.Flatlands, CellVegetation.None),
                cellsInRange[0]
            );

            var unit = BuildUnit(location, 2);

            MockGrid.Setup(grid => grid.GetCellsInRadius(location, 2)).Returns(cellsInRange);
            MockGrid.Setup(grid => grid.GetNeighbors    (location))   .Returns(new List<IHexCell>());
            MockGrid.Setup(grid => grid.GetCellsInRing  (location, 3)).Returns(new List<IHexCell>());

            var lineOfSightLogic = Container.Resolve<UnitVisibilityLogic>();

            CollectionAssert.IsEmpty(lineOfSightLogic.GetCellsVisibleToUnit(unit));
        }

        [Test]
        public void GetCellsVisibleToUnit_AndUnitOnFlatlands_HillsObscureVision() {
            var cellsInRange = new List<IHexCell>() {
                BuildCell(CellTerrain.Grassland, CellShape.Flatlands, CellVegetation.None)
            };

            var location = BuildCell(CellTerrain.Grassland, CellShape.Flatlands, CellVegetation.None);

            BuildLineOfCells(
                location,
                BuildCell(CellTerrain.Grassland, CellShape.Flatlands, CellVegetation.None),
                BuildCell(CellTerrain.Grassland, CellShape.Hills,     CellVegetation.None),
                BuildCell(CellTerrain.Grassland, CellShape.Flatlands, CellVegetation.None),
                cellsInRange[0]
            );

            var unit = BuildUnit(location, 2);

            MockGrid.Setup(grid => grid.GetCellsInRadius(location, 2)).Returns(cellsInRange);
            MockGrid.Setup(grid => grid.GetNeighbors    (location))   .Returns(new List<IHexCell>());
            MockGrid.Setup(grid => grid.GetCellsInRing  (location, 3)).Returns(new List<IHexCell>());

            var lineOfSightLogic = Container.Resolve<UnitVisibilityLogic>();

            CollectionAssert.IsEmpty(lineOfSightLogic.GetCellsVisibleToUnit(unit));
        }

        [Test]
        public void GetCellsVisibleToUnit_AndUnitOnFlatlands_MountainsObscureVision() {
            var cellsInRange = new List<IHexCell>() {
                BuildCell(CellTerrain.Grassland, CellShape.Flatlands, CellVegetation.None)
            };

            var location = BuildCell(CellTerrain.Grassland, CellShape.Flatlands, CellVegetation.None);

            BuildLineOfCells(
                location,
                BuildCell(CellTerrain.Grassland, CellShape.Flatlands, CellVegetation.None),
                BuildCell(CellTerrain.Grassland, CellShape.Mountains, CellVegetation.None),
                BuildCell(CellTerrain.Grassland, CellShape.Flatlands, CellVegetation.None),
                cellsInRange[0]
            );

            var unit = BuildUnit(location, 2);

            MockGrid.Setup(grid => grid.GetCellsInRadius(location, 2)).Returns(cellsInRange);
            MockGrid.Setup(grid => grid.GetNeighbors    (location))   .Returns(new List<IHexCell>());
            MockGrid.Setup(grid => grid.GetCellsInRing  (location, 3)).Returns(new List<IHexCell>());

            var lineOfSightLogic = Container.Resolve<UnitVisibilityLogic>();

            CollectionAssert.IsEmpty(lineOfSightLogic.GetCellsVisibleToUnit(unit));
        }

        [Test]
        public void GetCellsVisibleToUnit_AndUnitOnFlatlands_ForestedHillsObscureVision() {
            var cellsInRange = new List<IHexCell>() {
                BuildCell(CellTerrain.Grassland, CellShape.Flatlands, CellVegetation.None)
            };

            var location = BuildCell(CellTerrain.Grassland, CellShape.Flatlands, CellVegetation.None);

            BuildLineOfCells(
                location,
                BuildCell(CellTerrain.Grassland, CellShape.Flatlands, CellVegetation.None),
                BuildCell(CellTerrain.Grassland, CellShape.Hills,     CellVegetation.Forest),
                BuildCell(CellTerrain.Grassland, CellShape.Flatlands, CellVegetation.None),
                cellsInRange[0]
            );

            var unit = BuildUnit(location, 2);

            MockGrid.Setup(grid => grid.GetCellsInRadius(location, 2)).Returns(cellsInRange);
            MockGrid.Setup(grid => grid.GetNeighbors    (location))   .Returns(new List<IHexCell>());
            MockGrid.Setup(grid => grid.GetCellsInRing  (location, 3)).Returns(new List<IHexCell>());

            var lineOfSightLogic = Container.Resolve<UnitVisibilityLogic>();

            CollectionAssert.IsEmpty(lineOfSightLogic.GetCellsVisibleToUnit(unit));
        }

        [Test]
        public void GetCellsVisibleToUnit_AdjacentHillsAreAlwaysVisible() {
            var cellsInRange = new List<IHexCell>() {
                BuildCell(CellTerrain.Grassland, CellShape.Flatlands, CellVegetation.None)
            };

            var location = BuildCell(CellTerrain.Grassland, CellShape.Flatlands, CellVegetation.None);

            BuildLineOfCells(
                location,
                BuildCell(CellTerrain.Grassland, CellShape.Flatlands, CellVegetation.None),
                BuildCell(CellTerrain.Grassland, CellShape.Hills,     CellVegetation.None),
                BuildCell(CellTerrain.Grassland, CellShape.Flatlands, CellVegetation.None),
                cellsInRange[0]
            );

            var unit = BuildUnit(location, 2);

            MockGrid.Setup(grid => grid.GetCellsInRadius(location, 2)).Returns(cellsInRange);
            MockGrid.Setup(grid => grid.GetNeighbors    (location))   .Returns(cellsInRange);
            MockGrid.Setup(grid => grid.GetCellsInRing  (location, 3)).Returns(new List<IHexCell>());

            var lineOfSightLogic = Container.Resolve<UnitVisibilityLogic>();

            CollectionAssert.AreEquivalent(
                cellsInRange, lineOfSightLogic.GetCellsVisibleToUnit(unit)
            );
        }

        [Test]
        public void GetCellsVisibleToUnit_AdjacentForestsAreAlwaysVisible() {
            var cellsInRange = new List<IHexCell>() {
                BuildCell(CellTerrain.Grassland, CellShape.Flatlands, CellVegetation.None)
            };

            var location = BuildCell(CellTerrain.Grassland, CellShape.Flatlands, CellVegetation.None);

            BuildLineOfCells(
                location,
                BuildCell(CellTerrain.Grassland, CellShape.Flatlands, CellVegetation.None),
                BuildCell(CellTerrain.Grassland, CellShape.Flatlands, CellVegetation.Forest),
                BuildCell(CellTerrain.Grassland, CellShape.Flatlands, CellVegetation.None),
                cellsInRange[0]
            );

            var unit = BuildUnit(location, 2);

            MockGrid.Setup(grid => grid.GetCellsInRadius(location, 2)).Returns(cellsInRange);
            MockGrid.Setup(grid => grid.GetNeighbors    (location))   .Returns(cellsInRange);
            MockGrid.Setup(grid => grid.GetCellsInRing  (location, 3)).Returns(new List<IHexCell>());

            var lineOfSightLogic = Container.Resolve<UnitVisibilityLogic>();

            CollectionAssert.AreEquivalent(
                cellsInRange, lineOfSightLogic.GetCellsVisibleToUnit(unit)
            );
        }

        [Test]
        public void GetCellsVisibleToUnit_AdjacentMountainsAreAlwaysVisible() {
            var cellsInRange = new List<IHexCell>() {
                BuildCell(CellTerrain.Grassland, CellShape.Flatlands, CellVegetation.None)
            };

            var location = BuildCell(CellTerrain.Grassland, CellShape.Flatlands, CellVegetation.None);

            BuildLineOfCells(
                location,
                BuildCell(CellTerrain.Grassland, CellShape.Flatlands, CellVegetation.None),
                BuildCell(CellTerrain.Grassland, CellShape.Mountains, CellVegetation.None),
                BuildCell(CellTerrain.Grassland, CellShape.Flatlands, CellVegetation.None),
                cellsInRange[0]
            );

            var unit = BuildUnit(location, 2);

            MockGrid.Setup(grid => grid.GetCellsInRadius(location, 2)).Returns(cellsInRange);
            MockGrid.Setup(grid => grid.GetNeighbors    (location))   .Returns(cellsInRange);
            MockGrid.Setup(grid => grid.GetCellsInRing  (location, 3)).Returns(new List<IHexCell>());

            var lineOfSightLogic = Container.Resolve<UnitVisibilityLogic>();

            CollectionAssert.AreEquivalent(
                cellsInRange, lineOfSightLogic.GetCellsVisibleToUnit(unit)
            );
        }

        [Test]
        public void GetCellsVisibleToUnit_AdjacentForestedHillsAreAlwaysVisible() {
            var cellsInRange = new List<IHexCell>() {
                BuildCell(CellTerrain.Grassland, CellShape.Flatlands, CellVegetation.None)
            };

            var location = BuildCell(CellTerrain.Grassland, CellShape.Flatlands, CellVegetation.None);

            BuildLineOfCells(
                location,
                BuildCell(CellTerrain.Grassland, CellShape.Flatlands, CellVegetation.None),
                BuildCell(CellTerrain.Grassland, CellShape.Hills,     CellVegetation.Forest),
                BuildCell(CellTerrain.Grassland, CellShape.Flatlands, CellVegetation.None),
                cellsInRange[0]
            );

            var unit = BuildUnit(location, 2);

            MockGrid.Setup(grid => grid.GetCellsInRadius(location, 2)).Returns(cellsInRange);
            MockGrid.Setup(grid => grid.GetNeighbors    (location))   .Returns(cellsInRange);
            MockGrid.Setup(grid => grid.GetCellsInRing  (location, 3)).Returns(new List<IHexCell>());

            var lineOfSightLogic = Container.Resolve<UnitVisibilityLogic>();

            CollectionAssert.AreEquivalent(
                cellsInRange, lineOfSightLogic.GetCellsVisibleToUnit(unit)
            );
        }

        [Test]
        public void GetCellsVisibleToUnit_AndUnitOnHills_ForestsDontObscureVision() {
            var cellsInRange = new List<IHexCell>() {
                BuildCell(CellTerrain.Grassland, CellShape.Flatlands, CellVegetation.None)
            };

            var location = BuildCell(CellTerrain.Grassland, CellShape.Hills, CellVegetation.None);

            BuildLineOfCells(
                location,
                BuildCell(CellTerrain.Grassland, CellShape.Flatlands, CellVegetation.None),
                BuildCell(CellTerrain.Grassland, CellShape.Flatlands, CellVegetation.Forest),
                BuildCell(CellTerrain.Grassland, CellShape.Flatlands, CellVegetation.None),
                cellsInRange[0]
            );

            var unit = BuildUnit(location, 2);

            MockGrid.Setup(grid => grid.GetCellsInRadius(location, 2)).Returns(cellsInRange);
            MockGrid.Setup(grid => grid.GetNeighbors    (location))   .Returns(new List<IHexCell>());
            MockGrid.Setup(grid => grid.GetCellsInRing  (location, 3)).Returns(new List<IHexCell>());

            var lineOfSightLogic = Container.Resolve<UnitVisibilityLogic>();

            CollectionAssert.AreEquivalent(
                cellsInRange, lineOfSightLogic.GetCellsVisibleToUnit(unit)
            );
        }

        [Test]
        public void GetCellsVisibleToUnit_AndUnitOnHills_HillsDontObscureVision() {
            var cellsInRange = new List<IHexCell>() {
                BuildCell(CellTerrain.Grassland, CellShape.Flatlands, CellVegetation.None)
            };

            var location = BuildCell(CellTerrain.Grassland, CellShape.Hills, CellVegetation.None);

            BuildLineOfCells(
                location,
                BuildCell(CellTerrain.Grassland, CellShape.Flatlands, CellVegetation.None),
                BuildCell(CellTerrain.Grassland, CellShape.Hills,     CellVegetation.None),
                BuildCell(CellTerrain.Grassland, CellShape.Flatlands, CellVegetation.None),
                cellsInRange[0]
            );

            var unit = BuildUnit(location, 2);

            MockGrid.Setup(grid => grid.GetCellsInRadius(location, 2)).Returns(cellsInRange);
            MockGrid.Setup(grid => grid.GetNeighbors    (location))   .Returns(new List<IHexCell>());
            MockGrid.Setup(grid => grid.GetCellsInRing  (location, 3)).Returns(new List<IHexCell>());

            var lineOfSightLogic = Container.Resolve<UnitVisibilityLogic>();

            CollectionAssert.AreEquivalent(
                cellsInRange, lineOfSightLogic.GetCellsVisibleToUnit(unit)
            );
        }

        [Test]
        public void GetCellsVisibleToUnit_AndUnitOnHills_MountainsObscureVision() {
            var cellsInRange = new List<IHexCell>() {
                BuildCell(CellTerrain.Grassland, CellShape.Flatlands, CellVegetation.None)
            };

            var location = BuildCell(CellTerrain.Grassland, CellShape.Hills, CellVegetation.None);

            BuildLineOfCells(
                location,
                BuildCell(CellTerrain.Grassland, CellShape.Flatlands, CellVegetation.None),
                BuildCell(CellTerrain.Grassland, CellShape.Mountains, CellVegetation.None),
                BuildCell(CellTerrain.Grassland, CellShape.Flatlands, CellVegetation.None),
                cellsInRange[0]
            );

            var unit = BuildUnit(location, 2);

            MockGrid.Setup(grid => grid.GetCellsInRadius(location, 2)).Returns(cellsInRange);
            MockGrid.Setup(grid => grid.GetNeighbors    (location))   .Returns(new List<IHexCell>());
            MockGrid.Setup(grid => grid.GetCellsInRing  (location, 3)).Returns(new List<IHexCell>());

            var lineOfSightLogic = Container.Resolve<UnitVisibilityLogic>();

            CollectionAssert.IsEmpty(lineOfSightLogic.GetCellsVisibleToUnit(unit));
        }

        [Test]
        public void GetCellsVisibleToUnit_AndUnitOnHills_ForestedHillsObscureVision() {
            var cellsInRange = new List<IHexCell>() {
                BuildCell(CellTerrain.Grassland, CellShape.Flatlands, CellVegetation.None)
            };

            var location = BuildCell(CellTerrain.Grassland, CellShape.Hills, CellVegetation.None);

            BuildLineOfCells(
                location,
                BuildCell(CellTerrain.Grassland, CellShape.Flatlands, CellVegetation.None),
                BuildCell(CellTerrain.Grassland, CellShape.Hills,     CellVegetation.Forest),
                BuildCell(CellTerrain.Grassland, CellShape.Flatlands, CellVegetation.None),
                cellsInRange[0]
            );

            var unit = BuildUnit(location, 2);

            MockGrid.Setup(grid => grid.GetCellsInRadius(location, 2)).Returns(cellsInRange);
            MockGrid.Setup(grid => grid.GetNeighbors    (location))   .Returns(new List<IHexCell>());
            MockGrid.Setup(grid => grid.GetCellsInRing  (location, 3)).Returns(new List<IHexCell>());

            var lineOfSightLogic = Container.Resolve<UnitVisibilityLogic>();

            CollectionAssert.IsEmpty(lineOfSightLogic.GetCellsVisibleToUnit(unit));
        }

        #endregion

        #region utilities

        private IHexCell BuildCell(CellTerrain terrain, CellShape shape, CellVegetation vegetation) {
            var mockCell = new Mock<IHexCell>();
            
            mockCell.Setup(cell => cell.Terrain)   .Returns(terrain);
            mockCell.Setup(cell => cell.Shape)     .Returns(shape);
            mockCell.Setup(cell => cell.Vegetation).Returns(vegetation);

            return mockCell.Object;
        }

        private IUnit BuildUnit(IHexCell location, int visionRange) {
            var mockUnit = new Mock<IUnit>();

            mockUnit.Setup(unit => unit.VisionRange).Returns(visionRange);

            var newUnit = mockUnit.Object;

            MockUnitPositionCanon.Setup(canon => canon.GetOwnerOfPossession(newUnit)).Returns(location);

            return newUnit;
        }

        private void BuildLineOfCells(params IHexCell[] cells) {
            if(cells.Length == 0) {
                return;
            }

            for(int startIndex = 0; startIndex < cells.Length; startIndex++) {
                for(int endIndex = startIndex; endIndex < cells.Length; endIndex++) {
                    MockGrid.Setup(
                        grid => grid.GetCellsInLine(cells[startIndex], cells[endIndex])
                    ).Returns(
                        cells.Slice(startIndex, endIndex).ToList()
                    );
                }
            }
        }

        #endregion

        #endregion

    }

}
