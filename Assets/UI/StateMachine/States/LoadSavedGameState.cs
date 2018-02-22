using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

namespace Assets.UI.StateMachine.States {

    public class LoadSavedGameState : StateMachineBehaviour {

        #region instance fields and properties

        private UIStateMachineBrain Brain;

        private GameObject LoadSavedGameDisplay;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            UIStateMachineBrain brain,
            [Inject(Id = "Load Saved Game Display")] GameObject loadSavedGameDisplay
        ) {
            Brain                = brain;
            LoadSavedGameDisplay = loadSavedGameDisplay;
        }

        #region from StateMachineBehaviour

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            LoadSavedGameDisplay.SetActive(true);
            Brain.ClearListeners();

            Brain.ListenForTransitions(TransitionType.ReturnViaButton);
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            LoadSavedGameDisplay.SetActive(false);
        }

        #endregion

        #endregion

    }

}
