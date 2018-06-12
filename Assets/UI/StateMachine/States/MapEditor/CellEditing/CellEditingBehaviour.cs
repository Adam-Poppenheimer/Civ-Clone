using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.UI.MapEditor;

namespace Assets.UI.StateMachine.States.MapEditor.CellEditing {

    public class CellEditingBehaviour : StateMachineBehaviour {

        #region instance fields and properties

        private BrushPanel          BrushPanel;
        private UIStateMachineBrain Brain;
        private GameObject          EditTypeSelectionPanel;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            BrushPanel brushPanel, UIStateMachineBrain brain,
            [Inject(Id = "Edit Type Selection Panel")] GameObject editTypeSelectionPanel
        ){
            BrushPanel             = brushPanel;
            Brain                  = brain;
            EditTypeSelectionPanel = editTypeSelectionPanel;
        }

        #region from StateMachineBehaviour

        public override void OnStateMachineEnter(Animator animator, int stateMachinePathHash) {
            BrushPanel.gameObject .SetActive(true);
            EditTypeSelectionPanel.SetActive(true);

            Brain.ClearListeners();
            Brain.ListenForTransitions(TransitionType.ReturnViaButton);

            Brain.EnableCameraMovement();
            Brain.EnableCellHovering();
        }

        public override void OnStateMachineExit(Animator animator, int stateMachinePathHash) {
            BrushPanel.gameObject .SetActive(false);
            EditTypeSelectionPanel.SetActive(false);
        }

        #endregion

        #endregion

    }

}
