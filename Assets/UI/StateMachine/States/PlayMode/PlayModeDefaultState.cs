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
using Assets.UI.Cities;

namespace Assets.UI.StateMachine.States.PlayMode {

    public class PlayModeDefaultState : StateMachineBehaviour {

        #region instance fields and properties

        private List<CivilizationDisplayBase> CivilizationDisplays;

        private List<RectTransform> DefaultPanels;

        private IGameCore GameCore;

        private UIStateMachineBrain Brain;

        private CitySummaryManager CitySummaryManager;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            List<CivilizationDisplayBase> civilizationDisplays,
            [Inject(Id = "Play Mode Default Panels")] List<RectTransform> defaultPanels,
            IGameCore gameCore, UIStateMachineBrain brain, CitySummaryManager citySummaryManager
        ){
            CivilizationDisplays = civilizationDisplays;
            DefaultPanels        = defaultPanels;
            GameCore             = gameCore;
            Brain                = brain;
            CitySummaryManager   = citySummaryManager;
        }

        #region from StateMachineBehaviour

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            foreach(var display in CivilizationDisplays) {
                display.ObjectToDisplay = GameCore.ActiveCivilization;
                display.gameObject.SetActive(true);
                display.Refresh();
            }

            foreach(var panel in DefaultPanels) {
                panel.gameObject.SetActive(true);
            }

            Brain.ClearListeners();
            Brain.EnableCameraMovement();
            Brain.EnableCellHovering();

            Brain.ListenForTransitions(
                TransitionType.ToCitySelected, TransitionType.ToCellSelected,
                TransitionType.ToUnitSelected, TransitionType.ToEscapeMenu
            );

            CitySummaryManager.BuildSummaries();
        }

        public override void OnStateUpdate(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            CitySummaryManager.RefreshSummaries();
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            foreach(var display in CivilizationDisplays) {
                display.gameObject.SetActive(false);
                display.ObjectToDisplay = null;
            }

            foreach(var panel in DefaultPanels) {
                panel.gameObject.SetActive(false);
            }

            CitySummaryManager.ClearSummaries();
        }

        #endregion

        #endregion

    }

}
