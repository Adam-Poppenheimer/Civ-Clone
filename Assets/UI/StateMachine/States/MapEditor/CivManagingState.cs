﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.UI.MapEditor;

namespace Assets.UI.StateMachine.States.MapEditor {

    public class CivManagingState : StateMachineBehaviour {

        #region instance fields and properties

        private PlayerManagementPanel CivPanel;

        private UIStateMachineBrain Brain;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(PlayerManagementPanel civPanel, UIStateMachineBrain brain) {
            CivPanel = civPanel;
            Brain    = brain;
        }

        #region from StateMachineBehaviour

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            CivPanel.gameObject.SetActive(true);

            Brain.ClearListeners();

            Brain.ListenForTransitions(TransitionType.ReturnViaButton, TransitionType.CivSelected);
            Brain.EnableCameraMovement();
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            CivPanel.gameObject.SetActive(false);
        }

        #endregion

        #endregion

    }

}
