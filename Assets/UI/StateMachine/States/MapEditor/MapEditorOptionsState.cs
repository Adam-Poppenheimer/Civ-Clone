using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.UI.MapEditor;

namespace Assets.UI.StateMachine.States.MapEditor {

    public class MapEditorOptionsState : StateMachineBehaviour {

        #region instance fields and properties

        private OptionsPanel        OptionsPanel;
        private UIStateMachineBrain Brain;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(OptionsPanel optionsPanel, UIStateMachineBrain brain) {
            OptionsPanel = optionsPanel;
            Brain        = brain;
        }

        #region from StateMachineBehaviour

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            OptionsPanel.gameObject.SetActive(true);

            Brain.ClearListeners();

            Brain.ListenForTransitions(TransitionType.ReturnViaButton);
            Brain.DisableCameraMovement();
            Brain.DisableCellHovering();
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            OptionsPanel.gameObject.SetActive(false);
        }

        #endregion

        #endregion

    }

}
