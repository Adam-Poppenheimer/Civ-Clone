using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using Zenject;
using Moq;

using Assets.Simulation;
using Assets.Simulation.Units;
using Assets.Simulation.Civilizations;
using Assets.Simulation.AI;
using Assets.Simulation.Players.Barbarians;

namespace Assets.Tests.Simulation.Players {

    public class BarbarianPlayerBrainTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IPossessionRelationship<ICivilization, IUnit>> MockUnitPossessionCanon;
        private Mock<ICivilizationFactory>                          MockCivFactory;
        private Mock<IBarbarianUnitBrain>                           MockBarbarianUnitBrain;
        private Mock<IUnitCommandExecuter>                          MockUnitCommandExecuter;
        private Mock<IBarbarianInfluenceMapGenerator>               MockInfluenceMapGenerator;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockUnitPossessionCanon   = new Mock<IPossessionRelationship<ICivilization, IUnit>>();
            MockCivFactory            = new Mock<ICivilizationFactory>();
            MockBarbarianUnitBrain    = new Mock<IBarbarianUnitBrain>();
            MockUnitCommandExecuter   = new Mock<IUnitCommandExecuter>();
            MockInfluenceMapGenerator = new Mock<IBarbarianInfluenceMapGenerator>();

            Container.Bind<IPossessionRelationship<ICivilization, IUnit>>().FromInstance(MockUnitPossessionCanon  .Object);
            Container.Bind<ICivilizationFactory>                         ().FromInstance(MockCivFactory           .Object);
            Container.Bind<IBarbarianUnitBrain>                          ().FromInstance(MockBarbarianUnitBrain   .Object);
            Container.Bind<IUnitCommandExecuter>                         ().FromInstance(MockUnitCommandExecuter  .Object);
            Container.Bind<IBarbarianInfluenceMapGenerator>              ().FromInstance(MockInfluenceMapGenerator.Object);

            Container.Bind<BarbarianPlayerBrain>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void RefreshAnalysis_InfluenceMapsGenerated() {
            var barbarianBrain = Container.Resolve<BarbarianPlayerBrain>();

            barbarianBrain.RefreshAnalysis();

            MockInfluenceMapGenerator.Verify(generator => generator.GenerateMaps(), Times.Once);
        }

        [Test]
        public void Clear_InfluenceMapsCleared() {
            var barbarianBrain = Container.Resolve<BarbarianPlayerBrain>();

            barbarianBrain.Clear();

            MockInfluenceMapGenerator.Verify(generator => generator.ClearMaps(), Times.Once);
        }

        [Test]
        public void ExecuteTurn_ClearsAndThenSetsCommandsOfUnits_BelongingToFirstBarbarianCiv() {
            var unitsOne   = new List<IUnit>() { BuildUnit() };
            var unitsTwo   = new List<IUnit>() { BuildUnit() };
            var unitsThree = new List<IUnit>() { BuildUnit() };

            var allCivilizations = new List<ICivilization>() {
                BuildCiv(BuildCivTemplate(false), unitsOne),
                BuildCiv(BuildCivTemplate(true),  unitsTwo),
                BuildCiv(BuildCivTemplate(true),  unitsThree)
            };

            MockCivFactory.Setup(factory => factory.AllCivilizations).Returns(allCivilizations.AsReadOnly());

            var commandsTwo = new List<IUnitCommand>();

            MockBarbarianUnitBrain.Setup(brain => brain.GetCommandsForUnit(unitsTwo[0], It.IsAny<BarbarianInfluenceMaps>()))
                                  .Returns(commandsTwo);

            var executionSequence = new MockSequence();

            MockUnitCommandExecuter.InSequence(executionSequence).Setup(executer => executer.ClearCommandsForUnit(unitsTwo[0]));
            MockUnitCommandExecuter.InSequence(executionSequence).Setup(executer => executer.SetCommandsForUnit(unitsTwo[0], commandsTwo));

            var barbarianBrain = Container.Resolve<BarbarianPlayerBrain>();

            barbarianBrain.RefreshAnalysis();
            barbarianBrain.ExecuteTurn(() => { });

            MockUnitCommandExecuter.VerifyAll();
        }

        [Test]
        public void ExecuteTurn_DoesNotThrowIfNoBarbarianCivExists() {
            MockCivFactory.Setup(factory => factory.AllCivilizations).Returns(new List<ICivilization>().AsReadOnly());

            var barbarianBrain = Container.Resolve<BarbarianPlayerBrain>();

            barbarianBrain.RefreshAnalysis();

            Assert.DoesNotThrow(() => barbarianBrain.ExecuteTurn(() => { }));
        }

        [Test]
        public void ExecuteTurn_IteratesAllCommands_WithCorrectControlRelinquisher() {
            var allCivilizations = new List<ICivilization>() {
                BuildCiv(BuildCivTemplate(true), new List<IUnit>())
            };

            MockCivFactory.Setup(factory => factory.AllCivilizations).Returns(allCivilizations.AsReadOnly());

            Action controlRelinquisher = () => { };

            var barbarianBrain = Container.Resolve<BarbarianPlayerBrain>();

            barbarianBrain.RefreshAnalysis();
            barbarianBrain.ExecuteTurn(controlRelinquisher);

            MockUnitCommandExecuter.Verify(executer => executer.IterateAllCommands(controlRelinquisher));
        }

        #endregion

        #region utilities

        private IUnit BuildUnit() {
            return new Mock<IUnit>().Object;
        }

        private ICivilizationTemplate BuildCivTemplate(bool isBarbaric) {
            var mockTemplate = new Mock<ICivilizationTemplate>();

            mockTemplate.Setup(template => template.IsBarbaric).Returns(isBarbaric);

            return mockTemplate.Object;
        }

        private ICivilization BuildCiv(ICivilizationTemplate template, IEnumerable<IUnit> units) {
            var mockCiv = new Mock<ICivilization>();

            mockCiv.Setup(civ => civ.Template).Returns(template);

            var newCiv = mockCiv.Object;

            MockUnitPossessionCanon.Setup(canon => canon.GetPossessionsOfOwner(newCiv)).Returns(units);

            return newCiv;
        }

        #endregion

        #endregion

    }

}
