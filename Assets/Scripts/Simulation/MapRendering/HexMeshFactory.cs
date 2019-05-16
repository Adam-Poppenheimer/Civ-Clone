using System;
using System.Collections.ObjectModel;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Zenject;

namespace Assets.Simulation.MapRendering {

    public class HexMeshFactory : IHexMeshFactory {

        #region instance fields and properties

        #region from IHexMeshFactory

        public ReadOnlyCollection<IHexMesh> AllMeshes {
            get { return allMeshes.AsReadOnly(); }
        }
        private List<IHexMesh> allMeshes = new List<IHexMesh>();

        #endregion



        private IMemoryPool<string, HexMeshData, HexMesh> HexMeshPool;

        #endregion

        #region constructors

        [Inject]
        public HexMeshFactory(IMemoryPool<string, HexMeshData, HexMesh> hexMeshPool) {
            HexMeshPool = hexMeshPool;
        }

        #endregion

        #region instance methods

        #region from IHexMeshFactory

        public IHexMesh Create(string name, HexMeshData data) {
            var newMesh = HexMeshPool.Spawn(name, data);

            allMeshes.Add(newMesh);

            return newMesh;
        }

        public void Destroy(IHexMesh mesh) {
            allMeshes.Remove(mesh);

            var concreteMesh = mesh as HexMesh;

            if(concreteMesh != null) {
                HexMeshPool.Despawn(concreteMesh);
            }
        }

        #endregion

        #endregion

    }

}
