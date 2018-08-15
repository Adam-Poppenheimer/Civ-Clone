using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace Assets.Simulation.HexMap {

    public interface IOasisTriangulator {

        #region methods

        bool ShouldTriangulateOasis(CellTriangulationData data);
        void TriangulateOasis      (CellTriangulationData data);

        #endregion

    }

}
