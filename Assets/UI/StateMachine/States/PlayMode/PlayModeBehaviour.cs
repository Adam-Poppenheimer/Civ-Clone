using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.HexMap;

namespace Assets.UI.StateMachine.States.PlayMode {

    public class PlayModeBehaviour : StateMachineBehaviour {

        #region instance fields and properties

        private UIStateMachineBrain Brain;

        private IHexGrid Grid;

        private RectTransform PlayModeContainer;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            UIStateMachineBrain brain, IHexGrid grid,
            [Inject(Id = "Play Mode Container")] RectTransform playModeContainer
        ) {
            Brain             = brain;
            Grid              = grid;
            PlayModeContainer = playModeContainer;
        }

        #region from StateMachineBehaviour

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            Grid.Build();

            PlayModeContainer.gameObject.SetActive(true);

            Brain.ClearListeners();
            Brain.EnableCameraMovement();
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            Grid.Clear();

            PlayModeContainer.gameObject.SetActive(false);
        }

        #endregion

        #endregion

    }

}
