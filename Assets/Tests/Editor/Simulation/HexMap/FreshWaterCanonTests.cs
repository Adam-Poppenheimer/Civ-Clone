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

            public TerrainType Terrain;

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
                        new HexCellTestData() { Terrain = TerrainType.Grassland },
                        new HexCellTestData() { Terrain = TerrainType.ShallowWater },
                        new HexCellTestData() { Terrain = TerrainType.DeepWater }
                    }
                }).SetName("Cell with no rivers, non-FreshWater neighbors").Returns(false);

                yield return new TestCaseData(new HasAccessToFreshWaterTestData() {
                    Cell = new HexCellTestData(),
                    Neighbors = new List<HexCellTestData>() {
                        new HexCellTestData() { Terrain = TerrainType.FreshWater },
                        new HexCellTestData() { Terrain = TerrainType.FreshWater },
                        new HexCellTestData() { Terrain = TerrainType.FreshWater }
                    }
                }).SetName("Cell with no rivers, FreshWater neighbors").Returns(true);

                yield return new TestCaseData(new HasAccessToFreshWaterTestData() {
                    Cell = new HexCellTestData() { HasRiver = true },
                    Neighbors = new List<HexCellTestData>() {
                        new HexCellTestData() { Terrain = TerrainType.Grassland },
                        new HexCellTestData() { Terrain = TerrainType.ShallowWater },
                        new HexCellTestData() { Terrain = TerrainType.DeepWater }
                    }
                }).SetName("Cell with a river, non-FreshWater neighbors").Returns(true);

                yield return new TestCaseData(new HasAccessToFreshWaterTestData() {
                    Cell = new HexCellTestData(),
                    Neighbors = new List<HexCellTestData>() {
                        new HexCellTestData() { Terrain = TerrainType.Grassland },
                        new HexCellTestData() { Terrain = TerrainType.ShallowWater },
                        new HexCellTestData() { Terrain = TerrainType.FreshWater }
                    }
                }).SetName("Cell with no rivers, mix of FreshWater and non-FreshWater neighbors").Returns(true);

                yield return new TestCaseData(new HasAccessToFreshWaterTestData() {
                    Cell = new HexCellTestData() { Terrain = TerrainType.ShallowWater }
                }).SetName("Cell is ShallowWater, adjacent sources of fresh water").Returns(false);

                yield return new TestCaseData(new HasAccessToFreshWaterTestData() {
                    Cell = new HexCellTestData() { Terrain = TerrainType.DeepWater }
                }).SetName("Cell is DeepWater, adjacent sources of fresh water").Returns(false);

                yield return new TestCaseData(new HasAccessToFreshWaterTestData() {
                    Cell = new HexCellTestData() { Terrain = TerrainType.FreshWater }
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

            Container.Bind<FreshWaterCanon>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        [TestCaseSource("HasAccessToFreshWaterTestCases")]
        public bool HasAccessToFreshWaterTests(HasAccessToFreshWaterTestData testData) {
            var cell = BuildHexCell(testData.Cell);

            MockGrid.Setup(grid => grid.GetNeighbors(cell))
                    .Returns(testData.Neighbors.Select(cellData => BuildHexCell(cellData)).ToList());

            var freshWaterCanon = Container.Resolve<FreshWaterCanon>();

            return freshWaterCanon.HasAccessToFreshWater(cell);
        }

        #endregion

        #region utilities

        private IHexCell BuildHexCell(HexCellTestData testData) {
            var mockCell = new Mock<IHexCell>();

            mockCell.Setup(cell => cell.Terrain).Returns(testData.Terrain);

            var newCell = mockCell.Object;

            MockRiverCanon.Setup(canon => canon.HasRiver(newCell)).Returns(testData.HasRiver);

            return newCell;
        }

        #endregion

        #endregion

    }

}
