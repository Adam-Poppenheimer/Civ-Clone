using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation;
using Assets.Simulation.Units;
using Assets.Simulation.Units.Promotions;
using Assets.Simulation.Civilizations;
using Assets.Simulation.Cities;
using Assets.Simulation.HexMap;

namespace Assets.Tests.Simulation.Units {

    [TestFixture]
    public class UnitHealingLogicTests : ZenjectUnitTestFixture {

        #region internal types

        public class PerformHealingOnUnitTestData {

            public UnitTestData UnitToTest = new UnitTestData();

            public FocusedHexCellTestData LocationOfTestUnit = new FocusedHexCellTestData();

            public List<NeighboringHexCellTestData> NeighboringCells
                 = new List<NeighboringHexCellTestData>();

            public UnitConfigTestData Config = new UnitConfigTestData();

        }

        public class FocusedHexCellTestData {

            public bool HasCity;

            public bool OwnedByDomesticCiv;

        }

        public class NeighboringHexCellTestData {

            public List<UnitTestData> UnitsAtCell = new List<UnitTestData>();

        }

        public class UnitTestData {

            public UnitType Type;

            public HealingInfo HealingInfo = new HealingInfo();

            public bool BelongsToDomesticCiv;

            public int MaxHitpoints;

            public float CurrentMovement;

            public float MaxMovement;

        }

        public class UnitConfigTestData {

            public float CityRepairPercentPerTurn;

            public int ForeignNavalHealingPerTurn;
            public int FriendlyNavalHealingPerTurn;
            public int GarrisonedNavalHealingPerTurn;

            public int ForeignLandHealingPerTurn;
            public int FriendlyLandHealingPerTurn;
            public int GarrisonedLandHealingPerTurn;

        }

        #endregion

        #region static fields and properties

        public static IEnumerable PerformHealingOnUnitTestCases {
            get {
                foreach(var testCase in PerformHealingOnUnitTestCases_City) {
                    yield return testCase;
                }

                foreach(var testCase in PerformHealingOnUnitTestCases_Land) {
                    yield return testCase;
                }

                foreach(var testCase in PerformHealingOnUnitTestCases_Naval) {
                    yield return testCase;
                }
            }
        }

        public static IEnumerable PerformHealingOnUnitTestCases_City {
            get {
                yield return new TestCaseData(new PerformHealingOnUnitTestData() {
                    UnitToTest = new UnitTestData() {
                        Type = UnitType.City, MaxHitpoints = 200
                    },
                    Config = new UnitConfigTestData() { CityRepairPercentPerTurn = 0.5f }
                }).SetName("City repairs configured percentage of max hitpoints").Returns(100);
            }
        }

        private static IEnumerable PerformHealingOnUnitTestCases_Land {
            get {
                yield return new TestCaseData(new PerformHealingOnUnitTestData() {
                    UnitToTest = new UnitTestData() { Type = UnitType.Melee },
                    LocationOfTestUnit = new FocusedHexCellTestData() {
                        HasCity = false, OwnedByDomesticCiv = false
                    },
                    Config = new UnitConfigTestData() { ForeignLandHealingPerTurn = 10 }
                }).SetName("Land unit in foreign territory | Config contributes correct healing").Returns(10);

                yield return new TestCaseData(new PerformHealingOnUnitTestData() {
                    UnitToTest = new UnitTestData() { Type = UnitType.Melee },
                    LocationOfTestUnit = new FocusedHexCellTestData() {
                        HasCity = false, OwnedByDomesticCiv = true
                    },
                    Config = new UnitConfigTestData() { FriendlyLandHealingPerTurn = 20 }
                }).SetName("Land unit in friendly territory | Config contributes correct healing").Returns(20);

                yield return new TestCaseData(new PerformHealingOnUnitTestData() {
                    UnitToTest = new UnitTestData() { Type = UnitType.Melee },
                    LocationOfTestUnit = new FocusedHexCellTestData() {
                        HasCity = true, OwnedByDomesticCiv = false
                    },
                    Config = new UnitConfigTestData() { GarrisonedLandHealingPerTurn = 25 }
                }).SetName("Land unit in some city | Config contributes correct healing").Returns(25);

                yield return new TestCaseData(new PerformHealingOnUnitTestData() {
                    UnitToTest = new UnitTestData() {
                        Type = UnitType.Melee, HealingInfo = new HealingInfo() {
                            BonusHealingToSelf = 15
                        }
                    },
                    LocationOfTestUnit = new FocusedHexCellTestData() {
                        HasCity = false, OwnedByDomesticCiv = false
                    },
                }).SetName("Land units add bonus healing from parsed HealingInfo").Returns(15);

                yield return new TestCaseData(new PerformHealingOnUnitTestData() {
                    UnitToTest = new UnitTestData() {
                        Type = UnitType.Melee, CurrentMovement = 1, MaxMovement = 2
                    },
                    LocationOfTestUnit = new FocusedHexCellTestData() {
                        HasCity = false, OwnedByDomesticCiv = false
                    },
                    Config = new UnitConfigTestData() { ForeignLandHealingPerTurn = 10 }
                }).SetName("Land unit has spent movement | No healing occurs").Returns(0);

                yield return new TestCaseData(new PerformHealingOnUnitTestData() {
                    UnitToTest = new UnitTestData() {
                        Type = UnitType.Melee, CurrentMovement = 1, MaxMovement = 2,
                        HealingInfo = new HealingInfo() { HealsEveryTurn = true }
                    },
                    LocationOfTestUnit = new FocusedHexCellTestData() {
                        HasCity = false, OwnedByDomesticCiv = false
                    },
                    Config = new UnitConfigTestData() { ForeignLandHealingPerTurn = 10 }
                }).SetName("Land unit has spent movement, but HealingInfo.CanHealWhileActive is true | Healing occurs").Returns(10);

                yield return new TestCaseData(new PerformHealingOnUnitTestData() {
                    UnitToTest = new UnitTestData() { Type = UnitType.Melee },
                    NeighboringCells = new List<NeighboringHexCellTestData>() {
                        new NeighboringHexCellTestData() {
                            UnitsAtCell = new List<UnitTestData>() {
                                new UnitTestData() {
                                    BelongsToDomesticCiv = true,
                                    HealingInfo = new HealingInfo() { BonusHealingToAdjacent = 1 }
                                }
                            }
                        },
                        new NeighboringHexCellTestData() {
                            UnitsAtCell = new List<UnitTestData>() {
                                new UnitTestData() {
                                    BelongsToDomesticCiv = true,
                                    HealingInfo = new HealingInfo() { BonusHealingToAdjacent = 20 }
                                },
                                new UnitTestData() {
                                    BelongsToDomesticCiv = false,
                                    HealingInfo = new HealingInfo() { BonusHealingToAdjacent = 300 }
                                }
                            }
                        },
                        new NeighboringHexCellTestData() {
                            UnitsAtCell = new List<UnitTestData>() {
                                new UnitTestData() {
                                    BelongsToDomesticCiv = false,
                                    HealingInfo = new HealingInfo() { BonusHealingToAdjacent = 4000 }
                                }
                            }
                        },
                        new NeighboringHexCellTestData()
                    }
                }).SetName("Land unit pulls adjacency healing from neighboring cells only from friendly units").Returns(21);
            }
        }

        private static IEnumerable PerformHealingOnUnitTestCases_Naval {
            get {
                yield return new TestCaseData(new PerformHealingOnUnitTestData() {
                    UnitToTest = new UnitTestData() { Type = UnitType.NavalMelee },
                    LocationOfTestUnit = new FocusedHexCellTestData() {
                        HasCity = false, OwnedByDomesticCiv = false
                    },
                    Config = new UnitConfigTestData() { ForeignNavalHealingPerTurn = 11 }
                }).SetName("Naval unit in foreign territory | Config contributes correct healing").Returns(11);

                yield return new TestCaseData(new PerformHealingOnUnitTestData() {
                    UnitToTest = new UnitTestData() {
                        Type = UnitType.NavalMelee, HealingInfo = new HealingInfo() {
                            AlternateNavalBaseHealing = 15
                        }
                    },
                    LocationOfTestUnit = new FocusedHexCellTestData() {
                        HasCity = false, OwnedByDomesticCiv = false
                    },
                    Config = new UnitConfigTestData() { ForeignNavalHealingPerTurn = 11 }
                }).SetName("Naval unit in foreign territory | Overrides with AlternateNavalBaseHealing if larger").Returns(15);

                yield return new TestCaseData(new PerformHealingOnUnitTestData() {
                    UnitToTest = new UnitTestData() {
                        Type = UnitType.NavalMelee, HealingInfo = new HealingInfo() {
                            AlternateNavalBaseHealing = 3
                        }
                    },
                    LocationOfTestUnit = new FocusedHexCellTestData() {
                        HasCity = false, OwnedByDomesticCiv = false
                    },
                    Config = new UnitConfigTestData() { ForeignNavalHealingPerTurn = 11 }
                }).SetName("Naval unit in foreign territory | Keeps configured default if AlternateNavalBaseHealing is smaller").Returns(11);

                yield return new TestCaseData(new PerformHealingOnUnitTestData() {
                    UnitToTest = new UnitTestData() { Type = UnitType.NavalRanged },
                    LocationOfTestUnit = new FocusedHexCellTestData() {
                        HasCity = false, OwnedByDomesticCiv = true
                    },
                    Config = new UnitConfigTestData() { FriendlyNavalHealingPerTurn = 20 }
                }).SetName("Naval unit in friendly territory | Config contributes correct healing").Returns(20);

                yield return new TestCaseData(new PerformHealingOnUnitTestData() {
                    UnitToTest = new UnitTestData() { Type = UnitType.NavalMelee },
                    LocationOfTestUnit = new FocusedHexCellTestData() {
                        HasCity = true, OwnedByDomesticCiv = false
                    },
                    Config = new UnitConfigTestData() { GarrisonedNavalHealingPerTurn = 25 }
                }).SetName("Naval unit in some city | Config contributes correct healing").Returns(25);

                yield return new TestCaseData(new PerformHealingOnUnitTestData() {
                    UnitToTest = new UnitTestData() {
                        Type = UnitType.NavalRanged, HealingInfo = new HealingInfo() {
                            BonusHealingToSelf = 15
                        }
                    },
                    LocationOfTestUnit = new FocusedHexCellTestData() {
                        HasCity = false, OwnedByDomesticCiv = false
                    },
                }).SetName("Naval units add bonus healing from parsed HealingInfo").Returns(15);

                yield return new TestCaseData(new PerformHealingOnUnitTestData() {
                    UnitToTest = new UnitTestData() {
                        Type = UnitType.NavalMelee, CurrentMovement = 1, MaxMovement = 2
                    },
                    LocationOfTestUnit = new FocusedHexCellTestData() {
                        HasCity = false, OwnedByDomesticCiv = false
                    },
                    Config = new UnitConfigTestData() { ForeignNavalHealingPerTurn = 10 }
                }).SetName("Naval unit has spent movement | No healing occurs").Returns(0);

                yield return new TestCaseData(new PerformHealingOnUnitTestData() {
                    UnitToTest = new UnitTestData() {
                        Type = UnitType.NavalMelee, CurrentMovement = 1, MaxMovement = 2,
                        HealingInfo = new HealingInfo() { HealsEveryTurn = true }
                    },
                    LocationOfTestUnit = new FocusedHexCellTestData() {
                        HasCity = false, OwnedByDomesticCiv = false
                    },
                    Config = new UnitConfigTestData() { ForeignNavalHealingPerTurn = 10 }
                }).SetName("Naval unit has spent movement, but HealingInfo.CanHealWhileActive is true | Healing occurs").Returns(10);

                yield return new TestCaseData(new PerformHealingOnUnitTestData() {
                    UnitToTest = new UnitTestData() { Type = UnitType.NavalMelee },
                    NeighboringCells = new List<NeighboringHexCellTestData>() {
                        new NeighboringHexCellTestData() {
                            UnitsAtCell = new List<UnitTestData>() {
                                new UnitTestData() {
                                    BelongsToDomesticCiv = true,
                                    HealingInfo = new HealingInfo() { BonusHealingToAdjacent = 1 }
                                }
                            }
                        },
                        new NeighboringHexCellTestData() {
                            UnitsAtCell = new List<UnitTestData>() {
                                new UnitTestData() {
                                    BelongsToDomesticCiv = true,
                                    HealingInfo = new HealingInfo() { BonusHealingToAdjacent = 20 }
                                },
                                new UnitTestData() {
                                    BelongsToDomesticCiv = false,
                                    HealingInfo = new HealingInfo() { BonusHealingToAdjacent = 300 }
                                }
                            }
                        },
                        new NeighboringHexCellTestData() {
                            UnitsAtCell = new List<UnitTestData>() {
                                new UnitTestData() {
                                    BelongsToDomesticCiv = false,
                                    HealingInfo = new HealingInfo() { BonusHealingToAdjacent = 4000 }
                                }
                            }
                        },
                        new NeighboringHexCellTestData()
                    }
                }).SetName("Naval unit pulls adjacency healing from neighboring cells only from friendly units").Returns(21);
            }
        }

        #endregion

        #region instance fields and properties

        private Mock<IUnitConfig>                                   MockUnitConfig;
        private Mock<IPromotionParser>                              MockPromotionParser;
        private Mock<IUnitPositionCanon>                            MockUnitPositionCanon;
        private Mock<IPossessionRelationship<ICivilization, IUnit>> MockUnitPossessionCanon;
        private Mock<IPossessionRelationship<IHexCell, ICity>>      MockCityLocationCanon;
        private Mock<ICivilizationTerritoryLogic>                   MockCivTerritoryLogic;
        private Mock<IHexGrid>                                      MockGrid;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockUnitConfig          = new Mock<IUnitConfig>();
            MockPromotionParser     = new Mock<IPromotionParser>();
            MockUnitPositionCanon   = new Mock<IUnitPositionCanon>();
            MockUnitPossessionCanon = new Mock<IPossessionRelationship<ICivilization, IUnit>>();
            MockCityLocationCanon   = new Mock<IPossessionRelationship<IHexCell, ICity>>();
            MockCivTerritoryLogic   = new Mock<ICivilizationTerritoryLogic>();
            MockGrid                = new Mock<IHexGrid>();

            Container.Bind<IUnitConfig>                                  ().FromInstance(MockUnitConfig         .Object);
            Container.Bind<IPromotionParser>                             ().FromInstance(MockPromotionParser    .Object);
            Container.Bind<IUnitPositionCanon>                           ().FromInstance(MockUnitPositionCanon  .Object);
            Container.Bind<IPossessionRelationship<ICivilization, IUnit>>().FromInstance(MockUnitPossessionCanon.Object);
            Container.Bind<IPossessionRelationship<IHexCell, ICity>>     ().FromInstance(MockCityLocationCanon  .Object);
            Container.Bind<ICivilizationTerritoryLogic>                  ().FromInstance(MockCivTerritoryLogic  .Object);
            Container.Bind<IHexGrid>                                     ().FromInstance(MockGrid               .Object);

            Container.Bind<UnitHealingLogic>().AsSingle();
        }

        private void SetupConfig(UnitConfigTestData configData) {
            MockUnitConfig.Setup(config => config.CityRepairPercentPerTurn).Returns(configData.CityRepairPercentPerTurn);

            MockUnitConfig.Setup(config => config.ForeignNavalHealingPerTurn)   .Returns(configData.ForeignNavalHealingPerTurn);
            MockUnitConfig.Setup(config => config.FriendlyNavalHealingPerTurn)  .Returns(configData.FriendlyNavalHealingPerTurn);
            MockUnitConfig.Setup(config => config.GarrisonedNavalHealingPerTurn).Returns(configData.GarrisonedNavalHealingPerTurn);

            MockUnitConfig.Setup(config => config.ForeignLandHealingPerTurn)   .Returns(configData.ForeignLandHealingPerTurn);
            MockUnitConfig.Setup(config => config.FriendlyLandHealingPerTurn)  .Returns(configData.FriendlyLandHealingPerTurn);
            MockUnitConfig.Setup(config => config.GarrisonedLandHealingPerTurn).Returns(configData.GarrisonedLandHealingPerTurn);
        }

        #endregion

        #region tests

        [Test]
        [TestCaseSource("PerformHealingOnUnitTestCases")]
        public int PerformHealingOnUnitTests(PerformHealingOnUnitTestData testData) {
            SetupConfig(testData.Config);

            var domesticCiv = BuildCivilization();
            var foreignCiv  = BuildCivilization();

            var locationOfTestUnit = BuildHexCell(testData.LocationOfTestUnit, domesticCiv, foreignCiv);

            var unitToTest = BuildUnit(testData.UnitToTest, locationOfTestUnit, domesticCiv);

            var neighbors = testData.NeighboringCells.Select(
                neighborData => BuildHexCell(neighborData, domesticCiv, foreignCiv)
            );

            MockGrid.Setup(grid => grid.GetNeighbors(locationOfTestUnit)).Returns(neighbors.ToList());

            var healingLogic = Container.Resolve<UnitHealingLogic>();

            healingLogic.PerformHealingOnUnit(unitToTest);

            return unitToTest.CurrentHitpoints;
        }

        #endregion

        #region utilities

        private ICivilization BuildCivilization() {
            return new Mock<ICivilization>().Object;
        }

        private IHexCell BuildHexCell(
            FocusedHexCellTestData testData, ICivilization domesticCiv, ICivilization foreignCiv
        ){
            var newCell = new Mock<IHexCell>().Object;

            MockCivTerritoryLogic.Setup(logic => logic.GetCivClaimingCell(newCell))
                                 .Returns(testData.OwnedByDomesticCiv ? domesticCiv : foreignCiv);

            if(testData.HasCity) {
                BuildCity(newCell);
            }

            return newCell;
        }

        private IHexCell BuildHexCell(
            NeighboringHexCellTestData testData, ICivilization domesticCiv, ICivilization foreignCiv
        ){
            var newCell = new Mock<IHexCell>().Object;

            var unitsAt = new List<IUnit>();

            foreach(var unitData in testData.UnitsAtCell) {
                unitsAt.Add(BuildUnit(unitData, newCell, unitData.BelongsToDomesticCiv ? domesticCiv : foreignCiv));
            }

            MockUnitPositionCanon.Setup(canon => canon.GetPossessionsOfOwner(newCell)).Returns(unitsAt);

            return newCell;
        }

        private IUnit BuildUnit(UnitTestData testData, IHexCell location, ICivilization owningCiv) {
            var mockUnit = new Mock<IUnit>();

            mockUnit.SetupAllProperties();

            mockUnit.Setup(unit => unit.Type)           .Returns(testData.Type);
            mockUnit.Setup(unit => unit.MaxHitpoints)   .Returns(testData.MaxHitpoints);
            mockUnit.Setup(unit => unit.CurrentMovement).Returns(testData.CurrentMovement);
            mockUnit.Setup(unit => unit.MaxMovement)    .Returns(testData.MaxMovement);

            var newUnit = mockUnit.Object;

            MockUnitPositionCanon  .Setup(canon => canon.GetOwnerOfPossession(newUnit)).Returns(location);
            MockUnitPossessionCanon.Setup(canon => canon.GetOwnerOfPossession(newUnit)).Returns(owningCiv);

            MockPromotionParser.Setup(parser => parser.GetHealingInfo(newUnit)).Returns(testData.HealingInfo);

            return newUnit;
        }

        private ICity BuildCity(IHexCell location) {
            var newCity = new Mock<ICity>().Object;

            MockCityLocationCanon.Setup(canon => canon.GetOwnerOfPossession(newCity)).Returns(location);
            MockCityLocationCanon.Setup(canon => canon.GetPossessionsOfOwner(location))
                                 .Returns(new List<ICity>() { newCity });

            return newCity;
        }

        #endregion

        #endregion

    }

}
