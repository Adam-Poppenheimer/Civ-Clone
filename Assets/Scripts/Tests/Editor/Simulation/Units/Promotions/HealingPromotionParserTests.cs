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
using Assets.Simulation.HexMap;

namespace Assets.Tests.Simulation.Units.Promotions {

    [TestFixture]
    public class HealingPromotionParserTests : ZenjectUnitTestFixture {

        #region internal types

        public class ParsePromotionForHealingInfoTestData {

            public PromotionTestData PromotionToTest = new PromotionTestData();

            public UnitTestData UnitToTest = new UnitTestData();

            public HealingInfo HealingInfoToPass = new HealingInfo();

        }

        public class PromotionTestData {

            public bool RequiresForeignTerritory;

            public int AlternativeNavalBaseHealing;

            public int BonusHealingToAdjacent;

            public int BonusHealingToSelf;

            public bool HealsEveryTurn;

        }

        public class UnitTestData {

            public bool LocationBelongsToDomesticCiv;

        }

        #endregion

        #region static fields and properties

        public static IEnumerable ParsePromotionForHealingInfoTestCases {
            get {
                yield return new TestCaseData(new ParsePromotionForHealingInfoTestData() {
                    PromotionToTest = new PromotionTestData() {
                        BonusHealingToSelf = 10
                    },
                    HealingInfoToPass = new HealingInfo() {
                        BonusHealingToSelf = 15
                    }
                }).SetName("Applies BonusHealingToSelf from promotion to HealingInfo").Returns(new HealingInfo() {
                    BonusHealingToSelf = 25
                });

                yield return new TestCaseData(new ParsePromotionForHealingInfoTestData() {
                    PromotionToTest = new PromotionTestData() {
                        BonusHealingToAdjacent = 10
                    },
                    HealingInfoToPass = new HealingInfo() {
                        BonusHealingToAdjacent = 15
                    }
                }).SetName("Applies BonusHealingToAdjacent from promotion to HealingInfo").Returns(new HealingInfo() {
                    BonusHealingToAdjacent = 25
                });

                yield return new TestCaseData(new ParsePromotionForHealingInfoTestData() {
                    PromotionToTest = new PromotionTestData() {
                        HealsEveryTurn = true
                    },
                }).SetName("Applies HealsEveryTurn from promotion to HealingInfo").Returns(new HealingInfo() {
                    HealsEveryTurn = true
                });

                yield return new TestCaseData(new ParsePromotionForHealingInfoTestData() {
                    PromotionToTest = new PromotionTestData() {
                        AlternativeNavalBaseHealing = 15
                    },
                    HealingInfoToPass = new HealingInfo() {
                        AlternateNavalBaseHealing = 10
                    }
                }).SetName("Applies AlternateNavalBaseHealing from promotion to if larger than number in HealingInfo").Returns(new HealingInfo() {
                    AlternateNavalBaseHealing = 15
                });

                yield return new TestCaseData(new ParsePromotionForHealingInfoTestData() {
                    PromotionToTest = new PromotionTestData() {
                        AlternativeNavalBaseHealing = 5
                    },
                    HealingInfoToPass = new HealingInfo() {
                        AlternateNavalBaseHealing = 10
                    }
                }).SetName("Does not apply AlternateNavalBaseHealing from promotion to if smaller than number in HealingInfo").Returns(new HealingInfo() {
                    AlternateNavalBaseHealing = 10
                });



                yield return new TestCaseData(new ParsePromotionForHealingInfoTestData() {
                    PromotionToTest = new PromotionTestData() {
                        BonusHealingToSelf = 10, RequiresForeignTerritory = true
                    },
                    UnitToTest = new UnitTestData() { LocationBelongsToDomesticCiv = true }
                }).SetName("Promotion requires foreign territory and unit not in foreign territory | promotion not applied").Returns(new HealingInfo() {
                    BonusHealingToSelf = 0
                });

                yield return new TestCaseData(new ParsePromotionForHealingInfoTestData() {
                    PromotionToTest = new PromotionTestData() {
                        BonusHealingToSelf = 10, RequiresForeignTerritory = false
                    },
                    UnitToTest = new UnitTestData() { LocationBelongsToDomesticCiv = true }
                }).SetName("Promotion does not require foreign territory and unit not in foreign territory | promotion applied").Returns(new HealingInfo() {
                    BonusHealingToSelf = 10
                });

                yield return new TestCaseData(new ParsePromotionForHealingInfoTestData() {
                    PromotionToTest = new PromotionTestData() {
                        BonusHealingToSelf = 10, RequiresForeignTerritory = false
                    },
                    UnitToTest = new UnitTestData() { LocationBelongsToDomesticCiv = false }
                }).SetName("Promotion does not require foreign territory and unit in foreign territory | promotion applied").Returns(new HealingInfo() {
                    BonusHealingToSelf = 10
                });

                yield return new TestCaseData(new ParsePromotionForHealingInfoTestData() {
                    PromotionToTest = new PromotionTestData() {
                        BonusHealingToSelf = 10, RequiresForeignTerritory = true
                    },
                    UnitToTest = new UnitTestData() { LocationBelongsToDomesticCiv = false }
                }).SetName("Promotion requires foreign territory and unit in foreign territory | promotion applied").Returns(new HealingInfo() {
                    BonusHealingToSelf = 10
                });
            }
        }

        #endregion

        #region instance fields and properties

        private Mock<IUnitPositionCanon>                            MockUnitPositionCanon;
        private Mock<IPossessionRelationship<ICivilization, IUnit>> MockUnitPossessionCanon;
        private Mock<ICivilizationTerritoryLogic>                   MockCivTerritoryLogic;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockUnitPositionCanon   = new Mock<IUnitPositionCanon>();
            MockUnitPossessionCanon = new Mock<IPossessionRelationship<ICivilization, IUnit>>();
            MockCivTerritoryLogic   = new Mock<ICivilizationTerritoryLogic>();

            Container.Bind<IUnitPositionCanon>                           ().FromInstance(MockUnitPositionCanon  .Object);
            Container.Bind<IPossessionRelationship<ICivilization, IUnit>>().FromInstance(MockUnitPossessionCanon.Object);
            Container.Bind<ICivilizationTerritoryLogic>                  ().FromInstance(MockCivTerritoryLogic  .Object);

            Container.Bind<HealingPromotionParser>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        [TestCaseSource("ParsePromotionForHealingInfoTestCases")]
        public HealingInfo ParsePromotionForHealingInfoTests(ParsePromotionForHealingInfoTestData testData) {
            var promotionToTest = BuildPromotion(testData.PromotionToTest);

            var domesticCiv = BuildCivilization("Domestic Civ");
            var foreignCiv  = BuildCivilization("Foreign Civ");

            var unitLocation = BuildHexCell(testData.UnitToTest.LocationBelongsToDomesticCiv ? domesticCiv : foreignCiv);

            var unitToTest = BuildUnit(unitLocation, domesticCiv);

            var healingParser = Container.Resolve<HealingPromotionParser>();

            healingParser.ParsePromotionForHealingInfo(promotionToTest, unitToTest, testData.HealingInfoToPass);

            return testData.HealingInfoToPass;
        }

        #endregion

        #region utilities

        private IPromotion BuildPromotion(PromotionTestData testData) {
            var mockPromotion = new Mock<IPromotion>();

            mockPromotion.Setup(promotion => promotion.RequiresForeignTerritory)   .Returns(testData.RequiresForeignTerritory);
            mockPromotion.Setup(promotion => promotion.AlternativeNavalBaseHealing).Returns(testData.AlternativeNavalBaseHealing);
            mockPromotion.Setup(promotion => promotion.BonusHealingToSelf)         .Returns(testData.BonusHealingToSelf);
            mockPromotion.Setup(promotion => promotion.BonusHealingToAdjacent)     .Returns(testData.BonusHealingToAdjacent);
            mockPromotion.Setup(promotion => promotion.HealsEveryTurn)             .Returns(testData.HealsEveryTurn);

            return mockPromotion.Object;
        }

        private ICivilization BuildCivilization(string name) {
            var mockCiv = new Mock<ICivilization>();

            mockCiv.Name = name;

            return mockCiv.Object;
        }

        private IHexCell BuildHexCell(ICivilization owner) {
            var newCell = new Mock<IHexCell>().Object;

            MockCivTerritoryLogic.Setup(logic => logic.GetCivClaimingCell(newCell)).Returns(owner);

            return newCell;
        }

        private IUnit BuildUnit(IHexCell location, ICivilization owner) {
            var newUnit = new Mock<IUnit>().Object;

            MockUnitPositionCanon  .Setup(canon => canon.GetOwnerOfPossession(newUnit)).Returns(location);
            MockUnitPossessionCanon.Setup(canon => canon.GetOwnerOfPossession(newUnit)).Returns(owner);

            return newUnit;
        }

        #endregion

        #endregion

    }

}
