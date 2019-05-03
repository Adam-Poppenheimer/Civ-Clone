using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapRendering {

    public interface IHillsHeightmapLogic {

        #region methods

        float GetHeightForPoint(Vector2 xzPoint, IHexCell cell, HexDirection sextant, float elevationDuck);

        #endregion

    }

}
