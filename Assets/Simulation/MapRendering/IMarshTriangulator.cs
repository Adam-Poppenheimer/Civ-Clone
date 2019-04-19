using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapRendering {

    public interface IMarshTriangulator {

        #region methods

        void TriangulateMarshes(IHexCell cell, IHexMesh mesh);

        #endregion

    }

}
