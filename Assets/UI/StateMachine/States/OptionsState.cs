using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

namespace Assets.UI.StateMachine.States {

    public class OptionsState : StateMachineBehaviour {

        #region instance fields and properties

        private UIStateMachineBrain Brain;

        private GameObject OptionsDisplay;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(UIStateMachineBrain brain,
            [Inject(Id = "Options Display")] GameObject optionsDisplay
        ){
            Brain          = brain;
            OptionsDisplay = optionsDisplay;
        }

        #region from StateMachineBehaviour

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            OptionsDisplay.SetActive(true);

            Brain.ClearListeners();
            Brain.ListenForTransitions(TransitionType.ReturnViaButton);
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            OptionsDisplay.SetActive(false);
        }

        #endregion

        #endregion

    }

}
