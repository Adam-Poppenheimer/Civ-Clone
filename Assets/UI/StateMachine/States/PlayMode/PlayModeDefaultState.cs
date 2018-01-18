using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;
using UniRx;

using Assets.Simulation.Core;
using Assets.Simulation.Civilizations;
using Assets.Simulation.Cities;
using Assets.Simulation.Units;
using Assets.Simulation.HexMap;

using Assets.UI.Civilizations;

namespace Assets.UI.StateMachine.States.PlayMode {

    public class PlayModeDefaultState : StateMachineBehaviour {

        #region instance fields and properties

        private List<CivilizationDisplayBase> CivilizationDisplays;

        private List<RectTransform> DefaultPanels;

        private ICivilization PlayerCivilization;

        private UIStateMachineBrain Brain;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            List<CivilizationDisplayBase> civilizationDisplays,
            [Inject(Id = "Play Mode Default Panels")] List<RectTransform> defaultPanels,
            GameCore gameCore, UIStateMachineBrain brain
        ){
            CivilizationDisplays = civilizationDisplays;
            DefaultPanels        = defaultPanels;
            PlayerCivilization   = gameCore.PlayerCivilization;
            Brain                = brain;
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

            Brain.ListenForTransitions(
                TransitionType.ToCitySelected, TransitionType.ToCellSelected,
                TransitionType.ToUnitSelected
            );
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            foreach(var display in CivilizationDisplays) {
                display.gameObject.SetActive(false);
                display.ObjectToDisplay = null;
            }

            foreach(var panel in DefaultPanels) {
                panel.gameObject.SetActive(false);
            }
        }

        #endregion

        #endregion

    }

}
