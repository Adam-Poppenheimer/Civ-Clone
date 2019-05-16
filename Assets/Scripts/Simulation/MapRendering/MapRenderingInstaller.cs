﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

namespace Assets.Simulation.MapRendering {

    public class MapRenderingInstaller : MonoInstaller {

        #region instance fields and properties

        [SerializeField] private MapRenderConfig   RenderConfig;
        [SerializeField] private FeatureConfig     FeatureConfig;
        [SerializeField] private TerrainBakeConfig TerrainBakeConfig;
        [SerializeField] private HexCellShaderData ShaderData;
        [SerializeField] private MapChunk          MapChunkPrefab;
        [SerializeField] private HexSubMesh        HexSubMeshPrefab;
        [SerializeField] private TerrainBaker      TerrainBaker;
        [SerializeField] private OrientationBaker  OrientationBaker;

        #endregion

        #region instance methods

        #region from MonoInstaller

        public override void InstallBindings() {
            Container.Bind<IMapRenderConfig>  ().To<MapRenderConfig>  ().FromInstance(RenderConfig);
            Container.Bind<IFeatureConfig>    ().To<FeatureConfig>    ().FromInstance(FeatureConfig);
            Container.Bind<ITerrainBakeConfig>().To<TerrainBakeConfig>().FromInstance(TerrainBakeConfig);
            Container.Bind<IHexCellShaderData>().To<HexCellShaderData>().FromInstance(ShaderData);
            Container.Bind<ITerrainBaker>     ().To<TerrainBaker>     ().FromInstance(TerrainBaker);
            Container.Bind<IOrientationBaker> ().To<OrientationBaker> ().FromInstance(OrientationBaker);

            Container.BindMemoryPoolCustomInterface<Mesh, MeshMemoryPool, IMemoryPool<Mesh>>()
                     .WithInitialSize(50);

            Container.BindMemoryPoolCustomInterface<HexSubMesh, HexSubMesh.Pool, IMemoryPool<HexRenderingData, HexSubMesh>>()
                     .WithInitialSize(50)
                     .FromComponentInNewPrefab(HexSubMeshPrefab)
                     .UnderTransformGroup("Hex Sub-Meshes");

            Container.BindMemoryPoolCustomInterface<HexMesh, HexMesh.Pool, IMemoryPool<string, HexMeshData, HexMesh>>()
                     .WithInitialSize(10)
                     .FromNewComponentOnNewGameObject()
                     .UnderTransformGroup("Hex Meshes");

            Container.BindMemoryPoolCustomInterface<MapChunk, MapChunk.Pool, IMemoryPool<MapChunk>>()
                     .WithInitialSize(10)
                     .FromComponentInNewPrefab(MapChunkPrefab)
                     .UnderTransformGroup("Map Chunks");

            Container.Bind<MapRenderingSignals>    ().AsSingle();
            Container.Bind<TerrainRefreshResponder>().AsSingle().NonLazy();

            Container.Bind<INoiseGenerator>                ().To<NoiseGenerator>                ().AsSingle();
            Container.Bind<ITerrainAlphamapLogic>          ().To<TerrainAlphamapLogic>          ().AsSingle().NonLazy();
            Container.Bind<ITerrainHeightLogic>            ().To<TerrainHeightLogic>            ().AsSingle().NonLazy();
            Container.Bind<IPointOrientationLogic>         ().To<PointOrientationLogic>         ().AsSingle();
            Container.Bind<ICellHeightmapLogic>            ().To<CellHeightmapLogic>            ().AsSingle();
            Container.Bind<IMountainHeightmapLogic>        ().To<MountainHeightmapLogic>        ().AsSingle();
            Container.Bind<ITerrainMixingLogic>            ().To<TerrainMixingLogic>            ().AsSingle();
            Container.Bind<ICellAlphamapLogic>             ().To<CellAlphamapLogic>             ().AsSingle();
            Container.Bind<IWaterTriangulator>             ().To<WaterTriangulator>             ().AsSingle();
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
            Container.Bind<IContourRationalizer>           ().To<ContourRationalizer>           ().AsSingle();
            Container.Bind<IFlatlandsHeightmapLogic>       ().To<FlatlandsHeightmapLogic>       ().AsSingle();
            Container.Bind<ICultureTriangulator>           ().To<CultureTriangulator>           ().AsSingle();
            Container.Bind<IMapCollisionLogic>             ().To<MapCollisionLogic>             ().AsSingle();
            Container.Bind<IFarmTriangulator>              ().To<FarmTriangulator>              ().AsSingle();
            Container.Bind<IHexMeshFactory>                ().To<HexMeshFactory>                ().AsSingle();
            Container.Bind<IRoadTriangulator>              ().To<RoadTriangulator>              ().AsSingle();
            Container.Bind<IMarshTriangulator>             ().To<MarshTriangulator>             ().AsSingle();
            Container.Bind<IOasisTriangulator>             ().To<OasisTriangulator>             ().AsSingle();
            Container.Bind<IOrientationTriangulator>       ().To<OrientationTriangulator>       ().AsSingle();
            Container.Bind<IWeightsTriangulator>           ().To<WeightsTriangulator>           ().AsSingle();
            Container.Bind<IFullMapRefresher>              ().To<FullMapRefresher>              ().AsSingle();
            Container.Bind<IContourTriangulator>           ().To<ContourTriangulator>           ().AsSingle();
            Container.Bind<IBakedElementsLogic>            ().To<BakedElementsLogic>            ().AsSingle();
            Container.Bind<IHeightmapRefresher>            ().To<HeightmapRefresher>            ().AsSingle();
        }

        #endregion

        #endregion

    }

}
