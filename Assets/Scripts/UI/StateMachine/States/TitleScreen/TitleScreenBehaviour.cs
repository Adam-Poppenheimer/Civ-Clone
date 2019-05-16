using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.Visibility;

namespace Assets.UI.StateMachine.States.TitleScreen {

    public class TitleScreenBehaviour : StateMachineBehaviour {

        #region instance fields and properties

        private UIStateMachineBrain  Brain;
        private IVisibilityCanon     VisibilityCanon;
        private IVisibilityResponder VisibilityResponder;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            UIStateMachineBrain brain, IVisibilityCanon visibilityCanon,
            IVisibilityResponder visibilityResponder
        ) {
            Brain               = brain;
            VisibilityCanon     = visibilityCanon;
            VisibilityResponder = visibilityResponder;
        }

        #region from StateMachineBehaviour

        public override void OnStateMachineEnter(Animator animator, int stateMachinePathHash) {
            Brain.ClearListeners();
            Brain.DisableCameraMovement();

            VisibilityCanon.ResourceVisibilityMode = ResourceVisibilityMode.HideAll;
            VisibilityCanon.CellVisibilityMode     = CellVisibilityMode.HideAll;
            VisibilityCanon.RevealMode             = RevealMode.Immediate;

            VisibilityResponder.UpdateVisibility = false;
            VisibilityResponder.TryResetCellVisibility();
        }

        public override void OnStateMachineExit(Animator animator, int stateMachinePathHash) {
            
        }

        #endregion

        #endregion

    }

}
