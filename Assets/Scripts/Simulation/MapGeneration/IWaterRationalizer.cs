using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapGeneration {

    public interface IWaterRationalizer {

        #region methods

        void RationalizeWater(IEnumerable<IHexCell> cells);

        #endregion

    }

}
