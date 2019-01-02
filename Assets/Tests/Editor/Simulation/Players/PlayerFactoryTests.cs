using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using Zenject;
using Moq;
using UniRx;

using Assets.Simulation.Players;
using Assets.Simulation.Civilizations;

namespace Assets.Tests.Simulation.Players {

    public class PlayerFactoryTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private PlayerSignals PlayerSignals;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            PlayerSignals = new PlayerSignals();

            Container.Bind<PlayerSignals>().FromInstance(PlayerSignals);

            Container.Bind<PlayerFactory>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void CreatePlayer_CorrectCivPassedToNewPlayer() {
            var civ   = BuildCiv();
            var brain = BuildBrain();

            var factory = Container.Resolve<PlayerFactory>();

            var newPlayer = factory.CreatePlayer(civ, brain);

            Assert.AreEqual(civ, newPlayer.ControlledCiv);
        }

        [Test]
        public void CreatePlayer_CorrectBrainPassedToNewPlayer() {
            var civ   = BuildCiv();
            var brain = BuildBrain();

            var factory = Container.Resolve<PlayerFactory>();

            var newPlayer = factory.CreatePlayer(civ, brain);

            Assert.AreEqual(brain, newPlayer.Brain);
        }

        [Test]
        public void CreatePlayer_NewPlayerAddedToAllPlayers() {
            var civ   = BuildCiv();
            var brain = BuildBrain();

            var factory = Container.Resolve<PlayerFactory>();

            var newPlayer = factory.CreatePlayer(civ, brain);

            CollectionAssert.Contains(factory.AllPlayers, newPlayer);
        }

        [Test]
        public void CreatePlayer_PlayerCreatedSignalFired() {
            var civ   = BuildCiv();
            var brain = BuildBrain();

            var factory = Container.Resolve<PlayerFactory>();

            IPlayer playerPassedToSignal = null;

            PlayerSignals.PlayerCreated.Subscribe(delegate(IPlayer player) {
                playerPassedToSignal = player;
            });

            var newPlayer = factory.CreatePlayer(civ, brain);

            Assert.AreEqual(newPlayer, playerPassedToSignal);
        }

        [Test]
        public void DestroyPlayer_PlayerRemovedFromAllPlayers() {
            var civ   = BuildCiv();
            var brain = BuildBrain();

            var factory = Container.Resolve<PlayerFactory>();

            var newPlayer = factory.CreatePlayer(civ, brain);

            factory.DestroyPlayer(newPlayer);

            CollectionAssert.DoesNotContain(factory.AllPlayers, newPlayer);
        }

        [Test]
        public void DestroyPlayer_PlayerBeingDestroyedSignalFired() {
            var civ   = BuildCiv();
            var brain = BuildBrain();

            var factory = Container.Resolve<PlayerFactory>();

            IPlayer playerPassedToSignal = null;

            PlayerSignals.PlayerBeingDestroyed.Subscribe(delegate(IPlayer player) {
                playerPassedToSignal = player;
            });

            var newPlayer = factory.CreatePlayer(civ, brain);

            factory.DestroyPlayer(newPlayer);

            Assert.AreEqual(newPlayer, playerPassedToSignal);
        }

        #endregion

        #region utilities

        private ICivilization BuildCiv() {
            return new Mock<ICivilization>().Object;
        }

        private IPlayerBrain BuildBrain() {
            return new Mock<IPlayerBrain>().Object;
        }

        #endregion

        #endregion

    }

}
