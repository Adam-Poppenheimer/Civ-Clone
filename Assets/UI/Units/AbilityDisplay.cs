using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;

using Zenject;

using Assets.Simulation.Units;
using Assets.Simulation.Units.Abilities;

namespace Assets.UI.Units {

    public class AbilityDisplay : MonoBehaviour, IAbilityDisplay {

        #region instance fields and properties

        #region from IAbilityDisplay

        public IAbilityDefinition AbilityToDisplay { get; set; }

        public IUnit UnitToInvokeOn { get; set; }

        #endregion

        [InjectOptional(Id = "Ability Display Name Field")]
        private Text NameField {
            get { return _nameField; }
            set {
                if(value != null) {
                    _nameField = value;
                }
            }
        }
        [SerializeField] private Text _nameField;
        
        [InjectOptional(Id = "Ability Execute Button")]
        private Button ExecuteButton {
            get { return _executeButton; }
            set {
                if(value != null) {
                    _executeButton = value;
                }
            }
        }
        [SerializeField] private Button _executeButton;
        
        private IAbilityExecuter AbilityExecuter;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(IAbilityExecuter abilityExecuter) {
            AbilityExecuter = abilityExecuter;
        }

        #region from IAbilityToDisplay

        public void Refresh() {
            ExecuteButton.onClick.RemoveAllListeners();

            if(AbilityToDisplay == null || UnitToInvokeOn == null) {
                return;
            }

            NameField.text = AbilityToDisplay.name;

            ExecuteButton.onClick.AddListener(() => AbilityExecuter.ExecuteAbilityOnUnit(AbilityToDisplay, UnitToInvokeOn));

            ExecuteButton.interactable = AbilityExecuter.CanExecuteAbilityOnUnit(AbilityToDisplay, UnitToInvokeOn);
        }

        #endregion

        #endregion

    }

}
