using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.UI.SpecialtyResources;

namespace Assets.UI.StateMachine.States.PlayMode {

    public class SpecialtyResourceSummaryState : StateMachineBehaviour {

        #region instance fields and properties

        private SpecialtyResourceSummaryDisplay Display;

        private UIStateMachineBrain Brain;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(SpecialtyResourceSummaryDisplay display, UIStateMachineBrain brain) {
            Display = display;
            Brain   = brain;
        }

        #region from StateMachineBehaviour

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            Display.gameObject.SetActive(true);

            Brain.ClearListeners();
            Brain.ListenForTransitions(TransitionType.ReturnViaButton);
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            Display.gameObject.SetActive(false);

            Brain.ClearListeners();
        }

        #endregion

        #endregion

    }

}
