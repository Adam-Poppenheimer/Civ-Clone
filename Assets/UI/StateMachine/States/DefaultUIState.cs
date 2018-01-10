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

        private List<RectTransform> DefaultPanels;

        private ICivilization PlayerCivilization;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            List<CivilizationDisplayBase> civilizationDisplays,
            [Inject(Id = "Default Panels")] List<RectTransform> defaultPanels,
            GameCore gameCore
        ){
            CivilizationDisplays = civilizationDisplays;
            DefaultPanels = defaultPanels;
            PlayerCivilization = gameCore.PlayerCivilization;
        }

        #region from StateMachineBehaviour

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            foreach(var display in CivilizationDisplays) {
                display.gameObject.SetActive(true);
                display.ObjectToDisplay = PlayerCivilization;
                display.Refresh();
            }

            foreach(var panel in DefaultPanels) {
                panel.gameObject.SetActive(true);
            }
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            foreach(var display in CivilizationDisplays) {
                display.gameObject.SetActive(false);
                display.ObjectToDisplay = null;
            }

            foreach(var panel in DefaultPanels) {
                panel.gameObject.SetActive(false);
            }

            animator.ResetTrigger("Default State Requested");
            animator.ResetTrigger("Map Editing Return Requested");
        }

        #endregion

        #endregion

    }

}
