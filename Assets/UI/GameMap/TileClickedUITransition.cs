using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.EventSystems;

using Zenject;
using UniRx;

using BetterUI;

using Assets.Simulation.GameMap;

namespace Assets.UI.GameMap {

    public class TileClickedUITransition : UITransitionBase {

        #region instance fields and properties

        private bool WasClickedThisFrame;  

        #endregion

        #region instance methods

        [Inject]
        public void InjectDependencies(TileClickedSignal tileClickedSignal) {
            tileClickedSignal.AsObservable.Subscribe(OnTileClicked);
        }

        #region from UITransitionBase

        public override bool IsTransitionValid() {
            return WasClickedThisFrame;
        }

        #endregion

        #region ITileClickedEventHandler

        public void OnTileClicked(Tuple<IMapTile, PointerEventData> dataTuple) {
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
