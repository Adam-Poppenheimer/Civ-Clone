using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

namespace Assets.UI.StateMachine.States.PlayMode {

    public class DealsReceivedState : StateMachineBehaviour {

        #region instance fields and properties

        private UIStateMachineBrain Brain;
        private RectTransform       DealsReceivedDisplay;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            UIStateMachineBrain brain,
            [Inject(Id = "Deals Received Display")] RectTransform dealsReceivedDisplay
        ){
            Brain                = brain;
            DealsReceivedDisplay = dealsReceivedDisplay;
        }

        #region from StateMachineBehaviour

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            Brain.ClearListeners();

            DealsReceivedDisplay.gameObject.SetActive(true);

            Brain.DisableCameraMovement();
            Brain.DisableCellHovering();
            Brain.ListenForTransition(TransitionType.ReturnViaButton);
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            DealsReceivedDisplay.gameObject.SetActive(false);

            Brain.ClearListeners();
            Brain.DisableCameraMovement();
        }

        #endregion

        #endregion

    }

}
