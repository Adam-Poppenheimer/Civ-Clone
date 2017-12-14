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
            var abilityHandlers = new List<IUnitAbilityHandler>();

            Container.Bind<List<IUnitAbilityHandler>>().FromInstance(abilityHandlers);

            Container.Bind<IUnitAbilityExecuter>().To<UnitAbilityExecuter>().AsSingle();
        }

        #endregion

        #endregion

    }

}
