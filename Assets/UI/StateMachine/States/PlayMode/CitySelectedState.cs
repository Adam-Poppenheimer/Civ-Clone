using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.EventSystems;

using Zenject;
using UniRx;

using Assets.Simulation.Cities;

using Assets.UI.Cities;
using Assets.UI.Core;

namespace Assets.UI.StateMachine.States.PlayMode {

    public class CitySelectedState : StateMachineBehaviour {

        #region instance fields and properties

        private List<CityDisplayBase> DisplaysToManage;

        private UIStateMachineBrain Brain;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(List<CityDisplayBase> displaysToManage, UIStateMachineBrain brain){
            DisplaysToManage = displaysToManage;
            Brain            = brain;
        }

        #region from StateMachineBehaviour

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            foreach(var display in DisplaysToManage) {
                display.gameObject.SetActive(true);
                display.ObjectToDisplay = Brain.LastCityClicked;
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
