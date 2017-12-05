using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.EventSystems;

using Zenject;
using UniRx;

using Assets.Simulation.GameMap;

namespace Assets.UI.GameMap {

    public class MapTileSignalLogic : IMapTileSignalLogic {

        #region instance fields and properties

        public IObservable<IMapTile> BeginHoverSignal {
            get { return BeginHoverSubject; }
        }

        public IObservable<IMapTile> EndHoverSignal {
            get { return EndHoverSubject; }
        }

        private float HoverDelay;

        private bool ConsideredHovered = false;

        private ISubject<IMapTile> BeginHoverSubject;

        private ISubject<IMapTile> EndHoverSubject;

        private Coroutine HoverTimerCoroutine;

        private MonoBehaviour CoroutineInvoker;

        #endregion

        #region constructors

        [Inject]
        public MapTileSignalLogic(TilePointerEnterSignal enterSignal, TilePointerExitSignal exitSignal,
            [Inject(Id = "Coroutine Invoker")] MonoBehaviour coroutineInvoker,
            [Inject(Id = "Map Tile Hover Delay")] float hoverDelay
        ){
            BeginHoverSubject = new Subject<IMapTile>();
            EndHoverSubject = new Subject<IMapTile>();

            enterSignal.Listen(OnEnterSignal);
            exitSignal.Listen(OnExitSignal);

            CoroutineInvoker = coroutineInvoker;
            HoverDelay = hoverDelay;            
        }

        #endregion

        #region instance methods

        private void OnEnterSignal(IMapTile enteredTile, PointerEventData eventData) {
            ConsideredHovered = false;

            if(HoverTimerCoroutine != null) {
                CoroutineInvoker.StopCoroutine(HoverTimerCoroutine);
            }

            HoverTimerCoroutine = CoroutineInvoker.StartCoroutine(FireHoverSignalCoroutine(enteredTile));
        }

        private void OnExitSignal(IMapTile exitedTile, PointerEventData eventData) {
            if(HoverTimerCoroutine != null) {
                CoroutineInvoker.StopCoroutine(HoverTimerCoroutine);
            }

            if(ConsideredHovered) {
                ConsideredHovered = false;
                EndHoverSubject.OnNext(exitedTile);
            }
        }

        private IEnumerator FireHoverSignalCoroutine(IMapTile hoveredTile) {
            yield return new WaitForSecondsRealtime(HoverDelay);
            ConsideredHovered = true;
            BeginHoverSubject.OnNext(hoveredTile);
        }

        #endregion

    }

}
