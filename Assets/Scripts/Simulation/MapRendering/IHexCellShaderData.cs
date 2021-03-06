﻿using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapRendering {

    public interface IHexCellShaderData {

        #region methods

        void Initialize(int x, int z);

        void RefreshVisibility(IHexCell cell);

        void SetMapData(IHexCell cell, float data);

        #endregion

    }
}