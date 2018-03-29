using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.Technology;
using Assets.Simulation.HexMap;
using Assets.Simulation.MapManagement;
using Assets.Simulation.Civilizations;

namespace Assets.UI.StateMachine.States.MapEditor {

    public class MapEditorBehaviour : StateMachineBehaviour {

        #region instance fields and properties

        private UIStateMachineBrain  Brain;
        private IHexGrid             Grid;
        private IMapComposer         MapComposer;
        private ICivilizationFactory CivFactory;
        private ITechCanon           TechCanon;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            UIStateMachineBrain brain, IHexGrid grid, IMapComposer mapComposer,
            ICivilizationFactory civFactory, ITechCanon techCanon
        ) {
            Brain              = brain;
            Grid               = grid;
            MapComposer        = mapComposer;
            CivFactory         = civFactory;
            TechCanon          = techCanon;
        }

        #region from StateMachineBehaviour

        public override void OnStateMachineEnter(Animator animator, int stateMachinePathHash) {
            Brain.ClearListeners();
            Brain.EnableCameraMovement();

            Grid.Build();

            CivFactory.Create("Player Civilization", Color.red);

            TechCanon.IgnoreResourceVisibility = true;
        }

        public override void OnStateMachineExit(Animator animator, int stateMachinePathHash) {
            MapComposer.ClearRuntime();

            TechCanon.IgnoreResourceVisibility = false;
        }

        #endregion

        #endregion

    }

}
