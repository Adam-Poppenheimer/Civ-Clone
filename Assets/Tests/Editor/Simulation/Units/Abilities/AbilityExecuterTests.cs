using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using NUnit.Framework;
using Moq;
using UniRx;

using Assets.Simulation.Units;
using Assets.Simulation.Units.Abilities;

namespace Assets.Tests.Simulation.Units.Abilities {

    [TestFixture]
    public class AbilityExecuterTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private UnitSignals UnitSignals;

        private List<IAbilityHandler> AllAbilityHandlers = new List<IAbilityHandler>();

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            AllAbilityHandlers.Clear();

            UnitSignals = new UnitSignals();

            Container.Bind<List<IAbilityHandler>>().FromInstance(AllAbilityHandlers);

            Container.Bind<UnitSignals>().FromInstance(UnitSignals);

            Container.Bind<AbilityExecuter>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void CanExecuteAbilityOnUnit_FalseIfAbilityRequiresMovementAndUnitHasNone() {
            Mock<IUnit> mockUnit;
            var unit = BuildUnit(0, out mockUnit);

            var commands = new List<AbilityCommandRequest>();

            var ability = BuildAbility(requiresMovement: true, commandRequests: commands);

            var abilityExecuter = Container.Resolve<AbilityExecuter>();

            Assert.IsFalse(abilityExecuter.CanExecuteAbilityOnUnit(ability, unit));
        }

        [Test]
        public void CanExecuteAbilityOnUnit_FalseIfAnyCommandCannotBeHandledBySomeHandler() {
            Mock<IUnit> mockUnit;
            var unit = BuildUnit(0, out mockUnit);

            var commands = new List<AbilityCommandRequest>() {
                new AbilityCommandRequest(), new AbilityCommandRequest(),
                new AbilityCommandRequest()
            };

            BuildAbilityHandler(commands[0]);
            BuildAbilityHandler(commands[0], commands[1]);
            BuildAbilityHandler(commands[0]);

            var ability = BuildAbility(requiresMovement: false, commandRequests: commands);

            var abilityExecuter = Container.Resolve<AbilityExecuter>();

            Assert.IsFalse(abilityExecuter.CanExecuteAbilityOnUnit(ability, unit));
        }

        [Test]
        public void CanExecuteAbilityOnUnit_TrueIfAllCommandsCanBeHandled_UnitHasNoMovement_AndAbilityDoesntRequireIt() {
            Mock<IUnit> mockUnit;
            var unit = BuildUnit(0, out mockUnit);

            var commands = new List<AbilityCommandRequest>() {
                new AbilityCommandRequest(), new AbilityCommandRequest(),
                new AbilityCommandRequest()
            };

            BuildAbilityHandler(commands[0], commands[1], commands[2]);

            var ability = BuildAbility(requiresMovement: false, commandRequests: commands);

            var abilityExecuter = Container.Resolve<AbilityExecuter>();

            Assert.IsTrue(abilityExecuter.CanExecuteAbilityOnUnit(ability, unit));
        }

        [Test]
        public void CanExecuteAbilityOnUnit_TrueIfAllCommandsCanBeHandled_AbilityRequiresMovement_AndUnitHasMovement() {
            Mock<IUnit> mockUnit;
            var unit = BuildUnit(1, out mockUnit);

            var commands = new List<AbilityCommandRequest>() {
                new AbilityCommandRequest(), new AbilityCommandRequest(),
                new AbilityCommandRequest()
            };

            BuildAbilityHandler(commands[0], commands[1], commands[2]);

            var ability = BuildAbility(requiresMovement: true, commandRequests: commands);

            var abilityExecuter = Container.Resolve<AbilityExecuter>();

            Assert.IsTrue(abilityExecuter.CanExecuteAbilityOnUnit(ability, unit));
        }

        [Test]
        public void ExecuteAbilityOnUnit_EveryCommandHandledWithFirstValidAbilityHandler() {
            Mock<IUnit> mockUnit;
            var unit = BuildUnit(0, out mockUnit);

            var commands = new List<AbilityCommandRequest>() {
                new AbilityCommandRequest(), new AbilityCommandRequest(),
                new AbilityCommandRequest()
            };

            Mock<IAbilityHandler> mockHandlerOne, mockHandlerTwo, mockHandlerThree;

            BuildAbilityHandler(out mockHandlerOne,   commands[0]);
            BuildAbilityHandler(out mockHandlerTwo,   commands[0], commands[1], commands[2]);
            BuildAbilityHandler(out mockHandlerThree, commands[2]);

            var ability = BuildAbility(requiresMovement: false, commandRequests: commands);

            var abilityExecuter = Container.Resolve<AbilityExecuter>();

            abilityExecuter.ExecuteAbilityOnUnit(ability, unit);

            mockHandlerOne.Verify(
                handler => handler.HandleCommandOnUnit(It.IsAny<AbilityCommandRequest>(), unit),
                Times.Once, "HandlerOne handled and unexpected number of commands"
            );

            mockHandlerTwo.Verify(
                handler => handler.HandleCommandOnUnit(It.IsAny<AbilityCommandRequest>(), unit),
                Times.Exactly(2), "HandlerTwo handled and unexpected number of commands"
            );

            mockHandlerThree.Verify(
                handler => handler.HandleCommandOnUnit(It.IsAny<AbilityCommandRequest>(), unit),
                Times.Never, "HandlerThree handled and unexpected number of commands"
            );

            mockHandlerOne.Verify(
                handler => handler.HandleCommandOnUnit(commands[0], unit),
                Times.Once, "Commands[0] wasn't handled by the expected handler"
            );

            mockHandlerTwo.Verify(
                handler => handler.HandleCommandOnUnit(commands[1], unit),
                Times.Once, "Commands[1] wasn't handled by the expected handler"
            );

            mockHandlerTwo.Verify(
                handler => handler.HandleCommandOnUnit(commands[2], unit),
                Times.Once, "Commands[2] wasn't handled by the expected handler"
            );
        }

        [Test]
        public void ExecuteAbilityOnUnit_AndAbilityConsumesMovement_UnitMovementSetToZero() {
            Mock<IUnit> mockUnit;
            var unit = BuildUnit(3, out mockUnit);

            var commands = new List<AbilityCommandRequest>();

            var ability = BuildAbility(consumesMovement: true, commandRequests: commands);

            var abilityExecuter = Container.Resolve<AbilityExecuter>();

            abilityExecuter.ExecuteAbilityOnUnit(ability, unit);

            Assert.AreEqual(0, unit.CurrentMovement);
        }

        [Test]
        public void ExecuteAbilityOnUnit_AndAbilityDoesNotConsumeMovement_UnitMovementNotChanged() {
            Mock<IUnit> mockUnit;
            var unit = BuildUnit(3, out mockUnit);

            var commands = new List<AbilityCommandRequest>();

            var ability = BuildAbility(consumesMovement: false, commandRequests: commands);

            var abilityExecuter = Container.Resolve<AbilityExecuter>();

            abilityExecuter.ExecuteAbilityOnUnit(ability, unit);

            Assert.AreEqual(3, unit.CurrentMovement);
        }

        [Test]
        public void ExecuteAbilityOnUnit_AndAbiltiyDestroysUnit_UnitDestroyed() {
            Mock<IUnit> mockUnit;
            var unitToTest = BuildUnit(0, out mockUnit);

            var commands = new List<AbilityCommandRequest>();

            var ability = BuildAbility(destroysUnit: true, commandRequests: commands);

            var abilityExecuter = Container.Resolve<AbilityExecuter>();

            abilityExecuter.ExecuteAbilityOnUnit(ability, unitToTest);

            mockUnit.Verify(unit => unit.Destroy(), Times.Once);
        }

        [Test]
        public void ExecuteAbilityOnUnit_AndAbilityDoesntDestroyUnit_UnitNotDestroyed() {
            Mock<IUnit> mockUnit;
            var unitToTest = BuildUnit(0, out mockUnit);

            var commands = new List<AbilityCommandRequest>();

            var ability = BuildAbility(destroysUnit: false, commandRequests: commands);

            var abilityExecuter = Container.Resolve<AbilityExecuter>();

            abilityExecuter.ExecuteAbilityOnUnit(ability, unitToTest);

            mockUnit.Verify(unit => unit.Destroy(), Times.Never);
        }

        [Test]
        public void ExecuteAbilityOnUnit_ActivatedAbilitySignalFired() {
            Mock<IUnit> mockUnit;
            var unitToTest = BuildUnit(0, out mockUnit);

            var commands = new List<AbilityCommandRequest>();

            var ability = BuildAbility(destroysUnit: false, commandRequests: commands);

            var abilityExecuter = Container.Resolve<AbilityExecuter>();

            UnitSignals.ActivatedAbility.Subscribe(delegate(UniRx.Tuple<IUnit, IAbilityDefinition> data) {
                Assert.AreEqual(unitToTest, data.Item1, "Incorrect unit passed");
                Assert.AreEqual(ability,    data.Item2, "Incorrect ability passed");

                Assert.Pass();
            });

            abilityExecuter.ExecuteAbilityOnUnit(ability, unitToTest);

            Assert.Fail("ActivatedAbilitySignal never fired");
        }



        [Test]
        public void ExecuteAbilityOnUnit_ThrowsInvalidOperationExceptionIfAbilityCannotBeExecuted() {
            Mock<IUnit> mockUnit;
            var unit = BuildUnit(0, out mockUnit);

            var commands = new List<AbilityCommandRequest>();

            var ability = BuildAbility(requiresMovement: true, commandRequests: commands);

            var abilityExecuter = Container.Resolve<AbilityExecuter>();

            Assert.Throws<InvalidOperationException>(() => abilityExecuter.ExecuteAbilityOnUnit(ability, unit));
        }

        #endregion

        #region utilities

        private IUnit BuildUnit(int currentMovement, out Mock<IUnit> mock) {
            mock = new Mock<IUnit>();

            mock.SetupAllProperties();

            var newUnit = mock.Object;

            newUnit.CurrentMovement = currentMovement;

            return newUnit;
        }

        private IAbilityDefinition BuildAbility(
            List<AbilityCommandRequest> commandRequests, bool requiresMovement = false,
            bool consumesMovement = false, bool destroysUnit = false
        ) {
            var mockAbility = new Mock<IAbilityDefinition>();

            mockAbility.Setup(ability => ability.RequiresMovement).Returns(requiresMovement);
            mockAbility.Setup(ability => ability.ConsumesMovement).Returns(consumesMovement);
            mockAbility.Setup(ability => ability.DestroysUnit)    .Returns(destroysUnit);
            mockAbility.Setup(ability => ability.CommandRequests) .Returns(commandRequests);

            return mockAbility.Object;
        }

        private IAbilityHandler BuildAbilityHandler(params AbilityCommandRequest[] commandRequests) {
            Mock<IAbilityHandler> mock;

            return BuildAbilityHandler(out mock, commandRequests);
        }

        private IAbilityHandler BuildAbilityHandler(
            out Mock<IAbilityHandler> mock, params AbilityCommandRequest[] commandRequests
        ) {
            mock = new Mock<IAbilityHandler>();

            foreach(var command in commandRequests) {
                mock.Setup(
                    handler => handler.CanHandleCommandOnUnit(command, It.IsAny<IUnit>())
                ).Returns(true);
            }
            
            var newHandler = mock.Object;

            AllAbilityHandlers.Add(newHandler);

            return newHandler;
        }

        #endregion

        #endregion

    }

}
