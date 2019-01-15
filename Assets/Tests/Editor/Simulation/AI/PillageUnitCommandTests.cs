using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using Zenject;
using Moq;

using Assets.Simulation.AI;
using Assets.Simulation.Units;
using Assets.Simulation.Units.Abilities;

namespace Assets.Tests.Simulation.AI {

    public class PillageUnitCommandTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IAbilityExecuter> MockAbilityExecuter;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockAbilityExecuter = new Mock<IAbilityExecuter>();

            Container.Bind<IAbilityExecuter>().FromInstance(MockAbilityExecuter.Object);

            Container.Bind<PillageUnitCommand>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void StartExecution_ExecutesFirstValidPillagingAbility() {
            var abilities = new List<IAbilityDefinition>() {
                BuildAbility(false, new List<AbilityCommandRequest>()),
                BuildAbility(true,  new List<AbilityCommandRequest>() { new AbilityCommandRequest() { Type = AbilityCommandType.Fortify } }),
                BuildAbility(false, new List<AbilityCommandRequest>() { new AbilityCommandRequest() { Type = AbilityCommandType.Pillage } }),
                BuildAbility(true,  new List<AbilityCommandRequest>() { new AbilityCommandRequest() { Type = AbilityCommandType.Pillage } }),
                BuildAbility(true,  new List<AbilityCommandRequest>() { new AbilityCommandRequest() { Type = AbilityCommandType.Pillage } }),
            };

            var pillager = BuildUnit(abilities);

            var pillageCommand = Container.Resolve<PillageUnitCommand>();

            pillageCommand.Pillager = pillager;

            pillageCommand.StartExecution();

            MockAbilityExecuter.Verify(
                executer => executer.ExecuteAbilityOnUnit(abilities[0], pillager),
                Times.Never, "Unexpectedly executed abilities[0] on unit"
            );

            MockAbilityExecuter.Verify(
                executer => executer.ExecuteAbilityOnUnit(abilities[1], pillager),
                Times.Never, "Unexpectedly executed abilities[1] on unit"
            );

            MockAbilityExecuter.Verify(
                executer => executer.ExecuteAbilityOnUnit(abilities[2], pillager),
                Times.Never, "Unexpectedly executed abilities[2] on unit"
            );

            MockAbilityExecuter.Verify(
                executer => executer.ExecuteAbilityOnUnit(abilities[3], pillager),
                Times.Once, "Failed to execute abilities[3] on unit"
            );

            MockAbilityExecuter.Verify(
                executer => executer.ExecuteAbilityOnUnit(abilities[4], pillager),
                Times.Never, "Unexpectedly executed abilities[4] on unit"
            );
        }

        [Test]
        public void StartExecution_StatusSetToSucceededIfSomePillagingAbilityExecuted() {
            var abilities = new List<IAbilityDefinition>() {
                BuildAbility(false, new List<AbilityCommandRequest>()),
                BuildAbility(true,  new List<AbilityCommandRequest>() { new AbilityCommandRequest() { Type = AbilityCommandType.Pillage } }),
                BuildAbility(false, new List<AbilityCommandRequest>())
            };

            var pillager = BuildUnit(abilities);

            var pillageCommand = Container.Resolve<PillageUnitCommand>();

            pillageCommand.Pillager = pillager;

            pillageCommand.StartExecution();

            Assert.AreEqual(CommandStatus.Succeeded, pillageCommand.Status);
        }

        [Test]
        public void StartExecution_StatusSetToFailedIfNoPillagingAbilityExecuted() {
            var abilities = new List<IAbilityDefinition>() {
                BuildAbility(false, new List<AbilityCommandRequest>()),
                BuildAbility(false, new List<AbilityCommandRequest>())
            };

            var pillager = BuildUnit(abilities);

            var pillageCommand = Container.Resolve<PillageUnitCommand>();

            pillageCommand.Pillager = pillager;

            pillageCommand.StartExecution();

            Assert.AreEqual(CommandStatus.Failed, pillageCommand.Status);
        }

        [Test]
        public void StartExecution_ThrowsInvalidOperationExceptionIfPillagerIsNull() {
            var pillageCommand = Container.Resolve<PillageUnitCommand>();

            Assert.Throws<InvalidOperationException>(() => pillageCommand.StartExecution());
        }

        #endregion

        #region utilities

        private IAbilityDefinition BuildAbility(bool canExecute, IEnumerable<AbilityCommandRequest> commandRequests) {
            var mockAbility = new Mock<IAbilityDefinition>();

            mockAbility.Setup(ability => ability.CommandRequests).Returns(commandRequests);

            var newAbility = mockAbility.Object;

            MockAbilityExecuter.Setup(
                executer => executer.CanExecuteAbilityOnUnit(newAbility, It.IsAny<IUnit>())
            ).Returns(canExecute);

            return newAbility;
        }

        private IUnit BuildUnit(IEnumerable<IAbilityDefinition> abilities) {
            var mockUnit = new Mock<IUnit>();

            mockUnit.Setup(unit => unit.Abilities).Returns(abilities);

            return mockUnit.Object;
        }

        #endregion

        #endregion

    }

}
