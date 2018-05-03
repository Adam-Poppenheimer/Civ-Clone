using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation.MapManagement;
using Assets.Simulation.Diplomacy;
using Assets.Simulation.Civilizations;

namespace Assets.Tests.Simulation.MapManagement {

    [TestFixture]
    public class DiplomaticProposalComposerTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<ICivilizationFactory>        MockCivFactory;
        private Mock<IDiplomaticExchangeComposer> MockExchangeComposer;

        private List<ICivilization> AllCivs = new List<ICivilization>();

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            AllCivs.Clear();

            MockCivFactory       = new Mock<ICivilizationFactory>();
            MockExchangeComposer = new Mock<IDiplomaticExchangeComposer>();

            MockCivFactory.Setup(factory => factory.AllCivilizations).Returns(AllCivs.AsReadOnly());

            Container.Bind<ICivilizationFactory>       ().FromInstance(MockCivFactory      .Object);
            Container.Bind<IDiplomaticExchangeComposer>().FromInstance(MockExchangeComposer.Object);

            Container.Bind<DiplomaticProposalComposer>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void ComposeProposal_StoresSenderAndReceiverByName() {
            var civOne = BuildCivilization("Civ One");
            var civTwo = BuildCivilization("Civ Two");

            var proposal = BuildProposal(civOne, civTwo, null, null, null);

            var composer = Container.Resolve<DiplomaticProposalComposer>();

            var serialProposal = composer.ComposeProposal(proposal);

            Assert.AreEqual("Civ One", serialProposal.Sender,   "Unexpected Sender value");
            Assert.AreEqual("Civ Two", serialProposal.Receiver, "Unexpected Receiver value");
        }

        [Test]
        public void ComposeProposal_StoresOffersProvidedByExchangeComposer() {
            var civOne = BuildCivilization("Civ One");
            var civTwo = BuildCivilization("Civ Two");

            var exchangeOne = BuildExchange();
            var exchangeTwo = BuildExchange();

            var exchanges = new List<IDiplomaticExchange>() { exchangeOne, exchangeTwo };

            var serialExchangeOne = new SerializableDiplomaticExchangeData();
            var serialExchangeTwo = new SerializableDiplomaticExchangeData();

            var serialExchanges = new List<SerializableDiplomaticExchangeData>() {
                serialExchangeOne, serialExchangeTwo
            };

            MockExchangeComposer.Setup(composer => composer.ComposeExchange(exchangeOne)).Returns(serialExchangeOne);
            MockExchangeComposer.Setup(composer => composer.ComposeExchange(exchangeTwo)).Returns(serialExchangeTwo);

            var proposal = BuildProposal(
                civOne, civTwo, exchanges, new List<IDiplomaticExchange>(),
                new List<IDiplomaticExchange>()
            );

            var proposalComposer = Container.Resolve<DiplomaticProposalComposer>();

            var serialProposal = proposalComposer.ComposeProposal(proposal);

            CollectionAssert.AreEquivalent(
                serialExchanges, serialProposal.OfferedBySender
            );
        }

        [Test]
        public void ComposeProposal_StoresDemandsProvidedByExchangeComposer() {
            var civOne = BuildCivilization("Civ One");
            var civTwo = BuildCivilization("Civ Two");

            var exchangeOne = BuildExchange();
            var exchangeTwo = BuildExchange();

            var exchanges = new List<IDiplomaticExchange>() { exchangeOne, exchangeTwo };

            var serialExchangeOne = new SerializableDiplomaticExchangeData();
            var serialExchangeTwo = new SerializableDiplomaticExchangeData();

            var serialExchanges = new List<SerializableDiplomaticExchangeData>() {
                serialExchangeOne, serialExchangeTwo
            };

            MockExchangeComposer.Setup(composer => composer.ComposeExchange(exchangeOne)).Returns(serialExchangeOne);
            MockExchangeComposer.Setup(composer => composer.ComposeExchange(exchangeTwo)).Returns(serialExchangeTwo);

            var proposal = BuildProposal(
                civOne, civTwo, new List<IDiplomaticExchange>(), exchanges,
                new List<IDiplomaticExchange>()
            );

            var proposalComposer = Container.Resolve<DiplomaticProposalComposer>();

            var serialProposal = proposalComposer.ComposeProposal(proposal);

            CollectionAssert.AreEquivalent(
                serialExchanges, serialProposal.DemandedOfReceiver
            );
        }

        [Test]
        public void ComposeProposal_StoresBilateralsProvidedByExchangeComposer() {
            var civOne = BuildCivilization("Civ One");
            var civTwo = BuildCivilization("Civ Two");

            var exchangeOne = BuildExchange();
            var exchangeTwo = BuildExchange();

            var exchanges = new List<IDiplomaticExchange>() { exchangeOne, exchangeTwo };

            var serialExchangeOne = new SerializableDiplomaticExchangeData();
            var serialExchangeTwo = new SerializableDiplomaticExchangeData();

            var serialExchanges = new List<SerializableDiplomaticExchangeData>() {
                serialExchangeOne, serialExchangeTwo
            };

            MockExchangeComposer.Setup(composer => composer.ComposeExchange(exchangeOne)).Returns(serialExchangeOne);
            MockExchangeComposer.Setup(composer => composer.ComposeExchange(exchangeTwo)).Returns(serialExchangeTwo);

            var proposal = BuildProposal(
                civOne, civTwo, new List<IDiplomaticExchange>(),
                new List<IDiplomaticExchange>(), exchanges
            );

            var proposalComposer = Container.Resolve<DiplomaticProposalComposer>();

            var serialProposal = proposalComposer.ComposeProposal(proposal);

            CollectionAssert.AreEquivalent(
                serialExchanges, serialProposal.BilateralExchanges
            );
        }

        [Test]
        public void DecomposeProposal_LoadsSenderAndReceiverProperly() {
            var civOne = BuildCivilization("Civ One");
            var civTwo = BuildCivilization("Civ Two");

            var serialProposal = new SerializableProposalData() {
                Sender = "Civ One", Receiver = "Civ Two"
            };

            var proposalComposer = Container.Resolve<DiplomaticProposalComposer>();

            var proposal = proposalComposer.DecomposeProposal(serialProposal);

            Assert.AreEqual(civOne, proposal.Sender,   "Unexpected sender in returned proposal");
            Assert.AreEqual(civTwo, proposal.Receiver, "Unexpected receiver in returned proposal");
        }

        [Test]
        public void DecomposeProposal_LoadsOffersFromExchangeComposer() {
            BuildCivilization("Civ One");
            BuildCivilization("Civ Two");

            var exchangeOne = BuildExchange();
            var exchangeTwo = BuildExchange();

            var exchanges = new List<IDiplomaticExchange>() { exchangeOne, exchangeTwo };

            var serialExchangeOne = new SerializableDiplomaticExchangeData();
            var serialExchangeTwo = new SerializableDiplomaticExchangeData();

            var serialExchanges = new List<SerializableDiplomaticExchangeData>() {
                serialExchangeOne, serialExchangeTwo
            };

            MockExchangeComposer.Setup(composer => composer.DecomposeExchange(serialExchangeOne)).Returns(exchangeOne);
            MockExchangeComposer.Setup(composer => composer.DecomposeExchange(serialExchangeTwo)).Returns(exchangeTwo);

            var serialProposal = new SerializableProposalData() {
                Sender = "Civ One", Receiver = "Civ Two",
                OfferedBySender    = serialExchanges,
                DemandedOfReceiver = new List<SerializableDiplomaticExchangeData>(),
                BilateralExchanges = new List<SerializableDiplomaticExchangeData>()
            };

            var proposalComposer = Container.Resolve<DiplomaticProposalComposer>();

            var proposal = proposalComposer.DecomposeProposal(serialProposal);

            CollectionAssert.AreEquivalent(exchanges, proposal.OfferedBySender);
        }

        [Test]
        public void DecomposeProposal_LoadsDemandsFromExchangeComposer() {
            BuildCivilization("Civ One");
            BuildCivilization("Civ Two");

            var exchangeOne = BuildExchange();
            var exchangeTwo = BuildExchange();

            var exchanges = new List<IDiplomaticExchange>() { exchangeOne, exchangeTwo };

            var serialExchangeOne = new SerializableDiplomaticExchangeData();
            var serialExchangeTwo = new SerializableDiplomaticExchangeData();

            var serialExchanges = new List<SerializableDiplomaticExchangeData>() {
                serialExchangeOne, serialExchangeTwo
            };

            MockExchangeComposer.Setup(composer => composer.DecomposeExchange(serialExchangeOne)).Returns(exchangeOne);
            MockExchangeComposer.Setup(composer => composer.DecomposeExchange(serialExchangeTwo)).Returns(exchangeTwo);

            var serialProposal = new SerializableProposalData() {
                Sender = "Civ One", Receiver = "Civ Two",
                OfferedBySender    = new List<SerializableDiplomaticExchangeData>(),
                DemandedOfReceiver = serialExchanges,
                BilateralExchanges = new List<SerializableDiplomaticExchangeData>()
            };

            var proposalComposer = Container.Resolve<DiplomaticProposalComposer>();

            var proposal = proposalComposer.DecomposeProposal(serialProposal);

            CollectionAssert.AreEquivalent(exchanges, proposal.DemandedOfReceiver);
        }

        [Test]
        public void DecomposeProposal_LoadsBilateralsFromExchangeComposer() {
            BuildCivilization("Civ One");
            BuildCivilization("Civ Two");

            var exchangeOne = BuildExchange();
            var exchangeTwo = BuildExchange();

            var exchanges = new List<IDiplomaticExchange>() { exchangeOne, exchangeTwo };

            var serialExchangeOne = new SerializableDiplomaticExchangeData();
            var serialExchangeTwo = new SerializableDiplomaticExchangeData();

            var serialExchanges = new List<SerializableDiplomaticExchangeData>() {
                serialExchangeOne, serialExchangeTwo
            };

            MockExchangeComposer.Setup(composer => composer.DecomposeExchange(serialExchangeOne)).Returns(exchangeOne);
            MockExchangeComposer.Setup(composer => composer.DecomposeExchange(serialExchangeTwo)).Returns(exchangeTwo);

            var serialProposal = new SerializableProposalData() {
                Sender = "Civ One", Receiver = "Civ Two",
                OfferedBySender    = new List<SerializableDiplomaticExchangeData>(),
                DemandedOfReceiver = new List<SerializableDiplomaticExchangeData>(),
                BilateralExchanges = serialExchanges
            };

            var proposalComposer = Container.Resolve<DiplomaticProposalComposer>();

            var proposal = proposalComposer.DecomposeProposal(serialProposal);

            CollectionAssert.AreEquivalent(exchanges, proposal.BilateralExchanges);
        }

        #endregion

        #region utilities

        private ICivilization BuildCivilization(string name) {
            var mockCiv = new Mock<ICivilization>();

            mockCiv.Name = name;
            mockCiv.Setup(civ => civ.Name).Returns(name);

            var newCiv = mockCiv.Object;

            AllCivs.Add(newCiv);

            return newCiv;
        }

        private IDiplomaticProposal BuildProposal(
            ICivilization sender, ICivilization receiver,
            List<IDiplomaticExchange> offers, List<IDiplomaticExchange> demands,
            List<IDiplomaticExchange> bilaterals
        ) {
            var mockProposal = new Mock<IDiplomaticProposal>();

            mockProposal.Setup(proposal => proposal.Sender)  .Returns(sender);
            mockProposal.Setup(proposal => proposal.Receiver).Returns(receiver);

            if(offers != null) {
                mockProposal.Setup(proposal => proposal.OfferedBySender).Returns(offers);
            }

            if(offers != null) {
                mockProposal.Setup(proposal => proposal.DemandedOfReceiver).Returns(demands);
            }

            if(offers != null) {
                mockProposal.Setup(proposal => proposal.BilateralExchanges).Returns(bilaterals);
            }

            var newProposal = mockProposal.Object;

            return newProposal;
        }

        private IDiplomaticExchange BuildExchange() {
            var mockExchange = new Mock<IDiplomaticExchange>();

            mockExchange.Setup(
                exchange => exchange.CanExecuteBetweenCivs(It.IsAny<ICivilization>(), It.IsAny<ICivilization>())
            ).Returns(true);

            return mockExchange.Object;
        }

        #endregion

        #endregion

    }

}
