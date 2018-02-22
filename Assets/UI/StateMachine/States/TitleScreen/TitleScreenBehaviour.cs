﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

namespace Assets.UI.StateMachine.States.TitleScreen {

    public class TitleScreenBehaviour : StateMachineBehaviour {

        #region instance fields and properties

        private UIStateMachineBrain Brain;

        private RectTransform TitleScreenContainer;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            UIStateMachineBrain brain,
            [Inject(Id = "Title Screen Container")] RectTransform titleScreenContainer
        ) {
            Brain                = brain;
            TitleScreenContainer = titleScreenContainer;
        }

        #region from StateMachineBehaviour

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            TitleScreenContainer.gameObject.SetActive(true);

            Brain.ClearListeners();
            Brain.DisableCameraMovement();
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            TitleScreenContainer.gameObject.SetActive(false);
        }

        #endregion

        #endregion

    }

}
