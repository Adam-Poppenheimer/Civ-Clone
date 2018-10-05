using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation.SocialPolicies;
using Assets.Simulation.MapManagement;
using Assets.Simulation.Civilizations;

namespace Assets.Tests.Simulation.MapManagement {

    [TestFixture]
    public class SocialPolicyComposerTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<ISocialPolicyCanon> MockPolicyCanon;

        private List<IPolicyTreeDefinition>   AvailableTrees    = new List<IPolicyTreeDefinition>();
        private List<ISocialPolicyDefinition> AvailablePolicies = new List<ISocialPolicyDefinition>();

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            AvailableTrees   .Clear();
            AvailablePolicies.Clear();

            MockPolicyCanon = new Mock<ISocialPolicyCanon>();

            Container.Bind<ISocialPolicyCanon>().FromInstance(MockPolicyCanon.Object);

            Container.Bind<IEnumerable<IPolicyTreeDefinition>>()
                     .WithId("Available Policy Trees")
                     .FromInstance(AvailableTrees);

            Container.Bind<IEnumerable<ISocialPolicyDefinition>>()
                     .WithId("Available Policies")
                     .FromInstance(AvailablePolicies);

            Container.Bind<SocialPolicyComposer>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void ClearPolicyRuntimeForCiv_PolicyCanonCleared() {
            var composer = Container.Resolve<SocialPolicyComposer>();

            composer.ClearPolicyRuntime();

            MockPolicyCanon.Verify(canon => canon.Clear(), Times.Once);
        }

        [Test]
        public void ComposePoliciesFromCiv_ReturnedDataHasAllUnlockedTreeNames() {
            var treeOne   = BuildPolicyTree("Tree One");
            var treeTwo   = BuildPolicyTree("Tree Two");
            var treeThree = BuildPolicyTree("Tree Three");

            var trees = new List<IPolicyTreeDefinition>() { treeOne, treeTwo, treeThree };

            var civOne = BuildCivilization("Civ One", trees, new List<ISocialPolicyDefinition>());

            var composer = Container.Resolve<SocialPolicyComposer>();

            var policyData = composer.ComposePoliciesFromCiv(civOne);

            CollectionAssert.AreEquivalent(trees.Select(tree => tree.name), policyData.UnlockedTrees);
        }

        [Test]
        public void ComposePoliciesFromCiv_ReturnedDataHasAllUnlockedPolicyNames() {
            var policyOne   = BuildPolicy("Policy One");
            var policyTwo   = BuildPolicy("Policy Two");
            var policyThree = BuildPolicy("Policy Three");

            var policies = new List<ISocialPolicyDefinition>() { policyOne, policyTwo, policyThree };

            var civOne = BuildCivilization("Civ One", new List<IPolicyTreeDefinition>(), policies);

            var composer = Container.Resolve<SocialPolicyComposer>();

            var policyData = composer.ComposePoliciesFromCiv(civOne);

            CollectionAssert.AreEquivalent(policies.Select(policy => policy.name), policyData.UnlockedPolicies);
        }

        [Test]
        public void DecomposePoliciesIntoCiv_OverridesUnlockedTreesWithCorrectAvailableTrees() {
            var treeOne   = BuildPolicyTree("Tree One");
            BuildPolicyTree("Tree Two");
            var treeThree = BuildPolicyTree("Tree Three");

            var civOne = BuildCivilization("Civ One", null, null);

            var policyData = new SerializableSocialPolicyData() {
                UnlockedTrees = new List<string>() {
                    "Tree One", "Tree Three"
                },
                UnlockedPolicies = new List<string>()
            };

            var composer = Container.Resolve<SocialPolicyComposer>();

            MockPolicyCanon.Setup(
                canon => canon.OverrideUnlockedTreesForCiv(
                    It.IsAny<IEnumerable<IPolicyTreeDefinition>>(),
                    It.IsAny<ICivilization>()
                )
            ).Callback(delegate(IEnumerable<IPolicyTreeDefinition> newUnlockedTrees, ICivilization civ) {
                Assert.AreEqual(civOne, civ, "OverrideUnlockedTreesForCiv given an unexpected civ argument");
                CollectionAssert.AreEquivalent(
                    new List<IPolicyTreeDefinition>() { treeOne, treeThree }, newUnlockedTrees,
                    "OverrideUnlockedTreesForCiv given an unexpected newUnlockedTrees argument"
                );
                Assert.Pass();
            });

            composer.DecomposePoliciesIntoCiv(policyData, civOne);

            Assert.Fail("OverrideUnlockedTreesForCiv was never called");
        }

        [Test]
        public void DecomposePoliciesIntoCiv_OverridesUnlockedPoliciesWithCorrectAvailablePolicies() {
            var policyOne   = BuildPolicy("Policy One");
            BuildPolicy("Policy Two");
            var policyThree = BuildPolicy("Policy Three");

            var civOne = BuildCivilization("Civ One", null, null);

            var policyData = new SerializableSocialPolicyData() {
                UnlockedTrees = new List<string>(),
                UnlockedPolicies = new List<string>() {
                    "Policy One", "Policy Three"
                }
            };

            var composer = Container.Resolve<SocialPolicyComposer>();

            MockPolicyCanon.Setup(
                canon => canon.OverrideUnlockedPoliciesForCiv(
                    It.IsAny<IEnumerable<ISocialPolicyDefinition>>(),
                    It.IsAny<ICivilization>()
                )
            ).Callback(delegate(IEnumerable<ISocialPolicyDefinition> newUnlockedPolicies, ICivilization civ) {
                Assert.AreEqual(civOne, civ, "OverrideUnlockedPoliciesForCiv was given an unexpected civ argument");
                CollectionAssert.AreEquivalent(
                    new List<ISocialPolicyDefinition>() { policyOne, policyThree }, newUnlockedPolicies,
                    "OverrideUnlockedPoliciesForCiv was given an unexpected newUnlockedPolicies argument"
                );
                Assert.Pass();
            });

            composer.DecomposePoliciesIntoCiv(policyData, civOne);

            Assert.Fail("OverrideUnlockedPoliciesForCiv was never called");
        }

        #endregion

        #region utilities

        private IPolicyTreeDefinition BuildPolicyTree(string name) {
            var mockTree = new Mock<IPolicyTreeDefinition>();

            mockTree.Name = name;
            mockTree.Setup(tree => tree.name).Returns(name);

            var newTree = mockTree.Object;

            AvailableTrees.Add(newTree);

            return newTree;
        }

        private ISocialPolicyDefinition BuildPolicy(string name) {
            var mockPolicy = new Mock<ISocialPolicyDefinition>();

            mockPolicy.Name = name;
            mockPolicy.Setup(policy => policy.name).Returns(name);

            var newPolicy = mockPolicy.Object;

            AvailablePolicies.Add(newPolicy);

            return newPolicy;
        }

        private ICivilization BuildCivilization(
            string name, List<IPolicyTreeDefinition> unlockedTrees,
            List<ISocialPolicyDefinition> unlockedPolicies
        ){
            var mockCiv = new Mock<ICivilization>();

            var mockTemplate = new Mock<ICivilizationTemplate>();

            mockTemplate.Setup(template => template.Name).Returns(name);

            mockCiv.Name = name;
            mockCiv.Setup(civ => civ.Template).Returns(mockTemplate.Object);

            var newCiv = mockCiv.Object;

            MockPolicyCanon.Setup(canon => canon.GetTreesUnlockedFor   (newCiv)).Returns(unlockedTrees);
            MockPolicyCanon.Setup(canon => canon.GetPoliciesUnlockedFor(newCiv)).Returns(unlockedPolicies);

            return newCiv;
        }

        #endregion

        #endregion

    }

}
