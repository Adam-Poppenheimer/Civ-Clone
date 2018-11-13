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
    public class RiverCanonTests : ZenjectUnitTestFixture {

        #region internal types

        public class CanAddRiverToCellTestData {

            public HexCellTestData CellToTest = new HexCellTestData();

            public HexCellTestData Neighbor;
            public HexCellTestData PreviousNeighbor;
            public HexCellTestData NextNeighbor;

            public RiverFlow FlowToTest = RiverFlow.Clockwise;

        }

        public class HexCellTestData {

            public bool IsUnderwater;

            public CellShape Shape;

            public int EdgeElevation;

            public bool[]      HasRiverAtEdge    = new bool[6];
            public RiverFlow[] FlowOfRiverAtEdge = new RiverFlow[6];

        }

        #endregion

        #region static fields and properties

        public static IEnumerable CanAddRiverToCellTestCases {
            get {
                yield return new TestCaseData(new CanAddRiverToCellTestData() {
                    Neighbor = new HexCellTestData()
                }).SetName("No existing rivers, only neighbor exists").Returns(true);

                yield return new TestCaseData(new CanAddRiverToCellTestData() {

                }).SetName("No existing rivers, neighbor does not exist").Returns(false);

                yield return new TestCaseData(new CanAddRiverToCellTestData() {
                    CellToTest = new HexCellTestData() { IsUnderwater = true },
                    Neighbor = new HexCellTestData()
                }).SetName("Cell is underwater").Returns(false);

                yield return new TestCaseData(new CanAddRiverToCellTestData() {
                    Neighbor = new HexCellTestData() { IsUnderwater = true }
                }).SetName("Neighbor is underwater").Returns(false);

                yield return new TestCaseData(new CanAddRiverToCellTestData() {
                    FlowToTest = RiverFlow.Clockwise,
                    CellToTest = new HexCellTestData() {
                        HasRiverAtEdge = new bool[6] { false, true, false, false, false, false }
                    },
                    Neighbor = new HexCellTestData()
                }).SetName("River already exists").Returns(false);

                yield return new TestCaseData(new CanAddRiverToCellTestData() {
                    FlowToTest = RiverFlow.Clockwise,
                    CellToTest = new HexCellTestData() {
                        HasRiverAtEdge    = new bool[6] { true, false, false, false, false, false },
                        FlowOfRiverAtEdge = new RiverFlow[1] { RiverFlow.Counterclockwise }
                    },
                    PreviousNeighbor = new HexCellTestData(),
                    Neighbor = new HexCellTestData(),
                }).SetName("Checking Clockwise flow, River with Counterclockwise flow in previous direction").Returns(false);

                yield return new TestCaseData(new CanAddRiverToCellTestData() {
                    FlowToTest = RiverFlow.Clockwise,
                    CellToTest = new HexCellTestData() {
                        HasRiverAtEdge    = new bool[6] { true, false, false, false, false, false },
                        FlowOfRiverAtEdge = new RiverFlow[1] { RiverFlow.Clockwise }
                    },
                    PreviousNeighbor = new HexCellTestData(),
                    Neighbor = new HexCellTestData(),
                }).SetName("Checking Clockwise flow, River with Clockwise flow in previous direction").Returns(true);

                yield return new TestCaseData(new CanAddRiverToCellTestData() {
                    FlowToTest = RiverFlow.Counterclockwise,
                    CellToTest = new HexCellTestData() {
                        HasRiverAtEdge    = new bool[6] { true, false, false, false, false, false },
                        FlowOfRiverAtEdge = new RiverFlow[1] { RiverFlow.Clockwise }
                    },
                    PreviousNeighbor = new HexCellTestData(),
                    Neighbor = new HexCellTestData(),
                }).SetName("Checking Counterclockwise flow, River with Clockwise flow in previous direction").Returns(false);

                yield return new TestCaseData(new CanAddRiverToCellTestData() {
                    FlowToTest = RiverFlow.Counterclockwise,
                    CellToTest = new HexCellTestData() {
                        HasRiverAtEdge    = new bool[6] { true, false, false, false, false, false },
                        FlowOfRiverAtEdge = new RiverFlow[1] { RiverFlow.Counterclockwise }
                    },
                    PreviousNeighbor = new HexCellTestData(),
                    Neighbor = new HexCellTestData(),
                }).SetName("Checking Counterclockwise flow, River with Counterclockwise flow in previous direction").Returns(true);

                yield return new TestCaseData(new CanAddRiverToCellTestData() {
                    FlowToTest = RiverFlow.Clockwise,
                    CellToTest = new HexCellTestData() {
                        HasRiverAtEdge    = new bool[6] { false, false, true, false, false, false },
                        FlowOfRiverAtEdge = new RiverFlow[3] { RiverFlow.Clockwise, RiverFlow.Clockwise, RiverFlow.Counterclockwise }
                    },
                    Neighbor = new HexCellTestData(),
                    NextNeighbor = new HexCellTestData()
                }).SetName("Checking Clockwise flow, River with Counterclockwise flow in next direction").Returns(false);

                yield return new TestCaseData(new CanAddRiverToCellTestData() {
                    FlowToTest = RiverFlow.Clockwise,
                    CellToTest = new HexCellTestData() {
                        HasRiverAtEdge    = new bool[6] { false, false, true, false, false, false },
                        FlowOfRiverAtEdge = new RiverFlow[3] { RiverFlow.Clockwise, RiverFlow.Clockwise, RiverFlow.Clockwise }
                    },
                    Neighbor = new HexCellTestData(),
                    NextNeighbor = new HexCellTestData()
                }).SetName("Checking Clockwise flow, River with Clockwise flow in next direction").Returns(true);

                yield return new TestCaseData(new CanAddRiverToCellTestData() {
                    FlowToTest = RiverFlow.Counterclockwise,
                    CellToTest = new HexCellTestData() {
                        HasRiverAtEdge    = new bool[6] { false, false, true, false, false, false },
                        FlowOfRiverAtEdge = new RiverFlow[3] { RiverFlow.Clockwise, RiverFlow.Clockwise, RiverFlow.Clockwise }
                    },
                    Neighbor = new HexCellTestData(),
                    NextNeighbor = new HexCellTestData()
                }).SetName("Checking Counterclockwise flow, River with Clockwise flow in next direction").Returns(false);

                yield return new TestCaseData(new CanAddRiverToCellTestData() {
                    FlowToTest = RiverFlow.Counterclockwise,
                    CellToTest = new HexCellTestData() {
                        HasRiverAtEdge    = new bool[6] { false, false, true, false, false, false },
                        FlowOfRiverAtEdge = new RiverFlow[3] { RiverFlow.Clockwise, RiverFlow.Clockwise, RiverFlow.Counterclockwise }
                    },
                    Neighbor = new HexCellTestData(),
                    NextNeighbor = new HexCellTestData()
                }).SetName("Checking Counterclockwise flow, River with Counterclockwise flow in next direction").Returns(true);



                yield return new TestCaseData(new CanAddRiverToCellTestData() {
                    FlowToTest = RiverFlow.Clockwise,
                    CellToTest = new HexCellTestData(),
                    PreviousNeighbor = new HexCellTestData(),
                    Neighbor = new HexCellTestData() {
                        HasRiverAtEdge = new bool[] { false, false, false, false, false, true },
                        FlowOfRiverAtEdge = new RiverFlow[] {
                            RiverFlow.Clockwise, RiverFlow.Clockwise, RiverFlow.Clockwise,
                            RiverFlow.Clockwise, RiverFlow.Clockwise, RiverFlow.Clockwise,
                        }
                    },
                }).SetName("Checking Clockwise flow, river between previous neighbor and neighbor has Clockwise flow").Returns(false);

                yield return new TestCaseData(new CanAddRiverToCellTestData() {
                    FlowToTest = RiverFlow.Clockwise,
                    CellToTest = new HexCellTestData(),
                    PreviousNeighbor = new HexCellTestData(),
                    Neighbor = new HexCellTestData() {
                        HasRiverAtEdge = new bool[] { false, false, false, false, false, true },
                        FlowOfRiverAtEdge = new RiverFlow[] {
                            RiverFlow.Clockwise, RiverFlow.Clockwise, RiverFlow.Clockwise,
                            RiverFlow.Clockwise, RiverFlow.Clockwise, RiverFlow.Counterclockwise,
                        }
                    },
                }).SetName("Checking Clockwise flow, river between previous neighbor and neighbor has Counterclockwise flow").Returns(true);

                yield return new TestCaseData(new CanAddRiverToCellTestData() {
                    FlowToTest = RiverFlow.Counterclockwise,
                    CellToTest = new HexCellTestData(),
                    PreviousNeighbor = new HexCellTestData(),
                    Neighbor = new HexCellTestData() {
                        HasRiverAtEdge = new bool[] { false, false, false, false, false, true },
                        FlowOfRiverAtEdge = new RiverFlow[] {
                            RiverFlow.Clockwise, RiverFlow.Clockwise, RiverFlow.Clockwise,
                            RiverFlow.Clockwise, RiverFlow.Clockwise, RiverFlow.Clockwise,
                        }
                    },
                }).SetName("Checking Counterclockwise flow, river between previous neighbor and neighbor has Clockwise flow").Returns(true);

                yield return new TestCaseData(new CanAddRiverToCellTestData() {
                    FlowToTest = RiverFlow.Counterclockwise,
                    CellToTest = new HexCellTestData(),
                    PreviousNeighbor = new HexCellTestData(),
                    Neighbor = new HexCellTestData() {
                        HasRiverAtEdge = new bool[] { false, false, false, false, false, true },
                        FlowOfRiverAtEdge = new RiverFlow[] {
                            RiverFlow.Clockwise, RiverFlow.Clockwise, RiverFlow.Clockwise,
                            RiverFlow.Clockwise, RiverFlow.Clockwise, RiverFlow.Counterclockwise,
                        }
                    },
                }).SetName("Checking Counterclockwise flow, river between previous neighbor and neighbor has Counterclockwise flow").Returns(false);



                yield return new TestCaseData(new CanAddRiverToCellTestData() {
                    FlowToTest = RiverFlow.Clockwise,
                    CellToTest = new HexCellTestData(),
                    Neighbor = new HexCellTestData() {
                        HasRiverAtEdge = new bool[] { false, false, false, true, false, false },
                        FlowOfRiverAtEdge = new RiverFlow[] {
                            RiverFlow.Clockwise, RiverFlow.Clockwise, RiverFlow.Clockwise,
                            RiverFlow.Clockwise
                        }
                    },
                    NextNeighbor = new HexCellTestData()
                }).SetName("Checking Clockwise flow, river between next neighbor and neighbor has Clockwise flow").Returns(false);

                yield return new TestCaseData(new CanAddRiverToCellTestData() {
                    FlowToTest = RiverFlow.Clockwise,
                    CellToTest = new HexCellTestData(),
                    Neighbor = new HexCellTestData() {
                        HasRiverAtEdge = new bool[] { false, false, false, true, false, false },
                        FlowOfRiverAtEdge = new RiverFlow[] {
                            RiverFlow.Clockwise, RiverFlow.Clockwise, RiverFlow.Clockwise,
                            RiverFlow.Counterclockwise
                        }
                    },
                    NextNeighbor = new HexCellTestData()
                }).SetName("Checking Clockwise flow, river between next neighbor and neighbor has Counterclockwise flow").Returns(true);

                yield return new TestCaseData(new CanAddRiverToCellTestData() {
                    FlowToTest = RiverFlow.Counterclockwise,
                    CellToTest = new HexCellTestData(),
                    Neighbor = new HexCellTestData() {
                        HasRiverAtEdge = new bool[] { false, false, false, true, false, false },
                        FlowOfRiverAtEdge = new RiverFlow[] {
                            RiverFlow.Clockwise, RiverFlow.Clockwise, RiverFlow.Clockwise,
                            RiverFlow.Clockwise
                        }
                    },
                    NextNeighbor = new HexCellTestData()
                }).SetName("Checking Counterclockwise flow, river between next neighbor and neighbor has Clockwise flow").Returns(true);

                yield return new TestCaseData(new CanAddRiverToCellTestData() {
                    FlowToTest = RiverFlow.Counterclockwise,
                    CellToTest = new HexCellTestData(),
                    Neighbor = new HexCellTestData() {
                        HasRiverAtEdge = new bool[] { false, false, false, true, false, false },
                        FlowOfRiverAtEdge = new RiverFlow[] {
                            RiverFlow.Clockwise, RiverFlow.Clockwise, RiverFlow.Clockwise,
                            RiverFlow.Counterclockwise
                        }
                    },
                    NextNeighbor = new HexCellTestData()
                }).SetName("Checking Counterclockwise flow, river between next neighbor and neighbor has Counterclockwise flow").Returns(false);

                yield return new TestCaseData(new CanAddRiverToCellTestData() {
                    FlowToTest = RiverFlow.Clockwise,
                    CellToTest = new HexCellTestData() {
                        HasRiverAtEdge = new bool[] { true, false, false, false, false, false },
                        FlowOfRiverAtEdge = new RiverFlow[] { RiverFlow.Counterclockwise }
                    },
                    PreviousNeighbor = new HexCellTestData() {
                        HasRiverAtEdge = new bool[] { false, false, true, false, false, false },
                        FlowOfRiverAtEdge = new RiverFlow[] { RiverFlow.Clockwise, RiverFlow.Clockwise, RiverFlow.Clockwise }
                    },
                    Neighbor = new HexCellTestData()
                }).SetName("This/Previous has CCW river, Previous/Neighbor has CW river, checking CW").Returns(true);

                yield return new TestCaseData(new CanAddRiverToCellTestData() {
                    FlowToTest = RiverFlow.Counterclockwise,
                    CellToTest = new HexCellTestData() {
                        HasRiverAtEdge = new bool[] { true, false, false, false, false, false },
                        FlowOfRiverAtEdge = new RiverFlow[] { RiverFlow.Counterclockwise }
                    },
                    PreviousNeighbor = new HexCellTestData() {
                        HasRiverAtEdge = new bool[] { false, false, true, false, false, false },
                        FlowOfRiverAtEdge = new RiverFlow[] { RiverFlow.Clockwise, RiverFlow.Clockwise, RiverFlow.Clockwise }
                    },
                    Neighbor = new HexCellTestData()
                }).SetName("This/Previous has CCW river, Previous/Neighbor has CW river, checking CCW").Returns(true);

                yield return new TestCaseData(new CanAddRiverToCellTestData() {
                    FlowToTest = RiverFlow.Clockwise,
                    CellToTest = new HexCellTestData() {
                        HasRiverAtEdge = new bool[] { true, false, false, false, false, false },
                        FlowOfRiverAtEdge = new RiverFlow[] { RiverFlow.Clockwise }
                    },
                    PreviousNeighbor = new HexCellTestData() {
                        HasRiverAtEdge = new bool[] { false, false, true, false, false, false },
                        FlowOfRiverAtEdge = new RiverFlow[] { RiverFlow.Clockwise, RiverFlow.Clockwise, RiverFlow.Counterclockwise }
                    },
                    Neighbor = new HexCellTestData()
                }).SetName("This/Previous has CW river, Previous/Neighbor has CCW river, checking CW").Returns(true);

                yield return new TestCaseData(new CanAddRiverToCellTestData() {
                    FlowToTest = RiverFlow.Counterclockwise,
                    CellToTest = new HexCellTestData() {
                        HasRiverAtEdge = new bool[] { true, false, false, false, false, false },
                        FlowOfRiverAtEdge = new RiverFlow[] { RiverFlow.Clockwise }
                    },
                    PreviousNeighbor = new HexCellTestData() {
                        HasRiverAtEdge = new bool[] { false, false, true, false, false, false },
                        FlowOfRiverAtEdge = new RiverFlow[] { RiverFlow.Clockwise, RiverFlow.Clockwise, RiverFlow.Counterclockwise }
                    },
                    Neighbor = new HexCellTestData()
                }).SetName("This/Previous has CW river, Previous/Neighbor has CCW river, checking CWW").Returns(true);



                yield return new TestCaseData(new CanAddRiverToCellTestData() {
                    FlowToTest = RiverFlow.Clockwise,
                    CellToTest = new HexCellTestData() {
                        HasRiverAtEdge = new bool[] { false, false, true, false, false, false },
                        FlowOfRiverAtEdge = new RiverFlow[] { RiverFlow.Clockwise, RiverFlow.Clockwise, RiverFlow.Clockwise }
                    },
                    Neighbor = new HexCellTestData(),
                    NextNeighbor = new HexCellTestData() {
                        HasRiverAtEdge = new bool[] { true, false, false, false, false, false },
                        FlowOfRiverAtEdge = new RiverFlow[] { RiverFlow.Counterclockwise }
                    }
                }).SetName("This/Next has CW river, Next/Neighbor has CCW river, checking CW").Returns(true);

                yield return new TestCaseData(new CanAddRiverToCellTestData() {
                    FlowToTest = RiverFlow.Counterclockwise,
                    CellToTest = new HexCellTestData() {
                        HasRiverAtEdge = new bool[] { false, false, true, false, false, false },
                        FlowOfRiverAtEdge = new RiverFlow[] { RiverFlow.Clockwise, RiverFlow.Clockwise, RiverFlow.Clockwise }
                    },
                    Neighbor = new HexCellTestData(),
                    NextNeighbor = new HexCellTestData() {
                        HasRiverAtEdge = new bool[] { true, false, false, false, false, false },
                        FlowOfRiverAtEdge = new RiverFlow[] { RiverFlow.Counterclockwise }
                    }
                }).SetName("This/Next has CW river, Next/Neighbor has CCW river, checking CWW").Returns(true);

                yield return new TestCaseData(new CanAddRiverToCellTestData() {
                    FlowToTest = RiverFlow.Clockwise,
                    CellToTest = new HexCellTestData() {
                        HasRiverAtEdge = new bool[] { false, false, true, false, false, false },
                        FlowOfRiverAtEdge = new RiverFlow[] { RiverFlow.Clockwise, RiverFlow.Clockwise, RiverFlow.Counterclockwise }
                    },
                    Neighbor = new HexCellTestData(),
                    NextNeighbor = new HexCellTestData() {
                        HasRiverAtEdge = new bool[] { true, false, false, false, false, false },
                        FlowOfRiverAtEdge = new RiverFlow[] { RiverFlow.Clockwise }
                    }
                }).SetName("This/Next has CWW river, Next/Neighbor has CW river, checking CW").Returns(true);

                yield return new TestCaseData(new CanAddRiverToCellTestData() {
                    FlowToTest = RiverFlow.Counterclockwise,
                    CellToTest = new HexCellTestData() {
                        HasRiverAtEdge = new bool[] { false, false, true, false, false, false },
                        FlowOfRiverAtEdge = new RiverFlow[] { RiverFlow.Clockwise, RiverFlow.Clockwise, RiverFlow.Counterclockwise }
                    },
                    Neighbor = new HexCellTestData(),
                    NextNeighbor = new HexCellTestData() {
                        HasRiverAtEdge = new bool[] { true, false, false, false, false, false },
                        FlowOfRiverAtEdge = new RiverFlow[] { RiverFlow.Clockwise }
                    }
                }).SetName("This/Next has CWW river, Next/Neighbor has CW river, checking CWW").Returns(true);




                yield return new TestCaseData(new CanAddRiverToCellTestData() {
                    FlowToTest = RiverFlow.Clockwise,
                    CellToTest = new HexCellTestData() {
                        HasRiverAtEdge = new bool[] { true, false, false, false, false, false },
                        FlowOfRiverAtEdge = new RiverFlow[] { RiverFlow.Counterclockwise },
                        EdgeElevation = 2
                    },
                    PreviousNeighbor = new HexCellTestData() {
                        HasRiverAtEdge = new bool[] { false, false, true, false, false, false },
                        FlowOfRiverAtEdge = new RiverFlow[] { RiverFlow.Clockwise, RiverFlow.Clockwise, RiverFlow.Clockwise },
                        EdgeElevation = 1
                    },
                    Neighbor = new HexCellTestData() { EdgeElevation = 2 }
                }).SetName("Clockwise flow, otherwise valid confluence, this and neighbor are above previous neighbor").Returns(false);

                yield return new TestCaseData(new CanAddRiverToCellTestData() {
                    FlowToTest = RiverFlow.Counterclockwise,
                    CellToTest = new HexCellTestData() {
                        HasRiverAtEdge = new bool[] { true, false, false, false, false, false },
                        FlowOfRiverAtEdge = new RiverFlow[] { RiverFlow.Counterclockwise },
                        EdgeElevation = 2
                    },
                    PreviousNeighbor = new HexCellTestData() {
                        HasRiverAtEdge = new bool[] { false, false, true, false, false, false },
                        FlowOfRiverAtEdge = new RiverFlow[] { RiverFlow.Clockwise, RiverFlow.Clockwise, RiverFlow.Clockwise },
                        EdgeElevation = 1
                    },
                    Neighbor = new HexCellTestData() { EdgeElevation = 2 }
                }).SetName("Counterclockwise flow, otherwise valid confluence, this and neighbor are above previous neighbor").Returns(true);

                yield return new TestCaseData(new CanAddRiverToCellTestData() {
                    FlowToTest = RiverFlow.Clockwise,
                    CellToTest = new HexCellTestData() {
                        HasRiverAtEdge = new bool[] { false, false, true, false, false, false },
                        FlowOfRiverAtEdge = new RiverFlow[] { RiverFlow.Clockwise, RiverFlow.Clockwise, RiverFlow.Clockwise },
                        EdgeElevation = 2
                    },
                    Neighbor = new HexCellTestData() { EdgeElevation = 2 },
                    NextNeighbor = new HexCellTestData() {
                        HasRiverAtEdge = new bool[] { true, false, false, false, false, false },
                        FlowOfRiverAtEdge = new RiverFlow[] { RiverFlow.Counterclockwise },
                        EdgeElevation = 1
                    }
                }).SetName("Clockwise flow, otherwise valid confluence, this and neighbor are above next neighbor").Returns(true);

                yield return new TestCaseData(new CanAddRiverToCellTestData() {
                    FlowToTest = RiverFlow.Counterclockwise,
                    CellToTest = new HexCellTestData() {
                        HasRiverAtEdge = new bool[] { false, false, true, false, false, false },
                        FlowOfRiverAtEdge = new RiverFlow[] { RiverFlow.Clockwise, RiverFlow.Clockwise, RiverFlow.Clockwise },
                        EdgeElevation = 2
                    },
                    Neighbor = new HexCellTestData() { EdgeElevation = 2 },
                    NextNeighbor = new HexCellTestData() {
                        HasRiverAtEdge = new bool[] { true, false, false, false, false, false },
                        FlowOfRiverAtEdge = new RiverFlow[] { RiverFlow.Counterclockwise },
                        EdgeElevation = 1
                    }
                }).SetName("Counterclockwise flow, otherwise valid confluence, this and neighbor are above next neighbor").Returns(false);



                yield return new TestCaseData(new CanAddRiverToCellTestData() {
                    FlowToTest = RiverFlow.Clockwise,
                    CellToTest = new HexCellTestData() {
                        HasRiverAtEdge = new bool[] { true, false, false, false, false, false },
                        FlowOfRiverAtEdge = new RiverFlow[] { RiverFlow.Clockwise },
                        EdgeElevation = 2
                    },
                    PreviousNeighbor = new HexCellTestData() { EdgeElevation = 1 },
                    Neighbor = new HexCellTestData() { EdgeElevation = 2 }
                }).SetName("This/previous CW river, checking CW, this and neighbor are above previous neighbor").Returns(false);

                yield return new TestCaseData(new CanAddRiverToCellTestData() {
                    FlowToTest = RiverFlow.Counterclockwise,
                    CellToTest = new HexCellTestData() {
                        HasRiverAtEdge = new bool[] { true, false, false, false, false, false },
                        FlowOfRiverAtEdge = new RiverFlow[] { RiverFlow.Counterclockwise },
                        EdgeElevation = 2
                    },
                    PreviousNeighbor = new HexCellTestData() { EdgeElevation = 1 },
                    Neighbor = new HexCellTestData() { EdgeElevation = 2 }
                }).SetName("This/previous CWW river, checking CCW, this and neighbor are above previous neighbor").Returns(true);

                yield return new TestCaseData(new CanAddRiverToCellTestData() {
                    FlowToTest = RiverFlow.Clockwise,
                    CellToTest = new HexCellTestData() { EdgeElevation = 2 },
                    PreviousNeighbor = new HexCellTestData() {
                        HasRiverAtEdge = new bool[] { false, false, true, false, false, false },
                        FlowOfRiverAtEdge = new RiverFlow[] { RiverFlow.Clockwise, RiverFlow.Clockwise, RiverFlow.Clockwise },
                        EdgeElevation = 1
                    },
                    Neighbor = new HexCellTestData() { EdgeElevation = 2 }
                }).SetName("previous/neighbor CW river, checking CW, this and neighbor are above previous neighbor").Returns(false);

                yield return new TestCaseData(new CanAddRiverToCellTestData() {
                    FlowToTest = RiverFlow.Counterclockwise,
                    CellToTest = new HexCellTestData() { EdgeElevation = 2 },
                    PreviousNeighbor = new HexCellTestData() {
                        HasRiverAtEdge = new bool[] { false, false, true, false, false, false },
                        FlowOfRiverAtEdge = new RiverFlow[] { RiverFlow.Clockwise, RiverFlow.Clockwise, RiverFlow.Counterclockwise },
                        EdgeElevation = 1
                    },
                    Neighbor = new HexCellTestData() { EdgeElevation = 2 }
                }).SetName("previous/neighbor CWW river, checking CWW, this and neighbor are above previous neighbor").Returns(true);



                yield return new TestCaseData(new CanAddRiverToCellTestData() {
                    FlowToTest = RiverFlow.Clockwise,
                    CellToTest = new HexCellTestData() {
                        HasRiverAtEdge = new bool[] { false, false, true, false, false, false },
                        FlowOfRiverAtEdge = new RiverFlow[] { RiverFlow.Clockwise, RiverFlow.Clockwise, RiverFlow.Clockwise },
                        EdgeElevation = 2
                    },
                    Neighbor = new HexCellTestData() { EdgeElevation = 2 },
                    NextNeighbor = new HexCellTestData() { EdgeElevation = 1 }
                }).SetName("This/next CW river, checking CW, this and neighbor are above next neighbor").Returns(true);

                yield return new TestCaseData(new CanAddRiverToCellTestData() {
                    FlowToTest = RiverFlow.Counterclockwise,
                    CellToTest = new HexCellTestData() {
                        HasRiverAtEdge = new bool[] { false, false, true, false, false, false },
                        FlowOfRiverAtEdge = new RiverFlow[] { RiverFlow.Clockwise, RiverFlow.Clockwise, RiverFlow.Counterclockwise },
                        EdgeElevation = 2
                    },
                    Neighbor = new HexCellTestData() { EdgeElevation = 2 },
                    NextNeighbor = new HexCellTestData() { EdgeElevation = 1 }
                }).SetName("This/next CCW river, checking CCW, this and neighbor are above next neighbor").Returns(false);

                yield return new TestCaseData(new CanAddRiverToCellTestData() {
                    FlowToTest = RiverFlow.Clockwise,
                    CellToTest = new HexCellTestData() { EdgeElevation = 2 },
                    Neighbor = new HexCellTestData() { EdgeElevation = 2 },
                    NextNeighbor = new HexCellTestData() {
                        HasRiverAtEdge = new bool[] { true, false, false, false, false, false },
                        FlowOfRiverAtEdge = new RiverFlow[] { RiverFlow.Clockwise },
                        EdgeElevation = 1
                    }
                }).SetName("next/neighbor CW river, checking CW, this and neighbor are above next neighbor").Returns(true);

                yield return new TestCaseData(new CanAddRiverToCellTestData() {
                    FlowToTest = RiverFlow.Counterclockwise,
                    CellToTest = new HexCellTestData() { EdgeElevation = 2 },
                    Neighbor = new HexCellTestData() { EdgeElevation = 2 },
                    NextNeighbor = new HexCellTestData() {
                        HasRiverAtEdge = new bool[] { true, false, false, false, false, false },
                        FlowOfRiverAtEdge = new RiverFlow[] { RiverFlow.Counterclockwise },
                        EdgeElevation = 1
                    }
                }).SetName("next/neighbor CWW river, checking CWW, this and neighbor are above next neighbor").Returns(false);



                yield return new TestCaseData(new CanAddRiverToCellTestData() {
                    FlowToTest = RiverFlow.Counterclockwise,
                    CellToTest = new HexCellTestData() { EdgeElevation = 1 },
                    PreviousNeighbor = new HexCellTestData() {
                        HasRiverAtEdge = new bool[] { false, false, true, false, false, false },
                        FlowOfRiverAtEdge = new RiverFlow[] { RiverFlow.Clockwise, RiverFlow.Clockwise, RiverFlow.Counterclockwise },
                        EdgeElevation = 2
                    },
                    Neighbor = new HexCellTestData() { EdgeElevation = 2 }
                }).SetName("previous/neighbor CWW river, checking CWW, this is below neighbor and previous neighbor").Returns(false);

                yield return new TestCaseData(new CanAddRiverToCellTestData() {
                    FlowToTest = RiverFlow.Clockwise,
                    CellToTest = new HexCellTestData() { EdgeElevation = 1 },
                    PreviousNeighbor = new HexCellTestData() {
                        HasRiverAtEdge = new bool[] { false, false, true, false, false, false },
                        FlowOfRiverAtEdge = new RiverFlow[] { RiverFlow.Clockwise, RiverFlow.Clockwise, RiverFlow.Clockwise },
                        EdgeElevation = 2
                    },
                    Neighbor = new HexCellTestData() { EdgeElevation = 2 }
                }).SetName("previous/neighbor CW river, checking CW, this is below neighbor and previous neighbor").Returns(true);

                yield return new TestCaseData(new CanAddRiverToCellTestData() {
                    FlowToTest = RiverFlow.Clockwise,
                    CellToTest = new HexCellTestData() { EdgeElevation = 1 },
                    Neighbor = new HexCellTestData() { EdgeElevation = 2 },
                    NextNeighbor = new HexCellTestData() {
                        HasRiverAtEdge = new bool[] { true, false, false, false, false, false },
                        FlowOfRiverAtEdge = new RiverFlow[] { RiverFlow.Clockwise },
                        EdgeElevation = 2
                    }
                }).SetName("next/neighbor CW river, checking CW, this is below neighbor and next neighbor").Returns(false);

                yield return new TestCaseData(new CanAddRiverToCellTestData() {
                    FlowToTest = RiverFlow.Counterclockwise,
                    CellToTest = new HexCellTestData() { EdgeElevation = 1 },
                    Neighbor = new HexCellTestData() { EdgeElevation = 2 },
                    NextNeighbor = new HexCellTestData() {
                        HasRiverAtEdge = new bool[] { true, false, false, false, false, false },
                        FlowOfRiverAtEdge = new RiverFlow[] { RiverFlow.Counterclockwise },
                        EdgeElevation = 2
                    }
                }).SetName("next/neighbor CWW river, checking CWW, this is below neighbor and next neighbor").Returns(true);



                yield return new TestCaseData(new CanAddRiverToCellTestData() {
                    FlowToTest = RiverFlow.Clockwise,
                    CellToTest = new HexCellTestData() {
                        HasRiverAtEdge = new bool[] { true, false, false, false, false, false },
                        FlowOfRiverAtEdge = new RiverFlow[] { RiverFlow.Clockwise },
                        EdgeElevation = 2
                    },
                    PreviousNeighbor = new HexCellTestData() { EdgeElevation = 2 },
                    Neighbor         = new HexCellTestData() { EdgeElevation = 1 }
                }).SetName("this/previous CW river, checking CW, neighbor is below this and previous neighbor").Returns(true);

                yield return new TestCaseData(new CanAddRiverToCellTestData() {
                    FlowToTest = RiverFlow.Counterclockwise,
                    CellToTest = new HexCellTestData() {
                        HasRiverAtEdge = new bool[] { true, false, false, false, false, false },
                        FlowOfRiverAtEdge = new RiverFlow[] { RiverFlow.Counterclockwise },
                        EdgeElevation = 2
                    },
                    PreviousNeighbor = new HexCellTestData() { EdgeElevation = 2 },
                    Neighbor         = new HexCellTestData() { EdgeElevation = 1 }
                }).SetName("this/previous CWW river, checking CWW, neighbor is below this and previous neighbor").Returns(false);

                yield return new TestCaseData(new CanAddRiverToCellTestData() {
                    FlowToTest = RiverFlow.Clockwise,
                    CellToTest = new HexCellTestData() {
                        HasRiverAtEdge = new bool[] { false, false, true, false, false, false },
                        FlowOfRiverAtEdge = new RiverFlow[] { RiverFlow.Clockwise, RiverFlow.Clockwise, RiverFlow.Clockwise },
                        EdgeElevation = 2
                    },
                    Neighbor     = new HexCellTestData() { EdgeElevation = 1 },
                    NextNeighbor = new HexCellTestData() { EdgeElevation = 2 }
                }).SetName("this/next CW river, checking CW, neighbor is below this and next neighbor").Returns(false);

                yield return new TestCaseData(new CanAddRiverToCellTestData() {
                    FlowToTest = RiverFlow.Counterclockwise,
                    CellToTest = new HexCellTestData() {
                        HasRiverAtEdge = new bool[] { false, false, true, false, false, false },
                        FlowOfRiverAtEdge = new RiverFlow[] { RiverFlow.Clockwise, RiverFlow.Clockwise, RiverFlow.Counterclockwise },
                        EdgeElevation = 2
                    },
                    Neighbor     = new HexCellTestData() { EdgeElevation = 1 },
                    NextNeighbor = new HexCellTestData() { EdgeElevation = 2 }
                }).SetName("this/next CWW river, checking CWW, neighbor is below this and next neighbor").Returns(true);



                yield return new TestCaseData(new CanAddRiverToCellTestData() {
                    FlowToTest = RiverFlow.Clockwise,
                    CellToTest = new HexCellTestData() {
                        HasRiverAtEdge = new bool[] { true, false, true, false, false, false },
                        FlowOfRiverAtEdge = new RiverFlow[] { RiverFlow.Clockwise, RiverFlow.Clockwise, RiverFlow.Counterclockwise }
                    },
                    PreviousNeighbor = new HexCellTestData(),
                    Neighbor         = new HexCellTestData(),
                    NextNeighbor     = new HexCellTestData()
                }).SetName("Previous corner is valid, next corner is invalid").Returns(false);

                yield return new TestCaseData(new CanAddRiverToCellTestData() {
                    FlowToTest = RiverFlow.Clockwise,
                    CellToTest = new HexCellTestData() {
                        HasRiverAtEdge = new bool[] { true, false, true, false, false, false },
                        FlowOfRiverAtEdge = new RiverFlow[] { RiverFlow.Counterclockwise, RiverFlow.Clockwise, RiverFlow.Clockwise }
                    },
                    PreviousNeighbor = new HexCellTestData(),
                    Neighbor         = new HexCellTestData(),
                    NextNeighbor     = new HexCellTestData()
                }).SetName("Previous corner is invalid, next corner is valid").Returns(false);
            }
        }

        #endregion

        #region instance fields and properties

        private Mock<IHexGrid> MockGrid;

        private HexCellSignals CellSignals;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockGrid = new Mock<IHexGrid>();

            CellSignals = new HexCellSignals();

            Container.Bind<IHexGrid>      ().FromInstance(MockGrid.Object);
            Container.Bind<HexCellSignals>().FromInstance(CellSignals);

            Container.Bind<RiverCanon>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        [TestCaseSource("CanAddRiverToCellTestCases")]
        public bool CanAddRiverToCellTests(CanAddRiverToCellTestData testData) {
            var cellToTest = BuildHexCell(testData.CellToTest);

            var previousNeighbor = testData.PreviousNeighbor != null ? BuildHexCell(testData.PreviousNeighbor) : null;
            var neighbor         = testData.Neighbor         != null ? BuildHexCell(testData.Neighbor)         : null;
            var nextNeighbor     = testData.NextNeighbor     != null ? BuildHexCell(testData.NextNeighbor)     : null;

            MockGrid.Setup(grid => grid.GetNeighbor(cellToTest, HexDirection.NE)).Returns(previousNeighbor);
            MockGrid.Setup(grid => grid.GetNeighbor(cellToTest, HexDirection.E)) .Returns(neighbor);
            MockGrid.Setup(grid => grid.GetNeighbor(cellToTest, HexDirection.SE)).Returns(nextNeighbor);

            MockGrid.Setup(grid => grid.GetNeighbors(It.IsAny<IHexCell>())).Returns(new List<IHexCell>());

            if(previousNeighbor != null) {
                MockGrid.Setup(grid => grid.GetNeighbor(previousNeighbor, HexDirection.SE)).Returns(neighbor);
            }

            if(neighbor != null) {
                MockGrid.Setup(grid => grid.GetNeighbor(neighbor, HexDirection.NW)).Returns(previousNeighbor);
                MockGrid.Setup(grid => grid.GetNeighbor(neighbor, HexDirection.SW)).Returns(nextNeighbor);
            }

            if(nextNeighbor != null) {
                MockGrid.Setup(grid => grid.GetNeighbor(nextNeighbor, HexDirection.NE)).Returns(neighbor);
            }

            var riverCanon = Container.Resolve<RiverCanon>();

            for(int i = 0; i < 6; i++) {
                if(testData.CellToTest.HasRiverAtEdge[i]) {
                    riverCanon.AddRiverToCell(cellToTest, (HexDirection)i, testData.CellToTest.FlowOfRiverAtEdge[i]);
                }

                if(testData.PreviousNeighbor != null && testData.PreviousNeighbor.HasRiverAtEdge[i]) {
                    riverCanon.AddRiverToCell(previousNeighbor, (HexDirection)i, testData.PreviousNeighbor.FlowOfRiverAtEdge[i]);
                }

                if(testData.Neighbor != null && testData.Neighbor.HasRiverAtEdge[i]) {
                    riverCanon.AddRiverToCell(neighbor, (HexDirection)i, testData.Neighbor.FlowOfRiverAtEdge[i]);
                }

                if(testData.NextNeighbor != null && testData.NextNeighbor.HasRiverAtEdge[i]) {
                    riverCanon.AddRiverToCell(nextNeighbor, (HexDirection)i, testData.NextNeighbor.FlowOfRiverAtEdge[i]);
                }
            }

            return riverCanon.CanAddRiverToCell(cellToTest, HexDirection.E, testData.FlowToTest);
        }


        [Test]
        public void AddRiverToCell_ReflectedInHasRiverAlongEdgeOfThisCell() {
            var cellToTest = BuildHexCell(false, new List<IHexCell>() { BuildHexCell(false) });

            var riverCanon = Container.Resolve<RiverCanon>();

            riverCanon.AddRiverToCell(cellToTest, (HexDirection)0, RiverFlow.Counterclockwise);

            Assert.IsTrue(riverCanon.HasRiverAlongEdge(cellToTest, (HexDirection)0));
        }

        [Test]
        public void AddRiverToCell_ReflectedInHasRiverAlongEdgeOfNeighbor() {
            var neighbors = new List<IHexCell>() { BuildHexCell(false) };

            var cellToTest = BuildHexCell(false, neighbors);

            var riverCanon = Container.Resolve<RiverCanon>();

            riverCanon.AddRiverToCell(cellToTest, (HexDirection)0, RiverFlow.Counterclockwise);

            Assert.IsTrue(riverCanon.HasRiverAlongEdge(neighbors[0], ((HexDirection)0).Opposite()));
        }

        [Test]
        public void AddRiverToCell_ReflectedInGetEdgesWithRiversOfThisCell() {
            var cellToTest = BuildHexCell(false, new List<IHexCell>() { BuildHexCell(false) });

            var riverCanon = Container.Resolve<RiverCanon>();

            riverCanon.AddRiverToCell(cellToTest, (HexDirection)0, RiverFlow.Counterclockwise);

            CollectionAssert.AreEquivalent(
                new List<HexDirection>() { (HexDirection)0 }, riverCanon.GetEdgesWithRivers(cellToTest)
            );
        }

        [Test]
        public void AddRiverToCell_ReflectedInGetEdgesWithRiversOfNeighbor() {
            var neighbors = new List<IHexCell>() { BuildHexCell(false) };

            var cellToTest = BuildHexCell(false, neighbors);

            var riverCanon = Container.Resolve<RiverCanon>();

            riverCanon.AddRiverToCell(cellToTest, (HexDirection)0, RiverFlow.Counterclockwise);

            CollectionAssert.AreEquivalent(
                new List<HexDirection>() { ((HexDirection)0).Opposite() }, riverCanon.GetEdgesWithRivers(neighbors[0])
            );
        }

        [Test]
        public void AddRiverToCell_ReflectedInGetFlowDirectionOfRiverAtEdgeOfThisCell() {
            var cellToTest = BuildHexCell(false, new List<IHexCell>() { BuildHexCell(false) });

            var riverCanon = Container.Resolve<RiverCanon>();

            riverCanon.AddRiverToCell(cellToTest, (HexDirection)0, RiverFlow.Counterclockwise);

            Assert.AreEqual(
                RiverFlow.Counterclockwise, riverCanon.GetFlowOfRiverAtEdge(cellToTest, (HexDirection)0)
            );
        }

        [Test]
        public void AddRiverToCell_ReflectedInGetFlowDirectionOfRiverAtEdgeOfNeighbor() {
            var neighbors = new List<IHexCell>() { BuildHexCell(false) };

            var cellToTest = BuildHexCell(false, neighbors);

            var riverCanon = Container.Resolve<RiverCanon>();

            riverCanon.AddRiverToCell(cellToTest, (HexDirection)0, RiverFlow.Counterclockwise);

            Assert.AreEqual(
                RiverFlow.Clockwise, riverCanon.GetFlowOfRiverAtEdge(neighbors[0], ((HexDirection)0).Opposite())
            );
        }

        [Test]
        public void AddRiverToCell_RefreshesCellAndAllNeighbors() {
            Mock<IHexCell> mockNeighborOne, mockNeighborTwo, mockNeighborThree, mockCellToTest;

            var neighbors = new List<IHexCell>() {
                BuildHexCell(false, out mockNeighborOne),
                BuildHexCell(false, out mockNeighborTwo),
                BuildHexCell(false, out mockNeighborThree),
            };

            var cellToTest = BuildHexCell(false, neighbors, out mockCellToTest);

            var riverCanon = Container.Resolve<RiverCanon>();

            riverCanon.AddRiverToCell(cellToTest, (HexDirection)0, RiverFlow.Counterclockwise);

            mockCellToTest   .Verify(cell => cell.RefreshSelfOnly(), Times.Once, "CellToTest.RefreshSelfOnly was not called");

            mockNeighborOne  .Verify(cell => cell.RefreshSelfOnly(), Times.Once, "NeighborOne.RefreshSelfOnly was not called");
            mockNeighborTwo  .Verify(cell => cell.RefreshSelfOnly(), Times.Once, "NeighborTwo.RefreshSelfOnly was not called");
            mockNeighborThree.Verify(cell => cell.RefreshSelfOnly(), Times.Once, "NeighborThree.RefreshSelfOnly was not called");
        }

        [Test]
        public void AddRiverToCell_ThrowsIfCanAddRiverToCellFalse() {
            var cellToTest = BuildHexCell(false, new List<IHexCell>() { BuildHexCell(true) });

            var riverCanon = Container.Resolve<RiverCanon>();

            Assert.Throws<InvalidOperationException>(
                () => riverCanon.AddRiverToCell(cellToTest, (HexDirection)0, RiverFlow.Clockwise)
            );

            Assert.Throws<InvalidOperationException>(
                () => riverCanon.AddRiverToCell(cellToTest, (HexDirection)0, RiverFlow.Counterclockwise)
            );
        }


        [Test]
        public void HasRiver_TrueIfSomeEdgeHasRiver() {
            var cellToTest = BuildHexCell(false, new List<IHexCell>() { BuildHexCell(false) });

            var riverCanon = Container.Resolve<RiverCanon>();

            riverCanon.AddRiverToCell(cellToTest, (HexDirection)0, RiverFlow.Clockwise);

            Assert.IsTrue(riverCanon.HasRiver(cellToTest));
        }

        [Test]
        public void HasRiver_FalseIfNoEdgeHasRiver() {
            var cellToTest = BuildHexCell(false, new List<IHexCell>() { BuildHexCell(false) });

            var riverCanon = Container.Resolve<RiverCanon>();

            Assert.IsFalse(riverCanon.HasRiver(cellToTest));
        }


        [Test]
        public void GetFlowDirectionOfRiverAtEdge_ThrowsIfNoRiverInDirection() {
            var cellToTest = BuildHexCell(false, new List<IHexCell>() { BuildHexCell(false) });

            var riverCanon = Container.Resolve<RiverCanon>();

            riverCanon.AddRiverToCell(cellToTest, (HexDirection)0, RiverFlow.Clockwise);

            Assert.Throws<InvalidOperationException>(() => riverCanon.GetFlowOfRiverAtEdge(cellToTest, (HexDirection)1));
        }


        [Test]
        public void RemoveRiverFromCellInDirection_ReflectedInHasRiverAlongEdgeOfThisCell() {
            var cellToTest = BuildHexCell(false, new List<IHexCell>() { BuildHexCell(false) });

            var riverCanon = Container.Resolve<RiverCanon>();

            riverCanon.AddRiverToCell(cellToTest, (HexDirection)0, RiverFlow.Counterclockwise);
            riverCanon.RemoveRiverFromCellInDirection(cellToTest, (HexDirection)0);

            Assert.IsFalse(riverCanon.HasRiverAlongEdge(cellToTest, (HexDirection)0));
        }

        [Test]
        public void RemoveRiverFromCellInDirection_ReflectedInHasRiverAlongEdgeOfNeighbor() {
            var neighbors = new List<IHexCell>() { BuildHexCell(false) };

            var cellToTest = BuildHexCell(false, neighbors);

            var riverCanon = Container.Resolve<RiverCanon>();

            riverCanon.AddRiverToCell(cellToTest, (HexDirection)0, RiverFlow.Counterclockwise);
            riverCanon.RemoveRiverFromCellInDirection(cellToTest, (HexDirection)0);

            Assert.IsFalse(riverCanon.HasRiverAlongEdge(neighbors[0], ((HexDirection)0).Opposite()));
        }

        [Test]
        public void RemoveRiverFromCellInDirection_ReflectedInGetEdgesWithRiversOfThisCell() {
            var cellToTest = BuildHexCell(false, new List<IHexCell>() { BuildHexCell(false) });

            var riverCanon = Container.Resolve<RiverCanon>();

            riverCanon.AddRiverToCell(cellToTest, (HexDirection)0, RiverFlow.Counterclockwise);
            riverCanon.RemoveRiverFromCellInDirection(cellToTest, (HexDirection)0);

            CollectionAssert.DoesNotContain(riverCanon.GetEdgesWithRivers(cellToTest), (HexDirection)0);
        }

        [Test]
        public void RemoveRiverFromCellInDirection_ReflectedInGetEdgesWithRiversOfNeighbor() {
            var neighbors = new List<IHexCell>() { BuildHexCell(false) };

            var cellToTest = BuildHexCell(false, neighbors);

            var riverCanon = Container.Resolve<RiverCanon>();

            riverCanon.AddRiverToCell(cellToTest, (HexDirection)0, RiverFlow.Counterclockwise);
            riverCanon.RemoveRiverFromCellInDirection(cellToTest, (HexDirection)0);

            CollectionAssert.DoesNotContain(riverCanon.GetEdgesWithRivers(neighbors[0]), ((HexDirection)0).Opposite());
        }

        [Test]
        public void RemoveRiverFromCellInDirection_CellAndAllNeighborsRefreshed() {
            Mock<IHexCell> mockNeighborOne, mockNeighborTwo, mockNeighborThree, mockCellToTest;

            var neighbors = new List<IHexCell>() {
                BuildHexCell(false, out mockNeighborOne),
                BuildHexCell(false, out mockNeighborTwo),
                BuildHexCell(false, out mockNeighborThree),
            };

            var cellToTest = BuildHexCell(false, neighbors, out mockCellToTest);

            var riverCanon = Container.Resolve<RiverCanon>();

            riverCanon.AddRiverToCell(cellToTest, (HexDirection)0, RiverFlow.Counterclockwise);

            mockCellToTest   .ResetCalls();
            mockNeighborOne  .ResetCalls();
            mockNeighborTwo  .ResetCalls();
            mockNeighborThree.ResetCalls();

            riverCanon.RemoveRiverFromCellInDirection(cellToTest, (HexDirection)0);

            mockCellToTest   .Verify(cell => cell.RefreshSelfOnly(), Times.Once, "CellToTest.RefreshSelfOnly was not called");

            mockNeighborOne  .Verify(cell => cell.RefreshSelfOnly(), Times.Once, "NeighborOne.RefreshSelfOnly was not called");
            mockNeighborTwo  .Verify(cell => cell.RefreshSelfOnly(), Times.Once, "NeighborTwo.RefreshSelfOnly was not called");
            mockNeighborThree.Verify(cell => cell.RefreshSelfOnly(), Times.Once, "NeighborThree.RefreshSelfOnly was not called");
        }

        [Test]
        public void RemoveRiverFromCellInDirection_DoesNotThrowIfNoRiver() {
            var neighbors = new List<IHexCell>() { BuildHexCell(false) };

            var cellToTest = BuildHexCell(false, neighbors);

            var riverCanon = Container.Resolve<RiverCanon>();

            Assert.DoesNotThrow(() =>  riverCanon.RemoveRiverFromCellInDirection(cellToTest, (HexDirection)0));
        }


        [Test]
        public void RemoveAllRiversFromCell_AllRiversRemoved() {
            var neighbors = new List<IHexCell>() {
                BuildHexCell(false),
                BuildHexCell(false),
                BuildHexCell(false),
            };

            var cellToTest = BuildHexCell(false, neighbors);

            var riverCanon = Container.Resolve<RiverCanon>();

            riverCanon.AddRiverToCell(cellToTest, (HexDirection)0, RiverFlow.Counterclockwise);
            riverCanon.AddRiverToCell(cellToTest, (HexDirection)1, RiverFlow.Counterclockwise);
            riverCanon.AddRiverToCell(cellToTest, (HexDirection)2, RiverFlow.Counterclockwise);

            riverCanon.RemoveAllRiversFromCell(cellToTest);

            CollectionAssert.IsEmpty(
                riverCanon.GetEdgesWithRivers(cellToTest),
                "GetEdgesWithRivers returned a non-empty collection"
            );
        }


        [Test]
        public void ValidateRivers_RemovesAllInvalidRivers() {
            Mock<IHexCell> mockNeighborOne, mockNeighborTwo, mockNeighborThree;

            var neighbors = new List<IHexCell>() {
                BuildHexCell(false, out mockNeighborOne),
                BuildHexCell(false, out mockNeighborTwo),
                BuildHexCell(false, out mockNeighborThree),
            };

            var cellToTest = BuildHexCell(false, neighbors);

            var riverCanon = Container.Resolve<RiverCanon>();

            riverCanon.AddRiverToCell(cellToTest, (HexDirection)0, RiverFlow.Counterclockwise);
            riverCanon.AddRiverToCell(cellToTest, (HexDirection)1, RiverFlow.Counterclockwise);
            riverCanon.AddRiverToCell(cellToTest, (HexDirection)2, RiverFlow.Counterclockwise);

            mockNeighborOne  .Setup(cell => cell.Terrain).Returns(CellTerrain.FreshWater);
            mockNeighborThree.Setup(cell => cell.Terrain).Returns(CellTerrain.FreshWater);

            riverCanon.ValidateRivers(cellToTest);

            CollectionAssert.AreEquivalent(
                new List<HexDirection>() { (HexDirection)1 },
                riverCanon.GetEdgesWithRivers(cellToTest),
                "GetEdgesWithRivers returned an unexpected value"
            );
        }

        #endregion

        #region utilities

        private IHexCell BuildHexCell(bool isUnderwater) {
            Mock<IHexCell> mock;
            return BuildHexCell(isUnderwater, new List<IHexCell>(), out mock);
        }

        private IHexCell BuildHexCell(bool isUnderwater, out Mock<IHexCell> mock) {
            return BuildHexCell(isUnderwater, new List<IHexCell>(), out mock);
        }

        private IHexCell BuildHexCell(bool isUnderwater, List<IHexCell> neighbors) {
            Mock<IHexCell> mock;
            return BuildHexCell(isUnderwater, neighbors, out mock);
        }

        private IHexCell BuildHexCell(bool isUnderwater, List<IHexCell> neighbors, out Mock<IHexCell> mock) {
            mock = new Mock<IHexCell>();

            mock.Setup(cell => cell.Terrain).Returns(isUnderwater ? CellTerrain.FreshWater : CellTerrain.Grassland);

            var newCell = mock.Object;

            MockGrid.Setup(grid => grid.GetNeighbors(newCell)).Returns(neighbors);

            for(int i = 0; i < neighbors.Count; i++) {
                var direction = (HexDirection)i;
                var opposite = direction.Opposite();
                var neighbor = neighbors[i];

                MockGrid.Setup(grid => grid.GetNeighbor(newCell,  direction)).Returns(neighbors[i]);
                MockGrid.Setup(grid => grid.GetNeighbor(neighbor, opposite)) .Returns(newCell);
            }

            return newCell;
        }

        private IHexCell BuildHexCell(HexCellTestData testData) {
            var mockCell = new Mock<IHexCell>();

            mockCell.Setup(cell => cell.Terrain).Returns(testData.IsUnderwater ? CellTerrain.FreshWater : CellTerrain.Grassland);
            mockCell.Setup(cell => cell.Shape).Returns(testData.Shape);
            mockCell.Setup(cell => cell.EdgeElevation).Returns(testData.EdgeElevation);

            var newCell = mockCell.Object;

            return newCell;
        }

        #endregion

        #endregion

    }

}
