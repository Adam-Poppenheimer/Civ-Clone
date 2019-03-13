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

        private Mock<IHexGrid>                        MockGrid;
        private Mock<IPointOrientationInSextantLogic> MockPointOrientationInSextantLogic;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockGrid                           = new Mock<IHexGrid>();
            MockPointOrientationInSextantLogic = new Mock<IPointOrientationInSextantLogic>();

            Container.Bind<IHexGrid>                       ().FromInstance(MockGrid                          .Object);
            Container.Bind<IPointOrientationInSextantLogic>().FromInstance(MockPointOrientationInSextantLogic.Object);

            Container.Bind<PointOrientationLogic>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void GetOrientationDataForPoint_AndGridHasNoCellAtLocation_ReturnsEmptyData() {
            var point = new Vector2(1f, 2f);

            var orientationLogic = Container.Resolve<PointOrientationLogic>();

            Assert.AreEqual(new PointOrientationData(), orientationLogic.GetOrientationDataForPoint(point));
        }

        [Test]
        public void GetOrientationDataForPoint_AndCellAtLocation_ReturnsGridSextantData_IfGridSextantValid() {
            var point = new Vector2(1f, 2f);

            var gridCenter = BuildCell();

            MockGrid.Setup(grid => grid.HasCellAtLocation(new Vector3(1f, 0f, 2f))).Returns(true);
            MockGrid.Setup(grid => grid.GetCellAtLocation(new Vector3(1f, 0f, 2f))).Returns(gridCenter);

            HexDirection gridSextant = HexDirection.E;
            MockGrid.Setup(grid => grid.TryGetSextantOfPointInCell(point, gridCenter, out gridSextant)).Returns(true);

            PointOrientationData centerData = new PointOrientationData() {
                CenterWeight = 1f, LeftWeight = 2f, RightWeight = 3f, NextRightWeight = 4f
            };

            MockPointOrientationInSextantLogic.Setup(
                logic => logic.TryFindValidOrientation(point, gridCenter, gridSextant, out centerData)
            ).Returns(true);

            var orientationLogic = Container.Resolve<PointOrientationLogic>();

            Assert.AreEqual(centerData, orientationLogic.GetOrientationDataForPoint(point));
        }

        [Test]
        public void GetOrientationDataForPoint_AndCellAtLocation_ReturnsPreviousSextantData_IfPreviousSextantValid() {
            var point = new Vector2(1f, 2f);

            var gridCenter = BuildCell();

            MockGrid.Setup(grid => grid.HasCellAtLocation(new Vector3(1f, 0f, 2f))).Returns(true);
            MockGrid.Setup(grid => grid.GetCellAtLocation(new Vector3(1f, 0f, 2f))).Returns(gridCenter);

            HexDirection gridSextant = HexDirection.E;
            MockGrid.Setup(grid => grid.TryGetSextantOfPointInCell(point, gridCenter, out gridSextant)).Returns(true);

            PointOrientationData leftData = new PointOrientationData() {
                CenterWeight = 1f, LeftWeight = 2f, RightWeight = 3f, NextRightWeight = 4f
            };

            MockPointOrientationInSextantLogic.Setup(
                logic => logic.TryFindValidOrientation(point, gridCenter, gridSextant.Previous(), out leftData)
            ).Returns(true);

            var orientationLogic = Container.Resolve<PointOrientationLogic>();

            Assert.AreEqual(leftData, orientationLogic.GetOrientationDataForPoint(point));
        }

        [Test]
        public void GetOrientationDataForPoint_AndCellAtLocation_ReturnsNextSextantData_IfNextSextantValid() {
            var point = new Vector2(1f, 2f);

            var gridCenter = BuildCell();

            MockGrid.Setup(grid => grid.HasCellAtLocation(new Vector3(1f, 0f, 2f))).Returns(true);
            MockGrid.Setup(grid => grid.GetCellAtLocation(new Vector3(1f, 0f, 2f))).Returns(gridCenter);

            HexDirection gridSextant = HexDirection.E;
            MockGrid.Setup(grid => grid.TryGetSextantOfPointInCell(point, gridCenter, out gridSextant)).Returns(true);

            PointOrientationData nextRightData = new PointOrientationData() {
                CenterWeight = 1f, LeftWeight = 2f, RightWeight = 3f, NextRightWeight = 4f
            };

            MockPointOrientationInSextantLogic.Setup(
                logic => logic.TryFindValidOrientation(point, gridCenter, gridSextant.Next(), out nextRightData)
            ).Returns(true);

            var orientationLogic = Container.Resolve<PointOrientationLogic>();

            Assert.AreEqual(nextRightData, orientationLogic.GetOrientationDataForPoint(point));
        }

        [Test]
        public void GetOrientationDataForPoint_AndCellAtLocation_ReturnsEmptyData_IfNoSextantValid() {
            var point = new Vector2(1f, 2f);

            var gridCenter = BuildCell();

            MockGrid.Setup(grid => grid.HasCellAtLocation(new Vector3(1f, 0f, 2f))).Returns(true);
            MockGrid.Setup(grid => grid.GetCellAtLocation(new Vector3(1f, 0f, 2f))).Returns(gridCenter);

            HexDirection gridSextant = HexDirection.E;
            MockGrid.Setup(grid => grid.TryGetSextantOfPointInCell(point, gridCenter, out gridSextant)).Returns(true);

            var orientationLogic = Container.Resolve<PointOrientationLogic>();

            Assert.AreEqual(new PointOrientationData(), orientationLogic.GetOrientationDataForPoint(point));
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
