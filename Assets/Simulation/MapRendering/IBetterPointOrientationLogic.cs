using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Simulation.MapRendering {

    public interface IBetterPointOrientationLogic {

        #region methods

        PointOrientationData GetOrientationDataFromTextures(
            Vector2 textureNormal, Texture2D orientationTexture, Texture2D weightsTexture
        );

        #endregion

    }

}
