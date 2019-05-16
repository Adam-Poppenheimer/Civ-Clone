using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

using Assets.Simulation.HexMap;

namespace Assets.Simulation.MapGeneration {

    public interface IRiverGenerator {

        #region methods

        void CreateRivers(
            IEnumerable<IHexCell> landCells, IEnumerable<IHexCell> waterCells,
            int riverPercentage
        );

        #endregion

    }

}
