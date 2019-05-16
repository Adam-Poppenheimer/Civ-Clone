using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Simulation.MapRendering {

    public interface INoiseTexture {

        #region methods

        Vector4 SampleBilinear(float u, float v);

        #endregion

    }

}
