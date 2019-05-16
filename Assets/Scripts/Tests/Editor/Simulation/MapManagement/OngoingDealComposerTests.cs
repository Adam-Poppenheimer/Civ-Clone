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
    public class OngoingDealComposerTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<ICivilizationFactory>               MockCivFactory;
        private Mock<IOngoingDiplomaticExchangeComposer> MockOngoingExchangeComposer;

        private List<ICivilization> AllCivs = new List<ICivilization>();

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            AllCivs.Clear();

            MockCivFactory              = new Mock<ICivilizationFactory>();
            MockOngoingExchangeComposer = new Mock<IOngoingDiplomaticExchangeComposer>();

            MockCivFactory.Setup(factory => factory.AllCivilizations).Returns(AllCivs.AsReadOnly());

            Container.Bind<ICivilizationFactory>              ().FromInstance(MockCivFactory             .Object);
            Container.Bind<IOngoingDiplomaticExchangeComposer>().FromInstance(MockOngoingExchangeComposer.Object);

            Container.Bind<OngoingDealComposer>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void ComposeOngoingDeal_StoresSenderAndReceiverAsNames() {
            var civOne = BuildCivilization("Civ One");
            var civTwo = BuildCivilization("Civ Two");

            var proposal = BuildDeal(civOne, civTwo, 0, null, null, null);

            var composer = Container.Resolve<OngoingDealComposer>();

            var serialDeal = composer.ComposeOngoingDeal(proposal);

            Assert.AreEqual("Civ One", serialDeal.Sender,   "Unexpected Sender value");
            Assert.AreEqual("Civ Two", serialDeal.Receiver, "Unexpected Receiver value");
        }

        [Test]
        public void ComposeOngoingDeal_StoresTurnsLeftProperly() {
            var civOne = BuildCivilization("Civ One");
            var civTwo = BuildCivilization("Civ Two");

            var deal = BuildDeal(
                civOne, civTwo, 15,
                new List<IOngoingDiplomaticExchange>(),
                new List<IOngoingDiplomaticExchange>(),
                new List<IOngoingDiplomaticExchange>()
            );

            var proposalComposer = Container.Resolve<OngoingDealComposer>();

            var serialDeal = proposalComposer.ComposeOngoingDeal(deal);

            Assert.AreEqual(15, serialDeal.TurnsLeft);
        }

        [Test]
        public void ComposeOngoingDeal_StoresOffersProvidedByExchangeComposer() {
            var civOne = BuildCivilization("Civ One");
            var civTwo = BuildCivilization("Civ Two");

            var exchangeOne = BuildExchange();
            var exchangeTwo = BuildExchange();

            var exchanges = new List<IOngoingDiplomaticExchange>() { exchangeOne, exchangeTwo };

            var serialExchangeOne = new SerializableOngoingDiplomaticExchange();
            var serialExchangeTwo = new SerializableOngoingDiplomaticExchange();

            var serialExchanges = new List<SerializableOngoingDiplomaticExchange>() {
                serialExchangeOne, serialExchangeTwo
            };

            MockOngoingExchangeComposer.Setup(composer => composer.ComposeOngoingExchange(exchangeOne)).Returns(serialExchangeOne);
            MockOngoingExchangeComposer.Setup(composer => composer.ComposeOngoingExchange(exchangeTwo)).Returns(serialExchangeTwo);

            var deal = BuildDeal(
                civOne, civTwo, 0, exchanges, new List<IOngoingDiplomaticExchange>(),
                new List<IOngoingDiplomaticExchange>()
            );

            var dealComposer = Container.Resolve<OngoingDealComposer>();

            var serialDeal = dealComposer.ComposeOngoingDeal(deal);

            CollectionAssert.AreEquivalent(
                serialExchanges, serialDeal.ExchangesFromSender
            );
        }

        [Test]
        public void ComposeOngoingDeal_StoresDemandsProvidedByExchangeComposer() {
            var civOne = BuildCivilization("Civ One");
            var civTwo = BuildCivilization("Civ Two");

            var exchangeOne = BuildExchange();
            var exchangeTwo = BuildExchange();

            var exchanges = new List<IOngoingDiplomaticExchange>() { exchangeOne, exchangeTwo };

            var serialExchangeOne = new SerializableOngoingDiplomaticExchange();
            var serialExchangeTwo = new SerializableOngoingDiplomaticExchange();

            var serialExchanges = new List<SerializableOngoingDiplomaticExchange>() {
                serialExchangeOne, serialExchangeTwo
            };

            MockOngoingExchangeComposer.Setup(composer => composer.ComposeOngoingExchange(exchangeOne)).Returns(serialExchangeOne);
            MockOngoingExchangeComposer.Setup(composer => composer.ComposeOngoingExchange(exchangeTwo)).Returns(serialExchangeTwo);

            var deal = BuildDeal(
                civOne, civTwo, 0, new List<IOngoingDiplomaticExchange>(), exchanges,
                new List<IOngoingDiplomaticExchange>()
            );

            var dealComposer = Container.Resolve<OngoingDealComposer>();

            var serialDeal = dealComposer.ComposeOngoingDeal(deal);

            CollectionAssert.AreEquivalent(
                serialExchanges, serialDeal.ExchangesFromReceiver
            );
        }

        [Test]
        public void ComposeOngoingDeal_StoresBilateralsProvidedByExchangeComposer() {
            var civOne = BuildCivilization("Civ One");
            var civTwo = BuildCivilization("Civ Two");

            var exchangeOne = BuildExchange();
            var exchangeTwo = BuildExchange();

            var exchanges = new List<IOngoingDiplomaticExchange>() { exchangeOne, exchangeTwo };

            var serialExchangeOne = new SerializableOngoingDiplomaticExchange();
            var serialExchangeTwo = new SerializableOngoingDiplomaticExchange();

            var serialExchanges = new List<SerializableOngoingDiplomaticExchange>() {
                serialExchangeOne, serialExchangeTwo
            };

            MockOngoingExchangeComposer.Setup(composer => composer.ComposeOngoingExchange(exchangeOne)).Returns(serialExchangeOne);
            MockOngoingExchangeComposer.Setup(composer => composer.ComposeOngoingExchange(exchangeTwo)).Returns(serialExchangeTwo);

            var deal = BuildDeal(
                civOne, civTwo, 0, new List<IOngoingDiplomaticExchange>(),
                new List<IOngoingDiplomaticExchange>(), exchanges
            );

            var dealComposer = Container.Resolve<OngoingDealComposer>();

            var serialDeal = dealComposer.ComposeOngoingDeal(deal);

            CollectionAssert.AreEquivalent(
                serialExchanges, serialDeal.BilateralExchanges
            );
        }

        [Test]
        public void DecomposeOngoingDeal_LoadsSenderAndReceiverProperly() {
            var civOne = BuildCivilization("Civ One");
            var civTwo = BuildCivilization("Civ Two");

            var serialDeal = new SerializableOngoingDealData() {
                Sender = "Civ One", Receiver = "Civ Two"
            };

            var proposalComposer = Container.Resolve<OngoingDealComposer>();

            var deal = proposalComposer.DecomposeOngoingDeal(serialDeal);

            Assert.AreEqual(civOne, deal.Sender,   "Unexpected sender in returned deal");
            Assert.AreEqual(civTwo, deal.Receiver, "Unexpected receiver in returned deal");
        }

        [Test]
        public void DecomposeOngoingDeal_LoadsTurnsLeftProperly() {
            BuildCivilization("Civ One");
            BuildCivilization("Civ Two");

            var serialDeal = new SerializableOngoingDealData() {
                Sender = "Civ One", Receiver = "Civ Two", TurnsLeft = 15
            };

            var proposalComposer = Container.Resolve<OngoingDealComposer>();

            var deal = proposalComposer.DecomposeOngoingDeal(serialDeal);

            Assert.AreEqual(15, deal.TurnsLeft);
        }

        [Test]
        public void DecomposeOngoingDeal_LoadsOffersFromExchangeComposer() {
            BuildCivilization("Civ One");
            BuildCivilization("Civ Two");

            var exchangeOne = BuildExchange();
            var exchangeTwo = BuildExchange();

            var exchanges = new List<IOngoingDiplomaticExchange>() { exchangeOne, exchangeTwo };

            var serialExchangeOne = new SerializableOngoingDiplomaticExchange();
            var serialExchangeTwo = new SerializableOngoingDiplomaticExchange();

            var serialExchanges = new List<SerializableOngoingDiplomaticExchange>() {
                serialExchangeOne, serialExchangeTwo
            };

            MockOngoingExchangeComposer.Setup(composer => composer.DecomposeOngoingExchange(serialExchangeOne)).Returns(exchangeOne);
            MockOngoingExchangeComposer.Setup(composer => composer.DecomposeOngoingExchange(serialExchangeTwo)).Returns(exchangeTwo);

            var serialDeal = new SerializableOngoingDealData() {
                Sender = "Civ One", Receiver = "Civ Two",
                ExchangesFromSender   = serialExchanges,
                ExchangesFromReceiver = new List<SerializableOngoingDiplomaticExchange>(),
                BilateralExchanges    = new List<SerializableOngoingDiplomaticExchange>(),
                TurnsLeft = 15
            };

            var proposalComposer = Container.Resolve<OngoingDealComposer>();

            var proposal = proposalComposer.DecomposeOngoingDeal(serialDeal);

            CollectionAssert.AreEquivalent(exchanges, proposal.ExchangesFromSender);
        }

        [Test]
        public void DecomposeOngoingDeal_LoadsDemandsFromExchangeComposer() {
            BuildCivilization("Civ One");
            BuildCivilization("Civ Two");

            var exchangeOne = BuildExchange();
            var exchangeTwo = BuildExchange();

            var exchanges = new List<IOngoingDiplomaticExchange>() { exchangeOne, exchangeTwo };

            var serialExchangeOne = new SerializableOngoingDiplomaticExchange();
            var serialExchangeTwo = new SerializableOngoingDiplomaticExchange();

            var serialExchanges = new List<SerializableOngoingDiplomaticExchange>() {
                serialExchangeOne, serialExchangeTwo
            };

            MockOngoingExchangeComposer.Setup(composer => composer.DecomposeOngoingExchange(serialExchangeOne)).Returns(exchangeOne);
            MockOngoingExchangeComposer.Setup(composer => composer.DecomposeOngoingExchange(serialExchangeTwo)).Returns(exchangeTwo);

            var serialDeal = new SerializableOngoingDealData() {
                Sender = "Civ One", Receiver = "Civ Two",
                ExchangesFromSender   = new List<SerializableOngoingDiplomaticExchange>(),
                ExchangesFromReceiver = serialExchanges,
                BilateralExchanges    = new List<SerializableOngoingDiplomaticExchange>(),
                TurnsLeft = 15
            };

            var proposalComposer = Container.Resolve<OngoingDealComposer>();

            var proposal = proposalComposer.DecomposeOngoingDeal(serialDeal);

            CollectionAssert.AreEquivalent(exchanges, proposal.ExchangesFromReceiver);
        }

        [Test]
        public void DecomposeOngoingDeal_LoadsBilateralsFromExchangeComposer() {
            BuildCivilization("Civ One");
            BuildCivilization("Civ Two");

            var exchangeOne = BuildExchange();
            var exchangeTwo = BuildExchange();

            var exchanges = new List<IOngoingDiplomaticExchange>() { exchangeOne, exchangeTwo };

            var serialExchangeOne = new SerializableOngoingDiplomaticExchange();
            var serialExchangeTwo = new SerializableOngoingDiplomaticExchange();

            var serialExchanges = new List<SerializableOngoingDiplomaticExchange>() {
                serialExchangeOne, serialExchangeTwo
            };

            MockOngoingExchangeComposer.Setup(composer => composer.DecomposeOngoingExchange(serialExchangeOne)).Returns(exchangeOne);
            MockOngoingExchangeComposer.Setup(composer => composer.DecomposeOngoingExchange(serialExchangeTwo)).Returns(exchangeTwo);

            var serialDeal = new SerializableOngoingDealData() {
                Sender = "Civ One", Receiver = "Civ Two",
                ExchangesFromSender   = new List<SerializableOngoingDiplomaticExchange>(),
                ExchangesFromReceiver = new List<SerializableOngoingDiplomaticExchange>(),
                BilateralExchanges    = serialExchanges,
                TurnsLeft = 15
            };

            var proposalComposer = Container.Resolve<OngoingDealComposer>();

            var proposal = proposalComposer.DecomposeOngoingDeal(serialDeal);

            CollectionAssert.AreEquivalent(exchanges, proposal.BilateralExchanges);
        }

        #endregion

        #region utilities

        private ICivilization BuildCivilization(string name) {
            var mockCiv = new Mock<ICivilization>();

            var mockTemplate = new Mock<ICivilizationTemplate>();

            mockTemplate.Setup(template => template.Name).Returns(name);

            mockCiv.Name = name;
            mockCiv.Setup(civ => civ.Template).Returns(mockTemplate.Object);

            var newCiv = mockCiv.Object;

            AllCivs.Add(newCiv);

            return newCiv;
        }

        private IOngoingDeal BuildDeal(
            ICivilization sender, ICivilization receiver, int turnsLeft,
            List<IOngoingDiplomaticExchange> offers, List<IOngoingDiplomaticExchange> demands,
            List<IOngoingDiplomaticExchange> bilaterals
        ) {
            var mockDeal = new Mock<IOngoingDeal>();

            mockDeal.SetupAllProperties();

            mockDeal.Setup(deal => deal.Sender)  .Returns(sender);
            mockDeal.Setup(deal => deal.Receiver).Returns(receiver);

            if(offers != null) {
                mockDeal.Setup(deal => deal.ExchangesFromSender).Returns(offers);
            }

            if(offers != null) {
                mockDeal.Setup(deal => deal.ExchangesFromReceiver).Returns(demands);
            }

            if(offers != null) {
                mockDeal.Setup(deal => deal.BilateralExchanges).Returns(bilaterals);
            }

            var newDeal = mockDeal.Object;

            newDeal.TurnsLeft = turnsLeft;

            return newDeal;
        }

        private IOngoingDiplomaticExchange BuildExchange() {
            var mockExchange = new Mock<IOngoingDiplomaticExchange>();

            return mockExchange.Object;
        }

        #endregion

        #endregion

    }

}
