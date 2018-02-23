using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

namespace Assets.UI.StateMachine.States.PlayMode.EscapeMenu {

    public class EscapeMenuBehaviour : StateMachineBehaviour {

        #region instance fields and properties

        private UIStateMachineBrain Brain;

        private RectTransform EscapeMenuContainer;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            UIStateMachineBrain brain,
            [Inject(Id = "Escape Menu Container")] RectTransform escapeMenuContainer
        ) {
            Brain               = brain;
            EscapeMenuContainer = escapeMenuContainer;
        }

        #region from StateMachineBehaviour

        public override void OnStateMachineEnter(Animator animator, int stateMachinePathHash) {
            EscapeMenuContainer.gameObject.SetActive(true);

            Brain.ClearListeners();
            Brain.DisableCameraMovement();
        }

        public override void OnStateMachineExit(Animator animator, int stateMachinePathHash) {
            EscapeMenuContainer.gameObject.SetActive(false);

            Brain.EnableCameraMovement();
        }

        #endregion

        #endregion

    }

}
