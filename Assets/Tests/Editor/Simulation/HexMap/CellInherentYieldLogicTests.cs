using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation;
using Assets.Simulation.HexMap;

namespace Assets.Tests.Simulation.HexMap {

    public class CellInherentYieldLogicTests : ZenjectUnitTestFixture {

        #region internal types

        public class GetYieldTestData {

            public HexMapConfigTestData Config = new HexMapConfigTestData();

            public HexCellTestData Cell;

        }

        public class HexMapConfigTestData {

            public YieldSummary TerrainYield    = new YieldSummary(food: 1);
            public YieldSummary ShapeYield      = new YieldSummary(production: 20);
            public YieldSummary VegetationYield = new YieldSummary(gold: 300);

        }

        public class HexCellTestData {

            public CellTerrain    Terrain    = CellTerrain.Grassland;
            public CellShape      Shape      = CellShape.Flatlands;
            public CellVegetation Vegetation = CellVegetation.None;

        }

        #endregion

        #region static fields and properties

        public static IEnumerable GetYieldTestCases {
            get {
                yield return new TestCaseData(new GetYieldTestData() {
                    Cell = new HexCellTestData() {
                        Terrain = CellTerrain.Grassland, Shape = CellShape.Flatlands,
                        Vegetation = CellVegetation.Forest
                    }
                }).SetName("Grassland, flatlands, and forest | vegetation dominates").Returns(new YieldSummary(gold: 300));

                yield return new TestCaseData(new GetYieldTestData() {
                    Cell = new HexCellTestData() {
                        Terrain = CellTerrain.Grassland, Shape = CellShape.Flatlands,
                        Vegetation = CellVegetation.Jungle
                    }
                }).SetName("Grassland, flatlands, and jungle | vegetation dominates").Returns(new YieldSummary(gold: 300));

                yield return new TestCaseData(new GetYieldTestData() {
                    Cell = new HexCellTestData() {
                        Terrain = CellTerrain.Grassland, Shape = CellShape.Flatlands,
                        Vegetation = CellVegetation.Marsh
                    }
                }).SetName("Grassland, flatlands, and marsh | vegetation dominates").Returns(new YieldSummary(gold: 300));

                yield return new TestCaseData(new GetYieldTestData() {
                    Cell = new HexCellTestData() {
                        Terrain = CellTerrain.Grassland, Shape = CellShape.Hills,
                        Vegetation = CellVegetation.Forest
                    }
                }).SetName("Grassland, hills, and forest | vegetation dominates").Returns(new YieldSummary(gold: 300));

                yield return new TestCaseData(new GetYieldTestData() {
                    Cell = new HexCellTestData() {
                        Terrain = CellTerrain.Grassland, Shape = CellShape.Mountains,
                        Vegetation = CellVegetation.Forest
                    }
                }).SetName("Grassland, mountains, and forest | vegetation dominates").Returns(new YieldSummary(gold: 300));

                yield return new TestCaseData(new GetYieldTestData() {
                    Cell = new HexCellTestData() {
                        Terrain = CellTerrain.Plains, Shape = CellShape.Flatlands,
                        Vegetation = CellVegetation.Forest
                    }
                }).SetName("plains, flatlands, and forest | vegetation dominates").Returns(new YieldSummary(gold: 300));




                yield return new TestCaseData(new GetYieldTestData() {
                    Cell = new HexCellTestData() {
                        Terrain = CellTerrain.Grassland, Shape = CellShape.Hills,
                        Vegetation = CellVegetation.None
                    }
                }).SetName("Grassland, hills, and none | shape dominates").Returns(new YieldSummary(production: 20));

                yield return new TestCaseData(new GetYieldTestData() {
                    Cell = new HexCellTestData() {
                        Terrain = CellTerrain.Grassland, Shape = CellShape.Mountains,
                        Vegetation = CellVegetation.None
                    }
                }).SetName("Grassland, mountains, and none | shape dominates").Returns(new YieldSummary(production: 20));

                yield return new TestCaseData(new GetYieldTestData() {
                    Cell = new HexCellTestData() {
                        Terrain = CellTerrain.Plains, Shape = CellShape.Hills,
                        Vegetation = CellVegetation.None
                    }
                }).SetName("plains, hills, and none | shape dominates").Returns(new YieldSummary(production: 20));



                yield return new TestCaseData(new GetYieldTestData() {
                    Cell = new HexCellTestData() {
                        Terrain = CellTerrain.Grassland, Shape = CellShape.Flatlands,
                        Vegetation = CellVegetation.None
                    }
                }).SetName("Grassland, flatlands, and none | terrain dominates").Returns(new YieldSummary(food: 1));

                yield return new TestCaseData(new GetYieldTestData() {
                    Cell = new HexCellTestData() {
                        Terrain = CellTerrain.Plains, Shape = CellShape.Flatlands,
                        Vegetation = CellVegetation.None
                    }
                }).SetName("Plains, flatlands, and none | terrain dominates").Returns(new YieldSummary(food: 1));
            }
        }

        #endregion

        #region instance fields and properties

        private Mock<IHexMapConfig> MockConfig;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockConfig = new Mock<IHexMapConfig>();

            Container.Bind<IHexMapConfig>().FromInstance(MockConfig.Object);

            Container.Bind<InherentCellYieldLogic>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        [TestCaseSource("GetYieldTestCases")]
        public YieldSummary GetYieldFromInherentCellPropertiesTests(GetYieldTestData testData) {
            InitializeConfig(testData.Config);

            var cell = BuildCell(testData.Cell);

            var yieldLogic = Container.Resolve<InherentCellYieldLogic>();

            return yieldLogic.GetInherentCellYield(cell);
        }

        #endregion

        #region utilities

        private void InitializeConfig(HexMapConfigTestData configData) {
            MockConfig.Setup(config => config.GetYieldOfTerrain   (It.IsAny<CellTerrain>   ())).Returns(configData.TerrainYield);
            MockConfig.Setup(config => config.GetYieldOfShape     (It.IsAny<CellShape>     ())).Returns(configData.ShapeYield);
            MockConfig.Setup(config => config.GetYieldOfVegetation(It.IsAny<CellVegetation>())).Returns(configData.VegetationYield);
        }

        private IHexCell BuildCell(HexCellTestData cellData) {
            var mockCell = new Mock<IHexCell>();

            mockCell.Setup(cell => cell.Terrain)   .Returns(cellData.Terrain);
            mockCell.Setup(cell => cell.Shape)     .Returns(cellData.Shape);
            mockCell.Setup(cell => cell.Vegetation).Returns(cellData.Vegetation);

            return mockCell.Object;
        }

        #endregion

        #endregion

    }

}
