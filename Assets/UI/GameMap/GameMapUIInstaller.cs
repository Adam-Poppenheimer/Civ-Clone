using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.EventSystems;

using Zenject;
using UniRx;

namespace Assets.UI.GameMap {

    public class GameMapUIInstaller : MonoInstaller {

        #region instance fields and properties

        [SerializeField] private MapTileDisplay MapTileDisplay;

        #endregion

        #region instance methods

        #region from MonoInstaller

        public override void InstallBindings() {
            Container.ShouldCheckForInstallWarning = false;

            var clickedAnywhereSignal = Container.ResolveId<IObservable<Unit>>("Clicked Anywhere Signal");
            var cancelPressedSignal = Container.ResolveId<IObservable<Unit>>("Cancel Pressed Signal");

            var mapTileDisplayDeselectedSignal = Observable.Merge(
                SignalBuilderUtility.BuildMouseDeselectedSignal(MapTileDisplay.gameObject, clickedAnywhereSignal),
                cancelPressedSignal
            );

            Container.Bind<IObservable<Unit>>().WithId("MapTileDisplay Deselected").FromInstance(mapTileDisplayDeselectedSignal);
        }

        #endregion

        #endregion

    }

}
