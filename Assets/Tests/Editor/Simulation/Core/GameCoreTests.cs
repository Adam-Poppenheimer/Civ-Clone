using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using NUnit.Framework;
using Moq;
using UniRx;

using Assets.Simulation.Core;

using Assets.Simulation.Cities;
using Assets.Simulation.Civilizations;
using Assets.Simulation.Units;
using Assets.Simulation.Units.Abilities;
using Assets.Simulation.HexMap;

using Assets.UI.Core;

namespace Assets.Tests.Simulation.Core {

    [TestFixture]
    public class GameCoreTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<ICityFactory>         MockCityFactory;
        private Mock<ICivilizationFactory> MockCivilizationFactory;
        private Mock<IUnitFactory>         MockUnitFactory;

        private Mock<IRoundExecuter> MockRoundExecuter;

        private Mock<IAbilityExecuter> MockAbilityExecuter;

        private Mock<IHexGrid> MockGrid;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockRoundExecuter = new Mock<IRoundExecuter>();

            MockCityFactory = new Mock<ICityFactory>();
            MockCityFactory.Setup(factory => factory.AllCities).Returns(new List<ICity>().AsReadOnly());

            MockCivilizationFactory = new Mock<ICivilizationFactory>();
            MockCivilizationFactory.Setup(factory => factory.AllCivilizations).Returns(new List<ICivilization>().AsReadOnly());

            MockUnitFactory = new Mock<IUnitFactory>();
            MockUnitFactory.Setup(factory => factory.AllUnits).Returns(new List<IUnit>());

            MockAbilityExecuter = new Mock<IAbilityExecuter>();
            MockGrid            = new Mock<IHexGrid>();

            Container.Bind<ICityFactory>        ().FromInstance(MockCityFactory        .Object);
            Container.Bind<ICivilizationFactory>().FromInstance(MockCivilizationFactory.Object);
            Container.Bind<IUnitFactory>        ().FromInstance(MockUnitFactory        .Object);
            Container.Bind<IAbilityExecuter>    ().FromInstance(MockAbilityExecuter    .Object);

            Container.Bind<IRoundExecuter>().FromInstance(MockRoundExecuter.Object);
            Container.Bind<IHexGrid>      ().FromInstance(MockGrid.Object);

            Container.Bind<CoreSignals>().AsSingle();

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

            MockCityFactory.SetupGet(factory => factory.AllCities).Returns(allCities.AsReadOnly());

            var gameCore = Container.Resolve<GameCore>();

            gameCore.BeginRound();

            foreach(var city in allCities) {
                MockRoundExecuter.Verify(executer => executer.BeginRoundOnCity(city),
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

            MockCivilizationFactory.SetupGet(factory => factory.AllCivilizations).Returns(allCivilizations.AsReadOnly());

            var gameCore = Container.Resolve<GameCore>();

            gameCore.BeginRound();

            foreach(var civilization in allCivilizations) {
                MockRoundExecuter.Verify(executer => executer.BeginRoundOnCivilization(civilization),
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

            MockUnitFactory.Setup(factory => factory.AllUnits).Returns(allUnits);

            var gameCore = Container.Resolve<GameCore>();

            gameCore.BeginRound();

            foreach(var unit in allUnits) {
                MockRoundExecuter.Verify(executer => executer.BeginRoundOnUnit(unit),
                    "TurnExecuter was not called to begin a unit's turn");
            }
        }

        [Test(Description = "When BeginRound is called, there is a specific execution order " +
            "between different classes that is always maintained")]
        public void BeginRound_HasCorrectExecutionOrder() {
            var city          = new Mock<ICity>()       .Object;
            var civilization = new Mock<ICivilization>().Object;
            var unit         = new Mock<IUnit>()        .Object;

            MockCityFactory        .Setup(factory => factory.AllCities)       .Returns(new List<ICity>()         { city         }.AsReadOnly());
            MockCivilizationFactory.Setup(factory => factory.AllCivilizations).Returns(new List<ICivilization>() { civilization }.AsReadOnly());
            MockUnitFactory        .Setup(factory => factory.AllUnits)        .Returns(new List<IUnit>()         { unit         });

            var executionSequence = new MockSequence();

            MockRoundExecuter.InSequence(executionSequence).Setup(executer => executer.BeginRoundOnCity        (city));
            MockRoundExecuter.InSequence(executionSequence).Setup(executer => executer.BeginRoundOnCivilization(civilization));
            MockRoundExecuter.InSequence(executionSequence).Setup(executer => executer.BeginRoundOnUnit        (unit));

            var gameCore = Container.Resolve<GameCore>();
            gameCore.BeginRound();

            MockRoundExecuter.VerifyAll();
        }

        [Test(Description = "When BeginRound is called, after all objects have been passed " +
            "to TurnExecuter, GameCore should fire RoundBeganSignal")]
        public void BeginRound_FiresRoundBeganSignal() {
            var gameCore = Container.Resolve<GameCore>();

            var coreSignals = Container.Resolve<CoreSignals>();
            coreSignals.RoundBeganSignal.Subscribe(turn => Assert.Pass());

            gameCore.BeginRound();
            Assert.Fail("TurnBeganSignal was never fired");
        }

        [Test(Description = "")]
        public void EndTurn_VisibilityRefreshed() {
            var cellMocks = new List<Mock<IHexCell>>() {
                new Mock<IHexCell>(), new Mock<IHexCell>(), new Mock<IHexCell>()
            };

            var gameCore = Container.Resolve<GameCore>();

            MockGrid.Setup(grid => grid.AllCells).Returns(cellMocks.Select(mock => mock.Object).ToList().AsReadOnly());

            MockCivilizationFactory
                .Setup(factory => factory.AllCivilizations)
                .Returns(new List<ICivilization>() { BuildCivilization() }.AsReadOnly());

            MockCityFactory.Setup(factory => factory.AllCities).Returns(new List<ICity>().AsReadOnly());

            gameCore.EndTurn();

            foreach(var cellMock in cellMocks) {
                cellMock.Verify(cell => cell.RefreshVisibility(), Times.Once,
                    "A cell's visibility was not refreshed when BeginRound was called");
            }
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

            MockCityFactory.SetupGet(factory => factory.AllCities).Returns(allCities.AsReadOnly());

            var gameCore = Container.Resolve<GameCore>();

            gameCore.EndRound();

            foreach(var city in allCities) {
                MockRoundExecuter.Verify(executer => executer.EndRoundOnCity(city),
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

            MockCivilizationFactory.SetupGet(factory => factory.AllCivilizations).Returns(allCivilizations.AsReadOnly());

            var gameCore = Container.Resolve<GameCore>();

            gameCore.EndRound();

            foreach(var civilization in allCivilizations) {
                MockRoundExecuter.Verify(executer => executer.EndRoundOnCivilization(civilization),
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

            MockUnitFactory.Setup(factory => factory.AllUnits).Returns(allUnits);

            var gameCore = Container.Resolve<GameCore>();

            gameCore.EndRound();

            foreach(var unit in allUnits) {
                MockRoundExecuter.Verify(executer => executer.EndRoundOnUnit(unit),
                    "TurnExecuter was not called to begin a unit's turn");
            }
        }

        [Test(Description = "When EndRound is called, there is a specific execution order " +
            "between different classes that is always maintained")]
        public void EndRound_HasCorrectExecutionOrder() {
            var city = new Mock<ICity>().Object;
            var civilization = new Mock<ICivilization>().Object;
            var unit = new Mock<IUnit>().Object;

            MockCityFactory        .Setup(factory => factory.AllCities)       .Returns(new List<ICity>()         { city         }.AsReadOnly());
            MockCivilizationFactory.Setup(factory => factory.AllCivilizations).Returns(new List<ICivilization>() { civilization }.AsReadOnly());
            MockUnitFactory        .Setup(factory => factory.AllUnits)        .Returns(new List<IUnit>()         { unit         });

            var executionSequence = new MockSequence();

            MockRoundExecuter.InSequence(executionSequence).Setup(executer => executer.EndRoundOnCity        (city));
            MockRoundExecuter.InSequence(executionSequence).Setup(executer => executer.EndRoundOnCivilization(civilization));
            MockRoundExecuter.InSequence(executionSequence).Setup(executer => executer.EndRoundOnUnit        (unit));

            MockAbilityExecuter.InSequence(executionSequence).Setup(executer => executer.PerformOngoingAbilities());

            var gameCore = Container.Resolve<GameCore>();
            gameCore.EndRound();

            MockRoundExecuter.VerifyAll();
        }

        [Test(Description = "When EndRound is called, after all objects have been passed " +
            "to TurnExecuter, GameCore should fire TurnEndedSignal")]
        public void EndRound_FiresRoundEndedSignal() {
            var gameCore = Container.Resolve<GameCore>();

            var coreSignals = Container.Resolve<CoreSignals>();
            coreSignals.RoundEndedSignal.Subscribe(turn => Assert.Pass());

            gameCore.EndRound();
            Assert.Fail("TurnEndedSignal was never fired");
        }

        [Test(Description = "When EndTurnRequestedSignal fires, GameCore should call EndRound " +
            "and then call BeginRound")]
        public void EndTurnRequestedSignalFired_RoundEndedThenBegan() {
            var city = new Mock<ICity>().Object;
            var civilization = BuildCivilization();

            MockCityFactory.Setup(factory => factory.AllCities).Returns(new List<ICity>() { city }.AsReadOnly());

            MockCivilizationFactory
                .Setup(factory => factory.AllCivilizations)
                .Returns(new List<ICivilization>() { civilization }.AsReadOnly());

            MockGrid.Setup(grid => grid.AllCells).Returns(new List<IHexCell>().AsReadOnly());

            var playerSignals = Container.Resolve<PlayerSignals>();

            Container.Resolve<GameCore>();

            var executionSequence = new MockSequence();
            MockRoundExecuter.InSequence(executionSequence).Setup(executer => executer.EndRoundOnCity(city));
            MockRoundExecuter.InSequence(executionSequence).Setup(executer => executer.BeginRoundOnCity(city));

            playerSignals.EndTurnRequestedSignal.OnNext(UniRx.Unit.Default);

            MockRoundExecuter.VerifyAll();
        }

        #endregion

        #region utilities

        private ICivilization BuildCivilization() {
            return new Mock<ICivilization>().Object;
        }

        #endregion

        #endregion

    }

}
