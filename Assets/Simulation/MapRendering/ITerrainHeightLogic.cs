using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Assets.Util;

namespace Assets.Simulation.MapRendering {

    public interface ITerrainHeightLogic {

        #region methods

        float GetHeightForPoint(
            Vector2 xzPoint, PointOrientationData orientationData, AsyncTextureUnsafe<Color32> flatlandsNoise,
            AsyncTextureUnsafe<Color32> hillsNoise
        );

        #endregion

    }

}
