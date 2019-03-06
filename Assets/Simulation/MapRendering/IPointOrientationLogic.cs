using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapRendering {

    public interface IPointOrientationLogic {

        #region methods

        PointOrientationData GetOrientationDataForPoint(Vector2 xzPoint);

        #endregion

    }

}
