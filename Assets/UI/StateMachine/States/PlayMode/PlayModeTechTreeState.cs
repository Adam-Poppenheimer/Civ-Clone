using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.Core;
using Assets.UI.Technology;

namespace Assets.UI.StateMachine.States.PlayMode {

    public class PlayModeTechTreeState : StateMachineBehaviour {

        #region instance fields and properties

        private UIStateMachineBrain Brain;
        private TechTreeDisplay     TechTreeDisplay;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            UIStateMachineBrain brain, TechTreeDisplay techTreeDisplay
        ){
            Brain           = brain;
            TechTreeDisplay = techTreeDisplay;
        }

        #region from StateMachineBehaviour

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            Brain.ClearListeners();
            Brain.ListenForTransitions(TransitionType.ReturnViaButton);

            TechTreeDisplay.gameObject.SetActive(true);
            TechTreeDisplay.Refresh();
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            TechTreeDisplay.gameObject.SetActive(false);
        }

        #endregion

        #endregion

    }

}
