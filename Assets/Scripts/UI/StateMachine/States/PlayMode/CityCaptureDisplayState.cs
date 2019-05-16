using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.UI.Cities;

namespace Assets.UI.StateMachine.States.PlayMode {

    public class CityCaptureDisplayState : StateMachineBehaviour {

        #region instance fields and properties

        private ICityCaptureDisplay CityCaptureDisplay;
        private UIStateMachineBrain Brain;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(ICityCaptureDisplay cityCaptureDisplay, UIStateMachineBrain brain) {
            CityCaptureDisplay = cityCaptureDisplay;
            Brain              = brain;
        }

        #region from StateMachineBehaviour

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            CityCaptureDisplay.gameObject.SetActive(true);

            Brain.ClearListeners();
            Brain.EnableCameraMovement();
            Brain.EnableCellHovering();
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            CityCaptureDisplay.gameObject.SetActive(false);
        }

        #endregion

        #endregion

    }

}
