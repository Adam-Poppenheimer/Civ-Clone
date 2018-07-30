using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation.Diplomacy;
using Assets.Simulation.Civilizations;
using Assets.Simulation.MapResources;
using Assets.Simulation.MapManagement;

namespace Assets.Tests.Simulation.MapManagement {

    [TestFixture]
    public class OngoingDiplomaticExchangeComposerTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<ICivilizationFactory>       MockCivFactory;
        private Mock<IDiplomaticExchangeFactory> MockExchangeFactory;

        private List<ICivilization> AllCivs = new List<ICivilization>();

        private List<IResourceDefinition> AllResources = new List<IResourceDefinition>();

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            AllCivs.Clear();
            AllResources.Clear();

            MockCivFactory      = new Mock<ICivilizationFactory>();
            MockExchangeFactory = new Mock<IDiplomaticExchangeFactory>();

            MockCivFactory.Setup(factory => factory.AllCivilizations).Returns(AllCivs.AsReadOnly());

            MockExchangeFactory.Setup(
                factory => factory.BuildOngoingExchangeForType(It.IsAny<ExchangeType>())
            ).Returns<ExchangeType>(
                type => BuildOngoingExchange(type, null, null, null, 0)
            );
            
            Container.Bind<ICivilizationFactory>      ().FromInstance(MockCivFactory     .Object);
            Container.Bind<IDiplomaticExchangeFactory>().FromInstance(MockExchangeFactory.Object);

            Container.Bind<IEnumerable<IResourceDefinition>>()
                     .WithId("Available Resources")
                     .FromInstance(AllResources);

            Container.Bind<OngoingDiplomaticExchangeComposer>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void ComposeOngoingExchange_StoresType() {
            var civOne = BuildCivilization("Civ One");
            var civTwo = BuildCivilization("Civ Two");

            var resource = BuildResource("Resource One");

            var ongoingExchange = BuildOngoingExchange(ExchangeType.Peace, civOne, civTwo, resource, 0);

            var composer = Container.Resolve<OngoingDiplomaticExchangeComposer>();

            var serialExchange = composer.ComposeOngoingExchange(ongoingExchange);

            Assert.AreEqual(ExchangeType.Peace, serialExchange.Type);
        }

        [Test]
        public void ComposeOngoingExchange_StoresSenderAsName() {
            var civOne = BuildCivilization("Civ One");
            var civTwo = BuildCivilization("Civ Two");

            var resource = BuildResource("Resource One");

            var ongoingExchange = BuildOngoingExchange(ExchangeType.Peace, civOne, civTwo, resource, 0);

            var composer = Container.Resolve<OngoingDiplomaticExchangeComposer>();

            var serialExchange = composer.ComposeOngoingExchange(ongoingExchange);

            Assert.AreEqual("Civ One", serialExchange.Sender);
        }

        [Test]
        public void ComposeOngoingExchange_StoresReceiverAsName() {
            var civOne = BuildCivilization("Civ One");
            var civTwo = BuildCivilization("Civ Two");

            var resource = BuildResource("Resource One");

            var ongoingExchange = BuildOngoingExchange(ExchangeType.Peace, civOne, civTwo, resource, 0);

            var composer = Container.Resolve<OngoingDiplomaticExchangeComposer>();

            var serialExchange = composer.ComposeOngoingExchange(ongoingExchange);

            Assert.AreEqual("Civ Two", serialExchange.Receiver);
        }

        [Test]
        public void ComposeOngoingExchange_StoresResourceInputAsName() {
            var civOne = BuildCivilization("Civ One");
            var civTwo = BuildCivilization("Civ Two");

            var resource = BuildResource("Resource One");

            var ongoingExchange = BuildOngoingExchange(ExchangeType.Peace, civOne, civTwo, resource, 0);

            var composer = Container.Resolve<OngoingDiplomaticExchangeComposer>();

            var serialExchange = composer.ComposeOngoingExchange(ongoingExchange);

            Assert.AreEqual("Resource One", serialExchange.ResourceInput);
        }

        [Test]
        public void ComposeOngoingExchange_StoresIntegerInput() {
            var civOne = BuildCivilization("Civ One");
            var civTwo = BuildCivilization("Civ Two");

            var resource = BuildResource("Resource One");

            var ongoingExchange = BuildOngoingExchange(ExchangeType.Peace, civOne, civTwo, resource, 15);

            var composer = Container.Resolve<OngoingDiplomaticExchangeComposer>();

            var serialExchange = composer.ComposeOngoingExchange(ongoingExchange);

            Assert.AreEqual(15, serialExchange.IntInput);
        }

        [Test]
        public void DecomposeOngoingExchange_ConstructsOnTypeProperly() {
            BuildCivilization("Civ One");
            BuildCivilization("Civ Two");

            BuildResource("Resource One");

            var serialExchange = new SerializableOngoingDiplomaticExchange() {
                Type = ExchangeType.Peace, Sender = "Civ One", Receiver = "Civ Two",
                ResourceInput = "Resource One", IntInput = 15
            };

            var composer = Container.Resolve<OngoingDiplomaticExchangeComposer>();

            var exchange = composer.DecomposeOngoingExchange(serialExchange);

            Assert.AreEqual(ExchangeType.Peace, exchange.Type);
        }

        [Test]
        public void DecomposeOngoingExchange_SetsSenderProperly() {
            var civOne = BuildCivilization("Civ One");
            BuildCivilization("Civ Two");

            BuildResource("Resource One");

            var serialExchange = new SerializableOngoingDiplomaticExchange() {
                Type = ExchangeType.Peace, Sender = "Civ One", Receiver = "Civ Two",
                ResourceInput = "Resource One", IntInput = 15
            };

            var composer = Container.Resolve<OngoingDiplomaticExchangeComposer>();

            var exchange = composer.DecomposeOngoingExchange(serialExchange);

            Assert.AreEqual(civOne, exchange.Sender);
        }

        [Test]
        public void DecomposeOngoingExchange_SetReceiverProperly() {
            BuildCivilization("Civ One");
            var civTwo = BuildCivilization("Civ Two");

            BuildResource("Resource One");

            var serialExchange = new SerializableOngoingDiplomaticExchange() {
                Type = ExchangeType.Peace, Sender = "Civ One", Receiver = "Civ Two",
                ResourceInput = "Resource One", IntInput = 15
            };

            var composer = Container.Resolve<OngoingDiplomaticExchangeComposer>();

            var exchange = composer.DecomposeOngoingExchange(serialExchange);

            Assert.AreEqual(civTwo, exchange.Receiver);
        }

        [Test]
        public void DecomposeOngoingExchange_SetsIntegerInputProperly() {
            BuildCivilization("Civ One");
            BuildCivilization("Civ Two");

            BuildResource("Resource One");

            var serialExchange = new SerializableOngoingDiplomaticExchange() {
                Type = ExchangeType.Peace, Sender = "Civ One", Receiver = "Civ Two",
                ResourceInput = "Resource One", IntInput = 15
            };

            var composer = Container.Resolve<OngoingDiplomaticExchangeComposer>();

            var exchange = composer.DecomposeOngoingExchange(serialExchange);

            Assert.AreEqual(15, exchange.IntegerInput);
        }

        [Test]
        public void DecomposeOngoingExchange_SetsResourceInputProperly() {
            BuildCivilization("Civ One");
            BuildCivilization("Civ Two");

            var resourceOne = BuildResource("Resource One");

            var serialExchange = new SerializableOngoingDiplomaticExchange() {
                Type = ExchangeType.Peace, Sender = "Civ One", Receiver = "Civ Two",
                ResourceInput = "Resource One", IntInput = 15
            };

            var composer = Container.Resolve<OngoingDiplomaticExchangeComposer>();

            var exchange = composer.DecomposeOngoingExchange(serialExchange);

            Assert.AreEqual(resourceOne, exchange.ResourceInput);
        }

        #endregion

        #region utilities

        private IOngoingDiplomaticExchange BuildOngoingExchange(
            ExchangeType type, ICivilization sender, ICivilization receiver,
            IResourceDefinition resourceInput, int integerInput
        ) {
            var mockExchange = new Mock<IOngoingDiplomaticExchange>();

            mockExchange.SetupAllProperties();
            mockExchange.Setup(exchange => exchange.Type).Returns(type);
            
            var newExchange = mockExchange.Object;

            newExchange.Sender        = sender;
            newExchange.Receiver      = receiver;
            newExchange.ResourceInput = resourceInput;
            newExchange.IntegerInput  = integerInput;

            return newExchange;
        }

        private ICivilization BuildCivilization(string name) {
            var mockCiv = new Mock<ICivilization>();

            mockCiv.Name = name;
            mockCiv.Setup(civ => civ.Name).Returns(name);

            var newCiv = mockCiv.Object;

            AllCivs.Add(newCiv);

            return newCiv;
        }

        private IResourceDefinition BuildResource(string name) {
            var mockResource = new Mock<IResourceDefinition>();

            mockResource.Name = name;
            mockResource.Setup(resource => resource.name).Returns(name);

            var newResource = mockResource.Object;

            AllResources.Add(newResource);

            return newResource;
        }

        #endregion

        #endregion

    }

}
