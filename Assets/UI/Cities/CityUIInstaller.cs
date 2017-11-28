using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;
using UniRx;

using Assets.UI;

namespace Assets.UI.Cities {

    public class CityUIInstaller : MonoInstaller {

        #region instance fields and properties

        [SerializeField] private CityUIConfig UIConfig;

        [SerializeField] private WorkerSlotDisplay SlotDisplayPrefab;

        [SerializeField] private ResourceSummaryDisplay CityYieldDisplay;

        [SerializeField] private Transform CityDisplayRoot;

        #endregion

        #region instance methods

        #region from MonoInstaller

        public override void InstallBindings() {
            Container.Bind<ICityUIConfig>().To<CityUIConfig>().FromInstance(UIConfig);

            Container.Bind<IWorkerSlotDisplay>().To<WorkerSlotDisplay>().FromInstance(SlotDisplayPrefab);

            Container.Bind<IResourceSummaryDisplay>().WithId("City Yield Display").To<ResourceSummaryDisplay>().FromInstance(CityYieldDisplay);

            Container.ShouldCheckForInstallWarning = false;

            var clickedAnywhereSignal = Container.ResolveId<IObservable<Unit>>("Clicked Anywhere Signal");
            var cancelPressedSignal = Container.ResolveId<IObservable<Unit>>("Cancel Pressed Signal");

            var cityDisplayDeselected = Observable.Merge(
                SignalBuilderUtility.BuildMouseDeselectedSignal(CityDisplayRoot.gameObject, clickedAnywhereSignal),
                cancelPressedSignal
            );

            Container.Bind<IObservable<Unit>>().WithId("CityDisplay Deselected").FromInstance(cityDisplayDeselected);
        }

        #endregion

        #endregion

    }

}
