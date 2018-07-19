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
            Container.QueueForInject(Config);

            foreach(var template in Config.StartingLocationTemplates.Concat(Config.BoundaryTemplates)) {
                Container.QueueForInject(template);
            }

            Container.Bind<IRiverGenerator>().To<RiverGenerator>().AsSingle();

            Container.Bind<IHexMapGenerator>().To<SubdividingMapGenerator>().AsSingle();

            Container.Bind<IMapGenerationConfig>().To<MapGenerationConfig>().FromInstance(Config);

            Container.Bind<IContinentGenerator>  ().To<SubdividingContinentGenerator>().AsSingle();
            Container.Bind<IOceanGenerator>      ().To<SubdividingOceanGenerator>    ().AsSingle();
            Container.Bind<IRegionGenerator>     ().To<RegionGenerator>              ().AsSingle();
            Container.Bind<ICellTemperatureLogic>().To<CellTemperatureLogic>         ().AsSingle();
            Container.Bind<IGridTraversalLogic>  ().To<GridTraversalLogic>           ().AsSingle();
        }

        #endregion

        #endregion

    }

}
