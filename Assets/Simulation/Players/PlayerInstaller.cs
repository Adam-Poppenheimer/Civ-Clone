using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.Players.Barbarians;

namespace Assets.Simulation.Players {

    public class PlayerInstaller : MonoInstaller {

        #region instance fields and properties

        [SerializeField] private PlayerConfig PlayerConfig;

        #endregion

        #region instance methods

        #region from MonoInstaller

        public override void InstallBindings() {
            Container.Bind<IPlayerBrain>().WithId("Human Brain")    .To<HumanPlayerBrain>    ().AsSingle();
            Container.Bind<IPlayerBrain>().WithId("Barbarian Brain").To<BarbarianPlayerBrain>().AsSingle();

            Container.Bind<IPlayerConfig>().To<PlayerConfig>().FromInstance(PlayerConfig);

            Container.Bind<IPlayerFactory>                 ().To<PlayerFactory>                 ().AsSingle();
            Container.Bind<IBarbarianUnitBrain>            ().To<BarbarianUnitBrain>            ().AsSingle();
            Container.Bind<IBarbarianWanderBrain>          ().To<BarbarianWanderBrain>          ().AsSingle();
            Container.Bind<IBarbarianInfluenceMapGenerator>().To<BarbarianInfluenceMapGenerator>().AsSingle();
            Container.Bind<IBarbarianBrainTools>           ().To<BarbarianBrainTools>           ().AsSingle();

            Container.Bind<PlayerSignals>().AsSingle();
            
            Container.QueueForInject(PlayerConfig);
        }

        #endregion

        #endregion

    }

}
