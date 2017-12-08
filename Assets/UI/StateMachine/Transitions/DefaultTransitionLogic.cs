using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.EventSystems;

using Zenject;
using UniRx;

using Assets.Simulation.Cities;
using Assets.Simulation.GameMap;

namespace Assets.UI.StateMachine.Transitions {

    public class DefaultTransitionLogic {

        #region instance fields and properties

        private Animator StateMachineAnimator;

        #endregion

        #region constructors

        [Inject]
        public DefaultTransitionLogic(
            [Inject(Id = "Clicked Anywhere Signal"  )] IObservable<PointerEventData> clickedAnywhereSignal,
            [Inject(Id = "Cancel Pressed Signal"    )] IObservable<Unit> cancelPressedSignal,
            [Inject(Id = "UI State Machine Animator")] Animator stateMachineAnimator
        ) {
            clickedAnywhereSignal.Subscribe(OnClickedAnywhereFired);
            cancelPressedSignal.Subscribe(OnCancelPressedFired);
            StateMachineAnimator = stateMachineAnimator;
        }

        #endregion

        #region instance methods

        private void OnClickedAnywhereFired(PointerEventData eventData) {
            var raycastResults = new List<RaycastResult>();

            EventSystem.current.RaycastAll(eventData, raycastResults);

            foreach(var raycastResult in raycastResults) {
                var clickedObject = raycastResult.gameObject;

                if(clickedObject.GetComponentInParent<ICity>() != null) {
                    return;
                }else if(clickedObject.GetComponentInParent<IMapTile>() != null) {
                    return;
                }else if(clickedObject.GetComponentInParent<IDisplayBase>() != null) {
                    return;
                }
            }
            
            StateMachineAnimator.SetTrigger("Default State Requested");
        }

        private void OnCancelPressedFired(Unit unit) {
            StateMachineAnimator.SetTrigger("Default State Requested");
        }

        #endregion

    }

}
