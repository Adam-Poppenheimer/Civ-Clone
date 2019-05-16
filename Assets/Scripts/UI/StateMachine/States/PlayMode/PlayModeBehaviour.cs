using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.Visibility;
using Assets.Simulation.MapManagement;
using Assets.Simulation.Core;

using Assets.UI.Units;

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
        private IUnitMapIconManager             UnitMapIconManager;
        private List<IPlayModeSensitiveElement> PlayModeSensitiveElements;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            UIStateMachineBrain brain, IMapComposer mapComposer, IVisibilityResponder visibilityResponder,
            IVisibilityCanon visibilityCanon, IExplorationCanon explorationCanon, IGameCore gameCore,
            ICameraFocuser cameraFocuser, IUnitMapIconManager unitMapIconManager,
            List<IPlayModeSensitiveElement> playModeSensitiveElements
        ){
            Brain                     = brain;
            MapComposer               = mapComposer;
            VisibilityResponder       = visibilityResponder;
            VisibilityCanon           = visibilityCanon;
            ExplorationCanon          = explorationCanon;
            GameCore                  = gameCore;
            CameraFocuser             = cameraFocuser;
            UnitMapIconManager        = unitMapIconManager;
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

            UnitMapIconManager.BuildIcons();
            UnitMapIconManager.SetActive(true);

            foreach(var element in PlayModeSensitiveElements) {
                element.IsActive = true;
            }

            CameraFocuser.ReturnFocusToPlayer(GameCore.ActivePlayer);
        }

        public override void OnStateMachineExit(Animator animator, int stateMachinePathHash) {
            VisibilityResponder.UpdateVisibility = false;

            UnitMapIconManager.ClearIcons();
            UnitMapIconManager.SetActive(false);

            foreach(var element in PlayModeSensitiveElements) {
                element.IsActive = false;
            }

            MapComposer.ClearRuntime(false);
        }

        #endregion

        #endregion

    }

}
