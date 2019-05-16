using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

using UnityEngine;
using Unity.Collections;

namespace Assets.Util {

    public static class RawTextureSampler {

        #region static methods

        public static Vector4 SampleBilinear(float u, float v, AsyncTextureUnsafe<Color32> texture) {
            u = (u % 1f);
            v = (v % 1f);

            u = u * (texture.Width  - 1) - 0.5f;
            v = v * (texture.Height - 1) - 0.5f;

            if(u < 0f) u += (texture.Width  - 1);
            if(v < 0f) v += (texture.Height - 1);

            int x = Mathf.FloorToInt(u);
            int y = Mathf.FloorToInt(v);

            float uRatio = u - x;
            float vRatio = v - y;

            float uOpposite = 1 - uRatio;
            float vOpposite = 1 - vRatio;

            return (Color)texture.Raw[x     + y       * texture.Width] * uOpposite +
                   (Color)texture.Raw[x + 1 + y       * texture.Width] * uRatio +
                   (Color)texture.Raw[x     + (y + 1) * texture.Width] * vOpposite +
                   (Color)texture.Raw[x + 1 + (y + 1) * texture.Width] * vRatio;
        }

        public static Color32 SamplePoint(float u, float v, AsyncTextureUnsafe<Color32> texture) {
            u = Mathf.Clamp01(u);
            v = Mathf.Clamp01(v);

            int x = Mathf.FloorToInt(u * (texture.Width  - 1));
            int y = Mathf.FloorToInt(v * (texture.Height - 1));

            return texture.Raw[x + y * texture.Width];
        }

        #endregion

    }

}
