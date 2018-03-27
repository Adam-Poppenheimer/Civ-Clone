using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation.Civilizations;
using Assets.Simulation.Diplomacy;

namespace Assets.Tests.Simulation.Diplomacy {

    public class DiplomaticProposalTests : ZenjectUnitTestFixture {

        #region internal types

        public class CanAddExchangesTestData {

            public List<DiplomaticExchangeTestData> OfferedBySender = new List<DiplomaticExchangeTestData>();

            public List<DiplomaticExchangeTestData> DemandedOfReceiver = new List<DiplomaticExchangeTestData>();

            public List<DiplomaticExchangeTestData> BilateralExchanges = new List<DiplomaticExchangeTestData>();

            public DiplomaticExchangeTestData ExchangeToAdd;

        }

        public struct AddExchangeResults {

            public bool CanAddAsOffer;

            public bool CanAddAsDemand;

            public bool CanAddAsBilateral;

        }

        public class CanPerformProposalTestData {

            public List<DiplomaticExchangeTestData> OfferedBySender = new List<DiplomaticExchangeTestData>();

            public List<DiplomaticExchangeTestData> DemandedOfReceiver = new List<DiplomaticExchangeTestData>();

            public List<DiplomaticExchangeTestData> BilateralExchanges = new List<DiplomaticExchangeTestData>();

        }

        public class DiplomaticExchangeTestData {

            public bool CanBeExecuted = true;

            public bool OverlapsWithOtherExchanges = false;

        }

        #endregion

        #region static fields and properties

        public static IEnumerable CanAddAsOfferTestCases {
            get {
                yield return new TestCaseData(new CanAddExchangesTestData() {
                    ExchangeToAdd = new DiplomaticExchangeTestData() { CanBeExecuted = true, }

                }).SetName("Executable exchange, empty proposal").Returns(new AddExchangeResults() {
                    CanAddAsOffer = true, CanAddAsDemand = true, CanAddAsBilateral = true
                });

                yield return new TestCaseData(new CanAddExchangesTestData() {
                    ExchangeToAdd = new DiplomaticExchangeTestData() { CanBeExecuted = true, },
                    OfferedBySender = new List<DiplomaticExchangeTestData>() {
                        new DiplomaticExchangeTestData() { OverlapsWithOtherExchanges = false }
                    }

                }).SetName("Executable exchange, non-overlapping offers").Returns(new AddExchangeResults() {
                    CanAddAsOffer = true, CanAddAsDemand = true, CanAddAsBilateral = true
                });

                yield return new TestCaseData(new CanAddExchangesTestData() {
                    ExchangeToAdd = new DiplomaticExchangeTestData() { CanBeExecuted = true, },
                    OfferedBySender = new List<DiplomaticExchangeTestData>() {
                        new DiplomaticExchangeTestData() { OverlapsWithOtherExchanges = true }
                    }

                }).SetName("Executable exchange, overlapping offers").Returns(new AddExchangeResults() {
                    CanAddAsOffer = false, CanAddAsDemand = false, CanAddAsBilateral = false
                });

                yield return new TestCaseData(new CanAddExchangesTestData() {
                    ExchangeToAdd = new DiplomaticExchangeTestData() { CanBeExecuted = true, },
                    DemandedOfReceiver = new List<DiplomaticExchangeTestData>() {
                        new DiplomaticExchangeTestData() { OverlapsWithOtherExchanges = false }
                    }

                }).SetName("Executable exchange, non-overlapping demands").Returns(new AddExchangeResults() {
                    CanAddAsOffer = true, CanAddAsDemand = true, CanAddAsBilateral = true
                });

                yield return new TestCaseData(new CanAddExchangesTestData() {
                    ExchangeToAdd = new DiplomaticExchangeTestData() { CanBeExecuted = true, },
                    DemandedOfReceiver = new List<DiplomaticExchangeTestData>() {
                        new DiplomaticExchangeTestData() { OverlapsWithOtherExchanges = true }
                    }

                }).SetName("Executable exchange, overlapping demands").Returns(new AddExchangeResults() {
                    CanAddAsOffer = false, CanAddAsDemand = false, CanAddAsBilateral = false
                });

                yield return new TestCaseData(new CanAddExchangesTestData() {
                    ExchangeToAdd = new DiplomaticExchangeTestData() { CanBeExecuted = true, },
                    BilateralExchanges = new List<DiplomaticExchangeTestData>() {
                        new DiplomaticExchangeTestData() { OverlapsWithOtherExchanges = false }
                    }

                }).SetName("Executable exchange, non-overlapping bilaterals").Returns(new AddExchangeResults() {
                    CanAddAsOffer = true, CanAddAsDemand = true, CanAddAsBilateral = true
                });

                yield return new TestCaseData(new CanAddExchangesTestData() {
                    ExchangeToAdd = new DiplomaticExchangeTestData() { CanBeExecuted = true, },
                    BilateralExchanges = new List<DiplomaticExchangeTestData>() {
                        new DiplomaticExchangeTestData() { OverlapsWithOtherExchanges = true }
                    }

                }).SetName("Executable exchange, overlapping bilaterals").Returns(new AddExchangeResults() {
                    CanAddAsOffer = false, CanAddAsDemand = false, CanAddAsBilateral = false
                });
            }
        }

        public static IEnumerable CanPerformProposalTestCases {
            get {
                yield return new TestCaseData(new CanPerformProposalTestData() {

                }).SetName("Empty proposal").Returns(true);

                yield return new TestCaseData(new CanPerformProposalTestData() {
                    OfferedBySender = new List<DiplomaticExchangeTestData>() {
                        new DiplomaticExchangeTestData() { CanBeExecuted = true },
                        new DiplomaticExchangeTestData() { CanBeExecuted = true },
                        new DiplomaticExchangeTestData() { CanBeExecuted = true },
                    },
                    DemandedOfReceiver = new List<DiplomaticExchangeTestData>() {
                        new DiplomaticExchangeTestData() { CanBeExecuted = true },
                        new DiplomaticExchangeTestData() { CanBeExecuted = true },
                        new DiplomaticExchangeTestData() { CanBeExecuted = true },
                    },
                    BilateralExchanges = new List<DiplomaticExchangeTestData>() {
                        new DiplomaticExchangeTestData() { CanBeExecuted = true },
                        new DiplomaticExchangeTestData() { CanBeExecuted = true },
                        new DiplomaticExchangeTestData() { CanBeExecuted = true },
                    },
                }).SetName("Assorted exchanges, all executable").Returns(true);

                yield return new TestCaseData(new CanPerformProposalTestData() {
                    OfferedBySender = new List<DiplomaticExchangeTestData>() {
                        new DiplomaticExchangeTestData() { CanBeExecuted = true },
                        new DiplomaticExchangeTestData() { CanBeExecuted = false },
                        new DiplomaticExchangeTestData() { CanBeExecuted = true },
                    },
                    DemandedOfReceiver = new List<DiplomaticExchangeTestData>() {
                        new DiplomaticExchangeTestData() { CanBeExecuted = true },
                        new DiplomaticExchangeTestData() { CanBeExecuted = true },
                        new DiplomaticExchangeTestData() { CanBeExecuted = true },
                    },
                    BilateralExchanges = new List<DiplomaticExchangeTestData>() {
                        new DiplomaticExchangeTestData() { CanBeExecuted = true },
                        new DiplomaticExchangeTestData() { CanBeExecuted = true },
                        new DiplomaticExchangeTestData() { CanBeExecuted = true },
                    },
                }).SetName("Non-executable offer").Returns(false);

                yield return new TestCaseData(new CanPerformProposalTestData() {
                    OfferedBySender = new List<DiplomaticExchangeTestData>() {
                        new DiplomaticExchangeTestData() { CanBeExecuted = true },
                        new DiplomaticExchangeTestData() { CanBeExecuted = true },
                        new DiplomaticExchangeTestData() { CanBeExecuted = true },
                    },
                    DemandedOfReceiver = new List<DiplomaticExchangeTestData>() {
                        new DiplomaticExchangeTestData() { CanBeExecuted = false },
                        new DiplomaticExchangeTestData() { CanBeExecuted = true },
                        new DiplomaticExchangeTestData() { CanBeExecuted = true },
                    },
                    BilateralExchanges = new List<DiplomaticExchangeTestData>() {
                        new DiplomaticExchangeTestData() { CanBeExecuted = true },
                        new DiplomaticExchangeTestData() { CanBeExecuted = true },
                        new DiplomaticExchangeTestData() { CanBeExecuted = true },
                    },
                }).SetName("Non-executable demand").Returns(false);

                yield return new TestCaseData(new CanPerformProposalTestData() {
                    OfferedBySender = new List<DiplomaticExchangeTestData>() {
                        new DiplomaticExchangeTestData() { CanBeExecuted = true },
                        new DiplomaticExchangeTestData() { CanBeExecuted = true },
                        new DiplomaticExchangeTestData() { CanBeExecuted = true },
                    },
                    DemandedOfReceiver = new List<DiplomaticExchangeTestData>() {
                        new DiplomaticExchangeTestData() { CanBeExecuted = true },
                        new DiplomaticExchangeTestData() { CanBeExecuted = true },
                        new DiplomaticExchangeTestData() { CanBeExecuted = true },
                    },
                    BilateralExchanges = new List<DiplomaticExchangeTestData>() {
                        new DiplomaticExchangeTestData() { CanBeExecuted = true },
                        new DiplomaticExchangeTestData() { CanBeExecuted = true },
                        new DiplomaticExchangeTestData() { CanBeExecuted = false },
                    },
                }).SetName("Non-executable bilateral").Returns(false);
            }
        }

        #endregion

        #region instance fields and properties



        #endregion

        #region instance methods

        #region setup



        #endregion

        #region tests

        [TestCaseSource("CanAddAsOfferTestCases")]
        [Test(Description = "")]
        public AddExchangeResults CanAddExchangesTests(CanAddExchangesTestData testData) {
            var proposal = new DiplomaticProposal(BuildCivilization(), BuildCivilization());

            foreach(var offer in testData.OfferedBySender) {
                proposal.AddAsOffer(BuildExchange(offer));
            }

            foreach(var demand in testData.DemandedOfReceiver) {
                proposal.AddAsDemand(BuildExchange(demand));
            }

            foreach(var bilateral in testData.BilateralExchanges) {
                proposal.AddAsBilateralExchange(BuildExchange(bilateral));
            }

            var newExchange = BuildExchange(testData.ExchangeToAdd);

            return new AddExchangeResults() {
                CanAddAsOffer     = proposal.CanAddAsOffer            (newExchange),
                CanAddAsDemand    = proposal.CanAddAsDemand           (newExchange),
                CanAddAsBilateral = proposal.CanAddAsBilateralExchange(newExchange),
            };
        }

        [TestCaseSource("CanPerformProposalTestCases")]
        [Test(Description = "")]
        public bool CanPerformProposalTests(CanPerformProposalTestData testData) {
            var sender   = BuildCivilization();
            var receiver = BuildCivilization();

            var proposal = new DiplomaticProposal(sender, receiver);

            foreach(var offer in testData.OfferedBySender) {
                var mockExchange = BuildMockExchange();

                proposal.AddAsOffer(mockExchange.Object);

                mockExchange.Setup(
                    exchange => exchange.CanExecuteBetweenCivs(sender, receiver)
                ).Returns(offer.CanBeExecuted);
            }

            foreach(var demand in testData.DemandedOfReceiver) {
                var mockExchange = BuildMockExchange();

                proposal.AddAsDemand(mockExchange.Object);

                mockExchange.Setup(
                    exchange => exchange.CanExecuteBetweenCivs(receiver, sender)
                ).Returns(demand.CanBeExecuted);
            }

            foreach(var bilateral in testData.BilateralExchanges) {
                var mockExchange = BuildMockExchange();

                proposal.AddAsBilateralExchange(mockExchange.Object);

                mockExchange.Setup(
                    exchange => exchange.CanExecuteBetweenCivs(sender, receiver)
                ).Returns(bilateral.CanBeExecuted);
            }

            return proposal.CanPerformProposal();
        }

        [Test(Description = "When PerformProposal is called, every exchange attached to " +
            "the proposal should be executed")]
        public void PerformProposal_AllExchangesExecuted() {
            var sender   = BuildCivilization();
            var receiver = BuildCivilization();

            var proposal = new DiplomaticProposal(sender, receiver);

            var nonDemandExchangeMocks = new List<Mock<IDiplomaticExchange>>();
            var demandExchangeMocks    = new List<Mock<IDiplomaticExchange>>();

            for(int i = 0; i < 3; i++) {
                var mockOffer     = BuildMockExchange();
                var mockDemand    = BuildMockExchange();
                var mockBilateral = BuildMockExchange();

                proposal.AddAsOffer            (mockOffer    .Object);
                proposal.AddAsDemand           (mockDemand   .Object);
                proposal.AddAsBilateralExchange(mockBilateral.Object);

                nonDemandExchangeMocks.Add(mockOffer);
                nonDemandExchangeMocks.Add(mockBilateral);

                demandExchangeMocks.Add(mockDemand);
            }

            proposal.PerformProposal();

            foreach(var exchangeMock in nonDemandExchangeMocks) {
                exchangeMock.Verify(exchange => exchange.ExecuteBetweenCivs(sender, receiver), Times.Once);
            }

            foreach(var exchangeMock in demandExchangeMocks) {
                exchangeMock.Verify(exchange => exchange.ExecuteBetweenCivs(receiver, sender), Times.Once);
            }
        }

        #endregion

        #region utilities

        private ICivilization BuildCivilization() {
            return new Mock<ICivilization>().Object;
        }

        private IDiplomaticExchange BuildExchange(DiplomaticExchangeTestData exchangeData) {
            var mockExchange = new Mock<IDiplomaticExchange>();

            mockExchange.Setup(
                exchange => exchange.OverlapsWithExchange(It.IsAny<IDiplomaticExchange>())
            ).Returns(exchangeData.OverlapsWithOtherExchanges);

            mockExchange.Setup(
                exchange => exchange.CanExecuteBetweenCivs(It.IsAny<ICivilization>(), It.IsAny<ICivilization>())
            ).Returns(exchangeData.CanBeExecuted);

            return mockExchange.Object;
        }

        private Mock<IDiplomaticExchange> BuildMockExchange() {
            var mockExchange = new Mock<IDiplomaticExchange>();

            mockExchange.Setup(
                exchange => exchange.CanExecuteBetweenCivs(It.IsAny<ICivilization>(), It.IsAny<ICivilization>())
            ).Returns(true);

            mockExchange.Setup(
                exchange => exchange.OverlapsWithExchange(It.IsAny<IDiplomaticExchange>())
            ).Returns(false);

            return mockExchange;
        }

        #endregion

        #endregion

    }

}
