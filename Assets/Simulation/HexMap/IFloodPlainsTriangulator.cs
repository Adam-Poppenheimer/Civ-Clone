using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.HexMap {

    public interface IFloodPlainsTriangulator {

        #region methods

        bool ShouldTriangulateFloodPlainEdge  (CellTriangulationData data);
        bool ShouldTriangulateFloodPlainCorner(CellTriangulationData data);

        void TriangulateFloodPlainEdge  (CellTriangulationData data);
        void TriangulateFloodPlainCorner(CellTriangulationData data);

        #endregion

    }

}
