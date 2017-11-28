﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.EventSystems;

using Zenject;
using UniRx;

namespace Assets.Simulation.Core {

    public class PlayerInputInstaller : MonoInstaller {

        #region instance methods

        #region from MonoInstaller

        public override void InstallBindings() {
            var clickedAnywhereSignal = Observable.EveryUpdate().Where(ClickedAnywhereFilter).AsUnitObservable();
            var cancelPressedSignal = Observable.EveryUpdate().Where(CancelPressedFilter).AsUnitObservable();

            Container.Bind<IObservable<Unit>>().WithId("Clicked Anywhere Signal").FromInstance(clickedAnywhereSignal);
            Container.Bind<IObservable<Unit>>().WithId("Cancel Pressed Signal").FromInstance(cancelPressedSignal);

            Container.DeclareSignal<EndTurnRequestedSignal>();
        }

        #endregion

        private bool ClickedAnywhereFilter(long frameDuration) {
            return Input.GetMouseButtonUp(0) || Input.GetMouseButtonUp(1);
        }

        private bool CancelPressedFilter(long frameDuration) {
            return Input.GetButtonDown("Cancel");
        }

        #endregion

    }

}