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

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockCombatParser   = new Mock<ICombatPromotionParser>();
            MockMovementParser = new Mock<IMovementPromotionParser>();
            MockHealingParser  = new Mock<IHealingPromotionParser>();

            Container.Bind<ICombatPromotionParser>  ().FromInstance(MockCombatParser  .Object);
            Container.Bind<IMovementPromotionParser>().FromInstance(MockMovementParser.Object);
            Container.Bind<IHealingPromotionParser> ().FromInstance(MockHealingParser .Object);

            Container.Bind<PromotionParser>().AsSingle();
        }

        #endregion

        /*#region tests

        [Test]
        public void GetCombatInfo_AttackerPromotionsPassedToCombatParserCorrectly() {
            var attackerPromotions = new List<IPromotion>() {
                BuildPromotion("Promotion One"),
                BuildPromotion("Promotion Two"),
            };

            var defenderPromotions = new List<IPromotion>() {
                BuildPromotion("Promotion Three"),
                BuildPromotion("Promotion Four"),
            };

            var attacker = BuildUnit(attackerPromotions);
            var defender = BuildUnit(defenderPromotions);

            var location = BuildHexCell();

            var promotionParser = Container.Resolve<PromotionParser>();

            var returnedInfo = promotionParser.GetCombatInfo(attacker, defender, location, CombatType.Ranged);

            MockCombatParser.Verify(
                parser => parser.ParsePromotionForAttacker(attackerPromotions[0], attacker, defender, location, returnedInfo),
                Times.Once, "CombatParser.ParsePromotionForAttacker wasn't called as expected on attackerPromotions[0]"
            );

            MockCombatParser.Verify(
                parser => parser.ParsePromotionForAttacker(attackerPromotions[1], attacker, defender, location, returnedInfo),
                Times.Once, "CombatParser.ParsePromotionForAttacker wasn't called as expected on attackerPromotions[1]"
            );
        }

        [Test]
        public void GetCombatInfo_DefenderPromotionsPassedToCombatParserCorrectly() {
            var attackerPromotions = new List<IPromotion>() {
                BuildPromotion("Promotion One"),
                BuildPromotion("Promotion Two"),
            };

            var defenderPromotions = new List<IPromotion>() {
                BuildPromotion("Promotion Three"),
                BuildPromotion("Promotion Four"),
            };

            var attacker = BuildUnit(attackerPromotions);
            var defender = BuildUnit(defenderPromotions);

            var location = BuildHexCell();

            var promotionParser = Container.Resolve<PromotionParser>();

            var returnedInfo = promotionParser.GetCombatInfo(attacker, defender, location, CombatType.Ranged);

            MockCombatParser.Verify(
                parser => parser.ParsePromotionForDefender(defenderPromotions[0], attacker, defender, location, returnedInfo),
                Times.Once, "CombatParser.ParsePromotionForDefender wasn't called as expected on defenderPromotions[0]"
            );

            MockCombatParser.Verify(
                parser => parser.ParsePromotionForDefender(defenderPromotions[1], attacker, defender, location, returnedInfo),
                Times.Once, "CombatParser.ParsePromotionForDefender wasn't called as expected on defenderPromotions[1]"
            );
        }

        [Test]
        public void GetMovementInfo_CorrectArgumentsPassedToEveryPromotion() {
            var promotions = new List<IPromotion>() {
                BuildPromotion("Promotion One"),
                BuildPromotion("Promotion Two"),
            };

            var promotionParser = Container.Resolve<PromotionParser>();

            var movementSummary = new UnitMovementSummary();

            promotionParser.SetMovementSummary(movementSummary, promotions);

            MockMovementParser.Verify(
                parser => parser.AddPromotionToMovementSummary(promotions[0], movementSummary),
                Times.Once, "MovementParser.AddPromotionToMovementSummary wasn't called as expected on promotions[0]"
            );

            MockMovementParser.Verify(
                parser => parser.AddPromotionToMovementSummary(promotions[1], movementSummary),
                Times.Once, "MovementParser.AddPromotionToMovementSummary wasn't called as expected on promotions[1]"
            );
        }

        [Test]
        public void GetHealingInfo_CorrectArgumentsPassedToEveryPromotion() {
            var promotions = new List<IPromotion>() {
                BuildPromotion("Promotion One"),
                BuildPromotion("Promotion Two"),
            };

            var unit = BuildUnit(promotions);

            var promotionParser = Container.Resolve<PromotionParser>();

            var returnedInfo = promotionParser.GetHealingInfo(unit);

            MockHealingParser.Verify(
                parser => parser.ParsePromotionForHealingInfo(promotions[0], unit, returnedInfo),
                Times.Once, "HealingParser.ParsePromotionForHealingInfo wasn't called as expected on promotions[0]"
            );

            MockHealingParser.Verify(
                parser => parser.ParsePromotionForHealingInfo(promotions[1], unit, returnedInfo),
                Times.Once, "HealingParser.ParsePromotionForHealingInfo wasn't called as expected on promotions[1]"
            );
        }

        #endregion*/

        #region utilities

        private IPromotion BuildPromotion(string name) {
            var promotionMock = new Mock<IPromotion>();

            promotionMock.Name = name;
            promotionMock.Setup(promotion => promotion.name).Returns(name);

            return promotionMock.Object;
        }

        private IUnit BuildUnit(IEnumerable<IPromotion> promotions) {
            var mockUnit = new Mock<IUnit>();

            mockUnit.Setup(unit => unit.Promotions).Returns(promotions);

            return mockUnit.Object;
        }

        private IHexCell BuildHexCell() {
            return new Mock<IHexCell>().Object;
        }

        #endregion

        #endregion

    }

}
