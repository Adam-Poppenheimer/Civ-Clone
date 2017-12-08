using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.UI {

    public interface ICameraConfig {

        #region properties

        float PanningSpeed { get; }
        float ZoomingSpeed { get; }

        float MinFOV { get; }
        float MaxFOV { get; }

        #endregion

    }

}
