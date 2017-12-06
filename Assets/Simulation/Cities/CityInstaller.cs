using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.EventSystems;

using Zenject;
using UniRx;

using Assets.Simulation.GameMap;

using Assets.Simulation.Cities.Growth;
using Assets.Simulation.Cities.Territory;
using Assets.Simulation.Cities.ResourceGeneration;
using Assets.Simulation.Cities.Buildings;
using Assets.Simulation.Cities.Production;
using Assets.Simulation.Cities.Distribution;

namespace Assets.Simulation.Cities {

    public class CityInstaller : MonoInstaller {

        #region instance fields and properties

        [SerializeField] private GameObject CityPrefab;

        [SerializeField] private CityConfig CityConfig;

        [SerializeField] private MapHexGrid HexGrid;

        [SerializeField] private List<BuildingTemplate> AvailableBuildingTemplates;

        #endregion

        #region instance methods

        #region from MonoInstaller

        public override void InstallBindings() {
            Container.Bind<GameObject>().WithId("City Prefab").FromInstance(CityPrefab);

            Container.BindFactory<IMapTile, ICity, Factory<IMapTile, ICity>>().FromFactory<RecordkeepingCityFactory>();

            Container.Bind<ICityConfig>().To<CityConfig>().FromInstance(CityConfig);

            Container.Bind<List<IBuildingTemplate>>().FromInstance(AvailableBuildingTemplates.Cast<IBuildingTemplate>().ToList());

            Container.Bind<IPopulationGrowthLogic>  ().To<PopulationGrowthLogic>  ().AsSingle();
            Container.Bind<IProductionLogic>        ().To<ProductionLogic>        ().AsSingle();
            Container.Bind<IResourceGenerationLogic>().To<ResourceGenerationLogic>().AsSingle();
            Container.Bind<IBorderExpansionLogic>   ().To<BorderExpansionLogic>   ().AsSingle();
            Container.Bind<IWorkerDistributionLogic>().To<WorkerDistributionLogic>().AsSingle();
            Container.Bind<ITemplateValidityLogic>  ().To<TemplateValidityLogic>  ().AsSingle();
            Container.Bind<ITilePossessionCanon>    ().To<TilePossessionCanon>    ().AsSingle();
            Container.Bind<IBuildingPossessionCanon>().To<BuildingPossessionCanon>().AsSingle();
            Container.Bind<IIncomeModifierLogic>    ().To<IncomeModifierLogic>    ().AsSingle();  

            Container.Bind<IMapHexGrid>().To<MapHexGrid>().FromInstance(HexGrid);  

            Container.Bind<IBuildingFactory>         ().To<BuildingFactory>         ().AsSingle();
            Container.Bind<IProductionProjectFactory>().To<ProductionProjectFactory>().AsSingle();
            Container.Bind<IRecordkeepingCityFactory>().To<RecordkeepingCityFactory>().AsSingle();        

            var cityClickedSignal = new Subject<ICity>();

            Container.Bind<ISubject<ICity>>().WithId("City Clicked Subject").FromInstance(cityClickedSignal);
            Container.Bind<IObservable<ICity>>().WithId("City Clicked Signal").FromInstance(cityClickedSignal);

            Container.DeclareSignal<CityProjectChangedSignal>();
            Container.DeclareSignal<CityDistributionPerformedSignal>();

            Container.Bind<CitySignals>().AsSingle();
        }

        #endregion

        #endregion

    }

}
