using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

namespace Assets.UI.StateMachine.States.PlayMode.EscapeMenu {

    public class EscapeMenuDefaultState : StateMachineBehaviour {

        #region instance fields and properties

        private UIStateMachineBrain Brain;

        private GameObject EscapeMenuOptionsDisplay;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            UIStateMachineBrain brain,
            [Inject(Id = "Escape Menu Options Display")] GameObject escapeMenuOptionsDisplay
        ) {
            Brain                    = brain;
            EscapeMenuOptionsDisplay = escapeMenuOptionsDisplay;
        }

        #region from StateMachineBehaviour

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            EscapeMenuOptionsDisplay.SetActive(true);

            Brain.ClearListeners();
            Brain.ListenForTransitions(TransitionType.ReturnViaButton);
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            EscapeMenuOptionsDisplay.SetActive(false);
        }

        #endregion

        #endregion

    }

}
