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
using Assets.Simulation.Core;

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
        private ReadOnlyCollection<ICivilizationTemplate> CivTemplates;
        private List<IPlayModeSensitiveElement>           PlayModeSensitiveElements;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            UIStateMachineBrain brain, IHexGrid grid, IMapComposer mapComposer,
            ICivilizationFactory civFactory, IVisibilityResponder visibilityResponder,
            IVisibilityCanon visibilityCanon, IExplorationCanon explorationCanon,
            ReadOnlyCollection<ICivilizationTemplate> civTemplates,
            List<IPlayModeSensitiveElement> playModeSensitiveElements

        ) {
            Brain                     = brain;
            Grid                      = grid;
            MapComposer               = mapComposer;
            CivFactory                = civFactory;
            VisibilityResponder       = visibilityResponder;
            VisibilityCanon           = visibilityCanon;
            ExplorationCanon          = explorationCanon;
            CivTemplates              = civTemplates;
            PlayModeSensitiveElements = playModeSensitiveElements;
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

            foreach(var element in PlayModeSensitiveElements) {
                element.IsActive = false;
            }

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
