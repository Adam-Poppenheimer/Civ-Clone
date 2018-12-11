using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;
using UniRx;

using Assets.Simulation.Units;

namespace Assets.UI.StateMachine.States.MapEditor {

    public class MapEditorDefaultState : StateMachineBehaviour {

        #region instance fields and properties

        private List<RectTransform> DefaultPanels;
        private UIStateMachineBrain Brain;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            [Inject(Id = "Map Editor Default Panels")] List<RectTransform> defaultPanels,
            UIStateMachineBrain brain
        ){
            DefaultPanels = defaultPanels;
            Brain         = brain;
        }

        #region from StateMachineBehaviour

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            foreach(var panel in DefaultPanels) {
                panel.gameObject.SetActive(true);
            }

            Brain.ClearListeners();

            Brain.EnableCameraMovement();
            Brain.EnableCellHovering();
            Brain.ListenForTransitions(TransitionType.UnitSelected, TransitionType.CitySelected);
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            foreach(var panel in DefaultPanels) {
                panel.gameObject.SetActive(false);
            }
        }

        #endregion

        #endregion

    }

}
