using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation.MapResources;
using Assets.Simulation.HexMap;

namespace Assets.Tests.Simulation.MapResources {

    [TestFixture]
    public class ResourceRestrictionCanonTests : ZenjectUnitTestFixture {

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            Container.Bind<ResourceRestrictionCanon>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void GetPlacementWeightOnCell_IncludesWeightFromTerrain() {
            var mockResource = new Mock<IResourceDefinition>();

            mockResource.Setup(resource => resource.GetWeightFromTerrain(CellTerrain.Desert)).Returns(15);

            var cellToTest = BuildCell(terrain: CellTerrain.Desert);

            var restrictionCanon = Container.Resolve<ResourceRestrictionCanon>();

            int weight = restrictionCanon.GetPlacementWeightOnCell(mockResource.Object, cellToTest);

            Assert.AreEqual(15, weight);

            mockResource.Verify(
                resource => resource.GetWeightFromTerrain(CellTerrain.Desert), Times.Once,
                "resource.GetWeightFromTerrain wasn't called as expected"
            );
        }

        [Test]
        public void GetPlacementWeightOnCell_IncludesWeightFromShape() {
            var mockResource = new Mock<IResourceDefinition>();

            mockResource.Setup(resource => resource.GetWeightFromShape(CellShape.Mountains)).Returns(11);

            var cellToTest = BuildCell(shape: CellShape.Mountains);

            var restrictionCanon = Container.Resolve<ResourceRestrictionCanon>();

            int weight = restrictionCanon.GetPlacementWeightOnCell(mockResource.Object, cellToTest);

            Assert.AreEqual(11, weight);

            mockResource.Verify(
                resource => resource.GetWeightFromShape(CellShape.Mountains), Times.Once,
                "resource.GetWeightFromShape wasn't called as expected"
            );
        }

        [Test]
        public void GetPlacementWeightOnCell_IncludesWeightFromVegetation() {
            var mockResource = new Mock<IResourceDefinition>();

            mockResource.Setup(resource => resource.GetWeightFromVegetation(CellVegetation.Jungle)).Returns(17);

            var cellToTest = BuildCell(vegetation: CellVegetation.Jungle);

            var restrictionCanon = Container.Resolve<ResourceRestrictionCanon>();

            int weight = restrictionCanon.GetPlacementWeightOnCell(mockResource.Object, cellToTest);

            Assert.AreEqual(17, weight);

            mockResource.Verify(
                resource => resource.GetWeightFromVegetation(CellVegetation.Jungle), Times.Once,
                "resource.GetWeightFromVegetation wasn't called as expected"
            );
        }

        [Test]
        public void IsResourceValidOnCell_TrueIfWeightIsPositive() {
            var mockResource = new Mock<IResourceDefinition>();

            mockResource.Setup(resource => resource.GetWeightFromTerrain(CellTerrain.Grassland)).Returns(1);

            var cellToTest = BuildCell(terrain: CellTerrain.Grassland);

            var restrictionCanon = Container.Resolve<ResourceRestrictionCanon>();

            Assert.IsTrue(restrictionCanon.IsResourceValidOnCell(mockResource.Object, cellToTest));
        }

        [Test]
        public void IsResourceValidOnCell_FalseIfWeightIsNegative() {
            var mockResource = new Mock<IResourceDefinition>();

            mockResource.Setup(resource => resource.GetWeightFromTerrain(CellTerrain.Grassland)).Returns(-1);

            var cellToTest = BuildCell(terrain: CellTerrain.Grassland);

            var restrictionCanon = Container.Resolve<ResourceRestrictionCanon>();

            Assert.IsFalse(restrictionCanon.IsResourceValidOnCell(mockResource.Object, cellToTest));
        }

        [Test]
        public void IsResourceValidOnCell_FalseIfWeightIsZero() {
            var mockResource = new Mock<IResourceDefinition>();

            mockResource.Setup(resource => resource.GetWeightFromTerrain(CellTerrain.Grassland)).Returns(0);

            var cellToTest = BuildCell(terrain: CellTerrain.Grassland);

            var restrictionCanon = Container.Resolve<ResourceRestrictionCanon>();

            Assert.IsFalse(restrictionCanon.IsResourceValidOnCell(mockResource.Object, cellToTest));
        }

        #endregion

        #region utilities

        private IHexCell BuildCell(
            CellTerrain    terrain    = CellTerrain.Grassland,
            CellShape      shape      = CellShape.Flatlands,
            CellVegetation vegetation = CellVegetation.None
        ) {
            var mockCell = new Mock<IHexCell>();

            mockCell.Setup(cell => cell.Terrain)   .Returns(terrain);
            mockCell.Setup(cell => cell.Shape)     .Returns(shape);
            mockCell.Setup(cell => cell.Vegetation).Returns(vegetation);

            return mockCell.Object;
        }

        #endregion

        #endregion

    }

}
