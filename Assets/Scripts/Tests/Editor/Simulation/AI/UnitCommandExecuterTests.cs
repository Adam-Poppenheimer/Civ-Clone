using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.TestTools;
using UnityEditor;

using NUnit.Framework;
using Zenject;
using Moq;

using Assets.Simulation.Units;
using Assets.Simulation.AI;

namespace Assets.Tests.Simulation.AI {

    public class UnitCommandExecuterTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private MonoBehaviour CoroutineInvoker;

        #endregion

        #region instance methods

        #region setup

        [OneTimeSetUp]
        public void OneTimeInstall() {
            CoroutineInvoker = new GameObject().AddComponent<Text>();
        }

        [SetUp]
        public void CommonInstall() {
            CoroutineInvoker.StopAllCoroutines();

            Container.Bind<MonoBehaviour>().WithId("Coroutine Invoker").FromInstance(CoroutineInvoker);

            Container.Bind<UnitCommandExecuter>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void GetCommandsForUnit_DefaultsToEmptySet() {
            var unit = BuildUnit();

            var commandExecuter = Container.Resolve<UnitCommandExecuter>();

            CollectionAssert.IsEmpty(commandExecuter.GetCommandsForUnit(unit));
        }

        [Test]
        public void SetCommandsForUnit_ReflectedInGetCommandsForUnit() {
            var unit = BuildUnit();

            var commandsOne = new List<IUnitCommand>() {
                BuildUnitCommand(CommandStatus.NotStarted), BuildUnitCommand(CommandStatus.NotStarted)
            };

            var commandsTwo = new List<IUnitCommand>() {
                BuildUnitCommand(CommandStatus.NotStarted), BuildUnitCommand(CommandStatus.NotStarted),
                BuildUnitCommand(CommandStatus.NotStarted)
            };

            var commandExecuter = Container.Resolve<UnitCommandExecuter>();

            commandExecuter.SetCommandsForUnit(unit, commandsOne);

            CollectionAssert.AreEqual(
                commandsOne, commandExecuter.GetCommandsForUnit(unit),
                "GetCommandsForUnit returned an expected value after the first set operation"
            );

            commandExecuter.SetCommandsForUnit(unit, commandsTwo);

            CollectionAssert.AreEqual(
                commandsTwo, commandExecuter.GetCommandsForUnit(unit),
                "GetCommandsForUnit returned an expected value after the second set operation"
            );
        }

        [Test]
        public void ClearCommandsForUnit_ReflectedInGetCommandsForUnit() {
            var unit = BuildUnit();

            var commands = new List<IUnitCommand>() {
                BuildUnitCommand(CommandStatus.NotStarted), BuildUnitCommand(CommandStatus.NotStarted)
            };

            var commandExecuter = Container.Resolve<UnitCommandExecuter>();

            commandExecuter.SetCommandsForUnit(unit, commands);

            commandExecuter.ClearCommandsForUnit(unit);

            CollectionAssert.IsEmpty(commandExecuter.GetCommandsForUnit(unit));
        }

        [UnityTest]
        public IEnumerator IterateAllCommands_AndFramePasses_CallsPostExecutionActionIfNoUnitHasCommands() {
            var commandExecuter = Container.Resolve<UnitCommandExecuter>();

            bool postExecutionActionCalled = false;
            commandExecuter.IterateAllCommands(() => postExecutionActionCalled = true);

            yield return null;

            Assert.IsTrue(postExecutionActionCalled);
        }

        [UnityTest]
        public IEnumerator IterateAllCommands_EveryFrame_RemovesAllCommandsOfUnit_IfTheCurrentCommandHasFailed() {
            var unit = BuildUnit();

            var unitCommands = new List<IUnitCommand>() {
                BuildUnitCommand(CommandStatus.Failed), BuildUnitCommand(CommandStatus.NotStarted),
                BuildUnitCommand(CommandStatus.Running)
            };

            var commandExecuter = Container.Resolve<UnitCommandExecuter>();

            commandExecuter.SetCommandsForUnit(unit, unitCommands);

            commandExecuter.IterateAllCommands(() => { });

            yield return null;

            CollectionAssert.IsEmpty(commandExecuter.GetCommandsForUnit(unit));
        }

        [UnityTest]
        public IEnumerator IterateAllCommands_EveryFrame_DoesntRemoveAllCommandsOfUnit_IfANonCurrentActionHasFailed() {
            var unit = BuildUnit();

            var unitCommands = new List<IUnitCommand>() {
                BuildUnitCommand(CommandStatus.Running), BuildUnitCommand(CommandStatus.Failed),
                BuildUnitCommand(CommandStatus.Running)
            };

            var commandExecuter = Container.Resolve<UnitCommandExecuter>();

            commandExecuter.SetCommandsForUnit(unit, unitCommands);

            commandExecuter.IterateAllCommands(() => { });

            yield return null;

            CollectionAssert.AreEqual(unitCommands, commandExecuter.GetCommandsForUnit(unit));
        }

        [UnityTest]
        public IEnumerator IterateAllCommands_EveryFrame_RemovesOnlyCurrentCommandOfUnit_IfThatCommandHasSucceeded() {
            var unit = BuildUnit();

            var unitCommands = new List<IUnitCommand>() {
                BuildUnitCommand(CommandStatus.Succeeded), BuildUnitCommand(CommandStatus.Failed),
                BuildUnitCommand(CommandStatus.Running)
            };

            var expectedCommands = new List<IUnitCommand>() { unitCommands[1], unitCommands[2] };

            var commandExecuter = Container.Resolve<UnitCommandExecuter>();

            commandExecuter.SetCommandsForUnit(unit, unitCommands);

            commandExecuter.IterateAllCommands(() => { });

            yield return null;

            CollectionAssert.AreEqual(expectedCommands, commandExecuter.GetCommandsForUnit(unit));
        }

        [UnityTest]
        public IEnumerator IterateAllCommands_EveryFrame_StartsExecutionOfCurrentCommand_IfCommandHasNotStarted() {
            var unit = BuildUnit();

            Mock<IUnitCommand> mockCommandOne;
 
            var unitCommands = new List<IUnitCommand>() {
                BuildUnitCommand(CommandStatus.NotStarted, out mockCommandOne), BuildUnitCommand(CommandStatus.Failed),
                BuildUnitCommand(CommandStatus.Running)
            };

            var commandExecuter = Container.Resolve<UnitCommandExecuter>();

            commandExecuter.SetCommandsForUnit(unit, unitCommands);

            commandExecuter.IterateAllCommands(() => { });

            yield return null;

            CollectionAssert.AreEqual(
                unitCommands, commandExecuter.GetCommandsForUnit(unit),
                "Unit has an unexpected list of commands"
            );

            mockCommandOne.Verify(command => command.StartExecution(), "CommandOne not executed as expected");
        }

        #endregion

        #region utilities

        private IUnit BuildUnit() {
            return new Mock<IUnit>().Object;
        }

        private IUnitCommand BuildUnitCommand(CommandStatus status) {
            Mock<IUnitCommand> mock;

            return BuildUnitCommand(status, out mock);
        }

        private IUnitCommand BuildUnitCommand(CommandStatus status, out Mock<IUnitCommand> mock) {
            mock = new Mock<IUnitCommand>();

            mock.Setup(command => command.Status).Returns(status);

            return mock.Object;
        }

        #endregion

        #endregion

    }

}
