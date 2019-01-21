using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using Zenject;
using Moq;

using Assets.Simulation;
using Assets.Simulation.AI;
using Assets.Simulation.Barbarians;
using Assets.Simulation.Civilizations;
using Assets.Simulation.Players;
using Assets.Simulation.Units;


namespace Assets.Tests.Simulation.Barbarians {

    public class BarbarianPlayerBrainTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IPossessionRelationship<ICivilization, IUnit>> MockUnitPossessionCanon;
        private Mock<IBarbarianUnitBrain>                           MockBarbarianUnitBrain;
        private Mock<IUnitCommandExecuter>                          MockUnitCommandExecuter;
        private Mock<IBarbarianTurnExecuter>                        MockTurnExecuter;
        private Mock<IInfluenceMapLogic>                            MockInfluenceMapLogic;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockUnitPossessionCanon   = new Mock<IPossessionRelationship<ICivilization, IUnit>>();
            MockBarbarianUnitBrain    = new Mock<IBarbarianUnitBrain>();
            MockUnitCommandExecuter   = new Mock<IUnitCommandExecuter>();
            MockTurnExecuter          = new Mock<IBarbarianTurnExecuter>();
            MockInfluenceMapLogic     = new Mock<IInfluenceMapLogic>();

            Container.Bind<IPossessionRelationship<ICivilization, IUnit>>().FromInstance(MockUnitPossessionCanon.Object);
            Container.Bind<IBarbarianUnitBrain>                          ().FromInstance(MockBarbarianUnitBrain .Object);
            Container.Bind<IUnitCommandExecuter>                         ().FromInstance(MockUnitCommandExecuter.Object);
            Container.Bind<IBarbarianTurnExecuter>                       ().FromInstance(MockTurnExecuter       .Object);
            Container.Bind<IInfluenceMapLogic>                           ().FromInstance(MockInfluenceMapLogic  .Object);

            Container.Bind<BarbarianPlayerBrain>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void RefreshAnalysis_PerformsEncampmentSpawning() {
            var player = BuildPlayer(BuildCiv(new List<IUnit>()));

            Action postExecutionAction = () => { };

            var barbarianBrain = Container.Resolve<BarbarianPlayerBrain>();

            barbarianBrain.ExecuteTurn(player, postExecutionAction);

            MockTurnExecuter.Verify(spawner => spawner.PerformEncampmentSpawning(It.IsAny<InfluenceMaps>()), Times.Once);
        }

        [Test]
        public void RefreshAnalysis_PerformsUnitSpawning() {
            var player = BuildPlayer(BuildCiv(new List<IUnit>()));

            Action postExecutionAction = () => { };

            var barbarianBrain = Container.Resolve<BarbarianPlayerBrain>();

            barbarianBrain.ExecuteTurn(player, postExecutionAction);

            MockTurnExecuter.Verify(spawner => spawner.PerformUnitSpawning(), Times.Once);
        }

        [Test]
        public void RefreshAnalysis_InfluenceMapsGenerated() {
            var civ = BuildCiv(new List<IUnit>());

            var player = BuildPlayer(civ);

            var barbarianBrain = Container.Resolve<BarbarianPlayerBrain>();

            barbarianBrain.RefreshAnalysis(player);

            MockInfluenceMapLogic.Verify(generator => generator.AssignMaps(It.IsAny<InfluenceMaps>(), civ), Times.Once);
        }

        [Test]
        public void Clear_InfluenceMapsCleared() {
            var barbarianBrain = Container.Resolve<BarbarianPlayerBrain>();

            barbarianBrain.Clear();

            MockInfluenceMapLogic.Verify(generator => generator.ClearMaps(It.IsAny<InfluenceMaps>()), Times.Once);
        }

        [Test]
        public void ExecuteTurn_ClearsAndThenSetsCommandsOfUnits_BelongingToFirstBarbarianCiv() {            
            var player = BuildPlayer(BuildCiv(new List<IUnit>()));

                             new List<IUnit>() { BuildUnit() };
            var unitsTwo   = new List<IUnit>() { BuildUnit() };
                             new List<IUnit>() { BuildUnit() };

            var commandsTwo = new List<IUnitCommand>();

            MockBarbarianUnitBrain.Setup(brain => brain.GetCommandsForUnit(unitsTwo[0], It.IsAny<InfluenceMaps>()))
                                  .Returns(commandsTwo);

            var executionSequence = new MockSequence();

            MockUnitCommandExecuter.InSequence(executionSequence).Setup(executer => executer.ClearCommandsForUnit(unitsTwo[0]));
            MockUnitCommandExecuter.InSequence(executionSequence).Setup(executer => executer.SetCommandsForUnit(unitsTwo[0], commandsTwo));

            var barbarianBrain = Container.Resolve<BarbarianPlayerBrain>();

            barbarianBrain.RefreshAnalysis(player);
            barbarianBrain.ExecuteTurn(player, () => { });

            MockUnitCommandExecuter.VerifyAll();
        }

        [Test]
        public void ExecuteTurn_DoesNotThrowIfNoBarbarianCivExists() {
            var player = BuildPlayer(BuildCiv(new List<IUnit>()));

            var barbarianBrain = Container.Resolve<BarbarianPlayerBrain>();

            barbarianBrain.RefreshAnalysis(player);

            Assert.DoesNotThrow(() => barbarianBrain.ExecuteTurn(player, () => { }));
        }

        [Test]
        public void ExecuteTurn_IteratesAllCommands_WithCorrectControlRelinquisher() {
            var player = BuildPlayer(BuildCiv(new List<IUnit>()));

            Action controlRelinquisher = () => { };

            var barbarianBrain = Container.Resolve<BarbarianPlayerBrain>();

            barbarianBrain.RefreshAnalysis(player);
            barbarianBrain.ExecuteTurn(player, controlRelinquisher);

            MockUnitCommandExecuter.Verify(executer => executer.IterateAllCommands(controlRelinquisher));
        }

        #endregion

        #region utilities

        private IPlayer BuildPlayer(ICivilization controlledCiv) {
            var mockPlayer = new Mock<IPlayer>();

            mockPlayer.Setup(player => player.ControlledCiv).Returns(controlledCiv);

            return mockPlayer.Object;
        }

        private IUnit BuildUnit() {
            return new Mock<IUnit>().Object;
        }

        private ICivilization BuildCiv(IEnumerable<IUnit> units) {
            var newCiv = new Mock<ICivilization>().Object;

            MockUnitPossessionCanon.Setup(canon => canon.GetPossessionsOfOwner(newCiv)).Returns(units);

            return newCiv;
        }

        #endregion

        #endregion

    }

}
