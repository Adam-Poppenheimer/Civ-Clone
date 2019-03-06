using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapRendering {

    public interface IMountainHeightmapWeightLogic {

        #region methods

        void GetHeightWeightsForPoint(
            Vector2 pointXZ, IHexCell cell, HexDirection sextant,
            out float peakWeight, out float ridgeWeight, out float hillsWeight
        );

        #endregion

    }

}
