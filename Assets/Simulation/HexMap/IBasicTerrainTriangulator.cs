using System;

using UnityEngine;

namespace Assets.Simulation.HexMap {

    public interface IBasicTerrainTriangulator {

        #region methods

        void TriangulateTerrainCenter(CellTriangulationData data);

        bool ShouldTriangulateTerrainEdge(CellTriangulationData data);
        void TriangulateTerrainEdge      (CellTriangulationData data);

        #endregion

    }

}