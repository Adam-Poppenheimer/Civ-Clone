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

            throw new NotImplementedException();
        }

        [Test]
        public void GetPlacementWeightOnCell_IncludesWeightFromShape() {
            throw new NotImplementedException();
        }

        [Test]
        public void GetPlacementWeightOnCell_IncludesWeightFromVegetation() {
            throw new NotImplementedException();
        }

        [Test]
        public bool IsResourceValidOnCell_TrueIfWeightIsPositive() {
            throw new NotImplementedException();
        }

        [Test]
        public bool IsResourceValidOnCell_FalseIfWeightIsNegative() {
            throw new NotImplementedException();
        }

        [Test]
        public bool IsResourceValidOnCell_FalseIfWeightIsZero() {
            throw new NotImplementedException();
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
