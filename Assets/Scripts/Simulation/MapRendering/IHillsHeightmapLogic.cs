using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Assets.Simulation.HexMap;
using Assets.Util;

namespace Assets.Simulation.MapRendering {

    public interface IHillsHeightmapLogic {

        #region methods

        float GetHeightForPoint(
            Vector2 xzPoint, float elevationDuck, AsyncTextureUnsafe<Color32> flatlandsNoise,
            AsyncTextureUnsafe<Color32> hillsNoise
        );

        #endregion

    }

}
