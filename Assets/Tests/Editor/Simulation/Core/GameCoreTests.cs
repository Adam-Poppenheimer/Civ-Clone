using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation.Core;

using Assets.Simulation.Cities;
using Assets.Simulation.Civilizations;

namespace Assets.Tests.Simulation.Core {

    [TestFixture]
    public class GameCoreTests : ZenjectUnitTestFixture {

        private Mock<ITurnExecuter> TurnExecuterMock;
        private Mock<IRecordkeepingCityFactory> CityFactoryMock;
        private Mock<ICivilizationFactory> CivilizationFactoryMock;

        [SetUp]
        public void CommonInstall() {
            TurnExecuterMock = new Mock<ITurnExecuter>();

            CityFactoryMock = new Mock<IRecordkeepingCityFactory>();
            CityFactoryMock.Setup(factory => factory.AllCities).Returns(new List<ICity>().AsReadOnly());

            CivilizationFactoryMock = new Mock<ICivilizationFactory>();
            CivilizationFactoryMock.Setup(factory => factory.AllCivilizations).Returns(new List<ICivilization>());

            Container.Bind<ITurnExecuter>().FromInstance(TurnExecuterMock.Object);
            Container.Bind<IRecordkeepingCityFactory>().FromInstance(CityFactoryMock.Object);
            Container.Bind<ICivilizationFactory>().FromInstance(CivilizationFactoryMock.Object);

            Container.Bind<SignalManager>().AsSingle();

            Container.DeclareSignal<TurnBeganSignal>();
            Container.DeclareSignal<TurnEndedSignal>();
            Container.DeclareSignal<EndTurnRequestedSignal>();

            Container.Bind<GameCore>().AsSingle();
        }

        [Test(Description = "When BeginRound is called, all cities in CityFactory " +
            "are passed to TurnExecuter properly")]
        public void BeginRound_AllCitiesCalledToBeginTurn() {
            var allCities = new List<ICity>() {
                new Mock<ICity>().Object,
                new Mock<ICity>().Object,
                new Mock<ICity>().Object,
                new Mock<ICity>().Object,
                new Mock<ICity>().Object
            };

            CityFactoryMock.SetupGet(factory => factory.AllCities).Returns(allCities.AsReadOnly());

            var gameCore = Container.Resolve<GameCore>();

            gameCore.BeginRound();

            foreach(var city in allCities) {
                TurnExecuterMock.Verify(executer => executer.BeginTurnOnCity(city),
                    "TurnExecuter was not called to begin a city's turn");
            }
        }

        [Test(Description = "When BeginRound is called, all civilizations in CivilizationFactory " +
            "are passed to TurnExecuter properly")]
        public void BeginRound_AllCivilizationsCalled() {
            var allCivilizations = new List<ICivilization>() {
                new Mock<ICivilization>().Object,
                new Mock<ICivilization>().Object,
                new Mock<ICivilization>().Object,
                new Mock<ICivilization>().Object,
                new Mock<ICivilization>().Object
            };

            CivilizationFactoryMock.SetupGet(factory => factory.AllCivilizations).Returns(allCivilizations);

            var gameCore = Container.Resolve<GameCore>();

            gameCore.BeginRound();

            foreach(var civilization in allCivilizations) {
                TurnExecuterMock.Verify(executer => executer.BeginTurnOnCivilization(civilization),
                    "TurnExecuter was not called to begin a civilization's turn");
            }
        }

        [Test(Description = "When BeginRound is called, there is a specific execution order " +
            "between different classes that is always maintained")]
        public void BeginRound_HasCorrectExecutionOrder() {
            var city = new Mock<ICity>().Object;
            var civilization = new Mock<ICivilization>().Object;

            CityFactoryMock.Setup(factory => factory.AllCities).Returns(new List<ICity>() { city }.AsReadOnly());
            CivilizationFactoryMock.Setup(factory => factory.AllCivilizations).Returns(new List<ICivilization>() { civilization });

            var executionSequence = new MockSequence();

            TurnExecuterMock.InSequence(executionSequence).Setup(executer => executer.BeginTurnOnCity(city));
            TurnExecuterMock.InSequence(executionSequence).Setup(executer => executer.BeginTurnOnCivilization(civilization));

            var gameCore = Container.Resolve<GameCore>();
            gameCore.BeginRound();

            TurnExecuterMock.VerifyAll();
        }

        [Test(Description = "When BeginRound is called, after all objects have been passed " +
            "to TurnExecuter, GameCore should fire TurnBeganSignal")]
        public void BeginRound_FiresTurnBeganSignal() {
            var gameCore = Container.Resolve<GameCore>();

            var beganSignal = Container.Resolve<TurnBeganSignal>();
            beganSignal.Listen(turn => Assert.Pass());

            gameCore.BeginRound();
            Assert.Fail("TurnBeganSignal was never fired");
        }

        [Test(Description = "When EndRound is called, all cities in CityFactory " +
            "are passed to TurnExecuter properly")]
        public void EndRound_AllCitiesCalledToEndTurn() {
            var allCities = new List<ICity>() {
                new Mock<ICity>().Object,
                new Mock<ICity>().Object,
                new Mock<ICity>().Object,
                new Mock<ICity>().Object,
                new Mock<ICity>().Object
            };

            CityFactoryMock.SetupGet(factory => factory.AllCities).Returns(allCities.AsReadOnly());

            var gameCore = Container.Resolve<GameCore>();

            gameCore.EndRound();

            foreach(var city in allCities) {
                TurnExecuterMock.Verify(executer => executer.EndTurnOnCity(city),
                    "TurnExecuter was not called to end a city's turn");
            }
        }

        [Test(Description = "When EndRound is called, all civilizations in CivilizationFactory " +
            "are passed to TurnExecuter properly")]
        public void EndRound_AllCivilizationsCalled() {
            var allCivilizations = new List<ICivilization>() {
                new Mock<ICivilization>().Object,
                new Mock<ICivilization>().Object,
                new Mock<ICivilization>().Object,
                new Mock<ICivilization>().Object,
                new Mock<ICivilization>().Object
            };

            CivilizationFactoryMock.SetupGet(factory => factory.AllCivilizations).Returns(allCivilizations);

            var gameCore = Container.Resolve<GameCore>();

            gameCore.EndRound();

            foreach(var civilization in allCivilizations) {
                TurnExecuterMock.Verify(executer => executer.EndTurnOnCivilization(civilization),
                    "TurnExecuter was not called to end a civilization's turn");
            }
        }

        [Test(Description = "When EndRound is called, there is a specific execution order " +
            "between different classes that is always maintained")]
        public void EndRound_HasCorrectExecutionOrder() {
            var city = new Mock<ICity>().Object;
            var civilization = new Mock<ICivilization>().Object;

            CityFactoryMock.Setup(factory => factory.AllCities).Returns(new List<ICity>() { city }.AsReadOnly());
            CivilizationFactoryMock.Setup(factory => factory.AllCivilizations).Returns(new List<ICivilization>() { civilization });

            var executionSequence = new MockSequence();

            TurnExecuterMock.InSequence(executionSequence).Setup(executer => executer.EndTurnOnCity(city));
            TurnExecuterMock.InSequence(executionSequence).Setup(executer => executer.EndTurnOnCivilization(civilization));

            var gameCore = Container.Resolve<GameCore>();
            gameCore.EndRound();

            TurnExecuterMock.VerifyAll();
        }

        [Test(Description = "When EndRound is called, after all objects have been passed " +
            "to TurnExecuter, GameCore should fire TurnEndedSignal")]
        public void EndRound_FiresRoundEndedSignal() {
            var gameCore = Container.Resolve<GameCore>();

            var endedSignal = Container.Resolve<TurnEndedSignal>();
            endedSignal.Listen(turn => Assert.Pass());

            gameCore.EndRound();
            Assert.Fail("TurnEndedSignal was never fired");
        }

        [Test(Description = "When EndTurnRequestedSignal fires, GameCore should call EndRound " +
            "and then call BeginRound")]
        public void EndTurnRequestedSignalFired_RoundEndedThenBegan() {
            var city = new Mock<ICity>().Object;

            CityFactoryMock.Setup(factory => factory.AllCities).Returns(new List<ICity>() { city }.AsReadOnly());

            var signal = Container.Resolve<EndTurnRequestedSignal>();

            var gameCore = Container.Resolve<GameCore>();

            var executionSequence = new MockSequence();
            TurnExecuterMock.InSequence(executionSequence).Setup(executer => executer.EndTurnOnCity(city));
            TurnExecuterMock.InSequence(executionSequence).Setup(executer => executer.BeginTurnOnCity(city));

            signal.Fire();

            TurnExecuterMock.VerifyAll();
        }

    }

}
