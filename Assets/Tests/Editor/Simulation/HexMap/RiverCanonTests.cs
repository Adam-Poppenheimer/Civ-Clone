using System;
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

        #region instance fields and properties

        private Mock<IHexGrid> MockGrid;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockGrid = new Mock<IHexGrid>();

            Container.Bind<IHexGrid>().FromInstance(MockGrid.Object);

            Container.Bind<RiverCanon>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void CanAddRiverToCell_FalseIfNoNeighborInDirection() {
            var cellToTest = BuildHexCell(false, new List<IHexCell>() { BuildHexCell(false) });

            var riverCanon = Container.Resolve<RiverCanon>();

            Assert.IsFalse(
                riverCanon.CanAddRiverToCell(cellToTest, (HexDirection)1, RiverFlow.Clockwise),
                "CanAddRiverToCell unexpectedly returned true for a clockwise river direction"
            );

            Assert.IsFalse(
                riverCanon.CanAddRiverToCell(cellToTest, (HexDirection)1, RiverFlow.Counterclockwise),
                "CanAddRiverToCell unexpectedly returned true fora  counterclockwise river direction"
            );
        }

        [Test]
        public void CanAddRiverToCell_FalseIfNeighborUnderwater() {
            var cellToTest = BuildHexCell(false, new List<IHexCell>() { BuildHexCell(true) });

            var riverCanon = Container.Resolve<RiverCanon>();

            Assert.IsFalse(
                riverCanon.CanAddRiverToCell(cellToTest, (HexDirection)0, RiverFlow.Clockwise),
                "CanAddRiverToCell unexpectedly returned true for a clockwise river direction"
            );

            Assert.IsFalse(
                riverCanon.CanAddRiverToCell(cellToTest, (HexDirection)0, RiverFlow.Counterclockwise),
                "CanAddRiverToCell unexpectedly returned true for a counterclockwise river direction"
            );
        }

        [Test]
        public void CanAddRiverToCell_FalseIfCellUnderwater() {
            throw new NotImplementedException();
        }

        [Test]
        public void CanAddRiverToCell_FalseIfRiverAlreadyPresent() {
            var cellToTest = BuildHexCell(false, new List<IHexCell>() { BuildHexCell(false) });

            var riverCanon = Container.Resolve<RiverCanon>();

            riverCanon.AddRiverToCell(cellToTest, (HexDirection)0, RiverFlow.Clockwise);

            Assert.IsFalse(
                riverCanon.CanAddRiverToCell(cellToTest, (HexDirection)0, RiverFlow.Clockwise),
                "CanAddRiverToCell unexpectedly returned true for a clockwise river direction"
            );

            Assert.IsFalse(
                riverCanon.CanAddRiverToCell(cellToTest, (HexDirection)0, RiverFlow.Counterclockwise),
                "CanAddRiverToCell unexpectedly returned true for a counterclockwise river direction"
            );
        }

        [Test]
        public void CanAddRiverToCell_TrueIfNeighborExists_IsNotUnderwater_AndNoRiverExists() {
            var cellToTest = BuildHexCell(false, new List<IHexCell>() { BuildHexCell(false) });

            var riverCanon = Container.Resolve<RiverCanon>();

            Assert.IsTrue(
                riverCanon.CanAddRiverToCell(cellToTest, (HexDirection)0, RiverFlow.Clockwise),
                "CanAddRiverToCell unexpectedly returned false for a clockwise river direction"
            );

            Assert.IsTrue(
                riverCanon.CanAddRiverToCell(cellToTest, (HexDirection)0, RiverFlow.Counterclockwise),
                "CanAddRiverToCell unexpectedly returned false for a counterclockwise river direction"
            );
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

            mockNeighborOne  .Setup(cell => cell.IsUnderwater).Returns(true);
            mockNeighborThree.Setup(cell => cell.IsUnderwater).Returns(true);

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

            mock.Setup(cell => cell.IsUnderwater).Returns(isUnderwater);

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

        #endregion

        #endregion

    }

}
