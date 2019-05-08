﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using NUnit.Framework;
using Zenject;
using Moq;

using Assets.Simulation.HexMap;
using Assets.Simulation.Improvements;
using Assets.Simulation.MapRendering;

namespace Assets.Tests.Simulation.MapRendering {

    public class CellAlphamapLogicTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IMapRenderConfig>          MockRenderConfig;
        private Mock<IImprovementLocationCanon> MockImprovementLocationCanon;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockRenderConfig = new Mock<IMapRenderConfig>();
            MockImprovementLocationCanon = new Mock<IImprovementLocationCanon>();

            Container.Bind<IMapRenderConfig>         ().FromInstance(MockRenderConfig            .Object);
            Container.Bind<IImprovementLocationCanon>().FromInstance(MockImprovementLocationCanon.Object);

            Container.Bind<CellAlphamapLogic>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void BrokenTests() {
            throw new NotImplementedException();
        }

        /*[Test]
        public void GetAlphamapForCell_ReturnedArrayHasEntriesForEveryMapTexture() {
            var cell = BuildCell(CellTerrain.Grassland, CellShape.Flatlands);

            MockRenderConfig.Setup(config => config.MapTextures).Returns(new List<Texture2D>() { null, null, null, null, null });

            var alphamapLogic = Container.Resolve<CellAlphamapLogic>();

            Assert.AreEqual(5, alphamapLogic.GetAlphamapForCell(cell, HexDirection.E).Length);
        }

        [Test]
        public void GetAlphamapForCell_SetsOnlyIndexOfTerrainToOneByDefault() {
            var cell = BuildCell(CellTerrain.Desert, CellShape.Flatlands);

            MockRenderConfig.Setup(config => config.MapTextures).Returns(new List<Texture2D>() { null, null, null, null, null });

            var alphamapLogic = Container.Resolve<CellAlphamapLogic>();

            CollectionAssert.AreEqual(
                new float[] { 0f, 0f, 1f, 0f, 0f }, alphamapLogic.GetAlphamapForCell(cell, HexDirection.E)
            );
        }

        [Test]
        public void GetAlphamapForCell_AndCellIsWater_SetsOnlySeaFloorIndexToOne() {
            var cell = BuildCell(CellTerrain.DeepWater, CellShape.Flatlands);

            MockRenderConfig.Setup(config => config.MapTextures)         .Returns(new List<Texture2D>() { null, null, null, null, null });
            MockRenderConfig.Setup(config => config.SeaFloorTextureIndex).Returns(1);

            var alphamapLogic = Container.Resolve<CellAlphamapLogic>();

            CollectionAssert.AreEqual(
                new float[] { 0f, 1f, 0f, 0f, 0f }, alphamapLogic.GetAlphamapForCell(cell, HexDirection.E)
            );
        }

        [Test]
        public void GetAlphamapForCell_WaterOverridesMountains() {
            var cell = BuildCell(CellTerrain.DeepWater, CellShape.Mountains);

            MockRenderConfig.Setup(config => config.MapTextures)         .Returns(new List<Texture2D>() { null, null, null, null, null });
            MockRenderConfig.Setup(config => config.SeaFloorTextureIndex).Returns(1);

            var alphamapLogic = Container.Resolve<CellAlphamapLogic>();

            CollectionAssert.AreEqual(
                new float[] { 0f, 1f, 0f, 0f, 0f }, alphamapLogic.GetAlphamapForCell(cell, HexDirection.E)
            );
        }

        [Test]
        public void GetAlphamapForCell_WaterOverridesImprovements() {
            var cell = BuildCell(CellTerrain.DeepWater, CellShape.Flatlands, BuildImprovement(true, 4));

            MockRenderConfig.Setup(config => config.MapTextures)         .Returns(new List<Texture2D>() { null, null, null, null, null });
            MockRenderConfig.Setup(config => config.SeaFloorTextureIndex).Returns(1);

            var alphamapLogic = Container.Resolve<CellAlphamapLogic>();

            CollectionAssert.AreEqual(
                new float[] { 0f, 1f, 0f, 0f, 0f }, alphamapLogic.GetAlphamapForCell(cell, HexDirection.E)
            );
        }

        [Test]
        public void GetAlphamapForCell_AndCellIsMountains_SetsOnlyMountainsIndexToOne() {
            var cell = BuildCell(CellTerrain.Grassland, CellShape.Mountains);

            MockRenderConfig.Setup(config => config.MapTextures)         .Returns(new List<Texture2D>() { null, null, null, null, null });
            MockRenderConfig.Setup(config => config.MountainTextureIndex).Returns(3);

            var alphamapLogic = Container.Resolve<CellAlphamapLogic>();

            CollectionAssert.AreEqual(
                new float[] { 0f, 0f, 0f, 1f, 0f }, alphamapLogic.GetAlphamapForCell(cell, HexDirection.E)
            );
        }

        [Test]
        public void GetAlphamapForCell_MountainsOverrideImprovements() {
            var cell = BuildCell(CellTerrain.Grassland, CellShape.Mountains, BuildImprovement(true, 1));

            MockRenderConfig.Setup(config => config.MapTextures)         .Returns(new List<Texture2D>() { null, null, null, null, null });
            MockRenderConfig.Setup(config => config.MountainTextureIndex).Returns(3);

            var alphamapLogic = Container.Resolve<CellAlphamapLogic>();

            CollectionAssert.AreEqual(
                new float[] { 0f, 0f, 0f, 1f, 0f }, alphamapLogic.GetAlphamapForCell(cell, HexDirection.E)
            );
        }

        [Test]
        public void GetAlphamapForCell_AndCellHasOverridingImprovement_SetsOnlyFirstOverrideToOne() {
            var cell = BuildCell(
                CellTerrain.Grassland, CellShape.Flatlands, BuildImprovement(false, 2),
                BuildImprovement(true, 1), BuildImprovement(true, 3)
            );

            MockRenderConfig.Setup(config => config.MapTextures).Returns(new List<Texture2D>() { null, null, null, null, null });

            var alphamapLogic = Container.Resolve<CellAlphamapLogic>();

            CollectionAssert.AreEqual(
                new float[] { 0f, 1f, 0f, 0f, 0f }, alphamapLogic.GetAlphamapForCell(cell, HexDirection.E)
            );
        }*/

        #endregion

        #region utilities

        private IImprovement BuildImprovement(bool overridesTerrain, int overridingTerrainIndex) {
            var mockTemplate = new Mock<IImprovementTemplate>();

            mockTemplate.Setup(template => template.OverridesTerrain)      .Returns(overridesTerrain);
            mockTemplate.Setup(template => template.OverridingTerrainIndex).Returns(overridingTerrainIndex);

            var mockImprovement = new Mock<IImprovement>();

            mockImprovement.Setup(improvement => improvement.Template).Returns(mockTemplate.Object);

            return mockImprovement.Object;
        }

        private IHexCell BuildCell(CellTerrain terrain, CellShape shape, params IImprovement[] improvements) {
            var mockCell = new Mock<IHexCell>();

            mockCell.Setup(cell => cell.Terrain).Returns(terrain);
            mockCell.Setup(cell => cell.Shape)  .Returns(shape);

            var newCell = mockCell.Object;

            MockImprovementLocationCanon.Setup(canon => canon.GetPossessionsOfOwner(newCell)).Returns(improvements);

            return newCell;
        }

        #endregion

        #endregion

    }

}
