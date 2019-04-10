using System.Collections.ObjectModel;

namespace Assets.Simulation.MapRendering {

    public interface IHexMeshFactory {

        #region properties

        ReadOnlyCollection<IHexMesh> AllMeshes { get; }

        #endregion

        #region methods

        IHexMesh Create(string name, HexMeshData data);

        void Destroy(IHexMesh mesh);

        #endregion

    }

}