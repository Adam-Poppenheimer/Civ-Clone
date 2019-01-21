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
        private IGameCore                       GameCore;
        private ICameraFocuser                  CameraFocuser;
        private List<IPlayModeSensitiveElement> PlayModeSensitiveElements;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            UIStateMachineBrain brain, IMapComposer mapComposer, IVisibilityResponder visibilityResponder,
            IVisibilityCanon visibilityCanon, IExplorationCanon explorationCanon, IGameCore gameCore,
            ICameraFocuser cameraFocuser, List<IPlayModeSensitiveElement> playModeSensitiveElements
        ){
            Brain                     = brain;
            MapComposer               = mapComposer;
            VisibilityResponder       = visibilityResponder;
            VisibilityCanon           = visibilityCanon;
            ExplorationCanon          = explorationCanon;
            GameCore                  = gameCore;
            CameraFocuser             = cameraFocuser;
            PlayModeSensitiveElements = playModeSensitiveElements;
        }

        #region from StateMachineBehaviour

        public override void OnStateMachineEnter(Animator animator, int stateMachinePathHash) {
            Brain.ClearListeners();
            Brain.EnableCameraMovement();
            
            VisibilityResponder.UpdateVisibility = true;
            VisibilityResponder.TryResetCellVisibility();

            VisibilityCanon.ResourceVisibilityMode = ResourceVisibilityMode.ActiveCiv;
            VisibilityCanon.CellVisibilityMode     = CellVisibilityMode.RevealAll;
            VisibilityCanon.RevealMode             = RevealMode.Immediate;

            ExplorationCanon.ExplorationMode = CellExplorationMode.AllCellsExplored;

            foreach(var element in PlayModeSensitiveElements) {
                element.IsActive = true;
            }

            CameraFocuser.ReturnFocusToPlayer(GameCore.ActivePlayer);
        }

        public override void OnStateMachineExit(Animator animator, int stateMachinePathHash) {
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
