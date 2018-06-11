using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Zenject;

namespace Assets.Simulation.HexMap {

    public class HexGridChunkInstaller : MonoInstaller {

        #region instance fields and properties

        [SerializeField] private HexMesh Terrain;
        [SerializeField] private HexMesh Roads;
        [SerializeField] private HexMesh Rivers;
        [SerializeField] private HexMesh Water;
        [SerializeField] private HexMesh Culture;
        [SerializeField] private HexMesh WaterShore;
        [SerializeField] private HexMesh Estuaries;

        [SerializeField] private HexFeatureManager FeatureManager;

        #endregion

        #region instance methods

        #region from MonoInstaller

        public override void InstallBindings() {
            Container.Bind<HexMesh>().WithId("Terrain")    .FromInstance(Terrain);
            Container.Bind<HexMesh>().WithId("Roads")      .FromInstance(Roads);
            Container.Bind<HexMesh>().WithId("Rivers")     .FromInstance(Rivers);
            Container.Bind<HexMesh>().WithId("Water")      .FromInstance(Water);
            Container.Bind<HexMesh>().WithId("Culture")    .FromInstance(Culture);
            Container.Bind<HexMesh>().WithId("Water Shore").FromInstance(WaterShore);
            Container.Bind<HexMesh>().WithId("Estuaries")  .FromInstance(Estuaries);

            Container.Bind<IHexFeatureManager>().To<HexFeatureManager>().FromInstance(FeatureManager);

            Container.Bind<IHexGridMeshBuilder>      ().To<HexGridMeshBuilder>      ().AsSingle();
            Container.Bind<IRiverTriangulator>       ().To<RiverTriangulator>       ().AsSingle();
            Container.Bind<IRiverTroughTriangulator> ().To<RiverTroughTriangulator> ().AsSingle();
            Container.Bind<IRiverSurfaceTriangulator>().To<RiverSurfaceTriangulator>().AsSingle();
            Container.Bind<ICultureTriangulator>     ().To<CultureTriangulator>     ().AsSingle();
            Container.Bind<IBasicTerrainTriangulator>().To<BasicTerrainTriangulator>().AsSingle();
            Container.Bind<IWaterTriangulator>       ().To<WaterTriangulator>       ().AsSingle();
            Container.Bind<IRoadTriangulator>        ().To<RoadTriangulator>        ().AsSingle();
            Container.Bind<IHexCellTriangulator>     ().To<HexCellTriangulator>     ().AsSingle();
        }

        #endregion

        #endregion

    }

}
