﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using NUnit.Framework;
using Moq;
using UniRx;

using Assets.Simulation.SocialPolicies;
using Assets.Simulation.Technology;
using Assets.Simulation.Civilizations;

namespace Assets.Tests.Simulation.SocialPolicies {

    [TestFixture]
    public class SocialPolicyCanonTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<ITechCanon> MockTechCanon;

        private CivilizationSignals CivSignals;


        private List<ISocialPolicyDefinition> AvailablePolicies = new List<ISocialPolicyDefinition>();
        private List<IPolicyTreeDefinition>   AvailableTrees    = new List<IPolicyTreeDefinition>();

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            AvailablePolicies.Clear();
            AvailableTrees   .Clear();

            MockTechCanon = new Mock<ITechCanon>();

            CivSignals = new CivilizationSignals();

            Container.Bind<ITechCanon>         ().FromInstance(MockTechCanon.Object);
            Container.Bind<CivilizationSignals>().FromInstance(CivSignals);

            Container.Bind<IEnumerable<IPolicyTreeDefinition>>().WithId("Available Policy Trees").FromInstance(AvailableTrees);

            Container.Bind<SocialPolicyCanon>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void GetTreeWithPolicy_ReturnsTreeContainingPolicy() {
            var policies = new List<ISocialPolicyDefinition>() {
                BuildPolicy("Policy One", new List<ISocialPolicyDefinition>()),
                BuildPolicy("Policy Two", new List<ISocialPolicyDefinition>()),
                BuildPolicy("Policy Three", new List<ISocialPolicyDefinition>()),
            };

            var tree = BuildPolicyTree("Tree One", policies);

            var policyCanon = Container.Resolve<SocialPolicyCanon>();

            Assert.AreEqual(tree, policyCanon.GetTreeWithPolicy(policies[1]));
        }
        
        [Test]
        public void GetTreeWithPolicy_ReturnsNullIfNoTreeHasPolicy() {
            var policy = BuildPolicy("Policy", new List<ISocialPolicyDefinition>());

            var policyCanon = Container.Resolve<SocialPolicyCanon>();

            Assert.IsNull(policyCanon.GetTreeWithPolicy(policy));
        }

        [Test]
        public void GetPoliciesUnlockedFor_StartsAsEmptyCollection() {
            var civOne = BuildCivilization("Civ One");
            var civTwo = BuildCivilization("Civ Two");

            var policyCanon = Container.Resolve<SocialPolicyCanon>();

            CollectionAssert.IsEmpty(policyCanon.GetPoliciesUnlockedFor(civOne), "CivOne unexpectedly has unlocked policies");
            CollectionAssert.IsEmpty(policyCanon.GetPoliciesUnlockedFor(civTwo), "CivTwo unexpectedly has unlocked policies");
        }

        [Test]
        public void GetTreesUnlockedFor_StartsAsEmptyCollection() {
            var civOne = BuildCivilization("Civ One");
            var civTwo = BuildCivilization("Civ Two");

            var policyCanon = Container.Resolve<SocialPolicyCanon>();

            CollectionAssert.IsEmpty(policyCanon.GetTreesUnlockedFor(civOne), "CivOne unexpectedly has unlocked trees");
            CollectionAssert.IsEmpty(policyCanon.GetTreesUnlockedFor(civTwo), "CivTwo unexpectedly has unlocked trees");
        }

        [Test]
        public void GetPoliciesAvailableFor_StartsAsEmptyCollection() {
            var civOne = BuildCivilization("Civ One");
            var civTwo = BuildCivilization("Civ Two");

            var policyCanon = Container.Resolve<SocialPolicyCanon>();

            CollectionAssert.IsEmpty(policyCanon.GetPoliciesAvailableFor(civOne), "CivOne unexpectedly has available policies");
            CollectionAssert.IsEmpty(policyCanon.GetPoliciesAvailableFor(civTwo), "CivTwo unexpectedly has available policies");
        }

        [Test]
        public void GetTreesAvailableFor_StartsInformedByTechCanon() {
            var civOne = BuildCivilization("Civ One");

            var treeOne = BuildPolicyTree("Policy Tree One", new List<ISocialPolicyDefinition>());
            var treeTwo = BuildPolicyTree("Policy Tree Two", new List<ISocialPolicyDefinition>());
            BuildPolicyTree("Policy Tree Three", new List<ISocialPolicyDefinition>());

            var availableTrees = new List<IPolicyTreeDefinition>() { treeOne, treeTwo };

            MockTechCanon.Setup(canon => canon.GetResearchedPolicyTrees(civOne))
                         .Returns(availableTrees);

            var policyCanon = Container.Resolve<SocialPolicyCanon>();

            CollectionAssert.AreEquivalent(availableTrees, policyCanon.GetTreesAvailableFor(civOne));
        }

        [Test]
        public void SetPolicyAsUnlockedForCiv_NoLongerConsideredAvailable() {
            var civOne = BuildCivilization("Civ One");

            var policyOne = BuildPolicy("Policy One", new List<ISocialPolicyDefinition>());
            var policyTwo = BuildPolicy("Policy Two", new List<ISocialPolicyDefinition>());

            var treeOne = BuildPolicyTree("Policy Tree One", AvailablePolicies);

            MockTechCanon.Setup(canon => canon.GetResearchedPolicyTrees(civOne)).Returns(AvailableTrees);

            var policyCanon = Container.Resolve<SocialPolicyCanon>();

            policyCanon.SetTreeAsUnlockedForCiv(treeOne, civOne);
            policyCanon.SetPolicyAsUnlockedForCiv(policyOne, civOne);

            CollectionAssert.DoesNotContain(
                policyCanon.GetPoliciesAvailableFor(civOne), policyOne,
                "PolicyOne unexpectedly available to civOne"
            );

            CollectionAssert.Contains(
                policyCanon.GetPoliciesAvailableFor(civOne), policyTwo,
                "PolicyTwo unexpectedly not available to civOne"
            );
        }

        [Test]
        public void SetPolicyAsUnlockedForCiv_NowConsideredUnlocked() {
            var civOne = BuildCivilization("Civ One");

            var policyOne = BuildPolicy("Policy One", new List<ISocialPolicyDefinition>());
            var policyTwo = BuildPolicy("Policy Two", new List<ISocialPolicyDefinition>());

            var treeOne = BuildPolicyTree("Policy Tree One", AvailablePolicies);

            MockTechCanon.Setup(canon => canon.GetResearchedPolicyTrees(civOne)).Returns(AvailableTrees);

            var policyCanon = Container.Resolve<SocialPolicyCanon>();

            policyCanon.SetTreeAsUnlockedForCiv(treeOne, civOne);
            policyCanon.SetPolicyAsUnlockedForCiv(policyOne, civOne);

            CollectionAssert.Contains(
                policyCanon.GetPoliciesUnlockedFor(civOne), policyOne,
                "PolicyOne unexpectedly not unlocked for civOne"
            );

            CollectionAssert.DoesNotContain(
                policyCanon.GetPoliciesUnlockedFor(civOne), policyTwo,
                "PolicyTwo unexpectedly unlocked for civOne"
            );
        }

        [Test]
        public void SetPolicyAsUnlockedForCiv_MakesAvailablePostrequisitePolicies() {
            var civOne = BuildCivilization("Civ One");

            var policyOne   = BuildPolicy("Policy One",   new List<ISocialPolicyDefinition>());
            var policyTwo   = BuildPolicy("Policy Two",   new List<ISocialPolicyDefinition>() { policyOne });
            var policyThree = BuildPolicy("Policy Three", new List<ISocialPolicyDefinition>() { policyOne });

            var treeOne = BuildPolicyTree("Policy Tree One", AvailablePolicies);

            MockTechCanon.Setup(canon => canon.GetResearchedPolicyTrees(civOne)).Returns(AvailableTrees);

            var policyCanon = Container.Resolve<SocialPolicyCanon>();

            policyCanon.SetTreeAsUnlockedForCiv(treeOne, civOne);
            policyCanon.SetPolicyAsUnlockedForCiv(policyOne, civOne);
            
            CollectionAssert.AreEquivalent(
                policyCanon.GetPoliciesAvailableFor(civOne),
                new List<ISocialPolicyDefinition>() { policyTwo, policyThree }
            );
        }

        [Test]
        public void SetPolicyAsUnlockedForCiv_ThrowsIfNotAvailable() {
            var civOne = BuildCivilization("Civ One");

            var policyOne = BuildPolicy("Policy One", new List<ISocialPolicyDefinition>());

            var policyCanon = Container.Resolve<SocialPolicyCanon>();

            Assert.Throws<InvalidOperationException>(
                () => policyCanon.SetPolicyAsUnlockedForCiv(policyOne, civOne)
            );
        }

        [Test]
        public void SetPolicyAsUnlockedForCiv_ThrowsIfAlreadyUnlocked() {
            var civOne = BuildCivilization("Civ One");

            var policyOne = BuildPolicy("Policy One", new List<ISocialPolicyDefinition>());

            var treeOne = BuildPolicyTree("Policy Tree One", AvailablePolicies);

            MockTechCanon.Setup(canon => canon.GetResearchedPolicyTrees(civOne)).Returns(AvailableTrees);

            var policyCanon = Container.Resolve<SocialPolicyCanon>();

            policyCanon.SetTreeAsUnlockedForCiv(treeOne, civOne);
            policyCanon.SetPolicyAsUnlockedForCiv(policyOne, civOne);
            
            Assert.Throws<InvalidOperationException>(
                () => policyCanon.SetPolicyAsUnlockedForCiv(policyOne, civOne)
            );
        }

        [Test]
        public void SetPolicyAsLockedForCiv_BecomesAvailableIfAllPrerequisitesUnlocked() {
            var civOne = BuildCivilization("Civ One");

            var policyOne = BuildPolicy("Policy One", new List<ISocialPolicyDefinition>());
            var policyTwo = BuildPolicy("Policy Two", new List<ISocialPolicyDefinition>() { policyOne });

            var treeOne = BuildPolicyTree("Policy Tree One", AvailablePolicies);

            MockTechCanon.Setup(canon => canon.GetResearchedPolicyTrees(civOne)).Returns(AvailableTrees);

            var policyCanon = Container.Resolve<SocialPolicyCanon>();

            policyCanon.SetTreeAsUnlockedForCiv(treeOne, civOne);
            policyCanon.SetPolicyAsUnlockedForCiv(policyOne, civOne);
            policyCanon.SetPolicyAsUnlockedForCiv(policyTwo, civOne);

            policyCanon.SetPolicyAsLockedForCiv(policyTwo, civOne);

            CollectionAssert.Contains(policyCanon.GetPoliciesAvailableFor(civOne), policyTwo);
        }

        [Test]
        public void SetPolicyAsLockedForCiv_NotConsideredAvailableIfAnyPrerequisitesLocked() {
            var civOne = BuildCivilization("Civ One");

            var policyOne = BuildPolicy("Policy One", new List<ISocialPolicyDefinition>());
            var policyTwo = BuildPolicy("Policy Two", new List<ISocialPolicyDefinition>() { policyOne });

            var treeOne = BuildPolicyTree("Policy Tree One", AvailablePolicies);

            MockTechCanon.Setup(canon => canon.GetResearchedPolicyTrees(civOne)).Returns(AvailableTrees);

            var policyCanon = Container.Resolve<SocialPolicyCanon>();

            policyCanon.SetTreeAsUnlockedForCiv(treeOne, civOne);
            policyCanon.SetPolicyAsUnlockedForCiv(policyOne, civOne);
            policyCanon.SetPolicyAsUnlockedForCiv(policyTwo, civOne);

            policyCanon.SetPolicyAsLockedForCiv(policyOne, civOne);
            policyCanon.SetPolicyAsLockedForCiv(policyTwo, civOne);

            CollectionAssert.DoesNotContain(policyCanon.GetPoliciesAvailableFor(civOne), policyTwo);
        }

        [Test]
        public void SetPolicyAsLockedForCiv_NoLongerConsideredUnlocked() {
            var civOne = BuildCivilization("Civ One");

            var policyOne = BuildPolicy("Policy One", new List<ISocialPolicyDefinition>());

            var treeOne = BuildPolicyTree("Policy Tree One", AvailablePolicies);

            MockTechCanon.Setup(canon => canon.GetResearchedPolicyTrees(civOne)).Returns(AvailableTrees);

            var policyCanon = Container.Resolve<SocialPolicyCanon>();

            policyCanon.SetTreeAsUnlockedForCiv(treeOne, civOne);
            policyCanon.SetPolicyAsUnlockedForCiv(policyOne, civOne);

            policyCanon.SetPolicyAsLockedForCiv(policyOne, civOne);

            CollectionAssert.DoesNotContain(policyCanon.GetPoliciesUnlockedFor(civOne), policyOne);
        }

        [Test]
        public void SetPolicyAsLockedForCiv_ThrowsIfAlreadyLocked() {
            var civOne = BuildCivilization("Civ One");

            var policyOne = BuildPolicy("Policy One", new List<ISocialPolicyDefinition>());

            BuildPolicyTree("Policy Tree One", AvailablePolicies);

            MockTechCanon.Setup(canon => canon.GetResearchedPolicyTrees(civOne)).Returns(AvailableTrees);

            var policyCanon = Container.Resolve<SocialPolicyCanon>();

            Assert.Throws<InvalidOperationException>(
                () => policyCanon.SetPolicyAsLockedForCiv(policyOne, civOne)
            );
        }

        [Test]
        public void SetTreeAsUnlockedForCiv_NoLongerConsideredAvailable() {
            var civOne = BuildCivilization("Civ One");

            var treeOne = BuildPolicyTree("Policy Tree One", AvailablePolicies);

            MockTechCanon.Setup(canon => canon.GetResearchedPolicyTrees(civOne)).Returns(AvailableTrees);

            var policyCanon = Container.Resolve<SocialPolicyCanon>();
            
            policyCanon.SetTreeAsUnlockedForCiv(treeOne, civOne);

            CollectionAssert.DoesNotContain(policyCanon.GetTreesAvailableFor(civOne), treeOne);
        }

        [Test]
        public void SetTreeAsUnlockedForCiv_NowConsideredUnlocked() {
            var civOne = BuildCivilization("Civ One");

            var treeOne = BuildPolicyTree("Policy Tree One", AvailablePolicies);

            MockTechCanon.Setup(canon => canon.GetResearchedPolicyTrees(civOne)).Returns(AvailableTrees);

            var policyCanon = Container.Resolve<SocialPolicyCanon>();
            
            policyCanon.SetTreeAsUnlockedForCiv(treeOne, civOne);

            CollectionAssert.Contains(policyCanon.GetTreesUnlockedFor(civOne), treeOne);
        }

        [Test]
        public void SetTreeAsUnlockedForCiv_AllNoPrerequisitePoliciesWithinTreeBecomeAvailable() {
            var civOne = BuildCivilization("Civ One");

            var policyOne   = BuildPolicy("Policy One",   new List<ISocialPolicyDefinition>());
            var policyTwo   = BuildPolicy("Policy Two",   new List<ISocialPolicyDefinition>());
            BuildPolicy("Policy Three", new List<ISocialPolicyDefinition>() { policyOne });

            var treeOne = BuildPolicyTree("Policy Tree One", new List<ISocialPolicyDefinition>() {
                policyOne, policyTwo, 
            });

            MockTechCanon.Setup(canon => canon.GetResearchedPolicyTrees(civOne)).Returns(AvailableTrees);

            var policyCanon = Container.Resolve<SocialPolicyCanon>();
            
            policyCanon.SetTreeAsUnlockedForCiv(treeOne, civOne);

            CollectionAssert.AreEquivalent(
                policyCanon.GetPoliciesAvailableFor(civOne),
                new List<ISocialPolicyDefinition>() { policyOne, policyTwo }
            );
        }

        [Test]
        public void SetTreeAsUnlockedForCiv_ThrowsIfTreeNotAvailable() {
            var civOne = BuildCivilization("Civ One");

            var treeOne = BuildPolicyTree("Policy Tree One", AvailablePolicies);

            var policyCanon = Container.Resolve<SocialPolicyCanon>();
            
            Assert.Throws<InvalidOperationException>(
                () => policyCanon.SetTreeAsUnlockedForCiv(treeOne, civOne)
            );
        }

        [Test]
        public void SetTreeAsUnlockedForCiv_ThrowsIfTreeAlreadyUnlocked() {
            var civOne = BuildCivilization("Civ One");

            var treeOne = BuildPolicyTree("Policy Tree One", AvailablePolicies);

            MockTechCanon.Setup(canon => canon.GetResearchedPolicyTrees(civOne)).Returns(AvailableTrees);

            var policyCanon = Container.Resolve<SocialPolicyCanon>();
            
            policyCanon.SetTreeAsUnlockedForCiv(treeOne, civOne);

            Assert.Throws<InvalidOperationException>(
                () => policyCanon.SetTreeAsUnlockedForCiv(treeOne, civOne)
            );
        }

        [Test]
        public void SetTreeAsLockedForCiv_NoLongerConsideredUnlocked() {
            var civOne = BuildCivilization("Civ One");

            var treeOne = BuildPolicyTree("Policy Tree One", AvailablePolicies);

            MockTechCanon.Setup(canon => canon.GetResearchedPolicyTrees(civOne)).Returns(AvailableTrees);

            var policyCanon = Container.Resolve<SocialPolicyCanon>();
            
            policyCanon.SetTreeAsUnlockedForCiv(treeOne, civOne);
            policyCanon.SetTreeAsLockedForCiv(treeOne, civOne);

            CollectionAssert.DoesNotContain(policyCanon.GetTreesUnlockedFor(civOne), treeOne);
        }

        [Test]
        public void SetTreeAsLockedForCiv_NotConsideredAvailableIfNotAvailableInTechCanon() {
            var civOne = BuildCivilization("Civ One");

            var treeOne = BuildPolicyTree("Policy Tree One", AvailablePolicies);

            MockTechCanon.Setup(canon => canon.GetResearchedPolicyTrees(civOne)).Returns(AvailableTrees);

            var policyCanon = Container.Resolve<SocialPolicyCanon>();
            
            policyCanon.SetTreeAsUnlockedForCiv(treeOne, civOne);

            MockTechCanon.Setup(canon => canon.GetResearchedPolicyTrees(civOne)).Returns(new List<IPolicyTreeDefinition>());

            policyCanon.SetTreeAsLockedForCiv(treeOne, civOne);
            
            CollectionAssert.DoesNotContain(policyCanon.GetTreesAvailableFor(civOne), treeOne);
        }

        [Test]
        public void SetTreeAsLockedForCiv_NowConsideredAvailableIfAvailableInTechCanon() {
            var civOne = BuildCivilization("Civ One");

            var treeOne = BuildPolicyTree("Policy Tree One", AvailablePolicies);

            MockTechCanon.Setup(canon => canon.GetResearchedPolicyTrees(civOne)).Returns(AvailableTrees);

            var policyCanon = Container.Resolve<SocialPolicyCanon>();
            
            policyCanon.SetTreeAsUnlockedForCiv(treeOne, civOne);
            policyCanon.SetTreeAsLockedForCiv(treeOne, civOne);
            
            CollectionAssert.Contains(policyCanon.GetTreesAvailableFor(civOne), treeOne);
        }

        [Test]
        public void SetTreeAsLockedForCiv_AllPoliciesInTreeNowConsideredLockedAndNotAvailable() {
            var civOne = BuildCivilization("Civ One");

            var policyOne   = BuildPolicy("Policy One",   new List<ISocialPolicyDefinition>());
            var policyTwo   = BuildPolicy("Policy Two",   new List<ISocialPolicyDefinition>());
            var policyThree = BuildPolicy("Policy Three", new List<ISocialPolicyDefinition>());

            var treeOne = BuildPolicyTree("Policy Tree One", AvailablePolicies);

            MockTechCanon.Setup(canon => canon.GetResearchedPolicyTrees(civOne)).Returns(AvailableTrees);

            var policyCanon = Container.Resolve<SocialPolicyCanon>();
            
            policyCanon.SetTreeAsUnlockedForCiv(treeOne, civOne);

            policyCanon.SetPolicyAsUnlockedForCiv(policyOne,   civOne);
            policyCanon.SetPolicyAsUnlockedForCiv(policyTwo,   civOne);
            policyCanon.SetPolicyAsUnlockedForCiv(policyThree, civOne);

            policyCanon.SetTreeAsLockedForCiv(treeOne, civOne);

            CollectionAssert.IsEmpty(policyCanon.GetPoliciesUnlockedFor (civOne));
            CollectionAssert.IsEmpty(policyCanon.GetPoliciesAvailableFor(civOne));
        }

        [Test]
        public void SetTreeAsLockedForCiv_ThrowsIfAlreadyLocked() {
            var civOne = BuildCivilization("Civ One");

            var treeOne = BuildPolicyTree("Policy Tree One", AvailablePolicies);

            var policyCanon = Container.Resolve<SocialPolicyCanon>();

            Assert.Throws<InvalidOperationException>(
                () => policyCanon.SetTreeAsLockedForCiv(treeOne, civOne)
            );
        }

        [Test]
        public void TreeLockedAndUnlocked_AllPoliciesInTreeConsideredLocked() {
            var civOne = BuildCivilization("Civ One");

            var policyOne   = BuildPolicy("Policy One",   new List<ISocialPolicyDefinition>());
            var policyTwo   = BuildPolicy("Policy Two",   new List<ISocialPolicyDefinition>());
            var policyThree = BuildPolicy("Policy Three", new List<ISocialPolicyDefinition>());

            var treeOne = BuildPolicyTree("Policy Tree One", AvailablePolicies);

            MockTechCanon.Setup(canon => canon.GetResearchedPolicyTrees(civOne)).Returns(AvailableTrees);

            var policyCanon = Container.Resolve<SocialPolicyCanon>();
            
            policyCanon.SetTreeAsUnlockedForCiv(treeOne, civOne);

            policyCanon.SetPolicyAsUnlockedForCiv(policyOne,   civOne);
            policyCanon.SetPolicyAsUnlockedForCiv(policyTwo,   civOne);
            policyCanon.SetPolicyAsUnlockedForCiv(policyThree, civOne);

            policyCanon.SetTreeAsLockedForCiv  (treeOne, civOne);
            policyCanon.SetTreeAsUnlockedForCiv(treeOne, civOne);

            CollectionAssert.IsEmpty(policyCanon.GetPoliciesUnlockedFor(civOne));
        }

        [Test]
        public void IsTreeCompletedByCiv_TrueIfAllPoliciesUnlocked() {
            var civOne = BuildCivilization("Civ One");

            var policyOne   = BuildPolicy("Policy One",   new List<ISocialPolicyDefinition>());
            var policyTwo   = BuildPolicy("Policy Two",   new List<ISocialPolicyDefinition>());
            var policyThree = BuildPolicy("Policy Three", new List<ISocialPolicyDefinition>());

            var treeOne = BuildPolicyTree("Policy Tree One", AvailablePolicies);

            MockTechCanon.Setup(canon => canon.GetResearchedPolicyTrees(civOne)).Returns(AvailableTrees);

            var policyCanon = Container.Resolve<SocialPolicyCanon>();
            
            policyCanon.SetTreeAsUnlockedForCiv(treeOne, civOne);

            policyCanon.SetPolicyAsUnlockedForCiv(policyOne,   civOne);
            policyCanon.SetPolicyAsUnlockedForCiv(policyTwo,   civOne);
            policyCanon.SetPolicyAsUnlockedForCiv(policyThree, civOne);

            Assert.IsTrue(policyCanon.IsTreeCompletedByCiv(treeOne, civOne));
        }

        [Test]
        public void IsTreeCompletedByCiv_FalseIfAnyPoliciesNotUnlocked() {
            var civOne = BuildCivilization("Civ One");

            var policyOne   = BuildPolicy("Policy One",   new List<ISocialPolicyDefinition>());
            var policyTwo   = BuildPolicy("Policy Two",   new List<ISocialPolicyDefinition>());
            BuildPolicy("Policy Three", new List<ISocialPolicyDefinition>());

            var treeOne = BuildPolicyTree("Policy Tree One", AvailablePolicies);

            MockTechCanon.Setup(canon => canon.GetResearchedPolicyTrees(civOne)).Returns(AvailableTrees);

            var policyCanon = Container.Resolve<SocialPolicyCanon>();
            
            policyCanon.SetTreeAsUnlockedForCiv(treeOne, civOne);

            policyCanon.SetPolicyAsUnlockedForCiv(policyOne,   civOne);
            policyCanon.SetPolicyAsUnlockedForCiv(policyTwo,   civOne);

            Assert.IsFalse(policyCanon.IsTreeCompletedByCiv(treeOne, civOne));
        }

        [Test]
        public void Clear_LocksAllUnlockedTreesForAllCivs() {
            var civOne = BuildCivilization("Civ One");

            var treeOne   = BuildPolicyTree("Tree One",   new List<ISocialPolicyDefinition>());
            var treeTwo   = BuildPolicyTree("Tree Two",   new List<ISocialPolicyDefinition>());
            var treeThree = BuildPolicyTree("Tree Three", new List<ISocialPolicyDefinition>());

            MockTechCanon.Setup(canon => canon.GetResearchedPolicyTrees(civOne)).Returns(AvailableTrees);

            var policyCanon = Container.Resolve<SocialPolicyCanon>();
            
            policyCanon.SetTreeAsUnlockedForCiv(treeOne,   civOne);
            policyCanon.SetTreeAsUnlockedForCiv(treeTwo,   civOne);
            policyCanon.SetTreeAsUnlockedForCiv(treeThree, civOne);

            policyCanon.Clear();

            CollectionAssert.IsEmpty(policyCanon.GetTreesUnlockedFor(civOne));
        }

        [Test]
        public void Clear_LocksAllUnlockedPoliciesForAllCivs() {
            var civOne = BuildCivilization("Civ One");

            var policies = new List<ISocialPolicyDefinition>() {
                BuildPolicy("Policy One",   new List<ISocialPolicyDefinition>()),
                BuildPolicy("Policy Two",   new List<ISocialPolicyDefinition>()),
                BuildPolicy("Policy Three", new List<ISocialPolicyDefinition>())
            };

            var policyCanon = Container.Resolve<SocialPolicyCanon>();

            policyCanon.OverrideUnlockedPoliciesForCiv(policies, civOne);
            policyCanon.Clear();

            CollectionAssert.IsEmpty(policyCanon.GetPoliciesUnlockedFor(civOne));
        }

        [Test]
        public void OverrideUnlockedTreesForCiv_OverridesExistingUnlocks() {
            var civOne = BuildCivilization("Civ One");

            var treeOne   = BuildPolicyTree("Tree One",   new List<ISocialPolicyDefinition>());
            var treeTwo   = BuildPolicyTree("Tree Two",   new List<ISocialPolicyDefinition>());
            var treeThree = BuildPolicyTree("Tree Three", new List<ISocialPolicyDefinition>());

            MockTechCanon.Setup(canon => canon.GetResearchedPolicyTrees(civOne)).Returns(AvailableTrees);

            var policyCanon = Container.Resolve<SocialPolicyCanon>();

            policyCanon.SetTreeAsUnlockedForCiv(treeOne, civOne);

            policyCanon.OverrideUnlockedTreesForCiv(
                new List<IPolicyTreeDefinition>() { treeTwo, treeThree },
                civOne
            );

            CollectionAssert.AreEquivalent(
                new List<IPolicyTreeDefinition>() { treeTwo, treeThree },
                policyCanon.GetTreesUnlockedFor(civOne)
            );
        }

        [Test]
        public void OverrideUnlockedTreesForCiv_IgnoresAvailablity() {
            var civOne = BuildCivilization("Civ One");

            var treeOne   = BuildPolicyTree("Tree One",   new List<ISocialPolicyDefinition>());
            var treeTwo   = BuildPolicyTree("Tree Two",   new List<ISocialPolicyDefinition>());
            var treeThree = BuildPolicyTree("Tree Three", new List<ISocialPolicyDefinition>());

            var trees = new List<IPolicyTreeDefinition>() { treeOne, treeTwo, treeThree };

            var policyCanon = Container.Resolve<SocialPolicyCanon>();

            Assert.DoesNotThrow(
                () => policyCanon.OverrideUnlockedTreesForCiv(trees, civOne),
                "OverrideUnlockedTreesForCiv unexpectedly threw an exception"
            );

            CollectionAssert.AreEquivalent(
                trees, policyCanon.GetTreesUnlockedFor(civOne),
                "GetTreesUnlockedFor returned an unexpected value"
            );
        }

        [Test]
        public void OverrideUnlockedPoliciesForCiv_OverridesExistingUnlocks() {
            var civOne = BuildCivilization("Civ One");

            var policyOne   = BuildPolicy("Policy One",   new List<ISocialPolicyDefinition>());
            var policyTwo   = BuildPolicy("Policy Two",   new List<ISocialPolicyDefinition>());
            var policyThree = BuildPolicy("Policy Three", new List<ISocialPolicyDefinition>());

            var treeOne = BuildPolicyTree(
                "Tree One", new List<ISocialPolicyDefinition>() { policyOne, policyTwo, policyThree }
            );

            MockTechCanon.Setup(canon => canon.GetResearchedPolicyTrees(civOne)).Returns(AvailableTrees);

            var policyCanon = Container.Resolve<SocialPolicyCanon>();

            policyCanon.SetTreeAsUnlockedForCiv  (treeOne,   civOne);
            policyCanon.SetPolicyAsUnlockedForCiv(policyOne, civOne);

            var newPolicies = new List<ISocialPolicyDefinition>() { policyTwo, policyThree };

            policyCanon.OverrideUnlockedPoliciesForCiv(newPolicies, civOne);

            CollectionAssert.AreEquivalent(newPolicies, policyCanon.GetPoliciesUnlockedFor(civOne));
        }

        [Test]
        public void OverrideUnlockedPoliciesForCiv_IgnoresAvailablity() {
            var civOne = BuildCivilization("Civ One");

            var policyOne   = BuildPolicy("Policy One",   new List<ISocialPolicyDefinition>());
            var policyTwo   = BuildPolicy("Policy Two",   new List<ISocialPolicyDefinition>());
            var policyThree = BuildPolicy("Policy Three", new List<ISocialPolicyDefinition>());

            var policies = new List<ISocialPolicyDefinition>() { policyOne, policyTwo, policyThree };

            var policyCanon = Container.Resolve<SocialPolicyCanon>();

            Assert.DoesNotThrow(
                () => policyCanon.OverrideUnlockedPoliciesForCiv(policies, civOne),
                "OverrideUnlockedPoliciesForCiv unexpectedly threw an exception"
            );

            CollectionAssert.AreEquivalent(
                policies, policyCanon.GetPoliciesUnlockedFor(civOne),
                "GetPoliciesUnlockedFor returned an unexpected value"
            );
        }

        [Test]
        public void SetPolicyAsUnlockedForCiv_FiresPolicyUnlockedEvent() {
            var civOne = BuildCivilization("Civ One");

            var policyOne = BuildPolicy("Policy One", new List<ISocialPolicyDefinition>());

            var treeOne = BuildPolicyTree("Policy Tree One", AvailablePolicies);

            MockTechCanon.Setup(canon => canon.GetResearchedPolicyTrees(civOne)).Returns(AvailableTrees);

            var policyCanon = Container.Resolve<SocialPolicyCanon>();

            CivSignals.CivUnlockedPolicySignal.Subscribe(delegate(Tuple<ICivilization, ISocialPolicyDefinition> data) {
                Assert.AreEqual(civOne,    data.Item1, "Incorrect civilization passed");
                Assert.AreEqual(policyOne, data.Item2, "Incorrect policy passed");
                Assert.Pass();
            });

            policyCanon.SetTreeAsUnlockedForCiv(treeOne, civOne);
            policyCanon.SetPolicyAsUnlockedForCiv(policyOne, civOne);

            Assert.Fail("CivUnlockedPolicySignal never fired");
        }

        [Test]
        public void SetPolicyAsUnlockedForCiv_FiresTreeFinishedEventIfParentTreeCompleted() {
            var civOne = BuildCivilization("Civ One");

            var policyOne = BuildPolicy("Policy One", new List<ISocialPolicyDefinition>());

            var treeOne = BuildPolicyTree("Policy Tree One", AvailablePolicies);

            MockTechCanon.Setup(canon => canon.GetResearchedPolicyTrees(civOne)).Returns(AvailableTrees);

            var policyCanon = Container.Resolve<SocialPolicyCanon>();

            CivSignals.CivFinishedPolicyTreeSignal.Subscribe(delegate(Tuple<ICivilization, IPolicyTreeDefinition> data) {
                Assert.AreEqual(civOne,  data.Item1, "Incorrect civilization passed");
                Assert.AreEqual(treeOne, data.Item2, "Incorrect tree passed");
                Assert.Pass();
            });

            policyCanon.SetTreeAsUnlockedForCiv(treeOne, civOne);
            policyCanon.SetPolicyAsUnlockedForCiv(policyOne, civOne);

            Assert.Fail("CivFinishedPolicyTreeSignal never fired");
        }

        [Test]
        public void SetPolicyAsUnlockedForCiv_DoesntFireTreeFinishedEventIfParentTreeNotCompleted() {
            var civOne = BuildCivilization("Civ One");

            var policyOne = BuildPolicy("Policy One", new List<ISocialPolicyDefinition>());
            BuildPolicy("Policy Two", new List<ISocialPolicyDefinition>());

            var treeOne = BuildPolicyTree("Policy Tree One", AvailablePolicies);

            MockTechCanon.Setup(canon => canon.GetResearchedPolicyTrees(civOne)).Returns(AvailableTrees);

            var policyCanon = Container.Resolve<SocialPolicyCanon>();

            CivSignals.CivFinishedPolicyTreeSignal.Subscribe(data => Assert.Fail());

            policyCanon.SetTreeAsUnlockedForCiv(treeOne, civOne);
            policyCanon.SetPolicyAsUnlockedForCiv(policyOne, civOne);
        }

        [Test]
        public void SetPolicyAsLockedForCiv_FiresPolicyLockedEvent() {
            var civOne = BuildCivilization("Civ One");

            var policyOne = BuildPolicy("Policy One", new List<ISocialPolicyDefinition>());

            var treeOne = BuildPolicyTree("Policy Tree One", AvailablePolicies);

            MockTechCanon.Setup(canon => canon.GetResearchedPolicyTrees(civOne)).Returns(AvailableTrees);

            var policyCanon = Container.Resolve<SocialPolicyCanon>();

            CivSignals.CivLockedPolicySignal.Subscribe(delegate(Tuple<ICivilization, ISocialPolicyDefinition> data) {
                Assert.AreEqual(civOne,    data.Item1, "Incorrect civilization passed");
                Assert.AreEqual(policyOne, data.Item2, "Incorrect policy passed");
                Assert.Pass();
            });

            policyCanon.SetTreeAsUnlockedForCiv(treeOne, civOne);
            policyCanon.SetPolicyAsUnlockedForCiv(policyOne, civOne);
            policyCanon.SetPolicyAsLockedForCiv  (policyOne, civOne);

            Assert.Fail("CivLockedPolicySignal never fired");
        }

        [Test]
        public void SetPolicyAsLockedForCiv_FiresTreeUnfinishedEventIfTreeWasFinishedBefore() {
            var civOne = BuildCivilization("Civ One");

            var policyOne = BuildPolicy("Policy One", new List<ISocialPolicyDefinition>());
            var policyTwo = BuildPolicy("Policy Two", new List<ISocialPolicyDefinition>());

            var treeOne = BuildPolicyTree("Policy Tree One", AvailablePolicies);

            MockTechCanon.Setup(canon => canon.GetResearchedPolicyTrees(civOne)).Returns(AvailableTrees);

            var policyCanon = Container.Resolve<SocialPolicyCanon>();

            policyCanon.SetTreeAsUnlockedForCiv(treeOne, civOne);
            policyCanon.SetPolicyAsUnlockedForCiv(policyOne, civOne);
            policyCanon.SetPolicyAsUnlockedForCiv(policyTwo, civOne);

            CivSignals.CivUnfinishedPolicyTreeSignal.Subscribe(delegate(Tuple<ICivilization, IPolicyTreeDefinition> data) {
                Assert.AreEqual(civOne,  data.Item1, "Incorrect civilization passed");
                Assert.AreEqual(treeOne, data.Item2, "Incorrect tree passed");
                Assert.Pass();
            });

            policyCanon.SetPolicyAsLockedForCiv(policyTwo, civOne);
        }

        [Test]
        public void SetPolicyAsLockedForCiv_DoesNotFireTreeUnfinishedEventIfTreeWasntFinishedBefore() {
            var civOne = BuildCivilization("Civ One");

            var policyOne = BuildPolicy("Policy One", new List<ISocialPolicyDefinition>());
            BuildPolicy("Policy Two", new List<ISocialPolicyDefinition>());

            var treeOne = BuildPolicyTree("Policy Tree One", AvailablePolicies);

            MockTechCanon.Setup(canon => canon.GetResearchedPolicyTrees(civOne)).Returns(AvailableTrees);

            var policyCanon = Container.Resolve<SocialPolicyCanon>();

            policyCanon.SetTreeAsUnlockedForCiv(treeOne, civOne);
            policyCanon.SetPolicyAsUnlockedForCiv(policyOne, civOne);

            CivSignals.CivUnfinishedPolicyTreeSignal.Subscribe(data => Assert.Fail());

            policyCanon.SetPolicyAsLockedForCiv(policyOne, civOne);
        }

        [Test]
        public void GetPolicyBonusesForCiv_ContainsBonusesFromUnlockedPolicies() {
            var civ = BuildCivilization("Civ");

            var bonusesOne   = BuildBonusesData();
            var bonusesTwo   = BuildBonusesData();
            var bonusesThree = BuildBonusesData();

            var policyOne = BuildPolicy(bonusesOne);
            var policyTwo = BuildPolicy(bonusesTwo);
            BuildPolicy(bonusesThree);

            var treeOne = BuildPolicyTree("Policy Tree One", AvailablePolicies);

            MockTechCanon.Setup(canon => canon.GetResearchedPolicyTrees(civ)).Returns(AvailableTrees);

            var policyCanon = Container.Resolve<SocialPolicyCanon>();

            policyCanon.SetTreeAsUnlockedForCiv(treeOne, civ);
            policyCanon.SetPolicyAsUnlockedForCiv(policyOne, civ);
            policyCanon.SetPolicyAsUnlockedForCiv(policyTwo, civ);

            CollectionAssert.Contains(
                policyCanon.GetPolicyBonusesForCiv(civ), bonusesOne,
                "Does not contain bonusesOne"
            );

            CollectionAssert.Contains(
                policyCanon.GetPolicyBonusesForCiv(civ), bonusesTwo,
                "Does not contain bonusesTwo"
            );
        }

        [Test]
        public void GetPolicyBonusesForCiv_ContainsUnlockingBonusesFromUnlockedTrees() {
            var civ = BuildCivilization("Civ");

            var bonusesOne = BuildBonusesData();

            var treeOne = BuildPolicyTree(bonusesOne, null, AvailablePolicies);

            MockTechCanon.Setup(canon => canon.GetResearchedPolicyTrees(civ)).Returns(AvailableTrees);

            var policyCanon = Container.Resolve<SocialPolicyCanon>();

            policyCanon.SetTreeAsUnlockedForCiv(treeOne, civ);

            CollectionAssert.Contains(policyCanon.GetPolicyBonusesForCiv(civ), bonusesOne);
        }

        [Test]
        public void GetPolicyBonusesForCiv_ContainsCompletionBonusesFromCompletedTrees() {
            var civ = BuildCivilization("Civ");

            var bonusesOne = BuildBonusesData();
            var bonusesTwo = BuildBonusesData();

            var policyOne = BuildPolicy("Policy One", new List<ISocialPolicyDefinition>());
            var policyTwo = BuildPolicy("Policy Two", new List<ISocialPolicyDefinition>());

            var treeOne = BuildPolicyTree(null, bonusesOne, new List<ISocialPolicyDefinition>() { policyOne });
            var treeTwo = BuildPolicyTree(null, bonusesTwo, new List<ISocialPolicyDefinition>() { policyTwo });

            MockTechCanon.Setup(canon => canon.GetResearchedPolicyTrees(civ)).Returns(AvailableTrees);

            var policyCanon = Container.Resolve<SocialPolicyCanon>();

            policyCanon.SetTreeAsUnlockedForCiv(treeOne, civ);
            policyCanon.SetTreeAsUnlockedForCiv(treeTwo, civ);
            policyCanon.SetPolicyAsUnlockedForCiv(policyOne, civ);

            CollectionAssert.Contains      (policyCanon.GetPolicyBonusesForCiv(civ), bonusesOne, "Does not contain BonusesOne");
            CollectionAssert.DoesNotContain(policyCanon.GetPolicyBonusesForCiv(civ), bonusesTwo, "Falsely contains BonusesOne");
        }

        #endregion

        #region utilities

        private ICivilization BuildCivilization(string name) {
            var mockCiv = new Mock<ICivilization>();

            mockCiv.Name = name;

            return mockCiv.Object;
        }

        private ISocialPolicyDefinition BuildPolicy(string name, List<ISocialPolicyDefinition> prerequisites) {
            var mockPolicy = new Mock<ISocialPolicyDefinition>();

            mockPolicy.Name = name;
            mockPolicy.Setup(policy => policy.Prerequisites).Returns(prerequisites);

            var newPolicy = mockPolicy.Object;

            AvailablePolicies.Add(newPolicy);

            return newPolicy;
        }

        private IPolicyTreeDefinition BuildPolicyTree(string name, List<ISocialPolicyDefinition> policies) {
            var mockTree = new Mock<IPolicyTreeDefinition>();

            mockTree.Name = name;
            mockTree.Setup(tree => tree.Policies).Returns(policies);

            var newTree = mockTree.Object;

            AvailableTrees.Add(newTree);

            return newTree;
        }

        private IPolicyTreeDefinition BuildPolicyTree(
            ISocialPolicyBonusesData unlockingBonuses, ISocialPolicyBonusesData completionBonuses,
            List<ISocialPolicyDefinition> policies
        ) {
            var mockTree = new Mock<IPolicyTreeDefinition>();

            mockTree.Setup(tree => tree.UnlockingBonuses) .Returns(unlockingBonuses);
            mockTree.Setup(tree => tree.CompletionBonuses).Returns(completionBonuses);
            mockTree.Setup(tree => tree.Policies)         .Returns(policies);

            var newTree = mockTree.Object;

            AvailableTrees.Add(newTree);

            return newTree;
        }

        private ISocialPolicyBonusesData BuildBonusesData() {
            return new Mock<ISocialPolicyBonusesData>().Object;
        }

        private ISocialPolicyDefinition BuildPolicy(ISocialPolicyBonusesData bonuses) {
            var mockPolicy = new Mock<ISocialPolicyDefinition>();

            mockPolicy.Setup(policy => policy.Bonuses).Returns(bonuses);

            var newPolicy = mockPolicy.Object;

            AvailablePolicies.Add(newPolicy);

            return newPolicy;
        }

        #endregion

        #endregion

    }

}
