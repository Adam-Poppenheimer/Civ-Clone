﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Simulation.MapRendering {

    public interface ITerrainHeightLogic {

        #region methods

        float GetHeightForPoint(Vector2 xzPoint, PointOrientationData orientationData);

        #endregion

    }

}
