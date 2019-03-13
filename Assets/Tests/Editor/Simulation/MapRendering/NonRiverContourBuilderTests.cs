using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using NUnit.Framework;
using Zenject;
using Moq;

using Assets.Simulation.HexMap;
using Assets.Simulation.MapRendering;

namespace Assets.Tests.Simulation.MapRendering {

    public class NonRiverContourBuilderTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IHexGrid>              MockGrid;
        private Mock<IRiverCanon>           MockRiverCanon;
        private Mock<ICellEdgeContourCanon> MockCellEdgeContourCanon;
        private Mock<IMapRenderConfig>      MockRenderConfig;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockGrid                 = new Mock<IHexGrid>();
            MockRiverCanon           = new Mock<IRiverCanon>();
            MockCellEdgeContourCanon = new Mock<ICellEdgeContourCanon>();
            MockRenderConfig         = new Mock<IMapRenderConfig>();

            Container.Bind<IHexGrid>             ().FromInstance(MockGrid                .Object);
            Container.Bind<IRiverCanon>          ().FromInstance(MockRiverCanon          .Object);
            Container.Bind<ICellEdgeContourCanon>().FromInstance(MockCellEdgeContourCanon.Object);
            Container.Bind<IMapRenderConfig>     ().FromInstance(MockRenderConfig        .Object);

            Container.Bind<NonRiverContourBuilder>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void BuildNonRiverContour_AndHasRiverAtEdge_DoesNothing() {
            var center = BuildCell(Vector2.zero);

            MockRiverCanon.Setup(canon => canon.HasRiverAlongEdge(center, HexDirection.E)).Returns(true);

            var contourBuilder = Container.Resolve<NonRiverContourBuilder>();

            contourBuilder.BuildNonRiverContour(center, HexDirection.E);

            MockCellEdgeContourCanon.Verify(
                canon => canon.SetContourForCellEdge(center, HexDirection.E, It.IsAny<List<Vector2>>()),
                Times.Never, "SetContourForCellEdge called unexpectedly"
            );
        }

        [Test]
        public void BuildNonRiverContour_AndHasLeftCenterRiver_BeginsWithLastPointOfCenterLeftContour() {
            var center = BuildCell(new Vector2(1f, 1f));
            var left   = BuildCell(new Vector2(2f, 2f));

            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.NE)).Returns(left);

            MockRiverCanon.Setup(canon => canon.HasRiverAlongEdge(center, HexDirection.NE)).Returns(true);

            var centerLeftContour = new List<Vector2>() { new Vector2(11f, 11f), new Vector2(22f, 22f), new Vector2(33f, 33f) }.AsReadOnly();

            MockCellEdgeContourCanon.Setup(canon => canon.GetContourForCellEdge(center, HexDirection.NE)).Returns(centerLeftContour);

            MockCellEdgeContourCanon.Setup(
                canon => canon.SetContourForCellEdge(center, HexDirection.E, It.IsAny<List<Vector2>>())
            ).Callback<IHexCell, HexDirection, List<Vector2>>(
                (cell, direction, contour) => {
                    Assert.AreEqual(centerLeftContour.Last(), contour.First(), "Unexpected first contour point");
                }
            );

            var contourBuilder = Container.Resolve<NonRiverContourBuilder>();

            contourBuilder.BuildNonRiverContour(center, HexDirection.E);

            MockCellEdgeContourCanon.Verify(
                canon => canon.SetContourForCellEdge(It.IsAny<IHexCell>(), It.IsAny<HexDirection>(), It.IsAny<List<Vector2>>()),
                Times.Once, "SetContourForCellEdge called an unexpected number of times"
            );
        }

        [Test]
        public void BuildNonRiverContour_HasNoLeftCenterRiver_ButHasLeftRightRiver_BeginsWithFirstPointOfRightLeftContour() {
            var center = BuildCell(new Vector2(1f, 1f));
            var left   = BuildCell(new Vector2(2f, 2f));
            var right  = BuildCell(new Vector2(3f, 3f));

            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.NE)).Returns(left);
            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.E )).Returns(right);

            MockRiverCanon.Setup(canon => canon.HasRiverAlongEdge(left, HexDirection.SE)).Returns(true);

            var rightLeftContour = new List<Vector2>() { new Vector2(11f, 11f), new Vector2(22f, 22f), new Vector2(33f, 33f) }.AsReadOnly();

            MockCellEdgeContourCanon.Setup(canon => canon.GetContourForCellEdge(right, HexDirection.NW)).Returns(rightLeftContour);

            MockCellEdgeContourCanon.Setup(
                canon => canon.SetContourForCellEdge(center, HexDirection.E, It.IsAny<List<Vector2>>())
            ).Callback<IHexCell, HexDirection, List<Vector2>>(
                (cell, direction, contour) => {
                    Assert.AreEqual(rightLeftContour.First(), contour.First(), "Unexpected first contour point");
                }
            );

            var contourBuilder = Container.Resolve<NonRiverContourBuilder>();

            contourBuilder.BuildNonRiverContour(center, HexDirection.E);

            MockCellEdgeContourCanon.Verify(
                canon => canon.SetContourForCellEdge(It.IsAny<IHexCell>(), It.IsAny<HexDirection>(), It.IsAny<List<Vector2>>()),
                Times.Once, "SetContourForCellEdge called an unexpected number of times"
            );
        }

        [Test]
        public void BuildNonRiverContour_AndHasNoLeftCenter_OrLeftRightRiver_BeginsWithFirstCornerInDirection() {
            var center = BuildCell(new Vector2(1f, 2f));

            MockRenderConfig.Setup(config => config.GetFirstCornerXZ(HexDirection.E)).Returns(new Vector2(22f, 222f));

            MockCellEdgeContourCanon.Setup(
                canon => canon.SetContourForCellEdge(center, HexDirection.E, It.IsAny<List<Vector2>>())
            ).Callback<IHexCell, HexDirection, List<Vector2>>(
                (cell, direction, contour) => {
                    Assert.AreEqual(new Vector2(23f, 224f), contour.First(), "Unexpected first contour point");
                }
            );

            var contourBuilder = Container.Resolve<NonRiverContourBuilder>();

            contourBuilder.BuildNonRiverContour(center, HexDirection.E);

            MockCellEdgeContourCanon.Verify(
                canon => canon.SetContourForCellEdge(It.IsAny<IHexCell>(), It.IsAny<HexDirection>(), It.IsAny<List<Vector2>>()),
                Times.Once, "SetContourForCellEdge called an unexpected number of times"
            );
        }

        [Test]
        public void BuildNonRiverContour_HasCenterNextRightRiver_EndsWithFirstPointOnCenterNextRightContour() {
            var center    = BuildCell(new Vector2(1f, 1f));
            var nextRight = BuildCell(new Vector2(4f, 4f));

            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.SE)).Returns(nextRight);

            MockRiverCanon.Setup(canon => canon.HasRiverAlongEdge(center, HexDirection.SE)).Returns(true);

            var centerNextRightContour = new List<Vector2>() { new Vector2(11f, 11f), new Vector2(22f, 22f), new Vector2(33f, 33f) }.AsReadOnly();

            MockCellEdgeContourCanon.Setup(canon => canon.GetContourForCellEdge(center, HexDirection.SE)).Returns(centerNextRightContour);

            MockCellEdgeContourCanon.Setup(
                canon => canon.SetContourForCellEdge(center, HexDirection.E, It.IsAny<List<Vector2>>())
            ).Callback<IHexCell, HexDirection, List<Vector2>>(
                (cell, direction, contour) => {
                    Assert.AreEqual(centerNextRightContour.First(), contour.Last(), "Unexpected last contour point");
                }
            );

            var contourBuilder = Container.Resolve<NonRiverContourBuilder>();

            contourBuilder.BuildNonRiverContour(center, HexDirection.E);

            MockCellEdgeContourCanon.Verify(
                canon => canon.SetContourForCellEdge(It.IsAny<IHexCell>(), It.IsAny<HexDirection>(), It.IsAny<List<Vector2>>()),
                Times.Once, "SetContourForCellEdge called an unexpected number of times"
            );
        }

        [Test]
        public void BuildNonRiverContour_HasNoCenterNextRightRiver_ButHasNextRightRightRiver_EndsWithPointsFromBothContours() {
            var center    = BuildCell(new Vector2(1f, 1f));
            var right     = BuildCell(new Vector2(3f, 3f));
            var nextRight = BuildCell(new Vector2(4f, 4f));

            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.E )).Returns(right);
            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.SE)).Returns(nextRight);

            MockRiverCanon.Setup(canon => canon.HasRiverAlongEdge(nextRight, HexDirection.NE)).Returns(true);

            var rightNextRightContour = new List<Vector2>() { new Vector2(11f, 11f), new Vector2(22f, 22f), new Vector2(33f, 33f) }.AsReadOnly();
            var nextRightRightContour = new List<Vector2>() { new Vector2(44f, 44f), new Vector2(55f, 55f), new Vector2(66f, 66f) }.AsReadOnly();

            MockCellEdgeContourCanon.Setup(canon => canon.GetContourForCellEdge(right,     HexDirection.SW)).Returns(rightNextRightContour);
            MockCellEdgeContourCanon.Setup(canon => canon.GetContourForCellEdge(nextRight, HexDirection.NE)).Returns(nextRightRightContour);

            MockCellEdgeContourCanon.Setup(
                canon => canon.SetContourForCellEdge(center, HexDirection.E, It.IsAny<List<Vector2>>())
            ).Callback<IHexCell, HexDirection, List<Vector2>>(
                (cell, direction, contour) => {
                    Assert.AreEqual(rightNextRightContour.Last (), contour[contour.Count - 2], "Unexpected second to last contour point");
                    Assert.AreEqual(nextRightRightContour.First(), contour[contour.Count - 1], "Unexpected last contour point");
                }
            );

            var contourBuilder = Container.Resolve<NonRiverContourBuilder>();

            contourBuilder.BuildNonRiverContour(center, HexDirection.E);

            MockCellEdgeContourCanon.Verify(
                canon => canon.SetContourForCellEdge(It.IsAny<IHexCell>(), It.IsAny<HexDirection>(), It.IsAny<List<Vector2>>()),
                Times.Once, "SetContourForCellEdge called an unexpected number of times"
            );

        }

        [Test]
        public void BuildNonRiverContour_AndHasNoCenterNextRightRiver_OrNextRightRightRiver_EndsWithSecondCornerInDirection() {
            var center = BuildCell(new Vector2(1f, 2f));

            MockRenderConfig.Setup(config => config.GetSecondCornerXZ(HexDirection.E)).Returns(new Vector2(22f, 222f));

            MockCellEdgeContourCanon.Setup(
                canon => canon.SetContourForCellEdge(center, HexDirection.E, It.IsAny<List<Vector2>>())
            ).Callback<IHexCell, HexDirection, List<Vector2>>(
                (cell, direction, contour) => {
                    Assert.AreEqual(new Vector2(23f, 224f), contour.Last(), "Unexpected last contour point");
                }
            );

            var contourBuilder = Container.Resolve<NonRiverContourBuilder>();

            contourBuilder.BuildNonRiverContour(center, HexDirection.E);

            MockCellEdgeContourCanon.Verify(
                canon => canon.SetContourForCellEdge(It.IsAny<IHexCell>(), It.IsAny<HexDirection>(), It.IsAny<List<Vector2>>()),
                Times.Once, "SetContourForCellEdge called an unexpected number of times"
            );
        }

        #endregion

        #region utilities

        private IHexCell BuildCell(Vector2 absolutePositionXZ) {
            var mockCell = new Mock<IHexCell>();

            mockCell.Setup(cell => cell.AbsolutePositionXZ).Returns(absolutePositionXZ);

            return mockCell.Object;
        }

        #endregion

        #endregion

    }

}
