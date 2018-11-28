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

namespace Assets.UI.StateMachine.States.PlayMode {

    public class PlayModeBehaviour : StateMachineBehaviour {

        #region instance fields and properties

        private UIStateMachineBrain          Brain;
        private IMapComposer                 MapComposer;
        private IVisibilityResponder         VisibilityResponder;
        private IVisibilityCanon             VisibilityCanon;
        private IExplorationCanon            ExplorationCanon;
        private ICivDefeatExecutor           CivDefeatExecutor;
        private ICameraFocuser               CameraFocuser;
        private IGreatMilitaryPointGainLogic GreatMilitaryPointGainLogic;
        private IFreeBuildingsCanon          FreeBuildingsCanon;
        private IFreeUnitsResponder          FreeUnitsResponder;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            UIStateMachineBrain brain, IMapComposer mapComposer, ICameraFocuser cameraFocuser,
            IVisibilityResponder visibilityResponder, IVisibilityCanon visibilityCanon,
            IExplorationCanon explorationCanon, ICivDefeatExecutor civDefeatExecutor,
            IGreatMilitaryPointGainLogic greatMilitaryPointGainLogic,
            IFreeBuildingsCanon freeBuildingsCanon, IFreeUnitsResponder freeUnitsResponder
        ){
            Brain                       = brain;
            MapComposer                 = mapComposer;
            CameraFocuser               = cameraFocuser;
            VisibilityResponder         = visibilityResponder;
            VisibilityCanon             = visibilityCanon;
            ExplorationCanon            = explorationCanon;
            CivDefeatExecutor           = civDefeatExecutor;
            GreatMilitaryPointGainLogic = greatMilitaryPointGainLogic;
            FreeBuildingsCanon          = freeBuildingsCanon;
            FreeUnitsResponder          = freeUnitsResponder;
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

            CivDefeatExecutor.CheckForDefeat = true;

            CameraFocuser.ActivateBeginTurnFocusing();

            GreatMilitaryPointGainLogic.TrackPointGain = true;

            FreeBuildingsCanon.ApplyBuildingsToCities = true;
            FreeUnitsResponder.IsActive               = true;
        }

        public override void OnStateMachineExit(Animator animator, int stateMachinePathHash) {
            CameraFocuser.DeactivateBeginTurnFocusing();

            VisibilityResponder.UpdateVisibility = false;

            GreatMilitaryPointGainLogic.TrackPointGain = false;

            MapComposer.ClearRuntime();
        }

        #endregion

        #endregion

    }

}
