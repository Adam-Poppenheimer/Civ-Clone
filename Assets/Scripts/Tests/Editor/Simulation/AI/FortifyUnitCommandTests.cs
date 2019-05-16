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
using Assets.Simulation.Units.Combat;

namespace Assets.Tests.Simulation.AI {

    public class FortifyUnitCommandTests : ZenjectUnitTestFixture {

        #region instance fields and properties

        private Mock<IUnitFortificationLogic> MockFortificationLogic;
        private Mock<IAbilityExecuter>        MockAbilityExecuter;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockFortificationLogic = new Mock<IUnitFortificationLogic>();
            MockAbilityExecuter    = new Mock<IAbilityExecuter>();

            Container.Bind<IUnitFortificationLogic>().FromInstance(MockFortificationLogic.Object);
            Container.Bind<IAbilityExecuter>       ().FromInstance(MockAbilityExecuter   .Object);

            Container.Bind<FortifyUnitCommand>().AsSingle();
        }

        #endregion

        #region tests

        [Test]
        public void StartExecution_AndUnitAlreadyFortified_StatusSetToSucceeded() {
            var unit = BuildUnit();

            MockFortificationLogic.Setup(logic => logic.GetFortificationStatusForUnit(unit)).Returns(true);

            var unitCommand = Container.Resolve<FortifyUnitCommand>();

            unitCommand.UnitToFortify = unit;

            unitCommand.StartExecution();

            Assert.AreEqual(CommandStatus.Succeeded, unitCommand.Status);
        }

        [Test]
        public void StartExecution_AndUnitHasNoFortifyAbilities_StatusSetToFailed() {
            var unit = BuildUnit(
                BuildAbility(true),
                BuildAbility(
                    true,
                    new AbilityCommandRequest() { Type = AbilityCommandType.AnnexAdjacentTerritory }
                ),
                BuildAbility(
                    true,
                    new AbilityCommandRequest() { Type = AbilityCommandType.ClearVegetation },
                    new AbilityCommandRequest() { Type = AbilityCommandType.BuildRoad }
                )
            );

            var unitCommand = Container.Resolve<FortifyUnitCommand>();

            unitCommand.UnitToFortify = unit;

            unitCommand.StartExecution();

            Assert.AreEqual(CommandStatus.Failed, unitCommand.Status);
        }

        [Test]
        public void StartExecution_AndUnitCannotExecuteFirstFortifyAbility_StatusSetToFailed() {
            var unit = BuildUnit(
                BuildAbility(
                    false, new AbilityCommandRequest() { Type = AbilityCommandType.Fortify }
                ),
                BuildAbility(
                    true, new AbilityCommandRequest () { Type = AbilityCommandType.Fortify }
                )
            );

            var unitCommand = Container.Resolve<FortifyUnitCommand>();

            unitCommand.UnitToFortify = unit;

            unitCommand.StartExecution();

            Assert.AreEqual(CommandStatus.Failed, unitCommand.Status);
        }

        [Test]
        public void StartExecution_AndUnitCanExecuteFirstFortifyAbility_AbilityExecuted() {
            var abilities = new IAbilityDefinition[] {
                BuildAbility(true),
                BuildAbility(
                    false,
                    new AbilityCommandRequest() { Type = AbilityCommandType.GainFreeTech }
                ),
                BuildAbility(
                    true,
                    new AbilityCommandRequest() { Type = AbilityCommandType.Fortify },
                    new AbilityCommandRequest() { Type = AbilityCommandType.ClearVegetation }
                ),
                BuildAbility(
                    true,
                    new AbilityCommandRequest () { Type = AbilityCommandType.Fortify }
                )
            };

            var unit = BuildUnit(abilities);

            var unitCommand = Container.Resolve<FortifyUnitCommand>();

            unitCommand.UnitToFortify = unit;

            unitCommand.StartExecution();

            MockAbilityExecuter.Verify(
                executer => executer.ExecuteAbilityOnUnit(It.IsAny<IAbilityDefinition>(), unit),
                Times.Once, "ExecuteAbilityOnUnit called an unexpected number of times"
            );

            MockAbilityExecuter.Verify(
                executer => executer.ExecuteAbilityOnUnit(abilities[2], unit),
                Times.Once, "abilities[2] not executed as expected"
            );
        }

        [Test]
        public void StartExecution_AndUnitCanExecuteFirstFortifyAbility_StatusSetToSucceeded_IfUnitBecomesFortified() {
            var ability = BuildAbility(
                true,
                new AbilityCommandRequest() { Type = AbilityCommandType.Fortify },
                new AbilityCommandRequest() { Type = AbilityCommandType.ClearVegetation }
            );

            var unit = BuildUnit(ability);

            MockAbilityExecuter.Setup(
                executer => executer.ExecuteAbilityOnUnit(ability, unit)
            ).Callback(
                () => MockFortificationLogic.Setup(logic => logic.GetFortificationStatusForUnit(unit)).Returns(true)
            );

            var unitCommand = Container.Resolve<FortifyUnitCommand>();

            unitCommand.UnitToFortify = unit;

            unitCommand.StartExecution();

            Assert.AreEqual(CommandStatus.Succeeded, unitCommand.Status);
        }

        [Test]
        public void StartExecution_AndUnitCanExecuteFirstFortifyAbility_StatusSetToFailed_IfUnitDoesNotBecomeFortified() {
            var ability = BuildAbility(
                true,
                new AbilityCommandRequest() { Type = AbilityCommandType.Fortify },
                new AbilityCommandRequest() { Type = AbilityCommandType.ClearVegetation }
            );

            var unit = BuildUnit(ability);

            var unitCommand = Container.Resolve<FortifyUnitCommand>();

            unitCommand.UnitToFortify = unit;

            unitCommand.StartExecution();

            Assert.AreEqual(CommandStatus.Failed, unitCommand.Status);
        }

        [Test]
        public void StartExecution_ThrowsInvalidOperationException_IfUnitToFortifyNull() {
            var unitCommand = Container.Resolve<FortifyUnitCommand>();

            Assert.Throws<InvalidOperationException>(() => unitCommand.StartExecution());
        }

        #endregion

        #region utilities

        private IAbilityDefinition BuildAbility(
            bool canExecute, params AbilityCommandRequest[] commandRequests
        ) {
            var mockAbility = new Mock<IAbilityDefinition>();

            mockAbility.Setup(ability => ability.CommandRequests).Returns(commandRequests);

            var newAbility = mockAbility.Object;

            MockAbilityExecuter.Setup(
                executer => executer.CanExecuteAbilityOnUnit(newAbility, It.IsAny<IUnit>())
            ).Returns(canExecute);

            return newAbility;
        }

        private IUnit BuildUnit(params IAbilityDefinition[] abilities) {
            var mockUnit = new Mock<IUnit>();

            mockUnit.Setup(unit => unit.Abilities).Returns(abilities);

            return mockUnit.Object;
        }

        #endregion

        #endregion

    }

}
