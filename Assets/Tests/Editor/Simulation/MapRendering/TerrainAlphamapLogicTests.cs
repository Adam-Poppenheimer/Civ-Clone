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

    public class TerrainAlphamapLogicTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IMapRenderConfig>         MockRenderConfig;
        private Mock<IHexGrid>                 MockGrid;
        private Mock<IPointOrientationLogic>   MockPointOrientationLogic;
        private Mock<ICellAlphamapLogic>       MockCellAlphamapLogic;
        private Mock<ITerrainMixingLogic>      MockTerrainMixingLogic;
        private Mock<INoiseGenerator>          MockNoiseGenerator;
        private Mock<IAlphamapMixingFunctions> MockAlphamapMixingFunctions;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockRenderConfig          = new Mock<IMapRenderConfig>();
            MockGrid                  = new Mock<IHexGrid>();
            MockPointOrientationLogic = new Mock<IPointOrientationLogic>();
            MockCellAlphamapLogic     = new Mock<ICellAlphamapLogic>();
            MockTerrainMixingLogic    = new Mock<ITerrainMixingLogic>();
            MockNoiseGenerator        = new Mock<INoiseGenerator>();
            MockAlphamapMixingFunctions = new Mock<IAlphamapMixingFunctions>();

            Container.Bind<IMapRenderConfig>        ().FromInstance(MockRenderConfig           .Object);
            Container.Bind<IHexGrid>                ().FromInstance(MockGrid                   .Object);
            Container.Bind<IPointOrientationLogic>  ().FromInstance(MockPointOrientationLogic  .Object);
            Container.Bind<ICellAlphamapLogic>      ().FromInstance(MockCellAlphamapLogic      .Object);
            Container.Bind<ITerrainMixingLogic>     ().FromInstance(MockTerrainMixingLogic     .Object);
            Container.Bind<INoiseGenerator>         ().FromInstance(MockNoiseGenerator         .Object);
            Container.Bind<IAlphamapMixingFunctions>().FromInstance(MockAlphamapMixingFunctions.Object);

            Container.Bind<TerrainAlphamapLogic>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void GetAlphamapForPosition_AndNoCellAtPerturbedPosition_ReturnsEmptyArrayOfCorrectLength() {
            var position          = new Vector3(1f, 2f,  3f);
            var perturbedPosition = new Vector3(1f, 20f, 300f);

            var cell = BuildCell();

            MockNoiseGenerator.Setup(generator => generator.Perturb(position)).Returns(perturbedPosition);

            MockGrid.Setup(grid => grid.HasCellAtLocation(perturbedPosition)).Returns(false);
            MockGrid.Setup(grid => grid.GetCellAtLocation(perturbedPosition)).Returns(cell);

            MockRenderConfig.Setup(config => config.MapTextures)
                            .Returns(new List<Texture2D>() { null, null, null, null, null });

            var terrainAlphamapLogic = Container.Resolve<TerrainAlphamapLogic>();

            CollectionAssert.AreEqual(new float[] { 0f, 0f, 0f, 0f, 0f }, terrainAlphamapLogic.GetAlphamapForPosition(position));
        }

        [Test]
        public void GetAlphamapForPosition_AndPerturbedPositionHasCenterOrientation_ReturnsAlphamapOfCell() {
            var position          = new Vector3(1f, 2f,  3f);
            var perturbedPosition = new Vector3(1f, 20f, 300f);

            var cell = BuildCell();

            MockPointOrientationLogic.Setup(
                logic => logic.GetSextantOfPointForCell(perturbedPosition, cell)
            ).Returns(HexDirection.W);

            MockPointOrientationLogic.Setup(
                logic => logic.GetOrientationOfPointInCell(perturbedPosition, cell, HexDirection.W)
            ).Returns(PointOrientation.Center);

            var alphamap = new float[] { 2f, 15f, 3.51f, -4.4f };

            MockCellAlphamapLogic.Setup(
                logic => logic.GetAlphamapForPositionForCell(perturbedPosition, cell, HexDirection.W)
            ).Returns(alphamap);

            MockNoiseGenerator.Setup(generator => generator.Perturb(position)).Returns(perturbedPosition);

            MockGrid.Setup(grid => grid.HasCellAtLocation(perturbedPosition)).Returns(true);
            MockGrid.Setup(grid => grid.GetCellAtLocation(perturbedPosition)).Returns(cell);

            var terrainAlphamapLogic = Container.Resolve<TerrainAlphamapLogic>();

            CollectionAssert.AreEqual(alphamap, terrainAlphamapLogic.GetAlphamapForPosition(position));
        }

        [Test]
        public void GetAlphamapForPosition_AndPerturbedPositionHasVoidOrientation_ReturnsAlphamapOfCell() {
            var position          = new Vector3(1f, 2f,  3f);
            var perturbedPosition = new Vector3(1f, 20f, 300f);

            var cell = BuildCell();

            MockPointOrientationLogic.Setup(
                logic => logic.GetSextantOfPointForCell(perturbedPosition, cell)
            ).Returns(HexDirection.W);

            MockPointOrientationLogic.Setup(
                logic => logic.GetOrientationOfPointInCell(perturbedPosition, cell, HexDirection.W)
            ).Returns(PointOrientation.Void);

            var alphamap = new float[] { 2f, 15f, 3.51f, -4.4f };

            MockCellAlphamapLogic.Setup(
                logic => logic.GetAlphamapForPositionForCell(perturbedPosition, cell, HexDirection.W)
            ).Returns(alphamap);

            MockNoiseGenerator.Setup(generator => generator.Perturb(position)).Returns(perturbedPosition);

            MockGrid.Setup(grid => grid.HasCellAtLocation(perturbedPosition)).Returns(true);
            MockGrid.Setup(grid => grid.GetCellAtLocation(perturbedPosition)).Returns(cell);

            var terrainAlphamapLogic = Container.Resolve<TerrainAlphamapLogic>();

            CollectionAssert.AreEqual(alphamap, terrainAlphamapLogic.GetAlphamapForPosition(position));
        }

        [Test]
        public void GetAlphamapForPosition_AndPerturbedPositionHasEdgeOrientation_ReturnsEdgeMix() {
            var position          = new Vector3(1f, 2f,  3f);
            var perturbedPosition = new Vector3(1f, 20f, 300f);

            var center = BuildCell();
            var right  = BuildCell();

            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.W)).Returns(right);

            MockPointOrientationLogic.Setup(
                logic => logic.GetSextantOfPointForCell(perturbedPosition, center)
            ).Returns(HexDirection.W);

            MockPointOrientationLogic.Setup(
                logic => logic.GetOrientationOfPointInCell(perturbedPosition, center, HexDirection.W)
            ).Returns(PointOrientation.Edge);

            var alphamap = new float[] { 2f, 15f, 3.51f, -4.4f };

            MockTerrainMixingLogic.Setup(
                logic => logic.GetMixForEdgeAtPoint(
                    center, right, HexDirection.W, perturbedPosition,
                    It.IsAny<DataSelectorCallback<float[]>>(),
                    It.IsAny<Func<float[], float[], float[]>>()
                )
            ).Returns(alphamap);

            MockNoiseGenerator.Setup(generator => generator.Perturb(position)).Returns(perturbedPosition);

            MockGrid.Setup(grid => grid.HasCellAtLocation(perturbedPosition)).Returns(true);
            MockGrid.Setup(grid => grid.GetCellAtLocation(perturbedPosition)).Returns(center);

            var terrainAlphamapLogic = Container.Resolve<TerrainAlphamapLogic>();

            CollectionAssert.AreEqual(alphamap, terrainAlphamapLogic.GetAlphamapForPosition(position));
        }

        [Test]
        public void GetAlphamapForPosition_AndPerturbedPositionHasPreviousCornerOrientation_ReturnsPreviousCornerMix() {
            var position          = new Vector3(1f, 2f,  3f);
            var perturbedPosition = new Vector3(1f, 20f, 300f);

            var center = BuildCell();
            var left   = BuildCell();
            var right  = BuildCell();

            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.SW)).Returns(left);
            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.W )).Returns(right);

            MockPointOrientationLogic.Setup(
                logic => logic.GetSextantOfPointForCell(perturbedPosition, center)
            ).Returns(HexDirection.W);

            MockPointOrientationLogic.Setup(
                logic => logic.GetOrientationOfPointInCell(perturbedPosition, center, HexDirection.W)
            ).Returns(PointOrientation.PreviousCorner);

            var alphamap = new float[] { 2f, 15f, 3.51f, -4.4f };

            MockTerrainMixingLogic.Setup(
                logic => logic.GetMixForPreviousCornerAtPoint(
                    center, left, right, HexDirection.W, perturbedPosition,
                    It.IsAny<DataSelectorCallback<float[]>>(),
                    It.IsAny<Func<float[], float[], float[]>>()
                )
            ).Returns(alphamap);

            MockNoiseGenerator.Setup(generator => generator.Perturb(position)).Returns(perturbedPosition);

            MockGrid.Setup(grid => grid.HasCellAtLocation(perturbedPosition)).Returns(true);
            MockGrid.Setup(grid => grid.GetCellAtLocation(perturbedPosition)).Returns(center);

            var terrainAlphamapLogic = Container.Resolve<TerrainAlphamapLogic>();

            CollectionAssert.AreEqual(alphamap, terrainAlphamapLogic.GetAlphamapForPosition(position));
        }

        [Test]
        public void GetAlphamapForPosition_AndPerturbedPositionHasNextCornerOrientation_ReturnsNextCornerMix() {
            var position          = new Vector3(1f, 2f,  3f);
            var perturbedPosition = new Vector3(1f, 20f, 300f);

            var center    = BuildCell();
            var right     = BuildCell();
            var nextRight = BuildCell();

            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.W )).Returns(right);
            MockGrid.Setup(grid => grid.GetNeighbor(center, HexDirection.NW)).Returns(nextRight);

            MockPointOrientationLogic.Setup(
                logic => logic.GetSextantOfPointForCell(perturbedPosition, center)
            ).Returns(HexDirection.W);

            MockPointOrientationLogic.Setup(
                logic => logic.GetOrientationOfPointInCell(perturbedPosition, center, HexDirection.W)
            ).Returns(PointOrientation.NextCorner);

            var alphamap = new float[] { 2f, 15f, 3.51f, -4.4f };

            MockTerrainMixingLogic.Setup(
                logic => logic.GetMixForNextCornerAtPoint(
                    center, right, nextRight, HexDirection.W, perturbedPosition,
                    It.IsAny<DataSelectorCallback<float[]>>(),
                    It.IsAny<Func<float[], float[], float[]>>()
                )
            ).Returns(alphamap);

            MockNoiseGenerator.Setup(generator => generator.Perturb(position)).Returns(perturbedPosition);

            MockGrid.Setup(grid => grid.HasCellAtLocation(perturbedPosition)).Returns(true);
            MockGrid.Setup(grid => grid.GetCellAtLocation(perturbedPosition)).Returns(center);

            var terrainAlphamapLogic = Container.Resolve<TerrainAlphamapLogic>();

            CollectionAssert.AreEqual(alphamap, terrainAlphamapLogic.GetAlphamapForPosition(position));
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
