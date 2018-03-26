using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

namespace Assets.UI.StateMachine.States.PlayMode {

    public class DealProposalState : StateMachineBehaviour {

        #region instance fields and properties

        private UIStateMachineBrain Brain;
        private RectTransform       ProposalDisplay;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            UIStateMachineBrain brain,
            [Inject(Id = "Proposal Display")] RectTransform proposalDisplay
        ){
            Brain           = brain;
            ProposalDisplay = proposalDisplay;
        }

        #region from StateMachineBehaviour

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            Brain.ClearListeners();

            ProposalDisplay.gameObject.SetActive(true);

            Brain.DisableCameraMovement();
            Brain.DisableCellHovering();
            Brain.ListenForTransition(TransitionType.ReturnViaButton);
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            ProposalDisplay.gameObject.SetActive(false);

            Brain.ClearListeners();
            Brain.DisableCameraMovement();
        }

        #endregion

        #endregion

    }

}
