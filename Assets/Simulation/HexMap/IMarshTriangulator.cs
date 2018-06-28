using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.HexMap {

    public interface IMarshTriangulator {

        #region methods

        bool ShouldTriangulateMarshCenter(CellTriangulationData data);
        bool ShouldTriangulateMarshEdge  (CellTriangulationData data);
        bool ShouldTriangulateMarshCorner(CellTriangulationData data);

        void TriangulateMarshCenter(CellTriangulationData data);
        void TriangulateMarshEdge  (CellTriangulationData data);
        void TriangulateMarshCorner(CellTriangulationData data);

        #endregion

    }

}
