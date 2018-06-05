using System;

using UnityEngine;

namespace Assets.Simulation.HexMap {

    public interface IBasicTerrainTriangulator {

        #region methods

        void TriangulateTerrainCenter(CellTriangulationData data);

        bool ShouldTriangulateTerrainConnection(CellTriangulationData data);
        void TriangulateTerrainConnection      (CellTriangulationData data);

        #endregion

    }

}