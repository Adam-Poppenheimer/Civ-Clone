using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.Core;

using Assets.UI.SocialPolicies;

namespace Assets.UI.StateMachine.States.PlayMode {

    public class SocialPoliciesState : StateMachineBehaviour {

        #region instance fields and properties

        private SocialPoliciesDisplay SocialPoliciesDisplay;
        private UIStateMachineBrain   Brain;
        private IGameCore             GameCore;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            SocialPoliciesDisplay socialPoliciesDisplay, UIStateMachineBrain brain,
            IGameCore gameCore
        ){
            SocialPoliciesDisplay = socialPoliciesDisplay;
            Brain                 = brain;
            GameCore              = gameCore;
        }

        #region from StateMachineBehaviour

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            SocialPoliciesDisplay.ObjectToDisplay = GameCore.ActiveCiv;
            SocialPoliciesDisplay.IgnoreCost = false;

            SocialPoliciesDisplay.gameObject.SetActive(true);
            SocialPoliciesDisplay.Refresh();

            Brain.ClearListeners();
            Brain.DisableCameraMovement();
            Brain.DisableCellHovering();
            Brain.ListenForTransitions(TransitionType.ReturnViaButton);
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            SocialPoliciesDisplay.ObjectToDisplay = null;
            SocialPoliciesDisplay.gameObject.SetActive(false);
        }

        #endregion

        #endregion

    }

}
