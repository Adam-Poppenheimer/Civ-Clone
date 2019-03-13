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

    public class CellEdgeContourCanonTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IMapRenderConfig> MockRenderConfig;
        private Mock<IGeometry2D>      MockGeometry2D;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockRenderConfig = new Mock<IMapRenderConfig>();
            MockGeometry2D = new Mock<IGeometry2D>();

            Container.Bind<IMapRenderConfig>().FromInstance(MockRenderConfig.Object);
            Container.Bind<IGeometry2D>     ().FromInstance(MockGeometry2D  .Object);

            Container.Bind<CellEdgeContourCanon>().AsSingle();
        }

        #endregion

        #region tests

        #region getting and setting contours

        [Test]
        public void GetContourForCellEdge_DefaultsToAContourBetweenFirstAndSecondCorners() {
            var cell = BuildCell(new Vector2(1f, 2f));

            MockRenderConfig.Setup(config => config.GetFirstCornerXZ (HexDirection.E)).Returns(new Vector2(10f,  20f ));
            MockRenderConfig.Setup(config => config.GetSecondCornerXZ(HexDirection.E)).Returns(new Vector2(100f, 200f));

            var contourCanon = Container.Resolve<CellEdgeContourCanon>();

            CollectionAssert.AreEqual(
                new List<Vector2>() { new Vector2(11f, 22f), new Vector2(101f, 202f) },
                contourCanon.GetContourForCellEdge(cell, HexDirection.E)
            );
        }

        [Test]
        public void SetContourForCellEdge_ReflectedInGetContourForCellEdge() {
            var cell = BuildCell(new Vector2(1f, 2f));

            MockRenderConfig.Setup(config => config.GetFirstCornerXZ (HexDirection.E)).Returns(new Vector2(10f,  20f ));
            MockRenderConfig.Setup(config => config.GetSecondCornerXZ(HexDirection.E)).Returns(new Vector2(100f, 200f));

            var newContour = new List<Vector2>() { new Vector2(1f, 3f), new Vector2(1f, 2f), new Vector2(1f, 1f) };

            var contourCanon = Container.Resolve<CellEdgeContourCanon>();

            contourCanon.SetContourForCellEdge(cell, HexDirection.E, newContour);

            CollectionAssert.AreEqual(newContour, contourCanon.GetContourForCellEdge(cell, HexDirection.E));
        }

        [Test]
        public void SetContourForCellEdge_NotReflectedInContoursForDifferentCells() {
            var cellOne = BuildCell(new Vector2(1f, 2f));
            var cellTwo = BuildCell(Vector2.zero);

            MockRenderConfig.Setup(config => config.GetFirstCornerXZ (HexDirection.E)).Returns(new Vector2(10f,  20f ));
            MockRenderConfig.Setup(config => config.GetSecondCornerXZ(HexDirection.E)).Returns(new Vector2(100f, 200f));

            var newContour = new List<Vector2>() { new Vector2(1f, 1f), new Vector2(2f, 2f), new Vector2(3f, 3f) };

            var contourCanon = Container.Resolve<CellEdgeContourCanon>();

            contourCanon.SetContourForCellEdge(cellOne, HexDirection.E, newContour);

            CollectionAssert.AreEqual(
                new List<Vector2>() { new Vector2(10f, 20f), new Vector2(100f, 200f) },
                contourCanon.GetContourForCellEdge(cellTwo, HexDirection.E)
            );
        }

        [Test]
        public void SetContourForCellEdge_NotReflectedInDifferentEdgesOfTheSameCell() {
            var cell = BuildCell(new Vector2(1f, 2f));

            MockRenderConfig.Setup(config => config.GetFirstCornerXZ (It.IsAny<HexDirection>())).Returns(new Vector2(10f,  20f ));
            MockRenderConfig.Setup(config => config.GetSecondCornerXZ(It.IsAny<HexDirection>())).Returns(new Vector2(100f, 200f));

            var newContour = new List<Vector2>() { new Vector2(1f, 1f), new Vector2(2f, 2f), new Vector2(3f, 3f) };

            var contourCanon = Container.Resolve<CellEdgeContourCanon>();

            contourCanon.SetContourForCellEdge(cell, HexDirection.E, newContour);

            CollectionAssert.AreEqual(
                new List<Vector2>() { new Vector2(11f, 22f), new Vector2(101f, 202f) },
                contourCanon.GetContourForCellEdge(cell, HexDirection.SE)
            );
        }

        [Test]
        public void SetContourForCellEdge_ThrowsOnNullArguments() {
            var cell = BuildCell(new Vector2(1f, 2f));

            var newContour = new List<Vector2>() { new Vector2(1f, 1f), new Vector2(2f, 2f), new Vector2(3f, 3f) };

            var contourCanon = Container.Resolve<CellEdgeContourCanon>();

            Assert.Throws<ArgumentNullException>(
                () => contourCanon.SetContourForCellEdge(null, HexDirection.NE, newContour),
                "Not thrown when cell is null"
            );

            Assert.Throws<ArgumentNullException>(
                () => contourCanon.SetContourForCellEdge(cell, HexDirection.NE, null),
                "Not thrown when contour is null"
            );
        }

        [Test]
        public void SetContourForCellEdge_ThrowsArgumentExceptionIfContourHasOnePoint() {
            var cell = BuildCell(new Vector2(1f, 2f));

            var newContour = new List<Vector2>() { new Vector2(1f, 1f) };

            var contourCanon = Container.Resolve<CellEdgeContourCanon>();

            Assert.Throws<ArgumentException>(
                () => contourCanon.SetContourForCellEdge(cell, HexDirection.E, newContour)
            );
        }

        [Test]
        public void SetContourForCellEdge_ThrowsArgumentExceptionIfContourHasZeroPoints() {
            var cell = BuildCell(new Vector2(1f, 2f));

            var newContour = new List<Vector2>();

            var contourCanon = Container.Resolve<CellEdgeContourCanon>();

            Assert.Throws<ArgumentException>(
                () => contourCanon.SetContourForCellEdge(cell, HexDirection.E, newContour)
            );
        }

        [Test]
        public void SetContourForCellEdge_ReversesContourIfEndIsCCWOfStart() {
            var cell = BuildCell(new Vector2(1f, 1f));

            var newContour = new List<Vector2>() {
                new Vector2(11f, 1f), new Vector2(6f, 3f), new Vector2(3f, 6f), new Vector2(1f, 11f)
            };

            var contourCanon = Container.Resolve<CellEdgeContourCanon>();

            contourCanon.SetContourForCellEdge(cell, HexDirection.E, newContour);

            newContour.Reverse();

            CollectionAssert.AreEqual(newContour, contourCanon.GetContourForCellEdge(cell, HexDirection.E));
        }

        [Test]
        public void SetContourForCellEdge_DoesNotReverseContourIfEndIsCWOfStart() {
            var cell = BuildCell(new Vector2(1f, 1f));

            var newContour = new List<Vector2>() {
                new Vector2(1f, 11f), new Vector2(3f, 6f), new Vector2(6f, 3f), new Vector2(11f, 1f),
            };

            var contourCanon = Container.Resolve<CellEdgeContourCanon>();

            contourCanon.SetContourForCellEdge(cell, HexDirection.E, newContour);

            CollectionAssert.AreEqual(newContour, contourCanon.GetContourForCellEdge(cell, HexDirection.E));
        }

        [Test]
        public void GetContourForCellEdge_ThrowsOnNullCellArgument() {
            var contourCanon = Container.Resolve<CellEdgeContourCanon>();

            Assert.Throws<ArgumentNullException>(() => contourCanon.GetContourForCellEdge(null, HexDirection.E));
        }

        [Test]
        public void Clear_ResetsContoursToDefaults() {
            var cellOne   = BuildCell(new Vector2(1f, 1f));
            var cellTwo   = BuildCell(new Vector2(2f, 2f));
            var cellThree = BuildCell(new Vector2(3f, 3f));

            MockRenderConfig.Setup(config => config.GetFirstCornerXZ (It.IsAny<HexDirection>())).Returns(new Vector2(10f,  20f ));
            MockRenderConfig.Setup(config => config.GetSecondCornerXZ(It.IsAny<HexDirection>())).Returns(new Vector2(100f, 200f));

            var contourOne   = new List<Vector2>() { new Vector2(-11f, -11f), new Vector2(-11f, -11f) };
            var contourTwo   = new List<Vector2>() { new Vector2(-22f, -22f), new Vector2(-22f, -22f) };
            var contourThree = new List<Vector2>() { new Vector2(-33f, -33f), new Vector2(-33f, -33f) };
            var contourFour  = new List<Vector2>() { new Vector2(-44f, -44f), new Vector2(-44f, -44f) };

            var contourCanon = Container.Resolve<CellEdgeContourCanon>();

            contourCanon.SetContourForCellEdge(cellOne,   HexDirection.NE, contourOne);
            contourCanon.SetContourForCellEdge(cellTwo,   HexDirection.W,  contourTwo);
            contourCanon.SetContourForCellEdge(cellThree, HexDirection.NE, contourThree);
            contourCanon.SetContourForCellEdge(cellThree, HexDirection.SE, contourFour);

            contourCanon.Clear();

            CollectionAssert.AreEqual(
                new List<Vector2>() { new Vector2(11f, 21f), new Vector2(101f, 201f) },
                contourCanon.GetContourForCellEdge(cellOne, HexDirection.NE),
                "CellOne.NE not cleared as expected"
            );

            CollectionAssert.AreEqual(
                new List<Vector2>() { new Vector2(12f, 22f), new Vector2(102f, 202f) },
                contourCanon.GetContourForCellEdge(cellTwo, HexDirection.W),
                "CellTwo.W not cleared as expected"
            );

            CollectionAssert.AreEqual(
                new List<Vector2>() { new Vector2(13f, 23f), new Vector2(103f, 203f) },
                contourCanon.GetContourForCellEdge(cellThree, HexDirection.NE),
                "CellThree.NE not cleared as expected"
            );

            CollectionAssert.AreEqual(
                new List<Vector2>() { new Vector2(13f, 23f), new Vector2(103f, 203f) },
                contourCanon.GetContourForCellEdge(cellThree, HexDirection.SE),
                "CellThree.SE not cleared as expected"
            );
        }

        #endregion

        #region IsPointWithinContour

        [Test]
        public void IsPointWithinContour_ReturnsTrueIfPointInSomeTriangle_BetweenContourAtEdgeAndCellMidpoint() {
            var cell = BuildCell(new Vector2(1f, 2f));

            var contour = new List<Vector2>() {
                new Vector2(10f, 100f), new Vector2(20f, 200f), new Vector2(30f, 300f), new Vector2(40f, 400f)
            };

            var point = new Vector2(-1f, -1f);

            MockGeometry2D.Setup(geometry => geometry.IsPointWithinTriangle(
                point, cell.AbsolutePositionXZ, contour[0], contour[1]
            )).Returns(false);

            MockGeometry2D.Setup(geometry => geometry.IsPointWithinTriangle(
                point, cell.AbsolutePositionXZ, contour[1], contour[2]
            )).Returns(true);

            MockGeometry2D.Setup(geometry => geometry.IsPointWithinTriangle(
                point, cell.AbsolutePositionXZ, contour[2], contour[3]
            )).Returns(false);

            var contourCanon = Container.Resolve<CellEdgeContourCanon>();

            contourCanon.SetContourForCellEdge(cell, HexDirection.E, contour);

            Assert.IsTrue(contourCanon.IsPointWithinContour(point, cell, HexDirection.E));
        }

        [Test]
        public void IsPointWithinContour_ReturnsFalseIfPointInNoTriangle_BetweenContourAtEdgeAndCellMidpoint() {
            var cell = BuildCell(new Vector2(1f, 2f));

            var contour = new List<Vector2>() {
                new Vector2(10f, 100f), new Vector2(20f, 200f), new Vector2(30f, 300f), new Vector2(40f, 400f)
            };

            var point = new Vector2(-1f, -1f);

            MockGeometry2D.Setup(geometry => geometry.IsPointWithinTriangle(
                point, cell.AbsolutePositionXZ, contour[0], contour[1]
            )).Returns(false);

            MockGeometry2D.Setup(geometry => geometry.IsPointWithinTriangle(
                point, cell.AbsolutePositionXZ, contour[1], contour[2]
            )).Returns(false);

            MockGeometry2D.Setup(geometry => geometry.IsPointWithinTriangle(
                point, cell.AbsolutePositionXZ, contour[2], contour[3]
            )).Returns(false);

            var contourCanon = Container.Resolve<CellEdgeContourCanon>();

            contourCanon.SetContourForCellEdge(cell, HexDirection.E, contour);

            Assert.IsFalse(contourCanon.IsPointWithinContour(point, cell, HexDirection.E));
        }

        [Test]
        public void IsPointWithinContour_ThrowsIfCellNull() {
            var contourCanon = Container.Resolve<CellEdgeContourCanon>();

            Assert.Throws<ArgumentNullException>(() => contourCanon.IsPointWithinContour(Vector2.zero, null, HexDirection.E));
        }

        #endregion

        #region IsPointBetwenContours

        [Test(Description =
            "Quads should be assembled from all sets of four points opposite the midline of the contour. " +
            "For this reason, the first two elements of contourOne should be paired with the last two " +
            "elements of contourTwo, since all contours ought to be declared in clockwise order." 
        )]
        public void IsPointBetweenContours_PointInSomeQuadBetweenThem_AndContoursSameLength_ReturnsTrue() {
            var point = new Vector2(2f, 2f);

            var contourOne = new List<Vector2>() {
                new Vector2(1f, 1f), new Vector2(3f, 1f), new Vector2(5f, 1f)
            }.AsReadOnly();

            var contourTwo = new List<Vector2>() {
                new Vector2(5f, 3f), new Vector2(3f, 3f), new Vector2(1f, 3f)
            }.AsReadOnly();

            MockGeometry2D.Setup(geometry => geometry.IsPointWithinTriangle(point, contourOne[0], contourTwo[2], contourOne[1])).Returns(false);
            MockGeometry2D.Setup(geometry => geometry.IsPointWithinTriangle(point, contourOne[1], contourTwo[2], contourTwo[1])).Returns(true);

            MockGeometry2D.Setup(geometry => geometry.IsPointWithinTriangle(point, contourOne[2], contourTwo[1], contourOne[2])).Returns(false);
            MockGeometry2D.Setup(geometry => geometry.IsPointWithinTriangle(point, contourOne[2], contourTwo[1], contourTwo[0])).Returns(false);

            var cellEdgeContourCanon = Container.Resolve<CellEdgeContourCanon>();

            Assert.IsTrue(cellEdgeContourCanon.IsPointBetweenContours(point, contourOne, contourTwo));
        }

        [Test]
        public void IsPointBetweenContours_PointNotInSomeQuadBetweenThem_AndContoursSameLength_ReturnsFalse() {
            var point = new Vector2(2f, 2f);

            var contourOne = new List<Vector2>() {
                new Vector2(1f, 1f), new Vector2(3f, 1f), new Vector2(5f, 1f)
            }.AsReadOnly();

            var contourTwo = new List<Vector2>() {
                new Vector2(5f, 3f), new Vector2(3f, 3f), new Vector2(1f, 3f)
            }.AsReadOnly();

            MockGeometry2D.Setup(geometry => geometry.IsPointWithinTriangle(point, contourOne[0], contourTwo[2], contourOne[1])).Returns(false);
            MockGeometry2D.Setup(geometry => geometry.IsPointWithinTriangle(point, contourOne[1], contourTwo[2], contourTwo[1])).Returns(false);

            MockGeometry2D.Setup(geometry => geometry.IsPointWithinTriangle(point, contourOne[2], contourTwo[1], contourOne[2])).Returns(false);
            MockGeometry2D.Setup(geometry => geometry.IsPointWithinTriangle(point, contourOne[2], contourTwo[1], contourTwo[0])).Returns(false);

            var cellEdgeContourCanon = Container.Resolve<CellEdgeContourCanon>();

            Assert.IsFalse(cellEdgeContourCanon.IsPointBetweenContours(point, contourOne, contourTwo));
        }

        [Test]
        public void IsPointBetweenContours_PointInSomeQuadBetweenThem_AndContoursDifferentLength_ReturnsTrue() {
            var point = new Vector2(2f, 2f);

            var contourOne = new List<Vector2>() {
                new Vector2(1f, 1f), new Vector2(3f, 1f), new Vector2(5f, 1f)
            }.AsReadOnly();

            var contourTwo = new List<Vector2>() {
                new Vector2(7f, 3f), new Vector2(5f, 3f), new Vector2(3f, 3f), new Vector2(1f, 3f)
            }.AsReadOnly();

            MockGeometry2D.Setup(geometry => geometry.IsPointWithinTriangle(point, contourOne[0], contourTwo[3], contourOne[1])).Returns(false);
            MockGeometry2D.Setup(geometry => geometry.IsPointWithinTriangle(point, contourOne[1], contourTwo[3], contourTwo[2])).Returns(true);

            MockGeometry2D.Setup(geometry => geometry.IsPointWithinTriangle(point, contourOne[2], contourTwo[2], contourOne[2])).Returns(false);
            MockGeometry2D.Setup(geometry => geometry.IsPointWithinTriangle(point, contourOne[2], contourTwo[2], contourTwo[1])).Returns(false);

            var cellEdgeContourCanon = Container.Resolve<CellEdgeContourCanon>();

            Assert.IsTrue(cellEdgeContourCanon.IsPointBetweenContours(point, contourOne, contourTwo));
        }

        [Test]
        public void IsPointBetweenContours_ContourOneIsShorter_AndPointBeyondOne_ButPointWithinTwo_ReturnsTrue() {
            var point = new Vector2(6f, 2.5f);

            var contourOne = new List<Vector2>() {
                new Vector2(1f, 1f), new Vector2(3f, 1f), new Vector2(5f, 1f)
            }.AsReadOnly();

            var contourTwo = new List<Vector2>() {
                new Vector2(7f, 3f), new Vector2(5f, 3f), new Vector2(3f, 3f), new Vector2(1f, 3f)
            }.AsReadOnly();

            MockGeometry2D.Setup(geometry => geometry.IsPointWithinTriangle(point, contourOne[2], contourTwo[1], contourTwo[0])).Returns(true);

            var cellEdgeContourCanon = Container.Resolve<CellEdgeContourCanon>();

            Assert.IsTrue(cellEdgeContourCanon.IsPointBetweenContours(point, contourOne, contourTwo));
        }

        [Test]
        public void IsPointBetweenContours_ContourOneIsShorter_AndPointBeyondBothContours_ReturnsFalse() {
            var point = new Vector2(6f, 2.5f);

            var contourOne = new List<Vector2>() {
                new Vector2(1f, 1f), new Vector2(3f, 1f), new Vector2(5f, 1f)
            }.AsReadOnly();

            var contourTwo = new List<Vector2>() {
                new Vector2(7f, 3f), new Vector2(5f, 3f), new Vector2(3f, 3f), new Vector2(1f, 3f)
            }.AsReadOnly();

            MockGeometry2D.Setup(geometry => geometry.IsPointWithinTriangle(point, contourOne[2], contourTwo[1], contourTwo[0])).Returns(false);

            var cellEdgeContourCanon = Container.Resolve<CellEdgeContourCanon>();

            Assert.IsFalse(cellEdgeContourCanon.IsPointBetweenContours(point, contourOne, contourTwo));
        }

        [Test]
        public void IsPointBetweenContours_ContourTwoIsShorter_AndPointBeyondTwo_ButPointWithinOne_ReturnsTrue() {
            var point = new Vector2(6f, 2.5f);

            var contourOne = new List<Vector2>() {
                new Vector2(1f, 1f), new Vector2(3f, 1f), new Vector2(5f, 1f), new Vector2(7f, 1f)
            }.AsReadOnly();

            var contourTwo = new List<Vector2>() {
                new Vector2(5f, 3f), new Vector2(3f, 3f), new Vector2(1f, 3f)
            }.AsReadOnly();

            MockGeometry2D.Setup(geometry => geometry.IsPointWithinTriangle(point, contourOne[2], contourTwo[0], contourOne[3])).Returns(true);

            var cellEdgeContourCanon = Container.Resolve<CellEdgeContourCanon>();

            Assert.IsTrue(cellEdgeContourCanon.IsPointBetweenContours(point, contourOne, contourTwo));
        }

        [Test]
        public void IsPointBetweenContours_ContourTwoIsShorter_AndPointBeyondBothContours_ReturnsFalse() {
            var point = new Vector2(6f, 2.5f);

            var contourOne = new List<Vector2>() {
                new Vector2(1f, 1f), new Vector2(3f, 1f), new Vector2(5f, 1f), new Vector2(7f, 1f)
            }.AsReadOnly();

            var contourTwo = new List<Vector2>() {
                new Vector2(5f, 3f), new Vector2(3f, 3f), new Vector2(1f, 3f)
            }.AsReadOnly();

            MockGeometry2D.Setup(geometry => geometry.IsPointWithinTriangle(point, contourOne[2], contourTwo[0], contourOne[3])).Returns(false);

            var cellEdgeContourCanon = Container.Resolve<CellEdgeContourCanon>();

            Assert.IsFalse(cellEdgeContourCanon.IsPointBetweenContours(point, contourOne, contourTwo));
        }

        [Test]
        public void IsPointBetweenContours_ThrowsOnNullArguments() {
            var contour = new List<Vector2>().AsReadOnly();

            var cellEdgeContourCanon = Container.Resolve<CellEdgeContourCanon>();

            Assert.Throws<ArgumentNullException>(
                () => cellEdgeContourCanon.IsPointBetweenContours(Vector2.zero, null, contour),
                "Failed to throw when contourOne was null"
            );

            Assert.Throws<ArgumentNullException>(
                () => cellEdgeContourCanon.IsPointBetweenContours(Vector2.zero, contour, null),
                "Failed to throw when contourTwo was null"
            );
        }

        [Test]
        public void IsPointBetweenContours_AndContourOneHasLessThanTwoElements_ThrowsArgumentException() {
            var contourOne = new List<Vector2>() { Vector2.zero               }.AsReadOnly();
            var contourTwo = new List<Vector2>() { Vector2.zero, Vector2.zero }.AsReadOnly();

            var cellEdgeContourCanon = Container.Resolve<CellEdgeContourCanon>();

            Assert.Throws<ArgumentException>(() => cellEdgeContourCanon.IsPointBetweenContours(Vector2.zero, contourOne, contourTwo));
        }

        [Test]
        public void IsPointBetweenContours_AndContourTwoHasLessThanTwoElements_ThrowsArgumentException() {
            var contourOne = new List<Vector2>() { Vector2.zero, Vector2.zero }.AsReadOnly();
            var contourTwo = new List<Vector2>() { Vector2.zero               }.AsReadOnly();

            var cellEdgeContourCanon = Container.Resolve<CellEdgeContourCanon>();

            Assert.Throws<ArgumentException>(() => cellEdgeContourCanon.IsPointBetweenContours(Vector2.zero, contourOne, contourTwo));
        }

        #endregion

        #region GetClosestPointOnContour

        [Test]
        public void GetClosestPointOnContour_TakesTheClosestPoint_FromAllLineSegments_MadeByAdjacentContourPoints() {
            Vector2 point = new Vector2(1f, 1f);

            List<Vector2> contour = new List<Vector2>() {
                new Vector2(10f, 100f), new Vector2(20f, 200f), new Vector2(30f, 300f), new Vector2(40f, 400f)
            };

            MockGeometry2D.Setup(geometry => geometry.GetClosestPointOnLineSegment(new Vector2(10f, 100f), new Vector2(20f, 200f), point)).Returns(new Vector2(5f, 5f));
            MockGeometry2D.Setup(geometry => geometry.GetClosestPointOnLineSegment(new Vector2(20f, 200f), new Vector2(30f, 300f), point)).Returns(new Vector2(2f, 2f));
            MockGeometry2D.Setup(geometry => geometry.GetClosestPointOnLineSegment(new Vector2(30f, 300f), new Vector2(40f, 400f), point)).Returns(new Vector2(4f, 4f));

            var contourCanon = Container.Resolve<CellEdgeContourCanon>();

            Assert.AreEqual(new Vector2(2f, 2f), contourCanon.GetClosestPointOnContour(point, contour.AsReadOnly()));
        }

        [Test]
        public void GetClosestContourPoint_ThrowsIfContourNull() {
            var contourCanon = Container.Resolve<CellEdgeContourCanon>();

            Assert.Throws<ArgumentNullException>(() => contourCanon.GetClosestPointOnContour(Vector2.zero, null));
        }

        #endregion

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
