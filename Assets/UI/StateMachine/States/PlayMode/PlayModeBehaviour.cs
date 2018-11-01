using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.Visibility;
using Assets.Simulation.MapManagement;
using Assets.Simulation.Civilizations;

namespace Assets.UI.StateMachine.States.PlayMode {

    public class PlayModeBehaviour : StateMachineBehaviour {

        #region instance fields and properties

        private UIStateMachineBrain  Brain;
        private IMapComposer         MapComposer;
        private IVisibilityResponder VisibilityResponder;
        private IVisibilityCanon     VisibilityCanon;
        private IExplorationCanon    ExplorationCanon;
        private ICivDefeatExecutor   CivDefeatExecutor;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            UIStateMachineBrain brain, IMapComposer mapComposer,
            IVisibilityResponder visibilityResponder, IVisibilityCanon visibilityCanon,
            IExplorationCanon explorationCanon, ICivDefeatExecutor civDefeatExecutor
        ){
            Brain               = brain;
            MapComposer         = mapComposer;
            VisibilityResponder = visibilityResponder;
            VisibilityCanon     = visibilityCanon;
            ExplorationCanon    = explorationCanon;
            CivDefeatExecutor   = civDefeatExecutor;
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
        }

        public override void OnStateMachineExit(Animator animator, int stateMachinePathHash) {
            VisibilityResponder.UpdateVisibility = false;

            MapComposer.ClearRuntime();
        }

        #endregion

        #endregion

    }

}
