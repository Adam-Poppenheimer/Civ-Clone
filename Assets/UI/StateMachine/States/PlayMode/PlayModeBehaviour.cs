using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.Visibility;
using Assets.Simulation.MapManagement;
using Assets.Simulation.Civilizations;
using Assets.Simulation.Units;
using Assets.Simulation.Cities.Buildings;
using Assets.Simulation.Core;

namespace Assets.UI.StateMachine.States.PlayMode {

    public class PlayModeBehaviour : StateMachineBehaviour {

        #region instance fields and properties

        private UIStateMachineBrain             Brain;
        private IMapComposer                    MapComposer;
        private IVisibilityResponder            VisibilityResponder;
        private IVisibilityCanon                VisibilityCanon;
        private IExplorationCanon               ExplorationCanon;
        private ICameraFocuser                  CameraFocuser;
        private List<IPlayModeSensitiveElement> PlayModeSensitiveElements;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            UIStateMachineBrain brain, IMapComposer mapComposer, ICameraFocuser cameraFocuser,
            IVisibilityResponder visibilityResponder, IVisibilityCanon visibilityCanon,
            IExplorationCanon explorationCanon, List<IPlayModeSensitiveElement> playModeSensitiveElements
        ){
            Brain                       = brain;
            MapComposer                 = mapComposer;
            CameraFocuser               = cameraFocuser;
            VisibilityResponder         = visibilityResponder;
            VisibilityCanon             = visibilityCanon;
            ExplorationCanon            = explorationCanon;
            PlayModeSensitiveElements   = playModeSensitiveElements;
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

            ExplorationCanon.ExplorationMode = CellExplorationMode.ActiveCiv;

            CameraFocuser.ActivateBeginTurnFocusing();

            foreach(var element in PlayModeSensitiveElements) {
                element.IsActive = true;
            }
        }

        public override void OnStateMachineExit(Animator animator, int stateMachinePathHash) {
            CameraFocuser.DeactivateBeginTurnFocusing();

            VisibilityResponder.UpdateVisibility = false;

            foreach(var element in PlayModeSensitiveElements) {
                element.IsActive = false;
            }

            MapComposer.ClearRuntime(false);
        }

        #endregion

        #endregion

    }

}
