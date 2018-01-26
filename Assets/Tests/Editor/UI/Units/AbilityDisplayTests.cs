using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

using Zenject;
using NUnit.Framework;
using Moq;

using Assets.Simulation.Units;
using Assets.Simulation.Units.Abilities;

using Assets.UI.Units;

namespace Assets.Tests.UI.Units {

    [TestFixture]
    public class AbilityDisplayTests : ZenjectUnitTestFixture {

        #region static fields and properties

        #region test cases

        private static IEnumerable RefreshNameFieldCases {
            get {
                yield return new TestCaseData(true, true, "Ability One")
                    .SetName("Non-null unit and non-null ability sets")
                    .Returns("Ability One");

                yield return new TestCaseData(true, false, "Ability One")
                    .SetName("Non-null unit and null ability does not set")
                    .Returns("");

                yield return new TestCaseData(false, true, "Ability One")
                    .SetName("Null unit and non-null ability does not set")
                    .Returns("");

                yield return new TestCaseData(false, false, "Ability One")
                    .SetName("Null unit and null ability does not set")
                    .Returns("");
            }
        }

        private static IEnumerable RefreshExecuteButtonInteractabilityCases {
            get {
                yield return new TestCaseData(true, true, "Ability One", true)
                    .SetName("Non-null unit, non-null ability, and executable combo returns true")
                    .Returns(true);

                yield return new TestCaseData(true, true, "Ability One", false)
                    .SetName("Non-null unit, non-null ability, and non-executable combo returns false")
                    .Returns(false);

                yield return new TestCaseData(true, false, "Ability One", true)
                    .SetName("Non-null unit, null ability, and executable combo returns false")
                    .Returns(false);

                yield return new TestCaseData(false, true, "Ability One", true)
                    .SetName("Null unit, non-null ability, and executable combo returns false")
                    .Returns(false);

                yield return new TestCaseData(false, false, "Ability One", true)
                    .SetName("Null unit, null ability, and executable combo returns false")
                    .Returns(false);
            }
        }

        #endregion

        #endregion

        #region instance fields and properties

        private Text NameField;

        private Button ExecuteButton;

        private Mock<IAbilityExecuter> MockAbilityExecuter;

        #endregion

        #region instance methods

        #region setup

        [SetUp]
        public void CommonInstall() {
            MockAbilityExecuter = new Mock<IAbilityExecuter>();

            Container.Bind<IAbilityExecuter>().FromInstance(MockAbilityExecuter.Object);

            NameField     = Container.InstantiateComponentOnNewGameObject<Text>();
            ExecuteButton = Container.InstantiateComponentOnNewGameObject<Button>();

            Container.Bind<Text>()  .WithId("Ability Display Name Field").FromInstance(NameField);
            Container.Bind<Button>().WithId("Ability Execute Button"    ).FromInstance(ExecuteButton);

            Container.Bind<AbilityDisplay>().FromNewComponentOnNewGameObject().AsSingle();
        }

        #endregion

        #region tests

        [Test(Description = "Refresh should assign NameField the name of AbilityToDisplay " +
            "if and only if AbilityToDisplay and UnitToInvokeOn are non-null")]
        [TestCaseSource("RefreshNameFieldCases")]
        public string Refresh_AssignsNameFieldProperly(bool hasUnit, bool hasAbility, string abilityName) {
            NameField.text = "";

            var abilityDisplay = Container.Resolve<AbilityDisplay>();

            abilityDisplay.AbilityToDisplay = hasAbility ? BuildAbility(abilityName) : null;
            abilityDisplay.UnitToInvokeOn   = hasUnit    ? BuildUnit()               : null;

            abilityDisplay.Refresh();

            return NameField.text;
        }

        [Test(Description = "Refresh should make ExecuteButton interactable if and only if " +
            "AbilityToDisplay and UnitToInvokeOn are non null and AbilityExecuter claims that " +
            "unit/ability combination is valid for execution")]
        [TestCaseSource("RefreshExecuteButtonInteractabilityCases")]
        public bool Refresh_SetsExecuteButtonInteractabilityProperly(bool hasUnit, bool hasAbility, string abilityName,
            bool canExecuteCombination) {
            ExecuteButton.interactable = false;

            var abilityDisplay = Container.Resolve<AbilityDisplay>();

            var ability = hasAbility ? BuildAbility(abilityName) : null;
            var unit    = hasUnit    ? BuildUnit()               : null;

            abilityDisplay.AbilityToDisplay = ability;
            abilityDisplay.UnitToInvokeOn   = unit;

            MockAbilityExecuter.Setup(executer => executer.CanExecuteAbilityOnUnit(ability, unit)).Returns(canExecuteCombination);

            abilityDisplay.Refresh();

            return ExecuteButton.interactable;
        }

        [Test(Description = "When Refresh is called and both AbilityToDisplay and UnitToInvokeOn are non-null, " +
            "AbilityDisplay adds a listener to ExecuteButton that calls AbilityExecuter.ExecuteAbilityOnUnit. " +
            "That callback should be removed if Refresh is called again, regardless of the state of its properties")]
        public void Refresh_AddsCorrectListenerToExecuteButton() {
            var ability = BuildAbility("Ability One");
            var unit    = BuildUnit();

            var abilityDisplay = Container.Resolve<AbilityDisplay>();

            abilityDisplay.AbilityToDisplay = ability;
            abilityDisplay.UnitToInvokeOn = unit;

            abilityDisplay.Refresh();

            ExecuteButton.onClick.Invoke();

            MockAbilityExecuter.Verify(executer => executer.ExecuteAbilityOnUnit(ability, unit), Times.Once,
                "AbilityExecuter.ExecuteAbilityOnUnit was not called as expected after the first refresh");

            abilityDisplay.AbilityToDisplay = null;
            abilityDisplay.UnitToInvokeOn = null;

            abilityDisplay.Refresh();

            MockAbilityExecuter.ResetCalls();

            ExecuteButton.onClick.Invoke();

            MockAbilityExecuter.Verify(executer => executer.ExecuteAbilityOnUnit(ability, unit), Times.Never,
                "AbilityExecuter.ExecuteAbilityOnUnit was not called as expected after the second refresh");
        }

        #endregion

        #region utilities

        private IUnit BuildUnit() {
            return new Mock<IUnit>().Object;
        }

        private IAbilityDefinition BuildAbility(string name) {
            var mockAbility = new Mock<IAbilityDefinition>();

            mockAbility.Setup(ability => ability.name).Returns(name);

            return mockAbility.Object;
        }

        #endregion

        #endregion

    }

}
