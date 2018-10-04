using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation;
using Assets.Simulation.MapResources;
using Assets.Simulation.HexMap;
using Assets.Simulation.Improvements;

namespace Assets.Tests.Simulation.MapResources {

    [TestFixture]
    public class ResourceRestrictionCanonTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IPossessionRelationship<IHexCell, IResourceNode>> MockNodePositionCanon;
        private Mock<IPossessionRelationship<IHexCell, IImprovement>>  MockImprovementLocationCanon;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockNodePositionCanon        = new Mock<IPossessionRelationship<IHexCell, IResourceNode>>();
            MockImprovementLocationCanon = new Mock<IPossessionRelationship<IHexCell, IImprovement>>();

            Container.Bind<IPossessionRelationship<IHexCell, IResourceNode>>().FromInstance(MockNodePositionCanon       .Object);
            Container.Bind<IPossessionRelationship<IHexCell, IImprovement>> ().FromInstance(MockImprovementLocationCanon.Object);

            Container.Bind<ResourceRestrictionLogic>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void GetPlacementWeightOnCell_IncludesWeightFromTerrain() {
            var mockResource = new Mock<IResourceDefinition>();

            mockResource.Setup(resource => resource.GetWeightFromTerrain(CellTerrain.Desert)).Returns(15);

            var cellToTest = BuildCell(terrain: CellTerrain.Desert);

            var restrictionCanon = Container.Resolve<ResourceRestrictionLogic>();

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

            var restrictionCanon = Container.Resolve<ResourceRestrictionLogic>();

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

            var restrictionCanon = Container.Resolve<ResourceRestrictionLogic>();

            int weight = restrictionCanon.GetPlacementWeightOnCell(mockResource.Object, cellToTest);

            Assert.AreEqual(17, weight);

            mockResource.Verify(
                resource => resource.GetWeightFromVegetation(CellVegetation.Jungle), Times.Once,
                "resource.GetWeightFromVegetation wasn't called as expected"
            );
        }

        [Test]
        public void GetPlacementWeightOnCell_AlwaysZeroIfFeatureNotNone() {
            var mockResource = new Mock<IResourceDefinition>();

            mockResource.Setup(resource => resource.GetWeightFromTerrain   (CellTerrain.Grassland)).Returns(20);
            mockResource.Setup(resource => resource.GetWeightFromShape     (CellShape.Flatlands))  .Returns(20);
            mockResource.Setup(resource => resource.GetWeightFromVegetation(CellVegetation.None))  .Returns(20);

            var cellToTest = BuildCell(
                terrain: CellTerrain.Grassland, shape: CellShape.Flatlands,
                vegetation: CellVegetation.None, feature: CellFeature.Oasis
            );

            var restrictionCanon = Container.Resolve<ResourceRestrictionLogic>();

            Assert.AreEqual(0, restrictionCanon.GetPlacementWeightOnCell(mockResource.Object, cellToTest));
        }

        [Test]
        public void GetPlacementWeightOnCell_AlwaysZeroIfCellAlreadyHasNode() {
            var mockResource = new Mock<IResourceDefinition>();

            mockResource.Setup(resource => resource.GetWeightFromTerrain   (CellTerrain.Grassland)).Returns(20);
            mockResource.Setup(resource => resource.GetWeightFromShape     (CellShape.Flatlands))  .Returns(20);
            mockResource.Setup(resource => resource.GetWeightFromVegetation(CellVegetation.None))  .Returns(20);

            var cellToTest = BuildCell(
                terrain: CellTerrain.Grassland, shape: CellShape.Flatlands,
                vegetation: CellVegetation.None, feature: CellFeature.None
            );

            BuildResourceNode(cellToTest);

            var restrictionCanon = Container.Resolve<ResourceRestrictionLogic>();

            Assert.AreEqual(0, restrictionCanon.GetPlacementWeightOnCell(mockResource.Object, cellToTest));
        }

        [Test]
        public void IsResourceValidOnCell_TrueIfWeightIsPositive() {
            var mockResource = new Mock<IResourceDefinition>();

            mockResource.Setup(resource => resource.GetWeightFromTerrain(CellTerrain.Grassland)).Returns(1);

            var cellToTest = BuildCell(terrain: CellTerrain.Grassland);

            var restrictionCanon = Container.Resolve<ResourceRestrictionLogic>();

            Assert.IsTrue(restrictionCanon.IsResourceValidOnCell(mockResource.Object, cellToTest));
        }

        [Test]
        public void IsResourceValidOnCell_FalseIfWeightIsNegative() {
            var mockResource = new Mock<IResourceDefinition>();

            mockResource.Setup(resource => resource.GetWeightFromTerrain(CellTerrain.Grassland)).Returns(-1);

            var cellToTest = BuildCell(terrain: CellTerrain.Grassland);

            var restrictionCanon = Container.Resolve<ResourceRestrictionLogic>();

            Assert.IsFalse(restrictionCanon.IsResourceValidOnCell(mockResource.Object, cellToTest));
        }

        [Test]
        public void IsResourceValidOnCell_FalseIfWeightIsZero() {
            var mockResource = new Mock<IResourceDefinition>();

            mockResource.Setup(resource => resource.GetWeightFromTerrain(CellTerrain.Grassland)).Returns(0);

            var cellToTest = BuildCell(terrain: CellTerrain.Grassland);

            var restrictionCanon = Container.Resolve<ResourceRestrictionLogic>();

            Assert.IsFalse(restrictionCanon.IsResourceValidOnCell(mockResource.Object, cellToTest));
        }

        #endregion

        #region utilities

        private IHexCell BuildCell(
            CellTerrain    terrain    = CellTerrain.Grassland,
            CellShape      shape      = CellShape.Flatlands,
            CellVegetation vegetation = CellVegetation.None,
            CellFeature    feature    = CellFeature.None
        ) {
            var mockCell = new Mock<IHexCell>();

            mockCell.Setup(cell => cell.Terrain)   .Returns(terrain);
            mockCell.Setup(cell => cell.Shape)     .Returns(shape);
            mockCell.Setup(cell => cell.Vegetation).Returns(vegetation);
            mockCell.Setup(cell => cell.Feature)   .Returns(feature);

            return mockCell.Object;
        }

        private IResourceNode BuildResourceNode(IHexCell location) {
            var newNode = new Mock<IResourceNode>().Object;

            MockNodePositionCanon.Setup(canon => canon.GetPossessionsOfOwner(location))
                                 .Returns(new List<IResourceNode>() { newNode });

            return newNode;
        }

        #endregion

        #endregion

    }

}
