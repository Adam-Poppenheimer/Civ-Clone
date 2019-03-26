using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.EventSystems;

using Zenject;
using UniRx;

using Assets.Simulation.HexMap;
using Assets.Simulation.MapResources;
using Assets.Simulation.MapGeneration;

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
            //Can make use of open generic types if updated to the latest version of Zenject,
            //which requires a more modern version of Unity
            Container.Bind<IWeightedRandomSampler<IBalanceStrategy>>   ().To<WeightedRandomSampler<IBalanceStrategy>>   ().AsSingle();
            Container.Bind<IWeightedRandomSampler<IHexCell>>           ().To<WeightedRandomSampler<IHexCell>>           ().AsSingle();
            Container.Bind<IWeightedRandomSampler<IResourceDefinition>>().To<WeightedRandomSampler<IResourceDefinition>>().AsSingle();
            Container.Bind<IWeightedRandomSampler<MapSection>>         ().To<WeightedRandomSampler<MapSection>>         ().AsSingle();

            //This represents the execution order of the various round executers, which is considered important
            Container.Bind<IRoundExecuter>().To<UnitRoundExecuter>        ().AsSingle();
            Container.Bind<IRoundExecuter>().To<CityRoundExecuter>        ().AsSingle();
            Container.Bind<IRoundExecuter>().To<CivilizationRoundExecuter>().AsSingle();

            Container.Bind<ICoreConfig>().To<CoreConfig>().FromResource("Core");

            Container.Bind<CoreSignals>            ().AsSingle();
            Container.Bind<RoundExecutionSequencer>().AsSingle().NonLazy();

            Container.Bind<IRandomizer>().To<Randomizer>().AsSingle();
            Container.Bind<IGameCore>  ().To<GameCore>  ().AsSingle().NonLazy();
            

            
        }

        #endregion

        #endregion

    }

}
