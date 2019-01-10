using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using Zenject;
using Moq;

using Assets.Simulation;
using Assets.Simulation.Barbarians;
using Assets.Simulation.Civilizations;

namespace Assets.Tests.Simulation.Barbarians {

    public class BarbarianTurnExecuterTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IEncampmentFactory>          MockEncampmentFactory;
        private Mock<ICivilizationFactory>        MockCivFactory;
        private Mock<IBarbarianSpawningTools>     MockSpawningTools;
        private Mock<IRandomizer>                 MockRandomizer;
        private Mock<IBarbarianConfig>            MockBarbarianConfig;
        private Mock<IBarbarianEncampmentSpawner> MockEncampmentSpawner;
        private Mock<IBarbarianUnitSpawner>       MockUnitSpawner;

        private List<IEncampment> AllEncampments = new List<IEncampment>();

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            AllEncampments.Clear();

            MockEncampmentFactory = new Mock<IEncampmentFactory>();
            MockCivFactory        = new Mock<ICivilizationFactory>();
            MockSpawningTools     = new Mock<IBarbarianSpawningTools>();
            MockRandomizer        = new Mock<IRandomizer>();
            MockBarbarianConfig   = new Mock<IBarbarianConfig>();
            MockEncampmentSpawner = new Mock<IBarbarianEncampmentSpawner>();
            MockUnitSpawner       = new Mock<IBarbarianUnitSpawner>();

            MockEncampmentFactory.Setup(factory => factory.AllEncampments).Returns(AllEncampments.AsReadOnly());

            Container.Bind<IEncampmentFactory>         ().FromInstance(MockEncampmentFactory.Object);
            Container.Bind<ICivilizationFactory>       ().FromInstance(MockCivFactory       .Object);
            Container.Bind<IBarbarianSpawningTools>    ().FromInstance(MockSpawningTools    .Object);
            Container.Bind<IRandomizer>                ().FromInstance(MockRandomizer       .Object);
            Container.Bind<IBarbarianConfig>           ().FromInstance(MockBarbarianConfig  .Object);
            Container.Bind<IBarbarianEncampmentSpawner>().FromInstance(MockEncampmentSpawner.Object);
            Container.Bind<IBarbarianUnitSpawner>      ().FromInstance(MockUnitSpawner      .Object);

            Container.Bind<BarbarianTurnExecuter>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void PerformUnitSpawning_AddsRandomSpawnProgressToEachEncampment() {
            var encampmentOne   = BuildEncampment(0);
            var encampmentTwo   = BuildEncampment(15);
            var encampmentThree = BuildEncampment(4);

            MockBarbarianConfig.Setup(config => config.MinEncampmentSpawnProgress).Returns(7);
            MockBarbarianConfig.Setup(config => config.MaxEncampmentSpawnProgress).Returns(11);
            MockBarbarianConfig.Setup(config => config.ProgressNeededForUnitSpawn).Returns(100);

            MockRandomizer.SetupSequence(randomizer => randomizer.GetRandomRange(7, 11))
                          .Returns(17)
                          .Returns(20)
                          .Returns(-2);

            var turnExecuter = Container.Resolve<BarbarianTurnExecuter>();

            turnExecuter.PerformUnitSpawning();

            Assert.AreEqual(17, encampmentOne  .SpawnProgress, "EncampmentOne.SpawnProgress has an unexpected value");
            Assert.AreEqual(35, encampmentTwo  .SpawnProgress, "EncampmentTwo.SpawnProgress has an unexpected value");
            Assert.AreEqual(2,  encampmentThree.SpawnProgress, "EncampmentThree.SpawnProgress has an unexpected value");
        }

        [Test]
        public void PerformUnitSpawning_TriesToSpawnUnitOnEncampmentsWithSufficientProgress() {
            var encampmentOne   = BuildEncampment(0);
            var encampmentTwo   = BuildEncampment(15);
            var encampmentThree = BuildEncampment(4);

            MockBarbarianConfig.Setup(config => config.MinEncampmentSpawnProgress).Returns(7);
            MockBarbarianConfig.Setup(config => config.MaxEncampmentSpawnProgress).Returns(11);
            MockBarbarianConfig.Setup(config => config.ProgressNeededForUnitSpawn).Returns(21);

            MockRandomizer.Setup(randomizer => randomizer.GetRandomRange(7, 11)).Returns(20);

            var turnExecuter = Container.Resolve<BarbarianTurnExecuter>();

            turnExecuter.PerformUnitSpawning();

            MockUnitSpawner.Verify(spawner => spawner.TrySpawnUnit(encampmentOne),   Times.Never, "Unexpectedly spawned a unit on EncampmentOne");
            MockUnitSpawner.Verify(spawner => spawner.TrySpawnUnit(encampmentTwo),   Times.Once,  "Failed to spawn a unit on EncampmentTwo as expected");
            MockUnitSpawner.Verify(spawner => spawner.TrySpawnUnit(encampmentThree), Times.Once,  "Failed to spawn a unit on EncampmentThree as expected");
        }

        [Test]
        public void PerformUnitSpawning_DepletesProgressWhenTryingToSpawnAUnit() {
            var encampmentOne   = BuildEncampment(0);
            var encampmentTwo   = BuildEncampment(15);
            var encampmentThree = BuildEncampment(4);

            MockBarbarianConfig.Setup(config => config.MinEncampmentSpawnProgress).Returns(7);
            MockBarbarianConfig.Setup(config => config.MaxEncampmentSpawnProgress).Returns(11);
            MockBarbarianConfig.Setup(config => config.ProgressNeededForUnitSpawn).Returns(21);

            MockRandomizer.Setup(randomizer => randomizer.GetRandomRange(7, 11)).Returns(20);

            var turnExecuter = Container.Resolve<BarbarianTurnExecuter>();

            turnExecuter.PerformUnitSpawning();

            Assert.AreEqual(20, encampmentOne  .SpawnProgress, "EncampmentOne.SpawnProgress has an unexpected value");
            Assert.AreEqual(14, encampmentTwo  .SpawnProgress, "EncampmentTwo.SpawnProgress has an unexpected value");            
            Assert.AreEqual(3,  encampmentThree.SpawnProgress, "EncampmentThree.SpawnProgress has an unexpected value");
        }

        #endregion

        #region utilities

        private IEncampment BuildEncampment(int spawnProgress) {
            var mockEncampment = new Mock<IEncampment>();

            mockEncampment.SetupAllProperties();

            var newEncampment = mockEncampment.Object;

            newEncampment.SpawnProgress = spawnProgress;

            AllEncampments.Add(newEncampment);

            return newEncampment;
        }

        #endregion

        #endregion

    }

}
