using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;
using UnityEngine.Rendering;

namespace Assets.Simulation.MapRendering {

    [Serializable]
    public class HexRenderingData {

        public ShadowCastingMode ShadowCastingMode;
        public bool              ReceiveShadows;
        public Material          Material;
        public bool              SuppressRendering;

    }

}
