using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

namespace Assets.Simulation.MapGeneration {

    public class MapGenerationInstaller : MonoInstaller {

        #region instance fields and properties

        [SerializeField] private MapGenerationConfig Config;

        #endregion

        #region instance methods

        #region from MonoInstaller

        public override void InstallBindings() {
            foreach(var regionTemplate in Resources.LoadAll<RegionGenerationTemplate>("")) {
                Container.QueueForInject(regionTemplate);
            }

            Container.Bind<IRiverGenerator>().To<RiverGenerator>().AsSingle();

            Container.Bind<IMapGenerator>().To<MapGenerator>().AsSingle();

            Container.Bind<IMapGenerationConfig>().To<MapGenerationConfig>().FromInstance(Config);

            Container.Bind<IOceanGenerator>      ().To<OceanGenerator>      ().AsSingle();
            Container.Bind<IRegionGenerator>     ().To<RegionGenerator>     ().AsSingle();
            Container.Bind<ICellTemperatureLogic>().To<CellTemperatureLogic>().AsSingle();
            Container.Bind<IGridTraversalLogic>  ().To<GridTraversalLogic>  ().AsSingle();
            Container.Bind<IResourceDistributor> ().To<ResourceDistributor> ().AsSingle();
            Container.Bind<IResourceSampler>     ().To<ResourceSampler>     ().AsSingle();
            Container.Bind<IYieldScorer>         ().To<YieldScorer>         ().AsSingle();
            Container.Bind<IRegionBalancer>      ().To<RegionBalancer>      ().AsSingle();
            Container.Bind<IYieldEstimator>      ().To<YieldEstimator>      ().AsSingle();
            Container.Bind<IStrategicCopiesLogic>().To<StrategicCopiesLogic>().AsSingle();

            Container.Bind<IBalanceStrategy>().To<BonusResourceBalanceStrategy> ().AsSingle();
            //Container.Bind<IBalanceStrategy>().To<JungleBalanceStrategy>        ().AsSingle();
            //Container.Bind<IBalanceStrategy>().To<FreshWaterLakeBalanceStrategy>().AsSingle();
            Container.Bind<IBalanceStrategy>().To<HillsBalanceStrategy>         ().AsSingle();
        }

        #endregion

        #endregion

    }

}
