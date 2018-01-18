using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.Units;
using Assets.UI.Units;

namespace Assets.UI.StateMachine.States.PlayMode.Unit {

    public class DefaultUnitState : StateMachineBehaviour {

        #region instance fields and properties

        private List<UnitDisplayBase> DisplaysToManage;

        private UIStateMachineBrain Brain;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(List<UnitDisplayBase> displaysToManage, UIStateMachineBrain brain) {
            DisplaysToManage = displaysToManage;
            Brain            = brain;
        }

        #region from UnitUIState

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            foreach(var display in DisplaysToManage) {
                display.gameObject.SetActive(true);
                display.ObjectToDisplay = Brain.LastUnitClicked;
                display.Refresh();
            }

            Brain.ClearListeners();
            Brain.ListenForTransitions(TransitionType.ReturnViaButton, TransitionType.ReturnViaClick,
                TransitionType.ToUnitSelected, TransitionType.ToCitySelected);
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            foreach(var display in DisplaysToManage) {
                display.gameObject.SetActive(false);
                display.ObjectToDisplay = null;
            }
        }

        #endregion

        #endregion

    }

}
