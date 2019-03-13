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

    public class ContourRationalizerTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<ICellEdgeContourCanon> MockCellEdgeContourCanon;
        private Mock<IGeometry2D>           MockGeometry2D;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockCellEdgeContourCanon = new Mock<ICellEdgeContourCanon>();
            MockGeometry2D = new Mock<IGeometry2D>();

            Container.Bind<ICellEdgeContourCanon>().FromInstance(MockCellEdgeContourCanon.Object);
            Container.Bind<IGeometry2D>          ().FromInstance(MockGeometry2D          .Object);

            Container.Bind<ContourRationalizer>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void RationalizeCellContours_AndTestedContoursCollide_CollisionPointBecomesCLContourEndpoint() {
            var center = BuildCell();
            
            var centerLeftContour = new List<Vector2>() {
                new Vector2(1f, 1f), new Vector2(2f, 1f), new Vector2(3f, 1f),
                new Vector2(4f, 1f), new Vector2(5f, 1f), new Vector2(6f, 1f)
            }.AsReadOnly();

            var centerRightContour = new List<Vector2>() {
                new Vector2(2.5f, 1f), new Vector2(2.5f, 2f), new Vector2(2.5f, 3f),
                new Vector2(2.5f, 4f), new Vector2(2.5f, 5f), new Vector2(2.5f, 6f)
            }.AsReadOnly();

            MockCellEdgeContourCanon.Setup(canon => canon.GetContourForCellEdge(center, HexDirection.NE)).Returns(centerLeftContour);
            MockCellEdgeContourCanon.Setup(canon => canon.GetContourForCellEdge(center, HexDirection.E )).Returns(centerRightContour);

            Vector2 intersection = new Vector2(-1f, -1f);
            MockGeometry2D.Setup(
                geometry => geometry.AreLineSegmentsCrossing(
                    centerLeftContour [2], centerLeftContour [1], out intersection,
                    centerRightContour[1], centerRightContour[2], out intersection
                )
            ).Returns(true);

            MockCellEdgeContourCanon.Setup(
                canon => canon.SetContourForCellEdge(center, HexDirection.NE, It.IsAny<List<Vector2>>())
            ).Callback<IHexCell, HexDirection, List<Vector2>>(
                (cell, direction, contour) => {
                    Assert.AreEqual(intersection, contour.Last(), "CenterLeftContour has an unexpected last endpoint");
                }
            );

            var rationalizer = Container.Resolve<ContourRationalizer>();

            rationalizer.RationalizeCellContours(center, HexDirection.E);
        }

        [Test]
        public void RationalizeCellContours_AndTestedContoursCollide_CollisionPointBecomesNewCRContourStart() {
            var center = BuildCell();
            
            var centerLeftContour = new List<Vector2>() {
                new Vector2(1f, 1f), new Vector2(2f, 1f), new Vector2(3f, 1f),
                new Vector2(4f, 1f), new Vector2(5f, 1f), new Vector2(6f, 1f)
            }.AsReadOnly();

            var centerRightContour = new List<Vector2>() {
                new Vector2(2.5f, 1f), new Vector2(2.5f, 2f), new Vector2(2.5f, 3f),
                new Vector2(2.5f, 4f), new Vector2(2.5f, 5f), new Vector2(2.5f, 6f)
            }.AsReadOnly();

            MockCellEdgeContourCanon.Setup(canon => canon.GetContourForCellEdge(center, HexDirection.NE)).Returns(centerLeftContour);
            MockCellEdgeContourCanon.Setup(canon => canon.GetContourForCellEdge(center, HexDirection.E )).Returns(centerRightContour);

            Vector2 intersection = new Vector2(-1f, -1f);
            MockGeometry2D.Setup(
                geometry => geometry.AreLineSegmentsCrossing(
                    centerLeftContour [2], centerLeftContour [1], out intersection,
                    centerRightContour[1], centerRightContour[2], out intersection
                )
            ).Returns(true);

            MockCellEdgeContourCanon.Setup(
                canon => canon.SetContourForCellEdge(center, HexDirection.E, It.IsAny<List<Vector2>>())
            ).Callback<IHexCell, HexDirection, List<Vector2>>(
                (cell, direction, contour) => {
                    Assert.AreEqual(intersection, contour.First(), "CenterRightContour has an unexpected last endpoint");
                }
            );

            var rationalizer = Container.Resolve<ContourRationalizer>();

            rationalizer.RationalizeCellContours(center, HexDirection.E);
        }

        [Test]
        public void RationalizeCellContours_AndTestedContoursCollide_RemovesOverlappingPointsFromEndOfCLContour() {
            var center = BuildCell();
            
            var centerLeftContour = new List<Vector2>() {
                new Vector2(1f, 1f), new Vector2(2f, 1f), new Vector2(3f, 1f),
                new Vector2(4f, 1f), new Vector2(5f, 1f), new Vector2(6f, 1f)
            }.AsReadOnly();

            var centerRightContour = new List<Vector2>() {
                new Vector2(2.5f, 1f), new Vector2(2.5f, 2f), new Vector2(2.5f, 3f),
                new Vector2(2.5f, 4f), new Vector2(2.5f, 5f), new Vector2(2.5f, 6f)
            }.AsReadOnly();

            MockCellEdgeContourCanon.Setup(canon => canon.GetContourForCellEdge(center, HexDirection.NE)).Returns(centerLeftContour);
            MockCellEdgeContourCanon.Setup(canon => canon.GetContourForCellEdge(center, HexDirection.E )).Returns(centerRightContour);

            Vector2 intersection = new Vector2(-1f, -1f);
            MockGeometry2D.Setup(
                geometry => geometry.AreLineSegmentsCrossing(
                    centerLeftContour [2], centerLeftContour [1], out intersection,
                    centerRightContour[1], centerRightContour[2], out intersection
                )
            ).Returns(true);

            MockCellEdgeContourCanon.Setup(
                canon => canon.SetContourForCellEdge(center, HexDirection.NE, It.IsAny<List<Vector2>>())
            ).Callback<IHexCell, HexDirection, List<Vector2>>(
                (cell, direction, contour) => {
                    Assert.AreEqual(3, contour.Count, "CenterLeftContour has an unexpected number of elements");

                    Assert.AreEqual   (centerLeftContour[0], contour[0], "CenterLeftContour[0] changed unexpectedly");
                    Assert.AreEqual   (centerLeftContour[1], contour[1], "CenterLeftContour[1] changed unexpectedly");
                    Assert.AreNotEqual(centerLeftContour[2], contour[2], "CenterLeftContour[2] did not change as expected");
                }
            );

            var rationalizer = Container.Resolve<ContourRationalizer>();

            rationalizer.RationalizeCellContours(center, HexDirection.E);
        }

        [Test]
        public void RationalizeCellContours_AndTestedContoursCollide_RemovesOverlappingPointsFromStartOfCRContour() {
            var center = BuildCell();
            
            var centerLeftContour = new List<Vector2>() {
                new Vector2(1f, 1f), new Vector2(2f, 1f), new Vector2(3f, 1f),
                new Vector2(4f, 1f), new Vector2(5f, 1f), new Vector2(6f, 1f)
            }.AsReadOnly();

            var centerRightContour = new List<Vector2>() {
                new Vector2(2.5f, 1f), new Vector2(2.5f, 2f), new Vector2(2.5f, 3f),
                new Vector2(2.5f, 4f), new Vector2(2.5f, 5f), new Vector2(2.5f, 6f)
            }.AsReadOnly();

            MockCellEdgeContourCanon.Setup(canon => canon.GetContourForCellEdge(center, HexDirection.NE)).Returns(centerLeftContour);
            MockCellEdgeContourCanon.Setup(canon => canon.GetContourForCellEdge(center, HexDirection.E )).Returns(centerRightContour);

            Vector2 intersection = new Vector2(-1f, -1f);
            MockGeometry2D.Setup(
                geometry => geometry.AreLineSegmentsCrossing(
                    centerLeftContour [2], centerLeftContour [1], out intersection,
                    centerRightContour[1], centerRightContour[2], out intersection
                )
            ).Returns(true);

            MockCellEdgeContourCanon.Setup(
                canon => canon.SetContourForCellEdge(center, HexDirection.E, It.IsAny<List<Vector2>>())
            ).Callback<IHexCell, HexDirection, List<Vector2>>(
                (cell, direction, contour) => {
                    Assert.AreEqual(5, contour.Count, "CenterLeftContour has an unexpected number of elements");

                    Assert.AreEqual   (centerRightContour[5], contour[4], "CenterLeftContour[4] has an unexpected value");
                    Assert.AreEqual   (centerRightContour[4], contour[3], "CenterLeftContour[3] has an unexpected value");
                    Assert.AreEqual   (centerRightContour[3], contour[2], "CenterLeftContour[2] has an unexpected value");
                    Assert.AreEqual   (centerRightContour[2], contour[1], "CenterLeftContour[1] has an unexpected value");
                    Assert.AreNotEqual(centerRightContour[1], contour[0], "CenterLeftContour[0] has not changed as expected");
                }
            );

            var rationalizer = Container.Resolve<ContourRationalizer>();

            rationalizer.RationalizeCellContours(center, HexDirection.E);
        }

        [Test]
        public void RationalizeCellContours_AndOnlyCLContourStopsShort_CLExtrapolatedForwardFromLastTwoPoints() {
            var center = BuildCell();
            
            var centerLeftContour = new List<Vector2>() {
                new Vector2(0f, 0f), new Vector2(1f, 0f), new Vector2(2f, 0f)
            }.AsReadOnly();

            var centerRightContour = new List<Vector2>() {
                new Vector2(2.5f, -1.5f), new Vector2(2.5f, -0.5f), new Vector2(2.5f, 0.5f)
            }.AsReadOnly();

            MockCellEdgeContourCanon.Setup(canon => canon.GetContourForCellEdge(center, HexDirection.NE)).Returns(centerLeftContour);
            MockCellEdgeContourCanon.Setup(canon => canon.GetContourForCellEdge(center, HexDirection.E )).Returns(centerRightContour);

            Vector2 intersection = new Vector2(-1f, -1f);
            MockGeometry2D.Setup(
                geometry => geometry.AreLineSegmentsCrossing(
                    centerLeftContour [2], centerLeftContour [2] + (centerLeftContour[2] - centerLeftContour[1]), out intersection,
                    centerRightContour[1], centerRightContour[2], out intersection
                )
            ).Returns(true);

            MockCellEdgeContourCanon.Setup(
                canon => canon.SetContourForCellEdge(center, HexDirection.NE, It.IsAny<List<Vector2>>())
            ).Callback<IHexCell, HexDirection, List<Vector2>>(
                (cell, direction, contour) => {
                    Assert.AreEqual(intersection, contour.Last(), "CenterLeftContour has an unexpected last element");

                    CollectionAssert.AreEqual(centerLeftContour, contour.GetRange(0, 3), "CenterLeftContour doesn't contain its original points");
                }
            );

            MockCellEdgeContourCanon.Setup(
                canon => canon.SetContourForCellEdge(center, HexDirection.E, It.IsAny<List<Vector2>>())
            ).Callback<IHexCell, HexDirection, List<Vector2>>(
                (cell, direction, contour) => {
                    Assert.AreEqual(2, contour.Count, "CenterRightContour not culled as expected");
                }
            );

            var rationalizer = Container.Resolve<ContourRationalizer>();

            rationalizer.RationalizeCellContours(center, HexDirection.E);
        }

        [Test]
        public void RationalizeCellContours_AndOnlyCRContourStopsShort_CRExtrapolatedBackwardFromFirstTwoPoints() {
            var center = BuildCell();
            
            var centerLeftContour = new List<Vector2>() {
                new Vector2(0f, 2.5f), new Vector2(1f, 2.5f), new Vector2(2f, 2.5f)
            }.AsReadOnly();

            var centerRightContour = new List<Vector2>() {
                new Vector2(0.5f, 0f), new Vector2(0.5f, 1f), new Vector2(0.5f, 2f),
            }.AsReadOnly();

            MockCellEdgeContourCanon.Setup(canon => canon.GetContourForCellEdge(center, HexDirection.NE)).Returns(centerLeftContour);
            MockCellEdgeContourCanon.Setup(canon => canon.GetContourForCellEdge(center, HexDirection.E )).Returns(centerRightContour);

            Vector2 intersection = new Vector2(-1f, -1f);
            MockGeometry2D.Setup(
                geometry => geometry.AreLineSegmentsCrossing(
                    centerLeftContour [1], centerLeftContour [0], out intersection,
                    centerRightContour[0], centerRightContour[0] + (centerRightContour[0] - centerRightContour[1]), out intersection
                )
            ).Returns(true);

            MockCellEdgeContourCanon.Setup(
                canon => canon.SetContourForCellEdge(center, HexDirection.NE, It.IsAny<List<Vector2>>())
            ).Callback<IHexCell, HexDirection, List<Vector2>>(
                (cell, direction, contour) => {
                    Assert.AreEqual(2, contour.Count, "CenterLeftContour not culled as expected");
                }
            );

            MockCellEdgeContourCanon.Setup(
                canon => canon.SetContourForCellEdge(center, HexDirection.E, It.IsAny<List<Vector2>>())
            ).Callback<IHexCell, HexDirection, List<Vector2>>(
                (cell, direction, contour) => {
                    Assert.AreEqual(intersection, contour.First(), "CenterRightContour has an unexpected first element");

                    CollectionAssert.AreEqual(centerRightContour, contour.GetRange(1, 3), "CenterRightContour doesn't contain its original points");
                }
            );

            var rationalizer = Container.Resolve<ContourRationalizer>();

            rationalizer.RationalizeCellContours(center, HexDirection.E);
        }

        [Test]
        public void RationalizeCellContours_AndBothContoursStopShort_ExtrapolatesFromBoth() {
            var center = BuildCell();
            
            var centerLeftContour = new List<Vector2>() {
                new Vector2(0f, -5f), new Vector2(0f, -3f), new Vector2(0f, -1f)
            }.AsReadOnly();

            var centerRightContour = new List<Vector2>() {
                new Vector2(1f, 0f), new Vector2(3f, 0f), new Vector2(5f, 0f)
            }.AsReadOnly();

            MockCellEdgeContourCanon.Setup(canon => canon.GetContourForCellEdge(center, HexDirection.NE)).Returns(centerLeftContour);
            MockCellEdgeContourCanon.Setup(canon => canon.GetContourForCellEdge(center, HexDirection.E )).Returns(centerRightContour);

            Vector2 intersection = new Vector2(-1f, -1f);
            MockGeometry2D.Setup(
                geometry => geometry.AreLineSegmentsCrossing(
                    centerLeftContour [2], centerLeftContour [2] + (centerLeftContour [2] - centerLeftContour [1]), out intersection,
                    centerRightContour[0], centerRightContour[0] + (centerRightContour[0] - centerRightContour[1]), out intersection
                )
            ).Returns(true);

            MockCellEdgeContourCanon.Setup(
                canon => canon.SetContourForCellEdge(center, HexDirection.NE, It.IsAny<List<Vector2>>())
            ).Callback<IHexCell, HexDirection, List<Vector2>>(
                (cell, direction, contour) => {
                    Assert.AreEqual(intersection, contour.Last(), "CenterLeftContour has an unexpected last element");

                    CollectionAssert.AreEqual(centerLeftContour, contour.GetRange(0, 3), "CenterLeftContour doesn't contain its original points");
                }
            );

            MockCellEdgeContourCanon.Setup(
                canon => canon.SetContourForCellEdge(center, HexDirection.E, It.IsAny<List<Vector2>>())
            ).Callback<IHexCell, HexDirection, List<Vector2>>(
                (cell, direction, contour) => {
                    Assert.AreEqual(intersection, contour.First(), "CenterRightContour has an unexpected first element");

                    CollectionAssert.AreEqual(centerRightContour, contour.GetRange(1, 3), "CenterRightContour doesn't contain its original points");
                }
            );

            var rationalizer = Container.Resolve<ContourRationalizer>();

            rationalizer.RationalizeCellContours(center, HexDirection.E);
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
