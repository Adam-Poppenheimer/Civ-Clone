using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.EventSystems;

using Zenject;
using UniRx;

namespace Assets.Simulation.Players {

    public class PlayerSignals {

        #region instance fields and properties

        public ISubject<IPlayer> PlayerCreated        { get; private set; }
        public ISubject<IPlayer> PlayerBeingDestroyed { get; private set; }
        public ISubject<IPlayer> EndTurnRequested     { get; private set; }

        public IObservable<PointerEventData> ClickedAnywhere { get; private set; }

        public IObservable<Unit> CancelPressed { get; private set; }

        #endregion

        #region constructors

        public PlayerSignals() {
            PlayerCreated        = new Subject<IPlayer>();
            PlayerBeingDestroyed = new Subject<IPlayer>();
            EndTurnRequested     = new Subject<IPlayer>();

            ClickedAnywhere = Observable.EveryUpdate().Where(ClickedAnywhereFilter).Select(ClickedAnywhereSelector);
            CancelPressed   = Observable.EveryUpdate().Where(CancelPressedFilter).AsUnitObservable();
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
