using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using Zenject;
using Moq;

using Assets.Simulation;
using Assets.Simulation.SocialPolicies;
using Assets.Simulation.Civilizations;

namespace Assets.Tests.Simulation.SocialPolicies {

    public class SocialPolicyBonusLogicTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<ISocialPolicyCanon> MockSocialPolicyCanon;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockSocialPolicyCanon = new Mock<ISocialPolicyCanon>();

            Container.Bind<ISocialPolicyCanon>().FromInstance(MockSocialPolicyCanon.Object);

            Container.Bind<SocialPolicyBonusLogic>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void ExtractBonusFromCiv_IncludesBonusesOfUnlockedPolicies() {
            var unlockedPolicyOne   = BuildPolicy(new YieldSummary(food: 1f));
            var unlockedPolicyTwo   = BuildPolicy(new YieldSummary(food: 2f));
            var unlockedPolicyThree = BuildPolicy(new YieldSummary(food: 3f));

            BuildPolicy(new YieldSummary(food: 4f));

            var civ = BuildCiv(
                new List<IPolicyTreeDefinition>(),
                new List<IPolicyTreeDefinition>(),
                new List<ISocialPolicyDefinition>() { unlockedPolicyOne, unlockedPolicyTwo, unlockedPolicyThree }
            );

            var bonusLogic = Container.Resolve<SocialPolicyBonusLogic>();

            Assert.AreEqual(
                new YieldSummary(food: 6f),
                bonusLogic.ExtractBonusFromCiv(civ, bonuses => bonuses.CityYield, (a, b) => a + b)
            );
        }

        [Test]
        public void ExtractBonusFromCiv_IncludesUnlockingBonusesOfUnlockedTrees() {
            var policyTreeOne = BuildPolicyTree(
                new YieldSummary(production: 5f), new YieldSummary(production: 5f)
            );

            var policyTreeTwo = BuildPolicyTree(
                new YieldSummary(gold: 6f), new YieldSummary(gold: 6f)
            );

            var civ = BuildCiv(
                new List<IPolicyTreeDefinition>  () { policyTreeOne, policyTreeTwo },
                new List<IPolicyTreeDefinition>  (),
                new List<ISocialPolicyDefinition>()
            );

            var bonusLogic = Container.Resolve<SocialPolicyBonusLogic>();

            Assert.AreEqual(
                new YieldSummary(production: 5f, gold: 6f),
                bonusLogic.ExtractBonusFromCiv(civ, bonuses => bonuses.CityYield, (a, b) => a + b)
            );
        }

        [Test]
        public void ExtractBonusFromCiv_IncludesCompletionBonusesOfCompletedTrees() {
            var policyTreeOne = BuildPolicyTree(
                new YieldSummary(production: 5f), new YieldSummary(production: 5f)
            );

            var policyTreeTwo = BuildPolicyTree(
                new YieldSummary(gold: 6f), new YieldSummary(gold: 6f)
            );

            var civ = BuildCiv(
                new List<IPolicyTreeDefinition>  () { policyTreeOne, policyTreeTwo },
                new List<IPolicyTreeDefinition>  () { policyTreeOne },
                new List<ISocialPolicyDefinition>()
            );

            var bonusLogic = Container.Resolve<SocialPolicyBonusLogic>();

            Assert.AreEqual(
                new YieldSummary(production: 10f, gold: 6f),
                bonusLogic.ExtractBonusFromCiv(civ, bonuses => bonuses.CityYield, (a, b) => a + b)
            );
        }

        #endregion

        #region utilities

        private ISocialPolicyDefinition BuildPolicy(YieldSummary cityYield) {
            var mockPolicy = new Mock<ISocialPolicyDefinition>();

            var mockBonuses = new Mock<ISocialPolicyBonusesData>();

            mockBonuses.Setup(bonus => bonus.CityYield).Returns(cityYield);

            mockPolicy.Setup(policy => policy.Bonuses).Returns(mockBonuses.Object);

            return mockPolicy.Object;
        }

        private IPolicyTreeDefinition BuildPolicyTree(
            YieldSummary unlockingCityYield, YieldSummary completionCityYield,
            params ISocialPolicyDefinition[] policies
        ) {
            var mockUnlockingBonuses  = new Mock<ISocialPolicyBonusesData>();
            var mockCompletionBonuses = new Mock<ISocialPolicyBonusesData>();

            mockUnlockingBonuses .Setup(bonuses => bonuses.CityYield).Returns(unlockingCityYield);
            mockCompletionBonuses.Setup(bonuses => bonuses.CityYield).Returns(completionCityYield);

            var mockTree = new Mock<IPolicyTreeDefinition>();

            mockTree.Setup(tree => tree.UnlockingBonuses) .Returns(mockUnlockingBonuses .Object);
            mockTree.Setup(tree => tree.CompletionBonuses).Returns(mockCompletionBonuses.Object);

            mockTree.Setup(tree => tree.Policies).Returns(policies);

            return mockTree.Object;
        }

        private ICivilization BuildCiv(
            List<IPolicyTreeDefinition> unlockedTrees, List<IPolicyTreeDefinition> completedTrees,
            List<ISocialPolicyDefinition> unlockedPolicies
        ) {
            var newCiv = new Mock<ICivilization>().Object;

            MockSocialPolicyCanon.Setup(canon => canon.GetTreesUnlockedFor   (newCiv)).Returns(unlockedTrees);
            MockSocialPolicyCanon.Setup(canon => canon.GetPoliciesUnlockedFor(newCiv)).Returns(unlockedPolicies);

            foreach(var policyTree in completedTrees) {
                MockSocialPolicyCanon.Setup(canon => canon.IsTreeCompletedByCiv(policyTree, newCiv)).Returns(true);
            }

            return newCiv;
        }

        #endregion

        #endregion

    }

}
