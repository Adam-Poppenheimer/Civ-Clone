using System;

namespace Assets.Simulation.HexMap {

    public interface IRoadTriangulator {

        #region methods

        bool ShouldTriangulateRoads(CellTriangulationData data);
        void TriangulateRoads      (CellTriangulationData data);

        #endregion

    }

}