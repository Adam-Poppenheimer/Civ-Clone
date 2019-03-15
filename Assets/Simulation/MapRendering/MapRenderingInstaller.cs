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
        [SerializeField] private HexMesh         RiverMesh;

        #endregion

        #region instance methods

        #region from MonoInstaller

        public override void InstallBindings() {
            Container.Bind<IHexMesh>().WithId("River Mesh").FromInstance(RiverMesh);

            Container.Bind<IMapRenderConfig>().To<MapRenderConfig>().FromInstance(RenderConfig);
            Container.Bind<IFeatureConfig>  ().To<FeatureConfig>  ().FromInstance(FeatureConfig);

            Container.Bind<INoiseGenerator>                ().To<NoiseGenerator>                ().AsSingle();
            Container.Bind<ITerrainAlphamapLogic>          ().To<TerrainAlphamapLogic>          ().AsSingle().NonLazy();
            Container.Bind<ITerrainHeightLogic>            ().To<TerrainHeightLogic>            ().AsSingle().NonLazy();
            Container.Bind<IPointOrientationLogic>         ().To<PointOrientationLogic>         ().AsSingle();
            Container.Bind<ICellHeightmapLogic>            ().To<CellHeightmapLogic>            ().AsSingle();
            Container.Bind<IMountainHeightmapLogic>        ().To<MountainHeightmapLogic>        ().AsSingle();
            Container.Bind<ITerrainMixingLogic>            ().To<TerrainMixingLogic>            ().AsSingle();
            Container.Bind<ICellAlphamapLogic>             ().To<CellAlphamapLogic>             ().AsSingle();
            Container.Bind<IWaterTriangulator>             ().To<WaterTriangulator>             ().AsSingle();
            Container.Bind<IAlphamapMixingFunctions>       ().To<AlphamapMixingFunctions>       ().AsSingle();
            Container.Bind<IRiverSplineBuilder>            ().To<RiverSplineBuilder>            ().AsSingle();
            Container.Bind<IRiverAssemblyCanon>            ().To<RiverAssemblyCanon>            ().AsSingle();
            Container.Bind<IRiverBuilder>                  ().To<RiverBuilder>                  ().AsSingle();
            Container.Bind<IRiverSectionCanon>             ().To<RiverSectionCanon>             ().AsSingle();
            Container.Bind<IRiverBuilderUtilities>         ().To<RiverBuilderUtilities>         ().AsSingle();
            Container.Bind<IRiverTriangulator>             ().To<RiverTriangulator>             ().AsSingle().NonLazy();
            Container.Bind<ICellEdgeContourCanon>          ().To<CellEdgeContourCanon>          ().AsSingle();
            Container.Bind<INonRiverContourBuilder>        ().To<NonRiverContourBuilder>        ().AsSingle();
            Container.Bind<IRiverContourRationalizer>      ().To<RiverContourRationalizer>      ().AsSingle();
            Container.Bind<IHillsHeightmapLogic>           ().To<HillsHeightmapLogic>           ().AsSingle();
            Container.Bind<IPointOrientationInSextantLogic>().To<PointOrientationInSextantLogic>().AsSingle();
            Container.Bind<IPointOrientationWeightLogic>   ().To<PointOrientationWeightLogic>   ().AsSingle();
            Container.Bind<IContourRationalizer>           ().To<ContourRationalizer>           ().AsSingle();
        }

        #endregion

        #endregion

    }

}
