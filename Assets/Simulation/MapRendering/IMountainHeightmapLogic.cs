using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapRendering {

    public interface IMountainHeightmapLogic {

        #region methods

        float GetHeightForPosition(Vector3 position, IHexCell cell, HexDirection sextant);

        #endregion

    }

}
