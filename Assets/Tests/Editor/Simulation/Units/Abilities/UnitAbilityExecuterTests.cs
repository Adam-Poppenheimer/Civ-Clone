using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation.Units;
using Assets.Simulation.Units.Abilities;

namespace Assets.Tests.Simulation.Units.Abilities {

    [TestFixture]
    public class UnitAbilityExecuterTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private List<IUnitAbilityHandler> AllAbilityHandlers = new List<IUnitAbilityHandler>();

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            AllAbilityHandlers.Clear();

            Container.Bind<IEnumerable<IUnitAbilityHandler>>().WithId("Unit Ability Handlers").FromInstance(AllAbilityHandlers);

            Container.Bind<UnitAbilityExecuter>().AsSingle();
        }

        #endregion

        #region tests

        [Test(Description = "When ExecuteAbilityOnUnit is called, it should run down the " +
            "AllAbilityHandlers enumerable in order, calling TryHandleOnUnit on each of them " +
            "until one returns true, at which point it returns")]
        public void ExecuteAbilityOnUnit_CallsUpToFirstValidHandler() {
            var firstFalseHandler  = BuildMockHandler(false);
            var secondFalseHandler = BuildMockHandler(false);

            var firstTrueHandler  = BuildMockHandler(true);
            var secondTrueHandler = BuildMockHandler(true);

            var thirdFalseHandler = BuildMockHandler(false);

            var ability = new Mock<IUnitAbilityDefinition>().Object;
            var unit = new Mock<IUnit>().Object;            

            var abilityExecuter = Container.Resolve<UnitAbilityExecuter>();

            abilityExecuter.ExecuteAbilityOnUnit(ability, unit);

            firstFalseHandler.Verify(handler => handler.TryHandleAbilityOnUnit(ability, unit), Times.Once,
                "FirstFalseHandler.TryHandleAbilityOnUnit was not called as expected");

            secondFalseHandler.Verify(handler => handler.TryHandleAbilityOnUnit(ability, unit), Times.Once,
                "SecondFalseHandler.TryHandleAbilityOnUnit was not called as expected");

            firstTrueHandler.Verify(handler => handler.TryHandleAbilityOnUnit(ability, unit), Times.Once,
                "FirstTrueHandler.TryHandleAbilityOnUnit was not called as expected");

            secondTrueHandler.Verify(handler => handler.TryHandleAbilityOnUnit(ability, unit), Times.Never,
                "FirstTrueHandler.TryHandleAbilityOnUnit was not called as expected");

            thirdFalseHandler.Verify(handler => handler.TryHandleAbilityOnUnit(ability, unit), Times.Never,
                "FirstTrueHandler.TryHandleAbilityOnUnit was not called as expected");
        }

        [Test(Description = "When ExecuteAbilityOnUnit is called and there are no handlers " +
            "that can handle the argued ability on the argued unit, UnitAbilityExecuter should " +
            "throw a AbilityNotHandledException")]
        public void ExecuteAbilityOnUnit_ThrowsWhenNoValidHandler() {
            var ability = new Mock<IUnitAbilityDefinition>().Object;
            var unit = new Mock<IUnit>().Object;            

            var abilityExecuter = Container.Resolve<UnitAbilityExecuter>();

            Assert.Throws<AbilityNotHandledException>(() => abilityExecuter.ExecuteAbilityOnUnit(ability, unit));
        }

        #endregion

        #region utilities

        private Mock<IUnitAbilityHandler> BuildMockHandler(bool canHandle) {
            var mockHandler = new Mock<IUnitAbilityHandler>();

            mockHandler.Setup(handler => handler.TryHandleAbilityOnUnit(
                It.IsAny<IUnitAbilityDefinition>(), It.IsAny<IUnit>())
            ).Returns(canHandle);

            AllAbilityHandlers.Add(mockHandler.Object);

            return mockHandler;
        }

        #endregion

        #endregion

    }

}
