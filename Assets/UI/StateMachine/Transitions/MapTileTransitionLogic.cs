using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.EventSystems;

using Zenject;

using Assets.Simulation.HexMap;

using Assets.UI.StateMachine.States;

namespace Assets.UI.StateMachine.Transitions {

    public class MapTileTransitionLogic {

        #region instance fields and properties

        private TileUIState TileUIState;

        private Animator StateMachineAnimator;

        #endregion

        #region constructors

        [Inject]
        public MapTileTransitionLogic(HexCellSignals tileSignals, TileUIState tileUIState,
            [Inject(Id = "UI State Machine Animator")] Animator stateMachineAnimator) {

            tileSignals.ClickedSignal.Listen(OnTileClickedFired);
            TileUIState = tileUIState;
            StateMachineAnimator = stateMachineAnimator;
        }

        #endregion

        #region instance methods

        private void OnTileClickedFired(IHexCell tileClicked, Vector3 mousePosition) {
            TileUIState.TileToDisplay = tileClicked;
            StateMachineAnimator.SetTrigger("Tile State Requested");
        }

        #endregion

    }

}
