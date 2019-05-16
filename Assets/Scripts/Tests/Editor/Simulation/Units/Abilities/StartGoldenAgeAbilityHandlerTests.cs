using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using NUnit.Framework;
using Zenject;
using Moq;

using Assets.Simulation;
using Assets.Simulation.Units;
using Assets.Simulation.Units.Abilities;
using Assets.Simulation.Civilizations;

namespace Assets.Tests.Simulation.Units.Abilities {

    public class StartGoldenAgeAbilityHandlerTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IPossessionRelationship<ICivilization, IUnit>> MockUnitPossessionCanon;
        private Mock<IGoldenAgeCanon>                               MockGoldenAgeCanon;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockUnitPossessionCanon = new Mock<IPossessionRelationship<ICivilization, IUnit>>();
            MockGoldenAgeCanon      = new Mock<IGoldenAgeCanon>();

            Container.Bind<IPossessionRelationship<ICivilization, IUnit>>().FromInstance(MockUnitPossessionCanon.Object);
            Container.Bind<IGoldenAgeCanon>                              ().FromInstance(MockGoldenAgeCanon     .Object);

            Container.Bind<StartGoldenAgeAbilityHandler>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void CanHandleCommandOnUnit_TrueIfCommandOfTypeStartGoldenAge_AndHasNumericFirstArgument() {
            var command = new AbilityCommandRequest() {
                Type = AbilityCommandType.StartGoldenAge,
                ArgsToPass = new List<string>() { "10" }
            };

            var unit = BuildUnit(BuildCiv(false));

            var abilityHandler = Container.Resolve<StartGoldenAgeAbilityHandler>();

            Assert.IsTrue(abilityHandler.CanHandleCommandOnUnit(command, unit));
        }

        [Test]
        public void CanHandleCommandOnUnit_FalseIfCommandIsNotOfTypeStartGoldenAge() {
            var command = new AbilityCommandRequest() {
                Type = AbilityCommandType.BuildRoad,
                ArgsToPass = new List<string>() { "10" }
            };

            var unit = BuildUnit(BuildCiv(false));

            var abilityHandler = Container.Resolve<StartGoldenAgeAbilityHandler>();

            Assert.IsFalse(abilityHandler.CanHandleCommandOnUnit(command, unit));
        }

        [Test]
        public void CanHandleCommandOnUnit_FalseIfCommandHasMoreThanOneArgument() {
            var command = new AbilityCommandRequest() {
                Type = AbilityCommandType.StartGoldenAge,
                ArgsToPass = new List<string>() { "10", "15" }
            };

            var unit = BuildUnit(BuildCiv(false));

            var abilityHandler = Container.Resolve<StartGoldenAgeAbilityHandler>();

            Assert.IsFalse(abilityHandler.CanHandleCommandOnUnit(command, unit));
        }

        [Test]
        public void CanHandleCommandOnUnit_FalseIfArgumentIsntAValidInt() {
            var command = new AbilityCommandRequest() {
                Type = AbilityCommandType.StartGoldenAge,
                ArgsToPass = new List<string>() { "Gary" }
            };

            var unit = BuildUnit(BuildCiv(false));

            var abilityHandler = Container.Resolve<StartGoldenAgeAbilityHandler>();

            Assert.IsFalse(abilityHandler.CanHandleCommandOnUnit(command, unit));
        }

        [Test]
        public void HandleCommandOnUnit_StartsNewGoldenAgeWithArguedLengthIsOwnerNotInOne() {
            var command = new AbilityCommandRequest() {
                Type = AbilityCommandType.StartGoldenAge,
                ArgsToPass = new List<string>() { "10" }
            };

            var civ = BuildCiv(false);

            var unit = BuildUnit(civ);

            var abilityHandler = Container.Resolve<StartGoldenAgeAbilityHandler>();

            abilityHandler.HandleCommandOnUnit(command, unit);

            MockGoldenAgeCanon.Verify(canon => canon.StartGoldenAgeForCiv(civ, 10), Times.Once);
        }

        [Test]
        public void HandleCommandOnUnit_ExtendsExistingGoldenAgeWithArguedLengthIfOwnerInOne() {
            var command = new AbilityCommandRequest() {
                Type = AbilityCommandType.StartGoldenAge,
                ArgsToPass = new List<string>() { "10" }
            };

            var civ = BuildCiv(true);

            var unit = BuildUnit(civ);

            var abilityHandler = Container.Resolve<StartGoldenAgeAbilityHandler>();

            abilityHandler.HandleCommandOnUnit(command, unit);

            MockGoldenAgeCanon.Verify(canon => canon.ChangeTurnsOfGoldenAgeForCiv(civ, 10), Times.Once);
        }

        [Test]
        public void HandleCommandOnUnit_ThrowsInvalidOperationExceptionWhenCannotBeHandled() {
            var command = new AbilityCommandRequest() { Type = AbilityCommandType.BuildImprovement};

            var civ = BuildCiv(false);

            var unit = BuildUnit(civ);

            var abilityHandler = Container.Resolve<StartGoldenAgeAbilityHandler>();

            Assert.Throws<InvalidOperationException>(() => abilityHandler.HandleCommandOnUnit(command, unit));
        }

        #endregion

        #region utilities

        private ICivilization BuildCiv(bool isInGoldenAge) {
            var newCiv = new Mock<ICivilization>().Object;

            MockGoldenAgeCanon.Setup(canon => canon.IsCivInGoldenAge(newCiv)).Returns(isInGoldenAge);

            return newCiv;
        }

        private IUnit BuildUnit(ICivilization owner) {
            var newUnit = new Mock<IUnit>().Object;

            MockUnitPossessionCanon.Setup(canon => canon.GetOwnerOfPossession(newUnit)).Returns(owner);

            return newUnit;
        }

        private IAbilityDefinition BuildAbility(params AbilityCommandRequest[] commandRequests) {
            var mockAbility = new Mock<IAbilityDefinition>();

            mockAbility.Setup(ability => ability.CommandRequests).Returns(commandRequests);

            return mockAbility.Object;
        }

        #endregion

        #endregion

    }

}
