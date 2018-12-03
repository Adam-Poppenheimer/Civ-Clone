using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.Cities;
using Assets.Simulation.Units;

namespace Assets.Simulation.Civilizations {

    /// <summary>
    /// The installer that manages dependency injection for all classes and signals
    /// associated with the Civilizations namespace.
    /// </summary>
    public class CivilizationInstaller : MonoInstaller {

        #region instance fields and properties

        [SerializeField] private Transform CivContainer;

        #endregion

        #region instance methods

        #region from MonoInstaller

        /// <inheritdoc/>
        public override void InstallBindings() {
            var civTemplates = Resources.LoadAll<CivilizationTemplate>("Civilizations");

            Container.Bind<ReadOnlyCollection<ICivilizationTemplate>>()
                     .FromInstance(civTemplates.Cast<ICivilizationTemplate>().ToList().AsReadOnly());

            Container.Bind<Transform>().WithId("Civ Container").FromInstance(CivContainer);

            Container.Bind<ICivilizationFactory>().To<CivilizationFactory>().AsSingle();

            Container.Bind<ICivilizationConfig>().To<CivilizationConfig>().FromResource("Civilizations");

            Container.Bind<IPossessionRelationship<ICivilization, ICity>>()
                     .To<CityPossessionCanon>()
                     .AsSingle();

            Container.Bind<IPossessionRelationship<ICivilization, IUnit>>()
                     .To<UnitPossessionCanon>()
                     .AsSingle();

            Container.Bind<ICivilizationYieldLogic>     ().To<CivilizationYieldLogic>     ().AsSingle();
            Container.Bind<IResourceExtractionLogic>    ().To<ResourceExtractionLogic>    ().AsSingle();
            Container.Bind<IResourceLockingCanon>       ().To<ResourceLockingCanon>       ().AsSingle();
            Container.Bind<IFreeResourcesLogic>         ().To<FreeResourcesLogic>         ().AsSingle();
            Container.Bind<IResourceTransferCanon>      ().To<ResourceTransferCanon>      ().AsSingle();
            Container.Bind<ICivilizationHappinessLogic> ().To<CivilizationHappinessLogic> ().AsSingle();
            Container.Bind<ICapitalCityCanon>           ().To<CapitalCityCanon>           ().AsSingle();
            Container.Bind<IConnectionPathCostLogic>    ().To<ConnectionPathCostLogic>    ().AsSingle();
            Container.Bind<ICivilizationConnectionLogic>().To<CivilizationConnectionLogic>().AsSingle();
            Container.Bind<ICivilizationTerritoryLogic> ().To<CivilizationTerritoryLogic> ().AsSingle();
            Container.Bind<IFreeUnitsLogic>             ().To<FreeUnitsLogic>             ().AsSingle();
            Container.Bind<IUnitMaintenanceLogic>       ().To<UnitMaintenanceLogic>       ().AsSingle();
            Container.Bind<ICapitalCitySynchronizer>    ().To<CapitalCitySynchronizer>    ().AsSingle();
            Container.Bind<IGlobalPromotionCanon>       ().To<GlobalPromotionCanon>       ().AsSingle();
            Container.Bind<IGreatPersonCanon>           ().To<GreatPersonCanon>           ().AsSingle();
            Container.Bind<IFreeBuildingApplier>        ().To<FreeBuildingApplier>        ().AsSingle();
            Container.Bind<IFreeBuildingsCanon>         ().To<FreeBuildingsCanon>         ().AsSingle();
            Container.Bind<ICivModifiers>               ().To<CivModifiers>               ().AsSingle();
            Container.Bind<IGoldenAgeCanon>             ().To<GoldenAgeCanon>             ().AsSingle();

            Container.Bind<CivilizationSignals>().AsSingle();

            Container.Bind<CityPossessionResponder>     ()                                  .AsSingle().NonLazy();
            Container.Bind<ICivDefeatExecutor>          ().To<CivDefeatExecutor>          ().AsSingle().NonLazy();
            Container.Bind<IGreatMilitaryPointGainLogic>().To<GreatMilitaryPointGainLogic>().AsSingle().NonLazy();

            Container.Bind<ResourceTransferCanonSynchronizer>().AsSingle().NonLazy();
        }

        #endregion

        #endregion

    }

}
