using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Simulation.MapRendering {

    [Serializable]
    public class OrientationBakingDataData {

        #region instance fields and properties

        public int Resolution {
            get { return _resolution; }
        }
        [SerializeField] private int _resolution = 128;

        public int Depth {
            get { return _depth; }
        }
        [SerializeField] private int _depth = 8;

        public RenderTextureFormat RenderTextureFormat {
            get { return _renderTextureFormat; }
        }
        [SerializeField] private RenderTextureFormat _renderTextureFormat = RenderTextureFormat.ARGB32;

        public TextureFormat TextureFormat {
            get { return _textureFormat; }
        }
        [SerializeField] private TextureFormat _textureFormat = TextureFormat.ARGB32;

        #endregion

    }

}
