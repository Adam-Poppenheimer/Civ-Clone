using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

namespace Assets.Simulation.MapRendering {

    public class MapRenderingInstaller : MonoInstaller {

        #region instance fields and properties

        [SerializeField] private MapRenderConfig RenderConfig;
        [SerializeField] private FeatureConfig   FeatureConfig;

        #endregion

        #region instance methods

        #region from MonoInstaller

        public override void InstallBindings() {
            Container.Bind<IMapRenderConfig>().To<MapRenderConfig>().FromInstance(RenderConfig);
            Container.Bind<IFeatureConfig>  ().To<FeatureConfig>  ().FromInstance(FeatureConfig);

            Container.Bind<INoiseGenerator>              ().To<NoiseGenerator>              ().AsSingle();
            Container.Bind<ITerrainAlphamapLogic>        ().To<TerrainAlphamapLogic>        ().AsSingle();
            Container.Bind<ITerrainHeightLogic>          ().To<TerrainHeightLogic>          ().AsSingle();
            Container.Bind<IPointOrientationLogic>       ().To<PointOrientationLogic>       ().AsSingle();
            Container.Bind<ICellHeightmapLogic>          ().To<CellHeightmapLogic>          ().AsSingle();
            Container.Bind<IMountainHeightmapLogic>      ().To<MountainHeightmapLogic>      ().AsSingle();
            Container.Bind<IMountainHeightmapWeightLogic>().To<MountainHeightmapWeightLogic>().AsSingle();
            Container.Bind<ITerrainMixingLogic>          ().To<TerrainMixingLogic>          ().AsSingle();
            Container.Bind<ICellAlphamapLogic>           ().To<CellAlphamapLogic>           ().AsSingle();
        }

        #endregion

        #endregion

    }

}
