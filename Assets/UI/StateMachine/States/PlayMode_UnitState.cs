using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.Units;
using Assets.UI.Units;

namespace Assets.UI.StateMachine.States {

    public class PlayMode_UnitState : StateMachineBehaviour {

        #region instance fields and properties

        public IUnit UnitToDisplay { get; set; }

        private List<UnitDisplayBase> DisplaysToManage;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(List<UnitDisplayBase> displaysToManage) {
            DisplaysToManage = displaysToManage;
        }

        #region from UnitUIState

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            foreach(var display in DisplaysToManage) {
                display.gameObject.SetActive(true);
                display.ObjectToDisplay = UnitToDisplay;
                display.Refresh();
            }
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            foreach(var display in DisplaysToManage) {
                display.gameObject.SetActive(false);
                display.ObjectToDisplay = null;
            }

            animator.ResetTrigger("Unit State Requested");
        }

        #endregion

        #endregion

    }

}
