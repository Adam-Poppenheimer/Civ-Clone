using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;

using Zenject;

using Assets.UI.Common;
using Assets.Simulation.Visibility;

namespace Assets.UI.StateMachine.States {

    public class LoadingScreenState : StateMachineBehaviour {

        #region instance fields and properties

        private UIStateMachineBrain  Brain;
        private LoadingScreen        LoadingScreen;
        private IVisibilityResponder VisibilityResponder;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            UIStateMachineBrain brain, LoadingScreen loadingScreen, IVisibilityResponder visibilityResponder
        ) {
            Brain               = brain;
            LoadingScreen       = loadingScreen;
            VisibilityResponder = visibilityResponder;
        }

        #region from StateMachineBehaviour

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            VisibilityResponder.UpdateVisibility = false;

            LoadingScreen.gameObject.SetActive(true);

            Brain.ClearListeners();
            Brain.DisableCameraMovement();
            Brain.DisableCellHovering();
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            LoadingScreen.gameObject.SetActive(false);

            VisibilityResponder.UpdateVisibility = true;
        }

        #endregion

        #endregion

    }

}
