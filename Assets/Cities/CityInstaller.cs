using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.GameMap;
using Assets.Cities.Buildings;
using Assets.Cities.Production;
using Assets.Cities.UI;

namespace Assets.Cities {

    public class CityInstaller : MonoInstaller {

        #region instance fields and properties

        [SerializeField] private CityConfig               CityConfig;
        [SerializeField] private PopulationGrowthConfig   GrowthConfig;
        [SerializeField] private BorderExpansionConfig    ExpansionConfig;
        [SerializeField] private ResourceGenerationConfig GenerationConfig;
        [SerializeField] private ProductionLogicConfig    ProductionConfig;

        [SerializeField] private MapHexGrid HexGrid;

        [SerializeField] private List<BuildingTemplate> AvailableBuildingTemplates;

        #endregion

        #region instance methods

        #region from MonoInstaller

        public override void InstallBindings() {
            Container.Bind<ICityConfig>              ().To<CityConfig>              ().FromInstance(CityConfig);
            Container.Bind<IPopulationGrowthConfig>  ().To<PopulationGrowthConfig>  ().FromInstance(GrowthConfig);
            Container.Bind<IBorderExpansionConfig >  ().To<BorderExpansionConfig >  ().FromInstance(ExpansionConfig);
            Container.Bind<IResourceGenerationConfig>().To<ResourceGenerationConfig>().FromInstance(GenerationConfig);
            Container.Bind<IProductionLogicConfig>   ().To<ProductionLogicConfig>   ().FromInstance(ProductionConfig);

            Container.Bind<IPopulationGrowthLogic>  ().To<PopulationGrowthLogic>  ().AsSingle();
            Container.Bind<IProductionLogic>        ().To<ProductionLogic>        ().AsSingle();
            Container.Bind<IResourceGenerationLogic>().To<ResourceGenerationLogic>().AsSingle();
            Container.Bind<IBorderExpansionLogic>   ().To<BorderExpansionLogic>   ().AsSingle();
            Container.Bind<IWorkerDistributionLogic>().To<WorkerDistributionLogic>().AsSingle();

            Container.Bind<ITilePossessionCanon>().To<TilePossessionCanon>().AsSingle();

            Container.Bind<IMapHexGrid>().To<MapHexGrid>().FromInstance(HexGrid);

            Container.Bind<IBuildingPossessionCanon>().To<BuildingPossessionCanon>().AsSingle();

            Container.Bind<ITemplateValidityLogic>().To<TemplateValidityLogic>().AsSingle();

            Container.Bind<IBuildingFactory>().To<BuildingFactory>().AsSingle();

            Container.Bind<IProductionProjectFactory>().To<ProductionProjectFactory>().AsSingle();

            Container.Bind<ICityEventBroadcaster>().To<CityEventBroadcaster>().AsSingle();

            Container.Bind<IRecordkeepingCityFactory>().To<RecordkeepingCityFactory>().AsSingle();

            Container.Bind<List<IBuildingTemplate>>().FromInstance(AvailableBuildingTemplates.Cast<IBuildingTemplate>().ToList());
        }

        #endregion

        #endregion

    }

}
