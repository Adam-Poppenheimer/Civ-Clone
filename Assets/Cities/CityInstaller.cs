using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

namespace Assets.Cities {

    public class CityInstaller : MonoInstaller {

        #region instance fields and properties

        [SerializeField] private CityConfig Config;

        #endregion

        #region instance methods

        #region from MonoInstaller

        public override void InstallBindings() {
            Container.Bind<ICityConfig>().To<CityConfig>().FromInstance(Config);

            Container.Bind<IPopulationGrowthLogic>().To<PopulationGrowthLogic>().AsSingle();
            Container.Bind<IProductionLogic>().To<ProductionLogic>().AsSingle();
            Container.Bind<IResourceGenerationLogic>().To<ResourceGenerationLogic>().AsSingle();
            Container.Bind<IBorderExpansionLogic>().To<BorderExpansionLogic>().AsSingle();
            Container.Bind<IWorkerDistributionLogic>().To<WorkerDistributionLogic>().AsSingle();

            Container.Bind<ITilePossessionCanon>().To<TilePossessionCanon>().AsSingle();
        }

        #endregion

        #endregion

    }

}
