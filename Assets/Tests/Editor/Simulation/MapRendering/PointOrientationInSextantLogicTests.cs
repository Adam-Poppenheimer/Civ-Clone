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
using Assets.Util;

namespace Assets.Tests.Simulation.MapRendering {

    public class PointOrientationInSextantLogicTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IHexGrid>                     MockGrid;
        private Mock<ICellEdgeContourCanon>        MockCellEdgeContourCanon;
        private Mock<IRiverCanon>                  MockRiverCanon;
        private Mock<IGeometry2D>                  MockGeometry2D;
        private Mock<IPointOrientationWeightLogic> MockPointOrientationWeightLogic;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockGrid                        = new Mock<IHexGrid>();
            MockCellEdgeContourCanon        = new Mock<ICellEdgeContourCanon>();
            MockRiverCanon                  = new Mock<IRiverCanon>();
            MockGeometry2D                  = new Mock<IGeometry2D>();
            MockPointOrientationWeightLogic = new Mock<IPointOrientationWeightLogic>();

            Container.Bind<IHexGrid>                    ().FromInstance(MockGrid                       .Object);
            Container.Bind<ICellEdgeContourCanon>       ().FromInstance(MockCellEdgeContourCanon       .Object);
            Container.Bind<IRiverCanon>                 ().FromInstance(MockRiverCanon                 .Object);
            Container.Bind<IGeometry2D>                 ().FromInstance(MockGeometry2D                 .Object);
            Container.Bind<IPointOrientationWeightLogic>().FromInstance(MockPointOrientationWeightLogic.Object);

            Container.Bind<PointOrientationInSextantLogic>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void TryFindValidOrientation_AlwaysAppliesValuesToIsOnGrid_Sextant_AndCells() {
            var center    = BuildCell();
            var left      = BuildCell();
            var right     = BuildCell();
            var nextRight = BuildCell();

            MockGrid.Setup(grid => grid.HasNeighbor(center, HexDirection.NE)).Returns(true);
            MockGrid.Setup(grid => grid.HasNeighbor(center, HexDirection.E )).Returns(true);
            MockGrid.Setup(grid => grid.HasNeighbor(center, HexDirection.SE)).Returns(true);

            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.NE)).Returns(left);
            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.E )).Returns(right);
            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.SE)).Returns(nextRight);

            var data = new PointOrientationData();

            var sextantLogic = Container.Resolve<PointOrientationInSextantLogic>();

            sextantLogic.TryFindValidOrientation(Vector2.zero, center, HexDirection.E, out data);

            Assert.IsTrue(data.IsOnGrid, "IsOnGrid has an unexpected value");

            Assert.AreEqual(HexDirection.E, data.Sextant, "Sextant has an unexpected value");

            Assert.AreEqual(center,    data.Center,    "Center has an unexpected value");
            Assert.AreEqual(left,      data.Left,      "Left has an unexpected value");
            Assert.AreEqual(right,     data.Right,     "Right has an unexpected value");
            Assert.AreEqual(nextRight, data.NextRight, "NextRight has an unexpected value");
        }

        [Test]
        public void TryFindValidOrientation_PointInCenterRightContour_AndEdgeHasRiver_DoesLandBesideRiver() {
            var point = new Vector2(1f, 2f);

            var center = BuildCell();

            MockCellEdgeContourCanon.Setup(canon => canon.IsPointWithinContour(point, center, HexDirection.E)).Returns(true);

            MockRiverCanon.Setup(canon => canon.HasRiverAlongEdge(center, HexDirection.E)).Returns(true);

            PointOrientationData data;

            var sextantLogic = Container.Resolve<PointOrientationInSextantLogic>();

            Assert.IsTrue(
                sextantLogic.TryFindValidOrientation(point, center, HexDirection.E, out data),
                "Did not return true as expected"
            );

            MockPointOrientationWeightLogic.Verify(
                logic => logic.ApplyLandBesideRiverWeights(point, data), Times.Once,
                "Failed to call ApplyLandBesideRiverWeights"
            );
        }

        [Test]
        public void TryFindValidOrientation_PointInCenterRightContour_AndNoRiver_DoesLandBesideLand() {
            var point = new Vector2(1f, 2f);

            var center = BuildCell();

            MockCellEdgeContourCanon.Setup(canon => canon.IsPointWithinContour(point, center, HexDirection.E)).Returns(true);

            MockRiverCanon.Setup(canon => canon.HasRiverAlongEdge(center, HexDirection.E)).Returns(false);

            MockRiverCanon.Setup(canon => canon.HasRiverAlongEdge(center, HexDirection.NE)).Returns(true);

            PointOrientationData data;

            var sextantLogic = Container.Resolve<PointOrientationInSextantLogic>();

            Assert.IsTrue(
                sextantLogic.TryFindValidOrientation(point, center, HexDirection.E, out data),
                "Did not return true as expected"
            );

            MockPointOrientationWeightLogic.Verify(
                logic => logic.ApplyLandBesideLandWeights(point, data, true, false), Times.Once,
                "Failed to call ApplyLandBesideLandWeights"
            );
        }

        [Test]
        public void TryFindValidOrientation_PointNotInCenterRightContour_AndNoRightNeighbor_ReturnsFalse() {
            var point = new Vector2(1f, 2f);

            var center = BuildCell();

            PointOrientationData data;

            var sextantLogic = Container.Resolve<PointOrientationInSextantLogic>();

            Assert.IsFalse(
                sextantLogic.TryFindValidOrientation(point, center, HexDirection.E, out data),
                "Did not return false as expected"
            );
        }

        [Test]
        public void TryFindValidOrientation_PointInRightCenterContour_AndEdgeHasRiver_AppliesLandBesideRiver() {
            var point = new Vector2(1f, 2f);

            var center = BuildCell();
            var right  = BuildCell();

            MockGrid.Setup(grid => grid.HasNeighbor(center, HexDirection.E)).Returns(true);
            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.E)).Returns(right);

            MockCellEdgeContourCanon.Setup(canon => canon.IsPointWithinContour(point, right, HexDirection.W)).Returns(true);

            MockRiverCanon.Setup(canon => canon.HasRiverAlongEdge(center, HexDirection.E)).Returns(true);

            PointOrientationData data;

            var sextantLogic = Container.Resolve<PointOrientationInSextantLogic>();

            Assert.IsTrue(
                sextantLogic.TryFindValidOrientation(point, center, HexDirection.E, out data),
                "Did not return true as expected"
            );

            MockPointOrientationWeightLogic.Verify(
                logic => logic.ApplyLandBesideRiverWeights(point, data), Times.Once,
                "Did not call ApplyLandBesideRiverWeights as expected"
            );
        }

        [Test]
        public void TryFindValidOrientation_PointInRightCenterContour_InvertsData() {
            var point = new Vector2(1f, 2f);

            var center    = BuildCell();
            var left      = BuildCell();
            var right     = BuildCell();
            var nextRight = BuildCell();

            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.NE)).Returns(left);
            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.E )).Returns(right);
            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.SE)).Returns(nextRight);

            MockCellEdgeContourCanon.Setup(canon => canon.IsPointWithinContour(point, right, HexDirection.W)).Returns(true);

            PointOrientationData data;

            var sextantLogic = Container.Resolve<PointOrientationInSextantLogic>();

            sextantLogic.TryFindValidOrientation(point, center, HexDirection.E, out data);

            Assert.AreEqual(right,     data.Center,    "data.Center has an unexpected value");
            Assert.AreEqual(nextRight, data.Left,      "data.Left has an unexpected value");
            Assert.AreEqual(center,    data.Right,     "data.Right has an unexpected value");
            Assert.AreEqual(left,      data.NextRight, "data.NextRight has an unexpected value");

            Assert.AreEqual(HexDirection.W, data.Sextant, "data.Sextant has an unexpected value");
        }

        [Test]
        public void TryFindValidOrientation_PointInRightCenterContour_AndNoRiver_DoesLandBesideLand() {
            var point = new Vector2(1f, 2f);

            var center = BuildCell();
            var right  = BuildCell();

            MockGrid.Setup(grid => grid.HasNeighbor(center, HexDirection.E)).Returns(true);
            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.E)).Returns(right);

            MockCellEdgeContourCanon.Setup(canon => canon.IsPointWithinContour(point, right, HexDirection.W)).Returns(true);

            MockRiverCanon.Setup(canon => canon.HasRiverAlongEdge(center, HexDirection.E)).Returns(false);

            MockRiverCanon.Setup(canon => canon.HasRiverAlongEdge(right, HexDirection.SW)).Returns(true);

            PointOrientationData data;

            var sextantLogic = Container.Resolve<PointOrientationInSextantLogic>();

            Assert.IsTrue(
                sextantLogic.TryFindValidOrientation(point, center, HexDirection.E, out data),
                "Did not return true as expected"
            );

            MockPointOrientationWeightLogic.Verify(
                logic => logic.ApplyLandBesideLandWeights(point, data, true, false), Times.Once,
                "Did not call ApplyLandBesideRiverWeights as expected"
            );
        }

        [Test]
        public void TryFindValidOrientation_PointBetweenCenterRightAndRightCenterContours_AndEdgeHasRiver_AppliesRiverWeights_AndReturnsTrue() {
            var point = new Vector2(1f, 2f);

            var center = BuildCell();
            var right  = BuildCell();

            MockGrid.Setup(grid => grid.HasNeighbor(center, HexDirection.E)).Returns(true);
            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.E)).Returns(right);

            var centerRightContour = new List<Vector2>().AsReadOnly();
            var rightCenterContour = new List<Vector2>().AsReadOnly();

            MockCellEdgeContourCanon.Setup(canon => canon.GetContourForCellEdge(center, HexDirection.E)).Returns(centerRightContour);
            MockCellEdgeContourCanon.Setup(canon => canon.GetContourForCellEdge(right,  HexDirection.W)).Returns(rightCenterContour);

            MockCellEdgeContourCanon.Setup(canon => canon.IsPointBetweenContours(point, centerRightContour, rightCenterContour)).Returns(true);

            MockRiverCanon.Setup(canon => canon.HasRiverAlongEdge(center, HexDirection.E)).Returns(true);

            PointOrientationData data;

            var sextantLogic = Container.Resolve<PointOrientationInSextantLogic>();

            Assert.IsTrue(
                sextantLogic.TryFindValidOrientation(point, center, HexDirection.E, out data),
                "Did not return true as expected"
            );

            MockPointOrientationWeightLogic.Verify(
                logic => logic.ApplyRiverWeights(point, data), Times.Once,
                "Did not call ApplyRiverWeights as expected"
            );
        }

        [Test]
        public void TryFindValidOrientation_AndPointInPreviousConfluence_AppliesRiverCornerWeights_AndReturnsTrue() {
            var point = new Vector2(1f, 2f);

            var center = BuildCell();
            var left   = BuildCell();
            var right  = BuildCell();

            MockGrid.Setup(grid => grid.HasNeighbor(center, HexDirection.NE)).Returns(true);
            MockGrid.Setup(grid => grid.HasNeighbor(center, HexDirection.E )).Returns(true);            

            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.NE)).Returns(left);
            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.E )).Returns(right);

            MockRiverCanon.Setup(canon => canon.HasRiverAlongEdge(center, HexDirection.NE)).Returns(true);
            MockRiverCanon.Setup(canon => canon.HasRiverAlongEdge(center, HexDirection.E )).Returns(true);
            MockRiverCanon.Setup(canon => canon.HasRiverAlongEdge(left,   HexDirection.SE)).Returns(true);

            var centerRightContour = new List<Vector2>() { new Vector2(11f, 11f), new Vector2(111f, 111f) }.AsReadOnly();
            var leftCenterContour  = new List<Vector2>() { new Vector2(33f, 33f), new Vector2(333f, 333f) }.AsReadOnly();
            var rightCenterContour = new List<Vector2>() { new Vector2(22f, 22f), new Vector2(222f, 222f) }.AsReadOnly();

            MockCellEdgeContourCanon.Setup(canon => canon.GetContourForCellEdge(center, HexDirection.E )).Returns(centerRightContour);
            MockCellEdgeContourCanon.Setup(canon => canon.GetContourForCellEdge(right,  HexDirection.W )).Returns(rightCenterContour);
            MockCellEdgeContourCanon.Setup(canon => canon.GetContourForCellEdge(left,   HexDirection.SW)).Returns(leftCenterContour);

            MockGeometry2D.Setup(geometry => geometry.IsPointWithinTriangle(
                point, centerRightContour.First(), leftCenterContour.First(), rightCenterContour.Last()
            )).Returns(true);

            float centerWeight = 1f, leftWeight = 2f, rightWeight = 3f, riverHeightWeight = 4f, riverAlphaWeight = 5f;
            MockPointOrientationWeightLogic.Setup(
                logic => logic.GetRiverCornerWeights(
                    point, center, left, right, HexDirection.E, out centerWeight,
                    out leftWeight, out rightWeight, out riverHeightWeight, out riverAlphaWeight
                )
            );

            PointOrientationData data;

            var sextantLogic = Container.Resolve<PointOrientationInSextantLogic>();

            Assert.IsTrue(
                sextantLogic.TryFindValidOrientation(point, center, HexDirection.E, out data),
                "Did not return true as expected"
            );

            Assert.AreEqual(centerWeight,      data.CenterWeight,      "Unexpected CenterWeight");
            Assert.AreEqual(leftWeight,        data.LeftWeight,        "Unexpected LeftWeight");
            Assert.AreEqual(rightWeight,       data.RightWeight,       "Unexpected RightWeight");
            Assert.AreEqual(riverHeightWeight, data.RiverHeightWeight, "Unexpected RiverHeightWeight");
            Assert.AreEqual(riverAlphaWeight,  data.RiverAlphaWeight,  "Unexpected RiverAlphaWeight");
        }

        [Test]
        public void TryFindValidOrientation_AndPointInNextConfluence_AppliesRiverCornerWeights_AndReturnsTrue() {
            var point = new Vector2(1f, 2f);

            var center    = BuildCell();
            var right     = BuildCell();
            var nextRight = BuildCell();

            MockGrid.Setup(grid => grid.HasNeighbor(center, HexDirection.E )).Returns(true);
            MockGrid.Setup(grid => grid.HasNeighbor(center, HexDirection.SE)).Returns(true);            

            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.E )).Returns(right);
            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.SE)).Returns(nextRight);

            MockRiverCanon.Setup(canon => canon.HasRiverAlongEdge(center,    HexDirection.E )).Returns(true);
            MockRiverCanon.Setup(canon => canon.HasRiverAlongEdge(center,    HexDirection.SE)).Returns(true);
            MockRiverCanon.Setup(canon => canon.HasRiverAlongEdge(nextRight, HexDirection.NE)).Returns(true);

            var centerRightContour     = new List<Vector2>() { new Vector2(11f, 11f), new Vector2(111f, 111f) }.AsReadOnly();
            var rightCenterContour     = new List<Vector2>() { new Vector2(33f, 33f), new Vector2(333f, 333f) }.AsReadOnly();
            var nextRightCenterContour = new List<Vector2>() { new Vector2(22f, 22f), new Vector2(222f, 222f) }.AsReadOnly();

            MockCellEdgeContourCanon.Setup(canon => canon.GetContourForCellEdge(center,    HexDirection.E )).Returns(centerRightContour);
            MockCellEdgeContourCanon.Setup(canon => canon.GetContourForCellEdge(right,     HexDirection.W )).Returns(rightCenterContour);
            MockCellEdgeContourCanon.Setup(canon => canon.GetContourForCellEdge(nextRight, HexDirection.NW)).Returns(nextRightCenterContour);

            MockGeometry2D.Setup(geometry => geometry.IsPointWithinTriangle(
                point, centerRightContour.Last(), rightCenterContour.First(), nextRightCenterContour.Last()
            )).Returns(true);

            float centerWeight = 1f, rightWeight = 2f, nextRightWeight = 3f, riverHeightWeight = 4f, riverAlphaWeight = 5f;
            MockPointOrientationWeightLogic.Setup(
                logic => logic.GetRiverCornerWeights(
                    point, center, right, nextRight, HexDirection.E, out centerWeight,
                    out rightWeight, out nextRightWeight, out riverHeightWeight, out riverAlphaWeight
                )
            );

            PointOrientationData data;

            var sextantLogic = Container.Resolve<PointOrientationInSextantLogic>();

            Assert.IsTrue(
                sextantLogic.TryFindValidOrientation(point, center, HexDirection.E, out data),
                "Did not return true as expected"
            );

            Assert.AreEqual(centerWeight,      data.CenterWeight,      "Unexpected CenterWeight");
            Assert.AreEqual(rightWeight,       data.RightWeight,       "Unexpected RightWeight");
            Assert.AreEqual(nextRightWeight,   data.NextRightWeight,   "Unexpected NextRightWeight");
            Assert.AreEqual(riverHeightWeight, data.RiverHeightWeight, "Unexpected RiverHeightWeight");
            Assert.AreEqual(riverAlphaWeight,  data.RiverAlphaWeight,  "Unexpected RiverAlphaWeight");
        }

        [Test]
        public void TryFindValidOrientation_AndNoDefinedConditionsMet_ReturnsFalse() {
            var center    = BuildCell();
            var left      = BuildCell();
            var right     = BuildCell();
            var nextRight = BuildCell();

            MockGrid.Setup(grid => grid.HasNeighbor(center, HexDirection.NE)).Returns(true);
            MockGrid.Setup(grid => grid.HasNeighbor(center, HexDirection.E )).Returns(true);
            MockGrid.Setup(grid => grid.HasNeighbor(center, HexDirection.SE)).Returns(true);

            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.NE)).Returns(left);
            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.E )).Returns(right);
            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.SE)).Returns(nextRight);

            var data = new PointOrientationData();

            var sextantLogic = Container.Resolve<PointOrientationInSextantLogic>();

            Assert.IsFalse(sextantLogic.TryFindValidOrientation(Vector2.zero, center, HexDirection.E, out data));
        }

        #endregion

        #region utilities

        private IHexCell BuildCell() {
            return new Mock<IHexCell>().Object;
        }

        #endregion

        #endregion

    }

}
