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
using Assets.Simulation.Players;
using Assets.Simulation.HexMap;
using Assets.Simulation.MapResources;
using Assets.Simulation.Technology;
using Assets.Simulation.Diplomacy;

using Assets.UI.Core;

namespace Assets.Tests.Simulation.Core {

    [TestFixture]
    public class GameCoreTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<ICityFactory>   MockCityFactory;
        private Mock<IPlayerFactory> MockPlayerFactory;
        private Mock<IUnitFactory>   MockUnitFactory;
        private Mock<IRoundExecuter> MockRoundExecuter;
        private Mock<IHexGrid>       MockGrid;
        private Mock<ITechCanon>     MockTechCanon;
        private Mock<IDiplomacyCore> MockDiplomacyCore;

        private List<IResourceNode> AllResourceNodes = new List<IResourceNode>();

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            AllResourceNodes.Clear();

            MockRoundExecuter = new Mock<IRoundExecuter>();
            MockCityFactory   = new Mock<ICityFactory>();
            MockPlayerFactory = new Mock<IPlayerFactory>();
            MockUnitFactory   = new Mock<IUnitFactory>();
            MockGrid          = new Mock<IHexGrid>();
            MockTechCanon     = new Mock<ITechCanon>();
            MockDiplomacyCore = new Mock<IDiplomacyCore>();

            MockCityFactory  .Setup(factory => factory.AllCities) .Returns(new List<ICity>().AsReadOnly());
            MockPlayerFactory.Setup(factory => factory.AllPlayers).Returns(new List<IPlayer>().AsReadOnly());
            MockUnitFactory  .Setup(factory => factory.AllUnits)  .Returns(new List<IUnit>());

            Container.Bind<ICityFactory>  ().FromInstance(MockCityFactory  .Object);
            Container.Bind<IPlayerFactory>().FromInstance(MockPlayerFactory.Object);
            Container.Bind<IUnitFactory>  ().FromInstance(MockUnitFactory  .Object);
            Container.Bind<IRoundExecuter>().FromInstance(MockRoundExecuter.Object);
            Container.Bind<IHexGrid>      ().FromInstance(MockGrid         .Object);
            Container.Bind<ITechCanon>    ().FromInstance(MockTechCanon    .Object);
            Container.Bind<IDiplomacyCore>().FromInstance(MockDiplomacyCore.Object);

            Container.Bind<CoreSignals>().AsSingle();

            Container.Bind<PlayerSignals>().AsSingle();

            Container.Bind<CivilizationSignals>().AsSingle();

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

        [Test(Description = "When BeginRound is called, the controlled civs of all players in PlayerFactory " +
            "are passed to TurnExecuter properly")]
        public void BeginRound_AllCivilizationsCalled() {
            var allPlayers = new List<IPlayer>() {
                BuildPlayer(BuildCivilization()),
                BuildPlayer(BuildCivilization()),
                BuildPlayer(BuildCivilization()),
                BuildPlayer(BuildCivilization()),
                BuildPlayer(BuildCivilization()),
            };

            MockPlayerFactory.SetupGet(factory => factory.AllPlayers).Returns(allPlayers.AsReadOnly());

            var gameCore = Container.Resolve<GameCore>();

            gameCore.BeginRound();

            foreach(var player in allPlayers) {
                MockRoundExecuter.Verify(executer => executer.BeginRoundOnCivilization(player.ControlledCiv),
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
            var city   = new Mock<ICity>  ().Object;
            var player = BuildPlayer(BuildCivilization());
            var unit   = new Mock<IUnit>  ().Object;

            MockCityFactory  .Setup(factory => factory.AllCities) .Returns(new List<ICity>  () { city   }.AsReadOnly());
            MockPlayerFactory.Setup(factory => factory.AllPlayers).Returns(new List<IPlayer>() { player }.AsReadOnly());
            MockUnitFactory  .Setup(factory => factory.AllUnits)  .Returns(new List<IUnit>  () { unit   });

            var executionSequence = new MockSequence();

            MockRoundExecuter.InSequence(executionSequence).Setup(executer => executer.BeginRoundOnCity        (city));
            MockRoundExecuter.InSequence(executionSequence).Setup(executer => executer.BeginRoundOnCivilization(player.ControlledCiv));
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
        public void EndTurn_CellVisibilityRefreshed() {
            var cellMocks = new List<Mock<IHexCell>>() {
                new Mock<IHexCell>(), new Mock<IHexCell>(), new Mock<IHexCell>()
            };

            var gameCore = Container.Resolve<GameCore>();

            MockGrid.Setup(grid => grid.Cells).Returns(cellMocks.Select(mock => mock.Object).ToList().AsReadOnly());

            MockPlayerFactory.Setup(factory => factory.AllPlayers)
                             .Returns(new List<IPlayer>() { BuildPlayer(BuildCivilization()) }.AsReadOnly());

            MockCityFactory.Setup(factory => factory.AllCities).Returns(new List<ICity>().AsReadOnly());

            gameCore.ActivePlayer = MockPlayerFactory.Object.AllPlayers[0];

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

        [Test(Description = "When EndRound is called, the controlled civs of all players in PlayerFactory " +
            "are passed to TurnExecuter properly")]
        public void EndRound_AllCivilizationsCalled() {
            var allPlayers = new List<IPlayer>() {
                BuildPlayer(BuildCivilization()),
                BuildPlayer(BuildCivilization()),
                BuildPlayer(BuildCivilization()),
                BuildPlayer(BuildCivilization()),
                BuildPlayer(BuildCivilization()),
            };

            MockPlayerFactory.SetupGet(factory => factory.AllPlayers).Returns(allPlayers.AsReadOnly());

            var gameCore = Container.Resolve<GameCore>();

            gameCore.EndRound();

            foreach(var player in allPlayers) {
                MockRoundExecuter.Verify(executer => executer.EndRoundOnCivilization(player.ControlledCiv),
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
            var city   = new Mock<ICity>  ().Object;
            var player = BuildPlayer(BuildCivilization());
            var unit   = new Mock<IUnit>  ().Object;

            MockCityFactory  .Setup(factory => factory.AllCities) .Returns(new List<ICity>  () { city   }.AsReadOnly());
            MockPlayerFactory.Setup(factory => factory.AllPlayers).Returns(new List<IPlayer>() { player }.AsReadOnly());
            MockUnitFactory  .Setup(factory => factory.AllUnits)  .Returns(new List<IUnit>  () { unit   });

            var executionSequence = new MockSequence();

            MockRoundExecuter.InSequence(executionSequence).Setup(executer => executer.EndRoundOnCity        (city));
            MockRoundExecuter.InSequence(executionSequence).Setup(executer => executer.EndRoundOnCivilization(player.ControlledCiv));
            MockRoundExecuter.InSequence(executionSequence).Setup(executer => executer.EndRoundOnUnit        (unit));

            MockDiplomacyCore.InSequence(executionSequence).Setup(core => core.UpdateOngoingDeals());

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
            var player = BuildPlayer(civilization);

            MockCityFactory.Setup(factory => factory.AllCities).Returns(new List<ICity>() { city }.AsReadOnly());

            MockPlayerFactory.Setup(factory => factory.AllPlayers)
                             .Returns(new List<IPlayer>() { player }.AsReadOnly());

            MockGrid.Setup(grid => grid.Cells).Returns(new List<IHexCell>().AsReadOnly());

            var playerSignals = Container.Resolve<PlayerSignals>();

            Container.Resolve<GameCore>();

            var executionSequence = new MockSequence();
            MockRoundExecuter.InSequence(executionSequence).Setup(executer => executer.EndRoundOnCity(city));
            MockRoundExecuter.InSequence(executionSequence).Setup(executer => executer.BeginRoundOnCity(city));

            playerSignals.EndTurnRequested.OnNext(player);

            MockRoundExecuter.VerifyAll();
        }

        #endregion

        #region utilities

        private ICivilization BuildCivilization() {
            return new Mock<ICivilization>().Object;
        }

        private IPlayer BuildPlayer(ICivilization controlledCiv) {
            var mockPlayer = new Mock<IPlayer>();

            mockPlayer.Setup(player => player.ControlledCiv).Returns(controlledCiv);

            return mockPlayer.Object;
        }

        private IResourceNode BuildResourceNode(IResourceDefinition resource) {
            var mockNode = new Mock<IResourceNode>();

            mockNode.SetupAllProperties();
            mockNode.Setup(node => node.Resource).Returns(resource);

            var newNode = mockNode.Object;

            AllResourceNodes.Add(newNode);

            return mockNode.Object;
        }

        private IResourceDefinition BuildResourceDefinition() {
            return new Mock<IResourceDefinition>().Object;
        }

        #endregion

        #endregion

    }

}
