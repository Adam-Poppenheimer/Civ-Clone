using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation;
using Assets.Simulation.Civilizations;
using Assets.Simulation.Cities;
using Assets.Simulation.Cities.Buildings;
using Assets.Simulation.HexMap;

namespace Assets.Tests.Simulation.Civilizations {

    public class ConnectionPathCostLogicTests : ZenjectUnitTestFixture {

        #region internal types

        public class BuildPathCostFunctionTestData {

            public CivilizationTestData CivOne = new CivilizationTestData();
            public CivilizationTestData CivTwo = new CivilizationTestData();

            public HexCellTestData FromCell = new HexCellTestData();
            public HexCellTestData ToCell = new HexCellTestData();

        }

        public class CivilizationTestData {

            public HexCellTestData CapitalLocation = new HexCellTestData() {
                City = new CityTestData()
            };

        }

        public class HexCellTestData {

            public bool IsUnderwater;
            public bool HasRoads;

            public CityTestData City;

            public bool BelongsToCivOne;
            public bool BelongsToCivTwo;

        }

        public class CityTestData {

            public List<BuildingTestData> Buildings = new List<BuildingTestData>();

        }

        public class BuildingTestData {

            public bool ProvidesOverseaConnection;

        }

        #endregion

        #region static fields and properties

        public static IEnumerable BuildPathCostFunctionTestCases {
            get {
                yield return new TestCaseData(new BuildPathCostFunctionTestData() {
                    ToCell = new HexCellTestData() {
                        HasRoads = true
                    }
                }).SetName("ToCell has a road").Returns(1f);

                yield return new TestCaseData(new BuildPathCostFunctionTestData() {
                    ToCell = new HexCellTestData() {
                        HasRoads = false
                    }
                }).SetName("ToCell has no road").Returns(-1f);

                yield return new TestCaseData(new BuildPathCostFunctionTestData() {
                    ToCell = new HexCellTestData() {
                        BelongsToCivOne = true,
                        City = new CityTestData() { }
                    }
                }).SetName("ToCell has city belonging to CivOne").Returns(1f);

                yield return new TestCaseData(new BuildPathCostFunctionTestData() {
                    ToCell = new HexCellTestData() {
                        BelongsToCivTwo = true,
                        City = new CityTestData() { }
                    }
                }).SetName("ToCell has city belonging to CivTwo").Returns(1f);

                yield return new TestCaseData(new BuildPathCostFunctionTestData() {
                    ToCell = new HexCellTestData() {
                        City = new CityTestData() { }
                    }
                }).SetName("ToCell has city belonging to some other civ").Returns(-1f);

                yield return new TestCaseData(new BuildPathCostFunctionTestData() {
                    ToCell = new HexCellTestData() {
                        IsUnderwater = true
                    }
                }).SetName("ToCell is underwater, fromCell has no city").Returns(-1f);

                yield return new TestCaseData(new BuildPathCostFunctionTestData() {
                    ToCell = new HexCellTestData() {
                        IsUnderwater = true
                    },
                    FromCell = new HexCellTestData() {
                        BelongsToCivOne = true,
                        City = new CityTestData() {
                            Buildings = new List<BuildingTestData>() {
                                new BuildingTestData() { ProvidesOverseaConnection = false }
                            }
                        }
                    }
                }).SetName("ToCell is underwater, fromCell has a city with no oversea connectivity").Returns(-1f);

                yield return new TestCaseData(new BuildPathCostFunctionTestData() {
                    ToCell = new HexCellTestData() {
                        IsUnderwater = true
                    },
                    FromCell = new HexCellTestData() {
                        City = new CityTestData() {
                            Buildings = new List<BuildingTestData>() {
                                new BuildingTestData() { ProvidesOverseaConnection = true }
                            }
                        }
                    }
                }).SetName("ToCell is underwater, fromCell has a city with oversea connectivity but belonging to someone else").Returns(-1f);

                yield return new TestCaseData(new BuildPathCostFunctionTestData() {
                    ToCell = new HexCellTestData() {
                        IsUnderwater = true
                    },
                    FromCell = new HexCellTestData() {
                        BelongsToCivOne = true,
                        City = new CityTestData() {
                            Buildings = new List<BuildingTestData>() {
                                new BuildingTestData() { ProvidesOverseaConnection = true },
                                new BuildingTestData() { ProvidesOverseaConnection = false },
                            }
                        }
                    }
                }).SetName("ToCell is underwater, fromCell has a city with oversea connectivity belonging to civOne").Returns(1f);

                yield return new TestCaseData(new BuildPathCostFunctionTestData() {
                    ToCell = new HexCellTestData() {
                        IsUnderwater = true
                    },
                    FromCell = new HexCellTestData() {
                        BelongsToCivTwo = true,
                        City = new CityTestData() {
                            Buildings = new List<BuildingTestData>() {
                                new BuildingTestData() { ProvidesOverseaConnection = true },
                                new BuildingTestData() { ProvidesOverseaConnection = false },
                            }
                        }
                    }
                }).SetName("ToCell is underwater, fromCell has a city with oversea connectivity belonging to civTwo").Returns(1f);





                yield return new TestCaseData(new BuildPathCostFunctionTestData() {
                    FromCell = new HexCellTestData() {
                        IsUnderwater = true
                    }
                }).SetName("FromCell is underwater, toCell has no city").Returns(-1f);

                yield return new TestCaseData(new BuildPathCostFunctionTestData() {
                    FromCell = new HexCellTestData() {
                        IsUnderwater = true
                    },
                    ToCell = new HexCellTestData() {
                        BelongsToCivOne = true,
                        City = new CityTestData() {
                            Buildings = new List<BuildingTestData>() {
                                new BuildingTestData() { ProvidesOverseaConnection = false }
                            }
                        }
                    }
                }).SetName("FromCell is underwater, toCell has a city with no oversea connectivity").Returns(-1f);

                yield return new TestCaseData(new BuildPathCostFunctionTestData() {
                    FromCell = new HexCellTestData() {
                        IsUnderwater = true
                    },
                    ToCell = new HexCellTestData() {
                        City = new CityTestData() {
                            Buildings = new List<BuildingTestData>() {
                                new BuildingTestData() { ProvidesOverseaConnection = true }
                            }
                        }
                    }
                }).SetName("FromCell is underwater, toCell has a city with oversea connectivity but belonging to someone else").Returns(-1f);

                yield return new TestCaseData(new BuildPathCostFunctionTestData() {
                    FromCell = new HexCellTestData() {
                        IsUnderwater = true
                    },
                    ToCell = new HexCellTestData() {
                        BelongsToCivOne = true,
                        City = new CityTestData() {
                            Buildings = new List<BuildingTestData>() {
                                new BuildingTestData() { ProvidesOverseaConnection = true },
                                new BuildingTestData() { ProvidesOverseaConnection = false },
                            }
                        }
                    }
                }).SetName("FromCell is underwater, toCell has a city with oversea connectivity belonging to civOne").Returns(1f);

                yield return new TestCaseData(new BuildPathCostFunctionTestData() {
                    FromCell = new HexCellTestData() {
                        IsUnderwater = true
                    },
                    ToCell = new HexCellTestData() {
                        BelongsToCivTwo = true,
                        City = new CityTestData() {
                            Buildings = new List<BuildingTestData>() {
                                new BuildingTestData() { ProvidesOverseaConnection = true },
                                new BuildingTestData() { ProvidesOverseaConnection = false },
                            }
                        }
                    }
                }).SetName("FromCell is underwater, toCell has a city with oversea connectivity belonging to civTwo").Returns(1f);



                yield return new TestCaseData(new BuildPathCostFunctionTestData() {
                    FromCell = new HexCellTestData() {
                        IsUnderwater = true
                    },
                    ToCell = new HexCellTestData() {
                        IsUnderwater = true
                    }
                }).SetName("FromCell and toCell are both underwater").Returns(1f);
            }
        }

        #endregion

        #region instance fields and properties

        private Mock<IPossessionRelationship<ICivilization, ICity>> MockCityPossessionCanon;
        private Mock<IPossessionRelationship<IHexCell, ICity>>      MockCityLocationCanon;
        private Mock<IPossessionRelationship<ICity, IBuilding>>     MockBuildingPossessionCanon;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockCityPossessionCanon     = new Mock<IPossessionRelationship<ICivilization, ICity>>();
            MockCityLocationCanon       = new Mock<IPossessionRelationship<IHexCell, ICity>>();
            MockBuildingPossessionCanon = new Mock<IPossessionRelationship<ICity, IBuilding>>();

            Container.Bind<IPossessionRelationship<ICivilization, ICity>>().FromInstance(MockCityPossessionCanon    .Object);
            Container.Bind<IPossessionRelationship<IHexCell, ICity>>()     .FromInstance(MockCityLocationCanon      .Object);
            Container.Bind<IPossessionRelationship<ICity, IBuilding>>()    .FromInstance(MockBuildingPossessionCanon.Object);

            Container.Bind<ConnectionPathCostLogic>().AsSingle();
        }

        #endregion

        #region tests

        [TestCaseSource("BuildPathCostFunctionTestCases")]
        [Test(Description = "")]
        public float BuildPathCostFunctionTests(BuildPathCostFunctionTestData testData) {
            var civOne = BuildCivilization(testData.CivOne);
            var civTwo = BuildCivilization(testData.CivTwo);

            IHexCell fromCell, toCell;

            if(testData.FromCell.BelongsToCivOne) {
                fromCell = BuildHexCell(testData.FromCell, civOne);
            }else if(testData.FromCell.BelongsToCivTwo) {
                fromCell = BuildHexCell(testData.FromCell, civTwo);
            }else {
                fromCell = BuildHexCell(testData.FromCell, new Mock<ICivilization>().Object);
            }

            if(testData.ToCell.BelongsToCivOne) {
                toCell = BuildHexCell(testData.ToCell, civOne);
            }else if(testData.ToCell.BelongsToCivTwo) {
                toCell = BuildHexCell(testData.ToCell, civTwo);
            }else {
                toCell = BuildHexCell(testData.ToCell, new Mock<ICivilization>().Object);
            }

            var pathCostLogic = Container.Resolve<ConnectionPathCostLogic>();

            var function = pathCostLogic.BuildPathCostFunction(civOne, civTwo);

            return function(fromCell, toCell);
        }

        #endregion

        #region utilities

        private ICivilization BuildCivilization(CivilizationTestData civData) {
            var newCiv = new Mock<ICivilization>().Object;

            BuildHexCell(civData.CapitalLocation, newCiv);

            return newCiv;
        }

        private IHexCell BuildHexCell(HexCellTestData cellData, ICivilization cityOwner) {
            var mockCell = new Mock<IHexCell>();

            mockCell.Setup(cell => cell.IsUnderwater).Returns(cellData.IsUnderwater);
            mockCell.Setup(cell => cell.HasRoads)    .Returns(cellData.HasRoads);

            var newCell = mockCell.Object;

            if(cellData.City != null) {
                BuildCity(cellData.City, newCell, cityOwner);
            }

            return newCell;
        }

        private ICity BuildCity(CityTestData cityData, IHexCell location, ICivilization owner) {
            var newCity = new Mock<ICity>().Object;

            MockBuildingPossessionCanon.Setup(canon => canon.GetPossessionsOfOwner(newCity))
                .Returns(cityData.Buildings.Select(buildingData => BuildBuilding(buildingData)));

            MockCityLocationCanon.Setup(canon => canon.GetOwnerOfPossession(newCity)).Returns(location);
            MockCityLocationCanon.Setup(canon => canon.GetPossessionsOfOwner(location)).Returns(new List<ICity>() { newCity });

            MockCityPossessionCanon.Setup(canon => canon.GetOwnerOfPossession(newCity)).Returns(owner);

            return newCity;
        }

        private IBuilding BuildBuilding(BuildingTestData buildingData) {
            var mockBuilding = new Mock<IBuilding>();

            var mockTemplate = new Mock<IBuildingTemplate>();

            mockTemplate.Setup(template => template.ProvidesOverseaConnection).Returns(buildingData.ProvidesOverseaConnection);

            mockBuilding.Setup(building => building.Template).Returns(mockTemplate.Object);

            return mockBuilding.Object;
        }

        #endregion

        #endregion

    }

}
