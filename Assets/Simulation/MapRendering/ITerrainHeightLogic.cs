using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Simulation.MapRendering {

    public interface ITerrainHeightLogic {

        #region methods

        float GetHeightForPosition(Vector3 position);

        #endregion

    }

}
