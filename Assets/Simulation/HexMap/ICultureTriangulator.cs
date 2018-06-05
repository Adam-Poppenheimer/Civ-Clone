using Assets.Simulation.Civilizations;

namespace Assets.Simulation.HexMap {

    public interface ICultureTriangulator {

        #region methods

        bool ShouldTriangulateCulture(CellTriangulationData data);
        void TriangulateCulture      (CellTriangulationData data);

        #endregion

    }

}