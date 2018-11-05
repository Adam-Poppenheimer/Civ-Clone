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

            Container.Bind<IPossessionRelationship<IHexCell, ICity>> ().To<CityLocationCanon>              ().AsSingle();
            Container.Bind<IPopulationGrowthLogic>                   ().To<PopulationGrowthLogic>          ().AsSingle();
            Container.Bind<IProductionLogic>                         ().To<ProductionLogic>                ().AsSingle();
            Container.Bind<IYieldGenerationLogic>                    ().To<YieldGenerationLogic>           ().AsSingle();
            Container.Bind<IBorderExpansionLogic>                    ().To<BorderExpansionLogic>           ().AsSingle();
            Container.Bind<IWorkerDistributionLogic>                 ().To<WorkerDistributionLogic>        ().AsSingle();
            Container.Bind<IBuildingProductionValidityLogic>         ().To<BuildingProductionValidityLogic>().AsSingle();
            Container.Bind<IPossessionRelationship<ICity, IHexCell>> ().To<CellPossessionCanon>            ().AsSingle();
            Container.Bind<IPossessionRelationship<ICity, IBuilding>>().To<BuildingPossessionCanon>        ().AsSingle();
            Container.Bind<IIncomeModifierLogic>                     ().To<IncomeModifierLogic>            ().AsSingle();
            Container.Bind<ICityValidityLogic>                       ().To<CityValidityLogic>              ().AsSingle();
            Container.Bind<ICityCombatLogic>                         ().To<CityCombatLogic>                ().AsSingle();
            Container.Bind<ICityHappinessLogic>                      ().To<CityHappinessLogic>             ().AsSingle();
            Container.Bind<IBuildingInherentYieldLogic>              ().To<BuildingInherentYieldLogic>     ().AsSingle();
            Container.Bind<ICellYieldFromBuildingsLogic>             ().To<CellYieldFromBuildingsLogic>    ().AsSingle();
            Container.Bind<IUnemploymentLogic>                       ().To<UnemploymentLogic>              ().AsSingle();
            Container.Bind<ICityLineOfSightLogic>                    ().To<CityLineOfSightLogic>           ().AsSingle();
            Container.Bind<ICityRazer>                               ().To<CityRazer>                      ().AsSingle();

            Container.Bind<CityCaptureResponder>().AsSingle().NonLazy();

            Container.Bind<IBuildingFactory>         ().To<BuildingFactory>         ().AsSingle();
            Container.Bind<IProductionProjectFactory>().To<ProductionProjectFactory>().AsSingle();
            Container.Bind<ICityFactory>             ().To<CityFactory>             ().AsSingle();

            Container.Bind<IBuildingRestriction>().To<ResourceBuildingRestriction>      ().AsSingle();
            Container.Bind<IBuildingRestriction>().To<ImprovementBuildingRestriction>   ().AsSingle();
            Container.Bind<IBuildingRestriction>().To<OtherBuildingsBuildingRestriction>().AsSingle();
            Container.Bind<IBuildingRestriction>().To<TerritoryBuildingRestriction>     ().AsSingle();

            Container.Bind<CitySignals>().AsSingle();

            Container.Bind<CompositeCitySignals>().AsSingle();
        }

        #endregion

        #endregion

    }

}
