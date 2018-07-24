﻿using System;
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

            Container.Bind<IContinentGenerator>  ().To<ContinentGenerator>().AsSingle();
            Container.Bind<IOceanGenerator>      ().To<OceanGenerator>    ().AsSingle();
            Container.Bind<IRegionGenerator>     ().To<RegionGenerator>              ().AsSingle();
            Container.Bind<ICellTemperatureLogic>().To<CellTemperatureLogic>         ().AsSingle();
            Container.Bind<IGridTraversalLogic>  ().To<GridTraversalLogic>           ().AsSingle();
        }

        #endregion

        #endregion

    }

}
