using System;
using System.Collections.Generic;
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

        

        #endregion

        #region instance methods

        #region from MonoInstaller

        /// <inheritdoc/>
        public override void InstallBindings() {
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

            Container.Bind<CivilizationSignals>().AsSingle();

            Container.Bind<IVisibilityResponder>().To<VisibilityResponder>().AsSingle().NonLazy();

            Container.Bind<CityPossessionResponder>().AsSingle().NonLazy();

            Container.Bind<ResourceTransferCanonSynchronizer>().AsSingle().NonLazy();
        }

        #endregion

        #endregion

    }

}
