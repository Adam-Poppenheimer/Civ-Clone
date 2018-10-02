using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.Visibility;
using Assets.Simulation.MapManagement;
using Assets.Simulation.Technology;

namespace Assets.UI.StateMachine.States.PlayMode {

    public class PlayModeBehaviour : StateMachineBehaviour {

        #region instance fields and properties

        private UIStateMachineBrain  Brain;
        private IMapComposer         MapComposer;
        private IVisibilityResponder VisibilityResponder;
        private IVisibilityCanon     VisibilityCanon;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            UIStateMachineBrain brain, IMapComposer mapComposer,
            IVisibilityResponder visibilityResponder, IVisibilityCanon visibilityCanon
        ){
            Brain               = brain;
            MapComposer         = mapComposer;
            VisibilityResponder = visibilityResponder;
            VisibilityCanon     = visibilityCanon;
        }

        #region from StateMachineBehaviour

        public override void OnStateMachineEnter(Animator animator, int stateMachinePathHash) {
            Brain.ClearListeners();
            Brain.EnableCameraMovement();
            
            VisibilityResponder.UpdateVisibility = true;
            VisibilityResponder.TryResetCellVisibility();

            VisibilityCanon.ResourceVisibilityMode = ResourceVisibilityMode.ActiveCiv;
            VisibilityCanon.CellVisibilityMode     = CellVisibilityMode.ActiveCiv;
            VisibilityCanon.RevealMode             = RevealMode.Fade;
        }

        public override void OnStateMachineExit(Animator animator, int stateMachinePathHash) {
            VisibilityResponder.UpdateVisibility = false;

            MapComposer.ClearRuntime();
        }

        #endregion

        #endregion

    }

}
