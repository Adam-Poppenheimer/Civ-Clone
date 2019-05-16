using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapRendering {

    public interface IWeightsTriangulator {

        #region methods

        void TriangulateCellWeights(IHexCell center, IHexMesh weightsMesh);

        #endregion

    }

}