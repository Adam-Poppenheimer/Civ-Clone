using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.UI.MapEditor;

namespace Assets.UI.StateMachine.States.MapEditor.CellEditing {

    public class RiverPaintingState : StateMachineBehaviour {

        #region instance fields and properties

        private UIStateMachineBrain Brain;
        private RiverPaintingPanel  RiverPanel;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            UIStateMachineBrain brain, RiverPaintingPanel riverPanel
        ){
            Brain      = brain;
            RiverPanel = riverPanel;
        }

        #region from StateMachineBehaviour

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            Brain.ClearListeners();

            Brain.EnableCameraMovement();
            Brain.DisableCellHovering();

            Brain.ListenForTransitions(TransitionType.ReturnViaButton);

            RiverPanel.gameObject.SetActive(true);
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            Brain.ClearListeners();

            RiverPanel.gameObject.SetActive(false);
        }

        #endregion

        #endregion

    }

}
