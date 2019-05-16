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

            Container.Bind<IPlayerBrain>().WithId("Human Brain")    .FromInstance(new Mock<IPlayerBrain>().Object);
            Container.Bind<IPlayerBrain>().WithId("Barbarian Brain").FromInstance(new Mock<IPlayerBrain>().Object);

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
        public void DestroyPlayer_PlayerCleared() {
            var mockPlayer = new Mock<IPlayer>();

            var factory = Container.Resolve<PlayerFactory>();

            factory.DestroyPlayer(mockPlayer.Object);

            mockPlayer.Verify(player => player.Clear(), Times.Once);
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

        [Test]
        public void AllPlayers_SortedByPlayerName() {
            var brain = BuildBrain();

            var factory = Container.Resolve<PlayerFactory>();

            var playerThree = factory.CreatePlayer(BuildCiv(BuildCivTemplate("Civ 3", false)), brain);
            var playerOne   = factory.CreatePlayer(BuildCiv(BuildCivTemplate("Civ 1", false)), brain);
            var playerTwo   = factory.CreatePlayer(BuildCiv(BuildCivTemplate("Civ 2", false)), brain);
            var playerFour  = factory.CreatePlayer(BuildCiv(BuildCivTemplate("Civ 4", false)), brain);

            CollectionAssert.AreEqual(
                new List<IPlayer>() { playerOne, playerTwo, playerThree, playerFour },
                factory.AllPlayers
            );
        }

        [Test]
        public void AllPlayers_BarbaricPlayersSortedAfterNonBarbaricOnes() {
            var brain = BuildBrain();

            var factory = Container.Resolve<PlayerFactory>();

            var playerThree = factory.CreatePlayer(BuildCiv(BuildCivTemplate("Civ 3", true)),  brain);
            var playerOne   = factory.CreatePlayer(BuildCiv(BuildCivTemplate("Civ 1", true)),  brain);
            var playerTwo   = factory.CreatePlayer(BuildCiv(BuildCivTemplate("Civ 2", false)), brain);
            var playerFour  = factory.CreatePlayer(BuildCiv(BuildCivTemplate("Civ 4", false)), brain);

            CollectionAssert.AreEqual(
                new List<IPlayer>() { playerTwo, playerFour, playerOne, playerThree },
                factory.AllPlayers
            );
        }

        #endregion

        #region utilities

        private ICivilizationTemplate BuildCivTemplate(string name, bool isBarbaric) {
            var mockTemplate = new Mock<ICivilizationTemplate>();

            mockTemplate.Name = name;
            mockTemplate.Setup(template => template.Name)      .Returns(name);
            mockTemplate.Setup(template => template.IsBarbaric).Returns(isBarbaric);

            return mockTemplate.Object;
        }

        private ICivilization BuildCiv() {
            return new Mock<ICivilization>().Object;
        }

        private ICivilization BuildCiv(ICivilizationTemplate template) {
            var mockCiv = new Mock<ICivilization>();

            mockCiv.Setup(civ => civ.Template).Returns(template);

            return mockCiv.Object;
        }

        private IPlayerBrain BuildBrain() {
            return new Mock<IPlayerBrain>().Object;
        }

        #endregion

        #endregion

    }

}
