using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.UI.MapEditor;
using Assets.UI.Technology;
using Assets.Simulation.Civilizations;

namespace Assets.UI.StateMachine.States.MapEditor {

    public class TechnologyManagingState : StateMachineBehaviour {

        #region instance fields and properties

        private UIStateMachineBrain Brain;
        private TechTreeDisplay     TechTreeDisplay;
        private CivSelectionPanel   CivSelectionPanel;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            UIStateMachineBrain brain, TechTreeDisplay techTreeDisplay,
            CivSelectionPanel civSelectionPanel
        ) {
            Brain             = brain;
            TechTreeDisplay   = techTreeDisplay;
            CivSelectionPanel = civSelectionPanel;
        }

        #region from StateMachineBehaviour

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            Brain.ClearListeners();
            Brain.ListenForTransitions(TransitionType.ReturnViaButton);

            TechTreeDisplay.SelectionMode = TechSelectionMode.SetDiscoveredTechs;            
            TechTreeDisplay.gameObject.SetActive(true);

            CivSelectionPanel.SelectedCivChangedAction = OnSelectedCivChanged;
            CivSelectionPanel.gameObject.SetActive(true);
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            CivSelectionPanel.SelectedCivChangedAction = null;

            TechTreeDisplay  .gameObject.SetActive(false);
            CivSelectionPanel.gameObject.SetActive(false);
        }

        #endregion

        private void OnSelectedCivChanged(ICivilization civ) {
            TechTreeDisplay.ObjectToDisplay = civ;
            TechTreeDisplay.Refresh();
        }

        #endregion

    }

}
