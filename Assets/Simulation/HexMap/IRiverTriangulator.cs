using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Simulation.HexMap {

    public interface IRiverTriangulator {

        #region methods

        void TriangulateConnectionAsRiver(
            HexDirection direction, IHexCell cell, EdgeVertices nearEdge
        );

        bool HasRiverCorner(IHexCell cell, HexDirection direction);

        #endregion

    }

}
