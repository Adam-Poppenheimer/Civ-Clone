using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.GameMap;
using Assets.Cities.Buildings;

namespace Assets.Cities {

    public class CityInstaller : MonoInstaller {

        #region instance fields and properties

        [SerializeField] private CityConfig CityConfig;
        [SerializeField] private PopulationGrowthConfig GrowthConfig;
        [SerializeField] private BorderExpansionConfig ExpansionConfig;

        [SerializeField] private MapHexGrid HexGrid;

        #endregion

        #region instance methods

        #region from MonoInstaller

        public override void InstallBindings() {
            Container.Bind<ICityConfig>()            .To<CityConfig>()            .FromInstance(CityConfig);
            Container.Bind<IPopulationGrowthConfig>().To<PopulationGrowthConfig>().FromInstance(GrowthConfig);
            Container.Bind<IBorderExpansionConfig >().To<BorderExpansionConfig >().FromInstance(ExpansionConfig);

            Container.Bind<IPopulationGrowthLogic>().To<PopulationGrowthLogic>().AsSingle();
            Container.Bind<IProductionLogic>().To<ProductionLogic>().AsSingle();
            Container.Bind<IResourceGenerationLogic>().To<ResourceGenerationLogic>().AsSingle();
            Container.Bind<IBorderExpansionLogic>().To<BorderExpansionLogic>().AsSingle();
            Container.Bind<IWorkerDistributionLogic>().To<WorkerDistributionLogic>().AsSingle();

            Container.Bind<ITilePossessionCanon>().To<TilePossessionCanon>().AsSingle();

            Container.Bind<IMapHexGrid>().To<MapHexGrid>().FromInstance(HexGrid);

            Container.BindIFactory<IBuildingTemplate, IProductionProject, ProductionProjectFactory>();
        }

        #endregion

        #endregion

    }

}
