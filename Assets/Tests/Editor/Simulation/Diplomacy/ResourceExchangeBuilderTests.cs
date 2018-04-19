using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation.Diplomacy;
using Assets.Simulation.Civilizations;
using Assets.Simulation.SpecialtyResources;

using UnityCustomUtilities.Extensions;

namespace Assets.Tests.Simulation.Diplomacy {

    public class ResourceExchangeBuilderTests : ZenjectUnitTestFixture {

        #region internal types

        public class ResourceExchangeBuilderTestData {

            public List<ResourceDefinitionTestData> Resources;

            public CivilizationTestData Sender;

            public CivilizationTestData Receiver;

            public bool AreCivilizationsConnected;

        }

        public class ResourceDefinitionTestData {

            public SpecialtyResourceType Type;

        }

        public class CivilizationTestData {

            public List<ResourceSummaryTestData> ResourceSummaries;

        }

        public class ResourceSummaryTestData {

            public int FreeCopies;

            public int TradeableCopies;

            public bool ExpectsExchange;

            public bool ExpectsIntegerInput;

            public int IntegerInputExpected;

        }

        #endregion

        #region static fields and properties

        public static IEnumerable BuildResourceExchangesTestCases {
            get {
                yield return new TestCaseData(new ResourceExchangeBuilderTestData() {
                    Resources = new List<ResourceDefinitionTestData>() {
                        new ResourceDefinitionTestData() { Type = SpecialtyResourceType.Luxury }
                    },
                    Sender = new CivilizationTestData() {
                        ResourceSummaries = new List<ResourceSummaryTestData>() {
                            new ResourceSummaryTestData() {
                                TradeableCopies = 1, ExpectsExchange = true,
                                ExpectsIntegerInput = false, IntegerInputExpected = 1
                            }
                        }
                    },
                    Receiver = new CivilizationTestData() {
                        ResourceSummaries = new List<ResourceSummaryTestData>() {
                            new ResourceSummaryTestData() {
                                FreeCopies = 0
                            }
                        }
                    },
                    AreCivilizationsConnected = true
                }).SetName("Luxury, sender has tradeable copies, receiver has no free copies");

                yield return new TestCaseData(new ResourceExchangeBuilderTestData() {
                    Resources = new List<ResourceDefinitionTestData>() {
                        new ResourceDefinitionTestData() { Type = SpecialtyResourceType.Luxury }
                    },
                    Sender = new CivilizationTestData() {
                        ResourceSummaries = new List<ResourceSummaryTestData>() {
                            new ResourceSummaryTestData() {
                                TradeableCopies = 2, ExpectsExchange = false
                            }
                        }
                    },
                    Receiver = new CivilizationTestData() {
                        ResourceSummaries = new List<ResourceSummaryTestData>() {
                            new ResourceSummaryTestData() {
                                FreeCopies = 1
                            }
                        }
                    },
                    AreCivilizationsConnected = true
                }).SetName("Luxury, sender has tradeable copies, receiver has a free copy");

                yield return new TestCaseData(new ResourceExchangeBuilderTestData() {
                    Resources = new List<ResourceDefinitionTestData>() {
                        new ResourceDefinitionTestData() { Type = SpecialtyResourceType.Luxury }
                    },
                    Sender = new CivilizationTestData() {
                        ResourceSummaries = new List<ResourceSummaryTestData>() {
                            new ResourceSummaryTestData() {
                                TradeableCopies = 0, ExpectsExchange = false
                            }
                        }
                    },
                    Receiver = new CivilizationTestData() {
                        ResourceSummaries = new List<ResourceSummaryTestData>() {
                            new ResourceSummaryTestData() {
                                FreeCopies = 0
                            }
                        }
                    },
                    AreCivilizationsConnected = true
                }).SetName("Luxury, sender has no tradeable copies, receiver has no free copies");



                yield return new TestCaseData(new ResourceExchangeBuilderTestData() {
                    Resources = new List<ResourceDefinitionTestData>() {
                        new ResourceDefinitionTestData() { Type = SpecialtyResourceType.Luxury }
                    },
                    Sender = new CivilizationTestData() {
                        ResourceSummaries = new List<ResourceSummaryTestData>() {
                            new ResourceSummaryTestData() {
                                FreeCopies = 0
                            }
                        }
                    },
                    Receiver = new CivilizationTestData() {
                        ResourceSummaries = new List<ResourceSummaryTestData>() {
                            new ResourceSummaryTestData() {
                                TradeableCopies = 1, ExpectsExchange = true,
                                ExpectsIntegerInput = false, IntegerInputExpected = 1
                            }
                        }
                    },                    
                    AreCivilizationsConnected = true
                }).SetName("Luxury, sender no free copies, receiver has tradeable copies");

                yield return new TestCaseData(new ResourceExchangeBuilderTestData() {
                    Resources = new List<ResourceDefinitionTestData>() {
                        new ResourceDefinitionTestData() { Type = SpecialtyResourceType.Luxury }
                    },
                    Sender = new CivilizationTestData() {
                        ResourceSummaries = new List<ResourceSummaryTestData>() {
                            new ResourceSummaryTestData() {
                                FreeCopies = 1
                            }
                        }
                    },
                    Receiver = new CivilizationTestData() {
                        ResourceSummaries = new List<ResourceSummaryTestData>() {
                            new ResourceSummaryTestData() {
                                TradeableCopies = 2, ExpectsExchange = false
                            }
                        }
                    },
                    AreCivilizationsConnected = true
                }).SetName("Luxury, receiver has tradeable copies, sender has a free copy");

                yield return new TestCaseData(new ResourceExchangeBuilderTestData() {
                    Resources = new List<ResourceDefinitionTestData>() {
                        new ResourceDefinitionTestData() { Type = SpecialtyResourceType.Luxury }
                    },
                    Sender = new CivilizationTestData() {
                        ResourceSummaries = new List<ResourceSummaryTestData>() {
                            new ResourceSummaryTestData() {
                                FreeCopies = 0
                            }
                        }
                    },
                    Receiver = new CivilizationTestData() {
                        ResourceSummaries = new List<ResourceSummaryTestData>() {
                            new ResourceSummaryTestData() {
                                TradeableCopies = 0, ExpectsExchange = false
                            }
                        }
                    },
                    AreCivilizationsConnected = true
                }).SetName("Luxury, receiver has no tradeable copies, sender has no free copies");

                yield return new TestCaseData(new ResourceExchangeBuilderTestData() {
                    Resources = new List<ResourceDefinitionTestData>() {
                        new ResourceDefinitionTestData() { Type = SpecialtyResourceType.Luxury }
                    },
                    Sender = new CivilizationTestData() {
                        ResourceSummaries = new List<ResourceSummaryTestData>() {
                            new ResourceSummaryTestData() {
                                TradeableCopies = 1, ExpectsExchange = false
                            }
                        }
                    },
                    Receiver = new CivilizationTestData() {
                        ResourceSummaries = new List<ResourceSummaryTestData>() {
                            new ResourceSummaryTestData() {
                                FreeCopies = 0
                            }
                        }
                    },
                    AreCivilizationsConnected = false
                }).SetName("Luxury, otherwise valid sender/receiver trade, no connection between civs");

                yield return new TestCaseData(new ResourceExchangeBuilderTestData() {
                    Resources = new List<ResourceDefinitionTestData>() {
                        new ResourceDefinitionTestData() { Type = SpecialtyResourceType.Luxury }
                    },
                    Sender = new CivilizationTestData() {
                        ResourceSummaries = new List<ResourceSummaryTestData>() {
                            new ResourceSummaryTestData() {
                                FreeCopies = 0
                            }
                        }
                    },
                    Receiver = new CivilizationTestData() {
                        ResourceSummaries = new List<ResourceSummaryTestData>() {
                            new ResourceSummaryTestData() {
                                TradeableCopies = 1, ExpectsExchange = false
                            }
                        }
                    },                    
                    AreCivilizationsConnected = false
                }).SetName("Luxury, otherwise valid receiver/sender trade, no connection between civs");





                yield return new TestCaseData(new ResourceExchangeBuilderTestData() {
                    Resources = new List<ResourceDefinitionTestData>() {
                        new ResourceDefinitionTestData() { Type = SpecialtyResourceType.Strategic }
                    },
                    Sender = new CivilizationTestData() {
                        ResourceSummaries = new List<ResourceSummaryTestData>() {
                            new ResourceSummaryTestData() {
                                TradeableCopies = 5, ExpectsExchange = true,
                                ExpectsIntegerInput = true, IntegerInputExpected = 5
                            }
                        }
                    },
                    Receiver = new CivilizationTestData() {
                        ResourceSummaries = new List<ResourceSummaryTestData>() {
                            new ResourceSummaryTestData() {
                                FreeCopies = 0
                            }
                        }
                    },
                    AreCivilizationsConnected = true
                }).SetName("Strategic, sender has tradeable copies, receiver has no free copies");

                yield return new TestCaseData(new ResourceExchangeBuilderTestData() {
                    Resources = new List<ResourceDefinitionTestData>() {
                        new ResourceDefinitionTestData() { Type = SpecialtyResourceType.Strategic }
                    },
                    Sender = new CivilizationTestData() {
                        ResourceSummaries = new List<ResourceSummaryTestData>() {
                            new ResourceSummaryTestData() {
                                TradeableCopies = 5, ExpectsExchange = true,
                                ExpectsIntegerInput = true, IntegerInputExpected = 5
                            }
                        }
                    },
                    Receiver = new CivilizationTestData() {
                        ResourceSummaries = new List<ResourceSummaryTestData>() {
                            new ResourceSummaryTestData() {
                                FreeCopies = 5
                            }
                        }
                    },
                    AreCivilizationsConnected = true
                }).SetName("Strategic, sender has tradeable copies, receiver has free copies");

                yield return new TestCaseData(new ResourceExchangeBuilderTestData() {
                    Resources = new List<ResourceDefinitionTestData>() {
                        new ResourceDefinitionTestData() { Type = SpecialtyResourceType.Strategic }
                    },
                    Sender = new CivilizationTestData() {
                        ResourceSummaries = new List<ResourceSummaryTestData>() {
                            new ResourceSummaryTestData() {
                                TradeableCopies = 0, ExpectsExchange = false
                            }
                        }
                    },
                    Receiver = new CivilizationTestData() {
                        ResourceSummaries = new List<ResourceSummaryTestData>() {
                            new ResourceSummaryTestData() {
                                FreeCopies = 0
                            }
                        }
                    },
                    AreCivilizationsConnected = true
                }).SetName("Strategic, sender has no tradeable copies");

                yield return new TestCaseData(new ResourceExchangeBuilderTestData() {
                    Resources = new List<ResourceDefinitionTestData>() {
                        new ResourceDefinitionTestData() { Type = SpecialtyResourceType.Strategic }
                    },
                    Sender = new CivilizationTestData() {
                        ResourceSummaries = new List<ResourceSummaryTestData>() {
                            new ResourceSummaryTestData() {
                                FreeCopies = 0
                            }
                        }
                    },
                    Receiver = new CivilizationTestData() {
                        ResourceSummaries = new List<ResourceSummaryTestData>() {
                            new ResourceSummaryTestData() {
                                TradeableCopies = 0, ExpectsExchange = false
                            }
                        }
                    },
                    AreCivilizationsConnected = true
                }).SetName("Strategic, receiver has no tradeable copies");

                yield return new TestCaseData(new ResourceExchangeBuilderTestData() {
                    Resources = new List<ResourceDefinitionTestData>() {
                        new ResourceDefinitionTestData() { Type = SpecialtyResourceType.Strategic }
                    },
                    Sender = new CivilizationTestData() {
                        ResourceSummaries = new List<ResourceSummaryTestData>() {
                            new ResourceSummaryTestData() {
                                TradeableCopies = 5, ExpectsExchange = false
                            }
                        }
                    },
                    Receiver = new CivilizationTestData() {
                        ResourceSummaries = new List<ResourceSummaryTestData>() {
                            new ResourceSummaryTestData() {
                                FreeCopies = 0
                            }
                        }
                    },
                    AreCivilizationsConnected = false
                }).SetName("Strategic, otherwise valid sender/receiver trade, no connection between civs");

                yield return new TestCaseData(new ResourceExchangeBuilderTestData() {
                    Resources = new List<ResourceDefinitionTestData>() {
                        new ResourceDefinitionTestData() { Type = SpecialtyResourceType.Strategic }
                    },
                    Sender = new CivilizationTestData() {
                        ResourceSummaries = new List<ResourceSummaryTestData>() {
                            new ResourceSummaryTestData() {
                                FreeCopies = 0
                            }
                        }
                    },
                    Receiver = new CivilizationTestData() {
                        ResourceSummaries = new List<ResourceSummaryTestData>() {
                            new ResourceSummaryTestData() {
                                TradeableCopies = 5, ExpectsExchange = false
                            }
                        }
                    },                    
                    AreCivilizationsConnected = false
                }).SetName("Strategic, otherwise valid receiver/sender trade, no connection between civs");
            }
        }

        #endregion

        #region instance fields and properties

        private Mock<ICivilizationConnectionLogic> MockConnectionLogic;
        private Mock<IResourceTransferCanon>       MockResourceTransferCanon;
        private Mock<IFreeResourcesLogic>          MockFreeResourcesLogic;

        private List<ISpecialtyResourceDefinition> AvailableResources = new List<ISpecialtyResourceDefinition>();

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            AvailableResources.Clear();

            MockConnectionLogic       = new Mock<ICivilizationConnectionLogic>();
            MockResourceTransferCanon = new Mock<IResourceTransferCanon>();
            MockFreeResourcesLogic    = new Mock<IFreeResourcesLogic>();

            Container.Bind<ICivilizationConnectionLogic>().FromInstance(MockConnectionLogic      .Object);
            Container.Bind<IResourceTransferCanon>      ().FromInstance(MockResourceTransferCanon.Object);
            Container.Bind<IFreeResourcesLogic>         ().FromInstance(MockFreeResourcesLogic   .Object);

            Container.Bind<IEnumerable<ISpecialtyResourceDefinition>>()
                     .WithId("Available Specialty Resources")
                     .FromInstance(AvailableResources);

            Container.Bind<ResourceExchangeBuilder>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        [TestCaseSource("BuildResourceExchangesTestCases")]
        public void BuildResourceExchangesTests(ResourceExchangeBuilderTestData testData) {
            var resources = testData.Resources.Select(resourceData => BuildResource(resourceData)).ToList();

            var sender   = BuildCivilization(testData.Sender,   resources);
            var receiver = BuildCivilization(testData.Receiver, resources);

            var summary = new ExchangeSummary();

            MockConnectionLogic.Setup(logic => logic.AreCivilizationsConnected(sender, receiver))
                               .Returns(testData.AreCivilizationsConnected);

            MockConnectionLogic.Setup(logic => logic.AreCivilizationsConnected(receiver, sender))
                               .Returns(testData.AreCivilizationsConnected);

            var exchangeBuilder = Container.Resolve<ResourceExchangeBuilder>();

            exchangeBuilder.BuildResourceExchanges(sender, receiver, summary);

            var senderResourceExchanges = summary.AllPossibleOffersFromSender.Select(exchange => exchange as ResourceDiplomaticExchange);
            CheckExchangesAgainstExpectations(resources, senderResourceExchanges, testData.Sender.ResourceSummaries);
            
            var receiverResourceExchanges = summary.AllPossibleDemandsOfReceiver.Select(exchange => exchange as ResourceDiplomaticExchange);
            CheckExchangesAgainstExpectations(resources, receiverResourceExchanges, testData.Receiver.ResourceSummaries);
        }

        #endregion

        #region utilities

        private void CheckExchangesAgainstExpectations(
            List<ISpecialtyResourceDefinition> resources,
            IEnumerable<ResourceDiplomaticExchange> exchanges,
            List<ResourceSummaryTestData> expectations
        ) {
            for(int i = 0; i < expectations.Count; i++) {
                var senderExpectations = expectations[i];

                var resource = resources[i];

                if(senderExpectations.ExpectsExchange) {
                    var exchangeForResource = exchanges
                        .Where(exchange => exchange.ResourceToExchange == resource)
                        .FirstOrDefault();

                    Assert.NotNull(exchangeForResource, "There is no exchange serving resource at index " + i);

                    Assert.AreEqual(
                        senderExpectations.ExpectsIntegerInput, exchangeForResource.RequiresIntegerInput,
                        string.Format("Resource at index {0} has an unexpected ExpectsIntegerInput value", i)
                    );

                    Assert.AreEqual(
                        senderExpectations.IntegerInputExpected, exchangeForResource.IntegerInput,
                        string.Format("Resource at index {0} has an unexpected IntegerInput value", i)
                    );
                }
            }
        }

        private ISpecialtyResourceDefinition BuildResource(ResourceDefinitionTestData data) {
            var mockResource = new Mock<ISpecialtyResourceDefinition>();

            mockResource.Setup(resource => resource.Type).Returns(data.Type);

            var newResource = mockResource.Object;

            AvailableResources.Add(newResource);

            return newResource;
        }

        private ICivilization BuildCivilization(CivilizationTestData testData, List<ISpecialtyResourceDefinition> resources) {
            var newCiv = new Mock<ICivilization>().Object;

            for(int i = 0; i < testData.ResourceSummaries.Count; i++) {
                var resource = resources[i];
                var summary = testData.ResourceSummaries[i];

                MockResourceTransferCanon.Setup(canon => canon.GetTradeableCopiesOfResourceForCiv(resource, newCiv))
                                         .Returns(summary.TradeableCopies);

                MockFreeResourcesLogic.Setup(logic => logic.GetFreeCopiesOfResourceForCiv(resource, newCiv))
                                      .Returns(summary.FreeCopies);
            }

            return newCiv;
        }

        #endregion

        #endregion

    }

}
