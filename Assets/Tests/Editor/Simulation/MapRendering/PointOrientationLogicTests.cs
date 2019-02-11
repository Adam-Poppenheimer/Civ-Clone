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

    public class PointOrientationLogicTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IMapRenderConfig> MockRenderConfig;
        private Mock<IHexGrid>         MockGrid;
        private Mock<IGeometry2D>      MockGeometry2D;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockRenderConfig = new Mock<IMapRenderConfig>();
            MockGrid         = new Mock<IHexGrid>();
            MockGeometry2D   = new Mock<IGeometry2D>();

            Container.Bind<IMapRenderConfig>().FromInstance(MockRenderConfig.Object);
            Container.Bind<IHexGrid>        ().FromInstance(MockGrid        .Object);
            Container.Bind<IGeometry2D>     ().FromInstance(MockGeometry2D  .Object);

            Container.Bind<PointOrientationLogic>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void GetSextantOfPointForCell_ReturnsDirectionWithClosestMidpointToPosition() {
            var cell = BuildCell(new Vector3(10f, 0f, 10f));

            MockRenderConfig.Setup(config => config.GetEdgeMidpoint(HexDirection.NE)).Returns(new Vector3(-10f, 0f, -10f));
            MockRenderConfig.Setup(config => config.GetEdgeMidpoint(HexDirection.E)) .Returns(new Vector3(0f,   0f, 0f  ));
            MockRenderConfig.Setup(config => config.GetEdgeMidpoint(HexDirection.SE)).Returns(new Vector3(10f,  0f, 10f ));
            MockRenderConfig.Setup(config => config.GetEdgeMidpoint(HexDirection.SW)).Returns(new Vector3(20f,  0f, 20f ));
            MockRenderConfig.Setup(config => config.GetEdgeMidpoint(HexDirection.W)) .Returns(new Vector3(30f,  0f, 30f ));
            MockRenderConfig.Setup(config => config.GetEdgeMidpoint(HexDirection.NW)).Returns(new Vector3(40f,  0f, 40f ));

            var point = new Vector3(27f, 0f, 27f);

            var orientationLogic = Container.Resolve<PointOrientationLogic>();
            
            Assert.AreEqual(HexDirection.SW, orientationLogic.GetSextantOfPointForCell(point, cell));
        }

        [Test]
        public void GetOrientationOfPointInCell_AndPointInSolidTriangle_ReturnsCenter() {
            var cell = BuildCell(new Vector3(1f, 10f, 100f));

            MockRenderConfig.Setup(config => config.GetFirstSolidCorner (HexDirection.E)).Returns(new Vector3(2f, 20f, 200f));
            MockRenderConfig.Setup(config => config.GetSecondSolidCorner(HexDirection.E)).Returns(new Vector3(3f, 30f, 300f));

            var point = new Vector3(5f, 5.1f, 5.2f);

            MockGeometry2D.Setup(geometry => geometry.IsPointWithinTriangle(
                new Vector2(5f, 5.2f), new Vector2(1f, 100f), new Vector2(3f, 300f), new Vector2(4f, 400f)
            )).Returns(true);

            var orientationLogic = Container.Resolve<PointOrientationLogic>();

            Assert.AreEqual(PointOrientation.Center, orientationLogic.GetOrientationOfPointInCell(point, cell, HexDirection.E));
        }

        [Test]
        public void GetOrientationOfPointInCell_AndPointInFirstEdgeTriangle_ReturnsEdge() {
            var center = BuildCell(new Vector3(1f,   10f, 100f));
            var right  = BuildCell(new Vector3(0.5f, 0f,  0.5f));

            MockGrid.Setup(grid => grid.HasNeighbor(center, HexDirection.E)).Returns(true);
            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.E)).Returns(right);

            MockRenderConfig.Setup(config => config.GetFirstSolidCorner (HexDirection.E)).Returns(new Vector3(2f, 20f, 200f));
            MockRenderConfig.Setup(config => config.GetSecondSolidCorner(HexDirection.E)).Returns(new Vector3(3f, 30f, 300f));

            MockRenderConfig.Setup(config => config.GetFirstSolidCorner (HexDirection.W)).Returns(new Vector3(4.5f, 0f, 40.5f));
            MockRenderConfig.Setup(config => config.GetSecondSolidCorner(HexDirection.W)).Returns(new Vector3(5.5f, 0f, 50.5f));

            var point = new Vector3(5f, 5.1f, 5.2f);

            MockGeometry2D.Setup(geometry => geometry.IsPointWithinTriangle(
                new Vector2(5f, 5.2f), new Vector2(4f, 400f), new Vector2(3f, 300f),
                new Vector2((3f + 6f) / 2f, (300f + 51f) / 2f)
            )).Returns(true);

            var orientationLogic = Container.Resolve<PointOrientationLogic>();

            Assert.AreEqual(PointOrientation.Edge, orientationLogic.GetOrientationOfPointInCell(point, center, HexDirection.E));
        }

        [Test]
        public void GetOrientationOfPointInCell_AndPointInSecondEdgeTriangle_ReturnsEdge() {
            var center = BuildCell(new Vector3(1f,   10f, 100f));
            var right  = BuildCell(new Vector3(0.5f, 0f,  0.5f));

            MockGrid.Setup(grid => grid.HasNeighbor(center, HexDirection.E)).Returns(true);
            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.E)).Returns(right);

            MockRenderConfig.Setup(config => config.GetFirstSolidCorner (HexDirection.E)).Returns(new Vector3(2f, 20f, 200f));
            MockRenderConfig.Setup(config => config.GetSecondSolidCorner(HexDirection.E)).Returns(new Vector3(3f, 30f, 300f));

            MockRenderConfig.Setup(config => config.GetFirstSolidCorner (HexDirection.W)).Returns(new Vector3(4.5f, 0f, 40.5f));
            MockRenderConfig.Setup(config => config.GetSecondSolidCorner(HexDirection.W)).Returns(new Vector3(5.5f, 0f, 50.5f));

            var point = new Vector3(5f, 5.1f, 5.2f);

            MockGeometry2D.Setup(geometry => geometry.IsPointWithinTriangle(
                new Vector2(5f, 5.2f), new Vector2(4f, 400f),
                new Vector2((3f + 6f) / 2f, (300f + 51f) / 2f),
                new Vector2((4f + 5f) / 2f, (400f + 41f) / 2f)
            )).Returns(true);

            var orientationLogic = Container.Resolve<PointOrientationLogic>();

            Assert.AreEqual(PointOrientation.Edge, orientationLogic.GetOrientationOfPointInCell(point, center, HexDirection.E));
        }

        [Test]
        public void GetOrientationOfPointInCell_AndPointInPreviousCornerTriangle_ReturnsPreviousCorner() {
            var center = BuildCell(new Vector3(1f, 10f, 100f));
            var left   = BuildCell(new Vector3(2f, 2f,  2f));
            var right  = BuildCell(new Vector3(3f, 3f,  3f));
            
            MockGrid.Setup(grid => grid.HasNeighbor(center, HexDirection.NE)).Returns(true);
            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.NE)).Returns(left);

            MockGrid.Setup(grid => grid.HasNeighbor(center, HexDirection.E)).Returns(true);
            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.E)).Returns(right);

            MockRenderConfig.Setup(config => config.GetFirstSolidCorner(HexDirection.E)).Returns(new Vector3(2f, 20f, 200f));
            MockRenderConfig.Setup(config => config.GetFirstCorner          (HexDirection.E)).Returns(new Vector3(4f, 40f, 400f));

            MockRenderConfig.Setup(config => config.GetSecondSolidCorner(HexDirection.W)).Returns(new Vector3(1f, 2f, 3f));

            var point = new Vector3(5f, 5.1f, 5.2f);

            MockGeometry2D.Setup(geometry => geometry.IsPointWithinTriangle(
                new Vector2(5f, 5.2f), new Vector2(3f, 300f), new Vector2(5f, 500f),
                new Vector2((3f + 4f) / 2f, (300f + 6f) / 2f)
            )).Returns(true);

            var orientationLogic = Container.Resolve<PointOrientationLogic>();

            Assert.AreEqual(PointOrientation.PreviousCorner, orientationLogic.GetOrientationOfPointInCell(point, center, HexDirection.E));
        }

        [Test]
        public void GetOrientationOfPointInCell_AndPointInNextCornerTriangle_ReturnsNextCorner() {
            var center    = BuildCell(new Vector3(1f, 10f, 100f));
            var right     = BuildCell(new Vector3(3f, 3f,  3f));
            var nextRight = BuildCell(new Vector3(4f, 4f,  4f));
            
            MockGrid.Setup(grid => grid.HasNeighbor(center, HexDirection.E)).Returns(true);
            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.E)).Returns(right);

            MockGrid.Setup(grid => grid.HasNeighbor(center, HexDirection.SE)).Returns(true);
            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.SE)).Returns(nextRight);

            MockRenderConfig.Setup(config => config.GetSecondSolidCorner(HexDirection.E)).Returns(new Vector3(2f, 20f, 200f));
            MockRenderConfig.Setup(config => config.GetSecondCorner          (HexDirection.E)).Returns(new Vector3(4f, 40f, 400f));

            MockRenderConfig.Setup(config => config.GetFirstSolidCorner(HexDirection.W)).Returns(new Vector3(1f, 2f, 3f));

            var point = new Vector3(5f, 5.1f, 5.2f);

            MockGeometry2D.Setup(geometry => geometry.IsPointWithinTriangle(
                new Vector2(5f, 5.2f), new Vector2(3f, 300f),
                new Vector2((3f + 4f) / 2f, (300f + 6f) / 2f), new Vector2(5f, 500f)
            )).Returns(true);

            var orientationLogic = Container.Resolve<PointOrientationLogic>();

            Assert.AreEqual(PointOrientation.NextCorner, orientationLogic.GetOrientationOfPointInCell(point, center, HexDirection.E));
        }

        [Test]
        public void GetOrientationOfPointInCell_AndPointWouldBeInEdge_ButNoRightCell_ReturnsVoid() {
            var center = BuildCell(new Vector3(1f,   10f, 100f));
            var right  = BuildCell(new Vector3(0.5f, 0f,  0.5f));

            MockGrid.Setup(grid => grid.HasNeighbor(center, HexDirection.E)).Returns(false);
            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.E)).Returns(right);

            MockRenderConfig.Setup(config => config.GetFirstSolidCorner (HexDirection.E)).Returns(new Vector3(2f, 20f, 200f));
            MockRenderConfig.Setup(config => config.GetSecondSolidCorner(HexDirection.E)).Returns(new Vector3(3f, 30f, 300f));

            MockRenderConfig.Setup(config => config.GetFirstSolidCorner (HexDirection.W)).Returns(new Vector3(4.5f, 0f, 40.5f));
            MockRenderConfig.Setup(config => config.GetSecondSolidCorner(HexDirection.W)).Returns(new Vector3(5.5f, 0f, 50.5f));

            var point = new Vector3(5f, 5.1f, 5.2f);

            MockGeometry2D.Setup(geometry => geometry.IsPointWithinTriangle(
                new Vector2(5f, 5.2f), new Vector2(4f, 400f), new Vector2(3f, 300f),
                new Vector2((3f + 6f) / 2f, (300f + 51f) / 2f)
            )).Returns(true);

            var orientationLogic = Container.Resolve<PointOrientationLogic>();

            Assert.AreEqual(PointOrientation.Void, orientationLogic.GetOrientationOfPointInCell(point, center, HexDirection.E));
        }

        [Test]
        public void GetOrientationOfPointInCell_AndPointWouldBeInPreviousCorner_ButNoRightCell_ReturnsVoid() {
            var center = BuildCell(new Vector3(1f, 10f, 100f));
            var left   = BuildCell(new Vector3(2f, 2f,  2f));
            var right  = BuildCell(new Vector3(3f, 3f,  3f));
            
            MockGrid.Setup(grid => grid.HasNeighbor(center, HexDirection.NE)).Returns(true);
            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.NE)).Returns(left);

            MockGrid.Setup(grid => grid.HasNeighbor(center, HexDirection.E)).Returns(false);
            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.E)).Returns(right);

            MockRenderConfig.Setup(config => config.GetFirstSolidCorner(HexDirection.E)).Returns(new Vector3(2f, 20f, 200f));
            MockRenderConfig.Setup(config => config.GetFirstCorner          (HexDirection.E)).Returns(new Vector3(4f, 40f, 400f));

            MockRenderConfig.Setup(config => config.GetSecondSolidCorner(HexDirection.W)).Returns(new Vector3(1f, 2f, 3f));

            var point = new Vector3(5f, 5.1f, 5.2f);

            MockGeometry2D.Setup(geometry => geometry.IsPointWithinTriangle(
                new Vector2(5f, 5.2f), new Vector2(3f, 300f), new Vector2(5f, 500f),
                new Vector2((3f + 4f) / 2f, (300f + 6f) / 2f)
            )).Returns(true);

            var orientationLogic = Container.Resolve<PointOrientationLogic>();

            Assert.AreEqual(PointOrientation.Void, orientationLogic.GetOrientationOfPointInCell(point, center, HexDirection.E));
        }

        [Test]
        public void GetOrientationOfPointInCell_AndPointWouldBeInPreviousCorner_ButNoLeftCell_ReturnsVoid() {
            var center = BuildCell(new Vector3(1f, 10f, 100f));
            var left   = BuildCell(new Vector3(2f, 2f,  2f));
            var right  = BuildCell(new Vector3(3f, 3f,  3f));
            
            MockGrid.Setup(grid => grid.HasNeighbor(center, HexDirection.NE)).Returns(false);
            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.NE)).Returns(left);

            MockGrid.Setup(grid => grid.HasNeighbor(center, HexDirection.E)).Returns(true);
            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.E)).Returns(right);

            MockRenderConfig.Setup(config => config.GetFirstSolidCorner(HexDirection.E)).Returns(new Vector3(2f, 20f, 200f));
            MockRenderConfig.Setup(config => config.GetFirstCorner          (HexDirection.E)).Returns(new Vector3(4f, 40f, 400f));

            MockRenderConfig.Setup(config => config.GetSecondSolidCorner(HexDirection.W)).Returns(new Vector3(1f, 2f, 3f));

            var point = new Vector3(5f, 5.1f, 5.2f);

            MockGeometry2D.Setup(geometry => geometry.IsPointWithinTriangle(
                new Vector2(5f, 5.2f), new Vector2(3f, 300f), new Vector2(5f, 500f),
                new Vector2((3f + 4f) / 2f, (300f + 6f) / 2f)
            )).Returns(true);

            var orientationLogic = Container.Resolve<PointOrientationLogic>();

            Assert.AreEqual(PointOrientation.Void, orientationLogic.GetOrientationOfPointInCell(point, center, HexDirection.E));
        }

        [Test]
        public void GetOrientationOfPointInCell_AndPointWouldBeInNextCorner_ButNoRightCell_ReturnsVoid() {
            var center    = BuildCell(new Vector3(1f, 10f, 100f));
            var right     = BuildCell(new Vector3(3f, 3f,  3f));
            var nextRight = BuildCell(new Vector3(4f, 4f,  4f));
            
            MockGrid.Setup(grid => grid.HasNeighbor(center, HexDirection.E)).Returns(false);
            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.E)).Returns(right);

            MockGrid.Setup(grid => grid.HasNeighbor(center, HexDirection.SE)).Returns(true);
            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.SE)).Returns(nextRight);

            MockRenderConfig.Setup(config => config.GetSecondSolidCorner(HexDirection.E)).Returns(new Vector3(2f, 20f, 200f));
            MockRenderConfig.Setup(config => config.GetSecondCorner          (HexDirection.E)).Returns(new Vector3(4f, 40f, 400f));

            MockRenderConfig.Setup(config => config.GetFirstSolidCorner(HexDirection.W)).Returns(new Vector3(1f, 2f, 3f));

            var point = new Vector3(5f, 5.1f, 5.2f);

            MockGeometry2D.Setup(geometry => geometry.IsPointWithinTriangle(
                new Vector2(5f, 5.2f), new Vector2(3f, 300f),
                new Vector2((3f + 4f) / 2f, (300f + 6f) / 2f), new Vector2(5f, 500f)
            )).Returns(true);

            var orientationLogic = Container.Resolve<PointOrientationLogic>();

            Assert.AreEqual(PointOrientation.Void, orientationLogic.GetOrientationOfPointInCell(point, center, HexDirection.E));
        }

        [Test]
        public void GetOrientationOfPointInCell_AndPointWouldBeInNextCorner_ButNoNextRightCell_ReturnsVoid() {
            var center    = BuildCell(new Vector3(1f, 10f, 100f));
            var right     = BuildCell(new Vector3(3f, 3f,  3f));
            var nextRight = BuildCell(new Vector3(4f, 4f,  4f));
            
            MockGrid.Setup(grid => grid.HasNeighbor(center, HexDirection.E)).Returns(true);
            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.E)).Returns(right);

            MockGrid.Setup(grid => grid.HasNeighbor(center, HexDirection.SE)).Returns(false);
            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.SE)).Returns(nextRight);

            MockRenderConfig.Setup(config => config.GetSecondSolidCorner(HexDirection.E)).Returns(new Vector3(2f, 20f, 200f));
            MockRenderConfig.Setup(config => config.GetSecondCorner          (HexDirection.E)).Returns(new Vector3(4f, 40f, 400f));

            MockRenderConfig.Setup(config => config.GetFirstSolidCorner(HexDirection.W)).Returns(new Vector3(1f, 2f, 3f));

            var point = new Vector3(5f, 5.1f, 5.2f);

            MockGeometry2D.Setup(geometry => geometry.IsPointWithinTriangle(
                new Vector2(5f, 5.2f), new Vector2(3f, 300f),
                new Vector2((3f + 4f) / 2f, (300f + 6f) / 2f), new Vector2(5f, 500f)
            )).Returns(true);

            var orientationLogic = Container.Resolve<PointOrientationLogic>();

            Assert.AreEqual(PointOrientation.Void, orientationLogic.GetOrientationOfPointInCell(point, center, HexDirection.E));
        }

        #endregion

        #region utilities

        private IHexCell BuildCell(Vector3 absolutePosition) {
            var mockCell = new Mock<IHexCell>();

            mockCell.Setup(cell => cell.AbsolutePosition).Returns(absolutePosition);

            return mockCell.Object;
        }

        #endregion

        #endregion

    }

}
