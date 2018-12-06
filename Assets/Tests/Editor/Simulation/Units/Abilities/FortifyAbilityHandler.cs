using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using Zenject;
using Moq;

using Assets.Simulation.Units;
using Assets.Simulation.Units.Abilities;

namespace Assets.Tests.Simulation.Units.Abilities {

    public class FortifyAbilityHandlerTests : ZenjectUnitTestFixture {

        #region instance fields and properties



        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            Container.Bind<FortifyAbilityHandler>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void CanHandleCommandOnUnit_TrueIfCommandTypeFortifiedAndUnitNotFortified() {
            Mock<IUnit> mockUnit;

            var unit = BuildUnit(false, out mockUnit);

            var command = new AbilityCommandRequest() { Type = AbilityCommandType.Fortify };

            var handler = Container.Resolve<FortifyAbilityHandler>();

            Assert.IsTrue(handler.CanHandleCommandOnUnit(command, unit));
        }

        [Test]
        public void CanHandleCommandOnUnit_FalseIfCommandTypeNotFortify() {
            Mock<IUnit> mockUnit;

            var unit = BuildUnit(false, out mockUnit);

            var command = new AbilityCommandRequest() { Type = AbilityCommandType.BuildRoad };

            var handler = Container.Resolve<FortifyAbilityHandler>();

            Assert.IsFalse(handler.CanHandleCommandOnUnit(command, unit));
        }

        [Test]
        public void CanHandleCommandOnUnit_FalseIfUnitAlreadyFortified() {
            Mock<IUnit> mockUnit;

            var unit = BuildUnit(true, out mockUnit);

            var command = new AbilityCommandRequest() { Type = AbilityCommandType.Fortify };

            var handler = Container.Resolve<FortifyAbilityHandler>();

            Assert.IsFalse(handler.CanHandleCommandOnUnit(command, unit));
        }

        [Test]
        public void HandleCommandOnUnit_AndCommandValid_UnitFortified() {
            Mock<IUnit> mockUnit;

            var unitToTest = BuildUnit(false, out mockUnit);

            var command = new AbilityCommandRequest() { Type = AbilityCommandType.Fortify };

            var handler = Container.Resolve<FortifyAbilityHandler>();

            handler.HandleCommandOnUnit(command, unitToTest);

            mockUnit.Verify(unit => unit.BeginFortifying(), Times.Once);
        }

        [Test]
        public void HandleCommandOnUnit_AndCommandInvalid_ThrowsInvalidOperationException() {
            Mock<IUnit> mockUnit;

            var unitToTest = BuildUnit(true, out mockUnit);

            var command = new AbilityCommandRequest() { Type = AbilityCommandType.Fortify };

            var handler = Container.Resolve<FortifyAbilityHandler>();

            Assert.Throws<InvalidOperationException>(() => handler.HandleCommandOnUnit(command, unitToTest));
        }

        #endregion

        #region utilities

        private IUnit BuildUnit(bool isFortified, out Mock<IUnit> mock) {
            mock = new Mock<IUnit>();

            mock.Setup(unit => unit.IsFortified).Returns(isFortified);

            return mock.Object;
        }

        #endregion

        #endregion

    }
}
