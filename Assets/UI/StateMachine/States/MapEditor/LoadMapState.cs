using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.UI.MapManagement;

namespace Assets.UI.StateMachine.States.MapEditor {

    public class LoadMapState : StateMachineBehaviour {

        #region instance fields and properties

        private string TitleLabel        = "Load Map";
        private string AcceptButtonLabel = "Load";




        private UIStateMachineBrain Brain;
        private LoadGameDisplay     LoadGameDisplay;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            UIStateMachineBrain brain, LoadGameDisplay loadGameDisplay
        ) {
            Brain            = brain;
            LoadGameDisplay = loadGameDisplay;
        }

        #region from StateMachineBehaviour

        public override void OnStateEnter(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            LoadGameDisplay.LoadMode          = MapFileType.Map;
            LoadGameDisplay.TitleLabel        = TitleLabel;
            LoadGameDisplay.AcceptButtonLabel = AcceptButtonLabel;
            LoadGameDisplay.LoadAction        = () => animator.SetTrigger("Return Requested");

            LoadGameDisplay.gameObject.SetActive(true);

            Brain.ClearListeners();

            Brain.ListenForTransitions(TransitionType.ReturnViaButton);
        }

        public override void OnStateExit(Animator animator, AnimatorStateInfo stateInfo, int layerIndex) {
            LoadGameDisplay.gameObject.SetActive(false);
        }

        #endregion

        #endregion

    }

}
