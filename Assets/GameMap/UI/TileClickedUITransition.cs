using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.EventSystems;

using Zenject;

using BetterUI;

namespace Assets.GameMap.UI {

    public class TileClickedUITransition : UITransitionBase, ITileClickedEventHandler {

        #region instance fields and properties

        private bool WasClickedThisFrame;  

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(ITileEventBroadcaster eventBroadcaster) {
            eventBroadcaster.SubscribeTileClickedHandler(this);
        }

        #region from UITransitionBase

        public override bool IsTransitionValid() {
            return WasClickedThisFrame;
        }

        #endregion

        #region ITileClickedEventHandler

        public void OnTileClicked(IMapTile tile, PointerEventData eventData) {
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
