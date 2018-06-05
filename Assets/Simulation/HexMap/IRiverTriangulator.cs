using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Simulation.HexMap {

    public interface IRiverTriangulator {

        #region methods

        bool ShouldTriangulateRiverConnection(CellTriangulationData thisData);
        void TriangulateRiverConnection      (CellTriangulationData thisData);

        #endregion

    }

}
