using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation.Units.Combat;
using Assets.Simulation.Units.Promotions;

namespace Assets.Tests.Simulation.Units.Promotions {

    [TestFixture]
    public class CombatPromotionParserTests : ZenjectUnitTestFixture {

        #region instance fields and properties



        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            Container.Bind<CombatPromotionParser>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void AddPromotionToCombatSummary_SetsCanMoveAfterAttackingIfTrue() {
            var promotion = BuildPromotion(canMoveAfterAttacking: true);

            var combatSummary = new UnitCombatSummary();

            var parser = Container.Resolve<CombatPromotionParser>();

            parser.AddPromotionToCombatSummary(promotion, combatSummary);

            Assert.IsTrue(combatSummary.CanMoveAfterAttacking);
        }

        [Test]
        public void AddPromotionToCombatSummary_SetsCanAttackAfterAttackingIfTrue() {
            var promotion = BuildPromotion(canAttackAfterAttacking: true);

            var combatSummary = new UnitCombatSummary();

            var parser = Container.Resolve<CombatPromotionParser>();

            parser.AddPromotionToCombatSummary(promotion, combatSummary);

            Assert.IsTrue(combatSummary.CanAttackAfterAttacking);
        }

        [Test]
        public void AddPromotionToCombatSummary_SetsIgnoresAmphibiousPenaltyIfTrue() {
            var promotion = BuildPromotion(ignoresAmphibiousPenalty: true);

            var combatSummary = new UnitCombatSummary();

            var parser = Container.Resolve<CombatPromotionParser>();

            parser.AddPromotionToCombatSummary(promotion, combatSummary);

            Assert.IsTrue(combatSummary.IgnoresAmphibiousPenalty);
        }

        [Test]
        public void AddPromotionToCombatSummary_SetsIgnoresDefensiveTerrainBonusIfTrue() {
            var promotion = BuildPromotion(ignoresDefensiveTerrainBonus: true);

            var combatSummary = new UnitCombatSummary();

            var parser = Container.Resolve<CombatPromotionParser>();

            parser.AddPromotionToCombatSummary(promotion, combatSummary);

            Assert.IsTrue(combatSummary.IgnoresDefensiveTerrainBonus);
        }

        [Test]
        public void AddPromotionToCombatSummary_SetsIgnoresLineOfSightIfTrue() {
            var promotion = BuildPromotion(ignoresLineOfSight: true);

            var combatSummary = new UnitCombatSummary();

            var parser = Container.Resolve<CombatPromotionParser>();

            parser.AddPromotionToCombatSummary(promotion, combatSummary);

            Assert.IsTrue(combatSummary.IgnoresLineOfSight);
        }

        [Test]
        public void AddPromotionToCombatSummary_FlagsAlreadySetArentTurnedOff() {
            var promotion = BuildPromotion();

            var combatSummary = new UnitCombatSummary() {
                IgnoresLineOfSight = true
            };

            var parser = Container.Resolve<CombatPromotionParser>();

            parser.AddPromotionToCombatSummary(promotion, combatSummary);

            Assert.IsTrue(combatSummary.IgnoresLineOfSight);
        }

        [Test]
        public void AddPromotionToCombatSummary_AddsModifiersWhenAttackingToSummary() {
            var modifiers = new List<ICombatModifier>() {
                BuildCombatModifier(), BuildCombatModifier(), BuildCombatModifier()
            };

            var promotion = BuildPromotion(
                modifiersWhenAttacking:     modifiers,
                modifiersWhenDefending:     new List<ICombatModifier>(),
                auraModifiersWhenAttacking: new List<ICombatModifier>(),
                auraModifiersWhenDefending: new List<ICombatModifier>()
            );

            var combatSummary = new UnitCombatSummary();

            var parser = Container.Resolve<CombatPromotionParser>();

            parser.AddPromotionToCombatSummary(promotion, combatSummary);

            CollectionAssert.AreEquivalent(modifiers, combatSummary.ModifiersWhenAttacking);
        }

        [Test]
        public void AddPromotionToCombatSummary_AddsModifiersWhenDefendingToSummary() {
            var modifiers = new List<ICombatModifier>() {
                BuildCombatModifier(), BuildCombatModifier(), BuildCombatModifier()
            };

            var promotion = BuildPromotion(
                modifiersWhenAttacking:     new List<ICombatModifier>(),
                modifiersWhenDefending:     modifiers,
                auraModifiersWhenAttacking: new List<ICombatModifier>(),
                auraModifiersWhenDefending: new List<ICombatModifier>()
            );

            var combatSummary = new UnitCombatSummary();

            var parser = Container.Resolve<CombatPromotionParser>();

            parser.AddPromotionToCombatSummary(promotion, combatSummary);

            CollectionAssert.AreEquivalent(modifiers, combatSummary.ModifiersWhenDefending);
        }

        [Test]
        public void AddPromotionToCombatSummary_AddsAuraModifiersWhenAttackingToSummary() {
            var modifiers = new List<ICombatModifier>() {
                BuildCombatModifier(), BuildCombatModifier(), BuildCombatModifier()
            };

            var promotion = BuildPromotion(
                modifiersWhenAttacking:     new List<ICombatModifier>(),
                modifiersWhenDefending:     new List<ICombatModifier>(),
                auraModifiersWhenAttacking: modifiers,
                auraModifiersWhenDefending: new List<ICombatModifier>()
            );

            var combatSummary = new UnitCombatSummary();

            var parser = Container.Resolve<CombatPromotionParser>();

            parser.AddPromotionToCombatSummary(promotion, combatSummary);

            CollectionAssert.AreEquivalent(modifiers, combatSummary.AuraModifiersWhenAttacking);
        }

        [Test]
        public void AddPromotionToCombatSummary_AddsAuraModifiersWhenDefendingToSummary() {
            var modifiers = new List<ICombatModifier>() {
                BuildCombatModifier(), BuildCombatModifier(), BuildCombatModifier()
            };

            var promotion = BuildPromotion(
                modifiersWhenAttacking:     new List<ICombatModifier>(),
                modifiersWhenDefending:     new List<ICombatModifier>(),
                auraModifiersWhenAttacking: new List<ICombatModifier>(),
                auraModifiersWhenDefending: modifiers
            );

            var combatSummary = new UnitCombatSummary();

            var parser = Container.Resolve<CombatPromotionParser>();

            parser.AddPromotionToCombatSummary(promotion, combatSummary);

            CollectionAssert.AreEquivalent(modifiers, combatSummary.AuraModifiersWhenDefending);
        }

        #endregion

        #region utilities

        private IPromotion BuildPromotion(
            List<ICombatModifier> modifiersWhenAttacking, List<ICombatModifier> modifiersWhenDefending,
            List<ICombatModifier> auraModifiersWhenAttacking, List<ICombatModifier> auraModifiersWhenDefending
        ) {
            return BuildPromotion(
                false, false, false, false, false,
                modifiersWhenAttacking, modifiersWhenDefending,
                auraModifiersWhenAttacking, auraModifiersWhenDefending
            );
        }

        private IPromotion BuildPromotion(
            bool canMoveAfterAttacking        = false,
            bool canAttackAfterAttacking      = false,
            bool ignoresAmphibiousPenalty     = false,
            bool ignoresDefensiveTerrainBonus = false,
            bool ignoresLineOfSight           = false
        ) {
            return BuildPromotion(
                canMoveAfterAttacking, canAttackAfterAttacking, ignoresAmphibiousPenalty,
                ignoresDefensiveTerrainBonus, ignoresLineOfSight, new List<ICombatModifier>(),
                new List<ICombatModifier>(), new List<ICombatModifier>(), new List<ICombatModifier>()
            );
        }

        private IPromotion BuildPromotion(            
            bool canMoveAfterAttacking, bool canAttackAfterAttacking,
            bool ignoresAmphibiousPenalty, bool ignoresDefensiveTerrainBonus,
            bool ignoresLineOfSight, List<ICombatModifier> modifiersWhenAttacking,
            List<ICombatModifier> modifiersWhenDefending,
            List<ICombatModifier> auraModifiersWhenAttacking,
            List<ICombatModifier> auraModifiersWhenDefending
        ) {
            var mockPromotion = new Mock<IPromotion>();

            mockPromotion.Setup(promotion => promotion.CanMoveAfterAttacking)         .Returns(canMoveAfterAttacking);
            mockPromotion.Setup(promotion => promotion.CanAttackAfterAttacking)       .Returns(canAttackAfterAttacking);
            mockPromotion.Setup(promotion => promotion.IgnoresAmphibiousPenalty)      .Returns(ignoresAmphibiousPenalty);
            mockPromotion.Setup(promotion => promotion.IgnoresDefensiveTerrainBonuses).Returns(ignoresDefensiveTerrainBonus);
            mockPromotion.Setup(promotion => promotion.IgnoresLineOfSight)            .Returns(ignoresLineOfSight);
            mockPromotion.Setup(promotion => promotion.ModifiersWhenAttacking)        .Returns(modifiersWhenAttacking);
            mockPromotion.Setup(promotion => promotion.ModifiersWhenDefending)        .Returns(modifiersWhenDefending);
            mockPromotion.Setup(promotion => promotion.AuraModifiersWhenAttacking)    .Returns(auraModifiersWhenAttacking);
            mockPromotion.Setup(promotion => promotion.AuraModifiersWhenDefending)    .Returns(auraModifiersWhenDefending);

            return mockPromotion.Object;
        }

        private ICombatModifier BuildCombatModifier() {
            return new Mock<ICombatModifier>().Object;
        }

        #endregion

        #endregion

    }

}
