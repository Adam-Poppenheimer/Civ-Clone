using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

using Assets.Simulation.Barbarians;

namespace Assets.Simulation.Players {

    public class PlayerInstaller : MonoInstaller {

        #region instance methods

        #region from MonoInstaller

        public override void InstallBindings() {
            Container.Bind<IPlayerBrain>().WithId("Human Brain")    .To<HumanPlayerBrain>    ().AsSingle();
            Container.Bind<IPlayerBrain>().WithId("Barbarian Brain").To<BarbarianPlayerBrain>().AsSingle();

            Container.Bind<IPlayerFactory>().To<PlayerFactory>().AsSingle();
            Container.Bind<IBrainPile>    ().To<BrainPile>    ().AsSingle();

            Container.Bind<PlayerSignals>().AsSingle();
        }

        #endregion

        #endregion

    }

}
