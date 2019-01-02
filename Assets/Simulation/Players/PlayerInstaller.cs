using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

namespace Assets.Simulation.Players {

    public class PlayerInstaller : MonoInstaller {

        #region instance fields and properties

        [SerializeField] private PlayerConfig PlayerConfig;

        #endregion

        #region instance methods

        #region from MonoInstaller

        public override void InstallBindings() {
            Container.Bind<IPlayerBrain>().WithId("Human Brain").To<HumanPlayerBrain>().AsSingle();

            Container.Bind<IPlayerConfig>().To<PlayerConfig>().FromInstance(PlayerConfig);

            Container.Bind<IPlayerFactory>().To<PlayerFactory>().AsSingle();

            Container.Bind<PlayerSignals>().AsSingle();
            
            Container.QueueForInject(PlayerConfig);
        }

        #endregion

        #endregion

    }

}
