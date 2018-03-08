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

        [SerializeField] private Text   NameField;        
        [SerializeField] private Button ExecuteButton;
        [SerializeField] private Image  IconField;
        
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

            if(NameField != null) {
                NameField.text = AbilityToDisplay.name;
            }

            if(ExecuteButton != null) {
                ExecuteButton.onClick.AddListener(() => AbilityExecuter.ExecuteAbilityOnUnit(AbilityToDisplay, UnitToInvokeOn));

                ExecuteButton.interactable = AbilityExecuter.CanExecuteAbilityOnUnit(AbilityToDisplay, UnitToInvokeOn);
            }
            
            if(IconField != null) {
                IconField.sprite = AbilityToDisplay.Icon;
            }
        }

        #endregion

        #endregion

    }

}
