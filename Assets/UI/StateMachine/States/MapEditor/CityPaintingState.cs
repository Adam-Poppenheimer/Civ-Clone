using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.UI.MapEditor;

namespace Assets.UI.StateMachine.States.MapEditor {

    public class CityPaintingState : StateMachineBehaviour {

        #region instance fields and properties

        private CityPaintingPanel CityPaintingPanel;

        private UIStateMachineBrain Brain;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(CityPaintingPanel cityPaintingPanel, UIStateMachineBrain brain) {
            CityPaintingPanel = cityPaintingPanel;
            Brain             = brain;
        }

        #region from StateMachineBehaviour

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            CityPaintingPanel.gameObject.SetActive(true);

            Brain.ClearListeners();
            Brain.ListenForTransitions(TransitionType.ReturnViaButton);
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            CityPaintingPanel.gameObject.SetActive(false);
        }

        #endregion

        #endregion

    }

}
