using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;
using UniRx;

using Assets.Simulation.Core;
using Assets.Simulation.Civilizations;
using Assets.UI.Civilizations;

namespace Assets.UI.StateMachine.States {

    public class DefaultUIState : StateMachineBehaviour {

        #region instance fields and properties

        private List<CivilizationDisplayBase> CivilizationDisplays;

        private ICivilization PlayerCivilization;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(List<CivilizationDisplayBase> civilizationDisplays, GameCore gameCore) {
            CivilizationDisplays = civilizationDisplays;
            PlayerCivilization = gameCore.PlayerCivilization;
        }

        #region from StateMachineBehaviour

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            foreach(var display in CivilizationDisplays) {
                display.gameObject.SetActive(true);
                display.ObjectToDisplay = PlayerCivilization;
                display.Refresh();
            }
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            foreach(var display in CivilizationDisplays) {
                display.gameObject.SetActive(false);
                display.ObjectToDisplay = null;
            }
        }

        #endregion

        #endregion

    }

}
