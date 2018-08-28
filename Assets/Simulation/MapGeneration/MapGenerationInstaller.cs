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
            Container.Bind<IRiverGenerator>().To<RiverGenerator>().AsSingle();

            Container.Bind<IMapGenerator>().To<MapGenerator>().AsSingle();

            Container.Bind<IMapGenerationConfig>().To<MapGenerationConfig>().FromInstance(Config);

            Container.Bind<IOceanGenerator>            ().To<OceanGenerator>            ().AsSingle();
            Container.Bind<IRegionGenerator>           ().To<RegionGenerator>           ().AsSingle();
            Container.Bind<ICellClimateLogic>          ().To<CellClimateLogic>          ().AsSingle();
            Container.Bind<IGridTraversalLogic>        ().To<GridTraversalLogic>        ().AsSingle();
            Container.Bind<IResourceDistributor>       ().To<ResourceDistributor>       ().AsSingle();
            Container.Bind<IYieldScorer>               ().To<YieldScorer>               ().AsSingle();
            Container.Bind<IRegionBalancer>            ().To<RegionBalancer>            ().AsSingle();
            Container.Bind<IYieldEstimator>            ().To<YieldEstimator>            ().AsSingle();
            Container.Bind<IStrategicCopiesLogic>      ().To<StrategicCopiesLogic>      ().AsSingle();
            Container.Bind<IStartingUnitPlacementLogic>().To<StartingUnitPlacementLogic>().AsSingle();
            Container.Bind<ICivHomelandGenerator>      ().To<CivHomelandGenerator>      ().AsSingle();
            Container.Bind<IGridPartitionLogic>        ().To<GridPartitionLogic>        ().AsSingle();
            Container.Bind<ITemplateSelectionLogic>    ().To<TemplateSelectionLogic>    ().AsSingle();
            Container.Bind<IWaterRationalizer>         ().To<WaterRationalizer>         ().AsSingle();
            Container.Bind<IVegetationPainter>         ().To<VegetationPainter>         ().AsSingle();
            Container.Bind<ISectionSubdivisionLogic>   ().To<SectionSubdivisionLogic>   ().AsSingle();
            Container.Bind<ILuxuryDistributor>         ().To<LuxuryDistributor>         ().AsSingle();

            Container.Bind<IBalanceStrategy>().To<BonusResourceBalanceStrategy>().AsSingle();
            Container.Bind<IBalanceStrategy>().To<JungleBalanceStrategy>       ().AsSingle();
            Container.Bind<IBalanceStrategy>().To<LakeBalanceStrategy>         ().AsSingle();
            Container.Bind<IBalanceStrategy>().To<HillsBalanceStrategy>        ().AsSingle();
            Container.Bind<IBalanceStrategy>().To<OasisBalanceStrategy>        ().AsSingle();
        }

        #endregion

        #endregion

    }

}
