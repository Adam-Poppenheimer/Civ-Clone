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

        private Mock<IPlayerFactory> MockPlayerFactory;

        private CoreSignals   CoreSignals;
        private PlayerSignals PlayerSignals;

        private List<IPlayer>       AllPlayers       = new List<IPlayer>();
        private List<IResourceNode> AllResourceNodes = new List<IResourceNode>();

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            AllResourceNodes.Clear();
            AllPlayers      .Clear();

            MockPlayerFactory = new Mock<IPlayerFactory>();
            CoreSignals       = new CoreSignals();
            PlayerSignals     = new PlayerSignals();

            MockPlayerFactory.Setup(factory => factory.AllPlayers).Returns(AllPlayers.AsReadOnly());

            Container.Bind<IPlayerFactory>().FromInstance(MockPlayerFactory.Object);
            Container.Bind<CoreSignals>   ().FromInstance(CoreSignals);
            Container.Bind<PlayerSignals> ().FromInstance(PlayerSignals);

            Container.Bind<GameCore>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void BeginRound_FiresStartingRoundSignal_BeforeRoundBeganSignal() {
            CoreSignals.StartingRound.Subscribe(turnNumber => {
                Assert.Pass();
            });

            CoreSignals.RoundBegan.Subscribe(turnNumber => {
                Assert.Fail("RoundBegan fired before StartingRound");
            });

            var gameCore = Container.Resolve<GameCore>();

            gameCore.BeginRound();
        }

        [Test]
        public void BeginRound_FiresRoundBeganSignal() {
            var gameCore = Container.Resolve<GameCore>();

            var coreSignals = Container.Resolve<CoreSignals>();
            coreSignals.RoundBegan.Subscribe(turn => Assert.Pass());

            gameCore.BeginRound();
            Assert.Fail("RoundBegan was never fired");
        }

        [Test]
        public void EndRound_FiresEndingRoundSignal_BeforeRoundEndedSignal() {
            CoreSignals.EndingRound.Subscribe(turnNumber => {
                Assert.Pass();
            });

            CoreSignals.RoundEnded.Subscribe(turnNumber => {
                Assert.Fail("RoundEnded fired before EndingRound");
            });

            var gameCore = Container.Resolve<GameCore>();

            gameCore.EndRound();

            Assert.Fail("EndingRound not fired as expected");
        }

        [Test]
        public void EndRound_FiresRoundEndedSignal() {
            var gameCore = Container.Resolve<GameCore>();

            CoreSignals.RoundEnded.Subscribe(turn => Assert.Pass());

            gameCore.EndRound();
            Assert.Fail("RoundEnded was never fired");
        }

        [Test]
        public void EndTurn_FiresTurnEndedOnActivePlayer() {
            var activePlayer = BuildPlayer(BuildCivilization());

            BuildPlayer(BuildCivilization());

            CoreSignals.TurnEnded.Subscribe(player => {
                Assert.AreEqual(activePlayer, player, "TurnEnded fired on an unexpected player");
                Assert.Pass();
            });

            var gameCore = Container.Resolve<GameCore>();

            gameCore.ActivePlayer = activePlayer;

            gameCore.EndTurn();

            Assert.Fail("TurnEnded not fired as expected");
        }

        [Test]
        public void EndTurn_AndSomePlayerIsAfterActivePlayer_SetsNextPlayerToActivePlayer() {
            var previousPlayer = BuildPlayer(BuildCivilization());
            var activePlayer   = BuildPlayer(BuildCivilization());
            var nextPlayer     = BuildPlayer(BuildCivilization());

            var gameCore = Container.Resolve<GameCore>();

            gameCore.ActivePlayer = activePlayer;

            gameCore.EndTurn();

            Assert.AreEqual(nextPlayer, gameCore.ActivePlayer);
        }

        [Test]
        public void EndTurn_AndSomePlayerIsAfterActivePlayer_FiresTurnBeganOnNextPlayer() {
            var previousPlayer = BuildPlayer(BuildCivilization());
            var activePlayer   = BuildPlayer(BuildCivilization());
            var nextPlayer     = BuildPlayer(BuildCivilization());

            CoreSignals.TurnBegan.Subscribe(player => {
                Assert.AreEqual(nextPlayer, player, "TurnBegan fired on an unexpected player");
                Assert.Pass();
            });

            var gameCore = Container.Resolve<GameCore>();

            gameCore.ActivePlayer = activePlayer;

            gameCore.EndTurn();

            Assert.Fail("TurnBegan not fired as expected");
        }

        [Test]
        public void EndTurn_AndPlayerIsLastPlayer_EndsAndBeginsRound() {
            var previousPlayer = BuildPlayer(BuildCivilization());
            var activePlayer   = BuildPlayer(BuildCivilization());

            bool hasEndedRound = false;
            CoreSignals.RoundEnded.Subscribe(turn => {
                hasEndedRound = true;
            });

            CoreSignals.RoundBegan.Subscribe(turn => {
                if(!hasEndedRound) {
                    Assert.Fail("RoundBegan fired before RoundEnded");
                }

                Assert.Pass();
            });

            var gameCore = Container.Resolve<GameCore>();

            gameCore.ActivePlayer = activePlayer;

            gameCore.EndTurn();

            Assert.Fail("RoundEnded or RoundBegan not fired as expected");
        }

        [Test]
        public void EndTurn_AndPlayerIsLastPlayer_SetsFirstPlayerToActivePlayer() {
            var firstPlayer    = BuildPlayer(BuildCivilization());
            var previousPlayer = BuildPlayer(BuildCivilization());
            var activePlayer   = BuildPlayer(BuildCivilization());

            var gameCore = Container.Resolve<GameCore>();

            gameCore.ActivePlayer = activePlayer;

            gameCore.EndTurn();

            Assert.AreEqual(firstPlayer, gameCore.ActivePlayer);
        }

        [Test]
        public void EndTurn_AndPlayerIsLastPlayer_firesTurnBeganOnFirstPlayer() {
            var firstPlayer    = BuildPlayer(BuildCivilization());
            var previousPlayer = BuildPlayer(BuildCivilization());
            var activePlayer   = BuildPlayer(BuildCivilization());

            var gameCore = Container.Resolve<GameCore>();

            gameCore.ActivePlayer = activePlayer;

            gameCore.EndTurn();

            Assert.AreEqual(firstPlayer, gameCore.ActivePlayer);
        }

        [Test]
        public void EndTurn_DoesNotThrowIfOnlyOnePlayer() {
            var activePlayer = BuildPlayer(BuildCivilization());

            var gameCore = Container.Resolve<GameCore>();

            gameCore.ActivePlayer = activePlayer;

            Assert.DoesNotThrow(() => gameCore.EndTurn());
        }

        [Test]
        public void EndTurn_DoesNotThrowIfActivePlayerNull() {
            var gameCore = Container.Resolve<GameCore>();

            gameCore.ActivePlayer = null;

            Assert.DoesNotThrow(() => gameCore.EndTurn());
        }

        #endregion

        #region utilities

        private ICivilization BuildCivilization() {
            return new Mock<ICivilization>().Object;
        }

        private IPlayer BuildPlayer(ICivilization controlledCiv) {
            var mockPlayer = new Mock<IPlayer>();

            mockPlayer.Setup(player => player.ControlledCiv).Returns(controlledCiv);

            var newPlayer = mockPlayer.Object;

            AllPlayers.Add(newPlayer);

            return newPlayer;
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
