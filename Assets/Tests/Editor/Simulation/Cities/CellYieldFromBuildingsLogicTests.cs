using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation;
using Assets.Simulation.Cities.Buildings;
using Assets.Simulation.HexMap;

namespace Assets.Tests.Simulation.Cities {

    public class CellYieldFromBuildingsLogicTests : ZenjectUnitTestFixture {

        #region internal types

        public class GetBonusCellYieldFromBuildingsTestData {

            public HexCellTestData Cell;

            public List<BuildingTemplateTestData> Buildings = new List<BuildingTemplateTestData>();

        }

        public class HexCellTestData {

            public CellTerrain    Terrain;
            public CellShape      Shape;
            public CellVegetation Vegetation;

        }

        public class BuildingTemplateTestData {

            public List<CellYieldModificationData> ModificationData = new List<CellYieldModificationData>();

        }

        #endregion

        #region static fields and properties

        public static IEnumerable GetBonusCellYieldFromBuildingsTestCases {
            get {
                yield return new TestCaseData(new GetBonusCellYieldFromBuildingsTestData() {
                    Cell = new HexCellTestData() {
                        Terrain = CellTerrain.Plains, Shape = CellShape.Hills, Vegetation = CellVegetation.None
                    },
                    Buildings = new List<BuildingTemplateTestData>() {
                        new BuildingTemplateTestData() {
                            ModificationData = new List<CellYieldModificationData>() {
                                new CellYieldModificationData(CellTerrain.Grassland, new YieldSummary(food: 1)),
                                new CellYieldModificationData(CellTerrain.Plains, new YieldSummary(food: 2))
                            }
                        },
                        new BuildingTemplateTestData() {
                            ModificationData = new List<CellYieldModificationData>() {
                                new CellYieldModificationData(CellTerrain.Desert, new YieldSummary(production: 3))
                            }
                        },
                        new BuildingTemplateTestData() {
                            ModificationData = new List<CellYieldModificationData>() {
                                new CellYieldModificationData(CellTerrain.Plains, new YieldSummary(gold: 4))
                            }
                        }
                    }
                }).SetName("Mods considering terrain | apply only if cell has correct terrain").Returns(
                    new YieldSummary(food: 2, gold: 4)
                );

                yield return new TestCaseData(new GetBonusCellYieldFromBuildingsTestData() {
                    Cell = new HexCellTestData() {
                        Terrain = CellTerrain.Plains, Shape = CellShape.Hills, Vegetation = CellVegetation.None
                    },
                    Buildings = new List<BuildingTemplateTestData>() {
                        new BuildingTemplateTestData() {
                            ModificationData = new List<CellYieldModificationData>() {
                                new CellYieldModificationData(CellShape.Flatlands, new YieldSummary(food: 1)),
                                new CellYieldModificationData(CellShape.Hills, new YieldSummary(food: 2))
                            }
                        },
                        new BuildingTemplateTestData() {
                            ModificationData = new List<CellYieldModificationData>() {
                                new CellYieldModificationData(CellShape.Flatlands, new YieldSummary(production: 3))
                            }
                        },
                        new BuildingTemplateTestData() {
                            ModificationData = new List<CellYieldModificationData>() {
                                new CellYieldModificationData(CellShape.Hills, new YieldSummary(gold: 4))
                            }
                        }
                    }
                }).SetName("Mods considering shape | apply only if cell has correct shape").Returns(
                    new YieldSummary(food: 2, gold: 4)
                );

                yield return new TestCaseData(new GetBonusCellYieldFromBuildingsTestData() {
                    Cell = new HexCellTestData() {
                        Terrain = CellTerrain.Plains, Shape = CellShape.Hills, Vegetation = CellVegetation.None
                    },
                    Buildings = new List<BuildingTemplateTestData>() {
                        new BuildingTemplateTestData() {
                            ModificationData = new List<CellYieldModificationData>() {
                                new CellYieldModificationData(CellVegetation.Forest, new YieldSummary(food: 1)),
                                new CellYieldModificationData(CellVegetation.None, new YieldSummary(food: 2))
                            }
                        },
                        new BuildingTemplateTestData() {
                            ModificationData = new List<CellYieldModificationData>() {
                                new CellYieldModificationData(CellVegetation.Jungle, new YieldSummary(production: 3))
                            }
                        },
                        new BuildingTemplateTestData() {
                            ModificationData = new List<CellYieldModificationData>() {
                                new CellYieldModificationData(CellVegetation.None, new YieldSummary(gold: 4))
                            }
                        }
                    }
                }).SetName("Mods considering vegetation | apply only if cell has correct vegetation").Returns(
                    new YieldSummary(food: 2, gold: 4)
                );

                yield return new TestCaseData(new GetBonusCellYieldFromBuildingsTestData() {
                    Cell = new HexCellTestData() {
                        Terrain = CellTerrain.FreshWater, Shape = CellShape.Hills, Vegetation = CellVegetation.None
                    },
                    Buildings = new List<BuildingTemplateTestData>() {
                        new BuildingTemplateTestData() {
                            ModificationData = new List<CellYieldModificationData>() {
                                new CellYieldModificationData(false, new YieldSummary(food: 1)),
                                new CellYieldModificationData(true, new YieldSummary(food: 2))
                            }
                        },
                        new BuildingTemplateTestData() {
                            ModificationData = new List<CellYieldModificationData>() {
                                new CellYieldModificationData(false, new YieldSummary(production: 3))
                            }
                        },
                        new BuildingTemplateTestData() {
                            ModificationData = new List<CellYieldModificationData>() {
                                new CellYieldModificationData(true, new YieldSummary(gold: 4))
                            }
                        }
                    }

                }).SetName("Mods considering underwater status | apply only if cell terrain is water").Returns(
                    new YieldSummary(food: 2, gold: 4)
                );
            }
        }

        #endregion

        #region instance fields and properties



        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            Container.Bind<CellYieldFromBuildingsLogic>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        [TestCaseSource("GetBonusCellYieldFromBuildingsTestCases")]
        public YieldSummary GetBonusCellYieldFromBuildingsTests(GetBonusCellYieldFromBuildingsTestData testData) {
            var cell = BuildHexCell(testData.Cell);

            var buildings = testData.Buildings.Select(buildingData => BuildBuildingTemplate(buildingData));

            var yieldLogic = Container.Resolve<CellYieldFromBuildingsLogic>();

            return yieldLogic.GetBonusCellYieldFromBuildings(cell, buildings);
        }

        #endregion

        #region utilities

        private IHexCell BuildHexCell(HexCellTestData cellData) {
            var mockCell = new Mock<IHexCell>();

            mockCell.Setup(cell => cell.Terrain)   .Returns(cellData.Terrain);
            mockCell.Setup(cell => cell.Shape)     .Returns(cellData.Shape);
            mockCell.Setup(cell => cell.Vegetation).Returns(cellData.Vegetation);

            return mockCell.Object;
        }

        private IBuildingTemplate BuildBuildingTemplate(BuildingTemplateTestData buildingData) {
            var mockTemplate = new Mock<IBuildingTemplate>();

            mockTemplate.Setup(template => template.CellYieldModifications)
                        .Returns(buildingData.ModificationData.Cast<ICellYieldModificationData>());

            return mockTemplate.Object;
        }

        #endregion

        #endregion

    }

}
