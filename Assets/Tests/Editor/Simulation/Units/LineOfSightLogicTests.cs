using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using Zenject;
using Moq;

using Assets.Simulation.Units;
using Assets.Simulation.HexMap;

namespace Assets.Tests.Simulation.Units {

    [TestFixture]
    public class LineOfSightLogicTests : ZenjectUnitTestFixture {

        #region internal types

        public struct CanUnitSeeTestsData {

            public int UnitVisionRange;

            public CellData LocationData;

            public List<CellData> IntermediateCellData;

            public CellData CellToSeeData;

        }

        public struct CellData {

            public int Elevation;

            public TerrainFeature Feature;

            public bool IsUnderwater;

        }

        #endregion

        #region static fields and properties

        private static IEnumerable CanUnitSeeCellTestCases {
            get {
                yield return new TestCaseData(new CanUnitSeeTestsData() {
                    UnitVisionRange = 2,
                    LocationData = new CellData() {
                        Elevation = 0, Feature = TerrainFeature.None, IsUnderwater = false
                    },
                    IntermediateCellData = new List<CellData>(),
                    CellToSeeData = new CellData() {
                        Elevation = 0, Feature = TerrainFeature.None, IsUnderwater = false
                    }
                }).SetName("Unadorned adjacent cells").Returns(true);

                yield return new TestCaseData(new CanUnitSeeTestsData() {
                    UnitVisionRange = 2,
                    LocationData = new CellData() {
                        Elevation = 0, Feature = TerrainFeature.None, IsUnderwater = false
                    },
                    IntermediateCellData = new List<CellData>() {
                        new CellData() { Elevation = 0, Feature = TerrainFeature.None, IsUnderwater = false }
                    },
                    CellToSeeData = new CellData() {
                        Elevation = 0, Feature = TerrainFeature.None, IsUnderwater = false
                    }
                }).SetName("Unadorned cells at max visual range").Returns(true);

                yield return new TestCaseData(new CanUnitSeeTestsData() {
                    UnitVisionRange = 2,
                    LocationData = new CellData() {
                        Elevation = 0, Feature = TerrainFeature.None, IsUnderwater = false
                    },
                    IntermediateCellData = new List<CellData>() {
                        new CellData() { Elevation = 0, Feature = TerrainFeature.None, IsUnderwater = false },
                        new CellData() { Elevation = 0, Feature = TerrainFeature.None, IsUnderwater = false }
                    },
                    CellToSeeData = new CellData() {
                        Elevation = 0, Feature = TerrainFeature.None, IsUnderwater = false
                    }
                }).SetName("Unadorned cells beyond max visual range").Returns(false);

                yield return new TestCaseData(new CanUnitSeeTestsData() {
                    UnitVisionRange = 2,
                    LocationData = new CellData() {
                        Elevation = 0, Feature = TerrainFeature.Forest, IsUnderwater = false
                    },
                    IntermediateCellData = new List<CellData>() {
                        new CellData() { Elevation = 0, Feature = TerrainFeature.None, IsUnderwater = false }
                    },
                    CellToSeeData = new CellData() {
                        Elevation = 0, Feature = TerrainFeature.None, IsUnderwater = false
                    }
                }).SetName("Flatland, forest at location").Returns(true);

                yield return new TestCaseData(new CanUnitSeeTestsData() {
                    UnitVisionRange = 2,
                    LocationData = new CellData() {
                        Elevation = 0, Feature = TerrainFeature.None, IsUnderwater = false
                    },
                    IntermediateCellData = new List<CellData>() {
                        new CellData() { Elevation = 0, Feature = TerrainFeature.None, IsUnderwater = false }
                    },
                    CellToSeeData = new CellData() {
                        Elevation = 0, Feature = TerrainFeature.Forest, IsUnderwater = false
                    }
                }).SetName("Flatland, forest at cell to see").Returns(true);

                yield return new TestCaseData(new CanUnitSeeTestsData() {
                    UnitVisionRange = 2,
                    LocationData = new CellData() {
                        Elevation = 0, Feature = TerrainFeature.None, IsUnderwater = false
                    },
                    IntermediateCellData = new List<CellData>() {
                        new CellData() { Elevation = 0, Feature = TerrainFeature.Forest, IsUnderwater = false }
                    },
                    CellToSeeData = new CellData() {
                        Elevation = 0, Feature = TerrainFeature.None, IsUnderwater = false
                    }
                }).SetName("Flatland, forest at intermediate cell").Returns(false);

                yield return new TestCaseData(new CanUnitSeeTestsData() {
                    UnitVisionRange = 2,
                    LocationData = new CellData() {
                        Elevation = 0, Feature = TerrainFeature.None, IsUnderwater = false
                    },
                    IntermediateCellData = new List<CellData>() {
                        new CellData() { Elevation = 1, Feature = TerrainFeature.None, IsUnderwater = false }
                    },
                    CellToSeeData = new CellData() {
                        Elevation = 0, Feature = TerrainFeature.None, IsUnderwater = false
                    }
                }).SetName("Elevated intermediate cell").Returns(false);

                yield return new TestCaseData(new CanUnitSeeTestsData() {
                    UnitVisionRange = 2,
                    LocationData = new CellData() {
                        Elevation = 0, Feature = TerrainFeature.None, IsUnderwater = false
                    },
                    IntermediateCellData = new List<CellData>() {
                        new CellData() { Elevation = 0, Feature = TerrainFeature.None, IsUnderwater = false }
                    },
                    CellToSeeData = new CellData() {
                        Elevation = 1, Feature = TerrainFeature.None, IsUnderwater = false
                    }
                }).SetName("Elevated cell to see").Returns(true);

                yield return new TestCaseData(new CanUnitSeeTestsData() {
                    UnitVisionRange = 2,
                    LocationData = new CellData() {
                        Elevation = 1, Feature = TerrainFeature.None, IsUnderwater = false
                    },
                    IntermediateCellData = new List<CellData>() {
                        new CellData() { Elevation = 1, Feature = TerrainFeature.None, IsUnderwater = false }
                    },
                    CellToSeeData = new CellData() {
                        Elevation = 1, Feature = TerrainFeature.None, IsUnderwater = false
                    }
                }).SetName("All cells elevated equally").Returns(true);

                yield return new TestCaseData(new CanUnitSeeTestsData() {
                    UnitVisionRange = 2,
                    LocationData = new CellData() {
                        Elevation = 1, Feature = TerrainFeature.None, IsUnderwater = false
                    },
                    IntermediateCellData = new List<CellData>() {
                        new CellData() { Elevation = 0, Feature = TerrainFeature.None, IsUnderwater = false }
                    },
                    CellToSeeData = new CellData() {
                        Elevation = 1, Feature = TerrainFeature.None, IsUnderwater = false
                    }
                }).SetName("Start and endpoints elevated, canyon between").Returns(true);

                yield return new TestCaseData(new CanUnitSeeTestsData() {
                    UnitVisionRange = 2,
                    LocationData = new CellData() {
                        Elevation = 1, Feature = TerrainFeature.None, IsUnderwater = false
                    },
                    IntermediateCellData = new List<CellData>() {
                        new CellData() { Elevation = 0, Feature = TerrainFeature.Forest, IsUnderwater = false }
                    },
                    CellToSeeData = new CellData() {
                        Elevation = 1, Feature = TerrainFeature.None, IsUnderwater = false
                    }
                }).SetName("Start and endpoints elevated, forested canyon between").Returns(true);

                yield return new TestCaseData(new CanUnitSeeTestsData() {
                    UnitVisionRange = 2,
                    LocationData = new CellData() {
                        Elevation = 1, Feature = TerrainFeature.None, IsUnderwater = false
                    },
                    IntermediateCellData = new List<CellData>() {
                        new CellData() { Elevation = 1, Feature = TerrainFeature.Forest, IsUnderwater = false }
                    },
                    CellToSeeData = new CellData() {
                        Elevation = 1, Feature = TerrainFeature.None, IsUnderwater = false
                    }
                }).SetName("Start and endpoints elevated, equivalent forested elevation between").Returns(true);

                yield return new TestCaseData(new CanUnitSeeTestsData() {
                    UnitVisionRange = 2,
                    LocationData = new CellData() {
                        Elevation = 1, Feature = TerrainFeature.None, IsUnderwater = false
                    },
                    IntermediateCellData = new List<CellData>() {
                        new CellData() { Elevation = 2, Feature = TerrainFeature.None, IsUnderwater = false }
                    },
                    CellToSeeData = new CellData() {
                        Elevation = 1, Feature = TerrainFeature.None, IsUnderwater = false
                    }
                }).SetName("Start and endpoints elevated, higher elevation between").Returns(true);

                yield return new TestCaseData(new CanUnitSeeTestsData() {
                    UnitVisionRange = 2,
                    LocationData = new CellData() {
                        Elevation = 0, Feature = TerrainFeature.None, IsUnderwater = false
                    },
                    IntermediateCellData = new List<CellData>(),
                    CellToSeeData = new CellData() {
                        Elevation = 1, Feature = TerrainFeature.None, IsUnderwater = false
                    }
                }).SetName("Adjacent cells, cell to see elevation 1 greater than location").Returns(true);

                yield return new TestCaseData(new CanUnitSeeTestsData() {
                    UnitVisionRange = 2,
                    LocationData = new CellData() {
                        Elevation = 0, Feature = TerrainFeature.None, IsUnderwater = false
                    },
                    IntermediateCellData = new List<CellData>(),
                    CellToSeeData = new CellData() {
                        Elevation = 2, Feature = TerrainFeature.None, IsUnderwater = false
                    }
                }).SetName("Adjacent cells, cell to see elevation 2 greater than location").Returns(true);

                yield return new TestCaseData(new CanUnitSeeTestsData() {
                    UnitVisionRange = 2,
                    LocationData = new CellData() {
                        Elevation = 0, Feature = TerrainFeature.None, IsUnderwater = false
                    },
                    IntermediateCellData = new List<CellData>(),
                    CellToSeeData = new CellData() {
                        Elevation = 3, Feature = TerrainFeature.None, IsUnderwater = false
                    }
                }).SetName("Adjacent cells, cell to see elevation 3 greater than location").Returns(false);

                yield return new TestCaseData(new CanUnitSeeTestsData() {
                    UnitVisionRange = 2,
                    LocationData = new CellData() {
                        Elevation = 3, Feature = TerrainFeature.None, IsUnderwater = false
                    },
                    IntermediateCellData = new List<CellData>(),
                    CellToSeeData = new CellData() {
                        Elevation = 0, Feature = TerrainFeature.None, IsUnderwater = false
                    }
                }).SetName("Adjacent cells, cell to see elevation 3 lower than location").Returns(true);

                yield return new TestCaseData(new CanUnitSeeTestsData() {
                    UnitVisionRange = 2,
                    LocationData = new CellData() {
                        Elevation = 0, Feature = TerrainFeature.None, IsUnderwater = false
                    },
                    IntermediateCellData = new List<CellData>() {
                        new CellData() { Elevation = 0, Feature = TerrainFeature.None, IsUnderwater = false }
                    },
                    CellToSeeData = new CellData() {
                        Elevation = 3, Feature = TerrainFeature.None, IsUnderwater = false
                    }
                }).SetName("Separated cells, cell to see elevation 3 greater than location").Returns(true);

                yield return new TestCaseData(new CanUnitSeeTestsData() {
                    UnitVisionRange = 2,
                    LocationData = new CellData() {
                        Elevation = 0, Feature = TerrainFeature.None, IsUnderwater = false
                    },
                    IntermediateCellData = new List<CellData>() {
                        new CellData() { Elevation = 0, Feature = TerrainFeature.None, IsUnderwater = false }
                    },
                    CellToSeeData = new CellData() {
                        Elevation = 4, Feature = TerrainFeature.None, IsUnderwater = false
                    }
                }).SetName("Separated cells, cell to see elevation 4 greater than location").Returns(false);

                yield return new TestCaseData(new CanUnitSeeTestsData() {
                    UnitVisionRange = 2,
                    LocationData = new CellData() {
                        Elevation = 0, Feature = TerrainFeature.None, IsUnderwater = false
                    },
                    IntermediateCellData = new List<CellData>() {
                        new CellData() { Elevation = 0, Feature = TerrainFeature.None, IsUnderwater = true },
                        new CellData() { Elevation = 0, Feature = TerrainFeature.None, IsUnderwater = true }
                    },
                    CellToSeeData = new CellData() {
                        Elevation = 0, Feature = TerrainFeature.None, IsUnderwater = false
                    }
                }).SetName("Cell to see 1 beyond max vision range, intermediate water tiles").Returns(true);

                yield return new TestCaseData(new CanUnitSeeTestsData() {
                    UnitVisionRange = 2,
                    LocationData = new CellData() {
                        Elevation = 0, Feature = TerrainFeature.None, IsUnderwater = false
                    },
                    IntermediateCellData = new List<CellData>() {
                        new CellData() { Elevation = 0, Feature = TerrainFeature.None, IsUnderwater = true },
                        new CellData() { Elevation = 0, Feature = TerrainFeature.None, IsUnderwater = true },
                        new CellData() { Elevation = 0, Feature = TerrainFeature.None, IsUnderwater = true }
                    },
                    CellToSeeData = new CellData() {
                        Elevation = 0, Feature = TerrainFeature.None, IsUnderwater = false
                    }
                }).SetName("Cell to see 2 beyond max vision range, intermediate water tiles").Returns(false);

                yield return new TestCaseData(new CanUnitSeeTestsData() {
                    UnitVisionRange = 2,
                    LocationData = new CellData() {
                        Elevation = 0, Feature = TerrainFeature.None, IsUnderwater = false
                    },
                    IntermediateCellData = new List<CellData>() {
                        new CellData() { Elevation = 0, Feature = TerrainFeature.None, IsUnderwater = false },
                        new CellData() { Elevation = 0, Feature = TerrainFeature.None, IsUnderwater = true }
                    },
                    CellToSeeData = new CellData() {
                        Elevation = 0, Feature = TerrainFeature.None, IsUnderwater = false
                    }
                }).SetName("Cell to see 1 beyond max vision range, nonwater-then-water intermediate").Returns(false);

                yield return new TestCaseData(new CanUnitSeeTestsData() {
                    UnitVisionRange = 2,
                    LocationData = new CellData() {
                        Elevation = 0, Feature = TerrainFeature.None, IsUnderwater = false
                    },
                    IntermediateCellData = new List<CellData>() {
                        new CellData() { Elevation = 0, Feature = TerrainFeature.None, IsUnderwater = false },
                        new CellData() { Elevation = 0, Feature = TerrainFeature.None, IsUnderwater = false }
                    },
                    CellToSeeData = new CellData() {
                        Elevation = 1, Feature = TerrainFeature.None, IsUnderwater = false
                    }
                }).SetName("Cell to see 1 beyond max vision range, 1 elevation higher").Returns(false);

                yield return new TestCaseData(new CanUnitSeeTestsData() {
                    UnitVisionRange = 2,
                    LocationData = new CellData() {
                        Elevation = 0, Feature = TerrainFeature.None, IsUnderwater = false
                    },
                    IntermediateCellData = new List<CellData>() {
                        new CellData() { Elevation = 0, Feature = TerrainFeature.None, IsUnderwater = false },
                        new CellData() { Elevation = 0, Feature = TerrainFeature.None, IsUnderwater = false }
                    },
                    CellToSeeData = new CellData() {
                        Elevation = 2, Feature = TerrainFeature.None, IsUnderwater = false
                    }
                }).SetName("Cell to see 1 beyond max vision range, 2 elevation higher").Returns(true);

                yield return new TestCaseData(new CanUnitSeeTestsData() {
                    UnitVisionRange = 2,
                    LocationData = new CellData() {
                        Elevation = 0, Feature = TerrainFeature.None, IsUnderwater = false
                    },
                    IntermediateCellData = new List<CellData>() {
                        new CellData() { Elevation = 0, Feature = TerrainFeature.None, IsUnderwater = false },
                        new CellData() { Elevation = 0, Feature = TerrainFeature.None, IsUnderwater = false }
                    },
                    CellToSeeData = new CellData() {
                        Elevation = 3, Feature = TerrainFeature.None, IsUnderwater = false
                    }
                }).SetName("Cell to see 1 beyond max vision range, 3 elevation higher").Returns(true);

                yield return new TestCaseData(new CanUnitSeeTestsData() {
                    UnitVisionRange = 2,
                    LocationData = new CellData() {
                        Elevation = 0, Feature = TerrainFeature.None, IsUnderwater = false
                    },
                    IntermediateCellData = new List<CellData>() {
                        new CellData() { Elevation = 0, Feature = TerrainFeature.None, IsUnderwater = false },
                        new CellData() { Elevation = 0, Feature = TerrainFeature.None, IsUnderwater = false }
                    },
                    CellToSeeData = new CellData() {
                        Elevation = 4, Feature = TerrainFeature.None, IsUnderwater = false
                    }
                }).SetName("Cell to see 1 beyond max vision range, 4 elevation higher").Returns(true);

                yield return new TestCaseData(new CanUnitSeeTestsData() {
                    UnitVisionRange = 2,
                    LocationData = new CellData() {
                        Elevation = 0, Feature = TerrainFeature.None, IsUnderwater = false
                    },
                    IntermediateCellData = new List<CellData>() {
                        new CellData() { Elevation = 0, Feature = TerrainFeature.None, IsUnderwater = false },
                        new CellData() { Elevation = 0, Feature = TerrainFeature.None, IsUnderwater = false }
                    },
                    CellToSeeData = new CellData() {
                        Elevation = 5, Feature = TerrainFeature.None, IsUnderwater = false
                    }
                }).SetName("Cell to see 1 beyond max vision range, 5 elevation higher").Returns(false);
            }
        }

        #endregion

        #region instance fields and properties

        private Mock<IHexGrid> MockGrid;

        private Mock<IUnitPositionCanon> MockPositionCanon;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockGrid          = new Mock<IHexGrid>();
            MockPositionCanon = new Mock<IUnitPositionCanon>();

            Container.Bind<IHexGrid>          ().FromInstance(MockGrid         .Object);
            Container.Bind<IUnitPositionCanon>().FromInstance(MockPositionCanon.Object);

            Container.Bind<LineOfSightLogic>().AsSingle();
        }

        #endregion

        #region tests

        [Test(Description = "CanUnitSeeCell should always return true on the cell the unit occupies")]
        public void CanUnitSeeCell_OwnCellAlwaysVisible() {
            var unitLocation = BuildCell(new CellData());

            var unit = BuildUnit(0, unitLocation);

            var lineOfSightLogic = Container.Resolve<LineOfSightLogic>();

            Assert.True(lineOfSightLogic.CanUnitSeeCell(unit, unitLocation), "A unit cannot see its own cell");
        }

        [Test()]
        [TestCaseSource("CanUnitSeeCellTestCases")]
        public bool CanUnitSeeCellTests(CanUnitSeeTestsData testData) {
            var unitLocation = BuildCell(testData.LocationData);
            var cellToSee    = BuildCell(testData.CellToSeeData);

            List<IHexCell> intermediateCells = testData.IntermediateCellData.Select(data => BuildCell(data)).ToList();

            ConfigureCellChain(unitLocation, intermediateCells, cellToSee);

            var unit = BuildUnit(testData.UnitVisionRange, unitLocation);

            var lineOfSightLogic = Container.Resolve<LineOfSightLogic>();

            return lineOfSightLogic.CanUnitSeeCell(unit, cellToSee);
        }

        [Test(Description = "GetCellsVisibleToUnit should call CanUnitSeeCell on every cell within " +
            "the unit's vision range plus 1. It should return any cells for which CanUnitSeeCell " +
            "returns true")]
        public void GetCellsVisibleToUnit_SearchesThroughAppropriateCells() {
            Assert.Ignore("This method tests 2 lines of code that call into a CanUnitSeeCell, " +
                "which is already well-tested. A unit test here would be more complicated than " +
                "the code it's testing"
            );
        }

        #endregion

        #region utilities

        private void ConfigureCellChain(IHexCell start, List<IHexCell> intermediates, IHexCell end) {
            var cellLine = new List<IHexCell>();
            cellLine.Add(start);
            cellLine.AddRange(intermediates);
            cellLine.Add(end);

            MockGrid.Setup(grid => grid.GetCellsInLine(start, end)).Returns(cellLine);
        }

        private IHexCell BuildCell(CellData data) {
            var mockCell = new Mock<IHexCell>();

            mockCell.Setup(cell => cell.Elevation   ).Returns(data.Elevation);
            mockCell.Setup(cell => cell.Feature     ).Returns(data.Feature);
            mockCell.Setup(cell => cell.IsUnderwater).Returns(data.IsUnderwater);

            var newCell = mockCell.Object;

            MockGrid.Setup(grid => grid.GetCellsInLine(newCell, newCell)).Returns(new List<IHexCell>() { newCell });

            return newCell;
        }

        private IUnit BuildUnit(int visionRange, IHexCell unitLocation) {
            var mockUnit = new Mock<IUnit>();

            mockUnit.Setup(unit => unit.VisionRange).Returns(visionRange);

            MockPositionCanon.Setup(canon => canon.GetOwnerOfPossession(mockUnit.Object)).Returns(unitLocation);

            return mockUnit.Object;
        }

        #endregion

        #endregion

    }

}
