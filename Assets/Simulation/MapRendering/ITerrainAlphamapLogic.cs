using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

namespace Assets.Simulation.MapRendering {

    public interface ITerrainAlphamapLogic {

        #region methods

        void GetAlphamapFromOrientation(float[] returnMap, float[] intermediateMap, PointOrientationData orientationData);

        #endregion

    }

}
