using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation;
using Assets.Simulation.Civilizations;
using Assets.Simulation.Cities;
using Assets.Simulation.Diplomacy;

using Assets.UI;

namespace Assets.Tests.Simulation.Diplomacy {

    [TestFixture]
    public class ExchangeBuilderTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IPossessionRelationship<ICivilization, ICity>> MockCityPossessionCanon;
        private Mock<IWarCanon>                                     MockWarCanon;
        private Mock<IResourceExchangeBuilder>                      MockResourceExchangeBuilder;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockCityPossessionCanon     = new Mock<IPossessionRelationship<ICivilization, ICity>>();
            MockWarCanon                = new Mock<IWarCanon>();
            MockResourceExchangeBuilder = new Mock<IResourceExchangeBuilder>();

            Container.Bind<IPossessionRelationship<ICivilization, ICity>>().FromInstance(MockCityPossessionCanon    .Object);
            Container.Bind<IWarCanon>                                    ().FromInstance(MockWarCanon               .Object);
            Container.Bind<IResourceExchangeBuilder>                     ().FromInstance(MockResourceExchangeBuilder.Object);

            Container.Bind<IYieldFormatter>().FromMock();

            Container.Bind<ExchangeBuilder>().AsSingle();
        }

        #endregion

        #region tests

        [Test(Description = "BuildAllValidExchangesBetween should assign a bilateral exchange " +
            "of type EstablishPeaceDiplomaticExchange if and only if sender and receiver can " + 
            "establish a peace through WarCanon")]
        public void BuildAllValidExchangesBetween_ContainsPeaceBilateralOnlyIfPeacePossible() {
            var sender   = BuildCivilization(new List<ICity>());
            var receiver = BuildCivilization(new List<ICity>());

            var exchangeBuilder = Container.Resolve<ExchangeBuilder>();

            var peaceResults = exchangeBuilder.BuildAllValidExchangesBetween(sender, receiver);

            MockWarCanon.Setup(canon => canon.CanEstablishPeace(sender, receiver)).Returns(true);
            
            var warResults = exchangeBuilder.BuildAllValidExchangesBetween(sender, receiver);

            foreach(var bilateral in peaceResults.BilateralExchanges) {
                Assert.IsFalse(
                    bilateral is EstablishPeaceDiplomaticExchange,
                    "Peace bilateral present between civilizations not at war"
                );
            }

            foreach(var bilateral in warResults.BilateralExchanges) {
                if(bilateral is EstablishPeaceDiplomaticExchange) {
                    Assert.Pass();
                }
            }

            Assert.Fail("Peace bilateral not present between civilizations at war");
        }

        [Test(Description = "BuildAllExchangesBetween should assign a single offer of type " +
            "GoldDiplomaticExchange if and only if sender has a GoldStockpile property greater than zero")]
        public void BuildAllExchangesBetween_ContainsGoldOfferIfSenderHasGold() {
            var senderOne = BuildCivilization(new List<ICity>(), 100);
            var senderTwo = BuildCivilization(new List<ICity>(), 0);
            var receiver  = BuildCivilization(new List<ICity>());

            var exchangeBuilder = Container.Resolve<ExchangeBuilder>();

            var senderOneResults = exchangeBuilder.BuildAllValidExchangesBetween(senderOne, receiver);
            var senderTwoResults = exchangeBuilder.BuildAllValidExchangesBetween(senderTwo, receiver);

            Assert.AreEqual(
                1, senderOneResults.AllPossibleOffersFromSender.Where(offer => offer is GoldDiplomaticExchange).Count(),
                "Gold offer not present when sender has gold to trade"
            );

            Assert.AreEqual(
                0, senderTwoResults.AllPossibleOffersFromSender.Where(offer => offer is GoldDiplomaticExchange).Count(),
                "Gold offer falsely present when sender has no gold to trade"
            );
        }

        [Test(Description = "BuildAllExchangesBetween should assign a single demand of type " +
            "GoldDiplomaticExchange if and only if receiver has a GoldStockpile property greater than zero")]
        public void BuildAllExchangesBetween_ContainsGoldDemandIfReceiverHasGold() {
            var sender = BuildCivilization(new List<ICity>());
            var receiverOne = BuildCivilization(new List<ICity>(), 100);
            var receiverTwo = BuildCivilization(new List<ICity>(), 0);

            var exchangeBuilder = Container.Resolve<ExchangeBuilder>();

            var receiverOneResults = exchangeBuilder.BuildAllValidExchangesBetween(sender, receiverOne);
            var receiverTwoResults = exchangeBuilder.BuildAllValidExchangesBetween(sender, receiverTwo);

            Assert.AreEqual(
                1, receiverOneResults.AllPossibleDemandsOfReceiver.Where(offer => offer is GoldDiplomaticExchange).Count(),
                "Gold offer not present when receiver has gold to trade"
            );

            Assert.AreEqual(
                0, receiverTwoResults.AllPossibleDemandsOfReceiver.Where(offer => offer is GoldDiplomaticExchange).Count(),
                "Gold offer falsely present when receiver has no gold to trade"
            );
        }

        [Test(Description = "BuildAllExchangesBetween should assign a single offer of type " +
            "CityDiplomaticExchange for each city belonging to the sender")]
        public void BuildAllExchangesBetween_ContainsOffersForCitiesOfSender() {
            var senderCities = new List<ICity>() {
                BuildCity(), BuildCity(), BuildCity()
            };

            var sender = BuildCivilization(senderCities);
            var receiver = BuildCivilization(new List<ICity>());

            var exchangeBuilder = Container.Resolve<ExchangeBuilder>();

            var exchangeSummary = exchangeBuilder.BuildAllValidExchangesBetween(sender, receiver);

            var cityExchanges = exchangeSummary.AllPossibleOffersFromSender
                .Where(offer => offer is CityDiplomaticExchange).Select(offer => offer as CityDiplomaticExchange);

            CollectionAssert.AreEquivalent(senderCities, cityExchanges.Select(offer => offer.CityToExchange));
        }

        [Test(Description = "BuildAllExchangesBetween should assign a single demand of type " +
            "CityDiplomaticExchange for each city belonging to the receiver")]
        public void BuildAllExchangesBetween_ContainsDemandsForCitiesOfReceiver() {
            var receiverCities = new List<ICity>() {
                BuildCity(), BuildCity(), BuildCity()
            };

            var sender = BuildCivilization(new List<ICity>());
            var receiver = BuildCivilization(receiverCities);

            var exchangeBuilder = Container.Resolve<ExchangeBuilder>();

            var exchangeSummary = exchangeBuilder.BuildAllValidExchangesBetween(sender, receiver);

            var cityExchanges = exchangeSummary.AllPossibleDemandsOfReceiver
                .Where(demand => demand is CityDiplomaticExchange).Select(demand => demand as CityDiplomaticExchange);

            CollectionAssert.AreEquivalent(receiverCities, cityExchanges.Select(demand => demand.CityToExchange));
        }

        [Test(Description = "")]
        public void BuildAllExchangesBetween_CallsIntoResourceExchangeBuilderProperly() {
            var sender   = BuildCivilization(new List<ICity>());
            var receiver = BuildCivilization(new List<ICity>());

            var exchangeBuilder = Container.Resolve<ExchangeBuilder>();

            exchangeBuilder.BuildAllValidExchangesBetween(sender, receiver);

            MockResourceExchangeBuilder.Verify(
                builder => builder.BuildResourceExchanges(sender, receiver, It.IsAny<ExchangeSummary>()),
                "BuildResourceExchanges was not called as expected"
            );
        }

        #endregion

        #region utilities

        private ICivilization BuildCivilization(List<ICity> cities, int goldStockpile = 0) {
            var mockCiv = new Mock<ICivilization>();

            mockCiv.Setup(civ => civ.GoldStockpile).Returns(goldStockpile);

            var newCiv = mockCiv.Object;

            MockCityPossessionCanon.Setup(canon => canon.GetPossessionsOfOwner(newCiv)).Returns(cities);

            return newCiv;
        }

        private ICity BuildCity() {
            return new Mock<ICity>().Object;
        }

        #endregion

        #endregion

    }

}
