using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using NUnit.Framework;
using Moq;
using UniRx;

using Assets.Simulation.HexMap;

namespace Assets.Tests.Simulation.HexMap {

    [TestFixture]
    public class RiverCanonTests : ZenjectUnitTestFixture {

        #region internal types

        public class CanAddRiverToCellTestData {

            public HexCellTestData Center    = new HexCellTestData();
            public HexCellTestData Left      = new HexCellTestData();
            public HexCellTestData Right     = new HexCellTestData();
            public HexCellTestData NextRight = new HexCellTestData();

            public RiverFlow FlowToTest;

        }

        public class HexCellTestData {

            public bool IsUnderwater;

        }

        public class RiverData {

            public RiverFlow Flow;

        }

        #endregion

        #region static fields and properties

        public static IEnumerable CanAddRiverToCell_NoRiverCases {
            get {
                yield return new TestCaseData(new CanAddRiverToCellTestData() {

                }).SetName("All cells exist and are land").Returns(true);

                yield return new TestCaseData(new CanAddRiverToCellTestData() {
                    Right     = new HexCellTestData() { IsUnderwater = true },

                }).SetName("Right exists and is underwater").Returns(false);

                yield return new TestCaseData(new CanAddRiverToCellTestData() {
                    Right     = null,

                }).SetName("Right does not exist").Returns(false);

                yield return new TestCaseData(new CanAddRiverToCellTestData() {
                    Center    = new HexCellTestData() { IsUnderwater = true },

                }).SetName("Right is land, Center is water").Returns(false);

                yield return new TestCaseData(new CanAddRiverToCellTestData() {
                    Left      = null,

                }).SetName("Left does not exist").Returns(true);

                yield return new TestCaseData(new CanAddRiverToCellTestData() {
                    NextRight = null

                }).SetName("NextRight does not exist").Returns(true);
            }
        }

        #endregion

        #region instance fields and properties

        private Mock<IHexGrid>                  MockGrid;
        private Mock<IRiverCornerValidityLogic> MockRiverCornerValidityLogic;

        private HexCellSignals CellSignals;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockGrid                     = new Mock<IHexGrid>();
            MockRiverCornerValidityLogic = new Mock<IRiverCornerValidityLogic>();

            CellSignals = new HexCellSignals();

            Container.Bind<IHexGrid>                 ().FromInstance(MockGrid                    .Object);
            Container.Bind<IRiverCornerValidityLogic>().FromInstance(MockRiverCornerValidityLogic.Object);
            Container.Bind<HexCellSignals>           ().FromInstance(CellSignals);

            Container.Bind<RiverCanon>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        [TestCaseSource("CanAddRiverToCell_NoRiverCases")]
        public bool CanAddRiverToCellTests(CanAddRiverToCellTestData testData) {
            var center = BuildHexCell(testData.Center);

            var left      = testData.Left      != null ? BuildHexCell(testData.Left)      : null;
            var right     = testData.Right     != null ? BuildHexCell(testData.Right)     : null;
            var nextRight = testData.NextRight != null ? BuildHexCell(testData.NextRight) : null;

            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.NE)).Returns(left);
            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.E)) .Returns(right);
            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.SE)).Returns(nextRight);

            MockGrid.Setup(grid => grid.GetNeighbors(It.IsAny<IHexCell>())).Returns(new List<IHexCell>());

            MockRiverCornerValidityLogic.Setup(
                logic => logic.AreCornerFlowsValid(It.IsAny<RiverFlow>(), It.IsAny<RiverFlow?>(), It.IsAny<RiverFlow?>())
            ).Returns(true);

            var riverCanon = Container.Resolve<RiverCanon>();

            return riverCanon.CanAddRiverToCell(center, HexDirection.E, testData.FlowToTest);
        }

        public void CanAddRiverToCell_FalseIfPreviousCornerInvalid() {
            var center    = BuildHexCell(new HexCellTestData());
            var left      = BuildHexCell(new HexCellTestData());
            var right     = BuildHexCell(new HexCellTestData());
            var nextRight = BuildHexCell(new HexCellTestData());

            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.NE)).Returns(left);
            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.E )).Returns(right);
            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.SE)).Returns(nextRight);

            MockRiverCornerValidityLogic.Setup(
                logic => logic.AreCornerFlowsValid(RiverFlow.Clockwise, null, RiverFlow.Clockwise)
            ).Returns(false);

            MockRiverCornerValidityLogic.Setup(
                logic => logic.AreCornerFlowsValid(RiverFlow.Clockwise, RiverFlow.Counterclockwise, null)
            ).Returns(true);

            var riverCanon = Container.Resolve<RiverCanon>();

            riverCanon.OverrideRiverOnCell(center, HexDirection.E,  RiverFlow.Clockwise);
            riverCanon.OverrideRiverOnCell(left,   HexDirection.SE, RiverFlow.Clockwise);
            riverCanon.OverrideRiverOnCell(center, HexDirection.SE, RiverFlow.Counterclockwise);

            Assert.IsFalse(riverCanon.CanAddRiverToCell(center, HexDirection.E, RiverFlow.Clockwise));
        }

        public void CanAddRiverToCell_FalseIfNextCornerInvalid() {
            throw new NotImplementedException();
        }

        [Test]
        public void AddRiverToCell_ReflectedInHasRiverAlongEdgeOfThisCell() {
            var cellToTest = BuildHexCell(false, new List<IHexCell>() { BuildHexCell(false) });

            MockRiverCornerValidityLogic.Setup(
                logic => logic.AreCornerFlowsValid(It.IsAny<RiverFlow>(), It.IsAny<RiverFlow>(), It.IsAny<RiverFlow>())
            ).Returns(true);

            var riverCanon = Container.Resolve<RiverCanon>();

            riverCanon.AddRiverToCell(cellToTest, (HexDirection)0, RiverFlow.Counterclockwise);

            Assert.IsTrue(riverCanon.HasRiverAlongEdge(cellToTest, (HexDirection)0));
        }

        [Test]
        public void AddRiverToCell_ReflectedInHasRiverAlongEdgeOfNeighbor() {
            var neighbors = new List<IHexCell>() { BuildHexCell(false) };

            var cellToTest = BuildHexCell(false, neighbors);

            MockRiverCornerValidityLogic.Setup(
                logic => logic.AreCornerFlowsValid(It.IsAny<RiverFlow>(), It.IsAny<RiverFlow>(), It.IsAny<RiverFlow>())
            ).Returns(true);

            var riverCanon = Container.Resolve<RiverCanon>();

            riverCanon.AddRiverToCell(cellToTest, (HexDirection)0, RiverFlow.Counterclockwise);

            Assert.IsTrue(riverCanon.HasRiverAlongEdge(neighbors[0], ((HexDirection)0).Opposite()));
        }

        [Test]
        public void AddRiverToCell_ReflectedInGetEdgesWithRiversOfThisCell() {
            var cellToTest = BuildHexCell(false, new List<IHexCell>() { BuildHexCell(false) });

            MockRiverCornerValidityLogic.Setup(
                logic => logic.AreCornerFlowsValid(It.IsAny<RiverFlow>(), It.IsAny<RiverFlow>(), It.IsAny<RiverFlow>())
            ).Returns(true);

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

            MockRiverCornerValidityLogic.Setup(
                logic => logic.AreCornerFlowsValid(It.IsAny<RiverFlow>(), It.IsAny<RiverFlow>(), It.IsAny<RiverFlow>())
            ).Returns(true);

            var riverCanon = Container.Resolve<RiverCanon>();

            riverCanon.AddRiverToCell(cellToTest, (HexDirection)0, RiverFlow.Counterclockwise);

            CollectionAssert.AreEquivalent(
                new List<HexDirection>() { ((HexDirection)0).Opposite() }, riverCanon.GetEdgesWithRivers(neighbors[0])
            );
        }

        [Test]
        public void AddRiverToCell_ReflectedInGetFlowDirectionOfRiverAtEdgeOfThisCell() {
            var cellToTest = BuildHexCell(false, new List<IHexCell>() { BuildHexCell(false) });

            MockRiverCornerValidityLogic.Setup(
                logic => logic.AreCornerFlowsValid(It.IsAny<RiverFlow>(), It.IsAny<RiverFlow>(), It.IsAny<RiverFlow>())
            ).Returns(true);

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

            riverCanon.AddRiverToCell(cellToTest, HexDirection.NE, RiverFlow.Counterclockwise);

            Assert.AreEqual(
                RiverFlow.Clockwise, riverCanon.GetFlowOfRiverAtEdge(neighbors[0], ((HexDirection)0).Opposite())
            );
        }

        [Test]
        public void AddRiverToCell_FiresGainedRiveredEdgeOnCell() {
            var neighbors = new List<IHexCell>() { BuildHexCell(false) };

            var cellToTest = BuildHexCell(false, neighbors);

            var riverCanon = Container.Resolve<RiverCanon>();

            CellSignals.GainedRiveredEdge.Subscribe(cell => {
                if(cell != neighbors[0]) {
                    Assert.AreEqual(cellToTest, cell, "Signal passed unexpected cell");
                    Assert.Pass();
                }
            });

            riverCanon.AddRiverToCell(cellToTest, HexDirection.NE, RiverFlow.Clockwise);
            
            Assert.Fail("GainedRiveredEdge never fired on CellToTest");
        }

        [Test]
        public void AddRiverToCell_FiresGainedRiveredEdgeOnNeighbor() {
            var neighbors = new List<IHexCell>() { BuildHexCell(false) };

            var cellToTest = BuildHexCell(false, neighbors);

            var riverCanon = Container.Resolve<RiverCanon>();

            CellSignals.GainedRiveredEdge.Subscribe(cell => {
                if(cell != cellToTest) {
                    Assert.AreEqual(neighbors[0], cell, "Signal passed unexpected cell");
                    Assert.Pass();
                }
            });

            riverCanon.AddRiverToCell(cellToTest, HexDirection.NE, RiverFlow.Clockwise);
            
            Assert.Fail("GainedRiveredEdge never fired on neighbors[0]");
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

            riverCanon.OverrideRiverOnCell(cellToTest, (HexDirection)0, RiverFlow.Clockwise);

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

            riverCanon.OverrideRiverOnCell(cellToTest, (HexDirection)0, RiverFlow.Clockwise);

            Assert.Throws<InvalidOperationException>(() => riverCanon.GetFlowOfRiverAtEdge(cellToTest, (HexDirection)1));
        }


        [Test]
        public void RemoveRiverFromCell_ReflectedInHasRiverAlongEdgeOfThisCell() {
            var cellToTest = BuildHexCell(false, new List<IHexCell>() { BuildHexCell(false) });

            var riverCanon = Container.Resolve<RiverCanon>();

            riverCanon.OverrideRiverOnCell(cellToTest, (HexDirection)0, RiverFlow.Counterclockwise);

            riverCanon.RemoveRiverFromCell(cellToTest, (HexDirection)0);

            Assert.IsFalse(riverCanon.HasRiverAlongEdge(cellToTest, (HexDirection)0));
        }

        [Test]
        public void RemoveRiverFromCell_ReflectedInHasRiverAlongEdgeOfNeighbor() {
            var neighbors = new List<IHexCell>() { BuildHexCell(false) };

            var cellToTest = BuildHexCell(false, neighbors);

            var riverCanon = Container.Resolve<RiverCanon>();

            riverCanon.OverrideRiverOnCell(cellToTest, (HexDirection)0, RiverFlow.Counterclockwise);

            riverCanon.RemoveRiverFromCell(cellToTest, (HexDirection)0);

            Assert.IsFalse(riverCanon.HasRiverAlongEdge(neighbors[0], ((HexDirection)0).Opposite()));
        }

        [Test]
        public void RemoveRiverFromCell_ReflectedInGetEdgesWithRiversOfThisCell() {
            var cellToTest = BuildHexCell(false, new List<IHexCell>() { BuildHexCell(false) });

            var riverCanon = Container.Resolve<RiverCanon>();

            riverCanon.OverrideRiverOnCell(cellToTest, (HexDirection)0, RiverFlow.Counterclockwise);

            riverCanon.RemoveRiverFromCell(cellToTest, (HexDirection)0);

            CollectionAssert.DoesNotContain(riverCanon.GetEdgesWithRivers(cellToTest), (HexDirection)0);
        }

        [Test]
        public void RemoveRiverFromCell_ReflectedInGetEdgesWithRiversOfNeighbor() {
            var neighbors = new List<IHexCell>() { BuildHexCell(false) };

            var cellToTest = BuildHexCell(false, neighbors);

            var riverCanon = Container.Resolve<RiverCanon>();

            riverCanon.OverrideRiverOnCell(cellToTest, (HexDirection)0, RiverFlow.Counterclockwise);

            riverCanon.RemoveRiverFromCell(cellToTest, (HexDirection)0);

            CollectionAssert.DoesNotContain(riverCanon.GetEdgesWithRivers(neighbors[0]), ((HexDirection)0).Opposite());
        }

        [Test]
        public void RemoveRiverFromCell_DoesNotThrowIfNoRiver() {
            var neighbors = new List<IHexCell>() { BuildHexCell(false) };

            var cellToTest = BuildHexCell(false, neighbors);

            var riverCanon = Container.Resolve<RiverCanon>();

            Assert.DoesNotThrow(() =>  riverCanon.RemoveRiverFromCell(cellToTest, (HexDirection)0));
        }

        [Test]
        public void RemoveRiverFromCell_FiresLostRiveredEdgeOnCell() {
            var neighbors = new List<IHexCell>() { BuildHexCell(false) };

            var cellToTest = BuildHexCell(false, neighbors);

            var riverCanon = Container.Resolve<RiverCanon>();

            riverCanon.AddRiverToCell(cellToTest, HexDirection.NE, RiverFlow.Clockwise);

            CellSignals.LostRiveredEdge.Subscribe(cell => {
                if(cell != neighbors[0]) {
                    Assert.AreEqual(cellToTest, cell, "Signal passed unexpected cell");
                    Assert.Pass();
                }
            });

            riverCanon.RemoveRiverFromCell(cellToTest, HexDirection.NE);

            Assert.Fail("LostRiveredEdge never fired on CellToTest");
        }

        [Test]
        public void RemoveRiverFromCell_FiresLostRiveredEdgeOnNeighbor() {
            var neighbors = new List<IHexCell>() { BuildHexCell(false) };

            var cellToTest = BuildHexCell(false, neighbors);

            var riverCanon = Container.Resolve<RiverCanon>();

            riverCanon.AddRiverToCell(cellToTest, HexDirection.NE, RiverFlow.Clockwise);

            CellSignals.LostRiveredEdge.Subscribe(cell => {
                if(cell != cellToTest) {
                    Assert.AreEqual(neighbors[0], cell, "Signal passed unexpected cell");
                    Assert.Pass();
                }
            });

            riverCanon.RemoveRiverFromCell(cellToTest, HexDirection.NE);

            Assert.Fail("LostRiveredEdge never fired on neighbors[0]");
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

            riverCanon.OverrideRiverOnCell(cellToTest, (HexDirection)0, RiverFlow.Counterclockwise);
            riverCanon.OverrideRiverOnCell(cellToTest, (HexDirection)1, RiverFlow.Counterclockwise);
            riverCanon.OverrideRiverOnCell(cellToTest, (HexDirection)2, RiverFlow.Counterclockwise);

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

            MockRiverCornerValidityLogic.Setup(
                logic => logic.AreCornerFlowsValid(It.IsAny<RiverFlow>(), It.IsAny<RiverFlow?>(), It.IsAny<RiverFlow?>())
            ).Returns(true);

            var riverCanon = Container.Resolve<RiverCanon>();

            riverCanon.OverrideRiverOnCell(cellToTest, HexDirection.NE, RiverFlow.Counterclockwise);
            riverCanon.OverrideRiverOnCell(cellToTest, HexDirection.E,  RiverFlow.Counterclockwise);
            riverCanon.OverrideRiverOnCell(cellToTest, HexDirection.SE, RiverFlow.Counterclockwise);

            mockNeighborOne  .Setup(cell => cell.Terrain).Returns(CellTerrain.FreshWater);
            mockNeighborThree.Setup(cell => cell.Terrain).Returns(CellTerrain.FreshWater);

            riverCanon.ValidateRivers(cellToTest);

            CollectionAssert.AreEquivalent(
                new List<HexDirection>() { HexDirection.E },
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

            var newCell = mockCell.Object;

            return newCell;
        }

        #endregion

        #endregion

    }

}
