using System;

using UnityEngine;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapRendering {

    public interface IFlatlandsHeightmapLogic {

        #region methods

        float GetHeightForPoint(Vector2 xzPoint, IHexCell cell, HexDirection sextant);

        #endregion

    }

}