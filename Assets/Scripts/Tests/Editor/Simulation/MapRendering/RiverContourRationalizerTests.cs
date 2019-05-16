using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using Zenject;
using Moq;

using Assets.Simulation.HexMap;
using Assets.Simulation.MapRendering;

namespace Assets.Tests.Simulation.MapRendering {

    public class RiverContourRationalizerTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IHexGrid>             MockGrid;
        private Mock<IRiverCanon>          MockRiverCanon;
        private Mock<IRiverSectionCanon>   MockRiverSectionCanon;
        private Mock<IRiverAssemblyCanon>  MockRiverAssemblyCanon;
        private Mock<IContourRationalizer> MockContourRationalizer;

        private List<IHexCell> AllCells = new List<IHexCell>();

        private List<ReadOnlyCollection<RiverSection>> AllRivers = new List<ReadOnlyCollection<RiverSection>>();

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            AllCells .Clear();
            AllRivers.Clear();

            MockGrid                = new Mock<IHexGrid>();
            MockRiverCanon          = new Mock<IRiverCanon>();
            MockRiverSectionCanon   = new Mock<IRiverSectionCanon>();
            MockRiverAssemblyCanon  = new Mock<IRiverAssemblyCanon>();
            MockContourRationalizer = new Mock<IContourRationalizer>();

            MockGrid.Setup(grid => grid.Cells).Returns(AllCells.AsReadOnly());

            MockRiverAssemblyCanon.Setup(canon => canon.Rivers).Returns(AllRivers.AsReadOnly());

            Container.Bind<IHexGrid>            ().FromInstance(MockGrid               .Object);
            Container.Bind<IRiverCanon>         ().FromInstance(MockRiverCanon         .Object);
            Container.Bind<IRiverSectionCanon>  ().FromInstance(MockRiverSectionCanon  .Object);
            Container.Bind<IRiverAssemblyCanon> ().FromInstance(MockRiverAssemblyCanon .Object);
            Container.Bind<IContourRationalizer>().FromInstance(MockContourRationalizer.Object);

            Container.Bind<RiverContourRationalizer>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void RationalizeRiverContoursInCorner_DoesNothingIfPreviousCornerNotRiverConfluence() {
            var center = BuildCell();
            var left   = BuildCell();

            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.NE)).Returns(left);

            MockRiverCanon.Setup(canon => canon.HasRiverAlongEdge(center, HexDirection.NE)).Returns(true);
            MockRiverCanon.Setup(canon => canon.HasRiverAlongEdge(center, HexDirection.E )).Returns(true);
            MockRiverCanon.Setup(canon => canon.HasRiverAlongEdge(left,   HexDirection.SE)).Returns(false);

            var riverRationalizer = Container.Resolve<RiverContourRationalizer>();

            riverRationalizer.RationalizeRiverContoursInCorner(center, HexDirection.E);

            MockContourRationalizer.Verify(
                rationalizer => rationalizer.RationalizeCellContours(
                    It.IsAny<IHexCell>(), It.IsAny<HexDirection>()
                ), Times.Never, "RationalizerCellContours unexpectedly called"
            );
        }

        [Test]
        public void RationalizeRiverContoursInCorner_DoesNothingIfLeftAndRightRiversOnSameRiver_AndAdjacentInThatRiver() {
            var center = BuildCell();
            var left   = BuildCell();
            var right  = BuildCell();

            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.NE)).Returns(left);
            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.E )).Returns(right);

            MockRiverCanon.Setup(canon => canon.HasRiverAlongEdge(center, HexDirection.NE)).Returns(true);
            MockRiverCanon.Setup(canon => canon.HasRiverAlongEdge(center, HexDirection.E )).Returns(true);
            MockRiverCanon.Setup(canon => canon.HasRiverAlongEdge(left,   HexDirection.SE)).Returns(true);

            var centerLeftSection  = new RiverSection();
            var centerRightSection = new RiverSection();

            MockRiverSectionCanon.Setup(canon => canon.GetSectionBetweenCells(center, left )).Returns(centerLeftSection);
            MockRiverSectionCanon.Setup(canon => canon.GetSectionBetweenCells(center, right)).Returns(centerRightSection);

            BuildRiver(new RiverSection(), centerLeftSection, centerRightSection, new RiverSection());

            var riverRationalizer = Container.Resolve<RiverContourRationalizer>();

            riverRationalizer.RationalizeRiverContoursInCorner(center, HexDirection.E);

            MockContourRationalizer.Verify(
                rationalizer => rationalizer.RationalizeCellContours(
                    It.IsAny<IHexCell>(), It.IsAny<HexDirection>()
                ), Times.Never, "RationalizerCellContours unexpectedly called"
            );
        }

        [Test]
        public void RationalizeRiverContoursInCorner_RationalizesCellContours_IfLeftAndRightOnDifferentRivers() {
            var center = BuildCell();
            var left   = BuildCell();
            var right  = BuildCell();

            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.NE)).Returns(left);
            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.E )).Returns(right);

            MockRiverCanon.Setup(canon => canon.HasRiverAlongEdge(center, HexDirection.NE)).Returns(true);
            MockRiverCanon.Setup(canon => canon.HasRiverAlongEdge(center, HexDirection.E )).Returns(true);
            MockRiverCanon.Setup(canon => canon.HasRiverAlongEdge(left,   HexDirection.SE)).Returns(true);

            var centerLeftSection  = new RiverSection();
            var centerRightSection = new RiverSection();

            MockRiverSectionCanon.Setup(canon => canon.GetSectionBetweenCells(center, left )).Returns(centerLeftSection);
            MockRiverSectionCanon.Setup(canon => canon.GetSectionBetweenCells(center, right)).Returns(centerRightSection);

            var riverRationalizer = Container.Resolve<RiverContourRationalizer>();

            riverRationalizer.RationalizeRiverContoursInCorner(center, HexDirection.E);

            MockContourRationalizer.Verify(
                rationalizer => rationalizer.RationalizeCellContours(
                    It.IsAny<IHexCell>(), It.IsAny<HexDirection>()
                ), Times.Once, "RationalizerCellContours called an unexpected number of times"
            );

            MockContourRationalizer.Verify(
                rationalizer => rationalizer.RationalizeCellContours(center, HexDirection.E),
                Times.Once, "RationalizerCellContours wasn't called as expected"
            );
        }

        [Test]
        public void RationalizeRiverContoursInCorner_RationalizesCellContours_IfLeftAndRightOnSameRiver_ButNotAdjacent() {
            var center = BuildCell();
            var left   = BuildCell();
            var right  = BuildCell();

            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.NE)).Returns(left);
            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.E )).Returns(right);

            MockRiverCanon.Setup(canon => canon.HasRiverAlongEdge(center, HexDirection.NE)).Returns(true);
            MockRiverCanon.Setup(canon => canon.HasRiverAlongEdge(center, HexDirection.E )).Returns(true);
            MockRiverCanon.Setup(canon => canon.HasRiverAlongEdge(left,   HexDirection.SE)).Returns(true);

            var centerLeftSection  = new RiverSection();
            var centerRightSection = new RiverSection();

            MockRiverSectionCanon.Setup(canon => canon.GetSectionBetweenCells(center, left )).Returns(centerLeftSection);
            MockRiverSectionCanon.Setup(canon => canon.GetSectionBetweenCells(center, right)).Returns(centerRightSection);

            BuildRiver(new RiverSection(), centerLeftSection, new RiverSection(), centerRightSection);

            var riverRationalizer = Container.Resolve<RiverContourRationalizer>();

            riverRationalizer.RationalizeRiverContoursInCorner(center, HexDirection.E);

            MockContourRationalizer.Verify(
                rationalizer => rationalizer.RationalizeCellContours(
                    It.IsAny<IHexCell>(), It.IsAny<HexDirection>()
                ), Times.Once, "RationalizerCellContours called an unexpected number of times"
            );

            MockContourRationalizer.Verify(
                rationalizer => rationalizer.RationalizeCellContours(center, HexDirection.E),
                Times.Once, "RationalizerCellContours wasn't called as expected"
            );
        }

        #endregion

        #region utilities

        private IHexCell BuildCell() {
            var newCell = new Mock<IHexCell>().Object;

            AllCells.Add(newCell);

            return newCell;
        }

        private void BuildRiver(params RiverSection[] sections) {
            var newRiver = sections.ToList().AsReadOnly();

            AllRivers.Add(newRiver);
        }

        #endregion

        #endregion

    }

}
