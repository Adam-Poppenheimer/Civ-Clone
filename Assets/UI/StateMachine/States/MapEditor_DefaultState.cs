using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

namespace Assets.UI.StateMachine.States {

    public class MapEditor_DefaultState : StateMachineBehaviour {

        #region instance fields and properties

        private List<RectTransform> DefaultPanels;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            [Inject(Id = "Map Editor Default Panels")] List<RectTransform> defaultPanels
        ){
            DefaultPanels = defaultPanels;
        }

        #region from StateMachineBehaviour

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            foreach(var panel in DefaultPanels) {
                panel.gameObject.SetActive(true);
            }
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            foreach(var panel in DefaultPanels) {
                panel.gameObject.SetActive(false);
            }
            animator.ResetTrigger("Map Editing State Requested");
        }

        #endregion

        #endregion

    }

}
