using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation.Units;
using Assets.Simulation.Units.Combat;
using Assets.Simulation.Units.Promotions;
using Assets.Simulation.HexMap;

namespace Assets.Tests.Simulation.Units.Promotions {

    public class PromotionParserTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<ICombatPromotionParser>   MockCombatParser;
        private Mock<IMovementPromotionParser> MockMovementParser;
        private Mock<IHealingPromotionParser>  MockHealingParser;
        private Mock<IUnitPromotionLogic>      MockUnitPromotionLogic;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockCombatParser       = new Mock<ICombatPromotionParser>();
            MockMovementParser     = new Mock<IMovementPromotionParser>();
            MockHealingParser      = new Mock<IHealingPromotionParser>();
            MockUnitPromotionLogic = new Mock<IUnitPromotionLogic>();
            
            Container.Bind<ICombatPromotionParser>  ().FromInstance(MockCombatParser      .Object);
            Container.Bind<IMovementPromotionParser>().FromInstance(MockMovementParser    .Object);
            Container.Bind<IHealingPromotionParser> ().FromInstance(MockHealingParser     .Object);
            Container.Bind<IUnitPromotionLogic>     ().FromInstance(MockUnitPromotionLogic.Object);

            Container.Bind<PromotionParser>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void SetCombatSummary_ResetsSummary() {
            var summary = new UnitCombatSummary();

            summary.CanMoveAfterAttacking   = true;
            summary.CanAttackAfterAttacking = true;
            summary.BonusRange = 2;

            var promotions = new List<IPromotion>();

            var promotionParser = Container.Resolve<PromotionParser>();

            promotionParser.SetCombatSummary(summary, promotions);

            Assert.IsFalse(summary.CanMoveAfterAttacking);
            Assert.IsFalse(summary.CanAttackAfterAttacking);
            Assert.AreEqual(0, summary.BonusRange);
        }

        [Test]
        public void SetCombatSummary_PassesSummaryAndEachPromotionIntoCombatParser() {
            var summary = new UnitCombatSummary();

            var promotions = new List<IPromotion>() {
                BuildPromotion(), BuildPromotion(), BuildPromotion(),
                BuildPromotion(), BuildPromotion(), BuildPromotion()
            };

            var promotionParser = Container.Resolve<PromotionParser>();

            promotionParser.SetCombatSummary(summary, promotions);

            foreach(var promotion in promotions) {
                MockCombatParser.Verify(
                    parser => parser.AddPromotionToCombatSummary(promotion, summary),
                    Times.Once, "Failed to add a promotion to the combat summary"
                );
            }
        }

        [Test]
        public void SetCombatSummary_SetsSummaryOnPromotionsOfUnit() {
            var summary = new UnitCombatSummary();

            var promotions = new List<IPromotion>() {
                BuildPromotion(), BuildPromotion(), BuildPromotion(),
                BuildPromotion(), BuildPromotion(), BuildPromotion()
            };

            var unit = BuildUnit(promotions);

            var promotionParser = Container.Resolve<PromotionParser>();

            promotionParser.SetCombatSummary(summary, unit);

            foreach(var promotion in promotions) {
                MockCombatParser.Verify(
                    parser => parser.AddPromotionToCombatSummary(promotion, summary),
                    Times.Once, "Failed to add a promotion to the combat summary"
                );
            }
        }

        [Test]
        public void SetMovementSummary_ResetsSummary() {
            var summary = new UnitMovementSummary();

            summary.BonusMovement        = 2;
            summary.CanTraverseDeepWater = true;
            summary.CanTraverseLand      = true;

            var promotions = new List<IPromotion>();

            var promotionParser = Container.Resolve<PromotionParser>();

            promotionParser.SetMovementSummary(summary, promotions);

            Assert.AreEqual(0, summary.BonusMovement);
            Assert.IsFalse(summary.CanTraverseDeepWater);
            Assert.IsFalse(summary.CanTraverseLand);
        }

        [Test]
        public void SetMovementSummary_PassesSummaryAndEachPromotionIntoMovementParser() {
            var summary = new UnitMovementSummary();

            var promotions = new List<IPromotion>() {
                BuildPromotion(), BuildPromotion(), BuildPromotion(),
                BuildPromotion(), BuildPromotion(), BuildPromotion()
            };

            var promotionParser = Container.Resolve<PromotionParser>();

            promotionParser.SetMovementSummary(summary, promotions);

            foreach(var promotion in promotions) {
                MockMovementParser.Verify(
                    parser => parser.AddPromotionToMovementSummary(promotion, summary),
                    Times.Once, "Failed to add a promotion to the movement summary"
                );
            }
        }

        [Test]
        public void SetMovementSummary_SetsSummaryOnPromotionsOfUnit() {
            var summary = new UnitMovementSummary();

            var promotions = new List<IPromotion>() {
                BuildPromotion(), BuildPromotion(), BuildPromotion(),
                BuildPromotion(), BuildPromotion(), BuildPromotion()
            };

            var unit = BuildUnit(promotions);

            var promotionParser = Container.Resolve<PromotionParser>();

            promotionParser.SetMovementSummary(summary, unit);

            foreach(var promotion in promotions) {
                MockMovementParser.Verify(
                    parser => parser.AddPromotionToMovementSummary(promotion, summary),
                    Times.Once, "Failed to add a promotion to the movement summary"
                );
            }
        }

        [Test]
        public void GetVisionInfo_ThrowsNotImplementedException() {
            var unit = BuildUnit(new List<IPromotion>());

            var promotionParser = Container.Resolve<PromotionParser>();

            Assert.Throws<NotImplementedException>(() => promotionParser.GetVisionInfo(unit));
        }

        [Test]
        public void GetHealingInfo_ReturnsNonNullInfo() {
            var unit = BuildUnit(new List<IPromotion>());

            var promotionParser = Container.Resolve<PromotionParser>();

            Assert.IsNotNull(promotionParser.GetHealingInfo(unit));
        }

        [Test]
        public void GetHealingInfo_PassesReturnedInfoAndAllUnitPromotionsIntoHealingParser() {
            var promotions = new List<IPromotion>() {
                BuildPromotion(), BuildPromotion(), BuildPromotion(),
                BuildPromotion(), BuildPromotion(), BuildPromotion()
            };

            var unit = BuildUnit(promotions);

            var promotionParser = Container.Resolve<PromotionParser>();

            var healingInfo = promotionParser.GetHealingInfo(unit);

            foreach(var promotion in promotions) {
                MockHealingParser.Verify(
                    parser => parser.ParsePromotionForHealingInfo(promotion, unit, healingInfo),
                    Times.Once, "Failed to parse a promotion for the HealingInfo"
                );
            }
        }

        #endregion

        #region utilities
        
        private IPromotion BuildPromotion() {
            return new Mock<IPromotion>().Object;
        }

        private IUnit BuildUnit(IEnumerable<IPromotion> promotions) {
            var newUnit = new Mock<IUnit>().Object;

            MockUnitPromotionLogic.Setup(logic => logic.GetPromotionsForUnit(newUnit)).Returns(promotions);

            return newUnit;
        }

        #endregion

        #endregion

    }

}
