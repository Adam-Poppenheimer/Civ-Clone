using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapRendering {

    public interface IOasisTriangulator {

        #region methods

        void TrianglateOasis(IHexCell cell, IHexMesh waterMesh, IHexMesh landMesh);

        #endregion

    }

}