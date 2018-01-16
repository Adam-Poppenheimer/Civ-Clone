using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.UI.MapEditor;

namespace Assets.UI.StateMachine.States {

    public class MapEditor_TerrainEditingState : StateMachineBehaviour {

        #region instance fields and properties

        private TerrainEditingPanel TerrainEditor;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(TerrainEditingPanel terrainEditor) {
            TerrainEditor = terrainEditor;
        }

        #region from StateMachineBehaviour

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            TerrainEditor.gameObject.SetActive(true);
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            TerrainEditor.gameObject.SetActive(false);
        }

        #endregion

        #endregion

    }

}
