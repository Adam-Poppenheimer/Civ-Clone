using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapRendering {

    public interface ICellHeightmapLogic {

        #region methods

        float GetHeightForPositionForCell(Vector3 position, IHexCell cell, HexDirection sextant);

        #endregion

    }

}
