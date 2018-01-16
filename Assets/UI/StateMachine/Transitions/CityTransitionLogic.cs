using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;
using UniRx;

using Assets.Simulation.Cities;
using Assets.UI.Cities;

using Assets.UI.StateMachine.States;

namespace Assets.UI.StateMachine.Transitions {

    public class CityTransitionLogic {

        #region instance fields and properties

        private PlayMode_CityState CityUIState;

        private Animator StateMachineAnimator;

        #endregion

        #region constructors

        [Inject]
        public CityTransitionLogic(CitySignals citySignals, PlayMode_CityState cityUIState,
            [Inject(Id = "UI State Machine Animator")] Animator stateMachineAnimator 
        ) {
            citySignals.CityClickedSignal.Subscribe(OnCityClickedFired);
            CityUIState = cityUIState;
            StateMachineAnimator = stateMachineAnimator;
        }

        #endregion

        #region instance methods

        private void OnCityClickedFired(ICity clickedCity) {
            CityUIState.CityToDisplay = clickedCity;

            StateMachineAnimator.SetTrigger("City State Requested");
        }

        #endregion

    }

}
