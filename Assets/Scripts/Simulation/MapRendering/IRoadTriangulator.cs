using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapRendering {

    public interface IRoadTriangulator {

        #region methods

        void TriangulateRoads(IHexCell cell, IHexMesh roadsMesh);

        #endregion

    }
}