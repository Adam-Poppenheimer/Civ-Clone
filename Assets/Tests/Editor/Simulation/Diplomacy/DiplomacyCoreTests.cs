using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation.Civilizations;
using Assets.Simulation.Diplomacy;

namespace Assets.Tests.Simulation.Diplomacy {

    [TestFixture]
    public class DiplomacyCoreTests : ZenjectUnitTestFixture {

        #region instance fields and properties



        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            Container.Bind<DiplomacyCore>().AsSingle();
        }

        #endregion

        #region tests

        [Test(Description = "When SendProposal is called, the sent proposal should appear in the " +
            "collection returned by GetProposalsMadeByCiv, but only for the sending civilization")]
        public void SendProposal_ReflectedInGetProposalsMadeByCiv() {
            var sender   = BuildCivilization();
            var receiver = BuildCivilization();

            var proposal = BuildProposal(sender, receiver);

            var diplomacyCore = Container.Resolve<DiplomacyCore>();

            diplomacyCore.SendProposal(proposal);

            CollectionAssert.Contains      (diplomacyCore.GetProposalsMadeByCiv(sender),   proposal);
            CollectionAssert.DoesNotContain(diplomacyCore.GetProposalsMadeByCiv(receiver), proposal);
        }

        [Test(Description = "When SendProposal is called, the sent proposal should appear in the " +
            "collection returned by GetProposalsMadeToCiv, but only for the receiving civilization")]
        public void SendProposal_ReflectedInGetProposalsMadeToCiv() {
            var sender   = BuildCivilization();
            var receiver = BuildCivilization();

            var proposal = BuildProposal(sender, receiver);

            var diplomacyCore = Container.Resolve<DiplomacyCore>();

            diplomacyCore.SendProposal(proposal);

            CollectionAssert.DoesNotContain(diplomacyCore.GetProposalsMadeToCiv(sender),   proposal);
            CollectionAssert.Contains      (diplomacyCore.GetProposalsMadeToCiv(receiver), proposal);
        }

        [Test(Description = "TryAcceptProposal should return true if the argued proposal can " +
            "be performed, and false otherwise.")]
        public void TryAcceptProposal_ReturnsTrueIfProposalCanBePerformed() {
            var sender   = BuildCivilization();
            var receiver = BuildCivilization();

            var performableProposal   = BuildProposal(sender, receiver, true);
            var unperformableProposal = BuildProposal(sender, receiver, false);

            var diplomacyCore = Container.Resolve<DiplomacyCore>();

            Assert.IsTrue (diplomacyCore.TryAcceptProposal(performableProposal));
            Assert.IsFalse(diplomacyCore.TryAcceptProposal(unperformableProposal));
        }

        [Test(Description = "")]
        public void TryAcceptProposal_PerformsProposalIfPossible() {
            var sender   = BuildCivilization();
            var receiver = BuildCivilization();

            var performableProposal   = BuildProposal(sender, receiver, true, () => Assert.Pass());

            var unperformableProposal = BuildProposal(
                sender, receiver, false, () => Assert.Fail("unperformableProposal was performed")
            );

            var diplomacyCore = Container.Resolve<DiplomacyCore>();

            diplomacyCore.TryAcceptProposal(unperformableProposal);
            diplomacyCore.TryAcceptProposal(performableProposal);

            Assert.Fail("performableProposal was never performed");
        }

        [Test(Description = "When TryAcceptProposal is successfully executed, the accepted proposal " +
            "should no longer appear in GetProposalsMadeByCiv")]
        public void TryAcceptProposal_SuccessfulAcceptReflectedInGetProposalsMadeByCiv() {
            var sender   = BuildCivilization();
            var receiver = BuildCivilization();

            var proposal = BuildProposal(sender, receiver, true);

            var diplomacyCore = Container.Resolve<DiplomacyCore>();

            diplomacyCore.TryAcceptProposal(proposal);

            CollectionAssert.DoesNotContain(diplomacyCore.GetProposalsMadeByCiv(sender), proposal);
        }

        [Test(Description = "When TryAcceptProposal is successfully executed, the accepted proposal " +
            "should no longer appear in GetProposalsMadeToCiv")]
        public void TryAcceptProposal_SuccessfulAcceptReflectedInGetProposalsMadeToCiv() {
            var sender   = BuildCivilization();
            var receiver = BuildCivilization();

            var proposal = BuildProposal(sender, receiver, true);

            var diplomacyCore = Container.Resolve<DiplomacyCore>();

            diplomacyCore.TryAcceptProposal(proposal);

            CollectionAssert.DoesNotContain(diplomacyCore.GetProposalsMadeToCiv(receiver), proposal);
        }

        [Test(Description = "When RejectProposal is called, the rejected proposal should " +
            "no longer appear in GetProposalsMadeByCiv")]
        public void RejectProposal_ReflectedInGetProposalsMadeByCiv() {
            var sender   = BuildCivilization();
            var receiver = BuildCivilization();

            var proposal = BuildProposal(sender, receiver);

            var diplomacyCore = Container.Resolve<DiplomacyCore>();

            diplomacyCore.SendProposal(proposal);
            diplomacyCore.RejectProposal(proposal);

            CollectionAssert.DoesNotContain(diplomacyCore.GetProposalsMadeByCiv(sender), proposal);
        }

        [Test(Description = "When RejectProposal is called, the rejected proposal should " +
            "no longer appear in GetProposalsMadeToCiv")]
        public void RejectProposal_ReflectedInGetProposalsMadeToCiv() {
            var sender   = BuildCivilization();
            var receiver = BuildCivilization();

            var proposal = BuildProposal(sender, receiver);

            var diplomacyCore = Container.Resolve<DiplomacyCore>();

            diplomacyCore.SendProposal(proposal);
            diplomacyCore.RejectProposal(proposal);

            CollectionAssert.DoesNotContain(diplomacyCore.GetProposalsMadeToCiv(receiver), proposal);
        }

        #endregion

        #region utilities

        private ICivilization BuildCivilization() {
            return new Mock<ICivilization>().Object;
        }

        private IDiplomaticProposal BuildProposal(
            ICivilization sender, ICivilization receiver, bool canBePerformed = true,
            Action performCallback = null
        ) {
            var mockProposal = new Mock<IDiplomaticProposal>();

            mockProposal.Setup(proposal => proposal.Sender).Returns(sender);
            mockProposal.Setup(proposal => proposal.Receiver).Returns(receiver);
            mockProposal.Setup(proposal => proposal.CanPerformProposal()).Returns(canBePerformed);

            if(performCallback != null) {
                mockProposal.Setup(proposal => proposal.PerformProposal()).Callback(performCallback);
            }

            return mockProposal.Object;
        }

        #endregion

        #endregion

    }

}
