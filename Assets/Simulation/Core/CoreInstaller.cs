using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.EventSystems;

using Zenject;
using UniRx;

using Assets.Simulation.Civilizations;

namespace Assets.Simulation.Core {

    public class CoreInstaller : MonoInstaller {

        #region instance methods

        #region from MonoInstaller

        public override void InstallBindings() {
            Container.Bind<ICivilizationFactory>().To<CivilizationFactory>().AsSingle();

            Container.Bind<ITurnExecuter>().To<TurnExecuter>().AsSingle();

            Container.DeclareSignal<TurnBeganSignal>();
            Container.DeclareSignal<TurnEndedSignal>();

            Container.Bind<GameCore>().AsSingle().NonLazy();
        }

        #endregion

        #endregion

    }

}
