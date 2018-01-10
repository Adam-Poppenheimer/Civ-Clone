using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.HexMap;

namespace Assets.UI.StateMachine.States {

    public class MapEditorState : StateMachineBehaviour {

        #region instance fields and properties

        private HexMapEditor MapEditor;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(HexMapEditor mapEditor) {
            MapEditor = mapEditor;
        }

        #region from StateMachineBehaviour

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            MapEditor.gameObject.SetActive(true);
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            MapEditor.gameObject.SetActive(false);

            animator.ResetTrigger("Map Editing State Requested");
        }

        #endregion

        #endregion

    }

}
