using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.EventSystems;

using Zenject;

using BetterUI;

using Assets.Simulation.Cities;

namespace Assets.UI.Cities {

    public class CityClickedUITransition : UITransitionBase, ICityClickedEventHandler {

        #region instance fields and properties

        private bool WasClickedThisFrame;

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(ICityEventBroadcaster eventBroadcaster) {
            eventBroadcaster.SubscribeCityClickedHandler(this);
        }

        #region from UITransitionBase

        public override bool IsTransitionValid() {
            return WasClickedThisFrame;
        }        

        #endregion

        #region from ICityClickedEventHandler

        public void OnCityClicked(ICity city, PointerEventData eventData) {
            WasClickedThisFrame = true;
            StartCoroutine(ResetStateCoroutine());
        }

        #endregion

        private IEnumerator ResetStateCoroutine() {
            yield return new WaitForEndOfFrame();
            WasClickedThisFrame = false;
        }

        #endregion

    }

}
