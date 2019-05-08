using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using Zenject;
using Moq;

using UnityEngine;

using Assets.Simulation.HexMap;
using Assets.Simulation.MapRendering;

namespace Assets.Tests.Simulation.MapRendering {

    public class PointOrientationLogicTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IHexGrid>              MockGrid;
        private Mock<ICellEdgeContourCanon> MockCellContourCanon;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockGrid             = new Mock<IHexGrid>();
            MockCellContourCanon = new Mock<ICellEdgeContourCanon>();

            Container.Bind<IHexGrid>             ().FromInstance(MockGrid            .Object);
            Container.Bind<ICellEdgeContourCanon>().FromInstance(MockCellContourCanon.Object);

            Container.Bind<PointOrientationLogic>().AsSingle();
        }

        #endregion

        #region tests

        #region GetCellAtPoint

        [Test]
        public void GetCellAtPoint_AndHasNoCellAtPoint_ReturnsNull() {
            var point = new Vector3(1f, 2f, 3f);

            MockGrid.Setup(grid => grid.HasCellAtLocation(point)).Returns(false);

            var orientationLogic = Container.Resolve<PointOrientationLogic>();

            Assert.IsNull(orientationLogic.GetCellAtPoint(point));
        }

        [Test(Description = "The grid cell here is where the point would fall if all the cells were regular hexagons")]
        public void GetCellAtPoint_CellHasPoint_AndIsWithinContourOfGridCell_ReturnsCell() {
            var point = new Vector3(1f, 2f, 3f);

            var cell = BuildCell();

            MockGrid.Setup(grid => grid.HasCellAtLocation(point)).Returns(true);
            MockGrid.Setup(grid => grid.GetCellAtLocation(point)).Returns(cell);

            MockCellContourCanon.Setup(canon => canon.IsPointWithinContour(new Vector2(1f, 3f), cell, HexDirection.SW)).Returns(true);

            var orientationLogic = Container.Resolve<PointOrientationLogic>();

            Assert.AreEqual(cell, orientationLogic.GetCellAtPoint(point));
        }

        [Test]
        public void GetCellAtPoint_CellHasPoint_AndIsWithinContourOfNeighbor_ReturnsNeighbor() {
            var pointXYZ = new Vector3(1f, 2f, 3f);
            var pointXZ = new Vector2(1f, 3f); 

            var cell = BuildCell();

            MockGrid.Setup(grid => grid.HasCellAtLocation(pointXYZ)).Returns(true);
            MockGrid.Setup(grid => grid.GetCellAtLocation(pointXYZ)).Returns(cell);

            var neighborOne   = BuildCell();
            var neighborTwo   = BuildCell();
            var neighborThree = BuildCell();

            MockCellContourCanon.Setup(canon => canon.IsPointWithinContour(pointXZ, neighborTwo, HexDirection.SE)).Returns(true);
            MockCellContourCanon.Setup(canon => canon.IsPointWithinContour(pointXZ, neighborTwo, HexDirection.NW)).Returns(true);

            MockCellContourCanon.Setup(canon => canon.IsPointWithinContour(pointXZ, neighborThree, HexDirection.E)).Returns(true);

            var neighbors = new List<IHexCell>() { neighborOne, neighborTwo, neighborThree };

            MockGrid.Setup(grid => grid.GetNeighbors(cell)).Returns(neighbors);

            var orientationLogic = Container.Resolve<PointOrientationLogic>();

            Assert.AreEqual(neighborTwo, orientationLogic.GetCellAtPoint(pointXYZ));
        }

        [Test]
        public void GetCellAtPoint_CellhasPoint_ButNotWithinAnyNearbyContour_ReturnsNull() {
            var pointXYZ = new Vector3(1f, 2f, 3f);
            var pointXZ = new Vector2(1f, 3f); 

            var cell = BuildCell();

            MockGrid.Setup(grid => grid.HasCellAtLocation(pointXYZ)).Returns(true);
            MockGrid.Setup(grid => grid.GetCellAtLocation(pointXYZ)).Returns(cell);

            var neighborOne   = BuildCell();
            var neighborTwo   = BuildCell();
            var neighborThree = BuildCell();

            var neighbors = new List<IHexCell>() { neighborOne, neighborTwo, neighborThree };

            MockGrid.Setup(grid => grid.GetNeighbors(cell)).Returns(neighbors);

            var orientationLogic = Container.Resolve<PointOrientationLogic>();

            Assert.IsNull(orientationLogic.GetCellAtPoint(pointXYZ));
        }

        #endregion

        #region GetOrientationDataFromTextures

        [Test]
        public void BrokenTests() {
            throw new NotImplementedException();
        }

        /*[Test]
        public void GetOrientationDataFromColors_TakesDuckFromDuckColorR() {
            var cells = new List<IHexCell>() {
                BuildCell(), BuildCell(), BuildCell(), BuildCell()
            };

            MockGrid.Setup(grid => grid.Cells).Returns(cells.AsReadOnly());

            var orientationColor = new Color32(3, 0, 0, 0);
            var weightsColor     = new Color();
            var duckColor        = new Color(0.5f, 0.6f, 0.7f, 0.8f);

            var orientationLogic = Container.Resolve<PointOrientationLogic>();

            var orientationData = orientationLogic.GetOrientationDataFromColors(orientationColor, weightsColor, duckColor);

            Assert.AreEqual(0.5f, orientationData.ElevationDuck);
        }

        [Test]
        public void GetOrientationDataFromColors_PullsCenterFromOrientationRG_MinusOne_AndCastAsAnIndex() {
            var cells = new List<IHexCell>() {
                BuildCell(), BuildCell(), BuildCell(), BuildCell()
            };

            MockGrid.Setup(grid => grid.Cells).Returns(cells.AsReadOnly());

            var orientationColor = new Color32(3, 0, 0, 0);
            var weightsColor     = new Color();

            var orientationLogic = Container.Resolve<PointOrientationLogic>();

            var orientationData = orientationLogic.GetOrientationDataFromColors(orientationColor, weightsColor, Color.clear);

            Assert.AreEqual(cells[2], orientationData.Center);
        }

        [Test]
        public void GetOrientationDataFromColors_AndRGIndexNegative_CenterSetToNull() {
            var cells = new List<IHexCell>() {
                BuildCell(), BuildCell(), BuildCell(), BuildCell()
            };

            MockGrid.Setup(grid => grid.Cells).Returns(cells.AsReadOnly());

            var orientationColor = new Color32(0, 0, 0, 0);
            var weightsColor     = new Color();

            var orientationLogic = Container.Resolve<PointOrientationLogic>();

            var orientationData = orientationLogic.GetOrientationDataFromColors(orientationColor, weightsColor, Color.clear);

            Assert.IsNull(orientationData.Center);
        }

        [Test]
        public void GetOrientationDataFromColors_AndRGIndexGreaterThanCellCount_CenterSetToNull() {
            var cells = new List<IHexCell>() {
                BuildCell(), BuildCell(), BuildCell(), BuildCell()
            };

            MockGrid.Setup(grid => grid.Cells).Returns(cells.AsReadOnly());

            var orientationColor = new Color32(10, 0, 0, 0);
            var weightsColor     = new Color();

            var orientationLogic = Container.Resolve<PointOrientationLogic>();

            var orientationData = orientationLogic.GetOrientationDataFromColors(orientationColor, weightsColor, Color.clear);

            Assert.IsNull(orientationData.Center);
        }

        [Test]
        public void GetOrientationDataFromColors_PullsSextantFromOrientationB_CastAsHexDirection() {
            var cells = new List<IHexCell>() { BuildCell() };

            MockGrid.Setup(grid => grid.Cells).Returns(cells.AsReadOnly());

            var orientationColor = new Color32(1, 0, 3, 0);
            var weightsColor     = new Color();

            var orientationLogic = Container.Resolve<PointOrientationLogic>();

            var orientationData = orientationLogic.GetOrientationDataFromColors(orientationColor, weightsColor, Color.clear);

            Assert.AreEqual(HexDirection.SW, orientationData.Sextant);
        }

        [Test]
        public void GetOrientationDataFromColors_AndCenterNull_ReturnedDataIsClear() {
            var cells = new List<IHexCell>() { BuildCell() };

            MockGrid.Setup(grid => grid.Cells).Returns(cells.AsReadOnly());

            var orientationColor = new Color32(0, 0, 3, 0);
            var weightsColor     = new Color(1f, 1f, 1f, 1f);

            var orientationLogic = Container.Resolve<PointOrientationLogic>();

            orientationLogic.GetOrientationDataFromColors(new Color32(1, 0, 3, 0), weightsColor, Color.clear);

            var orientationData = orientationLogic.GetOrientationDataFromColors(orientationColor, weightsColor, Color.clear);

            Assert.IsFalse(orientationData.IsOnGrid, "IsOnGrid has an unexpected value");

            Assert.AreEqual(HexDirection.NE, orientationData.Sextant, "Sextant has an unexpected value");

            Assert.IsNull(orientationData.Center,    "Center has an unexpected value");
            Assert.IsNull(orientationData.Left,      "Left has an unexpected value");
            Assert.IsNull(orientationData.Right,     "Right has an unexpected value");
            Assert.IsNull(orientationData.NextRight, "NextRight has an unexpected value");

            Assert.AreEqual(0f, orientationData.CenterWeight,    "CenterWeight has an unexpected value");
            Assert.AreEqual(0f, orientationData.LeftWeight,      "LeftWeight has an unexpected value");
            Assert.AreEqual(0f, orientationData.RightWeight,     "RightWeight has an unexpected value");
            Assert.AreEqual(0f, orientationData.NextRightWeight, "NextRightWeight has an unexpected value");

            Assert.AreEqual(0f, orientationData.RiverWeight, "RiverWeight has an unexpected value");
        }

        [Test]
        public void GetOrientationDataFromColors_AndCenterNotNull_IsOnGridIsTrue() {
            var cells = new List<IHexCell>() { BuildCell() };

            MockGrid.Setup(grid => grid.Cells).Returns(cells.AsReadOnly());

            var orientationColor = new Color32(1, 0, 3, 0);
            var weightsColor     = new Color();

            var orientationLogic = Container.Resolve<PointOrientationLogic>();

            var orientationData = orientationLogic.GetOrientationDataFromColors(orientationColor, weightsColor, Color.clear);

            Assert.IsTrue(orientationData.IsOnGrid);
        }

        [Test]
        public void GetOrientationDataFromColors_AndCenterNotNull_NeighborsAssignedFromSextant() {
            var center    = BuildCell();
            var left      = BuildCell();
            var right     = BuildCell();
            var nextRight = BuildCell();

            var cells = new List<IHexCell>() { center, left, right, nextRight };

            MockGrid.Setup(grid => grid.Cells).Returns(cells.AsReadOnly());

            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.NE)).Returns(left);
            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.E )).Returns(right);
            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.SE)).Returns(nextRight);

            var orientationColor = new Color32(1, 0, 1, 0);
            var weightsColor     = new Color();

            var orientationLogic = Container.Resolve<PointOrientationLogic>();

            var orientationData = orientationLogic.GetOrientationDataFromColors(orientationColor, weightsColor, Color.clear);

            Assert.AreEqual(left,      orientationData.Left,      "Left has an unexpected value");
            Assert.AreEqual(right,     orientationData.Right,     "Right has an unexpected value");
            Assert.AreEqual(nextRight, orientationData.NextRight, "NextRight has an unexpected value");
        }

        [Test]
        public void GetOrientationDataFromColors_AndCenterNotNull_CenterWeightPulledFromSampledR_OfWeightsTexture() {
            var cells = new List<IHexCell>() { BuildCell() };

            MockGrid.Setup(grid => grid.Cells).Returns(cells.AsReadOnly());

            var orientationColor = new Color32(1, 0, 3, 0);
            var weightsColor     = new Color(0.45f, 0.55f, 0.65f, 0.75f);

            var orientationLogic = Container.Resolve<PointOrientationLogic>();

            var orientationData = orientationLogic.GetOrientationDataFromColors(orientationColor, weightsColor, Color.clear);

            Assert.AreEqual(weightsColor.r, orientationData.CenterWeight);
        }

        [Test]
        public void GetOrientationDataFromColors_AndCenterNotNull_LeftWeightPulledFromSampledG_OfWeightsTexture() {
            var cells = new List<IHexCell>() { BuildCell() };

            MockGrid.Setup(grid => grid.Cells).Returns(cells.AsReadOnly());

            var orientationColor = new Color32(1, 0, 3, 0);
            var weightsColor     = new Color(0.45f, 0.55f, 0.65f, 0.75f);

            var orientationLogic = Container.Resolve<PointOrientationLogic>();

            var orientationData = orientationLogic.GetOrientationDataFromColors(orientationColor, weightsColor, Color.clear);

            Assert.AreEqual(weightsColor.g, orientationData.LeftWeight);
        }

        [Test]
        public void GetOrientationDataFromColors_AndCenterNotNull_RightWeightPulledFromSampledB_OfWeightsTexture() {
            var cells = new List<IHexCell>() { BuildCell() };

            MockGrid.Setup(grid => grid.Cells).Returns(cells.AsReadOnly());

            var orientationColor = new Color32(1, 0, 3, 0);
            var weightsColor     = new Color(0.45f, 0.55f, 0.65f, 0.75f);

            var orientationLogic = Container.Resolve<PointOrientationLogic>();

            var orientationData = orientationLogic.GetOrientationDataFromColors(orientationColor, weightsColor, Color.clear);

            Assert.AreEqual(weightsColor.b, orientationData.RightWeight);
        }

        [Test]
        public void GetOrientationDataFromColors_AndCenterNotNull_NextRightWeightPulledFromSampledA_OfWeightsTexture() {
            var cells = new List<IHexCell>() { BuildCell() };

            MockGrid.Setup(grid => grid.Cells).Returns(cells.AsReadOnly());

            var orientationColor = new Color32(1, 0, 3, 0);
            var weightsColor     = new Color(0.45f, 0.55f, 0.65f, 0.75f);

            var orientationLogic = Container.Resolve<PointOrientationLogic>();

            var orientationData = orientationLogic.GetOrientationDataFromColors(orientationColor, weightsColor, Color.clear);

            Assert.AreEqual(weightsColor.a, orientationData.NextRightWeight);
        }

        [Test]
        public void GetOrientationDataFromColors_AndCenterNotNull_RiverWeightOneMinusOtherWeights() {
            var cells = new List<IHexCell>() { BuildCell() };

            MockGrid.Setup(grid => grid.Cells).Returns(cells.AsReadOnly());

            var orientationColor = new Color32(1, 0, 3, 0);
            var weightsColor     = new Color(0.1f, 0.15f, 0.2f, 0.25f);

            var orientationLogic = Container.Resolve<PointOrientationLogic>();

            var orientationData = orientationLogic.GetOrientationDataFromColors(orientationColor, weightsColor, Color.clear);

            Assert.AreEqual(1f - 0.1f - 0.15f - 0.2f - 0.25f, orientationData.RiverWeight);
        }

        [Test]
        public void GetOrientationDataFromColors_AndCenterNotNull_RiverWeightFloorsAtZero() {
            var cells = new List<IHexCell>() { BuildCell() };

            MockGrid.Setup(grid => grid.Cells).Returns(cells.AsReadOnly());

            var orientationColor = new Color32(1, 0, 3, 0);
            var weightsColor     = new Color(1f, 1f, 1f, 1f);

            var orientationLogic = Container.Resolve<PointOrientationLogic>();

            var orientationData = orientationLogic.GetOrientationDataFromColors(orientationColor, weightsColor, Color.clear);

            Assert.AreEqual(0f, orientationData.RiverWeight);
        }

        [Test]
        public void GetOrientationDataFromColors_AndCenterNotNull_RiverWeightCeilingsAtOne() {
           var cells = new List<IHexCell>() { BuildCell() };

            MockGrid.Setup(grid => grid.Cells).Returns(cells.AsReadOnly());

            var orientationColor = new Color32(1, 0, 3, 0);
            var weightsColor     = new Color(-1f, -1f, -1f, -1f);

            var orientationLogic = Container.Resolve<PointOrientationLogic>();

            var orientationData = orientationLogic.GetOrientationDataFromColors(orientationColor, weightsColor, Color.clear);

            Assert.AreEqual(1f, orientationData.RiverWeight);
        }*/

        #endregion

        #endregion

        #region utilities

        private IHexCell BuildCell() {
            return new Mock<IHexCell>().Object;
        }

        #endregion

        #endregion

    }

}
