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
    public class FreshWaterCanonTests : ZenjectUnitTestFixture {

        #region internal types

        public class HasAccessToFreshWaterTestData {

            public HexCellTestData Cell;

            public List<HexCellTestData> Neighbors = new List<HexCellTestData>();

        }

        public class HexCellTestData {

            public bool HasRiver;

            public CellTerrain Terrain;

            public CellFeature Feature;

        }

        #endregion

        #region static fields and properties

        public static IEnumerable HasAccessToFreshWaterTestCases {
            get {
                yield return new TestCaseData(new HasAccessToFreshWaterTestData() {
                    Cell = new HexCellTestData()
                }).SetName("Cell with no rivers, no neighbors").Returns(false);

                yield return new TestCaseData(new HasAccessToFreshWaterTestData() {
                    Cell = new HexCellTestData() { HasRiver = true }
                }).SetName("Cell with a river, no neighbors").Returns(true);

                yield return new TestCaseData(new HasAccessToFreshWaterTestData() {
                    Cell = new HexCellTestData(),
                    Neighbors = new List<HexCellTestData>() {
                        new HexCellTestData() { Terrain = CellTerrain.Grassland },
                        new HexCellTestData() { Terrain = CellTerrain.ShallowWater },
                        new HexCellTestData() { Terrain = CellTerrain.DeepWater }
                    }
                }).SetName("Cell with no rivers, non-FreshWater neighbors").Returns(false);

                yield return new TestCaseData(new HasAccessToFreshWaterTestData() {
                    Cell = new HexCellTestData(),
                    Neighbors = new List<HexCellTestData>() {
                        new HexCellTestData() { Terrain = CellTerrain.FreshWater },
                        new HexCellTestData() { Terrain = CellTerrain.FreshWater },
                        new HexCellTestData() { Terrain = CellTerrain.FreshWater }
                    }
                }).SetName("Cell with no rivers, FreshWater neighbors").Returns(true);

                yield return new TestCaseData(new HasAccessToFreshWaterTestData() {
                    Cell = new HexCellTestData() { HasRiver = true },
                    Neighbors = new List<HexCellTestData>() {
                        new HexCellTestData() { Terrain = CellTerrain.Grassland },
                        new HexCellTestData() { Terrain = CellTerrain.ShallowWater },
                        new HexCellTestData() { Terrain = CellTerrain.DeepWater }
                    }
                }).SetName("Cell with a river, non-FreshWater neighbors").Returns(true);

                yield return new TestCaseData(new HasAccessToFreshWaterTestData() {
                    Cell = new HexCellTestData(),
                    Neighbors = new List<HexCellTestData>() {
                        new HexCellTestData() { Terrain = CellTerrain.Grassland },
                        new HexCellTestData() { Terrain = CellTerrain.ShallowWater },
                        new HexCellTestData() { Terrain = CellTerrain.FreshWater }
                    }
                }).SetName("Cell with no rivers, mix of FreshWater and non-FreshWater neighbors").Returns(true);



                yield return new TestCaseData(new HasAccessToFreshWaterTestData() {
                    Cell = new HexCellTestData() { Feature = CellFeature.Oasis },
                    Neighbors = new List<HexCellTestData>() {

                    }
                }).SetName("Cell with no rivers, cell has oasis").Returns(true);

                yield return new TestCaseData(new HasAccessToFreshWaterTestData() {
                    Cell = new HexCellTestData() { Feature = CellFeature.Oasis },
                    Neighbors = new List<HexCellTestData>() {
                        new HexCellTestData() { Feature = CellFeature.None },
                        new HexCellTestData() { Feature = CellFeature.Oasis },
                        new HexCellTestData() { Feature = CellFeature.None }
                    }
                }).SetName("Cell with no rivers and no oasis, some adjacent cell has an oasis").Returns(true);



                yield return new TestCaseData(new HasAccessToFreshWaterTestData() {
                    Cell = new HexCellTestData() { Terrain = CellTerrain.ShallowWater }
                }).SetName("Cell is ShallowWater, adjacent sources of fresh water").Returns(false);

                yield return new TestCaseData(new HasAccessToFreshWaterTestData() {
                    Cell = new HexCellTestData() { Terrain = CellTerrain.DeepWater }
                }).SetName("Cell is DeepWater, adjacent sources of fresh water").Returns(false);

                yield return new TestCaseData(new HasAccessToFreshWaterTestData() {
                    Cell = new HexCellTestData() { Terrain = CellTerrain.FreshWater }
                }).SetName("Cell is FreshWater, no adjacent sources of fresh water").Returns(true);
            }
        }

        #endregion

        #region instance fields and properties

        private Mock<IHexGrid>    MockGrid;
        private Mock<IRiverCanon> MockRiverCanon;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockGrid       = new Mock<IHexGrid>();
            MockRiverCanon = new Mock<IRiverCanon>();

            Container.Bind<IHexGrid>()   .FromInstance(MockGrid      .Object);
            Container.Bind<IRiverCanon>().FromInstance(MockRiverCanon.Object);

            Container.Bind<FreshWaterLogic>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        [TestCaseSource("HasAccessToFreshWaterTestCases")]
        public bool HasAccessToFreshWaterTests(HasAccessToFreshWaterTestData testData) {
            var cell = BuildHexCell(testData.Cell);

            MockGrid.Setup(grid => grid.GetNeighbors(cell))
                    .Returns(testData.Neighbors.Select(cellData => BuildHexCell(cellData)).ToList());

            var freshWaterCanon = Container.Resolve<FreshWaterLogic>();

            return freshWaterCanon.HasAccessToFreshWater(cell);
        }

        #endregion

        #region utilities

        private IHexCell BuildHexCell(HexCellTestData testData) {
            var mockCell = new Mock<IHexCell>();

            mockCell.Setup(cell => cell.Terrain).Returns(testData.Terrain);
            mockCell.Setup(cell => cell.Feature).Returns(testData.Feature);

            var newCell = mockCell.Object;

            MockRiverCanon.Setup(canon => canon.HasRiver(newCell)).Returns(testData.HasRiver);

            return newCell;
        }

        #endregion

        #endregion

    }

}
