using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.UI.TitleScreen;

namespace Assets.UI.StateMachine.States.TitleScreen {

    public class StartNewGameState : StateMachineBehaviour {

        #region instance fields and properties

        private UIStateMachineBrain Brain;
        private NewGameDisplay      NewGameDisplay;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            UIStateMachineBrain brain, NewGameDisplay newGameDisplay
        ){
            Brain          = brain;
            NewGameDisplay = newGameDisplay;
        }

        #region from StateMachineBehaviour

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            NewGameDisplay.gameObject.SetActive(true);

            Brain.ClearListeners();
            Brain.ListenForTransitions(TransitionType.ReturnViaButton);
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            NewGameDisplay.gameObject.SetActive(false);
        }

        #endregion

        #endregion

    }

}
