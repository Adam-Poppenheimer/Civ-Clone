﻿using System;
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
            Container.Bind<IEnumerable<IAbilityHandler>>().WithId("Unit Ability Handlers").FromMethod(
                context => new List<IAbilityHandler>() {
                    context.Container.Instantiate<FoundCityAbilityHandler>(),
                    context.Container.Instantiate<BuildImprovementAbilityHandler>(),
                    context.Container.Instantiate<BuildRoadAbilityHandler>(),
                    context.Container.Instantiate<ClearVegetationAbilityHandler>(),
                    context.Container.Instantiate<SetUpToBombardAbilityHandler>(),
                    context.Container.Instantiate<FortifyAbilityHandler>()
                }
            );

            var availableAbilities = Resources.LoadAll<AbilityDefinition>("Unit Abilities");

            Container.Bind<IEnumerable<IAbilityDefinition>>().WithId("Available Abilities").FromInstance(availableAbilities);

            Container.Bind<IAbilityExecuter>().To<AbilityExecuter>().AsSingle();
        }

        #endregion

        #endregion

    }

}
