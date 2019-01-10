using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using Zenject;
using Moq;

using Assets.Simulation.Players;
using Assets.Simulation.Civilizations;
using Assets.Simulation.Core;
using Assets.Simulation.MapManagement;

namespace Assets.Tests.Simulation.MapManagement {

    public class PlayerComposerTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IPlayerFactory>       MockPlayerFactory;
        private Mock<ICivilizationFactory> MockCivFactory;
        private Mock<IGameCore>            MockGameCore;
        private Mock<IBrainPile>           MockBrainPile;

        private List<ICivilization> AllCivs    = new List<ICivilization>();
        private List<IPlayerBrain>  AllBrains  = new List<IPlayerBrain>();
        private List<IPlayer>       AllPlayers = new List<IPlayer>();

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            AllCivs   .Clear();
            AllBrains .Clear();
            AllPlayers.Clear();

            MockPlayerFactory = new Mock<IPlayerFactory>();
            MockCivFactory    = new Mock<ICivilizationFactory>();
            MockGameCore      = new Mock<IGameCore>();
            MockBrainPile     = new Mock<IBrainPile>();

            MockCivFactory.Setup(factory => factory.AllCivilizations).Returns(AllCivs.AsReadOnly());

            MockPlayerFactory.Setup(factory => factory.AllPlayers).Returns(AllPlayers.AsReadOnly());

            MockBrainPile.Setup(factory => factory.AllBrains) .Returns(AllBrains.AsReadOnly());

            Container.Bind<IPlayerFactory>      ().FromInstance(MockPlayerFactory.Object);
            Container.Bind<ICivilizationFactory>().FromInstance(MockCivFactory   .Object);
            Container.Bind<IGameCore>           ().FromInstance(MockGameCore     .Object);
            Container.Bind<IBrainPile>          ().FromInstance(MockBrainPile    .Object);

            Container.Bind<PlayerComposer>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void ClearRuntime_ActivePlayerSetToNull() {
            var playerComposer = Container.Resolve<PlayerComposer>();

            playerComposer.ClearRuntime();

            MockGameCore.VerifySet(core => core.ActivePlayer = null, Times.Once);
        }

        [Test]
        public void ClearRuntime_AllPlayersDestroyed() {
            var civ   = BuildCiv(BuildCivTemplate("Template"));
            var brain = BuildBrain("Brain");

            var players = new List<IPlayer>() {
                BuildPlayer(civ, brain), BuildPlayer(civ, brain),
                BuildPlayer(civ, brain), BuildPlayer(civ, brain)
            };

            var playerComposer = Container.Resolve<PlayerComposer>();

            playerComposer.ClearRuntime();

            foreach(var player in players) {
                MockPlayerFactory.Verify(
                    factory => factory.DestroyPlayer(player), Times.Once, "Failed to destroy player " + player.Name
                );
            }
        }

        [Test]
        public void ComposePlayers_PlayerDataAddedForEachPlayer() {
            BuildPlayer(BuildCiv(BuildCivTemplate("Civ One")),   BuildBrain("Brain One"));
            BuildPlayer(BuildCiv(BuildCivTemplate("Civ Two")),   BuildBrain("Brain Two"));
            BuildPlayer(BuildCiv(BuildCivTemplate("Civ Three")), BuildBrain("Brain Three"));

            var playerComposer = Container.Resolve<PlayerComposer>();

            var mapData = new SerializableMapData();

            playerComposer.ComposePlayers(mapData);

            Assert.AreEqual(3, mapData.Players.Count);
        }

        [Test]
        public void ComposePlayers_ControlledCivStoredByTemplateName() {
            BuildPlayer(BuildCiv(BuildCivTemplate("Civ One")),   BuildBrain("Brain One"));
            BuildPlayer(BuildCiv(BuildCivTemplate("Civ Two")),   BuildBrain("Brain Two"));
            BuildPlayer(BuildCiv(BuildCivTemplate("Civ Three")), BuildBrain("Brain Three"));

            var playerComposer = Container.Resolve<PlayerComposer>();

            var mapData = new SerializableMapData();

            playerComposer.ComposePlayers(mapData);

            Assert.AreEqual("Civ One",   mapData.Players[0].ControlledCiv, "First composed player has an unexpected ControlledCiv");
            Assert.AreEqual("Civ Two",   mapData.Players[1].ControlledCiv, "Second composed player has an unexpected ControlledCiv");
            Assert.AreEqual("Civ Three", mapData.Players[2].ControlledCiv, "Third composed player has an unexpected ControlledCiv");
        }

        [Test]
        public void ComposePlayers_BrainStoredByName() {
            BuildPlayer(BuildCiv(BuildCivTemplate("Civ One")),   BuildBrain("Brain One"));
            BuildPlayer(BuildCiv(BuildCivTemplate("Civ Two")),   BuildBrain("Brain Two"));
            BuildPlayer(BuildCiv(BuildCivTemplate("Civ Three")), BuildBrain("Brain Three"));

            var playerComposer = Container.Resolve<PlayerComposer>();

            var mapData = new SerializableMapData();

            playerComposer.ComposePlayers(mapData);

            Assert.AreEqual("Brain One",   mapData.Players[0].Brain, "First composed player has an unexpected Brain");
            Assert.AreEqual("Brain Two",   mapData.Players[1].Brain, "Second composed player has an unexpected Brain");
            Assert.AreEqual("Brain Three", mapData.Players[2].Brain, "Third composed player has an unexpected Brain");
        }

        [Test]
        public void ComposePlayers_AndActivePlayerNotNull_ActivePlayerStoredByName() {
                            BuildPlayer(BuildCiv(BuildCivTemplate("Civ One")),   BuildBrain("Brain One"));
            var playerTwo = BuildPlayer(BuildCiv(BuildCivTemplate("Civ Two")),   BuildBrain("Brain Two"));
                            BuildPlayer(BuildCiv(BuildCivTemplate("Civ Three")), BuildBrain("Brain Three"));

            MockGameCore.Setup(core => core.ActivePlayer).Returns(playerTwo);

            var playerComposer = Container.Resolve<PlayerComposer>();

            var mapData = new SerializableMapData();

            playerComposer.ComposePlayers(mapData);

            Assert.AreEqual("Civ Two", mapData.ActivePlayer);
        }

        [Test]
        public void ComposePlayers_AndActivePlayerNull_StoredActivePlayerSetToNull() {
            BuildPlayer(BuildCiv(BuildCivTemplate("Civ One")),   BuildBrain("Brain One"));
            BuildPlayer(BuildCiv(BuildCivTemplate("Civ Two")),   BuildBrain("Brain Two"));
            BuildPlayer(BuildCiv(BuildCivTemplate("Civ Three")), BuildBrain("Brain Three"));

            var playerComposer = Container.Resolve<PlayerComposer>();

            var mapData = new SerializableMapData();

            playerComposer.ComposePlayers(mapData);

            Assert.IsNull(mapData.ActivePlayer);
        }

        [Test]
        public void DecomposePlayers_PlayersCreatedFromAppropriateCivsAndBrains() {
            var civOne   = BuildCiv(BuildCivTemplate("Civ One"));
            var civTwo   = BuildCiv(BuildCivTemplate("Civ Two"));
            var civThree = BuildCiv(BuildCivTemplate("Civ Three"));

            var brainOne   = BuildBrain("Brain One");
            var brainTwo   = BuildBrain("Brain Two");
            var brainThree = BuildBrain("Brain Three");

            var mapData = new SerializableMapData() {
                Players = new List<SerializablePlayerData>() {
                    new SerializablePlayerData() { ControlledCiv = "Civ One",   Brain = "Brain One"   },
                    new SerializablePlayerData() { ControlledCiv = "Civ Two",   Brain = "Brain Two"   },
                    new SerializablePlayerData() { ControlledCiv = "Civ Three", Brain = "Brain Three" },
                }
            };

            var playerComposer = Container.Resolve<PlayerComposer>();

            playerComposer.DecomposePlayers(mapData);

            MockPlayerFactory.Verify(
                factory => factory.CreatePlayer(civOne, brainOne),
                Times.Once, "Failed to decompose first player data as expected"
            );

            MockPlayerFactory.Verify(
                factory => factory.CreatePlayer(civTwo, brainTwo),
                Times.Once, "Failed to decompose second player data as expected"
            );

            MockPlayerFactory.Verify(
                factory => factory.CreatePlayer(civThree, brainThree),
                Times.Once, "Failed to decompose third player data as expected"
            );
        }

        [Test]
        public void DecomposePlayers_ThrowsIfSimulationLacksAValidCiv() {
            BuildCiv(BuildCivTemplate("Civ One"));

            BuildBrain("Brain One");

            var mapData = new SerializableMapData() {
                Players = new List<SerializablePlayerData>() {
                    new SerializablePlayerData() { ControlledCiv = "Civ Two", Brain = "Brain One" }
                }
            };

            var playerComposer = Container.Resolve<PlayerComposer>();

            Assert.Throws<InvalidOperationException>(() => playerComposer.DecomposePlayers(mapData));
        }

        [Test]
        public void DecomposePlayers_ThrowsIfSimulationLacksAValidBrain() {
            BuildCiv(BuildCivTemplate("Civ One"));

            BuildBrain("Brain One");

            var mapData = new SerializableMapData() {
                Players = new List<SerializablePlayerData>() {
                    new SerializablePlayerData() { ControlledCiv = "Civ One", Brain = "Brain Two" }
                }
            };

            var playerComposer = Container.Resolve<PlayerComposer>();

            Assert.Throws<InvalidOperationException>(() => playerComposer.DecomposePlayers(mapData));
        }

        [Test]
        public void DecomposePlayers_AppropriatelyNamedPlayerMadeActive() {
            BuildCiv(BuildCivTemplate("Civ One"));
            BuildCiv(BuildCivTemplate("Civ Two"));
            BuildCiv(BuildCivTemplate("Civ Three"));

            BuildBrain("Brain One");
            BuildBrain("Brain Two");
            BuildBrain("Brain Three");

            var mapData = new SerializableMapData() {
                Players = new List<SerializablePlayerData>() {
                    new SerializablePlayerData() { ControlledCiv = "Civ One",   Brain = "Brain One"   },
                    new SerializablePlayerData() { ControlledCiv = "Civ Two",   Brain = "Brain Two"   },
                    new SerializablePlayerData() { ControlledCiv = "Civ Three", Brain = "Brain Three" },
                },
                ActivePlayer = "Civ Three"
            };

            MockPlayerFactory.Setup(
                factory => factory.CreatePlayer(It.IsAny<ICivilization>(), It.IsAny<IPlayerBrain>())
            ).Callback<ICivilization, IPlayerBrain>(
                (civ, brain) => BuildPlayer(civ, brain)
            );

            var playerComposer = Container.Resolve<PlayerComposer>();

            playerComposer.DecomposePlayers(mapData);

            MockGameCore.VerifySet(core => core.ActivePlayer = AllPlayers[2], Times.Once);
        }

        [Test]
        public void DecomposePlayers_ActivePlayerSetToNullIfMapDataHasNullActivePlayer() {
            BuildCiv(BuildCivTemplate("Civ One"));
            BuildCiv(BuildCivTemplate("Civ Two"));
            BuildCiv(BuildCivTemplate("Civ Three"));

            BuildBrain("Brain One");
            BuildBrain("Brain Two");
            BuildBrain("Brain Three");

            var mapData = new SerializableMapData() {
                Players = new List<SerializablePlayerData>() {
                    new SerializablePlayerData() { ControlledCiv = "Civ One",   Brain = "Brain One"   },
                    new SerializablePlayerData() { ControlledCiv = "Civ Two",   Brain = "Brain Two"   },
                    new SerializablePlayerData() { ControlledCiv = "Civ Three", Brain = "Brain Three" },
                },
                ActivePlayer = null
            };

            MockPlayerFactory.Setup(
                factory => factory.CreatePlayer(It.IsAny<ICivilization>(), It.IsAny<IPlayerBrain>())
            ).Callback<ICivilization, IPlayerBrain>(
                (civ, brain) => BuildPlayer(civ, brain)
            );

            var playerComposer = Container.Resolve<PlayerComposer>();

            playerComposer.DecomposePlayers(mapData);

            MockGameCore.VerifySet(core => core.ActivePlayer = null, Times.Once);
        }

        #endregion

        #region utilities

        private ICivilizationTemplate BuildCivTemplate(string name) {
            var mockTemplate = new Mock<ICivilizationTemplate>();

            mockTemplate.Name = name;
            mockTemplate.Setup(template => template.Name).Returns(name);

            return mockTemplate.Object;
        }

        private ICivilization BuildCiv(ICivilizationTemplate template) {
            var mockCiv = new Mock<ICivilization>();

            mockCiv.Name = template.Name;
            mockCiv.Setup(civ => civ.Template).Returns(template);

            var newCiv = mockCiv.Object;

            AllCivs.Add(newCiv);

            return newCiv;
        }

        private IPlayerBrain BuildBrain(string name) {
            var mockBrain = new Mock<IPlayerBrain>();

            mockBrain.Name = name;
            mockBrain.Setup(brain => brain.Name).Returns(name);

            var newBrain = mockBrain.Object;

            AllBrains.Add(newBrain);

            return newBrain;
        }

        private IPlayer BuildPlayer(ICivilization controlledCiv, IPlayerBrain brain) {
            var mockPlayer = new Mock<IPlayer>();

            mockPlayer.Setup(player => player.ControlledCiv).Returns(controlledCiv);
            mockPlayer.Setup(player => player.Brain)        .Returns(brain);
            mockPlayer.Setup(player => player.Name)         .Returns(controlledCiv.Template.Name);

            var newPlayer = mockPlayer.Object;

            AllPlayers.Add(newPlayer);

            return newPlayer;
        }

        #endregion

        #endregion

    }

}
