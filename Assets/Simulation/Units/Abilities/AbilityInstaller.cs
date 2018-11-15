using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

namespace Assets.Simulation.Units.Abilities {

    public class AbilityInstaller : MonoInstaller {

        #region instance methods

        #region from MonoInstaller

        public override void InstallBindings() {
            Container.Bind<IAbilityHandler>().To<FoundCityAbilityHandler>       ().AsSingle();
            Container.Bind<IAbilityHandler>().To<BuildImprovementAbilityHandler>().AsSingle();
            Container.Bind<IAbilityHandler>().To<BuildRoadAbilityHandler>       ().AsSingle();
            Container.Bind<IAbilityHandler>().To<ClearVegetationAbilityHandler> ().AsSingle();
            Container.Bind<IAbilityHandler>().To<SetUpToBombardAbilityHandler>  ().AsSingle();
            Container.Bind<IAbilityHandler>().To<FortifyAbilityHandler>         ().AsSingle();
            Container.Bind<IAbilityHandler>().To<PillageAbilityHandler>         ().AsSingle();
            Container.Bind<IAbilityHandler>().To<GainFreeTechAbilityHandler>    ().AsSingle();
            Container.Bind<IAbilityHandler>().To<HurryProductionAbilityHandler> ().AsSingle();

            var availableAbilities = Resources.LoadAll<AbilityDefinition>("Unit Abilities");

            Container.Bind<IEnumerable<IAbilityDefinition>>().WithId("Available Abilities").FromInstance(availableAbilities);

            Container.Bind<IAbilityExecuter>().To<AbilityExecuter>().AsSingle();
        }

        #endregion

        #endregion

    }

}
