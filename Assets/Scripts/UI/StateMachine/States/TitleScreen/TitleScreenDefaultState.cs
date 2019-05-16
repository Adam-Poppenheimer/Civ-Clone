using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

namespace Assets.UI.StateMachine.States.TitleScreen {

    public class TitleScreenDefaultState : StateMachineBehaviour {

        #region instance fields and properties

        private UIStateMachineBrain Brain;

        private GameObject StateSelectionDisplay;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(UIStateMachineBrain brain,
            [Inject(Id = "Title Screen State Selection Display")] GameObject stateSelectionDisplay
        ) {
            Brain                 = brain;
            StateSelectionDisplay = stateSelectionDisplay;
        }

        #region from StateMachineBehaviour

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            StateSelectionDisplay.gameObject.SetActive(true);

            Brain.ClearListeners();
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            StateSelectionDisplay.gameObject.SetActive(false);
        }

        #endregion

        #endregion

    }

}
