using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapRendering {

    public interface IPointOrientationLogic {

        #region methods

        HexDirection GetSextantOfPointForCell(Vector3 point, IHexCell cell);

        PointOrientation GetOrientationOfPointInCell(Vector3 point, IHexCell cell, HexDirection sextant);

        #endregion

    }

}
