using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapRendering {

    public interface IPointOrientationLogic {

        #region methods

        IHexCell GetCellAtPoint(Vector3 point);

        PointOrientationData GetOrientationDataFromColors(Color32 orientationColor, Color weightsColor);

        #endregion

    }

}
