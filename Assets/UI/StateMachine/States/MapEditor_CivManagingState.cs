using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.UI.MapEditor;

namespace Assets.UI.StateMachine.States {

    public class MapEditor_CivManagingState : StateMachineBehaviour {

        #region instance fields and properties

        CivManagementPanel CivPanel;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(CivManagementPanel civPanel) {
            CivPanel = civPanel;
        }

        #region from StateMachineBehaviour

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            CivPanel.gameObject.SetActive(true);
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            CivPanel.gameObject.SetActive(false);
        }

        #endregion

        #endregion

    }

}
