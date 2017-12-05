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

        [SerializeField] private float MapTileHoverDelay;

        [SerializeField] private ResourceSummaryDisplay MapTileHoverYieldDisplay;

        #endregion

        #region instance methods

        #region from MonoInstaller

        public override void InstallBindings() {
            Container.Bind<MonoBehaviour>().WithId("Coroutine Invoker").FromInstance(this);
            Container.Bind<float>().WithId("Map Tile Hover Delay").FromInstance(MapTileHoverDelay);            

            Container.Bind<IMapTileSignalLogic>().To<MapTileSignalLogic>().AsSingle();

            Container.Bind<IResourceSummaryDisplay>()
                .WithId("Tile Hover Yield Display")
                .To<ResourceSummaryDisplay>()
                .FromInstance(MapTileHoverYieldDisplay);

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
