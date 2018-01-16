using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.UI.MapEditor;

namespace Assets.UI.StateMachine.States {

    public class MapEditor_UnitPaintingState : StateMachineBehaviour {

        #region instance fields and properties

        private UnitPaintingPanel UnitPaintingPanel;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(UnitPaintingPanel unitPaintingPanel) {
            UnitPaintingPanel = unitPaintingPanel;
        }

        #region StateMachineBehaviour

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            UnitPaintingPanel.gameObject.SetActive(true);
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            UnitPaintingPanel.gameObject.SetActive(false);
        }

        #endregion

        #endregion

    }

}
