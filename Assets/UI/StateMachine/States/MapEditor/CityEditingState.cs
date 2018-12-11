using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;
using UniRx;

using Assets.UI.Cities;

namespace Assets.UI.StateMachine.States.MapEditor {

    public class CityEditingState : StateMachineBehaviour {

        #region instance fields and properties

        private List<CityDisplayBase> DisplaysToManage;
        private UIStateMachineBrain   Brain;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            List<CityDisplayBase> displaysToManage, UIStateMachineBrain brain
        ) {
            DisplaysToManage = displaysToManage;
            Brain            = brain;
        }

        #region from StateMachineBehaviour

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            foreach(var display in DisplaysToManage) {
                display.gameObject.SetActive(true);
                display.ObjectToDisplay = Brain.LastCityClicked;
                display.DisplayType = CityDisplayType.MapEditor;
                display.Refresh();
            }

            Brain.ClearListeners();
            Brain.DisableCameraMovement();
            Brain.ListenForTransitions(TransitionType.ReturnViaButton, TransitionType.CitySelected);
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
