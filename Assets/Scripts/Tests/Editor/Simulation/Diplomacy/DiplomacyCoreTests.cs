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

        private Mock<IDiplomacyConfig> MockConfig;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockConfig = new Mock<IDiplomacyConfig>();

            Container.Bind<IDiplomacyConfig>().FromInstance(MockConfig.Object);

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

            CollectionAssert.Contains      (diplomacyCore.GetProposalsSentFromCiv(sender),   proposal);
            CollectionAssert.DoesNotContain(diplomacyCore.GetProposalsSentFromCiv(receiver), proposal);
        }

        [Test(Description = "When SendProposal is called, the sent proposal should appear in the " +
            "collection returned by GetProposalsMadeToCiv, but only for the receiving civilization")]
        public void SendProposal_ReflectedInGetProposalsMadeToCiv() {
            var sender   = BuildCivilization();
            var receiver = BuildCivilization();

            var proposal = BuildProposal(sender, receiver);

            var diplomacyCore = Container.Resolve<DiplomacyCore>();

            diplomacyCore.SendProposal(proposal);

            CollectionAssert.DoesNotContain(diplomacyCore.GetProposalsReceivedByCiv(sender),   proposal);
            CollectionAssert.Contains      (diplomacyCore.GetProposalsReceivedByCiv(receiver), proposal);
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

            CollectionAssert.DoesNotContain(diplomacyCore.GetProposalsSentFromCiv(sender), proposal);
        }

        [Test(Description = "When TryAcceptProposal is successfully executed, the accepted proposal " +
            "should no longer appear in GetProposalsMadeToCiv")]
        public void TryAcceptProposal_SuccessfulAcceptReflectedInGetProposalsMadeToCiv() {
            var sender   = BuildCivilization();
            var receiver = BuildCivilization();

            var proposal = BuildProposal(sender, receiver, true);

            var diplomacyCore = Container.Resolve<DiplomacyCore>();

            diplomacyCore.TryAcceptProposal(proposal);

            CollectionAssert.DoesNotContain(diplomacyCore.GetProposalsReceivedByCiv(receiver), proposal);
        }

        [Test(Description = "")]
        public void TryAcceptProposal_SubscribesOngoingDealIfOneIsReturned() {
            var sender   = BuildCivilization();
            var receiver = BuildCivilization();

            var ongoingDeal = BuildOngoingDeal(sender, receiver);

            var proposal = BuildProposal(sender, receiver, true, ongoingDealReturned: ongoingDeal);

            var diplomacyCore = Container.Resolve<DiplomacyCore>();

            diplomacyCore.TryAcceptProposal(proposal);

            CollectionAssert.Contains(
                diplomacyCore.GetOngoingDealsSentFromCiv(sender), ongoingDeal,
                "GetOngoingDealsSentFromCiv does not contain the deal returned by the accepted proposal"
            );
        }

        [Test(Description = "")]
        public void TryAcceptProposal_SubscribesNothingIfNullReturned() {
            var sender   = BuildCivilization();
            var receiver = BuildCivilization();

            var proposal = BuildProposal(sender, receiver, true, ongoingDealReturned: null);

            var diplomacyCore = Container.Resolve<DiplomacyCore>();

            diplomacyCore.TryAcceptProposal(proposal);

            CollectionAssert.IsEmpty(
                diplomacyCore.GetOngoingDealsSentFromCiv(sender),
                "GetOngoingDealsSentFromCiv unexpectedly non-empty"
            );
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

            CollectionAssert.DoesNotContain(diplomacyCore.GetProposalsSentFromCiv(sender), proposal);
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

            CollectionAssert.DoesNotContain(diplomacyCore.GetProposalsReceivedByCiv(receiver), proposal);
        }

        [Test(Description = "")]
        public void SubscribeOngoingDeal_DealStarted() {
            var ongoingDealMock = new Mock<IOngoingDeal>();

            var diplomacyCore = Container.Resolve<DiplomacyCore>();

            diplomacyCore.SubscribeOngoingDeal(ongoingDealMock.Object);

            ongoingDealMock.Verify(deal => deal.Start(), Times.Once, "The subscribed ongoing deal was not started");
        }

        [Test(Description = "")]
        public void SubscribeOngoingDeal_AppearsInOngoingDealsForSender() {
            var sender   = BuildCivilization();
            var receiver = BuildCivilization();

            var ongoingDeal = BuildOngoingDeal(sender, receiver);

            var diplomacyCore = Container.Resolve<DiplomacyCore>();

            diplomacyCore.SubscribeOngoingDeal(ongoingDeal);

            CollectionAssert.Contains(diplomacyCore.GetOngoingDealsSentFromCiv(sender), ongoingDeal);
        }

        [Test(Description = "")]
        public void SubscribeOngoingDeal_AppearsInOngoingReceivedDeals() {
            var sender   = BuildCivilization();
            var receiver = BuildCivilization();

            var ongoingDeal = BuildOngoingDeal(sender, receiver);

            var diplomacyCore = Container.Resolve<DiplomacyCore>();

            diplomacyCore.SubscribeOngoingDeal(ongoingDeal);

            CollectionAssert.Contains(diplomacyCore.GetOngoingDealsReceivedByCiv(receiver), ongoingDeal);
        }

        [Test(Description = "When SubscribeOngoingDeal is called on an IOngoingDeal, that deal's " +
            "TurnsLeft field is set to the value in IDiplomacyConfig.TradeDuration")]
        public void SubscribeOngoingDeal_TurnsLeftSetProperly() {
            var sender   = BuildCivilization();
            var receiver = BuildCivilization();

            var ongoingDeal = BuildOngoingDeal(sender, receiver);

            MockConfig.Setup(config => config.TradeDuration).Returns(10);

            var diplomacyCore = Container.Resolve<DiplomacyCore>();

            diplomacyCore.SubscribeOngoingDeal(ongoingDeal);

            Assert.AreEqual(10, ongoingDeal.TurnsLeft);
        }

        [Test(Description = "")]
        public void UnsubscribeOngoingDeal_DealEnded() {
            var ongoingDealMock = new Mock<IOngoingDeal>();

            var diplomacyCore = Container.Resolve<DiplomacyCore>();

            diplomacyCore.SubscribeOngoingDeal  (ongoingDealMock.Object);
            diplomacyCore.UnsubscribeOngoingDeal(ongoingDealMock.Object);

            ongoingDealMock.Verify(deal => deal.End(), Times.Once, "The unsubscribed ongoing deal was not ended");
        }

        [Test(Description = "")]
        public void UnubscribeOngoingDeal_RemovedFromOngoingDealsForSender() {
            var sender   = BuildCivilization();
            var receiver = BuildCivilization();

            var ongoingDeal = BuildOngoingDeal(sender, receiver);

            var diplomacyCore = Container.Resolve<DiplomacyCore>();

            diplomacyCore.SubscribeOngoingDeal  (ongoingDeal);
            diplomacyCore.UnsubscribeOngoingDeal(ongoingDeal);

            CollectionAssert.DoesNotContain(diplomacyCore.GetOngoingDealsSentFromCiv(sender), ongoingDeal);
        }

        [Test(Description = "")]
        public void UnubscribeOngoingDeal_RemovedFromOngoingDealsForReceiver() {
            var sender   = BuildCivilization();
            var receiver = BuildCivilization();

            var ongoingDeal = BuildOngoingDeal(sender, receiver);

            var diplomacyCore = Container.Resolve<DiplomacyCore>();

            diplomacyCore.SubscribeOngoingDeal  (ongoingDeal);
            diplomacyCore.UnsubscribeOngoingDeal(ongoingDeal);

            CollectionAssert.DoesNotContain(diplomacyCore.GetOngoingDealsReceivedByCiv(receiver), ongoingDeal);
        }

        [Test(Description = "")]
        public void UpdateOngoingDeals_TurnsLeftDecremented() {
            var sender   = BuildCivilization();
            var receiver = BuildCivilization();

            var dealOne = BuildOngoingDeal(sender, receiver);
            var dealTwo = BuildOngoingDeal(sender, receiver);

            var diplomacyCore = Container.Resolve<DiplomacyCore>();

            diplomacyCore.SubscribeOngoingDeal(dealOne);
            diplomacyCore.SubscribeOngoingDeal(dealTwo);

            dealOne.TurnsLeft = 5;
            dealTwo.TurnsLeft = 3;

            diplomacyCore.UpdateOngoingDeals();

            Assert.AreEqual(4, dealOne.TurnsLeft, "DealOne.TurnsLeft has an unexpected value");
            Assert.AreEqual(2, dealTwo.TurnsLeft, "DealTwo.TurnsLeft has an unexpected value");
        }

        [Test(Description = "")]
        public void UpdateOngoingDeals_DealsUnsubscribedIfTurnsLeftNotPositive() {
            var sender   = BuildCivilization();
            var receiver = BuildCivilization();

            var dealOne = BuildOngoingDeal(sender, receiver);
            var dealTwo = BuildOngoingDeal(sender, receiver);

            var diplomacyCore = Container.Resolve<DiplomacyCore>();

            diplomacyCore.SubscribeOngoingDeal(dealOne);
            diplomacyCore.SubscribeOngoingDeal(dealTwo);

            dealOne.TurnsLeft = 2;
            dealTwo.TurnsLeft = 1;

            diplomacyCore.UpdateOngoingDeals();

            CollectionAssert.Contains(
                diplomacyCore.GetOngoingDealsSentFromCiv(sender), dealOne,
                "falsely unsubscribed a deal that should not have expired"
            );

            CollectionAssert.DoesNotContain(
                diplomacyCore.GetOngoingDealsSentFromCiv(sender), dealTwo,
                "Failed to unsubscribe a deal that should've expired"
            );
        }

        [Test(Description = "")]
        public void DealFiresTerminationRequest_DealUnsubscribed() {
            var sender   = BuildCivilization();
            var receiver = BuildCivilization();

            Mock<IOngoingDeal> dealMock;
            var ongoingDeal = BuildOngoingDeal(sender, receiver, out dealMock);

            var diplomacyCore = Container.Resolve<DiplomacyCore>();

            diplomacyCore.SubscribeOngoingDeal(ongoingDeal);

            dealMock.Raise(deal => deal.TerminationRequested += null, new OngoingDealEventArgs(ongoingDeal));

            CollectionAssert.DoesNotContain(
                diplomacyCore.GetOngoingDealsSentFromCiv(sender), ongoingDeal,
                "Failed to unsubscribe a deal that requested its termination"
            );
        }

        [Test(Description = "")]
        public void ClearProposals_AllProposalsRemoved() {
            var civOne   = BuildCivilization();
            var civTwo   = BuildCivilization();
            var civThree = BuildCivilization();

            var oneTwoProposal   = BuildProposal(civOne, civTwo,   true);
            var oneThreeProposal = BuildProposal(civOne, civThree, true);

            var TwoOneProposal   = BuildProposal(civTwo, civOne,   true);
            var TwoThreeProposal = BuildProposal(civTwo, civThree, true);

            var ThreeOneProposal = BuildProposal(civThree, civOne, true);
            var ThreeTwoProposal = BuildProposal(civThree, civTwo, true);

            var diplomacyCore = Container.Resolve<DiplomacyCore>();

            diplomacyCore.SendProposal(oneTwoProposal);
            diplomacyCore.SendProposal(oneThreeProposal);

            diplomacyCore.SendProposal(TwoOneProposal);
            diplomacyCore.SendProposal(TwoThreeProposal);

            diplomacyCore.SendProposal(ThreeOneProposal);
            diplomacyCore.SendProposal(ThreeTwoProposal);

            diplomacyCore.ClearProposals();

            CollectionAssert.IsEmpty(
                diplomacyCore.GetProposalsSentFromCiv(civOne),
                "GetProposalsSentFromCiv(civOne) is non-empty"
            );

            CollectionAssert.IsEmpty(
                diplomacyCore.GetProposalsSentFromCiv(civTwo),
                "GetProposalsSentFromCiv(civTwo) is non-empty"
            );

            CollectionAssert.IsEmpty(
                diplomacyCore.GetProposalsSentFromCiv(civThree),
                "GetProposalsSentFromCiv(civThree) is non-empty"
            );

            CollectionAssert.IsEmpty(
                diplomacyCore.GetProposalsReceivedByCiv(civOne),
                "GetProposalsReceivedByCiv(civOne) is non-empty"
            );

            CollectionAssert.IsEmpty(
                diplomacyCore.GetProposalsReceivedByCiv(civTwo),
                "GetProposalsReceivedByCiv(civTwo) is non-empty"
            );

            CollectionAssert.IsEmpty(
                diplomacyCore.GetProposalsReceivedByCiv(civThree),
                "GetProposalsReceivedByCiv(civThree) is non-empty"
            );
        }

        [Test(Description = "")]
        public void ClearOngoingDeals_AllDealsCleared() {
            var civOne   = BuildCivilization();
            var civTwo   = BuildCivilization();
            var civThree = BuildCivilization();

            var oneTwoDeal   = BuildOngoingDeal(civOne, civTwo);
            var oneThreeDeal = BuildOngoingDeal(civOne, civThree);

            var TwoOneDeal   = BuildOngoingDeal(civTwo, civOne);
            var TwoThreeDeal = BuildOngoingDeal(civTwo, civThree);

            var ThreeOneDeal = BuildOngoingDeal(civThree, civOne);
            var ThreeTwoDeal = BuildOngoingDeal(civThree, civTwo);

            var diplomacyCore = Container.Resolve<DiplomacyCore>();

            diplomacyCore.SubscribeOngoingDeal(oneTwoDeal);
            diplomacyCore.SubscribeOngoingDeal(oneThreeDeal);

            diplomacyCore.SubscribeOngoingDeal(TwoOneDeal);
            diplomacyCore.SubscribeOngoingDeal(TwoThreeDeal);

            diplomacyCore.SubscribeOngoingDeal(ThreeOneDeal);
            diplomacyCore.SubscribeOngoingDeal(ThreeTwoDeal);

            diplomacyCore.ClearOngoingDeals();

            CollectionAssert.IsEmpty(
                diplomacyCore.GetOngoingDealsSentFromCiv(civOne),
                "GetOngoingDealsSentFromCiv(civOne) is non-empty"
            );

            CollectionAssert.IsEmpty(
                diplomacyCore.GetOngoingDealsSentFromCiv(civTwo),
                "GetOngoingDealsSentFromCiv(civTwo) is non-empty"
            );

            CollectionAssert.IsEmpty(
                diplomacyCore.GetOngoingDealsSentFromCiv(civThree),
                "GetOngoingDealsSentFromCiv(civThree) is non-empty"
            );

            CollectionAssert.IsEmpty(
                diplomacyCore.GetOngoingDealsReceivedByCiv(civOne),
                "GetOngoingDealsReceivedByCiv(civOne) is non-empty"
            );

            CollectionAssert.IsEmpty(
                diplomacyCore.GetOngoingDealsReceivedByCiv(civTwo),
                "GetOngoingDealsReceivedByCiv(civTwo) is non-empty"
            );

            CollectionAssert.IsEmpty(
                diplomacyCore.GetOngoingDealsReceivedByCiv(civThree),
                "GetOngoingDealsReceivedByCiv(civThree) is non-empty"
            );
        }

        #endregion

        #region utilities

        private ICivilization BuildCivilization() {
            return new Mock<ICivilization>().Object;
        }

        private IDiplomaticProposal BuildProposal(
            ICivilization sender, ICivilization receiver, bool canBePerformed = true,
            Action performCallback = null, IOngoingDeal ongoingDealReturned = null
        ) {
            var mockProposal = new Mock<IDiplomaticProposal>();

            mockProposal.Setup(proposal => proposal.Sender).Returns(sender);
            mockProposal.Setup(proposal => proposal.Receiver).Returns(receiver);
            mockProposal.Setup(proposal => proposal.CanPerformProposal()).Returns(canBePerformed);

            mockProposal.Setup(proposal => proposal.PerformProposal()).Returns(ongoingDealReturned);

            if(performCallback != null) {
                mockProposal.Setup(proposal => proposal.PerformProposal()).Callback(performCallback);
            }

            return mockProposal.Object;
        }

        private IOngoingDeal BuildOngoingDeal(ICivilization sender, ICivilization receiver) {
            Mock<IOngoingDeal> mock;
            return BuildOngoingDeal(sender, receiver, out mock);
        }

        private IOngoingDeal BuildOngoingDeal(
            ICivilization sender, ICivilization receiver, out Mock<IOngoingDeal> mock
        ){
            mock = new Mock<IOngoingDeal>();

            mock.SetupAllProperties();
            mock.Setup(deal => deal.Sender)  .Returns(sender);
            mock.Setup(deal => deal.Receiver).Returns(receiver);

            var newDeal = mock.Object;

            return newDeal;
        }

        #endregion

        #endregion

    }

}
