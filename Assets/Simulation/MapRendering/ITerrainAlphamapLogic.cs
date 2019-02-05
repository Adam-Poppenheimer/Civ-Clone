using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Simulation.MapRendering {

    public interface ITerrainAlphamapLogic {

        #region methods

        float[] GetAlphamapForPosition(Vector3 position);

        #endregion

    }

}
