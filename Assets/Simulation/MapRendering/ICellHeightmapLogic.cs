using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Assets.Simulation.HexMap;
using Assets.Util;

namespace Assets.Simulation.MapRendering {

    public interface ICellHeightmapLogic {

        #region methods

        float GetHeightForPointForCell(
            Vector2 xzPoint, IHexCell cell, float elevationDuck,
            AsyncTextureUnsafe<Color32> flatlandNoise, AsyncTextureUnsafe<Color32> hillsNoise
        );

        #endregion

    }

}
