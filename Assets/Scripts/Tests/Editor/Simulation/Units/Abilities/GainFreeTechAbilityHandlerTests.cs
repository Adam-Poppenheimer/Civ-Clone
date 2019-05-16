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
using Assets.Simulation.Technology;

namespace Assets.Tests.Simulation.Units.Abilities {

    public class GainFreeTechAbilityHandlerTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IPossessionRelationship<ICivilization, IUnit>> MockUnitPossessionCanon;
        private Mock<ITechCanon>                                    MockTechCanon;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockUnitPossessionCanon = new Mock<IPossessionRelationship<ICivilization, IUnit>>();
            MockTechCanon = new Mock<ITechCanon>();

            Container.Bind<IPossessionRelationship<ICivilization, IUnit>>().FromInstance(MockUnitPossessionCanon.Object);
            Container.Bind<ITechCanon>                                   ().FromInstance(MockTechCanon          .Object);

            Container.Bind<GainFreeTechAbilityHandler>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void CanHandleCommandOnUnit_TrueIfCommandHasTypeGainFreeTech() {
            var command = new AbilityCommandRequest() { Type = AbilityCommandType.GainFreeTech };
            var unit = BuildUnit(BuildCiv());

            var abilityHandler = Container.Resolve<GainFreeTechAbilityHandler>();

            Assert.IsTrue(abilityHandler.CanHandleCommandOnUnit(command, unit));
        }

        [Test]
        public void CanHandleCommandOnUnit_FalseIfCommandDoesNotHaveTypeGainFreeTech() {
            var command = new AbilityCommandRequest() { Type = AbilityCommandType.BuildImprovement };
            var unit = BuildUnit(BuildCiv());

            var abilityHandler = Container.Resolve<GainFreeTechAbilityHandler>();

            Assert.IsFalse(abilityHandler.CanHandleCommandOnUnit(command, unit));
        }

        [Test]
        public void TryHandleCommandOnUnit_AndExecutionValid_AddsFreeTechToUnitOwner() {
            var civ = BuildCiv();

            var command = new AbilityCommandRequest() { Type = AbilityCommandType.GainFreeTech };

            var unit = BuildUnit(civ);

            var abilityHandler = Container.Resolve<GainFreeTechAbilityHandler>();

            abilityHandler.HandleCommandOnUnit(command, unit);

            MockTechCanon.Verify(canon => canon.AddFreeTechToCiv(civ), Times.Once);
        }

        [Test]
        public void TryHandleCommandOnUnit_AndExecutionValid_DoesNotThrow() {
            var civ = BuildCiv();

            var command = new AbilityCommandRequest() { Type = AbilityCommandType.GainFreeTech };

            var unit = BuildUnit(civ);

            var abilityHandler = Container.Resolve<GainFreeTechAbilityHandler>();

            Assert.DoesNotThrow(() => abilityHandler.HandleCommandOnUnit(command, unit));
        }

        [Test]
        public void TryHandleCommandOnUnit_AndExecutionInvalid_ReturnsCorrectResults() {
            var civ = BuildCiv();

            var command = new AbilityCommandRequest() { Type = AbilityCommandType.BuildImprovement };

            var unit = BuildUnit(civ);

            var abilityHandler = Container.Resolve<GainFreeTechAbilityHandler>();

            Assert.Throws<InvalidOperationException>(() => abilityHandler.HandleCommandOnUnit(command, unit));
        }

        #endregion

        #region utilities

        private IAbilityDefinition BuildAbility(params AbilityCommandRequest[] commandRequests) {
            var mockAbility = new Mock<IAbilityDefinition>();

            mockAbility.Setup(ability => ability.CommandRequests).Returns(commandRequests);

            return mockAbility.Object;
        }

        private IUnit BuildUnit(ICivilization owner) {
            var newUnit = new Mock<IUnit>().Object;

            MockUnitPossessionCanon.Setup(canon => canon.GetOwnerOfPossession(newUnit)).Returns(owner);

            return newUnit;
        }

        private ICivilization BuildCiv() {
            return new Mock<ICivilization>().Object;
        }

        #endregion

        #endregion

    }

}
