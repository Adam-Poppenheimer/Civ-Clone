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

        #region internal types

        public class IsResourceValidOnCellTestData {

            public HexCellTestData Cell;

            public ResourceDefinitionTestData Resource;

        }

        public class HexCellTestData {

            public CellTerrain    Terrain;
            public CellShape      Shape;
            public CellVegetation Vegetation;

        }

        public class ResourceDefinitionTestData {

            public bool ValidOnGrassland;
            public bool ValidOnPlains;
            public bool ValidOnDesert;
            public bool ValidOnTundra;
            public bool ValidOnSnow;
            public bool ValidOnShallowWater;

            public bool ValidOnHills;

            public bool ValidOnForest;
            public bool ValidOnJungle;
            public bool ValidOnMarsh;

        }

        #endregion

        #region static fields and properties

        public static IEnumerable IsResourceValidOnCellTestCases {
            get {
                yield return new TestCaseData(new IsResourceValidOnCellTestData() {
                    Cell     = new HexCellTestData() { Terrain = CellTerrain.Grassland },
                    Resource = new ResourceDefinitionTestData() { ValidOnGrassland = true }
                }).SetName("Resource valid on Grassland/Cell is Grassland").Returns(true);

                yield return new TestCaseData(new IsResourceValidOnCellTestData() {
                    Cell     = new HexCellTestData() { Terrain = CellTerrain.Plains },
                    Resource = new ResourceDefinitionTestData() { ValidOnPlains = true }
                }).SetName("Resource valid on Plains/Cell is Plains").Returns(true);

                yield return new TestCaseData(new IsResourceValidOnCellTestData() {
                    Cell     = new HexCellTestData() { Terrain = CellTerrain.Desert },
                    Resource = new ResourceDefinitionTestData() { ValidOnDesert = true }
                }).SetName("Resource valid on Desert/Cell is Desert").Returns(true);

                yield return new TestCaseData(new IsResourceValidOnCellTestData() {
                    Cell     = new HexCellTestData() { Terrain = CellTerrain.Tundra },
                    Resource = new ResourceDefinitionTestData() { ValidOnTundra = true }
                }).SetName("Resource valid on Tundra/Cell is Tundra").Returns(true);

                yield return new TestCaseData(new IsResourceValidOnCellTestData() {
                    Cell     = new HexCellTestData() { Terrain = CellTerrain.Snow },
                    Resource = new ResourceDefinitionTestData() { ValidOnSnow = true }
                }).SetName("Resource valid on Snow/Cell is Snow").Returns(true);

                yield return new TestCaseData(new IsResourceValidOnCellTestData() {
                    Cell     = new HexCellTestData() { Terrain = CellTerrain.ShallowWater },
                    Resource = new ResourceDefinitionTestData() { ValidOnShallowWater = true }
                }).SetName("Resource valid on Shallow Water/Cell is Shallow Water").Returns(true);

                yield return new TestCaseData(new IsResourceValidOnCellTestData() {
                    Cell     = new HexCellTestData() { Shape = CellShape.Hills },
                    Resource = new ResourceDefinitionTestData() { ValidOnHills = true }
                }).SetName("Resource valid on Hills/Cell is Hills").Returns(true);

                yield return new TestCaseData(new IsResourceValidOnCellTestData() {
                    Cell     = new HexCellTestData() { Vegetation = CellVegetation.Forest },
                    Resource = new ResourceDefinitionTestData() { ValidOnForest = true }
                }).SetName("Resource valid on Forest/Cell is Forest").Returns(true);

                yield return new TestCaseData(new IsResourceValidOnCellTestData() {
                    Cell     = new HexCellTestData() { Vegetation = CellVegetation.Jungle },
                    Resource = new ResourceDefinitionTestData() { ValidOnJungle = true }
                }).SetName("Resource valid on Jungle/Cell is Jungle").Returns(true);

                yield return new TestCaseData(new IsResourceValidOnCellTestData() {
                    Cell     = new HexCellTestData() { Vegetation = CellVegetation.Marsh },
                    Resource = new ResourceDefinitionTestData() { ValidOnMarsh = true }
                }).SetName("Resource valid on Marsh/Cell is Marsh").Returns(true);




                yield return new TestCaseData(new IsResourceValidOnCellTestData() {
                    Cell = new HexCellTestData() {
                        Terrain = CellTerrain.Grassland, Shape = CellShape.Mountains,
                        Vegetation = CellVegetation.Marsh
                    },
                    Resource = new ResourceDefinitionTestData() {
                        ValidOnPlains = true, ValidOnHills = true, ValidOnJungle = true
                    }
                }).SetName("Multiple valid options/Cell applies to none").Returns(false);

                yield return new TestCaseData(new IsResourceValidOnCellTestData() {
                    Cell = new HexCellTestData() {
                        Terrain = CellTerrain.Plains, Shape = CellShape.Mountains,
                        Vegetation = CellVegetation.Marsh
                    },
                    Resource = new ResourceDefinitionTestData() {
                        ValidOnPlains = true, ValidOnHills = true, ValidOnJungle = true
                    }
                }).SetName("Multiple valid options/Cell applies to only one").Returns(true);

                yield return new TestCaseData(new IsResourceValidOnCellTestData() {
                    Cell = new HexCellTestData() {
                        Terrain = CellTerrain.Plains, Shape = CellShape.Hills,
                        Vegetation = CellVegetation.Marsh
                    },
                    Resource = new ResourceDefinitionTestData() {
                        ValidOnPlains = true, ValidOnHills = true, ValidOnJungle = true
                    }
                }).SetName("Multiple valid options/Cell applies to two").Returns(true);

                yield return new TestCaseData(new IsResourceValidOnCellTestData() {
                    Cell = new HexCellTestData() {
                        Terrain = CellTerrain.Plains, Shape = CellShape.Hills,
                        Vegetation = CellVegetation.Jungle
                    },
                    Resource = new ResourceDefinitionTestData() {
                        ValidOnPlains = true, ValidOnHills = true, ValidOnJungle = true
                    }
                }).SetName("Multiple valid options/Cell applies to three").Returns(true);
                
            }
        }

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            Container.Bind<ResourceRestrictionCanon>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        [TestCaseSource("IsResourceValidOnCellTestCases")]
        public bool IsResourceValidOnCellTests(IsResourceValidOnCellTestData testData) {
            var cell = BuildCell(testData.Cell);
            var resource = BuildResource(testData.Resource);

            var restrictionCanon = Container.Resolve<ResourceRestrictionCanon>();

            return restrictionCanon.IsResourceValidOnCell(resource, cell);
        }

        #endregion

        #region utilities

        private IHexCell BuildCell(HexCellTestData cellData) {
            var mockCell = new Mock<IHexCell>();

            mockCell.Setup(cell => cell.Terrain)   .Returns(cellData.Terrain);
            mockCell.Setup(cell => cell.Shape)     .Returns(cellData.Shape);
            mockCell.Setup(cell => cell.Vegetation).Returns(cellData.Vegetation);

            return mockCell.Object;
        }

        private IResourceDefinition BuildResource(ResourceDefinitionTestData resourceData) {
            var mockResource = new Mock<IResourceDefinition>();

            mockResource.Setup(resource => resource.ValidOnGrassland)   .Returns(resourceData.ValidOnGrassland);
            mockResource.Setup(resource => resource.ValidOnPlains)      .Returns(resourceData.ValidOnPlains);
            mockResource.Setup(resource => resource.ValidOnDesert)      .Returns(resourceData.ValidOnDesert);
            mockResource.Setup(resource => resource.ValidOnTundra)      .Returns(resourceData.ValidOnTundra);
            mockResource.Setup(resource => resource.ValidOnSnow)        .Returns(resourceData.ValidOnSnow);
            mockResource.Setup(resource => resource.ValidOnShallowWater).Returns(resourceData.ValidOnShallowWater);

            mockResource.Setup(resource => resource.ValidOnHills)       .Returns(resourceData.ValidOnHills);

            mockResource.Setup(resource => resource.ValidOnForest)      .Returns(resourceData.ValidOnForest);
            mockResource.Setup(resource => resource.ValidOnJungle)      .Returns(resourceData.ValidOnJungle);
            mockResource.Setup(resource => resource.ValidOnMarsh)       .Returns(resourceData.ValidOnMarsh);

            return mockResource.Object;
        }

        #endregion

        #endregion

    }

}
