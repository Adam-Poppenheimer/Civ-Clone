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

        

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            Container.Bind<PromotionParser>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void GetCombatInfo_CorrectArgumentsPassedToAttackerPromotions() {
            Mock<IPromotion> mockPromotionOne, mockPromotionTwo, mockPromotionThree, mockPromotionFour;

            var attackerPromotions = new List<IPromotion>() {
                BuildPromotion("Promotion One", out mockPromotionOne),
                BuildPromotion("Promotion Two", out mockPromotionTwo),
            };

            var defenderPromotions = new List<IPromotion>() {
                BuildPromotion("Promotion Three", out mockPromotionThree),
                BuildPromotion("Promotion Four",  out mockPromotionFour),
            };

            var attacker = BuildUnit(attackerPromotions);
            var defender = BuildUnit(defenderPromotions);

            var location = BuildHexCell();

            var parser = Container.Resolve<PromotionParser>();

            var returnedInfo = parser.GetCombatInfo(attacker, defender, location, CombatType.Ranged);

            mockPromotionOne.Verify(
                promotion => promotion.ModifyCombatInfoForAttacker(
                    attacker, defender, location, CombatType.Ranged, returnedInfo
                ), Times.Once, "PromotionOne.ModifyCombatInfoForAttacker wasn't called as expected"
            );

            mockPromotionTwo.Verify(
                promotion => promotion.ModifyCombatInfoForAttacker(
                    attacker, defender, location, CombatType.Ranged, returnedInfo
                ), Times.Once, "PromotionTwo.ModifyCombatInfoForAttacker wasn't called as expected"
            );

            mockPromotionOne.Verify(
                promotion => promotion.ModifyCombatInfoForDefender(
                    attacker, defender, location, CombatType.Ranged, returnedInfo
                ), Times.Never, "PromotionOne.ModifyCombatInfoForDefender was called unexpectedly"
            );

            mockPromotionTwo.Verify(
                promotion => promotion.ModifyCombatInfoForDefender(
                    attacker, defender, location, CombatType.Ranged, returnedInfo
                ), Times.Never, "PromotionTwo.ModifyCombatInfoForDefender was called unexpectedly"
            );
        }

        [Test]
        public void GetCombatInfo_CorrectArgumentsPassedToDefenderPromotions() {
            Mock<IPromotion> mockPromotionOne, mockPromotionTwo, mockPromotionThree, mockPromotionFour;

            var attackerPromotions = new List<IPromotion>() {
                BuildPromotion("Promotion One", out mockPromotionOne),
                BuildPromotion("Promotion Two", out mockPromotionTwo),
            };

            var defenderPromotions = new List<IPromotion>() {
                BuildPromotion("Promotion Three", out mockPromotionThree),
                BuildPromotion("Promotion Four",  out mockPromotionFour),
            };

            var attacker = BuildUnit(attackerPromotions);
            var defender = BuildUnit(defenderPromotions);

            var location = BuildHexCell();

            var parser = Container.Resolve<PromotionParser>();

            var returnedInfo = parser.GetCombatInfo(attacker, defender, location, CombatType.Ranged);

            mockPromotionThree.Verify(
                promotion => promotion.ModifyCombatInfoForAttacker(
                    attacker, defender, location, CombatType.Ranged, returnedInfo
                ), Times.Never, "PromotionThree.ModifyCombatInfoForAttacker was called unexpectedly"
            );

            mockPromotionFour.Verify(
                promotion => promotion.ModifyCombatInfoForAttacker(
                    attacker, defender, location, CombatType.Ranged, returnedInfo
                ), Times.Never, "PromotionFour.ModifyCombatInfoForAttacker was called unexpectedly"
            );

            mockPromotionThree.Verify(
                promotion => promotion.ModifyCombatInfoForDefender(
                    attacker, defender, location, CombatType.Ranged, returnedInfo
                ), Times.Once, "PromotionThree.ModifyCombatInfoForDefender wasn't called as expected"
            );

            mockPromotionFour.Verify(
                promotion => promotion.ModifyCombatInfoForDefender(
                    attacker, defender, location, CombatType.Ranged, returnedInfo
                ), Times.Once, "PromotionFour.ModifyCombatInfoForDefender wasn't called as expected"
            );
        }

        [Test]
        public void GetMovementInfo_CorrectArgumentsPassedToEveryPromotion() {
            Mock<IPromotion> mockPromotionOne, mockPromotionTwo;

            var unit = BuildUnit(new List<IPromotion>() {
                BuildPromotion("Promotion One", out mockPromotionOne),
                BuildPromotion("Promotion Two", out mockPromotionTwo),
            });

            var parser = Container.Resolve<PromotionParser>();

            var returnedInfo = parser.GetMovementInfo(unit);

            mockPromotionOne.Verify(
                promotion => promotion.ModifyMovementInfo(unit, returnedInfo),
                Times.Once, "PromotionOne.ModifyMovementInfo wasn't called as expected"
            );

            mockPromotionTwo.Verify(
                promotion => promotion.ModifyMovementInfo(unit, returnedInfo),
                Times.Once, "PromotionTwo.ModifyMovementInfo wasn't called as expected"
            );
        }

        #endregion

        #region utilities

        private IPromotion BuildPromotion(string name, out Mock<IPromotion> mock) {
            mock = new Mock<IPromotion>();

            mock.Name = name;
            mock.Setup(promotion => promotion.name).Returns(name);

            return mock.Object;
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
