using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Simulation.MapRendering {

    [Serializable]
    public class RenderTextureData {

        #region instance fields and properties

        public int TexelsPerUnit {
            get { return _texelsPerUnit; }
        }
        [SerializeField] private int _texelsPerUnit;

        public int Depth {
            get { return _depth; }
        }
        [SerializeField] private int _depth;

        public RenderTextureFormat Format {
            get { return _format; }
        }
        [SerializeField] private RenderTextureFormat _format;

        #endregion

    }

}
