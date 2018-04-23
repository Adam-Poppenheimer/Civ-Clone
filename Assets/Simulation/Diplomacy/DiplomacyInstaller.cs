using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

namespace Assets.Simulation.Diplomacy {

    public class DiplomacyInstaller : MonoInstaller {

        #region instance fields and properties

        [SerializeField] private DiplomacyConfig Config;

        #endregion

        #region instance methods

        public override void InstallBindings() {
            Container.Bind<IDiplomacyConfig>        ().To<DiplomacyConfig>        ().FromInstance(Config);
            Container.Bind<IDiplomacyCore>          ().To<DiplomacyCore>          ().AsSingle();
            Container.Bind<IResourceExchangeBuilder>().To<ResourceExchangeBuilder>().AsSingle();
            Container.Bind<IExchangeBuilder>        ().To<ExchangeBuilder>        ().AsSingle();
            Container.Bind<IWarCanon>               ().To<WarCanon>               ().AsSingle();
        }

        #endregion

    }

}
