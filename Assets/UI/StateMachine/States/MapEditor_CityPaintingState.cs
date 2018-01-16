using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.UI.MapEditor;

namespace Assets.UI.StateMachine.States {

    public class MapEditor_CityPaintingState : StateMachineBehaviour {

        #region instance fields and properties

        private CityPaintingPanel CityPaintingPanel;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(CityPaintingPanel cityPaintingPanel) {
            CityPaintingPanel = cityPaintingPanel;
        }

        #region from StateMachineBehaviour

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            CityPaintingPanel.gameObject.SetActive(true);
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            CityPaintingPanel.gameObject.SetActive(false);
        }

        #endregion

        #endregion

    }

}
