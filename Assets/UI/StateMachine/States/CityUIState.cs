using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;
using UniRx;

using Assets.Simulation.Cities;

using Assets.UI.Cities;

namespace Assets.UI.StateMachine.States {

    public class CityUIState : StateMachineBehaviour {

        #region instance fields and properties

        public ICity CityToDisplay { get; set; }

        private List<CityDisplayBase> DisplaysToManage;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(List<CityDisplayBase> displaysToManage){
            DisplaysToManage = displaysToManage;
        }

        #region from StateMachineBehaviour

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            foreach(var display in DisplaysToManage) {
                display.gameObject.SetActive(true);
                display.ObjectToDisplay = CityToDisplay;
                display.Refresh();
            }
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
