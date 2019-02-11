using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using NUnit.Framework;
using Zenject;
using Moq;

using Assets.Simulation.MapRendering;
using Assets.Simulation.HexMap;

namespace Assets.Tests.Simulation.MapRendering {

    public class TerrainHeightLogicTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IHexGrid>               MockGrid;
        private Mock<IPointOrientationLogic> MockPointOrientationLogic;
        private Mock<ICellHeightmapLogic>    MockCellHeightmapLogic;
        private Mock<IHeightMixingLogic>     MockHeightMixingLogic;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockGrid                  = new Mock<IHexGrid>();
            MockPointOrientationLogic = new Mock<IPointOrientationLogic>();
            MockCellHeightmapLogic    = new Mock<ICellHeightmapLogic>();
            MockHeightMixingLogic     = new Mock<IHeightMixingLogic>();

            Container.Bind<IHexGrid>              ().FromInstance(MockGrid                 .Object);
            Container.Bind<IPointOrientationLogic>().FromInstance(MockPointOrientationLogic.Object);
            Container.Bind<ICellHeightmapLogic>   ().FromInstance(MockCellHeightmapLogic   .Object);
            Container.Bind<IHeightMixingLogic>    ().FromInstance(MockHeightMixingLogic    .Object);

            Container.Bind<TerrainHeightLogic>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void GetHeightForPosition_AndNoCellAtLocation_ReturnsZero() {
            Vector3 position = new Vector3(1f, 2f, 3f);

            var heightLogic = Container.Resolve<TerrainHeightLogic>();

            Assert.AreEqual(0f, heightLogic.GetHeightForPosition(position));
        }

        [Test]
        public void GetHeightForPosition_CellAtLocation_AndCenterOrientation_GetsHeightFromCell() {
            Vector3 position = new Vector3(1f, 2f, 3f);

            var center = BuildCell(HexDirection.E, null, null, null);

            MockGrid.Setup(grid => grid.HasCellAtLocation(position)).Returns(true);
            MockGrid.Setup(grid => grid.GetCellAtLocation(position)).Returns(center);

            MockPointOrientationLogic.Setup(logic => logic.GetSextantOfPointForCell(position, center))
                                     .Returns(HexDirection.E);

            MockPointOrientationLogic.Setup(logic => logic.GetOrientationOfPointInCell(position, center, HexDirection.E))
                                     .Returns(PointOrientation.Center);

            MockCellHeightmapLogic.Setup(logic => logic.GetHeightForPositionForCell(position, center, HexDirection.E)).Returns(15.5f);

            var heightLogic = Container.Resolve<TerrainHeightLogic>();

            Assert.AreEqual(15.5f, heightLogic.GetHeightForPosition(position));
        }

        [Test]
        public void GetHeightForPosition_CellAtLocation_AndEdgeOrientation_GetsMixFromCellAndNeighbor() {
            Vector3 position = new Vector3(1f, 2f, 3f);

            var right = BuildCell();

            var center = BuildCell(HexDirection.E, null, right, null);

            MockGrid.Setup(grid => grid.HasCellAtLocation(position)).Returns(true);
            MockGrid.Setup(grid => grid.GetCellAtLocation(position)).Returns(center);

            MockPointOrientationLogic.Setup(logic => logic.GetSextantOfPointForCell(position, center))
                                     .Returns(HexDirection.E);

            MockPointOrientationLogic.Setup(logic => logic.GetOrientationOfPointInCell(position, center, HexDirection.E))
                                     .Returns(PointOrientation.Edge);

            MockHeightMixingLogic.Setup(logic => logic.GetMixForEdgeAtPoint(center, right, HexDirection.E, position)).Returns(15.5f);

            var heightLogic = Container.Resolve<TerrainHeightLogic>();

            Assert.AreEqual(15.5f, heightLogic.GetHeightForPosition(position));
        }

        [Test]
        public void GetHeightForPosition_CellAtLocation_AndPreviousCornerOrientation_GetsCornerMixWithPreviousNeighbor() {
            Vector3 position = new Vector3(1f, 2f, 3f);

            var left  = BuildCell();
            var right = BuildCell();

            var center = BuildCell(HexDirection.E, left, right, null);

            MockGrid.Setup(grid => grid.HasCellAtLocation(position)).Returns(true);
            MockGrid.Setup(grid => grid.GetCellAtLocation(position)).Returns(center);

            MockPointOrientationLogic.Setup(logic => logic.GetSextantOfPointForCell(position, center))
                                     .Returns(HexDirection.E);

            MockPointOrientationLogic.Setup(logic => logic.GetOrientationOfPointInCell(position, center, HexDirection.E))
                                     .Returns(PointOrientation.PreviousCorner);

            MockHeightMixingLogic.Setup(
                logic => logic.GetMixForPreviousCornerAtPoint(center, left, right, HexDirection.E, position)
            ).Returns(15.5f);

            var heightLogic = Container.Resolve<TerrainHeightLogic>();

            Assert.AreEqual(15.5f, heightLogic.GetHeightForPosition(position));
        }

        [Test]
        public void GetHeightForPosition_cellAtLocation_AndNextCornerOrientation_GetsCornerMixWithNextNeighbor() {
            Vector3 position = new Vector3(1f, 2f, 3f);

            var right      = BuildCell();
            var nextRight  = BuildCell();

            var center = BuildCell(HexDirection.E, null, right, nextRight);

            MockGrid.Setup(grid => grid.HasCellAtLocation(position)).Returns(true);
            MockGrid.Setup(grid => grid.GetCellAtLocation(position)).Returns(center);

            MockPointOrientationLogic.Setup(logic => logic.GetSextantOfPointForCell(position, center))
                                     .Returns(HexDirection.E);

            MockPointOrientationLogic.Setup(logic => logic.GetOrientationOfPointInCell(position, center, HexDirection.E))
                                     .Returns(PointOrientation.NextCorner);

            MockHeightMixingLogic.Setup(
                logic => logic.GetMixForNextCornerAtPoint(center, right, nextRight, HexDirection.E, position)
            ).Returns(15.5f);

            var heightLogic = Container.Resolve<TerrainHeightLogic>();

            Assert.AreEqual(15.5f, heightLogic.GetHeightForPosition(position));
        }

        #endregion

        #region utilities

        private IHexCell BuildCell() {
            return new Mock<IHexCell>().Object;
        }

        private IHexCell BuildCell(HexDirection neighborDirection, IHexCell left, IHexCell right, IHexCell nextRight) {
            var newCell = new Mock<IHexCell>().Object;

            MockGrid.Setup(grid => grid.GetNeighbor(newCell, neighborDirection.Previous())).Returns(left);
            MockGrid.Setup(grid => grid.GetNeighbor(newCell, neighborDirection           )).Returns(right);
            MockGrid.Setup(grid => grid.GetNeighbor(newCell, neighborDirection.Next    ())).Returns(nextRight);

            return newCell;
        }

        #endregion

        #endregion

    }

}
