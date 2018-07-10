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
        private IVisibilityResponder VisibilityResponder;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            UIStateMachineBrain brain, IHexGrid grid, IMapComposer mapComposer,
            ICivilizationFactory civFactory, ITechCanon techCanon,
            IVisibilityResponder visibilityResponder
        ) {
            Brain               = brain;
            Grid                = grid;
            MapComposer         = mapComposer;
            CivFactory          = civFactory;
            TechCanon           = techCanon;
            VisibilityResponder = visibilityResponder;
        }

        #region from StateMachineBehaviour

        public override void OnStateMachineEnter(Animator animator, int stateMachinePathHash) {
            Brain.ClearListeners();
            Brain.EnableCameraMovement();

            Grid.Build(4, 3);

            CivFactory.Create("Player Civilization", Color.red);

            TechCanon.IgnoreResourceVisibility   = true;
            VisibilityResponder.UpdateVisibility = true;
        }

        public override void OnStateMachineExit(Animator animator, int stateMachinePathHash) {
            TechCanon.IgnoreResourceVisibility   = false;
            VisibilityResponder.UpdateVisibility = false;

            MapComposer.ClearRuntime();
        }

        #endregion

        #endregion

    }

}
