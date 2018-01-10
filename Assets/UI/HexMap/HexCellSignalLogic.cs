using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

using Zenject;
using UniRx;

using Assets.Simulation.HexMap;

namespace Assets.UI.HexMap {

    public class HexCellSignalLogic : IHexCellSignalLogic {

        #region instance fields and properties

        public IObservable<IHexCell> BeginHoverSignal {
            get { return BeginHoverSubject; }
        }
        private ISubject<IHexCell> BeginHoverSubject;

        public IObservable<IHexCell> EndHoverSignal {
            get { return EndHoverSubject; }
        }
        private ISubject<IHexCell> EndHoverSubject;

        private float HoverDelay;

        private bool ConsideredHovered = false;

        private Coroutine HoverTimerCoroutine;

        private HexCellSignals CellSignals;

        private MonoBehaviour CoroutineInvoker;

        #endregion

        #region constructors

        [Inject]
        public HexCellSignalLogic(HexCellSignals cellSignals,
            [Inject(Id = "Coroutine Invoker")] MonoBehaviour coroutineInvoker,
            [Inject(Id = "Map Tile Hover Delay")] float hoverDelay
        ){
            BeginHoverSubject = new Subject<IHexCell>();
            EndHoverSubject = new Subject<IHexCell>();

            CellSignals = cellSignals;

            cellSignals.PointerEnterSignal.Listen(OnEnterSignal);
            cellSignals.PointerExitSignal .Listen(OnExitSignal);

            CoroutineInvoker = coroutineInvoker;
            HoverDelay = hoverDelay;            
        }

        #endregion

        #region instance methods

        private void OnEnterSignal(IHexCell enteredTile) {
            ConsideredHovered = false;

            if(HoverTimerCoroutine != null) {
                CoroutineInvoker.StopCoroutine(HoverTimerCoroutine);
            }

            HoverTimerCoroutine = CoroutineInvoker.StartCoroutine(FireHoverSignalCoroutine(enteredTile));
        }

        private void OnExitSignal(IHexCell exitedTile) {
            if(HoverTimerCoroutine != null) {
                CoroutineInvoker.StopCoroutine(HoverTimerCoroutine);
            }

            if(ConsideredHovered) {
                ConsideredHovered = false;
                EndHoverSubject.OnNext(exitedTile);
            }
        }

        private IEnumerator FireHoverSignalCoroutine(IHexCell hoveredTile) {
            yield return new WaitForSecondsRealtime(HoverDelay);
            ConsideredHovered = true;
            BeginHoverSubject.OnNext(hoveredTile);
        }

        #endregion

    }

}
