using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Simulation.MapRendering {

    public class NoiseTexture : INoiseTexture {

        #region instance fields and properties

        private Texture2D SourceTexture;

        #endregion

        #region constructors

        public NoiseTexture(Texture2D sourceTexture) {
            SourceTexture = sourceTexture;
        }

        #endregion

        #region instance methods

        #region from INoiseTexture

        public Vector4 SampleBilinear(float u, float v) {
            return SourceTexture.GetPixelBilinear(u, v);
        }

        #endregion

        #endregion
        
    }

}
