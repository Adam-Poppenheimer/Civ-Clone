using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

namespace Assets.UI.StateMachine.States.PlayMode.EscapeMenu {

    public class SaveGameState : StateMachineBehaviour {

        #region instance fields and properties

        private UIStateMachineBrain Brain;

        private GameObject SaveGameDisplay;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(UIStateMachineBrain brain,
            [Inject(Id = "Save Game Display")] GameObject saveGameDisplay
        ) {
            Brain           = brain;
            SaveGameDisplay = saveGameDisplay;
        }

        #region from StateMachineBehaviour

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            SaveGameDisplay.SetActive(true);

            Brain.ClearListeners();
            Brain.ListenForTransitions(TransitionType.ReturnViaButton);
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            SaveGameDisplay.SetActive(false);
        }

        #endregion

        #endregion

    }

}
