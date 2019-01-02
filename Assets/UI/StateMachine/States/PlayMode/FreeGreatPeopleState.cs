using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.Core;

using Assets.UI.Units;

namespace Assets.UI.StateMachine.States.PlayMode {

    public class FreeGreatPeopleState : StateMachineBehaviour {

        #region instance fields and properties

        private UIStateMachineBrain    Brain;
        private IGameCore              GameCore;
        private FreeGreatPeopleDisplay FreeGreatPeopleDisplay;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            UIStateMachineBrain brain, IGameCore gameCore,
            FreeGreatPeopleDisplay freeGreatPeopleDisplay
        ) {
            Brain                  = brain;
            GameCore               = gameCore;
            FreeGreatPeopleDisplay = freeGreatPeopleDisplay;
        }

        #region from StateMachineBehaviour

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            FreeGreatPeopleDisplay.ObjectToDisplay = GameCore.ActivePlayer.ControlledCiv;

            FreeGreatPeopleDisplay.gameObject.SetActive(true);
            FreeGreatPeopleDisplay.Refresh();

            Brain.ClearListeners();
            Brain.DisableCameraMovement();
            Brain.DisableCellHovering();
            Brain.ListenForTransitions(TransitionType.ReturnViaButton);
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            FreeGreatPeopleDisplay.ObjectToDisplay = null;
            FreeGreatPeopleDisplay.gameObject.SetActive(false);
        }

        #endregion

        #endregion

    }

}
