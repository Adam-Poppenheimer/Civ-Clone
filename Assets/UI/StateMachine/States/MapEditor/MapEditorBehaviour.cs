using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.Visibility;
using Assets.Simulation.Units;
using Assets.Simulation.HexMap;
using Assets.Simulation.MapManagement;
using Assets.Simulation.Civilizations;
using Assets.Simulation.Cities.Buildings;

namespace Assets.UI.StateMachine.States.MapEditor {

    public class MapEditorBehaviour : StateMachineBehaviour {

        #region instance fields and properties

        private UIStateMachineBrain                       Brain;
        private IHexGrid                                  Grid;
        private IMapComposer                              MapComposer;
        private ICivilizationFactory                      CivFactory;
        private IVisibilityResponder                      VisibilityResponder;
        private IVisibilityCanon                          VisibilityCanon;
        private IExplorationCanon                         ExplorationCanon;
        private ICivDefeatExecutor                        CivDefeatExecutor;
        private IFreeBuildingsCanon                       FreeBuildingsCanon;
        private ReadOnlyCollection<ICivilizationTemplate> CivTemplates;
        private IFreeUnitsResponder                       FreeUnitsResponder;
        private IFreeBuildingsResponder                   FreeBuildingsResponder;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            UIStateMachineBrain brain, IHexGrid grid, IMapComposer mapComposer,
            ICivilizationFactory civFactory, IVisibilityResponder visibilityResponder,
            IVisibilityCanon visibilityCanon, IExplorationCanon explorationCanon,
            ICivDefeatExecutor civDefeatExecutor, IFreeBuildingsCanon freeBuildingsCanon,
            ReadOnlyCollection<ICivilizationTemplate> civTemplates,
            IFreeUnitsResponder freeUnitsResponder, IFreeBuildingsResponder freeBuildingsResponder
        ) {
            Brain                  = brain;
            Grid                   = grid;
            MapComposer            = mapComposer;
            CivFactory             = civFactory;
            VisibilityResponder    = visibilityResponder;
            VisibilityCanon        = visibilityCanon;
            ExplorationCanon       = explorationCanon;
            CivDefeatExecutor      = civDefeatExecutor;
            FreeBuildingsCanon     = freeBuildingsCanon;
            CivTemplates           = civTemplates;
            FreeUnitsResponder     = freeUnitsResponder;
            FreeBuildingsResponder = freeBuildingsResponder;
        }

        #region from StateMachineBehaviour

        public override void OnStateMachineEnter(Animator animator, int stateMachinePathHash) {
            Brain.ClearListeners();
            Brain.EnableCameraMovement();
            
            VisibilityResponder.UpdateVisibility = true;
            VisibilityResponder.TryResetCellVisibility();

            VisibilityCanon.ResourceVisibilityMode = ResourceVisibilityMode.RevealAll;
            VisibilityCanon.CellVisibilityMode     = CellVisibilityMode.RevealAll;
            VisibilityCanon.RevealMode             = RevealMode.Immediate;

            ExplorationCanon.ExplorationMode = CellExplorationMode.AllCellsExplored;

            CivDefeatExecutor.CheckForDefeat = false;
            FreeBuildingsCanon.ApplyBuildingsToCities = false;
            FreeUnitsResponder.IsActive = false;
            FreeBuildingsResponder.IsActive = false;

            Grid.Build(4, 3);

            CivFactory.Create(CivTemplates[0]);
        }

        public override void OnStateMachineExit(Animator animator, int stateMachinePathHash) {
            MapComposer.ClearRuntime();
        }

        #endregion

        #endregion

    }

}
