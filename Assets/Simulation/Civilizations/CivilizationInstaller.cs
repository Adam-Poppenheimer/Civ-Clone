using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.Cities;
using Assets.Simulation.Units;

namespace Assets.Simulation.Civilizations {

    public class CivilizationInstaller : MonoInstaller {

        #region instance fields and properties

        [SerializeField] private CivilizationConfig Config;

        #endregion

        #region instance methods

        #region from MonoInstaller

        public override void InstallBindings() {
            Container.Bind<CivilizationFactory>().AsSingle();

            Container.Bind<ICivilizationConfig>().To<CivilizationConfig>().FromInstance(Config);

            Container.Bind<IPossessionRelationship<ICivilization, ICity>>()
                     .To<PossessionRelationship<ICivilization, ICity>>()
                     .AsSingle();

            Container.Bind<IPossessionRelationship<ICivilization, IUnit>>()
                     .To<PossessionRelationship<ICivilization, IUnit>>()
                     .AsSingle();
        }

        #endregion

        #endregion

    }

}
