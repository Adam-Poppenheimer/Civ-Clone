using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

namespace Assets.UI.StateMachine.States.TitleScreen {

    public class StartNewGameState : StateMachineBehaviour {

        #region instance fields and properties

        private UIStateMachineBrain Brain;

        private GameObject NewGameDisplay;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            UIStateMachineBrain brain,
            [Inject(Id = "New Game Display")] GameObject newGameDisplay
        ){
            Brain          = brain;
            NewGameDisplay = newGameDisplay;
        }

        #region from StateMachineBehaviour

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            NewGameDisplay.SetActive(true);

            Brain.ClearListeners();
            Brain.ListenForTransitions(TransitionType.ReturnViaButton);
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            NewGameDisplay.SetActive(false);
        }

        #endregion

        #endregion

    }

}
