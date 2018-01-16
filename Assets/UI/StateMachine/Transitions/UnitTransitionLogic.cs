using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;
using UniRx;

using Assets.UI.StateMachine.States;

using Assets.Simulation.Units;

namespace Assets.UI.StateMachine.Transitions {

    public class UnitTransitionLogic {

        #region instance fields and properties

        private PlayMode_UnitState UnitUIState;

        private Animator StateMachineAnimator;

        #endregion

        #region constructors

        [Inject]
        public UnitTransitionLogic(UnitSignals unitSignals, PlayMode_UnitState unitUIState,
            [Inject(Id = "UI State Machine Animator")] Animator stateMachineAnimator
        ) {
            unitSignals.UnitClickedSignal.Subscribe(OnUnitClickedFired);
            UnitUIState = unitUIState;
            StateMachineAnimator = stateMachineAnimator;
        }

        #endregion

        #region instance methods

        private void OnUnitClickedFired(IUnit clickedUnit) {
            UnitUIState.UnitToDisplay = clickedUnit;

            StateMachineAnimator.SetTrigger("Unit State Requested");
        }

        #endregion

    }

}
