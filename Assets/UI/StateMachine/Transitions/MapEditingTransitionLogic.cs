using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;
using UniRx;

namespace Assets.UI.StateMachine.Transitions {

    public class MapEditingTransitionLogic {

        #region instance fields and properties

        private Animator StateMachineAnimator;

        #endregion

        #region constructors

        [Inject]
        public MapEditingTransitionLogic(
            [Inject(Id = "Cancel Pressed Signal")] IObservable<Unit> cancelPressedSignal,
            [Inject(Id = "UI State Machine Animator")] Animator stateMachineAnimator
        ) {
            cancelPressedSignal.Subscribe(OnCancelPressedFired);
            StateMachineAnimator = stateMachineAnimator;
        }

        #endregion

        #region instance methods

        private void OnCancelPressedFired(Unit unit) {
            StateMachineAnimator.SetTrigger("Map Editing Return Requested");
        }

        #endregion

    }

}
