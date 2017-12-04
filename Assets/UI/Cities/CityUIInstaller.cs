using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;
using UniRx;

using Assets.UI.Cities.Buildings;

using Assets.Simulation.Cities;

namespace Assets.UI.Cities {

    public class CityUIInstaller : MonoInstaller {

        #region instance fields and properties

        [SerializeField] private CityUIConfig UIConfig;

        [SerializeField] private GameObject SlotDisplayPrefab;

        [SerializeField] private GameObject BuildingDisplayPrefab;

        [SerializeField] private ResourceSummaryDisplay CityYieldDisplay;

        [SerializeField] private Transform CityDisplayRoot;

        #endregion

        #region instance methods

        #region from MonoInstaller

        public override void InstallBindings() {
            Container.Bind<ICityUIConfig>().To<CityUIConfig>().FromInstance(UIConfig);

            Container.Bind<IResourceSummaryDisplay>().WithId("City Yield Display").To<ResourceSummaryDisplay>().FromInstance(CityYieldDisplay);

            Container.BindFactory<WorkerSlotDisplay, WorkerSlotDisplay.Factory>().FromComponentInNewPrefab(SlotDisplayPrefab);

            Container.BindFactory<IBuildingDisplay, BuildingDisplayFactory>().FromComponentInNewPrefab(BuildingDisplayPrefab);

            Container.DeclareSignal<SlotDisplayClickedSignal>();

            Container.ShouldCheckForInstallWarning = false;

            var clickedAnywhereSignal = Container.ResolveId<IObservable<Unit>>("Clicked Anywhere Signal");
            var cancelPressedSignal = Container.ResolveId<IObservable<Unit>>("Cancel Pressed Signal");

            var cityDeselectedSignal = Observable.Merge(
                SignalBuilderUtility.BuildMouseDeselectedSignal(CityDisplayRoot.gameObject, clickedAnywhereSignal),
                cancelPressedSignal
            ).Select<Unit, ICity>(unit => null);

            Container.Bind<IObservable<ICity>>().WithId("Deselect Requested Signal").FromInstance(cityDeselectedSignal);
        }

        #endregion

        #endregion

    }

}
