using System;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapRendering {

    public interface ICultureTriangulator {

        #region methods

        void TriangulateCultureInDirection(
            IHexCell center, HexDirection direction, IHexMesh cultureMesh
        );

        #endregion

    }

}