using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.HexMap;
using Assets.Simulation.MapManagement;
using Assets.Simulation.Civilizations;

namespace Assets.UI.StateMachine.States.MapEditor {

    public class MapEditorBehaviour : StateMachineBehaviour {

        #region instance fields and properties

        private UIStateMachineBrain Brain;

        private IHexGrid Grid;

        private IMapComposer MapComposer;

        private ICivilizationFactory CivFactory;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            UIStateMachineBrain brain, IHexGrid grid, IMapComposer mapComposer,
            ICivilizationFactory civFactory
        ) {
            Brain              = brain;
            Grid               = grid;
            MapComposer        = mapComposer;
            CivFactory         = civFactory;
        }

        #region from StateMachineBehaviour

        public override void OnStateMachineEnter(Animator animator, int stateMachinePathHash) {
            Brain.ClearListeners();
            Brain.EnableCameraMovement();

            Grid.Build();

            CivFactory.Create("Player Civilization", Color.red);
        }

        public override void OnStateMachineExit(Animator animator, int stateMachinePathHash) {
            MapComposer.ClearRuntime();
        }

        #endregion

        #endregion

    }

}
