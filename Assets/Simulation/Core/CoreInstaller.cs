using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.EventSystems;

using Zenject;
using UniRx;

using Assets.Simulation.GameMap;

using Assets.Simulation.Cities;

namespace Assets.Simulation.Core {

    public class CoreInstaller : MonoInstaller {

        #region instance methods

        #region from MonoInstaller

        public override void InstallBindings() {
            Container.Bind<ITurnExecuter>().To<TurnExecuter>().AsSingle();
            Container.Bind<GameCore>().AsSingle().NonLazy();

            Container.DeclareSignal<TurnBeganSignal>();
            Container.DeclareSignal<TurnEndedSignal>();            
        }

        #endregion

        #endregion

    }

}
