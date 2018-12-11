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

        [SerializeField] private GameObject BuildingDisplayPrefab;

        [SerializeField] private CitySummaryDisplay CitySummaryPrefab;
        [SerializeField] private RectTransform      CitySummaryContainer;

        [SerializeField] private CityCaptureDisplay CityCaptureDisplay;

        #endregion

        #region instance methods

        #region from MonoInstaller

        public override void InstallBindings() {
            Container.Bind<ICityUIConfig>().To<CityUIConfig>().FromResource("Cities");

            Container
                .BindFactory<IBuildingDisplay, BuildingDisplayFactory>()
                .To<BuildingDisplay>()
                .FromComponentInNewPrefab(BuildingDisplayPrefab);

            Container.Bind<CitySummaryDisplay>().FromInstance(CitySummaryPrefab);

            Container.Bind<RectTransform>().WithId("City Summary Container").FromInstance(CitySummaryContainer);

            Container.Bind<CitySummaryManager>().AsSingle();

            Container.Bind<ICityCaptureDisplay>().To<CityCaptureDisplay>().FromInstance(CityCaptureDisplay);
        }

        #endregion

        #endregion

    }

}
