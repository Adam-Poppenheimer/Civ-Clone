using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.UI.MapEditor;

namespace Assets.UI.StateMachine.States.MapEditor {

    public class MapGenerationState : StateMachineBehaviour {

        #region instance fields and properties

        private MapGenerationPanel  MapGenerationPanel;
        private UIStateMachineBrain Brain;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            MapGenerationPanel mapGenerationPanel, UIStateMachineBrain brain
        ) {
            MapGenerationPanel = mapGenerationPanel;
            Brain              = brain;
        }

        #region from StateMachineBehaviour

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            MapGenerationPanel.gameObject.SetActive(true);

            Brain.ClearListeners();

            Brain.DisableCameraMovement();
            Brain.DisableCellHovering();

            Brain.ListenForTransition(TransitionType.ReturnViaButton);
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            MapGenerationPanel.gameObject.SetActive(false);
        }

        #endregion

        #endregion

    }

}
