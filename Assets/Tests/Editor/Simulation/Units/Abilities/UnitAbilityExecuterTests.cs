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
    public class UnitAbilityExecuterTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private List<IAbilityHandler> AllAbilityHandlers = new List<IAbilityHandler>();

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            AllAbilityHandlers.Clear();

            Container.Bind<IEnumerable<IAbilityHandler>>().WithId("Unit Ability Handlers").FromInstance(AllAbilityHandlers);

            Container.Bind<UnitSignals>().AsSingle();

            Container.Bind<AbilityExecuter>().AsSingle();
        }

        #endregion

        #region tests

        [Test(Description = "When ExecuteAbilityOnUnit is called, it should run down the " +
            "AllAbilityHandlers enumerable in order, calling CanHandleAbilityOnUnit on each of them " +
            "until one returns true, at which point it returns")]
        public void CanExecuteAbilityOnUnit_CallsUpToFirstValidHandler() {
            var firstFalseHandler  = BuildMockHandler(false);
            var secondFalseHandler = BuildMockHandler(false);

            var firstTrueHandler  = BuildMockHandler(true);
            var secondTrueHandler = BuildMockHandler(true);

            var thirdFalseHandler = BuildMockHandler(false);

            var ability = new Mock<IAbilityDefinition>().Object;
            var unit = new Mock<IUnit>().Object;            

            var abilityExecuter = Container.Resolve<AbilityExecuter>();

            abilityExecuter.CanExecuteAbilityOnUnit(ability, unit);

            firstFalseHandler.Verify(handler => handler.CanHandleAbilityOnUnit(ability, unit), Times.Once,
                "FirstFalseHandler.CanHandleAbilityOnUnit was not called as expected");

            secondFalseHandler.Verify(handler => handler.CanHandleAbilityOnUnit(ability, unit), Times.Once,
                "SecondFalseHandler.CanHandleAbilityOnUnit was not called as expected");

            firstTrueHandler.Verify(handler => handler.CanHandleAbilityOnUnit(ability, unit), Times.Once,
                "FirstTrueHandler.CanHandleAbilityOnUnit was not called as expected");

            secondTrueHandler.Verify(handler => handler.CanHandleAbilityOnUnit(ability, unit), Times.Never,
                "FirstTrueHandler.CanHandleAbilityOnUnit was unexpectedly called");

            thirdFalseHandler.Verify(handler => handler.CanHandleAbilityOnUnit(ability, unit), Times.Never,
                "FirstTrueHandler.CanHandleAbilityOnUnit was unexpectedly called");
        }

        [Test(Description = "When CanExecuteAbilityOnUnit is called and there are no handlers " +
            "for which CanHandleAbilityOnUnit returns true for the arguments, UnitAbilityExecuter " +
            "should return false")]
        public void CanExecuteAbilityOnUnit_ReturnsFalseIfNoValidHandlers() {
            var ability = new Mock<IAbilityDefinition>().Object;
            var unit = new Mock<IUnit>().Object;            

            var abilityExecuter = Container.Resolve<AbilityExecuter>();

            Assert.IsFalse(abilityExecuter.CanExecuteAbilityOnUnit(ability, unit),
                "CanExecuteAbilityOnUnit returned an unexpected value");
        }

        [Test(Description = "When ExecuteAbilityOnUnit is called, it should run down the " +
            "AllAbilityHandlers enumerable in order, calling TryHandleAbilityOnUnit on each of them " +
            "until one returns an AbilityExecutionResults object whose AbilityHandled field is true, " + 
            "at which point it should return")]
        public void ExecuteAbilityOnUnit_CallsUpToFirstValidHandler() {
            var firstFalseHandler  = BuildMockHandler(false);
            var secondFalseHandler = BuildMockHandler(false);

            var firstTrueHandler  = BuildMockHandler(true);
            var secondTrueHandler = BuildMockHandler(true);

            var thirdFalseHandler = BuildMockHandler(false);

            var ability = new Mock<IAbilityDefinition>().Object;
            var unit = new Mock<IUnit>().Object;            

            var abilityExecuter = Container.Resolve<AbilityExecuter>();

            abilityExecuter.ExecuteAbilityOnUnit(ability, unit);

            firstFalseHandler.Verify(handler => handler.TryHandleAbilityOnUnit(ability, unit), Times.Once,
                "FirstFalseHandler.TryHandleAbilityOnUnit was not called as expected");

            secondFalseHandler.Verify(handler => handler.TryHandleAbilityOnUnit(ability, unit), Times.Once,
                "SecondFalseHandler.TryHandleAbilityOnUnit was not called as expected");

            firstTrueHandler.Verify(handler => handler.TryHandleAbilityOnUnit(ability, unit), Times.Once,
                "FirstTrueHandler.TryHandleAbilityOnUnit was not called as expected");

            secondTrueHandler.Verify(handler => handler.TryHandleAbilityOnUnit(ability, unit), Times.Never,
                "FirstTrueHandler.TryHandleAbilityOnUnit was unexpectedly called");

            thirdFalseHandler.Verify(handler => handler.TryHandleAbilityOnUnit(ability, unit), Times.Never,
                "FirstTrueHandler.TryHandleAbilityOnUnit was unexpectedly called");
        }

        [Test(Description = "When ExecuteAbilityOnUnit is called and reaches a valid handler, it should " +
            "take the NewAbilityActivated field from the returned results, call BeginExecution on it, " +
            "and add it to its OngoingAbilities collection")]
        public void ExecuteAbilityOnUnit_DealsWithOngoingAbility() {
            var ongoingAbilityMock = new Mock<IOngoingAbility>();

            var handler = BuildMockHandler(true, ongoingAbilityMock.Object);

            var ability = new Mock<IAbilityDefinition>().Object;
            var unit = new Mock<IUnit>().Object;            

            var abilityExecuter = Container.Resolve<AbilityExecuter>();

            abilityExecuter.ExecuteAbilityOnUnit(ability, unit);

            ongoingAbilityMock.Verify(ongoing => ongoing.BeginExecution(), Times.Once,
                "The ongoing ability's BeginExecution method was not called as expected");

            CollectionAssert.Contains(abilityExecuter.OngoingAbilities, ongoingAbilityMock.Object,
                "UnitAbilityExecuter failed to add the new ongoing ability to its OngoingAbilities collection");
        }

        [Test(Description = "When ExecuteAbilityOnUnit is called and reaches a valid handler, it should " +
            "fire UnitSignals.UnitActivatedAbilitySignal on the argued unit and ability")]
        public void ExecuteAbilityOnUnit_FiresSignal() {
            var handler = BuildMockHandler(true, null);

            var ability = new Mock<IAbilityDefinition>().Object;
            var unit = new Mock<IUnit>().Object;            

            var abilityExecuter = Container.Resolve<AbilityExecuter>();

            Container.Resolve<UnitSignals>().UnitActivatedAbilitySignal.Subscribe(delegate(Tuple<IUnit, IAbilityDefinition> dataTuple) {
                Assert.AreEqual(unit, dataTuple.Item1, "UnitActivatedAbilitySignal passed an unexpected unit");
                Assert.AreEqual(ability, dataTuple.Item2, "UnitActivatedAbilitySignal passed an unexpected unit");
                Assert.Pass();
            });

            abilityExecuter.ExecuteAbilityOnUnit(ability, unit);

            Assert.Fail("UnitActivatedAbilitySignal was never fired");
        }

        [Test(Description = "When PerformOngoingAbilities is called, it should call the " +
            "TickExecution method of every IOngoingAbility in its OngoingAbilities collection")]
        public void PerformOngoingAbilities_TicksEveryAbility() {
            var ongoingMockOne   = new Mock<IOngoingAbility>();
            var ongoingMockTwo   = new Mock<IOngoingAbility>();
            var ongoingMockThree = new Mock<IOngoingAbility>();

            var handler = BuildMockHandler(true, ongoingMockOne.Object);

            var ability = new Mock<IAbilityDefinition>().Object;
            var unit = new Mock<IUnit>().Object;            

            var abilityExecuter = Container.Resolve<AbilityExecuter>();

            abilityExecuter.ExecuteAbilityOnUnit(ability, unit);

            handler.Setup(abilityHandler => abilityHandler.TryHandleAbilityOnUnit(ability, unit))
                .Returns(new AbilityExecutionResults(true, ongoingMockTwo.Object));

            abilityExecuter.ExecuteAbilityOnUnit(ability, unit);

            handler.Setup(abilityHandler => abilityHandler.TryHandleAbilityOnUnit(ability, unit))
                .Returns(new AbilityExecutionResults(true, ongoingMockThree.Object));

            abilityExecuter.ExecuteAbilityOnUnit(ability, unit);

            abilityExecuter.PerformOngoingAbilities();

            ongoingMockOne.Verify  (ongoing => ongoing.TickExecution(), Times.Once, "ongoingOne was not ticked as expected");
            ongoingMockTwo.Verify  (ongoing => ongoing.TickExecution(), Times.Once, "ongoingTwo was not ticked as expected");
            ongoingMockThree.Verify(ongoing => ongoing.TickExecution(), Times.Once, "ongoingThree was not ticked as expected");
        }

        [Test(Description = "When PerformOngoingAbilities is called, every ability should be " +
            "has its TerminateExecution method called if its IsReadyToTerminate method returns " +
            "true. Terminated abilities should be removed from the OngoingAbilities collection")]
        public void PerformOngoingAbilities_TerminatesAndRemovesValidAbilities() {
            var ongoingAbilityMock = new Mock<IOngoingAbility>();
            ongoingAbilityMock.Setup(ongoing => ongoing.IsReadyToTerminate()).Returns(true);

            var handler = BuildMockHandler(true, ongoingAbilityMock.Object);

            var ability = new Mock<IAbilityDefinition>().Object;
            var unit = new Mock<IUnit>().Object;            

            var abilityExecuter = Container.Resolve<AbilityExecuter>();

            abilityExecuter.ExecuteAbilityOnUnit(ability, unit);

            ongoingAbilityMock.ResetCalls();

            abilityExecuter.PerformOngoingAbilities();

            ongoingAbilityMock.Verify(ongoing => ongoing.TickExecution(),      Times.Once,        "ongoingAbility.TickExecution wasn't called as expected");
            ongoingAbilityMock.Verify(ongoing => ongoing.IsReadyToTerminate(), Times.AtLeastOnce, "ongoingAbility.IsReadyToTerminate wasn't called as expected");
            ongoingAbilityMock.Verify(ongoing => ongoing.TerminateExecution(), Times.Once,        "ongoingAbility.TerminateExecution wasn't called as expected");
        }

        [Test(Description = "When ExecuteAbilityOnUnit is called and there are no handlers " +
            "that can handle the argued ability on the argued unit, UnitAbilityExecuter should " +
            "throw a AbilityNotHandledException")]
        public void ExecuteAbilityOnUnit_ThrowsWhenNoValidHandler() {
            var ability = new Mock<IAbilityDefinition>().Object;
            var unit = new Mock<IUnit>().Object;            

            var abilityExecuter = Container.Resolve<AbilityExecuter>();

            Assert.Throws<AbilityNotHandledException>(() => abilityExecuter.ExecuteAbilityOnUnit(ability, unit));
        }

        #endregion

        #region utilities

        private Mock<IAbilityHandler> BuildMockHandler(bool canHandle, IOngoingAbility ongoingAbility = null) {
            var mockHandler = new Mock<IAbilityHandler>();

            mockHandler.Setup(handler => handler.TryHandleAbilityOnUnit(
                It.IsAny<IAbilityDefinition>(), It.IsAny<IUnit>())
            ).Returns(new AbilityExecutionResults(canHandle, ongoingAbility));

            mockHandler.Setup(handler => handler.CanHandleAbilityOnUnit(
                It.IsAny<IAbilityDefinition>(), It.IsAny<IUnit>()
            )).Returns(canHandle);

            AllAbilityHandlers.Add(mockHandler.Object);

            return mockHandler;
        }

        #endregion

        #endregion

    }

}
