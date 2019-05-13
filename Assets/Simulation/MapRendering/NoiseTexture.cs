using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Simulation.MapRendering {

    public class NoiseTexture : INoiseTexture {

        #region instance fields and properties

        private int TextureWidth;
        private int TextureHeight;

        private Texture2D SourceTexture;

        #endregion

        #region constructors

        public NoiseTexture(Texture2D sourceTexture) {
            SourceTexture = sourceTexture;

            TextureWidth  = SourceTexture.width;
            TextureHeight = SourceTexture.height;
        }

        #endregion

        #region instance methods

        #region from INoiseTexture

        public Vector4 SampleBilinear(float u, float v) {
            var nativeArray = SourceTexture.GetRawTextureData<Color>();

            u = u * TextureWidth  - 0.5f;
            v = v * TextureHeight - 0.5f;

            int x = Mathf.FloorToInt(u);
            int y = Mathf.FloorToInt(v);

            float uRatio = u - x;
            float vRatio = v - y;

            float uOpposite = 1 - uRatio;
            float vOpposite = 1 - vRatio;

            return (nativeArray[x + y       * TextureWidth] * uOpposite + nativeArray[x + 1 + y       * TextureWidth] * uRatio) * vOpposite +
                   (nativeArray[x + (y + 1) * TextureWidth] * uOpposite + nativeArray[x + 1 + (y + 1) * TextureWidth] * uRatio) * vRatio;
        }

        #endregion

        #endregion
        
    }

}
