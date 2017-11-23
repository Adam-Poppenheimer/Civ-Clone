﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Cities.Production.UI;
using Assets.UI;

namespace Assets.Cities.UI {

    public class CityUIInstaller : MonoInstaller {

        #region instance fields and properties

        [SerializeField] private CityUIConfig UIConfig;

        [SerializeField] private WorkerSlotDisplay SlotDisplayPrefab;

        [SerializeField] private CityTileDisplay CityTileDisplay;

        [SerializeField] private ProductionDisplay ProductionDisplay;

        [SerializeField] private ProductionProjectDisplay ProductionProjectDisplay;

        [SerializeField] private ProductionProjectChooser ProductionProjectChooser;

        [SerializeField] private CityGrowthDisplay GrowthDisplay;

        [SerializeField] private ResourceSummaryDisplay CityYieldDisplay;

        [SerializeField] private CityBuildingDisplay CityBuildingDisplay;

        [SerializeField] private CityExpansionDisplay CityExpansionDisplay;

        #endregion

        #region instance methods

        #region from MonoInstaller

        public override void InstallBindings() {
            Container.Bind<ICityUIConfig>().To<CityUIConfig>().FromInstance(UIConfig);
            Container.Bind<IWorkerSlotDisplay>().To<WorkerSlotDisplay>().FromInstance(SlotDisplayPrefab);

            Container.Bind<ICityTileDisplay>().To<CityTileDisplay>().FromInstance(CityTileDisplay);

            Container.Bind<IProductionDisplay>().To<ProductionDisplay>().AsSingle();

            Container.Bind<IProductionProjectDisplay>().To<ProductionProjectDisplay>().FromInstance(ProductionProjectDisplay);
            Container.Bind<IProductionProjectChooser>().To<ProductionProjectChooser>().FromInstance(ProductionProjectChooser);

            Container.Bind<ICityGrowthDisplay>().To<CityGrowthDisplay>().FromInstance(GrowthDisplay);

            Container.Bind<IResourceSummaryDisplay>().WithId("City Yield Display").FromInstance(CityYieldDisplay);

            Container.Bind<ICityBuildingDisplay>().To<CityBuildingDisplay>().FromInstance(CityBuildingDisplay);

            Container.Bind<ICityExpansionDisplay>().To<CityExpansionDisplay>().FromInstance(CityExpansionDisplay);
        }

        #endregion

        #endregion

    }

}
