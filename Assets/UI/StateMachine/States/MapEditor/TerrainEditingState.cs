using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.UI.MapEditor;

namespace Assets.UI.StateMachine.States.MapEditor {

    public class TerrainEditingState : StateMachineBehaviour {

        #region instance fields and properties

        private TerrainEditingPanel TerrainEditor;

        private UIStateMachineBrain Brain;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(TerrainEditingPanel terrainEditor, UIStateMachineBrain brain) {
            TerrainEditor = terrainEditor;
            Brain         = brain;
        }

        #region from StateMachineBehaviour

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            TerrainEditor.gameObject.SetActive(true);

            Brain.ClearListeners();
            Brain.ListenForTransitions(TransitionType.ReturnViaButton);

            Brain.EnableCameraMovement();
            Brain.EnableCellHovering();
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            TerrainEditor.gameObject.SetActive(false);
        }

        #endregion

        #endregion

    }

}
