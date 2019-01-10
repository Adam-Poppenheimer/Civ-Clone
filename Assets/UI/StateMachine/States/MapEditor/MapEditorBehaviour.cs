using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.Visibility;
using Assets.Simulation.HexMap;
using Assets.Simulation.MapManagement;
using Assets.Simulation.Civilizations;
using Assets.Simulation.Core;
using Assets.Simulation.Players;

namespace Assets.UI.StateMachine.States.MapEditor {

    public class MapEditorBehaviour : StateMachineBehaviour {

        #region instance fields and properties

        private UIStateMachineBrain             Brain;
        private IHexGrid                        Grid;
        private IMapComposer                    MapComposer;
        private ICivilizationFactory            CivFactory;
        private IPlayerFactory                  PlayerFactory;
        private IVisibilityResponder            VisibilityResponder;
        private IVisibilityCanon                VisibilityCanon;
        private IExplorationCanon               ExplorationCanon;
        private List<IPlayModeSensitiveElement> PlayModeSensitiveElements;
        private ICivilizationConfig             CivConfig;
        private IBrainPile                      BrainPile;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            UIStateMachineBrain brain, IHexGrid grid, IMapComposer mapComposer,
            ICivilizationFactory civFactory, IPlayerFactory playerFactory,
            IVisibilityResponder visibilityResponder, IVisibilityCanon visibilityCanon,
            IExplorationCanon explorationCanon, List<IPlayModeSensitiveElement> playModeSensitiveElements,
            ICivilizationConfig civConfig, IBrainPile brainPile

        ) {
            Brain                     = brain;
            Grid                      = grid;
            MapComposer               = mapComposer;
            CivFactory                = civFactory;
            PlayerFactory             = playerFactory;
            VisibilityResponder       = visibilityResponder;
            VisibilityCanon           = visibilityCanon;
            ExplorationCanon          = explorationCanon;
            PlayModeSensitiveElements = playModeSensitiveElements;
            CivConfig                 = civConfig;
            BrainPile                 = brainPile;
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

            var playerCiv    = CivFactory.Create(CivConfig.DefaultTemplate);
            var barbarianCiv = CivFactory.Create(CivConfig.BarbarianTemplate);

            PlayerFactory.CreatePlayer(playerCiv,    BrainPile.HumanBrain);
            PlayerFactory.CreatePlayer(barbarianCiv, BrainPile.BarbarianBrain);
        }

        public override void OnStateMachineExit(Animator animator, int stateMachinePathHash) {
            MapComposer.ClearRuntime(false);
        }

        #endregion

        #endregion

    }

}
