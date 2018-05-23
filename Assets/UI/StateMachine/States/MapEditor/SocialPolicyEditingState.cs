using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.UI.SocialPolicies;

namespace Assets.UI.StateMachine.States.MapEditor {

    public class SocialPolicyEditingState : StateMachineBehaviour {

        #region instance fields and properties

        private SocialPoliciesDisplay PoliciesDisplay;
        private UIStateMachineBrain   Brain;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(SocialPoliciesDisplay policiesDisplay, UIStateMachineBrain brain) {
            PoliciesDisplay = policiesDisplay;
            Brain           = brain;
        }

        #region from StateMachineBehaviour

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            PoliciesDisplay.ObjectToDisplay   = Brain.LastCivilizationSelected;
            PoliciesDisplay.IgnoreCost = true;

            PoliciesDisplay.gameObject.SetActive(true);
            PoliciesDisplay.Refresh();

            Brain.ClearListeners();
            Brain.DisableCameraMovement();
            Brain.DisableCellHovering();
            Brain.ListenForTransitions(TransitionType.ReturnViaButton);
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            PoliciesDisplay.ObjectToDisplay = null;
            PoliciesDisplay.gameObject.SetActive(false);
        }

        #endregion

        #endregion

    }

}
