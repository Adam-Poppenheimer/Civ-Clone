﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

namespace Assets.Simulation.MapGeneration {

    public class MapGenerationInstaller : MonoInstaller {

        #region instance fields and properties

        [SerializeField] private MapGenerationConfig Config = null;

        [SerializeField] private List<MapTemplate> MapTemplates = null;

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
            Container.Bind<IStrategicDistributor>      ().To<StrategicDistributor>      ().AsSingle();
            Container.Bind<IMapScorer>                 ().To<MapScorer>                 ().AsSingle();
            Container.Bind<IHomelandBalancer>          ().To<HomelandBalancer>          ().AsSingle();
            Container.Bind<IYieldEstimator>            ().To<YieldEstimator>            ().AsSingle();
            Container.Bind<IStrategicCopiesLogic>      ().To<StrategicCopiesLogic>      ().AsSingle();
            Container.Bind<IStartingUnitPlacementLogic>().To<StartingUnitPlacementLogic>().AsSingle();
            Container.Bind<IHomelandGenerator>         ().To<HomelandGenerator>         ().AsSingle();
            Container.Bind<IGridPartitionLogic>        ().To<GridPartitionLogic>        ().AsSingle();
            Container.Bind<ITemplateSelectionLogic>    ().To<TemplateSelectionLogic>    ().AsSingle();
            Container.Bind<IWaterRationalizer>         ().To<WaterRationalizer>         ().AsSingle();
            Container.Bind<IVegetationPainter>         ().To<VegetationPainter>         ().AsSingle();
            Container.Bind<ISectionSubdivisionLogic>   ().To<SectionSubdivisionLogic>   ().AsSingle();
            Container.Bind<ILuxuryDistributor>         ().To<LuxuryDistributor>         ().AsSingle();
            Container.Bind<ICellScorer>                ().To<CellScorer>                ().AsSingle();

            Container.Bind<IBalanceStrategy>().To<ResourceBalanceStrategy>     ().AsSingle();
            Container.Bind<IBalanceStrategy>().To<JungleBalanceStrategy>       ().AsSingle();
            Container.Bind<IBalanceStrategy>().To<LakeBalanceStrategy>         ().AsSingle();
            Container.Bind<IBalanceStrategy>().To<HillsBalanceStrategy>        ().AsSingle();
            Container.Bind<IBalanceStrategy>().To<OasisBalanceStrategy>        ().AsSingle();
            Container.Bind<IBalanceStrategy>().To<MountainBalanceStrategy>     ().AsSingle();
            Container.Bind<IBalanceStrategy>().To<ExpandOceanBalanceStrategy>  ().AsSingle();

            Container.Bind<IEnumerable<IMapTemplate>>().FromInstance(MapTemplates.Cast<IMapTemplate>());
        }

        #endregion

        #endregion

    }

}
