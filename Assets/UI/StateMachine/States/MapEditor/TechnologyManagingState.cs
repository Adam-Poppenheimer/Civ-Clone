using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.UI.MapEditor;

namespace Assets.UI.StateMachine.States.MapEditor {

    public class TechnologyManagingState : StateMachineBehaviour {

        #region instance fields and properties

        private UIStateMachineBrain Brain;

        private TechManagementPanel TechnologyManagementPanel;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(UIStateMachineBrain brain, TechManagementPanel technologyManagementPanel) {
            Brain                     = brain;
            TechnologyManagementPanel = technologyManagementPanel;
        }

        #region from StateMachineBehaviour

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            TechnologyManagementPanel.gameObject.SetActive(true);

            Brain.ClearListeners();
            Brain.ListenForTransitions(TransitionType.ReturnViaButton);
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            TechnologyManagementPanel.gameObject.SetActive(false);
        }

        #endregion

        #endregion

    }

}
