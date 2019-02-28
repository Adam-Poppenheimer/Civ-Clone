using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using UnityEngine;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapRendering {

    public class TerrainHeightWeightsLogic {

        #region instance fields and properties



        #endregion

        #region constructors



        #endregion

        #region instance methods

        #region from ITerrainHeightWeightsLogic

        public float[] GetHeightWeightsForPosition(Vector3 position, IHexCell center, HexDirection sextant) {
            var retval = new float[6];

            throw new NotImplementedException();
        }

        #endregion

        #endregion

    }

}
