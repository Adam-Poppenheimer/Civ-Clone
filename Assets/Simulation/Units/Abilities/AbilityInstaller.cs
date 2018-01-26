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
            Container.Bind<IEnumerable<IAbilityHandler>>().WithId("Unit Ability Handlers").FromMethod(
                context => new List<IAbilityHandler>() {
                    context.Container.Instantiate<FoundCityAbilityHandler>(),
                    context.Container.Instantiate<BuildImprovementAbilityHandler>(),
                }
            );

            Container.Bind<IAbilityExecuter>().To<AbilityExecuter>().AsSingle();
        }

        #endregion

        #endregion

    }

}
