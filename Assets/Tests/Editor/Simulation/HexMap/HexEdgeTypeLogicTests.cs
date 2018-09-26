using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using Moq;
using NUnit.Framework;

using Assets.Simulation.HexMap;

namespace Assets.Tests.Simulation.HexMap {

    public class HexEdgeTypeLogicTests : ZenjectUnitTestFixture {

        #region internal types

        public class GetEdgeTypeBetweenCellsTestData {

            public HexCellTestData CellOne;
            public HexCellTestData CellTwo;

            public bool HasRiverBetween;

        }

        public class HexCellTestData {

            public bool IsWater = false;

            public int EdgeElevation = 0;

        }

        #endregion

        #region static fields and properties

        public static IEnumerable GetEdgeTypeBetweenCellsTestCases {
            get {
                yield return new TestCaseData(new GetEdgeTypeBetweenCellsTestData() {
                    CellOne = null,
                    CellTwo = new HexCellTestData()
                }).SetName("CellOne is null").Returns(HexEdgeType.Void);

                yield return new TestCaseData(new GetEdgeTypeBetweenCellsTestData() {
                    CellOne = new HexCellTestData(),
                    CellTwo = null
                }).SetName("CellTwo is null").Returns(HexEdgeType.Void);

                yield return new TestCaseData(new GetEdgeTypeBetweenCellsTestData() {
                    CellOne = new HexCellTestData() { IsWater = true,  EdgeElevation = 1 },
                    CellTwo = new HexCellTestData() { IsWater = false, EdgeElevation = 1 }
                }).SetName("CellOne is water, CellTwo is at same elevation").Returns(HexEdgeType.Flat);

                yield return new TestCaseData(new GetEdgeTypeBetweenCellsTestData() {
                    CellOne = new HexCellTestData() { IsWater = true,  EdgeElevation = 1 },
                    CellTwo = new HexCellTestData() { IsWater = false, EdgeElevation = 2 }
                }).SetName("CellOne is water, CellTwo is at different elevation").Returns(HexEdgeType.Flat);

                yield return new TestCaseData(new GetEdgeTypeBetweenCellsTestData() {
                    CellOne = new HexCellTestData() { IsWater = false, EdgeElevation = 1 },
                    CellTwo = new HexCellTestData() { IsWater = true,  EdgeElevation = 1 }
                }).SetName("CellTwo is water, CellOne is at same elevation").Returns(HexEdgeType.Flat);

                yield return new TestCaseData(new GetEdgeTypeBetweenCellsTestData() {
                    CellOne = new HexCellTestData() { IsWater = false, EdgeElevation = 2 },
                    CellTwo = new HexCellTestData() { IsWater = true,  EdgeElevation = 1 }
                }).SetName("CellTwo is water, CellOne is at different elevation").Returns(HexEdgeType.Flat);

                yield return new TestCaseData(new GetEdgeTypeBetweenCellsTestData() {
                    CellOne = new HexCellTestData() { EdgeElevation = 1 },
                    CellTwo = new HexCellTestData() { EdgeElevation = 1 },
                    HasRiverBetween = true
                }).SetName("River exists between cells, same elevation").Returns(HexEdgeType.River);

                yield return new TestCaseData(new GetEdgeTypeBetweenCellsTestData() {
                    CellOne = new HexCellTestData() { EdgeElevation = 1 },
                    CellTwo = new HexCellTestData() { EdgeElevation = 2 },
                    HasRiverBetween = true
                }).SetName("River exists between cells, different elevation").Returns(HexEdgeType.River);

                yield return new TestCaseData(new GetEdgeTypeBetweenCellsTestData() {
                    CellOne = new HexCellTestData() { EdgeElevation = 2 },
                    CellTwo = new HexCellTestData() { EdgeElevation = 1 }
                }).SetName("CellOne 1 higher than CellTwo").Returns(HexEdgeType.Slope);

                yield return new TestCaseData(new GetEdgeTypeBetweenCellsTestData() {
                    CellOne = new HexCellTestData() { EdgeElevation = 3 },
                    CellTwo = new HexCellTestData() { EdgeElevation = 1 }
                }).SetName("CellOne 2 higher than CellTwo").Returns(HexEdgeType.Cliff);

                yield return new TestCaseData(new GetEdgeTypeBetweenCellsTestData() {
                    CellOne = new HexCellTestData() { EdgeElevation = 4 },
                    CellTwo = new HexCellTestData() { EdgeElevation = 1 }
                }).SetName("CellOne 3 higher than CellTwo").Returns(HexEdgeType.Cliff);

                yield return new TestCaseData(new GetEdgeTypeBetweenCellsTestData() {
                    CellOne = new HexCellTestData() { EdgeElevation = 1 },
                    CellTwo = new HexCellTestData() { EdgeElevation = 2 }
                }).SetName("CellOne 1 lower than CellTwo").Returns(HexEdgeType.Slope);

                yield return new TestCaseData(new GetEdgeTypeBetweenCellsTestData() {
                    CellOne = new HexCellTestData() { EdgeElevation = 1 },
                    CellTwo = new HexCellTestData() { EdgeElevation = 3 }
                }).SetName("CellOne 2 lower than CellTwo").Returns(HexEdgeType.Cliff);

                yield return new TestCaseData(new GetEdgeTypeBetweenCellsTestData() {
                    CellOne = new HexCellTestData() { EdgeElevation = 1 },
                    CellTwo = new HexCellTestData() { EdgeElevation = 4 }
                }).SetName("CellOne 3 lower than CellTwo").Returns(HexEdgeType.Cliff);
            }
        }

        #endregion

        #region instance fields and properties

        private Mock<IRiverCanon> MockRiverCanon;
        private Mock<IHexGrid>    MockGrid;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockRiverCanon = new Mock<IRiverCanon>();
            MockGrid       = new Mock<IHexGrid>();

            Container.Bind<IRiverCanon>().FromInstance(MockRiverCanon.Object);
            Container.Bind<IHexGrid>   ().FromInstance(MockGrid      .Object);

            Container.Bind<HexEdgeTypeLogic>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        [TestCaseSource("GetEdgeTypeBetweenCellsTestCases")]
        public HexEdgeType GetEdgeTypeBetweenCellsTests(GetEdgeTypeBetweenCellsTestData testData) {
            var cellOne = testData.CellOne == null ? null : BuildHexCell(testData.CellOne);
            var cellTwo = testData.CellTwo == null ? null : BuildHexCell(testData.CellTwo);

            MockGrid.Setup(grid => grid.GetNeighbor(cellOne, HexDirection.E)).Returns(cellTwo);
            MockGrid.Setup(grid => grid.GetNeighbor(cellTwo, HexDirection.W)).Returns(cellOne);

            MockRiverCanon.Setup(canon => canon.HasRiverAlongEdge(cellOne, HexDirection.E)).Returns(testData.HasRiverBetween);
            MockRiverCanon.Setup(canon => canon.HasRiverAlongEdge(cellTwo, HexDirection.W)).Returns(testData.HasRiverBetween);

            var edgeTypeLogic = Container.Resolve<HexEdgeTypeLogic>();

            return edgeTypeLogic.GetEdgeTypeBetweenCells(cellOne, cellTwo);
        }

        #endregion

        #region utilities

        private IHexCell BuildHexCell(HexCellTestData cellData) {
            var mockCell = new Mock<IHexCell>();

            mockCell.Setup(cell => cell.Terrain)      .Returns(cellData.IsWater ? CellTerrain.ShallowWater : CellTerrain.Grassland);
            mockCell.Setup(cell => cell.EdgeElevation).Returns(cellData.EdgeElevation);

            return mockCell.Object;
        }

        #endregion

        #endregion

    }

}
