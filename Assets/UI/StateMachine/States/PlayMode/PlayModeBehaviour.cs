using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.HexMap;
using Assets.Simulation.MapManagement;
using Assets.Simulation.Civilizations;

namespace Assets.UI.StateMachine.States.PlayMode {

    public class PlayModeBehaviour : StateMachineBehaviour {

        #region instance fields and properties

        private UIStateMachineBrain  Brain;
        private IMapComposer         MapComposer;
        private IVisibilityResponder VisibilityResponder;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            UIStateMachineBrain brain, IMapComposer mapComposer,
            IVisibilityResponder visibilityResponder
        ){
            Brain               = brain;
            MapComposer         = mapComposer;
            VisibilityResponder = visibilityResponder;
        }

        #region from StateMachineBehaviour

        public override void OnStateMachineEnter(Animator animator, int stateMachinePathHash) {
            Brain.ClearListeners();
            Brain.EnableCameraMovement();

            VisibilityResponder.UpdateVisibility = true;
            VisibilityResponder.TryResetAllVisibility();
        }

        public override void OnStateMachineExit(Animator animator, int stateMachinePathHash) {
            VisibilityResponder.UpdateVisibility = false;

            MapComposer.ClearRuntime();
        }

        #endregion

        #endregion

    }

}
