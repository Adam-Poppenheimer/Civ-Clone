using System;

using UnityEngine;

using Assets.Simulation.HexMap;
using Assets.Util;

namespace Assets.Simulation.MapRendering {

    public interface IFlatlandsHeightmapLogic {

        #region methods

        float GetHeightForPoint(Vector2 xzPoint, AsyncTextureUnsafe<Color32> noiseTexture);

        #endregion

    }

}