using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.HexMap;
using Assets.Simulation.MapManagement;

namespace Assets.UI.StateMachine.States.MapEditor {

    public class MapEditorBehaviour : StateMachineBehaviour {

        #region instance fields and properties

        private UIStateMachineBrain Brain;

        private IHexGrid Grid;

        private IMapComposer MapComposer;

        private RectTransform MapEditorContainer;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(
            UIStateMachineBrain brain, IHexGrid grid, IMapComposer mapComposer,
            [Inject(Id = "Map Editor Container")] RectTransform mapEditorContainer
        ) {
            Brain              = brain;
            Grid               = grid;
            MapComposer        = mapComposer;
            MapEditorContainer = mapEditorContainer;
        }

        #region from StateMachineBehaviour

        public override void OnStateMachineEnter(Animator animator, int stateMachinePathHash) {
            MapEditorContainer.gameObject.SetActive(true);

            Brain.ClearListeners();
            Brain.EnableCameraMovement();

            Grid.Build();
        }

        public override void OnStateMachineExit(Animator animator, int stateMachinePathHash) {
            MapEditorContainer.gameObject.SetActive(false);

            MapComposer.ClearRuntime();
        }

        #endregion

        #endregion

    }

}
