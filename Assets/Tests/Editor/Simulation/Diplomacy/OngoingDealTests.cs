using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation.Diplomacy;
using Assets.Simulation.Civilizations;

namespace Assets.Tests.Simulation.Diplomacy {

    [TestFixture]
    public class OngoingDealTests {

        #region instance methods

        #region tests

        [Test(Description = "")]
        public void StartCalled_AllExchangesAlsoStarted() {
            var fromSenderMocks = new List<Mock<IOngoingDiplomaticExchange>>() {
                new Mock<IOngoingDiplomaticExchange>(), new Mock<IOngoingDiplomaticExchange>()
            };

            var fromReceiverMocks = new List<Mock<IOngoingDiplomaticExchange>>() {
                new Mock<IOngoingDiplomaticExchange>(), new Mock<IOngoingDiplomaticExchange>()
            };

            var bilateralMocks = new List<Mock<IOngoingDiplomaticExchange>>() {
                new Mock<IOngoingDiplomaticExchange>(), new Mock<IOngoingDiplomaticExchange>()
            };

            var ongoingDeal = new OngoingDeal(
                fromSenderMocks  .Select(mock => mock.Object).ToList(),
                fromReceiverMocks.Select(mock => mock.Object).ToList(),
                bilateralMocks   .Select(mock => mock.Object).ToList()
            );

            ongoingDeal.Start();

            foreach(var mock in fromSenderMocks) {
                mock.Verify(exchange => exchange.Start(), Times.Once, "a fromSender exchange's Start method was not called as expected");
            }

            foreach(var mock in fromReceiverMocks) {
                mock.Verify(exchange => exchange.Start(), Times.Once, "a fromReceiver exchange's Start method was not called as expected");
            }

            foreach(var mock in bilateralMocks) {
                mock.Verify(exchange => exchange.Start(), Times.Once, "a bilateral exchange's Start method was not called as expected");
            }
        }

        [Test(Description = "")]
        public void EndCalled_AllExchangesAlsoEnded() {
            var fromSenderMocks = new List<Mock<IOngoingDiplomaticExchange>>() {
                new Mock<IOngoingDiplomaticExchange>(), new Mock<IOngoingDiplomaticExchange>()
            };

            var fromReceiverMocks = new List<Mock<IOngoingDiplomaticExchange>>() {
                new Mock<IOngoingDiplomaticExchange>(), new Mock<IOngoingDiplomaticExchange>()
            };

            var bilateralMocks = new List<Mock<IOngoingDiplomaticExchange>>() {
                new Mock<IOngoingDiplomaticExchange>(), new Mock<IOngoingDiplomaticExchange>()
            };

            var ongoingDeal = new OngoingDeal(
                fromSenderMocks  .Select(mock => mock.Object).ToList(),
                fromReceiverMocks.Select(mock => mock.Object).ToList(),
                bilateralMocks   .Select(mock => mock.Object).ToList()
            );

            ongoingDeal.End();

            foreach(var mock in fromSenderMocks) {
                mock.Verify(exchange => exchange.End(), Times.Once, "a fromSender exchange's End method was not called as expected");
            }

            foreach(var mock in fromReceiverMocks) {
                mock.Verify(exchange => exchange.End(), Times.Once, "a fromReceiver exchange's End method was not called as expected");
            }

            foreach(var mock in bilateralMocks) {
                mock.Verify(exchange => exchange.End(), Times.Once, "a bilateral exchange's End method was not called as expected");
            }
        }

        [Test(Description = "")]
        public void SomeExchangeRequestsTermination_TerminationRequestedEventFired() {
            var fromSenderMocks = new List<Mock<IOngoingDiplomaticExchange>>() {
                new Mock<IOngoingDiplomaticExchange>(), new Mock<IOngoingDiplomaticExchange>()
            };

            var fromReceiverMocks = new List<Mock<IOngoingDiplomaticExchange>>() {
                new Mock<IOngoingDiplomaticExchange>(), new Mock<IOngoingDiplomaticExchange>()
            };

            var bilateralMocks = new List<Mock<IOngoingDiplomaticExchange>>() {
                new Mock<IOngoingDiplomaticExchange>(), new Mock<IOngoingDiplomaticExchange>()
            };

            var ongoingDeal = new OngoingDeal(
                fromSenderMocks  .Select(mock => mock.Object).ToList(),
                fromReceiverMocks.Select(mock => mock.Object).ToList(),
                bilateralMocks   .Select(mock => mock.Object).ToList()
            );

            ongoingDeal.Start();

            int terminationRequestsMade = 0;
            ongoingDeal.TerminationRequested += delegate(object sender, OngoingDealEventArgs e) {
                Assert.AreEqual(ongoingDeal, e.Deal, "TerminationRequested fired on an unexpected deal");
                terminationRequestsMade++;
            };

            foreach(var mockExchange in fromSenderMocks.Concat(fromReceiverMocks).Concat(bilateralMocks)) {
                mockExchange.Raise(exchange => exchange.TerminationRequested += null, EventArgs.Empty);
            }

            Assert.AreEqual(6, terminationRequestsMade, "An unexpected number of termination requests were made");
        }

        #endregion

        #region utilities



        #endregion

        #endregion

    }

}
