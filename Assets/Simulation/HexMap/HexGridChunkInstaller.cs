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

        #endregion

        #region instance methods

        #region from MonoInstaller

        public override void InstallBindings() {
            Container.Bind<HexMesh>().WithId("Terrain").FromInstance(Terrain);
            Container.Bind<HexMesh>().WithId("Roads")  .FromInstance(Roads);
            Container.Bind<HexMesh>().WithId("Rivers") .FromInstance(Rivers);
            Container.Bind<HexMesh>().WithId("Water")  .FromInstance(Water);

            Container.Bind<IHexGridMeshBuilder>().To<HexGridMeshBuilder>().AsSingle();
            Container.Bind<IRiverTriangulator> ().To<RiverTriangulator> ().AsSingle();
        }

        #endregion

        #endregion

    }

}
