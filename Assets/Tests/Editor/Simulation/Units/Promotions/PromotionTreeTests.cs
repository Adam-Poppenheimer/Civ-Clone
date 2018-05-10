using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using NUnit.Framework;
using Moq;
using UniRx;

using Assets.Simulation.Units;
using Assets.Simulation.Units.Promotions;

namespace Assets.Tests.Simulation.Units.Promotions {

    [TestFixture]
    public class PromotionTreeTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private UnitSignals UnitSignals;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            UnitSignals = new UnitSignals();
        }

        #endregion

        #region tests

        [Test]
        public void ContainsPromotion_DrivenByPrerequisiteData() {
            var prerequisiteData = new List<IPromotionPrerequisiteData>() {
                BuildPrereqData(BuildPromotion("Promotion One"),   new List<IPromotion>()),
                BuildPrereqData(BuildPromotion("Promotion Two"),   new List<IPromotion>()),
                BuildPrereqData(BuildPromotion("Promotion Three"), new List<IPromotion>()),
                BuildPrereqData(BuildPromotion("Promotion Four"),  new List<IPromotion>()),
            };

            var unit = BuildUnit();

            var promotionTree = new PromotionTree(BuildPromotionTreeData(prerequisiteData), unit, UnitSignals);

            CollectionAssert.AreEquivalent(
                prerequisiteData.Select(data => data.Promotion),
                promotionTree.GetAvailablePromotions()
            );
        }

        [Test]
        public void GetAvailablePromotions_GetsAllPromotionsWithNoPrerequisites() {
            var promotionOne   = BuildPromotion("Promotion One");
            var promotionTwo   = BuildPromotion("Promotion Two");
            var promotionThree = BuildPromotion("Promotion Three");

            var treeData = BuildPromotionTreeData(new List<IPromotionPrerequisiteData>() {
                BuildPrereqData(promotionOne,   new List<IPromotion>()),
                BuildPrereqData(promotionTwo,   new List<IPromotion>()),
                BuildPrereqData(promotionThree, new List<IPromotion>() { BuildPromotion("") } ),
            });

            var unit = BuildUnit();

            var promotionTree = new PromotionTree(treeData, unit, UnitSignals);

            CollectionAssert.AreEquivalent(
                new List<IPromotion>() { promotionOne, promotionTwo },
                promotionTree.GetAvailablePromotions()
            );
        }

        [Test]
        public void GetAvailablePromotions_GetsPromotionsWithSomeChosenPrerequisite() {
            var promotionOne   = BuildPromotion("Promotion One");
            var promotionTwo   = BuildPromotion("Promotion Two");
            var promotionThree = BuildPromotion("Promotion Three");
            var promotionFour  = BuildPromotion("Promotion Four");

            var treeData = BuildPromotionTreeData(new List<IPromotionPrerequisiteData>() {
                BuildPrereqData(promotionOne,   new List<IPromotion>()),
                BuildPrereqData(promotionTwo,   new List<IPromotion>()),
                BuildPrereqData(promotionThree, new List<IPromotion>() { promotionOne } ),
                BuildPrereqData(promotionFour,  new List<IPromotion>() { promotionOne, promotionTwo } ),
            });

            var unit = BuildUnit();

            var promotionTree = new PromotionTree(treeData, unit, UnitSignals);

            promotionTree.ChoosePromotion(promotionOne);

            CollectionAssert.AreEquivalent(
                new List<IPromotion>() { promotionTwo, promotionThree, promotionFour },
                promotionTree.GetAvailablePromotions()
            );
        }

        [Test]
        public void GetAvailablePromotions_ExcludesPromotionsWithNoChosenPrerequisite() {
            var promotionOne   = BuildPromotion("Promotion One");
            var promotionTwo   = BuildPromotion("Promotion Two");
            var promotionThree = BuildPromotion("Promotion Three");

            var treeData = BuildPromotionTreeData(new List<IPromotionPrerequisiteData>() {
                BuildPrereqData(promotionOne,   new List<IPromotion>()),
                BuildPrereqData(promotionTwo,   new List<IPromotion>()),
                BuildPrereqData(promotionThree, new List<IPromotion>() { promotionOne } ),
            });

            var unit = BuildUnit();

            var promotionTree = new PromotionTree(treeData, unit, UnitSignals);

            CollectionAssert.DoesNotContain(promotionTree.GetAvailablePromotions(), promotionThree);
        }

        [Test]
        public void GetAvailablePromotions_ExcludesChosenPromotions() {
            var promotionOne   = BuildPromotion("Promotion One");
            var promotionTwo   = BuildPromotion("Promotion Two");
            var promotionThree = BuildPromotion("Promotion Three");

            var treeData = BuildPromotionTreeData(new List<IPromotionPrerequisiteData>() {
                BuildPrereqData(promotionOne,   new List<IPromotion>()),
                BuildPrereqData(promotionTwo,   new List<IPromotion>()),
                BuildPrereqData(promotionThree, new List<IPromotion>()),
            });

            var unit = BuildUnit();

            var promotionTree = new PromotionTree(treeData, unit, UnitSignals);

            promotionTree.ChoosePromotion(promotionOne);
            promotionTree.ChoosePromotion(promotionTwo);

            CollectionAssert.AreEquivalent(
                new List<IPromotion>() { promotionThree },
                promotionTree.GetAvailablePromotions()
            );
        }

        [Test]
        public void CanChoosePromotion_TrueIfPromotionIsAvailable() {
            var promotionOne   = BuildPromotion("Promotion One");
            var promotionTwo   = BuildPromotion("Promotion Two");
            var promotionThree = BuildPromotion("Promotion Three");

            var treeData = BuildPromotionTreeData(new List<IPromotionPrerequisiteData>() {
                BuildPrereqData(promotionOne,   new List<IPromotion>()),
                BuildPrereqData(promotionTwo,   new List<IPromotion>()),
                BuildPrereqData(promotionThree, new List<IPromotion>() { BuildPromotion("") } ),
            });

            var unit = BuildUnit();

            var promotionTree = new PromotionTree(treeData, unit, UnitSignals);

            Assert.IsTrue(
                promotionTree.CanChoosePromotion(promotionOne),
                "CanChoosePromotion(promotionOne) returned an unexpected value"
            );

            Assert.IsTrue(
                promotionTree.CanChoosePromotion(promotionTwo),
                "CanChoosePromotion(promotionTwo) returned an unexpected value"
            );
        }

        [Test]
        public void CanChoosePromotion_FalseIfPromotionIsNotAvailable() {
            var promotionOne   = BuildPromotion("Promotion One");
            var promotionTwo   = BuildPromotion("Promotion Two");
            var promotionThree = BuildPromotion("Promotion Three");

            var treeData = BuildPromotionTreeData(new List<IPromotionPrerequisiteData>() {
                BuildPrereqData(promotionOne,   new List<IPromotion>()),
                BuildPrereqData(promotionTwo,   new List<IPromotion>()),
                BuildPrereqData(promotionThree, new List<IPromotion>() { BuildPromotion("") } ),
            });

            var unit = BuildUnit();

            var promotionTree = new PromotionTree(treeData, unit, UnitSignals);

            Assert.IsFalse(
                promotionTree.CanChoosePromotion(promotionThree),
                "CanChoosePromotion(promotionThree) returned an unexpected value"
            );
        }

        [Test]
        public void ChoosePromotion_ReflectedInGetChosenPromotions() {
            var promotionOne   = BuildPromotion("Promotion One");
            var promotionTwo   = BuildPromotion("Promotion Two");

            var treeData = BuildPromotionTreeData(new List<IPromotionPrerequisiteData>() {
                BuildPrereqData(promotionOne, new List<IPromotion>()),
                BuildPrereqData(promotionTwo, new List<IPromotion>())
            });

            var unit = BuildUnit();

            var promotionTree = new PromotionTree(treeData, unit, UnitSignals);

            promotionTree.ChoosePromotion(promotionOne);

            CollectionAssert.AreEquivalent(
                new List<IPromotion>() { promotionOne },
                promotionTree.GetChosenPromotions()
            );
        }

        [Test]
        public void ChoosePromotion_UnitGainedPromotionSignalFired() {
            var promotionOne   = BuildPromotion("Promotion One");
            var promotionTwo   = BuildPromotion("Promotion Two");

            var treeData = BuildPromotionTreeData(new List<IPromotionPrerequisiteData>() {
                BuildPrereqData(promotionOne, new List<IPromotion>()),
                BuildPrereqData(promotionTwo, new List<IPromotion>())
            });

            var unit = BuildUnit();

            UnitSignals.UnitGainedPromotionSignal.Subscribe(delegate(IUnit signaledUnit) {
                Assert.AreEqual(signaledUnit, unit, "Signal was fired on an unexpected unit");
                Assert.Pass();
            });

            var promotionTree = new PromotionTree(treeData, unit, UnitSignals);

            promotionTree.ChoosePromotion(promotionOne);

            Assert.Fail("UnitGainedPromotionSignal was never fired");
        }

        #endregion

        #region utilities

        private IPromotion BuildPromotion(string name) {
            var mockPromotion = new Mock<IPromotion>();

            mockPromotion.Name = name;

            mockPromotion.Setup(promotion => promotion.name).Returns(name);

            return mockPromotion.Object;
        }

        private IPromotionTreeData BuildPromotionTreeData(IEnumerable<IPromotionPrerequisiteData> prerequisiteData) {
            var mockData = new Mock<IPromotionTreeData>();

            mockData.Setup(data => data.PrerequisiteData).Returns(prerequisiteData);

            return mockData.Object;
        }

        private IPromotionPrerequisiteData BuildPrereqData(IPromotion promotion, List<IPromotion> prereqs) {
            var mockData = new Mock<IPromotionPrerequisiteData>();

            mockData.Setup(data => data.Promotion)    .Returns(promotion);
            mockData.Setup(data => data.Prerequisites).Returns(prereqs);

            return mockData.Object;
        }

        private IUnit BuildUnit() {
            return new Mock<IUnit>().Object;
        }

        #endregion

        #endregion

    }

}
