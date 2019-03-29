﻿using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapRendering {

    public interface IFarmTriangulator {

        #region methods

        void TriangulateFarmland(IHexCell cell, IHexMesh mesh);

        #endregion

    }

}