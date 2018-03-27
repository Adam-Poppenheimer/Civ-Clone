using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

namespace Assets.UI.StateMachine.States.PlayMode {

    public class DeclareWarState : StateMachineBehaviour {

        #region instance fields and properties

        private UIStateMachineBrain Brain;
        private RectTransform       DeclareWarDisplay;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            UIStateMachineBrain brain,
            [Inject(Id = "Declare War Display")] RectTransform declareWarDisplay
        ) {
            Brain             = brain;
            DeclareWarDisplay = declareWarDisplay;
        }

        #region from StateMachineBehaviour

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            Brain.ClearListeners();

            DeclareWarDisplay.gameObject.SetActive(true);

            Brain.DisableCameraMovement();
            Brain.DisableCellHovering();
            Brain.ListenForTransition(TransitionType.ReturnViaButton);
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            DeclareWarDisplay.gameObject.SetActive(false);

            Brain.ClearListeners();
            Brain.DisableCameraMovement();
        }

        #endregion

        #endregion

    }

}
