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
using Assets.Simulation.Units;
using Assets.Simulation.Units.Abilities;

using Assets.UI.Core;

namespace Assets.Tests.Simulation.Core {

    [TestFixture]
    public class GameCoreTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<ICityFactory>         CityFactoryMock;
        private Mock<ICivilizationFactory> CivilizationFactoryMock;
        private Mock<IUnitFactory>         UnitFactoryMock;

        private Mock<ITurnExecuter> TurnExecuterMock;

        private Mock<IAbilityExecuter> MockAbilityExecuter;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            TurnExecuterMock = new Mock<ITurnExecuter>();

            CityFactoryMock = new Mock<ICityFactory>();
            CityFactoryMock.Setup(factory => factory.AllCities).Returns(new List<ICity>().AsReadOnly());

            CivilizationFactoryMock = new Mock<ICivilizationFactory>();
            CivilizationFactoryMock.Setup(factory => factory.AllCivilizations).Returns(new List<ICivilization>().AsReadOnly());

            UnitFactoryMock = new Mock<IUnitFactory>();
            UnitFactoryMock.Setup(factory => factory.AllUnits).Returns(new List<IUnit>());

            MockAbilityExecuter = new Mock<IAbilityExecuter>();

            Container.Bind<ICityFactory>        ().FromInstance(CityFactoryMock        .Object);
            Container.Bind<ICivilizationFactory>().FromInstance(CivilizationFactoryMock.Object);
            Container.Bind<IUnitFactory>        ().FromInstance(UnitFactoryMock        .Object);
            Container.Bind<IAbilityExecuter>().FromInstance(MockAbilityExecuter    .Object);

            Container.Bind<ITurnExecuter>().FromInstance(TurnExecuterMock.Object);

            Container.Bind<SignalManager>().AsSingle();

            Container.DeclareSignal<TurnBeganSignal>();
            Container.DeclareSignal<TurnEndedSignal>();

            Container.Bind<PlayerSignals>().AsSingle();

            Container.Bind<GameCore>().AsSingle();
        }

        #endregion

        #region tests

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

            CivilizationFactoryMock.SetupGet(factory => factory.AllCivilizations).Returns(allCivilizations.AsReadOnly());

            var gameCore = Container.Resolve<GameCore>();

            gameCore.BeginRound();

            foreach(var civilization in allCivilizations) {
                TurnExecuterMock.Verify(executer => executer.BeginTurnOnCivilization(civilization),
                    "TurnExecuter was not called to begin a civilization's turn");
            }
        }

        [Test(Description = "When BeginRound is called, all units in UnitFactory are passed to " +
            "TurnExecuter properly")]
        public void BeginRound_AllUnitsCalled() {
            var allUnits = new List<IUnit>() {
                new Mock<IUnit>().Object,
                new Mock<IUnit>().Object,
                new Mock<IUnit>().Object
            };

            UnitFactoryMock.Setup(factory => factory.AllUnits).Returns(allUnits);

            var gameCore = Container.Resolve<GameCore>();

            gameCore.BeginRound();

            foreach(var unit in allUnits) {
                TurnExecuterMock.Verify(executer => executer.BeginTurnOnUnit(unit),
                    "TurnExecuter was not called to begin a unit's turn");
            }
        }

        [Test(Description = "When BeginRound is called, there is a specific execution order " +
            "between different classes that is always maintained")]
        public void BeginRound_HasCorrectExecutionOrder() {
            var city          = new Mock<ICity>()       .Object;
            var civilization = new Mock<ICivilization>().Object;
            var unit         = new Mock<IUnit>()        .Object;

            CityFactoryMock        .Setup(factory => factory.AllCities)       .Returns(new List<ICity>()         { city         }.AsReadOnly());
            CivilizationFactoryMock.Setup(factory => factory.AllCivilizations).Returns(new List<ICivilization>() { civilization }.AsReadOnly());
            UnitFactoryMock        .Setup(factory => factory.AllUnits)        .Returns(new List<IUnit>()         { unit         });

            var executionSequence = new MockSequence();

            TurnExecuterMock.InSequence(executionSequence).Setup(executer => executer.BeginTurnOnCity        (city));
            TurnExecuterMock.InSequence(executionSequence).Setup(executer => executer.BeginTurnOnCivilization(civilization));
            TurnExecuterMock.InSequence(executionSequence).Setup(executer => executer.BeginTurnOnUnit        (unit));

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

            CivilizationFactoryMock.SetupGet(factory => factory.AllCivilizations).Returns(allCivilizations.AsReadOnly());

            var gameCore = Container.Resolve<GameCore>();

            gameCore.EndRound();

            foreach(var civilization in allCivilizations) {
                TurnExecuterMock.Verify(executer => executer.EndTurnOnCivilization(civilization),
                    "TurnExecuter was not called to end a civilization's turn");
            }
        }

        [Test(Description = "When EndRound is called, all units in UnitFactory are passed to " +
            "TurnExecuter properly")]
        public void EndRound_AllUnitsCalled() {
            var allUnits = new List<IUnit>() {
                new Mock<IUnit>().Object,
                new Mock<IUnit>().Object,
                new Mock<IUnit>().Object
            };

            UnitFactoryMock.Setup(factory => factory.AllUnits).Returns(allUnits);

            var gameCore = Container.Resolve<GameCore>();

            gameCore.EndRound();

            foreach(var unit in allUnits) {
                TurnExecuterMock.Verify(executer => executer.EndTurnOnUnit(unit),
                    "TurnExecuter was not called to begin a unit's turn");
            }
        }

        [Test(Description = "When EndRound is called, there is a specific execution order " +
            "between different classes that is always maintained")]
        public void EndRound_HasCorrectExecutionOrder() {
            var city = new Mock<ICity>().Object;
            var civilization = new Mock<ICivilization>().Object;
            var unit = new Mock<IUnit>().Object;

            CityFactoryMock        .Setup(factory => factory.AllCities)       .Returns(new List<ICity>()         { city         }.AsReadOnly());
            CivilizationFactoryMock.Setup(factory => factory.AllCivilizations).Returns(new List<ICivilization>() { civilization }.AsReadOnly());
            UnitFactoryMock        .Setup(factory => factory.AllUnits)        .Returns(new List<IUnit>()         { unit         });

            var executionSequence = new MockSequence();

            TurnExecuterMock.InSequence(executionSequence).Setup(executer => executer.EndTurnOnCity        (city));
            TurnExecuterMock.InSequence(executionSequence).Setup(executer => executer.EndTurnOnCivilization(civilization));
            TurnExecuterMock.InSequence(executionSequence).Setup(executer => executer.EndTurnOnUnit        (unit));

            MockAbilityExecuter.InSequence(executionSequence).Setup(executer => executer.PerformOngoingAbilities());

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

            var playerSignals = Container.Resolve<PlayerSignals>();

            Container.Resolve<GameCore>();

            var executionSequence = new MockSequence();
            TurnExecuterMock.InSequence(executionSequence).Setup(executer => executer.EndTurnOnCity(city));
            TurnExecuterMock.InSequence(executionSequence).Setup(executer => executer.BeginTurnOnCity(city));

            playerSignals.EndTurnRequestedSignal.OnNext(UniRx.Unit.Default);

            TurnExecuterMock.VerifyAll();
        }

        #endregion

        #region utilities



        #endregion

        #endregion

    }

}
