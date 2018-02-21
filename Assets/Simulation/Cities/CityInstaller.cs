using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.EventSystems;

using Zenject;
using UniRx;

using Assets.Simulation.HexMap;

using Assets.Simulation.Cities.Growth;
using Assets.Simulation.Cities.Territory;
using Assets.Simulation.Cities.ResourceGeneration;
using Assets.Simulation.Cities.Buildings;
using Assets.Simulation.Cities.Production;
using Assets.Simulation.Cities.Distribution;

using Assets.Simulation.Civilizations;

namespace Assets.Simulation.Cities {

    /// <summary>
    /// Installer that manages dependency injection for all classes and signals
    /// associated with the Cities namespace.
    /// </summary>
    public class CityInstaller : MonoInstaller {

        #region instance fields and properties

        [SerializeField] private GameObject CityPrefab;

        #endregion

        #region instance methods

        #region from MonoInstaller

        /// <inheritdoc/>
        public override void InstallBindings() {
            Container.Bind<GameObject>().WithId("City Prefab").FromInstance(CityPrefab);

            Container.Bind<ICityConfig>().To<CityConfig>().FromResource("Cities");

            var allBuildings = new List<IBuildingTemplate>(Resources.LoadAll<BuildingTemplate>("Buildings"));

            Container.Bind<List<IBuildingTemplate>>().FromInstance(allBuildings);

            Container.Bind<IPopulationGrowthLogic>                   ().To<PopulationGrowthLogic>          ().AsSingle();
            Container.Bind<IProductionLogic>                         ().To<ProductionLogic>                ().AsSingle();
            Container.Bind<IResourceGenerationLogic>                 ().To<ResourceGenerationLogic>        ().AsSingle();
            Container.Bind<IBorderExpansionLogic>                    ().To<BorderExpansionLogic>           ().AsSingle();
            Container.Bind<IWorkerDistributionLogic>                 ().To<WorkerDistributionLogic>        ().AsSingle();
            Container.Bind<IBuildingProductionValidityLogic>         ().To<BuildingProductionValidityLogic>().AsSingle();
            Container.Bind<IPossessionRelationship<ICity, IHexCell>> ().To<CellPossessionCanon>            ().AsSingle();
            Container.Bind<IPossessionRelationship<ICity, IBuilding>>().To<BuildingPossessionCanon>        ().AsSingle();
            Container.Bind<IIncomeModifierLogic>                     ().To<IncomeModifierLogic>            ().AsSingle();
            Container.Bind<ICityValidityLogic>                       ().To<CityValidityLogic>              ().AsSingle();
            Container.Bind<ICityCombatLogic>                         ().To<CityCombatLogic>                ().AsSingle();
            Container.Bind<IHealthLogic>                             ().To<HealthLogic>                    ().AsSingle();
            Container.Bind<IHappinessLogic>                          ().To<HappinessLogic>                 ().AsSingle();

            Container.Bind<IBuildingFactory>         ().To<BuildingFactory>         ().AsSingle();
            Container.Bind<IProductionProjectFactory>().To<ProductionProjectFactory>().AsSingle();
            Container.Bind<ICityFactory>             ().To<CityFactory>             ().AsSingle();

            Container.DeclareSignal<CityProjectChangedSignal>();
            Container.DeclareSignal<CityDistributionPerformedSignal>();

            Container.Bind<CitySignals>().AsSingle();

            Container.Bind<CompositeCitySignals>().AsSingle();
        }

        #endregion

        #endregion

    }

}
