using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.EventSystems;

using Zenject;
using UniRx;

namespace Assets.UI.Core {

    public class PlayerSignals {

        #region instance fields and properties

        public IObservable<PointerEventData> ClickedAnywhereSignal { get; private set; }

        public IObservable<Unit> CancelPressedSignal { get; private set; }

        public ISubject<Unit> EndTurnRequestedSignal { get; private set; }

        #endregion

        #region constructors

        public PlayerSignals() {
            ClickedAnywhereSignal = Observable.EveryUpdate().Where(ClickedAnywhereFilter).Select(ClickedAnywhereSelector);

            CancelPressedSignal = Observable.EveryUpdate().Where(CancelPressedFilter).AsUnitObservable();

            EndTurnRequestedSignal = new Subject<Unit>();
        }

        #endregion

        #region instance methods

        private bool ClickedAnywhereFilter(long frameDuration) {
            return Input.GetMouseButtonDown(0) || Input.GetMouseButtonDown(1);
        }

        private PointerEventData ClickedAnywhereSelector(long frameDuration) {
            var newData = new PointerEventData(EventSystem.current);
            newData.position = Input.mousePosition;
            return newData;
        }

        private bool CancelPressedFilter(long frameDuration) {
            return Input.GetButtonDown("Cancel");
        }

        #endregion

    }

}
