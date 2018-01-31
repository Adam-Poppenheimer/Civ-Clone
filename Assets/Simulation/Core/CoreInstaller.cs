﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.EventSystems;

using Zenject;
using UniRx;

using Assets.Simulation.Civilizations;

namespace Assets.Simulation.Core {

    /// <summary>
    /// The installer that managed dependency injection for all classes and signals
    /// associated with the Core namespace
    /// </summary>
    public class CoreInstaller : MonoInstaller {

        #region instance methods

        #region from MonoInstaller

        /// <inheritdoc/>
        public override void InstallBindings() {
            Container.Bind<ITurnExecuter>().To<TurnExecuter>().AsSingle();

            Container.DeclareSignal<TurnBeganSignal>();
            Container.DeclareSignal<TurnEndedSignal>();

            Container.Bind<IGameCore>().To<GameCore>().AsSingle().NonLazy();
        }

        #endregion

        #endregion

    }

}
