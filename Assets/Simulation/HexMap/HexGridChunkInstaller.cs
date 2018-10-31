using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

namespace Assets.Simulation.HexMap {

    public class HexGridChunkInstaller : MonoInstaller {

        #region instance fields and properties

        [SerializeField] private HexMesh SmoothTerrain;
        [SerializeField] private HexMesh JaggedTerrain;
        [SerializeField] private HexMesh Roads;
        [SerializeField] private HexMesh Rivers;
        [SerializeField] private HexMesh RiverConfluences;
        [SerializeField] private HexMesh RiverCorners;
        [SerializeField] private HexMesh Water;
        [SerializeField] private HexMesh Culture;
        [SerializeField] private HexMesh WaterShore;
        [SerializeField] private HexMesh Estuaries;
        [SerializeField] private HexMesh Marsh;
        [SerializeField] private HexMesh FloodPlains;
        [SerializeField] private HexMesh Oases;

        [SerializeField] private HexFeatureManager FeatureManager;
        [SerializeField] private Transform         FeatureContainer;

        #endregion

        #region instance methods

        #region from MonoInstaller

        public override void InstallBindings() {
            Container.Bind<HexMesh>().WithId("Smooth Terrain")   .FromInstance(SmoothTerrain);
            Container.Bind<HexMesh>().WithId("Jagged Terrain")   .FromInstance(JaggedTerrain);
            Container.Bind<HexMesh>().WithId("Roads")            .FromInstance(Roads);
            Container.Bind<HexMesh>().WithId("Rivers")           .FromInstance(Rivers);
            Container.Bind<HexMesh>().WithId("River Confluences").FromInstance(RiverConfluences);
            Container.Bind<HexMesh>().WithId("River Corners")    .FromInstance(RiverCorners);
            Container.Bind<HexMesh>().WithId("Water")            .FromInstance(Water);
            Container.Bind<HexMesh>().WithId("Culture")          .FromInstance(Culture);
            Container.Bind<HexMesh>().WithId("Water Shore")      .FromInstance(WaterShore);
            Container.Bind<HexMesh>().WithId("Estuaries")        .FromInstance(Estuaries);
            Container.Bind<HexMesh>().WithId("Marsh")            .FromInstance(Marsh);
            Container.Bind<HexMesh>().WithId("Flood Plains")     .FromInstance(FloodPlains);
            Container.Bind<HexMesh>().WithId("Oases")            .FromInstance(Oases);

            Container.Bind<Transform>().WithId("Feature Container").FromInstance(FeatureContainer);

            Container.Bind<IHexFeatureManager>       ().To<HexFeatureManager>       ().AsSingle();
            Container.Bind<IHexGridMeshBuilder>      ().To<HexGridMeshBuilder>      ().AsSingle();
            Container.Bind<IRiverTriangulator>       ().To<RiverTriangulator>       ().AsSingle();
            Container.Bind<IRiverTroughTriangulator> ().To<RiverTroughTriangulator> ().AsSingle();
            Container.Bind<IRiverSurfaceTriangulator>().To<RiverSurfaceTriangulator>().AsSingle();
            Container.Bind<ICultureTriangulator>     ().To<CultureTriangulator>     ().AsSingle();
            Container.Bind<IBasicTerrainTriangulator>().To<BasicTerrainTriangulator>().AsSingle();
            Container.Bind<IWaterTriangulator>       ().To<WaterTriangulator>       ().AsSingle();
            Container.Bind<IRoadTriangulator>        ().To<RoadTriangulator>        ().AsSingle();
            Container.Bind<IHexCellTriangulator>     ().To<HexCellTriangulator>     ().AsSingle();
            Container.Bind<IMarshTriangulator>       ().To<MarshTriangulator>       ().AsSingle();
            Container.Bind<IFloodPlainsTriangulator> ().To<FloodPlainsTriangulator> ().AsSingle();
            Container.Bind<IOasisTriangulator>       ().To<OasisTriangulator>       ().AsSingle();
            Container.Bind<IFeatureLocationLogic>    ().To<FeatureLocationLogic>    ().AsSingle();

            Container.Bind<IFeaturePlacer>().WithId("City Feature Placer")       .To<CityFeaturePlacer>       ().AsSingle();
            Container.Bind<IFeaturePlacer>().WithId("Resource Feature Placer")   .To<ResourceFeaturePlacer>   ().AsSingle();
            Container.Bind<IFeaturePlacer>().WithId("Improvement Feature Placer").To<ImprovementFeaturePlacer>().AsSingle();
            Container.Bind<IFeaturePlacer>().WithId("Tree Feature Placer")       .To<TreeFeaturePlacer>       ().AsSingle();
            Container.Bind<IFeaturePlacer>().WithId("Ruins Feature Placer")      .To<RuinsFeaturePlacer>      ().AsSingle();
        }

        #endregion

        #endregion

    }

}
