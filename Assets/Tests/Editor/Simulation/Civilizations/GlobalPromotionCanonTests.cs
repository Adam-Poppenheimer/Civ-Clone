using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using Zenject;
using Moq;

using Assets.Simulation;
using Assets.Simulation.Civilizations;
using Assets.Simulation.Units;
using Assets.Simulation.Units.Promotions;

namespace Assets.Tests.Simulation.Civilizations {

    public class GlobalPromotionCanonTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IPossessionRelationship<ICivilization, IUnit>> MockUnitPossessionCanon;
        private CivilizationSignals                                 CivSignals;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockUnitPossessionCanon = new Mock<IPossessionRelationship<ICivilization, IUnit>>();
            CivSignals = new CivilizationSignals();

            Container.Bind<IPossessionRelationship<ICivilization, IUnit>>().FromInstance(MockUnitPossessionCanon.Object);
            Container.Bind<CivilizationSignals>                          ().FromInstance(CivSignals);

            Container.Bind<GlobalPromotionCanon>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void AddGlobalPromotionToCiv_ReflectedInGetGlobalPromotionsOfCiv() {
            var promotionOne = BuildPromotion();
            var promotionTwo = BuildPromotion();

            var civ = BuildCiv();

            var promotionCanon = Container.Resolve<GlobalPromotionCanon>();

            promotionCanon.AddGlobalPromotionToCiv(promotionOne, civ);
            promotionCanon.AddGlobalPromotionToCiv(promotionTwo, civ);

            CollectionAssert.AreEquivalent(
                new List<IPromotion>() { promotionOne, promotionTwo },
                promotionCanon.GetGlobalPromotionsOfCiv(civ)
            );
        }

        [Test]
        public void AddGlobalPromotionToCiv_PromotionAppendedToCivsUnits() {
            Mock<IPromotionTree> mockTreeOne, mockTreeTwo;

            var unitOne = BuildUnit(BuildTree(out mockTreeOne));
            var unitTwo = BuildUnit(BuildTree(out mockTreeTwo));

            var civ = BuildCiv(unitOne, unitTwo);

            var promotion = BuildPromotion();

            var promotionCanon = Container.Resolve<GlobalPromotionCanon>();

            promotionCanon.AddGlobalPromotionToCiv(promotion, civ);

            mockTreeOne.Verify(tree => tree.AppendPromotion(promotion), Times.Once, "Promotion not appended to tree of UnitOne as expected");
            mockTreeTwo.Verify(tree => tree.AppendPromotion(promotion), Times.Once, "Promotion not appended to tree of UnitTwo as expected");
        }

        [Test]
        public void RemoveGlobalPromotionFromCiv_ReflectedInGetGlobalPromotionsOfCiv() {
            var promotionOne = BuildPromotion();
            var promotionTwo = BuildPromotion();

            var civ = BuildCiv();

            var promotionCanon = Container.Resolve<GlobalPromotionCanon>();

            promotionCanon.AddGlobalPromotionToCiv(promotionOne, civ);
            promotionCanon.AddGlobalPromotionToCiv(promotionTwo, civ);

            promotionCanon.RemoveGlobalPromotionFromCiv(promotionOne, civ);

            CollectionAssert.AreEquivalent(
                new List<IPromotion>() { promotionTwo },
                promotionCanon.GetGlobalPromotionsOfCiv(civ)
            );
        }

        [Test]
        public void RemoveGlobalPromotionFromCiv_PromotionRemovedFromCivsUnits() {
            Mock<IPromotionTree> mockTreeOne, mockTreeTwo;

            var unitOne = BuildUnit(BuildTree(out mockTreeOne));
            var unitTwo = BuildUnit(BuildTree(out mockTreeTwo));

            var civ = BuildCiv(unitOne, unitTwo);

            var promotion = BuildPromotion();

            var promotionCanon = Container.Resolve<GlobalPromotionCanon>();

            promotionCanon.AddGlobalPromotionToCiv     (promotion, civ);
            promotionCanon.RemoveGlobalPromotionFromCiv(promotion, civ);

            mockTreeOne.Verify(tree => tree.RemoveAppendedPromotion(promotion), Times.Once, "Promotion not removed from tree of UnitOne as expected");
            mockTreeTwo.Verify(tree => tree.RemoveAppendedPromotion(promotion), Times.Once, "Promotion not removed from tree of UnitTwo as expected");
        }

        [Test]
        public void OnCivGainedUnit_UnitGivenCivsGlobalPromotions() {
            Mock<IPromotionTree> mockTreeOne;

            var unitOne = BuildUnit(BuildTree(out mockTreeOne));

            var promotionOne = BuildPromotion();
            var promotionTwo = BuildPromotion();

            var civ = BuildCiv();

            var promotionCanon = Container.Resolve<GlobalPromotionCanon>();

            promotionCanon.AddGlobalPromotionToCiv(promotionOne, civ);
            promotionCanon.AddGlobalPromotionToCiv(promotionTwo, civ);

            CivSignals.CivGainedUnitSignal.OnNext(new UniRx.Tuple<ICivilization, IUnit>(civ, unitOne));

            mockTreeOne.Verify(tree => tree.AppendPromotion(promotionOne), Times.Once, "PromotionOne not appended as expected");
            mockTreeOne.Verify(tree => tree.AppendPromotion(promotionTwo), Times.Once, "PromotionTwo not appended as expected");
        }

        [Test]
        public void OnCivLostUnit_UnitStrippedOfCivsGlobalPromotions() {
            Mock<IPromotionTree> mockTreeOne;

            var unitOne = BuildUnit(BuildTree(out mockTreeOne));

            var promotionOne = BuildPromotion();
            var promotionTwo = BuildPromotion();

            var civ = BuildCiv();

            var promotionCanon = Container.Resolve<GlobalPromotionCanon>();

            promotionCanon.AddGlobalPromotionToCiv(promotionOne, civ);
            promotionCanon.AddGlobalPromotionToCiv(promotionTwo, civ);

            CivSignals.CivLostUnitSignal.OnNext(new UniRx.Tuple<ICivilization, IUnit>(civ, unitOne));

            mockTreeOne.Verify(tree => tree.RemoveAppendedPromotion(promotionOne), Times.Once, "PromotionOne not removed as expected");
            mockTreeOne.Verify(tree => tree.RemoveAppendedPromotion(promotionTwo), Times.Once, "PromotionTwo not removed as expected");
        }

        #endregion

        #region utilities

        private IPromotionTree BuildTree(out Mock<IPromotionTree> mock) {
            mock = new Mock<IPromotionTree>();

            return mock.Object;
        }

        private IUnit BuildUnit(IPromotionTree tree) {
            var mockUnit = new Mock<IUnit>();

            mockUnit.Setup(unit => unit.PromotionTree).Returns(tree);

            return mockUnit.Object;
        }

        private IPromotion BuildPromotion() {
            return new Mock<IPromotion>().Object;
        }

        private ICivilization BuildCiv(params IUnit[] units) {
            var newCiv = new Mock<ICivilization>().Object;

            MockUnitPossessionCanon.Setup(canon => canon.GetPossessionsOfOwner(newCiv)).Returns(units);

            return newCiv;
        }

        #endregion

        #endregion

    }

}
